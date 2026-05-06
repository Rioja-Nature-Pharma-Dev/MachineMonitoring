namespace MachineMonitoring.Application.UseCases.Production;

public sealed record FinishProductionOrderCommand(
    Guid ProductionOrderId,
    int GoodUnits,
    int BadUnits,
    int? FinalBoxCount,
    int? UnitsPerBox,
    string? BoxType,
    bool RequiresReprocess,
    bool RequiresManualProcess);