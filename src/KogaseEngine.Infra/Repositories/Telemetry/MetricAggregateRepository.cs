using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces.Telemetry;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Telemetry;

public class MetricAggregateRepository : Repository<MetricAggregate>, IMetricAggregateRepository
{
    public MetricAggregateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100)
    {
        return await _context.MetricAggregates
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByNameAsync(Guid projectId, string metricName)
    {
        return await _context.MetricAggregates
            .Where(m => m.ProjectId == projectId && m.MetricName == metricName)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByDimensionAsync(Guid projectId, string dimension, string dimensionValue)
    {
        return await _context.MetricAggregates
            .Where(m => m.ProjectId == projectId && m.Dimension == dimension && m.DimensionValue == dimensionValue)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByPeriodAsync(Guid projectId, AggregationPeriod period, DateTime start, DateTime end)
    {
        return await _context.MetricAggregates
            .Where(m => m.ProjectId == projectId && m.Period == period && m.Timestamp >= start && m.Timestamp <= end)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<MetricAggregate?> GetLatestMetricAsync(Guid projectId, string metricName, string dimension)
    {
        return await _context.MetricAggregates
            .Where(m => m.ProjectId == projectId && m.MetricName == metricName && m.Dimension == dimension)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task BatchUpsertMetricsAsync(IEnumerable<MetricAggregate> metrics)
    {
        // For each metric, check if it exists by project, name, dimension, dimension value, and timestamp
        foreach (var metric in metrics)
        {
            var existingMetric = await _context.MetricAggregates
                .FirstOrDefaultAsync(m => 
                    m.ProjectId == metric.ProjectId && 
                    m.MetricName == metric.MetricName && 
                    m.Dimension == metric.Dimension && 
                    m.DimensionValue == metric.DimensionValue && 
                    m.Timestamp == metric.Timestamp &&
                    m.Period == metric.Period);

            if (existingMetric != null)
            {
                // Update existing metric
                existingMetric.Sum = metric.Sum;
                existingMetric.Average = metric.Average;
                existingMetric.Min = metric.Min;
                existingMetric.Max = metric.Max;
                existingMetric.Count = metric.Count;
                existingMetric.UniqueCount = metric.UniqueCount;
                existingMetric.AdditionalData = metric.AdditionalData;
            }
            else
            {
                // Insert new metric
                await _context.MetricAggregates.AddAsync(metric);
            }
        }
    }
}