using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Common;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;
using MiniCrm.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Application.Common;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Note> Notes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void SetExpectedVersion<T>(T entity, byte[] version) where T : AuditableEntity;
}
