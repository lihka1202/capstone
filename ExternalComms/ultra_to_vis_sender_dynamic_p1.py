import paho.mqtt.client as mqtt
import ssl

broker = "armadillo.rmq.cloudamqp.com"
port = 8883  # Secure TLS port
MQTT_USER = "jzzkaznp:jzzkaznp" 
MQTT_PW = "qsM-cpATwPPXkferAG2MjPEeMgk1tvZC"

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("✅ Connected Successfully to MQTT Broker")
    else:
        print(f"❌ Connection failed with code {rc}")

def on_message(client, userdata, message):
    print(f"📩 Received: {message.payload.decode()} on topic {message.topic}")

def on_subscribe(client, userdata, mid, granted_qos):
    print(f"✅ Subscribed with MID {mid}, QoS {granted_qos}")

def on_log(client, userdata, level, buf):
    print(f"LOG: {buf}")

# ✅ Initialize MQTT Client for RabbitMQ
client = mqtt.Client(client_id="Sender1", protocol=mqtt.MQTTv311)
client.tls_set(tls_version=ssl.PROTOCOL_TLSv1_2)  # ✅ Enforce TLS version
client.username_pw_set(username=MQTT_USER, password=MQTT_PW)
client.on_connect = on_connect
# client.on_message = on_message
client.on_subscribe = on_subscribe
client.on_log = on_log

try:
    print("📡 Connecting to MQTT Broker...")
    client.connect(broker, port, 60)
    client.loop_start()  # ✅ Start the MQTT loop in a separate thread

    client.subscribe("topic/p1-lasertag", qos=0)
    print("📩 Subscribed to: topic/p1-lasertag")

    while True:
        # ✅ Get user input from terminal
        message = input("📝 Enter message to send (or type 'exit' to quit): ")
        if message.lower() == "exit":
            break  # ✅ Exit the loop when 'exit' is typed

        # ✅ Publish message to MQTT topic
        client.publish("topic/p1-lasertag", message)
        print(f"📤 Sent MQTT Message: {message}")

except Exception as e:
    print(f"❌ Connection Failed: {e}")

finally:
    client.loop_stop()  # ✅ Stop MQTT loop
    client.disconnect()
    print("🚪 Disconnected from MQTT Broker.")
