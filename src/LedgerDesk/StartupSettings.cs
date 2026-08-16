using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LedgerDesk;

public static class StartupSettings
{
    const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName = "LedgerDesk";

    public static void Attach(ShellWindow window)
    {
        if (window.FindName("SettingsPage") is not Grid settings || Find<CheckBox>(settings).Any(x => Equals(x.Tag, ValueName))) return;
        var panel = Find<UniformGrid>(settings).FirstOrDefault();
        if (panel is null) return;

        var toggle = new CheckBox
        {
            Tag = ValueName,
            Content = "登录 Windows 后自动启动简账",
            IsChecked = IsEnabled(),
            Margin = new Thickness(0, 15, 0, 0),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        var state = new TextBlock
        {
            Text = toggle.IsChecked == true ? "当前已启用" : "当前未启用",
            Foreground = Brush("#75817B"),
            Margin = new Thickness(0, 7, 0, 0)
        };
        toggle.Click += (_, _) =>
        {
            var requested = toggle.IsChecked == true;
            try
            {
                SetEnabled(requested);
                state.Text = requested ? "当前已启用，下次登录时生效" : "当前未启用";
            }
            catch (Exception ex)
            {
                toggle.IsChecked = IsEnabled();
                state.Text = toggle.IsChecked == true ? "当前已启用" : "当前未启用";
                MessageBox.Show($"无法更新开机启动设置：{ex.Message}", "简账");
            }
        };

        var content = new StackPanel();
        content.Children.Add(new TextBlock { Text = "开机启动", FontSize = 17, FontWeight = FontWeights.SemiBold });
        content.Children.Add(new TextBlock
        {
            Text = "这是可选设置，仅对当前 Windows 用户生效。",
            Foreground = Brush("#7A8881"),
            Margin = new Thickness(0, 5, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(toggle);
        content.Children.Add(state);
        panel.Children.Insert(Math.Min(1, panel.Children.Count), new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(24),
            Margin = new Thickness(7),
            Child = content
        });
    }

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
        return key?.GetValue(ValueName) is string value && !string.IsNullOrWhiteSpace(value);
    }

    static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, true)
            ?? throw new InvalidOperationException("无法访问当前用户启动项。");
        if (enabled)
        {
            var executable = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executable)) executable = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(executable)) throw new InvalidOperationException("无法确定程序路径。");
            key.SetValue(ValueName, $"\"{executable}\"", RegistryValueKind.String);
        }
        else key.DeleteValue(ValueName, false);
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
