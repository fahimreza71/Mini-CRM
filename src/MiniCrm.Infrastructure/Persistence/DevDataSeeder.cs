using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;
using MiniCrm.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Infrastructure.Persistence;

// Development only: fake data so you have something to query
public static class DevDataSeeder
{
    public static readonly Guid DemoOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Customers.IgnoreQueryFilters().AnyAsync(ct)) return;   // idempotent

        var ada = Customer.Create("Ada Lovelace", "ada@acme.example", "Acme Ltd", DemoOwnerId);
        var grace = Customer.Create("Grace Hopper", "grace@globex.example", "Globex", DemoOwnerId);
        db.Customers.AddRange(ada, grace);   // Ids are assigned when entities are added

        var deal = Deal.Create(ada.Id, "Annual license", 12_000m);
        deal.MoveTo(DealStage.Qualified);
        db.Deals.Add(deal);
        db.Notes.Add(Note.Create(ada.Id, deal.Id, "Intro call went well."));

        await db.SaveChangesAsync(ct);
    }
}
