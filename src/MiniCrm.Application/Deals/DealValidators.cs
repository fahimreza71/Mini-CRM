using FluentValidation;
using MiniCrm.Domain.Deals;

namespace MiniCrm.Application.Deals;

public abstract class DealDetailsValidator<T> : AbstractValidator<T> where T : IDealDetails
{
    protected DealDetailsValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Deal.TitleMaxLength);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, false);
    }
}

public sealed class CreateDealValidator : DealDetailsValidator<CreateDealRequest>
{
    public CreateDealValidator() => RuleFor(x => x.CustomerId).NotEmpty();
}

public sealed class UpdateDealValidator : DealDetailsValidator<UpdateDealRequest>
{
    public UpdateDealValidator() => RuleFor(x => x.Version).NotEmpty();
}

public sealed class ChangeDealStageValidator : AbstractValidator<ChangeDealStageRequest>
{
    public ChangeDealStageValidator()
    {
        RuleFor(x => x.Stage).IsInEnum();
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class ListDealsQueryValidator : AbstractValidator<ListDealsQuery>
{
    public ListDealsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Stage).IsInEnum();
        RuleFor(x => x.SortBy).IsInEnum();
    }
}
