using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class CustomFlashPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Своя прошивка", "Flash с disable-verity/verification, vbmeta");

        var partEntry = UIHelper.Entry("Раздел");
        p.Append(partEntry);
        var fileEntry = UIHelper.Entry("Путь к образу");
        p.Append(fileEntry);

        var optBox = UIHelper.HBox(12);
        var chkVerity = Gtk.CheckButton.NewWithLabel("--disable-verity");
        var chkVerification = Gtk.CheckButton.NewWithLabel("--disable-verification");
        optBox.Append(chkVerity); optBox.Append(chkVerification);
        p.Append(optBox);

        var flashBtn = UIHelper.Btn("Flash", "destructive-action");
        flashBtn.OnClicked += (s, e) =>
        {
            var part = partEntry.GetText(); var file = fileEntry.GetText();
            if (string.IsNullOrEmpty(part) || string.IsNullOrEmpty(file)) { _log?.Invoke("Заполните поля"); return; }
            var extra = "";
            if (chkVerity.GetActive()) extra += " --disable-verity";
            if (chkVerification.GetActive()) extra += " --disable-verification";
            Run($"flash{extra} {part} \"{file}\"", $"Flash {part}{extra}...");
        };
        p.Append(flashBtn);

        // vbmeta
        p.Append(UIHelper.SectionLabel("Flash vbmeta (отключить AVB)"));
        var vbEntry = UIHelper.Entry("Путь к vbmeta.img");
        p.Append(vbEntry);
        var vbRow = UIHelper.HBox();
        var vbBtn = UIHelper.Btn("Flash vbmeta (disabled)", "destructive-action");
        vbBtn.OnClicked += (s, e) =>
        {
            var f = vbEntry.GetText();
            if (!string.IsNullOrEmpty(f)) Run($"flash --disable-verity --disable-verification vbmeta \"{f}\"", "Flash vbmeta...");
        };
        vbRow.Append(vbBtn);
        var vbBothBtn = UIHelper.Btn("Flash vbmeta_a + vbmeta_b", "destructive-action");
        vbBothBtn.OnClicked += (s, e) =>
        {
            var f = vbEntry.GetText();
            if (string.IsNullOrEmpty(f)) return;
            _log?.Invoke("Flash vbmeta_a + vbmeta_b...");
            Task.Run(async () => {
                var r1 = await ProcessHelper.Fastboot($"flash --disable-verity --disable-verification vbmeta_a \"{f}\"");
                var r2 = await ProcessHelper.Fastboot($"flash --disable-verity --disable-verification vbmeta_b \"{f}\"");
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r1); _log?.Invoke(r2); return false; });
            });
        };
        vbRow.Append(vbBothBtn);
        p.Append(vbRow);

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
