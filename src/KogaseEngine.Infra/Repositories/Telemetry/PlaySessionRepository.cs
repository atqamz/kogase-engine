using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces.Telemetry;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Telemetry;

public class PlaySessionRepository : Repository<PlaySession>, IPlaySessionRepository
{
    public PlaySessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByProjectIdAsync(Guid projectId, int page = 1,
        int pageSize = 100)
    {
        return await _context.PlaySessions
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        return await _context.PlaySessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByDeviceIdAsync(Guid deviceId, int page = 1,
        int pageSize = 100)
    {
        return await _context.PlaySessions
            .Where(s => s.DeviceId == deviceId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlaySession>> GetActiveSessionsAsync(Guid projectId)
    {
        return await _context.PlaySessions
            .Where(s => s.ProjectId == projectId && s.Status == PlaySessionStatus.Active)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByTimeRangeAsync(Guid projectId, DateTime start,
        DateTime end, int page = 1, int pageSize = 100)
    {
        return await _context.PlaySessions
            .Where(s => s.ProjectId == projectId && s.StartTime >= start && (s.EndTime == null || s.EndTime <= end))
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetSessionCountByProjectIdAsync(Guid projectId)
    {
        return await _context.PlaySessions
            .Where(s => s.ProjectId == projectId)
            .CountAsync();
    }

    public async Task<double> GetAverageSessionDurationAsync(Guid projectId)
    {
        return await _context.PlaySessions
            .Where(s => s.ProjectId == projectId && s.DurationSeconds.HasValue)
            .AverageAsync(s => s.DurationSeconds!.Value);
    }
}