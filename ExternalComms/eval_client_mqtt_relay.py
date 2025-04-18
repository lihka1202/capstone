import time
from datetime import datetime, timedelta
import json
import os
import queue

import numpy as np
from pynq_driver import get_result
import enc_for as ef
from create_socket import create_socket as cs
from parse_gamestate import vis_payload
from mqtt_fxn import *
# from game_engine import *
import game_engine as ge
import game_engine_freeplay as gef

message_queue = queue.Queue()
p1_buffer = []
p2_buffer = []
p1_last_clear = datetime.now()
p2_last_clear = datetime.now()
rel_cli_thres = timedelta(seconds=2)
p1_rej, p2_rej = 0, 0
p1_count, p2_count = 0, 0
PACKET_COUNT = 12

def rel_cli_p1(c, u, m):
    global p1_last_clear
    global p1_rej
    global p1_count
    raw_json = m.payload.decode()
    raw_json = raw_json.replace("'", '"')
    payload = json.loads(raw_json)
    data_type = payload.get("data_type")
    p_id = payload.get("player_num")
    match data_type:
        case "GUN":
            print("P1: Gun")
            vis_pub.publish(topic=PUB_TOPIC_P1, payload="gun,1")
        case "IMU":
            p1_count += 1
            # print(f"P1: {p1_count} packet(s) received")
            if datetime.now() - p1_last_clear < rel_cli_thres:
                p1_rej += 1
                # print(f"P1: Rejected {p1_rej} packet(s)")
            else:
                if p1_rej % PACKET_COUNT == 0:
                    for val in payload.get("data_value"):
                        p1_buffer.append(val)
                    if len(p1_buffer) == 60:
                        move, confidence_score = get_result(np.reshape(p1_buffer, (10, 6)))
                        p1_buffer.clear()
                        # print("P1: buffer cleared on prediction")
                        p1_last_clear = datetime.now()
                        action = [
                            "badminton",
                            "boxing",
                            "fencing",
                            "golf",
                            "logout",
                            "reload",
                            "shield",
                            "snowbomb",
                        ][move]
                        print(p_id, ":", action + ",", confidence_score)
                        vis_pub.publish(
                            topic=PUB_TOPIC_P1,
                            payload=f"{action},{str(confidence_score)}"
                        )
                        p1_rej = 10-PACKET_COUNT
                else:
                    p1_rej += 1
                    # print(f"P1: Rejected {p1_rej} packet(s)")
        case "VEST":
            pass


def rel_cli_p2(c, u, m):
    global p2_last_clear
    global p2_rej
    raw_json = m.payload.decode()
    raw_json = raw_json.replace("'", '"')
    payload = json.loads(raw_json)
    data_type = payload.get("data_type")
    p_id = payload.get("player_num")
    match data_type:
        case "GUN":
            print("P2: Gun")
            vis_pub.publish(topic=PUB_TOPIC_P2, payload="gun,1")
        case "IMU":
            if datetime.now() - p2_last_clear < rel_cli_thres:
                p2_rej += 1
                # print(f"P2: Rejected {p2_rej} packet(s)")
            else:
                if p2_rej % PACKET_COUNT  == 0:
                    for val in payload.get("data_value"):
                        p2_buffer.append(val)
                    if len(p2_buffer) == 60:
                        move, confidence_score = get_result(np.reshape(p2_buffer, (10, 6)))
                        p2_buffer.clear()
                        # print("P2: buffer cleared on prediction")
                        p2_last_clear = datetime.now()
                        action = [
                            "badminton",
                            "boxing",
                            "fencing",
                            "golf",
                            "logout",
                            "reload",
                            "shield",
                            "snowbomb",
                        ][move]
                        print(p_id, ":", action + ",", confidence_score)
                        vis_pub.publish(
                            topic=PUB_TOPIC_P2,
                            payload=f"{action},{str(confidence_score)}"
                        )
                        p2_rej = 10 - PACKET_COUNT
                else:
                    p2_rej += 1
                    # print(f"P2: Rejected {p2_rej} packet(s)")
        case "VEST":
            pass


