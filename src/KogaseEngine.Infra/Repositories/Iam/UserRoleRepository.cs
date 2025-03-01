using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Infra.Persistence;

namespace KogaseEngine.Infra.Repositories.Iam;

public class UserRoleRepository : IUserRoleRepository
{
    readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole?> GetAsync(Guid userId, Guid roleId, Guid? projectId)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur =>
                ur.UserId == userId &&
                ur.RoleId == roleId &&
                ur.ProjectId == projectId);
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Include(ur => ur.Project)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.UserRoles
            .Where(ur => ur.ProjectId == projectId)
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<UserRole> AssignRoleAsync(UserRole userRole)
    {
        userRole.AssignedAt = DateTime.UtcNow;

        var existingUserRole = await GetAsync(userRole.UserId, userRole.RoleId, userRole.ProjectId);
        if (existingUserRole != null)
        {
            // Update if it already exists
            existingUserRole.AssignedAt = userRole.AssignedAt;
            existingUserRole.AssignedBy = userRole.AssignedBy;
            return existingUserRole;
        }

        await _context.UserRoles.AddAsync(userRole);
        return userRole;
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId, Guid? projectId)
    {
        var userRole = await GetAsync(userId, roleId, projectId);
        if (userRole != null) _context.UserRoles.Remove(userRole);
    }
}