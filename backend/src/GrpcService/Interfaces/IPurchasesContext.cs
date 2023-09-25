using Domain.Models;

namespace Backend.Interfaces;

public interface IPurchasesContext
{
    Task<IEnumerable<Purchase>> GetPurchasesAsync(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    Task AddPurchaseAsync(Purchase purchase);
}
