namespace MachineMonitoring.Domain.Entities;

public sealed class MachineRuleDefinition
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string RuleType { get; private set; }
    public string Expression { get; private set; }
    public bool IsEnabled { get; private set; }
    public int ExecutionOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public MachineRuleDefinition(
        Guid id,
        Guid machineId,
        string code,
        string name,
        string ruleType,
        string expression,
        bool isEnabled,
        int executionOrder,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Rule id cannot be empty.", nameof(id));

        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id cannot be empty.", nameof(machineId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Rule code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(ruleType))
            throw new ArgumentException("Rule type cannot be empty.", nameof(ruleType));

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Rule expression cannot be empty.", nameof(expression));

        if (executionOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(executionOrder), "Execution order cannot be negative.");

        Id = id;
        MachineId = machineId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        RuleType = ruleType.Trim();
        Expression = expression.Trim();
        IsEnabled = isEnabled;
        ExecutionOrder = executionOrder;
        CreatedAt = createdAt;
    }

    public void Update(
        string name,
        string ruleType,
        string expression,
        bool isEnabled,
        int executionOrder,
        DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(ruleType))
            throw new ArgumentException("Rule type cannot be empty.", nameof(ruleType));

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Rule expression cannot be empty.", nameof(expression));

        if (executionOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(executionOrder), "Execution order cannot be negative.");

        Name = name.Trim();
        RuleType = ruleType.Trim();
        Expression = expression.Trim();
        IsEnabled = isEnabled;
        ExecutionOrder = executionOrder;
        UpdatedAt = updatedAt;
    }
}