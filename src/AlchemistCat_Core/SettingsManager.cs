#pragma warning disable 0618
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Менеджер глобальных настроек, звука, музыки, лимитера кадров (защита от перегрева GPU) и локализации.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI Компоненты (Назначаются на сцене)")]
    public Slider soundSlider;
    public Slider musicSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;
    public Toggle fullscreenToggle;

    [Header("Аудио Смеситель")]
    public AudioMixer masterMixer;

    [Header("Источники Аудио")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Клипы эффектов и музыки")]
    [SerializeField] private AudioClip[] hoverSounds;
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private AudioClip[] menuPlaylist;
    [SerializeField] private AudioClip[] labPlaylist;
    [SerializeField] private AudioClip[] minigamePlaylist;

    private List<Resolution> resolutionsList = new List<Resolution>();
    private int currentPlaylistIndex = 0;
    private AudioClip[] activePlaylist;
    public bool isUpdatingSettings = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Если этот объект является дочерним, открепляем его для правильной работы DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.parent = null;
            }

            DontDestroyOnLoad(gameObject);

            // Принудительно запускаем интеллектуальную автокалибровку аудио источников
            CalibrateAudioSources();

            // Настраиваем маршрутизацию в микшер для точной работы слайдеров громкости
            RouteSourcesToMixer();

            InitializeSettings();
        }
        else if (Instance != this)
        {
            // Передаем новые UI ссылки в существующий инстанс
            Instance.soundSlider = this.soundSlider;
            Instance.musicSlider = this.musicSlider;
            Instance.qualityDropdown = this.qualityDropdown;
            Instance.resolutionDropdown = this.resolutionDropdown;
            Instance.languageDropdown = this.languageDropdown;
            Instance.fullscreenToggle = this.fullscreenToggle;

            // Останавливаем любые запущенные AudioSource на дубликате
            AudioSource[] duplicateSources = GetComponentsInChildren<AudioSource>(true);
            foreach (var src in duplicateSources)
            {
                if (src != null)
                {
                    src.Stop();
                }
            }

            Instance.BindUIElements();
            Destroy(gameObject); // Полностью уничтожаем дублирующий объект во избежание конфликтов
        }
    }

    private void Start()
    {
        BindUIElements();
        PlayThemeForActiveScene();
    }

    private void CalibrateAudioSources()
    {
        // Полностью отключено, чтобы Unity использовала только ручные ссылки из Inspector
    }

    private void RouteSourcesToMixer()
    {
        // Отключено перераспределение групп микшера, чтобы громкость менялась строго через AudioSource.volume
    }

    private void AutoCalibratePlaylistsAndSounds()
    {
        // Оставляем только базовую подстраховку для кликов
        if (clickSounds == null || clickSounds.Length == 0)
        {
            Debug.LogWarning("[ALCHEMIST AUDIO] Массив звуков клика пуст. Назначьте аудиоклип Click в Inspector.");
        }
    }

    private void InitializeSettings()
    {
        // Запускаем автокалибровку аудиоканалов
        AutoCalibratePlaylistsAndSounds();

        // 1. Загрузка громкости
        float sVol = PlayerPrefs.GetFloat("Vol_SFX", PlayerPrefs.GetFloat("SoundVolume", 0.75f));
        float mVol = PlayerPrefs.GetFloat("Vol_Music", PlayerPrefs.GetFloat("MusicVolume", 0.5f));
        SetSFXVolume(sVol);
        SetMusicVolume(mVol);

        // 2. Лимит кадров для защиты от перегрева (v18.11.16 Safeguard)
        int quality = PlayerPrefs.GetInt("QualitySetting", 2); // Среднее по умолчанию
        ApplyQualitySafeguards(quality);

        // 3. Восстановление разрешения
        bool isFull = PlayerPrefs.GetInt("FullscreenMode", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFull;
    }

    public void BindUIElements()
    {
        isUpdatingSettings = true;
        try
        {
            // 1. Привязка Слайдера Звука (строго по ссылке из Inspector)
            if (soundSlider != null)
            {
                soundSlider.value = PlayerPrefs.GetFloat("Vol_SFX", 0.75f);
                ClearPersistentListeners(soundSlider.onValueChanged);
                soundSlider.onValueChanged.RemoveAllListeners();
                soundSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            // 2. Привязка Слайдера Музыки (строго по ссылке из Inspector)
            if (musicSlider != null)
            {
                musicSlider.value = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
                ClearPersistentListeners(musicSlider.onValueChanged);
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            // 3. Выпадающий список Графики
            if (qualityDropdown != null)
            {
                Transtable_Dropdown transDD = qualityDropdown.GetComponent<Transtable_Dropdown>();
                if (transDD == null)
                {
                    transDD = qualityDropdown.gameObject.AddComponent<Transtable_Dropdown>();
                }
                
                transDD.translations.optionTextIDs = new int[] { 37, 38, 39, 40, 41, 42 };

                qualityDropdown.value = PlayerPrefs.GetInt("QualitySetting", 2);
                ClearPersistentListeners(qualityDropdown.onValueChanged);
                qualityDropdown.onValueChanged.RemoveAllListeners();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
                
                transDD.UpdateDropdown();
            }

            // 4. Полноэкранный режим
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = PlayerPrefs.GetInt("FullscreenMode", Screen.fullScreen ? 1 : 0) == 1;
                ClearPersistentListeners(fullscreenToggle.onValueChanged);
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            // 5. Выпадающий список Языков (Строго 3 нативных языка)
            if (languageDropdown != null)
            {
                Transtable_Dropdown transDD = languageDropdown.GetComponent<Transtable_Dropdown>();
                if (transDD != null)
                {
                    DestroyImmediate(transDD);
                }

                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new List<string> { "Русский", "English", "Türkçe" });

                AutoCalibrateDropdown(languageDropdown, 55f, 200f, 22f);

                languageDropdown.value = PlayerPrefs.GetInt("Alchemist_Language", 0);
                ClearPersistentListeners(languageDropdown.onValueChanged);
                languageDropdown.onValueChanged.RemoveAllListeners();
                languageDropdown.onValueChanged.AddListener(SetLanguage);
                languageDropdown.RefreshShownValue();
            }

            BuildResolutionsList();
            UpdateDropdownFontStyles();
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    private void BuildResolutionsList()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        resolutionsList.Clear();

        Resolution[] systemResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < systemResolutions.Length; i++)
        {
            string option = systemResolutions[i].width + " x " + systemResolutions[i].height;
            options.Add(option);
            resolutionsList.Add(systemResolutions[i]);

            if (systemResolutions[i].width == Screen.currentResolution.width &&
                systemResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void TrySetMixerFloat(string parameterName, float volValue)
    {
        // Заглушка: очищено для предотвращения ошибок консоли
    }

    public void SetSFXVolume(float val)
    {
        if (isUpdatingSettings) return;
        
        // Сохраняем значение
        PlayerPrefs.SetFloat("Vol_SFX", val);

        // Меняем громкость строго на источнике звуковых эффектов
        if (sfxSource != null)
        {
            sfxSource.volume = val;
        }
    }

    public void SetMusicVolume(float val)
    {
        if (isUpdatingSettings) return;
        
        // Сохраняем значение
        PlayerPrefs.SetFloat("Vol_Music", val);

        // Меняем громкость строго на источнике фоновой музыки
        if (musicSource != null)
        {
            musicSource.volume = val;
        }
    }

    public void SetQuality(int index)
    {
        if (isUpdatingSettings) return;

        isUpdatingSettings = true;
        try
        {
            QualitySettings.SetQualityLevel(index);
            PlayerPrefs.SetInt("QualitySetting", index);
            ApplyQualitySafeguards(index);
            Debug.Log($"[ALCHEMIST SETTINGS] Успешно установлено качество графики: {index}");
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    private void ApplyQualitySafeguards(int qualityLevel)
    {
        // Лимитер кадров для спасения видеокарт (GPU Anti-Overheat)
        switch (qualityLevel)
        {
            case 0: // Очень Низкое
                Application.targetFrameRate = 30;
                break;
            case 1: // Низкое
                Application.targetFrameRate = 30;
                break;
            case 2: // Среднее
                Application.targetFrameRate = 60;
                break;
            case 3: // Высокое
                Application.targetFrameRate = 60;
                break;
            case 4: // Очень Высокое
                Application.targetFrameRate = 120;
                break;
            case 5: // Ультра
                Application.targetFrameRate = 120;
                break;
            default:
                Application.targetFrameRate = 60;
                break;
        }
        Debug.Log($"[ALCHEMIST SETTINGS] Лимит кадров установлен на {Application.targetFrameRate} FPS (Качество: {qualityLevel})");
    }

    public void SetFullscreen(bool isFull)
    {
        if (isUpdatingSettings) return;
        isUpdatingSettings = true;
        try
        {
            Screen.fullScreen = isFull;
            PlayerPrefs.SetInt("FullscreenMode", isFull ? 1 : 0);
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    public void SetResolution(int index)
    {
        if (isUpdatingSettings) return;
        isUpdatingSettings = true;
        try
        {
            if (index >= 0 && index < resolutionsList.Count)
            {
                Resolution res = resolutionsList[index];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionIndex", index);
            }
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    public void SetLanguage(int index)
    {
        if (isUpdatingSettings) return;

        isUpdatingSettings = true;
        try
        {
            Translator.SelectLanguage(index);
            Debug.Log($"[ALCHEMIST SETTINGS] Успешно установлен язык: {index} (0=RU, 1=EN, 2=TR)");
            UpdateDropdownFontStyles();
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    /// <summary>
    /// Автоматическое обновление стиля шрифта выпадающих списков при смене языка.
    /// Русский язык получает жирное начертание (Bold) для лучшей читаемости и плотности.
    /// </summary>
    public void UpdateDropdownFontStyles()
    {
        int lang = PlayerPrefs.GetInt("Alchemist_Language", 0);
        TMPro.FontStyles style = (lang == 0) ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;

        // Принудительно устанавливаем правильный шрифт локализации, поддерживающий все символы (включая турецкий и русский)
        if (Translator.Instance != null && Translator.Instance.defaultFont != null)
        {
            if (languageDropdown != null)
            {
                if (languageDropdown.captionText != null) languageDropdown.captionText.font = Translator.Instance.defaultFont;
                if (languageDropdown.itemText != null) languageDropdown.itemText.font = Translator.Instance.defaultFont;
            }
            if (qualityDropdown != null)
            {
                if (qualityDropdown.captionText != null) qualityDropdown.captionText.font = Translator.Instance.defaultFont;
                if (qualityDropdown.itemText != null) qualityDropdown.itemText.font = Translator.Instance.defaultFont;
            }
            if (resolutionDropdown != null)
            {
                if (resolutionDropdown.captionText != null) resolutionDropdown.captionText.font = Translator.Instance.defaultFont;
                if (resolutionDropdown.itemText != null) resolutionDropdown.itemText.font = Translator.Instance.defaultFont;
            }
        }

        if (languageDropdown != null)
        {
            if (languageDropdown.captionText != null) languageDropdown.captionText.fontStyle = style;
            if (languageDropdown.itemText != null) languageDropdown.itemText.fontStyle = style;
            languageDropdown.RefreshShownValue();
        }

        if (qualityDropdown != null)
        {
            if (qualityDropdown.captionText != null) qualityDropdown.captionText.fontStyle = style;
            if (qualityDropdown.itemText != null) qualityDropdown.itemText.fontStyle = style;
            qualityDropdown.RefreshShownValue();
        }

        if (resolutionDropdown != null)
        {
            if (resolutionDropdown.captionText != null) resolutionDropdown.captionText.fontStyle = style;
            if (resolutionDropdown.itemText != null) resolutionDropdown.itemText.fontStyle = style;
            resolutionDropdown.RefreshShownValue();
        }
    }

    /// <summary>
    /// Автоматическая калибровка выпадающего списка TMP_Dropdown.
    /// </summary>
    private void AutoCalibrateDropdown(TMP_Dropdown dropdown, float itemHeight, float templateHeight, float fontSize)
    {
        if (dropdown == null) return;

        // Принудительно устанавливаем правильный шрифт локализации, поддерживающий все символы (включая турецкий и русский)
        if (Translator.Instance != null && Translator.Instance.defaultFont != null)
        {
            if (dropdown.captionText != null) dropdown.captionText.font = Translator.Instance.defaultFont;
            if (dropdown.itemText != null) dropdown.itemText.font = Translator.Instance.defaultFont;
        }

        // 1. Настройка основного текста (Label) на самой кнопке
        if (dropdown.captionText != null)
        {
            dropdown.captionText.fontSize = fontSize;
            dropdown.captionText.alignment = TextAlignmentOptions.Center;
            dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.captionText.overflowMode = TextOverflowModes.Overflow;
            dropdown.captionText.characterSpacing = 0f;
            dropdown.captionText.wordSpacing = 0f;
        }

        // 2. Настройка текста внутри элементов списка (Item Label)
        if (dropdown.itemText != null)
        {
            dropdown.itemText.fontSize = fontSize - 2f;
            dropdown.itemText.alignment = TextAlignmentOptions.Center;
            dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.itemText.overflowMode = TextOverflowModes.Overflow;
            dropdown.itemText.characterSpacing = 0f;
            dropdown.itemText.wordSpacing = 0f;
            dropdown.itemText.color = new Color(0.12f, 0.12f, 0.12f, 1f); // Темно-серый цвет для отличной читаемости на светлом фоне
        }

        // 3. Безопасная настройка размеров Template и Item с правильной версткой
        Transform templateTransform = dropdown.transform.Find("Template");
        if (templateTransform != null)
        {
            RectTransform templateRect = templateTransform.GetComponent<RectTransform>();
            if (templateRect != null)
            {
                // Рассчитываем динамическую высоту шторки на основе реального количества опций
                int optionCount = dropdown.options != null ? dropdown.options.Count : 3;
                float spacingVal = 2f;
                float paddingTotal = 16f; // Включает отступы сверху и снизу шторки
                float dynamicHeight = (optionCount * itemHeight) + ((optionCount - 1) * spacingVal) + paddingTotal;
                
                if (dynamicHeight > 360f) dynamicHeight = 360f; // Ограничиваем разумным максимумом для экранов высокой плотности

                templateRect.sizeDelta = new Vector2(templateRect.sizeDelta.x, dynamicHeight);
            }

            Transform viewport = templateTransform.Find("Viewport");
            if (viewport != null)
            {
                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    // Удаляем ContentSizeFitter на Content, так как он конфликтует с внутренним кодом позиционирования TMP_Dropdown и вызывает баги схлопывания (пустые белые шторки и улет наверх)
                    ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
                    if (csf != null)
                    {
                        Destroy(csf);
                    }

                    // Настраиваем VerticalLayoutGroup для принудительного контроля высоты элементов
                    VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.childControlHeight = true;
                        vlg.childControlWidth = true;
                        vlg.childForceExpandHeight = false;
                        vlg.childForceExpandWidth = true;
                        vlg.spacing = 2f;
                        vlg.padding = new RectOffset(0, 0, 8, 8); // 8px отступы сверху и снизу шторки, чтобы нижний элемент не врезался в рамку
                    }

                    // Настраиваем высоту эталонного элемента Item
                    Transform item = content.Find("Item");
                    if (item != null)
                    {
                        RectTransform itemRect = item.GetComponent<RectTransform>();
                        if (itemRect != null)
                        {
                            itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemHeight);
                        }

                        // Настраиваем LayoutElement элемента
                        LayoutElement itemLayout = item.GetComponent<LayoutElement>();
                        if (itemLayout == null) itemLayout = item.gameObject.AddComponent<LayoutElement>();
                        itemLayout.preferredHeight = itemHeight;
                        itemLayout.minHeight = itemHeight;

                        // Корректируем размеры Item Label, чтобы текст занимал всю высоту строки и не обрезался по вертикали
                        Transform itemLabel = item.Find("Item Label");
                        if (itemLabel != null)
                        {
                            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
                            if (itemLabelRect != null)
                            {
                                itemLabelRect.anchorMin = Vector2.zero;
                                itemLabelRect.anchorMax = Vector2.one;
                                itemLabelRect.offsetMin = new Vector2(30f, 0f); // Зазор под чекбокс слева
                                itemLabelRect.offsetMax = new Vector2(-15f, 0f);
                            }
                        }
                    }
                }
            }
        }
    }

    // Воспроизведение звуков
    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSounds != null && hoverSounds.Length > 0)
        {
            AudioClip clip = hoverSounds[Random.Range(0, hoverSounds.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSounds != null && clickSounds.Length > 0)
        {
            AudioClip clip = clickSounds[Random.Range(0, clickSounds.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- СОВМЕСТИМОСТЬ С FATE CONTINENT ---
    public void PlayHoverSound(int index)
    {
        PlayHoverSound();
    }

    public void PlaySound(AudioClip clip)
    {
        PlaySoundEffect(clip);
    }

    public void PlaySfx(AudioClip clip)
    {
        PlaySoundEffect(clip);
    }

    public void PlaySfx(string sfxName)
    {
        PlaySFX(sfxName);
    }

    public void PlaySFX(string sfxName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + sfxName);
        if (clip == null) clip = Resources.Load<AudioClip>(sfxName);
        if (clip != null) PlaySoundEffect(clip);
    }

    public void PlayMusicTrack(int playlistIndex, int trackIndex)
    {
        switch (playlistIndex)
        {
            case 0: ChangePlaylist(menuPlaylist); break;
            case 1: ChangePlaylist(menuPlaylist); break;
            case 2: ChangePlaylist(labPlaylist); break;
            case 3: ChangePlaylist(minigamePlaylist); break;
        }
    }

    public void BindLoadedUIElements()
    {
        BindUIElements();
    }
    // --------------------------------------

    // Воспроизведение фоновой музыки по плейлистам
    public void PlayThemeForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("menu") || sceneName.Contains("title"))
        {
            ChangePlaylist(menuPlaylist);
        }
        else if (sceneName.Contains("lab") || sceneName.Contains("alchemy") || sceneName.Contains("game"))
        {
            ChangePlaylist(labPlaylist);
        }
        else
        {
            ChangePlaylist(minigamePlaylist);
        }
    }

    private void ChangePlaylist(AudioClip[] newPlaylist)
    {
        if (newPlaylist == null || newPlaylist.Length == 0) return;

        // ПРОВЕРКА: Если этот плейлист уже играет, ничего не делаем, чтобы избежать наложения звуков
        if (activePlaylist == newPlaylist && musicSource != null && musicSource.isPlaying)
        {
            return;
        }

        activePlaylist = newPlaylist;
        currentPlaylistIndex = 0;
        PlayPlaylistTrack();
    }

    private void PlayPlaylistTrack()
    {
        if (musicSource == null || activePlaylist == null || activePlaylist.Length == 0) return;

        AudioClip track = activePlaylist[currentPlaylistIndex];
        if (track != null)
        {
            // Принудительно отключаем проигрывание на SFX источнике
            if (sfxSource != null && sfxSource.clip == track)
            {
                sfxSource.Stop();
                sfxSource.clip = null;
            }

            // Запускаем музыку строго через musicSource
            musicSource.clip = track;
            musicSource.loop = true;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    private void ClearPersistentListeners(UnityEngine.Events.UnityEventBase unityEvent)
    {
        if (unityEvent == null) return;
        int count = unityEvent.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
        {
            unityEvent.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        }
    }
}
