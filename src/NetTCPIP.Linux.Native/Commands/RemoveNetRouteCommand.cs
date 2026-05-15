// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        var (exit, stderr) = IpHelpers.RemoveRoute(DestinationPrefix, InterfaceAlias);
        if (exit != 0)
        {
            if (IpHelpers.IsPermissionDenied(exit, stderr))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Remove-NetRoute requires root privileges."),
                    "ElevationRequired", ErrorCategory.PermissionDenied, DestinationPrefix));
                return;
            }
            WriteError(new ErrorRecord(
                new InvalidOperationException($"ip route del failed (exit {exit}): {stderr.Trim()}"),
                "RemoveRouteFailed", ErrorCategory.InvalidOperation, DestinationPrefix));
        }
    }
}
