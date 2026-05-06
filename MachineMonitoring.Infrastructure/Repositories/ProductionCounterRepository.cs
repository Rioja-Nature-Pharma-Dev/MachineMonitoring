using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class ProductionCounterRepository : IProductionCounterRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public ProductionCounterRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductionCounter?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionCounters.FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId, cancellationToken);

    public Task<ProductionCounter?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _dbContext.ProductionCounters.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

    public async Task AddAsync(ProductionCounter counter, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductionCounters.AddAsync(counter, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionCounter counter, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionCounters.Update(counter);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}