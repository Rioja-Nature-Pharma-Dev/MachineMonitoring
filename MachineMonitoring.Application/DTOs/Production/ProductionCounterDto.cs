namespace MachineMonitoring.Application.DTOs.Production;

public sealed record ProductionCounterDto(
    Guid Id,
    Guid ProductionOrderId,
    int Quantity,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdatedAt,
    DateTimeOffset? LastCountedAt);