using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class GetActiveProductionOrderHandler
{
    private readonly IProductionOrderRepository _productionOrderRepository;

    public GetActiveProductionOrderHandler(IProductionOrderRepository productionOrderRepository)
    {
        _productionOrderRepository = productionOrderRepository;
    }

    public async Task<ProductionOrderDto?> HandleAsync(
        GetActiveProductionOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        var candidateStatuses = new[]
        {
            ProductionOrderStatus.InProgress,
            ProductionOrderStatus.Paused,
            ProductionOrderStatus.WaitingManualProcess,
            ProductionOrderStatus.ManualProcessInProgress
        };

        foreach (var status in candidateStatuses)
        {
            var orders = await _productionOrderRepository.GetByStatusAsync(status, cancellationToken);
            var order = orders
                .Where(o => o.MachineId == query.MachineId)
                .OrderByDescending(o => o.StartedAt ?? o.CreatedAt)
                .FirstOrDefault();

            if (order is not null)
            {
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

        return null;
    }
}