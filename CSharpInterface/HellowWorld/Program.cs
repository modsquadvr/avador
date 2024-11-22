using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    private static readonly string WebSocketUrl = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";
    private static readonly string ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    static async Task Main(string[] args)
    {
        using var client = new ClientWebSocket();
        // Add headers for authentication
        client.Options.SetRequestHeader("Authorization", $"Bearer {ApiKey}");
        client.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

        try
        {
            await client.ConnectAsync(new Uri(WebSocketUrl), CancellationToken.None);
            Console.WriteLine("Connected to server.");

            // Send initial message
            var sampleMessage = new { type = "message", content = "Hello, this is a test message" };
            await SendWebSocketMessage(client, sampleMessage);

            // Start receiving messages
            _ = ReceiveMessages(client);

            // Keep the application running
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task SendWebSocketMessage(ClientWebSocket client, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private static async Task ReceiveMessages(ClientWebSocket client)
    {
        var buffer = new byte[4096];

        try
        {
            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                        "Closing", CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                    await LogMessage(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }

    private static async Task LogMessage(string message)
    {
        try
        {
            await File.AppendAllTextAsync("websocket_log.txt", 
                $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging message: {ex.Message}");
        }
    }
}
