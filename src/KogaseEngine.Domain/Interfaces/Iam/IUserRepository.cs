using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Interfaces.Iam;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<Project?>> GetUserProjectsAsync(Guid userId);
}

