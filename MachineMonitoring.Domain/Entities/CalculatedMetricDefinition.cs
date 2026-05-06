using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Domain.Entities;

public sealed class CalculatedMetricDefinition
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public ParameterCode Code { get; private set; }
    public string Name { get; private set; }
    public MeasurementUnit Unit { get; private set; }
    public string FormulaExpression { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public CalculatedMetricDefinition(
        Guid id,
        Guid machineId,
        ParameterCode code,
        string name,
        MeasurementUnit unit,
        string formulaExpression,
        bool isEnabled,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Calculated metric id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Calculated metric name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(formulaExpression))
            throw new ArgumentException("Formula expression cannot be empty.", nameof(formulaExpression));

        Id = id;
        MachineId = machineId;
        Code = code;
        Name = name.Trim();
        Unit = unit;
        FormulaExpression = formulaExpression.Trim();
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public void Update(
        string name,
        MeasurementUnit unit,
        string formulaExpression,
        bool isEnabled,
        DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Calculated metric name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(formulaExpression))
            throw new ArgumentException("Formula expression cannot be empty.", nameof(formulaExpression));

        Name = name.Trim();
        Unit = unit;
        FormulaExpression = formulaExpression.Trim();
        IsEnabled = isEnabled;
        UpdatedAt = updatedAt;
    }
}