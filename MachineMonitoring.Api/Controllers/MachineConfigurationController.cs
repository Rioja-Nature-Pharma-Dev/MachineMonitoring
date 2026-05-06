using MachineMonitoring.Application.Handlers.MachineConfiguration;
using MachineMonitoring.Contracts.MachineConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

/// <summary>
/// Configuración avanzada de máquinas
/// Define parámetros, GPIOs, cálculos personalizados y reglas
/// </summary>
[ApiController]
[Route("api/machines/{machineCode}/config")]
public sealed class MachineConfigurationController : ControllerBase
{
    /// <summary>
    /// Agregar definición de parámetro a una máquina
    /// </summary>
    /// <remarks>
    /// Define un parámetro que la máquina puede medir o calcular
    /// Ejemplo: Temperatura, Presión, Velocidad, etc.
    /// </remarks>
    [HttpPost("parameters")]
    [ProducesResponseType(typeof(ParameterDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParameterDefinitionDto>> AddParameter(
        string machineCode,
        [FromBody] CreateParameterDefinitionRequest request,
        [FromServices] IAddParameterDefinitionHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddParameterDefinitionCommand(machineCode, request);
            var result = await handler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetParameter), new { machineCode, parameterId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener definición de parámetro
    /// </summary>
    [HttpGet("parameters/{parameterId}")]
    [ProducesResponseType(typeof(ParameterDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParameterDefinitionDto>> GetParameter(
        string machineCode,
        Guid parameterId,
        [FromServices] IGetParameterDefinitionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetParameterDefinitionQuery(machineCode, parameterId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Listar todos los parámetros de una máquina
    /// </summary>
    [HttpGet("parameters")]
    [ProducesResponseType(typeof(IEnumerable<ParameterDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParameterDefinitionDto>>> ListParameters(
        string machineCode,
        [FromServices] IListParametersHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListParametersQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Mapear entrada/GPIO a un parámetro
    /// </summary>
    /// <remarks>
    /// Mapea un GPIO (o MQTT topic) a un parámetro de máquina
    /// Soporta transformaciones: "valor * 1.5 + 10"
    /// </remarks>
    [HttpPost("gpio-mappings")]
    [ProducesResponseType(typeof(GpioMappingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GpioMappingDto>> MapGpio(
        string machineCode,
        [FromBody] CreateGpioMappingRequest request,
        [FromServices] ICreateGpioMappingHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateGpioMappingCommand(machineCode, request);
            var result = await handler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetGpioMapping), new { machineCode, mappingId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener mapeo de GPIO
    /// </summary>
    [HttpGet("gpio-mappings/{mappingId}")]
    [ProducesResponseType(typeof(GpioMappingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GpioMappingDto>> GetGpioMapping(
        string machineCode,
        Guid mappingId,
        [FromServices] IGetGpioMappingHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetGpioMappingQuery(machineCode, mappingId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Listar todos los mapeos de GPIO
    /// </summary>
    [HttpGet("gpio-mappings")]
    [ProducesResponseType(typeof(IEnumerable<GpioMappingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GpioMappingDto>>> ListGpioMappings(
        string machineCode,
        [FromServices] IListGpioMappingsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListGpioMappingsQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Definir cálculo personalizado (métrica)
    /// </summary>
    /// <remarks>
    /// Define una fórmula para calcular métricas personalizadas
    /// Ejemplo: "OEE = (availability * performance * quality) / 10000"
    /// </remarks>
    [HttpPost("calculated-metrics")]
    [ProducesResponseType(typeof(CalculatedMetricDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculatedMetricDto>> CreateCalculatedMetric(
        string machineCode,
        [FromBody] CreateCalculatedMetricRequest request,
        [FromServices] ICreateCalculatedMetricHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateCalculatedMetricCommand(machineCode, request);
            var result = await handler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetCalculatedMetric), new { machineCode, metricId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener definición de métrica calculada
    /// </summary>
    [HttpGet("calculated-metrics/{metricId}")]
    [ProducesResponseType(typeof(CalculatedMetricDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CalculatedMetricDto>> GetCalculatedMetric(
        string machineCode,
        Guid metricId,
        [FromServices] IGetCalculatedMetricHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetCalculatedMetricQuery(machineCode, metricId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Listar métricas calculadas
    /// </summary>
    [HttpGet("calculated-metrics")]
    [ProducesResponseType(typeof(IEnumerable<CalculatedMetricDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CalculatedMetricDto>>> ListCalculatedMetrics(
        string machineCode,
        [FromServices] IListCalculatedMetricsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListCalculatedMetricsQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener configuración completa de la máquina
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(MachineConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineConfigurationDto>> GetCompleteConfiguration(
        string machineCode,
        [FromServices] IGetMachineConfigurationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetMachineConfigurationQuery(machineCode),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }
}
