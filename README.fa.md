<div align="center">

<img src="assets/netdoctor-icon-512.png" alt="Net Doctor" width="120" />

# نت دکتر (Net Doctor)

**مرکز فرماندهی مدرن ویندوز برای عیب‌یابی شبکه، تعمیر امن، مانیتورینگ، تاریخچه و گزارش‌گیری.**

[English](README.md) - [فارسی](README.fa.md)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0a1622)](#پیش‌نیازها)
[![Version](https://img.shields.io/badge/version-0.6.0-7855ff)](https://github.com/miladateight/NetDoctor/releases/tag/v0.6.0)
[![License](https://img.shields.io/badge/license-Commercial-c83645)](LICENSE)

</div>

---

<div dir="rtl">

نت دکتر یک اپلیکیشن دسکتاپ ویندوز است که مشکلات رایج شبکه و اینترنت را با زبان قابل فهم تشخیص می‌دهد و فقط بعد از تایید کاربر، تعمیرهای کنترل‌شده را اجرا می‌کند. نسخه‌ی `0.6.0` روی تجربه‌ی کاربری بهتر تمرکز دارد: داشبورد ساده و حرفه‌ای، عیب‌یابی بر اساس نشانه‌ی مشکل، راهنمای ریست واضح‌تر و همان installer واحد.

> این مخزن به‌صورت **release-only** طراحی شده است. شامل سورس کد، تست‌ها، اسکریپت‌های build، کلیدهای خصوصی لایسنس یا ابزار صدور لایسنس محلی نیست. نصب‌کننده از طریق GitHub Releases توزیع می‌شود.

## اسکرین‌شات‌ها

<p float="right">
  <img src="assets/screenshots/start.png" width="45%" alt="شروع" />
  <img src="assets/screenshots/dashboard.png" width="45%" alt="داشبورد" />
</p>
<p float="right">
  <img src="assets/screenshots/start-iran.png" width="45%" alt="شروع (منطقه ایران)" />
  <img src="assets/screenshots/dashboard-iran.png" width="45%" alt="داشبورد (منطقه ایران)" />
</p>

## دانلود

- نصب‌کننده‌ی آخرین نسخه: [NetDoctorSetup-0.6.0.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.6.0/NetDoctorSetup-0.6.0.exe)
- SHA256: [NetDoctorSetup-0.6.0.exe.sha256](https://github.com/miladateight/NetDoctor/releases/download/v0.6.0/NetDoctorSetup-0.6.0.exe.sha256)
- همه‌ی نسخه‌ها: [GitHub Releases](https://github.com/miladateight/NetDoctor/releases)

برای استفاده از برنامه، لایسنس معتبر لازم است.

## نکات مهم

- UI جدید با سبک `Soft Command Center` شامل TopBar، Sidebar، Dashboard، StatusBar و tray.
- تم‌های `System`، `Light` و `Dark` همراه با تغییر سریع از TopBar.
- انتخاب زبان در زمان اجرا: `en`، `de`، `fa`، `ar`.
- پشتیبانی RTL برای فارسی و عربی.
- انتخاب منطقه در زمان اجرا: `World` و `Iran` در یک build واحد.
- First Run Wizard برای انتخاب زبان، منطقه و تم.
- داشبورد ساده و داشبورد حرفه‌ای برای کاربر عادی و کاربر حرفه‌ای.
- Dashboard با KPI برای Local، International، DNS و Quality.
- کارت وضعیت برای adapter، gateway، internet، DNS، packet loss، port، VPN، proxy و hosts file.
- Safe Reset و Deep Reset با توضیح دقیق ریسک.
- Speed test با latency، jitter، download، cancel و fallback endpoint.
- Monitor view با latency sparkline و refresh اطلاعات adapter.
- History view برای ذخیره‌ی sessionها، حذف و export.
- Logs view برای مشاهده‌ی log روز.
- خروجی گزارش در قالب‌های `TXT`، `JSON` و `HTML`.

## مدل امنیتی

برنامه به‌صورت پیش‌فرض administrator اجرا نمی‌شود. manifest روی `asInvoker` باقی می‌ماند و UAC فقط برای repairهایی درخواست می‌شود که نیاز دارند.

Undo فقط برای repairهایی فعال است که rollback واقعی دارند. عملیات‌هایی مثل Winsock reset، TCP/IP reset و reset کامل stack شبکه، undo مستقیم وعده نمی‌دهند؛ snapshot آن‌ها فقط برای گزارش و audit نگه‌داری می‌شود.

## پیش‌نیازها

- ویندوز 10 یا 11، نسخه‌ی 64 بیت
- لایسنس معتبر نت دکتر

## لایسنس و خرید

نت دکتر نرم‌افزار تجاری و اختصاصی است. برای استفاده از برنامه به لایسنس معتبر و محدود به زمان نیاز است. لایسنس‌ها شخصی و غیرقابل‌انتقال هستند.

برای خرید لایسنس و پشتیبانی:

- تلگرام: [@MiladAteight](https://t.me/MiladAteight)
- ایمیل: ateight088@gmail.com
- صفحه‌ی محصول: https://ateight.xyz/NetDoctor/

## پشتیبانی

GitHub Issues فقط برای مشکلات عمومی release/download قابل استفاده است. **آپلود نکنید:** کلیدهای لایسنس، logهای حاوی اطلاعات محرمانه، جزئیات شبکه‌ی خصوصی یا فایل‌های پیکربندی محلی.

## لایسنس

تمام حقوق برای Milad AT8 محفوظ است. Net Doctor نرم‌افزار تجاری و اختصاصی است. فایل [LICENSE](LICENSE) را ببینید.

</div>