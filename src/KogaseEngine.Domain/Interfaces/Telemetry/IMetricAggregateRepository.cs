using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Domain.Interfaces.Telemetry;

public interface IMetricAggregateRepository : IRepository<MetricAggregate>
{
    Task<IEnumerable<MetricAggregate>> GetMetricsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100);
    Task<IEnumerable<MetricAggregate>> GetMetricsByNameAsync(Guid projectId, string metricName);
    Task<IEnumerable<MetricAggregate>> GetMetricsByDimensionAsync(Guid projectId, string dimension, string dimensionValue);
    Task<IEnumerable<MetricAggregate>> GetMetricsByPeriodAsync(Guid projectId, AggregationPeriod period, DateTime start, DateTime end);
    Task<MetricAggregate?> GetLatestMetricAsync(Guid projectId, string metricName, string dimension);
    Task BatchUpsertMetricsAsync(IEnumerable<MetricAggregate> metrics);
}