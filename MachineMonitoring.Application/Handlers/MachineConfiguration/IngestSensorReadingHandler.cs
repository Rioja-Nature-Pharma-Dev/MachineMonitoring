using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.Services;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

public sealed record IngestSensorReadingCommand(
    string MachineCode,
    string GpioTopic,
    decimal RawValue
);

public sealed record IngestSensorReadingResult(
    string MachineCode,
    string GpioTopic,
    string ParameterCode,
    decimal RawValue,
    decimal TransformedValue,
    string? TransformExpression,
    DateTimeOffset Timestamp
);

/// <summary>
/// Recibe lectura de sensor (GPIO/MQTT), aplica transformacion y la registra
/// </summary>
public sealed class IngestSensorReadingHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineInputMappingRepository _mappingRepository;
    private readonly IMachineInputSourceRepository _sourceRepository;
    private readonly FormulaEvaluator _evaluator;
    private readonly IClock _clock;

    public IngestSensorReadingHandler(
        IMachineRepository machineRepository,
        IMachineInputMappingRepository mappingRepository,
        IMachineInputSourceRepository sourceRepository,
        FormulaEvaluator evaluator,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _mappingRepository = mappingRepository;
        _sourceRepository = sourceRepository;
        _evaluator = evaluator;
        _clock = clock;
    }

    public async Task<IngestSensorReadingResult?> HandleAsync(
        IngestSensorReadingCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(command.MachineCode, cancellationToken);
        if (machine is null)
            throw new InvalidOperationException($"Machine {command.MachineCode} not found.");

        var mappings = await _mappingRepository.GetByMachineIdAsync(machine.Id, cancellationToken);
        var source = await _sourceRepository.GetByTopicAsync(command.GpioTopic, cancellationToken);

        if (source is null)
            throw new InvalidOperationException($"GPIO topic '{command.GpioTopic}' not configured for machine.");

        var mapping = mappings.FirstOrDefault(m => m.InputSourceId == source.Id && m.IsEnabled);
        if (mapping is null)
            throw new InvalidOperationException($"No active mapping found for topic '{command.GpioTopic}'.");

        // Apply transformation
        var transformedValue = command.RawValue;
        if (!string.IsNullOrWhiteSpace(mapping.TransformExpression))
        {
            try
            {
                var vars = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["VALUE"] = command.RawValue,
                    ["RAW"] = command.RawValue
                };
                transformedValue = _evaluator.Evaluate(mapping.TransformExpression, vars);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to apply transformation '{mapping.TransformExpression}': {ex.Message}", ex);
            }
        }

        return new IngestSensorReadingResult(
            command.MachineCode,
            command.GpioTopic,
            mapping.ParameterCode.Value,
            command.RawValue,
            transformedValue,
            mapping.TransformExpression,
            _clock.UtcNow);
    }
}
