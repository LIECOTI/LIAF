using LIAF.Windows.Core.Logging;
using LIAF.Windows.Core.Services;

namespace LIAF.Windows.Services;

public sealed class InMemoryLogService(int capacity = 1_000) : ILogService
{
    private readonly object syncRoot = new();
    private readonly Queue<LogEntry> entries = new();

    public event EventHandler<LogEntry>? EntryAdded;

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (syncRoot)
            {
                return entries.ToArray();
            }
        }
    }

    public void Add(LogEntry entry)
    {
        lock (syncRoot)
        {
            entries.Enqueue(entry);
            while (entries.Count > capacity)
            {
                entries.Dequeue();
            }
        }

        EntryAdded?.Invoke(this, entry);
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            entries.Clear();
        }
    }
}
