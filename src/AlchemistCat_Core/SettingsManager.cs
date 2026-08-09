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
            AutoCalibrateDropdown(qualityDropdown, 55f, 320f, 22f);

            // Автоматически гарантируем наличие ровно 6 опций качества, если список пуст или поврежден
            if (qualityDropdown.options.Count != 6)
            {
                qualityDropdown.ClearOptions();
                List<string> optionsList = new List<string>();
                for (int i = 37; i <= 42; i++)
                {
                    optionsList.Add(Translator.GetText(i));
                }
                qualityDropdown.AddOptions(optionsList);
            }

            // Автоматически настраиваем и связываем Transtable_Dropdown, если его забыли настроить в Инспекторе
            Transtable_Dropdown transDD = qualityDropdown.GetComponent<Transtable_Dropdown>();
            if (transDD == null)
            {
                transDD = qualityDropdown.gameObject.AddComponent<Transtable_Dropdown>();
                transDD.translations.optionTextIDs = new int[] { 37, 38, 39, 40, 41, 42 };
            }
            else if (transDD.translations.optionTextIDs == null || transDD.translations.optionTextIDs.Length != 6)
            {
                transDD.translations.optionTextIDs = new int[] { 37, 38, 39, 40, 41, 42 };
            }

            qualityDropdown.value = PlayerPrefs.GetInt("QualitySetting", 2);
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
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
            AutoCalibrateDropdown(languageDropdown, 55f, 200f, 22f);

            // Автоматически гарантируем наличие 3 официальных языков (RU, EN, TR) в их нативном виде
            if (languageDropdown.options.Count != 3)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new List<string> { "Русский", "English", "Türkçe" });
            }

            // Отключаем Transtable_Dropdown для языка, так как названия языков должны оставаться нативными (Русский, English, Türkçe)
            Transtable_Dropdown transDD = languageDropdown.GetComponent<Transtable_Dropdown>();
            if (transDD != null)
            {
                Destroy(transDD);
            }

            languageDropdown.value = PlayerPrefs.GetInt("Alchemist_Language", 0);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(SetLanguage);
        }

        BuildResolutionsList();
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
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualitySetting", index);
        ApplyQualitySafeguards(index);
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
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt("FullscreenMode", isFull ? 1 : 0);
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < resolutionsList.Count)
        {
            Resolution res = resolutionsList[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", index);
        }
    }

    public void SetLanguage(int index)
    {
        Translator.SelectLanguage(index);
    }

    /// <summary>
    /// Автоматическая калибровка выпадающего списка TMP_Dropdown.
    /// Исправляет поломанные пивоты (Pivot), оффсеты, высоту элементов (Item Height),
    /// центрирует текст и запрещает перенос длинных слов, чтобы они не обрезались.
    /// </summary>
    private void AutoCalibrateDropdown(TMP_Dropdown dropdown, float itemHeight, float templateHeight, float fontSize)
    {
        if (dropdown == null) return;

        // 1. Позиционирование и размеры самого dropdown
        RectTransform ddRect = dropdown.GetComponent<RectTransform>();
        if (ddRect != null)
        {
            LayoutElement layout = dropdown.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredHeight = 45f;
            }
        }

        // Основной текст (Label) на самой кнопке
        Transform labelTrans = dropdown.transform.Find("Label");
        if (labelTrans != null)
        {
            TextMeshProUGUI tmpText = labelTrans.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.fontSize = fontSize;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.textWrappingMode = TextWrappingModes.NoWrap;
                tmpText.overflowMode = TextOverflowModes.Ellipsis;
                tmpText.characterSpacing = 0f; // СБРОС КРИТИЧЕСКОГО СПЕЙСИНГА СЛОВ!
                tmpText.wordSpacing = 0f;
                
                // Растягиваем RectTransform, чтобы текст влезал полностью без обрезки по бокам
                RectTransform labelRect = labelTrans.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = new Vector2(0f, 0f);
                    labelRect.anchorMax = new Vector2(1f, 1f);
                    labelRect.offsetMin = new Vector2(15f, 0f); // небольшой отступ слева
                    labelRect.offsetMax = new Vector2(-25f, 0f); // отступ справа под стрелочку
                }
            }
        }

        // Вычисляем оптимальную высоту шторки динамически на основе количества опций
        int optionCount = dropdown.options != null ? dropdown.options.Count : 3;
        float spacingVal = 2f;
        float paddingVal = 4f; // Минимальный паддинг
        float dynamicTemplateHeight = (optionCount * itemHeight) + ((optionCount - 1) * spacingVal) + paddingVal;
        
        // Ограничиваем разумным максимумом, если опций слишком много
        if (dynamicTemplateHeight > 400f) dynamicTemplateHeight = 400f;

        // 2. Исправляем Template
        Transform templateTransform = dropdown.transform.Find("Template");
        if (templateTransform != null)
        {
            RectTransform templateRect = templateTransform.GetComponent<RectTransform>();
            if (templateRect != null)
            {
                // КРИТИЧЕСКИЙ ФИКС: Сбрасываем съехавший Pivot (ставим его на верхнюю грань 0.5, 1.0)
                templateRect.pivot = new Vector2(0.5f, 1f);

                // Выравниваем анкоры (stretch по ширине, крепление к нижней грани кнопки)
                templateRect.anchorMin = new Vector2(0f, 0f);
                templateRect.anchorMax = new Vector2(1f, 0f);

                // Очищаем левый/правый оффсеты и сдвигаем слегка вниз
                templateRect.offsetMin = new Vector2(0f, -dynamicTemplateHeight);
                templateRect.offsetMax = new Vector2(0f, -2f);
                
                // Фиксируем высоту шторки
                templateRect.sizeDelta = new Vector2(templateRect.sizeDelta.x, dynamicTemplateHeight);
            }

            // Настройка Viewport (чтобы не резал элементы)
            Transform viewport = templateTransform.Find("Viewport");
            if (viewport != null)
            {
                RectTransform viewportRect = viewport.GetComponent<RectTransform>();
                if (viewportRect != null)
                {
                    viewportRect.anchorMin = Vector2.zero;
                    viewportRect.anchorMax = Vector2.one;
                    viewportRect.sizeDelta = Vector2.zero;
                    viewportRect.anchoredPosition = Vector2.zero;
                }

                // Настройка Content (контейнер для списка элементов)
                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    RectTransform contentRect = content.GetComponent<RectTransform>();
                    if (contentRect != null)
                    {
                        contentRect.anchorMin = new Vector2(0f, 1f);
                        contentRect.anchorMax = new Vector2(1f, 1f);
                        contentRect.pivot = new Vector2(0.5f, 1f);
                        contentRect.anchoredPosition = Vector2.zero;
                        
                        // Если на Content висит Vertical Layout Group или Content Size Fitter, настраиваем его
                        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
                        if (vlg != null)
                        {
                            vlg.childControlHeight = true;
                            vlg.childForceExpandHeight = false;
                            vlg.spacing = spacingVal;
                            vlg.padding = new RectOffset(0, 0, (int)(paddingVal/2), (int)(paddingVal/2));
                        }

                        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
                        if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
                        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }

                    // Настройка эталонного Item
                    Transform item = content.Find("Item");
                    if (item != null)
                    {
                        RectTransform itemRect = item.GetComponent<RectTransform>();
                        if (itemRect != null)
                        {
                            itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemHeight);
                        }

                        // Текст внутри элемента списка (Item Label)
                        Transform itemLabel = item.Find("Item Label");
                        if (itemLabel != null)
                        {
                            TextMeshProUGUI itemTmp = itemLabel.GetComponent<TextMeshProUGUI>();
                            if (itemTmp != null)
                            {
                                itemTmp.fontSize = fontSize - 2f; // Делаем текст читаемым и крупным
                                itemTmp.alignment = TextAlignmentOptions.Center; // По центру!
                                itemTmp.textWrappingMode = TextWrappingModes.NoWrap; // Отключаем перенос слов!
                                itemTmp.overflowMode = TextOverflowModes.Ellipsis;
                                itemTmp.characterSpacing = 0f; // СБРОС КРИТИЧЕСКОГО СПЕЙСИНГА!
                                itemTmp.wordSpacing = 0f;
                            }

                            // Расширяем оффсеты текста на всю ширину ячейки (чтобы длинные слова не резались)
                            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
                            if (itemLabelRect != null)
                            {
                                itemLabelRect.anchorMin = Vector2.zero;
                                itemLabelRect.anchorMax = Vector2.one;
                                itemLabelRect.offsetMin = new Vector2(30f, 0f); // Запас под галочку слева
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
