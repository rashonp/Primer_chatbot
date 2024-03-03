using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private Dropdown messageOptions;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are an intelligent agent that can answer questions, solve problems, and provide information.";

        private void Start()
        {
            // button.GetComponent<Image>().color = Color.red;
            button.onClick.AddListener(CreateMessage);

            // Make sure to add this listener in the Start or Awake function
            messageOptions.onValueChanged.AddListener(delegate {
                DropdownValueChanged(messageOptions);
            });
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);


            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        private void CreateMessage()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            AppendMessage(newMessage);
            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            SendReply();
        }

        private async void SendReply()
        {         
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
        // This function is called when the Dropdown value changes
        void DropdownValueChanged(Dropdown change)
        {
            var content = "";
            // You can also access change.options[change.value].text to get the display text
            if (change.value == 1)
            {
                Debug.Log("Simplify selected");
                content = "Simplify your previous response and make it more understandable.";
            }
            else if (change.value == 2)
            {
                Debug.Log("More details selected");
                content = "Add more details to your previous response.";
            }
            else if (change.value == 3)
            {
                Debug.Log("Why selected");
                content = "Explain what led you to provide this answer";
            }


            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = content
            };

            AppendMessage(newMessage);     
            messages.Add(newMessage);
            SendReply();
        }
    }
}
