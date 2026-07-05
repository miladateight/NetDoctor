using System.Globalization;
using System.Resources;

namespace NetDoctor.App.Localization;

/// <summary>
/// All user-facing strings. Each member returns Persian for the Iran edition and
/// English otherwise, so the rest of the code never branches on language.
/// </summary>
internal static class L
{
    private static readonly ResourceManager ResourceManager = new("NetDoctor.App.Localization.Strings", typeof(L).Assembly);

    public static string T(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public static string T(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentUICulture, T(key), args);
    }
    private static bool Fa => AppConfig.IsPersian;

    // ---- Start screen ----
    public static string AppTitle => AppConfig.ProductName;

    public static string HeroSubtitle => Fa
        ? "Ø§Ø¨Ø²Ø§Ø± Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ø´Ø¨Ú©Ù‡ Ú©Ù‡ Ø¨Ù‡ Ø²Ø¨Ø§Ù† Ø³Ø§Ø¯Ù‡ Ù…ÛŒâ€ŒÚ¯ÙˆÛŒØ¯ Ù…Ø´Ú©Ù„ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø§Ø² Ú©Ø¬Ø§Ø³Øª Ùˆ Ø±Ø§Ù‡â€ŒØ­Ù„ Ù‚Ø§Ø¨Ù„ Ø¨Ø§Ø²Ú¯Ø´Øª Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ Ù…ÛŒâ€ŒØ¯Ù‡Ø¯."
        : "A friendly network diagnosis tool that explains what is broken and offers reversible repairs.";

    public static string WhatProblem => Fa ? "Ú†Ù‡ Ù…Ø´Ú©Ù„ÛŒ Ø¯Ø§Ø±ÛŒØŸ" : "What problem are you having?";
    public static string HostOrWebsite => Fa ? "Ø¢Ø¯Ø±Ø³ Ø³Ø§ÛŒØª ÛŒØ§ Ø³Ø±ÙˆØ±" : "Host or website";
    public static string Port => Fa ? "Ù¾ÙˆØ±Øª" : "Port";
    public static string SelectedIssue => Fa ? "Ù…Ø´Ú©Ù„ Ø§Ù†ØªØ®Ø§Ø¨â€ŒØ´Ø¯Ù‡" : "Selected issue";
    public static string StartDiagnosis => Fa ? "Ø´Ø±ÙˆØ¹ Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ" : "Start diagnosis";

    public static string StartNote => Fa
        ? "Ù†Øªâ€ŒØ¯Ú©ØªØ± Ø§ÙˆÙ„ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨Ù‡ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒ Ø§ÛŒØ±Ø§Ù† Ø±Ø§ Ø¨Ø§ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ Ù…Ù‚Ø§ÛŒØ³Ù‡ Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ Ø¨Ø¹Ø¯ DNSØŒ Ú©ÛŒÙÛŒØª Ø§ØªØµØ§Ù„ØŒ Ù¾ÙˆØ±ØªØŒ VPN Ùˆ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ø±Ø§ Ø¨Ø±Ø±Ø³ÛŒ Ù…ÛŒâ€ŒÚ©Ù†Ø¯."
        : "Net Doctor will compare local-country access with international access, then check DNS, quality, ports, VPN and proxy.";

    // ---- Dashboard chrome ----
    public static string Ready => Fa ? "Ø¢Ù…Ø§Ø¯Ù‡" : "Ready";
    public static string Back => Fa ? "Ø¨Ø§Ø²Ú¯Ø´Øª" : "Back";
    public static string RunAgain => Fa ? "Ø§Ø¬Ø±Ø§ÛŒ Ø¯ÙˆØ¨Ø§Ø±Ù‡" : "Run again";
    public static string FixSafely => Fa ? "ØªØ¹Ù…ÛŒØ± Ø§Ù…Ù†" : "Fix Safely";
    public static string Undo => Fa ? "Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ" : "Undo";

    public static string TechnicalDetails => Fa ? "Ø¬Ø²Ø¦ÛŒØ§Øª ÙÙ†ÛŒ" : "Technical details";

    public static string ChooseAndStart => Fa
        ? "ÛŒÚ© Ù…Ø´Ú©Ù„ Ø±Ø§ Ø§Ù†ØªØ®Ø§Ø¨ Ú©Ù† Ùˆ Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ø±Ø§ Ø´Ø±ÙˆØ¹ Ú©Ù†."
        : "Choose a problem and start diagnosis.";

    public static string RunningDiagnosis => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ..." : "Running diagnosis...";

    public static string RunningSubtitle => Fa
        ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø±Ø±Ø³ÛŒ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒØŒ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ØŒ DNSØŒ Ú©ÛŒÙÛŒØªØŒ Ù¾ÙˆØ±ØªØŒ VPN Ùˆ Ù¾Ø±ÙˆÚ©Ø³ÛŒ."
        : "Testing local-country access, international access, DNS, quality, port, VPN and proxy.";

    public static string DefaultFixHint => Fa
        ? "Â«ØªØ¹Ù…ÛŒØ± Ø§Ù…Ù†Â» Ø¯Ø± ØµÙˆØ±Øª ÙˆØ¬ÙˆØ¯ØŒ ÛŒÚ© Ø§ØµÙ„Ø§Ø­ Ù‚Ø§Ø¨Ù„ Ø¨Ø§Ø²Ú¯Ø´Øª Ø§Ø¬Ø±Ø§ Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ ÛŒØ§ Ø¨Ø±Ø§ÛŒ Ù†ØªØ§ÛŒØ¬ Ù†Ø§Ù…Ø·Ù…Ø¦Ù† ÛŒÚ© Ø¨Ø§Ø²Ø¢ÙˆØ±ÛŒ Ø¨ÛŒâ€ŒØ®Ø·Ø± Ø´Ø¨Ú©Ù‡ Ø§Ù†Ø¬Ø§Ù… Ù…ÛŒâ€ŒØ¯Ù‡Ø¯."
        : "Fix Safely runs a reversible repair when one is available, or a harmless network refresh for uncertain results.";

    public static string AllHealthyNoFix => Fa
        ? "Ù‡Ù…Ù‡â€ŒÚ†ÛŒØ² Ø³Ø§Ù„Ù… Ø¨Ù‡ Ù†Ø¸Ø± Ù…ÛŒâ€ŒØ±Ø³Ø¯ØŒ Ù¾Ø³ Ø§Ù‚Ø¯Ø§Ù… ØªØ¹Ù…ÛŒØ±ÛŒ Ù„Ø§Ø²Ù… Ù†ÛŒØ³Øª."
        : "Everything looks healthy, so no repair action is recommended.";

    public static string DiagnosisComplete => Fa ? "Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ú©Ø§Ù…Ù„ Ø´Ø¯." : "Diagnosis complete.";
    public static string DiagnosisCompleteFixAvailable => Fa
        ? "Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ú©Ø§Ù…Ù„ Ø´Ø¯. ÛŒÚ© ØªØ¹Ù…ÛŒØ± Ø§Ù…Ù† Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ø§Ø³Øª."
        : "Diagnosis complete. A safe repair is available.";
    public static string DiagnosisCancelled => Fa ? "Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ù„ØºÙˆ Ø´Ø¯." : "Diagnosis cancelled.";
    public static string DiagnosisFailed => Fa ? "Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ú©Ø§Ù…Ù„ Ù†Ø´Ø¯." : "Diagnosis could not be completed.";
    public static string Error => Fa ? "Ø®Ø·Ø§" : "Error";
    public static string WaitingToRun => Fa ? "Ø¯Ø± Ø§Ù†ØªØ¸Ø§Ø± Ø§Ø¬Ø±Ø§..." : "Waiting to run...";

    // ---- Card titles ----
    public static string CardAdapter => Fa ? "Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡" : "Network adapter";
    public static string CardLocal => Fa ? "Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒ" : "Local internet";
    public static string CardInternational => Fa ? "Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„" : "International internet";
    public static string CardDns => Fa ? "DNS" : "DNS";
    public static string CardQuality => Fa ? "Ú©ÛŒÙÛŒØª Ø§ØªØµØ§Ù„" : "Connection quality";
    public static string CardPort => Fa ? "Ø¯Ø³ØªØ±Ø³ÛŒ Ù¾ÙˆØ±Øª" : "Port access";
    public static string CardVpn => Fa ? "VPN" : "VPN";
    public static string CardProxy => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ" : "Proxy";
    public static string CardHosts => Fa ? "ÙØ§ÛŒÙ„ Hosts" : "Hosts file";

    public static string LocalWithCountry(string country) => Fa
        ? $"Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒ ({country})"
        : $"Local internet ({country})";

    // ---- Card status labels ----
    public static string StatusHealthy => Fa ? "Ø³Ø§Ù„Ù…" : "Healthy";
    public static string StatusNeedsAttention => Fa ? "Ù†ÛŒØ§Ø² Ø¨Ù‡ Ø¨Ø±Ø±Ø³ÛŒ" : "Needs attention";
    public static string StatusProblem => Fa ? "Ù…Ø´Ú©Ù„ Ù¾ÛŒØ¯Ø§ Ø´Ø¯" : "Problem found";
    public static string StatusChecking => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø±Ø±Ø³ÛŒ..." : "Checking...";
    public static string StatusPending => Fa ? "Ø¯Ø± ØµÙ" : "Pending";

    // ---- Status words for the technical log ----
    public static string LogOk => Fa ? "Ø³Ø§Ù„Ù…" : "OK";
    public static string LogWarning => Fa ? "Ù‡Ø´Ø¯Ø§Ø±" : "Warning";
    public static string LogProblem => Fa ? "Ù…Ø´Ú©Ù„" : "Problem";
    public static string LogRunning => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø§Ø¬Ø±Ø§" : "Running";
    public static string LogPending => Fa ? "Ø¯Ø± ØµÙ" : "Pending";

