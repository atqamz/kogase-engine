using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Telemetry;

namespace KogaseEngine.Core.Services.Telemetry;

public class EventDefinitionService
{
    private readonly IEventDefinitionRepository _definitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventDefinitionService(
        IEventDefinitionRepository definitionRepository,
        IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDefinition> CreateDefinitionAsync(EventDefinition definition)
    {
        // Check if definition already exists
        var existingDefinition = await _definitionRepository.GetDefinitionByNameAsync(
            definition.ProjectId, 
            definition.EventName);
            
        if (existingDefinition != null)
            throw new InvalidOperationException($"Event definition for '{definition.EventName}' already exists");
            
        definition.Id = Guid.NewGuid();
        definition.CreatedAt = DateTime.UtcNow;
        
        await _definitionRepository.CreateAsync(definition);
        await _unitOfWork.SaveChangesAsync();
        
        return definition;
    }

    public async Task<EventDefinition?> UpdateDefinitionAsync(Guid definitionId, EventDefinition updatedDefinition)
    {
        var definition = await _definitionRepository.GetByIdAsync(definitionId);
        if (definition == null)
            throw new InvalidOperationException("Event definition not found");
            
        definition.Description = updatedDefinition.Description;
        definition.Category = updatedDefinition.Category;
        definition.IsEnabled = updatedDefinition.IsEnabled;
        definition.Schema = updatedDefinition.Schema;
        definition.UpdatedAt = DateTime.UtcNow;
        
        await _definitionRepository.UpdateAsync(definition);
        await _unitOfWork.SaveChangesAsync();
        
        return definition;
    }

    public async Task<EventDefinition?> GetDefinitionByIdAsync(Guid definitionId)
    {
        return await _definitionRepository.GetByIdAsync(definitionId);
    }

    public async Task<EventDefinition?> GetDefinitionByNameAsync(Guid projectId, string eventName)
    {
        return await _definitionRepository.GetDefinitionByNameAsync(projectId, eventName);
    }

    public async Task<IEnumerable<EventDefinition>> GetDefinitionsByProjectIdAsync(Guid projectId)
    {
        return await _definitionRepository.GetDefinitionsByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<EventDefinition>> GetDefinitionsByCategoryAsync(Guid projectId, string category)
    {
        return await _definitionRepository.GetDefinitionsByCategoryAsync(projectId, category);
    }

    public async Task<bool> DeleteDefinitionAsync(Guid definitionId)
    {
        var definition = await _definitionRepository.GetByIdAsync(definitionId);
        if (definition == null)
            return false;
            
        await _definitionRepository.DeleteAsync(definitionId);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ValidateEventPayloadAsync(Guid projectId, string eventName, string payload)
    {
        var definition = await _definitionRepository.GetDefinitionByNameAsync(projectId, eventName);
        if (definition == null || string.IsNullOrEmpty(definition.Schema))
            return true; // No schema to validate against
            
        // In a real implementation, you would use a JSON Schema validator library here
        // For this example, we'll just check if the payload is valid JSON
        try
        {
            JsonDocument.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }
}