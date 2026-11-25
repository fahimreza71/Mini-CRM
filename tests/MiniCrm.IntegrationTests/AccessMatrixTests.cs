// tests/MiniCrm.IntegrationTests/AccessMatrixTests.cs
using System.Net.Http.Json;
using Xunit;

namespace MiniCrm.IntegrationTests;

[Collection(SharedApiFixture.Name)]
public sealed class AccessMatrixTests(ApiFactory factory)
{
    [Theory]
    [InlineData(null, "/api/v1/customers", 401)]
    [InlineData("rep1@minicrm.test", "/api/v1/customers", 200)]
    [InlineData("manager@minicrm.test", "/api/v1/customers", 200)]
    [InlineData("admin@minicrm.test", "/api/v1/customers", 200)]
    [InlineData(null, "/api/v1/users", 401)]
    [InlineData("rep1@minicrm.test", "/api/v1/users", 403)]
    [InlineData("manager@minicrm.test", "/api/v1/users", 403)]
    [InlineData("admin@minicrm.test", "/api/v1/users", 200)]
    [InlineData(null, "/health", 200)]
    [InlineData("rep1@minicrm.test", "/health", 200)]
    public async Task GetEndpointsFollowTheMatrix(string? who, string path, int expected)
    {
        using var client = await factory.CreateClientForAsync(who);

        var response = await client.GetAsync(path);

        Assert.Equal(expected, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(null, 401)]
    [InlineData("rep1@minicrm.test", 403)]
    public async Task OnlyManagersAndAdminsCanReassign(string? who, int expected)
    {
        var customerId = await factory.CustomerIdAsync("ada@acme.example");
        using var client = await factory.CreateClientForAsync(who);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/customers/{customerId}/owner",
            new { ownerId = Guid.NewGuid(), version = "AAAA" });   // authorization runs before the body is read

        Assert.Equal(expected, (int)response.StatusCode);
    }
}