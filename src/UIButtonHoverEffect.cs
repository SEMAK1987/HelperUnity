using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки")]
    public float scaleMultiplier = 1.1f;
    public float animationSpeed = 15f;
    public bool useScaleAnimation = true;
    
    [Header("Курсор")]
    public Texture2D hoverCursor;
    public Vector2 hotspot = Vector2.zero;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color originalColor;
    private bool isInitialized = false;

    void Awake()
    {
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        if (isInitialized) return;
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null) originalColor = buttonImage.color;
        isInitialized = true;
    }

    void Update()
    {
        if (useScaleAnimation)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) InitializeEffect();
        if (useScaleAnimation) targetScale = originalScale * scaleMultiplier;
        if (buttonImage != null) buttonImage.color = new Color(originalColor.r + 0.1f, originalColor.g + 0.1f, originalColor.b + 0.15f, originalColor.a);
        
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useScaleAnimation) targetScale = originalScale;
        if (buttonImage != null) buttonImage.color = originalColor;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
