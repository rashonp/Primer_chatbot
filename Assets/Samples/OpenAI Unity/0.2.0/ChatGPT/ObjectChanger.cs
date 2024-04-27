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

        private string prompt = @"You are an intelligent agent, your purpose is to provide structured input for function calling. Given a function name and parameters";

        private List<Tool> tools = new List<Tool>();
        void Start()
        {
            var tool = new Tool()
            {
                Type = "function", // replace with actual tool type
                Function = new ToolFunction
                {
                    Name = "change_object_color",
                    Description = "Change the color of an object.",
                    Parameters = new Parameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Property>
                        {
                            {
                                "color", new Property
                                {
                                    Type = "string",
                                    Description = "The color to change the object to."
                                }
                            }
                        },
                        Required = new List<string> { "color" }
                    }
                }
            };

            var tool2 = new Tool()
            {
                Type = "function", // replace with actual tool type
                Function = new ToolFunction
                {
                    Name = "change_object_rotation_and_speed",
                    Description = "Changes the rotation axis and speed of the object.",
                    Parameters = new Parameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Property>
                        {
                            {
                                "rotation", new Property
                                {
                                    Type = "string",
                                    Description = "The new rotation to give to the object."
                                }
                            },
                            {
                                "speed", new Property
                                {
                                    Type = "number",
                                    Description = "The new rotation speed to give to the object."
                                }
                            }
                        },
                        Required = new List<string> { "rotation", "speed" }
                    } 
                }
            };

                    // Add the tool to the tools list
            tools.Add(tool);
            tools.Add(tool2);

            button.onClick.AddListener(CreateMessage);
        }

        void Update()
        {
            // Apply rotation based on current axis and speed
            RotateObject();
        }

        private void CreateMessage()
        {
            if (messages.Count == 0)
            {
                var staterMessage = new ChatMessage()
                {
                    Role = "system",
                    Content = @"You are an intelligent agent, your purpose is to generate parameters given a user input. Do you best to map the user input to a function. Ask for clarification if a user request is ambiguous."
                };
                messages.Add(staterMessage);
            }

            var newMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = inputField.text
                };

            messages.Add(newMessage);
            Debug.Log(messages);
            SendReply();
        }

        private async void SendReply()
        {
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            var response_message = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                // Model = "gpt-3.5-turbo-0613"
                Model = "gpt-4-turbo",
                Messages = messages,
                Tools = tools
            });

            if (response_message.Choices != null && response_message.Choices.Count > 0)
            {
                var message = response_message.Choices[0].Message;
                var tool_calls = response_message.Choices[0].Message.ToolCalls;

                // tools call message
                var toolsCallMessage = new ChatMessage()
                {
                    Role = "assistant",
                    Content = "",
                    ToolCalls = tool_calls
                };
                messages.Add(toolsCallMessage);
                // message.Content = message.Content.Trim();
                if (tool_calls != null && tool_calls.Count > 0)
                {
                    for (int i = 0; i < tool_calls.Count; i++)
                    {
                        Debug.LogWarning(tool_calls[i].Function.Name);
                        Debug.LogWarning(tool_calls[i].Function.Arguments);
                        parameters parameters = JsonUtility.FromJson<parameters>(tool_calls[i].Function.Arguments);
                        Debug.Log(JsonUtility.ToJson(parameters));





                        var result = "";
                        if (tool_calls[i].Function.Name == "change_object_color")
                        {
                            result = ChangeShapeColor(parameters.color);
                        }
                        else if (tool_calls[i].Function.Name == "change_object_rotation_and_speed")
                        {
                            result = ChangeRotation(parameters.rotation, parameters.speed);
                        }

                        var newMessage = new ChatMessage()
                        {
                            ToolCallId = tool_calls[i].Id,
                            Role = "tool",
                            Name = tool_calls[i].Function.Name,
                            Content = result
                        };
                        messages.Add(newMessage);
                    }
                    var response_message2 = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
                    {
                        Model = "gpt-4-turbo",
                        Messages = messages,
                    });
                    if (response_message2.Choices != null && response_message2.Choices.Count > 0)
                    {
                        var message2 = response_message2.Choices[0].Message;
                        shapeChangerText.text = message2.Content;
                    }
                    else
                    {
                        Debug.LogWarning("No text was generated from this prompt.");
                        shapeChangerText.text = "No text was generated from this prompt.";
                    }
                }
                else
                {
                    Debug.LogWarning("No tool calls were generated from this prompt.");
                    shapeChangerText.text = "No tool calls were generated from this prompt.";
                }
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
                case "orange":
                    return new Color(1.0f, 0.5f, 0.0f); // Orange is not a built-in color, so we create it manually
                case "purple":
                    return new Color(0.5f, 0.0f, 0.5f); // Purple is not a built-in color, so we create it manually
                case "pink":
                    return new Color(1.0f, 0.0f, 1.0f); // Pink is not a built-in color, so we create it manually
                case "brown":
                    return new Color(0.6f, 0.3f, 0.0f); // Brown is not a built-in color, so we create it manually
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

        private string ChangeShapeColor(string colorName)
        {
            var result = "";
            // Get the Renderer component from the objectToChange
            Renderer renderer = objectToChange.GetComponent<Renderer>();
            
            // Check if the Renderer exists to avoid errors
            if (renderer != null)
            {
                // Set the main material's color to the parsed color
                renderer.material.color = GetColorFromString(colorName);
                result = "Color changed to " + colorName;
            }
            else
            {
                result = "No Renderer component found on the object.";
            }

            return result;
        }
        
        void RotateObject()
        {
            if (objectToChange != null)
            {
                objectToChange.transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }
        }
        // Function to change the rotation axis and speed
        public string ChangeRotation(string newAxis, float newSpeed)
        {
            string result = "";
            switch (newAxis)
            {
                case "Vector3.right":
                    rotationAxis = Vector3.right;
                    result = "Rotation axis changed to Vector3.right";
                    break;
                case "Vector3.up":
                    rotationAxis = Vector3.up;
                    result = "Rotation axis changed to Vector3.up";
                    break;
                case "Vector3.forward":
                    rotationAxis = Vector3.forward;
                    result = "Rotation axis changed to Vector3.forward";
                    break;
                default:
                    result = "Rotation axis not recognized";
                    break;
            }
            rotationSpeed = newSpeed;
            result += $"\nRotation speed changed to {newSpeed} degrees per second";
            return result;
        }
    }
}
