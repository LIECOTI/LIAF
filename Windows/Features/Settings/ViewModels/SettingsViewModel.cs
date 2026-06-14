using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LIAF.Windows.Core.Services;

namespace LIAF.Windows.Features.Settings.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IToolSettingsService toolSettingsService;

    [ObservableProperty]
    private string adbExecutablePath;

    [ObservableProperty]
    private string fastbootExecutablePath;

    [ObservableProperty]
    private string statusMessage = "Tool paths are ready.";

    public SettingsViewModel(IToolSettingsService toolSettingsService)
    {
        this.toolSettingsService = toolSettingsService;
        adbExecutablePath = toolSettingsService.AdbExecutablePath;
        fastbootExecutablePath = toolSettingsService.FastbootExecutablePath;
    }

    [RelayCommand]
    private void Save()
    {
        toolSettingsService.UpdateAdbExecutablePath(AdbExecutablePath);
        toolSettingsService.UpdateFastbootExecutablePath(FastbootExecutablePath);
        StatusMessage = "Tool paths saved for this session.";
    }

    [RelayCommand]
    private void Reset()
    {
        AdbExecutablePath = "adb";
        FastbootExecutablePath = "fastboot";
        Save();
    }
}
