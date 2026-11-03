using MiniCrm.Api.Filters;
using MiniCrm.Application.Deals;

namespace MiniCrm.Api.Endpoints;

public static class DealEndpoints
{
    private const string GetDealRoute = "GetDeal";

    public static IEndpointRouteBuilder MapDeals(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/deals").WithTags("Deals");

        group.MapGet("/", async ([AsParameters] ListDealsQuery query, DealService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.ListAsync(query, ct)))
            .Validate<ListDealsQuery>();

        group.MapGet("/{id:guid}", async (Guid id, DealService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.GetAsync(id, ct)))
            .WithName(GetDealRoute);

        group.MapPost("/", async (CreateDealRequest request, DealService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(request, ct);
            return TypedResults.CreatedAtRoute(created, GetDealRoute, new { id = created.Id });
        })
            .Validate<CreateDealRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateDealRequest request, DealService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.UpdateAsync(id, request, ct)))
            .Validate<UpdateDealRequest>();

        group.MapPut("/{id:guid}/stage", async (Guid id, ChangeDealStageRequest request, DealService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.ChangeStageAsync(id, request, ct)))
            .Validate<ChangeDealStageRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, DealService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        });

        return api;
    }
}
