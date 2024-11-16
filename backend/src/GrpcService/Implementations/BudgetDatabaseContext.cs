using Backend.Exceptions;
using Backend.Interfaces;
using Dapper;
using Domain.Models;
using System.Transactions;

namespace Backend.Implementations;

public class BudgetDatabaseContext : IBudgetDatabaseContext
{
    private readonly ISqlHelper SqlHelper;

    private readonly string _budgetDatabaseName;

    public BudgetDatabaseContext(IConfiguration config, ISqlHelper sqlHelper)
    {
        SqlHelper = sqlHelper;
        _budgetDatabaseName = config["BudgetDatabaseName"]!;
    }

    #region Category

    public async Task<bool> DoesCategoryExistAsync(string category)
    {
        return await SqlHelper.ExistsAsync(_budgetDatabaseName, "SELECT 1 FROM Category WHERE Category = @category", new { category });
    }

    public async Task AddCategoryAsync(string category)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName, "INSERT INTO Category (Category) VALUES (@category)", new { category });
    }

    public async Task DeleteCategoryAsync(string category)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Category WHERE Category = @category", new { category });
    }

    public async Task UpdateCategoryAsync(string category, string updateTo)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName, "UPDATE Category SET Category = @updateTo WHERE Category = @category", new { category, updateTo });
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await SqlHelper.QueryAsync<string>(_budgetDatabaseName, "SELECT Category FROM Category");
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

        return await SqlHelper.QueryAsync<Purchase>(_budgetDatabaseName,
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

    public async Task<IEnumerable<Purchase>> GetMostCommonPurchasesAsync(string? category = null, DateTime? startDate = null, DateTime? endDate = null, int? count = null)
    {
        List<string> wheres = new();
        DynamicParameters sqlParams = new();

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

        string? limit = null;
        if (count is not null)
        {
            limit = $"LIMIT {count}";
        }

        return await SqlHelper.QueryAsync<Purchase>(_budgetDatabaseName,
$@"SELECT
    PurchaseId,
    Date,
    Description,
    Amount,
    Category
FROM (
    SELECT
        PurchaseId,
        Date,
        Description,
        Amount,
        Category,
        ROW_NUMBER() OVER (PARTITION BY p.Description ORDER BY p.Date DESC) AS row_num
    FROM
        Purchase p
    LEFT JOIN Category c
        ON p.CategoryId = c.CategoryId
    {(wheres.Any() ? $"WHERE {string.Join(" AND ", wheres)}" : string.Empty)}
) lp
WHERE
    lp.row_num = 1
ORDER BY
    (SELECT COUNT(*) FROM Purchase p2 WHERE p2.Description = lp.Description) DESC
{limit}", sqlParams);
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

        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
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
                    await AddCategoryAsync(purchase.Category);
                }

                await AddPurchaseAsync(purchase);
            }

            scope.Complete();
        }
    }

    public async Task UpdatePurchaseAsync(Purchase purchase)
    {
        if (purchase.Category is null)
        {
            throw new ArgumentException("Category is null. You can only add a purchase with a category.");
        }

        if (!await DoesCategoryExistAsync(purchase.Category))
        {
            throw new CategoryDoesNotExistException(purchase.Category);
        }

        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
$@"UPDATE Purchase
SET
    Date = @Date,
    Description = @Description,
    Amount = @Amount,
    CategoryId = (SELECT CategoryId FROM Category WHERE Category = @Category)
WHERE
    PurchaseId = @PurchaseId", purchase);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A purchase with that purchase id does not exist.");
        }
    }

    public async Task DeletePurchaseAsync(int purchaseId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
$@"DELETE FROM Purchase
WHERE
    PurchaseId = @purchaseId", new { purchaseId } );

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A purchase with that purchase id does not exist.");
        }
    }

    #endregion Purchase

    #region PayHistory

    public async Task AddPayHistoryAsync(PayHistory payHistory)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
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

    public async Task UpdatePayHistoryAsync(PayHistory payHistory)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE PayHistory SET
    PayPeriodStartDate = @PayPeriodStartDate,
    PayPeriodEndDate = @PayPeriodEndDate,
    Earnings = @Earnings,
    PreTaxDeductions = @PreTaxDeductions,
    Taxes = @Taxes,
    PostTaxDeductions = @PostTaxDeductions
