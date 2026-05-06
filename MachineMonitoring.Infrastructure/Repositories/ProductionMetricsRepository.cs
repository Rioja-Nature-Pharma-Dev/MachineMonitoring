using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class ProductionMetricsRepository : IProductionMetricsRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public ProductionMetricsRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductionMetrics?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionMetrics.FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId, cancellationToken);

    public Task<bool> ExistsByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionMetrics.AnyAsync(x => x.ProductionOrderId == productionOrderId, cancellationToken);

    public async Task AddAsync(ProductionMetrics metrics, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductionMetrics.AddAsync(metrics, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionMetrics metrics, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionMetrics.Update(metrics);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default)
    {
        var metrics = await _dbContext.ProductionMetrics
            .Where(x => x.ProductionOrderId == productionOrderId)
            .ToListAsync(cancellationToken);

        if (metrics.Count == 0)
            return;

        _dbContext.ProductionMetrics.RemoveRange(metrics);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}