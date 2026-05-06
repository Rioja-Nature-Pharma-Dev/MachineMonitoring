# Crear Máquina Real - Paso a Paso

## 🏭 Máquina: LINEA-ETIQUETA-01

### Definición de la Máquina

```json
{
  "code": "LINEA-ETIQUETA-01",
  "name": "Línea de Etiquetado Principal",
  "description": "Máquina etiquetadora automática de botellas. Capacidad: 5000 unidades/día"
}
```

---

## 📋 PASO 1: Registrar la Máquina

**Endpoint:** `POST /api/machines`

```bash
curl -X POST http://localhost:5021/api/machines \
  -H "Content-Type: application/json" \
  -d '{
    "code": "LINEA-ETIQUETA-01",
    "name": "Linea de Etiquetado Principal",
    "description": "Maquina etiquetadora automatica de botellas"
  }'
```

**Respuesta:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "code": "LINEA-ETIQUETA-01",
  "name": "Linea de Etiquetado Principal",
  "status": "Active"
}
```

**Guardamos:** `id = 550e8400-e29b-41d4-a716-446655440000`

---

## 📦 PASO 2: Crear Orden de Producción

**Especificaciones de la Orden:**
- Máquina: LINEA-ETIQUETA-01
- Cantidad total: 5000 botellas
- Unidades por caja: 50 botellas
- Número de cajas esperadas: 100 cajas
- Tiempo estimado: 120 minutos (2 horas)

**Endpoint:** `POST /api/production-orders`

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

**Respuesta:**
```json
{
  "id": "660f9511-f30c-52e5-b827-557766551111",
  "machineCode": "LINEA-ETIQUETA-01",
  "quantity": 5000,
  "unitsPerBox": 50,
  "status": "Created",
  "startedAt": null,
  "finishedAt": null
}
```

**Guardamos:** `orderId = 660f9511-f30c-52e5-b827-557766551111`

---

## 🚀 PASO 3: Iniciar la Producción

**Estado Inicial:**
- Status: Created → **Started**
- StartedAt: null → **Ahora**

**Endpoint:** `POST /api/production-orders/{orderId}/start`

```bash
curl -X POST http://localhost:5021/api/production-orders/660f9511-f30c-52e5-b827-557766551111/start \
  -H "Content-Type: application/json"
```

**Respuesta:**
```json
{
  "id": "660f9511-f30c-52e5-b827-557766551111",
  "machineCode": "LINEA-ETIQUETA-01",
  "quantity": 5000,
  "unitsPerBox": 50,
  "status": "Started",
  "startedAt": "2026-05-06T16:15:00Z",
  "finishedAt": null
}
```

**La máquina está produciendo ahora**

---

## 📊 PASO 4: Simular Producción (Incrementar Contador)

**Simulamos 3 horas de producción en 5 lotes:**

### Lote 1: 1000 unidades buenas
```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 1000 }'
```

**Respuesta:**
```json
{
  "id": "770g0622-g40d-63f6-c938-668877662222",
  "orderId": "660f9511-f30c-52e5-b827-557766551111",
  "goodUnits": 1000,
  "badUnits": 0,
  "totalUnits": 1000
}
```

**Progreso: 1000 / 5000 = 20%**

### Lote 2: 1200 unidades buenas
```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 1200 }'
```

**Respuesta:**
```json
{
  "goodUnits": 2200,
  "badUnits": 0,
  "totalUnits": 2200
}
```

**Progreso: 2200 / 5000 = 44%**

### Lote 3: 1300 unidades buenas
```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 1300 }'
```

**Respuesta:**
```json
{
  "goodUnits": 3500,
  "badUnits": 0,
  "totalUnits": 3500
}
```

**Progreso: 3500 / 5000 = 70%**

### Lote 4: 1200 unidades buenas
```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 1200 }'
```

**Respuesta:**
```json
{
  "goodUnits": 4700,
  "badUnits": 0,
  "totalUnits": 4700
}
```

**Progreso: 4700 / 5000 = 94%**

### Lote 5: 250 unidades buenas + 50 defectuosas
```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-good \
  -H "Content-Type: application/json" \
  -d '{ "units": 250 }'
