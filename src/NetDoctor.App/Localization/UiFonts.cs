using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace NetDoctor.App.Localization;

internal static class UiFonts
{
    private static readonly PrivateFontCollection PrivateFonts = new();
    private static bool attemptedLoad;
    private static FontFamily? persianFamily;

    private const string PersianBodyFont = "Tahoma";

    public static string FamilyName => AppConfig.IsPersian ? PersianBodyFont : "Segoe UI";

    /// <summary>
    /// The bundled "B Koodak Bold" font file only contains a bold face. Asking GDI+ for a
    /// "Regular" style from a bold-only family silently substitutes the bold glyphs anyway,
    /// which renders every "regular" Persian label wider/taller than its container expects
    /// and makes text spill out of its box. So Koodak is reserved for genuinely bold display
    /// text, and Tahoma (a native Persian-shaping Windows font) is used for everything else.
    /// </summary>
    public static Font Create(float size, FontStyle style = FontStyle.Regular)
    {
        if (AppConfig.IsPersian)
        {
            if (style.HasFlag(FontStyle.Bold) && TryGetPersianFamily() is { } family)
            {
                return new Font(family, size, style, GraphicsUnit.Point);
            }

            return new Font(PersianBodyFont, size, style, GraphicsUnit.Point);
        }

        return new Font("Segoe UI", size, style, GraphicsUnit.Point);
    }

    private static FontFamily? TryGetPersianFamily()
    {
        if (attemptedLoad)
        {
            return persianFamily;
        }

        attemptedLoad = true;
        foreach (var path in CandidateFontPaths())
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                PrivateFonts.AddFontFile(path);
                persianFamily = PrivateFonts.Families.FirstOrDefault();
                if (persianFamily is not null)
                {
                    return persianFamily;
                }
            }
            catch
            {
                // Fall back to Segoe UI if the bundled font cannot be loaded.
            }
        }

        return null;
    }

    private static IEnumerable<string> CandidateFontPaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "assets", "fonts", "B Koodak Bold_0.ttf");
        yield return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets", "fonts", "B Koodak Bold_0.ttf"));
    }
}