using Moq;
using NUnit.Framework;
using System.Text;
using Backend.Controllers;
using Microsoft.AspNetCore.Http;
using Domain.Models;
using Backend.Interfaces;
using System.Reflection;
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
}
