using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Auth;

public class SessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(
        ISessionRepository sessionRepository,
        IDeviceRepository deviceRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork
    )
    {
        _sessionRepository = sessionRepository;
        _deviceRepository = deviceRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Session?> GetSessionByIdAsync(Guid id)
    {
        return await _sessionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Session>> GetSessionsByDeviceIdAsync(Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new InvalidOperationException($"Device with ID {deviceId} not found.");

        return await _sessionRepository.GetByDeviceIdAsync(deviceId);
    }

    public async Task<IEnumerable<Session>> GetSessionsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {projectId} not found.");

        return await _sessionRepository.GetByProjectIdAsync(projectId, page, pageSize);
    }

    public async Task<IEnumerable<Session>> GetSessionsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        return await _sessionRepository.GetByUserIdAsync(userId, page, pageSize);
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {projectId} not found.");

        return await _sessionRepository.GetActiveSessionsAsync(projectId);
    }

    public async Task<Session> StartSessionAsync(Session session)
    {
        // Validate device
        var device = await _deviceRepository.GetByIdAsync(session.DeviceId);
        if (device == null)
            throw new InvalidOperationException($"Device with ID {session.DeviceId} not found.");

        // Validate project
        var project = await _projectRepository.GetByIdAsync(session.ProjectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {session.ProjectId} not found.");

        // Validate user if provided
        if (session.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(session.UserId.Value);
            if (user == null)
                throw new InvalidOperationException($"User with ID {session.UserId} not found.");
        }

        // Create the session
        var newSession = await _sessionRepository.CreateAsync(session);
        
        // Update device last active time
        device.LastActiveAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);
        
        await _unitOfWork.SaveChangesAsync();
        return newSession;
    }

    public async Task EndSessionAsync(Guid id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new InvalidOperationException($"Session with ID {id} not found.");

        await _sessionRepository.EndSessionAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task EndAllSessionsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        await _sessionRepository.EndAllSessionsForUserAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TerminateSessionAsync(Guid id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new InvalidOperationException($"Session with ID {id} not found.");

        await _sessionRepository.EndSessionAsync(id, SessionStatus.Terminated);
        await _unitOfWork.SaveChangesAsync();
    }
}