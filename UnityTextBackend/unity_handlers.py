
import json
import asyncio
from fastapi import WebSocket

# Add parent directory to path to import from app
from connections import OpenAIConnection
from message_utils import MessageHandler

class UnityWebSocketHandler:
    def __init__(self, websocket: WebSocket):
        self.websocket = websocket
        self.openai_ws = None
        self.receiver_task = None

    async def handle_connection(self):
        await self.websocket.accept()
        try:
            self.openai_ws = await OpenAIConnection().connect()
            print("[Unity] Connected to OpenAI WebSocket")
            
            # Start receiving messages from OpenAI
            self.receiver_task = asyncio.create_task(self.receive_openai_messages())
            # Handle messages from Unity
            await self.handle_unity_messages()
            
        except Exception as e:
            print(f"[Unity] Error in connection: {e}")
        finally:
            await self.cleanup()

    async def receive_openai_messages(self):
        try:
            while True:
                response = await self.openai_ws.recv()
                data = json.loads(response)
                
                # Add metadata for Unity
                unity_formatted = {
                    "source": "openai",
                    "timestamp": asyncio.get_event_loop().time(),
                    "data": data
                }
                
                await self.websocket.send_json(unity_formatted)
                print(f"[Unity] Relayed OpenAI message: {data.get('type')}")
                
        except Exception as e:
            print(f"[Unity] Error in receiving messages: {e}")

    async def handle_unity_messages(self):
        try:
            while True:
                message = await self.websocket.receive_text()
                print(f"[Unity] Received message: {message}")
                
                # Format and send to OpenAI
                formatted_msg = MessageHandler.create_user_message(message)
                await self.openai_ws.send(json.dumps(formatted_msg))
                await self.openai_ws.send(json.dumps({"type": "response.create"}))
                
        except Exception as e:
            print(f"[Unity] Error handling messages: {e}")

    async def cleanup(self):
        if self.receiver_task:
            self.receiver_task.cancel()
        if self.openai_ws:
            await self.openai_ws.close()
        await self.websocket.close()