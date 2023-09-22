using GrpcService.Interfaces;

namespace GrpcService.Implementations;

public class StandardConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private string _username;
    private string _password;
    private string _serverName;

    public StandardConnectionStringBuilder(string username, string password, string serverName)
    {
        _username = username;
        _password = password;
        _serverName = serverName;
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Database={database};User Id={_username};Password={_password};TrustServerCertificate=True;Encrypt=False;";
    }
}
