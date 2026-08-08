using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI элементы")]
    public GameObject loadingContainer; // Родитель всего экрана загрузки (в Overlay_Layer)
    public Slider progressBar;          // Полоска загрузки
    public TextMeshProUGUI progressText; // Текст процента
    public TextMeshProUGUI statusText;   // Текст статуса
    public float statusFontSize = 14f;   // Значительно уменьшаем шрифт

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // ЗАЩИТА: Отвязываем от Canvas перед DontDestroyOnLoad!
            DontDestroyOnLoad(gameObject);
            
            if (loadingContainer != null)
                loadingContainer.SetActive(false);

            if (statusText != null)
                statusText.fontSize = statusFontSize;
        }
        else if (Instance != this)
        {
            // ПАТТЕРН "Zenith Proxy": мягко копируем свежие UI-ссылки из новой сцены в выживший глобальный синглтон
            Instance.loadingContainer = this.loadingContainer;
            Instance.progressBar = this.progressBar;
            Instance.progressText = this.progressText;
            Instance.statusText = this.statusText;
            Instance.statusFontSize = this.statusFontSize;

            if (Instance.statusText != null)
                Instance.statusText.fontSize = Instance.statusFontSize;

            if (Instance.loadingContainer != null)
                Instance.loadingContainer.SetActive(false);

            // Безопасно уничтожаем дублирующий GameObject загрузочного экрана из новой сцены
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(int sceneIndex)
    {
        StopAllCoroutines(); // Останавливаем всё старое перед новым стартом
        StartCoroutine(LoadAsync(sceneIndex));
    }

    public void LoadScene(string sceneName)
    {
        StopAllCoroutines(); // Останавливаем всё старое перед новым стартом
        StartCoroutine(LoadAsyncByName(sceneName));
    }

    private IEnumerator LoadAsync(int sceneIndex)
    {
        if (loadingContainer != null)
            loadingContainer.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; 

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = (progress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.9f)
            {
                if (statusText != null) 
                {
                    statusText.text = "— ЗАГРУЗКА ЗАВЕРШЕНА —";
                }
                
                // Сразу активируем сцену
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingContainer != null) loadingContainer.SetActive(false);
    }

    private IEnumerator LoadAsyncByName(string sceneName)
    {
        if (loadingContainer != null)
            loadingContainer.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogError($"[FATE LOAD] Scene '{sceneName}' could not be loaded!");
            if (loadingContainer != null) loadingContainer.SetActive(false);
            yield break;
        }
        operation.allowSceneActivation = false; 

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = (progress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.9f)
            {
                if (statusText != null) 
                {
                    statusText.text = "— ЗАГРУЗКА ЗАВЕРШЕНА —";
                }
                
                // Сразу активируем сцену
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingContainer != null) loadingContainer.SetActive(false);
    }

    public void CancelLoading()
    {
        StopAllCoroutines(); 
        if (loadingContainer != null) loadingContainer.SetActive(false);
        
        // Безопасный вызов ShowMainMenu через рефлексию, чтобы не вызывать ошибку компиляции при отсутствии Menu_Game.cs
        System.Type menuGameType = System.Type.GetType("Menu_Game");
        if (menuGameType != null)
        {
            var instanceProp = menuGameType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            object instance = null;
            if (instanceProp != null)
            {
                instance = instanceProp.GetValue(null);
            }

            if (instance == null)
            {
                // Поиск по сцене, если синглтон не инициализирован
#if UNITY_2023_1_OR_NEWER
                instance = FindFirstObjectByType(menuGameType);
#else
                instance = FindObjectOfType(menuGameType);
#endif
            }

            if (instance != null)
            {
                var method = menuGameType.GetMethod("ShowMainMenu", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(instance, null);
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning("[FATE CORE] Menu_Game не найден в проекте. Не удалось автоматически вернуть игрока в Главное Меню.");
        }
    }
}
