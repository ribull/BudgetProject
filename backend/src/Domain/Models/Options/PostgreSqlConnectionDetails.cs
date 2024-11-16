namespace Domain.Models.Options;

public class PostgreSqlConnectionDetailsOptions
{
    public const string PostgreSqlConnectionDetails = nameof(PostgreSqlConnectionDetails);

    public string ServerName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Port { get; set; }
}
