# MachineMonitoring Platform

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blueviolet)](#architecture)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-green)](#status)
![GitHub last commit](https://img.shields.io/github/last-commit/Rioja-Nature-Pharma-Dev/MachineMonitoring)

Una plataforma RESTful de monitorización industrial que integra hardware GPIO, cálculo de métricas OEE y análisis de producción en tiempo real.

## 🎯 Descripción

**MachineMonitoring** es una solución empresarial de nivel producción para monitoreo y control de máquinas industriales. Proporciona una API RESTful completa, integración MQTT para eventos GPIO y cálculo automático de métricas de eficiencia (OEE: Overall Equipment Effectiveness).

### Casos de Uso Principales

- 📊 **Monitoreo de Producción**: Seguimiento en tiempo real de órdenes de fabricación
- 🔢 **Conteo de Unidades**: Registro automático de unidades buenas/malas mediante GPIO
- ⚠️ **Detección de Errores**: Alertas automáticas para errores de peso y etiquetado
- 📈 **Análisis de Eficiencia**: Cálculo de OEE (Disponibilidad × Rendimiento × Calidad)
- 📱 **API RESTful**: 18 endpoints documentados con OpenAPI/Swagger

## 🚀 Quick Start

### Requisitos Previos

- **.NET 10.0** o superior
- **PostgreSQL 15+**
- **MQTT Broker** (Mosquitto, HiveMQ, etc.)
- **PowerShell 5.1+** o **Bash** para scripts

### Instalación

```bash
# 1. Clonar repositorio
git clone https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring.git
cd MachineMonitoring

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar migraciones (automático en primer inicio)
dotnet run --project MachineMonitoring.Api
```

### Uso Básico

```bash
# Iniciar API (puerto 5021)
dotnet run --project MachineMonitoring.Api

# Acceso a documentación
# API: http://localhost:5021
# OpenAPI: http://localhost:5021/openapi/v1.json
```

### Ejemplo: Registrar Máquina

```bash
curl -X POST http://localhost:5021/api/machines \
  -H "Content-Type: application/json" \
  -d '{
    "code": "MAQUINA-001",
    "name": "Línea de Empaquetado A",
    "description": "Máquina empaquetadora principal"
  }'
```

## 📋 Características

### API Endpoints (18 total)

#### 🏭 Máquinas (3 endpoints)
| Verbo | Path | Descripción |
|-------|------|-------------|
| `POST` | `/api/machines` | Registrar nueva máquina |
| `GET` | `/api/machines/{code}` | Obtener máquina por código |
| `GET` | `/api/machines` | Listar todas las máquinas |

#### 📦 Órdenes de Producción (7 endpoints)
| Verbo | Path | Descripción |
|-------|------|-------------|
| `POST` | `/api/production-orders` | Crear orden |
| `GET` | `/api/production-orders/{id}` | Obtener orden |
| `POST` | `/api/production-orders/{id}/start` | Iniciar ejecución |
| `POST` | `/api/production-orders/{id}/finish` | Finalizar ejecución |
| `GET` | `/api/production-orders/active` | Obtener orden activa |
| `GET` | `/api/production-orders/{id}/counter` | Estado del contador |
| `GET` | `/api/production-orders/{id}/metrics` | Métricas OEE |

#### 🔢 Contador (4 endpoints)
| Verbo | Path | Descripción |
|-------|------|-------------|
| `POST` | `/api/counter/{id}/increment-good` | Registrar unidad buena |
| `POST` | `/api/counter/{id}/increment-bad` | Registrar unidad defectuosa |
| `GET` | `/api/counter/{id}` | Estado del contador |
| `GET` | `/api/counter/active/current` | Contador de orden activa |

#### 📊 Métricas (4 endpoints)
| Verbo | Path | Descripción |
|-------|------|-------------|
| `POST` | `/api/metrics/{id}/calculate` | Calcular OEE |
| `GET` | `/api/metrics/{id}` | Obtener métricas |
| `GET` | `/api/metrics/machine/{id}` | Métricas por máquina |
| `GET` | `/api/metrics/summary` | Resumen de métricas |

### Integración MQTT

**3 canales GPIO configurables**:

| GPIO | Topic | Función | Descripción |
|------|-------|---------|-------------|
| 23 | `cremer/gpio/23` | Contador | Incrementar contador de unidades |
| 22 | `cremer/gpio/22` | Error Peso | Detectar error de pesado |
| 19 | `cremer/gpio/19` | Error Etiqueta | Detectar error de etiquetado |

Documentación: Ver [GPIO_MAPPING.md](./GPIO_MAPPING.md)

### Métricas OEE

```
OEE = Availability × Performance × Quality × 100%

Availability  = (Tiempo activo / Tiempo total) × 100%
Performance   = (Unidades reales / Unidades planificadas) × 100%
Quality       = (Unidades buenas / Unidades totales) × 100%
```

## 🏗️ Arquitectura

```
┌────────────────────────────────────────┐
│         API LAYER (ASP.NET Core 10)    │
│  Controllers • OpenAPI/Swagger • Seeding
└──────────────┬───────────────────────┘
               │
┌──────────────v───────────────────────┐
│      APPLICATION LAYER                │
│  Handlers • DTOs • Repositories       │
│  Commands • Queries • ValueObjects    │
└──────────────┬───────────────────────┘
               │
┌──────────────v───────────────────────┐
│     INFRASTRUCTURE LAYER               │
│  EF Core • PostgreSQL • Repositories  │
│  Database Context • Migrations        │
└──────────────┬───────────────────────┘
               │
┌──────────────v───────────────────────┐
│       WORKER LAYER (MQTT)             │
│  MqttGpioListener • EventHandlers    │
│  HostedService • GPIO Configuration  │
└────────────────────────────────────────┘
```

### Patrones Implementados

- ✅ **Clean Architecture** - Separación de capas y responsabilidades
- ✅ **Domain-Driven Design** - Entidades de negocio rico
- ✅ **Repository Pattern** - Abstracción de datos
- ✅ **Command/Query Handlers** - Lógica de use cases
- ✅ **Value Objects** - Objetos de valor inmutables
- ✅ **Dependency Injection** - IoC container nativo
- ✅ **Async/Await** - Operaciones asincrónicas
- ✅ **SOLID Principles** - Código mantenible y extensible

Detalles: Ver [PROFESSIONAL_STANDARDS.md](./PROFESSIONAL_STANDARDS.md)

## 📁 Estructura del Proyecto

```
MachineMonitoring/
├── MachineMonitoring.Api/              # API REST (ASP.NET Core)
│   ├── Controllers/                    # 4 controllers, 18 endpoints
│   ├── Seeds/                          # Seeders automáticos
│   └── Program.cs                      # Configuración DI
│
├── MachineMonitoring.Application/      # Lógica de aplicación
│   ├── Handlers/                       # Command/Query handlers
│   ├── DTOs/                           # Data Transfer Objects
│   └── Repositories/                   # Interfaces de repositorio
│
├── MachineMonitoring.Domain/           # Dominio del negocio
│   ├── Entities/                       # Machine, ProductionOrder, etc.
│   ├── ValueObjects/                   # Objetos de valor
│   ├── Enums/                          # OrderStatus, MachineStatus, etc.
│   └── Exceptions/                     # Excepciones de dominio
│
├── MachineMonitoring.Infrastructure/   # Persistencia e integración
│   ├── Database/                       # EF Core, migrations
│   ├── Repositories/                   # Implementaciones
│   └── Services/                       # Servicios (SystemClock, etc.)
│
├── MachineMonitoring.Worker/           # Background services
│   ├── Consumers/                      # MQTT listener
│   ├── Services/                       # Hosted services
│   └── Extensions/                     # DI extensions
│
├── Software_Design_Document/           # Documentación LaTeX
│   └── sections/
│       └── 11_implementacion.tex       # Implementación técnica
│
└── 📚 Documentación
    ├── README.md                       # Este archivo
    ├── GPIO_MAPPING.md                 # Hardware y mapeo GPIO
    ├── PROFESSIONAL_STANDARDS.md       # Estándares empresariales
    ├── CONTRIBUTING.md                 # Guía para contribuyentes
    └── ARCHITECTURE.md                 # Detalles arquitectónicos
```

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=machinemonitoring;Username=postgres;Password=password"
  },
  "Mqtt": {
    "Host": "localhost",
    "Port": 1883,
    "ClientId": "MachineMonitoring-Worker",
    "Topics": [
      "cremer/gpio/23",
      "cremer/gpio/22",
      "cremer/gpio/19"
    ]
  }
}
```

### Variables de Entorno

```bash
# Base de datos
ASPNETCORE_ENVIRONMENT=Development
CONNECTION_STRING=Host=localhost;Database=machinemonitoring;...

