using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Auth;

public class AuthTokenRepository : IAuthTokenRepository
{
    readonly ApplicationDbContext _context;

    public AuthTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthToken?> GetByIdAsync(Guid id)
    {
        return await _context.AuthTokens
            .Include(t => t.User)
            .Include(t => t.Device)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<AuthToken>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.AuthTokens
            .Include(t => t.User)
            .Include(t => t.Device)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AuthToken> CreateAsync(AuthToken authToken)
    {
        authToken.IssuedAt = DateTime.UtcNow;

        await _context.AuthTokens.AddAsync(authToken);
        return authToken;
    }

    public Task UpdateAsync(AuthToken authToken)
    {
        _context.AuthTokens.Update(authToken);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var authToken = _context.AuthTokens.Find(id);
        if (authToken != null) _context.AuthTokens.Remove(authToken);

        return Task.CompletedTask;
    }

    public async Task<AuthToken?> GetByTokenAsync(string token)
    {
        return await _context.AuthTokens
            .Include(t => t.User)
            .Include(t => t.Device)
            .FirstOrDefaultAsync(t => t.Token == token && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.AuthTokens
            .Include(t => t.User)
            .Include(t => t.Device)
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken && t.RevokedAt == null);
    }

    public async Task RevokeAsync(Guid id)
    {
        var token = await _context.AuthTokens.FindAsync(id);
        if (token != null && token.RevokedAt == null)
        {
            token.RevokedAt = DateTime.UtcNow;
            _context.AuthTokens.Update(token);
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var tokens = await _context.AuthTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens) token.RevokedAt = DateTime.UtcNow;

        _context.AuthTokens.UpdateRange(tokens);
    }

    public async Task RevokeAllForDeviceAsync(Guid deviceId)
    {
        var tokens = await _context.AuthTokens
            .Where(t => t.DeviceId == deviceId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens) token.RevokedAt = DateTime.UtcNow;

        _context.AuthTokens.UpdateRange(tokens);
    }
}