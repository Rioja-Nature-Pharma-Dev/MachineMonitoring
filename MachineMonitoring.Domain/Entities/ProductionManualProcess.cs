namespace MachineMonitoring.Domain.Entities;

public sealed class ProductionManualProcess
{
    public Guid Id { get; init; }
    public Guid ProductionOrderId { get; init; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public decimal? TotalMinutes { get; private set; }
    public int ManualBoxCount { get; private set; }

    public ProductionManualProcess(
        Guid id,
        Guid productionOrderId,
        DateTimeOffset startedAt,
        int manualBoxCount = 0)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Manual process id cannot be empty.", nameof(id));

        if (productionOrderId == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(productionOrderId));

        if (manualBoxCount < 0)
            throw new ArgumentOutOfRangeException(nameof(manualBoxCount), "Manual box count cannot be negative.");

        Id = id;
        ProductionOrderId = productionOrderId;
        StartedAt = startedAt;
        ManualBoxCount = manualBoxCount;
    }

    public bool IsActive => !FinishedAt.HasValue;

    public void Finish(DateTimeOffset finishedAt, int manualBoxCount)
    {
        if (FinishedAt.HasValue)
            throw new InvalidOperationException("The manual process is already finished.");

        if (finishedAt < StartedAt)
            throw new ArgumentException("Finish date cannot be earlier than start date.", nameof(finishedAt));

        if (manualBoxCount < 0)
            throw new ArgumentOutOfRangeException(nameof(manualBoxCount), "Manual box count cannot be negative.");

        FinishedAt = finishedAt;
        ManualBoxCount = manualBoxCount;
        TotalMinutes = Convert.ToDecimal((FinishedAt.Value - StartedAt).TotalMinutes);
    }

    public void UpdateManualBoxCount(int manualBoxCount)
    {
        if (manualBoxCount < 0)
            throw new ArgumentOutOfRangeException(nameof(manualBoxCount), "Manual box count cannot be negative.");

        ManualBoxCount = manualBoxCount;
    }
}