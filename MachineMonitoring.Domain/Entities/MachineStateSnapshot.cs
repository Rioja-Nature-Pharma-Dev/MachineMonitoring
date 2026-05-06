namespace MachineMonitoring.Domain.Entities;

public sealed class MachineStateSnapshot
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string StateJson { get; private set; }
    public DateTimeOffset CalculatedAt { get; private set; }

    public MachineStateSnapshot(
        Guid id,
        Guid machineId,
        string stateJson,
        DateTimeOffset calculatedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Snapshot id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(stateJson))
            throw new ArgumentException("State payload cannot be empty.", nameof(stateJson));

        Id = id;
        MachineId = machineId;
        StateJson = stateJson.Trim();
        CalculatedAt = calculatedAt;
    }

    public void UpdateState(string stateJson, DateTimeOffset calculatedAt)
    {
        if (string.IsNullOrWhiteSpace(stateJson))
            throw new ArgumentException("State payload cannot be empty.", nameof(stateJson));

        StateJson = stateJson.Trim();
        CalculatedAt = calculatedAt;
    }
}