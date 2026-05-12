using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Transtable_Dropdown : MonoBehaviour
{
    public int[] optionIDs; // Массив ID из Translator для каждого пункта
    private TMP_Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        Translator.AddDropdown(this);
        UpdateDropdown();
    }

    public void UpdateDropdown()
    {
        if (dropdown == null) return;
        if (Translator.Instance == null) return;
 
        int lang = Translator.LanguageID;
        TMP_FontAsset activeFont = null;
 
        // Font Mapping (Triple Bridge Sync)
        if (lang == 7) // Korean
        {
            activeFont = Translator.Instance.koreanFont != null ? Translator.Instance.koreanFont : Translator.Instance.chineseFont;
        }
        else if (lang == 8 || lang == 6) // Chinese / Japanese
        {
            activeFont = Translator.Instance.chineseFont;
        }
        else // Russian / European / Default
        {
            activeFont = Translator.Instance.defaultFont;
        }
 
        // Global fallback if everything is empty
        if (activeFont == null) activeFont = Translator.Instance.chineseFont;
 
        // Apply to Caption (the selected item shown in the actual dropdown box)
        if (dropdown.captionText != null)
        {
            if (activeFont != null) dropdown.captionText.font = activeFont;
            dropdown.captionText.characterSpacing = 0;
            dropdown.captionText.wordSpacing = 0;
            dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.captionText.overflowMode = TextOverflowModes.Ellipsis;
            dropdown.captionText.alignment = TextAlignmentOptions.Left; // Ensure left alignment for better look
        }
 
        // Apply to Template Items (the list that pops up)
        if (dropdown.itemText != null)
        {
            if (activeFont != null) dropdown.itemText.font = activeFont;
            dropdown.itemText.characterSpacing = 0;
            dropdown.itemText.wordSpacing = 0;
            dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.itemText.overflowMode = TextOverflowModes.Ellipsis;
            dropdown.itemText.alignment = TextAlignmentOptions.Left;
        }
 
        // Translate options if IDs are assigned in the inspector
        if (optionIDs != null && optionIDs.Length > 0)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (i < optionIDs.Length)
                {
                    dropdown.options[i].text = Translator.GetText(optionIDs[i]);
                }
            }
        }
        else if (dropdown.options.Count == 6) // Auto-detect Quality Dropdown
        {
            for (int i = 0; i < 6; i++)
            {
                dropdown.options[i].text = Translator.GetText(37 + i); // 37-42 are quality labels
            }
        }
        else if (dropdown.options.Count == 2) // Auto-detect Full Screen (Yes/No)
        {
            dropdown.options[0].text = Translator.GetText(44); // Yes
            dropdown.options[1].text = Translator.GetText(45); // No
        }
        else if (dropdown.options.Count == 9) // Auto-detect Language Dropdown
        {
            dropdown.options[0].text = "English";
            dropdown.options[1].text = "Русский";
            dropdown.options[2].text = "Deutsch";
            dropdown.options[3].text = "Français";
            dropdown.options[4].text = "Español";
            dropdown.options[5].text = "Português";
            dropdown.options[6].text = "日本語";
            dropdown.options[7].text = "한국어";
            dropdown.options[8].text = "简体中文";
        }
        
        dropdown.RefreshShownValue(); 
    }

    void OnDestroy()
    {
        Translator.DeleteDropdown(this);
    }
}
