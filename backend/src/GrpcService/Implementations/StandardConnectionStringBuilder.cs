using Backend.Interfaces;

namespace Backend.Implementations;

public class StandardConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _serverName;

    public StandardConnectionStringBuilder(string username, string password, string serverName)
    {
        _username = username;
        _password = password;
        _serverName = serverName;
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Database={database};User Id={_username};Password={_password};TrustServerCertificate=True";
    }
}
