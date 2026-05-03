using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float fadeDuration = 1.0f;

    public void PlayGame()
    {
        Debug.Log("[FATE CORE] Загрузка основной сцены (Индекс 1)...");
        
        // Пытаемся найти менеджер, если инстанс не подхватился
        if (LoadingScreenManager.Instance == null)
        {
            LoadingScreenManager.Instance = FindFirstObjectByType<LoadingScreenManager>();
        }

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(1);
        }
        else
        {
            Debug.LogWarning("[FATE CORE] LoadingScreenManager не найден! Загрузка без экрана...");
            SceneManager.LoadScene(1);
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[FATE CORE] Открытие настроек...");
        // Здесь мы позже добавим анимацию выезда панели настроек
    }

    public void QuitGame()
    {
        Debug.Log("[FATE CORE] Выход из игры...");
        Application.Quit();
    }
}
