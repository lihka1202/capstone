import paho.mqtt.client as mqtt
import ssl

broker = "armadillo.rmq.cloudamqp.com"
port = 8883  # Using secure TLS port
MQTT_USER = "swnviqwj:swnviqwj"  # ✅ Remove the colon from the username
MQTT_PW = "ZOCHf82N8XcNHID-oGHtJUe8gkfxFfqN"

def on_message(client, userdata, message):
    print(f"📩 Received: {message.payload.decode()} on topic {message.topic}")

def on_subscribe(client, userdata, mid, granted_qos):
    print(f"✅ Subscribed with MID {mid}, QoS {granted_qos}")

def on_log(client, userdata, level, buf):
    print(f"LOG: {buf}")

# ✅ Change to MQTTv3.1.1 for RabbitMQ compatibility
client = mqtt.Client(client_id="Listener1", protocol=mqtt.MQTTv311)
client.tls_set(tls_version=ssl.PROTOCOL_TLSv1_2)  # ✅ Enforce TLS version
client.username_pw_set(username=MQTT_USER, password=MQTT_PW)
client.on_message = on_message
client.on_subscribe = on_subscribe
client.on_log = on_log

try:
    print("📡 Connecting to MQTT Broker...")
    client.connect(broker, port, 60)
    print("✅ Connected Successfully")

    # ✅ Use basic subscription format
    client.subscribe("topic/p2-visibility", qos=0)
    print("📩 Subscribed to: topic/p2-visibility")

#     # ✅ Publish a test message
#     client.publish("topic/visibility", "True")
#     print("📤 Sent MQTT Message: True")

    client.loop_forever()

except Exception as e:
    print(f"❌ Connection Failed: {e}")
