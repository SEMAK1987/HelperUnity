using UnityEngine;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Автоматический мост локализации для надписей TextMeshPro.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class Transtable_Text : MonoBehaviour
{
    [Tooltip("ID текстовой строки в базе переводчика Translator (например, 0 - Старт, 1 - Продолжить...)")]
    public int TextID;

    [HideInInspector]
    public TextMeshProUGUI UIText;
    
    [HideInInspector]
    public TMP_FontAsset originalFont;

    private void Awake()
    {
        UIText = GetComponent<TextMeshProUGUI>();
        if (UIText != null)
        {
            originalFont = UIText.font;
        }
    }

    private void OnEnable()
    {
        Translator.Add(this);
        UpdateText();
    }

    private void OnDisable()
    {
        Translator.Delete(this);
    }

    public void UpdateText()
    {
        Translator.FormatText(this);
    }
}
