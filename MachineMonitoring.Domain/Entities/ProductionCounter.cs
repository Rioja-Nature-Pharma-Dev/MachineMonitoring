namespace MachineMonitoring.Domain.Entities;

public sealed class ProductionCounter
{
    public Guid Id { get; init; }
    public Guid ProductionOrderId { get; init; }
    public int Quantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
    public DateTimeOffset? LastCountedAt { get; private set; }

    public ProductionCounter(
        Guid id,
        Guid productionOrderId,
        int quantity,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset lastUpdatedAt,
        DateTimeOffset? lastCountedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Counter id cannot be empty.", nameof(id));

        if (productionOrderId == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(productionOrderId));

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        if (lastUpdatedAt < createdAt)
            throw new ArgumentException("Last updated date cannot be earlier than created date.", nameof(lastUpdatedAt));

        if (lastCountedAt.HasValue && lastCountedAt.Value < createdAt)
            throw new ArgumentException("Last counted date cannot be earlier than created date.", nameof(lastCountedAt));

        Id = id;
        ProductionOrderId = productionOrderId;
        Quantity = quantity;
        IsActive = isActive;
        CreatedAt = createdAt;
        LastUpdatedAt = lastUpdatedAt;
        LastCountedAt = lastCountedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (updatedAt < CreatedAt)
            throw new ArgumentException("Updated date cannot be earlier than created date.", nameof(updatedAt));

        IsActive = true;
        LastUpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (updatedAt < CreatedAt)
            throw new ArgumentException("Updated date cannot be earlier than created date.", nameof(updatedAt));

        IsActive = false;
        LastUpdatedAt = updatedAt;
    }

    public void SetQuantity(int quantity, DateTimeOffset updatedAt, DateTimeOffset? lastCountedAt)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        if (updatedAt < CreatedAt)
            throw new ArgumentException("Updated date cannot be earlier than created date.", nameof(updatedAt));

        if (lastCountedAt.HasValue && lastCountedAt.Value < CreatedAt)
            throw new ArgumentException("Last counted date cannot be earlier than created date.", nameof(lastCountedAt));

        Quantity = quantity;
        LastUpdatedAt = updatedAt;
        LastCountedAt = lastCountedAt;
    }

    public void Increment(DateTimeOffset countedAt)
    {
        if (countedAt < CreatedAt)
            throw new ArgumentException("Counted date cannot be earlier than created date.", nameof(countedAt));

        Quantity++;
        LastUpdatedAt = countedAt;
        LastCountedAt = countedAt;
    }

    public void Reset(DateTimeOffset updatedAt)
    {
        if (updatedAt < CreatedAt)
            throw new ArgumentException("Updated date cannot be earlier than created date.", nameof(updatedAt));

        Quantity = 0;
        LastUpdatedAt = updatedAt;
        LastCountedAt = null;
    }
}