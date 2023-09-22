using Domain.Models;

namespace GrpcService.Interfaces;

public interface IPurchasesContext
{
    Task<IEnumerable<Purchase>> GetPurchases(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    Task AddPurchase(Purchase purchase);
}
