# Net Doctor

[English](README.md) | [فارسی](README.fa.md) | [العربية](README.ar.md) | [Deutsch](README.de.md)

Net Doctor is a user-friendly Windows network diagnostic and safe repair app. It starts by asking what is broken, then runs a guided diagnosis in plain English.

It is designed for everyday users who need a clear answer to questions like "Why does my internet not work?", without needing to understand DNS, gateways, routes, proxies, VPN adapters, or port tests.

The current version checks:

- Active adapter, gateway and DNS settings
- Country-local internet access based on Windows region/time zone
- International internet access
- DNS resolution
- Latency and packet loss
- TCP port reachability
- VPN-like adapters
- Windows proxy state

Safe repairs save the previous setting before changing anything, then offer Undo inside the app. The first repair actions cover DNS replacement, WinHTTP proxy reset, and a harmless network refresh for uncertain results.

## Download

Project page: [ateight.xyz/NetDoctor](https://ateight.xyz/NetDoctor/)

Download the current Windows installer:
[NetDoctorSetup-0.1.0.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.1.0/NetDoctorSetup-0.1.0.exe)

All release files are also available from the [Releases](https://github.com/miladateight/NetDoctor/releases) page.

## Features

- English graphical Windows interface
- Problem-first flow before diagnosis starts
- Plain-language diagnosis summary
- Country-local vs international connectivity checks
- DNS, packet loss, latency, TCP port and VPN status checks
- Safe Fix action with undo support where a reversible fix is available
- Inno Setup installer package script

## Build

Requires the .NET 8 SDK.

```powershell
.\scripts\publish-win-x64.ps1 -SelfContained
```

## Installer

```powershell
.\scripts\package-installer.ps1
```

The installer is written to `artifacts\installer`.

## Repository

GitHub: [miladateight/NetDoctor](https://github.com/miladateight/NetDoctor)

## License

MIT
