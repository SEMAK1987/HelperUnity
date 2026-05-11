using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Transtable_Text : MonoBehaviour
{
    public int textID;
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (Translator.Instance != null)
        {
            Translator.Instance.Add(this);
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (Translator.Instance != null)
        {
            Translator.Instance.Remove(this);
        }
    }

    public void Refresh()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();
        if (Translator.Instance == null) return;

        // Apply correct font based on language
        Translator.Instance.ApplyFont(textMesh);
        
        // Update text content if ID is valid (optional, depends on your system)
        // string translatedValue = Translator.Instance.GetTranslation(textID);
        // if (!string.IsNullOrEmpty(translatedValue)) textMesh.text = translatedValue;
    }
}
