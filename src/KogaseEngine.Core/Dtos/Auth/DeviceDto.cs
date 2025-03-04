using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Core.Dtos.Auth;

public class DeviceDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string InstallId { get; set; } = string.Empty;
    public string PlatformString { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DeviceStatus Status { get; set; }
    public object? Metadata { get; set; }
}

public class RegisterDeviceDto
{
    public Guid ProjectId { get; set; }
    public string InstallId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}

public class UpdateDeviceDto
{
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; }
    public string? Metadata { get; set; }
}