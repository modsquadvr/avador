
export class MessageHandler {
    constructor(audioProcessor) {
        this.audioProcessor = audioProcessor;
    }

    createUserMessage(userInput) {
        return {
            type: 'conversation.item.create',
            item: {
                type: 'message',
                role: 'user',
                content: [{ type: 'input_text', text: userInput }]
            }
        };
    }

    async handleIncomingMessage(message) {
        const data = JSON.parse(message.toString());
        
        if (data.type === 'response.audio.delta') {
            const chunkSize = await this.audioProcessor.processAudioChunk(data.delta);
            console.log(`Processed audio chunk: ${chunkSize} bytes`);
        }
        
        if (data.type === 'response.audio.done') {
            const metadata = await this.audioProcessor.finalizeAudio();
            console.log('Audio processing complete:', metadata);
            console.log('\nReady for next message (type "exit" to quit)');
        }
        
        await this.audioProcessor.fileManager.appendToLog(data);
    }
}