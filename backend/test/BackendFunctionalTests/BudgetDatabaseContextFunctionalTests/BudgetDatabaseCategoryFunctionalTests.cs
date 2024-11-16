using NUnit.Framework;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseCategoryFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";

        // Act
        await BudgetDatabaseContext.AddCategoryAsync(category);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, "SELECT 1 FROM Category WHERE Category = 'Utilities'"));
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        int categoryId = 101;
        string category = "Utilities";
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
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
    '{new DateTime(2023, 9, 22)}',
    '{description}',
    123.45,
    {categoryId}
)");

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"));
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, $"SELECT 1 FROM Purchase WHERE Description = '{description}' AND CategoryId = {categoryId}"));

        // Act
        await BudgetDatabaseContext.DeleteCategoryAsync(category);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, $"SELECT 1 FROM Category WHERE Category = '{category}'"), Is.False);
        Assert.That((await SqlHelper.QueryAsync<int?>(BudgetDatabaseName, $"SELECT CategoryId FROM Purchase WHERE Description = '{description}'")).Single(), Is.Null);
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
            await SqlHelper.ExecuteAsync(BudgetDatabaseName, $"INSERT INTO Category (Category) VALUES ('{testCategory}')");
        }

        // Act
        IEnumerable<string> resultCategories = await BudgetDatabaseContext.GetCategoriesAsync();

        // Assert
        Assert.That(resultCategories, Is.EquivalentTo(testCategories));
    }

    [Test]
    public async Task DoesCategoryExistTest()
    {
        // Arrange
        string category = "Utilities";
        await SqlHelper.ExecuteAsync(BudgetDatabaseName, $"INSERT INTO Category (Category) VALUES ('{category}')");

        // Act + Assert
        Assert.That(await BudgetDatabaseContext.DoesCategoryExistAsync(category), Is.True);
        Assert.That(await BudgetDatabaseContext.DoesCategoryExistAsync("Does not exist category"), Is.False);
    }
}
