using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace LedgerDesk;
public class Entry : INotifyPropertyChanged
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "支出";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Category { get; set; } = "其他支出";
    public string Account { get; set; } = "微信";
    public string ToAccount { get; set; } = "";
    public decimal Fee { get; set; }
    public string Party { get; set; } = "";
    public string Status { get; set; } = "已结清";
    public DateTime? DueDate { get; set; }
    public decimal Repaid { get; set; }
    public string Note { get; set; } = "";
    public string Project { get; set; } = "";
    public string Reference { get; set; } = "";
    public decimal? Rate { get; set; }
    public string Custom { get; set; } = "";
    public decimal Remaining => Math.Max(0, Amount - Repaid);
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed([CallerMemberName] string? name=null)=>PropertyChanged?.Invoke(this,new(name));
}
public class AccountDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "新账户";
    public string Type { get; set; } = "其他";
    public decimal OpeningBalance { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public bool IncludeInTotal { get; set; } = true;
    public bool IsArchived { get; set; }
    public int SortOrder { get; set; }
    public string Color { get; set; } = "#176B51";
}
public class LedgerData
{
    public List<Entry> Records { get; set; } = [];
    public List<AccountDefinition> Accounts { get; set; } = [];
    public Dictionary<string,List<string>> Categories { get; set; } = new()
    {
        ["收入"]=["工资","奖金","副业","报销","其他收入"], ["支出"]=["餐饮","购物","交通","住房","医疗","娱乐","教育","其他支出"],
        ["借入"]=["个人借款","银行贷款","信用借款"], ["借出"]=["亲友借款","业务往来","其他借出"], ["投资中"]=["基金","股票","债券","理财","项目投资"]
    };
}
