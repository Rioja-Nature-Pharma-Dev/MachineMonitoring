# Arquitectura - MachineMonitoring

Documento técnico detallado sobre la arquitectura de la plataforma.

## 📐 Visión General

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT LAYER (External)                  │
│  REST Clients • Postman • Frontend • Mobile Apps            │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/REST
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    API LAYER                                │
│  Controllers • DTOs • Routing • OpenAPI/Swagger            │
│  Validation • Error Handling                                │
└────────────────────────┬────────────────────────────────────┘
                         │ Commands/Queries
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                APPLICATION LAYER                            │
│  Handlers • Mappers • Business Logic Orchestration         │
│  Validation Rules • Use Case Coordinators                   │
└────────┬────────────────────────────┬─────────────────────┘
         │ Repository Calls           │ Event Publishing
         ▼                             ▼
┌──────────────────┐    ┌──────────────────────────────────┐
│ DOMAIN LAYER     │    │   INFRASTRUCTURE LAYER           │
│ Entities         │    │   EF Core • PostgreSQL           │
│ ValueObjects     │    │   Repositories • Services        │
│ Business Rules   │    │   SystemClock • Migrations       │
│ Exceptions       │    └──────────────────────────────────┘
└──────────────────┘
         ▲
         │
         │ Event Subscriptions
         │
┌─────────────────────────────────────────────────────────────┐
│                    WORKER LAYER (Async)                     │
│  MQTT Listener • GPIO Event Handlers • Background Jobs     │
└─────────────────────────────────────────────────────────────┘
```

## 🏗️ Capas y Responsabilidades

### 1. API Layer (Presentation)

**Responsabilidad**: Exponer HTTP endpoints y manejar comunicación con clientes.

**Componentes**:
- Controllers
- DTOs (Data Transfer Objects)
- Request/Response Mapping
- OpenAPI/Swagger Documentation

**Ubicación**: `MachineMonitoring.Api/Controllers/`

**Características**:
- Controllers RESTful (4 controladores, 18 endpoints)
- Validación de input vía attributes
- Mapeo de excepciones a HTTP status codes
- Documentación automática vía OpenAPI

**Ejemplo**:

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class ProductionOrdersController : ControllerBase
{
    private readonly ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto> _createHandler;
    private readonly IQueryHandler<GetProductionOrderQuery, ProductionOrderDto> _getHandler;

    [HttpPost]
    [ProducesResponseType(typeof(ProductionOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionOrderDto>> Create(
        CreateProductionOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateProductionOrderCommand(
                request.MachineCode,
                request.Quantity,
                request.UnitsPerBox,
                request.EstimatedMinutes);

            var result = await _createHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### 2. Application Layer (Use Cases)

**Responsabilidad**: Orquestar lógica de casos de uso.

**Componentes**:
- Command Handlers
- Query Handlers
- DTOs
- Mappers
- Application Services

**Ubicación**: `MachineMonitoring.Application/`

**Patrón**: Command/Query Handler Pattern (sin MediatR)

**Características**:
- Handlers CQRS-inspired
- Separación entre commands (mutating) y queries (read-only)
- Use case por handler
- CancellationToken propagado

**Estructura**:

```csharp
// Command - Mutates state
public sealed record CreateProductionOrderCommand(
    string MachineCode,
    int Quantity,
    int UnitsPerBox,
    int EstimatedMinutes);

// Handler
public sealed class CreateProductionOrderHandler 
    : ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto>
{
    private readonly IProductionOrderRepository _repository;
    private readonly IMachineRepository _machineRepository;
    
    public async Task<ProductionOrderDto> HandleAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        // Orquestar lógica de negocio
        // Validar con dominio
        // Persistir cambios
        // Retornar resultado
    }
}

// Query - Read-only
public sealed record GetProductionOrderQuery(Guid Id);

public sealed class GetProductionOrderHandler 
    : IQueryHandler<GetProductionOrderQuery, ProductionOrderDto>
{
    public async Task<ProductionOrderDto> HandleAsync(
        GetProductionOrderQuery query,
        CancellationToken cancellationToken)
    {
        // Obtener datos
        // Mapear a DTO
        // Retornar
    }
}
```

**Mappers**:

```csharp
public sealed class ProductionOrderMapper : IProductionOrderMapper
{
    public ProductionOrderDto ToDto(ProductionOrder order) =>
        new(
            order.Id,
            order.MachineCode.Value,
            order.Quantity,
            order.Status.ToString(),
            order.StartedAt,
            order.FinishedAt);

