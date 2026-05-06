# 🏭 Crear Máquina CON Configuración Personalizada

Esta es la forma **CORRECTA** de crear una máquina: definiendo todos sus parámetros, GPIOs y cálculos.

---

## 📋 PASO 0: Registrar la Máquina Base

Primero creas la máquina simple:

```bash
curl -X POST http://localhost:5021/api/machines \
  -H "Content-Type: application/json" \
  -d '{
    "code": "LINEA-ETIQUETA-PREMIUM",
    "name": "Linea Premium de Etiquetado",
    "description": "Maquina etiquetadora avanzada con sensores inteligentes"
  }'
```

**Respuesta:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "code": "LINEA-ETIQUETA-PREMIUM",
  "name": "Linea Premium de Etiquetado",
  "status": "Active"
}
```

Guardas: `machineCode = LINEA-ETIQUETA-PREMIUM`

---

## 🔧 PASO 1: Definir Parámetros (Qué mide la máquina)

### 1.1 Parámetro: Temperatura

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/parameters \
  -H "Content-Type: application/json" \
  -d '{
    "code": "TEMP_MOTOR",
    "name": "Temperatura Motor",
    "unit": "Celsius",
    "dataType": "decimal",
    "isRequired": true,
    "minValue": 0,
    "maxValue": 150,
    "isCalculated": false
  }'
```

**Respuesta:**
```json
{
  "id": "660f9511-f30c-52e5-b827-557766551111",
  "code": "TEMP_MOTOR",
  "name": "Temperatura Motor",
  "unit": "Celsius",
  "dataType": "decimal",
  "isRequired": true,
  "minValue": 0,
  "maxValue": 150,
  "isCalculated": false
}
```

### 1.2 Parámetro: Presión Hidráulica

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/parameters \
  -H "Content-Type: application/json" \
  -d '{
    "code": "PRESSURE_HYDRAULIC",
    "name": "Presion Hidraulica",
    "unit": "bar",
    "dataType": "decimal",
    "isRequired": true,
    "minValue": 20,
    "maxValue": 250,
    "isCalculated": false
  }'
```

### 1.3 Parámetro: Velocidad de Línea

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/parameters \
  -H "Content-Type: application/json" \
  -d '{
    "code": "SPEED_LINE",
    "name": "Velocidad de Linea",
    "unit": "m/min",
    "dataType": "decimal",
    "isRequired": true,
    "minValue": 0,
    "maxValue": 150,
    "isCalculated": false
  }'
```

### 1.4 Parámetro Calculado: Índice de Salud

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/parameters \
  -H "Content-Type: application/json" \
  -d '{
    "code": "HEALTH_INDEX",
    "name": "Indice de Salud de Maquina",
    "unit": "percentage",
    "dataType": "decimal",
    "isRequired": false,
    "isCalculated": true
  }'
```

---

## 📡 PASO 2: Mapear GPIOs a Parámetros

Conecta sensores reales a los parámetros definidos.

### 2.1 GPIO 25 → Temperatura Motor (con transformación)

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/gpio-mappings \
  -H "Content-Type: application/json" \
  -d '{
    "gpioTopic": "linea-premium/gpio/25",
    "parameterCode": "TEMP_MOTOR",
    "transformExpression": "value / 10",
    "isEnabled": true
  }'
```

**Explicación:**
- GPIO 25 envía valores 0-1500 (0-150°C)
- Transformamos: `value / 10` = temperatura real en Celsius
- Ejemplo: GPIO envía 850 → 850 / 10 = 85°C

### 2.2 GPIO 26 → Presión Hidráulica

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/gpio-mappings \
  -H "Content-Type: application/json" \
  -d '{
    "gpioTopic": "linea-premium/gpio/26",
    "parameterCode": "PRESSURE_HYDRAULIC",
    "transformExpression": "value * 0.5",
    "isEnabled": true
  }'
```

**Explicación:**
- GPIO 26 envía 0-500
- Transformamos: `value * 0.5` = bar
- Ejemplo: GPIO envía 400 → 400 * 0.5 = 200 bar

### 2.3 GPIO 27 → Velocidad de Línea

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/gpio-mappings \
  -H "Content-Type: application/json" \
  -d '{
    "gpioTopic": "linea-premium/gpio/27",
    "parameterCode": "SPEED_LINE",
    "transformExpression": "value",
    "isEnabled": true
  }'
```

### 2.4 GPIO 23 → Contador (mapping simple)

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/gpio-mappings \
  -H "Content-Type: application/json" \
  -d '{
    "gpioTopic": "linea-premium/gpio/23",
    "parameterCode": "UNIT_COUNT",
    "transformExpression": "value",
    "isEnabled": true
  }'
