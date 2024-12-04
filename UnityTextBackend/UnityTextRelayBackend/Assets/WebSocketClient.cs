using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

[Serializable]
public class WebSocketMessage
{
    public string type;
    public string event_id;
    public string response_id;
    public string item_id;
    public int output_index;
    public int content_index;
    public string delta;
}

public class WebSocketClient : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button stopButton;

    private AudioStreamPlayer audioPlayer;
    private ClientWebSocket webSocket;
    private const string SERVER_URL = "ws://localhost:8001/unity";
    private const string LOG_FILE_PATH = "logs.txt";
    private CancellationTokenSource cancellationSource;
    private bool isConnected = false;

    private void Start()
    {
        // Add AudioStreamPlayer component
        audioPlayer = gameObject.AddComponent<AudioStreamPlayer>();

        sendButton.onClick.AddListener(OnSendButtonClick);
        connectButton.onClick.AddListener(OnConnectButtonClick);
        stopButton.onClick.AddListener(OnStopButtonClick);

        sendButton.interactable = false;
        stopButton.interactable = false;
        connectButton.interactable = true;
    }

    private void OnConnectButtonClick()
    {
        InitializeWebSocket();
        ConnectToServer();
    }

    private void OnStopButtonClick()
    {
        StopWebSocketConnection();
    }

    private void InitializeWebSocket()
    {
        webSocket = new ClientWebSocket();
        cancellationSource = new CancellationTokenSource();
    }

    private async void ConnectToServer()
    {
        if (isConnected)
        {
            Debug.LogWarning("Already connected to WebSocket server");
            return;
        }

        try
        {
            connectButton.interactable = false;
            await webSocket.ConnectAsync(new Uri(SERVER_URL), cancellationSource.Token);

            Debug.Log("Connected to WebSocket server");
            WriteToFile("Connected to WebSocket server");

            isConnected = true;
            sendButton.interactable = true;
            stopButton.interactable = true;

            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
            WriteToFile($"Connection error: {e.Message}");
            ResetConnectionState();
        }
    }

    private async Task ReceiveMessages()
    {
        while (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var buffer = new byte[4096 * 16];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string rawMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        WebSocketMessage message = JsonUtility.FromJson<WebSocketMessage>(rawMessage);

                        if (message.type == "response.audio.delta" && !string.IsNullOrEmpty(message.delta))
                        {
                            // Process the audio delta through AudioStreamPlayer
                            audioPlayer.ProcessAudioChunk(message.delta);
                            Debug.Log($"Processing audio chunk of length: {message.delta.Length}");
                        }

                        WriteToFile(JsonUtility.ToJson(message));
                    }
                    catch (Exception jsonEx)
                    {
                        Debug.LogWarning($"JSON Error: {jsonEx.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                if (!cancellationSource.Token.IsCancellationRequested)
                {
                    Debug.LogError($"Error receiving message: {e.Message}");
                    WriteToFile($"Error receiving message: {e.Message}");
                }
                break;
            }
        }
    }

    private void WriteToFile(string message)
    {
        try
        {
            string timestampedMessage = $"[{DateTime.Now}] {message}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(Application.dataPath, LOG_FILE_PATH), timestampedMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error writing to log file: {e.Message}");
        }
    }

    private void OnSendButtonClick()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;
        _ = SendMessage(messageInput.text);
        messageInput.text = "";
    }

    private async Task SendMessage(string message)
    {
        if (webSocket?.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected");
            WriteToFile("WebSocket is not connected");
            return;
        }

        try
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(messageBytes),
                WebSocketMessageType.Text,
                true,
                cancellationSource.Token
            );

            Debug.Log($"Sent: {message}");
            WriteToFile($"Sent: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e.Message}");
            WriteToFile($"Error sending message: {e.Message}");
        }
    }

    private async void StopWebSocketConnection()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                Debug.Log("WebSocket connection closed");
                WriteToFile("WebSocket connection closed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error closing connection: {e.Message}");
                WriteToFile($"Error closing connection: {e.Message}");
            }
            finally
            {
                CleanupWebSocket();
            }
        }
        ResetConnectionState();
    }

    private void CleanupWebSocket()
    {
        if (cancellationSource != null)
        {
            cancellationSource.Cancel();
            cancellationSource.Dispose();
            cancellationSource = null;
        }

        if (webSocket != null)
        {
            webSocket.Dispose();
            webSocket = null;
        }
    }

    private void ResetConnectionState()
    {
        isConnected = false;
        connectButton.interactable = true;
        stopButton.interactable = false;
        sendButton.interactable = false;
    }

    private void OnDestroy()
    {
        StopWebSocketConnection();
    }
}