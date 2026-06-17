using NetDoctor.App.Models;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetDoctor.App.Services;

internal sealed class NetworkDiagnosticService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(7)
    };

    public async Task<DiagnosticReport> RunAsync(
        string scenario,
        PortProbeRequest portProbe,
        IProgress<DiagnosticProgress>? progress,
        CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.Now;
        var checks = new List<DiagnosticCheck>();
        var region = RegionProfile.Detect();

        foreach (var check in CreateRunningPlaceholders(region))
        {
            progress?.Report(new DiagnosticProgress(check.Id, check));
        }

        var adapter = await ReportAsync(CheckAdapterAsync(region), progress, checks);
        var local = await ReportAsync(CheckReachabilityAsync("local", $"Local internet ({region.CountryName})", region.LocalTargets, true, cancellationToken), progress, checks);
        var international = await ReportAsync(CheckReachabilityAsync("international", "International internet", region.InternationalTargets, false, cancellationToken), progress, checks);
        var dns = await ReportAsync(CheckDnsAsync(portProbe.Host, cancellationToken), progress, checks);
        var quality = await ReportAsync(CheckNetworkQualityAsync(cancellationToken), progress, checks);
        var port = await ReportAsync(CheckPortAsync(portProbe, cancellationToken), progress, checks);
        var vpn = await ReportAsync(CheckVpnAsync(scenario, local.Status, international.Status), progress, checks);
        var proxy = await ReportAsync(CheckProxyAsync(cancellationToken), progress, checks);

        var reportChecks = new[] { adapter, local, international, dns, quality, port, vpn, proxy };
        var fixKey = reportChecks.FirstOrDefault(check => check.SafeFixKey is not null)?.SafeFixKey;
        if (fixKey is null && reportChecks.Any(check => check.Status is CheckStatus.Warning or CheckStatus.Failed))
        {
            fixKey = "quick-refresh";
        }
        var summary = BuildPlainLanguageSummary(region, reportChecks);

        return new DiagnosticReport(
            started,
            DateTimeOffset.Now,
            scenario,
            reportChecks,
            summary,
            fixKey is not null,
            fixKey);
    }

    public NetworkInterface? FindPrimaryAdapter()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(IsUsableAdapter)
            .OrderByDescending(adapter => adapter.GetIPProperties().GatewayAddresses.Count > 0)
            .FirstOrDefault();
    }

    private static async Task<DiagnosticCheck> ReportAsync(
        Task<DiagnosticCheck> task,
        IProgress<DiagnosticProgress>? progress,
        ICollection<DiagnosticCheck> checks)
    {
        var check = await task;
        checks.Add(check);
        progress?.Report(new DiagnosticProgress(check.Id, check));
        return check;
    }

    private static IReadOnlyList<DiagnosticCheck> CreateRunningPlaceholders(RegionProfile region)
    {
        return
        [
            new("adapter", "Network adapter", CheckStatus.Running, "Reading the active adapter, gateway and DNS servers...", []),
            new("local", $"Local internet ({region.CountryName})", CheckStatus.Running, "Testing country-local websites...", []),
            new("international", "International internet", CheckStatus.Running, "Testing international destinations...", []),
            new("dns", "DNS", CheckStatus.Running, "Resolving the selected host...", []),
            new("quality", "Connection quality", CheckStatus.Running, "Measuring latency and packet loss...", []),
            new("port", "Port access", CheckStatus.Running, "Testing TCP connectivity...", []),
            new("vpn", "VPN", CheckStatus.Running, "Checking active VPN-like adapters...", []),
            new("proxy", "Proxy", CheckStatus.Running, "Checking Windows proxy settings...", [])
        ];
    }

    private static Task<DiagnosticCheck> CheckAdapterAsync(RegionProfile region)
    {
        var adapter = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(IsUsableAdapter);
        if (adapter is null)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                "Network adapter",
                CheckStatus.Failed,
                "No active IPv4 network adapter was found.",
                ["Windows does not report an active Ethernet or Wi-Fi adapter with IPv4."]));
        }

        var properties = adapter.GetIPProperties();
        var gateways = properties.GatewayAddresses
            .Where(gateway => gateway.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(gateway => gateway.Address.ToString())
            .Distinct()
            .ToList();
        var dnsServers = properties.DnsAddresses
            .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.ToString())
            .Distinct()
            .ToList();
        var addresses = properties.UnicastAddresses
            .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.Address.ToString())
            .ToList();

        var details = new List<string>
        {
            $"Detected country/region: {region.CountryName} ({region.CountryCode})",
            $"Adapter: {adapter.Name}",
            $"Type: {adapter.NetworkInterfaceType}",
            $"IPv4: {string.Join(", ", addresses.DefaultIfEmpty("none"))}",
            $"Gateway: {string.Join(", ", gateways.DefaultIfEmpty("none"))}",
            $"DNS: {string.Join(", ", dnsServers.DefaultIfEmpty("none"))}"
        };

        if (gateways.Count == 0)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                "Network adapter",
                CheckStatus.Warning,
                "The active adapter has no IPv4 gateway. Internet access may not route correctly.",
                details));
        }

        if (dnsServers.Count == 0)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                "Network adapter",
                CheckStatus.Warning,
                "The adapter is online, but no IPv4 DNS server is configured.",
                details,
                "dns-safe-public"));
        }

        return Task.FromResult(new DiagnosticCheck(
            "adapter",
            "Network adapter",
            CheckStatus.Healthy,
            "The active network adapter has IPv4, gateway and DNS settings.",
            details));
    }

    private static async Task<DiagnosticCheck> CheckReachabilityAsync(
        string id,
        string title,
        IReadOnlyList<Uri> targets,
        bool isLocal,
        CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var successes = 0;

        foreach (var target in targets)
        {
            var result = await ProbeHttpAsync(target, cancellationToken);
            details.Add(result.Detail);
            if (result.Success)
            {
                successes++;
            }
        }

        if (successes == targets.Count)
        {
            return new DiagnosticCheck(id, title, CheckStatus.Healthy, $"{title} is reachable.", details);
        }

        if (successes > 0)
        {
            return new DiagnosticCheck(id, title, CheckStatus.Warning, $"{title} is partially reachable. Some destinations failed.", details);
        }

        var summary = isLocal
            ? "Country-local internet targets did not respond."
            : "International internet targets did not respond.";

        return new DiagnosticCheck(id, title, CheckStatus.Failed, summary, details);
    }

    private static async Task<DiagnosticCheck> CheckDnsAsync(string host, CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var cleanHost = NormalizeHost(host);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(cleanHost, cancellationToken);
            stopwatch.Stop();
            details.Add($"{cleanHost} resolved to {addresses.Length} address(es).");
            details.Add($"DNS response time: {stopwatch.ElapsedMilliseconds} ms.");
            details.Add(GetDnsServersText());

            if (addresses.Length == 0)
            {
                return new DiagnosticCheck("dns", "DNS", CheckStatus.Failed, "DNS answered, but returned no usable address.", details, "dns-safe-public");
            }

            var status = stopwatch.ElapsedMilliseconds > 1500 ? CheckStatus.Warning : CheckStatus.Healthy;
            var summary = status == CheckStatus.Healthy
                ? "DNS is responding normally."
                : "DNS is responding, but it is slow enough to make websites feel broken.";

            return new DiagnosticCheck("dns", "DNS", status, summary, details);
        }
        catch (Exception ex) when (ex is SocketException or TaskCanceledException)
        {
            details.Add($"DNS failed for {cleanHost}: {ex.Message}");
            details.Add(GetDnsServersText());
            return new DiagnosticCheck(
                "dns",
                "DNS",
                CheckStatus.Failed,
                "The network may be connected, but DNS is not resolving names correctly.",
                details,
                "dns-safe-public");
        }
    }

    private static async Task<DiagnosticCheck> CheckNetworkQualityAsync(CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var replies = new List<PingResult>();

        for (var i = 0; i < 8; i++)
        {
            replies.Add(await PingOnceAsync("1.1.1.1", 1200, cancellationToken));
            await Task.Delay(120, cancellationToken);
        }

        var success = replies.Where(reply => reply.Success).ToList();
        var lossPercent = (int)Math.Round((replies.Count - success.Count) * 100.0 / replies.Count);
        var average = success.Count == 0 ? 0 : (int)Math.Round(success.Average(reply => reply.RoundtripTime));
        var max = success.Count == 0 ? 0 : success.Max(reply => reply.RoundtripTime);

        details.Add($"Packet loss: {lossPercent}%");
        details.Add(success.Count > 0 ? $"Average latency: {average} ms. Maximum latency: {max} ms." : "No successful ICMP replies were recorded.");

        if (success.Count == 0)
        {
            return new DiagnosticCheck("quality", "Connection quality", CheckStatus.Warning, "ICMP ping is blocked or the connection is not responding to ping.", details);
        }

        if (lossPercent >= 25 || average > 250)
        {
            return new DiagnosticCheck("quality", "Connection quality", CheckStatus.Failed, "The connection is unstable. Packet loss or high latency was detected.", details);
        }

        if (lossPercent > 0 || average > 100)
        {
            return new DiagnosticCheck("quality", "Connection quality", CheckStatus.Warning, "The connection works, but latency or packet loss may cause interruptions.", details);
        }

        return new DiagnosticCheck("quality", "Connection quality", CheckStatus.Healthy, "Latency and packet loss look healthy.", details);
    }

    private static async Task<DiagnosticCheck> CheckPortAsync(PortProbeRequest request, CancellationToken cancellationToken)
    {
        var cleanHost = NormalizeHost(request.Host);
        var details = new List<string> { $"Host: {cleanHost}", $"Port: {request.Port}" };
        using var tcp = new TcpClient();

        try
        {
            var connectTask = tcp.ConnectAsync(cleanHost, request.Port, cancellationToken).AsTask();
            var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(4), cancellationToken));
            if (completed != connectTask)
            {
                details.Add("TCP connection timed out.");
                return new DiagnosticCheck("port", "Port access", CheckStatus.Failed, $"TCP port {request.Port} on {cleanHost} did not respond.", details);
            }

            await connectTask;
            details.Add("TCP connection succeeded.");
            return new DiagnosticCheck("port", "Port access", CheckStatus.Healthy, $"TCP port {request.Port} on {cleanHost} is reachable.", details);
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            details.Add($"Error: {ex.Message}");
            return new DiagnosticCheck("port", "Port access", CheckStatus.Failed, $"TCP port {request.Port} on {cleanHost} is not reachable.", details);
        }
    }

    private static Task<DiagnosticCheck> CheckVpnAsync(string scenario, CheckStatus localStatus, CheckStatus internationalStatus)
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .ToList();

        var vpnAdapters = adapters
            .Where(adapter => LooksLikeVpn(adapter.Name) || LooksLikeVpn(adapter.Description) || adapter.NetworkInterfaceType == NetworkInterfaceType.Ppp)
            .Select(adapter => $"{adapter.Name} ({adapter.Description})")
            .ToList();

        var details = vpnAdapters.Count == 0
            ? new List<string> { "No active VPN-like adapter was found." }
            : vpnAdapters;

        var scenarioMentionsVpn = scenario.Contains("VPN", StringComparison.OrdinalIgnoreCase);
        if (vpnAdapters.Count == 0 && scenarioMentionsVpn)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", "VPN", CheckStatus.Warning, "The selected problem mentions VPN, but Windows does not show an active VPN adapter.", details));
        }

        if (vpnAdapters.Count > 0 && localStatus == CheckStatus.Healthy && internationalStatus != CheckStatus.Healthy)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", "VPN", CheckStatus.Warning, "VPN is active and international access is failing while local access works. VPN routing or DNS may be involved.", details));
        }

        if (vpnAdapters.Count > 0)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", "VPN", CheckStatus.Healthy, "A VPN-like adapter is active, but no clear VPN fault was detected.", details));
        }

        return Task.FromResult(new DiagnosticCheck("vpn", "VPN", CheckStatus.Healthy, "No active VPN adapter was detected.", details));
    }

    private static async Task<DiagnosticCheck> CheckProxyAsync(CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var winHttp = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var winHttpText = $"{winHttp.Output}\n{winHttp.Error}".Trim();
        details.Add(string.IsNullOrWhiteSpace(winHttpText) ? "WinHTTP proxy: no output." : winHttpText);

        var userProxyEnabled = false;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            userProxyEnabled = Convert.ToInt32(key?.GetValue("ProxyEnable") ?? 0, CultureInfo.InvariantCulture) == 1;
            var proxyServer = Convert.ToString(key?.GetValue("ProxyServer"), CultureInfo.InvariantCulture);
            details.Add(userProxyEnabled ? $"User proxy is enabled: {proxyServer}" : "User proxy is disabled.");
        }
        catch (Exception ex)
        {
            details.Add($"User proxy check failed: {ex.Message}");
        }

        var winHttpProxySet = !winHttpText.Contains("Direct access", StringComparison.OrdinalIgnoreCase)
            && !winHttpText.Contains("no proxy", StringComparison.OrdinalIgnoreCase)
            && winHttpText.Contains("Proxy", StringComparison.OrdinalIgnoreCase);

        if (winHttpProxySet)
        {
            return new DiagnosticCheck("proxy", "Proxy", CheckStatus.Warning, "WinHTTP proxy is configured. If it is stale, some apps may fail to reach the internet.", details, "proxy-reset");
        }

        if (userProxyEnabled)
        {
            return new DiagnosticCheck("proxy", "Proxy", CheckStatus.Warning, "A user proxy is enabled. Browsers and some apps may depend on it.", details);
        }

        return new DiagnosticCheck("proxy", "Proxy", CheckStatus.Healthy, "No risky Windows proxy setting was detected.", details);
    }

    private static async Task<HttpProbeResult> ProbeHttpAsync(Uri uri, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            stopwatch.Stop();
            var ok = (int)response.StatusCode < 500;
            return new HttpProbeResult(ok, $"{uri.Host}: HTTP {(int)response.StatusCode} in {stopwatch.ElapsedMilliseconds} ms.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or SocketException)
        {
            stopwatch.Stop();
            return new HttpProbeResult(false, $"{uri.Host}: failed after {stopwatch.ElapsedMilliseconds} ms ({ex.Message}).");
        }
    }

    private static async Task<PingResult> PingOnceAsync(string target, int timeoutMs, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(target, timeoutMs);
            return reply.Status == IPStatus.Success
                ? new PingResult(true, reply.RoundtripTime, "OK")
                : new PingResult(false, 0, reply.Status.ToString());
        }
        catch (Exception ex) when (ex is PingException or SocketException or OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new PingResult(false, 0, ex.Message);
        }
    }

    private static string NormalizeHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return "www.google.com";
        }

        if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return host.Trim().TrimEnd('/');
    }

    private static string GetDnsServersText()
    {
        var adapter = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(IsUsableAdapter);
        if (adapter is null)
        {
            return "DNS servers: no active adapter.";
        }

        var dnsServers = adapter.GetIPProperties().DnsAddresses
            .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.ToString())
            .Distinct()
            .ToList();

        return dnsServers.Count == 0
            ? $"DNS servers on {adapter.Name}: none."
            : $"DNS servers on {adapter.Name}: {string.Join(", ", dnsServers)}";
    }

    private static bool IsUsableAdapter(NetworkInterface adapter)
    {
        if (adapter.OperationalStatus != OperationalStatus.Up)
        {
            return false;
        }

        if (adapter.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        {
            return false;
        }

        var properties = adapter.GetIPProperties();
        return properties.UnicastAddresses.Any(address => address.Address.AddressFamily == AddressFamily.InterNetwork);
    }

    private static bool LooksLikeVpn(string value)
    {
        var words = new[] { "vpn", "wireguard", "openvpn", "tap", "tun", "tailscale", "zerotier", "nord", "cisco anyconnect", "fortinet", "juniper", "sstp", "ikev2", "l2tp", "proton" };
        return words.Any(word => value.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildPlainLanguageSummary(RegionProfile region, IReadOnlyList<DiagnosticCheck> checks)
    {
        var local = checks.First(check => check.Id == "local");
        var international = checks.First(check => check.Id == "international");

        if (local.Status == CheckStatus.Healthy && international.Status != CheckStatus.Healthy)
        {
            return $"Your connection can reach {region.CountryName}-local internet, but international destinations are failing. This points to upstream routing, filtering, DNS, VPN, or ISP international access rather than a completely offline computer.";
        }

        if (local.Status != CheckStatus.Healthy && international.Status == CheckStatus.Healthy)
        {
            return "International internet works, but the country-local test failed. The issue may be with local services, DNS for local domains, or the selected local targets.";
        }

        if (local.Status != CheckStatus.Healthy && international.Status != CheckStatus.Healthy)
        {
            return "Both local and international internet tests failed. The computer, Wi-Fi, modem, gateway, DNS, proxy, or VPN path needs attention.";
        }

        var failed = checks.FirstOrDefault(check => check.Status == CheckStatus.Failed);
        if (failed is not null)
        {
            return failed.Summary;
        }

        var warning = checks.FirstOrDefault(check => check.Status == CheckStatus.Warning);
        if (warning is not null)
        {
            return warning.Summary;
        }

        return "Local internet, international internet, DNS, connection quality, port access, VPN and proxy checks look healthy.";
    }

    private sealed record HttpProbeResult(bool Success, string Detail);
    private sealed record PingResult(bool Success, long RoundtripTime, string Message);

    private sealed class RegionProfile
    {
        private RegionProfile(string countryCode, string countryName, IReadOnlyList<Uri> localTargets)
        {
            CountryCode = countryCode;
            CountryName = countryName;
            LocalTargets = localTargets;
        }

        public string CountryCode { get; }
        public string CountryName { get; }
        public IReadOnlyList<Uri> LocalTargets { get; }
        public IReadOnlyList<Uri> InternationalTargets { get; } =
        [
            new("https://www.cloudflare.com/cdn-cgi/trace"),
            new("https://www.microsoft.com/"),
            new("https://www.google.com/generate_204")
        ];

        public static RegionProfile Detect()
        {
            var code = "US";
            var name = "United States";
            try
            {
                var region = RegionInfo.CurrentRegion;
                code = region.TwoLetterISORegionName.ToUpperInvariant();
                name = region.EnglishName;
            }
            catch
            {
                // Keep the default.
            }

            var timeZone = TimeZoneInfo.Local.Id;
            if (timeZone.Contains("Iran", StringComparison.OrdinalIgnoreCase) ||
                timeZone.Contains("Tehran", StringComparison.OrdinalIgnoreCase))
            {
                code = "IR";
                name = "Iran";
            }

            return new RegionProfile(code, name, LocalTargetsFor(code));
        }

        private static IReadOnlyList<Uri> LocalTargetsFor(string countryCode)
        {
            var map = new Dictionary<string, string[]>
            {
                ["IR"] = ["https://www.aparat.com/", "https://www.digikala.com/", "https://www.irna.ir/"],
                ["US"] = ["https://www.usa.gov/", "https://www.weather.gov/"],
                ["GB"] = ["https://www.gov.uk/", "https://www.bbc.co.uk/"],
                ["DE"] = ["https://www.bundesregierung.de/", "https://www.tagesschau.de/"],
                ["FR"] = ["https://www.service-public.fr/", "https://www.france24.com/fr/"],
                ["TR"] = ["https://www.turkiye.gov.tr/", "https://www.trt.net.tr/"],
                ["AE"] = ["https://u.ae/", "https://www.thenationalnews.com/"],
                ["CA"] = ["https://www.canada.ca/", "https://www.cbc.ca/"],
                ["AU"] = ["https://www.australia.gov.au/", "https://www.abc.net.au/"],
                ["IN"] = ["https://www.india.gov.in/", "https://www.nic.in/"]
            };

            if (!map.TryGetValue(countryCode, out var targets))
            {
                targets = ["https://www.wikipedia.org/", "https://www.microsoft.com/"];
            }

            return targets.Select(target => new Uri(target)).ToList();
        }
    }
}
