using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Domain.Entities;

public sealed class MachineAlert
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string Code { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    public MachineAlert(
        Guid id,
        Guid machineId,
        string code,
        string title,
        string? description,
        AlertSeverity severity,
        bool isActive,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Alert id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Alert code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert title cannot be empty.", nameof(title));

        if (endedAt.HasValue && endedAt.Value < startedAt)
            throw new ArgumentException("Alert end date cannot be earlier than start date.", nameof(endedAt));

        Id = id;
        MachineId = machineId;
        Code = code.Trim().ToUpperInvariant();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Severity = severity;
        IsActive = isActive;
        StartedAt = startedAt;
        EndedAt = endedAt;
    }

    public void Activate(string title, string? description, AlertSeverity severity, DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert title cannot be empty.", nameof(title));

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Severity = severity;
        IsActive = true;
        StartedAt = startedAt;
        EndedAt = null;
    }

    public void Resolve(DateTimeOffset endedAt)
    {
        if (endedAt < StartedAt)
            throw new ArgumentException("Alert end date cannot be earlier than start date.", nameof(endedAt));

        IsActive = false;
        EndedAt = endedAt;
    }
}