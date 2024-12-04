from fastapi import FastAPI, WebSocket
from unity_handlers import UnityWebSocketHandler
from log_manager import LogManager

app = FastAPI()
log_manager = LogManager()

@app.websocket("/unity")
async def unity_websocket_endpoint(websocket: WebSocket):
    handler = UnityWebSocketHandler(websocket)
    await handler.handle_connection()

@app.get("/last-message/{log_type}")
async def get_last_message(log_type: str):
    return {"message": log_manager.get_last_message(log_type)}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8001, reload=True)
