from fastapi import FastAPI, WebSocket
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
import websockets
import json
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")

async def openai_websocket():
    uri = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01"
    headers = {
        "Authorization": f"Bearer {os.getenv('OPENAI_API_KEY')}",
        "OpenAI-Beta": "realtime=v1"
    }
    
    return await websockets.connect(uri, additional_headers=headers)

@app.get("/")
async def read_root():
    return FileResponse('static/index.html')

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    print("New WebSocket connection established")
    await websocket.accept()
    
    try:
        # Connect to OpenAI WebSocket
        openai_ws = await openai_websocket()
        print("Connected to OpenAI WebSocket")
        
        # Send initial session configuration
        session_config = {
            "event_id": "event_123",
            "type": "session.update",
            "session": {
                "modalities": ["text", "audio"],
                "voice": "ash",
                "input_audio_format": "pcm16",
                "output_audio_format": "pcm16"
            }
        }
        await openai_ws.send(json.dumps(session_config))
        
        # Send test message
        test_message = {
            "type": "conversation.item.create",
            "item": {
                "type": "message",
                "role": "user",
                "content": [{
                    "type": "input_text",
                    "text": "Hello, how are you?"
                }]
            }
        }
        await openai_ws.send(json.dumps(test_message))
        await openai_ws.send(json.dumps({"type": "response.create"}))
        
        # Relay messages from OpenAI to frontend
        while True:
            message = await openai_ws.recv()
            print(f"Received from OpenAI: {message}")
            await websocket.send_text(message)
            print(f"Sent to client: {message}")
            
    except Exception as e:
        print(f"Error: {e}")
        await websocket.close()

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
