using System.Windows;using System.Windows.Controls;
namespace LedgerDesk;
public partial class App:Application
{
 protected override void OnStartup(StartupEventArgs e){EventManager.RegisterClassHandler(typeof(TextBox),FrameworkElement.LoadedEvent,new RoutedEventHandler(RoundLoadedTextBox));base.OnStartup(e);Activated+=ActivatedOnce;}
 void RoundLoadedTextBox(object sender,RoutedEventArgs e){if(sender is TextBox box&&TryFindResource("RoundedTextBox") is Style style&&box.Style!=style)box.Style=style;}
 void ActivatedOnce(object? sender,EventArgs e){if(MainWindow is not ShellWindow w||!w.IsLoaded)return;if(w.FindName("Search") is TextBox search){search.Visibility=Visibility.Collapsed;search.Text="";}MonthlyDashboard.Attach(w);Activated-=ActivatedOnce;}
}
