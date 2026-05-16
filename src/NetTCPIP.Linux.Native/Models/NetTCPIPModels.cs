// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Models/NetTCPIPModels.cs
// Output types for NetTCPIP.Linux.Native — mirrors Windows NetTCPIP module object shapes.

namespace Microsoft.PowerShell.Commands;

/// <summary>Address state for a network IP address.</summary>
public enum AddressState
{
    Tentative = 0,
    Deprecated = 1,
    Invalid = 2,
    Transitional = 3,
    Preferred = 4,
}

/// <summary>Type of next hop for a network route.</summary>
public enum NextHopType
{
    Other = 0,
    Invalid = 1,
    Direct = 2,
    Indirect = 3,
}

/// <summary>TCP connection state matching System.Net.Mib.TcpState.</summary>
public enum TcpState
{
    Closed = 1,
    Listen = 2,
    SynSent = 3,
    SynReceived = 4,
    Established = 5,
    FinWait1 = 6,
    FinWait2 = 7,
    CloseWait = 8,
    Closing = 9,
    LastAck = 10,
    TimeWait = 11,
    DeleteTCB = 12,
}

/// <summary>Offload state for a TCP connection.</summary>
public enum OffloadState
{
    Unknown = 0,
    Software = 1,
    Hardware = 2,
    Capable = 3,
    Disabled = 4,
}

public sealed class NetIPAddress
{
    public string   IPAddress         { get; set; } = string.Empty;
    public string   InterfaceAlias    { get; set; } = string.Empty;
    public int      InterfaceIndex    { get; set; }
    public string   AddressFamily     { get; set; } = "IPv4";
    public int      PrefixLength      { get; set; }
    public string   Type              { get; set; } = "Unicast";
    public string   PrefixOrigin      { get; set; } = "Manual";
    public string   SuffixOrigin      { get; set; } = "Manual";
    public AddressState AddressState      { get; set; } = AddressState.Preferred;
    public TimeSpan ValidLifetime     { get; set; } = TimeSpan.MaxValue;
    public TimeSpan PreferredLifetime { get; set; } = TimeSpan.MaxValue;
    public bool     SkipAsSource      { get; set; }
    public string   PolicyStore       { get; set; } = "ActiveStore";
    public override string ToString() => IPAddress;
}

public sealed class NetRoute
{
    public string DestinationPrefix { get; set; } = string.Empty;
    public string NextHop           { get; set; } = string.Empty;
    public string InterfaceAlias    { get; set; } = string.Empty;
    public int    InterfaceIndex    { get; set; }
    public string AddressFamily     { get; set; } = "IPv4";
    public int    RouteMetric       { get; set; }
    public NextHopType TypeOfNextHop     { get; set; } = NextHopType.Direct;
    public string Protocol          { get; set; } = "NetMgmt";
    public string Publish           { get; set; } = "No";
    public string PolicyStore       { get; set; } = "ActiveStore";
    public override string ToString() => DestinationPrefix;
}

public sealed class NetTCPConnection
{
    public string   LocalAddress  { get; set; } = string.Empty;
    public ushort   LocalPort     { get; set; }
    public string   RemoteAddress { get; set; } = string.Empty;
    public ushort   RemotePort    { get; set; }
    public TcpState   State         { get; set; } = TcpState.Established;
    public uint     OwningProcess { get; set; }
    public DateTime? CreationTime { get; set; }
    public OffloadState OffloadState  { get; set; } = OffloadState.Software;
    public override string ToString() => $"{LocalAddress}:{LocalPort} -> {RemoteAddress}:{RemotePort}";
}

public sealed class NetIPConfiguration
{
    public string   InterfaceAlias       { get; set; } = string.Empty;
    public int      InterfaceIndex       { get; set; }
    public string   InterfaceDescription { get; set; } = string.Empty;
    public string   NetProfile           { get; set; } = "Unknown";
    public string?  IPv4Address          { get; set; }
    public string?  IPv4DefaultGateway   { get; set; }
    public string[] DNSServer            { get; set; } = [];
    public string?  IPv6Address          { get; set; }
    public override string ToString() => InterfaceAlias;
}
