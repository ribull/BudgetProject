using BackendFunctionalTests.Helpers;
using Backend.Implementations;
using Backend.Interfaces;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Domain.Models;

namespace BackendFunctionalTests;

[TestFixture]
public class PayHistoryContextFunctionalTests
{
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IPayHistoryContext _payHistoryContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker("pay-history-context-tests-sqlserver-", "BudgetDatabase");
        await _budgetDatabaseDocker.StartContainer();

        _sqlHelper = new SqlHelper(_budgetDatabaseDocker.ConnectionString!);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "BudgetDatabaseName", _budgetDatabaseDocker.DatabaseName }
            }).Build();

        _payHistoryContext = new PayHistoryContext(_config, _sqlHelper);
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
        await _payHistoryContext.AddPayHistoryAsync(payHistory);

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
        await _payHistoryContext.AddPayHistoriesAsync(payHistories);

        // Assert
        Assert.That((await _sqlHelper.QueryAsync<int>(_budgetDatabaseDocker.DatabaseName, "SELECT COUNT(*) FROM PayHistory")).Single(), Is.EqualTo(payHistories.Count));
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
        IEnumerable<PayHistory> payHistoriesResult = await _payHistoryContext.GetPayHistoriesAsync(startDate, endDate);

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
        IEnumerable<PayHistory> payHistoriesResult = await _payHistoryContext.GetPayHistoriesForMonthAsync(monthDateTime);

        // Assert
        Assert.That(payHistoriesResult, Is.EquivalentTo(payHistories.Where(ph => ph.PayPeriodStartDate.Month == month && ph.PayPeriodEndDate.Month == month)) );
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
}
