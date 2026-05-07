# Configuración MQTT con Sistema Dinámico

Guía para configurar MQTT y conectar sensores reales al sistema.

## 🔌 Endpoint de Ingesta

El sistema ahora tiene un endpoint dinámico que recibe datos de cualquier sensor:

**POST** `/api/machines/{machineCode}/config/sensor-data`

```json
{
  "gpioTopic": "linea-premium/gpio/25",
  "rawValue": 850
}
```

El sistema automáticamente:
1. Busca el mapping configurado para ese topic
2. Aplica la transformación (ej: `VALUE / 10`)
3. Devuelve el valor del parámetro

## 🐳 Setup Local con Mosquitto

### Opción 1: Docker (Recomendado)

```bash
# Iniciar broker MQTT
docker run -d --name mosquitto \
  -p 1883:1883 \
  -p 9001:9001 \
  eclipse-mosquitto

# Verificar que está corriendo
docker logs mosquitto
```

### Opción 2: Windows (sin Docker)

1. Descargar Mosquitto: https://mosquitto.org/download/
2. Instalar como servicio
3. Configurar `mosquitto.conf`:
   ```
   listener 1883
   allow_anonymous true
   ```
4. Iniciar servicio

## 🧪 Pruebas con CLI

### Instalar mosquitto_pub/sub

```bash
# Linux
sudo apt install mosquitto-clients

# Mac
brew install mosquitto

# Windows
choco install mosquitto
```

### Publicar mensaje de sensor

```bash
# GPIO 25: Temperatura raw 850 (= 85°C después de transform)
mosquitto_pub -h localhost -t "linea-premium/gpio/25" -m "850"

# GPIO 26: Presion raw 400 (= 200 bar después de transform)
mosquitto_pub -h localhost -t "linea-premium/gpio/26" -m "400"

# GPIO 27: Velocidad raw 120 (= 120 m/min sin transform)
mosquitto_pub -h localhost -t "linea-premium/gpio/27" -m "120"
```

### Suscribirse para ver mensajes

```bash
mosquitto_sub -h localhost -t "linea-premium/#" -v
```

## 🔄 Bridge MQTT → API

Para conectar MQTT al endpoint de ingesta, hay dos opciones:

### Opción A: Worker existente (MqttGpioListener)

Configurar `appsettings.json` del Worker:

```json
{
  "Mqtt": {
    "Broker": "localhost",
    "Port": 1883,
    "ClientId": "MachineMonitoring-Worker",
    "Topics": [
      "linea-premium/gpio/25",
      "linea-premium/gpio/26",
      "linea-premium/gpio/27"
    ]
  },
  "MachineCode": "LINEA-PREMIUM"
}
```

### Opción B: Bridge Externo (Node-RED, Python)

Ejemplo con Python:

```python
import paho.mqtt.client as mqtt
import requests

API_BASE = "http://localhost:5021/api/machines/LINEA-PREMIUM/config"

def on_message(client, userdata, msg):
    topic = msg.topic
    raw_value = float(msg.payload.decode())

    response = requests.post(
        f"{API_BASE}/sensor-data",
        json={
            "gpioTopic": topic,
            "rawValue": raw_value
        }
    )
    print(f"{topic} = {raw_value} -> {response.json()}")

client = mqtt.Client()
client.on_message = on_message
client.connect("localhost", 1883)
client.subscribe("linea-premium/#")
client.loop_forever()
```

## 📋 Flujo Completo End-to-End

```
1. Sensor PLC envía señal eléctrica
       ↓
2. PLC publica en MQTT: "linea-premium/gpio/25" = "850"
       ↓
3. Worker/Bridge recibe mensaje MQTT
       ↓
4. POST /api/machines/LINEA-PREMIUM/config/sensor-data
   {"gpioTopic": "linea-premium/gpio/25", "rawValue": 850}
       ↓
5. API busca mapping: GPIO 25 → TEMP_MOTOR (transform "VALUE / 10")
       ↓
6. Aplica transformación: 850 / 10 = 85
       ↓
7. Almacena lectura en BD
       ↓
8. UI/Dashboard consulta últimos valores
       ↓
9. POST /evaluate-metrics calcula métricas con valores actuales
       ↓
10. Dashboard muestra HEALTH_INDEX, OEE_CUSTOM, etc.
```

## ✅ Test Sin MQTT (Solo HTTP)

Si no quieres configurar MQTT todavía, puedes simular usando solo curl:

```bash
# Simular evento GPIO 25 (Temperatura)
curl -X POST http://localhost:5021/api/machines/LINEA-PREMIUM/config/sensor-data \
  -H "Content-Type: application/json" \
  -d '{"gpioTopic":"linea-premium/gpio/25","rawValue":850}'

# Respuesta:
# {
#   "machineCode": "LINEA-PREMIUM",
#   "gpioTopic": "linea-premium/gpio/25",
#   "parameterCode": "TEMP_MOTOR",
#   "rawValue": 850,
#   "transformedValue": 85,
#   "transformExpression": "VALUE / 10",
#   "timestamp": "..."
# }
```

## 📊 Consultar Estado en Tiempo Real

```bash
# Ver configuración completa
curl http://localhost:5021/api/machines/LINEA-PREMIUM/config

# Evaluar métricas con valores actuales
curl -X POST http://localhost:5021/api/machines/LINEA-PREMIUM/config/evaluate-metrics \
  -H "Content-Type: application/json" \
  -d '{"AVAILABILITY":95,"PERFORMANCE":90,"QUALITY":99,"TEMP_MOTOR":85,"PRESSURE":200,"SPEED":120}'
```

## 🚀 Próximas Mejoras

- [ ] Almacenar histórico de lecturas (MachineReadingNormalized)
- [ ] WebSocket para actualizaciones en tiempo real
- [ ] Auto-detectar topics MQTT y crear mappings
- [ ] Alertas cuando valores excedan min/max del parámetro
- [ ] Dashboard UI consumiendo el sistema