    // ---- Details log labels ----
    public static string LblProblem => Fa ? "Ù…Ø´Ú©Ù„" : "Problem";
    public static string LblStarted => Fa ? "Ø´Ø±ÙˆØ¹" : "Started";
    public static string LblFinished => Fa ? "Ù¾Ø§ÛŒØ§Ù†" : "Finished";

    // ---- Message boxes ----
    public static string ConfirmContinue => Fa ? "Ø§Ø¯Ø§Ù…Ù‡ Ù…ÛŒâ€ŒØ¯Ù‡ÛŒØŸ" : "Continue?";
    public static string FixSavesFirst => Fa
        ? "Ù†Øªâ€ŒØ¯Ú©ØªØ± Ø§ÙˆÙ„ ØªÙ†Ø¸ÛŒÙ… ÙØ¹Ù„ÛŒ Ø±Ø§ Ø°Ø®ÛŒØ±Ù‡ Ù…ÛŒâ€ŒÚ©Ù†Ø¯ ØªØ§ Â«Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒÂ» Ø¨ØªÙˆØ§Ù†Ø¯ Ø¢Ù† Ø±Ø§ Ø¨Ø±Ú¯Ø±Ø¯Ø§Ù†Ø¯."
        : "Net Doctor saves the current setting first so Undo can restore it.";
    public static string UndoConfirm => Fa
        ? "Ø¢Ø®Ø±ÛŒÙ† ØªÙ†Ø¸ÛŒÙ… Ø°Ø®ÛŒØ±Ù‡â€ŒØ´Ø¯Ù‡ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†Ø¯Ù‡ Ø´ÙˆØ¯ØŸ"
        : "Restore the last saved setting?";
    public static string RunningFix => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø§Ø¬Ø±Ø§ÛŒ ØªØ¹Ù…ÛŒØ± Ø§Ù…Ù†..." : "Running Fix Safely...";
    public static string RestoringSetting => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ ØªÙ†Ø¸ÛŒÙ… Ù‚Ø¨Ù„ÛŒ..." : "Restoring the previous setting...";

    // ---- Running placeholders ----
    public static string PhAdapter => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø®ÙˆØ§Ù†Ø¯Ù† Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡ØŒ gateway Ùˆ Ø³Ø±ÙˆØ±Ù‡Ø§ÛŒ DNS..." : "Reading the active adapter, gateway and DNS servers...";
    public static string PhLocal => Fa ? "Ø¯Ø± Ø­Ø§Ù„ ØªØ³Øª Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒ..." : "Testing country-local websites...";
    public static string PhInternational => Fa ? "Ø¯Ø± Ø­Ø§Ù„ ØªØ³Øª Ù…Ù‚ØµØ¯Ù‡Ø§ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ÛŒ..." : "Testing international destinations...";
    public static string PhDns => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ù…Ù‚Ø§ÛŒØ³Ù‡â€ŒÛŒ DNS Ø³ÛŒØ³ØªÙ… Ø¨Ø§ DNSÙ‡Ø§ÛŒ Ø¯ÛŒÚ¯Ø±..." : "Comparing system DNS with other resolvers...";
    public static string PhQuality => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø§Ù†Ø¯Ø§Ø²Ù‡â€ŒÚ¯ÛŒØ±ÛŒ ØªØ£Ø®ÛŒØ± Ùˆ Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡..." : "Measuring latency and packet loss...";
    public static string PhPort => Fa ? "Ø¯Ø± Ø­Ø§Ù„ ØªØ³Øª Ø§ØªØµØ§Ù„ TCP..." : "Testing TCP connectivity...";
    public static string PhVpn => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø±Ø±Ø³ÛŒ Ú©Ø§Ø±Øªâ€ŒÙ‡Ø§ÛŒ Ø´Ø¨Ú©Ù‡â€ŒÛŒ Ø´Ø¨ÛŒÙ‡ VPN..." : "Checking active VPN-like adapters...";
    public static string PhProxy => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø±Ø±Ø³ÛŒ ØªÙ†Ø¸ÛŒÙ…Ø§Øª Ù¾Ø±ÙˆÚ©Ø³ÛŒ ÙˆÛŒÙ†Ø¯ÙˆØ²..." : "Checking Windows proxy settings...";
    public static string PhHosts => Fa ? "Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø±Ø±Ø³ÛŒ ÙØ§ÛŒÙ„ Hosts Ø¨Ø±Ø§ÛŒ Ø±Ú©ÙˆØ±Ø¯Ù‡Ø§ÛŒ Ø«Ø§Ø¨Øª..." : "Checking the Hosts file for static overrides...";

    // ---- Adapter check ----
    public static string AdapterNoneSummary => Fa ? "Ù‡ÛŒÚ† Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙØ¹Ø§Ù„ Ø¨Ø§ IPv4 Ù¾ÛŒØ¯Ø§ Ù†Ø´Ø¯." : "No active IPv4 network adapter was found.";
    public static string AdapterNoneDetail => Fa ? "ÙˆÛŒÙ†Ø¯ÙˆØ² Ù‡ÛŒÚ† Ú©Ø§Ø±Øª Ø§ØªØ±Ù†Øª ÛŒØ§ Wiâ€‘Fi ÙØ¹Ø§Ù„ÛŒ Ø¨Ø§ IPv4 Ú¯Ø²Ø§Ø±Ø´ Ù†Ù…ÛŒâ€ŒÚ©Ù†Ø¯." : "Windows does not report an active Ethernet or Wi-Fi adapter with IPv4.";
    public static string AdapterNoGateway => Fa ? "Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙØ¹Ø§Ù„ gateway Ø¨Ø§ IPv4 Ù†Ø¯Ø§Ø±Ø¯. Ù…Ù…Ú©Ù† Ø§Ø³Øª Ù…Ø³ÛŒØ± Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø±Ø³Øª Ø¨Ø±Ù‚Ø±Ø§Ø± Ù†Ø´ÙˆØ¯." : "The active adapter has no IPv4 gateway. Internet access may not route correctly.";
    public static string AdapterNoDns => Fa ? "Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡ Ø¢Ù†Ù„Ø§ÛŒÙ† Ø§Ø³ØªØŒ Ø§Ù…Ø§ Ù‡ÛŒÚ† Ø³Ø±ÙˆØ± DNS Ø¨Ø§ IPv4 ØªÙ†Ø¸ÛŒÙ… Ù†Ø´Ø¯Ù‡." : "The adapter is online, but no IPv4 DNS server is configured.";
    public static string AdapterHealthy => Fa ? "Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙØ¹Ø§Ù„ IPv4ØŒ gateway Ùˆ ØªÙ†Ø¸ÛŒÙ…Ø§Øª DNS Ø¯Ø§Ø±Ø¯." : "The active network adapter has IPv4, gateway and DNS settings.";
    public static string LblRegion => Fa ? "Ú©Ø´ÙˆØ±/Ù…Ù†Ø·Ù‚Ù‡â€ŒÛŒ Ø´Ù†Ø§Ø³Ø§ÛŒÛŒâ€ŒØ´Ø¯Ù‡" : "Detected country/region";
    public static string LblAdapter => Fa ? "Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡" : "Adapter";
    public static string LblType => Fa ? "Ù†ÙˆØ¹" : "Type";
    public static string LblGateway => Fa ? "Gateway" : "Gateway";
    public static string ValNone => Fa ? "Ù†Ø¯Ø§Ø±Ø¯" : "none";

    // ---- Reachability check ----
    public static string Reachable(string title) => Fa ? $"{title} Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ø§Ø³Øª." : $"{title} is reachable.";
    public static string PartiallyReachable(string title) => Fa ? $"{title} Ù†ÛŒÙ…Ù‡â€ŒÙØ¹Ø§Ù„ Ø§Ø³Øª. Ø¨Ø¹Ø¶ÛŒ Ù…Ù‚ØµØ¯Ù‡Ø§ Ù¾Ø§Ø³Ø® Ù†Ø¯Ø§Ø¯Ù†Ø¯." : $"{title} is partially reachable. Some destinations failed.";
    public static string LocalUnreachable => Fa ? "Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒ Ù¾Ø§Ø³Ø® Ù†Ø¯Ø§Ø¯Ù†Ø¯." : "Country-local internet targets did not respond.";
    public static string InternationalUnreachable => Fa ? "Ù…Ù‚ØµØ¯Ù‡Ø§ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ÛŒ Ù¾Ø§Ø³Ø® Ù†Ø¯Ø§Ø¯Ù†Ø¯." : "International internet targets did not respond.";
    public static string ProbeFailed(string host, long ms, string err) => Fa ? $"{host}: Ù¾Ø³ Ø§Ø² {ms} Ù…ÛŒÙ„ÛŒâ€ŒØ«Ø§Ù†ÛŒÙ‡ Ù†Ø§Ù…ÙˆÙÙ‚ ({err})." : $"{host}: failed after {ms} ms ({err}).";
    public static string ProbeOk(string host, int status, long ms) => Fa ? $"{host}: HTTP {status} Ø¯Ø± {ms} Ù…ÛŒÙ„ÛŒâ€ŒØ«Ø§Ù†ÛŒÙ‡." : $"{host}: HTTP {status} in {ms} ms.";

