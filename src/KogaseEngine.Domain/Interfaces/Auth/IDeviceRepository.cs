using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Domain.Interfaces.Auth;

public interface IDeviceRepository : IRepository<Device>
{
    Task<Device?> GetByInstallIdAsync(Guid projectId, string installId);
    Task<IEnumerable<Device>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10);
    Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId);
}