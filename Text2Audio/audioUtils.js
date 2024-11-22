// audioUtils.js
import { NONAME } from 'dns';
import fs from 'fs';
import path from 'path';
import { promisify } from 'util';

// Convert callback-based fs functions to Promise-based
const writeFileAsync = promisify(fs.writeFile);
const appendFileAsync = promisify(fs.appendFile);

/**
 * WavEncoder class handles the creation of WAV audio files
 * Supports configurable sample rate, channels, and bits per sample
 */
export class WavEncoder {
    constructor(sampleRate = 24000, numChannels = 1, bitsPerSample = 16) {
        this.sampleRate = sampleRate;
        this.numChannels = numChannels;
        this.bitsPerSample = bitsPerSample;
    }

    /**
     * Creates a WAV header with the specified parameters
     * @param {number} dataSize - Size of the PCM data in bytes
     * @returns {Buffer} WAV header as a Buffer
     */
    createWavHeader(dataSize) {
        const buffer = Buffer.alloc(44); // Standard WAV header is 44 bytes
        
        // RIFF chunk descriptor (12 bytes)
        buffer.write('RIFF', 0);                                  // ChunkID (4 bytes)
        buffer.writeUInt32LE(dataSize + 36, 4);                  // ChunkSize (4 bytes)
        buffer.write('WAVE', 8);                                 // Format (4 bytes)
        
        // Format chunk (24 bytes)
        buffer.write('fmt ', 12);                                // Subchunk1ID (4 bytes)
        buffer.writeUInt32LE(16, 16);                           // Subchunk1Size (4 bytes)
        buffer.writeUInt16LE(1, 20);                            // AudioFormat - PCM = 1 (2 bytes)
        buffer.writeUInt16LE(this.numChannels, 22);             // NumChannels (2 bytes)
        buffer.writeUInt32LE(this.sampleRate, 24);              // SampleRate (4 bytes)
        buffer.writeUInt32LE(this.sampleRate * this.numChannels * this.bitsPerSample / 8, 28); // ByteRate (4 bytes)
        buffer.writeUInt16LE(this.numChannels * this.bitsPerSample / 8, 32); // BlockAlign (2 bytes)
        buffer.writeUInt16LE(this.bitsPerSample, 34);          // BitsPerSample (2 bytes)
        
        // Data chunk (8 bytes + data)
        buffer.write('data', 36);                               // Subchunk2ID (4 bytes)
        buffer.writeUInt32LE(dataSize, 40);                     // Subchunk2Size (4 bytes)
        
        return buffer;
    }

    /**
     * Creates a complete WAV file by combining header and PCM data
     * @param {Buffer} pcmData - Raw PCM audio data
     * @returns {Buffer} Complete WAV file as a Buffer
     */
    createWavFile(pcmData) {
        const header = this.createWavHeader(pcmData.length);
        return Buffer.concat([header, pcmData]);
    }
}

/**
 * AudioFileManager handles file system operations for audio files
 * Manages directory structure and file saving operations
 */
export class AudioFileManager {
    constructor(baseDir = 'output_audio') {
        this.baseDir = baseDir;
        this.baseChunkDir = "";
        this.timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        this.ensureDirectoryExists();
    }

    /**
     * Creates necessary directories if they don't exist
     * Creates base directory and a timestamp-based chunks directory
     */
    ensureDirectoryExists() {
        if (!fs.existsSync(this.baseDir)) {
            fs.mkdirSync(this.baseDir);
        }
        
        // const chunksDir = path.join(this.baseDir, `chunks_${this.timestamp}`);
        // this.baseChunkDir = chunksDir;
        // if (!fs.existsSync(chunksDir)) {
        //     fs.mkdirSync(chunksDir);
        // }
    }

    /**
     * Saves a WAV buffer to a file
     * @param {Buffer} wavBuffer - WAV file data
     * @param {string} filename - Name of the output file
     * @returns {Promise<string>} Path to the saved file
     */
    async saveWavFile(wavBuffer, filename) {
        const filePath = path.join(this.baseDir, filename);
        await writeFileAsync(filePath, wavBuffer);
        return filePath;
    }

    /**
     * Saves metadata to a JSON file
     * @param {Object} metadata - Metadata to save
     * @param {string} filename - Name of the output file
     * @returns {Promise<string>} Path to the saved file
     */
    async saveMetadata(metadata, filename) {
        const filePath = path.join(this.baseDir, filename);
        await writeFileAsync(filePath, JSON.stringify(metadata, null, 2));
        return filePath;
    }

    /**
     * Generates a path for an audio chunk file
     * @param {number} chunkIndex - Index of the chunk
     * @returns {string} Path to the chunk file
     */
    getChunkPath(chunkIndex) {
        return path.join(this.baseChunkDir, `chunk_${chunkIndex.toString().padStart(5, '0')}.wav`);
    }

    /**
     * Appends data to a log file
     * @param {Object} data - Data to append to the log
     * @returns {Promise<void>}
     */
    async appendToLog(data) {
        const logPath = path.join(this.baseDir, this.timestamp + '_log.json');
        await appendFileAsync(logPath, JSON.stringify(data, null, 2) + ',\n');
    }
}