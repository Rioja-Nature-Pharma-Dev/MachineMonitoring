using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IMachineRepository
{
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Machine?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Machine>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Machine machine, CancellationToken cancellationToken = default);
}