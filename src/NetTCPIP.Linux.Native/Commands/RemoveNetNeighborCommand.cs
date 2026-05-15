// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        var (exit, stderr) = IpHelpers.RemoveNeighbor(IPAddress, InterfaceAlias);
        if (exit != 0)
        {
            if (IpHelpers.IsPermissionDenied(exit, stderr))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Remove-NetNeighbor requires root privileges."),
                    "ElevationRequired", ErrorCategory.PermissionDenied, IPAddress));
                return;
            }
            WriteError(new ErrorRecord(
                new InvalidOperationException($"ip neigh del failed (exit {exit}): {stderr.Trim()}"),
                "RemoveNeighborFailed", ErrorCategory.InvalidOperation, IPAddress));
        }
    }
}
