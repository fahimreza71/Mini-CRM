// tests/MiniCrm.IntegrationTests/ApiFactory.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniCrm.Application.Auth;
using MiniCrm.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Xunit;

namespace MiniCrm.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Password = "Test-Passw0rd!2026";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }   // the API sends enums as strings
    };

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        // Program.cs reads these while the host is built, so set them before the first Services/CreateClient call.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _sql.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-tests-only-signing-key-0123456789-abcdefghijklmnop");

        using var scope = Services.CreateScope();   // this builds the host
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DevDataSeeder.SeedAsync(scope.ServiceProvider, Password);   // 4 users, 3 customers
    }

    // "Testing" is not "Development": no seeder, no Scalar, no user-secrets. It is closer to production.
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sql.DisposeAsync();
    }

    public async Task<HttpClient> CreateClientForAsync(string? email)
    {
        var client = CreateClient();
        if (email is null) return client;   // anonymous

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, Password));
        response.EnsureSuccessStatusCode();
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    public async Task<Guid> CustomerIdAsync(string email)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Customers.Where(c => c.Email == email).Select(c => c.Id).SingleAsync();
    }

    public async Task<Guid> UserIdAsync(string email)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync();
    }

    public async Task DisableUserAsync(string email)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false));
    }
}