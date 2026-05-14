// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Helpers/IpHelpers.cs
// Core helper layer for NetTCPIP.Linux.Native.
//
// READ paths — no subprocesses:
//   GetIPAddresses()       → System.Net.NetworkInformation.NetworkInterface (BCL)
//   GetRoutes()            → /proc/net/route (IPv4) + /proc/net/ipv6_route (IPv6)
//   GetTcpConnections()    → /proc/net/tcp  + /proc/net/tcp6
//   GetIPConfiguration()   → composes GetIPAddresses() + GetRoutes()
//   BuildLinkMap()         → NetworkInterface (BCL)
//
// WRITE paths — subprocess (ip addr/route/neigh): no kernel API in BCL; Netlink
// socket P/Invoke would be the only alternative and is out of scope here.

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.Commands;

internal static class IpHelpers
{
    // -----------------------------------------------------------------------
    // GET IP ADDRESSES — BCL NetworkInterface
    // -----------------------------------------------------------------------

    internal static List<NetIPAddress> GetIPAddresses(NetworkInterface[]? nics = null)
    {
        var results = new List<NetIPAddress>();

        try
        {
            nics ??= NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                var ipProps = nic.GetIPProperties();
                int ifIndex = nic.GetIPProperties().GetIPv4Properties()?.Index
                              ?? nic.GetIPProperties().GetIPv6Properties()?.Index
                              ?? 0;

                foreach (var uni in ipProps.UnicastAddresses)
                {
                    var af  = uni.Address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                    var prefix = uni.PrefixLength;

                    // ValidLifetime / PreferredLifetime: Windows-only BCL properties; on Linux
                    // the kernel exposes these via rtnetlink but not via BCL — treat as infinite.
#pragma warning disable CA1416
                    var validLifeRaw     = OperatingSystem.IsWindows() ? uni.AddressValidLifetime     : uint.MaxValue;
                    var preferredLifeRaw = OperatingSystem.IsWindows() ? uni.AddressPreferredLifetime : uint.MaxValue;
#pragma warning restore CA1416
                    var validLife     = validLifeRaw     == uint.MaxValue ? TimeSpan.MaxValue : TimeSpan.FromSeconds(validLifeRaw);
                    var preferredLife = preferredLifeRaw == uint.MaxValue ? TimeSpan.MaxValue : TimeSpan.FromSeconds(preferredLifeRaw);

                    results.Add(new NetIPAddress
                    {
                        IPAddress         = uni.Address.ToString(),
                        InterfaceAlias    = nic.Name,
                        InterfaceIndex    = ifIndex,
                        AddressFamily     = af,
                        PrefixLength      = prefix,
                        ValidLifetime     = validLife,
                        PreferredLifetime = preferredLife,
                    });
                }
            }
        }
        catch { /* on non-Linux, return empty */ }

