using BudgetDatabase.Deployer;
using BudgetDatabase.Exceptions;
using CommandLine;

namespace BudgetDatabase;

public class DbUpArgs
{
    [Option('s', "server", Required = true)]
    public string ServerName { get; set; }

    [Option('p', "port", Required = false, Default = 5432)]
    public int Port { get; set; }

    [Option('u', "username", Required = false, Default = "postgres")]
    public string Username { get; set; }

    [Option('w', "password", Required = false, Default = "postgres")]
    public string Password { get; set; }
}

public class Program
{
    public static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<DbUpArgs>(args)
            .MapResult((dbUpArgs) =>
            {
                try
                {
                    DatabaseDeployer.DeployDatabase($"Server={dbUpArgs.ServerName};Port={dbUpArgs.Port};Database=postgres;User Id={dbUpArgs.Username};Password={dbUpArgs.Password}");
                    return 0;
                }
                catch (DatabaseDeployException e)
                {
                    Console.WriteLine(e);
                    return 1;
                }
            },
            (errs) => {
                foreach (Error err in errs)
                {
                    Console.WriteLine(err);
                }

                return 1;
            });
    }
}
