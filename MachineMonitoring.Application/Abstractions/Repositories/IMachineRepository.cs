using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Application.Abstractions.Repositories;

public interface IMachineRepository
{
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Machine?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}