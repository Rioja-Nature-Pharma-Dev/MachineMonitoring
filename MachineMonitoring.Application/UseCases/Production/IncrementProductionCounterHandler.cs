using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class IncrementProductionCounterHandler
{
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;

    public IncrementProductionCounterHandler(
        IProductionCounterRepository productionCounterRepository,
        IClock clock)
    {
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
    }

    public async Task<ProductionCounterDto> HandleAsync(
        IncrementProductionCounterCommand command,
        CancellationToken cancellationToken = default)
    {
        var counter = await _productionCounterRepository.GetByOrderIdAsync(command.ProductionOrderId, cancellationToken);

        if (counter is null)
        {
            counter = new ProductionCounter(
                Guid.NewGuid(),
                command.ProductionOrderId,
                0,
                true,
                _clock.UtcNow,
                _clock.UtcNow,
                null);

            await _productionCounterRepository.AddAsync(counter, cancellationToken);
        }

        counter.Increment(_clock.UtcNow);
        await _productionCounterRepository.UpdateAsync(counter, cancellationToken);

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