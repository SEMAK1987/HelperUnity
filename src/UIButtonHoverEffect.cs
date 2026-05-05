using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки")]
    public float scaleMultiplier = 1.15f;
    public float animationSpeed = 15f;
    public bool useScaleAnimation = true; // Можно выключить движение тут
    
    [Header("Курсор")]
    public Texture2D hoverCursor;
    public Vector2 hotspot = Vector2.zero;
    public float alphaThreshold = 0.5f; // Порог прозрачности (0 - любая точка, 1 - только 100% непрозрачность)
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color originalColor;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonImage = GetComponent<Image>();
        UpdateAlphaThreshold();
    }

    public void UpdateAlphaThreshold()
    {
        if (buttonImage != null) 
        {
            originalColor = buttonImage.color;
            // Безопасная установка точности клика
            try {
                // ВАЖНО: Требует 'Read/Write Enabled' в настройках импорта спрайта!
                buttonImage.alphaHitTestMinimumThreshold = alphaThreshold; 
            } catch (System.Exception) {
                // Warning is muted to prevent console spam for non-readable textures
            }
        }
    }

    void Update()
    {
        // Плавное изменение размера только если включено
        if (useScaleAnimation)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useScaleAnimation) targetScale = originalScale * scaleMultiplier;
        if (buttonImage != null) buttonImage.color = new Color(originalColor.r + 0.15f, originalColor.g + 0.15f, originalColor.b + 0.25f, originalColor.a);
        
        try {
            if (hoverCursor != null)
                Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
        } catch (System.Exception e) {
            Debug.LogError("[FATE CORE] Ошибка установки курсора: " + e.Message);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useScaleAnimation) targetScale = originalScale;
        if (buttonImage != null) buttonImage.color = originalColor;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
