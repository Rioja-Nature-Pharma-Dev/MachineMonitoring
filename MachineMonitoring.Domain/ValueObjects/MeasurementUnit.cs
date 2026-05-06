namespace MachineMonitoring.Domain.ValueObjects;

public sealed record MeasurementUnit
{
    public string Value { get; }

    public MeasurementUnit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Measurement unit cannot be empty.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}