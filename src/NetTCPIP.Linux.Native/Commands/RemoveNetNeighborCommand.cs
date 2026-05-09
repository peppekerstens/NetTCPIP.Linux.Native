// Commands/RemoveNetNeighborCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Remove, "NetNeighbor", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
public sealed class RemoveNetNeighborCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string IPAddress      { get; set; } = string.Empty;
    [Parameter(Mandatory = true)]               public string InterfaceAlias { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        if (!ShouldProcess($"{IPAddress} on {InterfaceAlias}", "Remove-NetNeighbor")) return;
        try { IpHelpers.RemoveNeighbor(IPAddress, InterfaceAlias); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "RemoveNeighborFailed", ErrorCategory.InvalidOperation, IPAddress)); }
    }
}
