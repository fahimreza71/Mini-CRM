using System;
using System.Collections.Generic;
using System.Text;

using MiniCrm.Domain.Common;

namespace MiniCrm.Domain.Customers;

public sealed class Customer : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int EmailMaxLength = 320;
    public const int CompanyMaxLength = 200;

    private Customer() { }   // for EF Core

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Company { get; private set; }
    public CustomerStatus Status { get; private set; }
    public Guid OwnerId { get; private set; }

    public static Customer Create(string name, string email, string? company, Guid ownerId)
    {
        var customer = new Customer { Status = CustomerStatus.Prospect };
        customer.UpdateDetails(name, email, company);
        customer.ReassignTo(ownerId);
        return customer;
    }

    public void UpdateDetails(string name, string email, string? company)
    {
        var cleanEmail = Guard.RequiredText(email, nameof(Email), EmailMaxLength).ToLowerInvariant();
        if (!cleanEmail.Contains('@', StringComparison.Ordinal))
            throw new DomainException("Email is not valid.");

        Name = Guard.RequiredText(name, nameof(Name), NameMaxLength);
        Email = cleanEmail;
        Company = string.IsNullOrWhiteSpace(company)
            ? null
            : Guard.RequiredText(company, nameof(Company), CompanyMaxLength);
    }

    public void ChangeStatus(CustomerStatus status)
    {
        if (!Enum.IsDefined(status)) throw new DomainException("Unknown customer status.");
        Status = status;
    }

    public void ReassignTo(Guid ownerId)
    {
        if (ownerId == Guid.Empty) throw new DomainException("Owner is required.");
        OwnerId = ownerId;
    }
}