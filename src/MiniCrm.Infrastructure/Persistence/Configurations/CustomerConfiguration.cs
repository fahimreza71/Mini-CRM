using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Customers;
using MiniCrm.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : AuditableConfiguration<Customer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers");
        b.Property(x => x.Name).HasMaxLength(Customer.NameMaxLength);
        b.Property(x => x.Email).HasMaxLength(Customer.EmailMaxLength);
        b.Property(x => x.Company).HasMaxLength(Customer.CompanyMaxLength);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        // unique only among live rows, so a deleted customer's email can be reused
        b.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
        // serves "my customers, filtered by status"
        b.HasIndex(x => new { x.OwnerId, x.Status });
        // The new relationship goes here, in the same block
        b.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
