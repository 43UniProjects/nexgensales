using NexGenSales.Models;
using NexGenSales.Services.Data.Repository;

namespace NexGenSales.Services;
public class ExpensesService(ExpensesRecordRepository repository)
{
    public ExpensesRecordRepository _repository = repository;

    public async Task<List<ExpensesRecord>> GetExpensesByDateRange(DateTime startDate, DateTime endDate)
    {
        string sql = @"
        SELECT * FROM ExpensesRecord 
        WHERE Date_Recorded >= @StartDate AND Date_Recorded <= @EndDate;";

        var parameters = new Dictionary<string, object>
    {
        { "@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss") },
        { "@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss") }
    };

        return await _repository.GetMany(sql, parameters);
    }
    
    
} 