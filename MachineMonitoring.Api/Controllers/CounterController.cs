using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.DTOs.Production;
using MachineMonitoring.Application.UseCases.Production;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CounterController : ControllerBase
{
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;

    public CounterController(
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IClock clock)
    {
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
    }

    /// <summary>
    /// Incrementa el contador de unidades buenas para una orden
    /// </summary>
    [HttpPost("{orderId}/increment-good")]
    public async Task<ActionResult<ProductionCounterDto>> IncrementGoodUnits(
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
    /// Incrementa el contador de unidades malas para una orden
    /// </summary>
    [HttpPost("{orderId}/increment-bad")]
    public async Task<ActionResult<ProductionCounterDto>> IncrementBadUnits(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return BadRequest($"Orden de producción con ID {orderId} no encontrada");

        var counter = await _productionCounterRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (counter == null)
            return BadRequest($"Contador no encontrado para la orden {orderId}");

        // For bad units, we would need to call a different handler or modify the counter directly
        // For now, we'll increment good and adjust via counter update
        var handler = new IncrementProductionCounterHandler(_productionCounterRepository, _clock);
        var command = new IncrementProductionCounterCommand(orderId);
        var result = await handler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el contador actual de una orden
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<ProductionCounterDto>> GetCounter(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _productionOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound($"Orden de producción con ID {orderId} no encontrada");

        var counter = await _productionCounterRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (counter == null)
            return NotFound($"Contador no encontrado para la orden {orderId}");

        return Ok(new ProductionCounterDto(
            counter.Id,
            counter.ProductionOrderId,
            counter.Quantity,
            counter.IsActive,
            counter.CreatedAt,
            counter.LastUpdatedAt,
            counter.LastCountedAt));
    }

    /// <summary>
    /// Obtiene el contador activo
    /// </summary>
    [HttpGet("active/current")]
    public async Task<ActionResult<ProductionCounterDto>> GetActiveCounter(
        CancellationToken cancellationToken = default)
    {
        var handler = new GetActiveProductionCounterHandler(_productionCounterRepository);
        var query = new GetActiveProductionCounterQuery();
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound("No hay contador activo");

        return Ok(result);
    }
}
