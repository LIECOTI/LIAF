using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class SettingsPage
{
    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Настройки", "Конфигурация LIAF");

        p.Append(UIHelper.SectionLabel("Пути к утилитам"));
        var adbLabel = Gtk.Label.New("ADB:"); adbLabel.SetXalign(0);
        p.Append(adbLabel);
        var adbEntry = UIHelper.Entry("adb");
        adbEntry.SetText(ProcessHelper.AdbPath);
        p.Append(adbEntry);

        var fbLabel = Gtk.Label.New("Fastboot:"); fbLabel.SetXalign(0);
        p.Append(fbLabel);
        var fbEntry = UIHelper.Entry("fastboot");
        fbEntry.SetText(ProcessHelper.FastbootPath);
        p.Append(fbEntry);

        var saveBtn = UIHelper.Btn("💾 Сохранить", "suggested-action");
        saveBtn.OnClicked += (s, e) =>
        {
            ProcessHelper.AdbPath = adbEntry.GetText() ?? "adb";
            ProcessHelper.FastbootPath = fbEntry.GetText() ?? "fastboot";
        };
        p.Append(saveBtn);

        // Тест
        p.Append(UIHelper.SectionLabel("Проверка"));
        var testRow = UIHelper.HBox();
        var testLabel = Gtk.Label.New(""); testLabel.SetWrap(true); testLabel.SetXalign(0); testLabel.SetHexpand(true);
        testRow.Append(testLabel);
        var testBtn = UIHelper.Btn("Проверить ADB/Fastboot");
        testBtn.OnClicked += (s, e) =>
        {
            Task.Run(async () =>
            {
                var a = await ProcessHelper.Adb("version");
                var f = await ProcessHelper.Fastboot("--version");
                GLib.Functions.IdleAdd(0, () => { testLabel.SetText($"ADB:\n{a}\n\nFastboot:\n{f}"); return false; });
            });
        };
        testRow.Append(testBtn);
        p.Append(testRow);

        // ADB server
        p.Append(UIHelper.SectionLabel("ADB Server"));
        var srvRow = UIHelper.HBox();
        var startSrv = UIHelper.Btn("Start server");
        startSrv.OnClicked += (s, e) => Task.Run(async () => { await ProcessHelper.Adb("start-server"); });
        srvRow.Append(startSrv);
        var killSrv = UIHelper.Btn("Kill server", "destructive-action");
        killSrv.OnClicked += (s, e) => Task.Run(async () => { await ProcessHelper.Adb("kill-server"); });
        srvRow.Append(killSrv);
        p.Append(srvRow);

        p.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));

        var about = Gtk.Label.New("LIAF — Linux Android Flash Tool\nВерсия 0.1.0\nАналог Uotan Toolbox\n\nАвтор: liecoti");
        about.SetWrap(true); about.SetXalign(0); about.AddCssClass("dim-label");
        p.Append(about);

        return UIHelper.Scrollable(p);
    }
}
