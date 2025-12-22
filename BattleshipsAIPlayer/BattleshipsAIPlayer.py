from feature_builder import build_features
import random

SHOT_SINGLE = 0
SHOT_AIRSTRIKE = 1
SHOT_BOMBARDMENT = 2
AIRSTRIKE_UP_RIGHT_DELTAS = [0, -9, -18]
AIRSTRIKE_DOWN_RIGHT_DELTAS = [0, 11, 22]
BOMBARDMENT_DELTAS = [0, -1, 1, -10, 10]

class BattleshipsAiPlayer:

    def get_state(self, state):
        features = build_features(state)

        single_available_cells = features["available_cells"]

    def select_next_shot(self, state):
        pass

