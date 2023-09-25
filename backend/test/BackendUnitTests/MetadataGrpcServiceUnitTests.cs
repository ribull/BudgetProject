using Backend.Services;
using BudgetProto;
using Grpc.Core;
using Backend.Interfaces;
using Moq;
using NUnit.Framework;

namespace BackendUnitTests;

public class MetadataGrpcServiceUnitTests
{
    private readonly Mock<IMetadataContext> _mockMetadataContext;

    public MetadataGrpcServiceUnitTests()
    {
        _mockMetadataContext = new();
    }

    [SetUp]
    public void Setup()
    {
        _mockMetadataContext.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        _mockMetadataContext.VerifyNoOtherCalls();
    }

    [Test]
    public async Task AddCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act
        await grpcService.AddCategory(new AddCategoryRequest { Category = category }, null);

        // Assert
        _mockMetadataContext.Verify(mc => mc.AddCategoryAsync(category), Times.Once);
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void AddCategoryAlreadyExistsTest()
    {
        // Arrange
        string category = "Utilities";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.AddCategory(new AddCategoryRequest { Category = category }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public async Task DeleteCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act
        await grpcService.DeleteCategory(new DeleteCategoryRequest { Category = category }, null);

        // Assert
        _mockMetadataContext.Verify(mc => mc.DeleteCategoryAsync(category), Times.Once);
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void DeleteCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "Utilities";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.DeleteCategory(new DeleteCategoryRequest { Category = category }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public async Task UpdateCategoryTest()
    {
        // Arrange
        string category = "Utilities";
        string updateTo = "UpdateTo";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(updateTo))
            .ReturnsAsync(false);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act
        await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = updateTo }, null);

        // Assert
        _mockMetadataContext.Verify(mc => mc.UpdateCategoryAsync(category, updateTo), Times.Once);
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(updateTo), Times.Once);
    }

    [Test]
    public void UpdateCategoryDoesNotExistTest()
    {
        // Arrange
        string category = "Utilities";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(false);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = "Irrelevant" }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
    }

    [Test]
    public void UpdateToCategoryAlreadyExistsTest()
    {
        // Arrange
        string category = "Utilities";
        string updateTo = "UpdateTo";
        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(category))
            .ReturnsAsync(true);

        _mockMetadataContext.Setup(mc => mc.DoesCategoryExistAsync(updateTo))
            .ReturnsAsync(true);

        MetadataGrpcService grpcService = new(_mockMetadataContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await grpcService.UpdateCategory(new UpdateCategoryRequest { Category = category, UpdateTo = updateTo }, null));
        Assert.That(ex.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(category), Times.Once);
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExistAsync(updateTo), Times.Once);
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

        _mockMetadataContext.Setup(mc => mc.GetCategoriesAsync())
            .ReturnsAsync(categories);

        MetadataGrpcService metadataGrpcService = new(_mockMetadataContext.Object);

        // Act
        GetCategoriesResponse response = await metadataGrpcService.GetCategories(new GetCategoriesRequest(), null);

        // Assert
        _mockMetadataContext.Verify(mc => mc.GetCategoriesAsync(), Times.Once);
        Assert.That(response.Categories, Is.EquivalentTo(categories));
    }
}