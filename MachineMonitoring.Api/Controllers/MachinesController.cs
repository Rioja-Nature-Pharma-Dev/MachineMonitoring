using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MachineMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMachineRepository _machineRepository;
    private readonly IClock _clock;

    public MachinesController(IMachineRepository machineRepository, IClock clock)
    {
        _machineRepository = machineRepository;
        _clock = clock;
    }

    /// <summary>
    /// Registra una nueva máquina en el sistema
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MachineResponseDto>> RegisterMachine(
        [FromBody] RegisterMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _machineRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing != null)
            return BadRequest($"Máquina con código {request.Code} ya existe");

        var machine = new Machine(
            Guid.NewGuid(),
            request.Code,
            request.Name ?? request.Code,
            request.Description,
            MachineStatus.Active,
            _clock.UtcNow);

        await _machineRepository.AddAsync(machine, cancellationToken);

        return CreatedAtAction(nameof(GetMachine), new { code = machine.Code },
            new MachineResponseDto(machine.Id, machine.Code, machine.Name, machine.Description, machine.Status.ToString(), machine.CreatedAt));
    }

    /// <summary>
    /// Obtiene una máquina por código
    /// </summary>
    [HttpGet("{code}")]
    public async Task<ActionResult<MachineResponseDto>> GetMachine(
        string code,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineRepository.GetByCodeAsync(code, cancellationToken);
        if (machine == null)
            return NotFound($"Máquina con código {code} no encontrada");

        return Ok(new MachineResponseDto(machine.Id, machine.Code, machine.Name, machine.Description, machine.Status.ToString(), machine.CreatedAt));
    }

    /// <summary>
    /// Obtiene todas las máquinas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MachineResponseDto>>> GetAllMachines(
        CancellationToken cancellationToken = default)
    {
        var machines = await _machineRepository.GetAllAsync(cancellationToken);
        var dtos = machines.Select(m => new MachineResponseDto(
            m.Id, m.Code, m.Name, m.Description, m.Status.ToString(), m.CreatedAt)).ToList();

        return Ok(dtos);
    }
}

public sealed record RegisterMachineRequest(string Code, string? Name, string? Description);

public sealed record MachineResponseDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);
