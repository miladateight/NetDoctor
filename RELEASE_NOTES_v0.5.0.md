# Net Doctor v0.5.0

Net Doctor v0.5.0 is a desktop-focused release with a redesigned Windows application shell, a unified runtime language/region model, safer repair metadata and a single installer.

## What's New

- New `Soft Command Center` desktop UI with TopBar, Sidebar, Dashboard, StatusBar and tray behavior.
- Light, Dark and System theme support.
- First Run Wizard for language, region and theme.
- Runtime language support for English, German, Persian and Arabic.
- RTL layout support for Persian and Arabic.
- Runtime region support for World and Iran in one build.
- Manual and Easy diagnosis flows.
- Dashboard KPI cards for Local, International, DNS and Quality.
- Status cards for adapter, gateway, internet, DNS, packet loss, port, VPN, proxy and hosts file checks.
- Safe Reset and Deep Reset flows with accurate risk and undo wording.
- Speed test with cancel and fallback endpoint.
- Monitor view with adapter refresh and latency sparkline.
- History, logs and report export to TXT, JSON and HTML.
- License validation updated to keep old edition-bearing tokens compatible while using a single app build.

## Safety

- The app does not run as administrator by default.
- UAC is requested only for fixes marked `RequiresAdmin=true`.
- Undo is shown only for fixes with real rollback data.
- Winsock/TCP/IP/network-stack resets do not promise direct rollback.

## Validation

- Debug build: pass
- Release build: pass
- Test runner: pass
- App publish: pass
- License tool publish: pass
- Satellite assemblies for `de`, `fa`, `ar`: pass
- English neutral resources embedded in the main assembly: confirmed
- Inno Setup installer: pass
- SHA256 file: pass

## SHA256

```text
72d031d3adcbe3dfafc9f203e53883f1611aac50d8d5b2d5b59db2d0bac98fe4  NetDoctorSetup-0.5.0.exe
```

## Assets

- `NetDoctorSetup-0.5.0.exe`
- `NetDoctorSetup-0.5.0.exe.sha256`
