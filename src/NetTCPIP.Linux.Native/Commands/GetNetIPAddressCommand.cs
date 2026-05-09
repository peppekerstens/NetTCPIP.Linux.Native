// Commands/GetNetIPAddressCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Get, "NetIPAddress")]
[OutputType(typeof(NetIPAddress))]
public sealed class GetNetIPAddressCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? IPAddress       { get; set; }
    [Parameter]                public string[]? InterfaceAlias  { get; set; }
    [Parameter]                public int[]?    InterfaceIndex  { get; set; }
    [Parameter][ValidateSet("IPv4","IPv6")] public string? AddressFamily { get; set; }

    protected override void ProcessRecord()
    {
        var results = IpHelpers.GetIPAddresses();

        if (IPAddress      != null) results = results.Where(r => IPAddress.Contains(r.IPAddress, StringComparer.OrdinalIgnoreCase)).ToList();
        if (InterfaceAlias != null) results = results.Where(r => InterfaceAlias.Any(a => WildMatch(r.InterfaceAlias, a))).ToList();
        if (InterfaceIndex != null) results = results.Where(r => InterfaceIndex.Contains(r.InterfaceIndex)).ToList();
        if (AddressFamily  != null) results = results.Where(r => r.AddressFamily == AddressFamily).ToList();

        foreach (var r in results) WriteObject(r);
    }

    private static bool WildMatch(string value, string pattern)
        => new System.Management.Automation.WildcardPattern(pattern,
            WildcardOptions.IgnoreCase).IsMatch(value);
}