    // ---- DNS check ----
    public static string DnsSystemLabel => Fa ? "DNS Ø³ÛŒØ³ØªÙ…" : "System DNS";
    public static string DnsResolvedVia(string resolver, string host, string ip, long ms) => Fa
        ? $"{resolver}: Â«{host}Â» Ø¨Ù‡ {ip} Ø±Ø³ÛŒØ¯ ({ms} Ù…ÛŒÙ„ÛŒâ€ŒØ«Ø§Ù†ÛŒÙ‡)."
        : $"{resolver}: resolved {host} to {ip} ({ms} ms).";
    public static string DnsFailedVia(string resolver, string host, string msg) => Fa
        ? $"{resolver}: Ø­Ù„ Ù†Ø§Ù… Â«{host}Â» Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯ ({msg})."
        : $"{resolver}: could not resolve {host} ({msg}).";
    public static string DnsHealthy => Fa ? "DNS Ø³ÛŒØ³ØªÙ… Ø¯Ø±Ø³Øª Ùˆ Ø³Ø±ÛŒØ¹ Ù¾Ø§Ø³Ø® Ù…ÛŒâ€ŒØ¯Ù‡Ø¯." : "System DNS is responding normally.";
    public static string DnsSlow => Fa ? "DNS Ø³ÛŒØ³ØªÙ… Ù¾Ø§Ø³Ø® Ù…ÛŒâ€ŒØ¯Ù‡Ø¯ØŒ Ø§Ù…Ø§ Ø¢Ù†â€ŒÙ‚Ø¯Ø± Ú©Ù†Ø¯ Ø§Ø³Øª Ú©Ù‡ Ø¨Ø§Ø² Ø´Ø¯Ù† Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ Ø±Ø§ Ù…Ø®ØªÙ„ Ù…ÛŒâ€ŒÚ©Ù†Ø¯." : "System DNS responds, but it is slow enough to make websites feel broken.";
    public static string DnsSystemBrokenOthersWork => Fa
        ? "DNS ÙØ¹Ù„ÛŒ Ø³ÛŒØ³ØªÙ… Ù†Ø§Ù…â€ŒÙ‡Ø§ Ø±Ø§ Ø­Ù„ Ù†Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ Ø§Ù…Ø§ DNSÙ‡Ø§ÛŒ Ø¯ÛŒÚ¯Ø± Ù…ÙˆÙÙ‚ Ø´Ø¯Ù†Ø¯. ØªØºÛŒÛŒØ± Ø§Ù…Ù† DNS Ù…Ø´Ú©Ù„ Ø±Ø§ Ø­Ù„ Ù…ÛŒâ€ŒÚ©Ù†Ø¯."
        : "The system DNS is not resolving names, but other resolvers worked. A safe DNS change should fix this.";
    public static string DnsAllFailed => Fa
        ? "Ù‡ÛŒÚ† DNSÛŒ Ù†Ø§Ù…â€ŒÙ‡Ø§ Ø±Ø§ Ø­Ù„ Ù†Ú©Ø±Ø¯. Ø§Ø­ØªÙ…Ø§Ù„Ø§Ù‹ Ù…Ø´Ú©Ù„ Ø¹Ù…ÛŒÙ‚â€ŒØªØ± Ø§Ø² DNS Ø§Ø³Øª (Ø§ØªØµØ§Ù„ØŒ gateway ÛŒØ§ ÙÛŒÙ„ØªØ±ÛŒÙ†Ú¯ Ú©Ø§Ù…Ù„)."
        : "No resolver could resolve names. The problem is likely deeper than DNS (connectivity, gateway, or full blocking).";
    public static string DnsSystemFailsFallback => Fa
        ? "DNS Ø³ÛŒØ³ØªÙ… Ù¾Ø§Ø³Ø® Ù†Ø¯Ø§Ø¯Ø› Ø§Ø² DNS Ø¬Ø§ÛŒÚ¯Ø²ÛŒÙ† Ø¨Ø±Ø§ÛŒ Ø§Ø¯Ø§Ù…Ù‡â€ŒÛŒ ØªØ³Øª Ø§Ø³ØªÙØ§Ø¯Ù‡ Ø´Ø¯. ØªØºÛŒÛŒØ± Ø§Ù…Ù† DNS Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ Ù…ÛŒâ€ŒØ´ÙˆØ¯."
        : "The system DNS did not answer; a fallback resolver was used. A safe DNS change is recommended.";

    // ---- Quality check ----
    public static string QualityPacketLoss(int pct) => Fa ? $"Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡: {pct}Ùª" : $"Packet loss: {pct}%";
    public static string QualityLatency(int avg, long max) => Fa ? $"Ù…ÛŒØ§Ù†Ú¯ÛŒÙ† ØªØ£Ø®ÛŒØ±: {avg} Ù…ÛŒÙ„ÛŒâ€ŒØ«Ø§Ù†ÛŒÙ‡. Ø¨ÛŒØ´ØªØ±ÛŒÙ† ØªØ£Ø®ÛŒØ±: {max} Ù…ÛŒÙ„ÛŒâ€ŒØ«Ø§Ù†ÛŒÙ‡." : $"Average latency: {avg} ms. Maximum latency: {max} ms.";
    public static string QualityNoReplies => Fa ? "Ù‡ÛŒÚ† Ù¾Ø§Ø³Ø® ICMP Ù…ÙˆÙÙ‚ÛŒ Ø«Ø¨Øª Ù†Ø´Ø¯." : "No successful ICMP replies were recorded.";
    public static string QualityIcmpBlocked => Fa ? "Ù¾ÛŒÙ†Ú¯ ICMP Ù…Ø³Ø¯ÙˆØ¯ Ø§Ø³Øª ÛŒØ§ Ø§ØªØµØ§Ù„ Ø¨Ù‡ Ù¾ÛŒÙ†Ú¯ Ù¾Ø§Ø³Ø® Ù†Ù…ÛŒâ€ŒØ¯Ù‡Ø¯." : "ICMP ping is blocked or the connection is not responding to ping.";
    public static string QualityUnstable => Fa ? "Ø§ØªØµØ§Ù„ Ù†Ø§Ù¾Ø§ÛŒØ¯Ø§Ø± Ø§Ø³Øª. Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡ ÛŒØ§ ØªØ£Ø®ÛŒØ± Ø¨Ø§Ù„Ø§ Ø´Ù†Ø§Ø³Ø§ÛŒÛŒ Ø´Ø¯." : "The connection is unstable. Packet loss or high latency was detected.";
    public static string QualityMinorIssues => Fa ? "Ø§ØªØµØ§Ù„ Ú©Ø§Ø± Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ Ø§Ù…Ø§ ØªØ£Ø®ÛŒØ± ÛŒØ§ Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡ Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¨Ø§Ø¹Ø« Ù‚Ø·Ø¹â€ŒÙˆØµÙ„ÛŒ Ø´ÙˆØ¯." : "The connection works, but latency or packet loss may cause interruptions.";
    public static string QualityHealthy => Fa ? "ØªØ£Ø®ÛŒØ± Ùˆ Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡ Ø³Ø§Ù„Ù… Ø¨Ù‡ Ù†Ø¸Ø± Ù…ÛŒâ€ŒØ±Ø³Ø¯." : "Latency and packet loss look healthy.";

    // ---- Port check ----
    public static string LblHost => Fa ? "Ø¢Ø¯Ø±Ø³" : "Host";
    public static string PortTimedOut => Fa ? "Ø§ØªØµØ§Ù„ TCP Ù…Ù†Ù‚Ø¶ÛŒ Ø´Ø¯ (timeout)." : "TCP connection timed out.";
    public static string PortNoResponse(int port, string host) => Fa ? $"Ù¾ÙˆØ±Øª TCP {port} Ø±ÙˆÛŒ {host} Ù¾Ø§Ø³Ø® Ù†Ø¯Ø§Ø¯." : $"TCP port {port} on {host} did not respond.";
    public static string PortSucceeded => Fa ? "Ø§ØªØµØ§Ù„ TCP Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯." : "TCP connection succeeded.";
    public static string PortReachable(int port, string host) => Fa ? $"Ù¾ÙˆØ±Øª TCP {port} Ø±ÙˆÛŒ {host} Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ø§Ø³Øª." : $"TCP port {port} on {host} is reachable.";
    public static string PortError(string msg) => Fa ? $"Ø®Ø·Ø§: {msg}" : $"Error: {msg}";
    public static string PortUnreachable(int port, string host) => Fa ? $"Ù¾ÙˆØ±Øª TCP {port} Ø±ÙˆÛŒ {host} Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ù†ÛŒØ³Øª." : $"TCP port {port} on {host} is not reachable.";

