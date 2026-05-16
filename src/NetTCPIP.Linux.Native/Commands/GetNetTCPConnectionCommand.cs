// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/GetNetTCPConnectionCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Get, "NetTCPConnection")]
[OutputType(typeof(NetTCPConnection))]
public sealed class GetNetTCPConnectionCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? LocalAddress  { get; set; }
    [Parameter]                public ushort[]? LocalPort     { get; set; }
    [Parameter]                public string[]? RemoteAddress { get; set; }
    [Parameter]                public ushort[]? RemotePort    { get; set; }
    [Parameter][ValidateSet("Closed","Listen","SynSent","SynReceived","Established","FinWait1","FinWait2","CloseWait","Closing","LastAck","TimeWait","DeleteTCB")]
    public TcpState[]? State { get; set; }
    [Parameter] public uint[]? OwningProcess { get; set; }

    protected override void ProcessRecord()
    {
        var results = IpHelpers.GetTcpConnections();

        if (LocalAddress  != null) results = results.Where(r => LocalAddress.Contains(r.LocalAddress,   StringComparer.OrdinalIgnoreCase)).ToList();
        if (LocalPort     != null) results = results.Where(r => LocalPort.Contains(r.LocalPort)).ToList();
        if (RemoteAddress != null) results = results.Where(r => RemoteAddress.Contains(r.RemoteAddress, StringComparer.OrdinalIgnoreCase)).ToList();
        if (RemotePort    != null) results = results.Where(r => RemotePort.Contains(r.RemotePort)).ToList();
        if (State         != null) results = results.Where(r => State.Contains(r.State)).ToList();
        if (OwningProcess != null) results = results.Where(r => OwningProcess.Contains(r.OwningProcess)).ToList();

        foreach (var r in results) WriteObject(r);
    }
}
