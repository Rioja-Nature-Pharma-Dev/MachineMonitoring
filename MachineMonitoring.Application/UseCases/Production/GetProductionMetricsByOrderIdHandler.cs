using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class GetProductionMetricsByOrderIdHandler
{
    private readonly IProductionMetricsRepository _productionMetricsRepository;

    public GetProductionMetricsByOrderIdHandler(IProductionMetricsRepository productionMetricsRepository)
    {
        _productionMetricsRepository = productionMetricsRepository;
    }

    public async Task<ProductionMetricsDto?> HandleAsync(
        GetProductionMetricsByOrderIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _productionMetricsRepository.GetByOrderIdAsync(query.ProductionOrderId, cancellationToken);
        if (metrics is null)
            return null;

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