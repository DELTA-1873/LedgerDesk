using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LedgerDesk;

public static class MonthlyDashboard
{
    static bool attached;
    static List<Entry> latest = [];
    static AnimatedMonthRing? ring;
    static HorizontalComparisonChart? comparison;

    public static void UpdateRecords(IEnumerable<Entry> records)
    {
        latest = records.Select(Clone).ToList();
        ring?.SetRecords(latest);
        comparison?.SetRecords(latest);
    }

    public static void Attach(ShellWindow window)
    {
        if (attached) return;
        attached = true;
        if ((window.FindName("DashboardPage") as ScrollViewer)?.Content is not StackPanel root) return;
        if (latest.Count == 0) latest = Read().Records;

        ring = new AnimatedMonthRing(); ring.SetRecords(latest);
        comparison = new HorizontalComparisonChart(); comparison.SetRecords(latest);
        var grid = new Grid { Height = 340, Margin = new Thickness(0, 0, 0, 18) };
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(16) });
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(1.4, GridUnitType.Star) });

        var months = Enumerable.Range(0, 36).Select(i => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-i)).ToList();
        var monthPicker = Picker(months, 118, "{0:yyyy年M月}");
        monthPicker.SelectedIndex = 0;
        monthPicker.SelectionChanged += (_, _) => { if (monthPicker.SelectedItem is DateTime month) ring.ShowMonth(month); };
        var scopePicker = Picker(new[] { "生活消费", "大额支出", "全部支出" }, 96, null);
        scopePicker.SelectedIndex = 0;
        scopePicker.SelectionChanged += (_, _) => ring.Scope = scopePicker.SelectedIndex switch { 1 => ExpenseScope.Large, 2 => ExpenseScope.All, _ => ExpenseScope.Living };
        var ringActions = new StackPanel { Orientation = Orientation.Horizontal };
        ringActions.Children.Add(scopePicker); ringActions.Children.Add(monthPicker);

        var modePicker = Picker(new[] { "月度对比", "年度对比" }, 105, null);
        modePicker.SelectionChanged += (_, _) => comparison.Mode = modePicker.SelectedIndex == 1 ? ComparisonMode.Year : ComparisonMode.Month;
        modePicker.SelectedIndex = 0;
        var pie = Card("单月消费构成", ring, ringActions);
        var bars = Card("收入 / 生活消费 / 大额支出", comparison, modePicker);
        Grid.SetColumn(bars, 2); grid.Children.Add(pie); grid.Children.Add(bars);
        root.Children.Insert(Math.Min(1, root.Children.Count), grid);
        ring.ShowMonth(months[0], false);
    }

    static ComboBox Picker(System.Collections.IEnumerable items, double width, string? format)
    {
        var picker = new ComboBox { Width = width, ItemsSource = items, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(4, 0, 0, 0) };
        if (format != null)
        {
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(".") { StringFormat = format });
            picker.ItemTemplate = new DataTemplate { VisualTree = factory };
        }
        return picker;
    }

    static Border Card(string title, FrameworkElement chart, FrameworkElement action)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        var label = new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(action, Dock.Right); header.Children.Add(action); header.Children.Add(label);
        var grid = new Grid(); grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(chart, 1); grid.Children.Add(header); grid.Children.Add(chart);
        return new Border { Background = Brushes.White, CornerRadius = new CornerRadius(14), Padding = new Thickness(18), Child = grid };
    }

    static LedgerData Read()
    {
        try { var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "简账", "ledger.json"); return File.Exists(file) ? JsonSerializer.Deserialize<LedgerData>(File.ReadAllText(file)) ?? new() : new(); }
        catch { return new(); }
    }
    static Entry Clone(Entry e) => new() { Id=e.Id,Type=e.Type,Amount=e.Amount,Date=e.Date,Category=e.Category,Account=e.Account,Party=e.Party,Status=e.Status,DueDate=e.DueDate,Repaid=e.Repaid,Note=e.Note,Project=e.Project,Reference=e.Reference,Rate=e.Rate,Custom=e.Custom };
}

public enum ExpenseScope { Living, Large, All }

