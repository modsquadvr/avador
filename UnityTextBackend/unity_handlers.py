import json
import asyncio
from fastapi import WebSocket
from colorama import init, Fore, Style
from log_manager import LogManager

# Add parent directory to path to import from app
from connections import OpenAIConnection
from message_utils import MessageHandler

# Initialize colorama for cross-platform color support
init()

def truncate_message(msg: str, max_length: int = 400) -> str:
    return msg if len(msg) <= max_length else msg[:max_length] + "..."

class UnityWebSocketHandler:
    def __init__(self, websocket: WebSocket):
        self.websocket = websocket
        self.openai_ws = None
        self.receiver_task = None
        self.is_connected = False
        self.log_manager = LogManager()

    async def handle_connection(self):
        await self.websocket.accept()
        self.is_connected = True
        try:
            self.openai_ws = await OpenAIConnection().connect()
            print(f"{Fore.BLUE}[Unity] Connected to OpenAI WebSocket{Style.RESET_ALL}")
            
            # Start receiving messages from OpenAI
            self.receiver_task = asyncio.create_task(self.receive_openai_messages())
            # Handle messages from Unity
            await self.handle_unity_messages()
            
        except Exception as e:
            print(f"{Fore.RED}[Unity] Error in connection: {e}{Style.RESET_ALL}")
        finally:
            if self.is_connected:
                await self.cleanup()
                self.log_manager.cleanup()

    async def receive_openai_messages(self):
        try:
            while True:
                response = await self.openai_ws.recv()
                print(f"{Fore.GREEN}[OpenAI -> Unity] Raw message received: {truncate_message(response)}{Style.RESET_ALL}")
                self.log_manager.log_message(self.log_manager.openai_to_python, response)
                
                data = json.loads(response)
                
                # Add metadata for Unity
                unity_formatted = {
                    "source": "openai",
                    "timestamp": asyncio.get_event_loop().time(),
                    "data": data
                }
                
                # await self.websocket.send_json(unity_formatted)
                await self.websocket.send_json(data)
                self.log_manager.log_message(self.log_manager.python_to_unity, json.dumps(data))
                print(f"{Fore.GREEN}[OpenAI -> Unity] Message sent: {truncate_message(json.dumps(data))}{Style.RESET_ALL}")
                
        except Exception as e:
            print(f"{Fore.RED}[Unity] Error in receiving messages: {e}{Style.RESET_ALL}")

    async def handle_unity_messages(self):
        try:
            while True:
                message = await self.websocket.receive_text()
                self.log_manager.log_message(self.log_manager.unity_to_python, message)
                print(f"{Fore.BLUE}[Unity -> OpenAI] Raw message received: {truncate_message(message)}{Style.RESET_ALL}")
                
                # Format and send to OpenAI
                formatted_msg = MessageHandler.create_user_message(message)
                self.log_manager.log_message(self.log_manager.python_to_openai, json.dumps(formatted_msg))
                print(f"{Fore.BLUE}[Unity -> OpenAI] Formatted message: {truncate_message(json.dumps(formatted_msg))}{Style.RESET_ALL}")
                await self.openai_ws.send(json.dumps(formatted_msg))
                
                create_response_msg = {"type": "response.create"}
                self.log_manager.log_message(self.log_manager.python_to_openai, json.dumps(create_response_msg))
                print(f"{Fore.BLUE}[Unity -> OpenAI] Sending response create: {truncate_message(json.dumps(create_response_msg))}{Style.RESET_ALL}")
                await self.openai_ws.send(json.dumps(create_response_msg))
                
        except Exception as e:
            print(f"{Fore.RED}[Unity] Error handling messages: {e}{Style.RESET_ALL}")

    async def cleanup(self):
        if self.receiver_task:
            self.receiver_task.cancel()
            try:
                await self.receiver_task
            except asyncio.CancelledError:
                pass

        if self.openai_ws:
            await self.openai_ws.close()
            self.openai_ws = None

        if self.is_connected:
            self.is_connected = False
            await self.websocket.close()