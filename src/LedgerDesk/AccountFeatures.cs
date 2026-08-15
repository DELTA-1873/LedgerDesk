using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LedgerDesk;

public static class AccountCalculator
{
    public static decimal Balance(AccountDefinition account, IEnumerable<Entry> records)
    {
        var balance = account.OpeningBalance;
        foreach (var entry in records.Where(x => x.Date.Date >= account.StartDate.Date))
        {
            if (entry.Type == "转账")
            {
                if (entry.Account == account.Name) balance -= entry.Amount + entry.Fee;
                if (entry.ToAccount == account.Name) balance += entry.Amount;
                continue;
            }
            if (entry.Account != account.Name) continue;
            balance += entry.Type switch
            {
                "收入" => entry.Amount,
                "支出" => -entry.Amount,
                "借入" => entry.Amount - entry.Repaid,
                "借出" => -entry.Amount + entry.Repaid,
                "投资中" => -entry.Amount + entry.Repaid,
                _ => 0
            };
        }
        return balance;
    }
}

public static class AccountDashboard
{
    static ShellWindow? owner;
    static TextBlock? total;
    static WrapPanel? cards;

    public static void Attach(ShellWindow window)
    {
        owner = window;
        if ((window.FindName("DashboardPage") as ScrollViewer)?.Content is StackPanel root)
        {
            var block = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(14), Padding = new Thickness(18), Margin = new Thickness(0,0,0,18) };
            var content = new StackPanel();
            var header = new DockPanel();
            total = new TextBlock { FontSize = 22, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Right };
            DockPanel.SetDock(total, Dock.Right); header.Children.Add(total);
            header.Children.Add(new TextBlock { Text = "个人账户总额", FontSize = 16, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });
            cards = new WrapPanel { Margin = new Thickness(0,14,0,0) };
            content.Children.Add(header); content.Children.Add(cards); block.Child = content;
            root.Children.Insert(Math.Min(1, root.Children.Count), block);
        }
        if (window.FindName("SettingsPage") is Grid settings)
        {
            var panel = Find<UniformGrid>(settings).FirstOrDefault();
            if (panel != null)
            {
                var button = new Button { Content = "管理个人账户", Background = Brush("#176B51"), Foreground = Brushes.White, Margin = new Thickness(0,14,0,0) };
                button.Click += (_,_) => { var dialog = new AccountManagerWindow(window) { Owner = window }; dialog.ShowDialog(); };
                var stack = new StackPanel(); stack.Children.Add(new TextBlock { Text="个人账户", FontSize=17, FontWeight=FontWeights.SemiBold });
                stack.Children.Add(new TextBlock { Text="设置启用日余额、账户类型和总额范围", Foreground=Brush("#7A8881"), Margin=new Thickness(0,5,0,0) }); stack.Children.Add(button);
                panel.Children.Insert(0, new Border { Background=Brushes.White, CornerRadius=new CornerRadius(15), Padding=new Thickness(24), Margin=new Thickness(7), Child=stack });
            }
        }
        Refresh();
    }

    public static void Refresh()
    {
        if (owner == null || total == null || cards == null) return;
        cards.Children.Clear();
        var included = owner.Accounts.Where(x=>x.IncludeInTotal && !x.IsArchived).ToList();
        total.Text = included.Sum(x=>AccountCalculator.Balance(x,owner.Records)).ToString("C2",CultureInfo.GetCultureInfo("zh-CN"));
        foreach (var account in owner.Accounts.Where(x=>!x.IsArchived).OrderBy(x=>x.SortOrder))
        {
            var balance = AccountCalculator.Balance(account, owner.Records);
            var stack = new StackPanel(); stack.Children.Add(new TextBlock { Text=account.Name, Foreground=Brush("#75817B"), FontSize=11 });
            stack.Children.Add(new TextBlock { Text=balance.ToString("C2",CultureInfo.GetCultureInfo("zh-CN")), FontWeight=FontWeights.SemiBold, FontSize=15, Margin=new Thickness(0,5,0,0) });
            cards.Children.Add(new Border { Background=Brush("#F4F7F5"), CornerRadius=new CornerRadius(11), Padding=new Thickness(13,10,13,10), Margin=new Thickness(0,0,9,0), MinWidth=125, Child=stack });
        }
    }
    static IEnumerable<T> Find<T>(DependencyObject root) where T:DependencyObject { for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T hit)yield return hit;foreach(var nested in Find<T>(child))yield return nested;} }
    static SolidColorBrush Brush(string value)=>new((Color)ColorConverter.ConvertFromString(value));
}

