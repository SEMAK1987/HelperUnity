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

    private void OnEnable()
    {
        if (Translator.Instance != null)
        {
            Translator.Instance.AddDropdown(this);
        }
    }

    private void OnDisable()
    {
        if (Translator.Instance != null)
        {
            Translator.Instance.DeleteDropdown(this);
        }
    }

    public void RefreshLocalizedOptions()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
        if (Translator.Instance == null) return;

        // Apply font to the main selection Label
        if (dropdown.captionText != null)
        {
            Translator.Instance.ApplyFont(dropdown.captionText as TextMeshProUGUI);
        }

        // Apply font to the Item template Label
        // IMPORTANT: We do NOT put Transtable_Text on the Item Label itself!
        if (dropdown.itemText != null)
        {
            Translator.Instance.ApplyFont(dropdown.itemText as TextMeshProUGUI);
        }

        dropdown.RefreshShownValue();
    }
}
