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

    private void Start()
    {
        settingsButton.onClick.AddListener(ToggleSettings);
        chatPanelButton.onClick.AddListener(ToggleChatPanel);
        gamePanelButton.onClick.AddListener(ToggleGamePanel);
        addImageButton.onClick.AddListener(ToggleInsertImagePanel);
        imagePanelCloseButton.onClick.AddListener(CloseInsertImagePanel);
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
}
