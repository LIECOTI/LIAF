using LIAF.Windows.Core.Models;

namespace LIAF.Windows.Core.Services;

public interface IDeviceMonitoringService : IAsyncDisposable
{
    event EventHandler<DeviceSnapshot>? DevicesChanged;

    IReadOnlyList<AndroidDevice> CurrentDevices { get; }

    bool IsRunning { get; }

    Task<IReadOnlyList<AndroidDevice>> RefreshAsync(CancellationToken cancellationToken = default);

    Task StartAsync(TimeSpan? pollingInterval = null, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
