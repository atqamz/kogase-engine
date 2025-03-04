using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Telemetry;

namespace KogaseEngine.Core.Services.Telemetry;

public class TelemetryEventService
{
    private readonly ITelemetryEventRepository _eventRepository;
    private readonly IPlaySessionRepository _sessionRepository;
    private readonly IEventDefinitionRepository _definitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TelemetryEventService(
        ITelemetryEventRepository eventRepository,
        IPlaySessionRepository sessionRepository,
        IEventDefinitionRepository definitionRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _sessionRepository = sessionRepository;
        _definitionRepository = definitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TelemetryEvent> LogEventAsync(TelemetryEvent telemetryEvent)
    {
        telemetryEvent.Id = Guid.NewGuid();
        telemetryEvent.Timestamp = DateTime.UtcNow;

        // Verify event definition exists for this project
        var definition = await _definitionRepository.GetDefinitionByNameAsync(telemetryEvent.ProjectId, telemetryEvent.EventName);
        
        // If definition doesn't exist, we'll create it automatically
        if (definition == null)
        {
            definition = new EventDefinition
            {
                Id = Guid.NewGuid(),
                ProjectId = telemetryEvent.ProjectId,
                EventName = telemetryEvent.EventName,
                Category = telemetryEvent.Category,
                Description = $"Auto-generated definition for {telemetryEvent.EventName}",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _definitionRepository.CreateAsync(definition);
        }
        
        await _eventRepository.CreateAsync(telemetryEvent);
        await _unitOfWork.SaveChangesAsync();
        
        return telemetryEvent;
    }

    public async Task<IEnumerable<TelemetryEvent>> LogEventsAsync(IEnumerable<TelemetryEvent> events)
    {
        var eventsList = events.ToList();
        var timestamp = DateTime.UtcNow;
        
        foreach (var telemetryEvent in eventsList)
        {
            telemetryEvent.Id = Guid.NewGuid();
            telemetryEvent.Timestamp = timestamp;
            
            // We'll auto-create definitions in bulk later
        }
        
        // Auto-create event definitions for any new event types
        var uniqueEventTypes = eventsList
            .Select(e => new { e.ProjectId, e.EventName, e.Category })
            .Distinct()
            .ToList();
            
        foreach (var eventType in uniqueEventTypes)
        {
            var definition = await _definitionRepository.GetDefinitionByNameAsync(eventType.ProjectId, eventType.EventName);
            
            if (definition == null)
            {
                definition = new EventDefinition
                {
                    Id = Guid.NewGuid(),
                    ProjectId = eventType.ProjectId,
                    EventName = eventType.EventName,
                    Category = eventType.Category,
                    Description = $"Auto-generated definition for {eventType.EventName}",
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _definitionRepository.CreateAsync(definition);
            }
        }
        
        await _eventRepository.BatchInsertEventsAsync(eventsList);
        await _unitOfWork.SaveChangesAsync();
        
        return eventsList;
    }

    public async Task<TelemetryEvent?> GetEventByIdAsync(Guid eventId)
    {
        return await _eventRepository.GetByIdAsync(eventId);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByProjectIdAsync(projectId, page, pageSize);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsBySessionIdAsync(Guid sessionId)
    {
        return await _eventRepository.GetEventsBySessionIdAsync(sessionId);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByUserIdAsync(userId, page, pageSize);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByDeviceIdAsync(Guid deviceId, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByDeviceIdAsync(deviceId, page, pageSize);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByNameAsync(Guid projectId, string eventName, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByNameAsync(projectId, eventName, page, pageSize);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByCategoryAsync(Guid projectId, string category, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByCategoryAsync(projectId, category, page, pageSize);
    }

    public async Task<IEnumerable<TelemetryEvent>> GetEventsByTimeRangeAsync(Guid projectId, DateTime start, DateTime end, int page = 1, int pageSize = 100)
    {
        return await _eventRepository.GetEventsByTimeRangeAsync(projectId, start, end, page, pageSize);
    }

    public async Task<int> GetEventCountByProjectIdAsync(Guid projectId)
    {
        return await _eventRepository.GetEventCountByProjectIdAsync(projectId);
    }

    public async Task<int> GetEventCountBySessionIdAsync(Guid sessionId)
    {
        return await _eventRepository.GetEventCountBySessionIdAsync(sessionId);
    }
}