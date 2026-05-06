using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.DTOs.Production;

namespace MachineMonitoring.Application.UseCases.Production;

public sealed class GetProductionOrderByIdHandler
{
    private readonly IProductionOrderRepository _productionOrderRepository;

    public GetProductionOrderByIdHandler(IProductionOrderRepository productionOrderRepository)
    {
        _productionOrderRepository = productionOrderRepository;
    }

    public async Task<ProductionOrderDto?> HandleAsync(
        GetProductionOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(query.ProductionOrderId, cancellationToken);
        if (order is null)
            return null;

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