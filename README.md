<div align="center">

<img src="assets/netdoctor-icon-512.png" alt="Net Doctor" width="120" />

# Net Doctor

**A modern Windows command center for network diagnosis, safe repair, monitoring, history and reports.**

[English](README.md) - [فارسی](README.fa.md)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0a1622)](#system-requirements)
[![Version](https://img.shields.io/badge/version-0.5.0-7855ff)](https://github.com/miladateight/NetDoctor/releases/tag/v0.5.0)
[![License](https://img.shields.io/badge/license-Commercial-c83645)](LICENSE)

</div>

---

Net Doctor is a Windows desktop application for diagnosing common network and internet problems in plain language, then applying controlled repairs only when the user confirms them.

Version `0.5.0` introduces a redesigned desktop shell, runtime language and region selection, Light/Dark/System themes, safer fix metadata, better undo wording, report export, session history, monitor tools, speed testing and a single unified installer.

## Download

- Latest installer: [NetDoctorSetup-0.5.0.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.5.0/NetDoctorSetup-0.5.0.exe)
- SHA256: [NetDoctorSetup-0.5.0.exe.sha256](https://github.com/miladateight/NetDoctor/releases/download/v0.5.0/NetDoctorSetup-0.5.0.exe.sha256)
- All releases: [GitHub Releases](https://github.com/miladateight/NetDoctor/releases)

A valid license is required to use the app.

## Highlights

- Modern `Soft Command Center` UI with TopBar, Sidebar, Dashboard, StatusBar and tray behavior.
- Theme support: `System`, `Light`, `Dark` with a quick TopBar toggle.
- Runtime language selection: `en`, `de`, `fa`, `ar`.
- RTL layout support for Persian and Arabic.
- Runtime region selection: `World` and `Iran` in one app build.
- First Run Wizard for language, region and theme.
- Diagnose view with Manual and Easy modes.
- Dashboard KPI strip for Local, International, DNS and Quality status.
- Status cards for adapter, gateway, internet, DNS, packet loss, port, VPN, proxy and hosts file checks.
- Safe Reset and Deep Reset flows with clear risk wording.
- Speed test with latency, jitter, download, cancel and fallback endpoint.
- Monitor view with latency sparkline and adapter refresh.
- History view for saved sessions, delete and export.
- Logs view with daily log access.
- Report export to `TXT`, `JSON` and `HTML`.
- Update notification service without automatic install.

## Safety Model

Net Doctor does not run as administrator by default. The app manifest remains `asInvoker`; UAC is requested only for repairs marked `RequiresAdmin=true`.

Undo is available only for repairs with real rollback data. Operations such as Winsock reset, TCP/IP reset and full network-stack reset do not promise direct rollback; their snapshots are kept for audit and reporting.

## Core Checks

The v0.5.0 core explicitly includes or wraps these checks:

- `DnsCheck`
- `GatewayCheck`
- `InternetCheck`
- `AdapterCheck`
- `ProxyCheck`
- `PacketLossCheck`
- `VpnCheck`
- `HostsFileCheck`
- `PortCheck`

`CaptivePortalCheck` and `IpConflictCheck` are deferred until endpoint policy, false-positive handling and ARP validation are reliable enough for production use.

## Licensing

The license is stored at:

```text
%PROGRAMDATA%\NetDoctor\license.json
```

Licenses validate signature, machine match, expiration and clock rollback. Older tokens that include an edition still validate for compatibility, but edition is no longer used to block activation.

The private issuer tool is kept under:

```text
tools\NetDoctor.LicenseTool\bin\Release\net8.0\netdoctor-license.exe
```

The `--edition` option is deprecated, still accepted, and ignored with a warning.

## Build

Requirements:

- Windows 10 or Windows 11, 64-bit
- .NET 8 SDK
- Inno Setup 6 for installer packaging

Common commands:

```powershell
dotnet build src\NetDoctor.App\NetDoctor.App.csproj -c Debug
dotnet build src\NetDoctor.App\NetDoctor.App.csproj -c Release
dotnet run --project tests\NetDoctor.Tests\NetDoctor.Tests.csproj -c Release
powershell -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

## Release Artifacts

Expected local outputs:

```text
artifacts\publish\NetDoctor.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe.sha256
tools\NetDoctor.LicenseTool\bin\Release\net8.0\netdoctor-license.exe
tools\NetDoctor.LicenseTool\issue-license.cmd
```

## Validation Status

The v0.5.0 package was verified with:

- Debug build: pass
- Release build: pass
- Test runner: pass
- App publish: pass
- License tool publish: pass
- Satellite assembly validation for `de`, `fa`, `ar`: pass
- English neutral resource explanation: pass
- Inno Setup installer compile: pass
- SHA256 file generation: pass

## Contact

- Telegram: [@MiladAteight](https://t.me/MiladAteight)
- Email: ateight088@gmail.com

## License

Copyright (c) 2026 Milad AT8. Net Doctor is proprietary commercial software. See [LICENSE](LICENSE).
