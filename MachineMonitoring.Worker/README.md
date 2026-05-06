# MachineMonitoring.Worker

Librería de worker para integración MQTT con máquinas industriales. Implementa listeners para eventos GPIO de máquinas como Cremer.

## Características

- **MqttGpioListener**: Escucha eventos MQTT de GPIO e incrementa contadores
- **MqttListenerHostedService**: Servicio hospedado para ejecutar el listener
- **Soporte para múltiples GPIO**: GPIO 23 (conteo), GPIO 22 (error peso), GPIO 19 (error etiqueta)

## Configuración

### appsettings.json

```json
{
  "Mqtt": {
    "Broker": "localhost",
    "Port": 1883,
    "MachineCode": "CREMER"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=machine_monitoring;User Id=postgres;Password=postgres;"
  }
}
```

## Uso

### Opción 1: En una aplicación Worker Service

```csharp
using MachineMonitoring.Infrastructure;
using MachineMonitoring.Worker.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configurar infraestructura y base de datos
        services.AddInfrastructure(context.Configuration);
        
        // Agregar el listener MQTT
        services.AddMqttListener();
    });

var host = builder.Build();
await host.RunAsync();
```

### Opción 2: En una aplicación existente (ej. ASP.NET Core)

```csharp
// En Program.cs
services.AddInfrastructure(configuration);
services.AddMqttListener();

// El listener se iniciará automáticamente cuando la aplicación inicie
```

## Eventos MQTT Soportados

### Cremer Machine
- **cremer/gpio/23**: Incremento de contador (conteo de unidades)
  - Payload: 1 (incrementa el contador de la orden activa)
  
- **cremer/gpio/22**: Error de peso detectado
  - Payload: 1 (inicia manejo de error)
  
- **cremer/gpio/19**: Error de etiqueta detectado
  - Payload: 1 (inicia manejo de error)

## Flujo de Procesamiento

1. MqttGpioListener se conecta al broker MQTT
2. Se suscribe a los topics de GPIO de la máquina configurada
3. Al recibir un evento:
   - Obtiene la máquina por código
   - Busca la orden de producción activa
   - Procesa el evento según el GPIO:
     - **GPIO 23**: Incrementa el contador
     - **GPIO 22/19**: Manejo de errores (TBD)
4. Registra los cambios en la base de datos

## Requisitos

- MQTT Broker (ej. Mosquitto) ejecutándose en el host/puerto configurado
- Base de datos PostgreSQL con el schema de MachineMonitoring
- Máquina registrada en el sistema (código debe coincidir con Mqtt:MachineCode)
- Orden de producción activa para que se incrementen los contadores

## Extensión para otras máquinas

Para agregar soporte a otras máquinas:

1. Crear una clase heredada de `MqttGpioListener`
2. Sobrescribir `HandleUnitCountEvent()` y otros handlers según sea necesario
3. Configurar los topics MQTT apropiados
4. Registrar en DI:

```csharp
services.AddHostedService<MqttListenerHostedService>();
// O crear un servicio personalizado para la nueva máquina
```

## Estado

- ✓ GPIO 23: Conteo de unidades implementado
- ⏳ GPIO 22: Manejo de error de peso (TODO)
- ⏳ GPIO 19: Manejo de error de etiqueta (TODO)
- ⏳ Soporte para múltiples máquinas (estructura lista)
