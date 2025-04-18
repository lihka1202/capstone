import json


def vis_payload(m):
    m = json.loads(m)
    p1_hp = m["game_state"]["p1"]["hp"]
    p1_b = m["game_state"]["p1"]["bullets"]
    p2_hp = m["game_state"]["p2"]["hp"]
    p2_b = m["game_state"]["p2"]["bullets"]

    new_payload = {
        "p1": {
            "hp": p1_hp,
            "bullets": p1_b
        },
        "p2": {
            "hp": p2_hp,
            "bullets": p2_b
        }
    }
    return json.dumps(new_payload)
