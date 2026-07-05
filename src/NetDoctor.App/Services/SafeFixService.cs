using NetDoctor.App.Localization;
using NetDoctor.App.Models;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetDoctor.App.Services;

internal sealed class SafeFixService
{
    private readonly NetworkDiagnosticService diagnosticService = new();
    private readonly string stateDirectory;
    private readonly string snapshotPath;

    public SafeFixService()
    {
        stateDirectory = PathService.SnapshotsDirectory;
        snapshotPath = Path.Combine(stateDirectory, "last-undo.json");
    }

    public bool HasUndoSnapshot => File.Exists(snapshotPath);

    public static string Describe(string? fixKey)
    {
        return fixKey switch
        {
            "dns-safe-public" => L.FixDescDnsPublic,
            "proxy-reset" => L.FixDescProxyReset,
            "quick-refresh" => L.FixDescQuickRefresh,
            "deep-repair" => L.FixDescDeepRepair,
            _ => L.FixDescNone
        };
    }

    public async Task<string> ApplyAsync(string fixKey, CancellationToken cancellationToken)
    {
        await new NetworkSnapshotService().CaptureAsync(fixKey, cancellationToken);

        return fixKey switch
        {
            "dns-safe-public" => await ApplyDnsPresetAsync(DnsPresets.Recommended, cancellationToken),
            "proxy-reset" => await ApplyProxyResetAsync("proxy-reset", cancellationToken),
            "quick-refresh" => await ApplyQuickRefreshAsync(cancellationToken),
            "deep-repair" => await ApplyDeepRepairAsync(cancellationToken),
            _ => L.FixDescNone
        };
    }

