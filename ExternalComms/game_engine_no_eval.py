from queue import Queue
from mqtt_fxn import *
import json
from create_socket import create_socket as cs
import enc_for as ef
import os

AES_KEY = "PLSFUCKINGWORK!!".encode("utf-8")
IV = os.urandom(16)
T_HOST, T_PORT = "127.0.0.1", 8888
hello = "hello"

class Player:
    def __init__(self,id):
        self.id = id
        self.health = 100
        self.bullets = 6
        self.bombs = 2
        self.shield_hp = 0
        self.deaths = 0
        self.shields = 3

class GameEngine:
    def __init__(self):
        self.queue = Queue()
        self.player1 = Player(1)
        self.player2 = Player(2)
        self.vizbcast1 = new_mqtt_client(client_id="Ultra96_broadcast_p1", user=PUB_USER, password=PUB_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
        self.vizbcast2 = new_mqtt_client(client_id="Ultra96_broadcast_p2", user=PUB_USER, password=PUB_PW, bkr=MQTT_BROKER, port=MQTT_PORT)
        self.socket = cs(T_HOST, T_PORT)
        if self.socket:
            handshake_msg = ef.encrypt_message(hello, IV, AES_KEY)
            formatted_msg = ef.format_message(handshake_msg)
            self.socket.sendall(formatted_msg)
            print("Handshake with eval_server complete.")

    def add_to_queue(self, id, action, visibility, debuf):
        self.queue.put((id, action, visibility, debuf))
        self.resolve()
    
    #def send_to_eval_server(self, payload):
    #    en_msg = ef.encrypt_message(payload, IV, AES_KEY)   # Encrypt message
    #    fo_msg = ef.format_message(en_msg)  # Format message
    #    self.socket.sendall(fo_msg) # Send to Server
    #    res = self.socket.recv(4096)

    def resolve(self):
        while not self.queue.empty():
            id, action, visibility, debuf = self.queue.get()
            actor = self.get_player_by_id(id)
            opponent = self.player2 if actor == self.player1 else self.player1
            # if action == "shield":
            #     # Check if the actor even has enough shields left
            #     if actor.shields > 1:
            #         # Enable shield with full health
            #         actor.shield_hp = 30

            #         # Decrease the number of existing shields
            #         actor.shields -= 1

            #         # Check if the opponent is visible and if any debufs
            #         if visibility and debuf > 0:
            #             total_debuf_applied = debuf * -5
            #             # Check if the opponent has any shields on
            #             if opponent.shield_hp >= -1*total_debuf_applied:
            #                 opponent.shield_hp += total_debuf_applied
            #             else:
            #                 ## Add whatever is left
            #                 total_debuf_applied += opponent.shield_hp

            #                 # Make the shield_hp 0
            #                 opponent.shield_hp = 0

            #                 # Subtract the rest from the health
            #                 opponent.health -= total_debuf_applied

            #                 if opponent.health <= 0:
            #                     opponent.deaths += 1
            #                     opponent.health = 100
            #                     opponent.shields = 3
            #                     opponent.shield_hp = 0
            #                     opponent.bombs = 2
            #                     opponent.bullets = 6

            if action == "shield":
                # Enable shield only if available
                if actor.shields > 0:
                    actor.shield_hp = 30
                    actor.shields -= 1
                    print(f"Player {actor.id} activated a 30HP shield.")

                    # Check if opponent is visible and under debuff
                    if visibility and debuf > 0:
                        total_debuf_applied = debuf * 5  # Total damage to be applied
                        print(f"Debuff effect: {total_debuf_applied} damage to Player {opponent.id}.")

                        if opponent.shield_hp > 0:
                            if opponent.shield_hp >= total_debuf_applied:
                                opponent.shield_hp -= total_debuf_applied
                                total_debuf_applied = 0
                                print(f"Opponent's shield absorbed all debuff damage.")
                            else:
                                total_debuf_applied -= opponent.shield_hp
                                print(f"Opponent's shield absorbed {opponent.shield_hp} damage.")
                                opponent.shield_hp = 0

                        # Apply remaining debuff damage to health
                        if total_debuf_applied > 0:
                            opponent.health -= total_debuf_applied
                            print(f"{total_debuf_applied} damage dealt to Player {opponent.id}'s health.")

                            # Check for death and respawn
                            if opponent.health <= 0:
                                opponent.deaths += 1
                                opponent.health = 100
                                opponent.shields = 3
                                opponent.shield_hp = 0
                                opponent.bombs = 2
                                opponent.bullets = 6
                                print(f"Player {opponent.id} died and respawned.")
            elif action == "reload":
                if actor.bullets > 0:
                    # DO nothing here since cannot really do anything
                    print("Can do nothing since gun is not empty")
                else:
                    # Reload regardless of visibility
                    actor.bullets = 6

                    # Check if opponent is visible and under debuff
                    if visibility and debuf > 0:
                        total_debuf_applied = debuf * 5  # Total damage to be applied
                        print(f"Debuff effect: {total_debuf_applied} damage to Player {opponent.id}.")

                        if opponent.shield_hp > 0:
                            if opponent.shield_hp >= total_debuf_applied:
                                opponent.shield_hp -= total_debuf_applied
                                total_debuf_applied = 0
                                print(f"Opponent's shield absorbed all debuff damage.")
                            else:
                                total_debuf_applied -= opponent.shield_hp
                                print(f"Opponent's shield absorbed {opponent.shield_hp} damage.")
                                opponent.shield_hp = 0

                        # Apply remaining debuff damage to health
                        if total_debuf_applied > 0:
                            opponent.health -= total_debuf_applied
                            print(f"{total_debuf_applied} damage dealt to Player {opponent.id}'s health.")

                            # Check for death and respawn
                            if opponent.health <= 0:
                                opponent.deaths += 1
                                opponent.health = 100
                                opponent.shields = 3
                                opponent.shield_hp = 0
                                opponent.bombs = 2
                                opponent.bullets = 6
                                print(f"Player {opponent.id} died and respawned.")

            elif action == "gun":
                # Check if the opponent is even visible
                if visibility:
                    # Check if you have enough ammo
                    if actor.bullets > 0:
                        # Reduce bullet count
                        actor.bullets -= 1

                        # Add to the total debuf (snow + bullet)
                        total_debuf_applied = debuf * 5 + 5 
                        print(f"Debuff effect: {total_debuf_applied} damage to Player {opponent.id}.")

                        if opponent.shield_hp > 0:
                            if opponent.shield_hp >= total_debuf_applied:
                                opponent.shield_hp -= total_debuf_applied
                                total_debuf_applied = 0
                                print(f"Opponent's shield absorbed all debuff damage.")
                            else:
                                total_debuf_applied -= opponent.shield_hp
                                print(f"Opponent's shield absorbed {opponent.shield_hp} damage.")
                                opponent.shield_hp = 0

                        # Apply remaining debuff damage to health
                        if total_debuf_applied > 0:
                            opponent.health -= total_debuf_applied
                            print(f"{total_debuf_applied} damage dealt to Player {opponent.id}'s health.")

                            # Check for death and respawn
                            if opponent.health <= 0:
                                opponent.deaths += 1
                                opponent.health = 100
                                opponent.shields = 3
                                opponent.shield_hp = 0
                                opponent.bombs = 2
                                opponent.bullets = 6
                                print(f"Player {opponent.id} died and respawned.")
                else:
                    print("No visibility, hence blind fire")
                    if actor.bullets > 0:
                        actor.bullets -= 1
            elif action == "snowbomb":
                if visibility and actor.bombs > 0:
                    # Debuf due to snowball itself is 5
                    total_debuf_applied = debuf * 5 + 5
                    print(f"Debuff effect: {total_debuf_applied} damage to Player {opponent.id}.")

                    if opponent.shield_hp > 0:
                        if opponent.shield_hp >= total_debuf_applied:
                            opponent.shield_hp -= total_debuf_applied
                            total_debuf_applied = 0
                            print(f"Opponent's shield absorbed all debuff damage.")
                        else:
                            total_debuf_applied -= opponent.shield_hp
                            print(f"Opponent's shield absorbed {opponent.shield_hp} damage.")
                            opponent.shield_hp = 0

                    # Apply remaining debuff damage to health
                    if total_debuf_applied > 0:
                        opponent.health -= total_debuf_applied
                        print(f"{total_debuf_applied} damage dealt to Player {opponent.id}'s health.")

                        # Check for death and respawn
                        if opponent.health <= 0:
                            opponent.deaths += 1
                            opponent.health = 100
                            opponent.shields = 3
                            opponent.shield_hp = 0
                            opponent.bombs = 2
                            opponent.bullets = 6
                            print(f"Player {opponent.id} died and respawned.")
                elif visibility and actor.bombs == 0:
                    print("viz but no bombs, do nothing")
                elif not visibility and actor.bombs > 0:
                    print("no viz but have bombs, waste")
                    actor.bombs -= 1

            elif action in ["golf", "fencing", "boxing", "badminton"]:
                if visibility:
                    # snow storm + 10 since each action here is 10 damage
                    total_debuf_applied = debuf * 5 + 10
                    print(f"Debuff effect: {total_debuf_applied} damage to Player {opponent.id}.")

                    if opponent.shield_hp > 0:
                        if opponent.shield_hp >= total_debuf_applied:
                            opponent.shield_hp -= total_debuf_applied
                            total_debuf_applied = 0
                            print(f"Opponent's shield absorbed all debuff damage.")
                        else:
                            total_debuf_applied -= opponent.shield_hp
                            print(f"Opponent's shield absorbed {opponent.shield_hp} damage.")
                            opponent.shield_hp = 0

                    # Apply remaining debuff damage to health
                    if total_debuf_applied > 0:
                        opponent.health -= total_debuf_applied
                        print(f"{total_debuf_applied} damage dealt to Player {opponent.id}'s health.")

                        # Check for death and respawn
                        if opponent.health <= 0:
                            opponent.deaths += 1
                            opponent.health = 100
                            opponent.shields = 3
                            opponent.shield_hp = 0
                            opponent.bombs = 2
                            opponent.bullets = 6
                            print(f"Player {opponent.id} died and respawned.")
            self.print_status()

            ## You need to broadcast to visualizers
            # publisher.publish(payload, topic)

            # This works for player 1, but you need to flip the system for player 2 side of things
            game_state_one = {
                    "game_state": {
                            "p1": player_to_dict(self.player1),
                            "p2": player_to_dict(self.player2)
                    }
            }
            game_state_two = {
                    "game_state": {
                            "p1": player_to_dict(self.player2),
                            "p2": player_to_dict(self.player1)
                    }
            }

            json_payload_one = json.dumps(game_state_one)
            json_payload_two = json.dumps(game_state_two)
            print(json_payload_one)
            print(json_payload_two)
            self.vizbcast1.publish(topic=BCAST_TOPIC_P1, payload=json_payload_one)
            self.vizbcast2.publish(topic=BCAST_TOPIC_P2, payload=json_payload_two)


            ## Send to the eval server
            #eval_payload = {
            #        "player_id": id,
            #        "action": action,
            #        "game_state": game_state_one["game_state"]
            #}
            #json_eval_payload = json.dumps(eval_payload)
            #send_to_eval_server(json_eval_payload)

    def get_player_by_id(self, id):
        return self.player1 if self.player1.id == id else self.player2

    def print_status(self):
        print(f"P1: HP={self.player1.health}, Bullets={self.player1.bullets}, Bombs={self.player1.bombs}, ShieldHP={self.player1.shield_hp}, Shields={self.player1.shields}, Deaths={self.player1.deaths}")
        print(f"P2: HP={self.player2.health}, Bullets={self.player2.bullets}, Bombs={self.player2.bombs}, ShieldHP={self.player2.shield_hp}, Shields={self.player2.shields}, Deaths={self.player2.deaths}")
        print(f"\n")


def player_to_dict(player):
    return {
        "hp": player.health,
        "bullets": player.bullets,
        "bombs": player.bombs,
        "shield_hp": player.shield_hp,
        "death": player.deaths,
        "shields": player.shields
    }

engine = GameEngine()
engine.add_to_queue(1, "snowbomb", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"shield",True,0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1,"gun", True, 0)
# engine.add_to_queue(1, "reload", False, 0)
# engine.add_to_queue(1,"gun", True, 0)

def testBasicShootingReloadingShielding():
    engine = GameEngine()
    # 1. Player 1 shoots until empty
    for _ in range(6):
        engine.add_to_queue(1, "gun", True, 0)

    # 2. Player 1 tries to shoot with empty gun
    engine.add_to_queue(1, "gun", True, 0)

    # 3. Player 1 reloads
    engine.add_to_queue(1, "reload", True, 0)

    # 4. Player 1 shoots again
    engine.add_to_queue(1, "gun", True, 0)

    # 5. Player 2 uses a shield
    engine.add_to_queue(2, "shield", True, 0)

    # 6. Player 1 shoots into shield
    engine.add_to_queue(1, "gun", True, 0)

    engine.add_to_queue(1, "fencing", True, 0)
    engine.add_to_queue(1, "fencing", True, 0)

    # Last one to break and see impact change
    engine.add_to_queue(1, "fencing", True, 0)

def testActionShieldInteracionOne():
    engine = GameEngine()
    engine.add_to_queue(1, "snowbomb", True, 2)

    # 8. Player 2 attacks with fencing + snowstorm effect
    engine.add_to_queue(2, "fencing", True, 3)

    # 9. Player 1 activates shield during snowstorm
    engine.add_to_queue(1, "shield", True, 2)

def testTryToKill():
    engine = GameEngine()
    for _ in range(10):
        engine.add_to_queue(1, "gun", True, 1)
        engine.add_to_queue(1, "reload", True, 0)
    engine.add_to_queue(1, "gun", True, 0)

def noVisAvailable():
    engine = GameEngine()
    # 12. Player 2 tries to use gold attack without visibility
    engine.add_to_queue(2, "golf", False, 3)

    # 13. Player 2 tries to gun Player 1 without visibility
    engine.add_to_queue(2, "gun", False, 0)

    # 14. Player 2 reloads during snowstorm without visibility (should still succeed)
    engine.add_to_queue(2, "reload", False, 3)

# testBasicShootingReloadingShielding()
# testActionShieldInteracionOne()
#testTryToKill()
# noVisAvailable()
