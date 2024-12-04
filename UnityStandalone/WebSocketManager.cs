using UnityEngine;
using System;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class WebSocketManager : MonoBehaviour
{
    private WebSocket websocket;
    private string serverUrl = "ws://127.0.0.1:8000/ws"; // Change to your server URL

    // Color codes for Unity console rich text
    private readonly string keyColor = "#729FCF";     // Blue for keys
    private readonly string stringColor = "#8AE234";  // Green for strings
    private readonly string numberColor = "#EF2929";  // Red for numbers
    private readonly string boolColor = "#AD7FA8";    // Purple for booleans
    private readonly string nullColor = "#555753";    // Gray for null

    private string FormatJSON(string jsonString)
    {
        try
        {
            // Parse and format the JSON with indentation
            var parsedJson = JToken.Parse(jsonString);
            string formattedJson = parsedJson.ToString(Formatting.Indented);

            // Add color formatting
            formattedJson = formattedJson
                // Color the keys
                .Replace("\"", "<color=" + keyColor + ">\"")
                .Replace("\": ", "\"</color>: ")
                // Color string values
                .Replace(": \"", ": <color=" + stringColor + ">\"")
                .Replace("\",", "\"</color>,")
                .Replace("\"}", "\"</color>}")
                .Replace("\"]", "\"</color>]")
                // Color numbers
                .Replace(": ", ": <color=" + numberColor + ">")
                .Replace(",\n", "</color>,\n")
                .Replace("}\n", "</color>}\n")
                .Replace("]\n", "</color>]\n")
                // Color booleans
                .Replace(": true", ": <color=" + boolColor + ">true</color>")
                .Replace(": false", ": <color=" + boolColor + ">false</color>")
                // Color null
                .Replace(": null", ": <color=" + nullColor + ">null</color>");

            return formattedJson;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error formatting JSON: {e.Message}");
            return jsonString;
        }
    }

    private void LogMessage(string message, string prefix = "")
    {
        try
        {
            // Try to parse as JSON for formatted output
            var parsedJson = JToken.Parse(message);
            Debug.Log($"{prefix}<b>Received message:</b>\n{FormatJSON(message)}");
        }
        catch
        {
            // If not JSON, log as plain text
            Debug.Log($"{prefix}{message}");
        }
    }

    async void Start()
    {
        Debug.Log("Attempting to connect to WebSocket server...");

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("<color=green>WebSocket connection established</color>");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"<color=red>WebSocket error: {e}</color>");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("<color=yellow>WebSocket connection closed</color>");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            LogMessage(message);
        };

        await websocket.Connect();
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