using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using TMPro;

public class WebSocketClient : MonoBehaviour
{
    // UI References
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button stopButton;

    // WebSocket connection
    private ClientWebSocket webSocket;
    private const string SERVER_URL = "ws://localhost:8001/unity";
    private const string LOG_FILE_PATH = "logs.txt";
    private CancellationTokenSource cancellationSource;
    private bool isConnected = false;

    private void Start()
    {
        // Setup UI button listeners
        sendButton.onClick.AddListener(OnSendButtonClick);
        connectButton.onClick.AddListener(OnConnectButtonClick);
        stopButton.onClick.AddListener(OnStopButtonClick);

        // Initial UI state
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

            // Update UI state
            isConnected = true;
            sendButton.interactable = true;
            stopButton.interactable = true;

            // Start listening for messages
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
                var buffer = new byte[4096*16];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"Received: {message}");
                    WriteToFile($"Received: {message}");
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
        messageInput.text = ""; // Clear input field after sending
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
    private async Task SendWebSocketMessage(string message)
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
}