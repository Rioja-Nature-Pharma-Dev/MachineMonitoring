using MachineMonitoring.Application.Abstractions.Services;

namespace MachineMonitoring.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}