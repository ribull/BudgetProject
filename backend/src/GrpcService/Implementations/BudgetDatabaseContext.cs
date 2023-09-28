using Backend.Exceptions;
using Backend.Interfaces;
using Dapper;
using Domain.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace Backend.Implementations;

public class BudgetDatabaseContext : IBudgetDatabaseContext
{
    private readonly ISqlHelper _sqlHelper;

    private readonly string _budgetDatabaseName;

    public BudgetDatabaseContext(IConfiguration config, ISqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
        _budgetDatabaseName = config["BudgetDatabaseName"];
    }

    #region Category

    public async Task<bool> DoesCategoryExistAsync(string category)
    {
        return await _sqlHelper.ExistsAsync(_budgetDatabaseName, "SELECT 1 FROM Category WHERE Category = @category", new { category });
    }

    public async Task AddCategoryAsync(string category)
    {
        await _sqlHelper.ExecuteAsync(_budgetDatabaseName, "INSERT INTO Category VALUES (@category)", new { category });
    }

    public async Task DeleteCategoryAsync(string category)
    {
        await _sqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Category WHERE Category = @category", new { category });
    }

    public async Task UpdateCategoryAsync(string category, string updateTo)
    {
        await _sqlHelper.ExecuteAsync(_budgetDatabaseName, "UPDATE Category SET Category = @updateTo WHERE Category = @category", new { category, updateTo });
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _sqlHelper.QueryAsync<string>(_budgetDatabaseName, "SELECT Category FROM Category");
    }

    #endregion Category

    #region Purchase

    public async Task<IEnumerable<Purchase>> GetPurchasesAsync(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        List<string> wheres = new();
        DynamicParameters sqlParams = new();

        if (description is not null)
        {
            wheres.Add("Description = @description");
            sqlParams.Add("description", description);
        }

        if (category is not null)
        {
            wheres.Add("Category = @category");
            sqlParams.Add("category", category);
        }

        if (startDate is not null)
        {
            wheres.Add("Date >= @startDate");
            sqlParams.Add("startDate", startDate);
        }

        if (endDate is not null)
        {
            wheres.Add("Date <= @endDate");
            sqlParams.Add("endDate", endDate);
        }

        return await _sqlHelper.QueryAsync<Purchase>(_budgetDatabaseName,
@$"SELECT
    PurchaseId,
    Date,
    Description,
    Amount,
    Category
FROM Purchase p
LEFT JOIN Category c
    ON p.CategoryId = c.CategoryId
{(wheres.Any() ? $"WHERE {string.Join(" AND ", wheres)}" : string.Empty)}", sqlParams);
    }

    public async Task AddPurchaseAsync(Purchase purchase)
    {
        if (purchase.Category is null)
        {
            throw new ArgumentException("Category is null. You can only add a purchase with a category.");
        }

        if (!await DoesCategoryExistAsync(purchase.Category))
        {
            throw new CategoryDoesNotExistException(purchase.Category);
        }

        await _sqlHelper.ExecuteAsync(_budgetDatabaseName,
$@"INSERT INTO Purchase
(
    Date,
    Description,
    Amount,
    CategoryId
)
VALUES
(
    @Date,
    @Description,
    @Amount,
    (SELECT CategoryId FROM Category WHERE Category = @Category)
)", purchase);
    }

    public async Task AddPurchasesAsync(IAsyncEnumerable<Purchase> purchases)
    {
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await foreach (Purchase purchase in purchases)
            {
                if (purchase.Category is null)
                {
                    throw new ArgumentException("Category is null. You can only add a purchase with a category.");
                }

                if (!await DoesCategoryExistAsync(purchase.Category))
                {
                    throw new CategoryDoesNotExistException(purchase.Category);
                }

                await AddPurchaseAsync(purchase);
            }

            scope.Complete();
        }
    }

    #endregion Purchase

    #region PayHistory

    public async Task AddPayHistoryAsync(PayHistory payHistory)
    {
        await _sqlHelper.ExecuteAsync(_budgetDatabaseName,
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
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            foreach (PayHistory payHistory in payHistories)
            {
                await AddPayHistoryAsync(payHistory);
            }

            scope.Complete();
        }
    }

    public async Task AddPayHistoriesAsync(IAsyncEnumerable<PayHistory> payHistories)
    {
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await foreach (PayHistory payHistory in payHistories)
            {
                await AddPayHistoryAsync(payHistory);
            }

            scope.Complete();
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

        return await _sqlHelper.QueryAsync<PayHistory>(_budgetDatabaseName,
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

    #endregion PayHistory
}
