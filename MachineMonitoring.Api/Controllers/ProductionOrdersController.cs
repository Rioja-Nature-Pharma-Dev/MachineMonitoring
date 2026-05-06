using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Application.UseCases.Production;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionOrdersController : ControllerBase
{
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IProductionMetricsRepository _productionMetricsRepository;
    private readonly IProductionPauseRepository _productionPauseRepository;
    private readonly IClock _clock;

    public ProductionOrdersController(
        IMachineRepository machineRepository,
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IProductionMetricsRepository productionMetricsRepository,
        IProductionPauseRepository productionPauseRepository,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _productionMetricsRepository = productionMetricsRepository;
        _productionPauseRepository = productionPauseRepository;
        _clock = clock;
    }

    /// <summary>
    /// Crea una nueva orden de producción
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductionOrderDto>> CreateOrder(
        [FromBody] CreateProductionOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByIdAsync(request.MachineId, cancellationToken);
        if (machine == null)
            return BadRequest($"Máquina con ID {request.MachineId} no encontrada");

        var exists = await _productionOrderRepository.ExistsByOrderCodeAsync(request.OrderCode, cancellationToken);
        if (exists)
            return BadRequest($"Orden de producción con código {request.OrderCode} ya existe");

        var handler = new CreateProductionOrderHandler(_machineRepository, _productionOrderRepository, _clock);
        var command = new CreateProductionOrderCommand(
            request.MachineId,
            request.OrderCode,
            request.OperatorName,
            request.Batch,
            request.Article,
            request.Description,
            request.PlannedQuantity,
            request.UnitsPerBox,
            request.BoxType,
            request.RequiresReprocess,
            request.RequiresManualProcess,
            request.FinalBoxCount,
            request.BottleFormat,
            request.ProductType,
            request.UnitsPerBottle,
            request.StandardReference,
            request.EstimatedMinutes);

        var result = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene una orden de producción por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionOrderDto>> GetOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var handler = new GetProductionOrderByIdHandler(_productionOrderRepository);
        var query = new GetProductionOrderByIdQuery(id);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound($"Orden de producción con ID {id} no encontrada");

        return Ok(result);
    }

    /// <summary>
    /// Inicia una orden de producción
    /// </summary>
    [HttpPost("{id}/start")]
    public async Task<ActionResult<ProductionOrderDto>> StartOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            return NotFound($"Orden de producción con ID {id} no encontrada");

        var handler = new StartProductionOrderHandler(_productionOrderRepository, _productionCounterRepository, _clock);
        var command = new StartProductionOrderCommand(id);
        var result = await handler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Finaliza una orden de producción
    /// </summary>
    [HttpPost("{id}/finish")]
    public async Task<ActionResult<ProductionOrderDto>> FinishOrder(
        Guid id,
        [FromBody] FinishOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            return NotFound($"Orden de producción con ID {id} no encontrada");

        var calculateMetricsHandler = new CalculateProductionMetricsHandler(_productionOrderRepository, _productionPauseRepository, _productionMetricsRepository);
        var handler = new FinishProductionOrderHandler(_productionOrderRepository, _productionPauseRepository, _productionCounterRepository, calculateMetricsHandler, _clock);
        var command = new FinishProductionOrderCommand(id, request.GoodUnits, request.BadUnits, request.FinalBoxCount, request.UnitsPerBox, request.BoxType, request.RequiresReprocess, request.RequiresManualProcess);
        var result = await handler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene la orden de producción activa para una máquina
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ProductionOrderDto>> GetActiveOrder(
        [FromQuery] Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var handler = new GetActiveProductionOrderHandler(_productionOrderRepository);
        var query = new GetActiveProductionOrderQuery(machineId);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound("No hay orden de producción activa");

        return Ok(result);
    }

    /// <summary>
    /// Incrementa el contador de unidades buenas o malas
    /// </summary>
    [HttpPost("{orderId}/counter/increment")]
    public async Task<ActionResult<ProductionCounterDto>> IncrementCounter(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return BadRequest($"Orden de producción con ID {orderId} no encontrada");

        var counter = await _productionCounterRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (counter == null)
            return BadRequest($"Contador no encontrado para la orden {orderId}");

        var handler = new IncrementProductionCounterHandler(_productionCounterRepository, _clock);
        var command = new IncrementProductionCounterCommand(orderId);
        var result = await handler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el contador activo
    /// </summary>
    [HttpGet("{orderId}/counter")]
    public async Task<ActionResult<ProductionCounterDto>> GetCounter(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var handler = new GetProductionCounterByOrderIdHandler(_productionCounterRepository);
        var query = new GetProductionCounterByOrderIdQuery(orderId);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound($"Contador no encontrado para la orden {orderId}");

        return Ok(result);
    }

    /// <summary>
    /// Obtiene las métricas de una orden
    /// </summary>
    [HttpGet("{orderId}/metrics")]
    public async Task<ActionResult<ProductionMetricsDto>> GetMetrics(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var handler = new GetProductionMetricsByOrderIdHandler(_productionMetricsRepository);
        var query = new GetProductionMetricsByOrderIdQuery(orderId);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound($"Métricas no encontradas para la orden {orderId}");

        return Ok(result);
    }
}

public sealed record CreateProductionOrderRequest(
    Guid MachineId,
    string OrderCode,
    string? OperatorName,
    string? Batch,
    string? Article,
    string? Description,
    int PlannedQuantity,
    int? UnitsPerBox,
    string? BoxType,
    bool RequiresReprocess,
    bool RequiresManualProcess,
    int? FinalBoxCount,
    string? BottleFormat,
    string? ProductType,
    int? UnitsPerBottle,
    decimal? StandardReference,
    decimal? EstimatedMinutes);

public sealed record FinishOrderRequest(
    int GoodUnits,
    int BadUnits,
    int? FinalBoxCount,
    int? UnitsPerBox,
    string? BoxType,
    bool RequiresReprocess,
    bool RequiresManualProcess);
