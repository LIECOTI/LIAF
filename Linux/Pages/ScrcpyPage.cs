using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class ScrcpyPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Scrcpy", "Зеркалирование экрана устройства");

        var row = UIHelper.HBox();
        var resEntry = UIHelper.Entry("Max size (1024)");
        row.Append(resEntry);
        var brEntry = UIHelper.Entry("Bitrate (8M)");
        row.Append(brEntry);
        var fpsEntry = UIHelper.Entry("Max FPS (60)");
        row.Append(fpsEntry);
        p.Append(row);

        var optBox = UIHelper.HBox(12);
        var chkNoAudio = Gtk.CheckButton.NewWithLabel("--no-audio");
        var chkStayAwake = Gtk.CheckButton.NewWithLabel("--stay-awake");
        var chkFullscreen = Gtk.CheckButton.NewWithLabel("--fullscreen");
        var chkRecord = Gtk.CheckButton.NewWithLabel("--record");
        optBox.Append(chkNoAudio); optBox.Append(chkStayAwake);
        optBox.Append(chkFullscreen); optBox.Append(chkRecord);
        p.Append(optBox);

        var recEntry = UIHelper.Entry("Путь записи (output.mp4)");
        p.Append(recEntry);

        var startBtn = UIHelper.Btn("Запустить Scrcpy", "suggested-action");
        startBtn.OnClicked += (s, e) =>
        {
            var args = "";
            var res = resEntry.GetText(); if (!string.IsNullOrEmpty(res)) args += $" --max-size {res}";
            var br = brEntry.GetText(); if (!string.IsNullOrEmpty(br)) args += $" --video-bit-rate {br}";
            var fps = fpsEntry.GetText(); if (!string.IsNullOrEmpty(fps)) args += $" --max-fps {fps}";
            if (chkNoAudio.GetActive()) args += " --no-audio";
            if (chkStayAwake.GetActive()) args += " --stay-awake";
            if (chkFullscreen.GetActive()) args += " --fullscreen";
            if (chkRecord.GetActive())
            {
                var rec = recEntry.GetText();
                if (!string.IsNullOrEmpty(rec)) args += $" --record \"{rec}\"";
            }
            _log?.Invoke($"scrcpy{args}");
            Task.Run(async () => {
                var r = await ProcessHelper.Scrcpy(args.Trim());
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
            });
        };
        p.Append(startBtn);

        var (scroll, append, _) = UIHelper.LogView();
        _log = append;
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }
}
