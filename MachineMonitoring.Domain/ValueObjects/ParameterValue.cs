namespace MachineMonitoring.Domain.ValueObjects;

public sealed record ParameterValue
{
    public decimal NumericValue { get; }
    public string? TextValue { get; }
    public bool? BooleanValue { get; }

    private ParameterValue(decimal numericValue, string? textValue, bool? booleanValue)
    {
        NumericValue = numericValue;
        TextValue = textValue;
        BooleanValue = booleanValue;
    }

    public static ParameterValue FromDecimal(decimal value) => new(value, null, null);

    public static ParameterValue FromText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Text value cannot be empty.", nameof(value));

        return new ParameterValue(0m, value.Trim(), null);
    }

    public static ParameterValue FromBoolean(bool value) => new(0m, null, value);
}