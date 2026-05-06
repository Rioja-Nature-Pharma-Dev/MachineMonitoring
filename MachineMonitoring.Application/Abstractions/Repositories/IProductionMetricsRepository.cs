using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IProductionMetricsRepository
{
    Task<ProductionMetrics?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionMetrics metrics, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionMetrics metrics, CancellationToken cancellationToken = default);
    Task DeleteByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
}