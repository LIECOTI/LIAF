namespace LIAF.Windows.Core.Logging;

public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogCategory Category,
    LogLevel Level,
    string Message,
    string? Command = null,
    string? Stream = null,
    string? DeviceSerial = null);
