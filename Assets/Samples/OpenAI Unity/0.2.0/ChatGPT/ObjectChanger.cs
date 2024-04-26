using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace OpenAI
{
    [System.Serializable] // This attribute makes the class serializable by Unity's JSON utility
    public class FunctionJson
    {
        public string function;
        public parameters parameters;
    }

    [System.Serializable] // This attribute makes the class serializable by Unity's JSON utility
    public class parameters
    {
        public string color;
        public string rotation;
        public float speed;
    }

    public class ObjectChanger : MonoBehaviour
    {
        // Public field to assign the 3D object in the Unity Editor
        [SerializeField] private GameObject objectToChange;
        [SerializeField] private Button button;
        [SerializeField] private InputField inputField;
        [SerializeField] private Text shapeChangerText;

        private List<ChatMessage> messages = new List<ChatMessage>();
        private OpenAIApi openai = new OpenAIApi();

        private Vector3 rotationAxis = new Vector3(0, 1, 1).normalized; // Initial rotation axis
        private float rotationSpeed = 50.0f; // Initial rotation speed in degrees per second

        private string prompt = @"You are an intelligent agent, your purpose is to provide structured input for function calling. Given a function name and parameters, 
        you should provide a JSON object that represents the function call. Do you best to map the user input to a function. The user may not explicitly state the parameters,
        in this scenario generate parameters for them. For example, if the user just say 'increase the speed' and you should map that to the function 'change_object_rotation_and_speed' 
        and chose a speed parameter that is greater the one from the previous prompt and has the same rotation as the previous prompt. The function call should be in the following format. If no function matches the user prompt, 
        say ~No function call matches this request!~, otherwise only output the function call based on the function description.";

        private string function = @"
        ""functions"": 
        [
            {
                ""name"": ""change_object_color"",
                ""description"": ""Change the color of an object."",
                ""parameters"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""color"": {
                            ""type"": ""string"",
                            ""description"": ""The color to change the object to.""
                            ""enum"": [""red"", ""green"", ""blue"", ""yellow"", ""cyan"", ""magenta"", ""black"", ""white"", ""grey"", ""gray""]
                        },
                    },
                    ""required"": [""color""]
                }
            },
            {
                ""name"": ""change_object_rotation_and_speed"",
                ""description"": ""Changes the rotation axis and speed of the object."",
                ""parameters"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""rotation"": {
                            ""type"": ""Vector3"",
                            ""description"": ""The new rotation to give to the object.""
                            ""enum"": [""Vector3.right"", ""Vector3.up"", ""Vector3.forward""]
                        },
                        ""speed"": {
                            ""type"": ""float"",
                            ""description"": ""The new rotation speed to give to the object.""
                        },
                    },
                    ""required"": [""rotation"", ""speed""]
                }
            }
        ]
        ";

        void Start()
        {
            button.onClick.AddListener(CreateMessage);
        }

        void Update()
        {
            // Apply rotation based on current axis and speed
            RotateObject();
        }

        private void CreateMessage()
        {
            var newMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = inputField.text
                };

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text + "\n" + function;

            // newMessage.Content += "\n" + function;

            Debug.Log(newMessage.Content);
            // print(newMessage.Content);
                
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
                // Model = "gpt-3.5-turbo-0613",
                Model = "gpt-4-0613",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                // AppendMessage(message);
                shapeChangerText.text = message.Content;

                string noFunction = @"""No function call matches this request!""";
                

                bool areEqual = string.Equals(message.Content, "No function call matches this request!", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(message.Content, "~No function call matches this request!~", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(message.Content, noFunction, StringComparison.OrdinalIgnoreCase);
                if (!areEqual)
                {
                    FunctionJson functionJson = JsonUtility.FromJson<FunctionJson>(message.Content);
                    Debug.Log(JsonUtility.ToJson(functionJson));

                    if (functionJson.function == "change_object_color")
                    {
                        parameters parameters = functionJson.parameters;
                        string color = parameters.color;
                        ChangeShapeColor(color);
                    }
                    else if (functionJson.function == "change_object_rotation_and_speed")
                    {
                        parameters parameters = functionJson.parameters;
                        ChangeRotation(parameters.rotation, parameters.speed);
                    }
                }
                
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }

        Color GetColorFromString(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red":
                    return Color.red;
                case "green":
                    return Color.green;
                case "blue":
                    return Color.blue;
                case "yellow":
                    return Color.yellow;
                case "cyan":
                    return Color.cyan;
                case "magenta":
                    return Color.magenta;
                case "black":
                    return Color.black;
                case "white":
                    return Color.white;
                case "grey":
                case "gray": // Support both spellings
                    return Color.grey;
                default:
                    Debug.LogWarning($"Color '{colorName}' not recognized. Defaulting to white.");
                    return Color.white; // Default color if no match is found
            }
        }

        private void ChangeShapeColor(string colorName)
        {
            // Get the Renderer component from the objectToChange
            Renderer renderer = objectToChange.GetComponent<Renderer>();
            
            // Check if the Renderer exists to avoid errors
            if (renderer != null)
            {
                // Set the main material's color to the parsed color
                renderer.material.color = GetColorFromString(colorName);
            }
        }
        
        void RotateObject()
        {
            if (objectToChange != null)
            {
                objectToChange.transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }
        }
        // Function to change the rotation axis and speed
        public void ChangeRotation(string newAxis, float newSpeed)
        {
            switch (newAxis)
            {
                case "Vector3.right":
                    rotationAxis = Vector3.right;
                    break;
                case "Vector3.up":
                    rotationAxis = Vector3.up;
                    break;
                case "Vector3.forward":
                    rotationAxis = Vector3.forward;
                    break;
                default:
                    break;
            }
            rotationSpeed = newSpeed;
        }
    }
}
