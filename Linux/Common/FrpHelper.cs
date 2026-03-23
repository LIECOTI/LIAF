using System;
using System.Threading.Tasks;

namespace LIAF.Common;

public static class FrpHelper
{
    public static async Task<string> RemoveFrpAdb(string serial, Action<string>? log = null)
    {
        log?.Invoke("Попытка сброса FRP через ADB...");

        // Способ 1: content provider
        log?.Invoke("Способ 1: content provider...");
        var r1 = await ProcessHelper.Adb($"-s {serial} shell content delete --uri content://settings/secure --where \"name='lock_screen_lock_after_timeout'\"");
        log?.Invoke(r1);

        // Способ 2: удаление frp partition
        log?.Invoke("Способ 2: очистка persistent data...");
        var r2 = await ProcessHelper.Adb($"-s {serial} shell settings put global setup_wizard_has_run 1");
        log?.Invoke(r2);

        var r3 = await ProcessHelper.Adb($"-s {serial} shell settings put secure user_setup_complete 1");
        log?.Invoke(r3);

        var r4 = await ProcessHelper.Adb($"-s {serial} shell settings put global device_provisioned 1");
        log?.Invoke(r4);

        log?.Invoke("Готово. Перезагрузите устройство.");
        return "FRP сброс выполнен (ADB)";
    }

    public static async Task<string> RemoveFrpFastboot(Action<string>? log = null)
    {
        log?.Invoke("Сброс FRP через Fastboot...");

        var r1 = await ProcessHelper.Fastboot("erase frp");
        log?.Invoke(r1);

        var r2 = await ProcessHelper.Fastboot("erase config");
        log?.Invoke(r2);

        log?.Invoke("Готово. Перезагрузите устройство.");
        return "FRP сброс выполнен (Fastboot)";
    }
}
