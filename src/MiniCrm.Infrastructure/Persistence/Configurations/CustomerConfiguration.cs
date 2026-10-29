using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCrm.Domain.Customers;

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
    }
}
