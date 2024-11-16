using Domain.Models;
using NUnit.Framework;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabasePayHistoryFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
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
        await BudgetDatabaseContext.AddPayHistoryAsync(payHistory);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        await BudgetDatabaseContext.AddPayHistoriesAsync(payHistories);

        // Assert
        Assert.That((await SqlHelper.QueryAsync<int>(BudgetDatabaseName, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(payHistories.Count));
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
        await BudgetDatabaseContext.AddPayHistoriesAsync(GetAsyncPayHistories());

        // Assert
        Assert.That((await SqlHelper.QueryAsync<int>(BudgetDatabaseName, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(3));
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
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        await BudgetDatabaseContext.UpdatePayHistoryAsync(newPayHistory);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
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
        Assert.That(await BudgetDatabaseContext.DoesPayHistoryExistAsync(payHistoryId), Is.True);
        Assert.That(await BudgetDatabaseContext.DoesPayHistoryExistAsync(1234), Is.False);
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
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, $"SELECT 1 FROM PayHistory WHERE PayHistoryId = {payHistoryId}"), Is.True);

        // Act
        await BudgetDatabaseContext.DeletePayHistoryAsync(payHistoryId);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName, $"SELECT 1 FROM PayHistory WHERE PayHistoryId = {payHistoryId}"), Is.False);
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
        IEnumerable<PayHistory> payHistoriesResult = await BudgetDatabaseContext.GetPayHistoriesAsync(startDate, endDate);

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
        IEnumerable<PayHistory> payHistoriesResult = await BudgetDatabaseContext.GetPayHistoriesForMonthAsync(monthDateTime);

        // Assert
        Assert.That(payHistoriesResult, Is.EquivalentTo(payHistories.Where(ph => ph.PayPeriodStartDate.Month == month && ph.PayPeriodEndDate.Month == month)));
    }

    private async Task InsertPayHistory(PayHistory payHistory)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
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
}
