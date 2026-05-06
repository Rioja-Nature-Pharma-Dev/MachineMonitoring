namespace MachineMonitoring.Domain.ValueObjects;

public sealed record ParameterCode
{
    public string Value { get; }

    public ParameterCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Parameter code cannot be empty.", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    public override string ToString() => Value;
}