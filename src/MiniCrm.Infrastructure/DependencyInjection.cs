using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniCrm.Application.Auth;
using MiniCrm.Application.Common;
using MiniCrm.Infrastructure.Auth;
using MiniCrm.Infrastructure.Identity;
using MiniCrm.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentityCore<ApplicationUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredLength = 12;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
