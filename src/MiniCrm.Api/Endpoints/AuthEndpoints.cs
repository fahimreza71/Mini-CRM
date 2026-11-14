using Microsoft.AspNetCore.Http.HttpResults;
using MiniCrm.Api.Filters;
using MiniCrm.Application.Auth;

namespace MiniCrm.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async Task<Results<Ok<TokenResponse>, ProblemHttpResult>> (
                LoginRequest request, IAuthService auth) =>
        {
            var token = await auth.LoginAsync(request);
            if (token is null)
                return TypedResults.Problem(
                    "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication failed.");

            return TypedResults.Ok(token);
        })
            .AllowAnonymous()
            .Validate<LoginRequest>();

        return api;
    }
}