```

---

## 🧮 PASO 3: Definir Cálculos Personalizados (Métricas)

### 3.1 Métrica: Índice de Salud Personalizado

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/calculated-metrics \
  -H "Content-Type: application/json" \
  -d '{
    "code": "HEALTH_INDEX",
    "name": "Indice de Salud de Maquina",
    "unit": "percentage",
    "formulaExpression": "((150 - TEMP_MOTOR) / 150 * 100 + (PRESSURE_HYDRAULIC / 250 * 100) + (SPEED_LINE / 150 * 100)) / 3",
    "isEnabled": true
  }'
```

**Explicación de la fórmula:**
```
Health Index = promedio de 3 métricas normalizadas:

1. Temperatura: (150 - TEMP) / 150 * 100
   - Máximo 150°C = 0% de salud
   - Mínimo 0°C = 100% de salud
   
2. Presión: PRESSURE / 250 * 100
   - Rango 0-250 bar normalizado a 0-100%
   
3. Velocidad: SPEED / 150 * 100
   - Rango 0-150 m/min normalizado a 0-100%

HEALTH = (component1 + component2 + component3) / 3
```

### 3.2 Métrica: OEE Personalizado (con fórmula compleja)

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/calculated-metrics \
  -H "Content-Type: application/json" \
  -d '{
    "code": "OEE_CUSTOM",
    "name": "OEE Personalizado (Etiquetadora)",
    "unit": "percentage",
    "formulaExpression": "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000 * (HEALTH_INDEX / 100)",
    "isEnabled": true
  }'
```

**Explicación:**
```
OEE Personalizado = OEE Estándar × Factor de Salud

Si HEALTH_INDEX = 80%:
OEE_CUSTOM = OEE × 0.80

Esto reduce el OEE si la máquina no está 100% saludable
```

### 3.3 Métrica: Eficiencia Energética

```bash
curl -X POST http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config/calculated-metrics \
  -H "Content-Type: application/json" \
  -d '{
    "code": "ENERGY_EFFICIENCY",
    "name": "Eficiencia Energetica",
    "unit": "percentage",
    "formulaExpression": "100 - ((TEMP_MOTOR / 150) * 30 + (PRESSURE_HYDRAULIC / 250) * 40 + ((150 - SPEED_LINE) / 150) * 30)",
    "isEnabled": true
  }'
```

**Explicación:**
```
Eficiencia = 100% - (consumo ponderado)

- Temperatura alta = mayor consumo (30%)
- Presión alta = mayor consumo (40%)
- Velocidad baja = menor consumo (30%)

