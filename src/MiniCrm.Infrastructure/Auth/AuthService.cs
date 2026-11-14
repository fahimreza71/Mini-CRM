using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;
using MiniCrm.Application.Auth;
using MiniCrm.Infrastructure.Identity;

namespace MiniCrm.Infrastructure.Auth;

public sealed class AuthService(UserManager<ApplicationUser> users, JwtTokenService tokens) : IAuthService
{
    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive) return null;
        if (await users.IsLockedOutAsync(user)) return null;

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);   // counts toward lockout
            return null;
        }

        await users.ResetAccessFailedCountAsync(user);

        var roles = await users.GetRolesAsync(user);
        var (token, expires) = tokens.CreateAccessToken(user, roles);
        return new TokenResponse(token, expires);
    }
}
