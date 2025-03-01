using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Auth;

public class AuthToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Device? Device { get; set; }
}