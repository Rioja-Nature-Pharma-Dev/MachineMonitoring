using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public ProductionOrderRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ProductionOrder?> GetByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.OrderCode == orderCode, cancellationToken);

    public async Task<IReadOnlyList<ProductionOrder>> GetByStatusAsync(
        ProductionOrderStatus status,
        CancellationToken cancellationToken = default) =>
        await _dbContext.ProductionOrders
            .Where(x => x.Status == status)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.ProductionOrders.ToListAsync(cancellationToken);

    public Task<bool> ExistsByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionOrders.AnyAsync(x => x.OrderCode == orderCode, cancellationToken);

    public async Task AddAsync(ProductionOrder order, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductionOrders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionOrders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}