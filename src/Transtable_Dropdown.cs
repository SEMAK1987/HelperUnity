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
        }
 
        // Apply to Template Items (the list that pops up)
        if (dropdown.itemText != null)
        {
            if (activeFont != null) dropdown.itemText.font = activeFont;
            dropdown.itemText.characterSpacing = 0;
            dropdown.itemText.wordSpacing = 0;
            dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.itemText.overflowMode = TextOverflowModes.Ellipsis;
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
        
        dropdown.RefreshShownValue(); 
    }

    void OnDestroy()
    {
        Translator.DeleteDropdown(this);
    }
}