    public ProductionOrder ToDomain(CreateProductionOrderRequest request) =>
        ProductionOrder.Create(
            new MachineCode(request.MachineCode),
            request.Quantity);
}
```

### 3. Domain Layer (Business Logic)

**Responsabilidad**: Modelar reglas de negocio.

**Componentes**:
- Entities (objetos con identidad)
- Value Objects (objetos sin identidad)
- Enums
- Exceptions
- Repository Interfaces

**Ubicación**: `MachineMonitoring.Domain/`

**Características**:
- Entidades con comportamiento rico
- Validaciones de negocio en el dominio
- Value Objects inmutables
- No depende de frameworks

**Entidades**:

```csharp
public sealed class ProductionOrder : Entity
{
    public MachineCode MachineCode { get; private set; }
    public int Quantity { get; private set; }
    public int UnitsPerBox { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    private ProductionOrder() { }

    public static ProductionOrder Create(
        MachineCode machineCode,
        int quantity,
        int unitsPerBox,
        int estimatedMinutes)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        return new ProductionOrder
        {
            MachineCode = machineCode,
            Quantity = quantity,
            UnitsPerBox = unitsPerBox,
            Status = OrderStatus.Created,
            StartedAt = DateTime.MinValue,
            FinishedAt = null
        };
    }

    public void Start()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Order must be in Created state");

        Status = OrderStatus.Started;
        StartedAt = DateTime.UtcNow;
    }

    public void Finish()
    {
        if (Status != OrderStatus.Started)
            throw new InvalidOperationException("Order must be Started to finish");

        Status = OrderStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }
}
```

**Value Objects**:

```csharp
public sealed record MachineCode(string Value)
{
    public MachineCode(string value) 
        : this(value?.Trim() ?? throw new ArgumentNullException(nameof(value)))
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("MachineCode cannot be empty");
        if (value.Length > 50)
            throw new DomainException("MachineCode cannot exceed 50 characters");
    }
}

public sealed record OrderStatus(string Value)
{
    public static readonly OrderStatus Created = new("Created");
    public static readonly OrderStatus Started = new("Started");
    public static readonly OrderStatus Finished = new("Finished");
    
    public static OrderStatus FromString(string value) =>
        value switch
        {
            "Created" => Created,
            "Started" => Started,
            "Finished" => Finished,
            _ => throw new DomainException($"Invalid status: {value}")
        };
}
```

### 4. Infrastructure Layer (Technical)

**Responsabilidad**: Manejar detalles técnicos de persistencia.

**Componentes**:
- Entity Framework Core
- Database Context
- Repository Implementations
- Migrations
- Services (SystemClock, etc.)

**Ubicación**: `MachineMonitoring.Infrastructure/`

**Características**:
- EF Core 10 con PostgreSQL
- Migrations automáticas
- Repository Pattern
- Lazy loading disabled (explícito)

**Database Context**:

```csharp
public sealed class MachineMonitoringContext : DbContext
{
    public DbSet<Machine> Machines { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionCounter> ProductionCounters { get; set; }
    public DbSet<ProductionMetrics> ProductionMetrics { get; set; }

    public MachineMonitoringContext(DbContextOptions<MachineMonitoringContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MachineMonitoringContext).Assembly);
        
        // Deshabilitar lazy loading
        this.ChangeTracker.LazyLoadingEnabled = false;
    }
}
```

**Repository**:

```csharp
public sealed class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly MachineMonitoringContext _context;

    public ProductionOrderRepository(MachineMonitoringContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        await _context.ProductionOrders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductionOrder> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new DomainException($"Order {id} not found");
    }

    public async Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        _context.ProductionOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### 5. Worker Layer (Background Services)

**Responsabilidad**: Procesar eventos asincronos en background.

**Componentes**:
- MQTT Listener
- GPIO Event Handlers
- Hosted Services
- Background Jobs

**Ubicación**: `MachineMonitoring.Worker/`

**Características**:
- MQTTnet 4.3.2 para integración
- Event-driven architecture
- Asincronía nativa
- Configurable sin recompilación

**MQTT Listener**:

```csharp
public sealed class MqttGpioListener
{
    private readonly MqttFactory _factory = new MqttFactory();
    private readonly IApplicationConfiguration _config;
    private readonly GpioEventHandlers _handlers;

    public MqttGpioListener(IApplicationConfiguration config, GpioEventHandlers handlers)
    {
        _config = config;
        _handlers = handlers;
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        var client = _factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_config.Mqtt.Host, _config.Mqtt.Port)
            .WithClientId(_config.Mqtt.ClientId)
            .Build();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            if (topic == "cremer/gpio/23")
                await _handlers.HandleUnitCountAsync(payload, cancellationToken);
            else if (topic == "cremer/gpio/22")
                await _handlers.HandleWeightErrorAsync(payload, cancellationToken);
            else if (topic == "cremer/gpio/19")
                await _handlers.HandleLabelErrorAsync(payload, cancellationToken);
        };

        await client.ConnectAsync(options, cancellationToken);
        await client.SubscribeAsync(_config.Mqtt.Topics.ToArray(), cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
            await Task.Delay(100, cancellationToken);
    }
}
```