Si máquina está fría, baja presión, alta velocidad = 100% eficiencia
```

---

## 📊 PASO 4: Obtener Configuración Completa

```bash
curl http://localhost:5021/api/machines/LINEA-ETIQUETA-PREMIUM/config
```

**Respuesta:**
```json
{
  "machineCode": "LINEA-ETIQUETA-PREMIUM",
  "machineName": "Linea Premium de Etiquetado",
  "parameters": [
    {
      "id": "660f9511-...",
      "code": "TEMP_MOTOR",
      "name": "Temperatura Motor",
      "unit": "Celsius",
      "dataType": "decimal",
      "isRequired": true,
      "minValue": 0,
      "maxValue": 150,
      "isCalculated": false
    },
    {
      "id": "770g0622-...",
      "code": "PRESSURE_HYDRAULIC",
      "name": "Presion Hidraulica",
      "unit": "bar",
      "dataType": "decimal",
      "isRequired": true,
      "minValue": 20,
      "maxValue": 250,
      "isCalculated": false
    },
    {
      "id": "880h1733-...",
      "code": "SPEED_LINE",
      "name": "Velocidad de Linea",
      "unit": "m/min",
      "dataType": "decimal",
      "isRequired": true,
      "minValue": 0,
      "maxValue": 150,
      "isCalculated": false
    },
    {
      "id": "990i2844-...",
      "code": "HEALTH_INDEX",
      "name": "Indice de Salud de Maquina",
      "unit": "percentage",
      "dataType": "decimal",
      "isRequired": false,
      "isCalculated": true
    }
  ],
  "gpioMappings": [
    {
      "id": "110j3955-...",
      "gpioTopic": "linea-premium/gpio/25",
      "parameterCode": "TEMP_MOTOR",
      "transformExpression": "value / 10",
      "isEnabled": true
    },
    {
      "id": "220k4066-...",
      "gpioTopic": "linea-premium/gpio/26",
      "parameterCode": "PRESSURE_HYDRAULIC",
      "transformExpression": "value * 0.5",
      "isEnabled": true
    },
    {
      "id": "330l5177-...",
      "gpioTopic": "linea-premium/gpio/27",
      "parameterCode": "SPEED_LINE",
      "transformExpression": "value",
      "isEnabled": true
    },
    {
      "id": "440m6288-...",
      "gpioTopic": "linea-premium/gpio/23",
      "parameterCode": "UNIT_COUNT",
      "transformExpression": "value",
      "isEnabled": true
    }
  ],
  "calculatedMetrics": [
    {
      "id": "550n7399-...",
      "code": "HEALTH_INDEX",
      "name": "Indice de Salud de Maquina",
      "unit": "percentage",
      "formulaExpression": "((150 - TEMP_MOTOR) / 150 * 100 + (PRESSURE_HYDRAULIC / 250 * 100) + (SPEED_LINE / 150 * 100)) / 3",
      "isEnabled": true
    },
    {
      "id": "660o8400-...",
      "code": "OEE_CUSTOM",
      "name": "OEE Personalizado (Etiquetadora)",
      "unit": "percentage",
      "formulaExpression": "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000 * (HEALTH_INDEX / 100)",
      "isEnabled": true
    },
    {
      "id": "770p9511-...",
      "code": "ENERGY_EFFICIENCY",
      "name": "Eficiencia Energetica",
      "unit": "percentage",
      "formulaExpression": "100 - ((TEMP_MOTOR / 150) * 30 + (PRESSURE_HYDRAULIC / 250) * 40 + ((150 - SPEED_LINE) / 150) * 30)",
      "isEnabled": true
    }
  ]
}
```

---

## 🔄 PASO 5: Operar con la Máquina Configurada

### Crear orden de producción

```bash
curl -X POST http://localhost:5021/api/production-orders \
  -H "Content-Type: application/json" \
  -d '{
    "machineCode": "LINEA-ETIQUETA-PREMIUM",
    "quantity": 10000,
    "unitsPerBox": 50,
    "estimatedMinutes": 180
  }'
```

### Simular eventos GPIO

```bash
# Temperatura 85°C → envía 850 (transformación inversa: 85 * 10)
mosquitto_pub -h localhost -t "linea-premium/gpio/25" -m "850"

# Presión 200 bar → envía 400 (transformación inversa: 200 / 0.5)
mosquitto_pub -h localhost -t "linea-premium/gpio/26" -m "400"

# Velocidad 120 m/min → envía 120
mosquitto_pub -h localhost -t "linea-premium/gpio/27" -m "120"

# Contador: 5000 unidades
mosquitto_pub -h localhost -t "linea-premium/gpio/23" -m "5000"
```

### Obtener métricas calculadas

```bash
curl http://localhost:5021/api/metrics/{order-id}
```

**Respuesta con cálculos personalizados:**
```json
{
  "availability": 95.5,
  "performance": 92.3,
  "quality": 99.0,
  "oee": 86.8,
  "customMetrics": {
    "healthIndex": 82.5,
    "oeeCustom": 71.5,
    "energyEfficiency": 78.9
  },
  "rawParameters": {
    "tempMotor": 85,
    "pressureHydraulic": 200,
    "speedLine": 120,
    "unitCount": 5000
  }
}
```

---

## 📚 Resumen de la Máquina Configurada

```
MAQUINA: LINEA-ETIQUETA-PREMIUM
├─ PARAMETROS (4):
│  ├─ TEMP_MOTOR (0-150°C)
│  ├─ PRESSURE_HYDRAULIC (20-250 bar)
│  ├─ SPEED_LINE (0-150 m/min)
│  └─ HEALTH_INDEX (calculado)
│
├─ GPIO MAPPINGS (4):
│  ├─ GPIO 25 → TEMP_MOTOR (÷10)
│  ├─ GPIO 26 → PRESSURE (×0.5)
│  ├─ GPIO 27 → SPEED (×1)
│  └─ GPIO 23 → COUNTER (×1)
│
└─ CALCULOS PERSONALIZADOS (3):
   ├─ HEALTH_INDEX: promedio salud
   ├─ OEE_CUSTOM: OEE × salud
   └─ ENERGY_EFFICIENCY: consumo
```

---

## 🎯 Ventajas del Sistema Personalizado

✅ **Flexible**: Define tus propios parámetros
✅ **Escalable**: Múltiples máquinas = múltiples configuraciones
✅ **Inteligente**: Fórmulas personalizadas por máquina
✅ **Integrado**: GPIO directo desde sensores
✅ **Histórico**: Todos los datos en BD

---

**¡Así es como se crea una máquina REAL con configuración profesional!**
