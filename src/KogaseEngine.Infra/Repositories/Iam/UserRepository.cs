using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Infra.Persistence;

namespace KogaseEngine.Infra.Repositories.Iam;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public override async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        return await base.CreateAsync(user);
    }

    public async Task<IEnumerable<Project?>> GetUserProjectsAsync(Guid userId)
    {
        // Get projects owned by the user
        var ownedProjects = await _context.Projects
            .Where(p => p.OwnerId == userId)
            .ToListAsync();

        // Get projects the user has roles in
        var roleProjects = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.ProjectId != null)
            .Select(ur => ur.Project)
            .ToListAsync();

        // Combine and deduplicate
        return ownedProjects.Union(roleProjects.Where(p => p != null)!);
    }
}