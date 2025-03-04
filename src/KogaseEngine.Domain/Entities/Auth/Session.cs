using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Auth;

public class Session
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid DeviceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }

    public virtual Project? Project { get; set; }
    public virtual User? User { get; set; }
    public virtual Device? Device { get; set; }
}

public enum SessionStatus
{
    Active,
    Ended,
    Expired,
    Terminated
}