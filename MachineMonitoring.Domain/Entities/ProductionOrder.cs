using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Domain.Entities;

public sealed class ProductionOrder
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string OrderCode { get; private set; }
    public string? OperatorName { get; private set; }
    public string? Batch { get; private set; }
    public string? Article { get; private set; }
    public string? Description { get; private set; }
    public ProductionOrderStatus Status { get; private set; }
    public int PlannedQuantity { get; private set; }
    public int GoodUnits { get; private set; }
    public int BadUnits { get; private set; }

    public int? UnitsPerBox { get; private set; }
    public string? BoxType { get; private set; }
    public bool RequiresReprocess { get; private set; }
    public bool RequiresManualProcess { get; private set; }
    public int? FinalBoxCount { get; private set; }

    public string? BottleFormat { get; private set; }
    public string? ProductType { get; private set; }
    public int? UnitsPerBottle { get; private set; }

    public decimal? StandardReference { get; private set; }
    public decimal? EstimatedMinutes { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }

    public ProductionOrder(
        Guid id,
        Guid machineId,
        string orderCode,
        string? operatorName,
        string? batch,
        string? article,
        string? description,
        int plannedQuantity,
        int? unitsPerBox,
        string? boxType,
        bool requiresReprocess,
        bool requiresManualProcess,
        int? finalBoxCount,
        string? bottleFormat,
        string? productType,
        int? unitsPerBottle,
        decimal? standardReference,
        decimal? estimatedMinutes,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Order code cannot be empty.", nameof(orderCode));

        if (plannedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(plannedQuantity), "Planned quantity cannot be negative.");

        if (unitsPerBox.HasValue && unitsPerBox.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(unitsPerBox), "Units per box cannot be negative.");

        if (finalBoxCount.HasValue && finalBoxCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(finalBoxCount), "Final box count cannot be negative.");

        if (unitsPerBottle.HasValue && unitsPerBottle.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(unitsPerBottle), "Units per bottle cannot be negative.");

        if (standardReference.HasValue && standardReference.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(standardReference), "Standard reference cannot be negative.");

        if (estimatedMinutes.HasValue && estimatedMinutes.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedMinutes), "Estimated minutes cannot be negative.");

        Id = id;
        MachineId = machineId;
        OrderCode = orderCode.Trim().ToUpperInvariant();
        OperatorName = string.IsNullOrWhiteSpace(operatorName) ? null : operatorName.Trim();
        Batch = string.IsNullOrWhiteSpace(batch) ? null : batch.Trim();
        Article = string.IsNullOrWhiteSpace(article) ? null : article.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = ProductionOrderStatus.Created;
        PlannedQuantity = plannedQuantity;
        GoodUnits = 0;
        BadUnits = 0;

        UnitsPerBox = unitsPerBox;
        BoxType = string.IsNullOrWhiteSpace(boxType) ? null : boxType.Trim();
        RequiresReprocess = requiresReprocess;
        RequiresManualProcess = requiresManualProcess;
        FinalBoxCount = finalBoxCount;

        BottleFormat = string.IsNullOrWhiteSpace(bottleFormat) ? null : bottleFormat.Trim();
        ProductType = string.IsNullOrWhiteSpace(productType) ? null : productType.Trim();
        UnitsPerBottle = unitsPerBottle;

        StandardReference = standardReference;
        EstimatedMinutes = estimatedMinutes;
        CreatedAt = createdAt;
    }

    public void UpdateDetails(
        string? operatorName,
        string? batch,
        string? article,
        string? description,
        int plannedQuantity,
        int? unitsPerBox,
        string? boxType,
        bool requiresReprocess,
        bool requiresManualProcess,
        int? finalBoxCount,
        string? bottleFormat,
        string? productType,
        int? unitsPerBottle,
        decimal? standardReference,
        decimal? estimatedMinutes)
    {
        if (plannedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(plannedQuantity), "Planned quantity cannot be negative.");

        if (unitsPerBox.HasValue && unitsPerBox.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(unitsPerBox), "Units per box cannot be negative.");

        if (finalBoxCount.HasValue && finalBoxCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(finalBoxCount), "Final box count cannot be negative.");

        if (unitsPerBottle.HasValue && unitsPerBottle.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(unitsPerBottle), "Units per bottle cannot be negative.");

        if (standardReference.HasValue && standardReference.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(standardReference), "Standard reference cannot be negative.");

        if (estimatedMinutes.HasValue && estimatedMinutes.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedMinutes), "Estimated minutes cannot be negative.");

        OperatorName = string.IsNullOrWhiteSpace(operatorName) ? null : operatorName.Trim();
        Batch = string.IsNullOrWhiteSpace(batch) ? null : batch.Trim();
        Article = string.IsNullOrWhiteSpace(article) ? null : article.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        PlannedQuantity = plannedQuantity;

        UnitsPerBox = unitsPerBox;
        BoxType = string.IsNullOrWhiteSpace(boxType) ? null : boxType.Trim();
        RequiresReprocess = requiresReprocess;
        RequiresManualProcess = requiresManualProcess;
        FinalBoxCount = finalBoxCount;

        BottleFormat = string.IsNullOrWhiteSpace(bottleFormat) ? null : bottleFormat.Trim();
        ProductType = string.IsNullOrWhiteSpace(productType) ? null : productType.Trim();
        UnitsPerBottle = unitsPerBottle;

        StandardReference = standardReference;
        EstimatedMinutes = estimatedMinutes;
    }

    public void Start(DateTimeOffset startedAt)
    {
        if (Status != ProductionOrderStatus.Created && Status != ProductionOrderStatus.Paused)
            throw new InvalidOperationException("Only a created or paused order can be started.");

        Status = ProductionOrderStatus.InProgress;
        StartedAt ??= startedAt;
        FinishedAt = null;
    }

    public void Pause()
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new InvalidOperationException("Only an order in progress can be paused.");

        Status = ProductionOrderStatus.Paused;
    }

    public void Resume()
    {
        if (Status != ProductionOrderStatus.Paused)
            throw new InvalidOperationException("Only a paused order can be resumed.");

        Status = ProductionOrderStatus.InProgress;
    }

    public void MarkWaitingManualProcess(bool requiresManualProcess = true)
    {
        if (Status != ProductionOrderStatus.InProgress && Status != ProductionOrderStatus.Paused)
            throw new InvalidOperationException("Only an active or paused order can move to waiting manual process.");

        RequiresManualProcess = requiresManualProcess;
        Status = ProductionOrderStatus.WaitingManualProcess;
    }

    public void StartManualProcess()
    {
        if (Status != ProductionOrderStatus.WaitingManualProcess)
            throw new InvalidOperationException("Only an order waiting for manual process can start manual work.");

        Status = ProductionOrderStatus.ManualProcessInProgress;
    }

    public void Finish(DateTimeOffset finishedAt)
    {
        if (StartedAt.HasValue && finishedAt < StartedAt.Value)
            throw new ArgumentException("Finish date cannot be earlier than start date.", nameof(finishedAt));

        if (Status == ProductionOrderStatus.Finished || Status == ProductionOrderStatus.Cancelled)
            throw new InvalidOperationException("The order is already closed.");

        Status = ProductionOrderStatus.Finished;
        FinishedAt = finishedAt;
    }

    public void Cancel()
    {
        if (Status == ProductionOrderStatus.Finished)
            throw new InvalidOperationException("A finished order cannot be cancelled.");

        Status = ProductionOrderStatus.Cancelled;
    }

    public void UpdateCounters(int goodUnits, int badUnits)
    {
        if (goodUnits < 0)
            throw new ArgumentOutOfRangeException(nameof(goodUnits), "Good units cannot be negative.");

        if (badUnits < 0)
            throw new ArgumentOutOfRangeException(nameof(badUnits), "Bad units cannot be negative.");

        GoodUnits = goodUnits;
        BadUnits = badUnits;
    }

    public void SetFinalProductionData(
        int goodUnits,
        int badUnits,
        int? finalBoxCount,
        int? unitsPerBox,
        string? boxType,
        bool requiresReprocess,
        bool requiresManualProcess)
    {
        if (goodUnits < 0)
            throw new ArgumentOutOfRangeException(nameof(goodUnits), "Good units cannot be negative.");

        if (badUnits < 0)
            throw new ArgumentOutOfRangeException(nameof(badUnits), "Bad units cannot be negative.");

        if (finalBoxCount.HasValue && finalBoxCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(finalBoxCount), "Final box count cannot be negative.");

        if (unitsPerBox.HasValue && unitsPerBox.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(unitsPerBox), "Units per box cannot be negative.");

        GoodUnits = goodUnits;
        BadUnits = badUnits;
        FinalBoxCount = finalBoxCount;
        UnitsPerBox = unitsPerBox;
        BoxType = string.IsNullOrWhiteSpace(boxType) ? null : boxType.Trim();
        RequiresReprocess = requiresReprocess;
        RequiresManualProcess = requiresManualProcess;
    }
}