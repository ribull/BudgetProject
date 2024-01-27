using Backend.HealthChecks;
using Backend.Implementations;
using Backend.Interfaces;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks()
    .AddCheck<DatabaseOnlineHealthCheck>("DatabaseOnline");

builder.Services.AddSingleton<IBudgetDatabaseContext, BudgetDatabaseContext>();
builder.Services.AddSingleton<ISqlConnectionStringBuilder, UsernamePasswordPostgresConnectionStringBuilder>();
builder.Services.AddSingleton<ISqlHelper, SqlHelper>();

var app = builder.Build();

app.MapGrpcService<BudgetGrpcService>();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
