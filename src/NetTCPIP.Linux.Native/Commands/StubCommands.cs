// Commands/StubCommands.cs
// Stub cmdlets for NetTCPIP.Linux.Native.
// Each writes a NotSupportedException ErrorRecord. Matching the PS reference behaviour.
// Stubs: Find-NetRoute, Get-NetCompartment, Get-NetIPInterface, Get-NetIPv4Protocol,
//        Get-NetIPv6Protocol, Get-NetNeighbor, Get-NetOffloadGlobalSetting, Get-NetPrefixPolicy,
//        Get-NetTCPSetting, Get-NetTransportFilter, Get-NetUDPEndpoint, Get-NetUDPSetting,
//        New-NetTransportFilter, Remove-NetTransportFilter,
//        Set-NetIPAddress, Set-NetIPInterface, Set-NetIPv4Protocol, Set-NetIPv6Protocol,
//        Set-NetNeighbor, Set-NetOffloadGlobalSetting, Set-NetRoute, Set-NetTCPSetting, Set-NetUDPSetting,
//        Test-NetConnection

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

// ---- shared helper ----
internal static class StubHelper
{
    internal static void Stub(PSCmdlet cmdlet, string name)
        => cmdlet.WriteError(new ErrorRecord(
            new NotSupportedException($"{name} is not yet implemented in NetTCPIP.Linux.Native."),
            "NotImplemented", ErrorCategory.NotImplemented, name));
}

// ---- Find-NetRoute ----
[Cmdlet(VerbsCommon.Find, "NetRoute")]
public sealed class FindNetRouteCommand : PSCmdlet
{
    [Parameter] public string? RemoteIPAddress { get; set; }
    [Parameter] public string? InterfaceAlias  { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Find-NetRoute");
}

// ---- Get-NetCompartment ----
[Cmdlet(VerbsCommon.Get, "NetCompartment")]
public sealed class GetNetCompartmentCommand : PSCmdlet
{
    [Parameter] public uint[]? CompartmentId { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetCompartment");
}

// ---- Get-NetIPInterface ----
[Cmdlet(VerbsCommon.Get, "NetIPInterface")]
public sealed class GetNetIPInterfaceCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? InterfaceAlias { get; set; }
    [Parameter]               public int[]?    InterfaceIndex { get; set; }
    [Parameter][ValidateSet("IPv4","IPv6")] public string? AddressFamily { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetIPInterface");
}

// ---- Get-NetIPv4Protocol ----
[Cmdlet(VerbsCommon.Get, "NetIPv4Protocol")]
public sealed class GetNetIPv4ProtocolCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetIPv4Protocol");
}

// ---- Get-NetIPv6Protocol ----
[Cmdlet(VerbsCommon.Get, "NetIPv6Protocol")]
public sealed class GetNetIPv6ProtocolCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetIPv6Protocol");
}

// ---- Get-NetNeighbor ----
[Cmdlet(VerbsCommon.Get, "NetNeighbor")]
public sealed class GetNetNeighborCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? IPAddress      { get; set; }
    [Parameter]               public string[]? InterfaceAlias { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetNeighbor");
}

// ---- Get-NetOffloadGlobalSetting ----
[Cmdlet(VerbsCommon.Get, "NetOffloadGlobalSetting")]
public sealed class GetNetOffloadGlobalSettingCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetOffloadGlobalSetting");
}

// ---- Get-NetPrefixPolicy ----
[Cmdlet(VerbsCommon.Get, "NetPrefixPolicy")]
public sealed class GetNetPrefixPolicyCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetPrefixPolicy");
}

// ---- Get-NetTCPSetting ----
[Cmdlet(VerbsCommon.Get, "NetTCPSetting")]
public sealed class GetNetTCPSettingCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? SettingName { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetTCPSetting");
}

// ---- Get-NetTransportFilter ----
[Cmdlet(VerbsCommon.Get, "NetTransportFilter")]
public sealed class GetNetTransportFilterCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetTransportFilter");
}

