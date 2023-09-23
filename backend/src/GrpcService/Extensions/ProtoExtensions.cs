using Google.Protobuf.WellKnownTypes;

namespace Backend.Extensions;

public static class ProtoExtensions
{
	public static BudgetProto.Purchase ToPurchaseProto(this Domain.Models.Purchase purchase)
	{
		return new BudgetProto.Purchase
		{
			Id = purchase.PurchaseId,
			Date = purchase.Date.ToUniversalTime().ToTimestamp(),
			Description = purchase.Description,
			Amount = purchase.Amount,
			Category = purchase.Category
		};
    }
}
