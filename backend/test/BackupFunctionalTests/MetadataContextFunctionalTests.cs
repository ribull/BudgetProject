using BackupFunctionalTests.Helpers;
using GrpcService.Implementations;
using GrpcService.Interfaces;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupFunctionalTests;

[TestFixture]
public class MetadataContextFunctionalTests
{
    private ISqlHelper _sqlHelper;
    private BudgetDatabaseDocker _budgetDatabaseDocker;

    private IMetadataContext _metadataContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _budgetDatabaseDocker = new BudgetDatabaseDocker();
        await _budgetDatabaseDocker.StartContainer();

        _sqlHelper = new SqlHelper(_budgetDatabaseDocker.ConnectionString!);

        _metadataContext = new MetadataContext(_sqlHelper);
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
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";

        // Act
        await _metadataContext.AddCategory(category);

        // Assert
        Assert.That(await _sqlHelper.Exists(BudgetDatabaseDocker.DATABASE_NAME, "SELECT 1 FROM Category WHERE Category = 'Utilities'"));
    }

    [Test]
    public async Task AddCategoryAlreadyExistsTest()
    {
        // Arrange
        string category = "Utilities";
        await _sqlHelper.ExecuteAsync(BudgetDatabaseDocker.DATABASE_NAME, $"INSERT INTO Category VALUES ('{category}')");

        // Act + Assert
        Assert.ThrowsAsync<SqlException>(async () => await _metadataContext.AddCategory(category));
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        int categoryId = 101;
        string category = "Utilities";
        await _sqlHelper.ExecuteAsync(BudgetDatabaseDocker.DATABASE_NAME,
$@"SET IDENTITY_INSERT Category ON;

INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {categoryId},
    '{category}'
)

SET IDENTITY_INSERT Category OFF;");

        string description = "xx_Description_xx";
        await _sqlHelper.ExecuteAsync(BudgetDatabaseDocker.DATABASE_NAME, $"INSERT INTO Purchase VALUES ('{new DateTime(2023, 9, 22)}', '{description}', 123.45, {categoryId})");

        // Sanity check
        Assert.That(await _sqlHelper.Exists(BudgetDatabaseDocker.DATABASE_NAME, $"SELECT 1 FROM Category WHERE Category = '{category}'"));
        Assert.That(await _sqlHelper.Exists(BudgetDatabaseDocker.DATABASE_NAME, $"SELECT 1 FROM Purchase WHERE Description = '{description}' AND CategoryId = {categoryId}"));

        // Act
        await _metadataContext.DeleteCategory(category);

        // Assert
        Assert.That(await _sqlHelper.Exists(BudgetDatabaseDocker.DATABASE_NAME, $"SELECT 1 FROM Category WHERE Category = '{category}'"), Is.False);
        Assert.That(await _sqlHelper.QueryAsync<int?>(BudgetDatabaseDocker.DATABASE_NAME, $"SELECT CategoryId FROM Purchase WHERE Description = '{description}'"), Is.Null);
    }
}
