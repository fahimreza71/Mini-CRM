using MiniCrm.Api.Auth;
using MiniCrm.Api.Filters;
using MiniCrm.Application.Auth;

namespace MiniCrm.Api.Endpoints;

public static class UserEndpoints
{
    private const string GetUserRoute = "GetUser";

    public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users")
            .RequireAuthorization(Policies.ManageUsers);   // Admin only, for the whole group

        group.MapGet("/", async (IUserService svc, CancellationToken ct) =>
            TypedResults.Ok(await svc.ListAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IUserService svc, CancellationToken ct) =>
                TypedResults.Ok(await svc.GetAsync(id, ct)))
            .WithName(GetUserRoute);

        group.MapPost("/", async (CreateUserRequest request, IUserService svc) =>
        {
            var created = await svc.CreateAsync(request);
            return TypedResults.CreatedAtRoute(created, GetUserRoute, new { id = created.Id });
        })
            .Validate<CreateUserRequest>();

        return api;
    }
}