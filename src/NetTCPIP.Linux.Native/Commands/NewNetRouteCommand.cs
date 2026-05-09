// Commands/NewNetRouteCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.New, "NetRoute", SupportsShouldProcess = true)]
[OutputType(typeof(NetRoute))]
public sealed class NewNetRouteCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)] public string  DestinationPrefix { get; set; } = string.Empty;
    [Parameter(Mandatory = true)]               public string  InterfaceAlias    { get; set; } = string.Empty;
    [Parameter]                                 public string? NextHop           { get; set; }
    [Parameter]                                 public int     RouteMetric       { get; set; }

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(DestinationPrefix, "New-NetRoute")) return;
        try { IpHelpers.AddRoute(DestinationPrefix, NextHop, InterfaceAlias, RouteMetric); }
        catch (Exception ex) { WriteError(new ErrorRecord(ex, "AddRouteFailed", ErrorCategory.InvalidOperation, DestinationPrefix)); return; }
        var result = IpHelpers.GetRoutes().FirstOrDefault(r =>
            r.DestinationPrefix == DestinationPrefix && r.InterfaceAlias == InterfaceAlias);
        if (result != null) WriteObject(result);
    }
}
