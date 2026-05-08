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

        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (i < optionIDs.Length)
            {
                dropdown.options[i].text = Translator.GetText(optionIDs[i]);
            }
        }
        dropdown.RefreshShownValue(); // Обновляем текст на самой кнопке
    }

    void OnDestroy()
    {
        Translator.DeleteDropdown(this);
    }
}
