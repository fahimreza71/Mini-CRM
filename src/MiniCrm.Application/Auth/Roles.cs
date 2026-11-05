using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Application.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string SalesRep = "SalesRep";

    public static readonly string[] All = [Admin, Manager, SalesRep];
}
