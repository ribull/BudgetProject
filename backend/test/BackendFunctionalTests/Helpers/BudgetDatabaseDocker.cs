using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using Backend.Implementations;
using Backend.Interfaces;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;

namespace BackendFunctionalTests.Helpers;

public class BudgetDatabaseDocker : IAsyncDisposable
{
    public const string SA_PASSWORD = "Password1";
    public const int BASE_CONTAINER_PORT = 10000;
    public const string CONTAINER_IMAGE = "mcr.microsoft.com/mssql/server:2019-latest";

    private readonly string _containerBaseName;
    private string? _containerId = null;
    private string? _containerName = null;
    private int? _containerPort = null;

    private ISqlHelper? _sqlHelper = null;
    private DockerClient _dockerClient;

    public ISqlConnectionStringBuilder? ConnectionString { get; private set; }

    public string DatabaseName { get; private set; }

    public BudgetDatabaseDocker(string baseName, string databaseName)
    {
        string dockerClientUri = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
            dockerClientUri = "unix:///var/run/docker.sock";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dockerClientUri = "npipe://./pipe/docker_engine";
        }

        _dockerClient = new DockerClientConfiguration(new Uri(dockerClientUri))
            .CreateClient();

        _containerBaseName = baseName;

        DatabaseName = databaseName;
    }

    public async Task StartContainer()
    {
        await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = CONTAINER_IMAGE }, new AuthConfig(), new Progress<JSONMessage>());

        IList<ContainerListResponse> containerListResponses = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

        int servernum = 1;
        foreach (ContainerListResponse containerListResponse in containerListResponses)
        {
            foreach (string containerName in containerListResponse.Names)
            {
                TestContext.Progress.WriteLine(containerName);
                if (containerName.StartsWith(_containerBaseName))
                {
                    servernum++;
                }
            }
        }

        _containerName = $"{_containerBaseName}{servernum}";
        _containerPort = BASE_CONTAINER_PORT + servernum;

        CreateContainerResponse response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = CONTAINER_IMAGE,
            Env = new List<string>() { "ACCEPT_EULA=Y", $"MSSQL_SA_PASSWORD={SA_PASSWORD}" },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>()
                {
                    { "1433/tcp", new List<PortBinding> { new() { HostPort = $"{_containerPort}" } } }
                }
            },
            Name = _containerName,
            Hostname = _containerName
        });

        ConnectionString = new StandardConnectionStringBuilder("sa", SA_PASSWORD, $"localhost,{_containerPort}");
        _sqlHelper = new SqlHelper(ConnectionString);
        _containerId = response.ID;

        if (!await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters()))
        {
            throw new Exception("An error occured while trying to start the container.");
        }

        for (int i = 0; i < 180; i++)
        {
            if (await IsSqlContainerOnline())
            {
                return;
            }

            Thread.Sleep(1000);
        }

        throw new Exception("The SQL Container failed to start!");
    }

    public void DeployBudgetDb()
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                using (DacPackage dacPackage = DacPackage.Load(Path.Join(Directory.GetCurrentDirectory(), "BudgetDatabase.dacpac")))
                {
                    DacServices dacServices = new(ConnectionString!.GetConnectionString("master"));
                    dacServices.Deploy(dacPackage, DatabaseName);
                }

                return;
            }
            catch (DacServicesException) { }

            Thread.Sleep(1000);
        }
    }

    public async Task DropDb()
    {
        await _sqlHelper!.ExecuteAsync(
$@"ALTER DATABASE {DatabaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE {DatabaseName}");
    }

    public async Task ResetDb()
    {
        await DropDb();
        DeployBudgetDb();
    }

    public async ValueTask DisposeAsync()
    {
        await _dockerClient.Containers.RemoveContainerAsync(_containerName, new ContainerRemoveParameters { Force = true });
        _dockerClient.Dispose();
    }

    private async Task<bool> IsSqlContainerOnline()
    {
        try
        {
            return await _sqlHelper!.Exists("master", "SELECT 1 FROM sys.databases WHERE database_id = DB_ID('tempdb') AND [state] = 0");
        }
        catch (Exception)
        {
            return false;
        }
    }
}
