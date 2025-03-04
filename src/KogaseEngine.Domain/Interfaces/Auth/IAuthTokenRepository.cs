using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Domain.Interfaces.Auth;

public interface IAuthTokenRepository : IRepository<AuthToken>
{
    Task<AuthToken?> GetByTokenAsync(string token);
    Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken);
    Task RevokeAsync(Guid id);
    Task RevokeAllForUserAsync(Guid userId);
    Task RevokeAllForDeviceAsync(Guid deviceId);
}