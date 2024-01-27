using Backend.Interfaces;

namespace Backend.Implementations;

public class UsernamePasswordPostgresConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _serverName;
    private readonly int _port;

    public UsernamePasswordPostgresConnectionStringBuilder(IConfiguration config)
    {
        _username = config["PostgreSqlConnectionSettings:Username"]!;
        _password = config["PostgreSqlConnectionSettings:Password"]!;
        _serverName = config["PostgreSqlConnectionSettings:ServerName"]!;
        _port = int.Parse(config["PostgreSqlConnectionSettings:Port"]!);
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Port={_port};Database={database};User Id={_username};Password={_password}";
    }
}
