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

    [Tooltip("Принудительно делать текст жирным (Bold) для русского языка")]
    public bool boldForRussian = true;

    private TMP_Dropdown dropdown;
    private TMP_FontAsset originalCaptionFont;
    private TMP_FontAsset originalItemFont;
    private bool isLocalUpdating = false;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            if (dropdown.captionText != null) originalCaptionFont = dropdown.captionText.font;
            if (dropdown.itemText != null) originalItemFont = dropdown.itemText.font;
        }
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
        if (isLocalUpdating) return;
        isLocalUpdating = true;

        try
        {
            if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown == null || Translator.Instance == null) return;

            bool oldIsUpdating = false;
            if (SettingsManager.Instance != null)
            {
                oldIsUpdating = SettingsManager.Instance.isUpdatingSettings;
                SettingsManager.Instance.isUpdatingSettings = true;
            }

            try
            {
                // Сохраняем оригинальные шрифты при первом обновлении, если Awake еще не отработал
                if (originalCaptionFont == null && dropdown.captionText != null) originalCaptionFont = dropdown.captionText.font;
                if (originalItemFont == null && dropdown.itemText != null) originalItemFont = dropdown.itemText.font;

                int lang = Translator.LanguageID;
                TMP_FontAsset font = Translator.Instance.defaultFont;
                TMP_FontAsset itemFont = Translator.Instance.defaultFont;

                // Для сохранения оригинальных шрифтов, если это русский или английский и оригинальные шрифты заданы.
                // Для турецкого (2), корейского (7), китайского (8/6) принудительно используем шрифты с полной поддержкой символов.
                if (lang == 0 || lang == 1)
                {
                    if (originalCaptionFont != null) font = originalCaptionFont;
                    if (originalItemFont != null) itemFont = originalItemFont;
                }
                else if (lang == 7) 
                {
                    font = Translator.Instance.koreanFont;
                    itemFont = Translator.Instance.koreanFont;
                }
                else if (lang == 8 || lang == 6) 
                {
                    font = Translator.Instance.chineseFont;
                    itemFont = Translator.Instance.chineseFont;
                }
                float charSpacing = 0f;
                charSpacing = 0f; // Сбрасываем межбуквенный интервал, чтобы русский и турецкий помещались идеально

                // Если включена опция boldForRussian и активный язык русский, делаем текст жирным (Bold)
                FontStyles style = (boldForRussian && lang == 0) ? FontStyles.Bold : FontStyles.Normal;

                if (dropdown.captionText != null)
                {
                    dropdown.captionText.font = font;
                    dropdown.captionText.characterSpacing = charSpacing;
                    dropdown.captionText.wordSpacing = 0;
                    dropdown.captionText.alignment = TextAlignmentOptions.Center;
                    dropdown.captionText.fontStyle = style;
                    dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
                    dropdown.captionText.overflowMode = TextOverflowModes.Overflow;
                }

                if (dropdown.itemText != null)
                {
                    dropdown.itemText.font = itemFont;
                    dropdown.itemText.characterSpacing = charSpacing;
                    dropdown.itemText.wordSpacing = 0;
                    dropdown.itemText.alignment = TextAlignmentOptions.Center;
                    dropdown.itemText.fontStyle = style;
                    dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap;
                    dropdown.itemText.overflowMode = TextOverflowModes.Overflow;
                }

                // Применяем перевод по ID или используем автоопределение
                if (gameObject.name.ToLower().Contains("lang") || gameObject.name.ToLower().Contains("language"))
                {
                    if (dropdown.options.Count != 3)
                    {
                        dropdown.ClearOptions();
                        dropdown.AddOptions(new List<string> { "Русский", "English", "Türkçe" });
                    }
                    else
                    {
                        dropdown.options[0].text = "Русский";
                        dropdown.options[1].text = "English";
                        dropdown.options[2].text = "Türkçe";
                    }
                }
                else if (translations.optionTextIDs != null && translations.optionTextIDs.Length > 0)
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
                    // AUTO-DETECT Logic for other types
                    string lowerName = gameObject.name.ToLower();
                    
                    if (dropdown.options.Count == 6) // Quality List (ID 37-42)
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
            finally
            {
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.isUpdatingSettings = oldIsUpdating;
                }
            }
        }
        finally
        {
            isLocalUpdating = false;
        }
    }
}
