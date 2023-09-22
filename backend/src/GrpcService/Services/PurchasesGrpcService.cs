using Grpc.Core;
using BudgetProto;
using GrpcService.Interfaces;
using GrpcService.Extensions;

namespace Budget.Services;

public class PurchasesGrpcService : PurchasesService.PurchasesServiceBase
{
    private readonly ILogger<PurchasesGrpcService> _logger;

    private readonly IPurchasesContext _purchasesContext;

    public PurchasesGrpcService(ILogger<PurchasesGrpcService> logger, IPurchasesContext purchasesContext)
    {
        _logger = logger;
        _purchasesContext = purchasesContext;
    }

    public override async Task<GetPurchasesResponse> GetPurchases(GetPurchasesRequest request, ServerCallContext context)
    {
        IEnumerable<Domain.Models.Purchase> purchases = await _purchasesContext.GetPurchases(
                description: request.Description,
                category: request.Category,
                startDate: request?.StartTime.ToDateTime(),
                endDate: request?.EndTime.ToDateTime());

        GetPurchasesResponse response = new();
        response.Purchases.Add(purchases.Select(p => p.ToPurchaseProto()));

        return response;
    }
}