"""
Publica mensajes MQTT al broker para probar el sistema dinamico.
"""
import paho.mqtt.client as mqtt
import time

BROKER = "192.168.20.82"
PORT = 1883

# Use callback API v2 (paho 2.x)
client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2)
client.connect(BROKER, PORT, 60)
client.loop_start()

print(f"=== Publicando a {BROKER}:{PORT} ===\n")

# Test 1: GPIO 25 - Temperatura (transform: VALUE / 10)
print("[1] GPIO 25: raw=850 -> deberia ser TEMP_MOTOR=85 (850/10)")
client.publish("linea-premium/gpio/25", "850")
time.sleep(1)

# Test 2: GPIO 26 - Presion (transform: VALUE * 0.5)
print("[2] GPIO 26: raw=400 -> deberia ser PRESSURE=200 (400*0.5)")
client.publish("linea-premium/gpio/26", "400")
time.sleep(1)

# Test 3: GPIO 27 - Velocidad (transform: VALUE)
print("[3] GPIO 27: raw=120 -> deberia ser SPEED=120")
client.publish("linea-premium/gpio/27", "120")
time.sleep(1)

# Test 4: Diferentes valores
print("\n[4] Cambio de valores reales:")
print("    GPIO 25: raw=950 -> TEMP=95")
client.publish("linea-premium/gpio/25", "950")
time.sleep(0.5)

print("    GPIO 26: raw=300 -> PRESSURE=150")
client.publish("linea-premium/gpio/26", "300")
time.sleep(0.5)

print("    GPIO 27: raw=130 -> SPEED=130")
client.publish("linea-premium/gpio/27", "130")
time.sleep(2)

client.loop_stop()
client.disconnect()
print("\n=== Mensajes publicados. Revisa los logs de la API ===")
