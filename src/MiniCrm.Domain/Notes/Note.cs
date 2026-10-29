using System;
using System.Collections.Generic;
using System.Text;

using MiniCrm.Domain.Common;

namespace MiniCrm.Domain.Notes;

public sealed class Note : AuditableEntity
{
    public const int TextMaxLength = 2000;

    private Note() { }

    public Guid CustomerId { get; private set; }
    public Guid? DealId { get; private set; }     // optional: a note can also belong to a deal
    public string Text { get; private set; } = string.Empty;

    public static Note Create(Guid customerId, Guid? dealId, string text)
    {
        if (customerId == Guid.Empty) throw new DomainException("Customer is required.");
        return new Note
        {
            CustomerId = customerId,
            DealId = dealId,
            Text = Guard.RequiredText(text, nameof(Text), TextMaxLength)
        };
    }
}