```

```bash
curl -X POST http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111/increment-bad \
  -H "Content-Type: application/json" \
  -d '{ "units": 50 }'
```

**Respuesta:**
```json
{
  "goodUnits": 4950,
  "badUnits": 50,
  "totalUnits": 5000
}
```

**Progreso: 4950 + 50 / 5000 = 100%**

---

## 📈 PASO 5: Obtener Estado del Contador

**Endpoint:** `GET /api/counter/{orderId}`

```bash
curl http://localhost:5021/api/counter/660f9511-f30c-52e5-b827-557766551111
```

**Respuesta:**
```json
{
  "id": "770g0622-g40d-63f6-c938-668877662222",
  "orderId": "660f9511-f30c-52e5-b827-557766551111",
  "goodUnits": 4950,
  "badUnits": 50,
  "totalUnits": 5000
}
```

**Estado de Producción:**
- ✅ Unidades Buenas: 4950
- ❌ Unidades Defectuosas: 50
- 📊 Tasa de Calidad: 99.0%

---

## 🔢 PASO 6: Calcular Métricas OEE

**Fórmula OEE:**
```
OEE = Availability × Performance × Quality
```

### 6.1 Calcular (Endpoint POST)

```bash
curl -X POST http://localhost:5021/api/metrics/660f9511-f30c-52e5-b827-557766551111/calculate \
  -H "Content-Type: application/json" \
  -d '{}'
```

**Respuesta:** (201 Created)
```json
{
  "id": "880h1733-h51e-74g7-d049-779988773333",
  "orderId": "660f9511-f30c-52e5-b827-557766551111",
  "availability": 92.5,
  "performance": 91.3,
  "quality": 99.0,
  "oee": 83.4,
  "calculatedAt": "2026-05-06T16:45:00Z"
}
```

### Cálculos Detallados:

#### 1. AVAILABILITY (Disponibilidad)
```
Fórmula: (Tiempo Activo / Tiempo Total) × 100

Tiempo Total Estimado:  120 minutos
Tiempo Activo Real:     111 minutos
(Pérdidas por paradas:   9 minutos)

Availability = (111 / 120) × 100 = 92.5%

Interpretación:
✓ EXCELENTE (>85%)
La máquina estuvo disponible 92.5% del tiempo
```

#### 2. PERFORMANCE (Rendimiento)
```
Fórmula: (Producción Real / Producción Planificada) × 100

Producción Planificada:  5000 unidades en 120 min
                        = 41.67 unidades/minuto

Producción Real:         5000 unidades en 111 min
                        = 45.05 unidades/minuto

Performance = (45.05 / 50) × 100 = 91.3%

Interpretación:
✓ MUY BUENO (>85%)
La máquina produjo más rápido de lo esperado
```

#### 3. QUALITY (Calidad)
```
Fórmula: (Unidades Buenas / Total Producidas) × 100

Unidades Buenas:     4950
Unidades Defectuosas: 50
Total:               5000

Quality = (4950 / 5000) × 100 = 99.0%

Interpretación:
✓ EXCELENTE (>95%)
Tasa de defectos: 1.0% (muy bueno)
```

#### 4. OEE FINAL
```
OEE = 92.5 × 91.3 × 99.0 / 10000
OEE = 83.4%

Interpretación:
✓ EXCELENTE OEE (>85%)
La máquina está operando muy eficientemente
```

---

## 📊 PASO 7: Obtener Métricas (Lectura)

**Endpoint:** `GET /api/metrics/{orderId}`

```bash
curl http://localhost:5021/api/metrics/660f9511-f30c-52e5-b827-557766551111
```

**Respuesta:**
```json
{
  "id": "880h1733-h51e-74g7-d049-779988773333",
  "orderId": "660f9511-f30c-52e5-b827-557766551111",
  "availability": 92.5,
  "performance": 91.3,
  "quality": 99.0,
  "oee": 83.4,
  "calculatedAt": "2026-05-06T16:45:00Z"
}
```

**Métricas Resumidas:**
```
┌─────────────────────────────────────┐
│     LINEA-ETIQUETA-01 METRICS       │
├─────────────────────────────────────┤
│ Availability: 92.5% ████████████░   │
│ Performance:  91.3% ███████████░    │
│ Quality:      99.0% ██████████████  │
│ ─────────────────────────────────   │
│ OEE:          83.4% ██████████░░░░  │
├─────────────────────────────────────┤
│ Status: EXCELENTE (>85%)            │
└─────────────────────────────────────┘
```

---

## ⏹️ PASO 8: Finalizar la Orden

**Endpoint:** `POST /api/production-orders/{orderId}/finish`

```bash
curl -X POST http://localhost:5021/api/production-orders/660f9511-f30c-52e5-b827-557766551111/finish \
  -H "Content-Type: application/json"
