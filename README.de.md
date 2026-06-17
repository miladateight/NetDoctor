# Net Doctor

[English](README.md) | [فارسی](README.fa.md) | [العربية](README.ar.md) | [Deutsch](README.de.md)

Net Doctor ist eine benutzerfreundliche Windows-App zur Netzwerkdiagnose und sicheren Reparatur. Die App fragt zuerst, welches Problem vorliegt, und führt danach eine geführte Diagnose in einfacher Sprache aus.

Sie richtet sich an Anwender, die eine klare Antwort auf Fragen wie "Warum funktioniert mein Internet nicht?" brauchen, ohne technische Details wie DNS, Gateway, Routing, Proxy, VPN-Adapter oder Porttests verstehen zu müssen.

Die aktuelle Version prüft:

- Aktiven Netzwerkadapter, Gateway und DNS-Einstellungen
- Länderbezogenen lokalen Internetzugang anhand von Windows-Region und Zeitzone
- Internationalen Internetzugang
- DNS-Auflösung
- Latenz und Paketverlust
- Erreichbarkeit von TCP-Ports
- VPN-ähnliche Adapter
- Windows-Proxy-Status

Sichere Reparaturen speichern die vorherige Einstellung, bevor etwas geändert wird, und bieten anschließend eine Undo-Option in der App. Die ersten Reparaturaktionen decken DNS-Änderungen, das Zurücksetzen des WinHTTP-Proxys und eine harmlose Netzwerkaktualisierung für unklare Ergebnisse ab.

## Download

Lade den aktuellen Windows-Installer über die Seite [Releases](https://github.com/miladateight/NetDoctor/releases) herunter.

## Funktionen

- Englische grafische Windows-Oberfläche
- Problemorientierter Ablauf vor dem Start der Diagnose
- Verständliche Zusammenfassung des Netzwerkzustands
- Prüfung von lokalem und internationalem Internetzugang
- Tests für DNS, Paketverlust, Latenz, TCP-Ports und VPN-Status
- Safe-Fix-Schaltfläche mit Undo-Unterstützung für reversible Änderungen
- Inno-Setup-Skript zum Erstellen des Installers

## Build

Erfordert das .NET 8 SDK.

```powershell
.\scripts\publish-win-x64.ps1 -SelfContained
```

## Installer

```powershell
.\scripts\package-installer.ps1
```

Der Installer wird nach `artifacts\installer` geschrieben.

## Repository

GitHub: [miladateight/NetDoctor](https://github.com/miladateight/NetDoctor)

## Lizenz

MIT
