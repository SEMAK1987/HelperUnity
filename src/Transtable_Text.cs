using UnityEngine;
using TMPro;

public class Transtable_Text : MonoBehaviour
{
    public int TextID;
    public TextMeshProUGUI UIText;

    void Awake()
    {
        if (UIText == null) UIText = GetComponent<TextMeshProUGUI>();
        if (UIText == null) UIText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (Translator.Instance != null) 
        { 
            Translator.Add(this); 
        }
    }

    void Start()
    {
        if (Translator.Instance != null) Translator.Update_texts();
    }

    void OnEnable() { if (Translator.Instance != null) { Translator.Add(this); Translator.Update_texts(); } }
    void OnDisable() { if (Translator.Instance != null) Translator.Delete(this); }
    void OnDestroy() { if (Translator.Instance != null) Translator.Delete(this); }
}
