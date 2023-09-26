using Domain.Models;

namespace Backend.Interfaces;

public interface IPayHistoryContext
{
    Task AddPayHistoryAsync(PayHistory payHistory);

    Task AddPayHistoriesAsync(IEnumerable<PayHistory> payHistories);

    Task<IEnumerable<PayHistory>> GetPayHistoriesAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task<IEnumerable<PayHistory>> GetPayHistoriesForMonthAsync(DateTime month);
}
