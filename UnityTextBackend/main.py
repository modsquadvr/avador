from fastapi import FastAPI, WebSocket
from unity_handlers import UnityWebSocketHandler

app = FastAPI()

@app.websocket("/unity")
async def unity_websocket_endpoint(websocket: WebSocket):
    handler = UnityWebSocketHandler(websocket)
    await handler.handle_connection()

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8001, reload=True)
