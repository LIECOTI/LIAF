using System.Diagnostics;
using System.Text;
using LIAF.Windows.Core.Logging;
using LIAF.Windows.Core.Models;
using LIAF.Windows.Core.Services;

namespace LIAF.Windows.Infrastructure.Process;

public sealed class ProcessRunner(ILogService? logService = null) : IProcessRunner
{
    public async Task<CommandResult> RunAsync(
        string fileName,
        string arguments,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var command = $"{fileName} {arguments}".Trim();
        logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.Command, LogLevel.Information, "Starting command.", Command: command));

        var stopwatch = Stopwatch.StartNew();
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };

        try
        {
            if (!process.Start())
            {
                stopwatch.Stop();
                var failedStart = CommandResult.Failed($"Failed to start process '{fileName}'.", duration: stopwatch.Elapsed);
                LogResult(command, failedStart);
                return failedStart;
            }
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            stopwatch.Stop();
            var failedStart = CommandResult.Failed($"Failed to start process '{fileName}': {ex.Message}", duration: stopwatch.Elapsed);
            LogResult(command, failedStart);
            return failedStart;
        }

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutCts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
        using var linkedCts = timeoutCts is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
            var standardOutput = await standardOutputTask.ConfigureAwait(false);
            var standardError = await standardErrorTask.ConfigureAwait(false);
            stopwatch.Stop();

            var result = new CommandResult(process.ExitCode, standardOutput, standardError, Duration: stopwatch.Elapsed);
            LogResult(command, result);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            throw;
        }
        catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true)
        {
            KillProcessTree(process);
            stopwatch.Stop();

            var standardOutput = await ReadCompletedOrEmptyAsync(standardOutputTask).ConfigureAwait(false);
            var standardError = await ReadCompletedOrEmptyAsync(standardErrorTask).ConfigureAwait(false);
            var timeoutResult = CommandResult.Timeout(timeout!.Value, standardOutput, standardError, stopwatch.Elapsed);
            LogResult(command, timeoutResult);
            return timeoutResult;
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            var failedExecution = CommandResult.Failed($"Process execution failed for '{fileName}': {ex.Message}", duration: stopwatch.Elapsed);
            LogResult(command, failedExecution);
            return failedExecution;
        }
    }

    private void LogResult(string command, CommandResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.StandardOutput, LogLevel.Information, result.StandardOutput, Command: command, Stream: "stdout"));
        }

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.StandardError, LogLevel.Error, result.StandardError, Command: command, Stream: "stderr"));
        }

        var level = result.IsSuccess ? LogLevel.Information : LogLevel.Error;
        var message = result.IsSuccess
            ? $"Command completed with exit code {result.ExitCode}."
            : result.ErrorMessage ?? $"Command failed with exit code {result.ExitCode}.";

        logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.Command, level, message, Command: command));
    }

    private static void KillProcessTree(System.Diagnostics.Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
        {
            // The process may have already exited or may not support tree termination.
        }
    }

    private static async Task<string> ReadCompletedOrEmptyAsync(Task<string> readTask)
    {
        try
        {
            return readTask.IsCompletedSuccessfully ? readTask.Result : await readTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (ObjectDisposedException)
        {
            return string.Empty;
        }
    }
}