// ---- Get-NetUDPEndpoint ----
[Cmdlet(VerbsCommon.Get, "NetUDPEndpoint")]
public sealed class GetNetUDPEndpointCommand : PSCmdlet
{
    [Parameter] public string[]? LocalAddress { get; set; }
    [Parameter] public ushort[]? LocalPort    { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetUDPEndpoint");
}

// ---- Get-NetUDPSetting ----
[Cmdlet(VerbsCommon.Get, "NetUDPSetting")]
public sealed class GetNetUDPSettingCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Get-NetUDPSetting");
}

// ---- New-NetTransportFilter ----
[Cmdlet(VerbsCommon.New, "NetTransportFilter", SupportsShouldProcess = true)]
public sealed class NewNetTransportFilterCommand : PSCmdlet
{
    [Parameter(Mandatory = true)] public string? SettingName { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "New-NetTransportFilter");
}

// ---- Remove-NetTransportFilter ----
[Cmdlet(VerbsCommon.Remove, "NetTransportFilter", SupportsShouldProcess = true)]
public sealed class RemoveNetTransportFilterCommand : PSCmdlet
{
    [Parameter(Mandatory = true)] public string? SettingName { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Remove-NetTransportFilter");
}

// ---- Set-NetIPAddress ----
[Cmdlet(VerbsCommon.Set, "NetIPAddress", SupportsShouldProcess = true)]
public sealed class SetNetIPAddressCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string? IPAddress    { get; set; }
    [Parameter]                                 public int     PrefixLength { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetIPAddress");
}

// ---- Set-NetIPInterface ----
[Cmdlet(VerbsCommon.Set, "NetIPInterface", SupportsShouldProcess = true)]
public sealed class SetNetIPInterfaceCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string[]? InterfaceAlias { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetIPInterface");
}

// ---- Set-NetIPv4Protocol ----
[Cmdlet(VerbsCommon.Set, "NetIPv4Protocol", SupportsShouldProcess = true)]
public sealed class SetNetIPv4ProtocolCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetIPv4Protocol");
}

// ---- Set-NetIPv6Protocol ----
[Cmdlet(VerbsCommon.Set, "NetIPv6Protocol", SupportsShouldProcess = true)]
public sealed class SetNetIPv6ProtocolCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetIPv6Protocol");
}

// ---- Set-NetNeighbor ----
[Cmdlet(VerbsCommon.Set, "NetNeighbor", SupportsShouldProcess = true)]
public sealed class SetNetNeighborCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string? IPAddress        { get; set; }
    [Parameter]                                 public string? InterfaceAlias   { get; set; }
    [Parameter]                                 public string? LinkLayerAddress { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetNeighbor");
}

// ---- Set-NetOffloadGlobalSetting ----
[Cmdlet(VerbsCommon.Set, "NetOffloadGlobalSetting", SupportsShouldProcess = true)]
public sealed class SetNetOffloadGlobalSettingCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetOffloadGlobalSetting");
}

// ---- Set-NetRoute ----
[Cmdlet(VerbsCommon.Set, "NetRoute", SupportsShouldProcess = true)]
public sealed class SetNetRouteCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string? DestinationPrefix { get; set; }
    [Parameter]                                 public string? InterfaceAlias    { get; set; }
    [Parameter]                                 public int     RouteMetric       { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetRoute");
}

// ---- Set-NetTCPSetting ----
[Cmdlet(VerbsCommon.Set, "NetTCPSetting", SupportsShouldProcess = true)]
public sealed class SetNetTCPSettingCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string? SettingName { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetTCPSetting");
}

// ---- Set-NetUDPSetting ----
[Cmdlet(VerbsCommon.Set, "NetUDPSetting", SupportsShouldProcess = true)]
public sealed class SetNetUDPSettingCommand : PSCmdlet
{
    protected override void ProcessRecord() => StubHelper.Stub(this, "Set-NetUDPSetting");
}

// ---- Test-NetConnection ----
[Cmdlet(VerbsDiagnostic.Test, "NetConnection")]
public sealed class TestNetConnectionCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string? ComputerName { get; set; }
    [Parameter]               public int     Port         { get; set; }
    protected override void ProcessRecord() => StubHelper.Stub(this, "Test-NetConnection");
}
