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

    public async Task<bool> DoesCategoryExist(string category)
    {
        return await _sqlHelper.Exists(_config["BudgetDatabaseName"], "SELECT 1 FROM Category WHERE Category = @category", new { category });
    }

    public async Task AddCategory(string category)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "INSERT INTO Category VALUES (@category)", new { category });
    }

    public async Task DeleteCategory(string category)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "DELETE FROM Category WHERE Category = @category", new { category });
    }

    public async Task UpdateCategory(string category, string updateTo)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"], "UPDATE Category SET Category = @updateTo WHERE Category = @category", new { category, updateTo });
    }

    public async Task<IEnumerable<string>> GetCategories()
    {
        return await _sqlHelper.QueryAsync<string>(_config["BudgetDatabaseName"], "SELECT Category FROM Category");
    }
}
