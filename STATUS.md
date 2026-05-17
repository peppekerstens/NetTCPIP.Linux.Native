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

**Fixed (2026-05-16, commit `9f0895a`):**
- `NetIPAddress.AddressState` changed from `string` to `AddressState` enum
- `NetRoute.TypeOfNextHop` changed from `string` to `NextHopType` enum
- `NetTCPConnection.State` changed from `string` to `TcpState` enum
- `NetTCPConnection.OffloadState` changed from `string` to `OffloadState` enum
- Updated cmdlet parameters (`Get-NetRoute -TypeOfNextHop`, `Get-NetTCPConnection -State`) to use enum types
- Updated Pester test valid State values list (removed `'Bound'`)

---

## Known Issues

None identified locally.

**Tracked on GitHub:** [Issues #1–#9](https://github.com/peppekerstens/NetTCPIP.Linux.Native/issues) (3 MUST, 6 SHOULD from code audit)

## Next Steps

No critical Rule 9 gaps — fully compliant.

---

## Reference

| Resource | Location |
|---|---|
| Source code | `src/NetTCPIP.Linux.Native/` |
| Tests | `tests/NetTCPIP.Linux.Native.Tests/` |
| Linux rules | `docs/linux-rules.md` |
| Coordination repo | `https://github.com/peppekerstens/opencode` |
