using NetDoctor.App.Localization;
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
        var local = await ReportAsync(CheckReachabilityAsync("local", L.LocalWithCountry(region.CountryName), region.LocalTargets, true, cancellationToken), progress, checks);
        var international = await ReportAsync(CheckReachabilityAsync("international", L.CardInternational, region.InternationalTargets, false, cancellationToken), progress, checks);
        var dns = await ReportAsync(CheckDnsAsync(portProbe.Host, cancellationToken), progress, checks);
        var quality = await ReportAsync(CheckNetworkQualityAsync(cancellationToken), progress, checks);
        var port = await ReportAsync(CheckPortAsync(portProbe, cancellationToken), progress, checks);
        var vpn = await ReportAsync(CheckVpnAsync(scenario, local.Status, international.Status, cancellationToken), progress, checks);
        var proxy = await ReportAsync(CheckProxyAsync(cancellationToken), progress, checks);
        var hosts = await ReportAsync(CheckHostsFileAsync(portProbe.Host, cancellationToken), progress, checks);

        var reportChecks = new[] { adapter, local, international, dns, quality, port, vpn, proxy, hosts };
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
            new("adapter", L.CardAdapter, CheckStatus.Running, L.PhAdapter, []),
            new("local", L.LocalWithCountry(region.CountryName), CheckStatus.Running, L.PhLocal, []),
            new("international", L.CardInternational, CheckStatus.Running, L.PhInternational, []),
            new("dns", L.CardDns, CheckStatus.Running, L.PhDns, []),
            new("quality", L.CardQuality, CheckStatus.Running, L.PhQuality, []),
            new("port", L.CardPort, CheckStatus.Running, L.PhPort, []),
            new("vpn", L.CardVpn, CheckStatus.Running, L.PhVpn, []),
            new("proxy", L.CardProxy, CheckStatus.Running, L.PhProxy, []),
            new("hosts", L.CardHosts, CheckStatus.Running, L.PhHosts, [])
        ];
    }

    private static Task<DiagnosticCheck> CheckAdapterAsync(RegionProfile region)
    {
        var adapter = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(IsUsableAdapter);
        if (adapter is null)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                L.CardAdapter,
                CheckStatus.Failed,
                L.AdapterNoneSummary,
                [L.AdapterNoneDetail]));
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
            $"{L.LblRegion}: {region.CountryName} ({region.CountryCode})",
            $"{L.LblAdapter}: {adapter.Name}",
            $"{L.LblType}: {adapter.NetworkInterfaceType}",
            $"IPv4: {string.Join(", ", addresses.DefaultIfEmpty(L.ValNone))}",
            $"{L.LblGateway}: {string.Join(", ", gateways.DefaultIfEmpty(L.ValNone))}",
            $"DNS: {string.Join(", ", dnsServers.DefaultIfEmpty(L.ValNone))}"
        };

        if (gateways.Count == 0)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                L.CardAdapter,
                CheckStatus.Warning,
                L.AdapterNoGateway,
                details));
        }

        if (dnsServers.Count == 0)
        {
            return Task.FromResult(new DiagnosticCheck(
                "adapter",
                L.CardAdapter,
                CheckStatus.Warning,
                L.AdapterNoDns,
                details,
                "dns-safe-public"));
        }

        return Task.FromResult(new DiagnosticCheck(
            "adapter",
            L.CardAdapter,
            CheckStatus.Healthy,
            L.AdapterHealthy,
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
            return new DiagnosticCheck(id, title, CheckStatus.Healthy, L.Reachable(title), details);
        }

        if (successes > 0)
        {
            return new DiagnosticCheck(id, title, CheckStatus.Warning, L.PartiallyReachable(title), details);
        }

        var summary = isLocal ? L.LocalUnreachable : L.InternationalUnreachable;
        return new DiagnosticCheck(id, title, CheckStatus.Failed, summary, details);
    }

    private static async Task<DiagnosticCheck> CheckDnsAsync(string host, CancellationToken cancellationToken)
    {
        var cleanHost = NormalizeHost(host);
        var details = new List<string>();

        // 1) System resolver (uses the DNS servers configured on the adapter).
        var stopwatch = Stopwatch.StartNew();
        var systemOk = false;
        long systemMs;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(cleanHost, cancellationToken);
            stopwatch.Stop();
            systemMs = stopwatch.ElapsedMilliseconds;
            systemOk = addresses.Length > 0;
            details.Add(systemOk
                ? L.DnsResolvedVia(L.DnsSystemLabel, cleanHost, addresses[0].ToString(), systemMs)
                : L.DnsFailedVia(L.DnsSystemLabel, cleanHost, L.ValNone));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            stopwatch.Stop();
            systemMs = stopwatch.ElapsedMilliseconds;
            details.Add(L.DnsFailedVia(L.DnsSystemLabel, cleanHost, ex.Message));
        }

        details.Add(GetDnsServersText());

        // 2) Compare against well-known resolvers by querying them directly over UDP.
        var anyOtherOk = false;
        foreach (var resolver in KnownDns.ComparisonSet)
        {
            var probe = await DnsProbe.ResolveAsync(resolver.Ip, cleanHost, 2500, cancellationToken);
            var label = $"{resolver.DisplayName} ({resolver.Ip})";
            details.Add(probe.Success
                ? L.DnsResolvedVia(label, cleanHost, probe.FirstAddress!, probe.ElapsedMs)
                : L.DnsFailedVia(label, cleanHost, probe.Message));
            if (probe.Success)
            {
                anyOtherOk = true;
            }
        }

        if (!systemOk && anyOtherOk)
        {
            return new DiagnosticCheck("dns", L.CardDns, CheckStatus.Failed, L.DnsSystemBrokenOthersWork, details, "dns-safe-public");
        }

        if (!systemOk)
        {
            return new DiagnosticCheck("dns", L.CardDns, CheckStatus.Failed, L.DnsAllFailed, details);
        }

        if (systemMs > 1500)
        {
            return new DiagnosticCheck("dns", L.CardDns, CheckStatus.Warning, L.DnsSlow, details);
        }

        return new DiagnosticCheck("dns", L.CardDns, CheckStatus.Healthy, L.DnsHealthy, details);
    }

    // Rotate across multiple well-known IPs rather than pinging just 1.1.1.1: some networks
    // (e.g. Iran-style filtering) block ICMP to one specific address while the rest of the
    // internet works fine, which previously showed up as a false "ICMP blocked" result.
    private static readonly string[] QualityPingTargets = ["1.1.1.1", "8.8.8.8", "9.9.9.9"];

    private static async Task<DiagnosticCheck> CheckNetworkQualityAsync(CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var replies = new List<PingResult>();

        for (var i = 0; i < 8; i++)
        {
            var target = QualityPingTargets[i % QualityPingTargets.Length];
            replies.Add(await PingOnceAsync(target, 1200, cancellationToken));
            await Task.Delay(120, cancellationToken);
        }

        var success = replies.Where(reply => reply.Success).ToList();
        var lossPercent = (int)Math.Round((replies.Count - success.Count) * 100.0 / replies.Count);
        var average = success.Count == 0 ? 0 : (int)Math.Round(success.Average(reply => reply.RoundtripTime));
        var max = success.Count == 0 ? 0 : success.Max(reply => reply.RoundtripTime);

        details.Add(L.QualityPacketLoss(lossPercent));
        details.Add(success.Count > 0 ? L.QualityLatency(average, max) : L.QualityNoReplies);

        if (success.Count == 0)
        {
            return new DiagnosticCheck("quality", L.CardQuality, CheckStatus.Warning, L.QualityIcmpBlocked, details);
        }

        if (lossPercent >= 25 || average > 250)
        {
            return new DiagnosticCheck("quality", L.CardQuality, CheckStatus.Failed, L.QualityUnstable, details);
        }

        if (lossPercent > 0 || average > 100)
        {
            return new DiagnosticCheck("quality", L.CardQuality, CheckStatus.Warning, L.QualityMinorIssues, details);
        }

        return new DiagnosticCheck("quality", L.CardQuality, CheckStatus.Healthy, L.QualityHealthy, details);
    }

    private static async Task<DiagnosticCheck> CheckPortAsync(PortProbeRequest request, CancellationToken cancellationToken)
    {
        var cleanHost = NormalizeHost(request.Host);
        var details = new List<string> { $"{L.LblHost}: {cleanHost}", $"{L.Port}: {request.Port}" };
        using var tcp = new TcpClient();

        try
        {
            var connectTask = tcp.ConnectAsync(cleanHost, request.Port, cancellationToken).AsTask();
            var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(4), cancellationToken));
            if (completed != connectTask)
            {
                details.Add(L.PortTimedOut);
                return new DiagnosticCheck("port", L.CardPort, CheckStatus.Failed, L.PortNoResponse(request.Port, cleanHost), details);
            }

            await connectTask;
            details.Add(L.PortSucceeded);
            return new DiagnosticCheck("port", L.CardPort, CheckStatus.Healthy, L.PortReachable(request.Port, cleanHost), details);
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            details.Add(L.PortError(ex.Message));
            return new DiagnosticCheck("port", L.CardPort, CheckStatus.Failed, L.PortUnreachable(request.Port, cleanHost), details);
        }
    }

    private static Task<DiagnosticCheck> CheckVpnAsync(string scenario, CheckStatus localStatus, CheckStatus internationalStatus, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var adapters = NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .ToList();

        var vpnAdapters = adapters
            .Where(adapter => LooksLikeVpn(adapter.Name) || LooksLikeVpn(adapter.Description) || adapter.NetworkInterfaceType == NetworkInterfaceType.Ppp)
            .ToList();

        var details = vpnAdapters.Count == 0
            ? new List<string> { L.VpnNoneDetail }
            : vpnAdapters.Select(adapter => $"{adapter.Name} ({adapter.Description})").ToList();

        var defaultRouteAdapters = adapters
            .Select(adapter => new
            {
                Adapter = adapter,
                Gateways = adapter.GetIPProperties().GatewayAddresses
                    .Where(gateway => gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(gateway => gateway.Address.ToString())
                    .Distinct()
                    .ToList()
            })
            .Where(item => item.Gateways.Count > 0)
            .ToList();

        foreach (var route in defaultRouteAdapters)
        {
            details.Add(L.VpnAdapterRoute(route.Adapter.Name, string.Join(", ", route.Gateways)));
        }

        if (defaultRouteAdapters.Count > 1)
        {
            details.Add(L.VpnMultipleDefaultRoutes(defaultRouteAdapters.Count));
        }

        var vpnHasDefaultRoute = defaultRouteAdapters.Any(route => vpnAdapters.Contains(route.Adapter));
        if (vpnHasDefaultRoute)
        {
            details.Add(L.VpnAdapterHasDefaultRoute);
        }

        var scenarioMentionsVpn = scenario.Contains("VPN", StringComparison.OrdinalIgnoreCase);
        if (vpnAdapters.Count == 0 && scenarioMentionsVpn)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", L.CardVpn, CheckStatus.Warning, L.VpnMentionedButNone, details));
        }

        if (vpnAdapters.Count > 0 && (vpnHasDefaultRoute || localStatus == CheckStatus.Healthy) && internationalStatus != CheckStatus.Healthy)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", L.CardVpn, CheckStatus.Warning, L.VpnActiveIntlFails, details));
        }

        if (vpnHasDefaultRoute && defaultRouteAdapters.Count > 1)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", L.CardVpn, CheckStatus.Warning, L.VpnAdapterHasDefaultRoute, details));
        }

        if (vpnAdapters.Count > 0)
        {
            return Task.FromResult(new DiagnosticCheck("vpn", L.CardVpn, CheckStatus.Healthy, L.VpnActiveNoFault, details));
        }

        return Task.FromResult(new DiagnosticCheck("vpn", L.CardVpn, CheckStatus.Healthy, L.VpnNone, details));
    }
    private static async Task<DiagnosticCheck> CheckProxyAsync(CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var winHttp = await ProcessRunner.RunAsync("netsh.exe", "winhttp show proxy", cancellationToken: cancellationToken);
        var winHttpText = $"{winHttp.Output}\n{winHttp.Error}".Trim();
        details.Add(string.IsNullOrWhiteSpace(winHttpText) ? L.ProxyNoOutput : winHttpText);

        var userProxyEnabled = false;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            userProxyEnabled = Convert.ToInt32(key?.GetValue("ProxyEnable") ?? 0, CultureInfo.InvariantCulture) == 1;
            var proxyServer = Convert.ToString(key?.GetValue("ProxyServer"), CultureInfo.InvariantCulture);
            details.Add(userProxyEnabled ? L.ProxyUserEnabled(proxyServer ?? string.Empty) : L.ProxyUserDisabled);
        }
        catch (Exception ex)
        {
            details.Add(L.ProxyUserCheckFailed(ex.Message));
        }

        var winHttpProxySet = !winHttpText.Contains("Direct access", StringComparison.OrdinalIgnoreCase)
            && !winHttpText.Contains("no proxy", StringComparison.OrdinalIgnoreCase)
            && winHttpText.Contains("Proxy", StringComparison.OrdinalIgnoreCase);

        if (winHttpProxySet)
        {
            return new DiagnosticCheck("proxy", L.CardProxy, CheckStatus.Warning, L.ProxyWinHttpSet, details, "proxy-reset");
        }

        if (userProxyEnabled)
        {
            return new DiagnosticCheck("proxy", L.CardProxy, CheckStatus.Warning, L.ProxyUserOn, details);
        }

        return new DiagnosticCheck("proxy", L.CardProxy, CheckStatus.Healthy, L.ProxyHealthy, details);
    }

    /// <summary>
    /// A stale Hosts entry is a classic cause of "only one specific site fails" while
    /// everything else works, since DNS for every other domain is never consulted for it.
    /// </summary>
    private static async Task<DiagnosticCheck> CheckHostsFileAsync(string host, CancellationToken cancellationToken)
    {
        var cleanHost = NormalizeHost(host);
        var hostsPath = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(hostsPath, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new DiagnosticCheck("hosts", L.CardHosts, CheckStatus.Warning, L.HostsUnreadable(ex.Message), [L.HostsUnreadable(ex.Message)]);
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.Split('#')[0].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var ip = parts[0];
            var matches = parts.Skip(1).Any(name => string.Equals(name, cleanHost, StringComparison.OrdinalIgnoreCase));
            if (matches)
            {
                return new DiagnosticCheck(
                    "hosts",
                    L.CardHosts,
                    CheckStatus.Warning,
                    L.HostsOverrideFound(cleanHost, ip),
                    [rawLine.Trim(), L.HostsOverrideHint]);
            }
        }

        return new DiagnosticCheck("hosts", L.CardHosts, CheckStatus.Healthy, L.HostsNoOverride, [L.HostsNoOverride]);
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
            return new HttpProbeResult(ok, L.ProbeOk(uri.Host, (int)response.StatusCode, stopwatch.ElapsedMilliseconds));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or SocketException)
        {
            stopwatch.Stop();
            return new HttpProbeResult(false, L.ProbeFailed(uri.Host, stopwatch.ElapsedMilliseconds, ex.Message));
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
            return $"{L.DnsSystemLabel}: {L.ValNone}.";
        }

        var dnsServers = adapter.GetIPProperties().DnsAddresses
            .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.ToString())
            .Distinct()
            .ToList();

        return dnsServers.Count == 0
            ? $"{L.DnsSystemLabel} ({adapter.Name}): {L.ValNone}."
            : $"{L.DnsSystemLabel} ({adapter.Name}): {string.Join(", ", dnsServers)}";
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
            return L.SummaryLocalOkIntlFails(region.CountryName);
        }

        if (local.Status != CheckStatus.Healthy && international.Status == CheckStatus.Healthy)
        {
            return L.SummaryIntlOkLocalFails;
        }

        if (local.Status != CheckStatus.Healthy && international.Status != CheckStatus.Healthy)
        {
            return L.SummaryBothFail;
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

        return L.SummaryAllHealthy;
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
            // The Iran edition is built specifically for Iranian users, so it always
            // uses the Iran profile regardless of the machine's locale settings.
            if (AppConfig.IsPersian)
            {
                return new RegionProfile("IR", L.CountryIran, LocalTargetsFor("IR"));
            }

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
                name = L.CountryIran;
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
