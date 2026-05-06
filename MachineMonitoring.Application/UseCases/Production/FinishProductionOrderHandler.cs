using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class FinishProductionOrderHandler
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionPauseRepository _productionPauseRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly CalculateProductionMetricsHandler _calculateProductionMetricsHandler;
    private readonly IClock _clock;

    public FinishProductionOrderHandler(
        IProductionOrderRepository productionOrderRepository,
        IProductionPauseRepository productionPauseRepository,
        IProductionCounterRepository productionCounterRepository,
        CalculateProductionMetricsHandler calculateProductionMetricsHandler,
        IClock clock)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionPauseRepository = productionPauseRepository;
        _productionCounterRepository = productionCounterRepository;
        _calculateProductionMetricsHandler = calculateProductionMetricsHandler;
        _clock = clock;
    }

    public async Task<ProductionOrderDto> HandleAsync(
        FinishProductionOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(command.ProductionOrderId, cancellationToken);
        if (order is null)
            throw new InvalidOperationException("Production order not found.");

        if (order.Status != ProductionOrderStatus.InProgress &&
            order.Status != ProductionOrderStatus.Paused)
        {
            throw new InvalidOperationException("Only an order in progress or paused can be finished.");
        }

        if (order.Status == ProductionOrderStatus.Paused)
        {
            var activePause = await _productionPauseRepository.GetActiveByOrderIdAsync(order.Id, cancellationToken);
            if (activePause is not null)
            {
                activePause.Finish(_clock.UtcNow);
                await _productionPauseRepository.UpdateAsync(activePause, cancellationToken);
            }

            order.Resume();
        }

        order.SetFinalProductionData(
            command.GoodUnits,
            command.BadUnits,
            command.FinalBoxCount,
            command.UnitsPerBox,
            command.BoxType,
            command.RequiresReprocess,
            command.RequiresManualProcess);

        if (command.RequiresManualProcess)
        {
            order.MarkWaitingManualProcess(true);
        }
        else
        {
            order.Finish(_clock.UtcNow);
        }

        await _productionOrderRepository.UpdateAsync(order, cancellationToken);

        var counter = await _productionCounterRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        if (counter is not null && counter.IsActive)
        {
            counter.Deactivate(_clock.UtcNow);
            await _productionCounterRepository.UpdateAsync(counter, cancellationToken);
        }

        await _calculateProductionMetricsHandler.HandleAsync(
            new CalculateProductionMetricsCommand(order.Id),
            cancellationToken);

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