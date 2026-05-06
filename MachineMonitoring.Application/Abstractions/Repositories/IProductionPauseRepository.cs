using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IProductionPauseRepository
{
    Task<ProductionPause?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductionPause?> GetActiveByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionPause>> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionPause pause, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionPause pause, CancellationToken cancellationToken = default);
}