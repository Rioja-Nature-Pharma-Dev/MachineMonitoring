using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IProductionCounterRepository
{
    Task<ProductionCounter?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default);
    Task<ProductionCounter?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProductionCounter counter, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionCounter counter, CancellationToken cancellationToken = default);
}