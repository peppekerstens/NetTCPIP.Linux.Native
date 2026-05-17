# NetTCPIP.Linux.Native

[![Pester Tests](https://github.com/peppekerstens/NetTCPIP.Linux.Native/actions/workflows/pester.yml/badge.svg)](https://github.com/peppekerstens/NetTCPIP.Linux.Native/actions/workflows/pester.yml)

> Native C# binary module implementing Windows-compatible `NetTCPIP` cmdlets for Linux via `iproute2` and `/proc` filesystem parsing.

This is the Tier 2 (C# native) successor to [`NetTCPIP.Linux`](https://github.com/peppekerstens/NetTCPIP.Linux), part of Stage 5 of the [PowerShell Linux Commands](https://peppekerstens.github.io) project.

---

## What it does

Provides the core `NetTCPIP` cmdlet surface for Linux. Read operations (`Get-*`) parse `/proc/net/` files and use the BCL's `NetworkInterface` APIs — no subprocesses for enumeration. Write operations (`New-`/`Remove-`) invoke `ip` from `iproute2`.

| Cmdlet | Status | Backend | Notes |
|---|---|---|---|
| `Get-NetIPAddress` | Full | BCL `NetworkInterface` + `/proc` | IPv4 + IPv6; wildcard filter |
| `Get-NetIPConfiguration` | Full | BCL `NetworkInterface` + `/proc/net/route` | Per-interface summary |
| `Get-NetRoute` | Full | `/proc/net/route` (IPv4) + `/proc/net/ipv6_route` (IPv6) | |
| `Get-NetTCPConnection` | Full | `/proc/net/tcp` + `/proc/net/tcp6` + `/proc/<pid>/fd/` | State, local/remote endpoint, PID |
| `New-NetIPAddress` | Full | `ip addr add` | Requires root |
| `Remove-NetIPAddress` | Full | `ip addr del` | Requires root |
| `New-NetRoute` | Full | `ip route add` | Requires root |
| `Remove-NetRoute` | Full | `ip route del` | Requires root |
| `New-NetNeighbor` | Full | `ip neigh add` | Requires root |
| `Remove-NetNeighbor` | Full | `ip neigh del` | Requires root |
| `Find-NetRoute` | Stub | — | |
| `Get-NetCompartment` | Stub | — | |
| `Get-NetIPInterface` | Stub | — | |
| `Get-NetIPv4Protocol` | Stub | — | |
| `Get-NetIPv6Protocol` | Stub | — | |
| `Get-NetNeighbor` | Stub | — | |
| `Get-NetOffloadGlobalSetting` | Stub | — | |
| `Get-NetPrefixPolicy` | Stub | — | |
| `Get-NetTCPSetting` | Stub | — | |
| `Get-NetTransportFilter` | Stub | — | |
| `Get-NetUDPEndpoint` | Stub | — | |
| `Get-NetUDPSetting` | Stub | — | |
| `New-NetTransportFilter` | Stub | — | |
| `Remove-NetTransportFilter` | Stub | — | |
| `Set-NetIPAddress` | Stub | — | |
| `Set-NetIPInterface` | Stub | — | |
| `Set-NetIPv4Protocol` | Stub | — | |
| `Set-NetIPv6Protocol` | Stub | — | |
| `Set-NetNeighbor` | Stub | — | |
| `Set-NetOffloadGlobalSetting` | Stub | — | |
| `Set-NetRoute` | Stub | — | |
| `Set-NetTCPSetting` | Stub | — | |
| `Set-NetUDPSetting` | Stub | — | |
| `Test-NetConnection` | Stub | — | |

Stubs write a `NotSupportedException` error record — they are intentionally not silent.

All write cmdlets support `-WhatIf` and `-Confirm`.

---

## Requirements

- Linux with `iproute2` (`ip` command) for write operations
- PowerShell 7.4+, .NET 8
- Root for all `New-`/`Remove-` cmdlets

---

## Installation

```powershell
git clone https://github.com/peppekerstens/NetTCPIP.Linux.Native
dotnet build NetTCPIP.Linux.Native/src/NetTCPIP.Linux.Native --configuration Release
Import-Module ./NetTCPIP.Linux.Native/src/NetTCPIP.Linux.Native/bin/Release/net8.0/NetTCPIP.Linux.Native.dll
```

---

## Usage

```powershell
# List all IP addresses
Get-NetIPAddress

# Filter IPv4 only
Get-NetIPAddress -AddressFamily IPv4

# Show the routing table
Get-NetRoute

# Show listening TCP ports
Get-NetTCPConnection -State Listen

# Per-interface summary
Get-NetIPConfiguration

# Add an IP address (root required)
New-NetIPAddress -IPAddress '10.10.10.1' -PrefixLength 24 -InterfaceAlias 'eth0'

# Remove an IP address (root required)
Remove-NetIPAddress -IPAddress '10.10.10.1' -PrefixLength 24 -InterfaceAlias 'eth0'

# Add a static route (root required)
New-NetRoute -DestinationPrefix '192.168.100.0/24' -NextHop '10.10.10.254' -InterfaceAlias 'eth0'
```

---

## Manual Testing

For a detailed, step-by-step guide on setting up your environment and testing these modules, see the blog post: [Testing the native layer](https://peppekerstens.github.io/blog/testing-the-native-layer).

### Option 1: Interactive Container (Recommended)
Use the pre-built CI images to avoid dependency issues.

```powershell
# Start an interactive shell in the Ubuntu 24.04 test container
docker compose -f docker-compose.test.yml run ubuntu-24 pwsh
```
Once inside:
```powershell
Import-Module /module/bin/Release/net8.0/publish/NetTCPIP.Linux.Native.dll
Get-NetIPInterface
```

### Option 2: Bare WSL
Test directly in your WSL distro (requires `.NET 8 SDK`).

```powershell
dotnet publish src/NetTCPIP.Linux.Native --configuration Release --output bin/Release/net8.0/publish
pwsh
Import-Module ./bin/Release/net8.0/publish/NetTCPIP.Linux.Native.dll
Get-NetIPInterface
```

---

## CI / Testing


| Distro | Image |
|---|---|
| Ubuntu 24.04 | `ghcr.io/peppekerstens/pwsh-pester-ubuntu:24.04` |
| Debian 12 | `ghcr.io/peppekerstens/pwsh-pester-debian:12` |
| Fedora 40 | `ghcr.io/peppekerstens/pwsh-pester-fedora:40` |
| openSUSE Tumbleweed | `ghcr.io/peppekerstens/pwsh-pester-opensuse:tumbleweed` |
| Arch Linux | `ghcr.io/peppekerstens/pwsh-pester-arch:latest` |

### Test scenarios

| Describe block | Scope | Tests |
|---|---|---|
| Module surface | everywhere | 34 cmdlet export checks |
| Stub cmdlets write ErrorRecord | everywhere | 13 stub error record checks |
| WhatIf safety | everywhere | New/Remove NetIPAddress, New/Remove NetRoute, New/Remove NetNeighbor |
| Get-NetIPAddress | Linux (any user) | Returns addresses; IPAddress, InterfaceAlias, AddressFamily fields; filter by family; loopback 127.0.0.1; wildcard; nonexistent empty |
| Get-NetRoute | Linux (any user) | Returns routes; DestinationPrefix field; AddressFamily filter; default route |
| Get-NetTCPConnection | Linux (any user) | Returns array; LISTEN sockets have RemotePort 0; valid State values |
| Get-NetIPConfiguration | Linux (any user) | Returns interfaces; InterfaceAlias field; loopback excluded by default; -All includes lo; wildcard |
| **Loopback alias add/remove** | **Linux + root** | **New-NetIPAddress adds 127.0.1.99/32 to lo; Get-NetIPAddress sees it; Get-NetIPConfiguration -All sees it; Remove-NetIPAddress removes it; gone from Get-NetIPAddress** |
| **Route round-trip** | **Linux + root** | **New-NetRoute adds 192.0.2.0/24; Get-NetRoute sees it; Remove-NetRoute removes it; gone from Get-NetRoute** |
| **Real TCP listener** | **Linux (any user)** | **TcpListener on port 19753; Get-NetTCPConnection -State Listen finds it; LocalAddress = 127.0.0.1; RemotePort = 0** |

Run locally:

```powershell
Invoke-Pester -Path tests/NetTCPIP.Linux.Native.Tests/ -Output Detailed
```

---

## Implementation Notes

- **Zero subprocesses for reads**: `Get-NetIPAddress` and `Get-NetIPConfiguration` use the BCL's `NetworkInterface` APIs. `Get-NetRoute` parses `/proc/net/route` (IPv4, hex little-endian) and `/proc/net/ipv6_route` (IPv6, 32-char big-endian hex). `Get-NetTCPConnection` parses `/proc/net/tcp` and `/proc/net/tcp6`, then resolves PIDs by scanning `/proc/<pid>/fd/` for `socket:[inode]` symlinks.
- **Write paths keep `ip`**: Netlink socket P/Invoke for route/address mutation is out of scope. `Process.Start(ip)` is the correct and standard approach.
- **`AddressValidLifetime` / `AddressPreferredLifetime`**: Windows-only BCL properties; on Linux these fields are not exposed via the BCL, so the module returns `TimeSpan.MaxValue` (infinite) for both.
- **`ConfirmImpact.High`** on all destructive cmdlets.

---

## Version history

| Version | Changes |
|---|---|
| 0.1.0 | Initial release. 10 full cmdlets, 24 stubs. BCL + `/proc` read paths. `ip` write paths. |
| 0.2.0 | Test expansion. Loopback alias add/remove round-trip; route round-trip; real `TcpListener` port filter for `Get-NetTCPConnection`; `Get-NetIPConfiguration -All` loopback check. |
| 0.3.0 | GHA all-green. Fixed `$script:isLinux` collision → `$script:onLinux`; `fail-fast: false`; `Get-NetTCPConnection` null result → `Should -Not -Throw`; openSUSE image now includes `gawk`+`findutils`. All 5 distros pass. |
| 1.0.0 | Rule 9 compliance: `AddressState`/`NextHopType`/`TcpState`/`OffloadState` string → enum. Elevation error translation. Windows pester tests. `STATUS.md` and `AGENTS.md` added. 22-rule audit passed. |

---

## Related

- [`NetTCPIP.Linux`](https://github.com/peppekerstens/NetTCPIP.Linux) — the Stage 1 PowerShell script wrapper this module replaces
- [opencode project plan](https://github.com/peppekerstens/opencode) — multi-stage project tracking
- [Blog series](https://peppekerstens.github.io) — write-up of the full journey

---

## License

[GNU General Public License v3](LICENSE)
