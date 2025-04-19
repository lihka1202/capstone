#!/usr/bin/env python3
import os
import sys
import threading
import traceback
import struct
import time
import datetime
import queue
from queue import Queue
import csv
import paho.mqtt.client as mqtt
import ssl
import json
from crccheck.crc import Crc8
from bluepy.btle import DefaultDelegate, Peripheral, BTLEDisconnectError


# Global game configuration
# Placeholder values for health and ammo
health = 100
ammo = 6


# MQTT broker info
BROKER = "armadillo.rmq.cloudamqp.com"
PORT = 8883
TOPIC1 = "topic/p1-data"
TOPIC2 = "topic/p2-data"
USER = "brfwagzs:brfwagzs"
PW = "Y6-QZRIw0YiLqUOLBPfGgPYLvB_YQJBv"


# MAC addresses for all Beetles (Player 1 and Player 2)
ALL_BEETLES_INFO = [
 # Player 1
 {
     "mac": "34:08:E1:28:16:E0", 
     "device_type": "G",         # 'G' for Gun
     "player_num": 1
 },
 {
     "mac": "34:08:E1:28:2A:1F",
     "device_type": "V",         # 'V' for Vest
     "player_num": 1
 },
 {
     "mac": "34:08:E1:28:1B:0C",
     "device_type": "M",         # 'M' for IMU
     "player_num": 1
 },
 # Player 2
 {
     "mac": "34:08:E1:28:0D:79",
     "device_type": "G",
     "player_num": 2
 },
 {
     "mac": "34:08:E1:28:0D:07",
     "device_type": "V",
     "player_num": 2
 },
 {
     "mac": "34:08:E1:2A:1C:3B",
     "device_type": "M",
     "player_num": 2
 },
]


# UUIDs
SERVICE_UUID = "0000dfb0-0000-1000-8000-00805f9b34fb"
CHARACTERISTIC_UUID = "0000dfb1-0000-1000-8000-00805f9b34fb"


# MQTT client (for publishing sensor data)
relay = mqtt.Client(client_id="relay", userdata=None, protocol=mqtt.MQTTv311)
relay.tls_set(tls_version=ssl.PROTOCOL_TLSv1_2)
relay.username_pw_set(username=USER, password=PW)
relay.connect(BROKER, PORT, 300)
MQTT_message_received = False
MQTT_message_payload = None


# For sensor and game state data
sensor_data_queue_from_beetle = Queue()
sensor_data_queue_from_beetle1 = Queue()
sensor_data_queue_from_beetle2 = Queue()


U_to_gun = {1: False, 2: False}   # e.g. “Reload” for P1 or P2
U_to_vest = {1: False, 2: False}  # e.g. “Shield” for P1 or P2


global_shutdown = threading.Event()



# MQTT subscriber callbacks
def on_connect(client, userdata, flags, rc):
 print("Subscriber connected with result code:", rc)
 client.subscribe(TOPIC2)
 client.subscribe(TOPIC1)
def on_message(client, userdata, msg):
 global U_to_gun, U_to_vest
 global MQTT_message_received, MQTT_message_payload
 global health, ammo
 try:
     get_topic = msg.topic
     payload_str = msg.payload.decode("utf-8")
     parsed_data = json.loads(payload_str)
 except Exception:
     parsed_data = msg.payload.decode("utf-8")
     return
 if "game_state" in parsed_data:
     game_state = parsed_data["game_state"]
     info = game_state.get("p1", {})
     hp = info.get("hp", 0)
     bullets = info.get("bullets", 0)
     if get_topic == TOPIC1:
         U_to_gun[1] = True
         U_to_vest[1] = True
         health = hp
         ammo = bullets
         print({get_topic})
     if get_topic == TOPIC2:
         U_to_gun[2] = True
         U_to_vest[2] = True
         health = hp
         ammo = bullets
         print({get_topic})    
   # U_to_gun[1] = true means Player 1
   # U_to_gun[2] = true means Player 2
   # Same for vest
   # Required Logic: Get the player_number -> Parse "Reload" -> U_to_gun[player_number] = true
   # -> Parse "Health" -> U_to_vest[player_number] = true -> health = health_data


 # Data received from MQTT
 MQTT_message_payload = parsed_data
 MQTT_message_received = True


 # print("Received message from MQTT:", MQTT_message_payload)
def mqtt_subscriber():
 client = mqtt.Client(client_id="subscriber", protocol=mqtt.MQTTv311)
 client.tls_set(tls_version=ssl.PROTOCOL_TLSv1_2)
 client.username_pw_set(username=USER, password=PW)
 client.on_connect = on_connect
 client.on_message = on_message
 client.connect(BROKER, PORT, 300)
 client.loop_forever()
