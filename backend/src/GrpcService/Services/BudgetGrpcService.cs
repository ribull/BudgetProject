using BudgetProto;
using Backend.Interfaces;
using Grpc.Core;
using Backend.Extensions;

namespace Backend.Services;

public class BudgetGrpcService : BudgetService.BudgetServiceBase
{
    private readonly IBudgetDatabaseContext _budgetDatabaseContext;

    public BudgetGrpcService(IBudgetDatabaseContext budgetDatabaseContext)
    {
        _budgetDatabaseContext = budgetDatabaseContext;
    }

    public override async Task<GetPurchasesResponse> GetPurchases(GetPurchasesRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Purchase> purchases = await _budgetDatabaseContext.GetPurchasesAsync(
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
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist"));
        }

        await _budgetDatabaseContext.AddPurchaseAsync(new Domain.Models.Purchase
        {
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category
        });

        return new AddPurchaseResponse();
    }

    public override async Task<AddCategoryResponse> AddCategory(AddCategoryRequest request, ServerCallContext? context)
    {
        if (await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"The category {request.Category} already exists."));
        }

        await _budgetDatabaseContext.AddCategoryAsync(request.Category);
        return new AddCategoryResponse();
    }

    public override async Task<DeleteCategoryResponse> DeleteCategory(DeleteCategoryRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist."));
        }

        await _budgetDatabaseContext.DeleteCategoryAsync(request.Category);
        return new DeleteCategoryResponse();
    }

    public override async Task<UpdateCategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist."));
        }

        if (await _budgetDatabaseContext.DoesCategoryExistAsync(request.UpdateTo))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"The category {request.UpdateTo} already exists."));
        }

        await _budgetDatabaseContext.UpdateCategoryAsync(request.Category, request.UpdateTo);
        return new UpdateCategoryResponse();
    }

    public override async Task<GetCategoriesResponse> GetCategories(GetCategoriesRequest request, ServerCallContext? context)
    {
        GetCategoriesResponse response = new();
        response.Categories.Add(await _budgetDatabaseContext.GetCategoriesAsync());

        return response;
    }
}
