using Backend.Controllers;
using Backend.HealthChecks;
using Backend.Implementations;
using Backend.Interfaces;
using Backend.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks()
    .AddCheck<DatabaseOnlineHealthCheck>("DatabaseOnline");

builder.Services.AddControllers();

builder.Services.AddSingleton<IBudgetDatabaseContext, BudgetDatabaseContext>();
builder.Services.AddSingleton<ISqlConnectionStringBuilder, UsernamePasswordPostgresConnectionStringBuilder>();
builder.Services.AddSingleton<ISqlHelper, SqlHelper>();

builder.WebHost.UseKestrel((options) =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.MapGrpcService<BudgetGrpcService>();
app.MapGrpcHealthChecksService();

app.UseRouting();
app.MapControllers();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

foreach (var c in builder.Configuration.AsEnumerable())
{
    Console.WriteLine(c.Key + " = " + c.Value);
}

app.Run();
