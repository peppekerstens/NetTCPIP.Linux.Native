# NetTCPIP.Linux.Native.Tests.ps1
# Pester 5 test suite for NetTCPIP.Linux.Native.
# Module surface and filter tests run everywhere.
# Integration tests (read-only) run on Linux only.
# Write-operation tests (New/Remove) run on Linux+root only.

BeforeDiscovery {
    $script:onLinux = $IsLinux
    $script:isRoot = $IsLinux -and (
        [System.IO.File]::ReadAllText('/proc/self/status') -match '(?m)^Uid:\s+(\d+)' -and
        $Matches[1] -eq '0')
}

# ---------------------------------------------------------------------------
# Module surface — 34 cmdlets
# ---------------------------------------------------------------------------
Describe 'Module surface' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    $cmdlets = @(
        'Get-NetIPAddress', 'Get-NetIPConfiguration', 'Get-NetRoute', 'Get-NetTCPConnection',
        'Find-NetRoute', 'Get-NetCompartment', 'Get-NetIPInterface',
        'Get-NetIPv4Protocol', 'Get-NetIPv6Protocol', 'Get-NetNeighbor',
        'Get-NetOffloadGlobalSetting', 'Get-NetPrefixPolicy', 'Get-NetTCPSetting',
        'Get-NetTransportFilter', 'Get-NetUDPEndpoint', 'Get-NetUDPSetting',
        'New-NetIPAddress', 'New-NetNeighbor', 'New-NetRoute', 'New-NetTransportFilter',
        'Remove-NetIPAddress', 'Remove-NetNeighbor', 'Remove-NetRoute', 'Remove-NetTransportFilter',
        'Set-NetIPAddress', 'Set-NetIPInterface', 'Set-NetIPv4Protocol', 'Set-NetIPv6Protocol',
        'Set-NetNeighbor', 'Set-NetOffloadGlobalSetting', 'Set-NetRoute',
        'Set-NetTCPSetting', 'Set-NetUDPSetting', 'Test-NetConnection'
    )

    It 'exports <_>' -ForEach $cmdlets {
        Get-Command $_ -Module NetTCPIP.Linux.Native | Should -Not -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Stub behaviour
# ---------------------------------------------------------------------------
Describe 'Stub cmdlets write ErrorRecord' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    $stubs = @(
        { Find-NetRoute -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetCompartment -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetIPInterface -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetIPv4Protocol -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetIPv6Protocol -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetNeighbor -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetOffloadGlobalSetting -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetPrefixPolicy -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetTCPSetting -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetTransportFilter -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetUDPEndpoint -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Get-NetUDPSetting -ErrorVariable e -ErrorAction SilentlyContinue; $e }
        { Test-NetConnection -ErrorVariable e -ErrorAction SilentlyContinue; $e }
    )

    It 'stub <_> writes an error' -ForEach @(
        'Find-NetRoute', 'Get-NetCompartment', 'Get-NetIPInterface',
        'Get-NetIPv4Protocol', 'Get-NetIPv6Protocol', 'Get-NetNeighbor',
        'Get-NetOffloadGlobalSetting', 'Get-NetPrefixPolicy', 'Get-NetTCPSetting',
        'Get-NetTransportFilter', 'Get-NetUDPEndpoint', 'Get-NetUDPSetting',
        'Test-NetConnection'
    ) {
        $err = @()
        & (Get-Command $_) -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
    }
}

