using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Dropdown dropdown;

    void Start()
    {
        // Make sure to add this listener in the Start or Awake function
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    // This function is called when the Dropdown value changes
    void DropdownValueChanged(Dropdown change)
    {
        Debug.Log("New Value Selected: " + change.value);
        // You can also access change.options[change.value].text to get the display text
    }
}
