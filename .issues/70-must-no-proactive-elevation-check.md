---
name: MUST — No proactive elevation check on write cmdlets
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 1
- **Rule name:** Elevation checks are mandatory for write operations

## Location
- **Files:** `src/NetTCPIP.Linux.Native/Commands/NewNetIPAddressCommand.cs`, `RemoveNetIPAddressCommand.cs`, `NewNetRouteCommand.cs`, `RemoveNetRouteCommand.cs`, `NewNetNeighborCommand.cs`, `RemoveNetNeighborCommand.cs`

## What's wrong
Write cmdlets don't check elevation before acting. `ip addr/route/neigh` commands require root.

## How to fix
Add `Utils.IsAdministrator()` check at the start of each write cmdlet's `ProcessRecord()`.

## Severity
- [x] MUST — blocks merge
