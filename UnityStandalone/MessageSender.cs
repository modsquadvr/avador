using UnityEngine;
using TMPro; // For TextMeshPro components
using UnityEngine.UI; // For UI components

public class MessageSender : MonoBehaviour
{
    public TMP_InputField inputField; // Drag your input field here
    public Button sendButton; // Drag your button here
    private OpenAIWebSocketClient client;

    void Start()
    {
        // Find the WebSocket client
        client = FindFirstObjectByType<OpenAIWebSocketClient>();

        // Set up button click handler
        sendButton.onClick.AddListener(SendMessage);
    }

    void SendMessage()
    {
        if (client != null && !string.IsNullOrEmpty(inputField.text))
        {
            client.SendMessage(inputField.text);
            inputField.text = ""; // Clear the input field
        }
    }
}