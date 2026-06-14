namespace LIAF.Windows.Features.Logcat.Models;

public sealed record LogcatEntry(DateTimeOffset Timestamp, string Priority, string Tag, string Message);
