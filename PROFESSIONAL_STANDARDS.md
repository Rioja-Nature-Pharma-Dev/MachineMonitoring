# Estándares Profesionales - MachineMonitoring

Documento que define los estándares de código, arquitectura y procesos para garantizar calidad empresarial.

## 📐 Principios SOLID

El código de este proyecto adhiere a los 5 principios SOLID:

### S - Single Responsibility Principle (SRP)
Cada clase tiene una única razón para cambiar.

✅ **Correcto**:
```csharp
// ProductionOrderHandler - responsable solo de crear órdenes
public class CreateProductionOrderHandler
{
    private readonly IProductionOrderRepository _repository;
    
    public async Task<ProductionOrderDto> HandleAsync(
        CreateProductionOrderCommand command, 
        CancellationToken cancellationToken)
    {
        // Crear orden
    }
}

// MetricsCalculator - responsable solo de calcular OEE
public class MetricsCalculator
{
    public OeeMetrics Calculate(ProductionOrder order) { }
}
```

❌ **Incorrecto**: Mezclar creación, cálculo y persistencia en un método.

### O - Open/Closed Principle (OCP)
Abierto para extensión, cerrado para modificación.

✅ **Correcto**:
```csharp
// Interface genérica para handlers
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

// Extensible sin modificar código existente
public class CreateProductionOrderHandler : ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto>
{
    public async Task<ProductionOrderDto> HandleAsync(CreateProductionOrderCommand command, CancellationToken cancellationToken)
    {
        // Implementación específica
    }
}
```

### L - Liskov Substitution Principle (LSP)
Las subclases pueden reemplazar a las clases base sin quebrar el código.

✅ **Correcto**:
```csharp
// Repository base
public interface IRepository<T> where T : Entity
{
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

// Implementaciones intercambiables
public class ProductionOrderRepository : IRepository<ProductionOrder> { }
public class MachineRepository : IRepository<Machine> { }
```

### I - Interface Segregation Principle (ISP)
Clientes no deben depender de interfaces que no usan.

✅ **Correcto**:
```csharp
// Interfaces específicas
public interface IMachineRepository
{
    Task AddAsync(Machine machine, CancellationToken cancellationToken);
    Task<Machine> GetByCodeAsync(string code, CancellationToken cancellationToken);
}

public interface IProductionOrderRepository
{
    Task AddAsync(ProductionOrder order, CancellationToken cancellationToken);
    Task<ProductionOrder> GetActiveAsync(CancellationToken cancellationToken);
}
```

❌ **Incorrecto**: Una interfaz megamonstruo con 30 métodos.

### D - Dependency Inversion Principle (DIP)
Depender de abstracciones, no de concreciones.

✅ **Correcto**:
```csharp
public class MetricsController
{
    private readonly ICommandHandler<CalculateMetricsCommand, OeeMetricsDto> _handler;
    
    public MetricsController(ICommandHandler<CalculateMetricsCommand, OeeMetricsDto> handler)
    {
        _handler = handler;
    }
}

// En Program.cs:
services.AddScoped<ICommandHandler<CalculateMetricsCommand, OeeMetricsDto>, 
    CalculateMetricsHandler>();
```

## 🎨 Convenciones de Nombrado

### Clases

```csharp
// Entidades de dominio
public sealed class ProductionOrder : Entity { }
public sealed class Machine : Entity { }

// Value Objects (sellados e inmutables)
public sealed record MachineCode(string Value);
public sealed record OrderStatus(string Value);

// Handlers
public sealed class CreateProductionOrderHandler : ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto> { }
public sealed class GetMachineByCodeHandler : IQueryHandler<GetMachineByCodeQuery, MachineDto> { }

// Repositories
public sealed class ProductionOrderRepository : IProductionOrderRepository { }

// Controllers
public sealed class ProductionOrdersController : ControllerBase { }

// Servicios
public sealed class MetricsCalculator { }
public sealed class SystemClock : ISystemClock { }
```

### Métodos

```csharp
// Async handlers
public async Task<ProductionOrderDto> HandleAsync(
    CreateProductionOrderCommand command, 
    CancellationToken cancellationToken) { }

// Métodos booleanos
public bool IsActive() { }
public bool HasStarted() { }

// Métodos de fábrica
public static ProductionOrder Create(MachineCode code, int quantity) { }
```

### Variables

