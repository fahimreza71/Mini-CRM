// src/MiniCrm.Application/Customers/CustomerAccess.cs
using MiniCrm.Application.Auth;
using MiniCrm.Application.Common;
using MiniCrm.Domain.Customers;

namespace MiniCrm.Application.Customers;

public sealed class CustomerAccess(IAppDbContext db, ICurrentUser currentUser)
{
    public bool SeesAll => currentUser.IsInRole(Roles.Manager) || currentUser.IsInRole(Roles.Admin);

    public IQueryable<Customer> VisibleCustomers()
    {
        if (SeesAll) return db.Customers;
        if (currentUser.UserId is not { } me) return db.Customers.Where(_ => false);   // fail closed
        return db.Customers.Where(c => c.OwnerId == me);
    }
}