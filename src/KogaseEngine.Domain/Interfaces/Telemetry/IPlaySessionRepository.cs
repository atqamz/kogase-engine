using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Domain.Interfaces.Telemetry;

public interface IPlaySessionRepository : IRepository<PlaySession>
{
    Task<IEnumerable<PlaySession>> GetSessionsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100);
    Task<IEnumerable<PlaySession>> GetSessionsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100);
    Task<IEnumerable<PlaySession>> GetSessionsByDeviceIdAsync(Guid deviceId, int page = 1, int pageSize = 100);
    Task<IEnumerable<PlaySession>> GetActiveSessionsAsync(Guid projectId);
    Task<IEnumerable<PlaySession>> GetSessionsByTimeRangeAsync(Guid projectId, DateTime start, DateTime end, int page = 1, int pageSize = 100);
    Task<int> GetSessionCountByProjectIdAsync(Guid projectId);
    Task<double> GetAverageSessionDurationAsync(Guid projectId);
}