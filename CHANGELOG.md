# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] — 2026-05-17

### Added
- Rule 9 cross-platform type alignment: `AddressState`, `NextHopType`, `TcpState`, `OffloadState` changed from `string` to enum types
- Elevation error translation for write cmdlets
- `STATUS.md` and `AGENTS.md` contributor documentation
- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`
- CODEOWNERS file
- PR validation workflow (`pr-validation.yml`)
- GitHub issue templates (bug report, feature request, code review finding)
- PR template with build/test checklist
- OpenCode configuration (`.opencode/`) for standalone development
- Copyright headers on all source files (Rule 10)

### Fixed
- `Get-NetTCPConnection` null result handling → `Should -Not -Throw`
- Elevation tests on Windows now skipped correctly
- `SupportsShouldProcess` removed from stub cmdlets

### Changed
- All 22 linux-rules.md applied and verified
- Pester test valid State values list updated (removed `'Bound'`)

## [0.3.0] — 2026-05-09

### Fixed
- `$script:isLinux` collision → `$script:onLinux`
- `fail-fast: false` in GHA matrix
- `BeOfType` null quirk → `Should -Not -Throw`
- openSUSE image now includes `gawk`+`findutils`

## [0.2.0] — 2026-05-08

### Added
- Loopback alias add/remove round-trip tests
- Route round-trip tests
- Real `TcpListener` port filter for `Get-NetTCPConnection`
- `Get-NetIPConfiguration -All` loopback check
- NUnitXML test output and artifact upload
- Windows pester job

## [0.1.0] — 2026-05-05

### Added
- Initial release
- 10 full cmdlets + 24 stubs
- BCL `NetworkInterface` + `/proc` read paths
- `ip` subprocess write paths
- 5-distro GHA matrix