def vis_cli_p1(c, u, m):
    msg = m.payload.decode()
    print("From Vis 1: ", msg)

    p_id, action, vis, sb_count = map(str.strip, msg.split(","))
    print(int(p_id), action, bool(vis), int(sb_count))
    status = True if vis[0]=='T' else False

    ## Add to the game engine queue
    engine.add_to_queue(int(p_id), action, status, int(sb_count))


def vis_cli_p2(c, u, m):
    msg = m.payload.decode()
    print("From Vis 2: ", msg)

    p_id, action, vis, sb_count = map(str.strip, msg.split(","))
    print(int(p_id), action, bool(vis), int(sb_count))
    status = True if vis[0]=='T' else False

    ## Add to the game engine queue
    engine.add_to_queue(int(p_id), action, status, int(sb_count))


# Take input to use relevant game_engine
status = input("Run with eval server?: ")
# Initialize game engine
if ("t" in status):
    # Run with eval
    print("Running WITH eval server")
    engine = ge.GameEngine()
else:
    print("Running WITHOUT eval server")
    engine = gef.GameEngine()

# Publish to Viz
vis_pub = new_mqtt_client(client_id="Ultra96_pub", user=PUB_USER, password=PUB_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
print("Visualizer publisher set")

# Broadcaster to viz 1
# vis_broadcast_p1 = new_mqtt_client(client_id="Ultra96_broadcast_p1", user=PUB_USER, password=PUB_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
# print("Visualizer broadcaster 1 set")

# Broadcaster to viz 2
# vis_broadcast_p2 = new_mqtt_client(client_id="Ultra96_broadcast_p2", user=PUB_USER, password=PUB_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
# print("Visualizer broadcaster 2 set")

# Subscribe to Viz 1
vis_cli_pone = new_mqtt_client(client_id="Ultra96_cli_one", user=CLI_USER, password=CLI_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
vis_cli_pone.on_message = vis_cli_p1
vis_cli_pone.subscribe(topic=CLI_TOPIC_P1, options=mqtt.SubscribeOptions(qos=0, noLocal=False))
vis_cli_pone.loop_start()
print("Subscriber to viz1 set")

# Subscribe to Viz 2
vis_cli_ptwo = new_mqtt_client(client_id="Ultra96_cli_two", user=CLI_USER, password=CLI_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
vis_cli_ptwo.on_message = vis_cli_p2
vis_cli_ptwo.subscribe(topic=CLI_TOPIC_P2, options=mqtt.SubscribeOptions(qos=0, noLocal=False))
vis_cli_ptwo.loop_start()
print("Subscriber to viz2 set")

# Subscribe to Relay For player 1
p1_rel_cli = new_mqtt_client(client_id="Relay_cli_p1", user=RELAY_USER, password=RELAY_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
p1_rel_cli.on_message = rel_cli_p1
p1_rel_cli.subscribe(topic=P1_RELAY_TOPIC, options=mqtt.SubscribeOptions(qos=0, noLocal=False))
p1_rel_cli.loop_start()
print("Subscriber for player ONE to relay node set")

# Subscribe to Relay for player 2
p2_rel_cli = new_mqtt_client(client_id="Relay_cli_p2", user=RELAY_USER, password=RELAY_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
p2_rel_cli.on_message = rel_cli_p2
p2_rel_cli.subscribe(topic=P2_RELAY_TOPIC, options=mqtt.SubscribeOptions(qos=0, noLocal=False))
p2_rel_cli.loop_start()
print("Subscriber for player TWO to the relay node set")

# Publish to Relay
rel_pub = new_mqtt_client(client_id="Relay_pub", user=RELAY_USER, password=RELAY_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
print("relay publisher is set")

print("MQTT connections done")

while True:
    pass
