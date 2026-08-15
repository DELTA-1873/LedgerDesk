using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LedgerDesk;

public sealed record ThemePalette(string Name, string Accent, string Deep, string Surface, string Canvas, string Soft, string Border, string Text, string Muted);

public static class ThemeManager
{
    public static readonly ThemePalette[] Themes =
    [
        new("绿色", "#176B51", "#123F32", "#FFFFFF", "#F5F8F6", "#E8F4EF", "#D9E3DE", "#263A32", "#75817B"),
        new("淡红色", "#B95F68", "#713D46", "#FFFDFD", "#FBF3F3", "#F8E6E8", "#EAD4D7", "#493438", "#8A7276"),
        new("白色", "#59645F", "#303633", "#FFFFFF", "#F7F7F7", "#ECEEED", "#DCDDDB", "#292D2B", "#737A77"),
        new("米色", "#9A7041", "#58452F", "#FFFDF8", "#F7F1E7", "#F1E5D2", "#E3D5BF", "#453A2D", "#827462"),
        new("蓝色", "#3976A8", "#244D70", "#FFFFFF", "#F1F6FA", "#E2EEF7", "#CFDFEA", "#293D4D", "#718492")
    ];

    static readonly string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "简账", "appearance.json");
    public static ThemePalette Current { get; private set; } = Themes[0];
    public static event Action? Changed;

    public static void Load()
    {
        try
        {
            if (File.Exists(settingsFile))
            {
                var name = JsonSerializer.Deserialize<AppearanceSettings>(File.ReadAllText(settingsFile))?.Theme;
                Current = Themes.FirstOrDefault(x => x.Name == name) ?? Themes[0];
            }
        }
        catch { Current = Themes[0]; }
        UpdateResources();
    }

    public static void Select(string name)
    {
        Current = Themes.FirstOrDefault(x => x.Name == name) ?? Themes[0];
        Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)!);
        File.WriteAllText(settingsFile, JsonSerializer.Serialize(new AppearanceSettings { Theme = Current.Name }));
        UpdateResources();
        foreach (Window window in Application.Current.Windows) Apply(window);
        Changed?.Invoke();
    }

    static void UpdateResources()
    {
        var r = Application.Current.Resources;
        r["Green"] = Brush(Current.Accent); r["Bg"] = Brush(Current.Canvas);
        r["Border"] = Brush(Current.Border); r["Muted"] = Brush(Current.Muted);
    }

    public static void Apply(DependencyObject root)
    {
        ApplyOne(root);
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) Apply(VisualTreeHelper.GetChild(root, i));
    }

    static void ApplyOne(DependencyObject item)
    {
        if (item is Control c)
        {
            c.Background = Convert(c.Background, Current);
            c.Foreground = Convert(c.Foreground, Current);
            c.BorderBrush = Convert(c.BorderBrush, Current);
        }
        else if (item is Border b)
        {
            b.Background = Convert(b.Background, Current);
            b.BorderBrush = Convert(b.BorderBrush, Current);
        }
        else if (item is Panel p) p.Background = Convert(p.Background, Current);
        else if (item is TextBlock t) t.Foreground = Convert(t.Foreground, Current);
    }

    static Brush Convert(Brush source, ThemePalette p)
    {
        if (source is not SolidColorBrush solid) return source;
        var hex = solid.Color.ToString().ToUpperInvariant();
        string? target = Role(hex) switch
        {
            1 => p.Accent, 2 => p.Deep, 3 => p.Surface, 4 => p.Canvas,
            5 => p.Soft, 6 => p.Border, 7 => p.Text, 8 => p.Muted, _ => null
        };
        return target is null ? source : Brush(target);
    }

    static int Role(string value)
    {
        foreach (var p in Themes)
        {
            if (Same(value, p.Accent)) return 1; if (Same(value, p.Deep)) return 2;
            if (Same(value, p.Surface)) return 3; if (Same(value, p.Canvas)) return 4;
            if (Same(value, p.Soft)) return 5; if (Same(value, p.Border)) return 6;
            if (Same(value, p.Text)) return 7; if (Same(value, p.Muted)) return 8;
        }
        if (value is "#FFF4F7F5" or "#FFF5F8F6") return 4;
        if (value is "#FFFFFFFF" or "#FFFFFDFD" or "#FFFFFDF8") return 3;
        if (value is "#FF176B51" or "#FF123F32") return value == "#FF176B51" ? 1 : 2;
        return 0;
    }

    static bool Same(string actual, string expected) => actual == "#FF" + expected.TrimStart('#').ToUpperInvariant();
    static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
    sealed class AppearanceSettings { public string Theme { get; set; } = "绿色"; }
}
