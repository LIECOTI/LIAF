using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LIAF.Common;

public record DeviceInfo(string Serial, string State, string Mode, string Model = "");

public static class DeviceManager
{
    public static async Task<List<DeviceInfo>> GetDevicesAsync()
    {
        var devices = new List<DeviceInfo>();
        try
        {
            var adbOut = await ProcessHelper.Adb("devices -l");
            foreach (var line in adbOut.Split('\n'))
            {
                var t = line.Trim();
                if (string.IsNullOrEmpty(t) || t.StartsWith("List") || t.StartsWith("*")) continue;
                var parts = t.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var model = "";
                    foreach (var p in parts)
                        if (p.StartsWith("model:")) model = p.Substring(6);
                    devices.Add(new DeviceInfo(parts[0], parts[1], "ADB", model));
                }
            }
        } catch { }
        try
        {
            var fbOut = await ProcessHelper.Fastboot("devices");
            foreach (var line in fbOut.Split('\n'))
            {
                var t = line.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                var parts = t.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1)
                    devices.Add(new DeviceInfo(parts[0], parts.Length > 1 ? parts[1] : "fastboot", "Fastboot"));
            }
        } catch { }
        return devices;
    }

    public static async Task<Dictionary<string, string>> GetPropsAsync(string serial)
    {
        var props = new Dictionary<string, string>();
        var keys = new[] {
            "ro.product.model", "ro.product.brand", "ro.product.device",
            "ro.build.version.release", "ro.build.version.sdk",
            "ro.build.display.id", "ro.product.cpu.abi",
            "ro.build.version.security_patch", "ro.hardware",
            "ro.boot.slot_suffix", "ro.boot.hardware.revision"
        };
        foreach (var key in keys)
        {
            try
            {
                var val = (await ProcessHelper.Adb($"-s {serial} shell getprop {key}")).Trim();
                if (!string.IsNullOrEmpty(val)) props[key] = val;
            } catch { }
        }
        return props;
    }

    public static async Task<string> GetBatteryAsync(string serial)
    {
        try { return (await ProcessHelper.Adb($"-s {serial} shell dumpsys battery")).Trim(); }
        catch { return ""; }
    }

    public static async Task<string> GetStorageAsync(string serial)
    {
        try { return (await ProcessHelper.Adb($"-s {serial} shell df -h")).Trim(); }
        catch { return ""; }
    }
}
