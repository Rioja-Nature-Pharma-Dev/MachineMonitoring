namespace MachineMonitoring.Domain.Enums;

public enum ProductionOrderStatus
{
    Created = 0,
    InProgress = 1,
    Paused = 2,
    WaitingManualProcess = 3,
    ManualProcessInProgress = 4,
    Finished = 5,
    Cancelled = 6
}