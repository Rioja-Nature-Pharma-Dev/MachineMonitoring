namespace MachineMonitoring.Domain.Entities;

public sealed class OrderAudit
{
    public Guid Id { get; init; }
    public Guid ProductionOrderId { get; init; }
    public string Action { get; private set; }
    public string PerformedBy { get; private set; }
    public string? Reason { get; private set; }
    public string? SnapshotJson { get; private set; }
    public DateTimeOffset PerformedAt { get; init; }

    public OrderAudit(
        Guid id,
        Guid productionOrderId,
        string action,
        string performedBy,
        string? reason,
        string? snapshotJson,
        DateTimeOffset performedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Audit id cannot be empty.", nameof(id));

        if (productionOrderId == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(productionOrderId));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Audit action cannot be empty.", nameof(action));

        if (string.IsNullOrWhiteSpace(performedBy))
            throw new ArgumentException("Performed by cannot be empty.", nameof(performedBy));

        Id = id;
        ProductionOrderId = productionOrderId;
        Action = action.Trim().ToUpperInvariant();
        PerformedBy = performedBy.Trim();
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        SnapshotJson = string.IsNullOrWhiteSpace(snapshotJson) ? null : snapshotJson.Trim();
        PerformedAt = performedAt;
    }

    public void UpdateReason(string? reason)
    {
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public void UpdateSnapshot(string? snapshotJson)
    {
        SnapshotJson = string.IsNullOrWhiteSpace(snapshotJson) ? null : snapshotJson.Trim();
    }
}