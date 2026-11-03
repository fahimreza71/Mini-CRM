using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Application.Common;
using MiniCrm.Domain.Common;
using MiniCrm.Domain.Deals;

namespace MiniCrm.Application.Deals;

public sealed class DealService(IAppDbContext db)
{
    private static readonly Expression<Func<Deal, DealDto>> ToDtoExpression = d =>
        new DealDto(d.Id, d.CustomerId, d.Title, d.Amount, d.Stage, d.CreatedAt, d.RowVersion);

    private static readonly Func<Deal, DealDto> ToDto = ToDtoExpression.Compile();

    public async Task<PagedResult<DealDto>> ListAsync(ListDealsQuery q, CancellationToken ct)
    {
        var query = db.Deals.AsNoTracking();

        if (q.CustomerId is { } customerId)
            query = query.Where(d => d.CustomerId == customerId);

        if (q.Stage is { } stage)
            query = query.Where(d => d.Stage == stage);

        var total = await query.CountAsync(ct);

        IOrderedQueryable<Deal> ordered = (q.SortBy, q.Descending) switch
        {
            (DealSort.Amount, false) => query.OrderBy(d => d.Amount),
            (DealSort.Amount, true) => query.OrderByDescending(d => d.Amount),
            (DealSort.Title, false) => query.OrderBy(d => d.Title),
            (DealSort.Title, true) => query.OrderByDescending(d => d.Title),
            (_, false) => query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        var items = await ordered
            .ThenBy(d => d.Id)                      // tie-breaker: stable pages
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(ToDtoExpression)
            .ToListAsync(ct);

        return new PagedResult<DealDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<DealDto> GetAsync(Guid id, CancellationToken ct) =>
        await db.Deals.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(ToDtoExpression)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(Deal), id);

    public async Task<DealDto> CreateAsync(CreateDealRequest request, CancellationToken ct)
    {
        var deal = Deal.Create(request.CustomerId, request.Title, request.Amount);

        // Phase 3: a SalesRep must not learn that someone else's customer exists
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId, ct))
            throw new DomainException("Customer does not exist.");

        db.Deals.Add(deal);
        await db.SaveChangesAsync(ct);
        return ToDto(deal);
    }

    public async Task<DealDto> UpdateAsync(Guid id, UpdateDealRequest request, CancellationToken ct)
    {
        var deal = await FindAsync(id, ct);
        deal.UpdateDetails(request.Title, request.Amount);
        db.SetExpectedVersion(deal, request.Version);
        await db.SaveChangesAsync(ct);
        return ToDto(deal);
    }

    public async Task<DealDto> ChangeStageAsync(Guid id, ChangeDealStageRequest request, CancellationToken ct)
    {
        var deal = await FindAsync(id, ct);
        deal.MoveTo(request.Stage);
        db.SetExpectedVersion(deal, request.Version);
        await db.SaveChangesAsync(ct);
        return ToDto(deal);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var deal = await FindAsync(id, ct);
        var notes = await db.Notes.Where(n => n.DealId == id).ToListAsync(ct);

        // the interceptor turns each Remove into a soft delete; one SaveChanges = one transaction
        db.Notes.RemoveRange(notes);
        db.Deals.Remove(deal);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Deal> FindAsync(Guid id, CancellationToken ct) =>
        await db.Deals.FirstOrDefaultAsync(d => d.Id == id, ct)
        ?? throw new NotFoundException(nameof(Deal), id);
}
