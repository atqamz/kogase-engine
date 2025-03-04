using KogaseEngine.Domain.Entities.Telemetry;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Telemetry;

namespace KogaseEngine.Core.Services.Telemetry;

public class MetricAggregateService
{
    private readonly IMetricAggregateRepository _metricRepository;
    private readonly ITelemetryEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MetricAggregateService(
        IMetricAggregateRepository metricRepository,
        ITelemetryEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _metricRepository = metricRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MetricAggregate> UpsertMetricAsync(MetricAggregate metric)
    {
        var existingMetric = await _metricRepository.GetLatestMetricAsync(
            metric.ProjectId, 
            metric.MetricName, 
            metric.Dimension);
            
        if (existingMetric != null)
        {
            // Update the existing metric
            existingMetric.Sum = metric.Sum;
            existingMetric.Average = metric.Average;
            existingMetric.Min = metric.Min;
            existingMetric.Max = metric.Max;
            existingMetric.Count = metric.Count;
            existingMetric.UniqueCount = metric.UniqueCount;
            existingMetric.AdditionalData = metric.AdditionalData;
            existingMetric.Timestamp = DateTime.UtcNow;
            
            await _metricRepository.UpdateAsync(existingMetric);
            await _unitOfWork.SaveChangesAsync();
            
            return existingMetric;
        }
        else
        {
            // Create a new metric
            metric.Id = Guid.NewGuid();
            metric.Timestamp = DateTime.UtcNow;
            
            await _metricRepository.CreateAsync(metric);
            await _unitOfWork.SaveChangesAsync();
            
            return metric;
        }
    }

    public async Task<IEnumerable<MetricAggregate>> UpsertMetricsAsync(IEnumerable<MetricAggregate> metrics)
    {
        var metricsList = metrics.ToList();
        await _metricRepository.BatchUpsertMetricsAsync(metricsList);
        await _unitOfWork.SaveChangesAsync();
        
        return metricsList;
    }

    public async Task<MetricAggregate?> GetMetricByIdAsync(Guid metricId)
    {
        return await _metricRepository.GetByIdAsync(metricId);
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 100)
    {
        return await _metricRepository.GetMetricsByProjectIdAsync(projectId, page, pageSize);
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByNameAsync(Guid projectId, string metricName)
    {
        return await _metricRepository.GetMetricsByNameAsync(projectId, metricName);
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByDimensionAsync(Guid projectId, string dimension, string dimensionValue)
    {
        return await _metricRepository.GetMetricsByDimensionAsync(projectId, dimension, dimensionValue);
    }

    public async Task<IEnumerable<MetricAggregate>> GetMetricsByPeriodAsync(Guid projectId, AggregationPeriod period, DateTime start, DateTime end)
    {
        return await _metricRepository.GetMetricsByPeriodAsync(projectId, period, start, end);
    }

    public async Task<MetricAggregate?> GetLatestMetricAsync(Guid projectId, string metricName, string dimension)
    {
        return await _metricRepository.GetLatestMetricAsync(projectId, metricName, dimension);
    }

    public async Task CalculateDailyMetricsAsync(Guid projectId, DateTime date)
    {
        // Get start and end of the day
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);
        
        // Get all events for the day
        var events = await _eventRepository.GetEventsByTimeRangeAsync(
            projectId, 
            startOfDay, 
            endOfDay, 
            1, 
            int.MaxValue);
            
        var eventsList = events.ToList();
        if (!eventsList.Any())
            return;
            
        // Calculate common metrics
        var dailyActiveUsers = eventsList
            .Where(e => e.UserId.HasValue)
            .Select(e => e.UserId!.Value)
            .Distinct()
            .Count();
            
        var dailyActiveSessions = eventsList
            .Where(e => e.SessionId.HasValue)
            .Select(e => e.SessionId!.Value)
            .Distinct()
            .Count();
            
        var dailyEventCount = eventsList.Count;
        
        // Create daily metrics
        var metrics = new List<MetricAggregate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MetricName = "daily_active_users",
                Dimension = "date",
                DimensionValue = startOfDay.ToString("yyyy-MM-dd"),
                Timestamp = endOfDay,
                Period = AggregationPeriod.Daily,
                Sum = dailyActiveUsers,
                Average = dailyActiveUsers,
                Min = dailyActiveUsers,
                Max = dailyActiveUsers,
                Count = 1,
                UniqueCount = dailyActiveUsers
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MetricName = "daily_active_sessions",
                Dimension = "date",
                DimensionValue = startOfDay.ToString("yyyy-MM-dd"),
                Timestamp = endOfDay,
                Period = AggregationPeriod.Daily,
                Sum = dailyActiveSessions,
                Average = dailyActiveSessions,
                Min = dailyActiveSessions,
                Max = dailyActiveSessions,
                Count = 1,
                UniqueCount = dailyActiveSessions
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MetricName = "daily_event_count",
                Dimension = "date",
                DimensionValue = startOfDay.ToString("yyyy-MM-dd"),
                Timestamp = endOfDay,
                Period = AggregationPeriod.Daily,
                Sum = dailyEventCount,
                Average = dailyEventCount,
                Min = dailyEventCount,
                Max = dailyEventCount,
                Count = 1,
                UniqueCount = 1
            }
        };
        
        // Group events by name to calculate event-specific metrics
        var eventsByName = eventsList.GroupBy(e => e.EventName);
        foreach (var eventGroup in eventsByName)
        {
            var eventName = eventGroup.Key;
            var count = eventGroup.Count();
            
            metrics.Add(new MetricAggregate
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MetricName = $"event_{eventName}_count",
                Dimension = "date",
                DimensionValue = startOfDay.ToString("yyyy-MM-dd"),
                Timestamp = endOfDay,
                Period = AggregationPeriod.Daily,
                Sum = count,
                Average = count,
                Min = count,
                Max = count,
                Count = 1,
                UniqueCount = 1
            });
        }
        
        await _metricRepository.BatchUpsertMetricsAsync(metrics);
        await _unitOfWork.SaveChangesAsync();
    }
}