    // ---- VPN check ----
    public static string VpnNoneDetail => Fa ? "Ù‡ÛŒÚ† Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ Ø´Ø¨ÛŒÙ‡ VPN ÙØ¹Ø§Ù„ÛŒ Ù¾ÛŒØ¯Ø§ Ù†Ø´Ø¯." : "No active VPN-like adapter was found.";
    public static string VpnMentionedButNone => Fa ? "Ù…Ø´Ú©Ù„ Ø§Ù†ØªØ®Ø§Ø¨â€ŒØ´Ø¯Ù‡ Ø¨Ù‡ VPN Ø§Ø´Ø§Ø±Ù‡ Ø¯Ø§Ø±Ø¯ØŒ Ø§Ù…Ø§ ÙˆÛŒÙ†Ø¯ÙˆØ² Ú©Ø§Ø±Øª VPN ÙØ¹Ø§Ù„ÛŒ Ù†Ø´Ø§Ù† Ù†Ù…ÛŒâ€ŒØ¯Ù‡Ø¯." : "The selected problem mentions VPN, but Windows does not show an active VPN adapter.";
    public static string VpnActiveIntlFails => Fa ? "VPN ÙØ¹Ø§Ù„ Ø§Ø³Øª Ùˆ Ø¯Ø± Ø­Ø§Ù„ÛŒ Ú©Ù‡ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒ Ú©Ø§Ø± Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ Ù†Ø§Ù…ÙˆÙÙ‚ Ø§Ø³Øª. Ù…Ø³ÛŒØ±Ø¯Ù‡ÛŒ ÛŒØ§ DNS Ù…Ø±Ø¨ÙˆØ· Ø¨Ù‡ VPN Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¯Ø®ÛŒÙ„ Ø¨Ø§Ø´Ø¯." : "VPN is active and international access is failing while local access works. VPN routing or DNS may be involved.";
    public static string VpnActiveNoFault => Fa ? "ÛŒÚ© Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ Ø´Ø¨ÛŒÙ‡ VPN ÙØ¹Ø§Ù„ Ø§Ø³ØªØŒ Ø§Ù…Ø§ Ø®Ø·Ø§ÛŒ Ù…Ø´Ø®ØµÛŒ Ø§Ø² VPN Ø¯ÛŒØ¯Ù‡ Ù†Ø´Ø¯." : "A VPN-like adapter is active, but no clear VPN fault was detected.";
    public static string VpnNone => Fa ? "Ù‡ÛŒÚ† Ú©Ø§Ø±Øª VPN ÙØ¹Ø§Ù„ÛŒ Ø´Ù†Ø§Ø³Ø§ÛŒÛŒ Ù†Ø´Ø¯." : "No active VPN adapter was detected.";
    public static string VpnAdapterRoute(string adapter, string gateway) => Fa ? $"Ù…Ø³ÛŒØ± Ù¾ÛŒØ´â€ŒÙØ±Ø¶ ÙØ¹Ø§Ù„: {adapter} Ø§Ø² gateway {gateway}." : $"Active default route: {adapter} via gateway {gateway}.";
    public static string VpnMultipleDefaultRoutes(int count) => Fa ? $"Ú†Ù†Ø¯ Ù…Ø³ÛŒØ± Ù¾ÛŒØ´â€ŒÙØ±Ø¶ Ù‡Ù…â€ŒØ²Ù…Ø§Ù† Ø¯ÛŒØ¯Ù‡ Ø´Ø¯ ({count}). Ø§Ú¯Ø± VPN ÛŒØ§ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø®Ø§Ø±Ø¬ÛŒ Ù…Ø´Ú©Ù„ Ø¯Ø§Ø±Ø¯ØŒ routeÙ‡Ø§ Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¨Ø§ Ù‡Ù… ØªØ¯Ø§Ø®Ù„ Ø¯Ø§Ø´ØªÙ‡ Ø¨Ø§Ø´Ù†Ø¯." : $"Multiple default routes are active ({count}). If VPN or international access is broken, routes may be conflicting.";
    public static string VpnAdapterHasDefaultRoute => Fa ? "ÛŒÚ© Ú©Ø§Ø±Øª Ø´Ø¨ÛŒÙ‡ VPN Ù…Ø³ÛŒØ± Ù¾ÛŒØ´â€ŒÙØ±Ø¶ Ø¯Ø§Ø±Ø¯Ø› Ø§Ú¯Ø± Ø§ÛŒÙ†ØªØ±Ù†Øª Ø®Ø§Ø±Ø¬ÛŒ Ù‚Ø·Ø¹ Ø§Ø³ØªØŒ route ÛŒØ§ DNS Ù‡Ù…Ø§Ù† VPN Ø±Ø§ Ø¨Ø±Ø±Ø³ÛŒ Ú©Ù†." : "A VPN-like adapter owns a default route; if international access fails, check that VPN route or DNS.";
    // ---- Proxy check ----
    public static string ProxyNoOutput => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP: Ø®Ø±ÙˆØ¬ÛŒâ€ŒØ§ÛŒ Ù†Ø¯Ø§Ø´Øª." : "WinHTTP proxy: no output.";
    public static string ProxyUserEnabled(string server) => Fa ? $"Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ú©Ø§Ø±Ø¨Ø± ÙØ¹Ø§Ù„ Ø§Ø³Øª: {server}" : $"User proxy is enabled: {server}";
    public static string ProxyUserDisabled => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ú©Ø§Ø±Ø¨Ø± ØºÛŒØ±ÙØ¹Ø§Ù„ Ø§Ø³Øª." : "User proxy is disabled.";
    public static string ProxyUserCheckFailed(string msg) => Fa ? $"Ø¨Ø±Ø±Ø³ÛŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ú©Ø§Ø±Ø¨Ø± Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯: {msg}" : $"User proxy check failed: {msg}";
    public static string ProxyWinHttpSet => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP ØªÙ†Ø¸ÛŒÙ… Ø´Ø¯Ù‡. Ø§Ú¯Ø± Ù‚Ø¯ÛŒÙ…ÛŒ Ø¨Ø§Ø´Ø¯ØŒ Ø¨Ø¹Ø¶ÛŒ Ø¨Ø±Ù†Ø§Ù…Ù‡â€ŒÙ‡Ø§ Ù†Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ù†Ø¯ Ø¨Ù‡ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¨Ø±Ø³Ù†Ø¯." : "WinHTTP proxy is configured. If it is stale, some apps may fail to reach the internet.";
    public static string ProxyUserOn => Fa ? "ÛŒÚ© Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ú©Ø§Ø±Ø¨Ø± ÙØ¹Ø§Ù„ Ø§Ø³Øª. Ù…Ø±ÙˆØ±Ú¯Ø±Ù‡Ø§ Ùˆ Ø¨Ø¹Ø¶ÛŒ Ø¨Ø±Ù†Ø§Ù…Ù‡â€ŒÙ‡Ø§ Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¨Ù‡ Ø¢Ù† ÙˆØ§Ø¨Ø³ØªÙ‡ Ø¨Ø§Ø´Ù†Ø¯." : "A user proxy is enabled. Browsers and some apps may depend on it.";
    public static string ProxyHealthy => Fa ? "ØªÙ†Ø¸ÛŒÙ… Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ù¾Ø±Ø®Ø·Ø±ÛŒ Ø¯Ø± ÙˆÛŒÙ†Ø¯ÙˆØ² Ø´Ù†Ø§Ø³Ø§ÛŒÛŒ Ù†Ø´Ø¯." : "No risky Windows proxy setting was detected.";

    // ---- Hosts file check ----
    public static string HostsOverrideFound(string host, string ip) => Fa
        ? $"Ø¢Ø¯Ø±Ø³ Â«{host}Â» Ø¯Ø± ÙØ§ÛŒÙ„ Hosts Ø¨Ù‡â€ŒØµÙˆØ±Øª Ø«Ø§Ø¨Øª Ø¨Ù‡ {ip} Ù†Ú¯Ø§Ø´Øª Ø´Ø¯Ù‡ Ø§Ø³Øª."
        : $"\"{host}\" is statically mapped to {ip} in the Hosts file.";
    public static string HostsOverrideHint => Fa
        ? "Ø§Ú¯Ø± Ø§ÛŒÙ† Ø®Ø· ØªÙˆØ³Ø· ÛŒÚ© Ù†Ø±Ù…â€ŒØ§ÙØ²Ø§Ø± Ù‚Ø¯ÛŒÙ…ÛŒ ÛŒØ§ ÛŒÚ© ØªØ¹Ù…ÛŒØ± Ù‚Ø¨Ù„ÛŒ Ø§Ø¶Ø§ÙÙ‡ Ø´Ø¯Ù‡ØŒ Ù‡Ù…ÛŒÙ† Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ø¯ Ø¯Ù„ÛŒÙ„ Ø¨Ø§Ø´Ø¯ Ú©Ù‡ ÙÙ‚Ø· Ù‡Ù…ÛŒÙ† ÛŒÚ© Ø³Ø§ÛŒØª Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆØ¯. Ø¢Ù† Ø±Ø§ Ù…ÛŒâ€ŒØªÙˆØ§Ù†ÛŒ Ø¨Ø§ Ø§Ø¯ÛŒØªÙˆØ± Ù…ØªÙ†ÛŒ (Ø¨Ù‡â€ŒØ¹Ù†ÙˆØ§Ù† Administrator) Ø§Ø² Ø§ÛŒÙ†Ø¬Ø§ Ø­Ø°Ù Ú©Ù†ÛŒ: %SystemRoot%\\System32\\drivers\\etc\\hosts"
        : "If this line was added by old software or a previous workaround, it can explain why only this one site fails while everything else works. You can remove it yourself (as Administrator) from: %SystemRoot%\\System32\\drivers\\etc\\hosts";
    public static string HostsNoOverride => Fa
        ? "Ù‡ÛŒÚ† Ø±Ú©ÙˆØ±Ø¯ Ø«Ø§Ø¨ØªÛŒ Ø¨Ø±Ø§ÛŒ Ø§ÛŒÙ† Ø¢Ø¯Ø±Ø³ Ø¯Ø± ÙØ§ÛŒÙ„ Hosts Ù¾ÛŒØ¯Ø§ Ù†Ø´Ø¯."
        : "No static override for this address was found in the Hosts file.";
    public static string HostsUnreadable(string err) => Fa
        ? $"ÙØ§ÛŒÙ„ Hosts Ù‚Ø§Ø¨Ù„ Ø®ÙˆØ§Ù†Ø¯Ù† Ù†Ø¨ÙˆØ¯: {err}"
        : $"The Hosts file could not be read: {err}";