```csharp
// Locales - camelCase
var machineCode = "CREMER-001";
var productionOrder = await repository.GetByIdAsync(orderId, cancellationToken);

// Constantes - UPPER_SNAKE_CASE
private const int MAX_UNITS_PER_BOX = 100;
private const string DEFAULT_MQTT_HOST = "localhost";

// Propiedades - PascalCase
public Guid Id { get; private set; }
public OrderStatus Status { get; private set; }
```

## 📝 Estructura de Clases

### Entity Base

```csharp
public abstract class Entity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    
    protected Entity() { }
    
    // Métodos protected para lógica de dominio
    protected void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;
}

// Uso
public sealed class ProductionOrder : Entity
{
    public MachineCode MachineCode { get; private set; }
    public int Quantity { get; private set; }
    
    private ProductionOrder() { }
    
    public static ProductionOrder Create(MachineCode machineCode, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");
        
        return new ProductionOrder { MachineCode = machineCode, Quantity = quantity };
    }
}
```

### Value Objects

```csharp
// Inmutables, sellados, comparables por valor
public sealed record MachineCode(string Value)
{
    public MachineCode(string value) : this(value?.Trim() ?? throw new ArgumentNullException(nameof(value)))
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("MachineCode cannot be empty");
    }
    
    public override string ToString() => Value;
}
```

### DTOs

```csharp
// Para request/response, sin lógica
public sealed record CreateProductionOrderRequest(
    string MachineCode,
    int Quantity,
    int UnitsPerBox,
    int EstimatedMinutes);

public sealed record ProductionOrderDto(
    Guid Id,
    string MachineCode,
    int Quantity,
    string Status,
    DateTime StartedAt,
    DateTime? FinishedAt);
```

### Handlers

```csharp
// Command Handler
public sealed class CreateProductionOrderHandler : ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto>
{
    private readonly IProductionOrderRepository _repository;
    private readonly IMachineRepository _machineRepository;
    private readonly IProductionOrderMapper _mapper;
    
    public CreateProductionOrderHandler(
        IProductionOrderRepository repository,
        IMachineRepository machineRepository,
        IProductionOrderMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<ProductionOrderDto> HandleAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var machine = await _machineRepository.GetByCodeAsync(
            command.MachineCode, 
            cancellationToken);
        
        if (machine is null)
            throw new DomainException($"Machine {command.MachineCode} not found");
        
        var order = ProductionOrder.Create(
            new MachineCode(command.MachineCode),
            command.Quantity);
        
        await _repository.AddAsync(order, cancellationToken);
        
        return _mapper.ToDto(order);
    }
}

// Query Handler
public sealed class GetMachineByCodeHandler : IQueryHandler<GetMachineByCodeQuery, MachineDto>
{
    private readonly IMachineRepository _repository;
    private readonly IMachineMapper _mapper;
    
    public GetMachineByCodeHandler(IMachineRepository repository, IMachineMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<MachineDto> HandleAsync(
        GetMachineByCodeQuery query,
        CancellationToken cancellationToken)
    {
        var machine = await _repository.GetByCodeAsync(query.Code, cancellationToken);
        return machine is not null ? _mapper.ToDto(machine) : null!;
    }
}
```

## 📌 Comentarios y Documentación

### Regla de Oro
**NO escribir comentarios que expliquen QUÉ hace el código** (el código mismo debe ser claro). Solo comentar el POR QUÉ si no es obvio.

✅ **Correcto**:
```csharp
public decimal CalculateOee(ProductionOrder order)
{
    // OEE = A × P × Q. Retornamos 0 si algún factor es 0 para evitar 
    // falsas métricas en órdenes no completadas
    var availability = CalculateAvailability(order);
    var performance = CalculatePerformance(order);
    var quality = CalculateQuality(order);
    
    return (availability * performance * quality) / 10000m;
}
```

❌ **Incorrecto**:
```csharp
public decimal CalculateOee(ProductionOrder order)
{
    // Calcular availability
    var availability = CalculateAvailability(order);
    // Calcular performance
    var performance = CalculatePerformance(order);
    // Calcular quality
    var quality = CalculateQuality(order);
    // Retornar OEE
    return (availability * performance * quality) / 10000m;
}
```

### Documentación de Métodos Públicos

