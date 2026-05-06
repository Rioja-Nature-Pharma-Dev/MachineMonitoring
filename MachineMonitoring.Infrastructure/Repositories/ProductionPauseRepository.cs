using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class ProductionPauseRepository : IProductionPauseRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public ProductionPauseRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductionPause?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionPauses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ProductionPause?> GetActiveByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionPauses.FirstOrDefaultAsync(
            x => x.ProductionOrderId == productionOrderId && x.EndedAt == null,
            cancellationToken);

    public async Task<IReadOnlyList<ProductionPause>> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        await _dbContext.ProductionPauses
            .Where(x => x.ProductionOrderId == productionOrderId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ProductionPause pause, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductionPauses.AddAsync(pause, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionPause pause, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionPauses.Update(pause);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}