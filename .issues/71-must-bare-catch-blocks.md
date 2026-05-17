---
name: MUST — Bare catch blocks swallow errors silently
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 7
- **Rule name:** No silent error swallowing

## Location
- **File:** `src/NetTCPIP.Linux.Native/Helpers/IpHelpers.cs`, lines 72, 120, 156, 224, 330, 473

## What's wrong
Bare `catch { /* on non-Linux, return empty */ }` and `catch { return (string.Empty, string.Empty, -1); }` silently swallow errors.

## How to fix
Replace with `WriteWarning()` or justified comments explaining why the error is non-critical (e.g., "on non-Linux platform, return empty").

## Severity
- [x] MUST — blocks merge
