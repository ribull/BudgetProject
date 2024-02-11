using Google.Protobuf.WellKnownTypes;

namespace Backend.Extensions;

public static class ProtoExtensions
{
	public static BudgetProto.Purchase ToPurchaseProto(this Domain.Models.Purchase purchase)
	{
		return new BudgetProto.Purchase
		{
			PurchaseId = purchase.PurchaseId,
			Date = purchase.Date.ToUniversalTime().ToTimestamp(),
			Description = purchase.Description,
			Amount = purchase.Amount,
			Category = purchase.Category
		};
    }

    public static BudgetProto.PayHistory ToPayHistoryProto(this Domain.Models.PayHistory payHistory)
    {
        return new BudgetProto.PayHistory
        {
			PayHistoryId = payHistory.PayHistoryId,
			PayPeriodStartDate = payHistory.PayPeriodStartDate.ToUniversalTime().ToTimestamp(),
			PayPeriodEndDate = payHistory.PayPeriodEndDate.ToUniversalTime().ToTimestamp(),
			Earnings = payHistory.Earnings,
			PreTaxDeductions = payHistory.PreTaxDeductions,
			Taxes = payHistory.Taxes,
			PostTaxDeductions = payHistory.PostTaxDeductions
        };
    }

	public static BudgetProto.Era ToEraProto(this Domain.Models.Era era)
	{
		return new BudgetProto.Era
		{
			EraId = era.EraId,
			Name = era.Name,
			StartDate = era.StartDate.ToUniversalTime().ToTimestamp(),
			EndDate = era.EndDate?.ToUniversalTime().ToTimestamp(),
		};
	}

	public static BudgetProto.FuturePurchase ToFuturePurchaseProto(this Domain.Models.FuturePurchase futurePurchase)
	{
		return new BudgetProto.FuturePurchase
		{
			FuturePurchaseId = futurePurchase.FuturePurchaseId,
            Date = futurePurchase.Date.ToUniversalTime().ToTimestamp(),
            Description = futurePurchase.Description,
            Amount = futurePurchase.Amount,
            Category = futurePurchase.Category
        };
	}

	public static BudgetProto.Investment ToInvestmentProto(this Domain.Models.Investment investment)
	{
		return new BudgetProto.Investment
		{
			InvestmentId = investment.InvestmentId,
			Description = investment.Description,
			CurrentAmount = investment.CurrentAmount,
			YearlyGrowthRate = investment.YearlyGrowthRate,
			LastUpdated = investment.LastUpdated.ToUniversalTime().ToTimestamp()
		};
	}

	public static BudgetProto.Saved ToSavedProto(this Domain.Models.Saved saved)
	{
		return new BudgetProto.Saved
		{
			SavedId = saved.SavedId,
			Date = saved.Date.ToUniversalTime().ToTimestamp(),
			Description = saved.Description,
			Amount = saved.Amount,
		};
	}

	public static BudgetProto.WishlistItem ToWishlistItemProto(this Domain.Models.WishlistItem wishlistItem)
	{
		return new BudgetProto.WishlistItem
		{
			WishlistItemId = wishlistItem.WishlistItemId,
			Description = wishlistItem.Description,
			Amount = wishlistItem.Amount,
			Notes = wishlistItem.Notes
		};
	}
}
