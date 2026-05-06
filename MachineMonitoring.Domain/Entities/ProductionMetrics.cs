namespace MachineMonitoring.Domain.Entities;

public sealed class ProductionMetrics
{
    public Guid Id { get; init; }
    public Guid ProductionOrderId { get; init; }
    public decimal? TotalMinutes { get; private set; }
    public decimal? PausedMinutes { get; private set; }
    public decimal? ActiveMinutes { get; private set; }
    public decimal? Availability { get; private set; }
    public decimal? Performance { get; private set; }
    public decimal? Quality { get; private set; }
    public decimal? Oee { get; private set; }
    public decimal? RealStandard { get; private set; }
    public decimal? OrderFulfillment { get; private set; }

    public ProductionMetrics(
        Guid id,
        Guid productionOrderId,
        decimal? totalMinutes,
        decimal? pausedMinutes,
        decimal? activeMinutes,
        decimal? availability,
        decimal? performance,
        decimal? quality,
        decimal? oee,
        decimal? realStandard,
        decimal? orderFulfillment)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Metrics id cannot be empty.", nameof(id));

        if (productionOrderId == Guid.Empty)
            throw new ArgumentException("Production order id cannot be empty.", nameof(productionOrderId));

        ValidateNonNegative(totalMinutes, nameof(totalMinutes));
        ValidateNonNegative(pausedMinutes, nameof(pausedMinutes));
        ValidateNonNegative(activeMinutes, nameof(activeMinutes));
        ValidateNonNegative(availability, nameof(availability));
        ValidateNonNegative(performance, nameof(performance));
        ValidateNonNegative(quality, nameof(quality));
        ValidateNonNegative(oee, nameof(oee));
        ValidateNonNegative(realStandard, nameof(realStandard));
        ValidateNonNegative(orderFulfillment, nameof(orderFulfillment));

        Id = id;
        ProductionOrderId = productionOrderId;
        TotalMinutes = totalMinutes;
        PausedMinutes = pausedMinutes;
        ActiveMinutes = activeMinutes;
        Availability = availability;
        Performance = performance;
        Quality = quality;
        Oee = oee;
        RealStandard = realStandard;
        OrderFulfillment = orderFulfillment;
    }

    public void Update(
        decimal? totalMinutes,
        decimal? pausedMinutes,
        decimal? activeMinutes,
        decimal? availability,
        decimal? performance,
        decimal? quality,
        decimal? oee,
        decimal? realStandard,
        decimal? orderFulfillment)
    {
        ValidateNonNegative(totalMinutes, nameof(totalMinutes));
        ValidateNonNegative(pausedMinutes, nameof(pausedMinutes));
        ValidateNonNegative(activeMinutes, nameof(activeMinutes));
        ValidateNonNegative(availability, nameof(availability));
        ValidateNonNegative(performance, nameof(performance));
        ValidateNonNegative(quality, nameof(quality));
        ValidateNonNegative(oee, nameof(oee));
        ValidateNonNegative(realStandard, nameof(realStandard));
        ValidateNonNegative(orderFulfillment, nameof(orderFulfillment));

        TotalMinutes = totalMinutes;
        PausedMinutes = pausedMinutes;
        ActiveMinutes = activeMinutes;
        Availability = availability;
        Performance = performance;
        Quality = quality;
        Oee = oee;
        RealStandard = realStandard;
        OrderFulfillment = orderFulfillment;
    }

    private static void ValidateNonNegative(decimal? value, string paramName)
    {
        if (value.HasValue && value.Value < 0)
            throw new ArgumentOutOfRangeException(paramName, "Metric values cannot be negative.");
    }
}