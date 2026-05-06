using System.Collections.Concurrent;
using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Infrastructure.Repositories;

// In-memory implementations for machine configuration
// TODO: Replace with EF Core implementations when migrations are added

public sealed class InMemoryMachineParameterDefinitionRepository : IMachineParameterDefinitionRepository
{
    private readonly ConcurrentDictionary<Guid, MachineParameterDefinition> _store = new();

    public Task<MachineParameterDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var definition);
        return Task.FromResult(definition);
    }

    public Task<IReadOnlyList<MachineParameterDefinition>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        var result = _store.Values.Where(d => d.MachineId == machineId).ToList();
        return Task.FromResult<IReadOnlyList<MachineParameterDefinition>>(result);
    }

    public Task AddAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default)
    {
        _store[definition.Id] = definition;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default)
    {
        _store[definition.Id] = definition;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryMachineInputMappingRepository : IMachineInputMappingRepository
{
    private readonly ConcurrentDictionary<Guid, MachineInputMapping> _store = new();

    public Task<MachineInputMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var mapping);
        return Task.FromResult(mapping);
    }

    public Task<IReadOnlyList<MachineInputMapping>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        var result = _store.Values.Where(m => m.MachineId == machineId).ToList();
        return Task.FromResult<IReadOnlyList<MachineInputMapping>>(result);
    }

    public Task AddAsync(MachineInputMapping mapping, CancellationToken cancellationToken = default)
    {
        _store[mapping.Id] = mapping;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryMachineInputSourceRepository : IMachineInputSourceRepository
{
    private readonly ConcurrentDictionary<Guid, MachineInputSource> _store = new();

    public Task<MachineInputSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var source);
        return Task.FromResult(source);
    }

    public Task<MachineInputSource?> GetByTopicAsync(string topic, CancellationToken cancellationToken = default)
    {
        var source = _store.Values.FirstOrDefault(s => s.EndpointOrTopic == topic);
        return Task.FromResult(source);
    }

    public Task AddAsync(MachineInputSource source, CancellationToken cancellationToken = default)
    {
        _store[source.Id] = source;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryCalculatedMetricDefinitionRepository : ICalculatedMetricDefinitionRepository
{
    private readonly ConcurrentDictionary<Guid, CalculatedMetricDefinition> _store = new();

    public Task<CalculatedMetricDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var definition);
        return Task.FromResult(definition);
    }

    public Task<IReadOnlyList<CalculatedMetricDefinition>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        var result = _store.Values.Where(d => d.MachineId == machineId).ToList();
        return Task.FromResult<IReadOnlyList<CalculatedMetricDefinition>>(result);
    }

    public Task AddAsync(CalculatedMetricDefinition definition, CancellationToken cancellationToken = default)
    {
        _store[definition.Id] = definition;
        return Task.CompletedTask;
    }
}
