using LIAF.Windows.Core.Logging;
using LIAF.Windows.Core.Models;
using LIAF.Windows.Core.Services;

namespace LIAF.Windows.Services;

public sealed class DeviceMonitoringService(
    IAdbService adbService,
    IFastbootService fastbootService,
    ILogService? logService = null) : IDeviceMonitoringService
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private readonly object stateLock = new();
    private CancellationTokenSource? pollingCts;
    private Task? pollingTask;
    private IReadOnlyList<AndroidDevice> currentDevices = Array.Empty<AndroidDevice>();

    public event EventHandler<DeviceSnapshot>? DevicesChanged;

    public IReadOnlyList<AndroidDevice> CurrentDevices
    {
        get
        {
            lock (stateLock)
            {
                return currentDevices.ToArray();
            }
        }
    }

    public bool IsRunning => pollingTask is { IsCompleted: false };

    public async Task<IReadOnlyList<AndroidDevice>> RefreshAsync(CancellationToken cancellationToken = default)
    {
        await refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var adbDevicesTask = adbService.GetDevicesAsync(cancellationToken);
            var fastbootDevicesTask = fastbootService.GetDevicesAsync(cancellationToken);
            await Task.WhenAll(adbDevicesTask, fastbootDevicesTask).ConfigureAwait(false);

            var nextDevices = adbDevicesTask.Result.Concat(fastbootDevicesTask.Result)
                .OrderBy(device => device.Type)
                .ThenBy(device => device.SerialNumber, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var changes = BuildChanges(CurrentDevices, nextDevices);
            lock (stateLock)
            {
                currentDevices = nextDevices;
            }

            var snapshot = new DeviceSnapshot(nextDevices, changes, DateTimeOffset.Now);
            if (changes.Count > 0)
            {
                LogChanges(changes);
                DevicesChanged?.Invoke(this, snapshot);
            }

            return nextDevices;
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public Task StartAsync(TimeSpan? pollingInterval = null, CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        pollingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        pollingTask = PollAsync(pollingInterval ?? TimeSpan.FromSeconds(3), pollingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (pollingCts is null || pollingTask is null)
        {
            return;
        }

        pollingCts.Cancel();
        try
        {
            await pollingTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            pollingCts.Dispose();
            pollingCts = null;
            pollingTask = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        refreshLock.Dispose();
    }

    private async Task PollAsync(TimeSpan pollingInterval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.DeviceMonitoring, LogLevel.Error, ex.Message));
            }

            await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private static IReadOnlyList<DeviceChange> BuildChanges(IReadOnlyList<AndroidDevice> previous, IReadOnlyList<AndroidDevice> next)
    {
        var changes = new List<DeviceChange>();
        var previousByKey = previous.ToDictionary(GetDeviceKey, StringComparer.OrdinalIgnoreCase);
        var nextByKey = next.ToDictionary(GetDeviceKey, StringComparer.OrdinalIgnoreCase);

        foreach (var device in next)
        {
            if (!previousByKey.TryGetValue(GetDeviceKey(device), out var previousDevice))
            {
                changes.Add(new DeviceChange(DeviceChangeKind.Connected, device));
            }
            else if (previousDevice != device)
            {
                changes.Add(new DeviceChange(DeviceChangeKind.Updated, device));
            }
        }

        foreach (var device in previous)
        {
            if (!nextByKey.ContainsKey(GetDeviceKey(device)))
            {
                changes.Add(new DeviceChange(DeviceChangeKind.Disconnected, device));
            }
        }

        return changes;
    }

    private void LogChanges(IEnumerable<DeviceChange> changes)
    {
        foreach (var change in changes)
        {
            var message = $"Device {change.Kind}: {change.Device.SerialNumber} ({change.Device.Type}, {change.Device.State})";
            logService?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.DeviceMonitoring, LogLevel.Information, message, DeviceSerial: change.Device.SerialNumber));
        }
    }

    private static string GetDeviceKey(AndroidDevice device)
    {
        return $"{device.Type}:{device.SerialNumber}";
    }
}
