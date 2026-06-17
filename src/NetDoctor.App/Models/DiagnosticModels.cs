namespace NetDoctor.App.Models;

internal enum CheckStatus
{
    Unknown,
    Running,
    Healthy,
    Warning,
    Failed
}

internal sealed record DiagnosticCheck(
    string Id,
    string Title,
    CheckStatus Status,
    string Summary,
    IReadOnlyList<string> Details,
    string? SafeFixKey = null);

internal sealed record DiagnosticReport(
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    string Scenario,
    IReadOnlyList<DiagnosticCheck> Checks,
    string PlainLanguageSummary,
    bool HasSafeFix,
    string? SafeFixKey);

internal sealed record DiagnosticProgress(string CheckId, DiagnosticCheck Check);

internal sealed record PortProbeRequest(string Host, int Port);

internal sealed record AdapterDnsSnapshot(
    string AdapterName,
    bool WasDhcp,
    IReadOnlyList<string> Ipv4Servers);

internal sealed record SafeFixSnapshot(
    DateTimeOffset CreatedAt,
    string FixKey,
    AdapterDnsSnapshot? DnsSnapshot,
    ProxySnapshot? ProxySnapshot = null);

internal sealed record ProxySnapshot(string WinHttpProxyOutput);
