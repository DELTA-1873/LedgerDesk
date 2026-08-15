namespace LedgerDesk;

public static class ExpenseClassification
{
    public const decimal LegacyLargeExpenseThreshold = 5000m;

    public static string Kind(Entry entry)
    {
        if (entry.Custom is "生活消费" or "大额支出") return entry.Custom;
        return entry.Amount >= LegacyLargeExpenseThreshold ? "大额支出" : "生活消费";
    }
}
