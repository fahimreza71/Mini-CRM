using System;
using System.Collections.Generic;
using System.Text;

using MiniCrm.Application.Common;

namespace MiniCrm.Infrastructure;

public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
}
