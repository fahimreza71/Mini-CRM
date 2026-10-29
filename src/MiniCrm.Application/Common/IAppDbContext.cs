using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Customers;
using MiniCrm.Domain.Deals;
using MiniCrm.Domain.Notes;

namespace MiniCrm.Application.Common;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Note> Notes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
