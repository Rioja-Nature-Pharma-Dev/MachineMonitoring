using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Domain.Entities;

public sealed class ProductionPause
{
    public Guid Id { get; init; }
    public Guid ProductionOrderId { get; init; }
    public PauseType? PauseType { get; private set; }
    public string? Description { get; private set; }
    public string? OperatorName { get; private set; }
    public bool? CountsTowardsMetrics { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public decimal? TotalMinutes { get; private set; }

    public ProductionPause(
        Guid id,
        Guid productionOrderId,
        PauseType? pauseType,
        string? description,
        string? operatorName,
        bool? countsTowardsMetrics,
        DateTimeOffset startedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Pause id cannot be empty.", nameof(id));

        if (productionOrderId == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(productionOrderId));

        Id = id;
        ProductionOrderId = productionOrderId;
        PauseType = pauseType;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        OperatorName = string.IsNullOrWhiteSpace(operatorName) ? null : operatorName.Trim();
        CountsTowardsMetrics = countsTowardsMetrics;
        StartedAt = startedAt;
    }

    public bool IsActive => !EndedAt.HasValue;

    public void UpdateDetails(
        PauseType? pauseType,
        string? description,
        string? operatorName,
        bool? countsTowardsMetrics)
    {
        PauseType = pauseType;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        OperatorName = string.IsNullOrWhiteSpace(operatorName) ? null : operatorName.Trim();
        CountsTowardsMetrics = countsTowardsMetrics;
    }

    public void AssignType(PauseType pauseType, bool countsTowardsMetrics)
    {
        PauseType = pauseType;
        CountsTowardsMetrics = countsTowardsMetrics;
    }

    public void AppendDescription(string? extraDescription)
    {
        if (string.IsNullOrWhiteSpace(extraDescription))
            return;

        if (string.IsNullOrWhiteSpace(Description))
        {
            Description = extraDescription.Trim();
            return;
        }

        Description = $"{Description} | {extraDescription.Trim()}";
    }

    public void Finish(DateTimeOffset endedAt)
    {
        if (EndedAt.HasValue)
            throw new InvalidOperationException("The pause is already finished.");

        if (endedAt < StartedAt)
            throw new ArgumentException("Pause end date cannot be earlier than start date.", nameof(endedAt));

        EndedAt = endedAt;
        TotalMinutes = Convert.ToDecimal((EndedAt.Value - StartedAt).TotalMinutes);
    }
}