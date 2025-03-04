using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Domain.Interfaces.Telemetry;

public interface ITelemetryEventRepository : IRepository<TelemetryEvent>
{
    Task<IEnumerable<TelemetryEvent>> GetEventsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100);
    Task<IEnumerable<TelemetryEvent>> GetEventsBySessionIdAsync(Guid sessionId);
    Task<IEnumerable<TelemetryEvent>> GetEventsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100);
    Task<IEnumerable<TelemetryEvent>> GetEventsByDeviceIdAsync(Guid deviceId, int page = 1, int pageSize = 100);
    Task<IEnumerable<TelemetryEvent>> GetEventsByNameAsync(Guid projectId, string eventName, int page = 1, int pageSize = 100);
    Task<IEnumerable<TelemetryEvent>> GetEventsByCategoryAsync(Guid projectId, string category, int page = 1, int pageSize = 100);
    Task<IEnumerable<TelemetryEvent>> GetEventsByTimeRangeAsync(Guid projectId, DateTime start, DateTime end, int page = 1, int pageSize = 100);
    Task<int> GetEventCountByProjectIdAsync(Guid projectId);
    Task<int> GetEventCountBySessionIdAsync(Guid sessionId);
    Task BatchInsertEventsAsync(IEnumerable<TelemetryEvent> events);
}