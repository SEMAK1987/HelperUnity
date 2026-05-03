using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки")]
    public float scaleMultiplier = 1.15f;
    public float animationSpeed = 15f;
    public Texture2D hoverCursor;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color originalColor;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null) originalColor = buttonImage.color;
    }

    void Update()
    {
        // Плавное изменение размера
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
        if (buttonImage != null) buttonImage.color = new Color(originalColor.r + 0.15f, originalColor.g + 0.15f, originalColor.b + 0.25f, originalColor.a);
        
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        if (buttonImage != null) buttonImage.color = originalColor;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
