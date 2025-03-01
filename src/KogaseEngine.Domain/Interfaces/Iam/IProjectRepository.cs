using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Interfaces.Iam;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId);
    Task<Project?> GetByApiKeyAsync(string apiKey);
    Task<string> GenerateNewApiKeyAsync(Guid projectId);
}