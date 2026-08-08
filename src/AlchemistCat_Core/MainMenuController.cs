using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Вспомогательный контроллер главного меню для управления красивым
/// параллаксом, инициализацией и анимациями.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Элементы Анимации Главного Экрана")]
    public RectTransform gameTitleText;
    public CanvasGroup mainMenuCanvasGroup;

    [Header("Параллакс Фонового Рисунка")]
    public RectTransform backgroundLayer;
    public float parallaxStrength = 20f;

    [Header("Настройки")]
    public float titleAnimSpeed = 3f;

    private Vector2 bgStartPos;
    private float titleTimer = 0f;

    private void Start()
    {
        if (backgroundLayer != null)
        {
            bgStartPos = backgroundLayer.anchoredPosition;
        }

        // Плавное проявление меню
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInMenuCoroutine());
        }

        // Автоматически запускаем музыку меню через SettingsManager
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayThemeForActiveScene();
        }
    }

    private void Update()
    {
        // 1. Анимация парения заголовка (Легкое дыхание)
        if (gameTitleText != null)
        {
            titleTimer += Time.deltaTime * titleAnimSpeed;
            float offset = Mathf.Sin(titleTimer) * 12f;
            gameTitleText.anchoredPosition = new Vector2(gameTitleText.anchoredPosition.x, offset);
        }

        // 2. Интерактивный Параллакс фона за счет наклона мыши
        if (backgroundLayer != null)
        {
            Vector2 mousePos = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
            }
            else
            {
                mousePos = Input.mousePosition;
            }
#else
            mousePos = Input.mousePosition;
#endif

            float normX = (mousePos.x / Screen.width) - 0.5f;
            float normY = (mousePos.y / Screen.height) - 0.5f;

            Vector2 targetPos = bgStartPos + new Vector2(normX * parallaxStrength, normY * parallaxStrength);
            backgroundLayer.anchoredPosition = Vector2.Lerp(backgroundLayer.anchoredPosition, targetPos, Time.deltaTime * 5f);
        }
    }

    private System.Collections.IEnumerator FadeInMenuCoroutine()
    {
        float elapsed = 0f;
        float duration = 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainMenuCanvasGroup.alpha = elapsed / duration;
            yield return null;
        }
        mainMenuCanvasGroup.alpha = 1f;
    }
}
