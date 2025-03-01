using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Infra.Persistence;

namespace KogaseEngine.Infra.Repositories.Iam;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Project> CreateAsync(Project project)
    {
        project.ApiKey = GenerateApiKey();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        
        await _context.Projects.AddAsync(project);
        
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        
        var existingProject = await _context.Projects.FindAsync(project.Id);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with ID {project.Id} not found.");
            
        _context.Entry(existingProject).CurrentValues.SetValues(project);
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
        }
    }

    public async Task<Project?> GetByApiKeyAsync(string apiKey)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.ApiKey == apiKey);
    }

    public async Task<string> GenerateNewApiKeyAsync(Guid projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            
        project.ApiKey = GenerateApiKey();
        project.UpdatedAt = DateTime.UtcNow;
        
        return project.ApiKey;
    }

    private string GenerateApiKey()
    {
        var key = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key).Replace("/", "_").Replace("+", "-").Replace("=", "");
    }
}