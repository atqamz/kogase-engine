using System.Text.Json;
using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class EventDefinition
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Schema { get; set; } = "{}"; // JSON Schema for validation
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Project? Project { get; set; }

    public T? GetSchema<T>()
    {
        if (string.IsNullOrEmpty(Schema))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(Schema);
        }
        catch
        {
            return default;
        }
    }
}