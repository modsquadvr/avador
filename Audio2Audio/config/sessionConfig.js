
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const readSystemPrompt = () => {
    try {
        return fs.readFileSync(path.join(__dirname, 'SYSTEM_PROMPT.txt'), 'utf-8').trim();
    } catch (error) {
        console.error('Error reading system prompt:', error);
        return 'Your knowledge cutoff is 2023-10. You are a helpful assistant.';
    }
};

export const sessionConfig = {
    event_id: "event_123",
    type: "session.update",
    session: {
        modalities: ["text", "audio"],
        instructions: readSystemPrompt(),
        voice: "ash",
        input_audio_format: "pcm16",
        output_audio_format: "pcm16",
        input_audio_transcription: {
            model: "whisper-1"
        },
        turn_detection: {
            type: "server_vad",
            threshold: 0.5,
            prefix_padding_ms: 300,
            silence_duration_ms: 500
        },
        tool_choice: "auto",
        temperature: 0.8,
        max_response_output_tokens: "inf"
    }
};