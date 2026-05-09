# NetTCPIP.Linux.Native

PowerShell C# binary module providing Windows-compatible `NetTCPIP` cmdlets for Linux via `ip` and `ss`.

## Cmdlets

| Cmdlet | Status | Backend |
|---|---|---|
| `Get-NetIPAddress` | ✅ Full | `ip -json addr show` |
| `Get-NetIPConfiguration` | ✅ Full | `ip -json addr show` + `ip -json route show` |
| `Get-NetRoute` | ✅ Full | `ip -json route show` (IPv4 + IPv6) |
| `Get-NetTCPConnection` | ✅ Full | `ss -tnap` |
| `New-NetIPAddress` | ✅ Full | `ip addr add` |
| `Remove-NetIPAddress` | ✅ Full | `ip addr del` |
| `New-NetRoute` | ✅ Full | `ip route add` |
| `Remove-NetRoute` | ✅ Full | `ip route del` |
| `New-NetNeighbor` | ✅ Full | `ip neigh add` |
| `Remove-NetNeighbor` | ✅ Full | `ip neigh del` |
| `Find-NetRoute` | ⚠️ Stub | — |
| `Get-NetCompartment` | ⚠️ Stub | — |
| `Get-NetIPInterface` | ⚠️ Stub | — |
| `Get-NetIPv4Protocol` | ⚠️ Stub | — |
| `Get-NetIPv6Protocol` | ⚠️ Stub | — |
| `Get-NetNeighbor` | ⚠️ Stub | — |
| `Get-NetOffloadGlobalSetting` | ⚠️ Stub | — |
| `Get-NetPrefixPolicy` | ⚠️ Stub | — |
| `Get-NetTCPSetting` | ⚠️ Stub | — |
| `Get-NetTransportFilter` | ⚠️ Stub | — |
| `Get-NetUDPEndpoint` | ⚠️ Stub | — |
| `Get-NetUDPSetting` | ⚠️ Stub | — |
| `New-NetTransportFilter` | ⚠️ Stub | — |
| `Remove-NetTransportFilter` | ⚠️ Stub | — |
| `Set-NetIPAddress` | ⚠️ Stub | — |
| `Set-NetIPInterface` | ⚠️ Stub | — |
| `Set-NetIPv4Protocol` | ⚠️ Stub | — |
| `Set-NetIPv6Protocol` | ⚠️ Stub | — |
| `Set-NetNeighbor` | ⚠️ Stub | — |
| `Set-NetOffloadGlobalSetting` | ⚠️ Stub | — |
| `Set-NetRoute` | ⚠️ Stub | — |
| `Set-NetTCPSetting` | ⚠️ Stub | — |
| `Set-NetUDPSetting` | ⚠️ Stub | — |
| `Test-NetConnection` | ⚠️ Stub | — |

Stubs write a `NotSupportedException` `ErrorRecord` and are intentionally not silent.

## Usage

```powershell
# Build
dotnet build src/NetTCPIP.Linux.Native --configuration Release

# Import
Import-Module ./src/NetTCPIP.Linux.Native/bin/Release/net8.0/NetTCPIP.Linux.Native.dll

# List all IP addresses
Get-NetIPAddress

# List IPv4 only
Get-NetIPAddress -AddressFamily IPv4

# Show routing table
Get-NetRoute

# Show TCP connections
Get-NetTCPConnection -State Listen

# Interface summary
Get-NetIPConfiguration

# Add/remove an IP address (requires root)
New-NetIPAddress -IPAddress '10.10.10.1' -PrefixLength 24 -InterfaceAlias 'eth0'
Remove-NetIPAddress -IPAddress '10.10.10.1' -PrefixLength 24 -InterfaceAlias 'eth0'
```

## Requirements

- Linux with `iproute2` (`ip`) and `ss` (from `iproute2` or `socket_statistics`)
- .NET 8 SDK (build) / Runtime (run)
- PowerShell 7.2+
- Root for write operations (`New-`/`Remove-` cmdlets)

## Stage

Part of the [opencode](https://github.com/peppekerstens/opencode) multi-stage project:
Tier 2 Priority 3 — C# binary module porting the Stage 1 PowerShell implementation.
