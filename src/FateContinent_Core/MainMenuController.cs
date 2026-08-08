using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки сцен")]
    [Tooltip("Название сцен выбора персонажа или начальной локации")]
    public string characterSelectionSceneName = "CharacterSelection";
    [Tooltip("Индекс сцен в настройках сборки (по умолчанию 1)")]
    public int characterSelectionSceneIndex = 1;
    [Tooltip("Загрузка может произойти по имени (true) или по индексу (false)")]
    public bool loadByName = false;

    /// <summary>
    /// Запуск игры. Интегрировано с системным просмотром загрузки
    /// </summary>
    public void PlayGame()
    {
        Debug.Log($"[FATE CORE] Запуск загрузки сцен. Способность: {(loadByName ? "По имени" : "По индексу")}.");

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

    /// <summary>
    /// Открытие панели настроек (звук, музыка, язык)
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("[FATE CORE] Открытие панели настроек главного меню.");
        
        // Безопасный вызов через рефлексию, чтобы избежать ошибок компиляции, если Menu_Game временно отсутствует
        System.Type menuGameType = System.Type.GetType("Menu_Game");
        if (menuGameType != null)
        {
            var instanceProp = menuGameType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var method = menuGameType.GetMethod("OnClickSettingsButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(instance, null);
                        return;
                    }
                }
            }
        }
        
        Debug.LogWarning("[FATE CORE] Menu_Game не найден в проекте или его Instance равен null. Настройки не могут быть открыты.");
    }

    /// <summary>
    /// Выход из игры. Поддерживает работу в редакторе и в готовой сборке
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[FATE CORE] Запрос на выход из игры...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
