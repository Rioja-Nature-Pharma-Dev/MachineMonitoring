using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MachineMonitoring.Infrastructure.Persistence.Repositories;

public sealed class MachineParameterDefinitionRepository : IMachineParameterDefinitionRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public MachineParameterDefinitionRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MachineParameterDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MachineParameterDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MachineParameterDefinition>> GetByMachineIdAsync(
        Guid machineId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.MachineParameterDefinitions
            .Where(x => x.MachineId == machineId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.MachineParameterDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.MachineParameterDefinitions.Update(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class MachineInputMappingRepository : IMachineInputMappingRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public MachineInputMappingRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MachineInputMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MachineInputMappings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MachineInputMapping>> GetByMachineIdAsync(
        Guid machineId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.MachineInputMappings
            .Where(x => x.MachineId == machineId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MachineInputMapping mapping, CancellationToken cancellationToken = default)
    {
        _dbContext.MachineInputMappings.Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class MachineInputSourceRepository : IMachineInputSourceRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public MachineInputSourceRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MachineInputSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MachineInputSources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<MachineInputSource?> GetByTopicAsync(string topic, CancellationToken cancellationToken = default) =>
        _dbContext.MachineInputSources.FirstOrDefaultAsync(x => x.EndpointOrTopic == topic, cancellationToken);

    public async Task AddAsync(MachineInputSource source, CancellationToken cancellationToken = default)
    {
        _dbContext.MachineInputSources.Add(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class CalculatedMetricDefinitionRepository : ICalculatedMetricDefinitionRepository
{
    private readonly MachineMonitoringDbContext _dbContext;

    public CalculatedMetricDefinitionRepository(MachineMonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CalculatedMetricDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.CalculatedMetricDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CalculatedMetricDefinition>> GetByMachineIdAsync(
        Guid machineId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.CalculatedMetricDefinitions
            .Where(x => x.MachineId == machineId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CalculatedMetricDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.CalculatedMetricDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
