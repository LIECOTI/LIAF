using System;

namespace LIAF;

public static class MainWindow
{
    public static Adw.ApplicationWindow Create(Adw.Application app)
    {
        var window = Adw.ApplicationWindow.New(app);
        window.Title = "LIAF — Android Flash Tool";
        window.SetDefaultSize(1150, 750);

        var stack = Gtk.Stack.New();
        stack.SetTransitionType(Gtk.StackTransitionType.SlideUpDown);
        stack.SetHexpand(true);
        stack.SetVexpand(true);

        stack.AddTitled(Pages.HomePage.Create(), "home", "Главная");
        stack.AddTitled(Pages.BasicFlashPage.Create(), "basic", "Базовая прошивка");
        stack.AddTitled(Pages.AdvancedFlashPage.Create(), "advanced", "Расширенная");
        stack.AddTitled(Pages.CustomFlashPage.Create(), "custom", "Своя прошивка");
        stack.AddTitled(Pages.WiredFlashPage.Create(), "wired", "Sideload / Push");
        stack.AddTitled(Pages.PatcherPage.Create(), "patcher", "Патчер");
        stack.AddTitled(Pages.PartitionPage.Create(), "partition", "Разделы");
        stack.AddTitled(Pages.FormatExtractPage.Create(), "format", "Формат / Извлечение");
        stack.AddTitled(Pages.FirmwarePage.Create(), "firmware", "Прошивки (Xiaomi)");
        stack.AddTitled(Pages.AppManagerPage.Create(), "apps", "Приложения");
        stack.AddTitled(Pages.ScrcpyPage.Create(), "scrcpy", "Scrcpy");
        stack.AddTitled(Pages.OthersPage.Create(), "others", "Другое");
        stack.AddTitled(Pages.SettingsPage.Create(), "settings", "Настройки");

        var sidebar = Gtk.StackSidebar.New();
        sidebar.SetStack(stack);
        sidebar.SetSizeRequest(220, -1);

        var headerBar = Adw.HeaderBar.New();
        headerBar.SetTitleWidget(Adw.WindowTitle.New("LIAF", "v0.3.0"));

        var rightBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        rightBox.Append(headerBar);
        rightBox.Append(stack);
        rightBox.SetHexpand(true);

        var mainBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        mainBox.Append(sidebar);
        mainBox.Append(Gtk.Separator.New(Gtk.Orientation.Vertical));
        mainBox.Append(rightBox);

        window.SetContent(mainBox);
        return window;
    }
}
