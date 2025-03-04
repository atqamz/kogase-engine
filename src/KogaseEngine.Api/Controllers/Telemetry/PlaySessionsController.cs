using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Telemetry;
using KogaseEngine.Core.Services.Telemetry;
using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Core.Services.Iam;
using KogaseEngine.Core.Services.Auth;

namespace KogaseEngine.Api.Controllers.Telemetry;

[ApiController]
[Route("api/v1/telemetry/sessions")]
public class PlaySessionsController : ControllerBase
{
    readonly PlaySessionService _sessionService;
    readonly TelemetryEventService _eventService;
    readonly UserService _userService;
    readonly DeviceService _deviceService;

    public PlaySessionsController(
        PlaySessionService sessionService,
        TelemetryEventService eventService,
        UserService userService,
        DeviceService deviceService)
    {
        _sessionService = sessionService;
        _eventService = eventService;
        _userService = userService;
        _deviceService = deviceService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<PlaySessionDto>> StartSession(StartPlaySessionDto startDto)
    {
        try
        {
            var session = new PlaySession
            {
                ProjectId = startDto.ProjectId,
                UserId = startDto.UserId,
                DeviceId = startDto.DeviceId,
                GameVersion = startDto.GameVersion,
                Platform = startDto.Platform,
                Country = startDto.Country,
                DeviceModel = startDto.DeviceModel,
                OsVersion = startDto.OsVersion,
                SessionProperties = startDto.SessionProperties != null
                    ? JsonSerializer.Serialize(startDto.SessionProperties)
                    : "{}"
            };

            var result = await _sessionService.StartSessionAsync(session);
            return Ok(await MapToDtoAsync(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("end/{sessionId:guid}")]
    public async Task<ActionResult<PlaySessionDto>> EndSession(Guid sessionId)
    {
        try
        {
            var session = await _sessionService.EndSessionAsync(sessionId);
            if (session == null)
                return NotFound();

            return Ok(await MapToDtoAsync(session));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("status/{sessionId:guid}")]
    public async Task<ActionResult<PlaySessionDto>> UpdateSessionStatus(Guid sessionId,
        UpdateSessionStatusDto updateDto)
    {
        try
        {
            var session = await _sessionService.UpdateSessionStatusAsync(sessionId, updateDto.Status);
            if (session == null)
                return NotFound();

            return Ok(await MapToDtoAsync(session));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<PlaySessionDto>> GetSession(Guid sessionId)
    {
        var session = await _sessionService.GetSessionByIdAsync(sessionId);
        if (session == null)
            return NotFound();

        return Ok(await MapToDtoAsync(session));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<PlaySessionDto>>> GetSessionsByProject(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var sessions = await _sessionService.GetSessionsByProjectIdAsync(projectId, page, pageSize);
        var sessionDtos = new List<PlaySessionDto>();

        foreach (var session in sessions) sessionDtos.Add(await MapToDtoAsync(session));

        return Ok(sessionDtos);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<PlaySessionDto>>> GetSessionsByUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var sessions = await _sessionService.GetSessionsByUserIdAsync(userId, page, pageSize);
        var sessionDtos = new List<PlaySessionDto>();

        foreach (var session in sessions) sessionDtos.Add(await MapToDtoAsync(session));

        return Ok(sessionDtos);
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<PlaySessionDto>>> GetSessionsByDevice(
        Guid deviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var sessions = await _sessionService.GetSessionsByDeviceIdAsync(deviceId, page, pageSize);
        var sessionDtos = new List<PlaySessionDto>();

        foreach (var session in sessions) sessionDtos.Add(await MapToDtoAsync(session));

        return Ok(sessionDtos);
    }

    [HttpGet("active/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<PlaySessionDto>>> GetActiveSessions(Guid projectId)
    {
        var sessions = await _sessionService.GetActiveSessionsAsync(projectId);
        var sessionDtos = new List<PlaySessionDto>();

        foreach (var session in sessions) sessionDtos.Add(await MapToDtoAsync(session));

        return Ok(sessionDtos);
    }

    [HttpGet("timerange/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<PlaySessionDto>>> GetSessionsByTimeRange(
        Guid projectId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var sessions = await _sessionService.GetSessionsByTimeRangeAsync(projectId, start, end, page, pageSize);
        var sessionDtos = new List<PlaySessionDto>();

        foreach (var session in sessions) sessionDtos.Add(await MapToDtoAsync(session));

        return Ok(sessionDtos);
    }

    [HttpGet("count/project/{projectId:guid}")]
    public async Task<ActionResult<int>> GetSessionCountByProject(Guid projectId)
    {
        var count = await _sessionService.GetSessionCountByProjectIdAsync(projectId);
        return Ok(new { count });
    }

    [HttpGet("average-duration/{projectId:guid}")]
    public async Task<ActionResult<double>> GetAverageSessionDuration(Guid projectId)
    {
        var averageDuration = await _sessionService.GetAverageSessionDurationAsync(projectId);
        return Ok(new { averageDuration });
    }

    async Task<PlaySessionDto> MapToDtoAsync(PlaySession session)
    {
        string? userName = null;
        if (session.UserId.HasValue)
        {
            var user = await _userService.GetUserByIdAsync(session.UserId.Value);
            if (user != null)
                userName = $"{user.FirstName} {user.LastName}";
        }

        string? deviceInfo = null;
        if (session.DeviceId.HasValue)
        {
            var device = await _deviceService.GetDeviceByIdAsync(session.DeviceId.Value);
            if (device != null)
                deviceInfo = $"{device.Platform} - {device.DeviceModel} ({device.OsVersion})";
        }

        object? sessionProperties = null;
        try
        {
            if (!string.IsNullOrEmpty(session.SessionProperties))
                sessionProperties = JsonSerializer.Deserialize<object>(session.SessionProperties);
        }
        catch
        {
            // If deserialization fails, leave as null
        }

        var eventCount = 0;
        if (session.Id != Guid.Empty)
            eventCount = await _eventService.GetEventCountBySessionIdAsync(session.Id);

        return new PlaySessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            UserId = session.UserId,
            UserName = userName,
            DeviceId = session.DeviceId,
            DeviceInfo = deviceInfo,
            GameVersion = session.GameVersion,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            DurationSeconds = session.DurationSeconds,
            Platform = session.Platform,
            Country = session.Country,
            DeviceModel = session.DeviceModel,
            OsVersion = session.OsVersion,
            SessionProperties = sessionProperties,
            Status = session.Status,
            StatusString = session.Status.ToString(),
            EventCount = eventCount
        };
    }
}