def mqtt_publish_loop():
 while not global_shutdown.is_set():
     if sensor_data_queue_from_beetle1.empty() and sensor_data_queue_from_beetle2.empty():
         # time.sleep(0.1)
         continue
     else:
         while not sensor_data_queue_from_beetle1.empty():
             payload = str(sensor_data_queue_from_beetle1.get())
             print("[P1] Publishing MQTT data ->", payload)
             relay.publish(topic=TOPIC1, payload=payload)
         while not sensor_data_queue_from_beetle2.empty():
             payload = str(sensor_data_queue_from_beetle2.get())
             print("[P2] Publishing MQTT data ->", payload)
             relay.publish(topic=TOPIC2, payload=payload)



# Dictionaries to track Beetle states, keyed by MAC
BTL_HANDSHAKE_STATUS     = {}
BTL_RESET_STATUS         = {}
ACK_HANDSHAKE_SENT_flag  = {}
ACK_IR_SENT_flag         = {}
sequence_number          = {}
prev_sequence_number     = {}
COUNT_GOOD_PKT           = {}
COUNT_FRAG_PKT           = {}
COUNT_DROPPED_PKT        = {}

for info in ALL_BEETLES_INFO:
 mac = info["mac"]
 BTL_HANDSHAKE_STATUS[mac]     = False
 BTL_RESET_STATUS[mac]         = False
 ACK_HANDSHAKE_SENT_flag[mac]  = False
 ACK_IR_SENT_flag[mac]         = False
 sequence_number[mac]          = 0
 prev_sequence_number[mac]     = 0
 COUNT_GOOD_PKT[mac]           = 0
 COUNT_FRAG_PKT[mac]           = 0
 COUNT_DROPPED_PKT[mac]        = 0


# NotificationDelegate: for handling incoming data from each Beetle
class NotificationDelegate(DefaultDelegate):
 def __init__(self, mac_address, device_type, player_num):
     super().__init__()
     self.macAddress = mac_address
     self.device_type = device_type
     self.player_num = player_num
     self.buffer = b''
     self.sequence_num = 0


 def handleNotification(self, cHandle, raw_packet):
     # Add data into buffer for fragmentation
     self.buffer += raw_packet
     if len(self.buffer) < 20:
         COUNT_FRAG_PKT[self.macAddress] += 1
     else:
         full_packet = self.buffer[:20]
         self.buffer = self.buffer[20:]
         self.manage_packet_data(full_packet)


 def crcCheck(self, full_packet):
     # Check crc for first 19 bytes
     # Last byte is the crc
     checksum = Crc8.calc(full_packet[0:19])
     return (checksum == full_packet[19])
 

 def manage_packet_data(self, full_packet):
     if not self.crcCheck(full_packet):
         COUNT_DROPPED_PKT[self.macAddress] += 1
         print("Dropping packet CRC check failed:", full_packet)
         return
     try:
         # ACK from Beetle
         if full_packet[0] == ord('A'):
             BTL_HANDSHAKE_STATUS[self.macAddress] = True
             ACK_HANDSHAKE_SENT_flag[self.macAddress] = True
             # to reset sequence number
             self.sequence_num = -1
             print(f"[{self.device_type}{self.player_num}] Handshake Complete")
         elif BTL_HANDSHAKE_STATUS[self.macAddress]:
             # If recieve gun, vest or motion data
             if full_packet[0] == ord('G') or full_packet[0] == ord('V'):
                 self.process_IR_data(full_packet)
             elif full_packet[0] == ord('M'):
                 self.process_motion_data(full_packet)
             else:
                 COUNT_DROPPED_PKT[self.macAddress] += 1
         else:
             COUNT_DROPPED_PKT[self.macAddress] += 1
     except Exception as e:
         print("manage_packet_data exception:", e)


 def process_IR_data(self, full_packet):
     try:
         packetFormat = '!c' + 19 * 'B'
         opened_packet = struct.unpack(packetFormat, full_packet)
         seq = opened_packet[1]
         if seq == self.sequence_num:
             # same sequence -> discard
             print(f"Dropped Packet due to same Sequence Number")
             COUNT_DROPPED_PKT[self.macAddress] += 1
             return
         self.sequence_num = seq
         sequence_number[self.macAddress] = self.sequence_num
         ACK_IR_SENT_flag[self.macAddress] = True
         COUNT_GOOD_PKT[self.macAddress] += 1
         if full_packet[0] == ord('G'):
             # Gun event
             gun_data_dict = {
                 'player_num': self.player_num,
                 'data_type': 'GUN',
                 'data_value': 1
             }
             if self.player_num == 1:
                 sensor_data_queue_from_beetle1.put(gun_data_dict)
             if self.player_num == 2:
                 sensor_data_queue_from_beetle2.put(gun_data_dict)
             print("Gun event:", gun_data_dict)
         elif full_packet[0] == ord('V'):
             # Vest event
             vest_data_dict = {
                 'player_num': self.player_num,
                 'data_type': 'VEST',
                 'data_value': 1
             }
             if self.player_num == 1:
                 sensor_data_queue_from_beetle1.put(vest_data_dict)
             if self.player_num == 2:
                 sensor_data_queue_from_beetle2.put(vest_data_dict)
             print("Vest event:", vest_data_dict)
     except Exception as e:
         print("process_IR_data exception:", e)


 def process_motion_data(self, full_packet):
     try:
         packetFormat = '!c' + (6 * 'h') + (7 * 'b')
         opened_packet = struct.unpack(packetFormat, full_packet)\
         # to get back original values, /100
         acc_x = float(opened_packet[1] / 100)
         acc_y = float(opened_packet[2] / 100)
         acc_z = float(opened_packet[3] / 100)
         gyro_x = float(opened_packet[4] / 100)
         gyro_y = float(opened_packet[5] / 100)
         gyro_z = float(opened_packet[6] / 100)
         COUNT_GOOD_PKT[self.macAddress] += 1
         # Push motion data
         motion_data_dict = {
             'player_num': self.player_num,
             'data_type': 'IMU',
             'data_value': [acc_x, acc_y, acc_z, gyro_x, gyro_y, gyro_z]
         }
         if self.player_num == 1:
              sensor_data_queue_from_beetle1.put(motion_data_dict)
         if self.player_num == 2:
              sensor_data_queue_from_beetle2.put(motion_data_dict)
         # print("Motion:", motion_data_dict)
     except Exception as e:
         pass
     


