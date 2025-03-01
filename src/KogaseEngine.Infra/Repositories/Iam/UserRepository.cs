using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Infra.Persistence;

namespace KogaseEngine.Infra.Repositories.Iam;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null)
            throw new KeyNotFoundException($"User with ID {user.Id} not found.");
            
        _context.Entry(existingUser).CurrentValues.SetValues(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
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