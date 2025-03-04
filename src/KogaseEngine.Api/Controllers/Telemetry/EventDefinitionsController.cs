using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Telemetry;
using KogaseEngine.Core.Services.Telemetry;
using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Api.Controllers.Telemetry;

[ApiController]
[Route("api/v1/telemetry/definitions")]
public class EventDefinitionsController : ControllerBase
{
    private readonly EventDefinitionService _definitionService;

    public EventDefinitionsController(EventDefinitionService definitionService)
    {
        _definitionService = definitionService;
    }

    [HttpPost]
    public async Task<ActionResult<EventDefinitionDto>> CreateDefinition(CreateEventDefinitionDto createDto)
    {
        try
        {
            var definition = new EventDefinition
            {
                ProjectId = createDto.ProjectId,
                EventName = createDto.EventName,
                Category = createDto.Category,
                Description = createDto.Description,
                IsEnabled = createDto.IsEnabled,
                Schema = createDto.Schema != null ? JsonSerializer.Serialize(createDto.Schema) : "{}"
            };
            
            var result = await _definitionService.CreateDefinitionAsync(definition);
            return CreatedAtAction(nameof(GetDefinition), new { definitionId = result.Id }, MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{definitionId:guid}")]
    public async Task<ActionResult<EventDefinitionDto>> UpdateDefinition(Guid definitionId, UpdateEventDefinitionDto updateDto)
    {
        try
        {
            var definition = new EventDefinition
            {
                Category = updateDto.Category,
                Description = updateDto.Description,
                IsEnabled = updateDto.IsEnabled,
                Schema = updateDto.Schema != null ? JsonSerializer.Serialize(updateDto.Schema) : "{}"
            };
            
            var result = await _definitionService.UpdateDefinitionAsync(definitionId, definition);
            if (result == null)
                return NotFound();
                
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{definitionId:guid}")]
    public async Task<ActionResult<EventDefinitionDto>> GetDefinition(Guid definitionId)
    {
        var definition = await _definitionService.GetDefinitionByIdAsync(definitionId);
        if (definition == null)
            return NotFound();
            
        return Ok(MapToDto(definition));
    }

    [HttpGet("name/{projectId:guid}/{eventName}")]
    public async Task<ActionResult<EventDefinitionDto>> GetDefinitionByName(Guid projectId, string eventName)
    {
        var definition = await _definitionService.GetDefinitionByNameAsync(projectId, eventName);
        if (definition == null)
            return NotFound();
            
        return Ok(MapToDto(definition));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<EventDefinitionDto>>> GetDefinitionsByProject(Guid projectId)
    {
        var definitions = await _definitionService.GetDefinitionsByProjectIdAsync(projectId);
        return Ok(definitions.Select(MapToDto));
    }

    [HttpGet("category/{projectId:guid}/{category}")]
    public async Task<ActionResult<IEnumerable<EventDefinitionDto>>> GetDefinitionsByCategory(Guid projectId, string category)
    {
        var definitions = await _definitionService.GetDefinitionsByCategoryAsync(projectId, category);
        return Ok(definitions.Select(MapToDto));
    }

    [HttpDelete("{definitionId:guid}")]
    public async Task<ActionResult> DeleteDefinition(Guid definitionId)
    {
        var success = await _definitionService.DeleteDefinitionAsync(definitionId);
        if (!success)
            return NotFound();
            
        return NoContent();
    }

    [HttpPost("validate")]
    public async Task<ActionResult> ValidateEventPayload(
        [FromQuery] Guid projectId, 
        [FromQuery] string eventName, 
        [FromBody] object payload)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var isValid = await _definitionService.ValidateEventPayloadAsync(projectId, eventName, payloadJson);
        
        if (isValid)
            return Ok(new { valid = true });
        else
            return BadRequest(new { valid = false, message = "Event payload does not match the schema" });
    }

    private EventDefinitionDto MapToDto(EventDefinition definition)
    {
        object? schema = null;
        try
        {
            if (!string.IsNullOrEmpty(definition.Schema))
                schema = JsonSerializer.Deserialize<object>(definition.Schema);
        }
        catch
        {
            // If deserialization fails, leave as null
        }
        
        return new EventDefinitionDto
        {
            Id = definition.Id,
            ProjectId = definition.ProjectId,
            EventName = definition.EventName,
            Category = definition.Category,
            Description = definition.Description,
            IsEnabled = definition.IsEnabled,
            Schema = schema,
            CreatedAt = definition.CreatedAt,
            UpdatedAt = definition.UpdatedAt
        };
    }
}