using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.8.0
/// Zenith Self-Healing UI & Ultimate Hover Effect for Gameplay Pause Scene
/// Специализированный скрипт для интерактивных кнопок в меню паузы (GamePause_Manager).
/// Автоматически меняет курсор мыши при наведении на изящную стрелку/указатель,
/// воспроизводит утонченный звук наведения и сочный клик при нажатии через SettingsManager и UIButtonSfxBinder.
/// </summary>
public class UIButtonPauseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("🖱️ Настройки курсора")]
    [Tooltip("Кастомная текстура курсора при наведении. Если пусто, плавно обнаружит глобальную текстуру.")]
    public Texture2D hoverCursor;
    
    [Tooltip("Активная точка позиционирования курсора")]
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("🎵 Кастомные звуковые волны (Опционально)")]
    [Tooltip("Переопределяемый звук клика. Если не задан, мгновенно подхватит глобальный sfx-клип из UIButtonSfxBinder.")]
    public AudioClip customClickSound;
    [Tooltip("Переопределяемый звук наведения (hover)")]
    public AudioClip customHoverSound;

    [Header("✨ Анимация упругости")]
    [Tooltip("Динамическое масштабирование размеров кнопки при наведении")]
    public bool animateScale = true;
    public float scaleFactor = 1.05f;
    public float animationSpeed = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button buttonComponent;
    private Image buttonImage;
    private Color originalColor;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        if (isInitialized) return;

        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonComponent = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) InitializeComponent();
        
        // Сбрасываем масштаб при активации элемента
        if (animateScale)
        {
            transform.localScale = originalScale;
            targetScale = originalScale;
        }
    }

    private void Update()
    {
        if (animateScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
        }
    }

    private void OnDisable()
    {
        ResetCursor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) InitializeComponent();
        if (buttonComponent != null && !buttonComponent.interactable) return;

        // 1. Смена текстуры курсора на кастомный
        if (hoverCursor != null)
        {
            Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            Texture2D globalCursor = TryFindGlobalHoverCursor();
            if (globalCursor != null)
            {
                Cursor.SetCursor(globalCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        // 2. Воспроизведение звука наведения (Hover Sfx)
        PlayHoverSfx();

        // 3. Плавный масштаб и подсветка
        if (animateScale)
        {
            targetScale = originalScale * scaleFactor;
        }

        if (buttonImage != null)
        {
            buttonImage.color = new Color(
                Mathf.Clamp01(originalColor.r + 0.08f), 
                Mathf.Clamp01(originalColor.g + 0.08f), 
                Mathf.Clamp01(originalColor.b + 0.12f), 
                originalColor.a
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetCursor();

        if (animateScale)
        {
            targetScale = originalScale;
        }

        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (buttonComponent != null && !buttonComponent.interactable) return;

        // 1. Если задан уникальный клип, играем его
        if (customClickSound != null)
        {
            PlaySoundFromAnywhere(customClickSound);
            return;
        }

        // 2. Иначе извлекаем из глобального системного синглтона UIButtonSfxBinder
        var sfxBinder = UIButtonSfxBinder.Instance;
        if (sfxBinder == null)
        {
            sfxBinder = FindFirstObjectByType<UIButtonSfxBinder>();
        }

        if (sfxBinder != null)
        {
            bool isBack = CheckIsBackButton(gameObject);
            AudioClip clipToPlay = isBack ? sfxBinder.backClickSound : sfxBinder.clickSound;

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

    private void PlayHoverSfx()
    {
        if (customHoverSound != null)
        {
            PlaySoundFromAnywhere(customHoverSound);
            return;
        }

        // Вызов через централизованную систему SettingsManager
        var settings = SettingsManager.Instance;
        if (settings == null)
        {
            settings = FindFirstObjectByType<SettingsManager>();
        }

        if (settings != null)
        {
            settings.PlayHoverSound(0);
        }
    }

    private void PlaySoundFromAnywhere(AudioClip clip)
    {
        if (clip == null) return;

        // Находим стабильный, никогда не выключающийся во время геймплея игровой объект (хост)
        GameObject stableHost = null;

        if (FateContinent.GamePause_Manager.Instance != null)
        {
            stableHost = FateContinent.GamePause_Manager.Instance.gameObject;
        }
        else if (SettingsManager.Instance != null)
        {
            stableHost = SettingsManager.Instance.gameObject;
        }
        else
        {
            // Если менеджер паузы не найден, пробуем использовать главную камеру
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                stableHost = mainCam.gameObject;
            }
        }

        // Если стабильный хост не определен, используем текущий объект как резерв
        if (stableHost == null)
        {
            stableHost = gameObject;
        }

        // Получаем или создаем AudioSource на стабильном хосте
        AudioSource hostSource = stableHost.GetComponent<AudioSource>();
        if (hostSource == null)
        {
            hostSource = stableHost.AddComponent<AudioSource>();
        }

        // Убеждаемся, что AudioSource включен
        hostSource.enabled = true;

        // Настраиваем AudioSource для идеального 2D звучания во время внутриигровой паузы
        hostSource.spatialBlend = 0f;          // Полный 2D Стерео (слышно везде на полную мощность)
        hostSource.playOnAwake = false;
        hostSource.ignoreListenerPause = true;   // Продолжить воспроизведение, даже если игра на паузе или Listener остановлен
        hostSource.mute = false;

        // Корректно считываем сохраненный уровень звуковых эффектов
        float sVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
        if (sVolume < 0.05f && PlayerPrefs.HasKey("SoundVolume"))
        {
            // Если звук выключен в ноль, сохраняем настройку пользователя
            hostSource.volume = sVolume;
        }
        else
        {
            // Иначе воспроизводим с приятной громкостью
            hostSource.volume = sVolume > 0.05f ? sVolume : 0.75f;
        }

        // Безопасное воспроизведение на активном объекте
        if (hostSource.gameObject.activeInHierarchy && hostSource.enabled)
        {
            hostSource.PlayOneShot(clip);
            Debug.Log($"[FATE PAUSE AUDIO] Стабильный 2D AudioSource на '{stableHost.name}' успешно воспроизвел клип '{clip.name}' для кнопки '{gameObject.name}'. Громкость: {hostSource.volume}");
        }
        else
        {
            // Резервный вариант PlayClipAtPoint, если хост почему-то неактивен в иерархии
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, hostSource.volume);
            Debug.LogWarning($"[FATE PAUSE AUDIO] Предупреждение: стабильный хост '{stableHost.name}' неактивен. Использован PlayClipAtPoint для клипа '{clip.name}'.");
        }
    }

    private Texture2D TryFindGlobalHoverCursor()
    {
        try
        {
            // Пытаемся найти текстуру курсора из других компонентов на сцене
            foreach (var mono in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mono != null)
                {
                    string className = mono.GetType().Name;
                    if (className == "UIButtonHoverEffect" || className == "UIButtonSelectionHover")
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
        if (objName.Contains("back") || objName.Contains("exit") || objName.Contains("close") || objName.Contains("return") || objName.Contains("cancel") || objName.Contains("назад") || objName.Contains("выход") || objName.Contains("no") || objName.Contains("нет"))
        {
            return true;
        }

        var texts = go.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var txt in texts)
        {
            string t = txt.text.ToLower();
            if (t.Contains("back") || t.Contains("exit") || t.Contains("close") || t.Contains("назад") || t.Contains("выход") || t.Contains("отмена") || t.Contains("return") || t.Contains("нет"))
            {
                return true;
            }
        }
        return false;
    }
}
