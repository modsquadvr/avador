
import os
import websockets
from dotenv import load_dotenv

load_dotenv()

class OpenAIConnection:
    def __init__(self):
        self.uri = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01"
        self.headers = {
            "Authorization": f"Bearer {os.getenv('OPENAI_API_KEY')}",
            "OpenAI-Beta": "realtime=v1"
        }

    async def connect(self):
        return await websockets.connect(self.uri, additional_headers=self.headers)