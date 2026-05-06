using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class GetProductionCounterByOrderIdHandler
{
    private readonly IProductionCounterRepository _productionCounterRepository;

    public GetProductionCounterByOrderIdHandler(IProductionCounterRepository productionCounterRepository)
    {
        _productionCounterRepository = productionCounterRepository;
    }

    public async Task<ProductionCounterDto?> HandleAsync(
        GetProductionCounterByOrderIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var counter = await _productionCounterRepository.GetByOrderIdAsync(query.ProductionOrderId, cancellationToken);
        if (counter is null)
            return null;

        return new ProductionCounterDto(
            counter.Id,
            counter.ProductionOrderId,
            counter.Quantity,
            counter.IsActive,
            counter.CreatedAt,
            counter.LastUpdatedAt,
            counter.LastCountedAt);
    }
}