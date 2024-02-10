using Backend.Exceptions;
using Backend.Implementations;
using Backend.Interfaces;
using BudgetDatabase.Deployer;
using BudgetProto;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Testcontainers.PostgreSql;
using PayHistory = Domain.Models.PayHistory;
using Purchase = Domain.Models.Purchase;

namespace BackendFunctionalTests;

[TestFixture]
public class BudgetDatabaseContextFunctionalTests
{
    private ISqlConnectionStringBuilder _connectionStringBuilder;
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;

    private IBudgetDatabaseContext _budgetDatabaseContext;

    private PostgreSqlContainer _postgreSqlContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .Build();

        await _postgreSqlContainer.StartAsync();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  _postgreSqlContainer.Hostname },
                { "PostgreSqlConnectionDetails:Username", "postgres" },
                { "PostgreSqlConnectionDetails:Password", "postgres" },
                { "PostgreSqlConnectionDetails:Port", $"{_postgreSqlContainer.GetMappedPublicPort(5432)}" }
            }).Build();

        _connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(_config);
        _sqlHelper = new SqlHelper(_connectionStringBuilder);

        _budgetDatabaseContext = new BudgetDatabaseContext(_config, _sqlHelper);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        DatabaseDeployer.DeployDatabase(_connectionStringBuilder.GetConnectionString(_config["BudgetDatabaseName"]!));
    }

    [TearDown]
    public async Task TearDown()
    {
        await _sqlHelper.ExecuteAsync("postgres", $"DROP DATABASE {_config["BudgetDatabaseName"]!} WITH (FORCE)");
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
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, "SELECT 1 FROM Category WHERE Category = 'Utilities'"));
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        int categoryId = 101;
        string category = "Utilities";
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
$@"INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {categoryId},
    '{category}'
)");

        string description = "xx_Description_xx";
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
$@"INSERT INTO Purchase
(
    Date,
    Description,
    Amount,
    CategoryId
)
VALUES
(
    '{new DateTime(2023, 9, 22)}',
    '{description}',
    123.45,
    {categoryId}
)");

        // Sanity check
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, $"SELECT 1 FROM Category WHERE Category = '{category}'"));
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, $"SELECT 1 FROM Purchase WHERE Description = '{description}' AND CategoryId = {categoryId}"));

        // Act
        await _budgetDatabaseContext.DeleteCategoryAsync(category);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, $"SELECT 1 FROM Category WHERE Category = '{category}'"), Is.False);
        Assert.That((await _sqlHelper.QueryAsync<int?>(_config["BudgetDatabaseName"]!, $"SELECT CategoryId FROM Purchase WHERE Description = '{description}'")).Single(), Is.Null);
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
            await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!, $"INSERT INTO Category (Category) VALUES ('{testCategory}')");
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
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!, $"INSERT INTO Category (Category) VALUES ('{category}')");

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
        Assert.That((await _sqlHelper.QueryAsync<int>(_config["BudgetDatabaseName"]!,
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
        Assert.That((await _sqlHelper.QueryAsync<int>(_config["BudgetDatabaseName"]!, $"SELECT COUNT(*) FROM Purchase WHERE CategoryId = {categoryId}")).Single(), Is.EqualTo(4));
    }

    [Test]
    public async Task AddPurchasesRollbackTest()
    {
        // Arrange
        string category = "TestCategory";

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
                Category = null
            };

            await Task.CompletedTask;
        };

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _budgetDatabaseContext.AddPurchasesAsync(GetAsyncPurchases()));

        // Make sure the transaction rolled back
        Assert.That((await _sqlHelper.QueryAsync<int>(_config["BudgetDatabaseName"]!, "SELECT COUNT(*) FROM Purchase")).Single(), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdatePurchaseTest()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        Purchase testPurchase = new Purchase
        {
            PurchaseId = 1001,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        await AddPurchase(testPurchase, new() { { category, categoryId } });

        // Sanity check
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1
FROM Purchase
WHERE
    PurchaseId = {testPurchase.PurchaseId}
    AND Date = '{testPurchase.Date}'
    AND Description = '{testPurchase.Description}'
    AND Amount = {testPurchase.Amount}
    AND CategoryId = {categoryId}"));

        int newCategoryId = 101;
        string newCategory = "TestCategoryNew";

        await AddCategory(newCategoryId, newCategory);

        Purchase newPurchase = new Purchase
        {
            PurchaseId = testPurchase.PurchaseId,
            Date = new DateTime(2023, 10, 12),
            Description = "Test Description New",
            Amount = 999.89,
            Category = newCategory
        };

        // Act
        await _budgetDatabaseContext.UpdatePurchaseAsync(newPurchase);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1
FROM Purchase
WHERE
    PurchaseId = {newPurchase.PurchaseId}
    AND Date = '{newPurchase.Date}'
    AND Description = '{newPurchase.Description}'
    AND Amount = {newPurchase.Amount}
    AND CategoryId = {newCategoryId}"));
    }

    [Test]
    public void UpdatePurchaseCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "TestCategory";

        Purchase testPurchase = new Purchase
        {
            PurchaseId = 1,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act + Assert
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await _budgetDatabaseContext.UpdatePurchaseAsync(testPurchase));
    }

    [Test]
    public async Task UpdatePurchaseIdDoesNotExistTest()
    {
        // Arrange
        string category = "TestCategory";
        await AddCategory(1, category);

        Purchase testPurchase = new Purchase
        {
            PurchaseId = 1,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _budgetDatabaseContext.UpdatePurchaseAsync(testPurchase));
    }

    [Test]
    public async Task DeletePurchaseTest()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        Purchase testPurchase = new Purchase
        {
            PurchaseId = 1001,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        await AddPurchase(testPurchase, new() { { category, categoryId } });

        // Sanity check
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1
FROM Purchase
WHERE
    PurchaseId = {testPurchase.PurchaseId}
    AND Date = '{testPurchase.Date}'
    AND Description = '{testPurchase.Description}'
    AND Amount = {testPurchase.Amount}
    AND CategoryId = {categoryId}"));
        
        // Act
        await _budgetDatabaseContext.DeletePurchaseAsync(testPurchase.PurchaseId);

        // Assert
        Assert.That(!await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1
FROM Purchase
WHERE
    PurchaseId = {testPurchase.PurchaseId}
    AND Date = '{testPurchase.Date}'
    AND Description = '{testPurchase.Description}'
    AND Amount = {testPurchase.Amount}
    AND CategoryId = {categoryId}"));
    }

    [Test]
    public void DeletePurchaseDoesNotExistTest()
    {
        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _budgetDatabaseContext.DeletePurchaseAsync(1001));
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
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
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

    [Test]
    public async Task GetMostCommonPurchasesTest()
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
            },
            new Purchase
            {
                PurchaseId = 11,
                Date = new DateTime(2023, 9, 22),
                Description = "TestDescription",
                Category = categoryMap.ElementAt(0).Key,
                Amount = 12.31
            },
            new Purchase
            {
                PurchaseId = 12,
                Date = new DateTime(2023, 9, 24),
                Description = "TestDescription",
                Category = categoryMap.ElementAt(0).Key,
                Amount = 999.99
            },
            new Purchase
            {
                PurchaseId = 13,
                Date = new DateTime(2023, 9, 21),
                Description = "TestDescription2",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 188.18
            },
            new Purchase
            {
                PurchaseId = 14,
                Date = new DateTime(2023, 9, 24),
                Description = "TestDescription2",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 188.18
            },
            new Purchase
            {
                PurchaseId = 15,
                Date = new DateTime(2023, 9, 26),
                Description = "TestDescription2",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 188.18
            },
            new Purchase
            {
                PurchaseId = 16,
                Date = new DateTime(2023, 9, 29),
                Description = "TestDescription2",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 188.18
            },
            new Purchase
            {
                PurchaseId = 17,
                Date = new DateTime(2023, 9, 29),
                Description = "TestDescriptionNoOtherPurchases",
                Category = categoryMap.ElementAt(3).Key,
                Amount = 0.23
            },
            new Purchase
            {
                PurchaseId = 18,
                Date = new DateTime(2023, 2, 12),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 0.45
            },
            new Purchase
            {
                PurchaseId = 19,
                Date = new DateTime(2023, 2, 13),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 0.67
            },
            new Purchase
            {
                PurchaseId = 20,
                Date = new DateTime(2023, 2, 14),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 0.89
            },
            new Purchase
            {
                PurchaseId = 21,
                Date = new DateTime(2024, 1, 1),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 1.0
            },
            new Purchase
            {
                PurchaseId = 22,
                Date = new DateTime(2024, 1, 2),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 1.1
            },
            new Purchase
            {
                PurchaseId = 23,
                Date = new DateTime(2024, 1, 3),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 1.2
            },
            new Purchase
            {
                PurchaseId = 24,
                Date = new DateTime(2024, 1, 4),
                Description = "Utilities1",
                Category = categoryMap.ElementAt(2).Key,
                Amount = 1.3
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

        // I want to define this in code as well, that way I can make sure I'm getting it right in SQL
        IEnumerable<Purchase> GetMostCommonPurchases(string? category = null, DateTime? startDate = null, DateTime? endDate = null, int? count = null)
        {
            IEnumerable<Purchase> filteredPurchases = testPurchases.Where(p =>
                (category is null || p.Category == category)
                && (startDate is null || p.Date >= startDate)
                && (endDate is null || p.Date <= endDate));

            // Get the top descriptions
            Dictionary<string, int> purchaseCounts = new();
            foreach (Purchase p in filteredPurchases)
            {
                if (purchaseCounts.ContainsKey(p.Description))
                {
                    purchaseCounts[p.Description] += 1;
                }
                else
                {
                    purchaseCounts[p.Description] = 1;
                }
            }

            IOrderedEnumerable<Purchase> latestPurchases = filteredPurchases.OrderByDescending(p => p.Date);
            List<Purchase> results = new();
            foreach ((string description, int cnt) in purchaseCounts.ToList().OrderByDescending(kvPair => kvPair.Value).Take(count is null ? purchaseCounts.Count : count.Value))
            {
                results.Add(latestPurchases.First(p => p.Description == description));
            }

            return results;
        }

        // Act + Assert
        IEnumerable<Purchase> purchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync();
        Assert.That(purchases, Is.EquivalentTo(GetMostCommonPurchases()));

        IEnumerable<Purchase> utilitiesPurchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync(category: "Utilities");
        Assert.That(utilitiesPurchases, Is.EquivalentTo(GetMostCommonPurchases(category: "Utilities")));

        IEnumerable<Purchase> dateAfterPurchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync(startDate: new DateTime(2023, 10, 1));
        Assert.That(dateAfterPurchases, Is.EquivalentTo(GetMostCommonPurchases(startDate: new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> dateBeforePurchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync(endDate: new DateTime(2023, 10, 1));
        Assert.That(dateBeforePurchases, Is.EquivalentTo(GetMostCommonPurchases(endDate: new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> countPurchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync(count: 3);
        Assert.That(countPurchases, Is.EquivalentTo(GetMostCommonPurchases(count: 3)));
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
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
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
        Assert.That((await _sqlHelper.QueryAsync<int>(_config["BudgetDatabaseName"]!, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(payHistories.Count));
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
        Assert.That((await _sqlHelper.QueryAsync<int>(_config["BudgetDatabaseName"]!, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(3));
    }

    [Test]
    public async Task UpdatePayHistoryTest()
    {
        // Arrange
        PayHistory originalPayHistory = new()
        {
            PayHistoryId = 1001,
            PayPeriodStartDate = new DateTime(2023, 10, 1),
            PayPeriodEndDate = new DateTime(2023, 10, 15),
            Earnings = 12345.67,
            PreTaxDeductions = 9876.54,
            Taxes = 321.09,
            PostTaxDeductions = 87.65
        };

        await InsertPayHistory(originalPayHistory);

        // Sanity check
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1 FROM PayHistory
WHERE
    PayHistoryId = {originalPayHistory.PayHistoryId}
    AND PayPeriodStartDate = '{originalPayHistory.PayPeriodStartDate}'
    AND PayPeriodEndDate = '{originalPayHistory.PayPeriodEndDate}'
    AND Earnings = {originalPayHistory.Earnings}
    AND PreTaxDeductions = {originalPayHistory.PreTaxDeductions}
    AND Taxes = {originalPayHistory.Taxes}
    AND PostTaxDeductions = {originalPayHistory.PostTaxDeductions}"));

        PayHistory newPayHistory = new()
        {
            PayHistoryId = originalPayHistory.PayHistoryId,
            PayPeriodStartDate = new DateTime(2023, 12, 1),
            PayPeriodEndDate = new DateTime(2023, 12, 15),
            Earnings = 4321.01,
            PreTaxDeductions = 998.1,
            Taxes = 1.0,
            PostTaxDeductions = 0.99,
        };

        // Act
        await _budgetDatabaseContext.UpdatePayHistoryAsync(newPayHistory);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!,
$@"SELECT 1 FROM PayHistory
WHERE
    PayHistoryId = {originalPayHistory.PayHistoryId}
    AND PayPeriodStartDate = '{newPayHistory.PayPeriodStartDate}'
    AND PayPeriodEndDate = '{newPayHistory.PayPeriodEndDate}'
    AND Earnings = {newPayHistory.Earnings}
    AND PreTaxDeductions = {newPayHistory.PreTaxDeductions}
    AND Taxes = {newPayHistory.Taxes}
    AND PostTaxDeductions = {newPayHistory.PostTaxDeductions}"));
    }

    [Test]
    public async Task DoesPayHistoryExistTest()
    {
        // Arrange
        int payHistoryId = 1001;
        await InsertPayHistory(new PayHistory
        {
            PayHistoryId = payHistoryId,
            PayPeriodStartDate = new DateTime(2023, 10, 1),
            PayPeriodEndDate = new DateTime(2023, 10, 15),
            Earnings = 12345.67,
            PreTaxDeductions = 9876.54,
            Taxes = 321.09,
            PostTaxDeductions = 87.65
        });

        // Act + Assert
        Assert.That(await _budgetDatabaseContext.DoesPayHistoryExistAsync(payHistoryId), Is.True);
        Assert.That(await _budgetDatabaseContext.DoesPayHistoryExistAsync(1234), Is.False);
    }

    [Test]
    public async Task DeletePayHistoryTest()
    {
        // Arrange
        int payHistoryId = 1001;
        await InsertPayHistory(new PayHistory
        {
            PayHistoryId = payHistoryId,
            PayPeriodStartDate = new DateTime(2023, 10, 1),
            PayPeriodEndDate = new DateTime(2023, 10, 15),
            Earnings = 12345.67,
            PreTaxDeductions = 9876.54,
            Taxes = 321.09,
            PostTaxDeductions = 87.65
        });

        // Sanity check
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, $"SELECT 1 FROM PayHistory WHERE PayHistoryId = {payHistoryId}"), Is.True);

        // Act
        await _budgetDatabaseContext.DeletePayHistoryAsync(payHistoryId);

        // Assert
        Assert.That(await _sqlHelper.ExistsAsync(_config["BudgetDatabaseName"]!, $"SELECT 1 FROM PayHistory WHERE PayHistoryId = {payHistoryId}"), Is.False);
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

        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
$@"INSERT INTO Purchase
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
);");
    }

    private async Task AddCategory(int id, string category)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
$@"INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {id},
    '{category}'
);");
    }

    private async Task InsertPayHistory(PayHistory payHistory)
    {
        await _sqlHelper.ExecuteAsync(_config["BudgetDatabaseName"]!,
$@"INSERT INTO PayHistory
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
)");
    }

    #endregion Helpers
}
