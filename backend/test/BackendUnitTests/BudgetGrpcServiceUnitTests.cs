using Backend.Services;
using BudgetProto;
using Grpc.Core;
using Backend.Interfaces;
using Moq;
using NUnit.Framework;
using Backend.Extensions;
using Google.Protobuf.WellKnownTypes;

namespace BackendUnitTests;

[TestFixture]
public class BudgetGrpcServiceUnitTests
{
    private readonly Mock<IBudgetDatabaseContext> _mockBudgetDatabaseContext;

    public BudgetGrpcServiceUnitTests()
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
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act
        await grpcService.AddCategory(new AddCategoryRequest { Category = category }, null);

        // Assert
        _mockBudgetDatabaseContext.Verify(mc => mc.AddCategoryAsync(category), Times.Once);
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void AddCategoryAlreadyExistsTest()
    {
        // Arrange
        string category = "Utilities";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.AddCategory(new AddCategoryRequest { Category = category }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act
        await grpcService.DeleteCategory(new DeleteCategoryRequest { Category = category }, null);

        // Assert
        _mockBudgetDatabaseContext.Verify(mc => mc.DeleteCategoryAsync(category), Times.Once);
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void DeleteCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "Utilities";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.DeleteCategory(new DeleteCategoryRequest { Category = category }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public async Task UpdateCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        string updateTo = "UpdateTo";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(updateTo))
            .ReturnsAsync(false);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act
        await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = updateTo }, null);

        // Assert
        _mockBudgetDatabaseContext.Verify(mc => mc.UpdateCategoryAsync(category, updateTo), Times.Once);
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(updateTo), Times.Once);
    }

    [Test]
    public void UpdateCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "Utilities";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = "Irrelevant" }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void UpdateToCategoryAlreadyExistsTest()
    {
        // Arrange
        string category = "Utilities";
        string updateTo = "UpdateTo";
        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(updateTo))
            .ReturnsAsync(true);

        BudgetGrpcService grpcService = new(_mockBudgetDatabaseContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = updateTo }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(updateTo), Times.Once);
    }

    [Test]
    public async Task GetCategoriesTest()
    {
        // Arrange
        List<string> categories = new()
        {
            "Utilities",
            "Gas",
            "Test",
            "Gifts",
            "Balderdash",
            "Random Category",
            "AYEEEEEEEEEEeeee"
        };

        _mockBudgetDatabaseContext.Setup(mc => mc.GetCategoriesAsync())
            .ReturnsAsync(categories);

        BudgetGrpcService metadataGrpcService = new(_mockBudgetDatabaseContext.Object);

        // Act
        GetCategoriesResponse response = await metadataGrpcService.GetCategories(new GetCategoriesRequest(), null);

        // Assert
        _mockBudgetDatabaseContext.Verify(mc => mc.GetCategoriesAsync(), Times.Once);
        Assert.That(response.Categories, Is.EquivalentTo(categories));
    }

    [Test]
    public async Task GetPurchasesRequestTest(
        [Values("Description", null)] string? description,
        [Values("Category", null)] string? category,
        [Values("2023-10-1", null)] DateTime? startDate,
        [Values("2023-10-2", null)] DateTime? endDate)
    {
        // Arrange
        List<Domain.Models.Purchase> purchases = new()
        {
            new Domain.Models.Purchase
            {
                PurchaseId = 1,
                Category = "Category",
                Description = "Description",
                Date = new DateTime(2023, 9, 18),
                Amount = 123.45
            },
            new Domain.Models.Purchase
            {
                PurchaseId = 2,
                Category = "Category2",
                Description = "Description2",
                Date = new DateTime(2023, 9, 12),
                Amount = 678.90
            },
            new Domain.Models.Purchase
            {
                PurchaseId = 8,
                Category = "Category15",
                Description = "Description15",
                Date = new DateTime(2023, 9, 21),
                Amount = 17548.99
            }
        };

        _mockBudgetDatabaseContext.Setup(pc => pc.GetPurchasesAsync(description, category, startDate == null ? null : startDate, endDate == null ? null : endDate))
            .ReturnsAsync(purchases);

        BudgetGrpcService service = new BudgetGrpcService(_mockBudgetDatabaseContext.Object);

        GetPurchasesRequest request = new();
        if (description is not null)
        {
            request.Description = description;
        }

        if (category is not null)
        {
            request.Category = category;
        }

        if (startDate is not null)
        {
            request.StartTime = Timestamp.FromDateTime(startDate.Value.ToUniversalTime());
        }

        if (endDate is not null)
        {
            request.EndTime = Timestamp.FromDateTime(endDate.Value.ToUniversalTime());
        }

        // Act
        GetPurchasesResponse response = await service.GetPurchases(request, null);

        // Assert
        _mockBudgetDatabaseContext.Verify(pc => pc.GetPurchasesAsync(description, category, startDate == null ? null : startDate, endDate == null ? null : endDate), Times.Once);
        Assert.That(response.Purchases, Is.EquivalentTo(purchases.Select(p => p.ToPurchaseProto())));
    }

    [Test]
    public async Task AddPurchaseTest()
    {
        // Arrange
        string description = "Description";
        string category = "Category";
        Timestamp date = Timestamp.FromDateTime(new DateTime(2023, 10, 1).ToUniversalTime());
        double amount = 123.45;

        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        BudgetGrpcService service = new BudgetGrpcService(_mockBudgetDatabaseContext.Object);

        // Act
        await service.AddPurchase(new AddPurchaseRequest
        {
            Description = description,
            Category = category,
            Date = date,
            Amount = amount
        }, null);

        // Assert
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
        _mockBudgetDatabaseContext.Verify(pc => pc.AddPurchaseAsync(new Domain.Models.Purchase
        {
            Description = description,
            Category = category,
            Date = date.ToDateTime(),
            Amount = amount
        }), Times.Once);
    }

    [Test]
    public void AddPurchaseCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "Category";

        _mockBudgetDatabaseContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        BudgetGrpcService service = new BudgetGrpcService(_mockBudgetDatabaseContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await service.AddPurchase(new AddPurchaseRequest
        {
            Description = "Description",
            Category = category,
            Date = Timestamp.FromDateTime(new DateTime(2023, 10, 1).ToUniversalTime()),
            Amount = 123.45
        }, null));

        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockBudgetDatabaseContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }
}
