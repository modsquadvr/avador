// main.js - Real-time Text-to-Speech WebSocket Client using OpenAI API

import dotenv from 'dotenv';
import { AudioProcessor } from './audioProcessor.js';
import { sessionConfig } from './config/sessionConfig.js';
import { WebSocketClient } from './src/websocket/wsClient.js';
import { UserInputHandler } from './src/input/userInput.js';
import { MessageHandler } from './src/message/messageHandler.js';

dotenv.config();

const processor = new AudioProcessor({
    sampleRate: 24000,
    outputDir: './Text2Audio/output_audio'
});

const url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";
const headers = {
    "Authorization": "Bearer " + process.env.OPENAI_API_KEY,
    "OpenAI-Beta": "realtime=v1",
};

const messageHandler = new MessageHandler(processor);
const wsClient = new WebSocketClient(url, headers, (msg) => messageHandler.handleIncomingMessage(msg));
const userInput = new UserInputHandler();

async function startConversation() {
    await wsClient.connect();
    wsClient.send(sessionConfig);
    
    while (true) {
        const input = await userInput.getUserInput();
        if (input.toLowerCase() === 'exit') {
            console.log('Ending conversation...');
            wsClient.close();
            userInput.close();
            break;
        }
        wsClient.send(messageHandler.createUserMessage(input));
        wsClient.send({type: 'response.create'});
    }
}

startConversation().catch(error => {
    console.error('Error in conversation:', error);
    process.exit(1);
});