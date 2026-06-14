using LIAF.Windows.Core.Logging;

namespace LIAF.Windows.Core.Services;

public interface ILogService
{
    event EventHandler<LogEntry>? EntryAdded;

    IReadOnlyList<LogEntry> Entries { get; }

    void Add(LogEntry entry);

    void Clear();
}
