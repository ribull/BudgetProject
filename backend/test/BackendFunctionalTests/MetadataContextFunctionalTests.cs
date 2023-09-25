using BackendFunctionalTests.Helpers;
using Backend.Implementations;
using Backend.Interfaces;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;

namespace BackendFunctionalTests;

[TestFixture]
public class MetadataContextFunctionalTests
{
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IMetadataContext _metadataContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker("metadata-context-tests-sqlserver-", "BudgetDatabase");
        await _budgetDatabaseDocker.StartContainer();

        _sqlHelper = new SqlHelper(_budgetDatabaseDocker.ConnectionString!);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "BudgetDatabaseName", _budgetDatabaseDocker.DatabaseName }
            }).Build();

        _metadataContext = new MetadataContext(_config, _sqlHelper);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _budgetDatabaseDocker.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        _budgetDatabaseDocker.DeployBudgetDb();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _budgetDatabaseDocker.DropDb();
    }

    [Test]
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";

        // Act
        await _metadataContext.AddCategory(category);

        // Assert
        Assert.That(await _sqlHelper.Exists(_budgetDatabaseDocker.DatabaseName, "SELECT 1 FROM Category WHERE Category = 'Utilities'"));
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        int categoryId = 101;
        string category = "Utilities";
        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName,
$@"SET IDENTITY_INSERT Category ON;

INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {categoryId},
    '{category}'
)

SET IDENTITY_INSERT Category OFF;");

        string description = "xx_Description_xx";
        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName, $"INSERT INTO Purchase VALUES ('{new DateTime(2023, 9, 22)}', '{description}', 123.45, {categoryId})");

        // Sanity check
        Assert.That(await _sqlHelper.Exists(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"));
        Assert.That(await _sqlHelper.Exists(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Purchase WHERE Description = '{description}' AND CategoryId = {categoryId}"));

        // Act
        await _metadataContext.DeleteCategory(category);

        // Assert
        Assert.That(await _sqlHelper.Exists(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"), Is.False);
        Assert.That((await _sqlHelper.QueryAsync<int?>(_budgetDatabaseDocker.DatabaseName, $"SELECT CategoryId FROM Purchase WHERE Description = '{description}'")).Single(), Is.Null);
    }

    [Test]
    public async Task GetCategoriesTest()
    {
        // Arrange
        List<string> testCategories = new()
        {
            "Utilities",
            "Gas",
            "Test",
            "Gifts",
            "Balderdash",
            "Random Category",
            "AYEEEEEEEEEEeeee"
        };

        foreach (string testCategory in testCategories)
        {
            await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName, $"INSERT INTO Category VALUES ('{testCategory}')");
        }

        // Act
        IEnumerable<string> resultCategories = await _metadataContext.GetCategories();

        // Assert
        Assert.That(resultCategories, Is.EquivalentTo(testCategories));
    }

    [Test]
    public async Task DoesCategoryExistTest()
    {
        // Arrange
        string category = "Utilities";
        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName, $"INSERT INTO Category VALUES ('{category}')");

        // Act + Assert
        Assert.That(await _metadataContext.DoesCategoryExist(category), Is.True);
        Assert.That(await _metadataContext.DoesCategoryExist("Does not exist category"), Is.False);
    }
}
