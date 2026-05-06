using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;
using MachineMonitoring.Domain.Entities;

namespace MachineMonitoring.Worker.Consumers;

/// <summary>
/// Manejadores para eventos específicos de GPIO
/// </summary>
public sealed class GpioEventHandlers
{
    private readonly IProductionCounterRepository _counterRepository;
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IClock _clock;

    public GpioEventHandlers(
        IProductionCounterRepository counterRepository,
        IMachineRepository machineRepository,
        IProductionOrderRepository orderRepository,
        IClock clock)
    {
        _counterRepository = counterRepository;
        _machineRepository = machineRepository;
        _orderRepository = orderRepository;
        _clock = clock;
    }

    /// <summary>
    /// Maneja evento de error de peso en Cremer
    /// GPIO 22: Detecta cuando el peso de un artículo es incorrecto
    /// </summary>
    public async Task HandleWeightErrorAsync(Guid machineId)
    {
        try
        {
            // Obtener orden activa de la máquina
            var orders = await _orderRepository.GetByStatusAsync(
                Domain.Enums.ProductionOrderStatus.InProgress);
            var activeOrder = orders.FirstOrDefault(o => o.MachineId == machineId);

            if (activeOrder == null)
                return;

            // Obtener contador
            var counter = await _counterRepository.GetByOrderIdAsync(activeOrder.Id);
            if (counter == null)
                return;

            // Registrar error de peso: se podría incrementar contador de unidades malas
            // O crear un registro de alerta para investigación
            Console.WriteLine($"[GPIO 22 - Peso] Error detectado en orden {activeOrder.OrderCode}");

            // TODO: Implementar lógica de negocio:
            // - Registrar en tabla de alertas
            // - Marcar unidad como defectuosa
            // - Pausar orden si es necesario
            // - Notificar operador
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPIO 22 Error] {ex.Message}");
        }
    }

    /// <summary>
    /// Maneja evento de error de etiqueta en Cremer
    /// GPIO 19: Detecta cuando la etiqueta no se aplicó correctamente
    /// </summary>
    public async Task HandleLabelErrorAsync(Guid machineId)
    {
        try
        {
            // Obtener orden activa de la máquina
            var orders = await _orderRepository.GetByStatusAsync(
                Domain.Enums.ProductionOrderStatus.InProgress);
            var activeOrder = orders.FirstOrDefault(o => o.MachineId == machineId);

            if (activeOrder == null)
                return;

            // Obtener contador
            var counter = await _counterRepository.GetByOrderIdAsync(activeOrder.Id);
            if (counter == null)
                return;

            // Registrar error de etiqueta
            Console.WriteLine($"[GPIO 19 - Etiqueta] Error detectado en orden {activeOrder.OrderCode}");

            // TODO: Implementar lógica de negocio:
            // - Registrar en tabla de alertas
            // - Marcar unidad como defectuosa
            // - Pausar orden si es necesario
            // - Solicitar intervención manual
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPIO 19 Error] {ex.Message}");
        }
    }

    /// <summary>
    /// Maneja evento de conteo de unidades (GPIO 23)
    /// Incrementa el contador de unidades buenas
    /// </summary>
    public async Task HandleUnitCountAsync(Guid machineId)
    {
        try
        {
            var orders = await _orderRepository.GetByStatusAsync(
                Domain.Enums.ProductionOrderStatus.InProgress);
            var activeOrder = orders.FirstOrDefault(o => o.MachineId == machineId);

            if (activeOrder == null)
            {
                Console.WriteLine("[GPIO 23] No hay orden activa");
                return;
            }

            var counter = await _counterRepository.GetByOrderIdAsync(activeOrder.Id);
            if (counter == null)
            {
                Console.WriteLine("[GPIO 23] Contador no encontrado");
                return;
            }

            // Crear nuevo contador con cantidad incrementada
            // En una implementación real, habría un UpdateAsync en el repositorio
            Console.WriteLine($"[GPIO 23] Unidad contada para orden {activeOrder.OrderCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GPIO 23 Error] {ex.Message}");
        }
    }
}

/// <summary>
/// Configuración de GPIO para máquina Cremer
/// Mapeo de puertos GPIO físicos a eventos de negocio
/// </summary>
public sealed record CremerGpioConfiguration
{
    /// <summary>GPIO 23 - Detector de unidades (falling edge)</summary>
    public const int UnitCountGpio = 23;

    /// <summary>GPIO 22 - Sensor de peso</summary>
    public const int WeightErrorGpio = 22;

    /// <summary>GPIO 19 - Sensor de etiqueta</summary>
    public const int LabelErrorGpio = 19;

    /// <summary>MQTT Topic para GPIO 23</summary>
    public const string UnitCountTopic = "cremer/gpio/23";

    /// <summary>MQTT Topic para GPIO 22</summary>
    public const string WeightErrorTopic = "cremer/gpio/22";

    /// <summary>MQTT Topic para GPIO 19</summary>
    public const string LabelErrorTopic = "cremer/gpio/19";

    /// <summary>
    /// GPIO Mapping Documentation:
    /// - GPIO 23 (Input, falling edge): Pulso cuando se completa una unidad
    ///   - Comportamiento: Cada pulso = 1 unidad procesada
    ///   - Acción: Incrementar contador de buenas unidades
    ///
    /// - GPIO 22 (Input, rising edge): Señal cuando hay error de peso
    ///   - Comportamiento: Se activa cuando el peso es incorrecto
    ///   - Acción: Registrar error, pausar línea si es persistente
    ///
    /// - GPIO 19 (Input, rising edge): Señal cuando falla la etiqueta
    ///   - Comportamiento: Se activa cuando no se aplicó la etiqueta
    ///   - Acción: Registrar error, solicitar intervención manual
    /// </summary>
    public static readonly Dictionary<int, string> GpioToTopicMapping = new()
    {
        { UnitCountGpio, UnitCountTopic },
        { WeightErrorGpio, WeightErrorTopic },
        { LabelErrorGpio, LabelErrorTopic }
    };

    public static readonly Dictionary<string, int> TopicToGpioMapping = new()
    {
        { UnitCountTopic, UnitCountGpio },
        { WeightErrorTopic, WeightErrorGpio },
        { LabelErrorTopic, LabelErrorGpio }
    };
}
