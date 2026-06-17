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
}
