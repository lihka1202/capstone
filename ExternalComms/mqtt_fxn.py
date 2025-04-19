import ssl
import paho.mqtt.client as mqtt


# MQTT
MQTT_BROKER = "armadillo.rmq.cloudamqp.com"
MQTT_PORT = 8883

PUB_TOPIC = "topic/lasertag"
PUB_TOPIC_P1 = "topic/p1-lasertag"
PUB_TOPIC_P2 = "topic/p2-lasertag"
# PUB_USER = "user1"
# PUB_PW = "Password1"
PUB_USER = "jzzkaznp:jzzkaznp"
PUB_PW = "qsM-cpATwPPXkferAG2MjPEeMgk1tvZC"

CLI_USER = "swnviqwj:swnviqwj"
CLI_PW = "ZOCHf82N8XcNHID-oGHtJUe8gkfxFfqN"
CLI_TOPIC = "topic/visibility"
CLI_TOPIC_P1 = "topic/p1-visibility"
CLI_TOPIC_P2 = "topic/p2-visibility"

RELAY_TOPIC = "topic/data"
# RELAY_USER = "user1"
# RELAY_PW = "Password1"
RELAY_USER = "brfwagzs:brfwagzs"
RELAY_PW = "Y6-QZRIw0YiLqUOLBPfGgPYLvB_YQJBv"

## Added by akhil
P1_RELAY_TOPIC = "topic/p1-data"
P2_RELAY_TOPIC = "topic/p2-data"
BCAST_TOPIC_P1 = "topic/p1-broadcast"
BCAST_TOPIC_P2 = "topic/p2-broadcast"

def new_mqtt_client(client_id, user, password, bkr, port):
    new_client = mqtt.Client(client_id=client_id, userdata=None, protocol=mqtt.MQTTv311)
    new_client.tls_set(tls_version=ssl.PROTOCOL_TLSv1_2)
    new_client.username_pw_set(username=user, password=password)
    new_client.connect(bkr, port, 300)
    return new_client


def on_message(client, userdata, message):
    print(f"Received message: '{message.payload.decode()}' on topic {message.topic}")


def on_subscribe(client, userdata, mid, granted_qos, _):
    print(f"Subscribed successfully with MID {mid}, QoS {granted_qos}")
