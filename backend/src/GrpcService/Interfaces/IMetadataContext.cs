namespace Backend.Interfaces;

public interface IMetadataContext
{
    Task<bool> DoesCategoryExistAsync(string category);

    Task AddCategoryAsync(string category);

    Task DeleteCategoryAsync(string category);

    Task UpdateCategoryAsync(string category, string updateTo);

    Task<IEnumerable<string>> GetCategoriesAsync();
}
