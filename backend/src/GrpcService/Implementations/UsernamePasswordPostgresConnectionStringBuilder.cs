using Backend.Interfaces;
using Domain.Models.Options;

namespace Backend.Implementations;

public class UsernamePasswordPostgresConnectionStringBuilder : ISqlConnectionStringBuilder
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _serverName;
    private readonly int _port;

    public UsernamePasswordPostgresConnectionStringBuilder(IConfiguration config)
    {
        PostgreSqlConnectionDetailsOptions? options = config.GetSection(PostgreSqlConnectionDetailsOptions.PostgreSqlConnectionDetails).Get<PostgreSqlConnectionDetailsOptions>();
        if (options is null)
        {
            throw new Exception($"There is no section {PostgreSqlConnectionDetailsOptions.PostgreSqlConnectionDetails}");
        }

        _username = options.Username;
        _password = options.Password;
        _serverName = options.ServerName;
        _port = options.Port;
    }

    public string GetConnectionString(string database)
    {
        return $"Server={_serverName};Port={_port};Database={database};User Id={_username};Password={_password}";
    }
}
