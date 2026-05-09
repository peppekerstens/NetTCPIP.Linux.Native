// Helpers/IpHelpers.cs
// Core helper layer for NetTCPIP.Linux.Native.
// Reads use `ip -json` and `ss`; writes use `Process.Start(ip)`.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.Commands;

internal static class IpHelpers
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
    };

    // -----------------------------------------------------------------------
    // ip -json addr show
    // -----------------------------------------------------------------------

    internal static List<NetIPAddress> GetIPAddresses()
    {
        var json = RunIp("-json", "addr", "show");
        if (string.IsNullOrWhiteSpace(json)) return [];

        var arr = JsonNode.Parse(json)?.AsArray();
        if (arr == null) return [];

        var results = new List<NetIPAddress>();
        foreach (var iface in arr)
        {
            if (iface == null) continue;
            var ifname  = iface["ifname"]?.GetValue<string>() ?? "";
            var ifindex = iface["ifindex"]?.GetValue<int>() ?? 0;
            var addrInfo = iface["addr_info"]?.AsArray();
            if (addrInfo == null) continue;

            foreach (var addr in addrInfo)
            {
                if (addr == null) continue;
                var family = addr["family"]?.GetValue<string>() ?? "";
                var af = family == "inet" ? "IPv4" : "IPv6";
                var local = addr["local"]?.GetValue<string>() ?? "";
                var prefix = addr["prefixlen"]?.GetValue<int>() ?? 0;

                var validRaw     = addr["valid_life_time"]?.GetValue<long>() ?? long.MaxValue;
                var preferredRaw = addr["preferred_life_time"]?.GetValue<long>() ?? long.MaxValue;
                const long maxLife = 4294967295L;

                results.Add(new NetIPAddress
                {
                    IPAddress         = local,
                    InterfaceAlias    = ifname,
                    InterfaceIndex    = ifindex,
                    AddressFamily     = af,
                    PrefixLength      = prefix,
                    ValidLifetime     = validRaw >= maxLife ? TimeSpan.MaxValue : TimeSpan.FromSeconds(validRaw),
                    PreferredLifetime = preferredRaw >= maxLife ? TimeSpan.MaxValue : TimeSpan.FromSeconds(preferredRaw),
                });
            }
        }
        return results;
    }

    // -----------------------------------------------------------------------
    // ip -json route show (IPv4 + IPv6)
    // -----------------------------------------------------------------------

    internal static List<NetRoute> GetRoutes()
    {
        var linkMap = BuildLinkMap();
        var results = new List<NetRoute>();

        foreach (var (args, af, defaultDst, defaultNH) in new[]
        {
            (new[]{ "-json", "route", "show" }, "IPv4", "0.0.0.0/0", "0.0.0.0"),
            (new[]{ "-6", "-json", "route", "show" }, "IPv6", "::/0", "::"),
        })
        {
            var json = RunIp(args);
            if (string.IsNullOrWhiteSpace(json)) continue;
            var arr = JsonNode.Parse(json)?.AsArray();
            if (arr == null) continue;

            foreach (var r in arr)
            {
                if (r == null) continue;
                var dst     = r["dst"]?.GetValue<string>() ?? "";
                if (dst == "default") dst = defaultDst;
                var gw      = r["gateway"]?.GetValue<string>();
                var dev     = r["dev"]?.GetValue<string>() ?? "";
                var metric  = r["metric"]?.GetValue<int>() ?? 0;
                var idx     = linkMap.GetValueOrDefault(dev);

                results.Add(new NetRoute
                {
                    DestinationPrefix = dst,
                    NextHop           = gw ?? defaultNH,
                    InterfaceAlias    = dev,
                    InterfaceIndex    = idx,
                    AddressFamily     = af,
                    RouteMetric       = metric,
                    TypeOfNextHop     = gw != null ? "Remote" : "Connected",
                });
            }
        }
        return results;
    }

    // -----------------------------------------------------------------------
    // ss -tnap  (TCP connections)
    // -----------------------------------------------------------------------

    private static readonly Dictionary<string, string> SsStateMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ESTAB"]      = "Established",
        ["LISTEN"]     = "Listen",
        ["TIME-WAIT"]  = "TimeWait",
        ["CLOSE-WAIT"] = "CloseWait",
        ["FIN-WAIT-1"] = "FinWait1",
        ["FIN-WAIT-2"] = "FinWait2",
        ["LAST-ACK"]   = "LastAck",
        ["CLOSING"]    = "Closing",
        ["SYN-SENT"]   = "SynSent",
        ["SYN-RECV"]   = "SynReceived",
        ["CLOSED"]     = "Closed",
        ["UNCONN"]     = "Closed",
    };

    internal static List<NetTCPConnection> GetTcpConnections()
    {
        var output = RunProcess("ss", "-tnap");
        var results = new List<NetTCPConnection>();

        foreach (var line in output.Split('\n').Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(new char[]{' ','\t'}, 6, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) continue;

            var stateRaw   = parts[0];
            var localFull  = parts[3];
            var remoteFull = parts[4];
            var procPart   = parts.Length >= 6 ? parts[5] : "";

            if (!TryParseEndpoint(localFull,  out var lAddr, out var lPort)) continue;
            if (!TryParseEndpointMayStar(remoteFull, out var rAddr, out var rPort)) continue;

            var state = SsStateMap.GetValueOrDefault(stateRaw, stateRaw);
            uint pid = 0;
            var pidMatch = Regex.Match(procPart, @"pid=(\d+)");
            if (pidMatch.Success) pid = uint.Parse(pidMatch.Groups[1].Value);

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
        return results;
    }

    // -----------------------------------------------------------------------
    // ip -json addr + route  ->  NetIPConfiguration
    // -----------------------------------------------------------------------

    internal static List<NetIPConfiguration> GetIPConfiguration(bool includeLoopback)
    {
        var addrJson = RunIp("-json", "addr", "show");
        if (string.IsNullOrWhiteSpace(addrJson)) return [];
        var addrArr = JsonNode.Parse(addrJson)?.AsArray();
        if (addrArr == null) return [];

        var routeJson = RunIp("-json", "route", "show");
        string? defaultGw = null;
        if (!string.IsNullOrWhiteSpace(routeJson))
        {
            var ra = JsonNode.Parse(routeJson)?.AsArray();
            if (ra != null)
                foreach (var r in ra)
                    if (r?["dst"]?.GetValue<string>() == "default")
                    { defaultGw = r["gateway"]?.GetValue<string>(); break; }
        }

        var results = new List<NetIPConfiguration>();
        foreach (var iface in addrArr)
        {
            if (iface == null) continue;
            var ifname  = iface["ifname"]?.GetValue<string>() ?? "";
            var ifindex = iface["ifindex"]?.GetValue<int>() ?? 0;
            if (!includeLoopback && ifname == "lo") continue;

            var addrInfo = iface["addr_info"]?.AsArray();
            string? ipv4 = null, ipv6 = null;
            if (addrInfo != null)
            {
                foreach (var a in addrInfo)
                {
                    if (a == null) continue;
                    var fam = a["family"]?.GetValue<string>();
                    if (fam == "inet"  && ipv4 == null) ipv4 = a["local"]?.GetValue<string>();
                    if (fam == "inet6" && ipv6 == null &&
                        a["scope"]?.GetValue<string>() != "link")
                        ipv6 = a["local"]?.GetValue<string>();
                }
            }

            results.Add(new NetIPConfiguration
            {
                InterfaceAlias       = ifname,
                InterfaceIndex       = ifindex,
                InterfaceDescription = ifname,
                IPv4Address          = ipv4,
                IPv4DefaultGateway   = defaultGw,
                IPv6Address          = ipv6,
            });
        }
        return results;
    }

    // -----------------------------------------------------------------------
    // Write helpers (ip addr add/del, ip route add/del/change, ip neigh)
    // -----------------------------------------------------------------------

    internal static void AddIPAddress(string ipAddress, int prefixLength, string interfaceAlias)
        => RunIpVoid("addr", "add", $"{ipAddress}/{prefixLength}", "dev", interfaceAlias);

    internal static void RemoveIPAddress(string ipAddress, int prefixLength, string interfaceAlias)
        => RunIpVoid("addr", "del", $"{ipAddress}/{prefixLength}", "dev", interfaceAlias);

    internal static void AddRoute(string dest, string? gateway, string interfaceAlias, int metric)
    {
        var args = new List<string> { "route", "add", dest };
        if (!string.IsNullOrEmpty(gateway)) { args.Add("via"); args.Add(gateway); }
        args.AddRange(new[] { "dev", interfaceAlias });
        if (metric > 0) { args.Add("metric"); args.Add(metric.ToString()); }
        RunIpVoid(args.ToArray());
    }

    internal static void RemoveRoute(string dest, string interfaceAlias)
        => RunIpVoid("route", "del", dest, "dev", interfaceAlias);

    internal static void AddNeighbor(string ipAddress, string linkLayerAddress, string interfaceAlias)
        => RunIpVoid("neigh", "add", ipAddress, "lladdr", linkLayerAddress, "dev", interfaceAlias, "nud", "permanent");

    internal static void RemoveNeighbor(string ipAddress, string interfaceAlias)
        => RunIpVoid("neigh", "del", ipAddress, "dev", interfaceAlias);

    // -----------------------------------------------------------------------
    // Low-level
    // -----------------------------------------------------------------------

    private static Dictionary<string, int> BuildLinkMap()
    {
        var json = RunIp("-json", "link", "show");
        if (string.IsNullOrWhiteSpace(json)) return [];
        var arr = JsonNode.Parse(json)?.AsArray();
        if (arr == null) return [];
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var link in arr)
        {
            var n = link?["ifname"]?.GetValue<string>();
            var i = link?["ifindex"]?.GetValue<int>() ?? 0;
            if (n != null) map[n] = i;
        }
        return map;
    }

    private static string RunIp(params string[] args)
        => RunProcess("ip", string.Join(' ', args));

    private static void RunIpVoid(params string[] args)
        => RunProcess("ip", string.Join(' ', args));

    private static string RunProcess(string exe, string arguments)
    {
        var psi = new ProcessStartInfo(exe, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        try
        {
            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return stdout;
        }
        catch { return string.Empty; }
    }

    private static bool TryParseEndpoint(string raw, out string addr, out ushort port)
    {
        addr = ""; port = 0;
        // IPv6 [::1]:80
        var m = Regex.Match(raw, @"^\[(.+)\]:(\d+)$");
        if (m.Success) { addr = m.Groups[1].Value; port = ushort.Parse(m.Groups[2].Value); return true; }
        // IPv4 1.2.3.4:80
        m = Regex.Match(raw, @"^(.*):(\d+)$");
        if (m.Success) { addr = m.Groups[1].Value; port = ushort.Parse(m.Groups[2].Value); return true; }
        return false;
    }

    private static bool TryParseEndpointMayStar(string raw, out string addr, out ushort port)
    {
        addr = ""; port = 0;
        // remote port may be '*' for LISTEN sockets
        var m = Regex.Match(raw, @"^(.*):\*$");
        if (m.Success) { addr = m.Groups[1].Value; port = 0; return true; }
        return TryParseEndpoint(raw, out addr, out port);
    }
}
