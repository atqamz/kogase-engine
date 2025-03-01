using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Entities.Auth;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class PlaySession
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? Duration { get; set; } // in seconds
    public string PlayData { get; set; } = string.Empty; // JSON

    public virtual Project Project { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual Device Device { get; set; } = null!;
    public virtual Session Session { get; set; } = null!;
}