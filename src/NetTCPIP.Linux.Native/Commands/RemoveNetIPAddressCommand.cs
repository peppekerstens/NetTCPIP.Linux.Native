// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        var (exit, stderr) = IpHelpers.RemoveIPAddress(IPAddress, PrefixLength, InterfaceAlias);
        if (exit != 0)
        {
            if (IpHelpers.IsPermissionDenied(exit, stderr))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Remove-NetIPAddress requires root privileges."),
                    "ElevationRequired", ErrorCategory.PermissionDenied, IPAddress));
                return;
            }
            WriteError(new ErrorRecord(
                new InvalidOperationException($"ip addr del failed (exit {exit}): {stderr.Trim()}"),
                "RemoveIPAddressFailed", ErrorCategory.InvalidOperation, IPAddress));
        }
    }
}
