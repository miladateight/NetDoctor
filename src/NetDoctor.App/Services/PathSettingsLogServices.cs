using System.Text.Json;
using System.Text.Json.Serialization;
using NetDoctor.App.Localization;

namespace NetDoctor.App.Services;

internal static class PathService
{
    public static string ProgramDataRoot => Ensure(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NetDoctor");
    public static string RoamingRoot => Ensure(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetDoctor");
    public static string LocalRoot => Ensure(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetDoctor");

    public static string LicenseFile => Path.Combine(ProgramDataRoot, "license.json");
    public static string SettingsFile => Path.Combine(RoamingRoot, "settings.json");
    public static string LogsDirectory => Ensure(LocalRoot, "logs");
    public static string TodayLogFile => Path.Combine(LogsDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
    public static string HistorySessionsDirectory => Ensure(RoamingRoot, "history", "sessions");
    public static string SnapshotsDirectory => Ensure(RoamingRoot, "snapshots");
    public static string ReportsDirectory => Ensure(RoamingRoot, "reports");

    public static string Ensure(params string[] parts)
    {
        var path = Path.Combine(parts);
        Directory.CreateDirectory(path);
        return path;
    }
}

internal sealed record AppSettings
{
    public string Language { get; init; } = "en";
    public string Region { get; init; } = "World";
    public string Theme { get; init; } = "System";
    public bool ReducedMotion { get; init; }
    public bool FirstRunCompleted { get; init; }
    public bool AutoStartEnabled { get; init; }
    public bool MinimizeToTrayOnClose { get; init; } = true;
    public string SpeedTestPrimaryEndpoint { get; init; } = "https://speed.cloudflare.com/__down?bytes=2000000";
    public string SpeedTestFallbackEndpoint { get; init; } = "https://proof.ovh.net/files/1Mb.dat";
}

internal static class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(PathService.SettingsFile))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(PathService.SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(PathService.SettingsFile)!;
        Directory.CreateDirectory(directory);
        var temp = Path.Combine(directory, $"settings.{Guid.NewGuid():N}.tmp");
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(temp, json);
        File.Move(temp, PathService.SettingsFile, overwrite: true);
    }

    public static AppSettings LoadAndApply()
    {
        var settings = Load();
        AppConfig.Apply(settings.Language, settings.Region, settings.Theme, settings.ReducedMotion);
        return settings;
    }
}

internal static class LogService
{
    public static void Info(string message) => Write("INFO", message);

    public static void Warn(string message) => Write("WARN", message);

    public static void Error(string message, Exception? exception = null)
    {
        Write("ERROR", exception is null ? message : $"{message}{Environment.NewLine}{exception}");
    }

    private static void Write(string level, string message)
    {
        Directory.CreateDirectory(PathService.LogsDirectory);
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}";
        using var stream = new FileStream(PathService.TodayLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream);
        writer.Write(line);
    }
}
