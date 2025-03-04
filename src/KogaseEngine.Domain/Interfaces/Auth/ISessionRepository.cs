using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Domain.Interfaces.Auth;

public interface ISessionRepository : IRepository<Session>
{
    Task<IEnumerable<Session>> GetByDeviceIdAsync(Guid deviceId);
    Task<IEnumerable<Session>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10);
    Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<IEnumerable<Session>> GetActiveSessionsAsync(Guid projectId);
    Task EndSessionAsync(Guid id, SessionStatus status = SessionStatus.Ended);
    Task EndAllSessionsForUserAsync(Guid userId, SessionStatus status = SessionStatus.Ended);
}