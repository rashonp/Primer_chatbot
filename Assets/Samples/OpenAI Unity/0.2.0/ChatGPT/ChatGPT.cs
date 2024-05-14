using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Threading.Tasks;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        // UI elements
        [SerializeField] private InputField inputField;
        [SerializeField] private Slider explanationSlider;
        [SerializeField] private Slider detailsSlider;
        [SerializeField] private Slider responseSlider;
        [SerializeField] private Button button;
        [SerializeField] private Dropdown messageOptions;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;
        [SerializeField] private RectTransform sampleTextRectTransform;
        [SerializeField] private Button sampleQuestionButton;
        [SerializeField] private TMP_InputField imageInputField;

        private TextMeshProUGUI sampleQuestionText;
        public Vector2 padding;
        private RectTransform rectTransform;
        private float height;
        private OpenAIApi openai = new OpenAIApi();
        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are an intelligent agent that can answer questions, solve problems, and provide information.";
        private Dictionary<int, string> explanationDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> detailsDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> responseDictionary = new Dictionary<int, string>();

        private void Start()
        {
            // Set up button and dropdown listeners
            button.onClick.AddListener(CreateMessage);
            messageOptions.onValueChanged.AddListener(delegate {
                DropdownValueChanged(messageOptions);
            });

            // Fill dictionaries with response templates
            InitializeDictionaries();

            // Initialize sample question button
            rectTransform = sampleQuestionButton.GetComponent<RectTransform>();
            sampleQuestionText = sampleTextRectTransform.GetComponent<TextMeshProUGUI>();
            sampleQuestionText.text = "What is astrophysics?";
            AdjustSize();
            sampleQuestionButton.onClick.AddListener(UpdateInput);
        }

        private void InitializeDictionaries()
        {
            explanationDictionary.Add(0, "Answer as if you were an elementary school teacher talking to a child. Your response should be really simple and very easy to understand.");
            explanationDictionary.Add(1, "Answer as if you were a high school teacher talking to a teenager. Your response should be understandable for a high school student.");
            explanationDictionary.Add(2, "Answer as if you were a professor talking to a college student. Your response should be understandable for someone who has completed high school.");
            explanationDictionary.Add(3, "Answer as if you were talking to a grad student. Your response should be understandable for someone who has completed college.");
            explanationDictionary.Add(4, "Answer as if you were a leading expert in this field talking to a colleague.");

            detailsDictionary.Add(0, "Your response should be very short and to the point. At most, it should be two short sentences.");
            detailsDictionary.Add(1, "Your response should be moderately detailed. It should be 3 to 5 sentences long.");
            detailsDictionary.Add(2, "Your response should be comprehensive. It should be at least 2 paragraphs long.");

            responseDictionary.Add(0, "");
            responseDictionary.Add(1, "Provide two different responses to the same question. Clearly separate and number the two responses.");
            responseDictionary.Add(2, "Provide three different responses to the same question. Clearly separate and number the three responses.");
        }

        private void AdjustSize()
        {
            if (sampleTextRectTransform != null)
            {
                // Adjust the size of the button based on text size
                rectTransform.sizeDelta = new Vector2(sampleQuestionText.preferredWidth + padding.x, sampleQuestionText.preferredHeight + padding.y);
            }
        }

        private void UpdateInput()
        {
            // Update input field with sample question text
            inputField.text = sampleQuestionText.text;
        }

        private async void GetNewSampleQuestion() 
        {
            // Create a follow-up question request
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = "Create a follow-up question for this message chain. Do not output any other information, just the question. Limit question to 6 words or less."
            };

            messages.Add(newMessage);
            var response = await SendMessageToOpenAI(messages);
            if (response != null)
            {
                sampleQuestionText.text = response;
                AdjustSize();
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt!!");
            }

            messages.RemoveAt(messages.Count - 1); // Remove the follow-up question request
        }

        private async Task<string> SendMessageToOpenAI(List<ChatMessage> messages)
        {
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4o",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                string content = message.Content as string;

                return content?.Trim();
            }
            return null;
        }

        private void AppendMessage(ChatMessage message)
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
        }

        private void CreateMessage()
        {
            // Create a new message based on user input
            if (imageInputField.text != "")
            {
                AddMessageWithImage();
            }
            else
            {
                AddTextMessage();
            }
            SendReply();
        }

        private void AddMessageWithImage()
        {
            var content = new List<ContentItem>
            {
                new ContentItem { Type = "text", Text = inputField.text },
                new ContentItem { Type = "image_url", image_url = new ImageUrl { Url = imageInputField.text } }
            };

            var newMessage = new ChatMessage { Role = "user", Content = content };
            messages.Add(newMessage);

            var displayMessage = new ChatMessage { Role = "user", Content = inputField.text };
            AppendMessage(displayMessage);
            imageInputField.text = "";
        }

        private void AddTextMessage()
        {
            var newMessage = new ChatMessage { Role = "user", Content = inputField.text };
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + " " + inputField.text;
            newMessage.Content += " " + explanationDictionary[(int)explanationSlider.value];
            newMessage.Content += " " + detailsDictionary[(int)detailsSlider.value];
            newMessage.Content += " " + responseDictionary[(int)responseSlider.value];

            messages.Add(newMessage);
        }

        private async void SendReply()
        {         
            // Send user message to OpenAI and get reply
            ToggleInputFields(false);

            var response = await SendMessageToOpenAI(messages);
            if (response != null)
            {
                var replyMessage = new ChatMessage { Role = "assistant", Content = response };
                messages.Add(replyMessage);
                AppendMessage(replyMessage);
                GetNewSampleQuestion();
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt....");
            }

            ToggleInputFields(true);
        }

        private void ToggleInputFields(bool isEnabled)
        {
            button.enabled = isEnabled;
            inputField.enabled = isEnabled;
            if (isEnabled) inputField.text = "";
        }

        void DropdownValueChanged(Dropdown change)
        {
            // Handle dropdown value change
            string content = change.value switch
            {
                1 => "Simplify your previous response and make it more understandable.",
                2 => "Add more details to your previous response.",
                3 => "Explain what led you to provide this answer",
                _ => ""
            };

            var newMessage = new ChatMessage { Role = "user", Content = content };
            AppendMessage(newMessage);     
            messages.Add(newMessage);
            SendReply();
        }
    }
}
