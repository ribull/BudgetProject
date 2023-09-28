using Backend.Controllers;
using Backend.Implementations;
using Backend.Interfaces;
using BackendFunctionalTests.Helpers;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BackendFunctionalTests;

[TestFixture]
public class FileImportControllerIntegrationTests
{
    private ISqlHelper _sqlHelper;
    private IConfiguration _config;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IBudgetDatabaseContext _budgetDatabaseContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker("file-import-functional-tests-sqlserver-", "BudgetDatabase");
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

    [Test]
    public async Task ImportPurchasesFromCsvIntegrationTest()
    {
        // Arrange
        string category1 = "Test Category 1";
        string category2 = "TestCategory2";
        string category3 = "Cat";

        await _budgetDatabaseContext.AddCategoryAsync(category1);
        await _budgetDatabaseContext.AddCategoryAsync(category2);
        await _budgetDatabaseContext.AddCategoryAsync(category3);

        List<Purchase> testPurchases = new()
        {
            new Purchase
            {
                PurchaseId = 1,
                Date = new DateTime(2023, 10, 1),
                Description = "Description 1",
                Amount = 123.45,
                Category = category1
            },
            new Purchase
            {
                PurchaseId = 2,
                Date = new DateTime(2023, 10, 2),
                Description = "Description 2",
                Amount = 123.45,
                Category = category1
            },
            new Purchase
            {
                PurchaseId = 3,
                Date = new DateTime(2023, 10, 2),
                Description = "Description 3",
                Amount = 67.89,
                Category = category2
            },
            new Purchase
            {
                PurchaseId = 4,
                Date = new DateTime(2023, 11, 4),
                Description = "Description 4",
                Amount = 12345.67,
                Category = category3
            },
            new Purchase
            {
                PurchaseId = 5,
                Date = new DateTime(2023, 10, 3),
                Description = "Description 5",
                Amount = 891011.12,
                Category = category1
            }
        };

        byte[] fileBytes = Encoding.UTF8.GetBytes(ToCsvString(testPurchases, new List<string>() { "Date", "Description", "Amount", "Category" }));

        // Act
        ActionResult result = await new FileImportController(_budgetDatabaseContext).ImportPurchasesFromCsv(new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "Data", "input.csv"));

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
        Assert.That(await _budgetDatabaseContext.GetPurchasesAsync(), Is.EquivalentTo(testPurchases));
    }

    [Test]
    public async Task ImportPayHistoryFromWorkdayCsvIntegrationTest()
    {
        // Arrange
        List<PayHistory> payHistories = new()
        {
            new PayHistory
            {
                PayHistoryId = 1,
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 15),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 6.78
            },
            new PayHistory
            {
                PayHistoryId = 2,
                PayPeriodStartDate = new DateTime(2023, 10, 16),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 1234.56,
                PreTaxDeductions = 789.01,
                Taxes = 23.45,
                PostTaxDeductions = 6.78
            },
            new PayHistory
            {
                PayHistoryId = 3,
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 9999.99,
                PreTaxDeductions = 888.88,
                Taxes = 77.77,
                PostTaxDeductions = 6.66
            },
            new PayHistory
            {
                PayHistoryId = 4,
                PayPeriodStartDate = new DateTime(2023, 10, 1),
                PayPeriodEndDate = new DateTime(2023, 10, 31),
                Earnings = 9999.99,
                PreTaxDeductions = 0.0,
                Taxes = 33.33,
                PostTaxDeductions = 0.0
            }
        };

        StringBuilder stringBuilder = new("Period,Earnings,Pre Tax Deductions,Employee Taxes,Post Tax Deductions,Net Pay\n");
        foreach (PayHistory payHistory in payHistories)
        {
            stringBuilder.AppendLine(ToWorkdayPayHistoryCsvLine(payHistory));
        }

        byte[] fileBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());

        // Act
        ActionResult result = await new FileImportController(_budgetDatabaseContext).ImportPayHistoryFromWorkdayCsv(new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "Data", "input.csv"));

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
        Assert.That(await _budgetDatabaseContext.GetPayHistoriesAsync(), Is.EquivalentTo(payHistories));
    }

    private string ToCsvString<T>(List<T> objects, List<string> properties, bool date = true)
    {
        StringBuilder stringBuilder = new(string.Join(',', properties) + "\n");

        foreach (T obj in objects)
        {
            if (obj is null)
            {
                continue;
            }

            List<string> values = new();
            Type type = obj.GetType();
            foreach (string property in properties)
            {
                PropertyInfo? pi = type.GetProperty(property);
                if (pi is null)
                {
                    throw new Exception($"The property '{property}' does not exist in type {type}");
                }

                object? value = pi.GetValue(obj, null);
                string toAdd = "";
                if (date && value is not null && value.GetType() == typeof(DateTime))
                {
                    toAdd = ((DateTime)value).ToString("MM/dd/yyyy");
                }
                else
                {
                    toAdd = value?.ToString() ?? "";
                }

                values.Add(toAdd);
            }

            stringBuilder.AppendLine(string.Join(',', values));
        }

        return stringBuilder.ToString();
    }

    private string ToWorkdayPayHistoryCsvLine(PayHistory payHistory)
    {
        return string.Join(',', new List<string>
        {
            $"{payHistory.PayPeriodStartDate.ToString("MM/dd/yyyy")} - {payHistory.PayPeriodEndDate.ToString("MM/dd/yyyy")} (US Salary)",
            $@"""{string.Format("{0:C}", payHistory.Earnings)} """,
            $@"""{string.Format("{0:C}", payHistory.PreTaxDeductions)} """,
            $@"""{string.Format("{0:C}", payHistory.Taxes)} """,
            $@"""{string.Format("{0:C}", payHistory.PostTaxDeductions)} """,
            $@"""{string.Format("{0:C}", payHistory.NetPay)} """
        });
    }
}
