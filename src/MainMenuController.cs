using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float fadeDuration = 1.0f;

    public void PlayGame()
    {
        Debug.Log("[FATE CORE] Загрузка основной сцены...");
        // Загружаем сцену под индексом 1 (GameScene)
        SceneManager.LoadScene(1);
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
