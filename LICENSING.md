# Net Doctor Licensing and Release Notes

This document describes the private licensing and release workflow for Net Doctor.

## License Model

Net Doctor uses offline signed license tokens:

```text
license key = base64url(payload JSON) + "." + base64url(ECDSA P-256 signature)
```

The current payload contains:

- customer name
- machine ID
- issued timestamp
- expiration timestamp

Older tokens that contain an `edition` field remain valid for compatibility. Starting with v0.5.0, edition is ignored during validation because language and region are runtime settings in a single app build.

The app validates:

- signature
- machine match
- expiration
- clock rollback

The license file is stored at:

```text
%PROGRAMDATA%\NetDoctor\license.json
```

## Issuer Tool

The issuer tool lives at:

```text
tools\NetDoctor.LicenseTool
```

Common commands:

```powershell
dotnet run --project tools\NetDoctor.LicenseTool -c Release -- keygen
dotnet run --project tools\NetDoctor.LicenseTool -c Release -- issue --name "Customer" --days 30 --machine "ND-XXXX-XXXX-XXXX-XXXX-XXXX"
```

`--edition` is deprecated. It is still accepted for old scripts, prints a warning and is ignored.

The private signing key must never be committed. It belongs in `licensing-keys/private-key.txt`, which is ignored by git.

## Build and Publish

Net Doctor v0.5.0 uses one Windows app build:

```powershell
dotnet build src\NetDoctor.App\NetDoctor.App.csproj -c Release
powershell -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

The publish script validates required satellite assemblies:

- `de\NetDoctor.resources.dll`
- `fa\NetDoctor.resources.dll`
- `ar\NetDoctor.resources.dll`

English is the neutral resource language and is embedded in the main assembly; a separate `en` satellite folder is not expected.

## Release Outputs

Expected local release outputs:

```text
artifacts\publish\NetDoctor.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe.sha256
tools\NetDoctor.LicenseTool\bin\Release\net8.0\netdoctor-license.exe
tools\NetDoctor.LicenseTool\issue-license.cmd
```

## Security Notes

- The app does not run as administrator by default.
- Only fixes marked `RequiresAdmin=true` request UAC.
- Undo is enabled only for fixes with real restore data.
- Winsock reset, TCP/IP reset and network-stack reset do not have direct rollback.
- Snapshots are retained for report and audit when direct rollback is not possible.

## If the Private Key Leaks

Generate a new keypair, replace the embedded public key in `LicenseManager`, rebuild the app and reissue customer licenses. Old licenses signed with the compromised key should be considered invalid.
