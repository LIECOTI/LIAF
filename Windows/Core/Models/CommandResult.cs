namespace LIAF.Windows.Core.Models;

public sealed record CommandResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool TimedOut = false,
    TimeSpan? Duration = null,
    string? ErrorMessage = null)
{
    public bool IsSuccess => ExitCode == 0 && !TimedOut && ErrorMessage is null;

    public static CommandResult Failed(string errorMessage, string standardError = "", int exitCode = -1, TimeSpan? duration = null)
    {
        return new CommandResult(exitCode, string.Empty, standardError, Duration: duration, ErrorMessage: errorMessage);
    }

    public static CommandResult Timeout(TimeSpan timeout, string standardOutput, string standardError, TimeSpan duration)
    {
        var message = $"The command timed out after {timeout}.";
        return new CommandResult(-1, standardOutput, standardError, TimedOut: true, Duration: duration, ErrorMessage: message);
    }
}
