using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Transtable_Text : MonoBehaviour
{
    public int TextID;
    [HideInInspector] public TextMeshProUGUI UIText;

    void Awake()
    {
        UIText = GetComponent<TextMeshProUGUI>();
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
        if (UIText == null) UIText = GetComponent<TextMeshProUGUI>();
        
        if (UIText != null)
        {
            UIText.text = Translator.GetText(TextID);
        }
    }

    // Метод для вызова из инспектора при изменении ID
    private void OnValidate()
    {
        if (UIText == null) UIText = GetComponent<TextMeshProUGUI>();
        if (UIText != null) UIText.text = Translator.GetText(TextID);
    }
}
