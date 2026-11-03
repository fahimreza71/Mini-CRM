using System.Text.Json.Serialization;
using Asp.Versioning;
using MiniCrm.Api.Endpoints;
using MiniCrm.Api.Errors;
using MiniCrm.Application;
using MiniCrm.Infrastructure;
using MiniCrm.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default is not configured. Set it with dotnet user-secrets.");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));   // "Prospect", not 1

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    await DevDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

var v1 = app.NewVersionedApi("MiniCrm")
    .MapGroup("/api/v{version:apiVersion}")
    .HasApiVersion(1.0);

v1.MapCustomers();
v1.MapDeals();

app.Run();

public partial class Program { }
