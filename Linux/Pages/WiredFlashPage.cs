using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class WiredFlashPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Sideload / Push / Pull", "ADB Sideload, Push и Pull файлов");

        // Sideload
        p.Append(UIHelper.SectionLabel("ADB Sideload"));
        var sRow = UIHelper.HBox();
        var zipEntry = UIHelper.Entry("Путь к ZIP");
        sRow.Append(zipEntry);
        var sideBtn = UIHelper.Btn("Sideload", "suggested-action");
        sideBtn.OnClicked += (s, e) =>
        {
            var f = zipEntry.GetText();
            if (!string.IsNullOrEmpty(f)) RunAdb($"sideload \"{f}\"", $"Sideload {f}...");
        };
        sRow.Append(sideBtn);
        p.Append(sRow);

        // Push
        p.Append(UIHelper.SectionLabel("Push файл"));
        var pRow = UIHelper.HBox();
        var srcEntry = UIHelper.Entry("Локальный файл");
        pRow.Append(srcEntry);
        var dstEntry = UIHelper.Entry("Путь на устройстве (/sdcard/)");
        pRow.Append(dstEntry);
        var pushBtn = UIHelper.Btn("Push");
        pushBtn.OnClicked += (s, e) =>
        {
            var src = srcEntry.GetText(); var dst = dstEntry.GetText();
            if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
                RunAdb($"push \"{src}\" \"{dst}\"", $"Push...");
        };
        pRow.Append(pushBtn);
        p.Append(pRow);

        // Pull
        p.Append(UIHelper.SectionLabel("Pull файл"));
        var prRow = UIHelper.HBox();
        var remEntry = UIHelper.Entry("Путь на устройстве");
        prRow.Append(remEntry);
        var locEntry = UIHelper.Entry("Локальный путь");
        prRow.Append(locEntry);
        var pullBtn = UIHelper.Btn("Pull");
        pullBtn.OnClicked += (s, e) =>
        {
            var rem = remEntry.GetText(); var loc = locEntry.GetText();
            if (!string.IsNullOrEmpty(rem) && !string.IsNullOrEmpty(loc))
                RunAdb($"pull \"{rem}\" \"{loc}\"", $"Pull...");
        };
        prRow.Append(pullBtn);
        p.Append(prRow);

        // Install APK
        p.Append(UIHelper.SectionLabel("Установка APK"));
        var iRow = UIHelper.HBox();
        var apkEntry = UIHelper.Entry("Путь к APK");
        iRow.Append(apkEntry);
        var instBtn = UIHelper.Btn("Install", "suggested-action");
        instBtn.OnClicked += (s, e) =>
        {
            var f = apkEntry.GetText();
            if (!string.IsNullOrEmpty(f)) RunAdb($"install \"{f}\"", $"Install...");
        };
        iRow.Append(instBtn);
        p.Append(iRow);

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
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
}
