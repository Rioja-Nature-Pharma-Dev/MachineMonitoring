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
