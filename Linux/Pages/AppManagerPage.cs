using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class AppManagerPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Приложения", "Управление приложениями на устройстве");

        var row = UIHelper.HBox();
        var listAll = UIHelper.Btn("Все пакеты");
        listAll.OnClicked += (s, e) => Run("shell pm list packages", "Все пакеты...");
        row.Append(listAll);

        var list3rd = UIHelper.Btn("Сторонние (-3)");
        list3rd.OnClicked += (s, e) => Run("shell pm list packages -3", "Сторонние...");
        row.Append(list3rd);

        var listSys = UIHelper.Btn("Системные (-s)");
        listSys.OnClicked += (s, e) => Run("shell pm list packages -s", "Системные...");
        row.Append(listSys);

        var listDisabled = UIHelper.Btn("Отключённые (-d)");
        listDisabled.OnClicked += (s, e) => Run("shell pm list packages -d", "Отключённые...");
        row.Append(listDisabled);
        p.Append(row);

        // Установка
        p.Append(UIHelper.SectionLabel("Установка"));
        var iRow = UIHelper.HBox();
        var apkEntry = UIHelper.Entry("Путь к APK");
        iRow.Append(apkEntry);
        var instBtn = UIHelper.Btn("Install", "suggested-action");
        instBtn.OnClicked += (s, e) => { var f = apkEntry.GetText(); if (!string.IsNullOrEmpty(f)) Run($"install \"{f}\"", $"Install..."); };
        iRow.Append(instBtn);
        var instRBtn = UIHelper.Btn("Install -r (replace)");
        instRBtn.OnClicked += (s, e) => { var f = apkEntry.GetText(); if (!string.IsNullOrEmpty(f)) Run($"install -r \"{f}\"", $"Install -r..."); };
        iRow.Append(instRBtn);
        p.Append(iRow);

        // Удаление
        p.Append(UIHelper.SectionLabel("Удаление / Управление"));
        var uRow = UIHelper.HBox();
        var pkgEntry = UIHelper.Entry("Имя пакета (com.example.app)");
        uRow.Append(pkgEntry);
        var uninstBtn = UIHelper.Btn("Uninstall", "destructive-action");
        uninstBtn.OnClicked += (s, e) => { var pk = pkgEntry.GetText(); if (!string.IsNullOrEmpty(pk)) Run($"uninstall {pk}", $"Uninstall {pk}..."); };
        uRow.Append(uninstBtn);
        var disBtn = UIHelper.Btn("Disable");
        disBtn.OnClicked += (s, e) => { var pk = pkgEntry.GetText(); if (!string.IsNullOrEmpty(pk)) Run($"shell pm disable-user --user 0 {pk}", $"Disable..."); };
        uRow.Append(disBtn);
        var enBtn = UIHelper.Btn("Enable");
        enBtn.OnClicked += (s, e) => { var pk = pkgEntry.GetText(); if (!string.IsNullOrEmpty(pk)) Run($"shell pm enable {pk}", $"Enable..."); };
        uRow.Append(enBtn);
        var clrBtn = UIHelper.Btn("Clear Data");
        clrBtn.OnClicked += (s, e) => { var pk = pkgEntry.GetText(); if (!string.IsNullOrEmpty(pk)) Run($"shell pm clear {pk}", $"Clear..."); };
        uRow.Append(clrBtn);
        p.Append(uRow);

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }

    static void Run(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Adb(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
}
