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
    public Image kittenSilhouette; // Ссылка на силуэт кота для динамического проявления alpha-канала

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

    private bool isLoading = false;

    /// <summary>
    /// Асинхронный запуск загрузки любой сцены по индексу.
    /// </summary>
    public void LoadScene(int sceneBuildIndex)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[FATE DIAGNOSTIC] Загрузка уже активна! Блокируем дублирующий вызов LoadScene(индекс: {sceneBuildIndex})");
            return;
        }
        isLoading = true;
        Debug.Log($"<color=#00FFCC>[FATE DIAGNOSTIC]</color> Вызван публичный метод LoadScene(индекс: {sceneBuildIndex}). Запускаем корутину.");
        StartCoroutine(LoadAsynchronously(sceneBuildIndex));
    }

    /// <summary>
    /// Асинхронный запуск загрузки любой сцены по имени.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[FATE DIAGNOSTIC] Загрузка уже активна! Блокируем дублирующий вызов LoadScene(имя: '{sceneName}')");
            return;
        }
        isLoading = true;
        Debug.Log($"<color=#00FFCC>[FATE DIAGNOSTIC]</color> Вызван публичный метод LoadScene(имя: '{sceneName}'). Запускаем корутину.");
        StartCoroutine(LoadAsynchronouslyByName(sceneName));
    }

    private IEnumerator LoadAsynchronouslyByName(string sceneName)
    {
        Debug.Log($"<color=#FF3366>[FATE DIAGNOSTIC]</color> Корутина LoadAsynchronouslyByName НАЧАТА для сцены: '{sceneName}'. Текущий Time.timeScale = {Time.timeScale}");
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            Debug.Log("[FATE DIAGNOSTIC] Панель загрузки активирована (SetActive(true)).");
        }
        else
        {
            Debug.LogError("[FATE DIAGNOSTIC] КРИТИЧЕСКАЯ ОШИБКА: loadingPanel равен NULL! Экрану загрузки нечего показывать.");
        }

        // Сбрасываем прозрачность силуэта кота в 0 в начале загрузки
        if (kittenSilhouette != null)
        {
            Color c = kittenSilhouette.color;
            c.a = 0f;
            kittenSilhouette.color = c;
            Debug.Log("[FATE DIAGNOSTIC] Прозрачность силуэта кота сброшена в 0.");
        }
        else
        {
            Debug.LogWarning("[FATE DIAGNOSTIC] Предупреждение: kittenSilhouette равен NULL.");
        }

        // Показываем случайный совет про зельеварение
        if (funnyQuoteText != null)
        {
            string quote = GetRandomCatQuote();
            funnyQuoteText.text = quote;
            Debug.Log($"[FATE DIAGNOSTIC] Установлен текст совета: '{quote}'");
        }

        AsyncOperation operation = null;
        try
        {
            Debug.Log($"[FATE DIAGNOSTIC] Запуск асинхронной загрузки сцены через SceneManager.LoadSceneAsync('{sceneName}')...");
            operation = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE DIAGNOSTIC] ИСКЛЮЧЕНИЕ при вызове LoadSceneAsync: {ex.Message}\n{ex.StackTrace}");
        }

        if (operation == null)
        {
            Debug.LogError($"[FATE DIAGNOSTIC] КРИТИЧЕСКАЯ ОШИБКА: AsyncOperation равен NULL! Проверьте, добавлена ли сцена '{sceneName}' в Build Settings (меню File -> Build Settings).");
            if (loadingPanel != null) loadingPanel.SetActive(false);
            isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = false;
        float visualProgress = 0f;
        float minLoadDuration = 2.5f; 
        int loopTicks = 0;

        Debug.Log("[FATE DIAGNOSTIC] Вход в цикл загрузки while (!operation.isDone)...");

        while (!operation.isDone)
        {
            loopTicks++;
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Плавно накапливаем визуальный прогресс с течением времени (используем unscaledDeltaTime)
            float step = Time.unscaledDeltaTime / minLoadDuration;
            visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, step);

            if (loopTicks <= 5 || loopTicks % 30 == 0)
            {
                Debug.Log($"[FATE DIAGNOSTIC] Итерация #{loopTicks}: RealProgress={operation.progress}, TargetProgress={targetProgress}, VisualProgress={visualProgress}, RealDeltaTime={Time.unscaledDeltaTime}, timeScale={Time.timeScale}");
            }

            if (progressBar != null) progressBar.value = visualProgress;
            if (progressText != null) progressText.text = $"Загрузка... {(visualProgress * 100f):F0}%";

            // Плавно проявляем силуэт кота в соответствии с визуальным прогрессом
            if (kittenSilhouette != null)
            {
                Color c = kittenSilhouette.color;
                c.a = visualProgress;
                kittenSilhouette.color = c;
            }

            // Переходим на сцену только если реальная загрузка завершена И шкала доползла до 100%
            if (operation.progress >= 0.9f && visualProgress >= 0.99f)
            {
                Debug.Log($"[FATE DIAGNOSTIC] УСПЕХ: Загрузка завершена! Ждем полсекунды (Realtime) и активируем сцену.");
                yield return new WaitForSecondsRealtime(0.5f); 
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log("[FATE DIAGNOSTIC] Выход из цикла корутины LoadAsynchronouslyByName. Загрузка завершена.");
        if (loadingPanel != null) loadingPanel.SetActive(false);
        isLoading = false;
    }

    private IEnumerator LoadAsynchronously(int sceneBuildIndex)
    {
        Debug.Log($"<color=#FF3366>[FATE DIAGNOSTIC]</color> Корутина LoadAsynchronously НАЧАТА для сцены по индексу: {sceneBuildIndex}. Текущий Time.timeScale = {Time.timeScale}");
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            Debug.Log("[FATE DIAGNOSTIC] Панель загрузки активирована (SetActive(true)).");
        }
        else
        {
            Debug.LogError("[FATE DIAGNOSTIC] КРИТИЧЕСКАЯ ОШИБКА: loadingPanel равен NULL! Экрану загрузки нечего показывать.");
        }

        // Сбрасываем прозрачность силуэта кота в 0 в начале загрузки
        if (kittenSilhouette != null)
        {
            Color c = kittenSilhouette.color;
            c.a = 0f;
            kittenSilhouette.color = c;
            Debug.Log("[FATE DIAGNOSTIC] Прозрачность силуэта кота сброшена в 0.");
        }
        else
        {
            Debug.LogWarning("[FATE DIAGNOSTIC] Предупреждение: kittenSilhouette равен NULL.");
        }

        // Показываем случайный совет про зельеварение
        if (funnyQuoteText != null)
        {
            string quote = GetRandomCatQuote();
            funnyQuoteText.text = quote;
            Debug.Log($"[FATE DIAGNOSTIC] Установлен текст совета: '{quote}'");
        }

        AsyncOperation operation = null;
        try
        {
            Debug.Log($"[FATE DIAGNOSTIC] Запуск асинхронной загрузки сцены через SceneManager.LoadSceneAsync({sceneBuildIndex})...");
            operation = SceneManager.LoadSceneAsync(sceneBuildIndex);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE DIAGNOSTIC] ИСКЛЮЧЕНИЕ при вызове LoadSceneAsync: {ex.Message}\n{ex.StackTrace}");
        }

        if (operation == null)
        {
            Debug.LogError($"[FATE DIAGNOSTIC] КРИТИЧЕСКАЯ ОШИБКА: AsyncOperation равен NULL! Проверьте, добавлена ли сцена с индексом {sceneBuildIndex} в Build Settings (меню File -> Build Settings).");
            if (loadingPanel != null) loadingPanel.SetActive(false);
            isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = false;
        float visualProgress = 0f;
        float minLoadDuration = 2.5f; 
        int loopTicks = 0;

        Debug.Log("[FATE DIAGNOSTIC] Вход в цикл загрузки while (!operation.isDone)...");

        while (!operation.isDone)
        {
            loopTicks++;
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Плавно накапливаем визуальный прогресс с течением времени (используем unscaledDeltaTime)
            float step = Time.unscaledDeltaTime / minLoadDuration;
            visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, step);

            if (loopTicks <= 5 || loopTicks % 30 == 0)
            {
                Debug.Log($"[FATE DIAGNOSTIC] Итерация #{loopTicks}: RealProgress={operation.progress}, TargetProgress={targetProgress}, VisualProgress={visualProgress}, RealDeltaTime={Time.unscaledDeltaTime}, timeScale={Time.timeScale}");
            }

            if (progressBar != null) progressBar.value = visualProgress;
            if (progressText != null) progressText.text = $"Загрузка... {(visualProgress * 100f):F0}%";

            // Плавно проявляем силуэт кота в соответствии с визуальным прогрессом
            if (kittenSilhouette != null)
            {
                Color c = kittenSilhouette.color;
                c.a = visualProgress;
                kittenSilhouette.color = c;
            }

            // Переходим на сцену только если реальная загрузка завершена И шкала доползла до 100%
            if (operation.progress >= 0.9f && visualProgress >= 0.99f)
            {
                Debug.Log($"[FATE DIAGNOSTIC] УСПЕХ: Загрузка завершена! Ждем полсекунды (Realtime) и активируем сцену.");
                yield return new WaitForSecondsRealtime(0.5f); 
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log("[FATE DIAGNOSTIC] Выход из цикла корутины LoadAsynchronouslyByIndex. Загрузка завершена.");
        if (loadingPanel != null) loadingPanel.SetActive(false);
        isLoading = false;
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
            },
            // Turkish
            new string[] {
                "Kazana bir damla balık yağı ekleniyor...",
                "Büyük iksir yapımından önce pençeler keskinleştiriliyor...",
                "Kedi nanesi sert akıl hocasından saklanıyor...",
                "Simya sırasında farelere uslu durmaları öğretiliyor...",
                "Kazan sıcaklığı patilerle test ediliyor...",
                "Sihirli iksirden tüyler temizleniyor..."
            }
        };

        int lang = PlayerPrefs.GetInt("Alchemist_Language", 0);
        if (lang < 0 || lang >= quotes.Length) lang = 1;
        int index = Random.Range(0, quotes[lang].Length);
        return quotes[lang][index];
    }
}
