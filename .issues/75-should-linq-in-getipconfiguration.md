---
name: SHOULD — LINQ used in GetIPConfiguration
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 17
- **Rule name:** Avoid LINQ and params arrays in performance-sensitive code

## Location
- **File:** `src/NetTCPIP.Linux.Native/Helpers/IpHelpers.cs`, lines 241-252

## What's wrong
`GetIPConfiguration()` uses `.Where()`, `.OrderBy()`, `.Select()`, `.GroupBy()`, `.ToDictionary()` LINQ. Rule 17 requires foreach loops.

## How to fix
Replace LINQ chains with foreach loops.

## Severity
- [ ] SHOULD — should be fixed before merge
