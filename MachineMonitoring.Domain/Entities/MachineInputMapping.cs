using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Domain.Entities;

public sealed class MachineInputMapping
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public Guid InputSourceId { get; init; }
    public ExternalFieldPath ExternalFieldPath { get; private set; }
    public ParameterCode ParameterCode { get; private set; }
    public string? TransformExpression { get; private set; }
    public bool IsEnabled { get; private set; }

    public MachineInputMapping(
        Guid id,
        Guid machineId,
        Guid inputSourceId,
        ExternalFieldPath externalFieldPath,
        ParameterCode parameterCode,
        string? transformExpression,
        bool isEnabled)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Mapping id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (inputSourceId == Guid.Empty)
            throw new ArgumentException("Input source id cannot be empty.", nameof(inputSourceId));

        Id = id;
        MachineId = machineId;
        InputSourceId = inputSourceId;
        ExternalFieldPath = externalFieldPath;
        ParameterCode = parameterCode;
        TransformExpression = string.IsNullOrWhiteSpace(transformExpression) ? null : transformExpression.Trim();
        IsEnabled = isEnabled;
    }

    public void UpdateMapping(
        ExternalFieldPath externalFieldPath,
        ParameterCode parameterCode,
        string? transformExpression,
        bool isEnabled)
    {
        ExternalFieldPath = externalFieldPath;
        ParameterCode = parameterCode;
        TransformExpression = string.IsNullOrWhiteSpace(transformExpression) ? null : transformExpression.Trim();
        IsEnabled = isEnabled;
    }
}