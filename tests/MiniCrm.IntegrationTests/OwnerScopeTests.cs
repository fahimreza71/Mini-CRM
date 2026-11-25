// tests/MiniCrm.IntegrationTests/OwnerScopeTests.cs
using System.Net;
using System.Net.Http.Json;
using MiniCrm.Application.Common;
using MiniCrm.Application.Customers;
using Xunit;

namespace MiniCrm.IntegrationTests;

[Collection(SharedApiFixture.Name)]
public sealed class OwnerScopeTests(ApiFactory factory)
{
    private const string Rep1 = "rep1@minicrm.test";
    private const string Rep2 = "rep2@minicrm.test";
    private const string Manager = "manager@minicrm.test";
    private const string AlanEmail = "alan@initech.example";   // seeded: owned by rep2

    [Fact]
    public async Task RepListsOnlyOwnCustomers()
    {
        var rep1Id = await factory.UserIdAsync(Rep1);
        using var client = await factory.CreateClientForAsync(Rep1);

        var page = (await client.GetFromJsonAsync<PagedResult<CustomerDto>>(
            "/api/v1/customers?pageSize=100", ApiFactory.Json))!;

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, c => Assert.Equal(rep1Id, c.OwnerId));
    }

    [Fact]
    public async Task RepCannotSeeAnotherRepsCustomer()
    {
        var alanId = await factory.CustomerIdAsync(AlanEmail);
        using var rep1 = await factory.CreateClientForAsync(Rep1);

        var response = await rep1.GetAsync($"/api/v1/customers/{alanId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);   // 404, not 403: don't confirm it exists
    }

    [Fact]
    public async Task RepCannotChangeAnotherRepsCustomer()
    {
        var alanId = await factory.CustomerIdAsync(AlanEmail);
        using var rep1 = await factory.CreateClientForAsync(Rep1);

        var update = await rep1.PutAsJsonAsync($"/api/v1/customers/{alanId}", new
        {
            name = "Hacked",
            email = "hacked@evil.example",
            company = (string?)null,
            version = Convert.ToBase64String(new byte[8])
        });
        var delete = await rep1.DeleteAsync($"/api/v1/customers/{alanId}");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        // and nothing actually changed
        using var manager = await factory.CreateClientForAsync(Manager);
        var alan = await manager.GetFromJsonAsync<CustomerDto>($"/api/v1/customers/{alanId}", ApiFactory.Json);
        Assert.Equal("Alan Turing", alan!.Name);
    }

    [Fact]
    public async Task ManagerSeesCustomersOfEveryOwner()
    {
        using var manager = await factory.CreateClientForAsync(Manager);

        var page = (await manager.GetFromJsonAsync<PagedResult<CustomerDto>>(
            "/api/v1/customers?pageSize=100", ApiFactory.Json))!;

        Assert.True(page.Items.Select(c => c.OwnerId).Distinct().Count() >= 2);
    }

    [Fact]
    public async Task ManagerCanReassignACustomer()
    {
        // arrange: rep2 creates a brand-new customer (so this test never disturbs shared data)
        using var rep2 = await factory.CreateClientForAsync(Rep2);
        var created = await rep2.PostAsJsonAsync("/api/v1/customers", new
        {
            name = "Reassign Me",
            email = $"{Guid.NewGuid():N}@reassign.example",
            company = (string?)null
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var customer = (await created.Content.ReadFromJsonAsync<CustomerDto>(ApiFactory.Json))!;

        // act: the manager hands it to rep1
        var rep1Id = await factory.UserIdAsync(Rep1);
        using var manager = await factory.CreateClientForAsync(Manager);
        var reassigned = await manager.PutAsJsonAsync($"/api/v1/customers/{customer.Id}/owner",
            new { ownerId = rep1Id, version = customer.Version });

        // assert: rep1 now sees it, rep2 no longer does
        Assert.Equal(HttpStatusCode.OK, reassigned.StatusCode);
        using var rep1 = await factory.CreateClientForAsync(Rep1);
        Assert.Equal(HttpStatusCode.OK, (await rep1.GetAsync($"/api/v1/customers/{customer.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await rep2.GetAsync($"/api/v1/customers/{customer.Id}")).StatusCode);
    }
}