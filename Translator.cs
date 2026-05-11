using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Translator : MonoBehaviour
{
    public static Translator Instance;

    [Header("Font Assets (Triple Bridge)")]
    public TMP_FontAsset defaultFont;   // Standard (English, etc.)
    public TMP_FontAsset chineseFont;   // SimHei
    public TMP_FontAsset koreanFont;    // Malgun Gothic / Noto Sans KR

    [Header("Data")]
    public int LanguageID = 0; 
    public string currentLanguage = "Russian";

    private List<Transtable_Text> textComponents = new List<Transtable_Text>();
    private List<Transtable_Dropdown> dropdownComponents = new List<Transtable_Dropdown>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    public static void Add(Transtable_Text comp) { if (Instance != null && !Instance.textComponents.Contains(comp)) Instance.textComponents.Add(comp); }
    public static void Remove(Transtable_Text comp) { if (Instance != null && Instance.textComponents.Contains(comp)) Instance.textComponents.Remove(comp); }
    
    public static void AddDropdown(Transtable_Dropdown comp) { if (Instance != null && !Instance.dropdownComponents.Contains(comp)) Instance.dropdownComponents.Add(comp); }
    public static void DeleteDropdown(Transtable_Dropdown comp) { if (Instance != null && Instance.dropdownComponents.Contains(comp)) Instance.dropdownComponents.Remove(comp); }

    public static void SelectLanguage(int index)
    {
        if (Instance != null) Instance.DoSelectLanguage(index);
    }

    public void DoSelectLanguage(int index)
    {
        string[] languages = { "Russian", "English", "German", "French", "Spanish", "Portuguese", "Japanese", "Korean", "Chinese" };
        if (index >= 0 && index < languages.Length)
        {
            LanguageID = index;
            currentLanguage = languages[index];
            Update_texts();
        }
    }

    public static void SetLanguage(int index) => SelectLanguage(index);

    public static void Update_texts()
    {
        if (Instance != null) Instance.DoUpdateTexts();
    }

    public void DoUpdateTexts()
    {
        foreach (var text in textComponents)
        {
            if (text != null) text.Refresh();
        }

        foreach (var dropdown in dropdownComponents)
        {
            if (dropdown != null) dropdown.RefreshLocalizedOptions();
        }
    }

    public void ApplyFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        textComponent.characterSpacing = 0;
        textComponent.wordSpacing = 0;
        textComponent.lineSpacing = 0;

        if (currentLanguage == "Chinese" || currentLanguage == "Japanese")
        {
            if (chineseFont != null) textComponent.font = chineseFont;
        }
        else if (currentLanguage == "Korean")
        {
            if (koreanFont != null) textComponent.font = koreanFont;
            else if (chineseFont != null) textComponent.font = chineseFont; 
        }
        else
        {
            if (defaultFont != null) textComponent.font = defaultFont;
        }
    }

    public string GetTranslation(int id)
    {
        // Placeholder for real localization logic
        return "Translated_" + id;
    }
}
