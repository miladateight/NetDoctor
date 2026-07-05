using NetDoctor.App.Localization;

namespace NetDoctor.App.Services;

/// <summary>A named public DNS resolver used for the comparison test.</summary>
internal sealed record DnsResolver(string Name, string PersianName, string Ip)
{
    public string DisplayName => AppConfig.IsPersian ? PersianName : Name;
}

internal static class KnownDns
{
    public static readonly DnsResolver Cloudflare = new("Cloudflare", "کلودفلر", "1.1.1.1");
    public static readonly DnsResolver Google = new("Google", "گوگل", "8.8.8.8");

    // Iranian resolvers that unblock internal CDNs and some local services.
    public static readonly DnsResolver Shecan = new("Shecan", "شکن", "178.22.122.100");
    public static readonly DnsResolver Electro = new("Electro (403)", "الکترو (۴۰۳)", "78.157.42.100");
    public static readonly DnsResolver Begzar = new("Begzar", "بگذر", "185.55.226.26");

    /// <summary>Resolvers compared against the system DNS, ordered by relevance to the edition.</summary>
    public static IReadOnlyList<DnsResolver> ComparisonSet => AppConfig.IsPersian
        ? [Shecan, Electro, Begzar, Cloudflare, Google]
        : [Cloudflare, Google];
}

/// <summary>
/// A DNS configuration the user can switch to from Fix Safely. <see cref="Primary"/> being
/// <c>null</c> means "restore the adapter to automatic (DHCP)".
/// </summary>
internal sealed record DnsPreset(string Id, string Name, string PersianName, string? Primary, string? Secondary, string Note, string PersianNote)
{
    public string DisplayName => AppConfig.IsPersian ? PersianName : Name;
    public string DisplayNote => AppConfig.IsPersian ? PersianNote : Note;
    public bool IsAutomatic => Primary is null;
}

internal static class DnsPresets
{
    public static readonly DnsPreset Shecan = new(
        "shecan", "Shecan", "شکن", "178.22.122.100", "185.51.200.2",
        "Unblocks many developer and internal services in Iran.",
        "بسیاری از سرویس‌های توسعه و داخلی ایران را باز می‌کند.");

    public static readonly DnsPreset Electro = new(
        "electro", "Electro (403)", "الکترو (۴۰۳)", "78.157.42.100", "78.157.42.101",
        "Iranian resolver, alternative to Shecan.",
        "DNS ایرانی، جایگزین شکن.");

    public static readonly DnsPreset Begzar = new(
        "begzar", "Begzar", "بگذر", "185.55.226.26", "185.55.225.25",
        "Iranian resolver focused on opening blocked sites.",
        "DNS ایرانی برای باز کردن سایت‌های مسدود.");

    public static readonly DnsPreset Cloudflare = new(
        "cloudflare", "Cloudflare", "کلودفلر", "1.1.1.1", "1.0.0.1",
        "Fast, privacy-friendly global resolver.",
        "DNS جهانی سریع و امن.");

    public static readonly DnsPreset Google = new(
        "google", "Google", "گوگل", "8.8.8.8", "8.8.4.4",
        "Reliable global resolver.",
        "DNS جهانی پایدار.");

    public static readonly DnsPreset Automatic = new(
        "auto", "Automatic (DHCP)", "خودکار (DHCP)", null, null,
        "Let the modem/ISP assign DNS automatically.",
        "بگذار مودم/اپراتور خودش DNS را تعیین کند.");

    /// <summary>All presets offered in the chooser, ordered by relevance to the edition.</summary>
    public static IReadOnlyList<DnsPreset> All => AppConfig.IsPersian
        ? [Shecan, Electro, Begzar, Cloudflare, Google, Automatic]
        : [Cloudflare, Google, Automatic];

    /// <summary>The preset Fix Safely recommends first when DNS is the likely cause.</summary>
    public static DnsPreset Recommended => AppConfig.IsPersian ? Shecan : Cloudflare;

    public static DnsPreset? FindById(string id) => All.FirstOrDefault(preset => preset.Id == id);
}
