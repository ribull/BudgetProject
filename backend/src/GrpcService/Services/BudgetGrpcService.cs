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

    #region Purchase

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

    public override async Task<GetMostCommonPurchasesResponse> GetMostCommonPurchases(GetMostCommonPurchasesRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Purchase> purchases = await _budgetDatabaseContext.GetMostCommonPurchasesAsync(
            category: request.Category,
            startDate: request.StartTime?.ToDateTime(),
            endDate: request.EndTime?.ToDateTime(),
            count: request.Count);

        GetMostCommonPurchasesResponse response = new();
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

    public override async Task<UpdatePurchaseResponse> UpdatePurchase(UpdatePurchaseRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist"));
        }

        await _budgetDatabaseContext.UpdatePurchaseAsync(new Domain.Models.Purchase
        {
            PurchaseId = request.PurchaseId,
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category
        });

        return new UpdatePurchaseResponse();
    }

    public override async Task<DeletePurchaseResponse> DeletePurchase(DeletePurchaseRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeletePurchaseAsync(request.PurchaseId);
        return new DeletePurchaseResponse();
    }

    #endregion Purchase

    #region Category

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

    #endregion Category

    #region Pay History

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

    public override async Task<UpdatePayHistoryResponse> UpdatePayHistory(UpdatePayHistoryRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.UpdatePayHistoryAsync(new Domain.Models.PayHistory
        {
            PayHistoryId = request.PayHistoryId,
            PayPeriodStartDate = request.PayPeriodStartDate.ToDateTime(),
            PayPeriodEndDate = request.PayPeriodEndDate.ToDateTime(),
            Earnings = request.Earnings,
            PreTaxDeductions = request.PreTaxDeductions,
            Taxes = request.Taxes,
            PostTaxDeductions = request.PostTaxDeductions
        });

        return new UpdatePayHistoryResponse();
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

    #endregion Pay History

    #region Era

    public override async Task<GetErasResponse> GetEras(GetErasRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Era> eras = await _budgetDatabaseContext.GetErasAsync();

        GetErasResponse response = new();
        response.Eras.Add(eras.Select(era => era.ToEraProto()));

        return response;
    }

    public override async Task<AddEraResponse> AddEra(AddEraRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.AddEraAsync(new Domain.Models.Era
        {
            Name = request.Name,
            StartDate = request.StartDate.ToDateTime(),
            EndDate = request.EndDate.ToDateTime()
        });

        return new AddEraResponse();
    }

    public override async Task<UpdateEraResponse> UpdateEra(UpdateEraRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.UpdateEraAsync(new Domain.Models.Era
        {
            EraId = request.EraId,
            Name = request.Name,
            StartDate = request.StartDate.ToDateTime(),
            EndDate = request.EndDate.ToDateTime()
        });

        return new UpdateEraResponse();
    }

    public override async Task<DeleteEraResponse> DeleteEra(DeleteEraRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeleteEraAsync(request.EraId);
        return new DeleteEraResponse();
    }

    #endregion Era

    #region FuturePurchase

    public override async Task<GetFuturePurchasesResponse> GetFuturePurchases(GetFuturePurchasesRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.FuturePurchase> futurePurchases = await _budgetDatabaseContext.GetFuturePurchasesAsync();

        GetFuturePurchasesResponse response = new();
        response.FuturePurchases.Add(futurePurchases.Select(futurePurchase => futurePurchase.ToFuturePurchaseProto()));

        return response;
    }

    public override async Task<AddFuturePurchaseResponse> AddFuturePurchase(AddFuturePurchaseRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist"));
        }

        await _budgetDatabaseContext.AddFuturePurchaseAsync(new Domain.Models.FuturePurchase
        {
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category
        });

        return new AddFuturePurchaseResponse();
    }

    public override async Task<UpdateFuturePurchaseResponse> UpdateFuturePurchase(UpdateFuturePurchaseRequest request, ServerCallContext? context)
    {
        if (!await _budgetDatabaseContext.DoesCategoryExistAsync(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist"));
        }

        await _budgetDatabaseContext.UpdateFuturePurchaseAsync(new Domain.Models.FuturePurchase
        {
            FuturePurchaseId = request.FuturePurchaseId,
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category
        });

        return new UpdateFuturePurchaseResponse();
    }

    public override async Task<DeleteFuturePurchaseResponse> DeleteFuturePurchase(DeleteFuturePurchaseRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeleteFuturePurchaseAsync(request.FuturePurchaseId);
        return new DeleteFuturePurchaseResponse();
    }

    #endregion FuturePurchase

    #region Investment

    public override async Task<GetInvestmentsResponse> GetInvestments(GetInvestmentsRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Investment> investments = await _budgetDatabaseContext.GetInvestmentsAsync();

        GetInvestmentsResponse response = new();
        response.Investments.Add(investments.Select(investment => investment.ToInvestmentProto()));

        return response;
    }

    public override async Task<AddInvestmentResponse> AddInvestment(AddInvestmentRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.AddInvestmentAsync(new Domain.Models.Investment
        {
            Description = request.Description,
            CurrentAmount = request.CurrentAmount,
            YearlyGrowthRate = request.YearlyGrowthRate,
        });

        return new AddInvestmentResponse();
    }

    public override async Task<UpdateInvestmentResponse> UpdateInvestment(UpdateInvestmentRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.UpdateInvestmentAsync(new Domain.Models.Investment
        {
            InvestmentId = request.InvestmentId,
            Description = request.Description,
            CurrentAmount = request.CurrentAmount,
            YearlyGrowthRate = request.YearlyGrowthRate,
        });

        return new UpdateInvestmentResponse();
    }

    public override async Task<DeleteInvestmentResponse> DeleteInvestment(DeleteInvestmentRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeleteInvestmentAsync(request.InvestmentId);
        return new DeleteInvestmentResponse();
    }

    #endregion Investment

    #region Saved

    public override async Task<GetSavingsResponse> GetSavings(GetSavingsRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.Saved> savings = await _budgetDatabaseContext.GetSavingsAsync();

        GetSavingsResponse response = new();
        response.Savings.Add(savings.Select(saved => saved.ToSavedProto()));

        return response;
    }

    public override async Task<AddSavedResponse> AddSaved(AddSavedRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.AddSavingAsync(new Domain.Models.Saved
        {
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
        });

        return new AddSavedResponse();
    }

    public override async Task<UpdateSavedResponse> UpdateSaved(UpdateSavedRequest request, ServerCallContext? context)
    {

        await _budgetDatabaseContext.UpdateSavingAsync(new Domain.Models.Saved
        {
            SavedId = request.SavedId,
            Date = request.Date.ToDateTime(),
            Description = request.Description,
            Amount = request.Amount,
        });

        return new UpdateSavedResponse();
    }

    public override async Task<DeleteSavedResponse> DeleteSaved(DeleteSavedRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeleteSavedAsync(request.SavedId);
        return new DeleteSavedResponse();
    }

    #endregion Saved

    #region Wishlist

    public override async Task<GetWishlistResponse> GetWishlist(GetWishlistRequest request, ServerCallContext? context)
    {
        IEnumerable<Domain.Models.WishlistItem> WishlistItems = await _budgetDatabaseContext.GetWishlistAsync();

        GetWishlistResponse response = new();
        response.WishlistItems.Add(WishlistItems.Select(wishlistItem => wishlistItem.ToWishlistItemProto()));

        return response;
    }

    public override async Task<AddWishlistItemResponse> AddWishlistItem(AddWishlistItemRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.AddWishlistItemAsync(new Domain.Models.WishlistItem
        {
            Description = request.Description,
            Amount = request.Amount,
            Notes = request.Notes
        });

        return new AddWishlistItemResponse();
    }

    public override async Task<UpdateWishlistItemResponse> UpdateWishlistItem(UpdateWishlistItemRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.UpdateWishlistItemAsync(new Domain.Models.WishlistItem
        {
            WishlistItemId = request.WishlistItemId,
            Description = request.Description,
            Amount = request.Amount,
            Notes = request.Notes
        });

        return new UpdateWishlistItemResponse();
    }

    public override async Task<DeleteWishlistItemResponse> DeleteWishlistItem(DeleteWishlistItemRequest request, ServerCallContext? context)
    {
        await _budgetDatabaseContext.DeleteWishlistItemAsync(request.WishlistItemId);
        return new DeleteWishlistItemResponse();
    }

    #endregion Wishlist
}
