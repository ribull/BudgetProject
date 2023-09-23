using Backend.Services;
using BudgetProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Backend.Extensions;
using Backend.Interfaces;
using Moq;
using NUnit.Framework;

namespace BackendUnitTests;

public class PurchasesGrpcServiceUnitTests
{
    private readonly Mock<IMetadataContext> _mockMetadataContext;
    private readonly Mock<IPurchasesContext> _mockPurchasesContext;

    public PurchasesGrpcServiceUnitTests()
    {
        _mockMetadataContext = new();
        _mockPurchasesContext = new();
    }

    [SetUp]
    public void Setup()
    {
        _mockMetadataContext.Reset();
        _mockPurchasesContext.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        _mockMetadataContext.VerifyNoOtherCalls();
        _mockPurchasesContext.VerifyNoOtherCalls();
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

        _mockPurchasesContext.Setup(pc => pc.GetPurchases(description, category, startDate == null ? null : startDate, endDate == null ? null : endDate))
            .ReturnsAsync(purchases);

        PurchasesGrpcService service = new PurchasesGrpcService(_mockPurchasesContext.Object, _mockMetadataContext.Object);

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
        _mockPurchasesContext.Verify(pc => pc.GetPurchases(description, category, startDate == null ? null : startDate, endDate == null ? null : endDate), Times.Once);
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

        _mockMetadataContext.Setup(mc => mc.DoesCategoryExist(category))
            .ReturnsAsync(true);

        PurchasesGrpcService service = new PurchasesGrpcService(_mockPurchasesContext.Object, _mockMetadataContext.Object);

        // Act
        await service.AddPurchase(new AddPurchaseRequest
        {
            Description = description,
            Category = category,
            Date = date,
            Amount = amount
        }, null);

        // Assert
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExist(category), Times.Once);
        _mockPurchasesContext.Verify(pc => pc.AddPurchase(new Domain.Models.Purchase
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

        _mockMetadataContext.Setup(mc => mc.DoesCategoryExist(category))
            .ReturnsAsync(false);

        PurchasesGrpcService service = new PurchasesGrpcService(_mockPurchasesContext.Object, _mockMetadataContext.Object);

        // Act + Assert
        RpcException ex = Assert.ThrowsAsync<RpcException>(async () => await service.AddPurchase(new AddPurchaseRequest
        {
            Description = "Description",
            Category = category,
            Date = Timestamp.FromDateTime(new DateTime(2023, 10, 1).ToUniversalTime()),
            Amount = 123.45
        }, null));

        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.NotFound));
        _mockMetadataContext.Verify(mc => mc.DoesCategoryExist(category), Times.Once);
    }
}
