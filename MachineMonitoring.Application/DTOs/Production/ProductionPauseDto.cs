using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.DTOs.Production;

public sealed record ProductionPauseDto(
    Guid Id,
    Guid ProductionOrderId,
    PauseType? PauseType,
    string? Description,
    string? OperatorName,
    bool? CountsTowardsMetrics,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    decimal? TotalMinutes);