using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace NetDoctor.App.Licensing;

/// <summary>
/// Creates a stable, non-reversible machine identifier from the Windows MachineGuid.
/// The raw MachineGuid is never shown or stored in a license.
/// </summary>
internal static class MachineFingerprint
{
    private const string RegistryPath = @"SOFTWARE\Microsoft\Cryptography";
    private const string RegistryValueName = "MachineGuid";
    private const string ProductSalt = "NetDoctor-License-v1";

    /// <summary>The machine identifier, or an empty string if it could not be read.</summary>
    public static string Current { get; }

    /// <summary>True when a stable machine identifier could be computed.</summary>
    public static bool IsAvailable { get; }

    static MachineFingerprint()
    {
        // Never let a missing MachineGuid crash the app at type-load time.
        try
        {
            Current = Create();
            IsAvailable = true;
        }
        catch (Exception)
        {
            Current = string.Empty;
            IsAvailable = false;
        }
    }

    private static string Create()
    {
        var machineGuid = ReadMachineGuid();
        var normalized = machineGuid.Trim().ToUpperInvariant();
        var input = $"{ProductSalt}|{normalized}";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        // Use the first 80 bits: short enough to send, strong enough to avoid collisions.
        var hex = Convert.ToHexString(hash.AsSpan(0, 10));

        return $"ND-{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}";
    }

    private static string ReadMachineGuid()
    {
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var key = baseKey.OpenSubKey(RegistryPath, writable: false);

            if (key?.GetValue(RegistryValueName) is string value &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException("Windows MachineGuid is unavailable.");
    }
}