using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;

namespace MiniCrm.Infrastructure.Persistence.Configurations;

public sealed class DealConfiguration : AuditableConfiguration<Deal>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Deal> b)
    {
        b.ToTable("Deals", t => t.HasCheckConstraint("CK_Deals_Amount_NonNegative", "[Amount] >= 0"));
        b.Property(x => x.Title).HasMaxLength(Deal.TitleMaxLength);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Customer).WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);   // we soft delete; never cascade-delete real rows

        b.HasIndex(x => new { x.CustomerId, x.Stage });
    }
}
