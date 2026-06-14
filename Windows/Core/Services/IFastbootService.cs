using LIAF.Windows.Core.Models;

namespace LIAF.Windows.Core.Services;

public interface IFastbootService
{
    Task<IReadOnlyList<AndroidDevice>> GetDevicesAsync(CancellationToken cancellationToken = default);

    Task<CommandResult> ExecuteAsync(string arguments, CancellationToken cancellationToken = default);

    Task<CommandResult> ExecuteAsync(string arguments, TimeSpan? timeout, CancellationToken cancellationToken = default);
}
