namespace MachineMonitoring.Application.UseCases.Production;

public sealed record GetActiveProductionPauseQuery(Guid ProductionOrderId);