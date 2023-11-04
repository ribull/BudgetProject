using Backend.Interfaces;

namespace Backend.Implementations;

public class UsernamePasswordSqlServerConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private readonly string _serverName;
    private readonly int _port;

    public UsernamePasswordSqlServerConnectionStringBuilder(string serverName, int port)
    {
        _serverName = serverName;
        _port = port;
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Port={_port};Database={database}";
    }
}