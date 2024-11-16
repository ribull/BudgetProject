using NUnit.Framework;
using Domain.Models;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseInvestmentFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task GetInvestmentsTest()
    {
        // Arrange
        List<Investment> investments = new()
        {
            new Investment
            {
                InvestmentId = 1,
                Description = "Test Description",
                CurrentAmount = 100.12,
                YearlyGrowthRate = .0198,
                LastUpdated = new DateTime(2023, 1, 1)

            },
            new Investment
            {
                InvestmentId = 9,
                Description = "Test Description 2",
                CurrentAmount = 99987.10,
                YearlyGrowthRate = .12,
                LastUpdated = new DateTime(2023, 2, 3)
            },
            new Investment
            {
                InvestmentId = 1000,
                Description = "Test Description  3",
                CurrentAmount = 123.51,
                YearlyGrowthRate = 1.2,
                LastUpdated = new DateTime(2023, 9, 12)
            }
        };

        foreach (Investment investment in investments)
        {
            await InsertInvestment(investment);
        }

        // Act
        IEnumerable<Investment> result = await BudgetDatabaseContext.GetInvestmentsAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(investments));
    }

    [Test]
    public async Task AddInvestmentTest()
    {
        // Arrange
        Investment investment = new Investment
        {
            Description = "Test Description",
            CurrentAmount = 100.12,
            YearlyGrowthRate = .0198,
        };

        // Act
        await BudgetDatabaseContext.AddInvestmentAsync(investment);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Investment
WHERE
    Description = '{investment.Description}'
    AND CurrentAmount = {investment.CurrentAmount}
    AND YearlyGrowthRate = {investment.YearlyGrowthRate}
    AND LastUpdated = '{DateTime.Now}'"));
    }

    [Test]
    public async Task UpdateInvestmentTest()
    {
        // Arrange
        Investment originalInvestment = new Investment
        {
            InvestmentId = 199,
            Description = "Test Description",
            CurrentAmount = 100.12,
            YearlyGrowthRate = .0198,
            LastUpdated = new DateTime(2023, 10, 1)
        };

        await InsertInvestment(originalInvestment);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Investment
WHERE
    InvestmentId = {originalInvestment.InvestmentId}
    AND Description = '{originalInvestment.Description}'
    AND CurrentAmount = {originalInvestment.CurrentAmount}
    AND YearlyGrowthRate = {originalInvestment.YearlyGrowthRate}
    AND LastUpdated = '{originalInvestment.LastUpdated}'"));

        Investment newInvestment = new Investment
        {
            InvestmentId = originalInvestment.InvestmentId,
            Description = "Test Description New",
            CurrentAmount = 99.86,
            YearlyGrowthRate = .21,
        };

        // Act
        await BudgetDatabaseContext.UpdateInvestmentAsync(newInvestment);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Investment
WHERE
    InvestmentId = {originalInvestment.InvestmentId}
    AND Description = '{newInvestment.Description}'
    AND CurrentAmount = {newInvestment.CurrentAmount}
    AND YearlyGrowthRate = {newInvestment.YearlyGrowthRate}
    AND LastUpdated = '{DateTime.Now}'"));
    }

    [Test]
    public void UpdateInvestmentDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdateInvestmentAsync(new Investment
        {
            InvestmentId = 100,
            Description = "Test Description New",
            CurrentAmount = 99.86,
            YearlyGrowthRate = .21,
        }));
    }

    [Test]
    public async Task DeleteInvestment()
    {
        // Arrange
        Investment investment = new Investment
        {
            InvestmentId = 199,
            Description = "Test Description",
            CurrentAmount = 100.12,
            YearlyGrowthRate = .0198,
            LastUpdated = new DateTime(2023, 10, 1)
        };

        await InsertInvestment(investment);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Investment
WHERE
    InvestmentId = {investment.InvestmentId}
    AND Description = '{investment.Description}'
    AND CurrentAmount = {investment.CurrentAmount}
    AND YearlyGrowthRate = {investment.YearlyGrowthRate}
    AND LastUpdated = '{investment.LastUpdated}'"));

        // Act
        await BudgetDatabaseContext.DeleteInvestmentAsync(investment.InvestmentId);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Investment
WHERE
    InvestmentId = {investment.InvestmentId}
    AND Description = '{investment.Description}'
    AND CurrentAmount = {investment.CurrentAmount}
    AND YearlyGrowthRate = {investment.YearlyGrowthRate}
    AND LastUpdated = '{investment.LastUpdated}'"), Is.False);
    }

    [Test]
    public void DeleteInvestmentDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeleteInvestmentAsync(100));
    }

    private async Task InsertInvestment(Investment investment)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
@$"INSERT INTO Investment
(
    InvestmentId,
    Description,
    CurrentAmount,
    YearlyGrowthRate,
    LastUpdated
)
VALUES
(
    {investment.InvestmentId},
    '{investment.Description}',
    {investment.CurrentAmount},
    {investment.YearlyGrowthRate},
    '{investment.LastUpdated}'
)");
    }
}
