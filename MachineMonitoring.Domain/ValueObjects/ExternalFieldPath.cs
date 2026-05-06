namespace MachineMonitoring.Domain.ValueObjects;

public sealed record ExternalFieldPath
{
    public string Value { get; }

    public ExternalFieldPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("External field path cannot be empty.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}