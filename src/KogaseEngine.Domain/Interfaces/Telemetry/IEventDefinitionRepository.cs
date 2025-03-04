using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Domain.Interfaces.Telemetry;

public interface IEventDefinitionRepository : IRepository<EventDefinition>
{
    Task<IEnumerable<EventDefinition>> GetDefinitionsByProjectIdAsync(Guid projectId);
    Task<EventDefinition?> GetDefinitionByNameAsync(Guid projectId, string eventName);
    Task<IEnumerable<EventDefinition>> GetDefinitionsByCategoryAsync(Guid projectId, string category);
}