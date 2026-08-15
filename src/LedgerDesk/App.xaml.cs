using System.Windows;using System.Windows.Controls;using System.Windows.Media;
namespace LedgerDesk;
public partial class App:Application
{
 protected override void OnStartup(StartupEventArgs e){ThemeManager.Load();EventManager.RegisterClassHandler(typeof(Window),FrameworkElement.LoadedEvent,new RoutedEventHandler(WindowLoaded));EventManager.RegisterClassHandler(typeof(TextBox),FrameworkElement.LoadedEvent,new RoutedEventHandler(RoundLoadedTextBox));base.OnStartup(e);}
 void WindowLoaded(object sender,RoutedEventArgs e){if(sender is Window w){TextOptions.SetTextFormattingMode(w,TextFormattingMode.Display);TextOptions.SetTextRenderingMode(w,TextRenderingMode.ClearType);RenderOptions.SetBitmapScalingMode(w,BitmapScalingMode.HighQuality);w.UseLayoutRounding=true;w.SnapsToDevicePixels=true;ThemeManager.Apply(w);}}
 void RoundLoadedTextBox(object sender,RoutedEventArgs e){if(sender is TextBox box&&TryFindResource("RoundedTextBox") is Style style&&box.Style!=style)box.Style=style;}
}
