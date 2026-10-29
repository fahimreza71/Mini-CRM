using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;
using MiniCrm.Domain.Notes;

namespace MiniCrm.Infrastructure.Persistence.Configurations;

public sealed class NoteConfiguration : AuditableConfiguration<Note>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Note> b)
    {
        b.ToTable("Notes");
        b.Property(x => x.Text).HasMaxLength(Note.TextMaxLength);

        b.HasOne<Customer>().WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Deal>().WithMany()
            .HasForeignKey(x => x.DealId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.CustomerId, x.CreatedAt });
        b.HasIndex(x => x.DealId);
    }
}
