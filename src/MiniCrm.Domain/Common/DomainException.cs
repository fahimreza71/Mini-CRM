using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Domain.Common
{
    public sealed class DomainException(string message) : Exception(message);
}
