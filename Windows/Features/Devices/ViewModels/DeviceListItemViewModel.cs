using LIAF.Windows.Core.Models;

namespace LIAF.Windows.Features.Devices.ViewModels;

public sealed class DeviceListItemViewModel(AndroidDevice device)
{
    public string SerialNumber { get; } = device.SerialNumber;

    public string State { get; } = device.State;

    public string Type { get; } = device.Type.ToString();

    public string DisplayName { get; } = device.Model ?? device.Product ?? device.SerialNumber;
}
