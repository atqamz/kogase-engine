using System.Text.Json.Serialization;

namespace KogaseEngine.Core.Dtos.Telemetry;

public class EventDefinitionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public object? Schema { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEventDefinitionDto
{
    [JsonRequired]
    public Guid ProjectId { get; set; }
    
    [JsonRequired]
    public string EventName { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public object? Schema { get; set; }
}

public class UpdateEventDefinitionDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public object? Schema { get; set; }
}