using Backend.Interfaces;

namespace Backend.Implementations;

public class MetadataContext : IMetadataContext
{
    private readonly IConfiguration _config;
    private readonly ISqlHelper _sqlHelper;

    public MetadataContext(IConfiguration config, ISqlHelper sqlHelper)
    {
        _config = config;
        _sqlHelper = sqlHelper;
    }

    public async Task<bool> DoesCategoryExistAsync(string category)
    {
        return await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"], "SELECT 1 FROM Category WHERE Category = @category", new { category });
    }

    public async Task AddCategoryAsync(string category)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "INSERT INTO Category VALUES (@category)", new { category });
    }

    public async Task DeleteCategoryAsync(string category)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "DELETE FROM Category WHERE Category = @category", new { category });
    }

    public async Task UpdateCategoryAsync(string category, string updateTo)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "UPDATE Category SET Category = @updateTo WHERE Category = @category", new { category, updateTo });
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _sqlHelper.QueryAsync<string>(_config["BudgetDatabaseName"], "SELECT Category FROM Category");
    }
}
