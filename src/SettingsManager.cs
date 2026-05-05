using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown languageDropdown;
    public Toggle fullScreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // 1. Настройка разрешений экрана
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // 2. Настройка качества
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        // 3. Настройка полноэкранного режима
        fullScreenToggle.isOn = Screen.fullScreen;

        // 4. Настройка языка (безопасная настройка через поиск)
        GameObject translatorObj = GameObject.Find("_Translator");
        if (languageDropdown != null && translatorObj != null)
        {
            // Устанавливаем значение по умолчанию (0 - Русский), обновление придет из скрипта локализации
            languageDropdown.value = 0;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions != null && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetLanguage(int langIndex)
    {
        Translator.SelectLanguage(langIndex);
        Debug.Log($"[SettingsManager] Язык изменен на индекс: {langIndex}");
    }

    public void OnBackToMenu()
    {
        Menu_Game.Instance?.ShowMainMenu();
    }
}
