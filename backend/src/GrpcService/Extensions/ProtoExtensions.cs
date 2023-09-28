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
}
