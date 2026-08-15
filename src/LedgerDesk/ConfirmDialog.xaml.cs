using System.Windows;using System.Windows.Input;
namespace LedgerDesk;
public partial class ConfirmDialog:Window
{
 public ConfirmDialog(Entry entry){InitializeComponent();EntryText.Text=$"{entry.Date:yyyy年M月d日} · {entry.Category} · ¥{entry.Amount:N2}";}
 void Confirm_Click(object s,RoutedEventArgs e)=>DialogResult=true;void Cancel_Click(object s,RoutedEventArgs e)=>DialogResult=false;
}
