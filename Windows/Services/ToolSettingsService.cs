using LIAF.Windows.Core.Services;

namespace LIAF.Windows.Services;

public sealed class ToolSettingsService : IToolSettingsService
{
    private readonly object syncRoot = new();
    private string adbExecutablePath = "adb";
    private string fastbootExecutablePath = "fastboot";

    public event EventHandler? SettingsChanged;

    public string AdbExecutablePath
    {
        get
        {
            lock (syncRoot)
            {
                return adbExecutablePath;
            }
        }
    }

    public string FastbootExecutablePath
    {
        get
        {
            lock (syncRoot)
            {
                return fastbootExecutablePath;
            }
        }
    }

    public void UpdateAdbExecutablePath(string path)
    {
        lock (syncRoot)
        {
            adbExecutablePath = NormalizePath(path, "adb");
        }

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateFastbootExecutablePath(string path)
    {
        lock (syncRoot)
        {
            fastbootExecutablePath = NormalizePath(path, "fastboot");
        }

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static string NormalizePath(string path, string fallback)
    {
        return string.IsNullOrWhiteSpace(path) ? fallback : path.Trim();
    }
}
