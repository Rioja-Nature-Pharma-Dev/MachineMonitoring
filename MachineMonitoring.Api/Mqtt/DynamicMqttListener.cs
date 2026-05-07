using System.Globalization;
using System.Text;
using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Handlers.MachineConfiguration;
using MQTTnet;
using MQTTnet.Client;

namespace MachineMonitoring.Api.Mqtt;

/// <summary>
/// MQTT listener dinamico:
/// - Al iniciar lee todos los GPIO mappings configurados
/// - Se suscribe a todos los topics
/// - Procesa mensajes usando IngestSensorReadingHandler (con transformaciones)
/// </summary>
public sealed class DynamicMqttListener : BackgroundService
{
    private readonly ILogger<DynamicMqttListener> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MqttSettings _settings;
    private IMqttClient? _client;

    public DynamicMqttListener(
        ILogger<DynamicMqttListener> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = configuration.GetSection("Mqtt").Get<MqttSettings>() ?? new MqttSettings();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("[MQTT] Listener disabled in configuration.");
            return;
        }

        // Wait for application to be fully ready (DB seeded, etc.)
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndListenAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTT] Listener error. Reconnecting in 10s...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken cancellationToken)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Broker, _settings.Port)
            .WithClientId($"{_settings.ClientId}-{Guid.NewGuid():N}".Substring(0, 23))
            .WithCleanSession(true)
            .Build();

        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        _logger.LogInformation("[MQTT] Connecting to {Broker}:{Port}...", _settings.Broker, _settings.Port);
        await _client.ConnectAsync(options, cancellationToken);
        _logger.LogInformation("[MQTT] Connected.");

        // Subscribe to all configured topics
        var topics = await GetConfiguredTopicsAsync(cancellationToken);
        foreach (var topic in topics)
        {
            var subOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic))
                .Build();
            await _client.SubscribeAsync(subOptions, cancellationToken);
            _logger.LogInformation("[MQTT] Subscribed to: {Topic}", topic);
        }

        // Keep alive
        while (_client.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task<List<string>> GetConfiguredTopicsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<IMachineInputSourceRepository>();
        var machineRepo = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

        var topics = new HashSet<string>();
        var machines = await machineRepo.GetAllAsync(cancellationToken);

        // For each configured topic across all machines (from input sources)
        // We use a generic wildcard fallback if no specific topics
        var mappingRepo = scope.ServiceProvider.GetRequiredService<IMachineInputMappingRepository>();
        foreach (var machine in machines)
        {
            var mappings = await mappingRepo.GetByMachineIdAsync(machine.Id, cancellationToken);
            foreach (var mapping in mappings.Where(m => m.IsEnabled))
            {
                var source = await sourceRepo.GetByIdAsync(mapping.InputSourceId, cancellationToken);
                if (source is not null)
                    topics.Add(source.EndpointOrTopic);
            }
        }

        // Fallback: subscribe to wildcard if no mappings yet
        if (topics.Count == 0 && _settings.FallbackTopics?.Length > 0)
        {
            foreach (var t in _settings.FallbackTopics)
                topics.Add(t);
        }

        return topics.ToList();
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

        _logger.LogInformation("[MQTT] Message: {Topic} = {Payload}", topic, payload);

        if (!decimal.TryParse(payload, NumberStyles.Any, CultureInfo.InvariantCulture, out var rawValue))
        {
            _logger.LogWarning("[MQTT] Could not parse '{Payload}' as decimal.", payload);
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IngestSensorReadingHandler>();
            var sourceRepo = scope.ServiceProvider.GetRequiredService<IMachineInputSourceRepository>();
            var mappingRepo = scope.ServiceProvider.GetRequiredService<IMachineInputMappingRepository>();
            var machineRepo = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            // Find which machine owns this topic
            var source = await sourceRepo.GetByTopicAsync(topic, default);
            if (source is null)
            {
                _logger.LogWarning("[MQTT] No mapping configured for topic: {Topic}", topic);
                return;
            }

            var machine = await machineRepo.GetByIdAsync(source.MachineId, default);
            if (machine is null) return;

            var result = await handler.HandleAsync(
                new IngestSensorReadingCommand(machine.Code, topic, rawValue),
                default);

            if (result is not null)
            {
                _logger.LogInformation(
                    "[MQTT] {Machine} | {Param} = {Transformed} (raw={Raw}, transform={Expr})",
                    result.MachineCode,
                    result.ParameterCode,
                    result.TransformedValue,
                    result.RawValue,
                    result.TransformExpression ?? "none");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT] Failed to process message from {Topic}", topic);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected == true)
        {
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}

public sealed class MqttSettings
{
    public bool Enabled { get; set; } = true;
    public string Broker { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "MachineMonitoring";
    public string[]? FallbackTopics { get; set; }
}
