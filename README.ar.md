# Net Doctor

[English](README.md) | [فارسی](README.fa.md) | [العربية](README.ar.md) | [Deutsch](README.de.md)

Net Doctor هو تطبيق رسومي سهل الاستخدام لنظام Windows يساعد على تشخيص مشاكل الإنترنت والشبكة الشائعة وشرح النتيجة بلغة واضحة.

تم تصميمه للمستخدمين الذين يريدون معرفة سبب تعطل الاتصال بدون الحاجة إلى فهم تفاصيل تقنية مثل DNS أو Gateway أو Routing أو Proxy أو VPN أو اختبارات المنافذ.

الإصدار الحالي يفحص:

- بطاقة الشبكة النشطة وإعدادات Gateway و DNS
- الوصول إلى الإنترنت المحلي داخل بلد المستخدم بناء على إعدادات Region و Time Zone في Windows
- الوصول إلى الإنترنت الدولي
- استجابة DNS
- زمن التأخير وفقدان الحزم
- إمكانية الوصول إلى منافذ TCP
- وجود محولات شبكة شبيهة بـ VPN
- حالة Proxy في Windows

تقوم ميزة Fix Safely بحفظ الإعداد السابق قبل أي تغيير، وتوفر خيار Undo داخل التطبيق عندما يكون الإصلاح قابلا للتراجع. تشمل الإصلاحات الأولى تغيير DNS بشكل آمن، وإعادة ضبط WinHTTP Proxy، وتنفيذ تحديث شبكة بسيط للنتائج غير المؤكدة.

## التنزيل

يمكن تنزيل أحدث ملف تثبيت لنظام Windows من صفحة [Releases](https://github.com/miladateight/NetDoctor/releases).

## الميزات

- واجهة رسومية إنجليزية وسهلة الاستخدام
- يبدأ التشخيص بسؤال المستخدم عن المشكلة
- ملخص واضح لحالة الشبكة
- مقارنة الاتصال المحلي داخل البلد بالإنترنت الدولي
- فحص DNS و Packet Loss و Latency ومنافذ TCP وحالة VPN
- زر Safe Fix مع دعم Undo للإصلاحات القابلة للتراجع
- سكربت إنشاء ملف تثبيت باستخدام Inno Setup

## البناء

يتطلب .NET 8 SDK.

```powershell
.\scripts\publish-win-x64.ps1 -SelfContained
```

## إنشاء ملف التثبيت

```powershell
.\scripts\package-installer.ps1
```

يتم إنشاء ملف التثبيت داخل `artifacts\installer`.

## المستودع

GitHub: [miladateight/NetDoctor](https://github.com/miladateight/NetDoctor)

## الترخيص

MIT
