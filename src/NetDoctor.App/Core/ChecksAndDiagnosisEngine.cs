using NetDoctor.App.Localization;
using NetDoctor.App.Models;
using NetDoctor.App.Services;

namespace NetDoctor.App.Core;

internal interface INetworkCheck
{
    string Id { get; }
    string TitleKey { get; }
    string DescriptionKey { get; }
}

internal sealed class DnsCheck : INetworkCheck
{
    public string Id => "dns";
    public string TitleKey => "Check.Dns.Title";
    public string DescriptionKey => "Check.Dns.Description";
}

internal sealed class GatewayCheck : INetworkCheck
{
    public string Id => "gateway";
    public string TitleKey => "Check.Gateway.Title";
    public string DescriptionKey => "Check.Gateway.Description";
}

internal sealed class InternetCheck : INetworkCheck
{
    public string Id => "internet";
    public string TitleKey => "Check.Internet.Title";
    public string DescriptionKey => "Check.Internet.Description";
}

internal sealed class AdapterCheck : INetworkCheck
{
    public string Id => "adapter";
    public string TitleKey => "Check.Adapter.Title";
    public string DescriptionKey => "Check.Adapter.Description";
}

internal sealed class ProxyCheck : INetworkCheck
{
    public string Id => "proxy";
    public string TitleKey => "Check.Proxy.Title";
    public string DescriptionKey => "Check.Proxy.Description";
}

internal sealed class PacketLossCheck : INetworkCheck
{
    public string Id => "packet-loss";
    public string TitleKey => "Check.PacketLoss.Title";
    public string DescriptionKey => "Check.PacketLoss.Description";
}

internal sealed class VpnCheck : INetworkCheck
{
    public string Id => "vpn";
    public string TitleKey => "Check.Vpn.Title";
    public string DescriptionKey => "Check.Vpn.Description";
}

internal sealed class HostsFileCheck : INetworkCheck
{
    public string Id => "hosts";
    public string TitleKey => "Check.HostsFile.Title";
    public string DescriptionKey => "Check.HostsFile.Description";
}

internal sealed class PortCheck : INetworkCheck
{
    public string Id => "port";
    public string TitleKey => "Check.Port.Title";
    public string DescriptionKey => "Check.Port.Description";
}

internal static class CoreCheckCatalog
{
    public static IReadOnlyList<INetworkCheck> RequiredChecks { get; } =
    [
        new DnsCheck(),
        new GatewayCheck(),
        new InternetCheck(),
        new AdapterCheck(),
        new ProxyCheck(),
        new PacketLossCheck(),
        new VpnCheck(),
        new HostsFileCheck(),
        new PortCheck()
    ];
}

internal sealed class DiagnosisEngine
{
    private readonly NetworkDiagnosticService _legacyService = new();

    public async Task<(DiagnosticReport LegacyReport, DiagnosisSession Session)> RunAsync(
        string scenario,
        PortProbeRequest portProbe,
        IProgress<DiagnosticProgress>? progress,
        CancellationToken cancellationToken)
    {
        var report = await _legacyService.RunAsync(scenario, portProbe, progress, cancellationToken);
        var checks = MapChecks(report);

        return (report, new DiagnosisSession(
            Guid.NewGuid().ToString("N"),
            report.StartedAt,
            report.FinishedAt,
            scenario,
            AppConfig.CultureName,
            AppConfig.Region.ToString(),
            checks,
            [],
            null,
            null,
            report.PlainLanguageSummary));
    }

    public IReadOnlyList<CheckResult> MapChecks(DiagnosticReport report)
    {
        var mapped = report.Checks.Select(Map).ToList();
        var hasLocal = report.Checks.FirstOrDefault(check => check.Id == "local");
        var hasInternational = report.Checks.FirstOrDefault(check => check.Id == "international");
        if (hasLocal is not null || hasInternational is not null)
        {
            mapped.Add(MapInternet(hasLocal, hasInternational));
        }

        var adapter = report.Checks.FirstOrDefault(check => check.Id == "adapter");
        if (adapter is not null)
        {
            mapped.Add(MapGateway(adapter));
        }

        var quality = report.Checks.FirstOrDefault(check => check.Id == "quality");
        if (quality is not null)
        {
            mapped.Add(MapPacketLoss(quality));
        }

        return mapped;
    }

    private static CheckResult Map(DiagnosticCheck check)
    {
        var checkId = check.Id == "quality" ? "packet-loss" : check.Id;
        return new CheckResult
        {
            Id = checkId,
            TitleKey = checkId switch
            {
                "dns" => "Check.Dns.Title",
                "adapter" => "Check.Adapter.Title",
                "proxy" => "Check.Proxy.Title",
                "vpn" => "Check.Vpn.Title",
                "hosts" => "Check.HostsFile.Title",
                "port" => "Check.Port.Title",
                "packet-loss" => "Check.PacketLoss.Title",
                _ => "Check.Internet.Title"
            },
            DescriptionKey = $"Check.{checkId}.Description",
            Severity = MapSeverity(check.Status),
            IsFixable = check.SafeFixKey is not null,
            SuggestedFixIds = check.SafeFixKey is null ? [] : [check.SafeFixKey],
            Evidence = BuildEvidence(check)
        };
    }

    private static CheckResult MapInternet(DiagnosticCheck? local, DiagnosticCheck? international)
    {
        var severity = new[] { local?.Status, international?.Status }
            .Select(status => status.HasValue ? MapSeverity(status.Value) : Severity.Info)
            .OrderByDescending(s => (int)s)
            .First();

        return new CheckResult
        {
            Id = "internet",
            TitleKey = "Check.Internet.Title",
            DescriptionKey = "Check.Internet.Description",
            Severity = severity,
            IsFixable = false,
            SuggestedFixIds = [],
            Evidence = new Dictionary<string, string>
            {
                ["local"] = local?.Summary ?? "not-run",
                ["international"] = international?.Summary ?? "not-run"
            }
        };
    }

    private static CheckResult MapGateway(DiagnosticCheck adapter)
    {
        var noGateway = adapter.Summary.Contains("gateway", StringComparison.OrdinalIgnoreCase);
        return new CheckResult
        {
            Id = "gateway",
            TitleKey = "Check.Gateway.Title",
            DescriptionKey = "Check.Gateway.Description",
            Severity = noGateway ? Severity.Warning : MapSeverity(adapter.Status),
            IsFixable = false,
            SuggestedFixIds = [],
            Evidence = BuildEvidence(adapter)
        };
    }

    private static CheckResult MapPacketLoss(DiagnosticCheck quality)
    {
        return new CheckResult
        {
            Id = "packet-loss",
            TitleKey = "Check.PacketLoss.Title",
            DescriptionKey = "Check.PacketLoss.Description",
            Severity = MapSeverity(quality.Status),
            IsFixable = false,
            SuggestedFixIds = [],
            Evidence = BuildEvidence(quality)
        };
    }

    private static Dictionary<string, string> BuildEvidence(DiagnosticCheck check)
    {
        var evidence = new Dictionary<string, string>
        {
            ["title"] = check.Title,
            ["summary"] = check.Summary,
            ["status"] = check.Status.ToString()
        };

        for (var i = 0; i < check.Details.Count; i++)
        {
            evidence[$"detail.{i + 1}"] = check.Details[i];
        }

        return evidence;
    }

    private static Severity MapSeverity(CheckStatus status) => status switch
    {
        CheckStatus.Healthy => Severity.Healthy,
        CheckStatus.Warning => Severity.Warning,
        CheckStatus.Failed => Severity.Error,
        CheckStatus.Running => Severity.Info,
        _ => Severity.Info
    };
}
