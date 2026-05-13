using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Transtable_Dropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    void OnEnable()
    {
        Translator.AddDropdown(this);
        UpdateDropdown();
    }

    void OnDisable()
    {
        Translator.DeleteDropdown(this);
    }

    public void UpdateDropdown()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null) return;

        // Reset spacing for Asian fonts
        if (dropdown.captionText != null)
        {
            dropdown.captionText.characterSpacing = 0;
            dropdown.captionText.wordSpacing = 0;
            dropdown.captionText.alignment = TextAlignmentOptions.Left;
        }

        if (dropdown.itemText != null)
        {
            dropdown.itemText.characterSpacing = 0;
            dropdown.itemText.wordSpacing = 0;
            dropdown.itemText.alignment = TextAlignmentOptions.Left;
        }

        // AUTO-DETECT Logic
        if (dropdown.options.Count == 9) // Language List
        {
            string[] langs = { "English", "Русский", "Deutsch", "Français", "Español", "Português", "日本語", "한국어", "简体中文" };
            for (int i = 0; i < 9; i++) dropdown.options[i].text = langs[i];
        }
        else if (dropdown.options.Count == 6) // Quality List (ID 37-42)
        {
            for (int i = 0; i < 6; i++) dropdown.options[i].text = Translator.GetText(37 + i);
        }
        else if (dropdown.options.Count == 2) // Full Screen (ID 44-45)
        {
            dropdown.options[0].text = Translator.GetText(44); // Yes/Да
            dropdown.options[1].text = Translator.GetText(45); // No/Нет
        }

        dropdown.RefreshShownValue();
    }
}
