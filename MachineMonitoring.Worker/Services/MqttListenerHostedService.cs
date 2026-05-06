using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Worker.Consumers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace MachineMonitoring.Worker.Services;

public sealed class MqttListenerHostedService : BackgroundService
{
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;
    private readonly IConfiguration _configuration;
    private MqttGpioListener? _mqttListener;

    public MqttListenerHostedService(
        IMachineRepository machineRepository,
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IClock clock,
        IConfiguration configuration)
    {
        _machineRepository = machineRepository;
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttBroker = _configuration["Mqtt:Broker"] ?? "localhost";
        var mqttPort = int.TryParse(_configuration["Mqtt:Port"], out var port) ? port : 1883;
        var machineCode = _configuration["Mqtt:MachineCode"] ?? "CREMER";

        _mqttListener = new MqttGpioListener(
            _machineRepository,
            _productionOrderRepository,
            _productionCounterRepository,
            _clock,
            mqttBroker,
            mqttPort,
            machineCode);

        try
        {
            await _mqttListener.StartAsync(stoppingToken);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Service is shutting down
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MqttListenerHostedService] Error: {ex.Message}");
        }
        finally
        {
            if (_mqttListener != null)
            {
                await _mqttListener.StopAsync(stoppingToken);
            }
        }
    }
}
