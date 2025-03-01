using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class TelemetryEvent
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid? SessionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty; // JSON
    public DateTime Timestamp { get; set; }
    public DateTime ServerTimestamp { get; set; }
    public string AppVersion { get; set; } = string.Empty;

    public virtual Project Project { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Device Device { get; set; } = null!;
    public virtual Session? Session { get; set; }
}