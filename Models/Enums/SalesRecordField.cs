namespace NexGenSales.Models.Enums;

public enum SalesRecordField
{
    DateTime,
    ItemId,
    SupplierId,
    QuantitySold,
    UnitPurchaseCost,
    UnitSalePrice,
    AllowedDiscount,
    NetRevenue,
    CurrentStock
}


public static class SalesRecordFieldExtensions
    {
        public static string ToColumnName(this SalesRecordField field)
        {
            return field switch
            {
                SalesRecordField.DateTime => "Date_Time",
                SalesRecordField.ItemId => "Item_ID",
                SalesRecordField.SupplierId => "Supplier_ID",
                SalesRecordField.QuantitySold => "Quantity_Sold",
                SalesRecordField.UnitPurchaseCost => "Unit_Purchase_Cost",
                SalesRecordField.UnitSalePrice => "Unit_Sale_Price",
                SalesRecordField.AllowedDiscount => "Allowed_Discount",
                SalesRecordField.NetRevenue => "Net_Revenue",
                SalesRecordField.CurrentStock => "Current_Stock",
                _ => throw new ArgumentOutOfRangeException(nameof(field), $"No column mapping exists for {field}")
            };
        }
    }