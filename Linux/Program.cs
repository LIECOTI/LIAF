using System;

namespace LIAF;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = Adw.Application.New("com.liaf.toolbox", Gio.ApplicationFlags.FlagsNone);
        app.OnActivate += (sender, e) =>
        {
            var win = MainWindow.Create((Adw.Application)sender!);
            win.Present();
        };
        return app.RunWithSynchronizationContext(null);
    }
}
