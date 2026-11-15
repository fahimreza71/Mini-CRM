// src/MiniCrm.Infrastructure/Auth/UserDirectory.cs
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Auth;
using MiniCrm.Application.Common;
using MiniCrm.Infrastructure.Persistence;

namespace MiniCrm.Infrastructure.Auth;

public sealed class UserDirectory(AppDbContext db) : IUserDirectory
{
    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive, ct);
}