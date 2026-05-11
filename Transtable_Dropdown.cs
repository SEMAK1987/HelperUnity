using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(TMP_Dropdown))]
public class Transtable_Dropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    
    [Header("Localization Keys")]
    public List<string> optionKeys = new List<string>();

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        RefreshLocalizedOptions();
    }

    public void RefreshLocalizedOptions()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
        if (Translator.Instance == null) return;

        // Save current value to restore after update
        int currentValue = dropdown.value;

        // Localize the main Label
        if (dropdown.captionText != null)
        {
            Translator.Instance.ApplyFont(dropdown.captionText as TextMeshProUGUI);
        }

        // Localize the Items in the list
        if (dropdown.itemText != null)
        {
            Translator.Instance.ApplyFont(dropdown.itemText as TextMeshProUGUI);
        }

        // IMPORTANT: If we manually change options, we need to ensure the list is correct
        // If the user has custom labels, we don't want to see "Start"
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            // If you have a key-based system, localizing here:
            // string key = optionKeys.Count > i ? optionKeys[i] : dropdown.options[i].text;
            // dropdown.options[i].text = Translator.Instance.GetTranslation(key);
        }

        dropdown.RefreshShownValue();
    }
}
