using Domain.Models;
using NUnit.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseSavedFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task GetSavingsTest()
    {
        // Arrange
        List<Saved> savings = new()
        {
            new Saved
            {
                SavedId = 1,
                Date = new DateTime(2023, 1, 2),
                Description = "Test Description",
                Amount = 123.45,

            },
            new Saved
            {
                SavedId = 9,
                Date = new DateTime(2020, 1, 19),
                Description = "Test Description 2",
                Amount = 200.1
            },
            new Saved
            {
                SavedId = 1000,
                Date = new DateTime(2023, 10, 9),
                Description = "Test Description 3",
                Amount = 99.87,
            }
        };

        foreach (Saved saved in savings)
        {
            await InsertSaved(saved);
        }

        // Act
        IEnumerable<Saved> result = await BudgetDatabaseContext.GetSavingsAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(savings));
    }

    [Test]
    public async Task AddSavedTest()
    {
        // Arrange
        Saved saved = new Saved
        {
            Date = new DateTime(2020, 1, 19),
            Description = "Test Description 2",
            Amount = 200.1
        };

        // Act
        await BudgetDatabaseContext.AddSavingAsync(saved);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Saved
WHERE
    Date = '{saved.Date}'
    AND Description = '{saved.Description}'
    AND Amount = {saved.Amount}"));
    }

    [Test]
    public async Task UpdateSavedTest()
    {
        // Arrange
        Saved originalSaved = new Saved
        {
            SavedId = 199,
            Date = new DateTime(2023, 9, 1),
            Description = "Test Description",
            Amount = 100.12,
        };

        await InsertSaved(originalSaved);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Saved
WHERE
    SavedId = {originalSaved.SavedId}
    AND Date = '{originalSaved.Date}'
    AND Description = '{originalSaved.Description}'
    AND Amount = {originalSaved.Amount}"));

        Saved newSaved = new Saved
        {
            SavedId = originalSaved.SavedId,
            Date = new DateTime(2023, 10,12),
            Description = "Test Description New",
            Amount = 99.86,
        };

        // Act
        await BudgetDatabaseContext.UpdateSavingAsync(newSaved);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Saved
WHERE
    SavedId = {originalSaved.SavedId}
    AND Date = '{newSaved.Date}'
    AND Description = '{newSaved.Description}'
    AND Amount = {newSaved.Amount}"));
    }

    [Test]
    public void UpdateSavedDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdateSavingAsync(new Saved
        {
            SavedId = 100,
            Description = "Test Description New",
            Amount = 99.86,
            Date = DateTime.Now,
        }));
    }

    [Test]
    public async Task DeleteSaved()
    {
        // Arrange
        Saved saved = new Saved
        {
            SavedId = 199,
            Date = new DateTime(2023, 9, 1),
            Description = "Test Description",
            Amount = 100.12,
        };

        await InsertSaved(saved);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Saved
WHERE
    SavedId = {saved.SavedId}
    AND Date = '{saved.Date}'
    AND Description = '{saved.Description}'
    AND Amount = {saved.Amount}"));

        // Act
        await BudgetDatabaseContext.DeleteSavedAsync(saved.SavedId);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Saved
WHERE
    SavedId = {saved.SavedId}
    AND Date = '{saved.Date}'
    AND Description = '{saved.Description}'
    AND Amount = {saved.Amount}"), Is.False);
    }

    [Test]
    public void DeleteSavedDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeleteSavedAsync(100));
    }

    private async Task InsertSaved(Saved saved)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
@$"INSERT INTO Saved
(
    SavedId,
    Date,
    Description,
    Amount
)
VALUES
(
    {saved.SavedId},
    '{saved.Date}',
    '{saved.Description}',
    {saved.Amount}
)");
    }
}
