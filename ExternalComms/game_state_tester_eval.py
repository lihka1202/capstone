# This is mostly to teset the game states and do it on the eva
import game_engine as ge
import game_engine_freeplay as gef

def run():
    # initiate the game engine
    engine = ge.GameEngine()

    # send action as p1,action,p2,action
    while(True):
        holder = input("block")
        engine.add_to_queue(1,"snowbomb", True, 0)
        engine.add_to_queue(2,"snowbomb", True, 0)
run()