public class AnimatedMonthRing : FrameworkElement
{
    Dictionary<(int year, int month, string kind, string category), decimal> cache = [];
    DateTime month = DateTime.Today;
    ExpenseScope scope;
    public ExpenseScope Scope { get => scope; set { scope = value; InvalidateVisual(); } }
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(AnimatedMonthRing), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));
    public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
    readonly Color[] colors = [Color.FromRgb(42,139,102),Color.FromRgb(234,177,69),Color.FromRgb(208,91,86),Color.FromRgb(72,123,180),Color.FromRgb(142,99,176),Color.FromRgb(72,164,168),Color.FromRgb(230,132,69)];

    public void SetRecords(IEnumerable<Entry> value) { cache = value.Where(x => x.Type == "支出").GroupBy(x => (x.Date.Year, x.Date.Month, ExpenseClassification.Kind(x), x.Category)).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount)); InvalidateVisual(); }
    public void ShowMonth(DateTime value, bool animate = true) { month = value; if (animate) BeginAnimation(ProgressProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(720)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } }); else { BeginAnimation(ProgressProperty, null); Progress = 1; } InvalidateVisual(); }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var rows = cache.Where(x => x.Key.year == month.Year && x.Key.month == month.Month && (Scope == ExpenseScope.All || x.Key.kind == (Scope == ExpenseScope.Large ? "大额支出" : "生活消费"))).GroupBy(x => x.Key.category).Select(g => (name:g.Key, value:g.Sum(x => x.Value))).OrderByDescending(x => x.value).Take(7).ToList();
        var total = rows.Sum(x => x.value);
        if (total <= 0) { Text(dc, $"{month:yyyy年M月}暂无记录", new Point(ActualWidth / 2, ActualHeight / 2 - 8), 12, TextAlignment.Center, Brushes.Gray); return; }
        double ringSize=Math.Min(ActualHeight-20,ActualWidth*.48),cx=ringSize/2+4,cy=ActualHeight/2,r=ringSize*.35,thickness=Math.Max(16,r*.28),start=-135,visible=360*Math.Clamp(Progress,0,1),used=0;
        for(int i=0;i<rows.Count&&used<visible;i++){double full=(double)(rows[i].value/total)*360,sweep=Math.Min(full,visible-used);Arc(dc,new Point(cx,cy),r,start+used,sweep,new SolidColorBrush(colors[i%colors.Length]),thickness);used+=full;}
        Text(dc,$"¥{total:N0}",new Point(cx,cy-12),15,TextAlignment.Center,Brushes.Black);Text(dc,Scope==ExpenseScope.Large?"大额支出":Scope==ExpenseScope.Living?"生活消费":"全部支出",new Point(cx,cy+10),10,TextAlignment.Center,Brushes.Gray);
        for(int i=0;i<rows.Count;i++){double y=12+i*27,x=ringSize+18;dc.DrawRoundedRectangle(new SolidColorBrush(colors[i%colors.Length]),null,new Rect(x,y+4,9,9),3,3);Text(dc,rows[i].name,new Point(x+16,y),11,TextAlignment.Left,Brushes.Black);Text(dc,$"{rows[i].value/total:P0}",new Point(ActualWidth-4,y),11,TextAlignment.Right,Brushes.DimGray);}
    }
    static void Arc(DrawingContext dc,Point c,double r,double start,double sweep,Brush brush,double width){if(sweep<=.01)return;double A(double x)=>x*Math.PI/180;var p1=new Point(c.X+r*Math.Cos(A(start)),c.Y+r*Math.Sin(A(start)));var p2=new Point(c.X+r*Math.Cos(A(start+sweep)),c.Y+r*Math.Sin(A(start+sweep)));var g=new StreamGeometry();using(var x=g.Open()){x.BeginFigure(p1,false,false);x.ArcTo(p2,new Size(r,r),0,sweep>180,SweepDirection.Clockwise,true,false);}dc.DrawGeometry(null,new Pen(brush,width){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},g);}
    internal static void Text(DrawingContext d,string s,Point p,double size,TextAlignment align,Brush brush){var f=new FormattedText(s,CultureInfo.GetCultureInfo("zh-CN"),FlowDirection.LeftToRight,new Typeface("Microsoft YaHei"),size,brush,1.25){TextAlignment=align};d.DrawText(f,p);}
}

