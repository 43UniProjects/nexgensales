using System;
using System.Collections.Generic;
using NexGenSales.Models;
using NexGenSales.Models.Enums;

namespace NexGenSales.Services.Data.Mapper;

public static class RecordMappers
{
    public static SalesRecord MapToSalesRecord(Dictionary<SalesRecordField, object> row)
    {
        var record = new SalesRecord();

        foreach (var entry in row)
        {
            var field = entry.Key;
            var value = entry.Value;
            if (value == null) continue;

            switch (field)
            {
                case SalesRecordField.DateTime:
                    if (value is DateTime datetime) record.Date_Time = datetime;
                    break;
                case SalesRecordField.ItemId:
                    record.Item_ID = value.ToString();
                    break;
                case SalesRecordField.QuantitySold:
                    if (value is int qty) record.Quantity_Sold = qty;
                    else if (int.TryParse(value.ToString(), out int pQty)) record.Quantity_Sold = pQty;
                    break;
                case SalesRecordField.NetRevenue:
                    if (value is double revenue) record.Net_Revenue = revenue;
                    else if (double.TryParse(value.ToString(), out double pRevanue)) record.Net_Revenue = pRevanue;
                    break;
                case SalesRecordField.SupplierId:
                    record.Supplier_ID = value.ToString();
                    break;
                case SalesRecordField.UnitPurchaseCost:
                    if (value is double cost) record.Unit_Purchase_Cost = cost;
                    else if (double.TryParse(value.ToString(), out var unitValue)) record.Unit_Purchase_Cost = unitValue;
                    break;
                case SalesRecordField.UnitSalePrice:
                    if (value is double price) record.Unit_Sale_Price = price;
                    else if (double.TryParse(value.ToString(), out var unitSalePriceValue)) record.Unit_Sale_Price = unitSalePriceValue;
                    break;
                case SalesRecordField.AllowedDiscount:
                    if (value is double discountAmount) record.Allowed_Discount = discountAmount;
                    else if (double.TryParse(value.ToString(), out var discountAmountValue)) record.Allowed_Discount = discountAmountValue;
                    break;
                default:
                    System.Diagnostics.Debug.Write("GetImportedData switch case for SalesRecord failed!");
                    break;
            }
        }
        return record;
    }

    /// <summary>
    /// Maps an array of string values parsed from a file into an ExpensesRecord object.
    /// Ensure the array indices match the logical order of ExpensesRecordField enum.
    /// </summary>
    public static ExpensesRecord MapToExpensesRecord(Dictionary<ExpensesRecordField, object> row)
    {
        return new ExpensesRecord
        {
            Date_Recorded = Convert.ToDateTime(row[ExpensesRecordField.DateRecorded]),
            Expense_Category = row[ExpensesRecordField.ExpenseCategory]?.ToString(),
            Specific_Type = row[ExpensesRecordField.SpecificType]?.ToString(),
            // Converts the generic object to double safely to match the model property
            Amount = Convert.ToDouble(row[ExpensesRecordField.Amount]),
            Asset_ID = row[ExpensesRecordField.AssetId]?.ToString()
        };
    }

    public static ExpensesRecord MapToExpenseRecord(Dictionary<ExpensesRecordField, object> row)
    {
        var record = new ExpensesRecord();

        foreach (var kvp in row)
        {

            var field = kvp.Key;
            var value = kvp.Value;
            if (value == null) continue;

            switch (field)
            {
                case ExpensesRecordField.DateRecorded:
                    if (value is DateTime dt) record.Date_Recorded = dt;
                    else if (DateTime.TryParse(value.ToString(), out var parsed)) record.Date_Recorded = parsed;
                    break;
                case ExpensesRecordField.ExpenseCategory:
                    record.Expense_Category = value.ToString();
                    break;
                case ExpensesRecordField.SpecificType:
                    record.Specific_Type = value.ToString();
                    break;
                case ExpensesRecordField.Amount:
                    if (value is double dv) record.Amount = dv;
                    else if (double.TryParse(value.ToString(), out var dv2)) record.Amount = dv2;
                    break;
                case ExpensesRecordField.AssetId:
                    record.Asset_ID = value.ToString();
                    break;
                default:
                    System.Diagnostics.Debug.Write("GetImportedExpensesData switch case for ExpensesRecord failed!");
                    break;
            }
        }


        return record;
    }


}