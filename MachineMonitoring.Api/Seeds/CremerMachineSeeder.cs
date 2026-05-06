using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Api.Seeds;

public sealed class CremerMachineSeeder
{
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;

    public CremerMachineSeeder(
        IMachineRepository machineRepository,
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IClock clock)
    {
        _machineRepository = machineRepository;
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Check if Cremer machine already exists
        var cremer = await _machineRepository.GetByCodeAsync("CREMER", cancellationToken);
        if (cremer != null)
            return; // Already seeded

        // Register Cremer machine
        var cremerMachine = new Machine(
            Guid.NewGuid(),
            "CREMER",
            "Cremer - Máquina de Empaque",
            "Máquina de empaque y producción con integración MQTT. GPIO: 23 (conteo), 22 (error peso), 19 (error etiqueta)",
            MachineStatus.Active,
            _clock.UtcNow);

        await _machineRepository.AddAsync(cremerMachine, cancellationToken);

        // Create test production order
        var testOrder = new ProductionOrder(
            Guid.NewGuid(),
            cremerMachine.Id,
            "CREMER-TEST-001",
            "Sistema",
            "TEST",
            "PRODUCTO-TEST",
            "Orden de prueba para validación de flujo",
            1000,
            10,
            "Caja Standard",
            false,
            false,
            null,
            null,
            null,
            null,
            1.0m,
            60m,
            _clock.UtcNow);

        await _productionOrderRepository.AddAsync(testOrder, cancellationToken);

        // Create counter for test order
        var testCounter = new ProductionCounter(
            Guid.NewGuid(),
            testOrder.Id,
            0,
            true,
            _clock.UtcNow,
            _clock.UtcNow,
            null);

        await _productionCounterRepository.AddAsync(testCounter, cancellationToken);
    }
}
