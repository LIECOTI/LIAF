using LIAF.Windows.Core.Models;

namespace LIAF.Windows.Infrastructure.AndroidTools;

public static class DeviceOutputParser
{
    public static IReadOnlyList<AndroidDevice> ParseAdbDevices(string output)
    {
        return ParseDeviceLines(output, AndroidDeviceType.Adb, skipHeader: true);
    }

    public static IReadOnlyList<AndroidDevice> ParseFastbootDevices(string output)
    {
        return ParseDeviceLines(output, AndroidDeviceType.Fastboot, skipHeader: false);
    }

    private static IReadOnlyList<AndroidDevice> ParseDeviceLines(string output, AndroidDeviceType type, bool skipHeader)
    {
        var devices = new List<AndroidDevice>();
        using var reader = new StringReader(output);

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (skipHeader && line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var columns = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length < 2)
            {
                continue;
            }

            var metadata = ParseMetadata(columns.Skip(2));
            devices.Add(new AndroidDevice(
                SerialNumber: columns[0],
                State: NormalizeState(columns[1], type),
                Type: type,
                Product: metadata.GetValueOrDefault("product"),
                Model: metadata.GetValueOrDefault("model"),
                Transport: metadata.GetValueOrDefault("transport_id")));
        }

        return devices;
    }

    private static string NormalizeState(string state, AndroidDeviceType type)
    {
        if (type == AndroidDeviceType.Fastboot && state.Equals("fastboot", StringComparison.OrdinalIgnoreCase))
        {
            return "device";
        }

        return state;
    }

    private static Dictionary<string, string> ParseMetadata(IEnumerable<string> tokens)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in tokens)
        {
            var separatorIndex = token.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex <= 0 || separatorIndex == token.Length - 1)
            {
                continue;
            }

            metadata[token[..separatorIndex]] = token[(separatorIndex + 1)..];
        }

        return metadata;
    }
}
