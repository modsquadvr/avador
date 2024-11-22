// audioProcessor.js
import path from 'path';
import { WavEncoder, AudioFileManager } from './audioUtils.js';

/**
 * Handles the processing and management of audio data, including chunk processing
 * and WAV file generation.
 */
export class AudioProcessor {
    /**
     * Initialize the AudioProcessor with custom configuration
     * @param {Object} config - Configuration options
     * @param {number} [config.sampleRate=24000] - Audio sample rate in Hz
     * @param {number} [config.numChannels=1] - Number of audio channels
     * @param {number} [config.bitsPerSample=16] - Bits per sample
     * @param {string} [config.outputDir] - Output directory for audio files
     */
    constructor(config = {}) {
        this.wavEncoder = new WavEncoder(
            config.sampleRate || 24000,
            config.numChannels || 1,
            config.bitsPerSample || 16
        );
        this.fileManager = new AudioFileManager(config.outputDir);
        this.buffer = [];          // Stores audio chunks
        this.chunkCounter = 0;     // Tracks number of processed chunks
    }

    /**
     * Process an individual audio chunk from base64 data
     * @param {string} base64Data - Base64 encoded audio data
     * @returns {Promise<number>} Length of processed audio content
     */
    async processAudioChunk(base64Data) {
        // Convert base64 to buffer and store
        const audioContent = Buffer.from(base64Data, 'base64');
        this.buffer.push(audioContent);

        // Create and save individual WAV chunk
        const wavChunk = this.wavEncoder.createWavFile(audioContent);
        const chunkPath = this.fileManager.getChunkPath(this.chunkCounter);
        // await this.fileManager.saveWavFile(wavChunk, chunkPath);
        
        this.chunkCounter++;
        return audioContent.length;
    }

    /**
     * Finalize audio processing by combining chunks and generating metadata
     * @returns {Promise<Object>} Audio metadata including format and statistics
     */
    async finalizeAudio() {
        // Combine all audio chunks
        const combinedPCM = Buffer.concat(this.buffer);
        const wavFile = this.wavEncoder.createWavFile(combinedPCM);
        
        // Generate filename and save complete WAV file
        const filename = `audio_${this.fileManager.timestamp}.wav`;
        await this.fileManager.saveWavFile(wavFile, filename);

        // Calculate and save audio metadata
        const metadata = {
            format: {
                sampleRate: this.wavEncoder.sampleRate,
                bitsPerSample: this.wavEncoder.bitsPerSample,
                channels: this.wavEncoder.numChannels,
                encoding: 'PCM'
            },
            audio: {
                durationSeconds: combinedPCM.length / (this.wavEncoder.sampleRate * 
                    this.wavEncoder.numChannels * this.wavEncoder.bitsPerSample / 8),
                totalBytes: combinedPCM.length,
                numberOfChunks: this.chunkCounter,
                timestamp: this.fileManager.timestamp
            }
        };
        
        // Save metadata to JSON file
        await this.fileManager.saveMetadata(
            metadata,
            `audio_${this.fileManager.timestamp}_metadata.json`
        );

        // Reset processor state
        this.buffer = [];
        this.chunkCounter = 0;

        return metadata;
    }
}