namespace MachineMonitoring.Application.UseCases.Production;

public sealed record CreateProductionOrderCommand(
    Guid MachineId,
    string OrderCode,
    string? OperatorName,
    string? Batch,
    string? Article,
    string? Description,
    int PlannedQuantity,
    int? UnitsPerBox,
    string? BoxType,
    bool RequiresReprocess,
    bool RequiresManualProcess,
    int? FinalBoxCount,
    string? BottleFormat,
    string? ProductType,
    int? UnitsPerBottle,
    decimal? StandardReference,
    decimal? EstimatedMinutes);