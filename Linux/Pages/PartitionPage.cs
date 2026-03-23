using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class PartitionPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Разделы", "Просмотр и управление разделами");

        var row = UIHelper.HBox();
        var btns = new[] {
            ("By-name", "shell ls -la /dev/block/by-name/"),
            ("df -h", "shell df -h"),
            ("mount", "shell mount"),
            ("Slot info", "shell getprop ro.boot.slot_suffix"),
            ("Super partition", "shell ls -la /dev/block/by-name/super"),
        };
        foreach (var (n, c) in btns)
        {
            var b = UIHelper.Btn(n);
            var cmd = c;
            b.OnClicked += (s, e) => RunAdb(cmd, $"{n}...");
            row.Append(b);
        }
        p.Append(row);

        // Fastboot partitions
        p.Append(UIHelper.SectionLabel("Fastboot"));
        var fr = UIHelper.HBox();
        var fbGetPart = UIHelper.Btn("getvar partition-type:boot");
        fbGetPart.OnClicked += (s, e) => RunFb("getvar partition-type:boot", "...");
        fr.Append(fbGetPart);
        var fbSlot = UIHelper.Btn("getvar current-slot");
        fbSlot.OnClicked += (s, e) => RunFb("getvar current-slot", "...");
        fr.Append(fbSlot);
        p.Append(fr);

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }

    static void RunAdb(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Adb(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
    static void RunFb(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Fastboot(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
}
