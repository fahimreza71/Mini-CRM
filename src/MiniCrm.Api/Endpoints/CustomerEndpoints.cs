using MiniCrm.Api.Filters;
using MiniCrm.Application.Customers;

namespace MiniCrm.Api.Endpoints;

public static class CustomerEndpoints
{
    private const string GetCustomerRoute = "GetCustomer";

    public static IEndpointRouteBuilder MapCustomers(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/customers").WithTags("Customers");

        group.MapGet("/", async ([AsParameters] ListCustomersQuery query, CustomerService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.ListAsync(query, ct)))
            .Validate<ListCustomersQuery>();

        group.MapGet("/{id:guid}", async (Guid id, CustomerService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.GetAsync(id, ct)))
            .WithName(GetCustomerRoute);

        group.MapPost("/", async (CreateCustomerRequest request, CustomerService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(request, ct);
            return TypedResults.CreatedAtRoute(created, GetCustomerRoute, new { id = created.Id });
        })
            .Validate<CreateCustomerRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, CustomerService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.UpdateAsync(id, request, ct)))
            .Validate<UpdateCustomerRequest>();

        group.MapPut("/{id:guid}/status", async (Guid id, ChangeCustomerStatusRequest request, CustomerService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.ChangeStatusAsync(id, request, ct)))
            .Validate<ChangeCustomerStatusRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, CustomerService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });

        return api;
    }
}
