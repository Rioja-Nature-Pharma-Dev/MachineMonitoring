using MachineMonitoring.Contracts.MachineConfiguration;

namespace MachineMonitoring.Application.Handlers.MachineConfiguration;

// Commands
public sealed record AddParameterDefinitionCommand(
    string MachineCode,
    CreateParameterDefinitionRequest Request
);

public sealed record CreateGpioMappingCommand(
    string MachineCode,
    CreateGpioMappingRequest Request
);

public sealed record CreateCalculatedMetricCommand(
    string MachineCode,
    CreateCalculatedMetricRequest Request
);

// Queries
public sealed record GetParameterDefinitionQuery(
    string MachineCode,
    Guid ParameterId
);

public sealed record ListParametersQuery(
    string MachineCode
);

public sealed record GetGpioMappingQuery(
    string MachineCode,
    Guid MappingId
);

public sealed record ListGpioMappingsQuery(
    string MachineCode
);

public sealed record GetCalculatedMetricQuery(
    string MachineCode,
    Guid MetricId
);

public sealed record ListCalculatedMetricsQuery(
    string MachineCode
);

public sealed record GetMachineConfigurationQuery(
    string MachineCode
);

// Handler Interfaces

public interface IAddParameterDefinitionHandler
{
    Task<ParameterDefinitionDto> HandleAsync(
        AddParameterDefinitionCommand command,
        CancellationToken cancellationToken);
}

public interface IGetParameterDefinitionHandler
{
    Task<ParameterDefinitionDto?> HandleAsync(
        GetParameterDefinitionQuery query,
        CancellationToken cancellationToken);
}

public interface IListParametersHandler
{
    Task<IReadOnlyList<ParameterDefinitionDto>> HandleAsync(
        ListParametersQuery query,
        CancellationToken cancellationToken);
}

public interface ICreateGpioMappingHandler
{
    Task<GpioMappingDto> HandleAsync(
        CreateGpioMappingCommand command,
        CancellationToken cancellationToken);
}

public interface IGetGpioMappingHandler
{
    Task<GpioMappingDto?> HandleAsync(
        GetGpioMappingQuery query,
        CancellationToken cancellationToken);
}

public interface IListGpioMappingsHandler
{
    Task<IReadOnlyList<GpioMappingDto>> HandleAsync(
        ListGpioMappingsQuery query,
        CancellationToken cancellationToken);
}

public interface ICreateCalculatedMetricHandler
{
    Task<CalculatedMetricDto> HandleAsync(
        CreateCalculatedMetricCommand command,
        CancellationToken cancellationToken);
}

public interface IGetCalculatedMetricHandler
{
    Task<CalculatedMetricDto?> HandleAsync(
        GetCalculatedMetricQuery query,
        CancellationToken cancellationToken);
}

public interface IListCalculatedMetricsHandler
{
    Task<IReadOnlyList<CalculatedMetricDto>> HandleAsync(
        ListCalculatedMetricsQuery query,
        CancellationToken cancellationToken);
}

public interface IGetMachineConfigurationHandler
{
    Task<MachineConfigurationDto?> HandleAsync(
        GetMachineConfigurationQuery query,
        CancellationToken cancellationToken);
}
