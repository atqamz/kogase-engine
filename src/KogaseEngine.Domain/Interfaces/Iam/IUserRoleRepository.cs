using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Interfaces.Iam;

public interface IUserRoleRepository
{
    Task<UserRole?> GetAsync(Guid userId, Guid roleId, Guid? projectId);
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRole>> GetByProjectIdAsync(Guid projectId);
    Task<UserRole> AssignRoleAsync(UserRole userRole);
    Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? projectId);
}