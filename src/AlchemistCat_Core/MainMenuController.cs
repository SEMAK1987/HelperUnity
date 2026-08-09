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

    [Header("Настройки Дня и Ночи (Day/Night Blending)")]
    [Tooltip("Картинка Дневного Фона (Day Background Image)")]
    public Image dayBackgroundImage;
    [Tooltip("Картинка Ночного Фона (Night Background Image)")]
    public Image nightBackgroundImage;
    [Tooltip("Включить автоматическую плавную смену суток в меню")]
    public bool autoCycleBackgrounds = true;
    [Tooltip("Скорость перехода (чем выше, тем быстрее меняются день и ночь)")]
    public float dayNightCycleSpeed = 0.5f;
    [Tooltip("Ручное смешивание (0 - чистый день, 1 - чистая ночь)")]
    [Range(0f, 1f)]
    public float dayNightBlendFactor = 0f;

    [Header("Настройки")]
    public float titleAnimSpeed = 3f;

    private Vector2 bgStartPos;
    private float titleTimer = 0f;
    private bool cycleDirectionUp = true;

    private void Start()
    {
        if (backgroundLayer != null)
        {
            bgStartPos = backgroundLayer.anchoredPosition;
        }

        // Инициализация прозрачности фонов на старте
        UpdateBackgroundBlending();

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

        // 3. Плавный цикл смены дня и ночи
        if (autoCycleBackgrounds)
        {
            if (cycleDirectionUp)
            {
                dayNightBlendFactor += Time.deltaTime * dayNightCycleSpeed;
                if (dayNightBlendFactor >= 1f)
                {
                    dayNightBlendFactor = 1f;
                    cycleDirectionUp = false;
                }
            }
            else
            {
                dayNightBlendFactor -= Time.deltaTime * dayNightCycleSpeed;
                if (dayNightBlendFactor <= 0f)
                {
                    dayNightBlendFactor = 0f;
                    cycleDirectionUp = true;
                }
            }
        }

        UpdateBackgroundBlending();
    }

    /// <summary>
    /// Обновляет прозрачность дневного и ночного слоев на основе dayNightBlendFactor (0 = чистый день, 1 = чистая ночь)
    /// </summary>
    public void UpdateBackgroundBlending()
    {
        if (dayBackgroundImage != null)
        {
            Color c = dayBackgroundImage.color;
            // Дневной фон плавно затухает от 1 до 0
            c.a = 1f - dayNightBlendFactor;
            dayBackgroundImage.color = c;
        }

        if (nightBackgroundImage != null)
        {
            Color c = nightBackgroundImage.color;
            // Ночной фон плавно проявляется от 0 до 1
            c.a = dayNightBlendFactor;
            nightBackgroundImage.color = c;
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
