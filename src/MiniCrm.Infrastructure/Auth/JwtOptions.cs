using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MiniCrm.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;
    [Required, MinLength(32)] public string Key { get; set; } = string.Empty;   // from user-secrets, never the repo
    [Range(1, 60)] public int AccessTokenMinutes { get; set; } = 15;
}