**GPIO Handlers**:

```csharp
public sealed class GpioEventHandlers
{
    private readonly IProductionCounterRepository _counterRepository;
    private readonly IProductionOrderRepository _orderRepository;

    public async Task HandleUnitCountAsync(string payload, CancellationToken cancellationToken)
    {
        // GPIO 23 - Incrementar contador
        if (int.TryParse(payload, out var units))
        {
            var activeCounter = await _counterRepository.GetActiveAsync(cancellationToken);
            if (activeCounter is not null)
            {
                activeCounter.IncrementGoodUnits(units);
                await _counterRepository.UpdateAsync(activeCounter, cancellationToken);
            }
        }
    }

    public async Task HandleWeightErrorAsync(string payload, CancellationToken cancellationToken)
    {
        // GPIO 22 - Error de peso
        // TODO: Implementar persistencia de alertas
    }

    public async Task HandleLabelErrorAsync(string payload, CancellationToken cancellationToken)
    {
        // GPIO 19 - Error de etiqueta
        // TODO: Implementar persistencia de alertas
    }
}
```

## 🔄 Flujos de Datos

### Flujo 1: Crear Orden de Producción

```
1. Cliente                          POST /api/production-orders
                                            │
2. ProductionOrdersController       Validar request
                                            │
3. Crear comando                    new CreateProductionOrderCommand(...)
                                            │
4. CreateProductionOrderHandler      ├─ Buscar máquina en repositorio
                                     ├─ Validar regla de negocio
                                     ├─ Crear entidad ProductionOrder
                                     └─ Persistir en base de datos
                                            │
5. Retornar DTO                     ProductionOrderDto(...)
                                            │
6. HTTP Response                    201 Created + JSON
```

### Flujo 2: Evento GPIO

```
1. GPIO 23 dispara evento

2. MQTT Broker recibe mensaje

3. MqttGpioListener escucha
   ├─ Valida topic
   └─ Mapea a handler

4. GpioEventHandlers.HandleUnitCountAsync()
   ├─ Obtiene contador activo
   ├─ Incrementa unidades
   └─ Persistencia

5. Base de datos se actualiza

6. Métricas se recalculan (en siguiente query)
```

## 📊 Data Model

```sql
-- Máquinas
TABLE machines (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP NOT NULL
)

-- Órdenes de Producción
TABLE production_orders (
    id UUID PRIMARY KEY,
    machine_code VARCHAR(50) NOT NULL,
    quantity INT NOT NULL,
    units_per_box INT NOT NULL,
    status VARCHAR(20) NOT NULL,
    started_at TIMESTAMP,
    finished_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL,
    FOREIGN KEY (machine_code) REFERENCES machines(code)
)

-- Contadores
TABLE production_counters (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    good_units INT NOT NULL DEFAULT 0,
    bad_units INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL,
    FOREIGN KEY (order_id) REFERENCES production_orders(id)
)

-- Métricas OEE
TABLE production_metrics (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    availability DECIMAL(5,2) NOT NULL,
    performance DECIMAL(5,2) NOT NULL,
    quality DECIMAL(5,2) NOT NULL,
    oee DECIMAL(5,2) NOT NULL,
    calculated_at TIMESTAMP NOT NULL,
    FOREIGN KEY (order_id) REFERENCES production_orders(id)
)
```

## 🔐 Decisiones Arquitectónicas

### 1. Clean Architecture

**Decisión**: Implementar Clean Architecture con 5 capas.

**Ventajas**:
- ✅ Independencia de frameworks
- ✅ Testeable en aislamiento
- ✅ Escalable
- ✅ Mantenible

**Desventajas**:
- ❌ Más boilerplate inicial
- ❌ Mapeo entre capas

**Alternativa Considerada**: Vertical Slice Architecture
- Mejor para equipos pequeños
- Menos separación
- Rechazada por requerimiento de escalabilidad

### 2. No usar MediatR

**Decisión**: Implementar pattern Command/Query sin librería MediatR.

**Ventajas**:
- ✅ Control total del flujo
- ✅ Menos dependencias
- ✅ Debugging más fácil
- ✅ Mejor rendimiento

**Desventajas**:
- ❌ Más código boilerplate en DI
- ❌ Menos convenciones

**Alternativa Considerada**: Usar MediatR
- Más maduro
- Mejor soporte
- Rechazada por simplicidad y control

