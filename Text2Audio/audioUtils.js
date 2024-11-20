// audioUtils.js
import fs from 'fs';
import path from 'path';
import { promisify } from 'util';

const writeFileAsync = promisify(fs.writeFile);
const appendFileAsync = promisify(fs.appendFile);

export class WavEncoder {
    constructor(sampleRate = 24000, numChannels = 1, bitsPerSample = 16) {
        this.sampleRate = sampleRate;
        this.numChannels = numChannels;
        this.bitsPerSample = bitsPerSample;
    }

    createWavHeader(dataSize) {
        const buffer = Buffer.alloc(44);
        
        // RIFF chunk descriptor
        buffer.write('RIFF', 0);
        buffer.writeUInt32LE(dataSize + 36, 4);
        buffer.write('WAVE', 8);
        
        // Format chunk
        buffer.write('fmt ', 12);
        buffer.writeUInt32LE(16, 16);
        buffer.writeUInt16LE(1, 20);
        buffer.writeUInt16LE(this.numChannels, 22);
        buffer.writeUInt32LE(this.sampleRate, 24);
        buffer.writeUInt32LE(this.sampleRate * this.numChannels * this.bitsPerSample / 8, 28);
        buffer.writeUInt16LE(this.numChannels * this.bitsPerSample / 8, 32);
        buffer.writeUInt16LE(this.bitsPerSample, 34);
        
        // Data chunk
        buffer.write('data', 36);
        buffer.writeUInt32LE(dataSize, 40);
        
        return buffer;
    }

    createWavFile(pcmData) {
        const header = this.createWavHeader(pcmData.length);
        return Buffer.concat([header, pcmData]);
    }
}

export class AudioFileManager {
    constructor(baseDir = 'output_audio') {
        this.baseDir = baseDir;
        this.timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        this.ensureDirectoryExists();
    }

    ensureDirectoryExists() {
        if (!fs.existsSync(this.baseDir)) {
            fs.mkdirSync(this.baseDir);
        }
        
        const chunksDir = path.join(this.baseDir, `chunks_${this.timestamp}`);
        if (!fs.existsSync(chunksDir)) {
            fs.mkdirSync(chunksDir);
        }
    }

    async saveWavFile(wavBuffer, filename) {
        const filePath = path.join(this.baseDir, filename);
        await writeFileAsync(filePath, wavBuffer);
        return filePath;
    }

    async saveMetadata(metadata, filename) {
        const filePath = path.join(this.baseDir, filename);
        await writeFileAsync(filePath, JSON.stringify(metadata, null, 2));
        return filePath;
    }

    getChunkPath(chunkIndex) {
        return path.join(this.baseDir, `chunks_${this.timestamp}`, `chunk_${chunkIndex.toString().padStart(5, '0')}.wav`);
    }

    async appendToLog(data) {
        const logPath = path.join(this.baseDir, 'output.json');
        await appendFileAsync(logPath, JSON.stringify(data, null, 2) + ',\n');
    }
}