using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float fadeDuration = 1.0f;

    [Header("Настройки сцены")]
    [Tooltip("Название сцены выбора персонажа, если загружаем по имени")]
    public string characterSelectionSceneName = "CharacterSelection";
    [Tooltip("Индекс сцены выбора персонажа, если загружаем по индексу (по умолчанию 1)")]
    public int characterSelectionSceneIndex = 1;
    [Tooltip("Включите это свойство, чтобы загружать сцену по Имени вместо Индекса")]
    public bool loadByName = false;

    public void PlayGame()
    {
        Debug.Log($"[FATE CORE] MainMenuController.PlayGame() вызван. Загрузка сцены выбора героя {(loadByName ? characterSelectionSceneName : characterSelectionSceneIndex.ToString())}.");
        
        if (loadByName)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneName);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneName);
            }
        }
        else
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneIndex);
            }
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[FATE CORE] MainMenuController.OpenSettings() вызван.");
        if (Menu_Game.Instance != null) Menu_Game.Instance.OnClickSettingsButton();
    }

    public void QuitGame()
    {
        Debug.Log("[FATE CORE] Выход из игры...");
        Application.Quit();
    }
}
