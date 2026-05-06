using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class CalculateProductionMetricsHandler
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionPauseRepository _productionPauseRepository;
    private readonly IProductionMetricsRepository _productionMetricsRepository;

    public CalculateProductionMetricsHandler(
        IProductionOrderRepository productionOrderRepository,
        IProductionPauseRepository productionPauseRepository,
        IProductionMetricsRepository productionMetricsRepository)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionPauseRepository = productionPauseRepository;
        _productionMetricsRepository = productionMetricsRepository;
    }

    public async Task<ProductionMetricsDto> HandleAsync(
        CalculateProductionMetricsCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(command.ProductionOrderId, cancellationToken);
        if (order is null)
            throw new InvalidOperationException("Production order not found.");

        if (order.StartedAt is null)
            throw new InvalidOperationException("Cannot calculate metrics for an order without a start date.");

        if (order.Status != ProductionOrderStatus.Finished &&
            order.Status != ProductionOrderStatus.WaitingManualProcess &&
            order.Status != ProductionOrderStatus.ManualProcessInProgress)
        {
            throw new InvalidOperationException("Metrics can only be calculated for orders that have left automatic production.");
        }

        var existingMetrics = await _productionMetricsRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        if (existingMetrics is not null)
        {
            await _productionMetricsRepository.DeleteByOrderIdAsync(order.Id, cancellationToken);
        }

        var effectiveEnd = order.FinishedAt ?? DateTimeOffset.UtcNow;

        var totalElapsedMinutes = Convert.ToDecimal((effectiveEnd - order.StartedAt.Value).TotalMinutes);

        var pauses = await _productionPauseRepository.GetByOrderIdAsync(order.Id, cancellationToken);

        decimal nonComputedPauseMinutes = pauses
            .Where(p => !p.IsActive && p.TotalMinutes.HasValue && p.CountsTowardsMetrics == false)
            .Sum(p => p.TotalMinutes!.Value);

        decimal computedPauseMinutes = pauses
            .Where(p => !p.IsActive && p.TotalMinutes.HasValue && p.CountsTowardsMetrics == true)
            .Sum(p => p.TotalMinutes!.Value);

        var totalMinutes = totalElapsedMinutes - nonComputedPauseMinutes;
        if (totalMinutes < 0)
            totalMinutes = 0;

        var activeMinutes = totalMinutes - computedPauseMinutes;
        if (activeMinutes <= 0)
            activeMinutes = 1;

        var availability = totalMinutes > 0
            ? activeMinutes / totalMinutes
            : 0m;

        var totalProduced = order.GoodUnits + order.BadUnits;

        var expectedProduction = (order.StandardReference ?? 0m) * activeMinutes;
        var performance = expectedProduction > 0
            ? totalProduced / expectedProduction
            : 0m;

        var quality = totalProduced > 0
            ? (decimal)order.GoodUnits / totalProduced
            : 0m;

        var oee = availability * performance * quality;

        var realStandard = activeMinutes > 0
            ? totalProduced / activeMinutes
            : 0m;

        var orderFulfillment = order.PlannedQuantity > 0
            ? (decimal)order.GoodUnits / order.PlannedQuantity
            : 0m;

        var metrics = new ProductionMetrics(
            Guid.NewGuid(),
            order.Id,
            totalMinutes,
            computedPauseMinutes,
            activeMinutes,
            availability,
            performance,
            quality,
            oee,
            realStandard,
            orderFulfillment);

        await _productionMetricsRepository.AddAsync(metrics, cancellationToken);

        return new ProductionMetricsDto(
            metrics.Id,
            metrics.ProductionOrderId,
            metrics.TotalMinutes,
            metrics.PausedMinutes,
            metrics.ActiveMinutes,
            metrics.Availability,
            metrics.Performance,
            metrics.Quality,
            metrics.Oee,
            metrics.RealStandard,
            metrics.OrderFulfillment);
    }
}