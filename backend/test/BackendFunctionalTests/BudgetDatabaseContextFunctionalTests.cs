using Backend.Exceptions;
using Backend.Implementations;
using Backend.Interfaces;
using BackendFunctionalTests.Helpers;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BackendFunctionalTests;

public class BudgetDatabaseContextFunctionalTests
{
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IBudgetDatabaseContext _budgetDatabaseContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker("budget-database-context-tests-sqlserver-", "BudgetDatabase");
        await _budgetDatabaseDocker.StartContainer();

        _sqlHelper = new SqlHelper(_budgetDatabaseDocker.ConnectionString!);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "BudgetDatabaseName", _budgetDatabaseDocker.DatabaseName }
            }).Build();

        _budgetDatabaseContext = new BudgetDatabaseContext(_config, _sqlHelper);
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

    #region Category

    [Test]
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";

        // Act
        await _budgetDatabaseContext.AddCategoryAsync(category);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_budgetDatabaseDocker.DatabaseName, "SELECT 1 FROM Category WHERE Category = 'Utilities'"));
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
        Assert.That(await _sqlHelper.ExistsAsync(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"));
        Assert.That(await _sqlHelper.ExistsAsync(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Purchase WHERE Description = '{description}' AND CategoryId = {categoryId}"));

        // Act
        await _budgetDatabaseContext.DeleteCategoryAsync(category);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_budgetDatabaseDocker.DatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"), Is.False);
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
        IEnumerable<string> resultCategories = await _budgetDatabaseContext.GetCategoriesAsync();

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
        Assert.That(await _budgetDatabaseContext.DoesCategoryExistAsync(category), Is.True);
        Assert.That(await _budgetDatabaseContext.DoesCategoryExistAsync("Does not exist category"), Is.False);
    }

    #endregion Category

    #region Purchase

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
        await _budgetDatabaseContext.AddPurchaseAsync(testPurchase);

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
    public void AddPurchaseCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "TestCategory";

        Purchase testPurchase = new Purchase
        {
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act + Assert
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await _budgetDatabaseContext.AddPurchaseAsync(testPurchase));
    }

    [Test]
    public async Task AddPurchasesTest()
    {
        // Arrange
        string category = "TestCategory";
        int categoryId = 100;
        await AddCategory(categoryId, category);

        async IAsyncEnumerable<Purchase> GetAsyncPurchases()
        {
            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 21),
                Description = "Test Description",
                Amount = 123.45,
                Category = category
            };

            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 22),
                Description = "Test Description2",
                Amount = 678.9,
                Category = category
            };

            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 23),
                Description = "Test Description3",
                Amount = 1234.56,
                Category = category
            };

            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 24),
                Description = "Test Description4",
                Amount = 81924.23,
                Category = category
            };

            await Task.CompletedTask;
        };

        // Act
        await _budgetDatabaseContext.AddPurchasesAsync(GetAsyncPurchases());

        // Assert
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName, $"SELECT COUNT(*) FROM Purchase WHERE CategoryId = {categoryId}")).Single(), Is.EqualTo(4));
    }

    [Test]
    public async Task AddPurchasesRollbackTest()
    {
        // Arrange
        string category = "TestCategory";
        int categoryId = 100;
        await AddCategory(categoryId, category);

        async IAsyncEnumerable<Purchase> GetAsyncPurchases()
        {
            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 21),
                Description = "Test Description",
                Amount = 123.45,
                Category = category
            };

            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 22),
                Description = "Test Description2",
                Amount = 678.9,
                Category = category
            };

            yield return new Purchase
            {
                Date = new DateTime(2023, 9, 23),
                Description = "Test Description3",
                Amount = 1234.56,
                Category = "Non-existing Category"
            };

            await Task.CompletedTask;
        };

        // Act + Assert
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await _budgetDatabaseContext.AddPurchasesAsync(GetAsyncPurchases()));

        // Make sure the transaction rolled back
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName, "SELECT COUNT(*) FROM Purchase")).Single(), Is.EqualTo(0));
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
        IEnumerable<Purchase> allPurchases = await _budgetDatabaseContext.GetPurchasesAsync();
        Assert.That(allPurchases, Is.EquivalentTo(testPurchases));

        IEnumerable<Purchase> utilitiesPurchases = await _budgetDatabaseContext.GetPurchasesAsync(category: "Utilities");
        Assert.That(utilitiesPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Category == "Utilities")));

        IEnumerable<Purchase> dateAfterPurchases = await _budgetDatabaseContext.GetPurchasesAsync(startDate: new DateTime(2023, 10, 1));
        Assert.That(dateAfterPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date >= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> dateBeforePurchases = await _budgetDatabaseContext.GetPurchasesAsync(endDate: new DateTime(2023, 10, 1));
        Assert.That(dateBeforePurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date <= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> descriptionPurchases = await _budgetDatabaseContext.GetPurchasesAsync(description: "TestDescriptionX");
        Assert.That(descriptionPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Description == "TestDescriptionX")));

        IEnumerable<Purchase> specificPurchase = await _budgetDatabaseContext.GetPurchasesAsync(description: "TestDescriptionX", category: "Utilities", startDate: new DateTime(2023, 10, 17), endDate: new DateTime(2023, 10, 17));
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
        IEnumerable<Purchase> purchases = await _budgetDatabaseContext.GetPurchasesAsync();

        // Assert
        Assert.That(purchases.Single().Category, Is.Null);
    }

    #endregion Purchase

    #region PayHistory

    [Test]
    public async Task AddPayHistoryTest()
    {
        // Arrange
        PayHistory payHistory = new()
        {
            PayPeriodStartDate = new DateTime(2023, 10, 1),
            PayPeriodEndDate = new DateTime(2023, 10, 15),
            Earnings = 12345.67,
            PreTaxDeductions = 9876.54,
            Taxes = 321.09,
            PostTaxDeductions = 87.65
        };

        // Act
        await _budgetDatabaseContext.AddPayHistoryAsync(payHistory);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_budgetDatabaseDocker.DatabaseName,
$@"SELECT 1 FROM PayHistory
WHERE
    PayPeriodStartDate = '{payHistory.PayPeriodStartDate}'
    AND PayPeriodEndDate = '{payHistory.PayPeriodEndDate}'
    AND Earnings = {payHistory.Earnings}
    AND PreTaxDeductions = {payHistory.PreTaxDeductions}
    AND Taxes = {payHistory.Taxes}
    AND PostTaxDeductions = {payHistory.PostTaxDeductions}"));
    }

    [Test]
    public async Task AddPayHistoriesTest()
    {
        // Arrange
        List<PayHistory> payHistories = new()
        {
            new()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 67.89
            },
            new()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 15),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
        };

        // Act
        await _budgetDatabaseContext.AddPayHistoriesAsync(payHistories);

        // Assert
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(payHistories.Count));
    }

    [Test]
    public async Task AddAsyncPayHistoriesTest()
    {
        // Arrange
        async IAsyncEnumerable<PayHistory> GetAsyncPayHistories()
        {
            yield return new PayHistory()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            };

            yield return new PayHistory()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 67.89
            };

            yield return new PayHistory()
            {
                PayPeriodStartDate = new DateTime(2023, 10, 15),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            };

            await Task.CompletedTask;
        };

        // Act
        await _budgetDatabaseContext.AddPayHistoriesAsync(GetAsyncPayHistories());

        // Assert
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(3));
    }

    [Test]
    [TestCase("10/1/2023", "11/16/2023")]
    [TestCase("8/1/2023", "10/16/2023")]
    [TestCase(null, "10/16/2023")]
    [TestCase("10/1/2023", null)]
    [TestCase(null, null)]
    public async Task GetPayHistoriesTest(DateTime? startDate, DateTime? endDate)
    {
        // Arrange
        List<PayHistory> payHistories = new()
        {
            new()
            {
                PayHistoryId = 1,
                PayPeriodStartDate = new DateTime(2023, 9, 1),
                PayPeriodEndDate = new DateTime(2023, 9, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 2,
                PayPeriodStartDate = new DateTime(2023, 9, 1),
                PayPeriodEndDate = new DateTime(2023, 9, 15),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 67.89
            },
            new()
            {
                PayHistoryId = 1001,
                PayPeriodStartDate = new DateTime(2023, 9, 15),
                PayPeriodEndDate = new DateTime(2023, 9, 30),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 9,
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 10,
                PayPeriodStartDate = new DateTime(2023, 10, 15),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 1234,
                PayPeriodStartDate = new DateTime(2023, 11, 1),
                PayPeriodEndDate = new DateTime(2023, 11, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
        };

        foreach (PayHistory payHistory in payHistories)
        {
            await InsertPayHistory(payHistory);
        }

        // Act
        IEnumerable<PayHistory> payHistoriesResult = await _budgetDatabaseContext.GetPayHistoriesAsync(startDate, endDate);

        // Assert
        Assert.That(payHistoriesResult, Is.EquivalentTo(payHistories.Where(ph => (startDate is null ? true : ph.PayPeriodStartDate >= startDate) && (endDate is null ? true : ph.PayPeriodEndDate <= endDate))));
    }

    [Test]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    public async Task GetPayHistoriesForMonthTest(int month)
    {
        // Arrange
        DateTime monthDateTime = new DateTime(2023, month, 1);

        List<PayHistory> payHistories = new()
        {
            new()
            {
                PayHistoryId = 1,
                PayPeriodStartDate = new DateTime(2023, 9, 1),
                PayPeriodEndDate = new DateTime(2023, 9, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 2,
                PayPeriodStartDate = new DateTime(2023, 9, 1),
                PayPeriodEndDate = new DateTime(2023, 9, 15),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 67.89
            },
            new()
            {
                PayHistoryId = 1001,
                PayPeriodStartDate = new DateTime(2023, 9, 15),
                PayPeriodEndDate = new DateTime(2023, 9, 30),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 9,
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 10,
                PayPeriodStartDate = new DateTime(2023, 10, 15),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
            new()
            {
                PayHistoryId = 1234,
                PayPeriodStartDate = new DateTime(2023, 11, 1),
                PayPeriodEndDate = new DateTime(2023, 11, 15),
                Earnings = 12345.67,
                PreTaxDeductions = 9876.54,
                Taxes = 321.09,
                PostTaxDeductions = 87.65
            },
        };

        foreach (PayHistory payHistory in payHistories)
        {
            await InsertPayHistory(payHistory);
        }

        // Act
        IEnumerable<PayHistory> payHistoriesResult = await _budgetDatabaseContext.GetPayHistoriesForMonthAsync(monthDateTime);

        // Assert
        Assert.That(payHistoriesResult, Is.EquivalentTo(payHistories.Where(ph => ph.PayPeriodStartDate.Month == month && ph.PayPeriodEndDate.Month == month)));
    }

    #endregion PayHistory

    #region Helpers

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

    private async Task InsertPayHistory(PayHistory payHistory)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"],
$@"SET IDENTITY_INSERT PayHistory ON;

INSERT INTO PayHistory
(
    PayHistoryId,
    PayPeriodStartDate,
    PayPeriodEndDate,
    Earnings,
    PreTaxDeductions,
    Taxes,
    PostTaxDeductions
)
VALUES
(
    {payHistory.PayHistoryId},
    '{payHistory.PayPeriodStartDate}',
    '{payHistory.PayPeriodEndDate}',
    {payHistory.Earnings},
    {payHistory.PreTaxDeductions},
    {payHistory.Taxes},
    {payHistory.PostTaxDeductions}
)

SET IDENTITY_INSERT PayHistory OFF;");
    }

    #endregion Helpers
}
