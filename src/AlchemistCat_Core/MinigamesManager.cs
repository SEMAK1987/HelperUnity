using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Менеджер развлекательных мини-игр для Кота: Дартс и Ловля Мышей.
/// Начисляет щедрые ресурсы (золото, кристаллы, опыт) за победы!
/// </summary>
public class MinigamesManager : MonoBehaviour
{
    public static MinigamesManager Instance { get; private set; }

    [Header("Интерфейс Мини-игр")]
    public GameObject minigameMenuPanel;
    public GameObject dartsPanel;
    public GameObject mouseCatchPanel;

    [Header("Дартс Элементы")]
    public RectTransform dartBoard;
    public Button throwButton;
    public TextMeshProUGUI dartsScoreText;
    public TextMeshProUGUI dartsHighscoreText;

    [Header("Ловля Мышей Элементы")]
    public Button mousePrefab; // Кнопка-мышка, которая прыгает по экрану
    public Transform miceSpawnContainer;
    public TextMeshProUGUI miceTimerText;
    public TextMeshProUGUI miceCountText;

    private int dartsHighScore = 0;
    private int miceCaughtThisSession = 0;
    private bool isMiceCatchActive = false;
    private float miceGameTimer = 15f;

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

        dartsHighScore = PlayerPrefs.GetInt("Darts_HighScore", 0);
    }

    private void Start()
    {
        CloseAllGames();
    }

    public void OpenMinigameMenu()
    {
        if (minigameMenuPanel != null) minigameMenuPanel.SetActive(true);
    }

    public void CloseAllGames()
    {
        if (minigameMenuPanel != null) minigameMenuPanel.SetActive(false);
        if (dartsPanel != null) dartsPanel.SetActive(false);
        if (mouseCatchPanel != null) mouseCatchPanel.SetActive(false);
        isMiceCatchActive = false;
    }

    #region Механики Дартса
    public void UnlockDarts()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.unlockedDarts = true;
            GameManager.Instance.AutoSave();
        }
        Debug.Log("[ALCHEMIST MINIGAMES] Игра ДАРТС разблокирована!");
    }

    public void StartDartsGame()
    {
        if (GameManager.Instance != null && !GameManager.Instance.unlockedDarts)
        {
            Debug.LogWarning("[ALCHEMIST MINIGAMES] Дартс еще заблокирован!");
            return;
        }

        CloseAllGames();
        if (dartsPanel != null)
        {
            dartsPanel.SetActive(true);
            if (dartsHighscoreText != null) dartsHighscoreText.text = $"Рекорд: {dartsHighScore}";
            if (dartsScoreText != null) dartsScoreText.text = "Брось дротик!";
        }
    }

    public void ThrowDart()
    {
        // Имитируем физику броска с колебанием прицела
        float distanceToCenter = Random.Range(0f, 100f);
        int points = 0;

        if (distanceToCenter < 10f)
        {
            points = 50; // Буллс-ай!
            if (dartsScoreText != null) dartsScoreText.text = "Мяу! БУЛЛС-АЙ! +50 очков!";
        }
        else if (distanceToCenter < 40f)
        {
            points = 25;
            if (dartsScoreText != null) dartsScoreText.text = "Отличный бросок! +25 очков!";
        }
        else if (distanceToCenter < 75f)
        {
            points = 10;
            if (dartsScoreText != null) dartsScoreText.text = "Попал в цель! +10 очков!";
        }
        else
        {
            points = 0;
            if (dartsScoreText != null) dartsScoreText.text = "Мимо мишени! Попробуй еще раз.";
        }

        if (points > 0 && GameManager.Instance != null)
        {
            // Награда за очки
            GameManager.Instance.AddGold(points * 2);
            GameManager.Instance.AddXP(points / 2);

            if (points > dartsHighScore)
            {
                dartsHighScore = points;
                PlayerPrefs.SetInt("Darts_HighScore", dartsHighScore);
                if (dartsHighscoreText != null) dartsHighscoreText.text = $"Рекорд: {dartsHighScore}";
            }
        }
    }
    #endregion

    #region Механики Ловли Мышей
    public void UnlockMouseCatch()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.unlockedMouseCatch = true;
            GameManager.Instance.AutoSave();
        }
        Debug.Log("[ALCHEMIST MINIGAMES] Игра ЛОВЛЯ МЫШЕЙ разблокирована!");
    }

    public void StartMouseCatchGame()
    {
        if (GameManager.Instance != null && !GameManager.Instance.unlockedMouseCatch)
        {
            Debug.LogWarning("[ALCHEMIST MINIGAMES] Ловля мышей заблокирована!");
            return;
        }

        CloseAllGames();
        if (mouseCatchPanel != null)
        {
            mouseCatchPanel.SetActive(true);
            miceCaughtThisSession = 0;
            miceGameTimer = 15f;
            isMiceCatchActive = true;
            if (miceCountText != null) miceCountText.text = "Мышей поймано: 0";
            StartCoroutine(MiceSpawningCycle());
        }
    }

    private IEnumerator MiceSpawningCycle()
    {
        while (isMiceCatchActive && miceGameTimer > 0)
        {
            miceGameTimer -= Time.deltaTime;
            if (miceTimerText != null) miceTimerText.text = $"Время: {Mathf.CeilToInt(miceGameTimer)} сек";

            // Спавним мышек со случайным интервалом
            if (miceSpawnContainer != null && mousePrefab != null && miceSpawnContainer.childCount < 3)
            {
                Button mouseBtn = Instantiate(mousePrefab, miceSpawnContainer);
                RectTransform rect = mouseBtn.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Случайное положение внутри контейнера
                    float rx = Random.Range(-150f, 150f);
                    float ry = Random.Range(-150f, 150f);
                    rect.anchoredPosition = new Vector2(rx, ry);
                }

                // Логика нажатия
                mouseBtn.onClick.AddListener(() => CatchSingleMouse(mouseBtn.gameObject));
                
                // Исчезновение мышки через 2 секунды, если ее не кликнули
                Destroy(mouseBtn.gameObject, 1.8f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        EndMiceCatchGame();
    }

    private void CatchSingleMouse(GameObject mouseObj)
    {
        Destroy(mouseObj);
        miceCaughtThisSession++;
        if (miceCountText != null) miceCountText.text = $"Мышей поймано: {miceCaughtThisSession}";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(15);
            GameManager.Instance.AddXP(5);
        }
    }

    private void EndMiceCatchGame()
    {
        isMiceCatchActive = false;
        Debug.Log($"[ALCHEMIST MINIGAMES] Игра завершена. Мышей поймано: {miceCaughtThisSession}");
        CloseAllGames();

        if (GameManager.Instance != null)
        {
            // Финальный бонус за сессию
            int bonusGold = miceCaughtThisSession * 50;
            GameManager.Instance.AddGold(bonusGold);
            if (CatController.Instance != null)
            {
                CatController.Instance.ShowMeowBubble($"Мяу! Я поймал {miceCaughtThisSession} мышей и получил {bonusGold} золота!");
            }
        }
    }
    #endregion
}
