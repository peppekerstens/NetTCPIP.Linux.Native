---
name: SHOULD — Field naming: ProcStateMap should be s_procStateMap
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 19
- **Rule name:** Field naming — s_ for static, _ for instance

## Location
- **File:** `src/NetTCPIP.Linux.Native/Helpers/IpHelpers.cs`, line 166

## What's wrong
`ProcStateMap` is a static readonly field but lacks the `s_` prefix.

## How to fix
Rename to `s_procStateMap`.

## Severity
- [ ] SHOULD — should be fixed before merge
