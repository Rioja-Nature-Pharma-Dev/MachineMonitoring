# Getting Started - MachineMonitoring

Guía rápida de qué está listo y cómo usarlo.

## ✅ ¿Qué Está Completo?

### 1. API RESTful Funcional (18 endpoints)
- ✅ Máquinas (POST/GET)
- ✅ Órdenes de producción (CRUD)
- ✅ Contador (incrementar, obtener)
- ✅ Métricas OEE (calcular, obtener)
- ✅ Documentación automática (OpenAPI)

### 2. MQTT Integration
- ✅ GPIO 23 (contador)
- ✅ GPIO 22 (error peso)
- ✅ GPIO 19 (error etiqueta)
- ✅ Event handlers

### 3. Datos de Prueba Automáticos
- ✅ Máquina CREMER (ya existe)
- ✅ Orden de prueba CREMER-TEST-001
- ✅ Se crean al iniciar en Development

### 4. Documentación Profesional (4,000+ líneas)
- ✅ README.md
- ✅ PROFESSIONAL_STANDARDS.md
- ✅ CONTRIBUTING.md
- ✅ ARCHITECTURE.md
- ✅ GITHUB_ISSUES.md
- ✅ RECOMMENDATIONS.md

## 🚀 Qué Puedes Hacer AHORA

### Opción A: Probar con Máquina de Ejemplo (CREMER)

**Paso 1: Iniciar la API**

```bash
cd F:\Dev\Monitorizacion\MachineMonitoring

# Compilar
dotnet build

# Ejecutar
dotnet run --project MachineMonitoring.Api
```

La API inicia en: `http://localhost:5021`

**Paso 2: Ver máquina de ejemplo**

```bash
curl http://localhost:5021/api/machines
```

Respuesta:
```json
[
  {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "code": "CREMER",
    "name": "CREMER Packaging Machine",
    "status": "Active"
  }
]
```

**Paso 3: Ver orden de prueba**

```bash
curl http://localhost:5021/api/production-orders/active
```

Verás la orden `CREMER-TEST-001` con 1000 unidades planificadas.

**Paso 4: Incrementar contador**

```bash
curl -X POST http://localhost:5021/api/counter/{id}/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 1 }'
```

**Paso 5: Ver métricas OEE**

```bash
curl http://localhost:5021/api/metrics/{order-id}
```

Verás cálculo de OEE: Availability × Performance × Quality

---

### Opción B: Crear Nueva Máquina (Tu Máquina Real)

**Paso 1: Registrar la máquina**

```bash
curl -X POST http://localhost:5021/api/machines \
  -H "Content-Type: application/json" \
  -d '{
    "code": "LINEA-ETIQUETA-01",
    "name": "Línea de Etiquetado Principal",
    "description": "Máquina de etiquetado de botellas - Producción"
  }'
```

Respuesta (guarda el ID):
```json
{
  "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "code": "LINEA-ETIQUETA-01",
  "name": "Línea de Etiquetado Principal",
  "status": "Active"
}
```

**Paso 2: Crear orden de producción**

```bash
curl -X POST http://localhost:5021/api/production-orders \
  -H "Content-Type: application/json" \
  -d '{
    "machineCode": "LINEA-ETIQUETA-01",
    "quantity": 5000,
    "unitsPerBox": 50,
    "estimatedMinutes": 120
  }'
```

Respuesta (guarda el ID):
```json
{
  "id": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
  "machineCode": "LINEA-ETIQUETA-01",
  "quantity": 5000,
  "unitsPerBox": 50,
  "status": "Created"
}
```

**Paso 3: Iniciar orden**

```bash
curl -X POST http://localhost:5021/api/production-orders/yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy/start \
  -H "Content-Type: application/json"
```

**Paso 4: Simular producción (incrementar contador)**

```bash
# Hacer esto múltiples veces para simular producción
curl -X POST http://localhost:5021/api/counter/yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 10 }'
```

**Paso 5: Ver progreso**

```bash
curl http://localhost:5021/api/production-orders/yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy/metrics
```

Verás:
```json
{
  "availability": 95.5,
  "performance": 87.3,
  "quality": 100.0,
  "oee": 83.2
}
```

**Paso 6: Finalizar orden**

```bash
curl -X POST http://localhost:5021/api/production-orders/yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy/finish \
  -H "Content-Type: application/json"
```

---

## 📊 Estructura de Datos (Base de Datos)

Cuando registras una máquina, se crea automáticamente:

```
Machine (LINEA-ETIQUETA-01)
├─ ProductionOrder (Orden 1)
│  ├─ ProductionCounter
│  │  └─ good_units: 500
│  │     bad_units: 0
│  │
│  └─ ProductionMetrics
│     └─ OEE: 87.5%
│
├─ ProductionOrder (Orden 2)
│  ├─ ProductionCounter
│  │  └─ good_units: 450
│  │     bad_units: 50
│  │
│  └─ ProductionMetrics
│     └─ OEE: 82.0%
```

---

## 🔌 Integración MQTT (Opcional)

Si tienes un broker MQTT (Mosquitto, HiveMQ, etc.):

**Configurar en appsettings.json**:

```json
{
  "Mqtt": {
    "Host": "localhost",
    "Port": 1883,
    "ClientId": "MachineMonitoring-Worker",
    "Topics": [
      "linea-etiqueta-01/gpio/23",
      "linea-etiqueta-01/gpio/22",
      "linea-etiqueta-01/gpio/19"
    ]
  }
}
```

**Enviar evento GPIO (desde tu PLC/sensor)**:

