using Backend.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.HealthChecks;

public class DatabaseOnlineHealthCheck : IHealthCheck
{
    private readonly ISqlHelper SqlHelper;
    private readonly string _databaseName;

    public DatabaseOnlineHealthCheck(IConfiguration config, ISqlHelper sqlHelper)
    {
        _databaseName = config["BudgetDatabaseName"]!;
        SqlHelper = sqlHelper;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            if (await SqlHelper.ExistsAsync("postgres", "SELECT 1 FROM pg_catalog.pg_database WHERE datname = @databaseName", new { databaseName = _databaseName }))
            {
                return HealthCheckResult.Healthy();
            }
            else
            {
                return HealthCheckResult.Degraded("PostgreSql server is online, but the database does not exist.");
            }
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(e.Message);
        }
    }
}
