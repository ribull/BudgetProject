using System.Reflection;
using BudgetDatabase.Exceptions;
using DbUp;
using DbUp.Engine;

namespace BudgetDatabase.Deployer;

public static class DatabaseDeployer
{
    public static void DeployDatabase(string connectionString)
    {
        DatabaseDeployException? err = null;
        for (int i = 0; i < 5; i++)
        {
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            UpgradeEngine upgradeEngine = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            DatabaseUpgradeResult result = upgradeEngine.PerformUpgrade();

            if (result.Successful)
            {
                return;
            }

            err = new DatabaseDeployException(result);
        }

        if (err is not null)
        {
            throw err;
        }
    }
}