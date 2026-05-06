namespace MachineMonitoring.Application.DTOs.Production;

public sealed record ProductionMetricsDto(
    Guid Id,
    Guid ProductionOrderId,
    decimal? TotalMinutes,
    decimal? PausedMinutes,
    decimal? ActiveMinutes,
    decimal? Availability,
    decimal? Performance,
    decimal? Quality,
    decimal? Oee,
    decimal? RealStandard,
    decimal? OrderFulfillment);