using GrpcService.Interfaces;

namespace GrpcService.Implementations;

public class MetadataContext : IMetadataContext
{
    private const string BUDGET_DB = "BudgetDatabase";

    private ISqlHelper _sqlHelper;

    public MetadataContext(ISqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    public async Task<bool> DoesCategoryExist(string category)
    {
        return await _sqlHelper.Exists(BUDGET_DB, "SELECT 1 FROM Category WHERE Category = @category", new { category });
    }

    public async Task AddCategory(string category)
    {
        await _sqlHelper.ExecuteAsync(BUDGET_DB, "INSERT INTO Category VALUES (@category)", new { category });
    }

    public async Task DeleteCategory(string category)
    {
        await _sqlHelper.ExecuteAsync(BUDGET_DB, "DELETE FROM Category WHERE Category = @category", new { category });
    }

    public async Task UpdateCategory(string category, string updateTo)
    {
        await _sqlHelper.ExecuteAsync(BUDGET_DB, "UPDATE Category SET Category = @updateTo WHERE Category = @category", new { category, updateTo });
    }

    public async Task<IEnumerable<string>> GetCategories()
    {
        return await _sqlHelper.QueryAsync<string>(BUDGET_DB, "SELECT Category FROM Category");
    }
}
