using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductionOrder?> GetByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionOrder>> GetByStatusAsync(
        ProductionOrderStatus status,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default);
}