
import decodeAudio from 'audio-decode';

export class AudioEncoder {
    /**
     * Converts Float32Array of audio data to PCM16 ArrayBuffer
     */
    static floatTo16BitPCM(float32Array) {
        const buffer = new ArrayBuffer(float32Array.length * 2);
        const view = new DataView(buffer);
        let offset = 0;
        for (let i = 0; i < float32Array.length; i++, offset += 2) {
            let s = Math.max(-1, Math.min(1, float32Array[i]));
            view.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7fff, true);
        }
        return buffer;
    }

    /**
     * Converts a Float32Array to base64-encoded PCM16 data
     */
    static base64EncodeAudio(float32Array) {
        const arrayBuffer = this.floatTo16BitPCM(float32Array);
        let binary = '';
        let bytes = new Uint8Array(arrayBuffer);
        const chunkSize = 0x8000; // 32KB chunk size
        for (let i = 0; i < bytes.length; i += chunkSize) {
            let chunk = bytes.subarray(i, i + chunkSize);
            binary += String.fromCharCode.apply(null, chunk);
        }
        return btoa(binary);
    }

    /**
     * Decodes audio file to channel data
     */
    static async decodeAudioFile(audioData) {
        const audioBuffer = await decodeAudio(audioData);
        return audioBuffer.getChannelData(0); // Returns mono channel
    }
}