    // ---- Plain-language summary ----
    public static string SummaryLocalOkIntlFails(string country) => Fa
        ? $"Ø§ØªØµØ§Ù„ Ø´Ù…Ø§ Ø¨Ù‡ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒ {country} Ù…ÛŒâ€ŒØ±Ø³Ø¯ØŒ Ø§Ù…Ø§ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨Ù‡ Ù…Ù‚ØµØ¯Ù‡Ø§ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ÛŒ Ù†Ø§Ù…ÙˆÙÙ‚ Ø§Ø³Øª. Ø§ÛŒÙ† Ø¨ÛŒØ´ØªØ± Ø¨Ù‡ Ù…Ø³ÛŒØ±Ø¯Ù‡ÛŒ Ø¨Ø§Ù„Ø§Ø¯Ø³ØªØŒ ÙÛŒÙ„ØªØ±ÛŒÙ†Ú¯ØŒ DNSØŒ VPN ÛŒØ§ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ Ø§Ù¾Ø±Ø§ØªÙˆØ± Ø§Ø´Ø§Ø±Ù‡ Ø¯Ø§Ø±Ø¯ ØªØ§ Ù‚Ø·Ø¹ Ú©Ø§Ù…Ù„ Ø§ÛŒÙ†ØªØ±Ù†Øª Ú©Ø§Ù…Ù¾ÛŒÙˆØªØ±."
        : $"Your connection can reach {country}-local internet, but international destinations are failing. This points to upstream routing, filtering, DNS, VPN, or ISP international access rather than a completely offline computer.";
    public static string SummaryIntlOkLocalFails => Fa
        ? "Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ Ú©Ø§Ø± Ù…ÛŒâ€ŒÚ©Ù†Ø¯ØŒ Ø§Ù…Ø§ ØªØ³Øª Ø¯Ø§Ø®Ù„ÛŒ Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯. Ù…Ø´Ú©Ù„ Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø§Ø² Ø³Ø±ÙˆÛŒØ³â€ŒÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒØŒ DNS Ø¯Ø§Ù…Ù†Ù‡â€ŒÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒ ÛŒØ§ Ø®ÙˆØ¯ Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ÛŒ Ù‡Ø¯Ù Ø¨Ø§Ø´Ø¯."
        : "International internet works, but the country-local test failed. The issue may be with local services, DNS for local domains, or the selected local targets.";
    public static string SummaryBothFail => Fa
        ? "Ù‡Ù… ØªØ³Øª Ø¯Ø§Ø®Ù„ÛŒ Ùˆ Ù‡Ù… Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯. Ú©Ø§Ù…Ù¾ÛŒÙˆØªØ±ØŒ Wiâ€‘FiØŒ Ù…ÙˆØ¯Ù…ØŒ gatewayØŒ DNSØŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ ÛŒØ§ Ù…Ø³ÛŒØ± VPN Ù†ÛŒØ§Ø² Ø¨Ù‡ Ø¨Ø±Ø±Ø³ÛŒ Ø¯Ø§Ø±Ø¯."
        : "Both local and international internet tests failed. The computer, Wi-Fi, modem, gateway, DNS, proxy, or VPN path needs attention.";
    public static string SummaryAllHealthy => Fa
        ? "Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¯Ø§Ø®Ù„ÛŒØŒ Ø§ÛŒÙ†ØªØ±Ù†Øª Ø¨ÛŒÙ†â€ŒØ§Ù„Ù…Ù„Ù„ØŒ DNSØŒ Ú©ÛŒÙÛŒØª Ø§ØªØµØ§Ù„ØŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ù¾ÙˆØ±ØªØŒ VPN Ùˆ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ù‡Ù…Ú¯ÛŒ Ø³Ø§Ù„Ù… Ø¨Ù‡ Ù†Ø¸Ø± Ù…ÛŒâ€ŒØ±Ø³Ù†Ø¯."
        : "Local internet, international internet, DNS, connection quality, port access, VPN and proxy checks look healthy.";

    // ---- Region names ----
    public static string CountryIran => Fa ? "Ø§ÛŒØ±Ø§Ù†" : "Iran";

    // ---- Problem tiles (the VPN title MUST keep the word "VPN" for the VPN check) ----
    public static string ProbWebsitesTitle => Fa ? "Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆÙ†Ø¯" : "Websites do not open";
    public static string ProbWebsitesDesc => Fa ? "Ø§ÛŒÙ†ØªØ±Ù†Øª ÙˆØµÙ„ Ø¨Ù‡ Ù†Ø¸Ø± Ù…ÛŒâ€ŒØ±Ø³Ø¯ØŒ Ø§Ù…Ø§ ØµÙØ­Ù‡â€ŒÙ‡Ø§ Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆÙ†Ø¯ ÛŒØ§ Ù…Ø¯Ø§Ù… Ø¯Ø± Ø­Ø§Ù„ Ø¨Ø§Ø±Ú¯Ø°Ø§Ø±ÛŒâ€ŒØ§Ù†Ø¯." : "Internet seems connected, but pages fail or spin forever.";
    public static string ProbIntlTitle => Fa ? "Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ÛŒ Ø®Ø§Ø±Ø¬ÛŒ Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆÙ†Ø¯" : "International sites fail";
    public static string ProbIntlDesc => Fa ? "Ø³Ø§ÛŒØªâ€ŒÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒ Ú©Ø§Ø± Ù…ÛŒâ€ŒÚ©Ù†Ù†Ø¯ØŒ Ø§Ù…Ø§ Ø³Ø±ÙˆÛŒØ³â€ŒÙ‡Ø§ÛŒ Ø®Ø§Ø±Ø¬ÛŒ Ù†Ù‡." : "Local sites may work, but global services do not.";
    public static string ProbOneSiteTitle => Fa ? "ÙÙ‚Ø· ÛŒÚ© Ø³Ø§ÛŒØª Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆØ¯" : "Only one website fails";
    public static string ProbOneSiteDesc => Fa ? "Ø¨Ø±Ø±Ø³ÛŒ DNS Ùˆ Ø¯Ø³ØªØ±Ø³ÛŒ TCP Ø¨Ø±Ø§ÛŒ ÛŒÚ© Ø³Ø§ÛŒØª Ù…Ø´Ø®Øµ." : "Check DNS and TCP access for one specific site.";
    public static string ProbVpnTitle => Fa ? "VPN ÙˆØµÙ„ Ø§Ø³Øª ÙˆÙ„ÛŒ Ø§ÛŒÙ†ØªØ±Ù†Øª Ù†Ø¯Ø§Ø±Ø¯" : "VPN connected, no internet";
    public static string ProbVpnDesc => Fa ? "Ù¾ÛŒØ¯Ø§ Ú©Ø±Ø¯Ù† Ù…Ø´Ú©Ù„ Ù…Ø³ÛŒØ±Ø¯Ù‡ÛŒØŒ DNS ÛŒØ§ Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ Ù†Ø§Ø´ÛŒ Ø§Ø² VPN." : "Find routing, DNS or adapter issues caused by VPN.";
    public static string ProbEmailTitle => Fa ? "Ø§ÛŒÙ…ÛŒÙ„ Ø¯Ø± ØµÙ†Ø¯ÙˆÙ‚ Ø®Ø±ÙˆØ¬ÛŒ Ú¯ÛŒØ± Ú©Ø±Ø¯Ù‡" : "Email stuck in Outbox";
    public static string ProbEmailDesc => Fa ? "Ø¨Ø±Ø±Ø³ÛŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¨Ù‡ SMTP Ø¨Ø±Ø§ÛŒ Ø§Ø±Ø³Ø§Ù„ Ø§ÛŒÙ…ÛŒÙ„." : "Check SMTP reachability for mail sending.";
    public static string ProbFileTitle => Fa ? "ÙØ§ÛŒÙ„ Ø´Ø¨Ú©Ù‡ Ø¨Ø§Ø² Ù†Ù…ÛŒâ€ŒØ´ÙˆØ¯" : "Network file does not open";
    public static string ProbFileDesc => Fa ? "Ø¨Ø±Ø±Ø³ÛŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ù¾ÙˆØ±Øª Ø§Ø´ØªØ±Ø§Ú© ÙØ§ÛŒÙ„ (SMB)." : "Check SMB/file sharing port access.";
    public static string ProbSlowTitle => Fa ? "Ø¨Ø±Ù†Ø§Ù…Ù‡ Ø¢Ù†Ù„Ø§ÛŒÙ† Ú©Ù†Ø¯ Ø§Ø³Øª" : "App is slow online";
    public static string ProbSlowDesc => Fa ? "Ø§Ù†Ø¯Ø§Ø²Ù‡â€ŒÚ¯ÛŒØ±ÛŒ ØªØ£Ø®ÛŒØ±ØŒ Ø§ØªÙ„Ø§Ù Ø¨Ø³ØªÙ‡ Ùˆ ÙˆØ¶Ø¹ÛŒØª Ù¾Ø±ÙˆÚ©Ø³ÛŒ." : "Measure latency, packet loss and proxy state.";
    public static string ProbAfterUpdateTitle => Fa ? "Ø¨Ø¹Ø¯ Ø§Ø² Ø¢Ù¾Ø¯ÛŒØªØŒ Ø§ÛŒÙ†ØªØ±Ù†Øª Ù‚Ø·Ø¹ Ø´Ø¯" : "After an update, internet broke";
    public static string ProbAfterUpdateDesc => Fa ? "Ø¨Ø±Ø±Ø³ÛŒ ØªØºÛŒÛŒØ±Ø§Øª DNSØŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒØŒ VPN Ùˆ Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡." : "Look for DNS, proxy, VPN and adapter changes.";

