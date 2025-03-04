using System.Text.Json;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class TelemetryEvent
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public Guid? SessionId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Payload { get; set; } = "{}";
    public string Parameters { get; set; } = "{}";
    public string ClientInfo { get; set; } = "{}";
    
    public virtual PlaySession? Session { get; set; }

    public T? GetPayload<T>()
    {
        if (string.IsNullOrEmpty(Payload))
            return default;
            
        try
        {
            return JsonSerializer.Deserialize<T>(Payload);
        }
        catch
        {
            return default;
        }
    }
    
    public T? GetParameters<T>()
    {
        if (string.IsNullOrEmpty(Parameters))
            return default;
            
        try
        {
            return JsonSerializer.Deserialize<T>(Parameters);
        }
        catch
        {
            return default;
        }
    }
    
    public T? GetClientInfo<T>()
    {
        if (string.IsNullOrEmpty(ClientInfo))
            return default;
            
        try
        {
            return JsonSerializer.Deserialize<T>(ClientInfo);
        }
        catch
        {
            return default;
        }
    }
}