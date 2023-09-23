namespace Backend.Interfaces;

public interface ISqlConnectionStringBuilder
{
    string GetConnectionString(string database);
}
