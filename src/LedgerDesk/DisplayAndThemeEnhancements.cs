using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LedgerDesk;

public static class DisplayAndThemeEnhancements
{
    public static void Initialize(ShellWindow window)
    {
        window.Loaded += (_, _) =>
        {
            FitToPrimaryWorkArea(window);
            AddThemeSelector(window);
            ThemeManager.Apply(window);
        };
    }

    static void FitToPrimaryWorkArea(Window window)
    {
        var area = SystemParameters.WorkArea;
        window.MaxWidth = area.Width;
        window.MaxHeight = area.Height;
        window.Width = Math.Clamp(area.Width * .82, window.MinWidth, 1500);
        window.Height = Math.Clamp(area.Height * .86, window.MinHeight, 980);
        window.Left = area.Left + (area.Width - window.Width) / 2;
        window.Top = area.Top + (area.Height - window.Height) / 2;
    }

    static void AddThemeSelector(ShellWindow window)
    {
        if (window.FindName("SettingsPage") is not Grid settings || Find<ComboBox>(settings).Any(x => Equals(x.Tag, "ThemePicker"))) return;
        var cards = Find<UniformGrid>(settings).FirstOrDefault();
        if (cards is null) return;

        var picker = new ComboBox
        {
            Tag = "ThemePicker", ItemsSource = ThemeManager.Themes.Select(x => x.Name),
            SelectedItem = ThemeManager.Current.Name, Margin = new Thickness(0, 14, 0, 0)
        };
        picker.SelectionChanged += (_, _) => { if (picker.SelectedItem is string name) ThemeManager.Select(name); };

        var swatches = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 2) };
        foreach (var theme in ThemeManager.Themes)
            swatches.Children.Add(new Border { Width = 28, Height = 28, Margin = new Thickness(0, 0, 9, 0), CornerRadius = new CornerRadius(9), Background = Brush(theme.Accent), ToolTip = theme.Name });

        var content = new StackPanel();
        content.Children.Add(new TextBlock { Text = "界面风格", FontSize = 17, FontWeight = FontWeights.SemiBold });
        content.Children.Add(new TextBlock { Text = "选择后立即应用，并在下次启动时保留。", Foreground = Brush("#7A8881"), Margin = new Thickness(0, 5, 0, 0) });
        content.Children.Add(swatches); content.Children.Add(picker);
        var card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(15), Padding = new Thickness(24), Margin = new Thickness(7), Child = content };
        cards.Children.Insert(1, card);
    }

    static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T hit) yield return hit;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }

    static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}
