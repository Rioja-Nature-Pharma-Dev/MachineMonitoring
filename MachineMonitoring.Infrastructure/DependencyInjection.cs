using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Infrastructure.Persistence;
using MachineMonitoring.Infrastructure.Persistence.Repositories;
using MachineMonitoring.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MachineMonitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MachineMonitoringDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IProductionPauseRepository, ProductionPauseRepository>();
        services.AddScoped<IProductionCounterRepository, ProductionCounterRepository>();
        services.AddScoped<IProductionMetricsRepository, ProductionMetricsRepository>();
        services.AddScoped<IProductionManualProcessRepository, ProductionManualProcessRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}