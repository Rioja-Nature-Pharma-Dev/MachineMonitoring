using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.DTOs.Production;

public sealed record ProductionOrderDto(
    Guid Id,
    Guid MachineId,
    string OrderCode,
    string? OperatorName,
    string? Batch,
    string? Article,
    string? Description,
    ProductionOrderStatus Status,
    int PlannedQuantity,
    int GoodUnits,
    int BadUnits,
    int? UnitsPerBox,
    string? BoxType,
    bool RequiresReprocess,
    bool RequiresManualProcess,
    int? FinalBoxCount,
    string? BottleFormat,
    string? ProductType,
    int? UnitsPerBottle,
    decimal? StandardReference,
    decimal? EstimatedMinutes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);