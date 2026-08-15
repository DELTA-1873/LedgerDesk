using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace LedgerDesk;
public partial class EntryDetailWindow:Window
{
 public Entry Entry{get;}public bool EditRequested{get;private set;}public bool DeleteRequested{get;private set;}
 public EntryDetailWindow(Entry entry){InitializeComponent();Entry=entry;DataContext=new EntryCardView{Entry=entry};BuildRows();}
 void BuildRows(){var rows=new List<(string,string)>{("日期",Entry.Date.ToString("yyyy年M月d日")),("类型",Entry.Type),("账户",Entry.Account),("转入账户",Entry.ToAccount),("手续费",Entry.Fee==0?"":$"¥{Entry.Fee:N2}"),("状态",Entry.Status),("对象",Entry.Party),("计划日期",Entry.DueDate?.ToString("yyyy年M月d日")??"—"),("已归还 / 收回",$"¥{Entry.Repaid:N2}"),("备注",Entry.Note),("项目 / 标签",Entry.Project),("凭证编号",Entry.Reference),("自定义明细",Entry.Custom)}.Where(x=>!string.IsNullOrWhiteSpace(x.Item2));var g=new Grid();g.ColumnDefinitions.Add(new(){Width=new GridLength(125)});g.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});int i=0;foreach(var(k,v)in rows){g.RowDefinitions.Add(new(){Height=GridLength.Auto});var a=new TextBlock{Text=k,Foreground=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(119,134,126)),Margin=new Thickness(0,7,0,7)};var b=new TextBlock{Text=v,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,7,0,7)};Grid.SetRow(a,i);Grid.SetRow(b,i);Grid.SetColumn(b,1);g.Children.Add(a);g.Children.Add(b);i++;}Details.Children.Add(g);}
 void Drag_Click(object s,MouseButtonEventArgs e){if(e.ChangedButton==MouseButton.Left)DragMove();}void Close_Click(object s,RoutedEventArgs e)=>Close();void Edit_Click(object s,RoutedEventArgs e){EditRequested=true;Close();}
 void Delete_Click(object s,RoutedEventArgs e){var dialog=new ConfirmDialog(Entry){Owner=this};if(dialog.ShowDialog()==true){DeleteRequested=true;Close();}}
}