    // ---- Safe fix descriptions / messages ----
    public static string FixDescDnsPublic => Fa
        ? "DNS ÙØ¹Ù„ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ù…ÛŒâ€ŒØ´ÙˆØ¯ØŒ Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙØ¹Ø§Ù„ Ø±ÙˆÛŒ Û±.Û±.Û±.Û± Ùˆ Û¸.Û¸.Û¸.Û¸ ØªÙ†Ø¸ÛŒÙ… Ùˆ Ø³Ù¾Ø³ DNS Ù¾Ø§Ú©â€ŒØ³Ø§Ø²ÛŒ Ù…ÛŒâ€ŒØ´ÙˆØ¯."
        : "Save current DNS, switch the active adapter to 1.1.1.1 and 8.8.8.8, then flush DNS.";
    public static string FixDescProxyReset => Fa
        ? "ØªÙ†Ø¸ÛŒÙ… ÙØ¹Ù„ÛŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø°Ø®ÛŒØ±Ù‡ØŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø±ÛŒØ³Øª Ùˆ Ø³Ù¾Ø³ DNS Ù¾Ø§Ú©â€ŒØ³Ø§Ø²ÛŒ Ù…ÛŒâ€ŒØ´ÙˆØ¯."
        : "Save the current WinHTTP proxy setting, reset WinHTTP proxy, then flush DNS.";
    public static string FixDescQuickRefresh => Fa
        ? "Ø­Ø§ÙØ¸Ù‡â€ŒÛŒ DNS Ù¾Ø§Ú© Ùˆ ÙˆØ¶Ø¹ÛŒØª Ø³Ø¨Ú© Ù†Ø§Ù…â€ŒÚ¯Ø°Ø§Ø±ÛŒ Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙˆÛŒÙ†Ø¯ÙˆØ² Ø¨Ø§Ø²Ø¢ÙˆØ±ÛŒ Ù…ÛŒâ€ŒØ´ÙˆØ¯ØŒ Ø³Ù¾Ø³ Ø¹ÛŒØ¨â€ŒÛŒØ§Ø¨ÛŒ Ø¯ÙˆØ¨Ø§Ø±Ù‡ Ø§Ø¬Ø±Ø§ Ù…ÛŒâ€ŒØ´ÙˆØ¯."
        : "Flush DNS cache and refresh lightweight Windows network name-resolution state, then run diagnosis again.";
    public static string FixDescNone => Fa
        ? "Ø¨Ø±Ø§ÛŒ Ø§ÛŒÙ† Ù†ØªÛŒØ¬Ù‡ ØªØ¹Ù…ÛŒØ± Ø®ÙˆØ¯Ú©Ø§Ø± Ø§Ù…Ù†ÛŒ Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ù†ÛŒØ³Øª."
        : "No safe automatic repair is available for this result.";
    public static string FixDescDeepRepair => Fa
        ? "Ø±ÛŒØ³Øª Ú©Ø§Ù…Ù„ Ù¾Ø´ØªÙ‡â€ŒÛŒ Ø´Ø¨Ú©Ù‡ (Winsock Ùˆ TCP/IP). ÙÙ‚Ø· ÙˆÙ‚ØªÛŒ Ø§Ø³ØªÙØ§Ø¯Ù‡ Ú©Ù† Ú©Ù‡ Ø¨Ù‚ÛŒÙ‡â€ŒÛŒ Ø±Ø§Ù‡â€ŒÙ‡Ø§ Ø¬ÙˆØ§Ø¨ Ù†Ø¯Ø§Ø¯Ù‡â€ŒØ§Ù†Ø¯."
        : "Full network stack reset (Winsock and TCP/IP). Use only when nothing else works.";

    // ---- Deep (advanced) repair ----
    public static string DeepRepairButton => Fa ? "ØªØ¹Ù…ÛŒØ± Ù¾ÛŒØ´Ø±ÙØªÙ‡" : "Advanced repair";
    public static string DeepRepairWarning => Fa
        ? "Ø§ÛŒÙ† Ú©Ø§Ø± Ù¾Ø´ØªÙ‡â€ŒÛŒ Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙˆÛŒÙ†Ø¯ÙˆØ² Ø±Ø§ Ú©Ø§Ù…Ù„Ø§Ù‹ Ø±ÛŒØ³Øª Ù…ÛŒâ€ŒÚ©Ù†Ø¯ (Winsock Ùˆ TCP/IP). Ù…Ù…Ú©Ù† Ø§Ø³Øª ØªÙ†Ø¸ÛŒÙ…Ø§Øª VPNØŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ùˆ Ù†Ø±Ù…â€ŒØ§ÙØ²Ø§Ø±Ù‡Ø§ÛŒ Ø³Ø§Ø²Ù…Ø§Ù†ÛŒ Ø±Ø§ Ø¨Ù‡â€ŒÙ‡Ù… Ø¨Ø²Ù†Ø¯ Ùˆ Ø¨Ø±Ø§ÛŒ Ø§Ø¹Ù…Ø§Ù„ Ú©Ø§Ù…Ù„ Ø¨Ù‡ ÛŒÚ©â€ŒØ¨Ø§Ø± Ø±ÛŒâ€ŒØ§Ø³ØªØ§Ø±Øª Ù†ÛŒØ§Ø² Ø¯Ø§Ø±Ø¯.\n\nØ§ÛŒÙ† Ø§Ù‚Ø¯Ø§Ù… Ù‚Ø§Ø¨Ù„ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø®ÙˆØ¯Ú©Ø§Ø± Ù†ÛŒØ³Øª. ÙÙ‚Ø· ÙˆÙ‚ØªÛŒ Ø§Ø¯Ø§Ù…Ù‡ Ø¨Ø¯Ù‡ Ú©Ù‡ Ù…Ø·Ù…Ø¦Ù†ÛŒ Ø¨Ù‚ÛŒÙ‡â€ŒÛŒ Ø±Ø§Ù‡â€ŒÙ‡Ø§ Ø¬ÙˆØ§Ø¨ Ù†Ø¯Ø§Ø¯Ù‡â€ŒØ§Ù†Ø¯."
        : "This fully resets the Windows network stack (Winsock and TCP/IP). It may disrupt VPN, proxy and corporate software, and needs a restart to fully apply.\n\nThis action cannot be undone automatically. Continue only if you are sure nothing else worked.";
    public static string DeepRepairApplied => Fa
        ? "Ù¾Ø´ØªÙ‡â€ŒÛŒ Ø´Ø¨Ú©Ù‡ Ø±ÛŒØ³Øª Ø´Ø¯. Ù„Ø·ÙØ§Ù‹ Ú©Ø§Ù…Ù¾ÛŒÙˆØªØ± Ø±Ø§ ÛŒÚ©â€ŒØ¨Ø§Ø± Ø±ÛŒâ€ŒØ§Ø³ØªØ§Ø±Øª Ú©Ù† ØªØ§ ØªØºÛŒÛŒØ±Ø§Øª Ú©Ø§Ù…Ù„ Ø§Ø¹Ù…Ø§Ù„ Ø´ÙˆÙ†Ø¯."
        : "The network stack was reset. Please restart the computer so the changes fully apply.";
    public static string DeepRepairRejected => Fa
        ? "ØªØ¹Ù…ÛŒØ± Ù¾ÛŒØ´Ø±ÙØªÙ‡ Ø§Ù†Ø¬Ø§Ù… Ù†Ø´Ø¯. Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¯Ø³ØªØ±Ø³ÛŒ Administrator Ù„ØºÙˆ Ø´Ø¯Ù‡ Ø¨Ø§Ø´Ø¯."
        : "Advanced repair did not run. Administrator permission may have been cancelled.";

    // ---- DNS chooser dialog ----
    public static string DnsChooserTitle => Fa ? "Ø§Ù†ØªØ®Ø§Ø¨ DNS Ø§Ù…Ù†" : "Choose a safe DNS";
    public static string DnsChooserPrompt => Fa
        ? "ÛŒÚ© DNS Ø§Ù†ØªØ®Ø§Ø¨ Ú©Ù†. ØªÙ†Ø¸ÛŒÙ… ÙØ¹Ù„ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ù…ÛŒâ€ŒØ´ÙˆØ¯ Ùˆ Ø¨Ø§ Â«Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒÂ» Ù‚Ø§Ø¨Ù„ Ø¨Ø±Ú¯Ø´Øª Ø§Ø³Øª:"
        : "Pick a DNS. Your current setting is saved and can be reverted with Undo:";
    public static string DnsChooserRecommended => Fa ? "(Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ÛŒ)" : "(recommended)";
    public static string Apply => Fa ? "Ø§Ø¹Ù…Ø§Ù„" : "Apply";
    public static string Cancel => Fa ? "Ø§Ù†ØµØ±Ø§Ù" : "Cancel";

    // ---- Before / after comparison ----
    public static string ResultBetter => Fa ? "âœ… ÙˆØ¶Ø¹ÛŒØª Ø¨Ù‡ØªØ± Ø´Ø¯: Ù…Ø´Ú©Ù„Ø§Øª Ú©Ù…ØªØ± Ø´Ø¯Ù†Ø¯." : "âœ… Things improved: fewer problems than before.";
    public static string ResultSame => Fa ? "â„¹ï¸ ÙˆØ¶Ø¹ÛŒØª ØªØºÛŒÛŒØ± Ù…Ø­Ø³ÙˆØ³ÛŒ Ù†Ú©Ø±Ø¯." : "â„¹ï¸ No noticeable change.";
    public static string ResultWorse => Fa ? "âš ï¸ ÙˆØ¶Ø¹ÛŒØª Ø¨Ø¯ØªØ± Ø´Ø¯. Ù…ÛŒâ€ŒØªÙˆØ§Ù†ÛŒ Ø¨Ø§ Â«Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒÂ» Ø¨Ù‡ Ø­Ø§Ù„Øª Ù‚Ø¨Ù„ Ø¨Ø±Ú¯Ø±Ø¯ÛŒ." : "âš ï¸ Things got worse. You can return to the previous state with Undo.";