### 3. Value Objects como Records

**Decisión**: Usar C# 9 records para Value Objects.

**Ventajas**:
- ✅ Inmutables por defecto
- ✅ Igualdad por valor automática
- ✅ Sintaxis más limpia
- ✅ Mejor performance

**Desventajas**:
- ❌ Requiere C# 9+
- ❌ Menos familiar para algunos

### 4. Entities Selladas

**Decisión**: Todas las entidades son `sealed`.

**Ventajas**:
- ✅ Previene herencia accidental
- ✅ Permite inlining del compilador
- ✅ Más seguro

**Desventajas**:
- ❌ Menos flexible

## 🔄 Patrones Implementados

### Repository Pattern

Abstrae acceso a datos:

```csharp
public interface IProductionOrderRepository
{
    Task AddAsync(ProductionOrder order, CancellationToken cancellationToken);
    Task<ProductionOrder> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(ProductionOrder order, CancellationToken cancellationToken);
}
```

**Ventajas**:
- Testeable (mockeable)
- Intercambiable
- Independencia de BD

### Command/Query Handler Pattern

```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

**Ventajas**:
- CQRS inspired
- Separación clara de responsabilidades
- Fácil de testear

### Mapper Pattern

```csharp
public interface IProductionOrderMapper
{
    ProductionOrderDto ToDto(ProductionOrder entity);
    ProductionOrder ToDomain(CreateProductionOrderRequest request);
}
```

**Ventajas**:
- Separación de DTOs y entidades
- Reutilizable
- Testeable

## 🚀 Agregar Nueva Funcionalidad

### Ejemplo: Agregar Endpoint de Pausa

**Paso 1: Definir en Domain**

```csharp
// Domain/Entities/ProductionOrder.cs
public void Pause()
{
    if (Status != OrderStatus.Started)
        throw new InvalidOperationException("Order must be Started to pause");
    
    Status = OrderStatus.Paused;
}
```

**Paso 2: Crear Command y Handler**

```csharp
// Application/Commands/PauseProductionOrderCommand.cs
public sealed record PauseProductionOrderCommand(Guid OrderId);

// Application/Handlers/PauseProductionOrderHandler.cs
public sealed class PauseProductionOrderHandler 
    : ICommandHandler<PauseProductionOrderCommand, ProductionOrderDto>
{
    private readonly IProductionOrderRepository _repository;
    private readonly IProductionOrderMapper _mapper;

    public async Task<ProductionOrderDto> HandleAsync(
        PauseProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, cancellationToken);
        order.Pause();
        await _repository.UpdateAsync(order, cancellationToken);
        return _mapper.ToDto(order);
    }
}
```

**Paso 3: Registrar en DI**

```csharp
// Program.cs
services.AddScoped<ICommandHandler<PauseProductionOrderCommand, ProductionOrderDto>, 
    PauseProductionOrderHandler>();
```

**Paso 4: Crear Endpoint**

```csharp
// Controllers/ProductionOrdersController.cs
[HttpPost("{id}/pause")]
public async Task<ActionResult<ProductionOrderDto>> Pause(
    Guid id,
    [FromServices] ICommandHandler<PauseProductionOrderCommand, ProductionOrderDto> handler,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await handler.HandleAsync(
            new PauseProductionOrderCommand(id),
            cancellationToken);
        return Ok(result);
    }
    catch (DomainException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

**Paso 5: Tests**

```csharp
[Fact]
public async Task GivenStartedOrder_WhenPause_ThenOrderIsPaused()
{
    // Arrange
    var order = ProductionOrder.Create(new MachineCode("TEST"), 100);
    order.Start();
    
    // Act
    order.Pause();
    
    // Assert
    Assert.Equal(OrderStatus.Paused, order.Status);
}
```

## 📈 Performance Considerations

### Current

- ✅ Async/await nativo
- ✅ CancellationToken propagado
- ✅ Lazy loading deshabilitado
- ✅ Índices en base de datos

### Future Improvements

- 🔜 Caching (Redis)
- 🔜 Pagination
- 🔜 Bulk operations
- 🔜 Query optimization

## 🔐 Seguridad

### Actual

- ⚠️ Sin autenticación (en roadmap Fase 4.1)
- ⚠️ CORS abierto en development
- ✅ SQL injection prevenido (EF Core)
- ✅ Input validation en DTOs

### Roadmap

- 🔜 JWT authentication
- 🔜 Role-based authorization
- 🔜 Rate limiting
- 🔜 HTTPS enforcement

## 📚 Referencias

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/aspnet/core/fundamentals/best-practices)

---

**Última actualización**: 2026-05-06  
**Versión**: 1.0.0
