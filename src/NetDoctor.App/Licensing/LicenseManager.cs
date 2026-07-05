using NetDoctor.App.Localization;
using NetDoctor.App.Services;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetDoctor.App.Licensing;

internal enum LicenseStatus
{
    Valid,
    Missing,
    Malformed,
    BadSignature,
    Expired,
    WrongEdition,
    WrongMachine,
    ClockTampered
}

internal sealed record LicenseInfo(
    string Name,
    AppEdition Edition,
    string Machine,
    DateTimeOffset IssuedUtc,
    DateTimeOffset ExpiresUtc)
{
    public int DaysRemaining =>
        (int)Math.Ceiling((ExpiresUtc - DateTimeOffset.UtcNow).TotalDays);
}

internal sealed record LicenseCheck(LicenseStatus Status, LicenseInfo? Info)
{
    public bool IsValid => Status == LicenseStatus.Valid;
}

/// <summary>
/// Offline signed-token licensing. v0.5.0 keeps old edition-bearing tokens valid,
/// but edition is now compatibility metadata and no longer blocks activation.
/// </summary>
internal sealed class LicenseManager
{
    private const string PublicKeyBase64 =
        "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEqzkZMTmJdi7YkjBdXJNPR8TTZ9ehMRp1B3G6JdhJ444Ppk+0y76hR2B+iPSd0Lm/kna/GKKdIOQsEuLJITezMQ==";

    private static readonly TimeSpan ClockTolerance = TimeSpan.FromDays(1);

    private readonly string licensePath;
    private readonly string statePath;
    private readonly string publicKey;

    public string MachineId => MachineFingerprint.Current;

    public LicenseManager()
        : this(PublicKeyBase64, PathService.ProgramDataRoot)
    {
    }

    internal LicenseManager(string publicKeyBase64, string storageDir)
    {
        publicKey = publicKeyBase64;
        licensePath = Path.Combine(storageDir, "license.json");
        statePath = Path.Combine(storageDir, "license.state");
    }

    public LicenseCheck CheckStored()
    {
        var token = ReadStoredToken();
        return token is null ? new LicenseCheck(LicenseStatus.Missing, null) : Validate(token);
    }

    public LicenseCheck Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new LicenseCheck(LicenseStatus.Missing, null);
        }

        var parts = token.Trim().Split('.');
        if (parts.Length != 2)
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        byte[] payloadBytes;
        byte[] signature;
        try
        {
            payloadBytes = Base64UrlDecode(parts[0]);
            signature = Base64UrlDecode(parts[1]);
        }
        catch (FormatException)
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        if (!VerifySignature(payloadBytes, signature))
        {
            return new LicenseCheck(LicenseStatus.BadSignature, null);
        }

        Payload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<Payload>(payloadBytes);
        }
        catch (JsonException)
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Machine))
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        var edition = ParseEdition(payload.Edition);
        DateTimeOffset issuedUtc;
        DateTimeOffset expiresUtc;
        try
        {
            issuedUtc = DateTimeOffset.FromUnixTimeSeconds(payload.Issued);
            expiresUtc = DateTimeOffset.FromUnixTimeSeconds(payload.Expires);
        }
        catch (ArgumentOutOfRangeException)
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        if (expiresUtc <= issuedUtc)
        {
            return new LicenseCheck(LicenseStatus.Malformed, null);
        }

        var machine = NormalizeMachineId(payload.Machine);
        var info = new LicenseInfo(
            payload.Name?.Trim() ?? string.Empty,
            edition,
            machine,
            issuedUtc,
            expiresUtc);

        if (!string.Equals(machine, NormalizeMachineId(MachineFingerprint.Current), StringComparison.Ordinal))
        {
            return new LicenseCheck(LicenseStatus.WrongMachine, info);
        }

        var now = DateTimeOffset.UtcNow;
        if (ReadLastSeen() is { } lastSeen && now < lastSeen - ClockTolerance)
        {
            return new LicenseCheck(LicenseStatus.ClockTampered, info);
        }

        if (now > expiresUtc)
        {
            return new LicenseCheck(LicenseStatus.Expired, info);
        }

        TouchLastSeen(now);
        return new LicenseCheck(LicenseStatus.Valid, info);
    }

    public LicenseCheck Activate(string token)
    {
        var result = Validate(token);
        if (!result.IsValid)
        {
            return result;
        }

        var dir = Path.GetDirectoryName(licensePath)!;
        Directory.CreateDirectory(dir);
        var document = new StoredLicense(token.Trim(), DateTimeOffset.UtcNow);
        File.WriteAllText(licensePath, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }));
        TouchLastSeen(DateTimeOffset.UtcNow);
        return result;
    }

    private string? ReadStoredToken()
    {
        try
        {
            if (File.Exists(licensePath))
            {
                var text = File.ReadAllText(licensePath).Trim();
                if (text.StartsWith("{", StringComparison.Ordinal))
                {
                    var stored = JsonSerializer.Deserialize<StoredLicense>(text);
                    return stored?.Token?.Trim();
                }

                return text;
            }

            var legacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetDoctor", "license.key");
            return File.Exists(legacy) ? File.ReadAllText(legacy).Trim() : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static AppEdition ParseEdition(string? value)
    {
        return value?.Equals("Iran", StringComparison.OrdinalIgnoreCase) == true
            ? AppEdition.Iran
            : AppEdition.International;
    }

    private static string NormalizeMachineId(string machineId) => machineId.Trim().ToUpperInvariant();

    private bool VerifySignature(byte[] payloadBytes, byte[] signature)
    {
        try
        {
            using var ec = ECDsa.Create();
            ec.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return ec.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private DateTimeOffset? ReadLastSeen()
    {
        try
        {
            if (!File.Exists(statePath))
            {
                return null;
            }

            var text = File.ReadAllText(statePath).Trim();
            return long.TryParse(text, out var unix) ? DateTimeOffset.FromUnixTimeSeconds(unix) : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private void TouchLastSeen(DateTimeOffset now)
    {
        try
        {
            var last = ReadLastSeen();
            var newest = last is { } value && value > now ? value : now;
            Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
            File.WriteAllText(statePath, newest.ToUnixTimeSeconds().ToString());
        }
        catch (IOException)
        {
            // Anti-rollback storage is best-effort.
        }
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var text = value.Replace('-', '+').Replace('_', '/');
        switch (text.Length % 4)
        {
            case 2:
                text += "==";
                break;
            case 3:
                text += "=";
                break;
        }

        return Convert.FromBase64String(text);
    }

    private sealed class Payload
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("edition")]
        public string? Edition { get; init; }

        [JsonPropertyName("machine")]
        public string? Machine { get; init; }

        [JsonPropertyName("issued")]
        public long Issued { get; init; }

        [JsonPropertyName("expires")]
        public long Expires { get; init; }
    }

    private sealed record StoredLicense(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("activatedUtc")] DateTimeOffset ActivatedUtc);
}
