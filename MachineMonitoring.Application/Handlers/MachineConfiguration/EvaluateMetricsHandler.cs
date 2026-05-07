using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Services;
using MachineMonitoring.Contracts.MachineConfiguration;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

public sealed record EvaluateMetricsCommand(
    string MachineCode,
    Dictionary<string, decimal> InputValues
);

public sealed record EvaluatedMetricResult(
    string Code,
    string Name,
    string Unit,
    decimal Value,
    string Formula,
    bool Success,
    string? Error
);

public sealed record EvaluateMetricsResultDto(
    string MachineCode,
    IReadOnlyDictionary<string, decimal> InputValues,
    IReadOnlyList<EvaluatedMetricResult> Metrics
);

public sealed class EvaluateMetricsHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly ICalculatedMetricDefinitionRepository _metricRepository;
    private readonly FormulaEvaluator _evaluator;

    public EvaluateMetricsHandler(
        IMachineRepository machineRepository,
        ICalculatedMetricDefinitionRepository metricRepository,
        FormulaEvaluator evaluator)
    {
        _machineRepository = machineRepository;
        _metricRepository = metricRepository;
        _evaluator = evaluator;
    }

    public async Task<EvaluateMetricsResultDto?> HandleAsync(
        EvaluateMetricsCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(command.MachineCode, cancellationToken);
        if (machine is null) return null;

        var metrics = await _metricRepository.GetByMachineIdAsync(machine.Id, cancellationToken);

        var results = new List<EvaluatedMetricResult>();
        var resolvedValues = new Dictionary<string, decimal>(command.InputValues, StringComparer.OrdinalIgnoreCase);

        // Iterative resolution: metrics may depend on other calculated metrics
        var pending = metrics.Where(m => m.IsEnabled).ToList();
        var maxIterations = pending.Count + 1;

        for (var iteration = 0; iteration < maxIterations && pending.Count > 0; iteration++)
        {
            var resolvedThisRound = new List<MachineMonitoring.Domain.Entities.CalculatedMetricDefinition>();

            foreach (var metric in pending)
            {
                var requiredVars = _evaluator.ExtractVariables(metric.FormulaExpression);
                var missingVars = requiredVars.Where(v => !resolvedValues.ContainsKey(v)).ToList();

                if (missingVars.Any()) continue;

                try
                {
                    var value = _evaluator.Evaluate(metric.FormulaExpression, resolvedValues);
                    resolvedValues[metric.Code.Value] = value;

                    results.Add(new EvaluatedMetricResult(
                        metric.Code.Value,
                        metric.Name,
                        metric.Unit.Value,
                        Math.Round(value, 4),
                        metric.FormulaExpression,
                        true,
                        null));

                    resolvedThisRound.Add(metric);
                }
                catch (Exception ex)
                {
                    results.Add(new EvaluatedMetricResult(
                        metric.Code.Value,
                        metric.Name,
                        metric.Unit.Value,
                        0,
                        metric.FormulaExpression,
                        false,
                        ex.Message));

                    resolvedThisRound.Add(metric);
                }
            }

            foreach (var resolved in resolvedThisRound)
                pending.Remove(resolved);
        }

        // Any pending metrics couldn't be resolved (circular deps or missing vars)
        foreach (var metric in pending)
        {
            var missing = _evaluator.ExtractVariables(metric.FormulaExpression)
                .Where(v => !resolvedValues.ContainsKey(v))
                .ToList();

            results.Add(new EvaluatedMetricResult(
                metric.Code.Value,
                metric.Name,
                metric.Unit.Value,
                0,
                metric.FormulaExpression,
                false,
                $"Missing variables: {string.Join(", ", missing)}"));
        }

        return new EvaluateMetricsResultDto(
            command.MachineCode,
            command.InputValues,
            results);
    }
}
