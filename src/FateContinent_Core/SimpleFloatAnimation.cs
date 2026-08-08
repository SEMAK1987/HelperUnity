using UnityEngine;
using UnityEngine.UI;

public class SimpleFloatAnimation : MonoBehaviour
{
    [Header("Настройки Парения (Position)")]
    public bool enableFloating = true;
    public float amplitude = 3f; // Размах парения
    public float speed = 0.5f;   // Скорость парения
    public bool useWorldSpace = false;

    [Header("Настройки Пульсации Свечения (Glow / Alpha)")]
    public bool enableAlphaPulsing = false;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1.0f;
    public float pulseSpeed = 2.0f;

    [Header("Настройки Пульсации Размера (Scale)")]
    public bool enableScalePulsing = false;
    public float minScale = 0.95f;
    public float maxScale = 1.05f;
    public float scaleSpeed = 1.5f;

    private Vector3 startPos;
    private Vector3 startScale;
    private RectTransform rectTransform;
    private Image targetImage;
    private CanvasGroup targetCanvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
            startPos = rectTransform.anchoredPosition;
        else
            startPos = transform.localPosition;

        startScale = transform.localScale;
        targetImage = GetComponent<Image>();
        targetCanvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // 1. Парение по оси Y
        if (enableFloating)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
            
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
            }
            else
            {
                if (useWorldSpace)
                    transform.position = new Vector3(startPos.x, newY, startPos.z);
                else
                    transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
            }
        }

        // 2. Пульсация прозрачности (Glow Alpha) для Zenith-эффекта
        if (enableAlphaPulsing)
        {
            float lerp = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // Нормализация в диапазон [0, 1]
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, lerp);

            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.alpha = currentAlpha;
            }
            else if (targetImage != null)
            {
                Color color = targetImage.color;
                color.a = currentAlpha;
                targetImage.color = color;
            }
        }

        // 3. Пульсация размера (Scale) для интерактивных кнопок/масок
        if (enableScalePulsing)
        {
            float lerp = (Mathf.Sin(Time.time * scaleSpeed) + 1f) / 2f;
            float currentScale = Mathf.Lerp(minScale, maxScale, lerp);
            transform.localScale = startScale * currentScale;
        }
    }
}
