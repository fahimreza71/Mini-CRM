using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Application.Auth;

namespace MiniCrm.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> b) =>
        b.HasData(
            Role("a1a1a1a1-0000-0000-0000-000000000001", Roles.Admin),
            Role("a1a1a1a1-0000-0000-0000-000000000002", Roles.Manager),
            Role("a1a1a1a1-0000-0000-0000-000000000003", Roles.SalesRep));

    private static IdentityRole<Guid> Role(string id, string name) =>
        new() { Id = Guid.Parse(id), Name = name, NormalizedName = name.ToUpperInvariant() };
}