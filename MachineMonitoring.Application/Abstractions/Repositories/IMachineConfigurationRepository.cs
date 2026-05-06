using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IMachineParameterDefinitionRepository
{
    Task<MachineParameterDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MachineParameterDefinition>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task AddAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default);
    Task UpdateAsync(MachineParameterDefinition definition, CancellationToken cancellationToken = default);
}

public interface IMachineInputMappingRepository
{
    Task<MachineInputMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MachineInputMapping>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task AddAsync(MachineInputMapping mapping, CancellationToken cancellationToken = default);
}

public interface IMachineInputSourceRepository
{
    Task<MachineInputSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MachineInputSource?> GetByTopicAsync(string topic, CancellationToken cancellationToken = default);
    Task AddAsync(MachineInputSource source, CancellationToken cancellationToken = default);
}

public interface ICalculatedMetricDefinitionRepository
{
    Task<CalculatedMetricDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalculatedMetricDefinition>> GetByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task AddAsync(CalculatedMetricDefinition definition, CancellationToken cancellationToken = default);
}
