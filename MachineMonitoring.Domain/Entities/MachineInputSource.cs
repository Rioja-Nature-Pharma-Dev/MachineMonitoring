using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Domain.Entities;

public sealed class MachineInputSource
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public InputSourceType SourceType { get; private set; }
    public string Name { get; private set; }
    public string EndpointOrTopic { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }

    public MachineInputSource(
        Guid id,
        Guid machineId,
        InputSourceType sourceType,
        string name,
        string endpointOrTopic,
        bool isEnabled,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Input source id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Input source name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(endpointOrTopic))
            throw new ArgumentException("Endpoint or topic cannot be empty.", nameof(endpointOrTopic));

        Id = id;
        MachineId = machineId;
        SourceType = sourceType;
        Name = name.Trim();
        EndpointOrTopic = endpointOrTopic.Trim();
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public void UpdateConfiguration(InputSourceType sourceType, string name, string endpointOrTopic, bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Input source name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(endpointOrTopic))
            throw new ArgumentException("Endpoint or topic cannot be empty.", nameof(endpointOrTopic));

        SourceType = sourceType;
        Name = name.Trim();
        EndpointOrTopic = endpointOrTopic.Trim();
        IsEnabled = isEnabled;
    }
}