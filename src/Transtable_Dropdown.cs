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
        if (dropdown == null || optionIDs == null || optionIDs.Length == 0) return;

        // Определяем нужный шрифт
        TMP_FontAsset activeFont = null;
        if (Translator.Instance != null)
        {
            int lang = Translator.LanguageID;
            // Упрощенная логика: если не русский и нет дефолтного - берем русский (SimHei)
            if (lang == 1) 
            {
                activeFont = Translator.Instance.russianFont;
            }
            else 
            {
                activeFont = Translator.Instance.defaultFont != null ? Translator.Instance.defaultFont : Translator.Instance.russianFont;
            }
        }

        // Настраиваем основной заголовок (Selected Value)
        if (dropdown.captionText != null)
        {
            dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap; 
            dropdown.captionText.overflowMode = TextOverflowModes.Ellipsis; 
            if (activeFont != null) dropdown.captionText.font = activeFont;
        }

        // Настраиваем шаблон элементов (List Items)
        if (dropdown.itemText != null)
        {
            dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap; 
            dropdown.itemText.overflowMode = TextOverflowModes.Ellipsis; 
            if (activeFont != null) dropdown.itemText.font = activeFont;
        }

        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (i < optionIDs.Length)
            {
                dropdown.options[i].text = Translator.GetText(optionIDs[i]);
            }
        }
        dropdown.RefreshShownValue(); 
    }

    void OnDestroy()
    {
        Translator.DeleteDropdown(this);
    }
}
