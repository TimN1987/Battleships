import numpy as np

EMPTY = 0
MISS = 1
HIT = 2
SUNK = 3

def build_features(state):
    grid = state.Grid

    available_cells = [
        i for i, cell in enumerate(grid)
        if cell == EMPTY
    ]

    missed_cells = [
        i for i, cell in enumerate(grid)
        if cell == MISS
    ]

    hit_cells = [
        i for i, cell in enumerate(grid)
        if cell == HIT
    ]

    sunk_cells = [
        i for i, cell in enumerate(grid)
        if cell == SUNK
    ]

    return {
        "available_cells": available_cells,
        "missed_cells": missed_cells,
        "hit_cells": hit_cells,
        "sunk_cells": sunk_cells,
        "remaining_ships": state.RemainingShipSizes
    }

