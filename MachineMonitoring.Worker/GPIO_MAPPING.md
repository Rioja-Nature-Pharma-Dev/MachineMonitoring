# GPIO Mapping - Cremer Machine

Documentación de mapeo de pines GPIO y eventos MQTT para la máquina Cremer de empaque y producción.

## Hardware Configuration

### GPIO Pins

| GPIO | Descripción | Tipo | Sensor | Evento |
|------|------------|------|--------|--------|
| 23 | Contador de unidades | Input | Fotocélula | Falling edge |
| 22 | Error de peso | Input | Balanza | Rising edge |
| 19 | Error de etiqueta | Input | Sensor óptico | Rising edge |

## MQTT Topics & Payloads

### GPIO 23: Unit Count (cremer/gpio/23)
```
Topic: cremer/gpio/23
Payload: 1 (cuando se detecta pulso)
Frecuencia: Variable según velocidad de línea
Acción: Incrementar contador de unidades buenas
```

**Comportamiento:**
- La fotocélula genera un pulso cuando una unidad pasa
- Cada pulso incrementa el contador de la orden activa
- Se registra en ProductionCounter.Quantity
- Contribuye al cálculo de métricas OEE (Performance)

**Fórmula de Performance:**
```
Performance = (Unidades producidas / Unidades planificadas) × 100
```

---

### GPIO 22: Weight Error (cremer/gpio/22)
```
Topic: cremer/gpio/22
Payload: 1 (cuando hay error)
Payload: 0 (cuando se recupera)
Acción: Registrar alerta, pausar línea si es persistente
```

**Comportamiento:**
- Se activa cuando la balanza detecta peso incorrecto
- Indica que el empaque no contiene la cantidad correcta de producto
- Requiere intervención manual o ajuste automático

**Registros a crear (TBD):**
- Tabla de alertas: timestamp, máquina, tipo, gravedad
- Impacta en métrica de Calidad (Quality % en OEE)

---

### GPIO 19: Label Error (cremer/gpio/19)
```
Topic: cremer/gpio/19
Payload: 1 (cuando hay error)
Payload: 0 (cuando se recupera)
Acción: Solicitar intervención manual, pausar línea
```

**Comportamiento:**
- Se activa cuando el sensor óptico no detecta etiqueta
- Indica que la etiqueta no se aplicó correctamente
- Requiere que un operador verifique y corrija

**Registros a crear (TBD):**
- Tabla de defectos: timestamp, máquina, tipo, operador, acción
- Impacta en métrica de Calidad (Quality % en OEE)

---

## OEE Calculation Impact

Las métricas GPIO afectan directamente al cálculo de OEE (Overall Equipment Effectiveness):

### Availability (Disponibilidad)
```
Availability = (Tiempo activo / Tiempo total) × 100
```
Afectado por:
- Pausas registradas en ProductionPause
- Tiempo de paradas por error (GPIO 22/19)

### Performance (Rendimiento)
```
Performance = (Unidades reales / Unidades planificadas) × 100
```
Afectado por:
- Contador de unidades (GPIO 23)
- Tiempo estimado vs. tiempo real

### Quality (Calidad)
```
Quality = (Unidades buenas / Unidades totales) × 100
```
Afectado por:
- Errores de peso (GPIO 22)
- Errores de etiqueta (GPIO 19)
- Conteo de unidades malas

### OEE
```
OEE = Availability × Performance × Quality
```

---

## Use Case Scenarios

### Scenario 1: Normal Operation
```
1. Orden creada: CREMER-TEST-001 (1000 unidades)
2. Operador inicia orden
3. Máquina procesa:
   - GPIO 23 genera pulsos: contador incrementa
   - GPIO 22/19: Sin activaciones
4. Orden finaliza: 1000 unidades procesadas
5. OEE ≈ 100% (suponiendo tiempo correcto)
```

### Scenario 2: Peso Incorrecto
```
1. Línea procesa normal
2. GPIO 22 se activa: Peso incorrecto
3. Sistema registra alerta
4. Opciones:
   - Pausa automática para revisión
   - Operador ajusta pesos
   - Reanuda producción
5. Afecta Quality %
```

### Scenario 3: Error de Etiqueta
```
1. Línea procesa normal
2. GPIO 19 se activa: Etiqueta no detectada
3. Sistema solicita intervención
4. Operador:
   - Verifica etiquetadora
   - Repone etiquetas si es necesario
   - Reinicia línea
5. Unidades afectadas cuentan como rechazo
```

---

## Configuration Example

En `appsettings.json`:

```json
{
  "Mqtt": {
    "Broker": "192.168.1.100",
    "Port": 1883,
    "MachineCode": "CREMER",
    "Timeout": 30
  },
  "Gpio": {
    "Cremer": {
      "UnitCountPin": 23,
      "WeightErrorPin": 22,
      "LabelErrorPin": 19,
      "PollInterval": 100
    }
  }
}
```

---

## Implementation Roadmap

- ✅ GPIO 23: Contador de unidades
  - [x] MQTT listener implementado
  - [x] Mapeo a incremento de contador
  - [x] Integración con ProductionCounter

- ⏳ GPIO 22: Error de peso
  - [ ] Handler de error implementado
  - [ ] Tabla de alertas creada
  - [ ] Lógica de pausa automática
  - [ ] Notificación a operador

- ⏳ GPIO 19: Error de etiqueta
  - [ ] Handler de error implementado
  - [ ] Tabla de defectos creada
  - [ ] Solicitud de intervención manual
  - [ ] Auditoría de acciones

- ⏳ Métricas
  - [ ] Cálculo de Availability incluyendo pausas por GPIO
  - [ ] Cálculo de Quality excluyendo unidades defectuosas
  - [ ] Dashboard con alertas en tiempo real

---

## Testing GPIO Events

### Simular eventos con mosquitto

```bash
# GPIO 23: Contador
mosquitto_pub -h localhost -t cremer/gpio/23 -m "1"
mosquitto_pub -h localhost -t cremer/gpio/23 -m "1"

# GPIO 22: Error de peso
mosquitto_pub -h localhost -t cremer/gpio/22 -m "1"
mosquitto_pub -h localhost -t cremer/gpio/22 -m "0"  # Recuperado

# GPIO 19: Error de etiqueta
mosquitto_pub -h localhost -t cremer/gpio/19 -m "1"
mosquitto_pub -h localhost -t cremer/gpio/19 -m "0"  # Recuperado
```

---

## Future Extensions

### Múltiples máquinas
Extender para otras máquinas con GPIO configuration:
- Máquina B: GPIO 15, 14, 13
- Máquina C: GPIO 27, 17, 4

### Configuración dinámica
Permitir cambiar GPIO mapping sin recompilación:
- Tabla MachineGpioConfiguration en base de datos
- Cargar en startup

### Análisis avanzado
- Patrones de errores: frecuencia, horarios
- Predicción de mantenimiento
- Optimización de línea
