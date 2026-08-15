namespace LedgerDesk;

public static class ExpenseClassification
{
public static string Kind(Entry entry)
    {
        if (entry.Custom is "生活消费" or "大额支出") return entry.Custom;
        return "未分类";
    }
}
