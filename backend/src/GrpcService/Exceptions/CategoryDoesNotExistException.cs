namespace Backend.Exceptions;

public class CategoryDoesNotExistException : Exception
{
    public CategoryDoesNotExistException(string category) : base($"The category '{category}' does not exist. You must create it first.")
    { }
}
