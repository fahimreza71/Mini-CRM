using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; private set; }
        public byte[] RowVersion { get; private set; } = [];   // optimistic concurrency token

        public DateTimeOffset CreatedAt { get; private set; }
        public Guid? CreatedBy { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public bool IsDeleted { get; private set; }            // soft delete
        public DateTimeOffset? DeletedAt { get; private set; }
        public Guid? DeletedBy { get; private set; }
    }
}
