using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider soundSlider;
    public Slider musicSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Аудио")]
    public AudioMixer masterMixer; 

    void Start()
    {
        // Загружаем сохраненные значения
        if (soundSlider != null) {
            soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
            SetSoundVolume(soundSlider.value);
        }
        
        if (musicSlider != null) {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            SetMusicVolume(musicSlider.value);
        }

        if (qualityDropdown != null) {
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            SetQuality(qualityDropdown.value);
        }

        if (resolutionDropdown != null) {
            resolutionDropdown.ClearOptions();
            Resolution[] resolutions = Screen.resolutions;
            List<string> options = new List<string>();
            int currentResIndex = 0;
            for (int i = 0; i < resolutions.Length; i++) {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) {
                    currentResIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", currentResIndex);
            resolutionDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null) {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    public void SetSoundVolume(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        if (masterMixer != null) {
            float vol = value > 0 ? Mathf.Log10(value) * 20 : -80;
            masterMixer.SetFloat("SoundVol", vol);
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (masterMixer != null) {
            float vol = value > 0 ? Mathf.Log10(value) * 20 : -80;
            masterMixer.SetFloat("MusicVol", vol);
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
        Resolution[] resolutions = Screen.resolutions;
        if (index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("Resolution", index);
        }
    }

    public void OnBackToMenu()
    {
        gameObject.SetActive(false);
    }
}
