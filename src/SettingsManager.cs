using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI Элементы")]
    public Slider soundSlider;
    public Slider musicSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;
    public Toggle fullscreenToggle;

    [Header("Смеситель Аудио")]
    public AudioMixer masterMixer; 

    [Header("Источники Звука")]
    [SerializeField] private AudioSource sfxSource;   // Источник для коротких эффектов (Hover/Click)
    [SerializeField] private AudioSource musicSource; // Источник для фоновой музыки (Looped)

    [Header("Каталог Hover Эффектов")]
    [Tooltip("Массив из 10 эффектов наведения на кнопки интерфейса")]
    [SerializeField] private AudioClip[] hoverSounds = new AudioClip[10];

    [Header("Каталог Плейлистов (В каждом по 5-10 треков)")]
    [SerializeField] private AudioClip[] mainMenuPlaylist;
    [SerializeField] private AudioClip[] charSelectionPlaylist;
    [SerializeField] private AudioClip[] worldExplorationPlaylist;
    [SerializeField] private AudioClip[] battlePlaylist;

    private void Awake()
    {
        if (Instance == null)
        {
            // Чтобы предотвратить перенос всего Canvas, настроек, кнопок или фона из-за DontDestroyOnLoad(gameObject),
            // мы всегда инициализируем синглтон на чистом, отдельно созданном при старте GameObject.
            if (gameObject.name != "FATE_SETTINGS_MANAGER")
            {
                Debug.Log($"[FATE SETTINGS] Инициализация синглтона на чистом объекте. Защищаем '{gameObject.name}' от DontDestroyOnLoad переноса при переходе на другие сцены.");
                
                GameObject sfxObject = new GameObject("FATE_SETTINGS_MANAGER");
                SettingsManager customManager = sfxObject.AddComponent<SettingsManager>();
                
                // Копируем все настройки каталогов звуков и музыки
                customManager.hoverSounds = this.hoverSounds;
                customManager.mainMenuPlaylist = this.mainMenuPlaylist;
                customManager.charSelectionPlaylist = this.charSelectionPlaylist;
                customManager.worldExplorationPlaylist = this.worldExplorationPlaylist;
                customManager.battlePlaylist = this.battlePlaylist;
                customManager.masterMixer = this.masterMixer;
                
                // Обязательно копируем UI ссылки на слайдеры, дропдауны и тоглы
                customManager.soundSlider = this.soundSlider;
                customManager.musicSlider = this.musicSlider;
                customManager.qualityDropdown = this.qualityDropdown;
                customManager.resolutionDropdown = this.resolutionDropdown;
                customManager.languageDropdown = this.languageDropdown;
                customManager.fullscreenToggle = this.fullscreenToggle;

                // Создаем и переносим источники звука на новый объект
                if (this.sfxSource != null)
                {
                    AudioSource newSfx = sfxObject.AddComponent<AudioSource>();
                    CopyAudioSource(this.sfxSource, newSfx);
                    customManager.sfxSource = newSfx;
                }
                else
                {
                    customManager.sfxSource = sfxObject.AddComponent<AudioSource>();
                }

                if (this.musicSource != null)
                {
                    AudioSource newMusic = sfxObject.AddComponent<AudioSource>();
                    CopyAudioSource(this.musicSource, newMusic);
                    customManager.musicSource = newMusic;
                }
                else
                {
                    customManager.musicSource = sfxObject.AddComponent<AudioSource>();
                }
                
                Instance = customManager;
                DontDestroyOnLoad(sfxObject);
                
                // Сразу же привязываем UI элементы в первой сцене
                Instance.BindLoadedUIElements();

                // Уничтожаем только этот дублирующий скрипт-компонент на исходной панели настроек,
                // чтобы сам GameObject панели со всеми UI кнопками, слайдерами и событиями остался внутри Canvas!
                Destroy(this);
                return;
            }

            Instance = this;
            Debug.Log("[FATE SETTINGS] Глобальный синглтон SettingsManager успешно запущен на выделенном объекте FATE_SETTINGS_MANAGER.");
        }
        else
        {
            if (Instance != this)
            {
                // Если мы зашли в меню повторно, передаем новые инспекторные ссылки на кнопки глобальному синглтону
                Instance.soundSlider = this.soundSlider;
                Instance.musicSlider = this.musicSlider;
                Instance.qualityDropdown = this.qualityDropdown;
                Instance.resolutionDropdown = this.resolutionDropdown;
                Instance.languageDropdown = this.languageDropdown;
                Instance.fullscreenToggle = this.fullscreenToggle;

                Instance.BindLoadedUIElements();

                // Уничтожаем этот дублирующий компонент, так как синглтон уже привязан к новым UI элементам
                Destroy(this);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance == this)
        {
            BindLoadedUIElements();
        }
    }

    void Start()
    {
        if (Instance == this)
        {
            BindLoadedUIElements();
        }
    }

    private void CopyAudioSource(AudioSource source, AudioSource target)
    {
        if (source == null || target == null) return;
        target.clip = source.clip;
        target.volume = source.volume;
        target.pitch = source.pitch;
        target.loop = source.loop;
        target.playOnAwake = source.playOnAwake;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.mute = source.mute;
        target.bypassEffects = source.bypassEffects;
        target.bypassListenerEffects = source.bypassListenerEffects;
        target.bypassReverbZones = source.bypassReverbZones;
    }

    public void BindLoadedUIElements()
    {
        Debug.Log("[FATE SETTINGS] Регистрация кнопок и слайдеров в активном синглтоне SettingsManager...");

        if (soundSlider != null) {
            soundSlider.wholeNumbers = false;
            soundSlider.minValue = 0f;
            soundSlider.maxValue = 1f;
            soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
            soundSlider.onValueChanged.RemoveAllListeners();
            soundSlider.onValueChanged.AddListener(SetSoundVolume);
            SetSoundVolume(soundSlider.value);
            Debug.Log("[FATE SETTINGS] soundSlider успешно привязан.");
        }
        
        if (musicSlider != null) {
            musicSlider.wholeNumbers = false;
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            SetMusicVolume(musicSlider.value);
            Debug.Log("[FATE SETTINGS] musicSlider успешно привязан.");
        }

        if (qualityDropdown != null) {
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
            SetQuality(qualityDropdown.value);
            Debug.Log("[FATE SETTINGS] qualityDropdown успешно привязан.");
        }

        if (languageDropdown != null) {
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            Debug.Log("[FATE SETTINGS] languageDropdown успешно привязан.");
        }

        UpdateDropdownTranslations();

        if (resolutionDropdown != null) {
            resolutionDropdown.ClearOptions();
            Resolution[] resolutions = Screen.resolutions;
            List<string> options = new List<string>();
            List<Resolution> uniqueResolutions = new List<Resolution>();
            int currentResIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++) {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                if (!options.Contains(option)) {
                    options.Add(option);
                    uniqueResolutions.Add(resolutions[i]);
                    if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) {
                        currentResIndex = options.Count - 1;
                    }
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", currentResIndex);
            resolutionDropdown.RefreshShownValue();
            Debug.Log("[FATE SETTINGS] resolutionDropdown успешно привязан.");
        }

        if (fullscreenToggle != null) {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

            // Форсируем ID текста 11 ("Весь экран" / "Full Screen"), чтобы убрать дефолтный "Старт" (ID 0)
            Transtable_Text tt = fullscreenToggle.GetComponentInChildren<Transtable_Text>(true);
            if (tt != null)
            {
                tt.TextID = 11;
                tt.UpdateText();
            }
            else
            {
                TMP_Text txt = fullscreenToggle.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.text = Translator.GetText(11);
            }
            Debug.Log("[FATE SETTINGS] fullscreenToggle успешно привязан.");
        }
    }

    // Воспроизведение звука наведения по индексу (0-9)
    public void PlayHoverSound(int index)
    {
        if (sfxSource == null) return;
        if (index >= 0 && index < hoverSounds.Length && hoverSounds[index] != null)
        {
            sfxSource.PlayOneShot(hoverSounds[index]);
        }
    }

    // Воспроизведение произвольного звукового эффекта (например, из UIButtonSfxBinder)
    public void PlaySoundEffect(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // Совместимый псевдоним для старых скриптов кнопок и диалогов
    public void PlaySound(AudioClip clip)
    {
        PlaySoundEffect(clip);
    }

    // Совместимый псевдоним для воспроизведения AudioClip через PlaySfx
    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // Совместимый псевдоним для воспроизведения по строковому названию (регистронезависимо по PlaySfx)
    public void PlaySfx(string sfxName)
    {
        PlaySFX(sfxName);
    }

    // Воспроизведение звука по строковому названию клипа (например, из Resources/Audio/)
    // Решает ошибку CS1061 в FactionMapMarker и других C#-скриптах проекта!
    public void PlaySFX(string sfxName)
    {
        if (sfxSource == null || string.IsNullOrEmpty(sfxName)) return;

        // Поиск в локальном массиве hoverSounds по имени ассета
        foreach (AudioClip clip in hoverSounds)
        {
            if (clip != null && clip.name == sfxName)
            {
                sfxSource.PlayOneShot(clip);
                return;
            }
        }

        // Пытаемся динамически загрузить из папки Resources (как Audio/name или просто по имени)
        AudioClip loadedClip = Resources.Load<AudioClip>("Audio/" + sfxName);
        if (loadedClip == null)
        {
            loadedClip = Resources.Load<AudioClip>(sfxName);
        }

        if (loadedClip != null)
        {
            sfxSource.PlayOneShot(loadedClip);
        }
        else
        {
            Debug.LogWarning($"[FATE SETTINGS] Аудиоклип '{sfxName}' не найден в массиве hoverSounds или Resources.");
        }
    }

    // Переключение Музыки в зависимости от состояния игры.
    // playlistIndex: 0 = Меню, 1 = Выбор Персонажей, 2 = Карта/Ходьба, 3 = Бой
    // trackIndex: номер трека в конкретном плейлисте (0-9)
    public void PlayMusicTrack(int playlistIndex, int trackIndex)
    {
        if (musicSource == null) return;

        AudioClip[] targetPlaylist = null;
        switch (playlistIndex)
        {
            case 0: targetPlaylist = mainMenuPlaylist; break;
            case 1: targetPlaylist = charSelectionPlaylist; break;
            case 2: targetPlaylist = worldExplorationPlaylist; break;
            case 3: targetPlaylist = battlePlaylist; break;
        }

        if (targetPlaylist != null && trackIndex >= 0 && trackIndex < targetPlaylist.Length)
        {
            AudioClip clip = targetPlaylist[trackIndex];
            if (clip != null && musicSource.clip != clip)
            {
                musicSource.Stop();
                musicSource.clip = clip;
                musicSource.loop = true; // Зацикливание всегда активно для фоновой музыки
                musicSource.Play();
            }
        }
    }

    // Сохранение и логарифмическая настройка громкости
    public void SetSoundVolume(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        if (masterMixer != null) {
            float vol = value > 0 ? Mathf.Log10(value) * 20 : -80;
            // В вашем микшере экспортирован ровно один параметр для звуков: SoundVol
            TrySetMixerFloat("SoundVol", vol);
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (masterMixer != null) {
            float vol = value > 0 ? Mathf.Log10(value) * 20 : -80;
            // В вашем микшере экспортирован ровно один параметр для музыки: MusicVol
            TrySetMixerFloat("MusicVol", vol);
        }
    }

    // Вспомогательный метод: безопасно задает значение в AudioMixer
    private void TrySetMixerFloat(string parameterName, float volValue)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat(parameterName, volValue);
        }
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int index)
    {
        List<Resolution> uniqueResolutions = new List<Resolution>();
        List<string> options = new List<string>();
        Resolution[] allResolutions = Screen.resolutions;
        
        for (int i = 0; i < allResolutions.Length; i++) {
            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            if (!options.Contains(option)) {
                options.Add(option);
                uniqueResolutions.Add(allResolutions[i]);
            }
        }

        if (index < uniqueResolutions.Count)
        {
            Resolution res = uniqueResolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("Resolution", index);
        }
    }

    public void OnBackToMenu()
    {
        gameObject.SetActive(false);
    }

    public void OnLanguageChanged(int index)
    {
        Translator.SelectLanguage(index);
        UpdateDropdownTranslations();
    }

    public void UpdateDropdownTranslations()
    {
        if (qualityDropdown != null)
        {
            int lang = PlayerPrefs.GetInt("Language", 0);
            TMP_FontAsset font = Translator.Instance != null ? Translator.Instance.defaultFont : null;
            float charSpacing = 0f;
            if (lang == 7 && Translator.Instance != null) font = Translator.Instance.koreanFont;
            else if ((lang == 8 || lang == 6) && Translator.Instance != null) font = Translator.Instance.chineseFont;
            else if (lang == 0 && Translator.Instance != null) charSpacing = Translator.Instance.russianCharacterSpacing;

            // Apply font and clear any spacing offset for the Quality dropdown
            if (qualityDropdown.captionText != null)
            {
                if (font != null) qualityDropdown.captionText.font = font;
                qualityDropdown.captionText.characterSpacing = charSpacing;
            }
            if (qualityDropdown.itemText != null)
            {
                if (font != null) qualityDropdown.itemText.font = font;
                qualityDropdown.itemText.characterSpacing = charSpacing;
            }

            // Populate localized quality names (IDs 37 to 42)
            if (qualityDropdown.options.Count != 6)
            {
                qualityDropdown.ClearOptions();
                List<string> qOptions = new List<string>();
                for (int i = 0; i < 6; i++)
                {
                    qOptions.Add(Translator.GetText(37 + i));
                }
                qualityDropdown.AddOptions(qOptions);
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    qualityDropdown.options[i].text = Translator.GetText(37 + i);
                }
            }
            qualityDropdown.RefreshShownValue();
        }

        // Автоматически переводим текст тогла полноэкранного режима при смене языка
        if (fullscreenToggle != null)
        {
            Transtable_Text tt = fullscreenToggle.GetComponentInChildren<Transtable_Text>(true);
            if (tt != null)
            {
                tt.TextID = 11;
                tt.UpdateText();
            }
            else
            {
                TMP_Text txt = fullscreenToggle.GetComponentInChildren<TMP_Text>(true);
                if (txt != null)
                {
                    txt.text = Translator.GetText(11);
                    
                    // Применяем настройки шрифтов CJK к тогле
                    int lang = PlayerPrefs.GetInt("Language", 0);
                    TMP_FontAsset font = Translator.Instance != null ? Translator.Instance.defaultFont : null;
                    float charSpacing = 0f;
                    if (lang == 7 && Translator.Instance != null) font = Translator.Instance.koreanFont;
                    else if ((lang == 8 || lang == 6) && Translator.Instance != null) font = Translator.Instance.chineseFont;
                    else if (lang == 0 && Translator.Instance != null) charSpacing = Translator.Instance.russianCharacterSpacing;

                    if (font != null) txt.font = font;
                    txt.characterSpacing = charSpacing;
                }
            }
        }
    }
}
