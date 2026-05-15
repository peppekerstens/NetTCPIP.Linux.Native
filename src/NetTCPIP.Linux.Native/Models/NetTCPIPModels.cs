// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Models/NetTCPIPModels.cs
// Output types for NetTCPIP.Linux.Native — mirrors Windows NetTCPIP module object shapes.

namespace Microsoft.PowerShell.Commands;

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
    public string   AddressState      { get; set; } = "Preferred";
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
    public string TypeOfNextHop     { get; set; } = "Connected";
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
    public string   State         { get; set; } = "Established";
    public uint     OwningProcess { get; set; }
    public DateTime? CreationTime { get; set; }
    public string   OffloadState  { get; set; } = "InHost";
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
