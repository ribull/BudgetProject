using Domain.Models;
using NUnit.Framework;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseEraFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task GetErasTest()
    {
        // Arrange
        List<Era> eras = new()
        {
            new Era
            {
                EraId = 1,
                Name = "Test",
                StartDate = new DateTime(2021, 10, 1),
                EndDate = new DateTime(2022, 1, 1),
            },
            new Era
            {
                EraId = 9,
                Name = "Test2",
                StartDate = new DateTime(2022, 1, 1),
                EndDate = new DateTime(2023, 2, 12),
            },
            new Era
            {
                EraId = 1000,
                Name = "Test3",
                StartDate = new DateTime(2023, 2, 12),
                EndDate = null,
            }
        };

        foreach (Era era in eras)
        {
            await InsertEra(era);
        }

        // Act
        IEnumerable<Era> result = await BudgetDatabaseContext.GetErasAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(eras));
    }

    [Test]
    public async Task AddEraTest([Values] bool endDateIsNull)
    {
        // Arrange
        Era era = new Era
        {
            Name = "Test",
            StartDate = new DateTime(2023, 10, 1),
            EndDate = endDateIsNull ? null : new DateTime(2023, 12, 1)
        };

        // Act
        await BudgetDatabaseContext.AddEraAsync(era);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Era
WHERE
    Name = '{era.Name}'
    AND StartDate = '{era.StartDate}'
    AND EndDate {(endDateIsNull ? "IS NULL" : $"= '{era.EndDate}'")}"));
    }

    [Test]
    public async Task UpdateEraTest()
    {
        // Arrange
        Era originalEra = new Era
        {
            EraId = 1234,
            Name = "Test",
            StartDate = new DateTime(2023, 10, 1),
            EndDate = new DateTime(2023, 12, 1)
        };

        await InsertEra(originalEra);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Era
WHERE
    EraId = {originalEra.EraId}
    AND Name = '{originalEra.Name}'
    AND StartDate = '
        {originalEra.StartDate}'
    AND EndDate = '{originalEra.EndDate}'"));

        Era newEra = new Era
        {
            EraId = originalEra.EraId,
            Name = "New Name",
            StartDate = new DateTime(2024, 1, 2),
            EndDate = null,
        };

        // Act
        await BudgetDatabaseContext.UpdateEraAsync(newEra);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Era
WHERE
    EraId = {originalEra.EraId}
    AND Name = '{newEra.Name}'
    AND StartDate = '{newEra.StartDate}'
    AND EndDate IS NULL"));
    }

    [Test]
    public void UpdateEraDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdateEraAsync(new Era
        {
            EraId = 100,
            Name = "New Name",
            StartDate = new DateTime(2024, 1, 2),
            EndDate = null,
        }));
    }

    [Test]
    public async Task DeleteEra()
    {
        // Arrange
        Era era = new Era
        {
            EraId = 1234,
            Name = "Test",
            StartDate = new DateTime(2023, 10, 1),
            EndDate = new DateTime(2023, 12, 1)
        };

        await InsertEra(era);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Era
WHERE
    EraId = {era.EraId}
    AND Name = '{era.Name}'
    AND StartDate = '{era.StartDate}'
    AND EndDate = '{era.EndDate}'"));

        // Act
        await BudgetDatabaseContext.DeleteEraAsync(era.EraId);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Era
WHERE
    EraId = {era.EraId}
    AND Name = '{era.Name}'
    AND StartDate = '{era.StartDate}'
    AND EndDate = '{era.EndDate}'"), Is.False);
    }

    [Test]
    public void DeleteEraDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeleteEraAsync(100));
    }

    private async Task InsertEra(Era era)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
@$"INSERT INTO Era
(
    EraId,
    Name,
    StartDate,
    EndDate
)
VALUES
(
    {era.EraId},
    '{era.Name}',
    '{era.StartDate}',
    {(era.EndDate is null ? "NULL" : $"'{era.EndDate}'")}
)");
    }
}
