using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Контроллер загрузочного экрана с веселыми кошачьими цитатами и советами.
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI Ссылки")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI funnyQuoteText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    /// <summary>
    /// Асинхронный запуск загрузки любой сцены по индексу.
    /// </summary>
    public void LoadScene(int sceneBuildIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneBuildIndex));
    }

    /// <summary>
    /// Асинхронный запуск загрузки любой сцены по имени.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsynchronouslyByName(sceneName));
    }

    private IEnumerator LoadAsynchronouslyByName(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // Показываем случайный совет про зельеварение
        if (funnyQuoteText != null)
        {
            funnyQuoteText.text = GetRandomCatQuote();
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogError($"[ALCHEMIST LOAD] Сцена '{sceneName}' не найдена в настройках сборки!");
            if (loadingPanel != null) loadingPanel.SetActive(false);
            yield break;
        }
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"Загрузка... {(progress * 100f):F0}%";

            // Даем игроку рассмотреть загрузку и плавно переходим
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1.0f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private IEnumerator LoadAsynchronously(int sceneBuildIndex)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // Показываем случайный совет про зельеварение
        if (funnyQuoteText != null)
        {
            funnyQuoteText.text = GetRandomCatQuote();
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneBuildIndex);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"Загрузка... {(progress * 100f):F0}%";

            // Даем игроку рассмотреть загрузку и плавно переходим
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1.0f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private string GetRandomCatQuote()
    {
        string[][] quotes = {
            // Russian
            new string[] {
                "Добавляем капельку рыбьего жира в котел...",
                "Натираем когти перед важной миссией...",
                "Прячем валерьянку от строгого наставника...",
                "Учим мышей стоять смирно во время варки...",
                "Проверяем температуру лапками...",
                "Выметаем шерсть из магического зелья..."
            },
            // English
            new string[] {
                "Adding a drop of fish oil to the cauldron...",
                "Sharpening claws before the big brew...",
                "Hiding catnip from the strict mentor...",
                "Teaching mice to sit still during alchemy...",
                "Testing cauldron temperature with paws...",
                "Sweeping fur out of the magic potion..."
            }
        };

        int lang = PlayerPrefs.GetInt("Alchemist_Language", 0) == 0 ? 0 : 1;
        int index = Random.Range(0, quotes[lang].Length);
        return quotes[lang][index];
    }
}
