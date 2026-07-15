# Changelog

## 0.8.0

- The app now renders in a single frame. On open and when switching sections the window appears complete instead of assembling itself in visible stages (background, then elements, then colour).
- Faster startup: the window is prepared off-screen and shown already at its final size, with no restore-size flash and no relayout after it appears.
- Instant section switching: moving between sidebar pages is now immediate, with no transition delay.
- Added a **subscription status** card: clicking the Premium badge shows the exact number of days remaining and the expiry date (Jalali date in the Persian UI), with a one-click renew.
- The Premium badge now shows the remaining days inline, plus a hover tooltip with the expiry date.
- A single running copy: launching Net Doctor again while it is already running restores the existing window instead of opening a second instance.
- Updated the public release pointers to the `0.8.0` installer.

## 0.7.0

- Diagnosis checks now run in parallel, so a full scan takes about the time of the single slowest check instead of the sum.
- Added a guided **Initial Setup** wizard that runs diagnose → bottleneck → DNS → apply → verify entirely in-place.
- Added a **bottleneck analyzer** that names the first broken link in the connection chain in plain language.
- The DNS chooser now live-pings each resolver, shows latency and pre-selects the fastest healthy one (domestic and global for Iran).
- Introduced **Free and Premium** tiers in one build: the app runs for free with a base feature set (adapter/gateway/DNS checks, IP display, simple ping, Flush DNS, simple dashboard); a valid license unlocks Premium (professional dashboard, VPN/Proxy/Port/Hosts, Safe/Deep reset, Speed Test, Monitor, History/Logs/export, DNS change, snapshots & undo). No trial, no time limit, and Premium data is never deleted on expiry.
- Updated the public release pointers to the `0.7.0` installer.

## 0.6.1

- Fixed the sidebar navigation transition: switching between sections no longer shows the new page rendering in visible stages; page switches are now an instant, smooth cross-fade.
- Updated the public release pointers to the `0.6.1` installer.

## 0.6.0

- Updated the public release pointers to the `0.6.0` installer.
- Added UI-focused release notes for the improved dashboard, diagnosis and reset flows.
- Kept the package release-only with no source code, tests, private keys or local tooling.

## 0.5.0

- Public release package prepared as release-only content.
- Installer download is provided through GitHub Releases.
- Source code, tests, LicenseTool, private keys, debug symbols, and local configuration files are intentionally excluded.
