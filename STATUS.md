# NetTCPIP.Linux.Native — Module Status

**Last updated:** 2026-05-16
**Version:** 1.0.0
**GHA Build:** ✅ green
**GHA Pester:** ✅ green (5-distro + Windows)

---

## Current State

10 cmdlets + 24 stubs implemented via `ip` subprocess. All write cmdlets translate elevation errors to `"CmdletName requires root privileges."`

### Output Types

| Type | Inherits | Windows Counterpart | Rule 9 Status |
|---|---|---|---|
| `NetIPAddress` | `object` | `CimInstance#MSFT_NetIPAddress` | ✅ Compliant (property names match) |
| `NetRoute` | `object` | `CimInstance#MSFT_NetRoute` | ✅ Compliant (property names match) |
| `NetTCPConnection` | `object` | `CimInstance#MSFT_NetTCPConnection` | ✅ Compliant (property names match) |
| `NetIPConfiguration` | `object` | `CimInstance#MSFT_NetIPConfiguration` | ✅ Compliant (property names match) |

### Rule 9 Compliance

Windows NetTCPIP returns `CimInstance` objects — no .NET class hierarchy to inherit from. Linux POCOs already mirror CIM property names exactly.

**Minor gaps (cosmetic):**
- `NetIPAddress.AddressState` is `string` (Windows: enum via CIM)
- `NetRoute.TypeOfNextHop` is `string` (Windows: enum via CIM)
- `NetTCPConnection.State` is `string` (Windows: `MibTcpState` enum)
- `NetTCPConnection.OffloadState` is `string` (Windows: enum via CIM)

These are low-priority string-vs-enum mismatches. No inheritance change needed.

---

## Known Issues

None.

## Next Steps

1. (Optional) Convert string-typed states to proper enums
2. No critical Rule 9 gaps — property alignment is sufficient

---

## Reference

| Resource | Location |
|---|---|
| Source code | `src/NetTCPIP.Linux.Native/` |
| Tests | `tests/NetTCPIP.Linux.Native.Tests/` |
| Linux rules | `docs/linux-rules.md` |
| Coordination repo | `https://github.com/peppekerstens/opencode` |
