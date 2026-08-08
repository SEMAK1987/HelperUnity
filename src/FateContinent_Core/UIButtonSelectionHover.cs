using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.7.4
/// Скрипт для кнопок в сцене выбора персонажа.
/// Автоматически меняет курсор мыши при наведении (на красивый указатель как в меню)
/// и воспроизводит звуковые эффекты клика при нажатии через SettingsManager/UIButtonSfxBinder.
/// </summary>
public class UIButtonSelectionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("🖱️ Настройки курсора")]
    [Tooltip("Текстура кастомного курсора (перетащите сюда изображение курсора из проекта)")]
    public Texture2D hoverCursor;
    
    [Tooltip("Активная точка курсора (обычно Vector2.zero или кончик стрелки)")]
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("🎵 Настройки звуков (Опциональное переопределение)")]
    [Tooltip("Звук клика для этой конкретной кнопки. Если пусто, берется автоматически из всеобщего UIButtonSfxBinder.")]
    public AudioClip customClickSound;

    [Header("✨ Визуальный отклик (Опционально)")]
    [Tooltip("Увеличивать ли кнопку при наведении?")]
    public bool animateScale = true;
    public float scaleFactor = 1.05f;
    public float animationSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button buttonComponent;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonComponent = GetComponent<Button>();
    }

    private void Update()
    {
        if (animateScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }
    }

    private void OnDisable()
    {
        // Сбрасываем курсор на дефолтный, если кнопка неожиданно отключилась/скрылась
        ResetCursor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonComponent != null && !buttonComponent.interactable) return;

        // Меняем курсор
        if (hoverCursor != null)
        {
            Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            // Попытаемся извлечь из UIButtonHoverEffect в сцене динамически без прямой зависимости при компиляции
            Texture2D globalCursor = TryFindGlobalHoverCursor();
            if (globalCursor != null)
            {
                Cursor.SetCursor(globalCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        // Проигрываем легкий звук наведения через SettingsManager
        var settings = SettingsManager.Instance;
        if (settings == null)
        {
            settings = FindFirstObjectByType<SettingsManager>();
        }
        if (settings != null)
        {
            settings.PlayHoverSound(0);
        }

        // Визуально увеличиваем кнопку
        if (animateScale)
        {
            targetScale = originalScale * scaleFactor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetCursor();

        // Возвращаем исходный размер
        if (animateScale)
        {
            targetScale = originalScale;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (buttonComponent != null && !buttonComponent.interactable) return;

        // Воспроизводим звук клика
        if (customClickSound != null)
        {
            PlaySoundFromAnywhere(customClickSound);
            return;
        }

        // Если кастомного звука нет, тянем его из глобального UIButtonSfxBinder
        var sfxBinder = UIButtonSfxBinder.Instance;
        if (sfxBinder == null)
        {
            sfxBinder = FindFirstObjectByType<UIButtonSfxBinder>();
        }

        if (sfxBinder != null)
        {
            bool isBack = CheckIsBackButton(gameObject);
            AudioClip clipToPlay = isBack ? sfxBinder.backClickSound : sfxBinder.clickSound;
            
            // Если звук не задан в специальной ячейке, берем стандартный клик в качестве запасного
            if (clipToPlay == null)
            {
                clipToPlay = sfxBinder.clickSound;
            }

            if (clipToPlay != null)
            {
                PlaySoundFromAnywhere(clipToPlay);
            }
        }
    }

    private void PlaySoundFromAnywhere(AudioClip clip)
    {
        if (clip == null) return;

        // 1. Пытаемся воспроизвести через глобальный синглтон SettingsManager
        var settings = SettingsManager.Instance;
        if (settings == null)
        {
            settings = FindFirstObjectByType<SettingsManager>();
        }

        if (settings != null)
        {
            settings.PlaySoundEffect(clip);
            Debug.Log($"[FATE AUDIO] Воспроизведен клик через SettingsManager: {gameObject.name}");
            return;
        }

        // 2. Если SettingsManager вообще нет в сцене (запустили сцену выбора сразу в Editor),
        // создаем локальный временный/динамический AudioSource для теста клика
        AudioSource localSource = GetComponent<AudioSource>();
        if (localSource == null)
        {
            localSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Настраиваем громкость из PlayerPrefs (если сохранено)
        float soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
        localSource.volume = soundVolume;
        localSource.PlayOneShot(clip);
        Debug.Log($"[FATE AUDIO] Воспроизведен клик через локальный AudioSource (SettingsManager не найден): {gameObject.name}");
    }

    private Texture2D TryFindGlobalHoverCursor()
    {
        try
        {
            foreach (var mono in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mono != null && mono.GetType().Name == "UIButtonHoverEffect")
                {
                    var field = mono.GetType().GetField("hoverCursor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var val = field.GetValue(mono) as Texture2D;
                        if (val != null) return val;
                    }
                }
            }
        }
        catch (System.Exception) { }
        return null;
    }

    private void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private bool CheckIsBackButton(GameObject go)
    {
        string objName = go.name.ToLower();
        if (objName.Contains("back") || objName.Contains("exit") || objName.Contains("close") || objName.Contains("return") || objName.Contains("назад") || objName.Contains("выход"))
        {
            return true;
        }

        var texts = go.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var txt in texts)
        {
            string t = txt.text.ToLower();
            if (t.Contains("back") || t.Contains("exit") || t.Contains("close") || t.Contains("назад") || t.Contains("выход") || t.Contains("return"))
            {
                return true;
            }
        }
        return false;
    }
}