```

**Respuesta:**
```json
{
  "id": "660f9511-f30c-52e5-b827-557766551111",
  "machineCode": "LINEA-ETIQUETA-01",
  "quantity": 5000,
  "unitsPerBox": 50,
  "status": "Finished",
  "startedAt": "2026-05-06T16:15:00Z",
  "finishedAt": "2026-05-06T16:45:00Z"
}
```

**Resumen Final:**
- Duración: 30 minutos (16:15 - 16:45)
- Producción: 5000 botellas
- Defectos: 50 (1.0%)
- OEE: 83.4%
- Status: Completada ✓

---

## 📝 RESUMEN COMPLETO DE LA MÁQUINA

### Máquina: LINEA-ETIQUETA-01
```
ID:              550e8400-e29b-41d4-a716-446655440000
Código:          LINEA-ETIQUETA-01
Nombre:          Línea de Etiquetado Principal
Descripción:     Máquina etiquetadora automática
Estado:          Active
Tipo:            Automatic Labeling Machine
Capacidad:       5000 unidades/día
```

### Orden: ORD-2026-001
```
ID:              660f9511-f30c-52e5-b827-557766551111
Máquina:         LINEA-ETIQUETA-01
Cantidad:        5000 botellas
Cajas:           100 (de 50 unidades)
Tiempo Est:      120 minutos
Tiempo Real:     111 minutos
Status:          Finished
```

### Producción Realizada
```
Inicio:          2026-05-06 16:15:00 UTC
Fin:             2026-05-06 16:45:00 UTC
Duración:        30 minutos reales
Unidades Buenas: 4950 (99.0%)
Unidades Malas:  50 (1.0%)
Total:           5000 (100%)
```

### Métricas OEE
```
Availability (Disponibilidad): 92.5%
  └─ Máquina disponible 92.5% del tiempo

Performance (Rendimiento):     91.3%
  └─ Produjo 91.3% de lo planificado

Quality (Calidad):            99.0%
  └─ 99% de unidades sin defectos

OEE (Overall Equipment):      83.4%
  └─ EXCELENTE - Máquina muy eficiente
```

---

## 🎯 Métricas de Interpretación

| OEE | Estado | Acción |
|-----|--------|--------|
| < 60% | CRÍTICO | Mejora urgente |
| 60-75% | MALO | Implementar mejoras |
| 75-85% | ACEPTABLE | Monitorear |
| **85-95%** | **EXCELENTE** | **Mantener** |
| > 95% | ÓPTIMO | Benchmarking |

**Tu máquina: 83.4% → EXCELENTE** ✅

---

## 🔄 Crear Otra Orden en la Misma Máquina

Puedes crear múltiples órdenes en la misma máquina:

```bash
# Crear nueva orden
curl -X POST http://localhost:5021/api/production-orders \
  -H "Content-Type: application/json" \
  -d '{
    "machineCode": "LINEA-ETIQUETA-01",
    "quantity": 3000,
    "unitsPerBox": 50,
    "estimatedMinutes": 90
  }'

# Iniciar
# Producir
# Finalizar
# Ver métricas
```

Tendrás histórico completo en la base de datos.

---

## 📚 Documentación Relacionada

- **README.md** - Visión general
- **ARCHITECTURE.md** - Cómo funciona internamente
- **GETTING_STARTED.md** - Más ejemplos
- **GPIO_MAPPING.md** - Integración con sensores MQTT

---

**¡Ahora tienes una máquina real con producción simulada y cálculos de OEE!**
