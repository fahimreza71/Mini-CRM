using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Application.Common;

public interface ICurrentUser
{
    Guid? UserId { get; }
}
