using System.Text.Json.Serialization;
using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Core.Dtos.Telemetry;

public class PlaySessionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? DeviceId { get; set; }
    public string? DeviceInfo { get; set; }
    public string GameVersion { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public object? SessionProperties { get; set; }
    public SessionStatus Status { get; set; }
    public string StatusString { get; set; } = string.Empty;
    public int EventCount { get; set; }
}

public class StartPlaySessionDto
{
    [JsonRequired]
    public Guid ProjectId { get; set; }
    
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public string GameVersion { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public object? SessionProperties { get; set; }
}

public class UpdateSessionStatusDto
{
    [JsonRequired]
    public SessionStatus Status { get; set; }
}