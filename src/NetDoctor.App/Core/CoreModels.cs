namespace NetDoctor.App.Core;

internal enum Severity
{
    Healthy,
    Info,
    Warning,
    Error,
    Critical
}

internal enum ProblemType
{
    Unknown,
    Dns,
    Gateway,
    Internet,
    Adapter,
    Proxy,
    PacketLoss,
    Vpn,
    HostsFile,
    Port
}

internal enum RiskLevel
{
    Low,
    Medium,
    High
}

internal sealed class CheckResult
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string DescriptionKey { get; init; } = string.Empty;
    public Severity Severity { get; init; }
    public bool IsFixable { get; init; }
    public IReadOnlyList<string> SuggestedFixIds { get; init; } = [];
    public Dictionary<string, string> Evidence { get; init; } = [];
}

internal sealed record FixMetadata(
    string Id,
    string TitleKey,
    string DescriptionKey,
    RiskLevel RiskLevel,
    bool RequiresAdmin,
    bool IsUndoable,
    IReadOnlyList<string> Restores,
    IReadOnlyList<string> Limitations);

internal sealed record FixResult(
    string FixId,
    bool Success,
    string Message,
    string? SnapshotId,
    bool IsUndoable);

internal sealed record DiagnosisSession(
    string Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string Mode,
    string Language,
    string Region,
    IReadOnlyList<CheckResult> Checks,
    IReadOnlyList<FixResult> Fixes,
    string? BeforeSnapshotId,
    string? AfterSnapshotId,
    string Result);
