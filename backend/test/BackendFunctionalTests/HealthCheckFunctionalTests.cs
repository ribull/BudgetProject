﻿using Backend.HealthChecks;
using Backend.Implementations;
using Backend.Interfaces;
using BudgetDatabase.Deployer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace BackendFunctionalTests;

[TestFixture]
public class HealthCheckFunctionalTests
{
    [Test]
    public async Task DatabaseIsOnlineTest()
    {
        // Arrange
        PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .Build();

        await postgreSqlContainer.StartAsync();

        IConfiguration config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  postgreSqlContainer.Hostname },
                { "PostgreSqlConnectionDetails:Username", "postgres" },
                { "PostgreSqlConnectionDetails:Password", "postgres" },
                { "PostgreSqlConnectionDetails:Port", $"{postgreSqlContainer.GetMappedPublicPort(5432)}" }
        }).Build();

        ISqlConnectionStringBuilder connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(config);
        ISqlHelper sqlHelper = new SqlHelper(connectionStringBuilder);

        DatabaseDeployer.DeployDatabase(connectionStringBuilder.GetConnectionString(config["BudgetDatabaseName"]!));

        // Act
        HealthCheckResult healthCheckResult = await new DatabaseOnlineHealthCheck(config, sqlHelper).CheckHealthAsync(new HealthCheckContext(), new CancellationToken());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Healthy));
        });

        // Cleanup
        await postgreSqlContainer.DisposeAsync();
    }

    [Test]
    public async Task ServerIsOnlineButDatabaseDoesNotExistTest()
    {
        // Arrange
        PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .Build();

        await postgreSqlContainer.StartAsync();

        IConfiguration config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  postgreSqlContainer.Hostname },
                { "PostgreSqlConnectionDetails:Username", "postgres" },
                { "PostgreSqlConnectionDetails:Password", "postgres" },
                { "PostgreSqlConnectionDetails:Port", $"{postgreSqlContainer.GetMappedPublicPort(5432)}" }
        }).Build();

        ISqlConnectionStringBuilder connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(config);
        ISqlHelper sqlHelper = new SqlHelper(connectionStringBuilder);

        // Act
        HealthCheckResult healthCheckResult = await new DatabaseOnlineHealthCheck(config, sqlHelper).CheckHealthAsync(new HealthCheckContext(), new CancellationToken());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Degraded));
            Assert.That(healthCheckResult.Description, Is.EqualTo("PostgreSql server is online, but the database does not exist."));
        });

        // Cleanup
        await postgreSqlContainer.DisposeAsync();
    }

    [Test]
    [TestCase("postgres", "badpassword")]
    [TestCase("badusername", "badpassword")]
    public async Task InvalidCredentialsTest(string username, string password)
    {
        // Arrange
        PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .Build();

        await postgreSqlContainer.StartAsync();

        IConfiguration config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  postgreSqlContainer.Hostname },
                { "PostgreSqlConnectionDetails:Username", username },
                { "PostgreSqlConnectionDetails:Password", password },
                { "PostgreSqlConnectionDetails:Port", $"{postgreSqlContainer.GetMappedPublicPort(5432)}" }
        }).Build();

        ISqlConnectionStringBuilder connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(config);
        ISqlHelper sqlHelper = new SqlHelper(connectionStringBuilder);

        // Act
        HealthCheckResult healthCheckResult = await new DatabaseOnlineHealthCheck(config, sqlHelper).CheckHealthAsync(new HealthCheckContext(), new CancellationToken());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(healthCheckResult.Description, Is.EqualTo(@$"28P01: password authentication failed for user ""{username}"""));
        });

        // Cleanup
        await postgreSqlContainer.DisposeAsync();
    }

    [Test]
    public async Task ServerIsNotOnline()
    {
        IConfiguration config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
                { "BudgetDatabaseName", "budgetdb" },
                { "PostgreSqlConnectionDetails:ServerName",  "irrelevant" },
                { "PostgreSqlConnectionDetails:Username", "postgres" },
                { "PostgreSqlConnectionDetails:Password", "postgres" },
                { "PostgreSqlConnectionDetails:Port", "1234" }
        }).Build();

        ISqlConnectionStringBuilder connectionStringBuilder = new UsernamePasswordPostgresConnectionStringBuilder(config);
        ISqlHelper sqlHelper = new SqlHelper(connectionStringBuilder);

        // Act
        HealthCheckResult healthCheckResult = await new DatabaseOnlineHealthCheck(config, sqlHelper).CheckHealthAsync(new HealthCheckContext(), new CancellationToken());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(healthCheckResult.Description, Is.EqualTo("No such host is known."));
        });
    }
}
