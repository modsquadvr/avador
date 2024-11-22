// main.js - Real-time Text-to-Speech WebSocket Client using OpenAI API

import WebSocket from "ws";
import dotenv from 'dotenv';
import { AudioProcessor } from './audioProcessor.js';
import { conversationEvents } from './config/conversationEvents.js';
import { sessionConfig } from './config/sessionConfig.js';

// Load environment variables from .env file
dotenv.config();

// Initialize audio processor with specific configuration
// This handles the incoming audio stream and saves it to files
const processor = new AudioProcessor({
    sampleRate: 24000,          // Audio sample rate in Hz
    outputDir: './Text2Audio/output_audio'   // Directory where audio files will be saved
});

// OpenAI WebSocket API endpoint for real-time text-to-speech
const url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";

// Initialize WebSocket connection with authentication headers
const ws = new WebSocket(url, {
    headers: {
        "Authorization": "Bearer " + process.env.OPENAI_API_KEY,  // API key from .env file
        "OpenAI-Beta": "realtime=v1",                            // Required header for beta access
    },
});

// Handle successful WebSocket connection
ws.on("open", function open() {
    console.log("Connected to server.");
    // Send session configuration
    ws.send(JSON.stringify(sessionConfig));
    // Use the imported event configuration
    ws.send(JSON.stringify(conversationEvents.sampleMessage));
    ws.send(JSON.stringify({type: 'response.create'}));
});

// Handle incoming WebSocket messages
ws.on("message", async function incoming(message) {
    try {
        // Parse the incoming message
        const data = JSON.parse(message.toString());
        
        // Handle audio chunks as they arrive
        if (data.type === 'response.audio.delta') {
            const chunkSize = await processor.processAudioChunk(data.delta);
            console.log(`Processed audio chunk: ${chunkSize} bytes`);
        }
        
        // Handle completion of audio stream
        if (data.type === 'response.audio.done') {
            const metadata = await processor.finalizeAudio();
            console.log('Audio processing complete:', metadata);
            // ws.close();  // Close the WebSocket connection
        }
        
        // Log all messages for debugging and analysis
        await processor.fileManager.appendToLog(data);
    } catch (error) {
        console.error('Error processing message:', error);
    }
});

// Handle WebSocket errors
ws.on('error', function error(err) {
    console.error('WebSocket error:', err);
});

// Handle WebSocket connection closure
ws.on('close', function close() {
    console.log('WebSocket connection closed');
});