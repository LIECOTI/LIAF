using LIAF.Windows.Core.Models;
using LIAF.Windows.Core.Services;
using LIAF.Windows.Infrastructure.AndroidTools;
using LIAF.Windows.Infrastructure.Process;

namespace LIAF.Windows.Infrastructure.Adb;

public sealed class AdbService(IProcessRunner processRunner, IToolSettingsService toolSettingsService) : IAdbService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DeviceListTimeout = TimeSpan.FromSeconds(10);

    public async Task<IReadOnlyList<AndroidDevice>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync("devices -l", DeviceListTimeout, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return Array.Empty<AndroidDevice>();
        }

        return DeviceOutputParser.ParseAdbDevices(result.StandardOutput);
    }

    public Task<CommandResult> ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(arguments, DefaultTimeout, cancellationToken);
    }

    public Task<CommandResult> ExecuteAsync(
        string arguments,
        TimeSpan? timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return processRunner.RunAsync(toolSettingsService.AdbExecutablePath, arguments, timeout, cancellationToken);
    }
}
