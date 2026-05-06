using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Domain.Entities;

public sealed class AlertRuleDefinition
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string ConditionExpression { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public AlertRuleDefinition(
        Guid id,
        Guid machineId,
        string code,
        string name,
        string conditionExpression,
        AlertSeverity severity,
        bool isEnabled,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Alert rule id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Alert rule code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Alert rule name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(conditionExpression))
            throw new ArgumentException("Alert rule condition cannot be empty.", nameof(conditionExpression));

        Id = id;
        MachineId = machineId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        ConditionExpression = conditionExpression.Trim();
        Severity = severity;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public void Update(
        string name,
        string conditionExpression,
        AlertSeverity severity,
        bool isEnabled,
        DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Alert rule name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(conditionExpression))
            throw new ArgumentException("Alert rule condition cannot be empty.", nameof(conditionExpression));

        Name = name.Trim();
        ConditionExpression = conditionExpression.Trim();
        Severity = severity;
        IsEnabled = isEnabled;
        UpdatedAt = updatedAt;
    }
}