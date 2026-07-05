using System.Globalization;

namespace NetDoctor.App.Localization;

internal enum AppEdition
{
    International,
    Iran
}

internal enum AppLanguage
{
    English,
    German,
    Persian,
    Arabic
}

internal enum AppRegion
{
    World,
    Iran
}

internal enum ThemePreference
{
    System,
    Light,
    Dark
}

/// <summary>
/// Runtime configuration for v0.5.0. Language, region and theme are no longer
/// compile-time editions; the single app build switches them from settings.
/// </summary>
internal static class AppConfig
{
    private static AppLanguage _language = DetectLanguage();
    private static AppRegion _region = AppRegion.World;
    private static ThemePreference _theme = ThemePreference.System;
    private static bool _reducedMotion;

    public static AppEdition Edition => _region == AppRegion.Iran ? AppEdition.Iran : AppEdition.International;

    public static AppLanguage Language => _language;

    public static AppRegion Region => _region;

    public static ThemePreference Theme => _theme;

    public static bool ReducedMotion => _reducedMotion;

    public static bool IsPersian => _language == AppLanguage.Persian;

    public static bool IsRtl => _language is AppLanguage.Persian or AppLanguage.Arabic;

    public static string FontFamily => UiFonts.FamilyName;

    public static string ProductName => "Net Doctor";

    public static string Version => Application.ProductVersion;

    public static string LicenseAudience => "net-doctor";

    public static string CultureName => LanguageCode(_language);

    public static CultureInfo Culture => new(CultureName);

    public static void Apply(string? language, string? region, string? theme, bool reducedMotion)
    {
        _language = ParseLanguage(language);
        _region = ParseRegion(region);
        _theme = ParseTheme(theme);
        _reducedMotion = reducedMotion;

        var culture = Culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static string LanguageCode(AppLanguage language) => language switch
    {
        AppLanguage.German => "de",
        AppLanguage.Persian => "fa",
        AppLanguage.Arabic => "ar",
        _ => "en"
    };

    public static AppLanguage ParseLanguage(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "de" or "german" => AppLanguage.German,
            "fa" or "persian" or "farsi" => AppLanguage.Persian,
            "ar" or "arabic" => AppLanguage.Arabic,
            _ => AppLanguage.English
        };
    }

    public static AppRegion ParseRegion(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "iran" or "ir" => AppRegion.Iran,
            _ => AppRegion.World
        };
    }

    public static ThemePreference ParseTheme(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "light" => ThemePreference.Light,
            "dark" => ThemePreference.Dark,
            _ => ThemePreference.System
        };
    }

    private static AppLanguage DetectLanguage()
    {
        return ParseLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    }
}
