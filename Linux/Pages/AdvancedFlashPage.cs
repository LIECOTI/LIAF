using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class AdvancedFlashPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Расширенная прошивка", "Flash/Boot/Erase разделов через Fastboot");

        var partEntry = UIHelper.Entry("Раздел (boot, recovery, system, vendor...)");
        p.Append(partEntry);
        var fileEntry = UIHelper.Entry("Путь к .img файлу");
        p.Append(fileEntry);

        var row = UIHelper.HBox();
        var flashBtn = UIHelper.Btn("Flash", "destructive-action");
        flashBtn.OnClicked += (s, e) =>
        {
            var part = partEntry.GetText(); var file = fileEntry.GetText();
            if (string.IsNullOrEmpty(part) || string.IsNullOrEmpty(file)) { _log?.Invoke("Заполните поля"); return; }
            Run($"flash {part} \"{file}\"", $"Flash {part}...");
        };
        row.Append(flashBtn);

        var bootBtn = UIHelper.Btn("Boot");
        bootBtn.OnClicked += (s, e) =>
        {
            var file = fileEntry.GetText();
            if (!string.IsNullOrEmpty(file)) Run($"boot \"{file}\"", "Boot...");
        };
        row.Append(bootBtn);

        var eraseBtn = UIHelper.Btn("Erase", "destructive-action");
        eraseBtn.OnClicked += (s, e) =>
        {
            var part = partEntry.GetText();
            if (!string.IsNullOrEmpty(part)) Run($"erase {part}", $"Erase {part}...");
        };
        row.Append(eraseBtn);

        var setActiveBtn = UIHelper.Btn("Set Active Slot");
        setActiveBtn.OnClicked += (s, e) =>
        {
            var part = partEntry.GetText();
            if (!string.IsNullOrEmpty(part)) Run($"--set-active={part}", $"Set active: {part}");
        };
        row.Append(setActiveBtn);
        p.Append(row);

        // Быстрые разделы
        p.Append(UIHelper.SectionLabel("Быстрый Flash"));
        foreach (var partName in new[] { "boot", "recovery", "vbmeta", "dtbo", "vendor_boot", "init_boot", "super" })
        {
            var qr = UIHelper.HBox();
            var ql = Gtk.Label.New(partName);
            ql.SetXalign(0); ql.SetHexpand(true);
            qr.Append(ql);
            var qf = UIHelper.Entry($"Путь к {partName}.img");
            qr.Append(qf);
            var qb = UIHelper.Btn("Flash");
            var pn = partName;
            qb.OnClicked += (s, e) =>
            {
                var f = qf.GetText();
                if (!string.IsNullOrEmpty(f)) Run($"flash {pn} \"{f}\"", $"Flash {pn}...");
            };
            qr.Append(qb);
            p.Append(qr);
        }

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }

    static void Run(string cmd, string msg)
    {
        _log?.Invoke(msg);
        Task.Run(async () => {
            var r = await ProcessHelper.Fastboot(cmd);
            GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
        });
    }
}
