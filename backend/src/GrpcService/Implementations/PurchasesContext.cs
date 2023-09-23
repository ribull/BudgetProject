using Dapper;
using Domain.Models;
using Backend.Interfaces;

namespace Backend.Implementations;

public class PurchasesContext : IPurchasesContext
{
    private readonly IConfiguration _config;
    private readonly ISqlHelper _sqlHelper;

    public PurchasesContext(IConfiguration config, ISqlHelper sqlHelper)
    {
        _config = config;
        _sqlHelper = sqlHelper;
    }

    public async Task<IEnumerable<Purchase>> GetPurchases(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
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

        return await _sqlHelper.QueryAsync<Purchase>(_config["BudgetDatabaseName"],
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

    public async Task AddPurchase(Purchase purchase)
    {
        if (purchase.Category is null)
        {
            throw new Exception("The category is null. You can only add a purchase with a category.");
        }

        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"],
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
}
