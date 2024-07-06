using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

[System.Serializable]
public class ResponseData
{
    public string response;
}

namespace OpenAI
{
    public class DallE : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Act as a random stranger in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";
        
        private SocketConnection socketConnection;

        private void Start()
        {
            button.onClick.AddListener(SendReply);
            socketConnection = GetComponent<SocketConnection>();
            socketConnection.OnMessageReceived += HandleResponse;
        }

        private IEnumerator AppendMessageCoroutine(ChatMessage message)
        {

            // Append new message to chat
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content as string;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
            yield return null;
        }

        private void AppendMessage(ChatMessage message)
        {
            // Start the coroutine to handle the UI updates on the main thread
            StartCoroutine(AppendMessageCoroutine(message));
        }

        private void SendReply()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };

            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text;

            messages.Add(newMessage);

            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;

            socketConnection.SendMessageToServer(newMessage.Content as string);
        }

        public void HandleResponse(string response)
        {
            // Parse the JSON string to an object
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

            // Extract the response text
            string responseText = responseData.response;

            // Print the response text
            var message = new ChatMessage()
            {
                Role = "assistant",
                Content = responseText
            };

            messages.Add(message);

            // Ensure the message is appended on the main thread
            MainThreadDispatcher.Enqueue(() => AppendMessage(message));

            MainThreadDispatcher.Enqueue(() =>
            {
                button.enabled = true;
                inputField.enabled = true;
            });
        }
    }
}
