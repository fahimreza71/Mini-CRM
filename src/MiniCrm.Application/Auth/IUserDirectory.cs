// src/MiniCrm.Application/Auth/IUserDirectory.cs
namespace MiniCrm.Application.Auth;

public interface IUserDirectory
{
    Task<bool> IsActiveAsync(Guid userId, CancellationToken ct);
}