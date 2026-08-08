using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Transtable_Text : MonoBehaviour
{
    public int TextID;
    [HideInInspector] public TextMeshProUGUI UIText;
    [System.NonSerialized] public TMP_FontAsset originalFont;

    void Awake()
    {
        UIText = this.GetComponent<TextMeshProUGUI>();
        if (UIText != null) originalFont = UIText.font;
    }

    void OnEnable()
    {
        Translator.Add(this);
        UpdateText();
    }

    void OnDisable()
    {
        Translator.Delete(this);
    }

    public void UpdateText()
    {
        if (UIText == null) UIText = this.GetComponent<TextMeshProUGUI>();
        
        if (UIText != null)
        {
            Translator.FormatText(this);
        }
    }

    // Метод для вызова из инспектора при изменении ID
    private void OnValidate()
    {
        if (UIText == null) UIText = this.GetComponent<TextMeshProUGUI>();
        if (UIText != null) UIText.text = Translator.GetText(TextID);
    }
}