        return results;
    }

    // -----------------------------------------------------------------------
    // GET ROUTES — /proc/net/route (IPv4) + /proc/net/ipv6_route (IPv6)
    // -----------------------------------------------------------------------

    internal static List<NetRoute> GetRoutes(NetworkInterface[]? nics = null)
    {
        var linkMap = BuildLinkMap(nics);
        var results = new List<NetRoute>();

        // IPv4: /proc/net/route
        // Columns: Iface Destination Gateway Flags RefCnt Use Metric Mask MTU Window IRTT
        // All numbers are hex, host byte order (little-endian on x86).
        try
        {
            var lines = File.ReadAllLines("/proc/net/route");
            foreach (var line in lines.Skip(1)) // skip header
            {
                var cols = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 11) continue;

                var iface   = cols[0].Trim();
                var dst     = ParseHexIPv4(cols[1]);
                var gw      = ParseHexIPv4(cols[2]);
                var mask    = ParseHexIPv4(cols[7]);
                var metric  = int.TryParse(cols[6], out int m) ? m : 0;

                var prefix  = MaskToPrefixLength(mask);
                var dstCidr = $"{dst}/{prefix}";
                var gwStr   = gw == "0.0.0.0" ? null : gw;
                var idx     = linkMap.GetValueOrDefault(iface);

                results.Add(new NetRoute
                {
                    DestinationPrefix = dstCidr,
                    NextHop           = gwStr ?? "0.0.0.0",
                    InterfaceAlias    = iface,
                    InterfaceIndex    = idx,
                    AddressFamily     = "IPv4",
                    RouteMetric       = metric,
                    TypeOfNextHop     = gwStr != null ? "Remote" : "Connected",
                });
            }
        }
        catch { /* /proc not available (Windows) */ }

        // IPv6: /proc/net/ipv6_route
        // Columns: dest_prefix dest_plen src_prefix src_plen nexthop metric refcnt use flags iface
        // All addresses are 32-hex-char (128-bit), no colons.
        try
        {
            var lines = File.ReadAllLines("/proc/net/ipv6_route");
            foreach (var line in lines)
            {
                var cols = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 10) continue;

                var dstRaw  = cols[0];
                var plen    = int.TryParse(cols[1], System.Globalization.NumberStyles.HexNumber, null, out int pl) ? pl : 0;
                var nhRaw   = cols[4];
                var metric  = int.TryParse(cols[5], System.Globalization.NumberStyles.HexNumber, null, out int met) ? met : 0;
                var iface   = cols[9].Trim();

                var dst     = ParseHexIPv6(dstRaw);
                var nh      = ParseHexIPv6(nhRaw);
                var nhStr   = nh == "::" ? null : nh;
                var idx     = linkMap.GetValueOrDefault(iface);

                results.Add(new NetRoute
                {
                    DestinationPrefix = $"{dst}/{plen}",
                    NextHop           = nhStr ?? "::",
                    InterfaceAlias    = iface,
                    InterfaceIndex    = idx,
                    AddressFamily     = "IPv6",
                    RouteMetric       = metric,
                    TypeOfNextHop     = nhStr != null ? "Remote" : "Connected",
                });
            }
        }
        catch { /* /proc not available */ }

        return results;
    }

    // -----------------------------------------------------------------------
    // GET TCP CONNECTIONS — /proc/net/tcp + /proc/net/tcp6
    // -----------------------------------------------------------------------

    // /proc/net/tcp state codes → string names
    private static readonly Dictionary<string, string> ProcStateMap = new()
    {
        ["01"] = "Established",
        ["02"] = "SynSent",
        ["03"] = "SynReceived",
        ["04"] = "FinWait1",
        ["05"] = "FinWait2",
        ["06"] = "TimeWait",
        ["07"] = "Closed",
        ["08"] = "CloseWait",
        ["09"] = "LastAck",
        ["0A"] = "Listen",
        ["0B"] = "Closing",
    };

    internal static List<NetTCPConnection> GetTcpConnections()
    {
        var results = new List<NetTCPConnection>();
        ParseProcTcp("/proc/net/tcp",  isIpv6: false, results);
        ParseProcTcp("/proc/net/tcp6", isIpv6: true,  results);
        return results;
    }

    private static void ParseProcTcp(string path, bool isIpv6, List<NetTCPConnection> results)
    {
        if (!File.Exists(path)) return;
        try
        {
            foreach (var line in File.ReadLines(path).Skip(1)) // skip header
            {
                // sl local_address rem_address st tx_queue:rx_queue tr:tm->when retrnsmt uid timeout inode ...
                var cols = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 10) continue;

                var localRaw  = cols[1];
                var remoteRaw = cols[2];
                var stateHex  = cols[3].ToUpperInvariant();
                var uid       = uint.TryParse(cols[7], out uint u) ? u : 0;
                var inode     = cols[9];

                if (!TryParseProcEndpoint(localRaw,  isIpv6, out var lAddr, out var lPort)) continue;
                if (!TryParseProcEndpoint(remoteRaw, isIpv6, out var rAddr, out var rPort)) continue;

                var state = ProcStateMap.GetValueOrDefault(stateHex, stateHex);
                var pid   = LookupPidByInode(inode);

                results.Add(new NetTCPConnection
                {
                    LocalAddress  = lAddr,
                    LocalPort     = lPort,
                    RemoteAddress = rAddr,
                    RemotePort    = rPort,
                    State         = state,
                    OwningProcess = pid,
                    OffloadState  = "InHost",
                });
            }
        }
        catch { /* proc not available or parse error */ }
    }

    // -----------------------------------------------------------------------
    // GET IP CONFIGURATION — composes GetIPAddresses() + GetRoutes()
    // -----------------------------------------------------------------------

    internal static List<NetIPConfiguration> GetIPConfiguration(bool includeLoopback)
    {
        NetworkInterface[] nics;
        try { nics = NetworkInterface.GetAllNetworkInterfaces(); }
        catch { return []; }

        var addresses = GetIPAddresses(nics);
        var routes    = GetRoutes(nics);

        // Find default IPv4 gateway (dest 0.0.0.0/0, IPv4, with a real nexthop)
        string? defaultGw = routes
            .Where(r => r.AddressFamily == "IPv4"
                        && (r.DestinationPrefix == "0.0.0.0/0")
                        && r.NextHop != "0.0.0.0")
            .OrderBy(r => r.RouteMetric)
            .Select(r => r.NextHop)
            .FirstOrDefault();

        // Group addresses by interface
        var byIface = addresses
            .GroupBy(a => a.InterfaceAlias, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Enumerate interfaces preserving order from BCL
        var results = new List<NetIPConfiguration>();
        foreach (var nic in nics)
        {
            if (!includeLoopback && nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

            var ifIndex = nic.GetIPProperties().GetIPv4Properties()?.Index
                          ?? nic.GetIPProperties().GetIPv6Properties()?.Index
                          ?? 0;

            var addrs = byIface.GetValueOrDefault(nic.Name, []);
            var ipv4  = addrs.FirstOrDefault(a => a.AddressFamily == "IPv4")?.IPAddress;
            var ipv6  = addrs.FirstOrDefault(a => a.AddressFamily == "IPv6"
                                                   && !a.IPAddress.StartsWith("fe80", StringComparison.OrdinalIgnoreCase))?.IPAddress;

            results.Add(new NetIPConfiguration
            {
                InterfaceAlias       = nic.Name,
                InterfaceIndex       = ifIndex,
                InterfaceDescription = nic.Description,
                IPv4Address          = ipv4,
                IPv4DefaultGateway   = defaultGw,
                IPv6Address          = ipv6,
            });
        }

        return results;
    }

    // -----------------------------------------------------------------------
    // WRITE HELPERS — subprocess (ip): no kernel BCL API for Netlink writes
    // -----------------------------------------------------------------------

    internal static void AddIPAddress(string ipAddress, int prefixLength, string interfaceAlias)
        => RunIpVoid("addr", "add", $"{ipAddress}/{prefixLength}", "dev", interfaceAlias);

    internal static void RemoveIPAddress(string ipAddress, int prefixLength, string interfaceAlias)
        => RunIpVoid("addr", "del", $"{ipAddress}/{prefixLength}", "dev", interfaceAlias);

    internal static void AddRoute(string dest, string? gateway, string interfaceAlias, int metric)
    {
        var args = new List<string> { "route", "add", dest };
        if (!string.IsNullOrEmpty(gateway)) { args.Add("via"); args.Add(gateway); }
        args.AddRange(["dev", interfaceAlias]);
        if (metric > 0) { args.Add("metric"); args.Add(metric.ToString()); }
        RunIpVoid([.. args]);
    }

    internal static void RemoveRoute(string dest, string interfaceAlias)
        => RunIpVoid("route", "del", dest, "dev", interfaceAlias);

    internal static void AddNeighbor(string ipAddress, string linkLayerAddress, string interfaceAlias)
        => RunIpVoid("neigh", "add", ipAddress, "lladdr", linkLayerAddress, "dev", interfaceAlias, "nud", "permanent");

    internal static void RemoveNeighbor(string ipAddress, string interfaceAlias)
        => RunIpVoid("neigh", "del", ipAddress, "dev", interfaceAlias);

    // -----------------------------------------------------------------------
    // BCL HELPERS
    // -----------------------------------------------------------------------

    // Build interface-name → interface-index map using BCL (no subprocess).
    private static Dictionary<string, int> BuildLinkMap(NetworkInterface[]? nics = null)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        try
        {
            nics ??= NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                var idx = nic.GetIPProperties().GetIPv4Properties()?.Index
                          ?? nic.GetIPProperties().GetIPv6Properties()?.Index
                          ?? 0;
                map.TryAdd(nic.Name, idx);
            }
        }
        catch { /* ignore */ }
        return map;
    }

    // -----------------------------------------------------------------------
    // /proc PARSING HELPERS
    // -----------------------------------------------------------------------

    // Parse a hex IPv4 address from /proc/net/route (little-endian 4-byte hex).
    private static string ParseHexIPv4(string hex)
    {
        if (!uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out uint raw))
            return "0.0.0.0";
        // Raw is stored little-endian: byte 0 = LSB
        byte b0 = (byte)(raw & 0xFF);
        byte b1 = (byte)((raw >> 8)  & 0xFF);
        byte b2 = (byte)((raw >> 16) & 0xFF);
        byte b3 = (byte)((raw >> 24) & 0xFF);
        return $"{b0}.{b1}.{b2}.{b3}";
    }

    // Convert dotted-decimal mask to prefix length.
    private static int MaskToPrefixLength(string mask)
    {
        if (!IPAddress.TryParse(mask, out var ip)) return 0;
        uint bits = 0;
        foreach (var b in ip.GetAddressBytes()) bits = (bits << 8) | b;
        int count = 0;
        while ((bits & 0x80000000u) != 0) { count++; bits <<= 1; }
        return count;
    }

    // Parse a 32-hex-char IPv6 address from /proc/net/ipv6_route (big-endian groups).
    private static string ParseHexIPv6(string hex)
    {
        if (hex.Length != 32) return "::";
        try
        {
            var bytes = new byte[16];
            for (int i = 0; i < 16; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return new IPAddress(bytes).ToString();
        }
        catch { return "::"; }
    }

    // Parse local_address / rem_address column from /proc/net/tcp[6].
    // Format: XXXXXXXX:PPPP (IPv4) or XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX:PPPP (IPv6)
    private static bool TryParseProcEndpoint(string raw, bool isIpv6, out string addr, out ushort port)
    {
        addr = ""; port = 0;
        var colon = raw.LastIndexOf(':');
        if (colon < 0) return false;

        var addrHex = raw[..colon];
        var portHex = raw[(colon + 1)..];

        if (!ushort.TryParse(portHex, System.Globalization.NumberStyles.HexNumber, null, out port))
            return false;

        if (isIpv6)
        {
            addr = ParseHexIPv6(addrHex);
        }
        else
        {
            addr = ParseHexIPv4(addrHex);
        }
        return true;
    }

    // Look up the PID that owns a given socket inode by scanning /proc/<pid>/fd/.
    // This is O(processes) but is the standard approach without CAP_SYS_PTRACE.
    private static uint LookupPidByInode(string inode)
    {
        if (string.IsNullOrEmpty(inode) || inode == "0") return 0;
        var target = $"socket:[{inode}]";
        try
        {
            foreach (var procDir in Directory.EnumerateDirectories("/proc"))
            {
                var name = Path.GetFileName(procDir);
                if (!uint.TryParse(name, out uint pid)) continue;
                var fdDir = Path.Combine(procDir, "fd");
                if (!Directory.Exists(fdDir)) continue;
                try
                {
                    foreach (var fd in Directory.EnumerateFiles(fdDir))
                    {
                        try
                        {
                            if (File.ResolveLinkTarget(fd, returnFinalTarget: false)?.Name == target)
                                return pid;
                        }
                        catch { /* fd vanished or not a symlink */ }
                    }
                }
                catch { /* no permission to read fd dir */ }
            }
        }
        catch { /* /proc not available */ }
        return 0;
    }

    // -----------------------------------------------------------------------
    // SUBPROCESS — write operations only (ip addr/route/neigh)
    // -----------------------------------------------------------------------

    private static void RunIpVoid(params string[] args)
    {
        var (stdout, exit) = RunProcess("ip", args);
        if (exit != 0)
            throw new InvalidOperationException($"ip {string.Join(' ', args)} failed (exit {exit}): {stdout}");
    }

    private static (string Stdout, int ExitCode) RunProcess(string exe, params string[] args)
    {
        var psi = new ProcessStartInfo(exe)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);
        try
        {
            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return (stdout, proc.ExitCode);
        }
        catch { return (string.Empty, -1); }
    }
}
