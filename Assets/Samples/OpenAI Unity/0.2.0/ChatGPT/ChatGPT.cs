using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Slider explanationSlider;
        [SerializeField] private Slider detailsSlider;
        [SerializeField] private Slider responseSlider;
        [SerializeField] private Button button;
        [SerializeField] private Dropdown messageOptions;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are an intelligent agent that can answer questions, solve problems, and provide information.";

        private Dictionary<int, string> explanationDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> detailsDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> responseDictionary = new Dictionary<int, string>();

        private void Start()
        {
            // button.GetComponent<Image>().color = Color.red;
            button.onClick.AddListener(CreateMessage);

            // Make sure to add this listener in the Start or Awake function
            messageOptions.onValueChanged.AddListener(delegate {
                DropdownValueChanged(messageOptions);
            });

            explanationDictionary.Add(0, "Answer as if you were an elementary school teacher talking to a child. Your response should be really simple and very easy to understand.");
            explanationDictionary.Add(1, "Answer as if you were a high school teach talking to a teenager. Your response should understandable for a high school student.");
            explanationDictionary.Add(2, "Answer as if you were a professor talking to a college student. Your response should be understandable for someone who has completed high school.");
            explanationDictionary.Add(3, "Answer as if you were talking to a grad student. Your response should be understandable for someone who has completed college.");
            explanationDictionary.Add(4, "Answer as if you were a leading expert in this field talking to colleague.");

            detailsDictionary.Add(0, "Your response should be very short and to the point. At most, it should be a few sentences.");
            detailsDictionary.Add(1, "Your response should be moderately detailed. It should be 3 to 5 sentences long.");
            detailsDictionary.Add(2, "Your response should be comprehensive. It should be over atleast 2 paragraphs long.");

            responseDictionary.Add(0, "");
            responseDictionary.Add(1, "Provide two different responses to the same question. Cleary separate and number the two responses.");
            responseDictionary.Add(2, "Provide three different responses to the same question. Cleary separate and number the three responses.");

            // explanationSlider.onValueChanged.AddListener(OnSliderValueChanged);
            // detailsSlider.onValueChanged.AddListener(OnSliderValueChanged);
            // responseSlider.onValueChanged.AddListener(OnSliderValueChanged);
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

            // var test = inputField.text + " " + explanationDictionary[(int)explanationSlider.value];
            // print(test);
            
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + " " + inputField.text; 
            newMessage.Content += " " + explanationDictionary[(int)explanationSlider.value];
            newMessage.Content += " " + detailsDictionary[(int)detailsSlider.value];
            newMessage.Content += " " + responseDictionary[(int)responseSlider.value];
            // print(detailsDictionary[(int)detailsSlider.value]);
            print(newMessage.Content);
             
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