public enum ComparisonMode { Month, Year }

public class HorizontalComparisonChart : FrameworkElement
{
    Dictionary<(string kind,int year,int month),decimal> monthCache=[]; Dictionary<(string kind,int year),decimal> yearCache=[]; ComparisonMode mode;
    public ComparisonMode Mode { get=>mode; set { mode=value; InvalidateVisual(); } }
    public void SetRecords(IEnumerable<Entry> value)
    {
        var rows=value.Where(x=>x.Type is "收入" or "支出").Select(x=>(kind:x.Type=="收入"?"收入":ExpenseClassification.Kind(x),entry:x)).ToList();
        monthCache=rows.GroupBy(x=>(x.kind,x.entry.Date.Year,x.entry.Date.Month)).ToDictionary(g=>g.Key,g=>g.Sum(x=>x.entry.Amount));
        yearCache=rows.GroupBy(x=>(x.kind,x.entry.Date.Year)).ToDictionary(g=>g.Key,g=>g.Sum(x=>x.entry.Amount)); InvalidateVisual();
    }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var rows=Mode==ComparisonMode.Month?Enumerable.Range(0,6).Select(i=>new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(i-5)).Select(d=>(label:$"{d.Month}月",inc:Sum("收入",d.Year,d.Month),life:Sum("生活消费",d.Year,d.Month),large:Sum("大额支出",d.Year,d.Month))).ToList():Enumerable.Range(0,5).Select(i=>DateTime.Today.Year+i-4).Select(y=>(label:$"{y}年",inc:Sum("收入",y,null),life:Sum("生活消费",y,null),large:Sum("大额支出",y,null))).ToList();
        decimal max=Math.Max(1,rows.SelectMany(x=>new[]{x.inc,x.life,x.large}).Max());double labelW=50,right=54,top=22,rowH=(ActualHeight-top-6)/rows.Count;
        for(int i=0;i<rows.Count;i++){double y=top+i*rowH;Text(dc,rows[i].label,new Point(0,y+rowH*.34),10,TextAlignment.Left,Brushes.DimGray);DrawBar(dc,labelW,y+2,rows[i].inc,max,ActualWidth-labelW-right,rowH*.20,Color.FromRgb(64,151,115));DrawBar(dc,labelW,y+rowH*.31,rows[i].life,max,ActualWidth-labelW-right,rowH*.20,Color.FromRgb(72,123,180));DrawBar(dc,labelW,y+rowH*.60,rows[i].large,max,ActualWidth-labelW-right,rowH*.20,Color.FromRgb(214,94,87));Text(dc,$"¥{Math.Max(rows[i].inc,Math.Max(rows[i].life,rows[i].large)):N0}",new Point(ActualWidth-2,y+rowH*.34),9,TextAlignment.Right,Brushes.Gray);}
        Legend(dc,labelW,6,"收入",Color.FromRgb(64,151,115));Legend(dc,labelW+58,6,"生活",Color.FromRgb(72,123,180));Legend(dc,labelW+116,6,"大额",Color.FromRgb(214,94,87));
    }
    decimal Sum(string kind,int year,int? month)=>month.HasValue?(monthCache.TryGetValue((kind,year,month.Value),out var m)?m:0):(yearCache.TryGetValue((kind,year),out var y)?y:0);
    static void Legend(DrawingContext dc,double x,double y,string label,Color color){dc.DrawEllipse(new SolidColorBrush(color),null,new Point(x,y),4,4);Text(dc,label,new Point(x+9,y-6),10,TextAlignment.Left,Brushes.Gray);}
    static void Text(DrawingContext dc,string text,Point point,double size,TextAlignment align,Brush brush)=>AnimatedMonthRing.Text(dc,text,point,size,align,brush);
    static void DrawBar(DrawingContext dc,double x,double y,decimal value,decimal max,double width,double height,Color color){double w=(double)(value/max)*width;dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(239,243,241)),null,new Rect(x,y,width,height),height/2,height/2);if(w>0)dc.DrawRoundedRectangle(new SolidColorBrush(color),null,new Rect(x,y,w,height),height/2,height/2);}
}
