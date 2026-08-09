using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки сцен")]
    [Tooltip("Название сцен выбора персонажа или начальной локации")]
    public string characterSelectionSceneName = "CharacterSelection";
    [Tooltip("Индекс сцен в настройках сборки (по умолчанию 1)")]
    public int characterSelectionSceneIndex = 1;
    [Tooltip("Загрузка может произойти по имени (true) или по индексу (false)")]
    public bool loadByName = false;

    [Header("Элементы Анимации Главного Экрана")]
    [Tooltip("Заголовок игры (для покачивания или анимации)")]
    public RectTransform gameTitleText;
    [Tooltip("CanvasGroup панели главного меню для плавного появления")]
    public CanvasGroup mainMenuCanvasGroup;

    [Header("Параллакс Фонового Рисунка")]
    [Tooltip("Слой фонового рисунка для параллакса")]
    public RectTransform backgroundLayer;
    [Tooltip("Сила смещения фона при движении мыши")]
    public float parallaxStrength = 25f;
    [Tooltip("Скорость плавного сглаживания параллакса")]
    public float smoothSpeed = 4f;

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

    private Vector2 targetBackgroundPos;
    private Vector2 initialBackgroundPos;
    private bool cycleDirectionUp = true;

    private void Start()
    {
        // Инициализация прозрачности фонов на старте
        UpdateBackgroundBlending();

        // Плавное появление панели главного меню на старте
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInMenu());
        }

        if (backgroundLayer != null)
        {
            initialBackgroundPos = backgroundLayer.anchoredPosition;
        }
    }

    private void Update()
    {
        // Интерактивный параллакс-эффект на основе движения мыши
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

            float mouseX = (mousePos.x / Screen.width) - 0.5f;
            float mouseY = (mousePos.y / Screen.height) - 0.5f;

            targetBackgroundPos = initialBackgroundPos + new Vector2(mouseX * parallaxStrength, mouseY * parallaxStrength);
            backgroundLayer.anchoredPosition = Vector2.Lerp(backgroundLayer.anchoredPosition, targetBackgroundPos, Time.deltaTime * smoothSpeed);
        }

        // Плавный цикл смены дня и ночи
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

    private IEnumerator FadeInMenu()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainMenuCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        mainMenuCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Запуск игры. Интегрировано с системным просмотром загрузки
    /// </summary>
    public void PlayGame()
    {
        Debug.Log($"[FATE CORE] Запуск загрузки сцен. Способность: {(loadByName ? "По имени" : "По индексу")}.");

        if (loadByName)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneName);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneName);
            }
        }
        else
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneIndex);
            }
        }
    }

    /// <summary>
    /// Открытие панели настроек (звук, музыка, язык)
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("[FATE CORE] Открытие панели настроек главного меню.");
        
        // Безопасный вызов через рефлексию, чтобы избежать ошибок компиляции, если Menu_Game временно отсутствует
        System.Type menuGameType = System.Type.GetType("Menu_Game");
        if (menuGameType != null)
        {
            var instanceProp = menuGameType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var method = menuGameType.GetMethod("OnClickSettingsButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(instance, null);
                        return;
                    }
                }
            }
        }
        
        Debug.LogWarning("[FATE CORE] Menu_Game не найден в проекте или его Instance равен null. Настройки не могут быть открыты.");
    }

    /// <summary>
    /// Выход из игры. Поддерживает работу в редакторе и в готовой сборке
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[FATE CORE] Запрос на выход из игры...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
