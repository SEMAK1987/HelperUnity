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
        
        // В Unity нельзя прямо отменить LoadSceneAsync, но можно просто скрыть UI 
        // и не активировать сцену. Принудительно вызываем ShowMainMenu.
        if (Menu_Game.Instance != null) 
        {
            Menu_Game.Instance.ShowMainMenu();
        }
        else
        {
            // Если синглтон недоступен, пробуем найти его
            Menu_Game mg = FindFirstObjectByType<Menu_Game>();
            if (mg != null) mg.ShowMainMenu();
        }
    }
}
