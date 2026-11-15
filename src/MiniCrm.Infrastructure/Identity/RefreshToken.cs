namespace MiniCrm.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }                 // all tokens from one login share this
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }        // set when exchanged for a new token
    public DateTimeOffset? RevokedAt { get; set; }     // set on logout or reuse detection
}