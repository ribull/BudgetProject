using Backend.Interfaces;
using Dapper;
using Domain.Models;

namespace Backend.Implementations;

public class PayHistoryContext : IPayHistoryContext
{
    private readonly IConfiguration _config;
    private readonly ISqlHelper _sqlHelper;

    public PayHistoryContext(IConfiguration config, ISqlHelper sqlHelper)
    {
        _config = config;
        _sqlHelper = sqlHelper;
    }

    public async Task AddPayHistoryAsync(PayHistory payHistory)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"],
@"INSERT INTO PayHistory
(
    PayPeriodStartDate,
    PayPeriodEndDate,
    Earnings,
    PreTaxDeductions,
    Taxes,
    PostTaxDeductions
)
VALUES
(
    @PayPeriodStartDate,
    @PayPeriodEndDate,
    @Earnings,
    @PreTaxDeductions,
    @Taxes,
    @PostTaxDeductions
)", payHistory);
    }

    public async Task AddPayHistoriesAsync(IEnumerable<PayHistory> payHistories)
    {
        foreach (PayHistory payHistory in payHistories)
        {
            await AddPayHistoryAsync(payHistory);
        }
    }

    public async Task<IEnumerable<PayHistory>> GetPayHistoriesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        List<string> wheres = new();
        DynamicParameters sqlParams = new();

        if (startDate is not null)
        {
            wheres.Add("PayPeriodStartDate >= @startDate");
            sqlParams.Add("startDate", startDate);
        }

        if (endDate is not null)
        {
            wheres.Add("PayPeriodEndDate <= @endDate");
            sqlParams.Add("endDate", endDate);
        }

        return await _sqlHelper.QueryAsync<PayHistory>(_config["BudgetDatabaseName"],
$@"SELECT
    PayHistoryId,
    PayPeriodStartDate,
    PayPeriodEndDate,
    Earnings,
    PreTaxDeductions,
    Taxes,
    PostTaxDeductions
FROM PayHistory
{(wheres.Any() ? $"WHERE {string.Join(" AND ", wheres)}" : string.Empty)}", sqlParams);
    }

    public async Task<IEnumerable<PayHistory>> GetPayHistoriesForMonthAsync(DateTime month)
    {
        return await GetPayHistoriesAsync(new DateTime(month.Year, month.Month, 1), new DateTime(month.Year, month.Month, 1).AddMonths(1).AddSeconds(-1));
    }
}