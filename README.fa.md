# Net Doctor

[English](README.md) | [فارسی](README.fa.md) | [العربية](README.ar.md) | [Deutsch](README.de.md)

Net Doctor یک برنامه گرافیکی و کاربرپسند برای ویندوز است که مشکلات رایج اینترنت و شبکه را بررسی می‌کند و نتیجه را به زبان ساده توضیح می‌دهد.

این برنامه برای کاربرانی ساخته شده که می‌خواهند بدانند «چرا اینترنت یا شبکه کار نمی‌کند؟» بدون اینکه لازم باشد مفاهیمی مثل DNS، Gateway، Route، Proxy، VPN یا Port را به صورت فنی بشناسند.

نسخه فعلی این موارد را بررسی می‌کند:

- کارت شبکه فعال، Gateway و تنظیمات DNS
- دسترسی به اینترنت داخل کشور بر اساس Region و Time Zone ویندوز
- دسترسی به اینترنت بین‌المللی
- پاسخ‌دهی DNS
- تاخیر و Packet Loss
- باز بودن پورت TCP
- تشخیص Adapterهای شبیه VPN
- وضعیت Proxy ویندوز

قابلیت Fix Safely قبل از هر تغییر، تنظیم قبلی را ذخیره می‌کند و در صورت امکان گزینه Undo را داخل برنامه ارائه می‌دهد. اصلاح‌های اولیه شامل تغییر امن DNS، ریست WinHTTP Proxy و یک Refresh بدون خطر برای نتیجه‌های نامشخص است.

## دانلود

آخرین فایل نصب ویندوز را از صفحه [Releases](https://github.com/miladateight/NetDoctor/releases) دانلود کنید.

## قابلیت‌ها

- رابط گرافیکی انگلیسی و ساده
- شروع عیب‌یابی با انتخاب مشکل توسط کاربر
- نمایش نتیجه عیب‌یابی به زبان ساده
- بررسی تفاوت اتصال داخلی کشور و اینترنت بین‌المللی
- بررسی DNS، Packet Loss، Latency، TCP Port و وضعیت VPN
- دکمه Safe Fix با پشتیبانی از Undo برای تغییرات قابل برگشت
- اسکریپت ساخت فایل نصب با Inno Setup

## ساخت برنامه

برای ساخت برنامه به .NET 8 SDK نیاز دارید.

```powershell
.\scripts\publish-win-x64.ps1 -SelfContained
```

## ساخت فایل نصب

```powershell
.\scripts\package-installer.ps1
```

خروجی نصب‌کننده در مسیر `artifacts\installer` ساخته می‌شود.

## مخزن

GitHub: [miladateight/NetDoctor](https://github.com/miladateight/NetDoctor)

## مجوز

MIT
