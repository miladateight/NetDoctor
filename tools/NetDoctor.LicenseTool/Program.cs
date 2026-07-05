using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetDoctor.LicenseTool;

// Private command-line tool for issuing Net Doctor licenses.
//
//   netdoctor-license keygen
//   netdoctor-license issue --name "Ali Rezaei" --days 30 --machine "ND-1234-5678-9ABC-DEF0-1234"
//
// The token format here MUST stay in sync with LicenseManager in the app:
//   token = base64url(payloadJsonUtf8) + "." + base64url(ecdsaP256SignatureOverPayloadBytes)
// Every license is bound to one machine; the --machine value comes from the customer's
// activation screen (Machine ID).
internal static class Program
{
    private static readonly string KeysDir =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "licensing-keys"));

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            return args[0].ToLowerInvariant() switch
            {
                "keygen" => KeyGen(),
                "issue" when args.Length == 1 => IssueInteractive(),
                "issue" => Issue(args),
                _ => PrintUsage()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static int PrintUsage()
    {
        Console.WriteLine("Net Doctor license tool");
        Console.WriteLine();
        Console.WriteLine("  issue                                      Interactive mode: just answer 3 questions.");
        Console.WriteLine("                                             (easiest - or double-click issue-license.cmd)");
        Console.WriteLine();
        Console.WriteLine("  issue --name \"X\" --days 30 --machine \"ND-XXXX-XXXX-XXXX-XXXX-XXXX\"");
        Console.WriteLine("                                             Non-interactive mode, for scripting.");
        Console.WriteLine("    --name     required   customer name");
        Console.WriteLine("    --edition  deprecated, accepted for old scripts and ignored");
        Console.WriteLine("    --days     license length in days (default 30)");
        Console.WriteLine("    --machine  required   Machine ID from the customer's activation screen");
        Console.WriteLine();
        Console.WriteLine("  keygen                                    Generate the signing keypair into licensing-keys/.");
        Console.WriteLine();
        Console.WriteLine($"Keys directory: {KeysDir}");
        return 0;
    }

    private static int KeyGen()
    {
        Directory.CreateDirectory(KeysDir);
        var privatePath = Path.Combine(KeysDir, "private-key.txt");
        var publicPath = Path.Combine(KeysDir, "public-key.txt");

        if (File.Exists(privatePath))
        {
            Console.Error.WriteLine($"A private key already exists at {privatePath}. Refusing to overwrite it.");
            Console.Error.WriteLine("Delete it manually first if you really want to regenerate (this invalidates all existing licenses).");
            return 1;
        }

        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var priv = Convert.ToBase64String(ec.ExportPkcs8PrivateKey());
        var pub = Convert.ToBase64String(ec.ExportSubjectPublicKeyInfo());

        File.WriteAllText(privatePath, priv, new UTF8Encoding(false));
        File.WriteAllText(publicPath, pub, new UTF8Encoding(false));

        Console.WriteLine("Keypair generated.");
        Console.WriteLine($"  Private key -> {privatePath}  (keep secret, never commit)");
        Console.WriteLine($"  Public key  -> {publicPath}");
        Console.WriteLine();
        Console.WriteLine("Embed this public key in the app (LicenseManager.PublicKeyBase64):");
        Console.WriteLine(pub);
        return 0;
    }

    private static int Issue(string[] args)
    {
        var options = ParseOptions(args);
        var name = options.GetValueOrDefault("name", "").Trim();
        var edition = options.GetValueOrDefault("edition", string.Empty).Trim();
        var days = int.TryParse(options.GetValueOrDefault("days", "30"), out var d) ? d : 30;
        var machine = options.GetValueOrDefault("machine", "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.Error.WriteLine("--name is required.");
            return 1;
        }
        if (!string.IsNullOrWhiteSpace(edition))
        {
            Console.Error.WriteLine("Warning: --edition is deprecated in v0.5.0 and will be ignored.");
        }

        if (string.IsNullOrWhiteSpace(machine))
        {
            Console.Error.WriteLine("--machine is required. Ask the customer for the Machine ID shown on the activation screen.");
            return 1;
        }

        if (!IsValidMachineId(machine))
        {
            Console.Error.WriteLine("--machine has an invalid format. Expected ND-XXXX-XXXX-XXXX-XXXX-XXXX where X is 0-9 or A-F.");
            return 1;
        }

        if (days <= 0)
        {
            Console.Error.WriteLine("--days must be greater than zero.");
            return 1;
        }

        return SignAndPrint(name, machine, days, edition);
    }

    // Sentinel thrown when stdin is closed/exhausted (EOF) while a prompt loop is waiting
    // for input. Without this, a closed input stream makes Console.ReadLine() return null
    // forever, which would otherwise spin the retry loops below indefinitely.
    private sealed class InputClosedException : Exception
    {
    }

    private static string ReadLineOrThrow()
    {
        var line = Console.ReadLine();
        if (line is null)
        {
            throw new InputClosedException();
        }

        return line;
    }

    // Friendly mode for non-technical use: just answer three questions and get a clean
    // license key back. Invoked when "issue" is run with no further flags.
    private static int IssueInteractive()
    {
        Console.WriteLine("=== Net Doctor - License Issuer ===");
        Console.WriteLine();

        try
        {
            var name = PromptLine("Customer name (optional, Enter to skip)", "Customer");
            int days;
            while (true)
            {
                Console.Write("Number of days [30]: ");
                var raw = ReadLineOrThrow().Trim();
                if (raw.Length == 0)
                {
                    days = 30;
                    break;
                }
                if (int.TryParse(raw, out days) && days > 0)
                {
                    break;
                }
                Console.WriteLine("Please enter a whole number of days greater than zero.");
            }

            string machine;
            while (true)
            {
                Console.Write("Machine ID (from the customer's activation screen): ");
                machine = ReadLineOrThrow().Trim().ToUpperInvariant();
                if (IsValidMachineId(machine))
                {
                    break;
                }
                Console.WriteLine("That doesn't look like a Machine ID. Expected format: ND-XXXX-XXXX-XXXX-XXXX-XXXX");
            }

            Console.WriteLine();
            Console.WriteLine("Issuing license...");
            Console.WriteLine();

            var result = SignAndPrint(name, machine, days, null);

            Console.WriteLine();
            Console.WriteLine("Copy everything after 'License key:' above and send it to the customer.");
            Console.WriteLine();
            Console.Write("Press Enter to exit... ");
            Console.ReadLine();
            return result;
        }
        catch (InputClosedException)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("Input closed before all answers were given. No license was issued.");
            return 1;
        }
    }

    private static string PromptLine(string prompt, string defaultValue)
    {
        Console.Write($"{prompt}: ");
        var raw = ReadLineOrThrow().Trim();
        return raw.Length == 0 ? defaultValue : raw;
    }

    private static int SignAndPrint(string name, string machine, int days, string? deprecatedEdition)
    {
        var privatePath = Path.Combine(KeysDir, "private-key.txt");
        if (!File.Exists(privatePath))
        {
            Console.Error.WriteLine($"No private key found at {privatePath}. Run 'keygen' first.");
            return 1;
        }

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddDays(days);
        var payload = new
        {
            name,
            machine,
            issued = now.ToUnixTimeSeconds(),
            expires = expires.ToUnixTimeSeconds()
        };

        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        ec.ImportPkcs8PrivateKey(Convert.FromBase64String(File.ReadAllText(privatePath).Trim()), out _);
        var signature = ec.SignData(payloadBytes, HashAlgorithmName.SHA256);

        var token = Base64Url(payloadBytes) + "." + Base64Url(signature);

        Console.WriteLine($"Customer  : {name}");
        if (!string.IsNullOrWhiteSpace(deprecatedEdition))
        {
            Console.WriteLine("Edition   : deprecated/ignored");
        }
        Console.WriteLine($"Machine ID: {machine}");
        Console.WriteLine($"Issued    : {now:yyyy-MM-dd HH:mm} UTC");
        Console.WriteLine($"Expires   : {expires:yyyy-MM-dd HH:mm} UTC  ({days} days)");
        Console.WriteLine();
        Console.WriteLine("License key:");
        Console.WriteLine(token);
        return 0;
    }

    // Matches the format produced by MachineFingerprint: ND-XXXX-XXXX-XXXX-XXXX-XXXX (hex groups).
    private static bool IsValidMachineId(string machine) =>
        Regex.IsMatch(machine, "^ND-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$");

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                result[args[i][2..]] = args[i + 1];
            }
        }

        return result;
    }

    private static string Base64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
