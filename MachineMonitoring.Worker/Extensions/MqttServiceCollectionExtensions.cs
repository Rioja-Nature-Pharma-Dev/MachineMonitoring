using MachineMonitoring.Worker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MachineMonitoring.Worker.Extensions;

public static class MqttServiceCollectionExtensions
{
    public static IServiceCollection AddMqttListener(this IServiceCollection services)
    {
        services.AddHostedService<MqttListenerHostedService>();
        return services;
    }
}
