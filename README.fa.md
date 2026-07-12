<div align="center">

<img src="assets/netdoctor-icon-512.png" alt="Net Doctor" width="120" />

# نت دکتر (Net Doctor)

**مرکز فرماندهی مدرن ویندوز برای عیب‌یابی شبکه، تعمیر امن، مانیتورینگ، تاریخچه و گزارش‌گیری.**

[English](README.md) - [فارسی](README.fa.md)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0a1622)](#پیش‌نیازها)
[![Version](https://img.shields.io/badge/version-0.7.0-7855ff)](https://github.com/miladateight/NetDoctor/releases/tag/v0.7.0)
[![License](https://img.shields.io/badge/license-Commercial-c83645)](LICENSE)

</div>

---

<div dir="rtl">

نت دکتر یک اپلیکیشن دسکتاپ ویندوز است که مشکلات رایج شبکه و اینترنت را با زبان قابل فهم تشخیص می‌دهد و فقط بعد از تایید کاربر، تعمیرهای کنترل‌شده را اجرا می‌کند. نسخه‌ی `0.7.0` برنامه را به‌طور محسوسی سریع‌تر می‌کند (بررسی‌ها هم‌زمان اجرا می‌شوند)، یک ویزارد **راه‌اندازی اولیه‌ی** گام‌به‌گام، یک **آنالیزگر گلوگاه** که اولین حلقه‌ی خراب زنجیره‌ی اتصال را مشخص می‌کند و یک انتخابگر DNS که هر سرور را **پینگ زنده** می‌گیرد و سریع‌ترینِ سالم را از قبل انتخاب می‌کند اضافه می‌کند. همچنین دو سطح **رایگان و پرمیوم** در یک build معرفی می‌شود — برنامه به‌صورت رایگان با مجموعه‌ی پایه اجرا می‌شود و لایسنس همه‌چیز را باز می‌کند.

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

- نصب‌کننده‌ی آخرین نسخه: [NetDoctorSetup-0.7.0.exe](https://github.com/miladateight/NetDoctor/releases/download/v0.7.0/NetDoctorSetup-0.7.0.exe)
- SHA256: [NetDoctorSetup-0.7.0.exe.sha256](https://github.com/miladateight/NetDoctor/releases/download/v0.7.0/NetDoctorSetup-0.7.0.exe.sha256)
- همه‌ی نسخه‌ها: [GitHub Releases](https://github.com/miladateight/NetDoctor/releases)

برنامه بدون لایسنس در حالت **رایگان** نصب و اجرا می‌شود. لایسنس معتبر، حالت **پرمیوم** را باز می‌کند.

## رایگان در برابر پرمیوم

یک build، دو سطح دسترسی — بدون نصب‌کننده‌ی جدا، بدون نسخه‌ی آزمایشی و بدون محدودیت زمانی. بدون لایسنس (یا با لایسنس منقضی‌شده) در حالت **رایگان** و با لایسنس معتبر در حالت **پرمیوم** اجرا می‌شود. قابلیت‌های پرمیوم با قفل در برنامه دیده می‌شوند و با کلیک، صفحه‌ی فعال‌سازی باز می‌شود. با انقضای لایسنس هیچ داده‌ای (تاریخچه، snapshot، تنظیمات) حذف نمی‌شود؛ فقط دسترسی قفل می‌شود.

| رایگان | پرمیوم (همه‌ی موارد رایگان، به‌علاوه…) |
| --- | --- |
| بررسی کارت شبکه و Gateway | داشبورد حرفه‌ای |
| بررسی ساده‌ی Internet و DNS | بررسی و تعمیر VPN، Proxy، Port و Hosts |
| نمایش کارت شبکه‌ی فعال و IP | Safe Reset، Deep Reset، ریست Winsock و TCP/IP |
| تست پینگ ساده | Speed Test کامل و Live Monitor |
| تعمیر Flush DNS | History، Logs و خروجی TXT/JSON/HTML |
| داشبورد ساده (نتیجه‌ی فعلی) | تغییر DNS (انتخابگر با پینگ زنده) + ویزارد راه‌اندازی اولیه |
|  | Snapshot و Undo |

## نکات مهم

- UI جدید با سبک `Soft Command Center` شامل TopBar، Sidebar، Dashboard، StatusBar و tray.
- تم‌های `System`، `Light` و `Dark` همراه با تغییر سریع از TopBar.
- انتخاب زبان در زمان اجرا: `en`، `de`، `fa`، `ar`.
- پشتیبانی RTL برای فارسی و عربی.
- انتخاب منطقه در زمان اجرا: `World` و `Iran` در یک build واحد.
- First Run Wizard برای انتخاب زبان، منطقه و تم.
- **راه‌اندازی اولیه‌ی** گام‌به‌گام: عیب‌یابی ← گلوگاه ← DNS ← اعمال ← بررسی، همه در همان صفحه.
- **آنالیزگر گلوگاه** که اولین حلقه‌ی خراب زنجیره‌ی اتصال را به زبان ساده مشخص می‌کند.
- عیب‌یابی موازی (تقریباً به‌اندازه‌ی کندترین بررسی به‌جای جمع همه).
- انتخابگر DNS که هر سرور را پینگ زنده می‌گیرد و سریع‌ترینِ سالم را از قبل انتخاب می‌کند (داخلی و خارجی برای ایران).
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