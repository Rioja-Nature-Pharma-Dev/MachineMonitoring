using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Domain.Entities;

public sealed class MachineReadingNormalized
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public ParameterCode ParameterCode { get; private set; }
    public ParameterValue Value { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public string? SourceMessageId { get; private set; }

    private MachineReadingNormalized() { }

    public MachineReadingNormalized(
        Guid id,
        Guid machineId,
        ParameterCode parameterCode,
        ParameterValue value,
        DateTimeOffset recordedAt,
        string? sourceMessageId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Normalized reading id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        Id = id;
        MachineId = machineId;
        ParameterCode = parameterCode;
        Value = value;
        RecordedAt = recordedAt;
        SourceMessageId = string.IsNullOrWhiteSpace(sourceMessageId) ? null : sourceMessageId.Trim();
    }

    public void UpdateValue(ParameterValue value, DateTimeOffset recordedAt, string? sourceMessageId)
    {
        Value = value;
        RecordedAt = recordedAt;
        SourceMessageId = string.IsNullOrWhiteSpace(sourceMessageId) ? null : sourceMessageId.Trim();
    }
}