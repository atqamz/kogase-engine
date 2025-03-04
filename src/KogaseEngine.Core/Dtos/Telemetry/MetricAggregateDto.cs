using System.Text.Json.Serialization;
using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Core.Dtos.Telemetry;

public class MetricAggregateDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty;
    public string DimensionValue { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public AggregationPeriod Period { get; set; }
    public string PeriodString { get; set; } = string.Empty;
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public int Count { get; set; }
    public int UniqueCount { get; set; }
    public object? AdditionalData { get; set; }
}

public class UpsertMetricDto
{
    [JsonRequired]
    public Guid ProjectId { get; set; }
    
    [JsonRequired]
    public string MetricName { get; set; } = string.Empty;
    
    [JsonRequired]
    public string Dimension { get; set; } = string.Empty;
    
    [JsonRequired]
    public string DimensionValue { get; set; } = string.Empty;
    
    [JsonRequired]
    public AggregationPeriod Period { get; set; }
    
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public int Count { get; set; }
    public int UniqueCount { get; set; }
    public object? AdditionalData { get; set; }
}