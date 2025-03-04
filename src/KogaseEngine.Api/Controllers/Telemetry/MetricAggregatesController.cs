using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Telemetry;
using KogaseEngine.Core.Services.Telemetry;
using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Api.Controllers.Telemetry;

[ApiController]
[Route("api/v1/telemetry/metrics")]
public class MetricAggregatesController : ControllerBase
{
    private readonly MetricAggregateService _metricService;

    public MetricAggregatesController(MetricAggregateService metricService)
    {
        _metricService = metricService;
    }

    [HttpPost]
    public async Task<ActionResult<MetricAggregateDto>> UpsertMetric(UpsertMetricDto upsertDto)
    {
        try
        {
            var metric = new MetricAggregate
            {
                ProjectId = upsertDto.ProjectId,
                MetricName = upsertDto.MetricName,
                Dimension = upsertDto.Dimension,
                DimensionValue = upsertDto.DimensionValue,
                Period = upsertDto.Period,
                Sum = upsertDto.Sum,
                Average = upsertDto.Average,
                Min = upsertDto.Min,
                Max = upsertDto.Max,
                Count = upsertDto.Count,
                UniqueCount = upsertDto.UniqueCount,
                AdditionalData = upsertDto.AdditionalData != null ? JsonSerializer.Serialize(upsertDto.AdditionalData) : "{}"
            };
            
            var result = await _metricService.UpsertMetricAsync(metric);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<MetricAggregateDto>>> UpsertMetrics([FromBody] IEnumerable<UpsertMetricDto> upsertDtos)
    {
        try
        {
            var metrics = new List<MetricAggregate>();
            
            foreach (var upsertDto in upsertDtos)
            {
                var metric = new MetricAggregate
                {
                    ProjectId = upsertDto.ProjectId,
                    MetricName = upsertDto.MetricName,
                    Dimension = upsertDto.Dimension,
                    DimensionValue = upsertDto.DimensionValue,
                    Period = upsertDto.Period,
                    Sum = upsertDto.Sum,
                    Average = upsertDto.Average,
                    Min = upsertDto.Min,
                    Max = upsertDto.Max,
                    Count = upsertDto.Count,
                    UniqueCount = upsertDto.UniqueCount,
                    AdditionalData = upsertDto.AdditionalData != null ? JsonSerializer.Serialize(upsertDto.AdditionalData) : "{}"
                };
                
                metrics.Add(metric);
            }
            
            var results = await _metricService.UpsertMetricsAsync(metrics);
            return Ok(results.Select(MapToDto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{metricId:guid}")]
    public async Task<ActionResult<MetricAggregateDto>> GetMetric(Guid metricId)
    {
        var metric = await _metricService.GetMetricByIdAsync(metricId);
        if (metric == null)
            return NotFound();
            
        return Ok(MapToDto(metric));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<MetricAggregateDto>>> GetMetricsByProject(
        Guid projectId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 100)
    {
        var metrics = await _metricService.GetMetricsByProjectIdAsync(projectId, page, pageSize);
        return Ok(metrics.Select(MapToDto));
    }

    [HttpGet("name/{projectId:guid}/{metricName}")]
    public async Task<ActionResult<IEnumerable<MetricAggregateDto>>> GetMetricsByName(
        Guid projectId, 
        string metricName)
    {
        var metrics = await _metricService.GetMetricsByNameAsync(projectId, metricName);
        return Ok(metrics.Select(MapToDto));
    }

    [HttpGet("dimension/{projectId:guid}/{dimension}/{dimensionValue}")]
    public async Task<ActionResult<IEnumerable<MetricAggregateDto>>> GetMetricsByDimension(
        Guid projectId, 
        string dimension, 
        string dimensionValue)
    {
        var metrics = await _metricService.GetMetricsByDimensionAsync(projectId, dimension, dimensionValue);
        return Ok(metrics.Select(MapToDto));
    }

    [HttpGet("period/{projectId:guid}/{period}")]
    public async Task<ActionResult<IEnumerable<MetricAggregateDto>>> GetMetricsByPeriod(
        Guid projectId, 
        AggregationPeriod period, 
        [FromQuery] DateTime start, 
        [FromQuery] DateTime end)
    {
        var metrics = await _metricService.GetMetricsByPeriodAsync(projectId, period, start, end);
        return Ok(metrics.Select(MapToDto));
    }

    [HttpGet("latest/{projectId:guid}/{metricName}/{dimension}")]
    public async Task<ActionResult<MetricAggregateDto>> GetLatestMetric(
        Guid projectId, 
        string metricName, 
        string dimension)
    {
        var metric = await _metricService.GetLatestMetricAsync(projectId, metricName, dimension);
        if (metric == null)
            return NotFound();
            
        return Ok(MapToDto(metric));
    }

    [HttpPost("calculate-daily/{projectId:guid}")]
    public async Task<ActionResult> CalculateDailyMetrics(
        Guid projectId, 
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var calculationDate = date ?? DateTime.UtcNow.Date;
            await _metricService.CalculateDailyMetricsAsync(projectId, calculationDate);
            return Ok(new { message = $"Daily metrics calculated for {calculationDate:yyyy-MM-dd}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private MetricAggregateDto MapToDto(MetricAggregate metric)
    {
        object? additionalData = null;
        try
        {
            if (!string.IsNullOrEmpty(metric.AdditionalData))
                additionalData = JsonSerializer.Deserialize<object>(metric.AdditionalData);
        }
        catch
        {
            // If deserialization fails, leave as null
        }
        
        return new MetricAggregateDto
        {
            Id = metric.Id,
            ProjectId = metric.ProjectId,
            MetricName = metric.MetricName,
            Dimension = metric.Dimension,
            DimensionValue = metric.DimensionValue,
            Timestamp = metric.Timestamp,
            Period = metric.Period,
            PeriodString = metric.Period.ToString(),
            Sum = metric.Sum,
            Average = metric.Average,
            Min = metric.Min,
            Max = metric.Max,
            Count = metric.Count,
            UniqueCount = metric.UniqueCount,
            AdditionalData = additionalData
        };
    }
}