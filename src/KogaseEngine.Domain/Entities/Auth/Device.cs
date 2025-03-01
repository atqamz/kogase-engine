using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Auth;

public class Device
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string InstallId { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DeviceStatus Status { get; set; }
    public string Metadata { get; set; } = string.Empty; // JSON

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
}

public enum DevicePlatform
{
    iOS,
    Android,
    Windows,
    macOS,
    Linux,
    WebGL
}

public enum DeviceStatus
{
    Active,
    Inactive
} 