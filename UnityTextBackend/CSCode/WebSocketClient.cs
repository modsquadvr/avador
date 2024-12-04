using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketClient : MonoBehaviour
{
    // WebSocket connection instance
    private ClientWebSocket webSocket;

    // Server URL from the documentation
    private readonly string serverUrl = "ws://localhost:8001/unity";

    // Buffer for receiving messages
    private readonly byte[] receiveBuffer = new byte[1024];

    // Cancellation token to handle cleanup
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        // Initialize the WebSocket connection when the script starts
        _ = InitializeWebSocketConnection();
    }

    private async Task InitializeWebSocketConnection()
    {
        try
        {
            // Create new instances
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log("Attempting to connect to WebSocket server...");

            // Connect to the WebSocket server
            await webSocket.ConnectAsync(new Uri(serverUrl), cancellationTokenSource.Token);

            Debug.Log("Successfully connected to WebSocket server!");

            // Start listening for messages
            _ = ReceiveMessagesAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to WebSocket server: {e.Message}");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                // Create a buffer to store the received message
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(receiveBuffer),
                    cancellationTokenSource.Token
                );

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    // Convert the received bytes to a string
                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);

                    // Log the received message
                    Debug.Log($"Received message: {message}");
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    // Handle WebSocket closure
                    Debug.Log("WebSocket connection closed by server");
                    await CloseConnection();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving message: {e.Message}");
            await CloseConnection();
        }
    }

    // Method to send messages to the server
    public async Task SendMessageAsync(string message)
    {
        try
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning("WebSocket is not connected. Attempting to reconnect...");
                await InitializeWebSocketConnection();
                return;
            }

            // Convert string message to bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Send the message
            await webSocket.SendAsync(
                new ArraySegment<byte>(messageBytes),
                WebSocketMessageType.Text,
                true,
                cancellationTokenSource.Token
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e.Message}");
        }
    }

    private async Task CloseConnection()
    {
        if (webSocket != null)
        {
            // Close the WebSocket connection if it's still open
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed by client",
                    CancellationToken.None
                );
            }

            // Dispose of the WebSocket instance
            webSocket.Dispose();
        }
    }

    private void OnDestroy()
    {
        // Ensure cleanup when the GameObject is destroyed
        cancellationTokenSource?.Cancel();
        _ = CloseConnection();
    }

    // Example method to test sending a message
    public void TestSendMessage(string testMessage)
    {
        _ = SendMessageAsync(testMessage);
    }
}