<div align="center">

<img src="assets/netdoctor-icon-512.png" alt="Net Doctor" width="120" />

# نت دکتر (Net Doctor)

**مرکز فرماندهی مدرن ویندوز برای عیب‌یابی شبکه، تعمیر امن، مانیتورینگ، تاریخچه و گزارش‌گیری.**

[English](README.md) - [فارسی](README.fa.md)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0a1622)](#پیشنیازها)
[![Version](https://img.shields.io/badge/version-0.5.0-7855ff)](https://github.com/miladateight/NetDoctor/releases/tag/v0.5.0)
[![License](https://img.shields.io/badge/license-Commercial-c83645)](LICENSE)

</div>

---

<div dir="rtl">

نت دکتر یک اپلیکیشن دسکتاپ ویندوز برای تشخیص مشکلات رایج شبکه و اینترنت است. برنامه مشکل را با زبان قابل فهم توضیح می‌دهد و فقط بعد از تایید کاربر، تعمیرهای کنترل‌شده را اجرا می‌کند.

نسخه `0.5.0` یک بازطراحی جدی برای خود اپلیکیشن است: shell جدید، انتخاب زبان و منطقه در زمان اجرا، تم‌های روشن/تاریک/سیستمی، مدل امن‌تر برای fixها، متن دقیق‌تر برای Undo، خروجی گزارش، تاریخچه، مانیتور، تست سرعت و یک installer واحد.

## دانلود

- نصب‌کننده آخرین نسخه: [NetDoctorSetup-0.5.0.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.5.0/NetDoctorSetup-0.5.0.exe)
- SHA256: [NetDoctorSetup-0.5.0.exe.sha256](https://github.com/miladateight/NetDoctor/releases/download/v0.5.0/NetDoctorSetup-0.5.0.exe.sha256)
- همه نسخه‌ها: [GitHub Releases](https://github.com/miladateight/NetDoctor/releases)

برای استفاده از برنامه، لایسنس معتبر لازم است.

## نکات مهم نسخه 0.5.0

- UI جدید با سبک `Soft Command Center` شامل TopBar، Sidebar، Dashboard، StatusBar و tray.
- تم‌های `System`، `Light` و `Dark` همراه با تغییر سریع از TopBar.
- انتخاب زبان در زمان اجرا: `en`، `de`، `fa`، `ar`.
- پشتیبانی RTL برای فارسی و عربی.
- انتخاب منطقه در زمان اجرا: `World` و `Iran` در یک build واحد.
- First Run Wizard برای انتخاب زبان، منطقه و تم.
- Diagnose view با حالت Manual و Easy.
- Dashboard با KPI برای Local، International، DNS و Quality.
- کارت وضعیت برای adapter، gateway، internet، DNS، packet loss، port، VPN، proxy و hosts file.
- Safe Reset و Deep Reset با توضیح دقیق ریسک.
- Speed test با latency، jitter، download، cancel و fallback endpoint.
- Monitor view با latency sparkline و refresh اطلاعات adapter.
- History view برای ذخیره sessionها، حذف و export.
- Logs view برای مشاهده log روز.
- خروجی گزارش در قالب‌های `TXT`، `JSON` و `HTML`.
- سرویس notification آپدیت بدون نصب خودکار.

## مدل امنیتی

برنامه به صورت پیش‌فرض administrator اجرا نمی‌شود. manifest روی `asInvoker` باقی مانده و UAC فقط برای repairهایی درخواست می‌شود که `RequiresAdmin=true` دارند.

Undo فقط برای repairهایی فعال است که rollback واقعی دارند. عملیات‌هایی مثل Winsock reset، TCP/IP reset و reset کامل stack شبکه، undo مستقیم وعده نمی‌دهند؛ snapshot آن‌ها فقط برای گزارش و audit نگه‌داری می‌شود.

## checkهای اصلی

در نسخه 0.5.0 این checkها به صورت صریح پیاده یا wrap شده‌اند:

- `DnsCheck`
- `GatewayCheck`
- `InternetCheck`
- `AdapterCheck`
- `ProxyCheck`
- `PacketLossCheck`
- `VpnCheck`
- `HostsFileCheck`
- `PortCheck`

`CaptivePortalCheck` و `IpConflictCheck` فعلا deferred هستند تا سیاست endpoint، کنترل false-positive و اعتبارسنجی ARP قابل اعتمادتر شود.

## لایسنس

مسیر لایسنس:

```text
%PROGRAMDATA%\NetDoctor\license.json
```

اعتبارسنجی شامل signature، machine match، expiration و clock rollback است. tokenهای قدیمی که edition دارند همچنان برای سازگاری validate می‌شوند، اما edition دیگر باعث رد شدن activation نمی‌شود.

ابزار صدور لایسنس:

```text
tools\NetDoctor.LicenseTool\bin\Release\net8.0\netdoctor-license.exe
```

گزینه `--edition` deprecated است، هنوز پذیرفته می‌شود و فقط warning می‌دهد.

## ساخت و تست

پیش‌نیازها:

- ویندوز 10 یا 11، نسخه 64 بیت
- .NET 8 SDK
- Inno Setup 6 برای ساخت installer

دستورهای رایج:

```powershell
dotnet build src\NetDoctor.App\NetDoctor.App.csproj -c Debug
dotnet build src\NetDoctor.App\NetDoctor.App.csproj -c Release
dotnet run --project tests\NetDoctor.Tests\NetDoctor.Tests.csproj -c Release
powershell -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

## خروجی‌های release

```text
artifacts\publish\NetDoctor.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe
artifacts\installer\NetDoctorSetup-0.5.0.exe.sha256
tools\NetDoctor.LicenseTool\bin\Release\net8.0\netdoctor-license.exe
tools\NetDoctor.LicenseTool\issue-license.cmd
```

## وضعیت اعتبارسنجی

پکیج v0.5.0 با این موارد بررسی شد:

- Debug build: pass
- Release build: pass
- Test runner: pass
- App publish: pass
- License tool publish: pass
- Satellite assembly validation برای `de`، `fa`، `ar`: pass
- توضیح neutral resource برای English: pass
- ساخت installer با Inno Setup: pass
- تولید SHA256: pass

## تماس

- تلگرام: [@MiladAteight](https://t.me/MiladAteight)
- ایمیل: ateight088@gmail.com

## لایسنس

تمام حقوق برای Milad AT8 محفوظ است. Net Doctor نرم‌افزار تجاری و اختصاصی است. فایل [LICENSE](LICENSE) را ببینید.

</div>
