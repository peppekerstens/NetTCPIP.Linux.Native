// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/NewNetNeighborCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.New, "NetNeighbor", SupportsShouldProcess = true)]
public sealed class NewNetNeighborCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string IPAddress        { get; set; } = string.Empty;
    [Parameter(Mandatory = true)]               public string LinkLayerAddress { get; set; } = string.Empty;
    [Parameter(Mandatory = true)]               public string InterfaceAlias   { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        if (!ShouldProcess($"{IPAddress} on {InterfaceAlias}", "New-NetNeighbor")) return;
        try { IpHelpers.AddNeighbor(IPAddress, LinkLayerAddress, InterfaceAlias); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "AddNeighborFailed", ErrorCategory.InvalidOperation, IPAddress)); }
    }
}
