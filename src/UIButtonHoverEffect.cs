using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки курсора")]
    public Texture2D hoverCursor; // Текстура для курсора при наведении
    public Vector2 hotspot = Vector2.zero;

    [Header("Визуальный эффект кнопки")]
    public float scaleMultiplier = 1.05f;
    public float animationSpeed = 10f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color originalColor;
    private Color hoverColor;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            hoverColor = new Color(originalColor.r + 0.1f, originalColor.g + 0.1f, originalColor.b + 0.2f, originalColor.a);
        }
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
        if (buttonImage != null) buttonImage.color = hoverColor;
        
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        if (buttonImage != null) buttonImage.color = originalColor;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
