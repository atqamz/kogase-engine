using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class MetricAggregate
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? Dimension { get; set; }
    public string? DimensionValue { get; set; }
    public decimal Value { get; set; }
    public DateOnly Date { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
} 