using Backend.Exceptions;
using Domain.Models;
using NUnit.Framework;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseFuturePurchaseFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task GetFuturePurchasesTest()
    {
        // Arrange
        Dictionary<string, int> categoryMapping = new() { { "Test Cat", 1 }, { "Test Cat 2", 5 } };

        foreach ((string category, int categoryId) in categoryMapping)
        {
            await AddCategory(categoryId, category);
        }

        List<FuturePurchase> futurePurchases = new()
        {
            new FuturePurchase
            {
                FuturePurchaseId = 1,
                Date = new DateTime(2023, 9, 21),
                Description = "Test Description",
                Amount = 123.45,
                Category = "Test Cat",
            },
            new FuturePurchase
            {
                FuturePurchaseId = 10,
                Date = new DateTime(2023, 9, 21),
                Description = "Test Description",
                Amount = 123.45,
                Category = "Test Cat",
            },
            new FuturePurchase
            {
                FuturePurchaseId = 20,
                Date = new DateTime(2023, 9, 21),
                Description = "Test Description",
                Amount = 123.45,
                Category = "Test Cat 2",
            }
        };

        foreach (FuturePurchase futurePurchase in futurePurchases)
        {
            await InsertFuturePurchase(futurePurchase, categoryMapping);
        }

        // Act
        IEnumerable<FuturePurchase> result = await BudgetDatabaseContext.GetFuturePurchasesAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(futurePurchases));
    }

    [Test]
    public async Task AddFuturePurchaseTest()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        FuturePurchase futurePurchase = new FuturePurchase
        {
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category,
        };

        // Act
        await BudgetDatabaseContext.AddFuturePurchaseAsync(futurePurchase);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM FuturePurchase
WHERE
    Date = '{futurePurchase.Date}'
    AND Description = '{futurePurchase.Description}'
    AND Amount = {futurePurchase.Amount}
    AND CategoryId = {categoryId}"));
    }

    [Test]
    public async Task UpdateFuturePurchaseTest()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        int newCategoryId = 999;
        string newCategory = "NewCategory";

        await AddCategory(newCategoryId, newCategory);

        Dictionary<string, int> categoryMap = new() { { category, categoryId }, { newCategory, newCategoryId } };

        // Arrange
        FuturePurchase originalFuturePurchase = new FuturePurchase
        {
            FuturePurchaseId = 1234,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category,
        };

        await InsertFuturePurchase(originalFuturePurchase, categoryMap);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM FuturePurchase
WHERE
    FuturePurchaseId = {originalFuturePurchase.FuturePurchaseId}
    AND Date = '{originalFuturePurchase.Date}'
    AND Description = '{originalFuturePurchase.Description}'
    AND Amount = {originalFuturePurchase.Amount}
    AND CategoryId = {categoryId}"));

        FuturePurchase newFuturePurchase = new FuturePurchase
        {
            FuturePurchaseId = 1234,
            Date = new DateTime(2020, 7, 12),
            Description = "Test Description New",
            Amount = 9987.65,
            Category = newCategory,
        };

        // Act
        await BudgetDatabaseContext.UpdateFuturePurchaseAsync(newFuturePurchase);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM FuturePurchase
WHERE
    FuturePurchaseId = {originalFuturePurchase.FuturePurchaseId}
    AND Date = '{newFuturePurchase.Date}'
    AND Description = '{newFuturePurchase.Description}'
    AND Amount = {newFuturePurchase.Amount}
    AND CategoryId = {newCategoryId}"));
    }

    [Test]
    public void UpdateFuturePurchaseCategoryDoesNotExist()
    {
        // Arrange
        string category = "TestCategory";

        FuturePurchase testFuturePurchase = new FuturePurchase
        {
            FuturePurchaseId = 1,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act + Assert
        Assert.ThrowsAsync<CategoryDoesNotExistException>(async () => await BudgetDatabaseContext.UpdateFuturePurchaseAsync(testFuturePurchase));
    }

    [Test]
    public async Task UpdateFuturePurchaseDoesNotExist()
    {
        // Arrange
        string category = "TestCategory";
        await AddCategory(1, category);

        FuturePurchase futurePurchase = new FuturePurchase
        {
            FuturePurchaseId = 1,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdateFuturePurchaseAsync(futurePurchase));
    }

    [Test]
    public async Task DeleteFuturePurchase()
    {
        // Arrange
        int categoryId = 100;
        string category = "TestCategory";

        await AddCategory(categoryId, category);

        FuturePurchase testFuturePurchase = new FuturePurchase
        {
            FuturePurchaseId = 1001,
            Date = new DateTime(2023, 9, 21),
            Description = "Test Description",
            Amount = 123.45,
            Category = category
        };

        await InsertFuturePurchase(testFuturePurchase, new() { { category, categoryId } });

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
$@"SELECT 1
FROM FuturePurchase
WHERE
    FuturePurchaseId = {testFuturePurchase.FuturePurchaseId}
    AND Date = '{testFuturePurchase.Date}'
    AND Description = '{testFuturePurchase.Description}'
    AND Amount = {testFuturePurchase.Amount}
    AND CategoryId = {categoryId}"));

        // Act
        await BudgetDatabaseContext.DeleteFuturePurchaseAsync(testFuturePurchase.FuturePurchaseId);

        // Assert
        Assert.That(!await SqlHelper.ExistsAsync(BudgetDatabaseName,
$@"SELECT 1
FROM FuturePurchase
WHERE
    FuturePurchaseId = {testFuturePurchase.FuturePurchaseId}
    AND Date = '{testFuturePurchase.Date}'
    AND Description = '{testFuturePurchase.Description}'
    AND Amount = {testFuturePurchase.Amount}
    AND CategoryId = {categoryId}"));
    }

    [Test]
    public void DeleteFuturePurchaseDoesNotExist()
    {
        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeleteFuturePurchaseAsync(100));
    }

    private async Task InsertFuturePurchase(FuturePurchase futurePurchase, Dictionary<string, int> categoryMap)
    {
        if (futurePurchase.Category is null)
        {
            throw new Exception("The category is null, you should have passed a purchase without a null category");
        }

        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
$@"INSERT INTO FuturePurchase
(
    FuturePurchaseId,
    Date,
    Description,
    Amount,
    CategoryId
)
VALUES
(
    {futurePurchase.FuturePurchaseId},
    '{futurePurchase.Date}',
    '{futurePurchase.Description}',
    {futurePurchase.Amount},
    {categoryMap[futurePurchase.Category]}
);");
    }
}
