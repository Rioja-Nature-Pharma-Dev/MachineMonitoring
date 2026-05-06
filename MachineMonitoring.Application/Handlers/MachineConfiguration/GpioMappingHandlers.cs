using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Contracts.MachineConfiguration;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;
using MachineMonitoring.Domain.ValueObjects;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

public sealed class CreateGpioMappingHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineInputSourceRepository _sourceRepository;
    private readonly IMachineInputMappingRepository _mappingRepository;
    private readonly IClock _clock;

    public CreateGpioMappingHandler(
        IMachineRepository machineRepository,
        IMachineInputSourceRepository sourceRepository,
        IMachineInputMappingRepository mappingRepository,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _sourceRepository = sourceRepository;
        _mappingRepository = mappingRepository;
        _clock = clock;
    }

    public async Task<GpioMappingDto> HandleAsync(
        CreateGpioMappingCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(command.MachineCode, cancellationToken)
            ?? throw new InvalidOperationException($"Machine {command.MachineCode} not found.");

        // Get or create input source
        var source = await _sourceRepository.GetByTopicAsync(command.Request.GpioTopic, cancellationToken);
        if (source is null)
        {
            source = new MachineInputSource(
                Guid.NewGuid(),
                machine.Id,
                InputSourceType.Mqtt,
                command.Request.GpioTopic,
                command.Request.GpioTopic,
                true,
                _clock.UtcNow);
            await _sourceRepository.AddAsync(source, cancellationToken);
        }

        var mapping = new MachineInputMapping(
            Guid.NewGuid(),
            machine.Id,
            source.Id,
            new ExternalFieldPath(command.Request.GpioTopic),
            new ParameterCode(command.Request.ParameterCode),
            command.Request.TransformExpression,
            command.Request.IsEnabled);

        await _mappingRepository.AddAsync(mapping, cancellationToken);

        return ToDto(mapping, source);
    }

    public static GpioMappingDto ToDto(MachineInputMapping mapping, MachineInputSource source) =>
        new(
            mapping.Id,
            source.EndpointOrTopic,
            mapping.ParameterCode.Value,
            mapping.TransformExpression,
            mapping.IsEnabled);
}

public sealed class GetGpioMappingHandler
{
    private readonly IMachineInputMappingRepository _mappingRepository;
    private readonly IMachineInputSourceRepository _sourceRepository;

    public GetGpioMappingHandler(
        IMachineInputMappingRepository mappingRepository,
        IMachineInputSourceRepository sourceRepository)
    {
        _mappingRepository = mappingRepository;
        _sourceRepository = sourceRepository;
    }

    public async Task<GpioMappingDto?> HandleAsync(
        GetGpioMappingQuery query,
        CancellationToken cancellationToken = default)
    {
        var mapping = await _mappingRepository.GetByIdAsync(query.MappingId, cancellationToken);
        if (mapping is null) return null;

        var source = await _sourceRepository.GetByIdAsync(mapping.InputSourceId, cancellationToken);
        return source is null ? null : CreateGpioMappingHandler.ToDto(mapping, source);
    }
}

public sealed class ListGpioMappingsHandler
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineInputMappingRepository _mappingRepository;
    private readonly IMachineInputSourceRepository _sourceRepository;

    public ListGpioMappingsHandler(
        IMachineRepository machineRepository,
        IMachineInputMappingRepository mappingRepository,
        IMachineInputSourceRepository sourceRepository)
    {
        _machineRepository = machineRepository;
        _mappingRepository = mappingRepository;
        _sourceRepository = sourceRepository;
    }

    public async Task<IReadOnlyList<GpioMappingDto>> HandleAsync(
        ListGpioMappingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(query.MachineCode, cancellationToken);
        if (machine is null) return Array.Empty<GpioMappingDto>();

        var mappings = await _mappingRepository.GetByMachineIdAsync(machine.Id, cancellationToken);
        var result = new List<GpioMappingDto>();

        foreach (var mapping in mappings)
        {
            var source = await _sourceRepository.GetByIdAsync(mapping.InputSourceId, cancellationToken);
            if (source is not null)
                result.Add(CreateGpioMappingHandler.ToDto(mapping, source));
        }

        return result;
    }
}
