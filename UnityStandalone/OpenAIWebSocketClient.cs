using UnityEngine;
using System;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;


public class OpenAIWebSocketClient : MonoBehaviour
{
    private WebSocket websocket;
    private readonly string apiKey = ""; // Replace with your OpenAI API key
    //private readonly string websocketUrl = "wss://api.openai.com/v1/realtime";
    private readonly string websocketUrl = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";
    private AudioStreamPlayer audioPlayer;

    async void Start()
    {
        audioPlayer = gameObject.AddComponent<AudioStreamPlayer>();
        Debug.Log("Connecting to OpenAI WebSocket...");

        // Initialize WebSocket with headers
        websocket = new WebSocket(websocketUrl, new Dictionary<string, string> {
            { "Authorization", $"Bearer {apiKey}" },
            { "OpenAI-Beta", "realtime=v1" }
        });

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to OpenAI!");
            //SendInitialConfig();
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket Error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed");
        };

        await websocket.Connect();
    }

    private void SendInitialConfig()
    {
        var sessionConfig = new
        {
            type = "session.update",
            event_id = Guid.NewGuid().ToString(),
            session = new
            {
                voice = "ash",
                audio_format = "pcm16",
                vad = new
                {
                    type = "server",
                    threshold = 0.5,
                    prefix_padding = 300,
                    silence_duration = 500
                },
                tool_choice = "auto",
                temperature = 0.8,
                max_tokens = "inf"
            }
        };

        string configJson = JsonConvert.SerializeObject(sessionConfig);
        websocket.SendText(configJson);
    }

    public new async void SendMessage(string text)
    {
        if (websocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected!");
            return;
        }

        var message = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new
                    {
                        type = "input_text",
                        text = text
                    }
                }
            }
        };

        string messageJson = JsonConvert.SerializeObject(message);
        await websocket.SendText(messageJson);

        // Send response request
        var responseRequest = new
        {
            type = "response.create"
        };
        await websocket.SendText(JsonConvert.SerializeObject(responseRequest));
    }

    private void HandleMessage(string message)
    {
        try
        {
            var response = JObject.Parse(message);
            string messageType = response["type"].ToString();

            switch (messageType)
            {
                case "response.audio.delta":
                    string audioData = response["delta"].ToString();
                    ProcessAudioChunk(audioData);
                    break;

                case "response.audio.done":
                    Debug.Log("Audio response completed");
                    break;

                default:
                    Debug.Log($"Received message: {message}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing message: {e.Message}");
        }
    }

    private void ProcessAudioChunk(string base64Audio)
    {
        // Here you would handle the audio chunk
        // This could involve:
        // 1. Decoding the base64 string
        // 2. Converting to Unity AudioClip
        // 3. Playing the audio or saving it
        Debug.Log("Received audio chunk");
        audioPlayer.ProcessAudioChunk(base64Audio);
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}