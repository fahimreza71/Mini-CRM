using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Auth;
using MiniCrm.Application.Common;
using MiniCrm.Domain.Common;
using MiniCrm.Infrastructure.Identity;
using MiniCrm.Infrastructure.Persistence;

namespace MiniCrm.Infrastructure.Auth;

public sealed class UserService(AppDbContext db, UserManager<ApplicationUser> users) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct) =>
        await (from u in db.Users.AsNoTracking()
               join ur in db.UserRoles on u.Id equals ur.UserId
               join r in db.Roles on ur.RoleId equals r.Id
               orderby u.Email
               select new UserDto(u.Id, u.Email!, u.DisplayName, r.Name!, u.IsActive))
            .Take(200)   // bounded; add paging if the user list ever grows
            .ToListAsync(ct);

    public async Task<UserDto> GetAsync(Guid id, CancellationToken ct) =>
        await (from u in db.Users.AsNoTracking()
               join ur in db.UserRoles on u.Id equals ur.UserId
               join r in db.Roles on ur.RoleId equals r.Id
               where u.Id == id
               select new UserDto(u.Id, u.Email!, u.DisplayName, r.Name!, u.IsActive))
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("User", id);

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email, DisplayName = request.DisplayName.Trim() };

        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            if (created.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
                throw new ConflictException("A user with this email already exists.");

            // password-policy messages are safe to show: they describe the rules, not the data
            throw new DomainException(string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        var roleResult = await users.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);   // don't leave a user without a role
            throw new DomainException("The role could not be assigned.");
        }

        return new UserDto(user.Id, email, user.DisplayName, request.Role, user.IsActive);
    }
}
