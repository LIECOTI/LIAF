using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LIAF.Common;

public static class ProcessHelper
{
    public static string AdbPath { get; set; } = "adb";
    public static string FastbootPath { get; set; } = "fastboot";

    public static async Task<string> RunAsync(string command, string arguments, Action<string>? onOutput = null)
    {
        try
        {
            var psi = new ProcessStartInfo(command, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using var process = new Process { StartInfo = psi };
            process.Start();

            if (onOutput != null)
            {
                var outTask = ReadStreamAsync(process.StandardOutput, onOutput);
                var errTask = ReadStreamAsync(process.StandardError, onOutput);
                await Task.WhenAll(process.WaitForExitAsync(), outTask, errTask);
                return "";
            }

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(stdout)) sb.Append(stdout.TrimEnd());
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append(stderr.TrimEnd());
            }
            return sb.ToString();
        }
        catch (Exception ex) { return "Ошибка: " + ex.Message; }
    }

    private static async Task ReadStreamAsync(StreamReader reader, Action<string> callback)
    {
        char[] buf = new char[512];
        int read;
        while ((read = await reader.ReadAsync(buf, 0, buf.Length)) > 0)
            callback(new string(buf, 0, read));
    }

    public static Task<string> Adb(string args, Action<string>? cb = null) => RunAsync(AdbPath, args, cb);
    public static Task<string> Fastboot(string args, Action<string>? cb = null) => RunAsync(FastbootPath, args, cb);
    public static Task<string> Shell(string cmd, string args = "") => RunAsync(cmd, args);
    public static Task<string> Scrcpy(string args) => RunAsync("scrcpy", args);

    public static async Task<int> RunWithExitCodeAsync(string command, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo(command, arguments)
            {
                CreateNoWindow = true, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            using var p = new Process { StartInfo = psi };
            p.Start();
            await p.WaitForExitAsync();
            return p.ExitCode;
        }
        catch { return -1; }
    }
}
