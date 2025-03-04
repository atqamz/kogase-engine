using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Core.Dtos.Auth;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceInfo { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public int? DurationSeconds { get; set; }
}

public class StartSessionDto
{
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid DeviceId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}