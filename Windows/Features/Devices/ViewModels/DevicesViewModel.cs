using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LIAF.Windows.Core.Models;
using LIAF.Windows.Core.Services;
using Microsoft.UI.Dispatching;

namespace LIAF.Windows.Features.Devices.ViewModels;

public partial class DevicesViewModel : ObservableObject
{
    private readonly IAdbService adbService;
    private readonly IDeviceMonitoringService deviceMonitoringService;
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string commandText = "devices -l";

    [ObservableProperty]
    private string commandOutput = string.Empty;

    [ObservableProperty]
    private string commandError = string.Empty;

    [ObservableProperty]
    private string commandStatus = "No command executed.";

    public DevicesViewModel(IAdbService adbService, IDeviceMonitoringService deviceMonitoringService)
    {
        this.adbService = adbService;
        this.deviceMonitoringService = deviceMonitoringService;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.deviceMonitoringService.DevicesChanged += OnDevicesChanged;
    }

    public ObservableCollection<DeviceListItemViewModel> Devices { get; } = new();

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasCommandError => !string.IsNullOrWhiteSpace(CommandError);

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnCommandErrorChanged(string value)
    {
        OnPropertyChanged(nameof(HasCommandError));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await RunWithLoadingAsync(async cancellationToken =>
        {
            var devices = await deviceMonitoringService.RefreshAsync(cancellationToken).ConfigureAwait(true);
            ReplaceDevices(devices);
        }).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RunAdbCommandAsync()
    {
        await RunWithLoadingAsync(async cancellationToken =>
        {
            CommandOutput = string.Empty;
            CommandError = string.Empty;
            CommandStatus = "Running...";

            var result = await adbService.ExecuteAsync(CommandText, TimeSpan.FromSeconds(60), cancellationToken).ConfigureAwait(true);
            CommandOutput = result.StandardOutput;
            CommandError = result.IsSuccess
                ? string.Empty
                : result.ErrorMessage ?? result.StandardError;

            var duration = result.Duration is null ? "n/a" : $"{result.Duration.Value.TotalMilliseconds:0} ms";
            CommandStatus = $"Exit code: {result.ExitCode}; Duration: {duration}; Timed out: {result.TimedOut}";

            if (!result.IsSuccess && string.IsNullOrWhiteSpace(CommandError))
            {
                CommandError = $"ADB command failed with exit code {result.ExitCode}.";
            }
        }).ConfigureAwait(true);
    }

    private async Task RunWithLoadingAsync(Func<CancellationToken, Task> action)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await action(CancellationToken.None).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Operation canceled.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnDevicesChanged(object? sender, DeviceSnapshot snapshot)
    {
        if (dispatcherQueue.HasThreadAccess)
        {
            ReplaceDevices(snapshot.Devices);
            return;
        }

        dispatcherQueue.TryEnqueue(() => ReplaceDevices(snapshot.Devices));
    }

    private void ReplaceDevices(IEnumerable<AndroidDevice> devices)
    {
        Devices.Clear();
        foreach (var device in devices)
        {
            Devices.Add(new DeviceListItemViewModel(device));
        }
    }

}