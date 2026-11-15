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
                LoginRequest request, IAuthService auth, CancellationToken ct) =>
        {
            var tokens = await auth.LoginAsync(request, ct);
            if (tokens is null) return Unauthorized("Invalid email or password.");
            return TypedResults.Ok(tokens);
        })
            .AllowAnonymous()
            .Validate<LoginRequest>();

        // Anonymous on purpose: the access token has usually expired by the time you refresh.
        // The refresh token itself is the credential.
        group.MapPost("/refresh", async Task<Results<Ok<TokenResponse>, ProblemHttpResult>> (
                RefreshRequest request, IAuthService auth, CancellationToken ct) =>
        {
            var tokens = await auth.RefreshAsync(request, ct);
            if (tokens is null) return Unauthorized("Invalid or expired refresh token.");
            return TypedResults.Ok(tokens);
        })
            .AllowAnonymous()
            .Validate<RefreshRequest>();

        group.MapPost("/logout", async (RefreshRequest request, IAuthService auth, CancellationToken ct) =>
        {
            await auth.LogoutAsync(request, ct);
            return TypedResults.NoContent();
        })
            .AllowAnonymous()
            .Validate<RefreshRequest>();

        return api;
    }

    private static ProblemHttpResult Unauthorized(string detail) =>
        TypedResults.Problem(detail, statusCode: StatusCodes.Status401Unauthorized, title: "Authentication failed.");
}