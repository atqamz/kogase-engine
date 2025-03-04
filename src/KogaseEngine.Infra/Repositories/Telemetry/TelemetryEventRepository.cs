using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces.Telemetry;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Telemetry;

public class TelemetryEventRepository : Repository<TelemetryEvent>, ITelemetryEventRepository
{
    public TelemetryEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByProjectIdAsync(Guid projectId, int page = 1,
        int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsBySessionIdAsync(Guid sessionId)
    {
        return await _context.TelemetryEvents
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByDeviceIdAsync(Guid deviceId, int page = 1,
        int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.DeviceId == deviceId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByNameAsync(Guid projectId, string eventName, int page = 1,
        int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.ProjectId == projectId && e.EventName == eventName)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByCategoryAsync(Guid projectId, string category,
        int page = 1, int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.ProjectId == projectId && e.Category == category)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByTimeRangeAsync(Guid projectId, DateTime start,
        DateTime end, int page = 1, int pageSize = 100)
    {
        return await _context.TelemetryEvents
            .Where(e => e.ProjectId == projectId && e.Timestamp >= start && e.Timestamp <= end)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetEventCountByProjectIdAsync(Guid projectId)
    {
        return await _context.TelemetryEvents
            .Where(e => e.ProjectId == projectId)
            .CountAsync();
    }

    public async Task<int> GetEventCountBySessionIdAsync(Guid sessionId)
    {
        return await _context.TelemetryEvents
            .Where(e => e.SessionId == sessionId)
            .CountAsync();
    }

    public async Task BatchInsertEventsAsync(IEnumerable<TelemetryEvent> events)
    {
        await _context.TelemetryEvents.AddRangeAsync(events);
    }
}