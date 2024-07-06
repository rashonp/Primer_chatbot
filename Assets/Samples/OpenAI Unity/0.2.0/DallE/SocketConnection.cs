using UnityEngine;
using WebSocketSharp;

namespace OpenAI
{
    public class SocketConnection : MonoBehaviour
    {
        private WebSocket websocket;
        private bool connected = false;

        public delegate void ConnectionStatusChanged(bool isConnected);
        public event ConnectionStatusChanged OnConnectionStatusChanged;

        public delegate void MessageReceived(string message);
        public event MessageReceived OnMessageReceived;

        void Start()
        {
            ConnectToServer();
        }

        void ConnectToServer()
        {
            websocket = new WebSocket("ws://127.0.0.1:5000/ws");

            websocket.OnOpen += (sender, e) =>
            {
                Debug.Log("Connected to server");
                connected = true;
                OnConnectionStatusChanged?.Invoke(true);
            };

            websocket.OnMessage += (sender, e) =>
            {
                Debug.Log("Received message: " + e.Data);
                if (e.IsText)
                {
                    Debug.Log("Message is text: " + e.Data);
                    if (OnMessageReceived != null)
                    {
                        Debug.Log("Invoking OnMessageReceived event");
                        OnMessageReceived.Invoke(e.Data);
                        Debug.Log("OnMessageReceived event invoked");
                    }
                    else
                    {
                        Debug.LogWarning("OnMessageReceived event has no subscribers");
                    }
                }
                else
                {
                    Debug.LogWarning("Received non-text message, ignoring");
                }
            };

            websocket.OnClose += (sender, e) =>
            {
                Debug.Log("Disconnected from server");
                connected = false;
                OnConnectionStatusChanged?.Invoke(false);
            };

            websocket.OnError += (sender, e) =>
            {
                Debug.LogError("WebSocket Error: " + e.Message);
            };

            websocket.Connect();
        }

        public void SendMessageToServer(string message)
        {
            if (connected)
            {
                var formattedMessage = $"{{\"message\":\"{message.Replace("\"", "\\\"")}\"}}";
                Debug.Log("Sending message: " + formattedMessage);
                websocket.Send(formattedMessage);
            }
            else
            {
                Debug.Log("Not connected to server");
            }
        }

        private void OnDestroy()
        {
            if (websocket != null)
            {
                websocket.Close();
            }
        }
    }
}
