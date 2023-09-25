using BackendFunctionalTests.Helpers;
using Domain.Models;
using Backend.Implementations;
using Backend.Interfaces;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;

namespace BackendFunctionalTests;

[TestFixture]
public class PurchasesContextFunctionalTests
{
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IPurchasesContext _purchasesContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker("purchases-context-tests-sqlserver-", "BudgetDatabase");
        await _budgetDatabaseDocker.StartContainer();

        _sqlHelper = new SqlHelper(_budgetDatabaseDocker.ConnectionString!);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "BudgetDatabaseName", _budgetDatabaseDocker.DatabaseName }
            }).Build();

        _purchasesContext = new PurchasesContext(_config, _sqlHelper);
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
    public async Task AddPurchaseTest()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        Purchase testPurchase = new Purchase
        {
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act
        await _purchasesContext.AddPurchaseAsync(testPurchase);

        // Assert
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName,
$@"SELECT
    CategoryId
FROM Purchase
WHERE
    Date = '{testPurchase.Date}'
    AND Description = '{testPurchase.Description}'
    AND Amount = {testPurchase.Amount}")).Single(), Is.EqualTo(categoryId));
    }

    [Test]
    public async Task GetPurchasesTest()
    {
        // Arrange
        Dictionary<string, int> categoryMap = new()
        {
            { "TestCategory1", 1 },
            { "TestCategory1234", 1234 },
            { "Utilities", 2 },
            { "Hello", 5 }
        };

        List<Purchase> testPurchases = new()
        {
            new Purchase
            {
                PurchaseId = 1,
                Date = new DateTime(2023, 9, 21),
                Description = "TestDescription",
                Category = categoryMap.ElementAt(0).Key,
                Amount = 152.89
            },
            new Purchase
            {
                PurchaseId = 2,
                Date = new DateTime(2023, 9, 21),
                Description = "TestDescription2",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 188.18
            },
            new Purchase
            {
                PurchaseId = 3,
                Date = new DateTime(2023, 8, 13),
                Description = "TestDescriptionX",
                Category = categoryMap.ElementAt(1).Key,
                Amount = 19.19
            },
            new Purchase
            {
                PurchaseId = 10,
                Date = new DateTime(2023, 10, 17),
                Description = "TestDescriptionX",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 26.67
            },
            new Purchase
            {
                PurchaseId = 101,
                Date = new DateTime(2023, 10, 18),
                Description = "TestDescriptionX",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 29.97
            }
        };

        foreach (KeyValuePair<string, int> keyValuePair in categoryMap)
        {
            await AddCategory(keyValuePair.Value, keyValuePair.Key);
        }

        foreach (Purchase purchase in testPurchases)
        {
            await AddPurchase(purchase, categoryMap);
        }

        // Act + Assert
        IEnumerable<Purchase> allPurchases = await _purchasesContext.GetPurchasesAsync();
        Assert.That(allPurchases, Is.EquivalentTo(testPurchases));

        IEnumerable<Purchase> utilitiesPurchases = await _purchasesContext.GetPurchasesAsync(category: "Utilities");
        Assert.That(utilitiesPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Category == "Utilities")));

        IEnumerable<Purchase> dateAfterPurchases = await _purchasesContext.GetPurchasesAsync(startDate: new DateTime(2023, 10, 1));
        Assert.That(dateAfterPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date >= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> dateBeforePurchases = await _purchasesContext.GetPurchasesAsync(endDate: new DateTime(2023, 10, 1));
        Assert.That(dateBeforePurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date <= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> descriptionPurchases = await _purchasesContext.GetPurchasesAsync(description: "TestDescriptionX");
        Assert.That(descriptionPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Description == "TestDescriptionX")));

        IEnumerable<Purchase> specificPurchase = await _purchasesContext.GetPurchasesAsync(description: "TestDescriptionX", category: "Utilities", startDate: new DateTime(2023, 10, 17), endDate: new DateTime(2023, 10, 17));
        Assert.That(specificPurchase, Is.EquivalentTo(testPurchases.Where(tp => tp.Description == "TestDescriptionX" && tp.Category == "Utilities" && tp.Date == new DateTime(2023, 10, 17))));
    }

    [Test]
    public async Task GetPurchasesNullCategoryTest()
    {
        // Arrange
        // Simulate a deleted category
        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName,
$@"INSERT INTO Purchase
(
    Date,
    Description,
    Amount,
    CategoryId
)
VALUES
(
    '{new DateTime(2023, 10, 10)}',
    'Description',
    123.45,
    NULL
)");

        // Act
        IEnumerable<Purchase> purchases = await _purchasesContext.GetPurchasesAsync();

        // Assert
        Assert.That(purchases.Single().Category, Is.Null);
    }

    private async Task AddPurchase(Purchase purchase, Dictionary<string, int> categoryMap)
    {
        if (purchase.Category is null)
        {
            throw new Exception("The category is null, you should have passed a purchase without a null category");
        }

        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName,
$@"SET IDENTITY_INSERT Purchase ON;

INSERT INTO Purchase
(
    PurchaseId,
    Date,
    Description,
    Amount,
    CategoryId
)
VALUES
(
    {purchase.PurchaseId},
    '{purchase.Date}',
    '{purchase.Description}',
    {purchase.Amount},
    {categoryMap[purchase.Category]}
);

SET IDENTITY_INSERT Purchase OFF;");
    }

    private async Task AddCategory(int id, string category)
    {
        await _sqlHelper.ExecuteAsync(_budgetDatabaseDocker.DatabaseName,
$@"SET IDENTITY_INSERT Category ON;

INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {id},
    '{category}'
);

SET IDENTITY_INSERT Category OFF;");
    }
}