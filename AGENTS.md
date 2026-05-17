# NetTCPIP.Linux.Native — Contributor Guide

## What this module is

A C# binary PowerShell module providing Linux `*-NetIPAddress`, `*-NetRoute`, `*-NetTCPConnection`, and `*-NetIPConfiguration` cmdlets via `ip` subprocess. 10 cmdlets + 24 stubs. Designed as a drop-in replacement for Windows `NetTCPIP` on Linux.

Part of the [PowerShell Linux Commands](https://github.com/peppekerstens/opencode) project.

---

## Quick Start

```bash
# Build
dotnet build -c Release

# Run tests (requires pwsh)
pwsh -c "Import-Module ./src/NetTCPIP.Linux.Native/bin/Release/net8.0/NetTCPIP.Linux.Native.dll; Invoke-Pester ./tests/"
```

---

## Architecture

```
src/NetTCPIP.Linux.Native/
├── Commands/          # Cmdlet implementations (10 + 24 stubs)
│   ├── GetNetIPAddressCommand.cs
│   ├── NewNetIPAddressCommand.cs
│   ├── RemoveNetIPAddressCommand.cs
│   ├── GetNetRouteCommand.cs
│   ├── NewNetRouteCommand.cs
│   ├── RemoveNetRouteCommand.cs
│   ├── GetNetTCPConnectionCommand.cs
│   ├── GetNetIPConfigurationCommand.cs
│   ├── NewNetNeighborCommand.cs
│   ├── RemoveNetNeighborCommand.cs
│   └── StubCommands.cs         # 24 stub cmdlets
├── Helpers/
│   └── IpHelpers.cs            # ip subprocess runner, output parsing
└── Models/
    └── NetTCPIPModels.cs       # NetIPAddress, NetRoute, NetTCPConnection, NetIPConfiguration
```

### Key design decisions

- **`ip` subprocess** — `ip addr`, `ip route`, `ss`, `ip neigh` for network state. Parsed output into POCOs.
- **Type alignment (Rule 9)** — `AddressState`, `NextHopType`, `TcpState`, `OffloadState` are enums (not strings). POCOs match Windows CIM property names.
- **Elevation** — Subprocess permission errors translated to `"CmdletName requires root privileges."` (Error ID: `UnauthorizedAccess`).
- **Stubs** — 24 cmdlets throw `NotImplementedException` — no `SupportsShouldProcess` on stubs.

---

## C# Conventions

| Rule | Detail |
|---|---|
| **Target** | `net8.0`, `TreatWarningsAsErrors=true`, `Deterministic=true` |
| **SMA** | Pinned to `7.4.6` exactly |
| **Namespaces** | File-scoped (`namespace Foo;`) |
| **Process** | `ProcessStartInfo.ArgumentList` only, `ReadToEndAsync()` on stdout/stderr |
| **Cmdlets** | `SupportsShouldProcess` on write cmdlets only, stubs throw `NotImplementedException` |
| **Async** | `ConfigureAwait(false)` on all async methods |
| **Errors** | `ErrorRecord` with `UnauthorizedAccess` ID, `SecurityError` category |
| **Copyright** | `// Copyright (c) peppekerstens. All rights reserved.` |

Full rules: `docs/linux-rules.md`

### Version alignment
- **Single source of truth:** `<Version>` in `.csproj`
- **Must match:** `STATUS.md` `**Version:**` line, README.md version history table (latest entry)
- **Bump rule:** `.csproj` first, then `STATUS.md`, then README.md — in that order

---

## Testing

- **Framework:** Pester 5
- **Runner:** `pwsh -c "Invoke-Pester ./tests/"`
- **GHA:** 5-distro matrix + Windows
- **Test file:** `tests/NetTCPIP.Linux.Native.Tests.ps1`

---

## Current State

See `STATUS.md` for module state, open issues, and next steps.

**Open issues:** 0 — fully compliant with all rules.

---

## Boundaries

### What lives in this repo
- Source code (`src/NetTCPIP.Linux.Native/`)
- Pester tests (`tests/`)
- CI/CD (`.github/workflows/`)
- Module status (`STATUS.md`)
- Contributor guide (`AGENTS.md`)
- Development rules (`docs/linux-rules.md`)
- OpenCode config (`.opencode/`)

### What lives elsewhere
- Cross-repo planning, status aggregation, project plan → https://github.com/peppekerstens/opencode
- Other modules → https://github.com/peppekerstens/
- Blog posts → https://peppekerstens.github.io

### What to do when
| Scenario | Where |
|---|---|
| Bug in this module | File issue in **this repo** |
| Feature request for this module | File issue in **this repo** |
| Cross-module convention change | File issue in **opencode** |

---

## Coordination

This module is part of a larger project. Cross-repo planning lives at:
- **Coordination repo:** https://github.com/peppekerstens/opencode
- **Project plan:** https://github.com/peppekerstens/opencode/blob/main/plan.md
