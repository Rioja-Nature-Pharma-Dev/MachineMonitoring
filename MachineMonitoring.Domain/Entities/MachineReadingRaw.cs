namespace MachineMonitoring.Domain.Entities;

public sealed class MachineReadingRaw
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public Guid InputSourceId { get; init; }
    public string Payload { get; private set; }
    public DateTimeOffset ReceivedAt { get; init; }
    public string? CorrelationId { get; private set; }

    public MachineReadingRaw(
        Guid id,
        Guid machineId,
        Guid inputSourceId,
        string payload,
        DateTimeOffset receivedAt,
        string? correlationId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Raw reading id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (inputSourceId == Guid.Empty)
            throw new ArgumentException("Input source id cannot be empty.", nameof(inputSourceId));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be empty.", nameof(payload));

        Id = id;
        MachineId = machineId;
        InputSourceId = inputSourceId;
        Payload = payload.Trim();
        ReceivedAt = receivedAt;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
    }
}