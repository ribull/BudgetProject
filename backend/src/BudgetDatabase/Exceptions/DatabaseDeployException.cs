using DbUp.Engine;

namespace BudgetDatabase.Exceptions;

public class DatabaseDeployException : Exception
{
    public DatabaseDeployException(DatabaseUpgradeResult result) : base($"An error occurred while trying to upgrade the database. Error: {result.Error}") { }
}