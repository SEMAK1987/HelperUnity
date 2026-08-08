using UnityEngine;
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

    private Vector2 targetBackgroundPos;
    private Vector2 initialBackgroundPos;

    private void Start()
    {
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
