using Backend.Exceptions;
using Backend.Implementations;
using Backend.Interfaces;
using BudgetDatabase.Deployer;
using BudgetProto;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Testcontainers.PostgreSql;
using PayHistory = Domain.Models.PayHistory;
using Purchase = Domain.Models.Purchase;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

[TestFixture]
public class BaseBudgetDatabaseContextFunctionalTests
{
    private ISqlConnectionStringBuilder _connectionStringBuilder;
    private IConfiguration _config;

    private PostgreSqlContainer _postgreSqlContainer;

    protected ISqlHelper SqlHelper;
    protected string BudgetDatabaseName;
    protected IBudgetDatabaseContext BudgetDatabaseContext;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .Build();

        await _postgreSqlContainer.StartAsync();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  _postgreSqlContainer.Hostname },
                { "PostgreSqlConnectionDetails:Username", "postgres" },
                { "PostgreSqlConnectionDetails:Password", "postgres" },
                { "PostgreSqlConnectionDetails:Port", $"{_postgreSqlContainer.GetMappedPublicPort(5432)}" }
            }).Build();

        _connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(_config);

        SqlHelper = new SqlHelper(_connectionStringBuilder);
        BudgetDatabaseName = _config["BudgetDatabaseName"]!;
        BudgetDatabaseContext = new BudgetDatabaseContext(_config, SqlHelper);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        DatabaseDeployer.DeployDatabase(_connectionStringBuilder.GetConnectionString(BudgetDatabaseName));
    }

    [TearDown]
    public async Task TearDown()
    {
        await SqlHelper.ExecuteAsync("postgres", $"DROP DATABASE {BudgetDatabaseName} WITH (FORCE)");
    }

    protected async Task AddCategory(int id, string category)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
$@"INSERT INTO Category
(
    CategoryId,
    Category
)
VALUES
(
    {id},
    '{category}'
);");
    }
}
