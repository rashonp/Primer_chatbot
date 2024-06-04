using UnityEngine;
using UnityEngine.UI;

public class ActionButtons : MonoBehaviour
{
    [SerializeField] private GameObject textInputView;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;

    [SerializeField] private GameObject chatPanel;
    [SerializeField] private Button chatPanelButton;

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button gamePanelButton;

    [SerializeField] private GameObject insertImagePanel;
    [SerializeField] private Button addImageButton;
    [SerializeField] private Button imagePanelCloseButton;
    [SerializeField] private Button takePictureButton;
    [SerializeField] private RawImage imageDisplay;

    public int maxSize = 512; // Maximum size for captured image
    private Texture2D capturedTexture; // Variable to store the captured image

    private void Start()
    {
        settingsButton.onClick.AddListener(ToggleSettings);
        chatPanelButton.onClick.AddListener(ToggleChatPanel);
        gamePanelButton.onClick.AddListener(ToggleGamePanel);
        addImageButton.onClick.AddListener(ToggleInsertImagePanel);
        imagePanelCloseButton.onClick.AddListener(CloseInsertImagePanel);
        takePictureButton.onClick.AddListener(OnTakePictureButtonClick);
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ToggleInsertImagePanel()
    {
        insertImagePanel.SetActive(!insertImagePanel.activeSelf);
    }

    public void CloseInsertImagePanel()
    {
        insertImagePanel.SetActive(false);
    }

    public void ToggleChatPanel()
    {
        chatPanel.SetActive(true);
        gamePanel.SetActive(false);
        textInputView.SetActive(true);
    }

    public void ToggleGamePanel()
    {
        gamePanel.SetActive(true);
        chatPanel.SetActive(false);
        textInputView.SetActive(false);
    }

    // Call this method when the "Take Picture" button is clicked
    public void OnTakePictureButtonClick()
    {
        if (NativeCamera.IsCameraBusy())
            return;

        TakePicture(maxSize);
        print("Take Picture Button Clicked");
    }

    // Call this method when the "Record Video" button is clicked
    public void OnRecordVideoButtonClick()
    {
        if (NativeCamera.IsCameraBusy())
            return;

        RecordVideo();
    }

    private void TakePicture(int maxSize)
    {
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {
            if (path != null)
            {
                // Load the captured image into a Texture2D
                capturedTexture = NativeCamera.LoadImageAtPath(path, maxSize);

                if (capturedTexture != null)
                {
                    // Display the captured image on the UI RawImage component
                    imageDisplay.texture = capturedTexture;
                }
            }
        }, maxSize);

        Debug.Log("Permission result: " + permission);
    }

    private void RecordVideo()
    {
        NativeCamera.Permission permission = NativeCamera.RecordVideo((path) =>
        {
            // Your existing code for handling video recording
        });

        Debug.Log("Permission result: " + permission);
    }
}
