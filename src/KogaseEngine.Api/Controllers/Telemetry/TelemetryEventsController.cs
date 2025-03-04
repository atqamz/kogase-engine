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
[Route("api/v1/telemetry/events")]
public class TelemetryEventsController : ControllerBase
{
    readonly TelemetryEventService _eventService;
    readonly PlaySessionService _sessionService;
    readonly EventDefinitionService _definitionService;

    public TelemetryEventsController(
        TelemetryEventService eventService,
        PlaySessionService sessionService,
        EventDefinitionService definitionService)
    {
        _eventService = eventService;
        _sessionService = sessionService;
        _definitionService = definitionService;
    }

    [HttpPost("log")]
    public async Task<ActionResult<TelemetryEventDto>> LogEvent(LogEventDto logDto)
    {
        try
        {
            // Validate event payload against schema if one exists
            if (logDto.Payload != null)
            {
                var payloadJson = JsonSerializer.Serialize(logDto.Payload);
                var isValid = await _definitionService.ValidateEventPayloadAsync(
                    logDto.ProjectId,
                    logDto.EventName,
                    payloadJson);

                if (!isValid)
                    return BadRequest(new { message = "Invalid event payload format" });
            }

            var telemetryEvent = new TelemetryEvent
            {
                ProjectId = logDto.ProjectId,
                UserId = logDto.UserId,
                DeviceId = logDto.DeviceId,
                SessionId = logDto.SessionId,
                EventName = logDto.EventName,
                Category = logDto.Category,
                Payload = logDto.Payload != null ? JsonSerializer.Serialize(logDto.Payload) : "{}",
                Parameters = logDto.Parameters != null ? JsonSerializer.Serialize(logDto.Parameters) : "{}",
                ClientInfo = logDto.ClientInfo != null ? JsonSerializer.Serialize(logDto.ClientInfo) : "{}"
            };

            var result = await _eventService.LogEventAsync(telemetryEvent);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> LogBatchEvents(BatchLogEventDto batchDto)
    {
        try
        {
            var events = new List<TelemetryEvent>();

            foreach (var logDto in batchDto.Events)
            {
                // We'll do basic validation but not schema validation for batch operations
                if (logDto.ProjectId != batchDto.ProjectId)
                    return BadRequest(new { message = "All events in batch must have the same ProjectId" });

                var telemetryEvent = new TelemetryEvent
                {
                    ProjectId = logDto.ProjectId,
                    UserId = logDto.UserId,
                    DeviceId = logDto.DeviceId,
                    SessionId = logDto.SessionId,
                    EventName = logDto.EventName,
                    Category = logDto.Category,
                    Payload = logDto.Payload != null ? JsonSerializer.Serialize(logDto.Payload) : "{}",
                    Parameters = logDto.Parameters != null ? JsonSerializer.Serialize(logDto.Parameters) : "{}",
                    ClientInfo = logDto.ClientInfo != null ? JsonSerializer.Serialize(logDto.ClientInfo) : "{}"
                };

                events.Add(telemetryEvent);
            }

            var results = await _eventService.LogEventsAsync(events);
            return Ok(results.Select(MapToDto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<TelemetryEventDto>> GetEvent(Guid eventId)
    {
        var telemetryEvent = await _eventService.GetEventByIdAsync(eventId);
        if (telemetryEvent == null)
            return NotFound();

        return Ok(MapToDto(telemetryEvent));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByProject(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByProjectIdAsync(projectId, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsBySession(Guid sessionId)
    {
        var session = await _sessionService.GetSessionByIdAsync(sessionId);
        if (session == null)
            return NotFound(new { message = "Session not found" });

        var events = await _eventService.GetEventsBySessionIdAsync(sessionId);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByUserIdAsync(userId, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByDevice(
        Guid deviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByDeviceIdAsync(deviceId, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("name/{projectId:guid}/{eventName}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByName(
        Guid projectId,
        string eventName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByNameAsync(projectId, eventName, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("category/{projectId:guid}/{category}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByCategory(
        Guid projectId,
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByCategoryAsync(projectId, category, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("timerange/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<TelemetryEventDto>>> GetEventsByTimeRange(
        Guid projectId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var events = await _eventService.GetEventsByTimeRangeAsync(projectId, start, end, page, pageSize);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("count/project/{projectId:guid}")]
    public async Task<ActionResult<int>> GetEventCountByProject(Guid projectId)
    {
        var count = await _eventService.GetEventCountByProjectIdAsync(projectId);
        return Ok(new { count });
    }

    [HttpGet("count/session/{sessionId:guid}")]
    public async Task<ActionResult<int>> GetEventCountBySession(Guid sessionId)
    {
        var count = await _eventService.GetEventCountBySessionIdAsync(sessionId);
        return Ok(new { count });
    }

    TelemetryEventDto MapToDto(TelemetryEvent telemetryEvent)
    {
        object? payload = null;
        object? parameters = null;
        object? clientInfo = null;

        try
        {
            if (!string.IsNullOrEmpty(telemetryEvent.Payload))
                payload = JsonSerializer.Deserialize<object>(telemetryEvent.Payload);

            if (!string.IsNullOrEmpty(telemetryEvent.Parameters))
                parameters = JsonSerializer.Deserialize<object>(telemetryEvent.Parameters);

            if (!string.IsNullOrEmpty(telemetryEvent.ClientInfo))
                clientInfo = JsonSerializer.Deserialize<object>(telemetryEvent.ClientInfo);
        }
        catch
        {
            // If deserialization fails, leave as null
        }

        return new TelemetryEventDto
        {
            Id = telemetryEvent.Id,
            ProjectId = telemetryEvent.ProjectId,
            UserId = telemetryEvent.UserId,
            DeviceId = telemetryEvent.DeviceId,
            SessionId = telemetryEvent.SessionId,
            EventName = telemetryEvent.EventName,
            Category = telemetryEvent.Category,
            Timestamp = telemetryEvent.Timestamp,
            Payload = payload,
            Parameters = parameters,
            ClientInfo = clientInfo
        };
    }
}