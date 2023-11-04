using Backend.Interfaces;

namespace Backend.Implementations;

public class UsernamePasswordPostgresConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _serverName;
    private readonly int _port;

    public UsernamePasswordPostgresConnectionStringBuilder(string username, string password, string serverName, int port)
    {
        _username = username;
        _password = password;
        _serverName = serverName;
        _port = port;
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Port={_port};Database={database};User Id={_username};Password={_password}";
    }
}
