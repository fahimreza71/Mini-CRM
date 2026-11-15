using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Application.Auth;

public sealed record LoginRequest(string Email, string Password)
{
    public override string ToString() => $"LoginRequest {{ Email = {Email} }}";   // never print the password
}

public sealed record CreateUserRequest(string Email, string DisplayName, string Password, string Role)
{
    public override string ToString() => $"CreateUserRequest {{ Email = {Email}, Role = {Role} }}";
}

public sealed record UserDto(Guid Id, string Email, string DisplayName, string Role, bool IsActive);

public sealed record TokenResponse(
    string AccessToken, DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken, DateTimeOffset RefreshTokenExpiresAt)
{
    public override string ToString() => "TokenResponse { tokens hidden }";
}

public sealed record RefreshRequest(string RefreshToken)
{
    public override string ToString() => "RefreshRequest { token hidden }";
}

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<TokenResponse?> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(RefreshRequest request, CancellationToken ct);
}

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct);
    Task<UserDto> GetAsync(Guid id, CancellationToken ct);
    Task<UserDto> CreateAsync(CreateUserRequest request);
}