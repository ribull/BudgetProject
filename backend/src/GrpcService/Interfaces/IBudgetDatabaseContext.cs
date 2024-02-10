﻿using Domain.Models;

namespace Backend.Interfaces;

public interface IBudgetDatabaseContext
{
    #region Category

    Task<bool> DoesCategoryExistAsync(string category);

    Task AddCategoryAsync(string category);

    Task DeleteCategoryAsync(string category);

    Task UpdateCategoryAsync(string category, string updateTo);

    Task<IEnumerable<string>> GetCategoriesAsync();

    #endregion Category

    #region Purchase

    Task<IEnumerable<Purchase>> GetPurchasesAsync(string? description = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    Task<IEnumerable<Purchase>> GetMostCommonPurchasesAsync(string? category = null, DateTime? startDate = null, DateTime? endDate = null, int? count = null);

    Task AddPurchaseAsync(Purchase purchase);

    Task AddPurchasesAsync(IAsyncEnumerable<Purchase> purchases);

    Task UpdatePurchaseAsync(Purchase purchase);

    Task DeletePurchaseAsync(int purchaseId);

    #endregion Purchase

    #region PayHistory

    Task AddPayHistoryAsync(PayHistory payHistory);

    Task AddPayHistoriesAsync(IEnumerable<PayHistory> payHistories);

    Task AddPayHistoriesAsync(IAsyncEnumerable<PayHistory> payHistories);

    Task UpdatePayHistoryAsync(PayHistory payHistory);

    Task<bool> DoesPayHistoryExistAsync(int purchaseId);

    Task DeletePayHistoryAsync(int purchaseId);

    Task<IEnumerable<PayHistory>> GetPayHistoriesAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task<IEnumerable<PayHistory>> GetPayHistoriesForMonthAsync(DateTime month);

    #endregion PayHistory
}
