// Commands/NewNetIPAddressCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.New, "NetIPAddress", SupportsShouldProcess = true)]
[OutputType(typeof(NetIPAddress))]
public sealed class NewNetIPAddressCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string IPAddress      { get; set; } = string.Empty;
    [Parameter(Mandatory = true)]               public int    PrefixLength   { get; set; }
    [Parameter(Mandatory = true)]               public string InterfaceAlias { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        if (!ShouldProcess($"{IPAddress}/{PrefixLength} on {InterfaceAlias}", "New-NetIPAddress")) return;
        try { IpHelpers.AddIPAddress(IPAddress, PrefixLength, InterfaceAlias); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "AddIPAddressFailed", ErrorCategory.InvalidOperation, IPAddress)); return; }
        var result = IpHelpers.GetIPAddresses().FirstOrDefault(r =>
            r.IPAddress == IPAddress && r.InterfaceAlias == InterfaceAlias);
        if (result != null) WriteObject(result);
    }
}
