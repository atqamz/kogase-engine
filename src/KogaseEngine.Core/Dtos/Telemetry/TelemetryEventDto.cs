using System.Text.Json.Serialization;

namespace KogaseEngine.Core.Dtos.Telemetry;

public class TelemetryEventDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public Guid? SessionId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Payload { get; set; }
    public object? Parameters { get; set; }
    public object? ClientInfo { get; set; }
}

public class LogEventDto
{
    [JsonRequired]
    public Guid ProjectId { get; set; }
    
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public Guid? SessionId { get; set; }
    
    [JsonRequired]
    public string EventName { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public object? Parameters { get; set; }
    public object? ClientInfo { get; set; }
}

public class BatchLogEventDto
{
    [JsonRequired]
    public Guid ProjectId { get; set; }
    
    [JsonRequired]
    public LogEventDto[] Events { get; set; } = [];
}