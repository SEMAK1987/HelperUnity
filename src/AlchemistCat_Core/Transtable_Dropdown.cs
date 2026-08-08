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
        if (dropdown == null || translations.optionTextIDs == null || translations.optionTextIDs.Length == 0) return;

        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (i < translations.optionTextIDs.Length)
            {
                dropdown.options[i].text = Translator.GetText(translations.optionTextIDs[i]);
            }
        }
        dropdown.RefreshShownValue();
    }
}
