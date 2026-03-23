using System;
using System.Threading.Tasks;
using LIAF.Common;

namespace LIAF.Pages;

public static class FirmwarePage
{
    private static Action<string>? _log;
    private static Gtk.Box? _resultsBox;

    public static Gtk.Widget Create()
    {
        var p = UIHelper.Page("Прошивки Xiaomi", "Поиск прошивок и загрузка через быстрое зеркало Aliyun");

        var searchRow = UIHelper.HBox();
        var codeEntry = UIHelper.Entry("Кодовое имя (например: tucana, sweet, alioth)");
        searchRow.Append(codeEntry);

        var searchBtn = UIHelper.Btn("Поиск", "suggested-action");
        searchBtn.OnClicked += (s, e) =>
        {
            var code = codeEntry.GetText()?.Trim();
            if (string.IsNullOrEmpty(code)) { _log?.Invoke("Введите кодовое имя устройства"); return; }
            
            // Очищаем старые результаты
            while (_resultsBox!.GetFirstChild() != null)
                _resultsBox.Remove(_resultsBox.GetFirstChild()!);

            Task.Run(async () =>
            {
                var roms = await FirmwareHelper.GetFirmwaresAsync(code, msg =>
                    GLib.Functions.IdleAdd(0, () => { _log?.Invoke(msg); return false; }));

                GLib.Functions.IdleAdd(0, () =>
                {
                    if (roms.Count == 0)
                    {
                        var lbl = Gtk.Label.New("Прошивки не найдены.");
                        _resultsBox.Append(lbl);
                    }
                    else
                    {
                        foreach (var rom in roms)
                        {
                            var romRow = UIHelper.HBox();
                            romRow.SetMarginTop(4); romRow.SetMarginBottom(4);
                            
                            var titleLbl = Gtk.Label.New(rom.Title);
                            titleLbl.SetXalign(0);
                            titleLbl.SetHexpand(true);
                            titleLbl.SetWrap(true);
                            romRow.Append(titleLbl);

                            var dlBtn = UIHelper.Btn("Скачать");
                            var link = rom.MirrorLink;
                            dlBtn.OnClicked += (sender, args) =>
                            {
                                _log?.Invoke($"Открытие ссылки: {link}");
                                // Открываем ссылку в браузере по умолчанию на Linux
                                Task.Run(() => ProcessHelper.RunAsync("xdg-open", $"\"{link}\""));
                            };
                            romRow.Append(dlBtn);
                            
                            _resultsBox.Append(romRow);
                            _resultsBox.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));
                        }
                    }
                    return false;
                });
            });
        };
        searchRow.Append(searchBtn);
        p.Append(searchRow);

        p.Append(UIHelper.SectionLabel("Результаты"));
        _resultsBox = Gtk.Box.New(Gtk.Orientation.Vertical, 6);
        
        // Оборачиваем результаты в скролл, чтобы страница не растягивалась бесконечно
        var resultsScroll = Gtk.ScrolledWindow.New();
        resultsScroll.SetChild(_resultsBox);
        resultsScroll.SetMinContentHeight(200);
        resultsScroll.SetMaxContentHeight(300);
        resultsScroll.AddCssClass("card");
        p.Append(resultsScroll);

        var (logScroll, append, clear) = UIHelper.LogView(120);
        _log = append;
        p.Append(UIHelper.SectionLabel("Лог"));
        p.Append(logScroll);

        return UIHelper.Scrollable(p);
    }
}