    // ---- Licensing ----
    public static string LicenseTitle => Fa ? "ÙØ¹Ø§Ù„â€ŒØ³Ø§Ø²ÛŒ Ù†ÙØªâ€ŒØ¯Ú©ØªØ±" : "Activate Net Doctor";
    public static string LicenseIntro => Fa
        ? "Ø¨Ø±Ø§ÛŒ Ø§Ø³ØªÙØ§Ø¯Ù‡ Ø§Ø² Ù†ÙØªâ€ŒØ¯Ú©ØªØ± Ø¨Ù‡ ÛŒÚ© Ù„Ø§ÛŒØ³Ù†Ø³ Ù…Ø¹ØªØ¨Ø± Ù†ÛŒØ§Ø² Ø¯Ø§Ø±ÛŒ. Ø§ÙˆÙ„ Ø´Ù†Ø§Ø³Ù‡â€ŒÛŒ Ø¯Ø³ØªÚ¯Ø§Ù‡Øª Ø±Ø§ Ø¨Ø±Ø§ÛŒ ÙØ±ÙˆØ´Ù†Ø¯Ù‡ Ø¨ÙØ±Ø³ØªØŒ Ø¨Ø¹Ø¯ Ú©Ù„ÛŒØ¯ Ø¯Ø±ÛŒØ§ÙØªÛŒ Ø±Ø§ Ø§ÛŒÙ†â€ŒØ¬Ø§ ÙˆØ§Ø±Ø¯ Ú©Ù†:"
        : "Net Doctor needs a valid license to run. First send your Machine ID to the seller, then paste the license key you receive below:";
    public static string LicenseMachineLabel => Fa ? "Ø´Ù†Ø§Ø³Ù‡â€ŒÛŒ Ø¯Ø³ØªÚ¯Ø§Ù‡ Ø´Ù…Ø§" : "Your Machine ID";
    public static string LicenseMachineHint => Fa
        ? "Ø§ÛŒÙ† Ø´Ù†Ø§Ø³Ù‡ Ø±Ø§ Ø¨Ø±Ø§ÛŒ ÙØ±ÙˆØ´Ù†Ø¯Ù‡ Ø¨ÙØ±Ø³Øª ØªØ§ Ù„Ø§ÛŒØ³Ù†Ø³ Ù…Ø®ØµÙˆØµ Ù‡Ù…ÛŒÙ† Ú©Ø§Ù…Ù¾ÛŒÙˆØªØ± Ø¨Ø±Ø§ÛŒØª ØµØ§Ø¯Ø± Ø´ÙˆØ¯."
        : "Send this ID to the seller so a license is issued for this computer only.";
    public static string LicenseCopyButton => Fa ? "Ú©Ù¾ÛŒ" : "Copy";
    public static string LicenseCopied => Fa ? "Ø´Ù†Ø§Ø³Ù‡â€ŒÛŒ Ø¯Ø³ØªÚ¯Ø§Ù‡ Ú©Ù¾ÛŒ Ø´Ø¯." : "Machine ID copied.";
    public static string LicenseKeyLabel => Fa ? "Ú©Ù„ÛŒØ¯ Ù„Ø§ÛŒØ³Ù†Ø³" : "License key";
    public static string LicenseActivateButton => Fa ? "ÙØ¹Ø§Ù„â€ŒØ³Ø§Ø²ÛŒ" : "Activate";
    public static string LicenseExitButton => Fa ? "Ø®Ø±ÙˆØ¬" : "Exit";
    public static string LicenseBuyHint => Fa
        ? "Ù„Ø§ÛŒØ³Ù†Ø³ Ù†Ø¯Ø§Ø±ÛŒØŸ Ø¨Ø±Ø§ÛŒ Ø®Ø±ÛŒØ¯ Ø¨Ø§ Ù¾Ø´ØªÛŒØ¨Ø§Ù†ÛŒ ØªÙ…Ø§Ø³ Ø¨Ú¯ÛŒØ±."
        : "No license? Contact support to purchase one.";
    public const string LicensePurchaseUrl = "https://ateight.xyz/NetDoctor/";
    public static string LicenseBuyLinkText => Fa
        ? "Ø®Ø±ÛŒØ¯ Ù„Ø§ÛŒØ³Ù†Ø³: ateight.xyz/NetDoctor"
        : "Buy a license: ateight.xyz/NetDoctor";

    public static string LicenseActivated(string name, int days) => Fa
        ? $"Ù„Ø§ÛŒØ³Ù†Ø³ ÙØ¹Ø§Ù„ Ø´Ø¯. Ø®ÙˆØ´ Ø¢Ù…Ø¯ÛŒ {name}! Ø§Ø¹ØªØ¨Ø§Ø±: {days} Ø±ÙˆØ²."
        : $"License activated. Welcome {name}! Valid for {days} more day(s).";

    public static string LicenseErrMissing => Fa ? "Ú©Ù„ÛŒØ¯ Ù„Ø§ÛŒØ³Ù†Ø³ Ø±Ø§ ÙˆØ§Ø±Ø¯ Ú©Ù†." : "Please enter a license key.";
    public static string LicenseErrMalformed => Fa ? "ÙØ±Ù…Øª Ú©Ù„ÛŒØ¯ Ù„Ø§ÛŒØ³Ù†Ø³ Ù†Ø§Ù…Ø¹ØªØ¨Ø± Ø§Ø³Øª." : "The license key format is invalid.";
    public static string LicenseErrBadSignature => Fa ? "Ú©Ù„ÛŒØ¯ Ù„Ø§ÛŒØ³Ù†Ø³ Ù…Ø¹ØªØ¨Ø± Ù†ÛŒØ³Øª ÛŒØ§ Ø¯Ø³ØªÚ©Ø§Ø±ÛŒ Ø´Ø¯Ù‡ Ø§Ø³Øª." : "The license key is not valid or has been tampered with.";
    public static string LicenseErrExpired => Fa ? "Ø§Ø¹ØªØ¨Ø§Ø± Ø§ÛŒÙ† Ù„Ø§ÛŒØ³Ù†Ø³ ØªÙ…Ø§Ù… Ø´Ø¯Ù‡ Ø§Ø³Øª. Ù„Ø·ÙØ§Ù‹ Ù„Ø§ÛŒØ³Ù†Ø³ Ø¬Ø¯ÛŒØ¯ ØªÙ‡ÛŒÙ‡ Ú©Ù†." : "This license has expired. Please obtain a new one.";
    public static string LicenseErrWrongEdition => Fa ? "Ø§ÛŒÙ† Ù„Ø§ÛŒØ³Ù†Ø³ Ø¨Ø±Ø§ÛŒ Ù†Ø³Ø®Ù‡â€ŒÛŒ Ø¯ÛŒÚ¯Ø±ÛŒ ØµØ§Ø¯Ø± Ø´Ø¯Ù‡ Ùˆ Ø¨Ø§ Ø§ÛŒÙ† Ù†Ø³Ø®Ù‡ Ú©Ø§Ø± Ù†Ù…ÛŒâ€ŒÚ©Ù†Ø¯." : "This license was issued for a different edition and does not work with this build.";
    public static string LicenseErrWrongMachine => Fa ? "Ø§ÛŒÙ† Ù„Ø§ÛŒØ³Ù†Ø³ Ø¨Ø±Ø§ÛŒ Ú©Ø§Ù…Ù¾ÛŒÙˆØªØ± Ø¯ÛŒÚ¯Ø±ÛŒ ØµØ§Ø¯Ø± Ø´Ø¯Ù‡ Ø§Ø³Øª. Ø´Ù†Ø§Ø³Ù‡â€ŒÛŒ Ø¯Ø³ØªÚ¯Ø§Ù‡ Ø®ÙˆØ¯ Ø±Ø§ Ø¨Ø±Ø§ÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù†ÛŒ Ø§Ø±Ø³Ø§Ù„ Ú©Ù†ÛŒØ¯." : "This license was issued for another computer. Send your Machine ID to support to obtain the correct license.";
    public static string LicenseErrMachineUnavailable => Fa ? "Ø´Ù†Ø§Ø³Ù‡â€ŒÛŒ Ø¯Ø³ØªÚ¯Ø§Ù‡ Ù‚Ø§Ø¨Ù„ Ø®ÙˆØ§Ù†Ø¯Ù† Ù†ÛŒØ³Øª. Ù„Ø·ÙØ§Ù‹ Ø¨Ø±Ù†Ø§Ù…Ù‡ Ø±Ø§ Ø¯ÙˆØ¨Ø§Ø±Ù‡ Ø§Ø¬Ø±Ø§ Ú©Ù† ÛŒØ§ Ø¨Ø§ Ù¾Ø´ØªÛŒØ¨Ø§Ù†ÛŒ ØªÙ…Ø§Ø³ Ø¨Ú¯ÛŒØ±." : "The Machine ID could not be read. Please restart the app or contact support.";
    public static string LicenseErrClock => Fa ? "Ø³Ø§Ø¹Øª Ø³ÛŒØ³ØªÙ… Ø¨Ù‡ Ø¹Ù‚Ø¨ Ø¨Ø±Ú¯Ø´ØªÙ‡ Ø§Ø³Øª. Ù„Ø·ÙØ§Ù‹ ØªØ§Ø±ÛŒØ® Ùˆ Ø³Ø§Ø¹Øª ÙˆÛŒÙ†Ø¯ÙˆØ² Ø±Ø§ Ø¯Ø±Ø³Øª Ú©Ù†." : "The system clock appears to be set back. Please correct the Windows date and time.";

    // ---- License status shown on the dashboard ----
    public static string LicenseDaysRemaining(int days) => Fa ? $"Ù„Ø§ÛŒØ³Ù†Ø³: {days} Ø±ÙˆØ² Ø¨Ø§Ù‚ÛŒ Ù…Ø§Ù†Ø¯Ù‡" : $"License: {days} day(s) left";
    public static string LicenseExpiringSoon(int days) => Fa ? $"âš ï¸ Ù„Ø§ÛŒØ³Ù†Ø³ ØªØ§ {days} Ø±ÙˆØ² Ø¯ÛŒÚ¯Ø± Ù…Ù†Ù‚Ø¶ÛŒ Ù…ÛŒâ€ŒØ´ÙˆØ¯" : $"âš ï¸ License expires in {days} day(s)";

