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
            if (gameObject.name != "ALCHEMIST_SETTINGS_MANAGER")
            {
                GameObject managerObject = new GameObject("ALCHEMIST_SETTINGS_MANAGER");
                SettingsManager customManager = managerObject.AddComponent<SettingsManager>();
                
                customManager.hoverSounds = this.hoverSounds;
                customManager.clickSounds = this.clickSounds;
                customManager.menuPlaylist = this.menuPlaylist;
                customManager.labPlaylist = this.labPlaylist;
                customManager.minigamePlaylist = this.minigamePlaylist;
                customManager.masterMixer = this.masterMixer;
                
                customManager.soundSlider = this.soundSlider;
                customManager.musicSlider = this.musicSlider;
                customManager.qualityDropdown = this.qualityDropdown;
                customManager.resolutionDropdown = this.resolutionDropdown;
                customManager.languageDropdown = this.languageDropdown;
                customManager.fullscreenToggle = this.fullscreenToggle;

                customManager.sfxSource = managerObject.AddComponent<AudioSource>();
                customManager.musicSource = managerObject.AddComponent<AudioSource>();
                customManager.musicSource.loop = true;

                Instance = customManager;
                DontDestroyOnLoad(managerObject);
                
                Instance.InitializeSettings();
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else if (Instance != this)
        {
            // Передаем новые UI ссылки
            Instance.soundSlider = this.soundSlider;
            Instance.musicSlider = this.musicSlider;
            Instance.qualityDropdown = this.qualityDropdown;
            Instance.resolutionDropdown = this.resolutionDropdown;
            Instance.languageDropdown = this.languageDropdown;
            Instance.fullscreenToggle = this.fullscreenToggle;
            
            Instance.BindUIElements();
            Destroy(this);
        }
    }

    private void Start()
    {
        BindUIElements();
        PlayThemeForActiveScene();
    }

    private void InitializeSettings()
    {
        // 1. Загрузка громкости
        float sVol = PlayerPrefs.GetFloat("Vol_SFX", 0.75f);
        float mVol = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
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
            if (soundSlider != null)
            {
                soundSlider.value = PlayerPrefs.GetFloat("Vol_SFX", 0.75f);
                soundSlider.onValueChanged.RemoveAllListeners();
                soundSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.value = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (qualityDropdown != null)
            {
                // Сбрасываем автоматическую калибровку верстки для качества! (Убираем AutoCalibrateDropdown)
                // Это оставляет оригинальные настройки шаблона (Template, Content, Viewport) из Инспектора нетронутыми.

                // Настраиваем только перевод текстов через Transtable_Dropdown (это безопасно, не ломает верстку)
                Transtable_Dropdown transDD = qualityDropdown.GetComponent<Transtable_Dropdown>();
                if (transDD == null)
                {
                    transDD = qualityDropdown.gameObject.AddComponent<Transtable_Dropdown>();
                }
                
                // Задаем ID текстовых строк для опций качества (37 = Очень Низкое, ..., 42 = Ультра)
                transDD.translations.optionTextIDs = new int[] { 37, 38, 39, 40, 41, 42 };

                qualityDropdown.value = PlayerPrefs.GetInt("QualitySetting", 2);
                qualityDropdown.onValueChanged.RemoveAllListeners();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
                
                // Обновляем переводы текстов безопасным образом
                transDD.UpdateDropdown();
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = PlayerPrefs.GetInt("FullscreenMode", Screen.fullScreen ? 1 : 0) == 1;
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            if (languageDropdown != null)
            {
                // Полностью уничтожаем Transtable_Dropdown для языка, чтобы он никогда не вмешивался и не очищал названия
                Transtable_Dropdown transDD = languageDropdown.GetComponent<Transtable_Dropdown>();
                if (transDD != null)
                {
                    DestroyImmediate(transDD);
                }

                // Всегда принудительно очищаем и устанавливаем 3 официальных языка (Русский, English, Türkçe) в нативном виде
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new List<string> { "Русский", "English", "Türkçe" });

                // Калибруем размеры, темно-серый цвет текста и пивоты шторки под 3 опции
                AutoCalibrateDropdown(languageDropdown, 55f, 200f, 22f);

                languageDropdown.value = PlayerPrefs.GetInt("Alchemist_Language", 0);
                languageDropdown.onValueChanged.RemoveAllListeners();
                languageDropdown.onValueChanged.AddListener(SetLanguage);
                languageDropdown.RefreshShownValue();
            }

            BuildResolutionsList();
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

    public void SetSFXVolume(float val)
    {
        if (isUpdatingSettings) return;
        PlayerPrefs.SetFloat("Vol_SFX", val);
        if (sfxSource != null)
        {
            sfxSource.volume = val;
        }
        if (masterMixer != null)
        {
            float db = Mathf.Log10(Mathf.Clamp(val, 0.0001f, 1f)) * 20f;
            masterMixer.SetFloat("SFXVolume", db);
        }
    }

    public void SetMusicVolume(float val)
    {
        if (isUpdatingSettings) return;
        PlayerPrefs.SetFloat("Vol_Music", val);
        if (musicSource != null)
        {
            musicSource.volume = val;
        }
        if (masterMixer != null)
        {
            float db = Mathf.Log10(Mathf.Clamp(val, 0.0001f, 1f)) * 20f;
            masterMixer.SetFloat("MusicVolume", db);
        }
    }

    public void SetQuality(int index)
    {
        if (isUpdatingSettings) return;

        // Полная проверка: вызов разрешен только при непосредственном ручном выборе пользователем в qualityDropdown
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                bool isQualityActive = (qualityDropdown != null && (selected == qualityDropdown.gameObject || selected.transform.IsChildOf(qualityDropdown.transform)));
                if (!isQualityActive)
                {
                    Debug.LogWarning($"[ALCHEMIST SETTINGS] SetQuality проигнорирован: активен выбранный объект {selected.name}, а не qualityDropdown");
                    return;
                }
            }
        }

        // Жесткая защита от перекрестного срабатывания событий в Unity:
        // Если вызов пришел не от qualityDropdown, игнорируем его!
        if (qualityDropdown != null && qualityDropdown.value != index)
        {
            Debug.LogWarning($"[ALCHEMIST SETTINGS] SetQuality проигнорирован: пришел индекс {index}, но в качестве выбрано {qualityDropdown.value}");
            return;
        }

        isUpdatingSettings = true;
        try
        {
            QualitySettings.SetQualityLevel(index);
            PlayerPrefs.SetInt("QualitySetting", index);
            ApplyQualitySafeguards(index);
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

        // Полная проверка: вызов разрешен только при непосредственном ручном выборе пользователем в languageDropdown
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                bool isLanguageActive = (languageDropdown != null && (selected == languageDropdown.gameObject || selected.transform.IsChildOf(languageDropdown.transform)));
                if (!isLanguageActive)
                {
                    Debug.LogWarning($"[ALCHEMIST SETTINGS] SetLanguage проигнорирован: активен выбранный объект {selected.name}, а не languageDropdown");
                    return;
                }
            }
        }

        // Жесткая защита от перекрестного срабатывания событий в Unity:
        // Если вызов пришел не от languageDropdown, игнорируем его!
        if (languageDropdown != null && languageDropdown.value != index)
        {
            Debug.LogWarning($"[ALCHEMIST SETTINGS] SetLanguage проигнорирован: пришел индекс {index}, но в языке выбрано {languageDropdown.value}");
            return;
        }

        isUpdatingSettings = true;
        try
        {
            Translator.SelectLanguage(index);
        }
        finally
        {
            isUpdatingSettings = false;
        }
    }

    /// <summary>
    /// Автоматическая калибровка выпадающего списка TMP_Dropdown.
    /// <summary>
    /// Безопасно калибрует выпадающий список: предотвращает перенос слов, центрирует текст
    /// и настраивает высоту элементов без нарушения встроенной логики разметки TMP_Dropdown.
    /// </summary>
    private void AutoCalibrateDropdown(TMP_Dropdown dropdown, float itemHeight, float templateHeight, float fontSize)
    {
        if (dropdown == null) return;

        // 1. Настройка основного текста (Label) на самой кнопке
        if (dropdown.captionText != null)
        {
            dropdown.captionText.fontSize = fontSize;
            dropdown.captionText.alignment = TextAlignmentOptions.Center;
            dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.captionText.overflowMode = TextOverflowModes.Ellipsis;
            dropdown.captionText.characterSpacing = 0f;
            dropdown.captionText.wordSpacing = 0f;
        }

        // 2. Настройка текста внутри элементов списка (Item Label)
        if (dropdown.itemText != null)
        {
            dropdown.itemText.fontSize = fontSize - 2f;
            dropdown.itemText.alignment = TextAlignmentOptions.Center;
            dropdown.itemText.textWrappingMode = TextWrappingModes.NoWrap;
            dropdown.itemText.overflowMode = TextOverflowModes.Ellipsis;
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
            musicSource.clip = track;
            musicSource.Play();
        }
    }
}
