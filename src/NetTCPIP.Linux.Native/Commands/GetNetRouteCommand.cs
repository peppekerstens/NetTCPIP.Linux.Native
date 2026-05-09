// Commands/GetNetRouteCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Get, "NetRoute")]
[OutputType(typeof(NetRoute))]
public sealed class GetNetRouteCommand : PSCmdlet
{
    [Parameter(Position = 0)] public string[]? DestinationPrefix { get; set; }
    [Parameter]                public string[]? InterfaceAlias    { get; set; }
    [Parameter]                public int[]?    InterfaceIndex    { get; set; }
    [Parameter]                public string[]? NextHop           { get; set; }
    [Parameter][ValidateSet("IPv4","IPv6")] public string? AddressFamily   { get; set; }
    [Parameter][ValidateSet("Connected","Unreachable","Remote")] public string? TypeOfNextHop { get; set; }

    protected override void ProcessRecord()
    {
        var results = IpHelpers.GetRoutes();

        if (DestinationPrefix != null) results = results.Where(r => DestinationPrefix.Contains(r.DestinationPrefix, StringComparer.OrdinalIgnoreCase)).ToList();
        if (InterfaceAlias    != null) results = results.Where(r => InterfaceAlias.Any(a => WildMatch(r.InterfaceAlias, a))).ToList();
        if (InterfaceIndex    != null) results = results.Where(r => InterfaceIndex.Contains(r.InterfaceIndex)).ToList();
        if (NextHop           != null) results = results.Where(r => NextHop.Contains(r.NextHop, StringComparer.OrdinalIgnoreCase)).ToList();
        if (AddressFamily     != null) results = results.Where(r => r.AddressFamily == AddressFamily).ToList();
        if (TypeOfNextHop     != null) results = results.Where(r => r.TypeOfNextHop == TypeOfNextHop).ToList();

        foreach (var r in results) WriteObject(r);
    }

    private static bool WildMatch(string value, string pattern)
        => new System.Management.Automation.WildcardPattern(pattern,
            WildcardOptions.IgnoreCase).IsMatch(value);
}
