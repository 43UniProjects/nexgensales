# DataRepository Implementation Guide

We recently updated the project to use a real database repository instead of hard-coded mock data. This guide shows how to implement the SQL queries inside `DataRepository.cs`.

## 1. How the Date Range Works

The UI now captures the user's selected "Date Range" (e.g., 1 Month) and calculates a `startDate`.
This `startDate` is passed into the constructor of `DataRepository` and saved in a private field:

```csharp
private readonly DateTime _startDate; // Used to filter database queries

public DataRepository(DateTime startDate)
{
    _startDate = startDate;
}
```

## 2. How to Implement the Queries

Your task is to fill in the five methods inside `DataRepository.cs` with actual SQLite queries using the `SalesRecord` table.

**The Golden Rule:** Every query must filter for records that occurred **on or after** `_startDate`.

### Example Implementation

Here is an example of how you might implement the `GetItemVelocityData` method:

```csharp
public (string[] Items, int[] QuantitiesSold) GetItemVelocityData()
{
    // 1. Write the SQL Query, filtering by _startDate
    string sql = @"
        SELECT Item_ID, SUM(Quantity_Sold) as TotalSold 
        FROM SalesRecord 
        WHERE Date_Time >= @StartDate
        GROUP BY Item_ID
        ORDER BY TotalSold DESC
        LIMIT 10
    ";

    // 2. Setup Lists to hold the results
    var items = new List<string>();
    var quantities = new List<int>();

    // 3. Execute the query using SQLite
    // (Assuming you instantiate a SqliteConnection or use an existing Database service)
    using var connection = new SqliteConnection("Data Source=Database/app.db");
    connection.Open();
    using var command = new SqliteCommand(sql, connection);
    
    // 4. Pass the _startDate to the query parameter
    command.Parameters.AddWithValue("@StartDate", _startDate);

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        items.Add(reader.GetString(0));
        quantities.Add(reader.GetInt32(1));
    }

    // 5. Return the expected arrays
    return (items.ToArray(), quantities.ToArray());
}
```

## 3. Checklist for Methods

Inside `DataRepository.cs`, look for the `TODO` comments. Here's a quick summary of what needs to be calculated for each method:

- **`GetSupplierProfitabilityData`**: Group by `Supplier_ID`. Calculate Total Revenue & Total Cost to get Profit Ratio.
- **`GetItemVelocityData`**: Group by `Item_ID` and sum `Quantity_Sold`.
- **`GetRevenueContributionData`**: Group by `Item_ID` and sum `Net_Revenue`.
- **`GetTrendAnalysisData`**: Group by the Day of `Date_Time` and sum `Net_Revenue` for daily trends.
- **`GetDiscountEffectivenessData`**: Group by `Allowed_Discount` tier and sum `Net_Revenue` to see which discount level is most effective.

> **Tip**: If you need to see the exact structure of the `SalesRecord` table, check `Models/SalesRecord.cs` or the `SalesRecordRepository` initialization!
