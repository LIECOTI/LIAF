using System;

namespace LIAF.Common;

public static class FilePickerHelper
{
    public static void PickFile(Gtk.Window parent, string title, Action<string> onSelected)
    {
        var chooser = Gtk.FileChooserNative.New(title, parent, Gtk.FileChooserAction.Open, "Открыть", "Отмена");
        chooser.OnResponse += (sender, args) =>
        {
            if (args.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var file = chooser.GetFile();
                var path = file?.GetPath();
                if (path != null) onSelected(path);
            }
            chooser.Destroy();
        };
        chooser.Show();
    }

    public static void PickFolder(Gtk.Window parent, string title, Action<string> onSelected)
    {
        var chooser = Gtk.FileChooserNative.New(title, parent, Gtk.FileChooserAction.SelectFolder, "Выбрать", "Отмена");
        chooser.OnResponse += (sender, args) =>
        {
            if (args.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var file = chooser.GetFile();
                var path = file?.GetPath();
                if (path != null) onSelected(path);
            }
            chooser.Destroy();
        };
        chooser.Show();
    }
}
