using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniCrm.Application.Auth;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;
using MiniCrm.Domain.Notes;
using MiniCrm.Infrastructure.Identity;

namespace MiniCrm.Infrastructure.Persistence;

// Development only: fake users and data so you have something to test against
public static class DevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, string password, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Customers.IgnoreQueryFilters().AnyAsync(ct)) return;   // idempotent

        await EnsureUserAsync(users, "admin@minicrm.test", "Ada Admin", Roles.Admin, password);
        await EnsureUserAsync(users, "manager@minicrm.test", "Max Manager", Roles.Manager, password);
        var rep1 = await EnsureUserAsync(users, "rep1@minicrm.test", "Rita Rep", Roles.SalesRep, password);
        var rep2 = await EnsureUserAsync(users, "rep2@minicrm.test", "Sam Rep", Roles.SalesRep, password);

        var ada = Customer.Create("Ada Lovelace", "ada@acme.example", "Acme Ltd", rep1.Id);
        var grace = Customer.Create("Grace Hopper", "grace@globex.example", "Globex", rep1.Id);
        var alan = Customer.Create("Alan Turing", "alan@initech.example", "Initech", rep2.Id);
        db.Customers.AddRange(ada, grace, alan);

        var deal = Deal.Create(ada.Id, "Annual license", 12_000m);
        deal.MoveTo(DealStage.Qualified);
        db.Deals.Add(deal);
        db.Notes.Add(Note.Create(ada.Id, deal.Id, "Intro call went well."));

        await db.SaveChangesAsync(ct);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> users, string email, string name, string role, string password)
    {
        var existing = await users.FindByEmailAsync(email);
        if (existing is not null) return existing;

        var user = new ApplicationUser { UserName = email, Email = email, DisplayName = name };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                "Seeding user failed: " + string.Join("; ", created.Errors.Select(e => e.Description)));

        var inRole = await users.AddToRoleAsync(user, role);
        if (!inRole.Succeeded)
            throw new InvalidOperationException(
                "Seeding role failed: " + string.Join("; ", inRole.Errors.Select(e => e.Description)));

        return user;
    }
}