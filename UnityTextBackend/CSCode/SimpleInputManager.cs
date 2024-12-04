using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleInputManager : MonoBehaviour
{
    // UI Components
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;

    // Reference to WebSocket client
    private WebSocketClient webSocketClient;

    void Start()
    {
        // Get reference to WebSocket client
        webSocketClient = FindFirstObjectByType<WebSocketClient>();

        if (webSocketClient == null)
        {
            Debug.LogError("WebSocketClient not found in scene!");
            return;
        }

        // Setup button click listener
        sendButton.onClick.AddListener(SendMessage);

        // Setup input field enter key listener
        messageInput.onSubmit.AddListener((_) => SendMessage());
    }

    // Method to send message and clear input
    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(messageInput.text))
            return;

        // Send message through WebSocket
        webSocketClient.TestSendMessage(messageInput.text);

        // Clear input field
        messageInput.text = "What is today's date?";

        // Refocus input field
        messageInput.ActivateInputField();
    }
}
