using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class CreateProductionOrderHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IClock _clock;

    public CreateProductionOrderHandler(
        IMachineRepository machineRepository,
        IProductionOrderRepository productionOrderRepository,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _productionOrderRepository = productionOrderRepository;
        _clock = clock;
    }

    public async Task<ProductionOrderDto> HandleAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByIdAsync(command.MachineId, cancellationToken);
        if (machine is null)
            throw new InvalidOperationException("Machine not found.");

        var exists = await _productionOrderRepository.ExistsByOrderCodeAsync(command.OrderCode, cancellationToken);
        if (exists)
            throw new InvalidOperationException("A production order with the same code already exists.");

        var order = new ProductionOrder(
            Guid.NewGuid(),
            command.MachineId,
            command.OrderCode,
            command.OperatorName,
            command.Batch,
            command.Article,
            command.Description,
            command.PlannedQuantity,
            command.UnitsPerBox,
            command.BoxType,
            command.RequiresReprocess,
            command.RequiresManualProcess,
            command.FinalBoxCount,
            command.BottleFormat,
            command.ProductType,
            command.UnitsPerBottle,
            command.StandardReference,
            command.EstimatedMinutes,
            _clock.UtcNow);

        await _productionOrderRepository.AddAsync(order, cancellationToken);

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