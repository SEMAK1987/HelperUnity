using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FateMainMenuTitleAnimator : MonoBehaviour
{
    [Header("Настройки парения (Floating)")]
    [Tooltip("Амплитуда движения по вертикали (Y)")]
    public float floatAmplitude = 12f;
    [Tooltip("Скорость изменения парения")]
    public float floatSpeed = 1.6f;

    [Header("Настройки дыхания (Scale Breathing)")]
    [Tooltip("Диапазон изменения размера (например, от 97% до 103%)")]
    public float scaleAmplitude = 0.03f;
    [Tooltip("Скорость общения")]
    public float scaleSpeed = 1.3f;

    [Header("Настройки сияния (Glow Lerp)")]
    [Tooltip("Включить плавное перелив цвета текста / свечения")]
    public bool enableGlowLerp = true;
    public Color glowColorStart = new Color(1f, 0.85f, 0.3f, 1f); // Теплый золотой
    public Color glowColorEnd = new Color(0.85f, 0.35f, 1f, 1f); // Магический пурпурный
    [Tooltip("Скорость перелива цветов")]
    public float glowSpeed = 1.8f;

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
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Плавное парение по синусоиде Y
        if (rectTransform != null)
        {
            float newY = startAnchoredPosition.y + Mathf.Sin(time * floatSpeed) * floatAmplitude;
            rectTransform.anchoredPosition = new Vector2(startAnchoredPosition.x, newY);

            // 2. Эффект мягкого дыхания (Scale)
            float scaleMultiplier = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmplitude;
            rectTransform.localScale = startScale * scaleMultiplier;
        }

        // 3. Мягкое переливание цвета текста и свечения в материале
        if (enableGlowLerp && titleText != null)
        {
            float t = (Mathf.Sin(time * glowSpeed) + 1f) * 0.5f; 
            Color lerpedColor = Color.Lerp(glowColorStart, glowColorEnd, t);
            
            titleText.color = lerpedColor;
            
            // Безопасно обновляем цвет обводки в материале, если он поддерживает это
            if (titleText.fontSharedMaterial != null)
            {
                titleText.fontSharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, lerpedColor * 0.4f);
            }
        }
    }
}
