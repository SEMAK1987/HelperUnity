using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Эффект плавного масштабирования и проигрывания звуков при наведении курсора на кнопки.
/// </summary>
public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Настройки анимации")]
    public float hoverScaleMultiplier = 1.05f;
    public float animationSpeed = 15f;

    [Header("Звуковые эффекты")]
    public bool playSfxOnHover = true;
    public bool playSfxOnClick = true;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered = false;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Плавная интерполяция размера для предотвращения резкого дергания
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = originalScale * hoverScaleMultiplier;

        if (playSfxOnHover && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        targetScale = originalScale; // Сброс размера при клике
        
        if (playSfxOnClick && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayClickSound();
        }
    }

    private void OnDisable()
    {
        // Сброс размера в исходное состояние при скрытии панели
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}
