using UnityEngine;
using TMPro;

public class FateMainMenuTitleAnimator : MonoBehaviour
{
    [Header("Настройки парения (Floating)")]
    [Tooltip("Амплитуда движения по вертикали (Y)")]
    public float floatAmplitude = 15f;
    [Tooltip("Скорость колебаний парения")]
    public float floatSpeed = 1.5f;

    [Header("Настройки дыхания (Scale Breathing)")]
    [Tooltip("Диапазон изменения размера (например, 0.95 до 1.05)")]
    public float scaleAmplitude = 0.04f;
    [Tooltip("Скорость дыхания")]
    public float scaleSpeed = 1.2f;

    [Header("Настройки сияния (Glowing)")]
    [Tooltip("Включить циклическое изменение цвета или свечения текста")]
    public bool enableGlowLerp = true;
    public Color glowColorStart = new Color(1f, 0.85f, 0.4f, 1f); // Золотой
    public Color glowColorEnd = new Color(0.9f, 0.4f, 1f, 1f);    // Фиолетовый
    public float glowSpeed = 2f;

    private RectTransform rectTransform;
    private TextMeshProUGUI titleText;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        titleText = GetComponent<TextMeshProUGUI>();

        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }

        if (titleText == null)
        {
            Debug.LogWarning("[FateTitleAnimator] TextMeshProUGUI не найден на этом объекте! Эффект сияния будет недоступен.");
        }
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Парение по оси Y
        if (rectTransform != null)
        {
            float newY = startAnchoredPosition.y + Mathf.Sin(time * floatSpeed) * floatAmplitude;
            rectTransform.anchoredPosition = new Vector2(startAnchoredPosition.x, newY);

            // 2. Дыхание (Scale)
            float scaleMultiplier = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmplitude;
            rectTransform.localScale = startScale * scaleMultiplier;
        }

        // 3. Мягкое изменение цвета свечения (TMP)
        if (enableGlowLerp && titleText != null)
        {
            float t = (Mathf.Sin(time * glowSpeed) + 1f) * 0.5f; // Плавный спектр от 0 до 1
            Color lerpedColor = Color.Lerp(glowColorStart, glowColorEnd, t);
            
            // Настраиваем основной цвет текста или цвет свечения (Glow)
            titleText.color = lerpedColor;
            
            // Если включен Face/Outline в материале, мы можем изменять его свечение
            titleText.fontSharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, lerpedColor);
        }
    }
}
