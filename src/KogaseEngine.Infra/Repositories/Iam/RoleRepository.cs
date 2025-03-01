using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Infra.Persistence;

namespace KogaseEngine.Infra.Repositories.Iam;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Roles
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        return role;
    }

    public async Task UpdateAsync(Role role)
    {
        var existingRole = await _context.Roles.FindAsync(role.Id);
        if (existingRole == null)
            throw new KeyNotFoundException($"Role with ID {role.Id} not found.");
            
        _context.Entry(existingRole).CurrentValues.SetValues(role);
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role != null)
        {
            _context.Roles.Remove(role);
        }
    }
}