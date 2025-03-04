using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Auth;
using KogaseEngine.Core.Services.Auth;
using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Api.Controllers.Auth;

[ApiController]
[Route("api/v1/auth/sessions")]
public class SessionsController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly DeviceService _deviceService;

    public SessionsController(SessionService sessionService, DeviceService deviceService)
    {
        _sessionService = sessionService;
        _deviceService = deviceService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<SessionDto>> StartSession(StartSessionDto startDto)
    {
        try
        {
            var session = new Session
            {
                ProjectId = startDto.ProjectId,
                UserId = startDto.UserId,
                DeviceId = startDto.DeviceId,
                IpAddress = startDto.IpAddress,
                UserAgent = startDto.UserAgent
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
    public async Task<ActionResult> EndSession(Guid sessionId)
    {
        try
        {
            await _sessionService.EndSessionAsync(sessionId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<SessionDto>> GetSession(Guid sessionId)
    {
        var session = await _sessionService.GetSessionByIdAsync(sessionId);
        if (session == null)
            return NotFound();

        return Ok(await MapToDtoAsync(session));
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetSessionsByDevice(Guid deviceId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByDeviceIdAsync(deviceId);
            var sessionDtos = new List<SessionDto>();
            
            foreach (var session in sessions)
            {
                sessionDtos.Add(await MapToDtoAsync(session));
            }
            
            return Ok(sessionDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetSessionsByProject(
        Guid projectId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByProjectIdAsync(projectId, page, pageSize);
            var sessionDtos = new List<SessionDto>();
            
            foreach (var session in sessions)
            {
                sessionDtos.Add(await MapToDtoAsync(session));
            }
            
            return Ok(sessionDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetSessionsByUser(
        Guid userId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByUserIdAsync(userId, page, pageSize);
            var sessionDtos = new List<SessionDto>();
            
            foreach (var session in sessions)
            {
                sessionDtos.Add(await MapToDtoAsync(session));
            }
            
            return Ok(sessionDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("active/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetActiveSessions(Guid projectId)
    {
        try
        {
            var sessions = await _sessionService.GetActiveSessionsAsync(projectId);
            var sessionDtos = new List<SessionDto>();
            
            foreach (var session in sessions)
            {
                sessionDtos.Add(await MapToDtoAsync(session));
            }
            
            return Ok(sessionDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<SessionDto> MapToDtoAsync(Session session)
    {
        string deviceInfo = "Unknown Device";
        
        if (session.Device != null)
        {
            deviceInfo = $"{session.Device.Platform} - {session.Device.DeviceModel} ({session.Device.OsVersion})";
        }
        else if (session.DeviceId != Guid.Empty)
        {
            var device = await _deviceService.GetDeviceByIdAsync(session.DeviceId);
            if (device != null)
            {
                deviceInfo = $"{device.Platform} - {device.DeviceModel} ({device.OsVersion})";
            }
        }

        int? durationSeconds = null;
        if (session.StartTime != default && session.EndTime.HasValue)
        {
            durationSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
        }

        return new SessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            UserId = session.UserId,
            UserName = session.User?.FirstName + " " + session.User?.LastName,
            DeviceId = session.DeviceId,
            DeviceInfo = deviceInfo,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            Status = session.Status,
            DurationSeconds = durationSeconds
        };
    }
}