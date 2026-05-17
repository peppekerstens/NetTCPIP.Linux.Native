---
name: MUST — No OperatingSystem.IsWindows() guard on any cmdlet
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 8
- **Rule name:** Platform branching at the top of ProcessRecord()

## Location
- **Files:** All 10 cmdlets + stubs in `src/NetTCPIP.Linux.Native/Commands/`

## What's wrong
No cmdlet has `OperatingSystem.IsWindows()` branching. Windows has a built-in `NetTCPIP` module.

## How to fix
Add at the top of each `ProcessRecord()`:
```csharp
if (OperatingSystem.IsWindows())
{
    string cmdletName = MyInvocation.MyCommand.Name;
    InvokeCommand.InvokeScript($"NetTCPIP\\{cmdletName}");
    return;
}
```

## Severity
- [x] MUST — blocks merge
