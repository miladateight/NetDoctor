using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using NetDoctor.App.Licensing;
using NetDoctor.App.Localization;

namespace NetDoctor.Tests;

// Dependency-free license test runner. Signs tokens with a freshly generated TEMPORARY
// keypair and validates them against a LicenseManager configured with the matching
// temporary public key. The real private key is never used or referenced here.
internal static class Program
{
    private static int failures;

    private static int Main()
    {
        var correctEdition = AppConfig.Edition == AppEdition.Iran ? "Iran" : "International";
        var wrongEdition = AppConfig.Edition == AppEdition.Iran ? "International" : "Iran";
        var thisMachine = MachineFingerprint.Current;

        Console.WriteLine($"Build edition : {AppConfig.Edition}");
        Console.WriteLine($"Machine ID    : {thisMachine} (available={MachineFingerprint.IsAvailable})");
        Console.WriteLine();

        var (priv, pub) = NewKeyPair();

        // 1) Valid legacy license for this machine + edition.
        Check("Valid legacy token (this machine + edition)",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", correctEdition, thisMachine, Now - 3600, Now + 86400)).Status,
            LicenseStatus.Valid);

        // 2) Same license, different machine.
        Check("WrongMachine (different machine)",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", correctEdition, "ND-0000-0000-0000-0000-0000", Now - 3600, Now + 86400)).Status,
            LicenseStatus.WrongMachine);

        // 3) Deprecated edition is ignored for compatibility.
        Check("Deprecated edition ignored",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", wrongEdition, thisMachine, Now - 3600, Now + 86400)).Status,
            LicenseStatus.Valid);

        Check("Valid v0.5 token without edition",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", null, thisMachine, Now - 3600, Now + 86400)).Status,
            LicenseStatus.Valid);

        // 4) Expired.
        Check("Expired",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", correctEdition, thisMachine, Now - 100000, Now - 1000)).Status,
            LicenseStatus.Expired);

        // 5) Tampered signature (validated against a non-matching public key).
        var (_, otherPub) = NewKeyPair();
        Check("BadSignature (wrong key)",
            new LicenseManager(otherPub, TempDir()).Validate(
                Token(priv, "Milad", correctEdition, thisMachine, Now - 3600, Now + 86400)).Status,
            LicenseStatus.BadSignature);

        // 6) Token without a machine field is malformed.
        Check("Malformed (legacy token, no machine)",
            new LicenseManager(pub, TempDir()).Validate(
                Token(priv, "Milad", correctEdition, machine: null, Now - 3600, Now + 86400)).Status,
            LicenseStatus.Malformed);

        Check("Malformed (garbage)",
            new LicenseManager(pub, TempDir()).Validate("not-a-real-token").Status,
            LicenseStatus.Malformed);

        // 7) A copied license.key from another machine must not activate (and must not be stored).
        var dir = TempDir();
        var copyResult = new LicenseManager(pub, dir).Activate(
            Token(priv, "Milad", correctEdition, "ND-1111-2222-3333-4444-5555", Now - 3600, Now + 86400));
        Check("Copied-from-other-machine activate -> WrongMachine", copyResult.Status, LicenseStatus.WrongMachine);
        CheckBool("Copied-from-other-machine not stored", !File.Exists(Path.Combine(dir, "license.json")));

        // 8) Valid activation stores token inside license.json (no private material).
        var dir2 = TempDir();
        var lm2 = new LicenseManager(pub, dir2);
        var validToken = Token(priv, "Milad", correctEdition, thisMachine, Now - 3600, Now + 86400);
        CheckBool("Valid activate stores token", lm2.Activate(validToken).IsValid
            && File.ReadAllText(Path.Combine(dir2, "license.json")).Contains(validToken, StringComparison.Ordinal));

        // 9) Machine ID is well-formed and stable across reads.
        CheckBool("Machine ID well-formed", Regex.IsMatch(thisMachine,
            "^ND-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$"));
        CheckBool("Machine ID stable", thisMachine == MachineFingerprint.Current);

        Console.WriteLine();
        Console.WriteLine(failures == 0 ? "ALL TESTS PASSED" : $"{failures} TEST(S) FAILED");
        return failures == 0 ? 0 : 1;
    }

    private static void Check(string name, LicenseStatus actual, LicenseStatus expected)
        => CheckBool($"{name}: {actual}", actual == expected);

    private static void CheckBool(string name, bool ok)
    {
        Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {name}");
        if (!ok)
        {
            failures++;
        }
    }

    private static (string Priv, string Pub) NewKeyPair()
    {
        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return (Convert.ToBase64String(ec.ExportPkcs8PrivateKey()),
                Convert.ToBase64String(ec.ExportSubjectPublicKeyInfo()));
    }

    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ndtests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string B64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private static string Token(string privB64, string name, string? edition, string? machine, long issued, long expires)
    {
        object payload = (edition, machine) switch
        {
            (null, null) => new { name, issued, expires },
            (null, _) => new { name, machine, issued, expires },
            (_, null) => new { name, edition, issued, expires },
            _ => new { name, edition, machine, issued, expires }
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        ec.ImportPkcs8PrivateKey(Convert.FromBase64String(privB64), out _);
        var signature = ec.SignData(bytes, HashAlgorithmName.SHA256);

        return B64Url(bytes) + "." + B64Url(signature);
    }
}
