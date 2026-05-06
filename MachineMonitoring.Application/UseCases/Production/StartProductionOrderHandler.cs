using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class StartProductionOrderHandler
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;

    public StartProductionOrderHandler(
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IClock clock)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
    }

    public async Task<ProductionOrderDto> HandleAsync(
        StartProductionOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(command.ProductionOrderId, cancellationToken);
        if (order is null)
            throw new InvalidOperationException("Production order not found.");

        order.Start(_clock.UtcNow);
        await _productionOrderRepository.UpdateAsync(order, cancellationToken);

        var counter = await _productionCounterRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        if (counter is null)
        {
            counter = new ProductionCounter(
                Guid.NewGuid(),
                order.Id,
                0,
                true,
                _clock.UtcNow,
                _clock.UtcNow,
                null);

            await _productionCounterRepository.AddAsync(counter, cancellationToken);
        }
        else
        {
            counter.Activate(_clock.UtcNow);
            await _productionCounterRepository.UpdateAsync(counter, cancellationToken);
        }

        return new ProductionOrderDto(
            order.Id,
            order.MachineId,
            order.OrderCode,
            order.OperatorName,
            order.Batch,
            order.Article,
            order.Description,
            order.Status,
            order.PlannedQuantity,
            order.GoodUnits,
            order.BadUnits,
            order.UnitsPerBox,
            order.BoxType,
            order.RequiresReprocess,
            order.RequiresManualProcess,
            order.FinalBoxCount,
            order.BottleFormat,
            order.ProductType,
            order.UnitsPerBottle,
            order.StandardReference,
            order.EstimatedMinutes,
            order.CreatedAt,
            order.StartedAt,
            order.FinishedAt);
    }
}