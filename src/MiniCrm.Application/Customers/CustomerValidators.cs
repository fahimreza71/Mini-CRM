using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;
using MiniCrm.Domain.Customers;

namespace MiniCrm.Application.Customers;

public abstract class CustomerDetailsValidator<T> : AbstractValidator<T> where T : ICustomerDetails
{
    protected CustomerDetailsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Customer.NameMaxLength);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(Customer.EmailMaxLength).EmailAddress();
        RuleFor(x => x.Company).MaximumLength(Customer.CompanyMaxLength);
    }
}

public sealed class CreateCustomerValidator : CustomerDetailsValidator<CreateCustomerRequest>;

public sealed class UpdateCustomerValidator : CustomerDetailsValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator() => RuleFor(x => x.Version).NotEmpty();
}

public sealed class ChangeCustomerStatusValidator : AbstractValidator<ChangeCustomerStatusRequest>
{
    public ChangeCustomerStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public ListCustomersQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.SortBy).IsInEnum();
    }
}
