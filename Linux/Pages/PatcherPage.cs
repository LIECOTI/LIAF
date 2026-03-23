using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class PatcherPage
{
    private static Action<string>? _log;
    private static Action? _clear;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Патчер", "Magisk патч boot.img, извлечение payload.bin, FRP сброс");

        // === MAGISK ===
        p.Append(UIHelper.SectionLabel("Magisk Patch boot.img"));

        var bootRow = UIHelper.HBox();
        var bootEntry = UIHelper.Entry("Путь к boot.img");
        bootRow.Append(bootEntry);
        p.Append(bootRow);

        var magiskRow = UIHelper.HBox();
        var magiskEntry = UIHelper.Entry("Путь к Magisk APK (опционально)");
        magiskRow.Append(magiskEntry);
        p.Append(magiskRow);

        var magiskBtnRow = UIHelper.HBox();
        var patchBtn = UIHelper.Btn("🔧 Patch boot.img", "suggested-action");
        patchBtn.OnClicked += (s, e) =>
        {
            var boot = bootEntry.GetText();
            if (string.IsNullOrEmpty(boot)) { _log?.Invoke("Укажите путь к boot.img"); return; }
            var magisk = magiskEntry.GetText();
            _log?.Invoke("Начинаю патч...");
            Task.Run(async () =>
            {
                var result = await MagiskPatcher.PatchBootImage(boot, magisk ?? "", msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke($"Результат: {result}"); return false; });
            });
        };
        magiskBtnRow.Append(patchBtn);

        var infoBtn = UIHelper.Btn("ℹ️ Info boot.img");
        infoBtn.OnClicked += (s, e) =>
        {
            var boot = bootEntry.GetText();
            if (string.IsNullOrEmpty(boot)) { _log?.Invoke("Укажите путь к boot.img"); return; }
            Task.Run(async () =>
            {
                var result = await MagiskPatcher.GetBootImageInfo(boot, msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke(result); return false; });
            });
        };
        magiskBtnRow.Append(infoBtn);
        p.Append(magiskBtnRow);

        // === PAYLOAD ===
        p.Append(UIHelper.SectionLabel("Payload.bin — извлечение разделов"));

        var payRow = UIHelper.HBox();
        var payEntry = UIHelper.Entry("Путь к payload.bin");
        payRow.Append(payEntry);
        p.Append(payRow);

        var outRow = UIHelper.HBox();
        var outEntry = UIHelper.Entry("Папка для извлечения (/tmp/payload_out)");
        outEntry.SetText("/tmp/payload_out");
        outRow.Append(outEntry);
        p.Append(outRow);

        var payBtnRow = UIHelper.HBox();

        var listBtn = UIHelper.Btn("📋 Список разделов");
        listBtn.OnClicked += (s, e) =>
        {
            var pay = payEntry.GetText();
            if (string.IsNullOrEmpty(pay)) { _log?.Invoke("Укажите путь к payload.bin"); return; }
            Task.Run(async () =>
            {
                var parts = await PayloadExtractor.ListPartitions(pay, msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
                GLib.Functions.IdleAdd(0, () =>
                {
                    _log?.Invoke($"Найдено разделов: {parts.Count}");
                    foreach (var pt in parts) _log?.Invoke($"  {pt}");
                    return false;
                });
            });
        };
        payBtnRow.Append(listBtn);

        var extractAllBtn = UIHelper.Btn("📦 Извлечь все", "suggested-action");
        extractAllBtn.OnClicked += (s, e) =>
        {
            var pay = payEntry.GetText(); var outDir = outEntry.GetText();
            if (string.IsNullOrEmpty(pay)) { _log?.Invoke("Укажите путь"); return; }
            Task.Run(async () =>
            {
                var result = await PayloadExtractor.ExtractAll(pay, outDir ?? "/tmp/payload_out", msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke($"Результат: {result}"); return false; });
            });
        };
        payBtnRow.Append(extractAllBtn);
        p.Append(payBtnRow);

        // Извлечение одного раздела
        var singleRow = UIHelper.HBox();
        var partNameEntry = UIHelper.Entry("Имя раздела (boot, system...)");
        singleRow.Append(partNameEntry);
        var extractOneBtn = UIHelper.Btn("Извлечь раздел");
        extractOneBtn.OnClicked += (s, e) =>
        {
            var pay = payEntry.GetText();
            var partName = partNameEntry.GetText();
            var outDir = outEntry.GetText();
            if (string.IsNullOrEmpty(pay) || string.IsNullOrEmpty(partName)) { _log?.Invoke("Заполните поля"); return; }
            Task.Run(async () =>
            {
                var result = await PayloadExtractor.ExtractPartition(pay, partName, outDir ?? "/tmp/payload_out", msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
                GLib.Functions.IdleAdd(0, () => { _log?.Invoke($"Результат: {result}"); return false; });
            });
        };
        singleRow.Append(extractOneBtn);
        p.Append(singleRow);

        // === FRP ===
        p.Append(UIHelper.SectionLabel("FRP сброс"));

        var frpRow = UIHelper.HBox();
        var frpAdbBtn = UIHelper.Btn("Сброс FRP (ADB)", "destructive-action");
        frpAdbBtn.OnClicked += (s, e) =>
        {
            Task.Run(async () =>
            {
                // Берём первое устройство
                var devs = await DeviceManager.GetDevicesAsync();
                if (devs.Count == 0) { GLib.Functions.IdleAdd(0, () => { _log?.Invoke("Нет устройств"); return false; }); return; }
                var serial = devs[0].Serial;
                await FrpHelper.RemoveFrpAdb(serial, msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
            });
        };
        frpRow.Append(frpAdbBtn);

        var frpFbBtn = UIHelper.Btn("Сброс FRP (Fastboot)", "destructive-action");
        frpFbBtn.OnClicked += (s, e) =>
        {
            Task.Run(async () =>
            {
                await FrpHelper.RemoveFrpFastboot(msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));
            });
        };
        frpRow.Append(frpFbBtn);
        p.Append(frpRow);

        // === ЛОГ ===
        var (scroll, append, clear) = UIHelper.LogView();
        _log = append; _clear = clear;
        var lh = UIHelper.HBox();
        lh.Append(UIHelper.SectionLabel("Вывод"));
        var clrBtn = UIHelper.Btn("Очистить");
        clrBtn.OnClicked += (s, e) => _clear?.Invoke();
        lh.Append(clrBtn);
        p.Append(lh);
        p.Append(scroll);

        return UIHelper.Scrollable(p);
    }
}
