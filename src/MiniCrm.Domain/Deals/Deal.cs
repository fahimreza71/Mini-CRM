using System;
using System.Collections.Generic;
using System.Text;

using MiniCrm.Domain.Common;
using MiniCrm.Domain.Customers;

namespace MiniCrm.Domain.Deals;

public sealed class Deal : AuditableEntity
{
    public const int TitleMaxLength = 200;

    private Deal() { }

    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;   // standard EF navigation idiom
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DealStage Stage { get; private set; }

    public static Deal Create(Guid customerId, string title, decimal amount)
    {
        if (customerId == Guid.Empty) throw new DomainException("Customer is required.");

        var deal = new Deal { CustomerId = customerId, Stage = DealStage.Lead };
        deal.UpdateDetails(title, amount);
        return deal;
    }

    public void UpdateDetails(string title, decimal amount)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        Title = Guard.RequiredText(title, nameof(Title), TitleMaxLength);
        Amount = amount;
    }

    public void MoveTo(DealStage next)
    {
        if (!Enum.IsDefined(next)) throw new DomainException("Unknown deal stage.");
        if (Stage is DealStage.Won or DealStage.Lost)
            throw new DomainException($"A {Stage} deal can no longer change stage.");
        if (next != DealStage.Lost && next <= Stage)
            throw new DomainException($"Cannot move a deal from {Stage} back to {next}.");

        Stage = next;
    }
}
