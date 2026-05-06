namespace MachineMonitoring.Contracts.MachineConfiguration;

/// <summary>
/// Definición de parámetro de máquina
/// </summary>
public sealed record ParameterDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    string DataType,
    bool IsRequired,
    decimal? MinValue,
    decimal? MaxValue,
    bool IsCalculated
);

/// <summary>
/// Crear definición de parámetro
/// </summary>
public sealed record CreateParameterDefinitionRequest(
    string Code,
    string Name,
    string Unit,
    string DataType,
    bool IsRequired,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    bool IsCalculated = false
);

/// <summary>
/// Mapeo de GPIO a parámetro
/// </summary>
public sealed record GpioMappingDto(
    Guid Id,
    string GpioTopic,
    string ParameterCode,
    string? TransformExpression,
    bool IsEnabled
);

/// <summary>
/// Crear mapeo de GPIO
/// </summary>
public sealed record CreateGpioMappingRequest(
    string GpioTopic,
    string ParameterCode,
    string? TransformExpression = null,
    bool IsEnabled = true
);

/// <summary>
/// Definición de métrica calculada
/// </summary>
public sealed record CalculatedMetricDto(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    string FormulaExpression,
    bool IsEnabled
);

/// <summary>
/// Crear métrica calculada
/// </summary>
public sealed record CreateCalculatedMetricRequest(
    string Code,
    string Name,
    string Unit,
    string FormulaExpression,
    bool IsEnabled = true
);

/// <summary>
/// Configuración completa de máquina
/// </summary>
public sealed record MachineConfigurationDto(
    string MachineCode,
    string MachineName,
    IReadOnlyList<ParameterDefinitionDto> Parameters,
    IReadOnlyList<GpioMappingDto> GpioMappings,
    IReadOnlyList<CalculatedMetricDto> CalculatedMetrics
);
