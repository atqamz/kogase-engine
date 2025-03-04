using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces.Telemetry;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Telemetry;

public class EventDefinitionRepository : Repository<EventDefinition>, IEventDefinitionRepository
{
    public EventDefinitionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EventDefinition>> GetDefinitionsByProjectIdAsync(Guid projectId)
    {
        return await _context.EventDefinitions
            .Where(d => d.ProjectId == projectId)
            .OrderBy(d => d.EventName)
            .ToListAsync();
    }

    public async Task<EventDefinition?> GetDefinitionByNameAsync(Guid projectId, string eventName)
    {
        return await _context.EventDefinitions
            .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.EventName == eventName);
    }

    public async Task<IEnumerable<EventDefinition>> GetDefinitionsByCategoryAsync(Guid projectId, string category)
    {
        return await _context.EventDefinitions
            .Where(d => d.ProjectId == projectId && d.Category == category)
            .OrderBy(d => d.EventName)
            .ToListAsync();
    }
}