using System;
using System.Text.Json;

namespace KogaseEngine.Domain.Entities.Telemetry;

public class MetricAggregate
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty; // e.g., "daily", "user_type", "country"
    public string DimensionValue { get; set; } = string.Empty; // e.g., "2023-03-01", "premium", "US"
    public DateTime Timestamp { get; set; }
    public AggregationPeriod Period { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public int Count { get; set; }
    public int UniqueCount { get; set; }
    public string AdditionalData { get; set; } = "{}";
    
    public T? GetAdditionalData<T>()
    {
        if (string.IsNullOrEmpty(AdditionalData))
            return default;
            
        try
        {
            return JsonSerializer.Deserialize<T>(AdditionalData);
        }
        catch
        {
            return default;
        }
    }
}

public enum AggregationPeriod
{
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Yearly,
    Total
}