using MiniCrm.Domain.Deals;

namespace MiniCrm.Application.Deals;

public sealed record DealDto(
    Guid Id, Guid CustomerId, string Title, decimal Amount,
    DealStage Stage, DateTimeOffset CreatedAt, byte[] Version);

public interface IDealDetails
{
    string Title { get; }
    decimal Amount { get; }
}

public sealed record CreateDealRequest(Guid CustomerId, string Title, decimal Amount) : IDealDetails;

public sealed record UpdateDealRequest(string Title, decimal Amount, byte[] Version) : IDealDetails;

public sealed record ChangeDealStageRequest(DealStage Stage, byte[] Version);

public enum DealSort { CreatedAt, Amount, Title }

public sealed record ListDealsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CustomerId = null,
    DealStage? Stage = null,
    DealSort SortBy = DealSort.CreatedAt,
    bool Descending = true);