```bash
# Simular contador (GPIO 23)
mosquitto_pub -h localhost -t "linea-etiqueta-01/gpio/23" -m "1"

# Simular error de peso (GPIO 22)
mosquitto_pub -h localhost -t "linea-etiqueta-01/gpio/22" -m "ERROR_WEIGHT"

# Simular error de etiqueta (GPIO 19)
mosquitto_pub -h localhost -t "linea-etiqueta-01/gpio/19" -m "ERROR_LABEL"
```

---

## 🎯 Casos de Uso Prácticos

### Caso 1: Monitoreo en Tiempo Real

```
Máquina produciendo
    ↓
Cada unidad → POST /api/counter/{id}/increment-good
    ↓
GET /api/production-orders/{id}/metrics
    ↓
Ver OEE en tiempo real
```

### Caso 2: Análisis de Eficiencia

```
Orden completada
    ↓
GET /api/metrics/machine/{id}
    ↓
Ver histórico de todas las órdenes
    ↓
Identificar patrones, mejoras
```

### Caso 3: Alertas de Error

```
Sensor detecta error (GPIO 22)
    ↓
MQTT event → cremer/gpio/22
    ↓
Worker recibe y procesa
    ↓
Registra defecto en base de datos
    ↓
Notificar al operador (future)
```

---

## 📱 Alternativa: Usar Postman

En lugar de curl:

1. Descargar [Postman](https://www.postman.com/)
2. Ir a `http://localhost:5021/openapi/v1.json`
3. Importar en Postman (File → Import)
4. Tendrás todos los endpoints documentados
5. Hacer requests directamente

---

## 🧪 Probar Todo (Script PowerShell)

Crear archivo `test-api.ps1`:

```powershell
$BaseUrl = "http://localhost:5021/api"
$Headers = @{"Content-Type" = "application/json"}

# 1. Registrar máquina
$machine = Invoke-RestMethod -Uri "$BaseUrl/machines" -Method POST `
  -Headers $Headers `
  -Body (@{
    code = "TEST-MAQUINA-01"
    name = "Máquina de Test"
    description = "Para testing"
  } | ConvertTo-Json)

Write-Host "✓ Máquina registrada: $($machine.id)"

# 2. Crear orden
$order = Invoke-RestMethod -Uri "$BaseUrl/production-orders" -Method POST `
  -Headers $Headers `
  -Body (@{
    machineCode = "TEST-MAQUINA-01"
    quantity = 1000
    unitsPerBox = 10
    estimatedMinutes = 60
  } | ConvertTo-Json)

Write-Host "✓ Orden creada: $($order.id)"

# 3. Iniciar orden
Invoke-RestMethod -Uri "$BaseUrl/production-orders/$($order.id)/start" -Method POST `
  -Headers $Headers | Out-Null

Write-Host "✓ Orden iniciada"

# 4. Incrementar contador (simular 100 unidades)
for ($i = 1; $i -le 10; $i++) {
  Invoke-RestMethod -Uri "$BaseUrl/counter/$($order.id)/increment-good" -Method POST `
    -Headers $Headers `
    -Body (@{ units = 10 } | ConvertTo-Json) | Out-Null
}

Write-Host "✓ Contador incrementado: 100 unidades"

# 5. Ver métricas
$metrics = Invoke-RestMethod -Uri "$BaseUrl/metrics/$($order.id)" -Method GET

Write-Host "`n=== RESULTADOS OEE ==="
Write-Host "Availability: $($metrics.availability)%"
Write-Host "Performance:  $($metrics.performance)%"
Write-Host "Quality:      $($metrics.quality)%"
Write-Host "OEE:          $($metrics.oee)%"
```

Ejecutar:
```bash
powershell -ExecutionPolicy Bypass -File test-api.ps1
```

---

## 📊 Dashboard Futuro (No implementado aún)

Lo que tendrás en Fase 7:

```
┌─────────────────────────────────────┐
│  MachineMonitoring Dashboard        │
├─────────────────────────────────────┤
│ Máquina: LINEA-ETIQUETA-01          │
│ Status: ▓▓▓▓▓ Produciendo            │
│                                     │
│ Orden Activa: ORD-2025-001          │
│ Unidades: 4,250 / 5,000 (85%)      │
│ Tiempo: 85 min / 120 min (71%)     │
│                                     │
│ OEE: 87.5% ┌──────────────┐        │
│            │Availability: 95%       │
│            │Performance:  92%       │
│            │Quality:      100%      │
│            └──────────────┘        │
│                                     │
│ Alertas: ✓ Sin errores              │
│                                     │
└─────────────────────────────────────┘
```

---

## 🔄 Próximo Paso

Ahora puedes:

1. **Opción A**: Probar con máquinas de ejemplo (5 min)
   ```bash
   dotnet run --project MachineMonitoring.Api
   ```

2. **Opción B**: Crear tus máquinas reales (15 min)
   - Usar los curl commands arriba
   - Registrar tus máquinas
   - Simular producción

3. **Opción C**: Configurar GitHub (para equipo)
   - Crear GitHub Project
   - Configurar branch protection
   - Crear issues

---

## 📞 Información Útil

| Necesito... | Comando |
|-------------|---------|
| Ver documentación API | `http://localhost:5021/openapi/v1.json` |
| Ver código standards | `cat PROFESSIONAL_STANDARDS.md` |
| Entender arquitectura | `cat ARCHITECTURE.md` |
| Agregar nueva máquina | `curl -X POST .../api/machines` |
| Ver todas las máquinas | `curl http://localhost:5021/api/machines` |
| Crear orden | `curl -X POST .../api/production-orders` |
| Incrementar contador | `curl -X POST .../api/counter/{id}/increment-good` |

---

**¿Qué quieres hacer?**

```
[ ] Probar API ahora mismo
[ ] Crear mis máquinas reales
[ ] Configurar GitHub (issues, Kanban)
[ ] Implementar Fase 4 (autenticación)
[ ] Algo más
```
