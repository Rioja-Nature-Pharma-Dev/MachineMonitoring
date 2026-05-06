using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Domain.Entities;

public sealed class MachineParameterDefinition
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public ParameterCode Code { get; private set; }
    public string Name { get; private set; }
    public MeasurementUnit Unit { get; private set; }
    public string DataType { get; private set; }
    public bool IsRequired { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public bool IsCalculated { get; private set; }

    public MachineParameterDefinition(
        Guid id,
        Guid machineId,
        ParameterCode code,
        string name,
        MeasurementUnit unit,
        string dataType,
        bool isRequired,
        decimal? minValue,
        decimal? maxValue,
        bool isCalculated)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Parameter definition id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(dataType))
            throw new ArgumentException("Data type cannot be empty.", nameof(dataType));

        if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
            throw new ArgumentException("Minimum value cannot be greater than maximum value.");

        Id = id;
        MachineId = machineId;
        Code = code;
        Name = name.Trim();
        Unit = unit;
        DataType = dataType.Trim();
        IsRequired = isRequired;
        MinValue = minValue;
        MaxValue = maxValue;
        IsCalculated = isCalculated;
    }

    public void UpdateDefinition(
        string name,
        MeasurementUnit unit,
        string dataType,
        bool isRequired,
        decimal? minValue,
        decimal? maxValue,
        bool isCalculated)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(dataType))
            throw new ArgumentException("Data type cannot be empty.", nameof(dataType));

        if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
            throw new ArgumentException("Minimum value cannot be greater than maximum value.");

        Name = name.Trim();
        Unit = unit;
        DataType = dataType.Trim();
        IsRequired = isRequired;
        MinValue = minValue;
        MaxValue = maxValue;
        IsCalculated = isCalculated;
    }
}