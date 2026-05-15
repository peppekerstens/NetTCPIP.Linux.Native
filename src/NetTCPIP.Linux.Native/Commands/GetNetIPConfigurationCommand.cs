// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/GetNetIPConfigurationCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

[Cmdlet(VerbsCommon.Get, "NetIPConfiguration")]
[OutputType(typeof(NetIPConfiguration))]
public sealed class GetNetIPConfigurationCommand : PSCmdlet
{
    [Parameter(Position = 0)] [SupportsWildcards] public string[]? InterfaceAlias  { get; set; }
    [Parameter]                                   public int[]?    InterfaceIndex  { get; set; }
    [Parameter]                                   public SwitchParameter Detailed  { get; set; }
    [Parameter]                                   public SwitchParameter All       { get; set; }

    protected override void ProcessRecord()
    {
        var results = IpHelpers.GetIPConfiguration(All.IsPresent);

        if (InterfaceAlias != null) results = results.Where(r => InterfaceAlias.Any(a => WildMatch(r.InterfaceAlias, a))).ToList();
        if (InterfaceIndex != null) results = results.Where(r => InterfaceIndex.Contains(r.InterfaceIndex)).ToList();

        foreach (var r in results) WriteObject(r);
    }

    private static bool WildMatch(string value, string pattern)
        => new System.Management.Automation.WildcardPattern(pattern,
            WildcardOptions.IgnoreCase).IsMatch(value);
}
