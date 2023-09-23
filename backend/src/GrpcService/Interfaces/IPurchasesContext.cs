using Domain.Models;

namespace Backend.Interfaces;

public interface IPurchasesContext
{
    Task<IEnumerable<Purchase>> GetPurchases(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    Task AddPurchase(Purchase purchase);
}
