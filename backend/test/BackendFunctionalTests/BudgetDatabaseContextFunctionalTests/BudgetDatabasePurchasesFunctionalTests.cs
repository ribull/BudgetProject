using Backend.Exceptions;
using Domain.Models;
using NUnit.Framework;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabasePurchasesFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
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
        await BudgetDatabaseContext.AddPurchaseAsync(testPurchase);

        // Assert
        Assert.That((await SqlHelper.QueryAsync<int>(BudgetDatabaseName,
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
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await BudgetDatabaseContext.AddPurchaseAsync(testPurchase));
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
        await BudgetDatabaseContext.AddPurchasesAsync(GetAsyncPurchases());

        // Assert
        Assert.That((await SqlHelper.QueryAsync<int>(BudgetDatabaseName, $"SELECT COUNT(*) FROM Purchase WHERE CategoryId = {categoryId}")).Single(), Is.EqualTo(4));
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
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.AddPurchasesAsync(GetAsyncPurchases()));

        // Make sure the transaction rolled back
        Assert.That((await SqlHelper.QueryAsync<int>(BudgetDatabaseName, "SELECT COUNT(*) FROM Purchase")).Single(), Is.EqualTo(0));
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
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        await BudgetDatabaseContext.UpdatePurchaseAsync(newPurchase);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await BudgetDatabaseContext.UpdatePurchaseAsync(testPurchase));
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
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdatePurchaseAsync(testPurchase));
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
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
$@"SELECT 1
FROM Purchase
WHERE
    PurchaseId = {testPurchase.PurchaseId}
    AND Date = '{testPurchase.Date}'
    AND Description = '{testPurchase.Description}'
    AND Amount = {testPurchase.Amount}
    AND CategoryId = {categoryId}"));

        // Act
        await BudgetDatabaseContext.DeletePurchaseAsync(testPurchase.PurchaseId);

        // Assert
        Assert.That(!await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeletePurchaseAsync(1001));
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
        IEnumerable<Purchase> allPurchases = await BudgetDatabaseContext.GetPurchasesAsync();
        Assert.That(allPurchases, Is.EquivalentTo(testPurchases));

        IEnumerable<Purchase> utilitiesPurchases = await BudgetDatabaseContext.GetPurchasesAsync(category: "Utilities");
        Assert.That(utilitiesPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Category == "Utilities")));

        IEnumerable<Purchase> dateAfterPurchases = await BudgetDatabaseContext.GetPurchasesAsync(startDate: new DateTime(2023, 10, 1));
        Assert.That(dateAfterPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date >= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> dateBeforePurchases = await BudgetDatabaseContext.GetPurchasesAsync(endDate: new DateTime(2023, 10, 1));
        Assert.That(dateBeforePurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Date <= new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> descriptionPurchases = await BudgetDatabaseContext.GetPurchasesAsync(description: "TestDescriptionX");
        Assert.That(descriptionPurchases, Is.EquivalentTo(testPurchases.Where(tp => tp.Description == "TestDescriptionX")));

        IEnumerable<Purchase> specificPurchase = await BudgetDatabaseContext.GetPurchasesAsync(description: "TestDescriptionX", category: "Utilities", startDate: new DateTime(2023, 10, 17), endDate: new DateTime(2023, 10, 17));
        Assert.That(specificPurchase, Is.EquivalentTo(testPurchases.Where(tp => tp.Description == "TestDescriptionX" && tp.Category == "Utilities" && tp.Date == new DateTime(2023, 10, 17))));
    }

    [Test]
    public async Task GetPurchasesNullCategoryTest()
    {
        // Arrange
        // Simulate a deleted category
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
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
        IEnumerable<Purchase> purchases = await BudgetDatabaseContext.GetPurchasesAsync();

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
        IEnumerable<Purchase> purchases = await BudgetDatabaseContext.GetMostCommonPurchasesAsync();
        Assert.That(purchases, Is.EquivalentTo(GetMostCommonPurchases()));

        IEnumerable<Purchase> utilitiesPurchases = await BudgetDatabaseContext.GetMostCommonPurchasesAsync(category: "Utilities");
        Assert.That(utilitiesPurchases, Is.EquivalentTo(GetMostCommonPurchases(category: "Utilities")));

        IEnumerable<Purchase> dateAfterPurchases = await BudgetDatabaseContext.GetMostCommonPurchasesAsync(startDate: new DateTime(2023, 10, 1));
        Assert.That(dateAfterPurchases, Is.EquivalentTo(GetMostCommonPurchases(startDate: new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> dateBeforePurchases = await BudgetDatabaseContext.GetMostCommonPurchasesAsync(endDate: new DateTime(2023, 10, 1));
        Assert.That(dateBeforePurchases, Is.EquivalentTo(GetMostCommonPurchases(endDate: new DateTime(2023, 10, 1))));

        IEnumerable<Purchase> countPurchases = await BudgetDatabaseContext.GetMostCommonPurchasesAsync(count: 3);
        Assert.That(countPurchases, Is.EquivalentTo(GetMostCommonPurchases(count: 3)));
    }

    private async Task AddPurchase(Purchase purchase, Dictionary<string, int> categoryMap)
    {
        if (purchase.Category is null)
        {
            throw new Exception("The category is null, you should have passed a purchase without a null category");
        }

        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
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
}
