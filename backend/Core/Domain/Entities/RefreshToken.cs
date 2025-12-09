namespace Core.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public required Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public DateTimeOffset Expires { get; set; }
    public DateTimeOffset? Revoked { get; set; }
    
    public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;
}