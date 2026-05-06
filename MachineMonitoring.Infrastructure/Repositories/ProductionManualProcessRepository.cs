using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class ProductionManualProcessRepository : IProductionManualProcessRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public ProductionManualProcessRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductionManualProcess?> GetByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionManualProcesses.FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId, cancellationToken);

    public Task<ProductionManualProcess?> GetActiveByOrderIdAsync(Guid productionOrderId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductionManualProcesses.FirstOrDefaultAsync(
            x => x.ProductionOrderId == productionOrderId && x.FinishedAt == null,
            cancellationToken);

    public async Task AddAsync(ProductionManualProcess manualProcess, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductionManualProcesses.AddAsync(manualProcess, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductionManualProcess manualProcess, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductionManualProcesses.Update(manualProcess);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}