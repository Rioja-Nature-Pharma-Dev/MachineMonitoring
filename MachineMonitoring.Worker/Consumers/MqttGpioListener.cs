using System.Text;
using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Application.UseCases.Production;
using MQTTnet;
using MQTTnet.Client;

namespace MachineMonitoring.Worker.Consumers;

public sealed class MqttGpioListener
{
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _productionOrderRepository;
    private readonly IProductionCounterRepository _productionCounterRepository;
    private readonly IClock _clock;
    private readonly string _mqttBroker;
    private readonly int _mqttPort;
    private readonly string _machineCode;
    private IMqttClient? _mqttClient;

    public MqttGpioListener(
        IMachineRepository machineRepository,
        IProductionOrderRepository productionOrderRepository,
        IProductionCounterRepository productionCounterRepository,
        IClock clock,
        string mqttBroker,
        int mqttPort,
        string machineCode)
    {
        _machineRepository = machineRepository;
        _productionOrderRepository = productionOrderRepository;
        _productionCounterRepository = productionCounterRepository;
        _clock = clock;
        _mqttBroker = mqttBroker;
        _mqttPort = mqttPort;
        _machineCode = machineCode;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBroker, _mqttPort)
                .WithClientId($"MachineMonitoring-{_machineCode}")
                .Build();

            // Subscribe to application messages
            _mqttClient.ApplicationMessageReceivedAsync += OnMqttMessageReceived;

            await _mqttClient.ConnectAsync(options, cancellationToken);

            // Subscribe to GPIO topics for Cremer
            // GPIO 23: Unit count (falling edge detection)
            // GPIO 22: Weight failure
            // GPIO 19: Label failure
            var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic($"cremer/gpio/23"))
                .WithTopicFilter(f => f.WithTopic($"cremer/gpio/22"))
                .WithTopicFilter(f => f.WithTopic($"cremer/gpio/19"))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);

            Console.WriteLine($"[MQTT] Conectado a {_mqttBroker}:{_mqttPort}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MQTT Error] Error al iniciar listener: {ex.Message}");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
        }
    }

    private async Task OnMqttMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

            Console.WriteLine($"[MQTT] Mensaje recibido: {topic} = {payload}");

            if (topic == "cremer/gpio/23" && int.TryParse(payload, out var unitCount) && unitCount == 1)
            {
                // GPIO 23: Unit count (falling edge)
                await HandleUnitCountEvent();
            }
            else if (topic == "cremer/gpio/22" && int.TryParse(payload, out var weightError) && weightError == 1)
            {
                // GPIO 22: Weight failure
                await HandleWeightErrorEvent();
            }
            else if (topic == "cremer/gpio/19" && int.TryParse(payload, out var labelError) && labelError == 1)
            {
                // GPIO 19: Label failure
                await HandleLabelErrorEvent();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MQTT Error] Error al procesar mensaje: {ex.Message}");
        }
    }

    private async Task HandleUnitCountEvent()
    {
        try
        {
            // Get Cremer machine
            var machine = await _machineRepository.GetByCodeAsync(_machineCode);
            if (machine == null)
            {
                Console.WriteLine($"[GPIO 23] Máquina {_machineCode} no encontrada");
                return;
            }

            // Get active production order
            var orders = await _productionOrderRepository.GetByStatusAsync(
                Domain.Enums.ProductionOrderStatus.InProgress);
            var activeOrder = orders.FirstOrDefault(o => o.MachineId == machine.Id);

            if (activeOrder == null)
            {
                Console.WriteLine("[GPIO 23] No hay orden activa");
                return;
            }

            // Increment counter
            var counter = await _productionCounterRepository.GetByOrderIdAsync(activeOrder.Id);
            if (counter == null)
            {
                Console.WriteLine("[GPIO 23] Contador no encontrado");
                return;
            }

            var handler = new IncrementProductionCounterHandler(_productionCounterRepository, _clock);
            var command = new IncrementProductionCounterCommand(activeOrder.Id);
            await handler.HandleAsync(command);

            Console.WriteLine($"[GPIO 23] Contador incrementado para orden {activeOrder.OrderCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPIO 23 Error] {ex.Message}");
        }
    }

    private async Task HandleWeightErrorEvent()
    {
        Console.WriteLine("[GPIO 22] Error de peso detectado");
        // TODO: Implement weight error handling logic
        // Could mark units as bad in the counter, trigger alert, etc.
        await Task.CompletedTask;
    }

    private async Task HandleLabelErrorEvent()
    {
        Console.WriteLine("[GPIO 19] Error de etiqueta detectado");
        // TODO: Implement label error handling logic
        // Could mark units as bad in the counter, trigger alert, etc.
        await Task.CompletedTask;
    }
}
