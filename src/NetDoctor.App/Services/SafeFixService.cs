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
        stateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetDoctor");
        snapshotPath = Path.Combine(stateDirectory, "safe-fix-snapshot.json");
    }

    public bool HasUndoSnapshot => File.Exists(snapshotPath);

    public static string Describe(string? fixKey)
    {
        return fixKey switch
        {
            "dns-safe-public" => "Save current DNS, switch the active adapter to 1.1.1.1 and 8.8.8.8, then flush DNS.",
            "proxy-reset" => "Save the current WinHTTP proxy setting, reset WinHTTP proxy, then flush DNS.",
            "quick-refresh" => "Flush DNS cache and refresh lightweight Windows network name-resolution state, then run diagnosis again.",
            _ => "No safe automatic repair is available for this result."
        };
    }

    public async Task<string> ApplyAsync(string fixKey, CancellationToken cancellationToken)
    {
        return fixKey switch
        {
            "dns-safe-public" => await ApplyDnsFixAsync(fixKey, cancellationToken),
            "proxy-reset" => await ApplyProxyResetAsync(fixKey, cancellationToken),
            "quick-refresh" => await ApplyQuickRefreshAsync(cancellationToken),
            _ => "No safe automatic repair is available for this result."
        };
    }

    public async Task<string> UndoAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(snapshotPath))
        {
            return "No saved repair snapshot was found.";
        }

        var json = await File.ReadAllTextAsync(snapshotPath, cancellationToken);
        var snapshot = JsonSerializer.Deserialize<SafeFixSnapshot>(json, JsonOptions());
        if (snapshot is null)
        {
            return "The repair snapshot is not valid.";
        }

        var message = snapshot.FixKey switch
        {
            "dns-safe-public" => await UndoDnsAsync(snapshot, cancellationToken),
            "proxy-reset" => await UndoProxyAsync(snapshot, cancellationToken),
            _ => "This snapshot cannot be restored by this version."
        };

        if (!message.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
            !message.Contains("could not", StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(snapshotPath);
        }

        return message;
    }

    private async Task<string> ApplyDnsFixAsync(string fixKey, CancellationToken cancellationToken)
    {
        var adapter = diagnosticService.FindPrimaryAdapter();
        if (adapter is null)
        {
            return "No active network adapter was found. No changes were made.";
        }

        Directory.CreateDirectory(stateDirectory);
        var snapshot = new SafeFixSnapshot(DateTimeOffset.Now, fixKey, await ReadDnsSnapshotAsync(adapter, cancellationToken));
        await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions()), cancellationToken);

        var adapterName = adapter.Name;
        var setResult = await ProcessRunner.RunAsync(
            "netsh.exe",
            $"interface ipv4 set dnsservers name=\"{adapterName}\" source=static address=1.1.1.1 validate=no",
            elevated: true,
            cancellationToken);

        if (setResult.ExitCode != 0)
        {
            return "DNS was not changed. Administrator permission may have been cancelled or Windows rejected the change.";
        }

        await ProcessRunner.RunAsync(
            "netsh.exe",
            $"interface ipv4 add dnsservers name=\"{adapterName}\" address=8.8.8.8 index=2 validate=no",
            elevated: true,
            cancellationToken);

        await ProcessRunner.RunAsync("ipconfig.exe", "/flushdns", cancellationToken: cancellationToken);
        return $"DNS on {adapterName} was changed to 1.1.1.1 and 8.8.8.8. The previous setting was saved for Undo.";
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

        var reset = await ProcessRunner.RunAsync("netsh.exe", "winhttp reset proxy", elevated: true, cancellationToken);
        if (reset.ExitCode != 0)
        {
            return "WinHTTP proxy was not reset. Administrator permission may have been cancelled or Windows rejected the change.";
        }

        await ProcessRunner.RunAsync("ipconfig.exe", "/flushdns", cancellationToken: cancellationToken);
        return "WinHTTP proxy was reset and DNS cache was flushed. The previous proxy output was saved for Undo.";
    }

    private static async Task<string> ApplyQuickRefreshAsync(CancellationToken cancellationToken)
    {
        var steps = new List<string>();

        var flushDns = await ProcessRunner.RunAsync("ipconfig.exe", "/flushdns", cancellationToken: cancellationToken);
        steps.Add(flushDns.ExitCode == 0 ? "DNS cache flushed." : $"DNS cache flush failed: {flushDns.Error.Trim()}");

        var registerDns = await ProcessRunner.RunAsync("ipconfig.exe", "/registerdns", cancellationToken: cancellationToken);
        steps.Add(registerDns.ExitCode == 0 ? "DNS registration refresh requested." : "DNS registration refresh could not be requested.");

        var winHttp = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var proxyText = $"{winHttp.Output}\n{winHttp.Error}".Trim();
        steps.Add(proxyText.Contains("Direct access", StringComparison.OrdinalIgnoreCase)
            ? "WinHTTP proxy already uses direct access."
            : "WinHTTP proxy is configured; Net Doctor did not change it without a proxy-specific warning.");

        return string.Join(Environment.NewLine, steps);
    }

    private static async Task<string> UndoDnsAsync(SafeFixSnapshot snapshot, CancellationToken cancellationToken)
    {
        if (snapshot.DnsSnapshot is null)
        {
            return "The DNS repair snapshot is missing.";
        }

        var dns = snapshot.DnsSnapshot;
        if (dns.WasDhcp || dns.Ipv4Servers.Count == 0)
        {
            var dhcpResult = await ProcessRunner.RunAsync(
                "netsh.exe",
                $"interface ipv4 set dnsservers name=\"{dns.AdapterName}\" source=dhcp",
                elevated: true,
                cancellationToken);

            if (dhcpResult.ExitCode != 0)
            {
                return "Restoring DNS to DHCP failed. Please check Administrator permission.";
            }
        }
        else
        {
            var first = dns.Ipv4Servers[0];
            var setResult = await ProcessRunner.RunAsync(
                "netsh.exe",
                $"interface ipv4 set dnsservers name=\"{dns.AdapterName}\" source=static address={first} validate=no",
                elevated: true,
                cancellationToken);

            if (setResult.ExitCode != 0)
            {
                return "Restoring the previous DNS server failed. Please check Administrator permission.";
            }

            for (var index = 1; index < dns.Ipv4Servers.Count; index++)
            {
                await ProcessRunner.RunAsync(
                    "netsh.exe",
                    $"interface ipv4 add dnsservers name=\"{dns.AdapterName}\" address={dns.Ipv4Servers[index]} index={index + 1} validate=no",
                    elevated: true,
                    cancellationToken);
            }
        }

        await ProcessRunner.RunAsync("ipconfig.exe", "/flushdns", cancellationToken: cancellationToken);
        return "The previous DNS setting was restored.";
    }

    private static async Task<string> UndoProxyAsync(SafeFixSnapshot snapshot, CancellationToken cancellationToken)
    {
        var output = snapshot.ProxySnapshot?.WinHttpProxyOutput ?? string.Empty;
        if (string.IsNullOrWhiteSpace(output) || output.Contains("Direct access", StringComparison.OrdinalIgnoreCase))
        {
            var reset = await ProcessRunner.RunAsync("netsh.exe", "winhttp reset proxy", elevated: true, cancellationToken);
            return reset.ExitCode == 0 ? "WinHTTP proxy was restored to direct access." : "Restoring WinHTTP direct access failed.";
        }

        var proxy = Regex.Match(output, @"Proxy Server\(s\)\s*:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        var bypass = Regex.Match(output, @"Bypass List\s*:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

        if (string.IsNullOrWhiteSpace(proxy))
        {
            return "The previous proxy value could not be parsed. The saved snapshot remains in AppData.";
        }

        var args = string.IsNullOrWhiteSpace(bypass)
            ? $"winhttp set proxy proxy-server=\"{proxy}\""
            : $"winhttp set proxy proxy-server=\"{proxy}\" bypass-list=\"{bypass}\"";
        var restore = await ProcessRunner.RunAsync("netsh.exe", args, elevated: true, cancellationToken);
        return restore.ExitCode == 0 ? "The previous WinHTTP proxy setting was restored." : "Restoring WinHTTP proxy failed.";
    }

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
