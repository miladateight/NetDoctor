<div align="center">

<img src="assets/netdoctor-icon-512.png" alt="Net Doctor" width="120" />

# Net Doctor

**A modern Windows command center for network diagnosis, safe repair, monitoring, history and reports.**

[English](README.md) - [فارسی](README.fa.md)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0a1622)](#requirements)
[![Version](https://img.shields.io/badge/version-0.6.1-7855ff)](https://github.com/miladateight/NetDoctor/releases/tag/v0.6.1)
[![License](https://img.shields.io/badge/license-Commercial-c83645)](LICENSE)

</div>

---

Net Doctor is a Windows desktop application that diagnoses common network and internet problems in plain language, then applies controlled repairs only when the user confirms them. Version `0.6.1` polishes the desktop UI and daily workflows: simple/pro dashboard modes, symptom-first diagnosis, stronger reset guidance, a smoother sidebar navigation transition and the same single unified installer.

> This repository is intentionally **release-only**. It does not contain source code, tests, build scripts, private licensing keys, or the local license issuer tool. The installer is distributed through GitHub Releases.

## Screenshots

<p float="left">
  <img src="assets/screenshots/start.png" width="45%" alt="Start" />
  <img src="assets/screenshots/dashboard.png" width="45%" alt="Dashboard" />
</p>
<p float="left">
  <img src="assets/screenshots/start-iran.png" width="45%" alt="Start (Iran region)" />
  <img src="assets/screenshots/dashboard-iran.png" width="45%" alt="Dashboard (Iran region)" />
</p>

## Download

- Latest installer: [NetDoctorSetup-0.6.1.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.6.1/NetDoctorSetup-0.6.1.exe)
- SHA256: [NetDoctorSetup-0.6.1.exe.sha256](https://github.com/miladateight/NetDoctor/releases/download/v0.6.1/NetDoctorSetup-0.6.1.exe.sha256)
- All releases: [GitHub Releases](https://github.com/miladateight/NetDoctor/releases)

A valid license is required to use the app.

## Highlights

- Modern `Soft Command Center` UI with TopBar, Sidebar, Dashboard, StatusBar and tray behavior.
- Theme support: `System`, `Light`, `Dark` with a quick TopBar toggle.
- Runtime language selection: `en`, `de`, `fa`, `ar`.
- RTL layout support for Persian and Arabic.
- Runtime region selection: `World` and `Iran` in one app build.
- First Run Wizard for language, region and theme.
- Simple and Professional dashboard modes for normal users and power users.
- Dashboard KPI strip for Local, International, DNS and Quality status.
- Status cards for adapter, gateway, internet, DNS, packet loss, port, VPN, proxy and hosts file checks.
- Safe Reset and Deep Reset flows with clear risk wording.
- Speed test with latency, jitter, download, cancel and fallback endpoint.
- Monitor view with latency sparkline and adapter refresh.
- History view for saved sessions, delete and export.
- Logs view with daily log access.
- Report export to `TXT`, `JSON` and `HTML`.

## Safety Model

Net Doctor does not run as administrator by default. The app manifest remains `asInvoker`; UAC is requested only for repairs that require it.

Undo is available only for repairs with real rollback data. Operations such as Winsock reset, TCP/IP reset and full network-stack reset do not promise direct rollback; their snapshots are kept for audit and reporting.

## Requirements

- Windows 10 or Windows 11, 64-bit
- A valid Net Doctor license

## License And Purchase

Net Doctor is proprietary commercial software. A valid, time-limited license is required to use the application. Licenses are personal and non-transferable.

For license purchase and support, contact:

- Telegram: [@MiladAteight](https://t.me/MiladAteight)
- Email: ateight088@gmail.com
- Product page: https://ateight.xyz/NetDoctor/

## Support

GitHub Issues may be used only for public release/download problems. **Do not upload** license keys, logs containing secrets, private network details, or local configuration files.

## License

Copyright (c) 2026 Milad AT8. Net Doctor is proprietary commercial software. See [LICENSE](LICENSE).