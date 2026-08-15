using System.Windows.Media;
namespace LedgerDesk;
public class EntryCardView
{
 public required Entry Entry{get;init;}public string Category=>Entry.Category;public string Description{get{var bits=new[]{Entry.Type,Entry.Party,Entry.Note,Entry.Project}.Where(x=>!string.IsNullOrWhiteSpace(x));return string.Join("  ·  ",bits);}}public string DisplayDate=>$"{Entry.Date:M月d日}  {Entry.Account}";public string DisplayAmount=>$"{(Entry.Type is "支出" or "借出" or "投资中"?"−":"+")} ¥{Entry.Amount:N2}";public Brush AmountBrush=>B(Entry.Type is "收入" or "借入"?"#248160":"#C34C4C");public Brush IconBackground=>B(Entry.Type switch{"收入"=>"#E5F6EE","支出"=>"#FDEBEA","投资中"=>"#EAF1FB","借入"=>"#F0EBF8",_=>"#FFF4DE"});public Brush IconBrush=>B(Entry.Type switch{"收入"=>"#248160","支出"=>"#BE4C46","投资中"=>"#4274AA","借入"=>"#7E55A4",_=>"#B4751D"});
 public string Icon=>Entry.Category switch{"餐饮"=>"utensils","购物"=>"shopping-bag","交通"=>"car-taxi-front","住房"=>"house","医疗"=>"heart-pulse","娱乐"=>"clapperboard","教育"=>"graduation-cap","基金" or "股票" or "债券" or "理财" or "项目投资"=>"chart-no-axes-combined","个人借款" or "银行贷款" or "信用借款" or "亲友借款"=>"hand-coins","工资" or "奖金" or "副业"=>"circle-dollar-sign",_=>"wallet-cards"};static Brush B(string s)=>(Brush)new BrushConverter().ConvertFromString(s)!;
}