# Threads for Beetles
class BeetleThread(threading.Thread):
 def __init__(self, mac_address, device_type, player_num):
     super().__init__()
     self.mac_address = mac_address
     self.device_type = device_type
     self.player_num = player_num
     self.peripheral_obj_beetle = None
     self.serial_service = None
     self.serial_chars = None


 def connect(self):
     mac = self.mac_address
     print(f"[{self.device_type}{self.player_num}] Attempting connection to {mac}...")
     try:
         self.peripheral_obj_beetle = Peripheral(mac)
         delegate = NotificationDelegate(mac, self.device_type, self.player_num)
         self.peripheral_obj_beetle.withDelegate(delegate)
         # print(f"[{self.device_type}{self.player_num}] Connected successfully -> {mac}")
         self.serial_service = self.peripheral_obj_beetle.getServiceByUUID(SERVICE_UUID)
         self.serial_chars = self.serial_service.getCharacteristics()[0]
         return True  # signal success
     except BTLEDisconnectError as e:
         print(f"[{self.device_type}{self.player_num}] BTLEDisconnectError -> {e}")
         self.peripheral_obj_beetle = None
         BTL_HANDSHAKE_STATUS[self.mac_address] = False
         self.disconnect()
         return False
     except Exception as e:
         print(f"[{self.device_type}{self.player_num}] Connection failed -> {e}")
         self.peripheral_obj_beetle = None
         BTL_HANDSHAKE_STATUS[self.mac_address] = False
         self.disconnect()
         return False
     

 def reset(self):
     mac = self.mac_address
     padding = (0,) * 19
     packetFormat = 'c' + 19 * 'B'
     resetByte = bytes('R', 'utf-8')
     packet = struct.pack(packetFormat, resetByte, *padding)
     if self.serial_chars is not None:
         self.serial_chars.write(packet, withResponse=False)
     BTL_RESET_STATUS[mac] = False
     self.disconnect()

 def disconnect(self):
     if self.peripheral_obj_beetle:
         try:
             self.peripheral_obj_beetle.disconnect()
         except:
             pass
     self.peripheral_obj_beetle = None
     self.serial_chars = None
     self.serial_service = None
     BTL_HANDSHAKE_STATUS[self.mac_address] = False


 def initiate_handshake(self):
     mac = self.mac_address
     timeout_count = 0
     packetFormat = 'c' + 19 * 'B'
     padding = (0,) * 19
     try:
         while not BTL_HANDSHAKE_STATUS[mac]:
             print(f"[{self.device_type}{self.player_num}] LAPTOP->BTL H sent")
             handshakeByte = bytes('H', 'utf-8')
             packet1 = struct.pack(packetFormat, handshakeByte, *padding)
             timeout_count += 1
             if self.serial_chars is not None:
                 self.serial_chars.write(packet1, withResponse=False)
             print(f"H #{timeout_count} sent to [{self.device_type}{self.player_num}]")
             if timeout_count % 10 == 0:
                 print(f"10 H sent, TIMEOUT & resetting [{self.device_type}{self.player_num}]")
                 timeout_count = 0
                 self.reset()
             if self.peripheral_obj_beetle and self.peripheral_obj_beetle.waitForNotifications(3.0):
                 # Send ACK after ACK received
                 if ACK_HANDSHAKE_SENT_flag[mac]:
                     ackByte = bytes('A', 'utf-8')
                     packet2 = struct.pack(packetFormat, ackByte, *padding)
                     if self.serial_chars is not None:
                         self.serial_chars.write(packet2, withResponse=False)
         return True
     except BTLEDisconnectError:
         print(f"[{self.device_type}{self.player_num}] handshake exception -> reconnect")
         BTL_HANDSHAKE_STATUS[self.mac_address] = False
         self.disconnect()
         return False
     except Exception as e:
         print(f"[{self.device_type}{self.player_num}] handshake other exception -> {e}")
         BTL_HANDSHAKE_STATUS[self.mac_address] = False
         self.disconnect()
         return False
     

 def handle_incoming_data(self):

     pass
 

 def run(self):
     mac = self.mac_address
     while not global_shutdown.is_set():
         # 1) Connect to Beetle
         if self.peripheral_obj_beetle is None:
             if not self.connect():
                 time.sleep(1)
                 continue
         # 2) Initiate handshake
         if not BTL_HANDSHAKE_STATUS.get(mac, False):
             if not self.initiate_handshake():
                 time.sleep(1)
                 continue
         # 3) Run as normal
         try:
             if BTL_RESET_STATUS[mac]:
                 self.reset()
             if self.peripheral_obj_beetle and self.peripheral_obj_beetle.waitForNotifications(1.0):
                 self.handle_incoming_data()
         except Exception as e:
             print(f"[{self.device_type}{self.player_num}] run() exception: {e}")
             self.disconnect()
             time.sleep(1)
             continue
         # Check reload/shield flags
         if self.device_type == 'G' and U_to_gun[self.player_num]:
             padding = (0,) * 18
             packetFormat = 'c' + 19 * 'B'
             statusByte = bytes('U', 'utf-8')
             packet4 = struct.pack(packetFormat, statusByte, ammo, *padding)
             if self.serial_chars:
                 self.serial_chars.write(packet4, withResponse=False)
             U_to_gun[self.player_num] = False
             print(f"[{self.device_type}{self.player_num}] Reload update sent")
         if self.device_type == 'V' and U_to_vest[self.player_num]:
             padding = (0,) * 18
             packetFormat = 'c' + 19 * 'B'
             statusByte = bytes('U', 'utf-8')
             packet4 = struct.pack(packetFormat, statusByte, health, *padding)
             if self.serial_chars:
                 self.serial_chars.write(packet4, withResponse=False)
             U_to_vest[self.player_num] = False
             print(f"[{self.device_type}{self.player_num}] Health update sent")
         # Check for IR ACK
         if self.peripheral_obj_beetle and self.peripheral_obj_beetle.waitForNotifications(0.1):
             if ACK_IR_SENT_flag[mac]:
                 # Send ACK after receiving IR data
                 padding = (0,) * 19
                 packetFormat = 'c' + 19 * 'B'
                 ackByte = bytes('A', 'utf-8')
                 packet3 = struct.pack(packetFormat, ackByte, *padding)
                 if self.serial_chars:
                     self.serial_chars.write(packet3, withResponse=False)
                 ACK_IR_SENT_flag[mac] = False
     # If global shutdown is set, we exit run().
     # Optionally do a final disconnect here if you want
     self.disconnect()


# Main
if __name__ == '__main__':
 
 if len(sys.argv) > 1:
     mode = int(sys.argv[1])
     if mode != 2:
         sys.exit()
 
 # Create a BeetleThread for each MAC
 beetles = []
 for binfo in ALL_BEETLES_INFO:
     mac = binfo["mac"]
     device_type = binfo["device_type"]
     player_num = binfo["player_num"]
     bthread = BeetleThread(mac, device_type, player_num)
     beetles.append(bthread)
 input("Press Enter when ready to start...")
 # Start all Beetle threads
 for b in beetles:
     b.start()
 # Start MQTT threads
 mqtt_thread = threading.Thread(target=mqtt_publish_loop, daemon=True)
 mqtt_sub_thread = threading.Thread(target=mqtt_subscriber, daemon=True)
 mqtt_thread.start()
 mqtt_sub_thread.start()
 try:
     # Wait for all beetle threads to finish
     for b in beetles:
         b.join(timeout=5)
 except (Exception, KeyboardInterrupt) as e:
     global_shutdown.set()
     traceback.print_exc()
