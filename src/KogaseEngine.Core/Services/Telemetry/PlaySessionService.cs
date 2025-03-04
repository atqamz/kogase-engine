using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Telemetry;

namespace KogaseEngine.Core.Services.Telemetry;

public class PlaySessionService
{
    readonly IPlaySessionRepository _sessionRepository;
    readonly IUnitOfWork _unitOfWork;

    public PlaySessionService(
        IPlaySessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlaySession> StartSessionAsync(PlaySession session)
    {
        session.Id = Guid.NewGuid();
        session.StartTime = DateTime.UtcNow;
        session.Status = PlaySessionStatus.Active;

        await _sessionRepository.CreateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public async Task<PlaySession?> EndSessionAsync(Guid sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found");

        if (session.Status != PlaySessionStatus.Active)
            throw new InvalidOperationException("Session is not active");

        session.EndTime = DateTime.UtcNow;
        session.Status = PlaySessionStatus.Ended;
        session.DurationSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;

        await _sessionRepository.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public async Task<PlaySession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _sessionRepository.GetByIdAsync(sessionId);
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByProjectIdAsync(Guid projectId, int page = 1,
        int pageSize = 100)
    {
        return await _sessionRepository.GetSessionsByProjectIdAsync(projectId, page, pageSize);
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        return await _sessionRepository.GetSessionsByUserIdAsync(userId, page, pageSize);
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByDeviceIdAsync(Guid deviceId, int page = 1,
        int pageSize = 100)
    {
        return await _sessionRepository.GetSessionsByDeviceIdAsync(deviceId, page, pageSize);
    }

    public async Task<IEnumerable<PlaySession>> GetActiveSessionsAsync(Guid projectId)
    {
        return await _sessionRepository.GetActiveSessionsAsync(projectId);
    }

    public async Task<IEnumerable<PlaySession>> GetSessionsByTimeRangeAsync(Guid projectId, DateTime start,
        DateTime end, int page = 1, int pageSize = 100)
    {
        return await _sessionRepository.GetSessionsByTimeRangeAsync(projectId, start, end, page, pageSize);
    }

    public async Task<int> GetSessionCountByProjectIdAsync(Guid projectId)
    {
        return await _sessionRepository.GetSessionCountByProjectIdAsync(projectId);
    }

    public async Task<double> GetAverageSessionDurationAsync(Guid projectId)
    {
        return await _sessionRepository.GetAverageSessionDurationAsync(projectId);
    }

    public async Task<PlaySession?> UpdateSessionStatusAsync(Guid sessionId, PlaySessionStatus status)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found");

        session.Status = status;

        if (status != PlaySessionStatus.Active && !session.EndTime.HasValue)
        {
            session.EndTime = DateTime.UtcNow;
            session.DurationSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
        }

        await _sessionRepository.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return session;
    }
}