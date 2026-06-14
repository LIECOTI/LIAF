using LIAF.Windows.Core.Models;

namespace LIAF.Windows.Infrastructure.Process;

public interface IProcessRunner
{
    Task<CommandResult> RunAsync(
        string fileName,
        string arguments,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}
