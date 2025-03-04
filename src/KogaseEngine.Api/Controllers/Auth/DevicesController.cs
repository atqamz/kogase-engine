using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Auth;
using KogaseEngine.Core.Services.Auth;
using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Api.Controllers.Auth;

[ApiController]
[Route("api/v1/auth/devices")]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DevicesController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<DeviceDto>> RegisterDevice(RegisterDeviceDto registerDto)
    {
        try
        {
            // Parse platform enum
            if (!Enum.TryParse<DevicePlatform>(registerDto.Platform, true, out var platform))
            {
                return BadRequest(new { message = "Invalid platform value." });
            }

            var device = new Device
            {
                ProjectId = registerDto.ProjectId,
                InstallId = registerDto.InstallId,
                Platform = platform,
                DeviceModel = registerDto.DeviceModel,
                OsVersion = registerDto.OsVersion,
                AppVersion = registerDto.AppVersion,
                Status = DeviceStatus.Active,
                Metadata = registerDto.Metadata ?? "{}"
            };

            var result = await _deviceService.RegisterDeviceAsync(device);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<DeviceDto>> GetDevice(Guid deviceId)
    {
        var device = await _deviceService.GetDeviceByIdAsync(deviceId);
        if (device == null)
            return NotFound();

        return Ok(MapToDto(device));
    }

    [HttpPut("{deviceId:guid}")]
    public async Task<ActionResult> UpdateDevice(Guid deviceId, UpdateDeviceDto updateDto)
    {
        try
        {
            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound();

            device.DeviceModel = updateDto.DeviceModel;
            device.OsVersion = updateDto.OsVersion;
            device.AppVersion = updateDto.AppVersion;
            device.Status = updateDto.Status;
            device.Metadata = updateDto.Metadata ?? "{}";

            await _deviceService.UpdateDeviceAsync(device);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{deviceId:guid}")]
    public async Task<ActionResult> UnregisterDevice(Guid deviceId)
    {
        try
        {
            await _deviceService.UnregisterDeviceAsync(deviceId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private DeviceDto MapToDto(Device device)
    {
        object? metadata = null;
        try
        {
            if (!string.IsNullOrEmpty(device.Metadata))
            {
                metadata = JsonSerializer.Deserialize<object>(device.Metadata);
            }
        }
        catch
        {
            // If deserialization fails, just leave it as null
        }

        return new DeviceDto
        {
            Id = device.Id,
            ProjectId = device.ProjectId,
            InstallId = device.InstallId,
            Platform = device.Platform,
            PlatformString = device.Platform.ToString(),
            DeviceModel = device.DeviceModel,
            OsVersion = device.OsVersion,
            AppVersion = device.AppVersion,
            FirstSeenAt = device.FirstSeenAt,
            LastActiveAt = device.LastActiveAt,
            Status = device.Status,
            Metadata = metadata
        };
    }
}