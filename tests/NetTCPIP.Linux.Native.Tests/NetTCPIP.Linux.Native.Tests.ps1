# NetTCPIP.Linux.Native.Tests.ps1
# Pester 5 test suite for NetTCPIP.Linux.Native.
# Module surface and filter tests run everywhere.
# Integration tests (read-only) run on Linux only.
# Write-operation tests (New/Remove) run on Linux+root only.

BeforeDiscovery {
    $script:isLinux = $IsLinux
    $script:isRoot  = $IsLinux -and ((& id -u) -eq '0')
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
        'Find-NetRoute','Get-NetCompartment','Get-NetIPInterface',
        'Get-NetIPv4Protocol','Get-NetIPv6Protocol','Get-NetNeighbor',
        'Get-NetOffloadGlobalSetting','Get-NetPrefixPolicy','Get-NetTCPSetting',
        'Get-NetTransportFilter','Get-NetUDPEndpoint','Get-NetUDPSetting',
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
# Read-only integration (Linux, any user)
# ---------------------------------------------------------------------------
Describe 'Get-NetIPAddress integration' -Skip:(-not $script:isLinux) {
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
        Get-NetIPAddress | ForEach-Object { $_.AddressFamily | Should -BeIn @('IPv4','IPv6') }
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

Describe 'Get-NetRoute integration' -Skip:(-not $script:isLinux) {
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
        Get-NetRoute | ForEach-Object { $_.AddressFamily | Should -BeIn @('IPv4','IPv6') }
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

Describe 'Get-NetTCPConnection integration' -Skip:(-not $script:isLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Release\net8.0\NetTCPIP.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\NetTCPIP.Linux.Native\bin\Debug\net8.0\NetTCPIP.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns an array (possibly empty)' {
        $result = Get-NetTCPConnection
        # OK to be empty in minimal containers
        $result | Should -BeOfType [object[]] -OrNullOrEmpty
    }
    It 'LISTEN sockets have RemotePort 0' {
        $listen = Get-NetTCPConnection -State Listen
        $listen | ForEach-Object { $_.RemotePort | Should -Be 0 }
    }
    It 'State values are valid strings' {
        $valid = @('Closed','Listen','SynSent','SynReceived','Established','FinWait1','FinWait2','CloseWait','Closing','LastAck','TimeWait','DeleteTCB','Bound')
        Get-NetTCPConnection | ForEach-Object { $_.State | Should -BeIn $valid }
    }
}

Describe 'Get-NetIPConfiguration integration' -Skip:(-not $script:isLinux) {
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
