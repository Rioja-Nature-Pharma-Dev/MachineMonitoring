using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Application.UseCases.Production;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionPauseRepository _productionPauseRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IProductionMetricsRepository _productionMetricsRepository;
    private readonly IClock _clock;

    public MetricsController(
        IProductionOrderRepository productionOrderRepository,
        IProductionPauseRepository productionPauseRepository,
        IProductionCounterRepository productionCounterRepository,
        IProductionMetricsRepository productionMetricsRepository,
        IClock clock)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionPauseRepository = productionPauseRepository;
        _productionCounterRepository = productionCounterRepository;
        _productionMetricsRepository = productionMetricsRepository;
        _clock = clock;
    }

    /// <summary>
    /// Calcula las métricas de OEE para una orden de producción
    /// </summary>
    [HttpPost("{orderId}/calculate")]
    public async Task<ActionResult<ProductionMetricsDto>> CalculateMetrics(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return BadRequest($"Orden de producción con ID {orderId} no encontrada");

        var handler = new CalculateProductionMetricsHandler(_productionOrderRepository, _productionPauseRepository, _productionMetricsRepository);
        var command = new CalculateProductionMetricsCommand(orderId);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene las métricas de una orden de producción
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<ProductionMetricsDto>> GetMetrics(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound($"Orden de producción con ID {orderId} no encontrada");

        var handler = new GetProductionMetricsByOrderIdHandler(_productionMetricsRepository);
        var query = new GetProductionMetricsByOrderIdQuery(orderId);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound($"Métricas no encontradas para la orden {orderId}. Las métricas se calculan al finalizar la orden.");

        return Ok(result);
    }

    /// <summary>
    /// Obtiene las métricas de múltiples órdenes
    /// </summary>
    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<ProductionMetricsDto>>> GetMachineMetrics(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        // Check if machine exists
        var machineRepository = HttpContext.RequestServices.GetService(typeof(IMachineRepository)) as IMachineRepository;
        if (machineRepository != null)
        {
            var machine = await machineRepository.GetByIdAsync(machineId, cancellationToken);
            if (machine == null)
                return NotFound($"Máquina con ID {machineId} no encontrada");
        }

        // Get all orders for the machine
        var orders = await _productionOrderRepository.GetAllAsync(cancellationToken);
        var machineOrders = orders.Where(o => o.MachineId == machineId).ToList();

        if (!machineOrders.Any())
            return Ok(new List<ProductionMetricsDto>());

        var metrics = new List<ProductionMetricsDto>();
        var handler = new GetProductionMetricsByOrderIdHandler(_productionMetricsRepository);

        foreach (var order in machineOrders)
        {
            var query = new GetProductionMetricsByOrderIdQuery(order.Id);
            var result = await handler.HandleAsync(query, cancellationToken);
            if (result != null)
                metrics.Add(result);
        }

        return Ok(metrics);
    }

    /// <summary>
    /// Obtiene un resumen de métricas
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<MetricsSummary>> GetMetricsSummary(
        CancellationToken cancellationToken = default)
    {
        var allOrders = await _productionOrderRepository.GetAllAsync(cancellationToken);
        var handler = new GetProductionMetricsByOrderIdHandler(_productionMetricsRepository);

        decimal totalOee = 0;
        decimal totalAvailability = 0;
        decimal totalPerformance = 0;
        decimal totalQuality = 0;
        int metricsCount = 0;

        foreach (var order in allOrders)
        {
            var query = new GetProductionMetricsByOrderIdQuery(order.Id);
            var metrics = await handler.HandleAsync(query, cancellationToken);
            if (metrics != null)
            {
                totalOee += metrics.Oee ?? 0;
                totalAvailability += metrics.Availability ?? 0;
                totalPerformance += metrics.Performance ?? 0;
                totalQuality += metrics.Quality ?? 0;
                metricsCount++;
            }
        }

        if (metricsCount == 0)
            return Ok(new MetricsSummary(0, 0, 0, 0, 0));

        var summary = new MetricsSummary(
            AverageOee: totalOee / metricsCount,
            AverageAvailability: totalAvailability / metricsCount,
            AveragePerformance: totalPerformance / metricsCount,
            AverageQuality: totalQuality / metricsCount,
            TotalOrdersWithMetrics: metricsCount);

        return Ok(summary);
    }
}

public sealed record MetricsSummary(
    decimal AverageOee,
    decimal AverageAvailability,
    decimal AveragePerformance,
    decimal AverageQuality,
    int TotalOrdersWithMetrics);
