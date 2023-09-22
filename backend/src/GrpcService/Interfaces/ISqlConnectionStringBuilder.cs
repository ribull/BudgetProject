namespace GrpcService.Interfaces;

public interface ISqlConnectionStringBuilder
{
    string GetConnectionString(string database);
}