```csharp
/// <summary>
/// Calcula el Overall Equipment Effectiveness (OEE) para una orden de producción.
/// </summary>
/// <param name="order">La orden para calcular OEE</param>
/// <returns>OEE como valor decimal (0-100)</returns>
/// <exception cref="ArgumentNullException">Si order es null</exception>
public decimal CalculateOee(ProductionOrder order)
{
    // Implementación
}
```

### Excepciones en Comentarios

```csharp
// Usar comentarios SOLO para invariantes no obvios o workarounds
public async Task StartAsync(CancellationToken cancellationToken)
{
    // MqttNet requiere que nos desuscribamos antes de cambiar tópicos
    // para evitar memory leaks. Ver: github.com/dotnet/MQTTnet/issues/1234
    if (_client.IsConnected)
        await _client.DisconnectAsync();
    
    await ConnectAsync(cancellationToken);
}
```

## ⚠️ Manejo de Errores

### Excepciones de Dominio

```csharp
// Definir excepciones específicas del dominio
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public sealed class OrderAlreadyStartedException : DomainException
{
    public OrderAlreadyStartedException(Guid orderId)
        : base($"Order {orderId} has already started") { }
}
```

### En Handlers

```csharp
public async Task<ProductionOrderDto> HandleAsync(
    StartProductionOrderCommand command,
    CancellationToken cancellationToken)
{
    var order = await _repository.GetByIdAsync(command.OrderId, cancellationToken)
        ?? throw new DomainException($"Order {command.OrderId} not found");
    
    // Validaciones de reglas de negocio
    if (order.Status == OrderStatus.Started)
        throw new OrderAlreadyStartedException(order.Id);
    
    order.Start();
    await _repository.UpdateAsync(order, cancellationToken);
    
    return _mapper.ToDto(order);
}
```

### En Controllers

```csharp
[HttpPost("{id}/start")]
public async Task<ActionResult<ProductionOrderDto>> Start(
    Guid id,
    [FromServices] ICommandHandler<StartProductionOrderCommand, ProductionOrderDto> handler,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await handler.HandleAsync(
            new StartProductionOrderCommand(id),
            cancellationToken);
        return Ok(result);
    }
    catch (DomainException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

## 🧪 Testing

### Estructura de Tests

```
MachineMonitoring.Domain.Tests/
├── Entities/
│   └── ProductionOrderTests.cs
├── ValueObjects/
│   └── MachineCodeTests.cs
└── Services/
    └── MetricsCalculatorTests.cs

MachineMonitoring.Application.Tests/
├── Handlers/
│   └── CreateProductionOrderHandlerTests.cs
└── Mappers/
    └── ProductionOrderMapperTests.cs

MachineMonitoring.Api.Tests/
└── Controllers/
    └── ProductionOrdersControllerTests.cs
```

### Unit Test Naming

```csharp
public class CreateProductionOrderHandlerTests
{
    private readonly CreateProductionOrderHandler _handler;
    private readonly Mock<IProductionOrderRepository> _repositoryMock;
    private readonly Mock<IMachineRepository> _machineRepositoryMock;
    
    // Patrón: GivenContext_WhenAction_ThenResult
    [Fact]
    public async Task GivenValidCommand_WhenHandle_ThenCreatesOrder()
    {
        // Arrange
        var command = new CreateProductionOrderCommand("CREMER-001", 100);
        
        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductionOrder>(), It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task GivenUnknownMachine_WhenHandle_ThenThrows()
    {
        // Arrange
        var command = new CreateProductionOrderCommand("UNKNOWN", 100);
        _machineRepositoryMock.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Machine)null!);
        
        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
    }
}
```

## 📝 Commits y Pull Requests

### Formato de Commit

```
[TYPE] Description

[OPTIONAL BODY]

[OPTIONAL FOOTER]

Ejemplos:
- [feat] Add production order endpoints
- [fix] Fix OEE calculation for zero quantity
- [test] Add unit tests for metrics calculator
- [docs] Update README with API examples
- [refactor] Extract metrics calculation logic
```

**Tipos permitidos**:
- `[feat]` - Nueva funcionalidad
- `[fix]` - Corrección de bug
- `[test]` - Tests (sin cambios de código)
- `[docs]` - Documentación
- `[refactor]` - Refactoring (sin cambios de comportamiento)
- `[chore]` - Tareas de mantenimiento
- `[perf]` - Mejoras de rendimiento

### Reglas de Commits

1. **Atómico**: Un commit = un cambio lógico
2. **Testeable**: Cada commit debe compilar y pasar tests
3. **Descriptivo**: Mensaje claro en imperativo
4. **Sin WIP**: No commitear trabajo incompleto

❌ **Incorrecto**:
```
git commit -m "agg stuff"
git commit -m "WIP"
git commit -m "Updated files"
```

✅ **Correcto**:
```
[feat] Add endpoint to start production order
[test] Add unit tests for ProductionOrder.Start()
[docs] Update API documentation with examples
```

### Pull Requests

**Estructura mínima**:

```markdown
## Descripción
Breve descripción de qué cambia y por qué.

