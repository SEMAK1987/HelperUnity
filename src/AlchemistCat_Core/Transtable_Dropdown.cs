using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Автоматический мост локализации для выпадающих списков TMP_Dropdown.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class Transtable_Dropdown : MonoBehaviour
{
    [System.Serializable]
    public struct DropdownOptionTranslation
    {
        [Tooltip("Массив ID строк из Translator для каждой опции выпадающего списка")]
        public int[] optionTextIDs;
    }

    public DropdownOptionTranslation translations;

    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        Translator.AddDropdown(this);
        UpdateDropdown();
    }

    private void OnDisable()
    {
        Translator.DeleteDropdown(this);
    }

    public void UpdateDropdown()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null || Translator.Instance == null) return;

        int lang = Translator.LanguageID;
        TMP_FontAsset font = Translator.Instance.defaultFont;
        float charSpacing = 0f;

        if (lang == 7) font = Translator.Instance.koreanFont;
        else if (lang == 8 || lang == 6) font = Translator.Instance.chineseFont;
        charSpacing = 0f; // Сбрасываем межбуквенный интервал, чтобы русский и турецкий помещались идеально

        if (dropdown.captionText != null)
        {
            dropdown.captionText.font = font;
            dropdown.captionText.characterSpacing = charSpacing;
            dropdown.captionText.wordSpacing = 0;
            dropdown.captionText.alignment = TextAlignmentOptions.Left;
        }

        if (dropdown.itemText != null)
        {
            dropdown.itemText.font = font;
            dropdown.itemText.characterSpacing = charSpacing;
            dropdown.itemText.wordSpacing = 0;
            dropdown.itemText.alignment = TextAlignmentOptions.Left;
        }

        // Применяем перевод по ID или используем автоопределение
        if (translations.optionTextIDs != null && translations.optionTextIDs.Length > 0)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (i < translations.optionTextIDs.Length)
                {
                    dropdown.options[i].text = Translator.GetText(translations.optionTextIDs[i]);
                }
            }
        }
        else
        {
            // AUTO-DETECT Logic
            if (dropdown.options.Count == 9) // Language List
            {
                string[] langs = { "Русский", "English", "Deutsch", "Français", "Español", "Português", "日本語", "한국어", "简体中文" };
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
        }

        dropdown.RefreshShownValue();
    }
}
