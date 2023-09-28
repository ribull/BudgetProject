using Moq;
using NUnit.Framework;
using System.Text;
using Backend.Controllers;
using Microsoft.AspNetCore.Http;
using Domain.Models;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendUnitTests;

[TestFixture]
public class FileImportControllerUnitTests
{
    private readonly Mock<IBudgetDatabaseContext> _mockBudgetDatabaseContext;

    public FileImportControllerUnitTests()
    {
        _mockBudgetDatabaseContext = new();
    }

    [SetUp]
    public void Setup()
    {
        _mockBudgetDatabaseContext.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        _mockBudgetDatabaseContext.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ImportPurchasesFromCsvTest()
    {
        // Arrange
        byte[] fileBytes = Encoding.UTF8.GetBytes(
@"Date,Description,Amount,Category
5/6/2020,Kendall Support,125.00,Donations
5/6/2020,Phone Payment,15.67,Utilities
5/12/2020,Hannah Gift: Pencils,19.16,Gifts
5/12/2020,Hannah Gift: Cady Book,12.99,Gifts
5/12/2020,Hannah Gift: Coloring Book,5.00,Gifts
5/12/2020,Hannah Gift: Nerds,12.66,Gifts
5/14/2020,Robot Roller Derby Disco Dodgeball,1.06,Video Games");

        FileImportController controller = new(_mockBudgetDatabaseContext.Object);
        
        // Act
        ActionResult result = await controller.ImportPurchasesFromCsv(new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "Data", "input.csv"));

        // Assert
        _mockBudgetDatabaseContext.Verify(bdc => bdc.AddPurchasesAsync(It.IsAny<IAsyncEnumerable<Purchase>>()), Times.Once);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task ImportPayHistoryFromWorkdayCsvTest()
    {
        // Arrange
        byte[] fileBytes = Encoding.UTF8.GetBytes(
@"Period,Earnings,Pre Tax Deductions,Employee Taxes,Post Tax Deductions,Net Pay
07/16/2020 - 07/31/2020  (US Salary),""$10,976.57 "",$0.00 ,""$3,441.21 "",$0.00 ,""$7,535.36 ""
08/01/2020 - 08/15/2020  (US Salary),""$3,125.00 "",$187.50 ,$735.17 ,$0.00 ,""$2,202.33 ""
08/16/2020 - 08/31/2020  (US Salary),""$9,870.00 "",$187.50 ,""$2,480.18 "",""$5,000.00 "",""$2,202.32 ""
09/01/2020 - 09/15/2020  (US Salary),""$3,133.64 "",$187.50 ,$738.14 ,$0.00 ,""$2,199.36 """);

        FileImportController controller = new(_mockBudgetDatabaseContext.Object);

        // Act
        ActionResult result = await controller.ImportPayHistoryFromWorkdayCsv(new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "Data", "input.csv"));

        // Assert
        _mockBudgetDatabaseContext.Verify(bdc => bdc.AddPayHistoriesAsync(It.IsAny<IAsyncEnumerable<PayHistory>>()), Times.Once);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
