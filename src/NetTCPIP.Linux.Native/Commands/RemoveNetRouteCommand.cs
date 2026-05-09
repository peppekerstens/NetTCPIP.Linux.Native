// Commands/RemoveNetRouteCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Remove, "NetRoute", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
public sealed class RemoveNetRouteCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)] public string DestinationPrefix { get; set; } = string.Empty;
    [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]               public string InterfaceAlias    { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(DestinationPrefix, "Remove-NetRoute")) return;
        try { IpHelpers.RemoveRoute(DestinationPrefix, InterfaceAlias); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "RemoveRouteFailed", ErrorCategory.InvalidOperation, DestinationPrefix)); }
    }
}
