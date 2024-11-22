import WebSocket from "ws";
import dotenv from 'dotenv';
import fs from 'fs';
import { AudioProcessor } from '../Text2Audio/audioProcessor.js';
import { AudioStreamProcessor } from './audioStreamProcessor.js';
import { sessionConfig } from './config/sessionConfig.js';

dotenv.config();

const processor = new AudioProcessor({
    sampleRate: 24000,
    outputDir: './Audio2Audio/output_audio'
});

const url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";

const ws = new WebSocket(url, {
    headers: {
        "Authorization": "Bearer " + process.env.OPENAI_API_KEY,
        "OpenAI-Beta": "realtime=v1",
    },
});

const audioStreamProcessor = new AudioStreamProcessor(ws);

ws.on("open", async function open() {
    console.log("Connected to server.");
    
    // Send session configuration
    ws.send(JSON.stringify(sessionConfig));
    
    // Read and process input audio file
    const inputAudio = fs.readFileSync('./Audio2Audio/input_audio/sample.wav');
    await audioStreamProcessor.processAudio(inputAudio);
});

ws.on("message", async function incoming(message) {
    try {
        const data = JSON.parse(message.toString());
        
        if (data.type === 'response.audio.delta') {
            const chunkSize = await processor.processAudioChunk(data.delta);
            console.log(`Processed output audio chunk: ${chunkSize} bytes`);
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
