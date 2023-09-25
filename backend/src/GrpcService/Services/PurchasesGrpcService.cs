using Grpc.Core;
using BudgetProto;
using Backend.Interfaces;
using Backend.Extensions;

namespace Backend.Services;

public class PurchasesGrpcService : PurchasesService.PurchasesServiceBase
{
    private readonly IPurchasesContext _purchasesContext;
    private readonly IMetadataContext _metadataContext;

    public PurchasesGrpcService(IPurchasesContext purchasesContext, IMetadataContext metadataContext)
    {
        _purchasesContext = purchasesContext;
        _metadataContext = metadataContext;
    }

    public override async Task<GetPurchasesResponse> GetPurchases(GetPurchasesRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Purchase> purchases = await _purchasesContext.GetPurchasesAsync(
                description: request.Description,
                category: request.Category,
                startDate: request.StartTime?.ToDateTime(),
                endDate: request.EndTime?.ToDateTime());

        GetPurchasesResponse response = new();
        response.Purchases.Add(purchases.Select(p => p.ToPurchaseProto()));

        return response;
    }

    public override async Task<AddPurchaseResponse> AddPurchase(AddPurchaseRequest request, ServerCallContext? context)
    {
        if (!await _metadataContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist"));
        }

        await _purchasesContext.AddPurchaseAsync(new Domain.Models.Purchase
        {
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category
        });

        return new AddPurchaseResponse();
    }
}