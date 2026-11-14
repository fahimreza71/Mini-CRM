using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Application.Auth;

namespace MiniCrm.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> b) =>
        b.HasData(
            Role("a1a1a1a1-0000-0000-0000-000000000001", Roles.Admin, "68af2c71-ad77-4226-8387-3a5fb94048d8"),
            Role("a1a1a1a1-0000-0000-0000-000000000002", Roles.Manager, "24560d0a-3c32-471e-937a-a91ce0984c71"),
            Role("a1a1a1a1-0000-0000-0000-000000000003", Roles.SalesRep, "0161c19a-3bb8-4ff3-b624-a2fefe343073"));

    // IdentityRole's constructor assigns ConcurrencyStamp = Guid.NewGuid(), which makes seed data
    // non-deterministic (PendingModelChangesWarning). Pin it to a fixed value.
    private static IdentityRole<Guid> Role(string id, string name, string concurrencyStamp) =>
        new()
        {
            Id = Guid.Parse(id),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = concurrencyStamp,
        };
}