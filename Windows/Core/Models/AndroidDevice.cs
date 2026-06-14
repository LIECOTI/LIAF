namespace LIAF.Windows.Core.Models;

public sealed record AndroidDevice(
    string SerialNumber,
    string State,
    AndroidDeviceType Type = AndroidDeviceType.Adb,
    string? Product = null,
    string? Model = null,
    string? Transport = null);
