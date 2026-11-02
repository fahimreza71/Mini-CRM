using System;
using System.Collections.Generic;
using System.Text;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Common;
using MiniCrm.Domain.Customers;

namespace MiniCrm.Application.Customers;

public sealed class CustomerService(IAppDbContext db, ICurrentUser currentUser)
{
    // One mapping used two ways: translated to SQL for reads, compiled for in-memory results
    private static readonly Expression<Func<Customer, CustomerDto>> ToDtoExpression = c =>
        new CustomerDto(c.Id, c.Name, c.Email, c.Company, c.Status, c.OwnerId, c.CreatedAt, c.RowVersion);

    private static readonly Func<Customer, CustomerDto> ToDto = ToDtoExpression.Compile();

    public async Task<PagedResult<CustomerDto>> ListAsync(ListCustomersQuery q, CancellationToken ct)
    {
        var query = db.Customers.AsNoTracking();

        if (q.Status is { } status)
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Email.Contains(term));
        }

        var total = await query.CountAsync(ct);

        IOrderedQueryable<Customer> ordered = (q.SortBy, q.Descending) switch
        {
            (CustomerSort.Email, false) => query.OrderBy(c => c.Email),
            (CustomerSort.Email, true) => query.OrderByDescending(c => c.Email),
            (CustomerSort.CreatedAt, false) => query.OrderBy(c => c.CreatedAt),
            (CustomerSort.CreatedAt, true) => query.OrderByDescending(c => c.CreatedAt),
            (_, true) => query.OrderByDescending(c => c.Name),
            _ => query.OrderBy(c => c.Name)
        };

        var items = await ordered
            .ThenBy(c => c.Id)                      // tie-breaker: stable pages
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(ToDtoExpression)
            .ToListAsync(ct);

        return new PagedResult<CustomerDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<CustomerDto> GetAsync(Guid id, CancellationToken ct) =>
        await db.Customers.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDtoExpression)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(Customer), id);

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        var ownerId = currentUser.UserId
            ?? throw new InvalidOperationException("No current user for an owned resource.");

        var customer = Customer.Create(request.Name, request.Email, request.Company, ownerId);

        if (await db.Customers.AnyAsync(c => c.Email == customer.Email, ct))
            throw new ConflictException("A customer with this email already exists.");

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        return ToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await FindAsync(id, ct);
        customer.UpdateDetails(request.Name, request.Email, request.Company);

        if (await db.Customers.AnyAsync(c => c.Id != id && c.Email == customer.Email, ct))
            throw new ConflictException("Another customer already uses this email.");

        db.SetExpectedVersion(customer, request.Version);
        await db.SaveChangesAsync(ct);
        return ToDto(customer);
    }

    public async Task<CustomerDto> ChangeStatusAsync(
        Guid id, ChangeCustomerStatusRequest request, CancellationToken ct)
    {
        var customer = await FindAsync(id, ct);
        customer.ChangeStatus(request.Status);
        db.SetExpectedVersion(customer, request.Version);
        await db.SaveChangesAsync(ct);
        return ToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var customer = await FindAsync(id, ct);
        var deals = await db.Deals.Where(d => d.CustomerId == id).ToListAsync(ct);
        var notes = await db.Notes.Where(n => n.CustomerId == id).ToListAsync(ct);

        // the interceptor turns each Remove into a soft delete; one SaveChanges = one transaction
        db.Deals.RemoveRange(deals);
        db.Notes.RemoveRange(notes);
        db.Customers.Remove(customer);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Customer> FindAsync(Guid id, CancellationToken ct) =>
        await db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw new NotFoundException(nameof(Customer), id);
}
