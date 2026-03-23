using System;

namespace LIAF.Common;

public static class UIHelper
{
    public static Gtk.Box Page(string titleText, string? subtitle = null)
    {
        var box = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        box.SetMarginTop(20); box.SetMarginBottom(20);
        box.SetMarginStart(20); box.SetMarginEnd(20);

        var title = Gtk.Label.New(titleText);
        title.AddCssClass("title-1");
        title.SetXalign(0);
        box.Append(title);

        if (subtitle != null)
        {
            var sub = Gtk.Label.New(subtitle);
            sub.AddCssClass("dim-label");
            sub.SetWrap(true);
            sub.SetXalign(0);
            box.Append(sub);
        }
        box.Append(Gtk.Separator.New(Gtk.Orientation.Horizontal));
        return box;
    }

    public static (Gtk.ScrolledWindow scroll, Action<string> append, Action clear) LogView(int minHeight = 180)
    {
        var tv = Gtk.TextView.New();
        tv.SetEditable(false);
        tv.SetMonospace(true);
        tv.SetWrapMode(Gtk.WrapMode.WordChar);
        tv.AddCssClass("card");
        tv.SetTopMargin(6); tv.SetBottomMargin(6);
        tv.SetLeftMargin(8); tv.SetRightMargin(8);

        var scroll = Gtk.ScrolledWindow.New();
        scroll.SetChild(tv);
        scroll.SetVexpand(true);
        scroll.SetMinContentHeight(minHeight);

        var buffer = tv.GetBuffer();
        var log = "";

        Action<string> append = (text) =>
        {
            log += text + "\n";
            buffer.SetText(log, -1);
        };
        Action clear = () => { log = ""; buffer.SetText("", -1); };

        return (scroll, append, clear);
    }

    public static Gtk.Button Btn(string label, string? css = null)
    {
        var btn = Gtk.Button.NewWithLabel(label);
        if (css != null) btn.AddCssClass(css);
        return btn;
    }

    public static Gtk.Box HBox(int spacing = 6)
    {
        return Gtk.Box.New(Gtk.Orientation.Horizontal, spacing);
    }

    public static Gtk.Entry Entry(string placeholder)
    {
        var e = Gtk.Entry.New();
        e.SetPlaceholderText(placeholder);
        e.SetHexpand(true);
        return e;
    }

    public static Gtk.Label SectionLabel(string text)
    {
        var l = Gtk.Label.New(text);
        l.AddCssClass("title-4");
        l.SetXalign(0);
        l.SetMarginTop(8);
        return l;
    }

    public static Gtk.ScrolledWindow Scrollable(Gtk.Widget child)
    {
        var sw = Gtk.ScrolledWindow.New();
        sw.SetChild(child);
        sw.SetVexpand(true);
        return sw;
    }
}
