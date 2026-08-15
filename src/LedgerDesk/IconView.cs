using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
namespace LedgerDesk;
public class IconView : FrameworkElement
{
    public static readonly DependencyProperty SourceProperty=DependencyProperty.Register(nameof(Source),typeof(string),typeof(IconView),new FrameworkPropertyMetadata("wallet-cards",FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty StrokeProperty=DependencyProperty.Register(nameof(Stroke),typeof(Brush),typeof(IconView),new FrameworkPropertyMetadata(Brushes.Black,FrameworkPropertyMetadataOptions.AffectsRender));
    public string Source{get=>(string)GetValue(SourceProperty);set=>SetValue(SourceProperty,value);} public Brush Stroke{get=>(Brush)GetValue(StrokeProperty);set=>SetValue(StrokeProperty,value);}
    protected override void OnRender(DrawingContext dc){base.OnRender(dc);if(ActualWidth<=0||ActualHeight<=0)return;var gs=Load(Source);if(gs==null)return;var z=Math.Min(ActualWidth,ActualHeight)/24d;dc.PushTransform(new TranslateTransform((ActualWidth-24*z)/2,(ActualHeight-24*z)/2));dc.PushTransform(new ScaleTransform(z,z));var p=new Pen(Stroke,1.8){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};foreach(var g in gs)dc.DrawGeometry(null,p,g);dc.Pop();dc.Pop();}
    static List<Geometry>? Load(string name){try{var uri=new Uri($"pack://application:,,,/Assets/{name}.svg");using var s=Application.GetResourceStream(uri)?.Stream;if(s==null)return null;var doc=XDocument.Load(s);var list=new List<Geometry>();foreach(var e in doc.Root!.Elements()){Geometry? g=e.Name.LocalName switch{"path"=>Geometry.Parse((string?)e.Attribute("d")??""),"circle"=>new EllipseGeometry(new Point(N(e,"cx"),N(e,"cy")),N(e,"r"),N(e,"r")),"line"=>new LineGeometry(new Point(N(e,"x1"),N(e,"y1")),new Point(N(e,"x2"),N(e,"y2"))),"rect"=>new RectangleGeometry(new Rect(N(e,"x"),N(e,"y"),N(e,"width"),N(e,"height")),N(e,"rx"),N(e,"rx")),"polyline"=>Poly((string?)e.Attribute("points"),false),"polygon"=>Poly((string?)e.Attribute("points"),true),_=>null};if(g!=null)list.Add(g);}return list;}catch{return null;}}
    static double N(XElement e,string n)=>double.TryParse((string?)e.Attribute(n),NumberStyles.Any,CultureInfo.InvariantCulture,out var v)?v:0;
    static Geometry? Poly(string? s,bool close){if(string.IsNullOrWhiteSpace(s))return null;var a=s.Replace(","," ").Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(x=>double.Parse(x,CultureInfo.InvariantCulture)).ToArray();if(a.Length<4)return null;var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(new Point(a[0],a[1]),false,close);for(int i=2;i+1<a.Length;i+=2)c.LineTo(new Point(a[i],a[i+1]),true,false);}g.Freeze();return g;}
}
