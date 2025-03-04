using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Auth;

public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Session?> GetByIdAsync(Guid id)
    {
        return await _context.Sessions
            .Include(s => s.Device)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public override async Task<IEnumerable<Session>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Sessions
            .Include(s => s.Device)
            .Include(s => s.User)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public override async Task<Session> CreateAsync(Session session)
    {
        session.StartTime = DateTime.UtcNow;
        session.Status = SessionStatus.Active;

        return await base.CreateAsync(session);
    }

    public async Task<IEnumerable<Session>> GetByDeviceIdAsync(Guid deviceId)
    {
        return await _context.Sessions
            .Where(s => s.DeviceId == deviceId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10)
    {
        return await _context.Sessions
            .Include(s => s.Device)
            .Include(s => s.User)
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        return await _context.Sessions
            .Include(s => s.Device)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync(Guid projectId)
    {
        return await _context.Sessions
            .Include(s => s.Device)
            .Include(s => s.User)
            .Where(s => s.ProjectId == projectId && s.Status == SessionStatus.Active)
            .ToListAsync();
    }

    public async Task EndSessionAsync(Guid id, SessionStatus status = SessionStatus.Ended)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session != null && session.Status == SessionStatus.Active)
        {
            session.EndTime = DateTime.UtcNow;
            session.Status = status;
            _context.Sessions.Update(session);
        }
    }

    public async Task EndAllSessionsForUserAsync(Guid userId, SessionStatus status = SessionStatus.Ended)
    {
        var activeSessions = await _context.Sessions
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Active)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.EndTime = DateTime.UtcNow;
            session.Status = status;
        }

        _context.Sessions.UpdateRange(activeSessions);
    }
}