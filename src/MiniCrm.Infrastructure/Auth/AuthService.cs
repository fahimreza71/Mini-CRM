using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniCrm.Application.Auth;
using MiniCrm.Infrastructure.Identity;
using MiniCrm.Infrastructure.Persistence;

namespace MiniCrm.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> users,
    JwtTokenService tokens,
    AppDbContext db,
    TimeProvider clock,
    IOptions<JwtOptions> options) : IAuthService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
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
        return await IssueAsync(user, Guid.NewGuid(), ct);   // a new login starts a new family
    }

    public async Task<TokenResponse?> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var hash = Hash(request.RefreshToken);

        var stored = await db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is null) return null;

        // 1. Already used or revoked: either a double-send or a stolen copy. Cancel the family.
        if (stored.UsedAt is not null || stored.RevokedAt is not null)
        {
            await RevokeFamilyAsync(stored.FamilyId, now, ct);
            return null;
        }

        if (stored.ExpiresAt <= now) return null;

        // 2. The account must still be allowed to sign in.
        var user = await users.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive || await users.IsLockedOutAsync(user))
        {
            await RevokeFamilyAsync(stored.FamilyId, now, ct);
            return null;
        }

        // 3. Claim the token. Only one concurrent caller can flip UsedAt from NULL.
        var claimed = await db.RefreshTokens
            .Where(t => t.Id == stored.Id && t.UsedAt == null && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, (DateTimeOffset?)now), ct);

        if (claimed != 1)   // we lost a race against another request using the same token
        {
            await RevokeFamilyAsync(stored.FamilyId, now, ct);
            return null;
        }

        return await IssueAsync(user, stored.FamilyId, ct);   // same family, new token
    }

    public async Task LogoutAsync(RefreshRequest request, CancellationToken ct)
    {
        var hash = Hash(request.RefreshToken);
        var familyId = await db.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.FamilyId)
            .FirstOrDefaultAsync(ct);

        if (familyId is { } id)
            await RevokeFamilyAsync(id, clock.GetUtcNow(), ct);
    }

    private async Task<TokenResponse> IssueAsync(ApplicationUser user, Guid familyId, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var roles = await users.GetRolesAsync(user);   // roles are read fresh on every issue
        var (accessToken, accessExpires) = tokens.CreateAccessToken(user, roles);

        var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
        var refreshExpires = now.AddDays(_jwt.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = Hash(refreshToken),   // only the hash is stored
            CreatedAt = now,
            ExpiresAt = refreshExpires
        });
        await db.SaveChangesAsync(ct);

        return new TokenResponse(accessToken, accessExpires, refreshToken, refreshExpires);
    }

    private Task<int> RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken ct) =>
        db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTimeOffset?)now), ct);

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}