namespace LIAF.Windows.Core.Models;

public sealed record DeviceSnapshot(IReadOnlyList<AndroidDevice> Devices, IReadOnlyList<DeviceChange> Changes, DateTimeOffset Timestamp);
