using Backend.Implementations;
using Backend.Interfaces;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IMetadataContext, MetadataContext>();
builder.Services.AddSingleton<IPurchasesContext, PurchasesContext>();
builder.Services.AddSingleton<ISqlConnectionStringBuilder, StandardConnectionStringBuilder>();
builder.Services.AddSingleton<ISqlHelper, SqlHelper>();

var app = builder.Build();

app.MapGrpcService<MetadataGrpcService>();
app.MapGrpcService<PurchasesGrpcService>();

// Configure the HTTP request pipeline.

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
