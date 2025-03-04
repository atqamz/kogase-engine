using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class PlaySession
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public string GameVersion { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string SessionProperties { get; set; } = "{}";
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    
    public virtual Project? Project { get; set; }
    public virtual User? User { get; set; }
    public virtual Device? Device { get; set; }
    public virtual ICollection<TelemetryEvent> Events { get; set; } = new List<TelemetryEvent>();
}

public enum SessionStatus
{
    Active,
    Ended,
    TimedOut,
    Crashed
}