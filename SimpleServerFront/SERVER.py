from fastapi import FastAPI, WebSocket
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
import websockets
import json
import os
import asyncio
from dotenv import load_dotenv
import base64

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
                "output_audio_format": "pcm16",
                "sample_rate": 24000
            }
        }
        await openai_ws.send(json.dumps(session_config))
        
        async def receive_openai_messages():
            try:
                while True:
                    response = await openai_ws.recv()
                    data = json.loads(response)
                    
                    # Forward all messages to the client
                    await websocket.send_text(response)
                    
                    # Log message types for debugging
                    print(f"Message type: {data.get('type')}")
                    
                    if data.get('type') == 'response.audio.done':
                        print("Audio stream completed")
                    
                    elif data.get('type') == 'error':
                        print(f"Error from OpenAI: {data}")
                        
            except websockets.ConnectionClosed:
                print("OpenAI connection closed")
            except Exception as e:
                print(f"Error in receive_openai_messages: {e}")

        # Start background task to receive messages
        receiver_task = asyncio.create_task(receive_openai_messages())
        
        # Handle client messages
        while True:
            message = await websocket.receive_text()
            print(f"Received from client: {message}")
            
            # Create and send message to OpenAI
            user_message = {
                "type": "conversation.item.create",
                "item": {
                    "type": "message",
                    "role": "user",
                    "content": [{
                        "type": "input_text",
                        "text": message
                    }]
                }
            }
            await openai_ws.send(json.dumps(user_message))
            await openai_ws.send(json.dumps({"type": "response.create"}))
            
    except websockets.ConnectionClosed:
        print("Client connection closed")
    except Exception as e:
        print(f"Error in websocket_endpoint: {e}")
    finally:
        # Clean up
        try:
            receiver_task.cancel()
            await openai_ws.close()
        except:
            pass
        await websocket.close()

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000, reload=True)
