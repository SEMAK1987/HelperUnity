using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LanguageSelector : MonoBehaviour
{
    public TMP_Dropdown LanguageDropdown;

    void Start()
    {
        if (LanguageDropdown != null)
        {
            LanguageDropdown.ClearOptions();
            List<string> options = new List<string> { "Русский", "English", "Deutsch", "Français", "Español", "Português", "日本語", "한국어", "简体中文" }; 
            LanguageDropdown.AddOptions(options);
            LanguageDropdown.value = PlayerPrefs.GetInt("Language", 0);
            LanguageDropdown.onValueChanged.AddListener(SetLanguage);
        }
    }

    public void SetLanguage(int index)
    {
        Translator.SelectLanguage(index);
    }
}
