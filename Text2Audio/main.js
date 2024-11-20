// main.js
import WebSocket from "ws";
import dotenv from 'dotenv';
import { AudioProcessor } from './audioProcessor.js';

dotenv.config();

const processor = new AudioProcessor({
    sampleRate: 24000,
    outputDir: 'output_audio'
});

const url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";
const ws = new WebSocket(url, {
    headers: {
        "Authorization": "Bearer " + process.env.OPENAI_API_KEY,
        "OpenAI-Beta": "realtime=v1",
    },
});

ws.on("open", function open() {
    console.log("Connected to server.");
    const event = {
        type: 'conversation.item.create',
        item: {
            type: 'message',
            role: 'user',
            content: [
                {
                    type: 'input_text',
                    text: 'What is the capital of France? Give me the humurous answer.'
                }
            ]
        }
    };
    ws.send(JSON.stringify(event));
    ws.send(JSON.stringify({type: 'response.create'}));
});

ws.on("message", async function incoming(message) {
    try {
        const data = JSON.parse(message.toString());
        
        if (data.type === 'response.audio.delta') {
            const chunkSize = await processor.processAudioChunk(data.delta);
            console.log(`Processed audio chunk: ${chunkSize} bytes`);
        }
        
        if (data.type === 'response.audio.done') {
            const metadata = await processor.finalizeAudio();
            console.log('Audio processing complete:', metadata);
        }
        
        await processor.fileManager.appendToLog(data);
    } catch (error) {
        console.error('Error processing message:', error);
    }
});

ws.on('error', function error(err) {
    console.error('WebSocket error:', err);
});

ws.on('close', function close() {
    console.log('WebSocket connection closed');
});