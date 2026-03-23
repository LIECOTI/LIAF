using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class BasicFlashPage
{
    private static Action<string>? _log;
    private static Action? _clear;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Базовая прошивка", "Перезагрузка, bootloader, быстрые команды");

        // ADB Reboot
        p.Append(UIHelper.SectionLabel("Перезагрузка (ADB)"));
        var ar = UIHelper.HBox();
        foreach (var (name, cmd) in new[] {
            ("System", "reboot"), ("Recovery", "reboot recovery"),
            ("Bootloader", "reboot bootloader"), ("Fastboot", "reboot fastboot"),
            ("EDL", "reboot edl"), ("Sideload", "reboot sideload"),
            ("Выключить", "shell reboot -p") })
        {
            var b = UIHelper.Btn(name);
            var c = cmd;
            b.OnClicked += (s, e) => RunAdb(c, $"ADB: {c}");
            ar.Append(b);
        }
        p.Append(ar);

        // Fastboot Reboot
        p.Append(UIHelper.SectionLabel("Перезагрузка (Fastboot)"));
        var fr = UIHelper.HBox();
        foreach (var (name, cmd) in new[] {
            ("System", "reboot"), ("Bootloader", "reboot-bootloader"),
            ("Fastbootd", "reboot-fastboot"), ("Recovery", "reboot-recovery"),
            ("EDL", "oem edl") })
        {
            var b = UIHelper.Btn(name);
            var c = cmd;
            b.OnClicked += (s, e) => RunFb(c, $"Fastboot: {c}");
            fr.Append(b);
        }
        p.Append(fr);

        // Bootloader
        p.Append(UIHelper.SectionLabel("Bootloader"));
        var br = UIHelper.HBox();
        var unlockBtn = UIHelper.Btn("🔓 Unlock", "destructive-action");
        unlockBtn.OnClicked += (s, e) => RunFb("flashing unlock", "UNLOCK BOOTLOADER...");
        br.Append(unlockBtn);
        var lockBtn = UIHelper.Btn("🔒 Lock", "destructive-action");
        lockBtn.OnClicked += (s, e) => RunFb("flashing lock", "LOCK BOOTLOADER...");
        br.Append(lockBtn);
        var critUnlock = UIHelper.Btn("Unlock Critical", "destructive-action");
        critUnlock.OnClicked += (s, e) => RunFb("flashing unlock_critical", "Unlock critical...");
        br.Append(critUnlock);
        var getvar = UIHelper.Btn("Getvar All");
        getvar.OnClicked += (s, e) => RunFb("getvar all", "Getvar all...");
        br.Append(getvar);
        p.Append(br);

        // Anti-rollback
        p.Append(UIHelper.SectionLabel("Информация"));
        var ir = UIHelper.HBox();
        var arbBtn = UIHelper.Btn("Anti-rollback");
        arbBtn.OnClicked += (s, e) => RunFb("getvar anti", "Anti-rollback info...");
        ir.Append(arbBtn);
        var slotBtn = UIHelper.Btn("Current Slot");
        slotBtn.OnClicked += (s, e) => RunFb("getvar current-slot", "Current slot...");
        ir.Append(slotBtn);
        var unlockStateBtn = UIHelper.Btn("Unlock State");
        unlockStateBtn.OnClicked += (s, e) => RunFb("getvar unlocked", "Unlock state...");
        ir.Append(unlockStateBtn);
        p.Append(ir);

        // ADB cmd
        p.Append(UIHelper.SectionLabel("ADB команда"));
        var ac = UIHelper.HBox();
        var aEntry = UIHelper.Entry("Команда (без adb)");
        ac.Append(aEntry);
        var aRun = UIHelper.Btn("▶", "suggested-action");
        aRun.OnClicked += (s, e) => { var c = aEntry.GetText(); if (!string.IsNullOrEmpty(c)) RunAdb(c, $"$ adb {c}"); };
        ac.Append(aRun);
        p.Append(ac);

        // Fastboot cmd
        p.Append(UIHelper.SectionLabel("Fastboot команда"));
        var fc = UIHelper.HBox();
        var fEntry = UIHelper.Entry("Команда (без fastboot)");
        fc.Append(fEntry);
        var fRun = UIHelper.Btn("▶", "suggested-action");
        fRun.OnClicked += (s, e) => { var c = fEntry.GetText(); if (!string.IsNullOrEmpty(c)) RunFb(c, $"$ fastboot {c}"); };
        fc.Append(fRun);
        p.Append(fc);

        // Log
        var (scroll, append, clear) = UIHelper.LogView();
        _log = append; _clear = clear;
        var lh = UIHelper.HBox();
        lh.Append(UIHelper.SectionLabel("Вывод"));
        var clr = UIHelper.Btn("Очистить");
        clr.OnClicked += (s, e) => _clear?.Invoke();
        lh.Append(clr);
        p.Append(lh);
        p.Append(scroll);

        return UIHelper.Scrollable(p);
    }

    static void RunAdb(string cmd, string msg)
    {
        _log?.Invoke(msg);
        Task.Run(async () => {
            var r = await ProcessHelper.Adb(cmd);
            GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
        });
    }
    static void RunFb(string cmd, string msg)
    {
        _log?.Invoke(msg);
        Task.Run(async () => {
            var r = await ProcessHelper.Fastboot(cmd);
            GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
        });
    }
}