# ---------------------------------------------------------------------------
# WhatIf safety
# ---------------------------------------------------------------------------
Describe 'WhatIf safety' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'New-NetIPAddress -WhatIf does not throw' {
        { New-NetIPAddress -IPAddress '192.0.2.1' -PrefixLength 24 -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
    It 'Remove-NetIPAddress -WhatIf does not throw' {
        { Remove-NetIPAddress -IPAddress '192.0.2.1' -PrefixLength 24 -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
    It 'New-NetRoute -WhatIf does not throw' {
        { New-NetRoute -DestinationPrefix '10.0.0.0/8' -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
    It 'Remove-NetRoute -WhatIf does not throw' {
        { Remove-NetRoute -DestinationPrefix '10.0.0.0/8' -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
    It 'New-NetNeighbor -WhatIf does not throw' {
        { New-NetNeighbor -IPAddress '192.0.2.1' -LinkLayerAddress 'aa:bb:cc:dd:ee:ff' -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
    It 'Remove-NetNeighbor -WhatIf does not throw' {
        { Remove-NetNeighbor -IPAddress '192.0.2.1' -InterfaceAlias 'eth0' -WhatIf } | Should -Not -Throw
    }
}

# ---------------------------------------------------------------------------
# Elevation errors — Linux + non-root only
# ---------------------------------------------------------------------------
Describe 'Elevation errors' -Skip:($script:isRoot -or -not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'New-NetIPAddress writes a meaningful error when not root' {
        $err = @()
        New-NetIPAddress -IPAddress '192.0.2.1' -PrefixLength 24 -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'New-NetIPAddress requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.NewNetIPAddressCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Remove-NetIPAddress writes a meaningful error when not root' {
        $err = @()
        Remove-NetIPAddress -IPAddress '192.0.2.1' -PrefixLength 24 -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Remove-NetIPAddress requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.RemoveNetIPAddressCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'New-NetRoute writes a meaningful error when not root' {
        $err = @()
        New-NetRoute -DestinationPrefix '10.0.0.0/8' -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'New-NetRoute requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.NewNetRouteCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Remove-NetRoute writes a meaningful error when not root' {
        $err = @()
        Remove-NetRoute -DestinationPrefix '10.0.0.0/8' -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Remove-NetRoute requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.RemoveNetRouteCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'New-NetNeighbor writes a meaningful error when not root' {
        $err = @()
        New-NetNeighbor -IPAddress '192.0.2.1' -LinkLayerAddress 'aa:bb:cc:dd:ee:ff' -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'New-NetNeighbor requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.NewNetNeighborCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Remove-NetNeighbor writes a meaningful error when not root' {
        $err = @()
        Remove-NetNeighbor -IPAddress '192.0.2.1' -InterfaceAlias 'eth0' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Remove-NetNeighbor requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.RemoveNetNeighborCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }
}

# ---------------------------------------------------------------------------
# Read-only integration (Linux, any user)
# ---------------------------------------------------------------------------
Describe 'Get-NetIPAddress integration' -Skip:(-not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns at least one address' {
        $addrs = Get-NetIPAddress
        $addrs | Should -Not -BeNullOrEmpty
    }
    It 'each result has non-empty IPAddress' {
        Get-NetIPAddress | ForEach-Object { $_.IPAddress | Should -Not -BeNullOrEmpty }
    }
    It 'each result has non-empty InterfaceAlias' {
        Get-NetIPAddress | ForEach-Object { $_.InterfaceAlias | Should -Not -BeNullOrEmpty }
    }
    It 'AddressFamily is IPv4 or IPv6' {
        Get-NetIPAddress | ForEach-Object { $_.AddressFamily | Should -BeIn @('IPv4', 'IPv6') }
    }
    It '-AddressFamily IPv4 returns only IPv4' {
        $result = Get-NetIPAddress -AddressFamily IPv4
        $result | ForEach-Object { $_.AddressFamily | Should -Be 'IPv4' }
    }
    It '-AddressFamily IPv6 returns only IPv6' {
        $result = Get-NetIPAddress -AddressFamily IPv6
        $result | ForEach-Object { $_.AddressFamily | Should -Be 'IPv6' }
    }
    It 'loopback address 127.0.0.1 is present' {
        $lo = Get-NetIPAddress -IPAddress '127.0.0.1'
        $lo | Should -Not -BeNullOrEmpty
    }
    It '-InterfaceAlias wildcard lo* returns results' {
        $lo = Get-NetIPAddress -InterfaceAlias 'lo*'
        $lo | Should -Not -BeNullOrEmpty
    }
    It '-IPAddress nonexistent returns empty' {
        Get-NetIPAddress -IPAddress '192.0.2.254' | Should -BeNullOrEmpty
    }
}

Describe 'Get-NetRoute integration' -Skip:(-not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns at least one route' {
        Get-NetRoute | Should -Not -BeNullOrEmpty
    }
    It 'each route has DestinationPrefix' {
        Get-NetRoute | ForEach-Object { $_.DestinationPrefix | Should -Not -BeNullOrEmpty }
    }
    It 'AddressFamily is IPv4 or IPv6' {
        Get-NetRoute | ForEach-Object { $_.AddressFamily | Should -BeIn @('IPv4', 'IPv6') }
    }
    It '-AddressFamily IPv4 returns only IPv4 routes' {
        Get-NetRoute -AddressFamily IPv4 | ForEach-Object { $_.AddressFamily | Should -Be 'IPv4' }
    }
    It 'default route 0.0.0.0/0 present (if any IPv4 gateway)' {
        $def = Get-NetRoute -DestinationPrefix '0.0.0.0/0'
        # May or may not be present in all containers; just verify type if present
        if ($def) { $def.DestinationPrefix | Should -Be '0.0.0.0/0' }
    }
}

Describe 'Get-NetTCPConnection integration' -Skip:(-not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns an array (possibly empty)' {
        # Pester 5 quirk: -OrNullOrEmpty fails when value is $null; wrap in Should -Not -Throw
        { Get-NetTCPConnection | Out-Null } | Should -Not -Throw
    }
    It 'LISTEN sockets have RemotePort 0' {
        $listen = Get-NetTCPConnection -State Listen
        $listen | ForEach-Object { $_.RemotePort | Should -Be 0 }
    }
    It 'State values are valid strings' {
        $valid = @('Closed', 'Listen', 'SynSent', 'SynReceived', 'Established', 'FinWait1', 'FinWait2', 'CloseWait', 'Closing', 'LastAck', 'TimeWait', 'DeleteTCB', 'Bound')
        Get-NetTCPConnection | ForEach-Object { $_.State | Should -BeIn $valid }
    }
}

Describe 'Get-NetIPConfiguration integration' -Skip:(-not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns at least one interface' {
        Get-NetIPConfiguration | Should -Not -BeNullOrEmpty
    }
    It 'each result has InterfaceAlias' {
        Get-NetIPConfiguration | ForEach-Object { $_.InterfaceAlias | Should -Not -BeNullOrEmpty }
    }
    It 'loopback excluded by default' {
        $result = Get-NetIPConfiguration
        $result | Where-Object { $_.InterfaceAlias -eq 'lo' } | Should -BeNullOrEmpty
    }
    It '-All includes loopback' {
        $result = Get-NetIPConfiguration -All
        $result | Where-Object { $_.InterfaceAlias -eq 'lo' } | Should -Not -BeNullOrEmpty
    }
    It '-InterfaceAlias wildcard works' {
        $result = Get-NetIPConfiguration -InterfaceAlias '*'
        $result | Should -Not -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Loopback alias add/remove round-trip — Linux + root only
# ---------------------------------------------------------------------------
Describe 'New-NetIPAddress / Remove-NetIPAddress loopback alias' -Skip:(-not $script:isRoot) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        # Clean up in case a previous run left this alias
        Remove-NetIPAddress -IPAddress '127.0.1.99' -PrefixLength 32 -InterfaceAlias 'lo' `
            -Confirm:$false -ErrorAction SilentlyContinue
    }

    AfterAll {
        # Always clean up so other tests are not affected
        Remove-NetIPAddress -IPAddress '127.0.1.99' -PrefixLength 32 -InterfaceAlias 'lo' `
            -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'New-NetIPAddress adds alias to loopback without error' {
        { New-NetIPAddress -IPAddress '127.0.1.99' -PrefixLength 32 -InterfaceAlias 'lo' } | Should -Not -Throw
    }
    It 'alias is visible in Get-NetIPAddress after add' {
        $addr = Get-NetIPAddress -IPAddress '127.0.1.99'
        $addr | Should -Not -BeNullOrEmpty
        $addr.InterfaceAlias | Should -Be 'lo'
    }
    It 'Get-NetIPConfiguration -All sees alias on lo' {
        $cfg = Get-NetIPConfiguration -All | Where-Object { $_.InterfaceAlias -eq 'lo' }
        $cfg | Should -Not -BeNullOrEmpty
    }
    It 'Remove-NetIPAddress removes alias without error' {
        { Remove-NetIPAddress -IPAddress '127.0.1.99' -PrefixLength 32 -InterfaceAlias 'lo' -Confirm:$false } | Should -Not -Throw
    }
    It 'alias gone from Get-NetIPAddress after remove' {
        Get-NetIPAddress -IPAddress '127.0.1.99' | Should -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# New-NetRoute / Remove-NetRoute round-trip — Linux + root only
# ---------------------------------------------------------------------------
Describe 'New-NetRoute / Remove-NetRoute round-trip' -Skip:(-not $script:isRoot) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        # Determine default interface (first non-loopback with an IPv4 address)
        $script:iface = (Get-NetIPAddress -AddressFamily IPv4 |
                Where-Object { $_.InterfaceAlias -ne 'lo' } |
                Select-Object -First 1).InterfaceAlias

        # Remove any leftover
        Remove-NetRoute -DestinationPrefix '192.0.2.0/24' -InterfaceAlias $script:iface `
            -Confirm:$false -ErrorAction SilentlyContinue
    }

    AfterAll {
        Remove-NetRoute -DestinationPrefix '192.0.2.0/24' -InterfaceAlias $script:iface `
            -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'New-NetRoute adds documentation-prefix route without error' {
        { New-NetRoute -DestinationPrefix '192.0.2.0/24' -InterfaceAlias $script:iface } | Should -Not -Throw
    }
    It 'route appears in Get-NetRoute after add' {
        $route = Get-NetRoute -DestinationPrefix '192.0.2.0/24'
        $route | Should -Not -BeNullOrEmpty
    }
    It 'Remove-NetRoute removes route without error' {
        { Remove-NetRoute -DestinationPrefix '192.0.2.0/24' -InterfaceAlias $script:iface -Confirm:$false } | Should -Not -Throw
    }
    It 'route gone from Get-NetRoute after remove' {
        Get-NetRoute -DestinationPrefix '192.0.2.0/24' | Should -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Get-NetTCPConnection with real listener — Linux, any user
# ---------------------------------------------------------------------------
Describe 'Get-NetTCPConnection with real listener' -Skip:(-not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        # Start a background TCP listener on a high port so we can see it
        $script:listenerPort = 19753
        $script:listener = [System.Net.Sockets.TcpListener]::new(
            [System.Net.IPAddress]::Loopback, $script:listenerPort)
        $script:listener.Start()
    }

    AfterAll {
        if ($null -ne $script:listener) {
            $script:listener.Stop()
        }
    }

    It 'listener port appears in Get-NetTCPConnection -State Listen' {
        $conns = Get-NetTCPConnection -State Listen
        $conns | Where-Object { $_.LocalPort -eq $script:listenerPort } | Should -Not -BeNullOrEmpty
    }
    It 'listener LocalAddress is 127.0.0.1' {
        $conn = Get-NetTCPConnection -State Listen |
            Where-Object { $_.LocalPort -eq $script:listenerPort } |
            Select-Object -First 1
        $conn.LocalAddress | Should -Be '127.0.0.1'
    }
    It 'listener RemotePort is 0' {
        $conn = Get-NetTCPConnection -State Listen |
            Where-Object { $_.LocalPort -eq $script:listenerPort } |
            Select-Object -First 1
        $conn.RemotePort | Should -Be 0
    }
}
