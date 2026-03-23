using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class FormatExtractPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Формат / Извлечение", "Форматирование и извлечение разделов");

        p.Append(UIHelper.SectionLabel("Fastboot Format/Erase"));
        var partEntry = UIHelper.Entry("Раздел (data, cache, system...)");
        p.Append(partEntry);

        var row = UIHelper.HBox();
        var eraseBtn = UIHelper.Btn("Erase", "destructive-action");
        eraseBtn.OnClicked += (s, e) => { var pt = partEntry.GetText(); if (!string.IsNullOrEmpty(pt)) RunFb($"erase {pt}", $"Erase {pt}..."); };
        row.Append(eraseBtn);

        var wipeBtn = UIHelper.Btn("Wipe Data (-w)", "destructive-action");
        wipeBtn.OnClicked += (s, e) => RunFb("-w", "Wipe...");
        row.Append(wipeBtn);

        var formatBtn = UIHelper.Btn("Format Data (ADB)", "destructive-action");
        formatBtn.OnClicked += (s, e) => RunAdb("shell recovery --wipe_data", "Format data...");
        row.Append(formatBtn);
        p.Append(row);

        // Извлечение
        p.Append(UIHelper.SectionLabel("Извлечение раздела (ADB dd + pull)"));
        var exRow = UIHelper.HBox();
        var exPart = UIHelper.Entry("Раздел (boot, recovery...)");
        exRow.Append(exPart);
        var exSave = UIHelper.Entry("Куда сохранить (/tmp/boot.img)");
        exRow.Append(exSave);
        var exBtn = UIHelper.Btn("Извлечь", "suggested-action");
        exBtn.OnClicked += (s, e) =>
        {
            var pt = exPart.GetText(); var sv = exSave.GetText();
            if (string.IsNullOrEmpty(pt) || string.IsNullOrEmpty(sv)) { _log?.Invoke("Заполните поля"); return; }
            _log?.Invoke($"Извлечение {pt}...");
            Task.Run(async () => {
                await ProcessHelper.Adb($"shell dd if=/dev/block/by-name/{pt} of=/sdcard/_extract_{pt}.img");
                var r = await ProcessHelper.Adb($"pull /sdcard/_extract_{pt}.img \"{sv}\"");
                await ProcessHelper.Adb($"shell rm /sdcard/_extract_{pt}.img");
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); _log?.Invoke("Готово!"); return false; });
            });
        };
        exRow.Append(exBtn);
        p.Append(exRow);

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }

    static void RunFb(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Fastboot(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
    static void RunAdb(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Adb(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
}