    public static string FixNoAdapter => Fa ? "Ù‡ÛŒÚ† Ú©Ø§Ø±Øª Ø´Ø¨Ú©Ù‡â€ŒÛŒ ÙØ¹Ø§Ù„ÛŒ Ù¾ÛŒØ¯Ø§ Ù†Ø´Ø¯. ØªØºÛŒÛŒØ±ÛŒ Ø§Ø¹Ù…Ø§Ù„ Ù†Ø´Ø¯." : "No active network adapter was found. No changes were made.";
    public static string FixDnsRejected => Fa ? "DNS ØªØºÛŒÛŒØ± Ù†Ú©Ø±Ø¯. Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¯Ø³ØªØ±Ø³ÛŒ Administrator Ù„ØºÙˆ Ø´Ø¯Ù‡ Ø¨Ø§Ø´Ø¯ ÛŒØ§ ÙˆÛŒÙ†Ø¯ÙˆØ² ØªØºÛŒÛŒØ± Ø±Ø§ Ù†Ù¾Ø°ÛŒØ±ÙØªÙ‡ Ø¨Ø§Ø´Ø¯." : "DNS was not changed. Administrator permission may have been cancelled or Windows rejected the change.";
    public static string FixDnsApplied(string adapter) => Fa ? $"DNS Ø±ÙˆÛŒ Â«{adapter}Â» Ø¨Ù‡ Û±.Û±.Û±.Û± Ùˆ Û¸.Û¸.Û¸.Û¸ ØªØºÛŒÛŒØ± Ú©Ø±Ø¯. ØªÙ†Ø¸ÛŒÙ… Ù‚Ø¨Ù„ÛŒ Ø¨Ø±Ø§ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ø´Ø¯." : $"DNS on {adapter} was changed to 1.1.1.1 and 8.8.8.8. The previous setting was saved for Undo.";
    public static string FixDnsAppliedPreset(string adapter, string preset) => Fa ? $"DNS Ø±ÙˆÛŒ Â«{adapter}Â» Ø¨Ù‡ {preset} ØªØºÛŒÛŒØ± Ú©Ø±Ø¯. ØªÙ†Ø¸ÛŒÙ… Ù‚Ø¨Ù„ÛŒ Ø¨Ø±Ø§ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ø´Ø¯." : $"DNS on {adapter} was changed to {preset}. The previous setting was saved for Undo.";
    public static string FixDnsAppliedAuto(string adapter) => Fa ? $"DNS Ø±ÙˆÛŒ Â«{adapter}Â» Ø¨Ù‡ Ø­Ø§Ù„Øª Ø®ÙˆØ¯Ú©Ø§Ø± (DHCP) Ø¨Ø±Ú¯Ø´Øª. ØªÙ†Ø¸ÛŒÙ… Ù‚Ø¨Ù„ÛŒ Ø¨Ø±Ø§ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ø´Ø¯." : $"DNS on {adapter} was set back to automatic (DHCP). The previous setting was saved for Undo.";
    public static string FixProxyRejected => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø±ÛŒØ³Øª Ù†Ø´Ø¯. Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø¯Ø³ØªØ±Ø³ÛŒ Administrator Ù„ØºÙˆ Ø´Ø¯Ù‡ Ø¨Ø§Ø´Ø¯ ÛŒØ§ ÙˆÛŒÙ†Ø¯ÙˆØ² ØªØºÛŒÛŒØ± Ø±Ø§ Ù†Ù¾Ø°ÛŒØ±ÙØªÙ‡ Ø¨Ø§Ø´Ø¯." : "WinHTTP proxy was not reset. Administrator permission may have been cancelled or Windows rejected the change.";
    public static string FixProxyApplied => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø±ÛŒØ³Øª Ùˆ Ø­Ø§ÙØ¸Ù‡â€ŒÛŒ DNS Ù¾Ø§Ú© Ø´Ø¯. Ø®Ø±ÙˆØ¬ÛŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ù‚Ø¨Ù„ÛŒ Ø¨Ø±Ø§ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø°Ø®ÛŒØ±Ù‡ Ø´Ø¯." : "WinHTTP proxy was reset and DNS cache was flushed. The previous proxy output was saved for Undo.";

    public static string RefreshDnsFlushed => Fa ? "Ø­Ø§ÙØ¸Ù‡â€ŒÛŒ DNS Ù¾Ø§Ú© Ø´Ø¯." : "DNS cache flushed.";
    public static string RefreshDnsFlushFailed(string err) => Fa ? $"Ù¾Ø§Ú©â€ŒØ³Ø§Ø²ÛŒ Ø­Ø§ÙØ¸Ù‡â€ŒÛŒ DNS Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯: {err}" : $"DNS cache flush failed: {err}";
    public static string RefreshRegisterOk => Fa ? "Ø¨Ø§Ø²Ø¢ÙˆØ±ÛŒ Ø«Ø¨Øª DNS Ø¯Ø±Ø®ÙˆØ§Ø³Øª Ø´Ø¯." : "DNS registration refresh requested.";
    public static string RefreshRegisterFail => Fa ? "Ø¨Ø§Ø²Ø¢ÙˆØ±ÛŒ Ø«Ø¨Øª DNS Ù‚Ø§Ø¨Ù„ Ø¯Ø±Ø®ÙˆØ§Ø³Øª Ù†Ø¨ÙˆØ¯." : "DNS registration refresh could not be requested.";
    public static string RefreshProxyDirect => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø§Ø² Ù‚Ø¨Ù„ Ø±ÙˆÛŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ù…Ø³ØªÙ‚ÛŒÙ… Ø§Ø³Øª." : "WinHTTP proxy already uses direct access.";
    public static string RefreshProxyConfigured => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP ØªÙ†Ø¸ÛŒÙ… Ø´Ø¯Ù‡Ø› Ù†Øªâ€ŒØ¯Ú©ØªØ± Ø¨Ø¯ÙˆÙ† Ù‡Ø´Ø¯Ø§Ø± Ø§Ø®ØªØµØ§ØµÛŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ø¢Ù† Ø±Ø§ ØªØºÛŒÛŒØ± Ù†Ø¯Ø§Ø¯." : "WinHTTP proxy is configured; Net Doctor did not change it without a proxy-specific warning.";

    public static string UndoNoSnapshot => Fa ? "Ù‡ÛŒÚ† Ù†Ø³Ø®Ù‡â€ŒÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù† ØªØ¹Ù…ÛŒØ±ÛŒ Ù¾ÛŒØ¯Ø§ Ù†Ø´Ø¯." : "No saved repair snapshot was found.";
    public static string UndoInvalid => Fa ? "Ù†Ø³Ø®Ù‡â€ŒÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù† ØªØ¹Ù…ÛŒØ± Ù…Ø¹ØªØ¨Ø± Ù†ÛŒØ³Øª." : "The repair snapshot is not valid.";
    public static string UndoUnsupported => Fa ? "Ø§ÛŒÙ† Ù†Ø³Ø®Ù‡â€ŒÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù† Ø¨Ø§ Ø§ÛŒÙ† Ù†Ø³Ø®Ù‡â€ŒÛŒ Ø¨Ø±Ù†Ø§Ù…Ù‡ Ù‚Ø§Ø¨Ù„ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ù†ÛŒØ³Øª." : "This snapshot cannot be restored by this version.";
    public static string UndoDnsMissing => Fa ? "Ù†Ø³Ø®Ù‡â€ŒÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù† DNS Ù…ÙˆØ¬ÙˆØ¯ Ù†ÛŒØ³Øª." : "The DNS repair snapshot is missing.";
    public static string UndoDhcpFailed => Fa ? "Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ DNS Ø¨Ù‡ DHCP Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯. Ù„Ø·ÙØ§Ù‹ Ø¯Ø³ØªØ±Ø³ÛŒ Administrator Ø±Ø§ Ø¨Ø±Ø±Ø³ÛŒ Ú©Ù†." : "Restoring DNS to DHCP failed. Please check Administrator permission.";
    public static string UndoDnsServerFailed => Fa ? "Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø³Ø±ÙˆØ± DNS Ù‚Ø¨Ù„ÛŒ Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯. Ù„Ø·ÙØ§Ù‹ Ø¯Ø³ØªØ±Ø³ÛŒ Administrator Ø±Ø§ Ø¨Ø±Ø±Ø³ÛŒ Ú©Ù†." : "Restoring the previous DNS server failed. Please check Administrator permission.";
    public static string UndoDnsRestored => Fa ? "ØªÙ†Ø¸ÛŒÙ… DNS Ù‚Ø¨Ù„ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†Ø¯Ù‡ Ø´Ø¯." : "The previous DNS setting was restored.";
    public static string UndoProxyDirectRestored => Fa ? "Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ø¨Ù‡ Ø¯Ø³ØªØ±Ø³ÛŒ Ù…Ø³ØªÙ‚ÛŒÙ… Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†Ø¯Ù‡ Ø´Ø¯." : "WinHTTP proxy was restored to direct access.";
    public static string UndoProxyDirectFailed => Fa ? "Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ø¯Ø³ØªØ±Ø³ÛŒ Ù…Ø³ØªÙ‚ÛŒÙ… WinHTTP Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯." : "Restoring WinHTTP direct access failed.";
    public static string UndoProxyParseFailed => Fa ? "Ù…Ù‚Ø¯Ø§Ø± Ù¾Ø±ÙˆÚ©Ø³ÛŒ Ù‚Ø¨Ù„ÛŒ Ù‚Ø§Ø¨Ù„ Ø®ÙˆØ§Ù†Ø¯Ù† Ù†Ø¨ÙˆØ¯. Ù†Ø³Ø®Ù‡â€ŒÛŒ Ù¾Ø´ØªÛŒØ¨Ø§Ù† Ø°Ø®ÛŒØ±Ù‡â€ŒØ´Ø¯Ù‡ Ø¯Ø± AppData Ø¨Ø§Ù‚ÛŒ Ù…ÛŒâ€ŒÙ…Ø§Ù†Ø¯." : "The previous proxy value could not be parsed. The saved snapshot remains in AppData.";
    public static string UndoProxyRestored => Fa ? "ØªÙ†Ø¸ÛŒÙ… Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ù‚Ø¨Ù„ÛŒ Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†Ø¯Ù‡ Ø´Ø¯." : "The previous WinHTTP proxy setting was restored.";
    public static string UndoProxyFailed => Fa ? "Ø¨Ø§Ø²Ú¯Ø±Ø¯Ø§Ù†ÛŒ Ù¾Ø±ÙˆÚ©Ø³ÛŒ WinHTTP Ù†Ø§Ù…ÙˆÙÙ‚ Ø¨ÙˆØ¯." : "Restoring WinHTTP proxy failed.";
}
