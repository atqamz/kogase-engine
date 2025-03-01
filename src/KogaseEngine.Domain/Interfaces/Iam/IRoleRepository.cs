using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Interfaces.Iam;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
}