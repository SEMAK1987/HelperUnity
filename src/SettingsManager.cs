using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider soundSlider;
    public Slider musicSlider;
    public Slider sensitivitySlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Аудио")]
    public AudioMixer masterMixer; // Если используете AudioMixer

    void Start()
    {
        // Загружаем сохраненные значения или ставим стандартные
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        
        // Применяем настройки при старте
        SetSoundVolume(soundSlider.value);
        SetMusicVolume(musicSlider.value);
    }

    public void SetSoundVolume(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        // Здесь логика изменения громкости звука (например, через Mixer или AudioSource)
        if (masterMixer != null) masterMixer.SetFloat("SoundVol", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (masterMixer != null) masterMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        // Передаем значение в контроллер игрока
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

    public void OnBackToMenu()
    {
        // Логика закрытия панели опций
        GameObject.Find("Options_Menu_Panel")?.SetActive(false);
    }
}
