# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run tests for a single project
dotnet test MachineMonitoring.Domain.Tests
dotnet test MachineMonitoring.Application.Tests

# Run API
dotnet run --project MachineMonitoring.Api

# EF Core migrations (Infrastructure has the DbContext; Api is the startup project)
dotnet ef migrations add <Name> --project MachineMonitoring.Infrastructure --startup-project MachineMonitoring.Api
dotnet ef database update --project MachineMonitoring.Infrastructure --startup-project MachineMonitoring.Api
```

## Architecture

**Clean Architecture / DDD** with four source projects and a background worker:

```
Domain → Application → Infrastructure
                     ↑
                    Api (startup)
                    Worker (background)
                    Contracts (shared message contracts)
```

- **Domain** (`MachineMonitoring.Domain`): Entities, value objects, enums. No external dependencies. Entities enforce all invariants in their constructors and behaviour methods — never bypass them by setting properties directly.
- **Application** (`MachineMonitoring.Application`): Use-case handlers (plain classes, no MediatR), DTOs, repository interfaces, `IClock` abstraction. Each handler receives a command/query record and returns a DTO. Application has no reference to Infrastructure.
- **Infrastructure** (`MachineMonitoring.Infrastructure`): EF Core + PostgreSQL (Npgsql). Implements repository interfaces. `DependencyInjection.AddInfrastructure()` wires everything. Each repository calls `SaveChangesAsync` immediately — there is no shared Unit of Work.
- **Api** (`MachineMonitoring.Api`): ASP.NET Core 10 Web API. OpenAPI via `MapOpenApi()`. References Application, Infrastructure, and Contracts.
- **Worker** (`MachineMonitoring.Worker`): Background service scaffold. Planned folders: `Consumers/` (MQTT), `Jobs/`, `Processors/`. References Application only.
- **Contracts** (`MachineMonitoring.Contracts`): Shared message/event contracts for the worker and API.

## Key domain concepts

- **Machine** — the monitored industrial machine, identified by a unique `Code` (uppercased).
- **MachineInputSource** — how telemetry enters the system: `Mqtt`, `HttpEndpoint`, `Manual`, or `FileImport`.
- **MachineParameterDefinition** / **MachineInputMapping** — describe which sensor fields map to which typed parameters.
- **MachineReadingRaw / MachineReadingNormalized** — raw ingested values and their normalised counterparts.
- **ProductionOrder** lifecycle: `Created → InProgress → Paused → WaitingManualProcess → ManualProcessInProgress → Finished | Cancelled`. Transitions are enforced by domain methods (`Start`, `Pause`, `Resume`, `Finish`, `Cancel`, …).
- **ProductionPause** — a pause on an active order; `CountsTowardsMetrics` controls whether pause time is subtracted from active time in OEE.
- **ProductionCounter** — tracks good/bad unit counts for an order.
- **ProductionMetrics** — OEE calculation (Availability × Performance × Quality) computed by `CalculateProductionMetricsHandler` after automatic production ends.
- **MachineRuleDefinition / AlertRuleDefinition / CalculatedMetricDefinition** — configurable expression-based rules and derived metrics per machine (expressions stored as strings, evaluation engine is TBD).
- **MachineAlert** — active/resolved alerts raised against a machine.

## Conventions

- Value objects (`ParameterCode`, `MeasurementUnit`, `ExternalFieldPath`) are `sealed record`s; they normalise their input in the constructor.
- Use `IClock` (not `DateTime.UtcNow`) everywhere time is needed so tests can control time.
- EF Core entity configurations live in `Infrastructure/Persistence/Configurations/`, one file per entity, applied via `ApplyConfigurationsFromAssembly`.
- Test projects mirror source projects (Api.Tests, Application.Tests, Domain.Tests, Infrastructure.Tests) and use xUnit.

## Project Context

### Overview

MachineMonitoring is a .NET + PostgreSQL platform for monitoring heterogeneous industrial machines in a corporate intranet. The system is designed to absorb new machine types progressively without redesigning the core.

**Architecture layers:**
- **Domain** — Common business model (Machine, ProductionOrder, ProductionPause, ProductionCounter, ProductionMetrics, ProductionManualProcess, MachineAlert, etc.)
- **Application** — Use cases and orchestration (commands, queries, DTOs, repository interfaces, IClock abstraction)
- **Infrastructure** — EF Core + PostgreSQL persistence, concrete repository implementations
- **Api** — ASP.NET Core 10 HTTP endpoints
- **Worker** — Background processing (MQTT integration planned)
- **Contracts** — Shared message/event contracts

### First Real Case: Cremer

Cremer is the first production machine integrated into the system. It is a packaging/production machine connected via MQTT with GPIO-based event signals:
- **GPIO 23** — unit count (falling edge detection)
- **GPIO 22** — weight failure
- **GPIO 19** — label failure

Cremer operates on production orders with the lifecycle: `Created → InProgress → Paused → WaitingManualProcess → ManualProcessInProgress → Finished | Cancelled`. Metrics (OEE, availability, performance, quality) are calculated when automatic production ends, before the optional manual phase.

Key domain entities for Cremer:
- `ProductionOrder` — includes units per box, box type, reprocess flag, manual process flag, final box total, bottle format, product type, units per bottle, standard reference, estimated time
- `ProductionPause` — allows pause type and counts-towards-metrics to be absent initially and assigned later
- `ProductionCounter` — single counter entity (replaces legacy Botes and BottleCounter)
- `ProductionManualProcess` — represents the post-automatic manual phase (corresponds to legacy Acumula)

### Current Status

The Domain and Application layers are complete for the Cremer flow. Infrastructure has DbContext, repository implementations, and the initial migration applied. The database exists. The API layer has not yet been built — the next step is exposing minimum endpoints to register the Cremer machine, manage orders (create, start, pause, resume, finish), handle counters, and query metrics. After API validation, MQTT integration via the Worker layer will follow.

### Design Philosophy

The architecture is intentionally **intermediate** between a single rigid model and a fully generic rule engine:
- A stable common core handles universal concepts (machine, readings, state, alerts, orders)
- Each machine adds its own parameter definitions, input mappings, rules, alerts, and operational logic
- This allows fast progress with a real case without foreclosing future expansion to new machine types
