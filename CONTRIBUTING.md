# Guía de Contribución - MachineMonitoring

Gracias por tu interés en contribuir al proyecto MachineMonitoring. Esta guía explica cómo participar de manera efectiva.

## 📋 Tabla de Contenidos

- [Code of Conduct](#code-of-conduct)
- [Cómo Empezar](#cómo-empezar)
- [Reportar Bugs](#reportar-bugs)
- [Solicitar Funcionalidades](#solicitar-funcionalidades)
- [Submitting Changes](#submitting-changes)
- [Proceso de Review](#proceso-de-review)

## 📜 Code of Conduct

### Nuestro Compromiso

Nos comprometemos a proporcionar un ambiente inclusivo y respetuoso para todos.

### Comportamiento Esperado

- Usar lenguaje profesional y respetuoso
- Ser abierto a críticas constructivas
- Enfocarse en lo mejor para la comunidad
- Mostrar empatía hacia otros contribuyentes

### Consecuencias

El comportamiento inapropiado puede resultar en:
- Comentarios editados o eliminados
- Restricción temporal de participación
- Expulsión del proyecto

## 🚀 Cómo Empezar

### 1. Fork y Clonar

```bash
# Fork el repositorio en GitHub
# Luego, clonar tu fork
git clone https://github.com/YOUR_USERNAME/MachineMonitoring.git
cd MachineMonitoring

# Agregar upstream
git remote add upstream https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring.git

# Verificar
git remote -v
# origin: tu fork
# upstream: repositorio oficial
```

### 2. Configurar Ambiente Local

#### Requisitos

- .NET 10.0 SDK
- PostgreSQL 15+
- Git

#### Pasos

```bash
# 1. Restaurar dependencias
dotnet restore

# 2. Crear base de datos (en PostgreSQL)
CREATE DATABASE machinemonitoring;

# 3. Aplicar migraciones
dotnet ef database update --project MachineMonitoring.Infrastructure

# 4. Verificar compilación
dotnet build

# 5. Ejecutar tests
dotnet test

# 6. Iniciar API
dotnet run --project MachineMonitoring.Api
```

#### Variables de Entorno

Crear `.env` en raíz del proyecto:

```bash
# Database
CONNECTION_STRING=Host=localhost;Database=machinemonitoring;Username=postgres;Password=password

# MQTT
MQTT_HOST=localhost
MQTT_PORT=1883

# Logging
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5021
```

### 3. Crear Rama Feature

```bash
# Actualizar main
git checkout main
git pull upstream main

# Crear rama
git checkout -b feature/amazing-feature

# Formato: [type]/[description]
# feature/add-jwt-auth
# fix/oee-calculation-bug
# docs/update-readme
```

## 🐛 Reportar Bugs

### Antes de Reportar

- [ ] Verificar que el bug aún existe en `main`
- [ ] Buscar en issues existentes
- [ ] Revisar la documentación
- [ ] Probar en ambiente limpio

### Cómo Reportar

1. Ir a [Issues en GitHub](https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring/issues)
2. Click "New Issue"
3. Seleccionar template "Bug Report"
4. Completar toda la información

### Template de Bug Report

```markdown
## Descripción
[Descripción clara y concisa del bug]

## Pasos para Reproducir
1. [Paso 1]
2. [Paso 2]
3. [Paso 3]

## Comportamiento Esperado
[Qué debería pasar]

## Comportamiento Real
[Qué está pasando]

## Información del Sistema
- OS: [Windows/Mac/Linux]
- .NET Version: [10.0]
- PostgreSQL Version: [15]

## Logs
```
[Pegue los logs de error aquí]
```

## Posible Solución
[Si tienes ideas sobre cómo arreglarlo]
```

## 💡 Solicitar Funcionalidades

### Antes de Solicitar

- [ ] Revisar el roadmap en [KANBAN_UPDATE.md](./KANBAN_UPDATE.md)
- [ ] Buscar solicitudes similares

### Cómo Solicitar

1. Ir a [Issues](https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring/issues)
2. Click "New Issue"
3. Seleccionar template "Feature Request"

### Template de Solicitud

```markdown
## Descripción
[Descripción clara de la funcionalidad deseada]

## Caso de Uso
¿Cuál es el problema que resuelve?

## Solución Propuesta
[Cómo debería implementarse]

## Alternativas Consideradas
[Otras formas de abordar esto]

## Contexto Adicional
[Información relevante]
```

## ✏️ Submitting Changes

### Antes de Empezar

1. Crear un issue para discutir cambios mayores
2. Asignarte el issue
3. Esperar aprobación del mantenedor

### Durante el Desarrollo

#### Formato de Código

Seguir [PROFESSIONAL_STANDARDS.md](./PROFESSIONAL_STANDARDS.md).

**Resumen rápido**:
- PascalCase para clases, métodos públicos
- camelCase para variables locales
- UPPER_SNAKE_CASE para constantes
- Máximo 120 caracteres por línea
- 4 espacios de indentación
- Clases selladas por defecto (`sealed`)

#### Escribir Tests

Para cada funcionalidad:

```bash
# Unit tests
dotnet test MachineMonitoring.Domain.Tests
dotnet test MachineMonitoring.Application.Tests
dotnet test MachineMonitoring.Api.Tests
```

Ejemplo de test:

```csharp
[Fact]
public async Task GivenValidCommand_WhenHandle_ThenCreatesOrder()
{
    // Arrange
    var command = new CreateProductionOrderCommand("CREMER-001", 100);
    
    // Act
    var result = await _handler.HandleAsync(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("CREMER-001", result.MachineCode);
}
```

#### Actualizar Documentación

- [ ] README.md si cambios visibles al usuario
- [ ] PROFESSIONAL_STANDARDS.md si cambios de arquitectura
- [ ] Comentarios XML en métodos públicos

### Hacer Commits

**Reglas**:

1. **Commits atómicos**: Un commit = un cambio lógico
2. **Mensajes descriptivos**: Imperativo, < 50 caracteres
3. **Cada commit debe compilar y pasar tests**

```bash
# Hacer cambios
git add file1.cs file2.cs

# Commit con formato [TYPE] description
git commit -m "[feat] Add endpoint to start production order"

# Tipos permitidos
# [feat] - Nueva funcionalidad
# [fix] - Corrección de bug
# [test] - Tests
# [docs] - Documentación
# [refactor] - Refactoring
# [perf] - Rendimiento
```

**Ejemplos**:

✅ Correcto:
```
[feat] Add JWT authentication to API
[test] Add unit tests for OEE calculation
[fix] Fix null reference in metrics handler
```

❌ Incorrecto:
```
updated files
WIP
fixed stuff
```

### Hacer Push

```bash
# Actualizar con upstream
git fetch upstream
git rebase upstream/main

# Resolver conflictos si hay
# Luego:
git push origin feature/amazing-feature --force-with-lease
```

## 🔄 Proceso de Review

### Crear Pull Request

1. Push a tu rama
2. Ir a [Pull Requests](https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring/pulls)
3. Click "New Pull Request"
4. Completar template

### Template de PR

```markdown
## Descripción
[Qué cambia y por qué]

## Tipo de Cambio
- [ ] Bug fix
- [ ] Nueva funcionalidad
- [ ] Breaking change
- [ ] Cambio de documentación

## Issues Relacionados
Closes #123

## Cambios Principales
- [ ] Cambio 1
- [ ] Cambio 2
- [ ] Cambio 3

## Testing
- [ ] Unit tests agregados
- [ ] Tests existentes pasan
- [ ] Testing manual realizado

## Documentación
- [ ] README actualizado (si aplica)
- [ ] Comentarios en código agregados
- [ ] PROFESSIONAL_STANDARDS.md actualizado (si aplica)

## Checklist
- [ ] Mi código sigue los estándares del proyecto
- [ ] He ejecutado `dotnet build` sin errores
- [ ] Todos los tests pasan
- [ ] Commits están bien formateados
- [ ] No he agregado secrets o credenciales
- [ ] No hay código muerto o comentarios de debug
```

### Qué Esperar

1. **Revisión Automática** (CI/CD)
   - Compilación
   - Tests unitarios
   - Análisis de código

2. **Revisión Manual** (48 horas)
   - Mínimo 2 revisores
   - Feedback en comentarios

3. **Iteración**
   - Hacer cambios si es necesario
   - Responder comentarios
   - Push nuevos commits

4. **Merge** (cuando está aprobado)
   - Rebase y squash (si es necesario)
   - Merge a main

### Cómo Responder a Feedback

✅ **Correcto**:
```
Buen punto. Cambié la lógica a [nueva implementación].
¿Qué te parece?
```

✅ **También correcto**:
```
Entiendo. Creo que mi enfoque inicial es mejor porque [razón].
¿Podemos discutir?
```

❌ **Evitar**:
```
No, tu feedback es incorrecto.
Mi código está bien.
```

## 📚 Recursos Útiles

### Documentación del Proyecto

- [README.md](./README.md) - Overview del proyecto
- [PROFESSIONAL_STANDARDS.md](./PROFESSIONAL_STANDARDS.md) - Estándares de código
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Detalles de arquitectura
- [GPIO_MAPPING.md](./GPIO_MAPPING.md) - Documentación de hardware

### Links Externos

- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

## 🎯 Roadmap de Contribución

### Fases Abiertas para Contribuyentes

**Fase 4: Mejoras de API**
- Autenticación JWT
- Autorización basada en roles
- Paginación y filtrado
- Rate limiting

**Fase 5: Handlers GPIO Completos**
- Persistencia de alertas
- WebSocket para alertas en tiempo real
- Sistema de notificaciones

**Fase 6: Múltiples Máquinas**
- Configuración dinámica de GPIO
- Soporte para N máquinas

Ver [KANBAN_UPDATE.md](./KANBAN_UPDATE.md) para detalles completos.

## ❓ Preguntas?

- Crear una [Discussion](https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring/discussions)
- Revisar issues abiertos
- Contactar a los mantenedores

## 📄 Licencia

Al contribuir, aceptas que tu código será licenciado bajo MIT.

---

**Gracias por contribuir a MachineMonitoring!** 🎉

Cualquier pregunta, abre una Discussion o comenta en una Issue.
