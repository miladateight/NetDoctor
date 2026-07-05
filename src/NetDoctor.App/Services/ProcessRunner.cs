using System.Diagnostics;
using System.ComponentModel;
using System.Text;

namespace NetDoctor.App.Services;

internal static class ProcessRunner
{
    public static async Task<(int ExitCode, string Output, string Error)> RunAsync(
        string fileName,
        string arguments,
        bool elevated = false,
        CancellationToken cancellationToken = default)
    {
        if (elevated)
        {
            Process? elevatedProcess;
            try
            {
                elevatedProcess = Process.Start(new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Win32Exception ex)
            {
                return (-1, string.Empty, ex.Message);
            }

            if (elevatedProcess is null)
            {
                return (-1, string.Empty, "The process did not start.");
            }

            using (elevatedProcess)
            {
                await elevatedProcess.WaitForExitAsync(cancellationToken);
                return (elevatedProcess.ExitCode, string.Empty, string.Empty);
            }
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return (process.ExitCode, await outputTask, await errorTask);
    }

    /// <summary>
    /// Runs several Windows commands in a single elevated batch file so the user sees only
    /// one UAC prompt for the whole repair. A command whose failure must fail the whole repair
    /// has to end with <c>|| exit /b 1</c>; reaching the end of the script returns 0. This way a
    /// non-critical command failing mid-script (for example adding a secondary DNS that already
    /// exists) does not make the whole fix look failed. Returns the batch exit code, or -1 if
    /// elevation was declined.
    /// </summary>
    public static async Task<int> RunElevatedScriptAsync(
        IReadOnlyList<string> commandLines,
        CancellationToken cancellationToken = default)
    {
        var batPath = Path.Combine(Path.GetTempPath(), $"netdoctor-fix-{Guid.NewGuid():N}.bat");
        var script = "@echo off\r\nchcp 65001 >nul\r\n" + string.Join("\r\n", commandLines) + "\r\nexit /b 0\r\n";
        await File.WriteAllTextAsync(batPath, script, cancellationToken);

        try
        {
            Process? elevatedProcess;
            try
            {
                elevatedProcess = Process.Start(new ProcessStartInfo(batPath)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Win32Exception)
            {
                return -1;
            }

            if (elevatedProcess is null)
            {
                return -1;
            }

            using (elevatedProcess)
            {
                await elevatedProcess.WaitForExitAsync(cancellationToken);
                return elevatedProcess.ExitCode;
            }
        }
        finally
        {
            try
            {
                File.Delete(batPath);
            }
            catch (IOException)
            {
                // Best-effort cleanup; the temp file is harmless if it lingers.
            }
        }
    }
}