## Cambios Principales
- [ ] Cambio 1
- [ ] Cambio 2
- [ ] Cambio 3

## Testing
- [ ] Unit tests agregados
- [ ] Testing manual realizado
- [ ] Casos edge considerados

## Checklist
- [ ] Código formatea según estándares
- [ ] No hay breaking changes
- [ ] Documentación actualizada
- [ ] Commits bien formateados
```

**Reglas**:
1. Un PR = una funcionalidad
2. Máximo 400 líneas de cambios
3. Mínimo 2 revisiones antes de merge
4. Pasar CI/CD antes de merge
5. Squash antes de merge (si aplica)

## 🔄 Async/Await

### Reglas

✅ **Correcto**:
```csharp
// Siempre async en métodos I/O
public async Task<Machine> GetByCodeAsync(string code, CancellationToken cancellationToken)
{
    return await _context.Machines.FirstOrDefaultAsync(
        m => m.Code == code, 
        cancellationToken);
}

// CancellationToken siempre
public async Task HandleAsync(
    CreateProductionOrderCommand command,
    CancellationToken cancellationToken)
{
    await _repository.AddAsync(order, cancellationToken);
}

// Await siempre
var result = await GetDataAsync(cancellationToken);

// ConfigureAwait(false) en librerías
var result = await GetDataAsync(cancellationToken).ConfigureAwait(false);
```

❌ **Incorrecto**:
```csharp
// Sync sobre async
Task<Machine> GetByCode(string code) => GetByCodeAsync(code).Result;

// Sin CancellationToken
public async Task HandleAsync(CreateProductionOrderCommand command)

// Fire and forget
_ = SomeAsyncMethod();

// Async void (excepto event handlers)
public async void DoSomething() { }
```

## 🏗️ Dependency Injection

### Estructura en Program.cs

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Servicios de infraestructura
builder.Services.AddDbContext<MachineMonitoringContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();

// Handlers
builder.Services.AddScoped<ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto>, 
    CreateProductionOrderHandler>();

// Servicios
builder.Services.AddScoped<ISystemClock, SystemClock>();

// Mappers
builder.Services.AddScoped<IProductionOrderMapper, ProductionOrderMapper>();
```

### Validación de Constructor

```csharp
public sealed class ProductionOrdersController
{
    private readonly ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto> _createHandler;
    private readonly ICommandHandler<StartProductionOrderCommand, ProductionOrderDto> _startHandler;
    
    public ProductionOrdersController(
        ICommandHandler<CreateProductionOrderCommand, ProductionOrderDto> createHandler,
        ICommandHandler<StartProductionOrderCommand, ProductionOrderDto> startHandler)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _startHandler = startHandler ?? throw new ArgumentNullException(nameof(startHandler));
    }
}
```

## ✅ Checklist de Calidad

Antes de hacer commit, verificar:

- [ ] Código formatea (indentación, espacios)
- [ ] Nombres claros y descriptivos
- [ ] Métodos pequeños (< 20 líneas)
- [ ] Clases con una única responsabilidad
- [ ] Sin código duplicado
- [ ] Excepciones manejadas correctamente
- [ ] Async/await en todas las I/O
- [ ] CancellationToken propagado
- [ ] Null checks en entradas públicas
- [ ] Tests unitarios agregados
- [ ] Documentación actualizada
- [ ] Commit message descriptivo
- [ ] Sin secrets en código
- [ ] Sin TODO/FIXME sin contexto

## 📊 Métricas de Código

Objetivos:

- **Code Coverage**: ≥ 70% (production code)
- **Cyclomatic Complexity**: ≤ 10 por método
- **Method Length**: ≤ 20 líneas
- **Class Length**: ≤ 200 líneas
- **Duplication**: < 5%

---

**Última actualización**: 2026-05-06  
**Versión**: 1.0.0
