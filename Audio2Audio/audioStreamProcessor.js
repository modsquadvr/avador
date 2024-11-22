
import { AudioEncoder } from './audioEncoder.js';

export class AudioStreamProcessor {
    constructor(ws) {
        this.ws = ws;
    }

    /**
     * Process and send audio data through WebSocket
     */
    async processAudio(audioData) {
        try {
            const channelData = await AudioEncoder.decodeAudioFile(audioData);
            const base64AudioData = AudioEncoder.base64EncodeAudio(channelData);
            
            const event = {
                type: 'conversation.item.create',
                item: {
                    type: 'message',
                    role: 'user',
                    content: [{
                        type: 'input_audio',
                        audio: base64AudioData
                    }]
                }
            };

            this.ws.send(JSON.stringify(event));
            this.ws.send(JSON.stringify({type: 'response.create'}));
            
            return true;
        } catch (error) {
            console.error('Error processing audio:', error);
            return false;
        }
    }
}