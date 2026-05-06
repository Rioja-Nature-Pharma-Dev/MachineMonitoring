using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Contracts.MachineConfiguration;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

public sealed class AddParameterDefinitionHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineParameterDefinitionRepository _repository;

    public AddParameterDefinitionHandler(
        IMachineRepository machineRepository,
        IMachineParameterDefinitionRepository repository)
    {
        _machineRepository = machineRepository;
        _repository = repository;
    }

    public async Task<ParameterDefinitionDto> HandleAsync(
        AddParameterDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(command.MachineCode, cancellationToken)
            ?? throw new InvalidOperationException($"Machine {command.MachineCode} not found.");

        var definition = new MachineParameterDefinition(
            Guid.NewGuid(),
            machine.Id,
            new ParameterCode(command.Request.Code),
            command.Request.Name,
            new MeasurementUnit(command.Request.Unit),
            command.Request.DataType,
            command.Request.IsRequired,
            command.Request.MinValue,
            command.Request.MaxValue,
            command.Request.IsCalculated);

        await _repository.AddAsync(definition, cancellationToken);

        return ToDto(definition);
    }

    public static ParameterDefinitionDto ToDto(MachineParameterDefinition definition) =>
        new(
            definition.Id,
            definition.Code.Value,
            definition.Name,
            definition.Unit.Value,
            definition.DataType,
            definition.IsRequired,
            definition.MinValue,
            definition.MaxValue,
            definition.IsCalculated);
}

public sealed class GetParameterDefinitionHandler
{
    private readonly IMachineParameterDefinitionRepository _repository;

    public GetParameterDefinitionHandler(IMachineParameterDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ParameterDefinitionDto?> HandleAsync(
        GetParameterDefinitionQuery query,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(query.ParameterId, cancellationToken);
        return definition is null ? null : AddParameterDefinitionHandler.ToDto(definition);
    }
}

public sealed class ListParametersHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineParameterDefinitionRepository _repository;

    public ListParametersHandler(
        IMachineRepository machineRepository,
        IMachineParameterDefinitionRepository repository)
    {
        _machineRepository = machineRepository;
        _repository = repository;
    }

    public async Task<IReadOnlyList<ParameterDefinitionDto>> HandleAsync(
        ListParametersQuery query,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(query.MachineCode, cancellationToken);
        if (machine is null) return Array.Empty<ParameterDefinitionDto>();

        var definitions = await _repository.GetByMachineIdAsync(machine.Id, cancellationToken);
        return definitions.Select(AddParameterDefinitionHandler.ToDto).ToList();
    }
}
