using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class HomePage
{
    private static Gtk.StringList? _devModel;
    private static Gtk.DropDown? _devDrop;
    private static Gtk.Label? _infoLabel;
    private static Action<string>? _log;
    private static System.Collections.Generic.List<DeviceInfo> _devs = new();

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Главная", "Подключение устройств, информация, Wireless ADB");

        // Устройства
        var row = UIHelper.HBox();
        _devModel = Gtk.StringList.New(new[] { "Нет устройств" });
        _devDrop = Gtk.DropDown.New(_devModel, null);
        _devDrop.SetHexpand(true);
        row.Append(_devDrop);

        var refBtn = UIHelper.Btn("🔄 Обновить", "suggested-action");
        refBtn.OnClicked += OnRefresh;
        row.Append(refBtn);
        p.Append(row);

        // Инфо
        _infoLabel = Gtk.Label.New("Подключите устройство и нажмите «Обновить»");
        _infoLabel.SetWrap(true); _infoLabel.SetXalign(0);
        _infoLabel.AddCssClass("card");
        _infoLabel.SetMarginTop(6); _infoLabel.SetMarginBottom(6);
        p.Append(_infoLabel);

        // Кнопки устройства
        var dRow = UIHelper.HBox();
        var detBtn = UIHelper.Btn("📋 Подробнее");
        detBtn.OnClicked += OnDetail;
        dRow.Append(detBtn);

        var batBtn = UIHelper.Btn("🔋 Батарея");
        batBtn.OnClicked += OnBattery;
        dRow.Append(batBtn);

        var wifiBtn = UIHelper.Btn("📶 WiFi ADB (tcpip 5555)");
        wifiBtn.OnClicked += OnWifi;
        dRow.Append(wifiBtn);

        var discBtn = UIHelper.Btn("❌ Disconnect All");
        discBtn.OnClicked += (s, e) => RunAdb("disconnect", "Отключение всех...");
        dRow.Append(discBtn);
        p.Append(dRow);

        // Wireless connect
        p.Append(UIHelper.SectionLabel("Wireless ADB Connect"));
        var wRow = UIHelper.HBox();
        var ipEntry = UIHelper.Entry("IP:PORT (192.168.1.100:5555)");
        wRow.Append(ipEntry);
        var conBtn = UIHelper.Btn("Подключить");
        conBtn.OnClicked += (s, e) =>
        {
            var ip = ipEntry.GetText();
            if (!string.IsNullOrEmpty(ip)) RunAdb($"connect {ip}", $"Подключение к {ip}...");
        };
        wRow.Append(conBtn);
        p.Append(wRow);

        // ADB Pair
        p.Append(UIHelper.SectionLabel("ADB Pair (Android 11+)"));
        var pRow = UIHelper.HBox();
        var pairAddr = UIHelper.Entry("IP:PORT для pair");
        pRow.Append(pairAddr);
        var pairCode = UIHelper.Entry("Код сопряжения");
        pRow.Append(pairCode);
        var pairBtn = UIHelper.Btn("Pair");
        pairBtn.OnClicked += (s, e) =>
        {
            var a = pairAddr.GetText(); var c = pairCode.GetText();
            if (!string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(c))
                RunAdb($"pair {a} {c}", "Сопряжение...");
        };
        pRow.Append(pairBtn);
        p.Append(pRow);

        // ADB Shell
        p.Append(UIHelper.SectionLabel("ADB Shell"));
        var sRow = UIHelper.HBox();
        var shellEntry = UIHelper.Entry("Команда (без adb)");
        sRow.Append(shellEntry);
        var shellBtn = UIHelper.Btn("▶ Run", "suggested-action");
        shellBtn.OnClicked += (s, e) =>
        {
            var cmd = shellEntry.GetText();
            if (!string.IsNullOrEmpty(cmd)) RunAdb(cmd, $"$ adb {cmd}");
        };
        sRow.Append(shellBtn);
        p.Append(sRow);

        var (scroll, append, clear) = UIHelper.LogView();
        _log = append;
        var lh = UIHelper.HBox();
        var ll = Gtk.Label.New("Лог"); ll.SetHexpand(true); ll.SetXalign(0);
        lh.Append(ll);
        var clrBtn = UIHelper.Btn("Очистить");
        clrBtn.OnClicked += (s, e) => clear();
        lh.Append(clrBtn);
        p.Append(lh);
        p.Append(scroll);

        return UIHelper.Scrollable(p);
    }

    static void RunAdb(string cmd, string msg)
    {
        _log?.Invoke(msg);
        Task.Run(async () =>
        {
            var r = await ProcessHelper.Adb(cmd);
            GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
        });
    }

    static void OnRefresh(Gtk.Button b, EventArgs e)
    {
        _log?.Invoke("Поиск устройств...");
        Task.Run(async () =>
        {
            var devs = await DeviceManager.GetDevicesAsync();
            GLib.Functions.IdleAdd(0, () =>
            {
                _devs = devs;
                while (_devModel!.GetNItems() > 0) _devModel.Remove(0);
                if (devs.Count == 0)
                {
                    _devModel.Append("Нет устройств");
                    _infoLabel!.SetText("Не найдено"); _log?.Invoke("Не найдено");
                }
                else
                {
                    foreach (var d in devs)
                    {
                        var n = string.IsNullOrEmpty(d.Model) ? d.Serial : $"{d.Model} ({d.Serial})";
                        _devModel.Append($"{n} [{d.Mode}]");
                    }
                    _infoLabel!.SetText($"Найдено: {devs.Count}");
                    _log?.Invoke($"Найдено: {devs.Count}");
                }
                _devDrop!.SetSelected(0);
                return false;
            });
        });
    }

    static void OnDetail(Gtk.Button b, EventArgs e)
    {
        var i = (int)_devDrop!.GetSelected();
        if (i < 0 || i >= _devs.Count) { _log?.Invoke("Выберите устройство"); return; }
        var d = _devs[i];
        if (d.Mode != "ADB") { _log?.Invoke("Только для ADB"); return; }
        _log?.Invoke("Получаю инфо...");
        Task.Run(async () =>
        {
            var props = await DeviceManager.GetPropsAsync(d.Serial);
            GLib.Functions.IdleAdd(0, () =>
            {
                var g = (string key) => { string v; return props.TryGetValue(key, out v) ? v : "—"; };
                _infoLabel!.SetText(
                    $"Бренд: {g("ro.product.brand")}\n"
                    + $"Модель: {g("ro.product.model")}\n"
                    + $"Устройство: {g("ro.product.device")}\n"
                    + $"Android: {g("ro.build.version.release")} (SDK {g("ro.build.version.sdk")})\n"
                    + $"CPU: {g("ro.product.cpu.abi")}\n"
                    + $"Сборка: {g("ro.build.display.id")}\n"
                    + $"Патч: {g("ro.build.version.security_patch")}\n"
                    + $"Hardware: {g("ro.hardware")}\n"
                    + $"Слот: {g("ro.boot.slot_suffix")}");
                _log?.Invoke("Готово");
                return false;
            });
        });
    }

    static void OnBattery(Gtk.Button b, EventArgs e)
    {
        var i = (int)_devDrop!.GetSelected();
        if (i < 0 || i >= _devs.Count || _devs[i].Mode != "ADB") return;
        Task.Run(async () =>
        {
            var r = await DeviceManager.GetBatteryAsync(_devs[i].Serial);
            GLib.Functions.IdleAdd(0, () => { _log?.Invoke(r); return false; });
        });
    }

    static void OnWifi(Gtk.Button b, EventArgs e)
    {
        var i = (int)_devDrop!.GetSelected();
        if (i < 0 || i >= _devs.Count || _devs[i].Mode != "ADB") return;
        RunAdb($"-s {_devs[i].Serial} tcpip 5555", "WiFi ADB включён на 5555");
    }
}
