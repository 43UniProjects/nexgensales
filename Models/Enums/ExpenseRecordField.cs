namespace NexGenSales.Models.Enums;

public enum ExpensesRecordField
{
    DateRecorded,
    ExpenseCategory,
    SpecificType,
    Amount,
    AssetId

}


public static class ExpensesRecordFieldExtensions
{
    public static string ToColumnName(this ExpensesRecordField field)
    {
        return field switch
        {
            ExpensesRecordField.DateRecorded => "Date_Recorded",
            ExpensesRecordField.ExpenseCategory => "Expense_Category",
            ExpensesRecordField.SpecificType => "Specific_Type",
            ExpensesRecordField.Amount => "Amount",
            ExpensesRecordField.AssetId => "Asset_ID",
            _ => throw new ArgumentOutOfRangeException(nameof(field), $"No column mapping exists for {field}")
        };
    }
}