# MQTT
MQTT_HOST=localhost
MQTT_PORT=1883
```

## 📊 Estado del Proyecto

| Aspecto | Completitud | Estado |
|---------|------------|--------|
| **API Endpoints** | 18/18 | ✅ 100% |
| **Controllers** | 4/4 | ✅ 100% |
| **MQTT Integration** | 3/3 GPIO | ✅ 100% |
| **Test Data** | Automático | ✅ ✓ |
| **Compilación** | 0 errores | ✅ ✓ |
| **Documentación** | Completa | ✅ ✓ |
| **GitHub** | Pushed | ✅ ✓ |

### Fases Completadas

- ✅ **Fase 1**: API RESTful (5/5 sub-fases)
- ✅ **Fase 2**: Datos de Prueba (2/2 sub-fases)
- ✅ **Fase 3**: Integración MQTT (2/2 sub-fases)

### Roadmap Futuro

- 🔜 **Fase 4**: Mejoras de API (autenticación JWT, paginación, rate limiting)
- 🔜 **Fase 5**: Handlers GPIO completos (alertas persistentes, WebSocket)
- 🔜 **Fase 6**: Múltiples máquinas (configuración dinámica)
- 🔜 **Fase 7**: Dashboard UI (React/Angular, reportes)
- 🔜 **Fase 8**: Integración empresarial (ERP, ML predictivo)

Detalles: Ver [KANBAN_UPDATE.md](./KANBAN_UPDATE.md)

## 🧪 Testing

### Unit Tests

```bash
dotnet test MachineMonitoring.Domain.Tests
dotnet test MachineMonitoring.Application.Tests
dotnet test MachineMonitoring.Api.Tests
```

### Integration Tests

```bash
# Requiere PostgreSQL en ejecución
dotnet test MachineMonitoring.IntegrationTests
```

### Manual Testing

```bash
# Con API en ejecución (http://localhost:5021)
# Ver documentación en /openapi/v1.json o usar Postman
```

## 🔐 Seguridad

### Estado Actual
- ❌ Autenticación: No implementada (Fase 4.1)
- ❌ Autorización: No implementada (Fase 4.2)
- ⚠️ CORS: Abierto en desarrollo
- ⚠️ Rate Limiting: No implementado (Fase 4.4)

### Roadmap de Seguridad
1. Autenticación JWT (Fase 4.1)
2. Control de acceso por roles (Fase 4.2)
3. Rate limiting y protección CORS (Fase 4.4)

**⚠️ NO deployer a producción sin autenticación.**

## 📚 Documentación

| Documento | Propósito |
|-----------|-----------|
| [README.md](./README.md) | Este archivo |
| [GPIO_MAPPING.md](./GPIO_MAPPING.md) | Hardware y mapeo GPIO |
| [PROFESSIONAL_STANDARDS.md](./PROFESSIONAL_STANDARDS.md) | Estándares de código |
| [CONTRIBUTING.md](./CONTRIBUTING.md) | Guía para colaboradores |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Detalles arquitectónicos |
| [11_implementacion.tex](./Software_Design_Document/sections/11_implementacion.tex) | Documento técnico LaTeX |

### OpenAPI/Swagger

La documentación interactiva está disponible en:
```
http://localhost:5021/openapi/v1.json (Schema)
```

## 🤝 Contribuir

Para contribuir al proyecto, consulta [CONTRIBUTING.md](./CONTRIBUTING.md).

### Resumen Rápido

1. Fork el repositorio
2. Crea una rama feature (`git checkout -b feature/amazing-feature`)
3. Commit tus cambios (`git commit -m 'Add amazing feature'`)
4. Push a la rama (`git push origin feature/amazing-feature`)
5. Abre un Pull Request

### Estándares de Código

Ver [PROFESSIONAL_STANDARDS.md](./PROFESSIONAL_STANDARDS.md) para:
- Nombrado de variables y métodos
- Estructura de clases
- Manejo de errores
- Comentarios y documentación
- Commits y PRs

## 📞 Soporte

### Problemas Comunes

**¿La API no inicia?**
- Verificar PostgreSQL está en ejecución
- Revisar `appsettings.json`
- Ejecutar `dotnet build` primero

**¿MQTT no conecta?**
- Verificar broker MQTT está activo
- Revisar configuración en `appsettings.json`
- Revisar logs en consola

**¿Base de datos no se crea?**
- Ejecutar: `dotnet ef database update --project MachineMonitoring.Infrastructure`
- Verificar conexión PostgreSQL

### Reportar Issues

Usa [Issues en GitHub](https://github.com/Rioja-Nature-Pharma-Dev/MachineMonitoring/issues) con template estándar.

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver [LICENSE](./LICENSE) para detalles.

## 👥 Autores

- **Equipo de Desarrollo** - Departamento IT, Natura Pharma
- **Arquitectura**: Clean Architecture / Domain-Driven Design
- **Framework**: .NET Core 10.0, ASP.NET Core 10

## 🙋 Preguntas Frecuentes

**¿Puedo usar esto en producción?**
Sí, el MVP está listo para producción, pero asegúrate de:
- Implementar autenticación JWT (Fase 4.1)
- Configurar HTTPS
- Proteger credenciales en variables de entorno
- Realizar testing de carga
- Configurar backups de base de datos

**¿Cuál es la hoja de ruta?**
Ver [KANBAN_UPDATE.md](./KANBAN_UPDATE.md) para detalles de Fases 4-8.

**¿Cómo contribuir?**
Ver [CONTRIBUTING.md](./CONTRIBUTING.md).

**¿Soporte para múltiples máquinas?**
Planeado en Fase 6. Actualmente soporta 1 máquina con GPIO configurable.

---

**Versión**: 1.0.0 (MVP Completado)  
**Última actualización**: 2026-05-06  
**Estado**: ✅ Production Ready (sin autenticación)
