# Motor de Evaluación de Fórmulas

Sistema para definir y evaluar fórmulas matemáticas personalizadas por máquina.

## 🧮 Capacidades

### Operadores Soportados
- Aritméticos: `+`, `-`, `*`, `/`, `%`
- Paréntesis: `(`, `)`
- Comparaciones: `>`, `<`, `>=`, `<=`, `=`, `<>`

### Funciones Built-in
- `IIF(condition, true_value, false_value)` - Condicional
- `ABS(value)` - Valor absoluto
- `MIN(a, b)` - Mínimo
- `MAX(a, b)` - Máximo

### Variables
- Case-insensitive: `TEMP_MOTOR` = `temp_motor`
- Convención: MAYÚSCULAS_CON_GUIONES_BAJOS
- Resolución encadenada: una métrica calculada puede usar el resultado de otra

---

## 📐 Ejemplos de Fórmulas

### OEE Estándar
```
(AVAILABILITY * PERFORMANCE * QUALITY) / 10000
```

### Índice de Salud Compuesto
```
((150 - TEMP_MOTOR) / 150 * 100 + PRESSURE_HYDRAULIC / 250 * 100 + SPEED_LINE / 150 * 100) / 3
```

### OEE Personalizado (con factor de salud)
```
(AVAILABILITY * PERFORMANCE * QUALITY) / 10000 * (HEALTH_INDEX / 100)
```

### Eficiencia Energética
```
100 - ((TEMP_MOTOR / 150) * 30 + (PRESSURE_HYDRAULIC / 250) * 40 + ((150 - SPEED_LINE) / 150) * 30)
```

### Condicional con IIF
```
IIF(TEMP_MOTOR > 100, 50, 100)
```

---

## 🚀 Endpoint: Evaluar Métricas

**POST** `/api/machines/{machineCode}/config/evaluate-metrics`

### Request
```json
{
  "AVAILABILITY": 95,
  "PERFORMANCE": 90,
  "QUALITY": 99,
  "TEMP_MOTOR": 75,
  "PRESSURE_HYDRAULIC": 200,
  "SPEED_LINE": 120
}
```

### Response
```json
{
  "machineCode": "LINEA-PREMIUM",
  "inputValues": {
    "AVAILABILITY": 95,
    "PERFORMANCE": 90,
    "QUALITY": 99,
    "TEMP_MOTOR": 75,
    "PRESSURE_HYDRAULIC": 200,
    "SPEED_LINE": 120
  },
  "metrics": [
    {
      "code": "HEALTH_INDEX",
      "name": "Indice de Salud",
      "unit": "percentage",
      "value": 70.0,
      "formula": "((150 - TEMP_MOTOR) / 150 * 100 + ...) / 3",
      "success": true,
      "error": null
    },
    {
      "code": "OEE_CUSTOM",
      "name": "OEE Personalizado",
      "unit": "percentage",
      "value": 59.25,
      "formula": "(AVAILABILITY * PERFORMANCE * QUALITY) / 10000 * (HEALTH_INDEX / 100)",
      "success": true,
      "error": null
    }
  ]
}
```

**Nota**: El sistema resuelve dependencias automáticamente.
`OEE_CUSTOM` usa `HEALTH_INDEX` que se calcula primero.

---

## 🔄 Resolución de Dependencias

El motor resuelve métricas en orden iterativo:

```
Iteración 1: Métricas que solo dependen de inputs
  ├─ HEALTH_INDEX (usa: TEMP_MOTOR, PRESSURE, SPEED)
  └─ ENERGY_EFFICIENCY (usa: TEMP_MOTOR, PRESSURE, SPEED)

Iteración 2: Métricas que dependen de las anteriores
  └─ OEE_CUSTOM (usa: AVAILABILITY, PERFORMANCE, QUALITY, HEALTH_INDEX)
```

Si una métrica tiene dependencias circulares o variables faltantes, se reporta como error pero el resto continúa.

---

## 🧪 Tests

Tests unitarios en `MachineMonitoring.Application.Tests/Services/FormulaEvaluatorTests.cs`:

```
✓ Evaluate_SimpleArithmetic_ReturnsCorrectResult
✓ Evaluate_OEEFormula_CalculatesCorrectly
✓ Evaluate_HealthIndexFormula_CalculatesCorrectly
✓ Evaluate_MissingVariable_ThrowsException
✓ Evaluate_DivisionByZero_ThrowsException
✓ ExtractVariables_ReturnsAllVariables
✓ TryValidate_ValidFormula_ReturnsTrue
✓ TryValidate_InvalidFormula_ReturnsFalseWithError
✓ Evaluate_CaseInsensitiveVariables
```

**Ejecutar**:
```bash
dotnet test MachineMonitoring.Application.Tests --filter "FullyQualifiedName~FormulaEvaluatorTests"
```

---

## 📊 Flujo Completo

```
1. Definir parámetros (POST /api/machines/{code}/config/parameters)
   └─ Ej: TEMP_MOTOR (0-150°C), PRESSURE (20-250 bar)

2. Mapear GPIOs (POST /api/machines/{code}/config/gpio-mappings)
   └─ Ej: GPIO 25 → TEMP_MOTOR (transform: "value / 10")

3. Definir métricas (POST /api/machines/{code}/config/calculated-metrics)
   └─ Ej: HEALTH_INDEX = "((150 - TEMP) / 150 * 100 + ...) / 3"

4. Evaluar métricas (POST /api/machines/{code}/config/evaluate-metrics)
   └─ Envía valores actuales → recibe métricas calculadas
```

---

## ⚠️ Limitaciones Actuales

1. **DataTable.Compute** se usa internamente (limita algunas funciones)
2. **No soporta funciones matemáticas avanzadas** (sin, cos, log, exp)
3. **Sin variables de tiempo** (no se puede usar NOW(), TODAY())

### Mejoras Futuras
- Migrar a NCalc para fórmulas más complejas
- Agregar funciones trigonométricas
- Soporte para timestamps
- Aggregations (SUM, AVG sobre rangos de tiempo)
