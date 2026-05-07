using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.Handlers.MachineConfiguration;
using MachineMonitoring.Application.Services;
using MachineMonitoring.Infrastructure.Persistence;
using MachineMonitoring.Infrastructure.Persistence.Repositories;
using MachineMonitoring.Infrastructure.Repositories;
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

        // Production repositories (EF Core)
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IProductionPauseRepository, ProductionPauseRepository>();
        services.AddScoped<IProductionCounterRepository, ProductionCounterRepository>();
        services.AddScoped<IProductionMetricsRepository, ProductionMetricsRepository>();
        services.AddScoped<IProductionManualProcessRepository, ProductionManualProcessRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();

        // Configuration repositories (in-memory, singleton for persistence across requests)
        services.AddSingleton<IMachineParameterDefinitionRepository, InMemoryMachineParameterDefinitionRepository>();
        services.AddSingleton<IMachineInputMappingRepository, InMemoryMachineInputMappingRepository>();
        services.AddSingleton<IMachineInputSourceRepository, InMemoryMachineInputSourceRepository>();
        services.AddSingleton<ICalculatedMetricDefinitionRepository, InMemoryCalculatedMetricDefinitionRepository>();

        // Configuration handlers
        services.AddScoped<AddParameterDefinitionHandler>();
        services.AddScoped<GetParameterDefinitionHandler>();
        services.AddScoped<ListParametersHandler>();
        services.AddScoped<CreateGpioMappingHandler>();
        services.AddScoped<GetGpioMappingHandler>();
        services.AddScoped<ListGpioMappingsHandler>();
        services.AddScoped<CreateCalculatedMetricHandler>();
        services.AddScoped<GetCalculatedMetricHandler>();
        services.AddScoped<ListCalculatedMetricsHandler>();
        services.AddScoped<GetMachineConfigurationHandler>();
        services.AddScoped<EvaluateMetricsHandler>();

        // Formula evaluator
        services.AddSingleton<FormulaEvaluator>();

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}