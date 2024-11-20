// audioProcessor.js
import path from 'path';
import { WavEncoder, AudioFileManager } from './audioUtils.js';

export class AudioProcessor {
    constructor(config = {}) {
        this.wavEncoder = new WavEncoder(
            config.sampleRate || 24000,
            config.numChannels || 1,
            config.bitsPerSample || 16
        );
        this.fileManager = new AudioFileManager(config.outputDir);
        this.buffer = [];
        this.chunkCounter = 0;
    }

    async processAudioChunk(base64Data) {
        const audioContent = Buffer.from(base64Data, 'base64');
        this.buffer.push(audioContent);

        // Save individual chunk as WAV
        const wavChunk = this.wavEncoder.createWavFile(audioContent);
        const chunkPath = this.fileManager.getChunkPath(this.chunkCounter);
        await this.fileManager.saveWavFile(wavChunk, path.basename(chunkPath));
        
        this.chunkCounter++;
        return audioContent.length;
    }

    async finalizeAudio() {
        const combinedPCM = Buffer.concat(this.buffer);
        const wavFile = this.wavEncoder.createWavFile(combinedPCM);
        
        // Save complete WAV file
        const filename = `audio_${this.fileManager.timestamp}.wav`;
        await this.fileManager.saveWavFile(wavFile, filename);

        // Save metadata
        const metadata = {
            format: {
                sampleRate: this.wavEncoder.sampleRate,
                bitsPerSample: this.wavEncoder.bitsPerSample,
                channels: this.wavEncoder.numChannels,
                encoding: 'PCM'
            },
            audio: {
                durationSeconds: combinedPCM.length / (this.wavEncoder.sampleRate * this.wavEncoder.numChannels * this.wavEncoder.bitsPerSample / 8),
                totalBytes: combinedPCM.length,
                numberOfChunks: this.chunkCounter,
                timestamp: this.fileManager.timestamp
            }
        };
        
        await this.fileManager.saveMetadata(
            metadata,
            `audio_${this.fileManager.timestamp}_metadata.json`
        );

        // Reset state
        this.buffer = [];
        this.chunkCounter = 0;

        return metadata;
    }
}