using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class GetActiveProductionPauseHandler
{
    private readonly IProductionPauseRepository _productionPauseRepository;

    public GetActiveProductionPauseHandler(IProductionPauseRepository productionPauseRepository)
    {
        _productionPauseRepository = productionPauseRepository;
    }

    public async Task<ProductionPauseDto?> HandleAsync(
        GetActiveProductionPauseQuery query,
        CancellationToken cancellationToken = default)
    {
        var pause = await _productionPauseRepository.GetActiveByOrderIdAsync(query.ProductionOrderId, cancellationToken);
        if (pause is null)
            return null;

        return new ProductionPauseDto(
            pause.Id,
            pause.ProductionOrderId,
            pause.PauseType,
            pause.Description,
            pause.OperatorName,
            pause.CountsTowardsMetrics,
            pause.StartedAt,
            pause.EndedAt,
            pause.TotalMinutes);
    }
}