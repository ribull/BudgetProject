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

    public override async Task<AddPayHistoryResponse> AddPayHistory(AddPayHistoryRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.AddPayHistoryAsync(new Domain.Models.PayHistory
        {
            PayPeriodStartDate = request.PayPeriodStartDate.ToDateTime(),
            PayPeriodEndDate = request.PayPeriodEndDate.ToDateTime(),
            Earnings = request.Earnings,
            PreTaxDeductions = request.PreTaxDeductions,
            Taxes = request.Taxes,
            PostTaxDeductions = request.PostTaxDeductions
        });

        return new AddPayHistoryResponse();
    }

    public override async Task<DeletePayHistoryResponse> DeletePayHistory(DeletePayHistoryRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesPayHistoryExistAsync(request.PayHistoryId))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The pay history with id {request.PayHistoryId} does not exist."));
        }

        await _budgetDatabaseContext.DeletePayHistoryAsync(request.PayHistoryId);

        return new DeletePayHistoryResponse();
    }

    public override async Task<GetPayHistoriesResponse> GetPayHistories(GetPayHistoriesRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.PayHistory> payHistories = await _budgetDatabaseContext.GetPayHistoriesAsync(request.StartTime?.ToDateTime(), request.EndTime?.ToDateTime());
        
        GetPayHistoriesResponse response = new();
        response.PayHistories.Add(payHistories.Select(ph => ph.ToPayHistoryProto()));

        return response;
    }

    public override async Task<GetPayHistoriesForMonthResponse> GetPayHistoriesForMonth(GetPayHistoriesForMonthRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.PayHistory> payHistories = await _budgetDatabaseContext.GetPayHistoriesForMonthAsync(request.Month.ToDateTime());
        
        GetPayHistoriesForMonthResponse response = new();
        response.PayHistories.Add(payHistories.Select(ph => ph.ToPayHistoryProto()));

        return response;
    }
}
