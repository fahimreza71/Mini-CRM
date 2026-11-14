using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using MiniCrm.Application.Common;

namespace MiniCrm.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}