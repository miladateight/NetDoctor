using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using NetDoctor.App.Models;

namespace NetDoctor.App.Services;

internal sealed class HistorySessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> SaveAsync(DiagnosticReport report, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(PathService.HistorySessionsDirectory);
        var id = $"session-{report.StartedAt:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}";
        var path = Path.Combine(PathService.HistorySessionsDirectory, $"{id}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, JsonOptions), cancellationToken);
        return path;
    }

    public IReadOnlyList<string> ListSessionFiles()
    {
        Directory.CreateDirectory(PathService.HistorySessionsDirectory);
        return Directory.GetFiles(PathService.HistorySessionsDirectory, "session-*.json")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();
    }

    public DiagnosticReport? Open(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<DiagnosticReport>(File.ReadAllText(path), JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

internal sealed class ReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> ExportAsync(DiagnosticReport report, string format, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(PathService.ReportsDirectory);
        var extension = format.Equals("json", StringComparison.OrdinalIgnoreCase) ? "json"
            : format.Equals("html", StringComparison.OrdinalIgnoreCase) ? "html"
            : "txt";
        var path = Path.Combine(PathService.ReportsDirectory, $"netdoctor-report-{report.FinishedAt:yyyyMMdd-HHmmss}.{extension}");
        var content = extension switch
        {
            "json" => JsonSerializer.Serialize(report, JsonOptions),
            "html" => BuildHtml(report),
            _ => BuildText(report)
        };
        await File.WriteAllTextAsync(path, content, cancellationToken);
        return path;
    }

    private static string BuildText(DiagnosticReport report)
    {
        var lines = new List<string>
        {
            "Net Doctor Report",
            $"Started: {report.StartedAt:O}",
            $"Finished: {report.FinishedAt:O}",
            $"Scenario: {report.Scenario}",
            string.Empty,
            report.PlainLanguageSummary,
            string.Empty
        };

        foreach (var check in report.Checks)
        {
            lines.Add($"[{check.Status}] {check.Title}: {check.Summary}");
            lines.AddRange(check.Details.Select(detail => $"  - {detail}"));
            lines.Add(string.Empty);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildHtml(DiagnosticReport report)
    {
        var rows = string.Join(Environment.NewLine, report.Checks.Select(check =>
            $"<tr><td>{E(check.Title)}</td><td>{E(check.Status.ToString())}</td><td>{E(check.Summary)}</td></tr>"));
        return "<!doctype html><html><head><meta charset=\"utf-8\"><title>Net Doctor Report</title>" +
               "<style>body{font-family:Segoe UI,Tahoma,sans-serif;margin:32px;background:#f7f7fb;color:#181826}table{width:100%;border-collapse:collapse;background:white}td,th{padding:12px;border-bottom:1px solid #ececf5;text-align:left}.status{font-weight:700}</style>" +
               $"</head><body><h1>Net Doctor Report</h1><p>{E(report.PlainLanguageSummary)}</p><table><thead><tr><th>Check</th><th>Status</th><th>Summary</th></tr></thead><tbody>{rows}</tbody></table></body></html>";
    }

    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}

internal sealed class UpdateCheckService
{
    public async Task<string?> CheckNotificationAsync(Uri jsonUri, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        var json = await client.GetStringAsync(jsonUri, cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("message", out var message))
        {
            return message.GetString();
        }

        return null;
    }
}

internal static class AutoStartService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "NetDoctor";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(ValueName) is string;
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true) ?? Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
        {
            key.SetValue(ValueName, $"\"{Application.ExecutablePath}\"");
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}

internal sealed record SpeedTestResult(double LatencyMs, double JitterMs, double DownloadMbps, bool UsedFallback);

internal sealed class SpeedTestService
{
    public async Task<SpeedTestResult> RunAsync(AppSettings settings, IProgress<string>? progress, CancellationToken cancellationToken)
    {
        var latencySamples = new List<double>();
        for (var i = 0; i < 4; i++)
        {
            var start = Stopwatch.GetTimestamp();
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync("1.1.1.1", 2000);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                latencySamples.Add(reply.RoundtripTime);
            }
            else
            {
                latencySamples.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            }
        }

        var latency = latencySamples.Count == 0 ? 0 : latencySamples.Average();
        var jitter = latencySamples.Count <= 1 ? 0 : latencySamples.Zip(latencySamples.Skip(1), (a, b) => Math.Abs(a - b)).Average();
        var usedFallback = false;
        double download;
        try
        {
            progress?.Report("primary");
            download = await MeasureDownloadAsync(settings.SpeedTestPrimaryEndpoint, cancellationToken);
        }
        catch
        {
            usedFallback = true;
            progress?.Report("fallback");
            download = await MeasureDownloadAsync(settings.SpeedTestFallbackEndpoint, cancellationToken);
        }

        return new SpeedTestResult(latency, jitter, download, usedFallback);
    }

    private static async Task<double> MeasureDownloadAsync(string endpoint, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
        var start = Stopwatch.GetTimestamp();
        var bytes = await client.GetByteArrayAsync(endpoint, cancellationToken);
        var elapsed = Math.Max(Stopwatch.GetElapsedTime(start).TotalSeconds, 0.001);
        return bytes.Length * 8d / 1_000_000d / elapsed;
    }
}