public sealed class AccountManagerWindow : Window
{
    readonly ShellWindow shell; readonly StackPanel list = new();
    public AccountManagerWindow(ShellWindow owner)
    {
        shell=owner;Title="个人账户管理";Width=560;Height=640;WindowStartupLocation=WindowStartupLocation.CenterOwner;Background=Brush("#F5F8F6");
        var root=new DockPanel{Margin=new Thickness(24)};var add=new Button{Content="＋ 添加账户",Background=Brush("#176B51"),Foreground=Brushes.White,Padding=new Thickness(16,10,16,10),HorizontalAlignment=HorizontalAlignment.Right};add.Click+=(_,_)=>Edit(null);DockPanel.SetDock(add,Dock.Top);root.Children.Add(add);
        var title=new TextBlock{Text="个人账户",FontSize=22,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,14)};DockPanel.SetDock(title,Dock.Top);root.Children.Add(title);root.Children.Add(new ScrollViewer{Content=list,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});Content=root;Render();
    }
    void Render(){list.Children.Clear();foreach(var account in shell.Accounts.OrderBy(x=>x.SortOrder)){var row=new Button{HorizontalContentAlignment=HorizontalAlignment.Stretch,Background=Brushes.White,Margin=new Thickness(0,0,0,9),Padding=new Thickness(15)};var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});grid.ColumnDefinitions.Add(new(){Width=GridLength.Auto});var text=new StackPanel();text.Children.Add(new TextBlock{Text=account.Name,FontWeight=FontWeights.SemiBold});text.Children.Add(new TextBlock{Text=$"{account.Type} · 启用日 {account.StartDate:yyyy-MM-dd} · 初始 {account.OpeningBalance:C2}",Foreground=Brush("#75817B"),FontSize=11,Margin=new Thickness(0,4,0,0)});grid.Children.Add(text);var value=new TextBlock{Text=AccountCalculator.Balance(account,shell.Records).ToString("C2"),VerticalAlignment=VerticalAlignment.Center,FontWeight=FontWeights.SemiBold};Grid.SetColumn(value,1);grid.Children.Add(value);row.Content=grid;row.Click+=(_,_)=>Edit(account);list.Children.Add(row);}}
    void Edit(AccountDefinition? account){var dialog=new AccountEditWindow(account){Owner=this};if(dialog.ShowDialog()!=true)return;if(account==null)shell.Accounts.Add(dialog.Result);else{var oldName=account.Name;var i=shell.Accounts.FindIndex(x=>x.Id==account.Id);shell.Accounts[i]=dialog.Result;if(oldName!=dialog.Result.Name){foreach(var entry in shell.Records){if(entry.Account==oldName)entry.Account=dialog.Result.Name;if(entry.ToAccount==oldName)entry.ToAccount=dialog.Result.Name;}}}shell.PersistAndRefresh();Render();}
    static SolidColorBrush Brush(string value)=>new((Color)ColorConverter.ConvertFromString(value));
}

public sealed class AccountEditWindow : Window
{
    readonly TextBox name=new();readonly ComboBox type=new();readonly TextBox balance=new();readonly DatePicker date=new();readonly CheckBox included=new(){Content="计入个人账户总额"};public AccountDefinition Result{get;}
    public AccountEditWindow(AccountDefinition? source)
    {
        Result=source==null?new AccountDefinition():new AccountDefinition{Id=source.Id,Name=source.Name,Type=source.Type,OpeningBalance=source.OpeningBalance,StartDate=source.StartDate,IncludeInTotal=source.IncludeInTotal,IsArchived=source.IsArchived,SortOrder=source.SortOrder,Color=source.Color};Title=source==null?"添加账户":"编辑账户";Width=430;Height=470;WindowStartupLocation=WindowStartupLocation.CenterOwner;ResizeMode=ResizeMode.NoResize;
        type.ItemsSource=new[]{"电子钱包","银行卡","现金","其他"};name.Text=Result.Name;type.SelectedItem=Result.Type;balance.Text=Result.OpeningBalance.ToString();date.SelectedDate=Result.StartDate;included.IsChecked=Result.IncludeInTotal;
        var root=new StackPanel{Margin=new Thickness(28)};root.Children.Add(new TextBlock{Text=Title,FontSize=21,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,18)});Field(root,"账户名称",name);Field(root,"账户类型",type);Field(root,"启用日初始余额",balance);Field(root,"启用日期",date);root.Children.Add(included);var save=new Button{Content="保存账户",Background=new SolidColorBrush(Color.FromRgb(23,107,81)),Foreground=Brushes.White,Padding=new Thickness(16,10,16,10),Margin=new Thickness(0,22,0,0)};save.Click+=Save;root.Children.Add(save);Content=root;
    }
    static void Field(Panel root,string label,Control input){root.Children.Add(new TextBlock{Text=label,Margin=new Thickness(0,6,0,2)});root.Children.Add(input);}
    void Save(object s,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(name.Text)||!decimal.TryParse(balance.Text,out var amount)){MessageBox.Show("请填写账户名称和正确余额。","简账");return;}Result.Name=name.Text.Trim();Result.Type=type.SelectedItem?.ToString()??"其他";Result.OpeningBalance=amount;Result.StartDate=date.SelectedDate??DateTime.Today;Result.IncludeInTotal=included.IsChecked==true;DialogResult=true;}
}
