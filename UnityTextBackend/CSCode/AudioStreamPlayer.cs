using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles real-time streaming of audio data from base64-encoded PCM16 format.
/// Designed for Unity's AudioSource system with WebSocket streaming support.
/// </summary>
public class AudioStreamPlayer : MonoBehaviour
{
    private AudioSource audioSource;      // Unity component for audio playback
    private Queue<float> audioQueue;      // Stores incoming audio samples
    private float[] audioBuffer;          // Temporary buffer for audio processing
    private int sampleRate = 24000;       // Sample rate matching API specs
    private int bufferSize = 1024;        // Size of audio processing buffer
    private bool isPlaying = false;       // Audio playback state

    /// <summary>
    /// Initializes audio components and buffers on startup.
    /// </summary>
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioBuffer = new float[bufferSize];
        audioQueue = new Queue<float>();
    }

    /// <summary>
    /// Processes incoming base64-encoded audio chunks from WebSocket.
    /// </summary>
    /// <param name="base64Audio">Base64 encoded PCM16 audio data</param>
    public void ProcessAudioChunk(string base64Audio)
    {
        if (string.IsNullOrEmpty(base64Audio)) return;

        byte[] audioData = Convert.FromBase64String(base64Audio);
        float[] samples = ConvertBytesToFloat(audioData);

        foreach (float sample in samples)
        {
            audioQueue.Enqueue(sample);
        }

        if (!isPlaying)
        {
            StartNewAudioClip();
        }
    }

    /// <summary>
    /// Creates and starts a new looping AudioClip for streaming playback.
    /// </summary>
    private void StartNewAudioClip()
    {
        AudioClip clip = AudioClip.Create(
            "StreamingAudio",
            bufferSize,
            1,              // Mono channel
            sampleRate,
            true,           // Stream audio data
            OnAudioRead
        );
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
        isPlaying = true;
    }

    /// <summary>
    /// Unity callback for streaming audio data playback.
    /// Fills the output buffer with queued audio samples.
    /// </summary>
    /// <param name="data">Buffer to fill with audio samples</param>
    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = audioQueue.Count > 0 ? audioQueue.Dequeue() : 0f;
        }
    }

    /// <summary>
    /// Converts PCM16 byte data to Unity's float audio format (-1.0f to 1.0f).
    /// </summary>
    /// <param name="audioData">Raw PCM16 audio bytes</param>
    /// <returns>Normalized float array of audio samples</returns>
    private float[] ConvertBytesToFloat(byte[] audioData)
    {
        float[] samples = new float[audioData.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short sample = BitConverter.ToInt16(audioData, i * 2);
            samples[i] = sample / 32768f;  // Normalize to [-1.0f, 1.0f]
        }
        return samples;
    }
}