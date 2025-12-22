from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from BattleshipsAiPlayer import BattleshipsAiPlayer

app = FastAPI()
ai_player = BattleshipsAiPlayer()

class GameStateDTO(BaseModel):
    Grid: list[int]
    RemainingShipSizes: list[int]
    ShipsCanTouch: bool
    AirstrikeAllowed: bool
    BombardmentAllowed: bool
    AirstrikeHitCount: int
    BombardmentHitCount: int
    AirstrikeAvailable: bool
    BombardmentAvailable: bool

class AIMoveResponse(BaseModel):
    CellIndex: int
    ShotType: int   # keep int to match C#

@app.post("/next-move", response_model=AIMoveResponse)
def next_move(state: GameStateDTO):
    try:
        cell, shot_type = ai_player.select_next_shot(state)
        return AIMoveResponse(
            CellIndex=cell,
            ShotType=shot_type
        )
    except Exception as ex:
        # Let C# decide fallback strategy
        raise HTTPException(status_code=500, detail=str(ex))

