
import json
import asyncio
from fastapi import WebSocket
from .connections import OpenAIConnection
from .message_utils import MessageHandler

class WebSocketHandler:
    def __init__(self, websocket: WebSocket):
        self.websocket = websocket
        self.openai_ws = None
        self.receiver_task = None

    async def handle_connection(self):
        await self.websocket.accept()
        try:
            self.openai_ws = await OpenAIConnection().connect()
            print("Connected to OpenAI WebSocket")
            
            self.receiver_task = asyncio.create_task(self.receive_openai_messages())
            await self.handle_client_messages()
            
        except Exception as e:
            print(f"Error in websocket_handler: {e}")
        finally:
            await self.cleanup()

    async def receive_openai_messages(self):
        try:
            while True:
                response = await self.openai_ws.recv()
                data = json.loads(response)
                await self.websocket.send_text(response)
                print(f"Message type: {data.get('type')}")
                
                if data.get('type') == 'response.audio.done':
                    print("Audio stream completed")
                elif data.get('type') == 'error':
                    print(f"Error from OpenAI: {data}")
                    
        except Exception as e:
            print(f"Error in receive_openai_messages: {e}")

    async def handle_client_messages(self):
        while True:
            message = await self.websocket.receive_text()
            print(f"Received from client: {message}")
            
            user_message = MessageHandler.create_user_message(message)
            await self.openai_ws.send(json.dumps(user_message))
            await self.openai_ws.send(json.dumps({"type": "response.create"}))

    async def cleanup(self):
        if self.receiver_task:
            self.receiver_task.cancel()
        if self.openai_ws:
            await self.openai_ws.close()
        await self.websocket.close()