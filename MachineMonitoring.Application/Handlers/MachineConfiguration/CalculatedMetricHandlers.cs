using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Contracts.MachineConfiguration;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

public sealed class CreateCalculatedMetricHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly ICalculatedMetricDefinitionRepository _repository;
    private readonly IClock _clock;

    public CreateCalculatedMetricHandler(
        IMachineRepository machineRepository,
        ICalculatedMetricDefinitionRepository repository,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _repository = repository;
        _clock = clock;
    }

    public async Task<CalculatedMetricDto> HandleAsync(
        CreateCalculatedMetricCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(command.MachineCode, cancellationToken)
            ?? throw new InvalidOperationException($"Machine {command.MachineCode} not found.");

        var definition = new CalculatedMetricDefinition(
            Guid.NewGuid(),
            machine.Id,
            new ParameterCode(command.Request.Code),
            command.Request.Name,
            new MeasurementUnit(command.Request.Unit),
            command.Request.FormulaExpression,
            command.Request.IsEnabled,
            _clock.UtcNow);

        await _repository.AddAsync(definition, cancellationToken);

        return ToDto(definition);
    }

    public static CalculatedMetricDto ToDto(CalculatedMetricDefinition definition) =>
        new(
            definition.Id,
            definition.Code.Value,
            definition.Name,
            definition.Unit.Value,
            definition.FormulaExpression,
            definition.IsEnabled);
}

public sealed class GetCalculatedMetricHandler
{
    private readonly ICalculatedMetricDefinitionRepository _repository;

    public GetCalculatedMetricHandler(ICalculatedMetricDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CalculatedMetricDto?> HandleAsync(
        GetCalculatedMetricQuery query,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(query.MetricId, cancellationToken);
        return definition is null ? null : CreateCalculatedMetricHandler.ToDto(definition);
    }
}

public sealed class ListCalculatedMetricsHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly ICalculatedMetricDefinitionRepository _repository;

    public ListCalculatedMetricsHandler(
        IMachineRepository machineRepository,
        ICalculatedMetricDefinitionRepository repository)
    {
        _machineRepository = machineRepository;
        _repository = repository;
    }

    public async Task<IReadOnlyList<CalculatedMetricDto>> HandleAsync(
        ListCalculatedMetricsQuery query,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(query.MachineCode, cancellationToken);
        if (machine is null) return Array.Empty<CalculatedMetricDto>();

        var definitions = await _repository.GetByMachineIdAsync(machine.Id, cancellationToken);
        return definitions.Select(CreateCalculatedMetricHandler.ToDto).ToList();
    }
}

public sealed class GetMachineConfigurationHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineParameterDefinitionRepository _parameterRepository;
    private readonly IMachineInputMappingRepository _mappingRepository;
    private readonly IMachineInputSourceRepository _sourceRepository;
    private readonly ICalculatedMetricDefinitionRepository _metricRepository;

    public GetMachineConfigurationHandler(
        IMachineRepository machineRepository,
        IMachineParameterDefinitionRepository parameterRepository,
        IMachineInputMappingRepository mappingRepository,
        IMachineInputSourceRepository sourceRepository,
        ICalculatedMetricDefinitionRepository metricRepository)
    {
        _machineRepository = machineRepository;
        _parameterRepository = parameterRepository;
        _mappingRepository = mappingRepository;
        _sourceRepository = sourceRepository;
        _metricRepository = metricRepository;
    }

    public async Task<MachineConfigurationDto?> HandleAsync(
        GetMachineConfigurationQuery query,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(query.MachineCode, cancellationToken);
        if (machine is null) return null;

        var parameters = await _parameterRepository.GetByMachineIdAsync(machine.Id, cancellationToken);
        var mappings = await _mappingRepository.GetByMachineIdAsync(machine.Id, cancellationToken);
        var metrics = await _metricRepository.GetByMachineIdAsync(machine.Id, cancellationToken);

        var mappingDtos = new List<GpioMappingDto>();
        foreach (var mapping in mappings)
        {
            var source = await _sourceRepository.GetByIdAsync(mapping.InputSourceId, cancellationToken);
            if (source is not null)
                mappingDtos.Add(CreateGpioMappingHandler.ToDto(mapping, source));
        }

        return new MachineConfigurationDto(
            machine.Code,
            machine.Name,
            parameters.Select(AddParameterDefinitionHandler.ToDto).ToList(),
            mappingDtos,
            metrics.Select(CreateCalculatedMetricHandler.ToDto).ToList());
    }
}
