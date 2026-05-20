using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public Slider soundSlider;
    public Slider musicSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;
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

        if (languageDropdown != null) {
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
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
            if (lang == 7 && Translator.Instance != null) font = Translator.Instance.koreanFont;
            else if ((lang == 8 || lang == 6) && Translator.Instance != null) font = Translator.Instance.chineseFont;

            // Apply font and clear any spacing offset for the Quality dropdown
            if (qualityDropdown.captionText != null)
            {
                if (font != null) qualityDropdown.captionText.font = font;
                qualityDropdown.captionText.characterSpacing = 0;
            }
            if (qualityDropdown.itemText != null)
            {
                if (font != null) qualityDropdown.itemText.font = font;
                qualityDropdown.itemText.characterSpacing = 0;
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
    }
}
