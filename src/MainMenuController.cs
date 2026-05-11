using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float fadeDuration = 1.0f;

    public void PlayGame()
    {
        Debug.Log("[FATE CORE] MainMenuController.PlayGame() вызван. Если вы хотите переключать панели прямо в меню, используйте Menu_Game.OnStartButtonClicked на кнопке.");
        SceneManager.LoadScene(1);
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
