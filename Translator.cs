using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Translator : MonoBehaviour
{
    public static Translator Instance;

    [Header("Font Assets")]
    public TMP_FontAsset defaultFont;   // Standard (English, French, etc.)
    public TMP_FontAsset cjkFont;       // SimHei (Chinese, Japanese, Korean)

    [Header("State")]
    public string currentLanguage = "Russian";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Initial setup
        UpdateAllTexts();
    }

    public void SetLanguage(int index)
    {
        string[] languages = { "Russian", "English", "German", "French", "Spanish", "Portuguese", "Japanese", "Korean", "Chinese" };
        if (index >= 0 && index < languages.Length)
        {
            currentLanguage = languages[index];
            UpdateAllTexts();
        }
    }

    public void UpdateAllTexts()
    {
        // Find all localized components
        Transtable_Text[] allTexts = FindObjectsByType<Transtable_Text>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            ApplyFont(text.GetComponent<TextMeshProUGUI>());
        }

        // Find all localized dropdowns
        Transtable_Dropdown[] allDropdowns = FindObjectsByType<Transtable_Dropdown>(FindObjectsSortMode.None);
        foreach (var dropdown in allDropdowns)
        {
            dropdown.RefreshLocalizedOptions();
        }
    }

    public void ApplyFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        // Is it an Asian language?
        bool isCJK = currentLanguage == "Japanese" || currentLanguage == "Korean" || currentLanguage == "Chinese";

        if (isCJK && cjkFont != null)
        {
            textComponent.font = cjkFont;
        }
        else if (defaultFont != null)
        {
            textComponent.font = defaultFont;
        }
    }

    // Helper for strings
    public string GetTranslation(string key)
    {
        // Simple mock dictionary (In real app, load from JSON)
        if (key == "Quality") {
            if (currentLanguage == "Russian") return "Качество";
            if (currentLanguage == "English") return "Quality";
            return key;
        }
        return key;
    }
}