    public async Task<string> UndoAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(snapshotPath))
        {
            return L.UndoNoSnapshot;
        }

        var json = await File.ReadAllTextAsync(snapshotPath, cancellationToken);
        var snapshot = JsonSerializer.Deserialize<SafeFixSnapshot>(json, JsonOptions());
        if (snapshot is null)
        {
            return L.UndoInvalid;
        }

        var outcome = snapshot.FixKey switch
        {
            "dns-safe-public" => await UndoDnsAsync(snapshot, cancellationToken),
            "proxy-reset" => await UndoProxyAsync(snapshot, cancellationToken),
            _ => new FixOutcome(false, L.UndoUnsupported)
        };

        if (outcome.Success)
        {
            File.Delete(snapshotPath);
        }

        return outcome.Message;
    }

    /// <summary>
    /// Switches the primary adapter to the chosen DNS preset (or back to DHCP) after saving a
    /// snapshot for Undo. All elevated commands run in one batch, so the user sees a single
    /// UAC prompt.
    /// </summary>
    public async Task<string> ApplyDnsPresetAsync(DnsPreset preset, CancellationToken cancellationToken)
    {
        var adapter = diagnosticService.FindPrimaryAdapter();
        if (adapter is null)
        {
            return L.FixNoAdapter;
        }

        Directory.CreateDirectory(stateDirectory);
        var snapshot = new SafeFixSnapshot(DateTimeOffset.Now, "dns-safe-public", await ReadDnsSnapshotAsync(adapter, cancellationToken));
        await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions()), cancellationToken);

        var adapterName = adapter.Name;
        List<string> commands;

        if (preset.IsAutomatic)
        {
            commands =
            [
                $"netsh interface ipv4 set dnsservers name=\"{adapterName}\" source=dhcp || exit /b 1",
                "ipconfig /flushdns"
            ];
        }
        else
        {
            commands =
            [
                $"netsh interface ipv4 set dnsservers name=\"{adapterName}\" source=static address={preset.Primary} validate=no || exit /b 1"
            ];

            if (!string.IsNullOrWhiteSpace(preset.Secondary))
            {
                commands.Add($"netsh interface ipv4 add dnsservers name=\"{adapterName}\" address={preset.Secondary} index=2 validate=no");
            }

            commands.Add("ipconfig /flushdns");
        }

        var exitCode = await ProcessRunner.RunElevatedScriptAsync(commands, cancellationToken);
        if (exitCode != 0)
        {
            return L.FixDnsRejected;
        }

        if (preset.IsAutomatic)
        {
            return L.FixDnsAppliedAuto(adapterName);
        }

        var description = string.IsNullOrWhiteSpace(preset.Secondary)
            ? $"{preset.DisplayName} ({preset.Primary})"
            : $"{preset.DisplayName} ({preset.Primary}, {preset.Secondary})";
        return L.FixDnsAppliedPreset(adapterName, description);
    }

    private static async Task<string> ApplyDeepRepairAsync(CancellationToken cancellationToken)
    {
        // Full network stack reset. Not reversible by Undo, so it is gated behind a strong
        // warning in the UI and never auto-recommended.
        var commands = new List<string>
        {
            "netsh winsock reset || exit /b 1",
            "netsh int ip reset",
            "ipconfig /flushdns"
        };

        var exitCode = await ProcessRunner.RunElevatedScriptAsync(commands, cancellationToken);
        return exitCode == 0 ? L.DeepRepairApplied : L.DeepRepairRejected;
    }

    private async Task<string> ApplyProxyResetAsync(string fixKey, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(stateDirectory);
        var current = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var snapshot = new SafeFixSnapshot(
            DateTimeOffset.Now,
            fixKey,
            null,
            new ProxySnapshot($"{current.Output}\n{current.Error}".Trim()));
        await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions()), cancellationToken);

        var exitCode = await ProcessRunner.RunElevatedScriptAsync(
            ["netsh winhttp reset proxy || exit /b 1", "ipconfig /flushdns"],
            cancellationToken);
        if (exitCode != 0)
        {
            return L.FixProxyRejected;
        }

        return L.FixProxyApplied;
    }

    private static async Task<string> ApplyQuickRefreshAsync(CancellationToken cancellationToken)
    {
        var steps = new List<string>();

        var flushDns = await ProcessRunner.RunAsync("ipconfig.exe", "/flushdns", cancellationToken: cancellationToken);
        steps.Add(flushDns.ExitCode == 0 ? L.RefreshDnsFlushed : L.RefreshDnsFlushFailed(flushDns.Error.Trim()));

        var registerDns = await ProcessRunner.RunAsync("ipconfig.exe", "/registerdns", cancellationToken: cancellationToken);
        steps.Add(registerDns.ExitCode == 0 ? L.RefreshRegisterOk : L.RefreshRegisterFail);

        var winHttp = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var proxyText = $"{winHttp.Output}\n{winHttp.Error}".Trim();
        steps.Add(proxyText.Contains("Direct access", StringComparison.OrdinalIgnoreCase)
            ? L.RefreshProxyDirect
            : L.RefreshProxyConfigured);

        return string.Join(Environment.NewLine, steps);
    }

    private static async Task<FixOutcome> UndoDnsAsync(SafeFixSnapshot snapshot, CancellationToken cancellationToken)
    {
        if (snapshot.DnsSnapshot is null)
        {
            return new FixOutcome(false, L.UndoDnsMissing);
        }

        var dns = snapshot.DnsSnapshot;
        List<string> commands;

        if (dns.WasDhcp || dns.Ipv4Servers.Count == 0)
        {
            commands =
            [
                $"netsh interface ipv4 set dnsservers name=\"{dns.AdapterName}\" source=dhcp || exit /b 1",
                "ipconfig /flushdns"
            ];

            var dhcpExit = await ProcessRunner.RunElevatedScriptAsync(commands, cancellationToken);
            return dhcpExit == 0
                ? new FixOutcome(true, L.UndoDnsRestored)
                : new FixOutcome(false, L.UndoDhcpFailed);
        }

        commands =
        [
            $"netsh interface ipv4 set dnsservers name=\"{dns.AdapterName}\" source=static address={dns.Ipv4Servers[0]} validate=no || exit /b 1"
        ];

        for (var index = 1; index < dns.Ipv4Servers.Count; index++)
        {
            commands.Add($"netsh interface ipv4 add dnsservers name=\"{dns.AdapterName}\" address={dns.Ipv4Servers[index]} index={index + 1} validate=no");
        }

        commands.Add("ipconfig /flushdns");

        var exitCode = await ProcessRunner.RunElevatedScriptAsync(commands, cancellationToken);
        return exitCode == 0
            ? new FixOutcome(true, L.UndoDnsRestored)
            : new FixOutcome(false, L.UndoDnsServerFailed);
    }

    private static async Task<FixOutcome> UndoProxyAsync(SafeFixSnapshot snapshot, CancellationToken cancellationToken)
    {
        var output = snapshot.ProxySnapshot?.WinHttpProxyOutput ?? string.Empty;
        if (string.IsNullOrWhiteSpace(output) || output.Contains("Direct access", StringComparison.OrdinalIgnoreCase))
        {
            var reset = await ProcessRunner.RunElevatedScriptAsync(["netsh winhttp reset proxy || exit /b 1"], cancellationToken);
            return reset == 0
                ? new FixOutcome(true, L.UndoProxyDirectRestored)
                : new FixOutcome(false, L.UndoProxyDirectFailed);
        }

        var proxy = Regex.Match(output, @"Proxy Server\(s\)\s*:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        var bypass = Regex.Match(output, @"Bypass List\s*:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

        if (string.IsNullOrWhiteSpace(proxy))
        {
            return new FixOutcome(false, L.UndoProxyParseFailed);
        }

        var command = string.IsNullOrWhiteSpace(bypass)
            ? $"netsh winhttp set proxy proxy-server=\"{proxy}\" || exit /b 1"
            : $"netsh winhttp set proxy proxy-server=\"{proxy}\" bypass-list=\"{bypass}\" || exit /b 1";
        var restore = await ProcessRunner.RunElevatedScriptAsync([command], cancellationToken);
        return restore == 0
            ? new FixOutcome(true, L.UndoProxyRestored)
            : new FixOutcome(false, L.UndoProxyFailed);
    }

    private sealed record FixOutcome(bool Success, string Message);

    private static async Task<AdapterDnsSnapshot> ReadDnsSnapshotAsync(NetworkInterface adapter, CancellationToken cancellationToken)
    {
        var result = await ProcessRunner.RunAsync(
            "netsh.exe",
            $"interface ipv4 show dnsservers name=\"{adapter.Name}\"",
            cancellationToken: cancellationToken);

        var output = $"{result.Output}\n{result.Error}";
        var wasDhcp = output.Contains("DHCP", StringComparison.OrdinalIgnoreCase);
        var servers = Regex.Matches(output, @"\b(?:\d{1,3}\.){3}\d{1,3}\b")
            .Select(match => match.Value)
            .Distinct()
            .ToList();

        if (servers.Count == 0)
        {
            servers = adapter.GetIPProperties().DnsAddresses
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                .Select(address => address.ToString())
                .Distinct()
                .ToList();
        }

        return new AdapterDnsSnapshot(adapter.Name, wasDhcp, servers);
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions { WriteIndented = true };
    }
}
