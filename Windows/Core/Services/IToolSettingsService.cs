namespace LIAF.Windows.Core.Services;

public interface IToolSettingsService
{
    event EventHandler? SettingsChanged;

    string AdbExecutablePath { get; }

    string FastbootExecutablePath { get; }

    void UpdateAdbExecutablePath(string path);

    void UpdateFastbootExecutablePath(string path);
}
