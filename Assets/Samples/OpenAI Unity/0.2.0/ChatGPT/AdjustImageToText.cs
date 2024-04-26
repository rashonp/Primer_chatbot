using UnityEngine;
using UnityEngine.UI;
using TMPro; // Namespace for TextMeshPro

[RequireComponent(typeof(RectTransform))]
public class AdjustImageToText : MonoBehaviour
{
    public RectTransform textRectTransform; // Assign this to your text's RectTransform
    [SerializeField] private InputField inputField;
    [SerializeField] private Button button;
    public Vector2 padding; // How much padding to add around the text

    private Image image;
    private RectTransform rectTransform;
    private TextMeshProUGUI textComponent; // TextMeshPro component
    

    // void Start()
    // {
    //     image = GetComponent<Image>();
    //     rectTransform = GetComponent<RectTransform>();
    //     textComponent = textRectTransform.GetComponent<TextMeshProUGUI>();

    //     // Define the initial text here
    //     string initialText = "What is quatum computing?";

    //     // Set the initial text
    //     textComponent.text = initialText;

    //     // Adjust the size of the image based on initial text
    //     AdjustSize();
    //     button.onClick.AddListener(UpdateInput);
    // }

    // void Update()
    // {
    //     // Potentially check if the text has changed to update the size accordingly
    //     // Note: It's more efficient to update only when text changes, not every frame.
    // }

    // private void AdjustSize()
    // {
    //     if (textRectTransform != null)
    //     {
    //         // Use the preferred values to accommodate for any dynamic resizing of text
    //         rectTransform.sizeDelta = new Vector2(textComponent.preferredWidth + padding.x, textComponent.preferredHeight + padding.y);
    //     }
    // }

    // private async void UpdateInput()
    // {
    //     inputField.text = textComponent.text;
    // }
}
