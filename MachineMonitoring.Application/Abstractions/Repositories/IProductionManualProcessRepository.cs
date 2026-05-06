using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IProductionManualProcessRepository
{
    Task<ProductionManualProcess?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task<ProductionManualProcess?> GetActiveByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionManualProcess manualProcess, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionManualProcess manualProcess, CancellationToken cancellationToken = default);
}