WHERE PayHistoryId = @PayHistoryId", payHistory);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A pay history with that pay history id does not exist.");
        }
    }

    public async Task<bool> DoesPayHistoryExistAsync(int payHistoryId)
    {
        return await SqlHelper.ExistsAsync(_budgetDatabaseName, "SELECT 1 FROM PayHistory WHERE PayHistoryId = @payHistoryId", new { payHistoryId });
    }

    public async Task DeletePayHistoryAsync(int payHistoryId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM PayHistory WHERE PayHistoryId = @payHistoryId", new { payHistoryId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A pay history with that pay history id does not exist.");
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

        return await SqlHelper.QueryAsync<PayHistory>(_budgetDatabaseName,
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

    #region Era

    public async Task<IEnumerable<Era>> GetErasAsync()
    {
        return await SqlHelper.QueryAsync<Era>(_budgetDatabaseName,
@"SELECT
    EraId,
    Name,
    StartDate,
    EndDate
FROM Era");
    }

    public async Task AddEraAsync(Era era)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"INSERT INTO Era
(
    Name,
    StartDate,
    EndDate
)
VALUES
(
    @Name,
    @StartDate,
    @EndDate
)", era);
    }

    public async Task UpdateEraAsync(Era era)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE Era SET
    Name = @Name,
    StartDate = @StartDate,
    EndDate = @EndDate
WHERE EraId = @EraId", era);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("An era with that era id does not exist.");
        }
    }

    public async Task DeleteEraAsync(int eraId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Era WHERE EraId = @eraId", new { eraId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("An era with that era id does not exist.");
        }
    }

    #endregion Era

    #region FuturePurchase

    public async Task<IEnumerable<FuturePurchase>> GetFuturePurchasesAsync()
    {
        return await SqlHelper.QueryAsync<FuturePurchase>(_budgetDatabaseName,
@"SELECT
    FuturePurchaseId,
    Date,
    Description,
    Amount,
    Category
FROM FuturePurchase fp
LEFT JOIN Category c
    ON fp.CategoryId = c.CategoryId");
    }

    public async Task AddFuturePurchaseAsync(FuturePurchase futurePurchase)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"INSERT INTO FuturePurchase
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
)", futurePurchase);
    }

    public async Task UpdateFuturePurchaseAsync(FuturePurchase futurePurchase)
    {
        if (futurePurchase.Category is null)
        {
            throw new ArgumentException("Category is null. You can only add a purchase with a category.");
        }

        if (!await DoesCategoryExistAsync(futurePurchase.Category))
        {
            throw new CategoryDoesNotExistException(futurePurchase.Category);
        }

        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE FuturePurchase SET
    Date = @Date,
    Description = @Description,
    Amount = @Amount,
    CategoryId = (SELECT CategoryId FROM Category WHERE Category = @Category)
WHERE FuturePurchaseId = @FuturePurchaseId", futurePurchase);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A purchase with that purchase id does not exist.");
        }
    }

    public async Task DeleteFuturePurchaseAsync(int futurePurchaseId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM FuturePurchase WHERE FuturePurchaseId = @futurePurchaseId", new { futurePurchaseId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A purchase with that purchase id does not exist.");
        }
    }

    #endregion FuturePurchase

    #region Investments

    public async Task<IEnumerable<Investment>> GetInvestmentsAsync()
    {
        return await SqlHelper.QueryAsync<Investment>(_budgetDatabaseName,
@"SELECT
    InvestmentId,
    Description,
    CurrentAmount,
    YearlyGrowthRate,
    LastUpdated
FROM Investment");
    }

    public async Task AddInvestmentAsync(Investment investment)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"INSERT INTO Investment
(
    Description,
    CurrentAmount,
    YearlyGrowthRate,
    LastUpdated
)
VALUES
(
    @Description,
    @CurrentAmount,
    @YearlyGrowthRate,
    (SELECT CURRENT_DATE)
)", investment);
    }

    public async Task UpdateInvestmentAsync(Investment investment)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE Investment SET
    Description = @Description,
    CurrentAmount = @CurrentAmount,
    YearlyGrowthRate = @YearlyGrowthRate,
    LastUpdated = (SELECT CURRENT_DATE)
WHERE InvestmentId = @InvestmentId", investment);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("An investment with that investment id does not exist.");
        }
    }

    public async Task DeleteInvestmentAsync(int investmentId)
    {
        int modifiedRows =  await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Investment WHERE InvestmentId = @investmentId", new { investmentId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("An investment with that investment id does not exist.");
        }
    }

    #endregion Investments

    #region Saved

    public async Task<IEnumerable<Saved>> GetSavingsAsync()
    {
        return await SqlHelper.QueryAsync<Saved>(_budgetDatabaseName,
@"SELECT
    SavedId,
    Date,
    Description,
    Amount
FROM Saved");
    }

    public async Task AddSavingAsync(Saved saved)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"INSERT INTO Saved
(
    Date,
    Description,
    Amount
)
VALUES
(
    @Date,
    @Description,
    @Amount
)", saved);
    }

    public async Task UpdateSavingAsync(Saved saved)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE Saved SET
    Date = @Date,
    Description = @Description,
    Amount = @Amount
WHERE SavedId = @SavedId", saved);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A saving with that saved id does not exist.");
        }
    }

    public async Task DeleteSavedAsync(int savedId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Saved WHERE SavedId = @savedId", new { savedId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A saving with that saved id does not exist.");
        }
    }

    #endregion Saved

    #region Wishlist

    public async Task<IEnumerable<WishlistItem>> GetWishlistAsync()
    {
        return await SqlHelper.QueryAsync<WishlistItem>(_budgetDatabaseName,
@"SELECT
    WishlistItemId,
    Description,
    Amount,
    Notes
FROM Wishlist");
    }

    public async Task AddWishlistItemAsync(WishlistItem wishlistItem)
    {
        await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"INSERT INTO Wishlist
(
    Description,
    Amount,
    Notes
)
VALUES
(
    @Description,
    @Amount,
    @Notes
)", wishlistItem);
    }

    public async Task UpdateWishlistItemAsync(WishlistItem wiishlistItem)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName,
@"UPDATE Wishlist SET
    Description = @Description,
    Amount = @Amount,
    Notes = @Notes
WHERE WishlistItemId = @WishlistItemId", wiishlistItem);

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A wishlist item with that wishlist item id does not exist.");
        }
    }

    public async Task DeleteWishlistItemAsync(int wishlistItemId)
    {
        int modifiedRows = await SqlHelper.ExecuteAsync(_budgetDatabaseName, "DELETE FROM Wishlist WHERE WishlistItemId = @wishlistItemId", new { wishlistItemId });

        if (modifiedRows == 0)
        {
            throw new ArgumentException("A wishlist item with that wishlist item id does not exist.");
        }
    }

    #endregion Wishlist
}
