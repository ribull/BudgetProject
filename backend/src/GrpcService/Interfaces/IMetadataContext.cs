namespace Backend.Interfaces;

public interface IMetadataContext
{
    Task<bool> DoesCategoryExist(string category);

    Task AddCategory(string category);

    Task DeleteCategory(string category);

    Task UpdateCategory(string category, string updateTo);

    Task<IEnumerable<string>> GetCategories();
}
