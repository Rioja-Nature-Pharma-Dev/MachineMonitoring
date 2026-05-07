using MachineMonitoring.Application.Handlers.MachineConfiguration;
using MachineMonitoring.Contracts.MachineConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

/// <summary>
/// Configuracion avanzada de maquinas: parametros, GPIOs, metricas calculadas
/// </summary>
[ApiController]
[Route("api/machines/{machineCode}/config")]
public sealed class MachineConfigurationController : ControllerBase
{
    [HttpPost("parameters")]
    public async Task<ActionResult<ParameterDefinitionDto>> AddParameter(
        string machineCode,
        [FromBody] CreateParameterDefinitionRequest request,
        [FromServices] AddParameterDefinitionHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new AddParameterDefinitionCommand(machineCode, request),
                cancellationToken);
            return CreatedAtAction(nameof(GetParameter), new { machineCode, parameterId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("parameters/{parameterId}")]
    public async Task<ActionResult<ParameterDefinitionDto>> GetParameter(
        string machineCode,
        Guid parameterId,
        [FromServices] GetParameterDefinitionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetParameterDefinitionQuery(machineCode, parameterId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("parameters")]
    public async Task<ActionResult<IEnumerable<ParameterDefinitionDto>>> ListParameters(
        string machineCode,
        [FromServices] ListParametersHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListParametersQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("gpio-mappings")]
    public async Task<ActionResult<GpioMappingDto>> MapGpio(
        string machineCode,
        [FromBody] CreateGpioMappingRequest request,
        [FromServices] CreateGpioMappingHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new CreateGpioMappingCommand(machineCode, request),
                cancellationToken);
            return CreatedAtAction(nameof(GetGpioMapping), new { machineCode, mappingId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("gpio-mappings/{mappingId}")]
    public async Task<ActionResult<GpioMappingDto>> GetGpioMapping(
        string machineCode,
        Guid mappingId,
        [FromServices] GetGpioMappingHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetGpioMappingQuery(machineCode, mappingId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("gpio-mappings")]
    public async Task<ActionResult<IEnumerable<GpioMappingDto>>> ListGpioMappings(
        string machineCode,
        [FromServices] ListGpioMappingsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListGpioMappingsQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("calculated-metrics")]
    public async Task<ActionResult<CalculatedMetricDto>> CreateCalculatedMetric(
        string machineCode,
        [FromBody] CreateCalculatedMetricRequest request,
        [FromServices] CreateCalculatedMetricHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new CreateCalculatedMetricCommand(machineCode, request),
                cancellationToken);
            return CreatedAtAction(nameof(GetCalculatedMetric), new { machineCode, metricId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("calculated-metrics/{metricId}")]
    public async Task<ActionResult<CalculatedMetricDto>> GetCalculatedMetric(
        string machineCode,
        Guid metricId,
        [FromServices] GetCalculatedMetricHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetCalculatedMetricQuery(machineCode, metricId),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("calculated-metrics")]
    public async Task<ActionResult<IEnumerable<CalculatedMetricDto>>> ListCalculatedMetrics(
        string machineCode,
        [FromServices] ListCalculatedMetricsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListCalculatedMetricsQuery(machineCode),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("")]
    public async Task<ActionResult<MachineConfigurationDto>> GetCompleteConfiguration(
        string machineCode,
        [FromServices] GetMachineConfigurationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetMachineConfigurationQuery(machineCode),
            cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Evalua todas las metricas calculadas con los valores actuales
    /// </summary>
    /// <remarks>
    /// Envia los valores de los parametros y el sistema calculara
    /// todas las metricas configuradas (incluyendo dependencias entre ellas).
    /// </remarks>
    [HttpPost("evaluate-metrics")]
    public async Task<ActionResult<EvaluateMetricsResultDto>> EvaluateMetrics(
        string machineCode,
        [FromBody] Dictionary<string, decimal> inputValues,
        [FromServices] EvaluateMetricsHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new EvaluateMetricsCommand(machineCode, inputValues),
                cancellationToken);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
