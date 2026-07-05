using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetDoctor.App.Core;
using NetDoctor.App.Models;

namespace NetDoctor.App.Services;

internal static class FixRegistry
{
    private static readonly IReadOnlyDictionary<string, FixMetadata> Items = new Dictionary<string, FixMetadata>(StringComparer.OrdinalIgnoreCase)
    {
        ["dns-safe-public"] = new(
            "dns-safe-public",
            "Fix.DnsSafePublic.Title",
            "Fix.DnsSafePublic.Description",
            RiskLevel.Low,
            RequiresAdmin: true,
            IsUndoable: true,
            Restores: ["DNS static/DHCP state", "DNS server list"],
            Limitations: ["Does not restore ISP routing or external DNS policy"]),
        ["proxy-reset"] = new(
            "proxy-reset",
            "Fix.ProxyReset.Title",
            "Fix.ProxyReset.Description",
            RiskLevel.Low,
            RequiresAdmin: true,
            IsUndoable: true,
            Restores: ["WinHTTP proxy"],
            Limitations: ["User proxy is audited but only restored when a fix explicitly changes it"]),
        ["quick-refresh"] = new(
            "quick-refresh",
            "Fix.QuickRefresh.Title",
            "Fix.QuickRefresh.Description",
            RiskLevel.Low,
            RequiresAdmin: false,
            IsUndoable: false,
            Restores: [],
            Limitations: ["Flush/register DNS has no meaningful rollback"]),
        ["deep-repair"] = new(
            "deep-repair",
            "Fix.DeepRepair.Title",
            "Fix.DeepRepair.Description",
            RiskLevel.High,
            RequiresAdmin: true,
            IsUndoable: false,
            Restores: [],
            Limitations: ["Winsock reset", "TCP/IP reset", "Network stack reset", "Windows may rebuild system-level state", "Manual restart may be required"])
    };

    public static IReadOnlyCollection<FixMetadata> All => Items.Values.ToList();

    public static FixMetadata? Find(string? id)
    {
        return id is not null && Items.TryGetValue(id, out var metadata) ? metadata : null;
    }

    public static bool RequiresAdmin(string? id) => Find(id)?.RequiresAdmin == true;

    public static bool IsUndoable(string? id) => Find(id)?.IsUndoable == true;
}

internal sealed record NetworkSnapshot(
    string Id,
    DateTimeOffset Timestamp,
    IReadOnlyList<string> AdaptersSummary,
    IReadOnlyList<string> IpConfig,
    IReadOnlyList<string> DnsServers,
    IReadOnlyList<string> Gateway,
    string WinHttpProxy,
    string UserProxy,
    IReadOnlyList<string> RouteSummary,
    string HostsFileHash,
    string TriggerFixId);

internal sealed class NetworkSnapshotService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<NetworkSnapshot> CaptureAsync(string triggerFixId, CancellationToken cancellationToken)
    {
        var id = $"snapshot-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        var adapters = NetworkInterface.GetAllNetworkInterfaces()
            .Select(adapter => $"{adapter.Name} | {adapter.NetworkInterfaceType} | {adapter.OperationalStatus} | {adapter.Description}")
            .ToList();

        var ipConfig = await ReadLinesAsync("ipconfig.exe", "/all", cancellationToken);
        var routes = await ReadLinesAsync("route.exe", "print", cancellationToken);
        var winHttp = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var userProxy = ReadUserProxy();

        var dnsServers = NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(adapter => adapter.GetIPProperties().DnsAddresses.Select(address => $"{adapter.Name}: {address}"))
            .Distinct()
            .ToList();
        var gateways = NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(adapter => adapter.GetIPProperties().GatewayAddresses.Select(address => $"{adapter.Name}: {address.Address}"))
            .Distinct()
            .ToList();

        var snapshot = new NetworkSnapshot(
            id,
            DateTimeOffset.Now,
            adapters,
            ipConfig,
            dnsServers,
            gateways,
            $"{winHttp.Output}{Environment.NewLine}{winHttp.Error}".Trim(),
            userProxy,
            routes.Take(250).ToList(),
            ComputeHostsHash(),
            triggerFixId);

        Directory.CreateDirectory(PathService.SnapshotsDirectory);
        var path = Path.Combine(PathService.SnapshotsDirectory, $"{id}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(snapshot, JsonOptions), cancellationToken);
        return snapshot;
    }

    private static async Task<IReadOnlyList<string>> ReadLinesAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        var result = await ProcessRunner.RunAsync(fileName, arguments, cancellationToken: cancellationToken);
        return $"{result.Output}{Environment.NewLine}{result.Error}"
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Take(400)
            .ToList();
    }

    private static string ReadUserProxy()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            var enabled = key?.GetValue("ProxyEnable")?.ToString() ?? "0";
            var server = key?.GetValue("ProxyServer")?.ToString() ?? string.Empty;
            var overrideValue = key?.GetValue("ProxyOverride")?.ToString() ?? string.Empty;
            return $"enabled={enabled}; server={server}; override={overrideValue}";
        }
        catch (Exception ex)
        {
            return $"unavailable: {ex.Message}";
        }
    }

    private static string ComputeHostsHash()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
            if (!File.Exists(path))
            {
                return "missing";
            }

            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            return $"unavailable: {ex.Message}";
        }
    }
}

internal static class ElevationService
{
    public static string ShieldGlyph => "\uE83D";

    public static bool RequiresAdmin(string fixId) => FixRegistry.RequiresAdmin(fixId);

    public static Task<int> RunAdminCommandsAsync(IReadOnlyList<string> commands, CancellationToken cancellationToken)
    {
        return ProcessRunner.RunElevatedScriptAsync(commands, cancellationToken);
    }

    public static void MarkShield(Button button, string fixId)
    {
        button.Text = RequiresAdmin(fixId) ? $"{ShieldGlyph} {button.Text}" : button.Text;
    }
}
