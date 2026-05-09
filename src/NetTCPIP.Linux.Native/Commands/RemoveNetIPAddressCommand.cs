// Commands/RemoveNetIPAddressCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Remove, "NetIPAddress", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
public sealed class RemoveNetIPAddressCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)] public string IPAddress      { get; set; } = string.Empty;
    [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]               public int    PrefixLength   { get; set; }
    [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]               public string InterfaceAlias { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        if (!ShouldProcess($"{IPAddress}/{PrefixLength} on {InterfaceAlias}", "Remove-NetIPAddress")) return;
        try { IpHelpers.RemoveIPAddress(IPAddress, PrefixLength, InterfaceAlias); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "RemoveIPAddressFailed", ErrorCategory.InvalidOperation, IPAddress)); }
    }
}
