using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class OthersPage
{
    private static Action<string>? _log;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Другое", "Дополнительные утилиты и информация");

        var row1 = UIHelper.HBox();
        var btns1 = new[] {
            ("📸 Скриншот", new Action(() => {
                _log?.Invoke("Скриншот...");
                Task.Run(async () => {
                    await ProcessHelper.Adb("shell screencap /sdcard/_scr.png");
                    var r = await ProcessHelper.Adb("pull /sdcard/_scr.png /tmp/screenshot.png");
                    await ProcessHelper.Adb("shell rm /sdcard/_scr.png");
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r + "\n-> /tmp/screenshot.png"); return false; });
                });
            })),
            ("📋 Logcat (50)", new Action(() => RunAdb("logcat -d -t 50", "Logcat..."))),
            ("🐧 dmesg", new Action(() => RunAdb("shell dmesg | tail -80", "dmesg..."))),
            ("🔌 lsusb", new Action(() => { _log?.Invoke("lsusb..."); Task.Run(async () => { var r = await ProcessHelper.Shell("lsusb"); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); })),
            ("📊 top (snapshot)", new Action(() => RunAdb("shell top -n 1 -b | head -30", "top..."))),
        };
        foreach (var (name, action) in btns1)
        {
            var b = UIHelper.Btn(name);
            var a = action;
            b.OnClicked += (s, e) => a();
            row1.Append(b);
        }
        p.Append(row1);

        var row2 = UIHelper.HBox();
        var btns2 = new[] {
            ("🌐 IP адрес", new Action(() => RunAdb("shell ip addr show wlan0", "IP..."))),
            ("💾 Память", new Action(() => RunAdb("shell cat /proc/meminfo | head -5", "Память..."))),
            ("🖥 CPU", new Action(() => RunAdb("shell cat /proc/cpuinfo | head -20", "CPU..."))),
            ("📱 Screen size", new Action(() => RunAdb("shell wm size", "Screen..."))),
            ("🔊 Volume", new Action(() => RunAdb("shell media volume --show", "Volume..."))),
        };
        foreach (var (name, action) in btns2)
        {
            var b = UIHelper.Btn(name);
            var a = action;
            b.OnClicked += (s, e) => a();
            row2.Append(b);
        }
        p.Append(row2);

        // Shell
        p.Append(UIHelper.SectionLabel("Произвольная команда"));
        var sRow = UIHelper.HBox();
        var shellEntry = UIHelper.Entry("Команда (выполнится в терминале)");
        sRow.Append(shellEntry);
        var shellBtn = UIHelper.Btn("▶ Run");
        shellBtn.OnClicked += (s, e) =>
        {
            var cmd = shellEntry.GetText();
            if (string.IsNullOrEmpty(cmd)) return;
            _log?.Invoke($"$ {cmd}");
            var parts = cmd.Split(' ', 2);
            Task.Run(async () => {
                var r = await ProcessHelper.Shell(parts[0], parts.Length > 1 ? parts[1] : "");
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
            });
        };
        sRow.Append(shellBtn);
        p.Append(sRow);

        var (scroll, append, clear) = UIHelper.LogView();
        _log = append;
        var lh = UIHelper.HBox();
        lh.Append(UIHelper.SectionLabel("Вывод"));
        var clrBtn = UIHelper.Btn("Очистить");
        clrBtn.OnClicked += (s, e) => clear();
        lh.Append(clrBtn);
        p.Append(lh);
        p.Append(scroll);
        return UIHelper.Scrollable(p);
    }

    static void RunAdb(string c, string m) { _log?.Invoke(m); Task.Run(async () => { var r = await ProcessHelper.Adb(c); GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; }); }); }
}
