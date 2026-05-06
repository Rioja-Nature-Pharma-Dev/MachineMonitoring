using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class MachineRepository : IMachineRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public MachineRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Machines.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Machine?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.Machines.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
}