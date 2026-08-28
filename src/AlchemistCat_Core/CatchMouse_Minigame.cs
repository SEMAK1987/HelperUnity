using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Мини-игра «Поймай мышку» (Catch The Mouse) для Колеса Мини-Игр Кота-Алхимика.
/// Игрок должен за отведенное время кликнуть на мышку нужного цвета (Золотая, Серебряная, Черная),
/// которая перебегает между 5 норками по дорожке.
/// </summary>
public class CatchMouse_Minigame : MonoBehaviour
{
    public static CatchMouse_Minigame Instance;

    [Header("Главная панель мини-игры")]
    public GameObject gamePanel;                // CatchMouse_Game_Panel
    public Button closeButton;                  // Кнопка-крестик

    [Header("Верхняя плашка цели (Задание)")]
    public Image targetMouseDisplayImage;       // Иконка целевой мышки
    public TextMeshProUGUI targetTitleText;     // "ПОЙМАЙ ЭТУ МЫШКУ!"
    public TextMeshProUGUI timerText;           // "Время: 00:15"
    public float roundTime = 15f;               // Длительность раунда в секундах

    [Header("Спрайты Мышек")]
    public Sprite goldenMouseSprite;            // Золотая мышь
    public Sprite silverMouseSprite;            // Серебряная мышь
    public Sprite blackMouseSprite;             // Черная мышь

    [Header("Спрайт Норки и Дорожки")]
    public Sprite holeSprite;                   // Спрайт норки
    public Sprite roadSprite;                   // Спрайт дорожки

    [Header("5 Норок и Дорожка")]
    public RectTransform[] holes;               // 5 позиций норок (Hole_1 .. Hole_5)
    public RectTransform roadTrack;             // Дорожка под норками
    public RectTransform miceRunningLayer;      // Контейнер, где бегают мышки

    [Header("Окно Победы и Награды")]
    public GameObject rewardPopupPanel;         // Reward_Popup_Panel
    public Image potionRewardIcon;              // Potion_Icon (+500 XP)
    public TextMeshProUGUI rewardDescriptionText; // "+5000 Золота\n+10 Камней\n+1 Свиток\n+500 Опыта"
    public Button claimRewardButton;            // Кнопка "ЗАБРАТЬ НАГРАДУ"

    [Header("Звуки")]
    public AudioClip winFanfareSound;           // Звук победы
    public AudioClip wrongMouseSound;           // Звук промаха / писк
    public AudioClip mouseRunSound;             // Звук шажков/бега
    public AudioClip clickSound;

    // Внутреннее состояние игры
    public enum MouseType { Golden, Silver, Black }
    private MouseType currentTargetType;
    private float currentTimer;
    private bool isGameActive = false;
    private bool isGameWon = false;

    // Пул активных бегущих мышек
    private List<GameObject> activeMice = new List<GameObject>();
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMinigame);

        if (claimRewardButton != null)
            claimRewardButton.onClick.AddListener(ClaimRewardAndExit);

        if (rewardPopupPanel != null)
            rewardPopupPanel.SetActive(false);
    }

    private void OnEnable()
    {
        StartNewRound();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ClearAllMice();
    }

    /// <summary>
    /// Старт нового раунда игры
    /// </summary>
    public void StartNewRound()
    {
        isGameWon = false;
        isGameActive = true;
        currentTimer = roundTime;

        if (rewardPopupPanel != null)
            rewardPopupPanel.SetActive(false);

        ClearAllMice();

        // 1. Случайный выбор целевой мышки
        int rand = Random.Range(0, 3);
        currentTargetType = (MouseType)rand;

        // 2. Отображение цели
        if (targetMouseDisplayImage != null)
        {
            targetMouseDisplayImage.sprite = GetMouseSprite(currentTargetType);
            targetMouseDisplayImage.preserveAspect = true;
        }

        if (targetTitleText != null)
        {
            string colorName = currentTargetType == MouseType.Golden ? "<color=#FFD166>ЗОЛОТУЮ</color>" :
                              (currentTargetType == MouseType.Silver ? "<color=#E0E1DD>СЕРЕБРЯНУЮ</color>" : "<color=#B5838D>ТЕМНУЮ</color>");
            targetTitleText.text = $"ПОЙМАЙ {colorName} МЫШКУ!";
        }

        // 3. Запуск корутины спавна мышек
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(MiceSpawnLoop());
    }

    private void Update()
    {
        if (!isGameActive || isGameWon) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            isGameActive = false;
            // Время вышло - перезапуск или предложение повторить
            StartNewRound();
        }

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTimer);
            timerText.text = $"Время: 00:{seconds:D2}";
        }
    }

    /// <summary>
    /// Цикл выбегания мышек из норок
    /// </summary>
    private IEnumerator MiceSpawnLoop()
    {
        while (isGameActive && !isGameWon)
        {
            yield return new WaitForSeconds(Random.Range(0.8f, 1.6f));

            if (holes == null || holes.Length < 2) continue;

            // Выбираем случайную норку старта и норку финиша
            int startHoleIndex = Random.Range(0, holes.Length);
            int endHoleIndex = Random.Range(0, holes.Length);
            while (endHoleIndex == startHoleIndex)
            {
                endHoleIndex = Random.Range(0, holes.Length);
            }

            // Выбираем тип мышки (с вероятностью 45% это целевая мышка)
            MouseType spawnType;
            if (Random.value < 0.45f)
            {
                spawnType = currentTargetType;
            }
            else
            {
                int r = Random.Range(0, 3);
                spawnType = (MouseType)r;
            }

            SpawnMouseRunner(spawnType, startHoleIndex, endHoleIndex);
        }
    }

    /// <summary>
    /// Создание бегущей мышки
    /// </summary>
    private void SpawnMouseRunner(MouseType type, int startIdx, int endIdx)
    {
        if (miceRunningLayer == null || holes == null) return;

        GameObject mouseObj = new GameObject($"Mouse_{type}");
        mouseObj.transform.SetParent(miceRunningLayer, false);

        RectTransform rt = mouseObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 65f);

        Image img = mouseObj.AddComponent<Image>();
        img.sprite = GetMouseSprite(type);
        img.preserveAspect = true;

        Button btn = mouseObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        Vector2 startPos = holes[startIdx].anchoredPosition;
        Vector2 endPos = holes[endIdx].anchoredPosition;

        // Смещение вниз на уровень дорожки
        float roadY = roadTrack != null ? roadTrack.anchoredPosition.y : -50f;
        startPos.y = roadY;
        endPos.y = roadY;

        // Поворот по горизонтали (flipX) в зависимости от направления бега
        bool runRight = endPos.x > startPos.x;
        rt.localScale = new Vector3(runRight ? 1f : -1f, 1f, 1f);
        rt.anchoredPosition = startPos;

        btn.onClick.AddListener(() => OnMouseClicked(mouseObj, type));

        activeMice.Add(mouseObj);
        StartCoroutine(AnimateMouseRunning(mouseObj, rt, startPos, endPos));
    }

    private IEnumerator AnimateMouseRunning(GameObject mouseObj, RectTransform rt, Vector2 start, Vector2 end)
    {
        float duration = Random.Range(1.8f, 2.8f);
        float elapsed = 0f;

        while (elapsed < duration && mouseObj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Плавное движение с легким подскакиванием
            float jumpOffset = Mathf.Sin(t * Mathf.PI * 6f) * 6f;
            rt.anchoredPosition = Vector2.Lerp(start, end, t) + new Vector2(0f, jumpOffset);
            yield return null;
        }

        if (mouseObj != null)
        {
            activeMice.Remove(mouseObj);
            Destroy(mouseObj);
        }
    }

    /// <summary>
    /// Обработка клика по мышке
    /// </summary>
    public void OnMouseClicked(GameObject mouseObj, MouseType type)
    {
        if (!isGameActive || isGameWon) return;

        if (type == currentTargetType)
        {
            // ПОБЕДА!
            isGameWon = true;
            isGameActive = false;

            if (winFanfareSound != null && SettingsManager.Instance != null)
                SettingsManager.Instance.PlaySoundEffect(winFanfareSound);

            ClearAllMice();
            ShowVictoryPopup();
        }
        else
        {
            // ПРОМАХ / НЕ ТА МЫШКА
            if (wrongMouseSound != null && SettingsManager.Instance != null)
                SettingsManager.Instance.PlaySoundEffect(wrongMouseSound);

            // Мышка пугается и быстро исчезает
            if (mouseObj != null)
            {
                activeMice.Remove(mouseObj);
                Destroy(mouseObj);
            }
        }
    }

    private void ShowVictoryPopup()
    {
        if (rewardPopupPanel != null)
        {
            rewardPopupPanel.SetActive(true);
        }

        if (rewardDescriptionText != null)
        {
            rewardDescriptionText.text = "<b><color=#FFD166>+5000 Золота</color></b>\n<b><color=#A0C4FF>+10 Камней</color></b>\n<b><color=#CDB4DB>+1 Свиток</color></b>\n<b><color=#80FFDB>+500 Опыта Игрока</color></b>";
        }
    }

    /// <summary>
    /// Выдача всех наград игроку и выход
    /// </summary>
    public void ClaimRewardAndExit()
    {
        if (clickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(clickSound);

        // 1. Начисление ресурсов в PlayerPrefs
        int gold = PlayerPrefs.GetInt("Player_Gold", 0) + 5000;
        int stones = PlayerPrefs.GetInt("Player_Stones", 0) + 10;
        int scrolls = PlayerPrefs.GetInt("Player_Scrolls", 0) + 1;
        int xp = PlayerPrefs.GetInt("Player_XP", 0) + 500;

        PlayerPrefs.SetInt("Player_Gold", gold);
        PlayerPrefs.SetInt("Player_Stones", stones);
        PlayerPrefs.SetInt("Player_Scrolls", scrolls);
        PlayerPrefs.SetInt("Player_XP", xp);
        PlayerPrefs.Save();

        // 2. Добавление зелья опыта в 100-слотный инвентарь
        if (RecipeCrafting_Manager.Instance != null)
        {
            RecipeCrafting_Manager.Instance.AddPotionToFirstEmptySlot("Player_Potion_XP_500", "Зелье Опыта (+500 XP)");
        }

        // 3. Обновление визуальных ресурсов в верхнем UI
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.RefreshResourceDisplay();
        }

        // 4. Начисление опыта на шкалу уровня аватара
        if (Avatar_Manager.Instance != null)
        {
            Avatar_Manager.Instance.GainPlayerExperience(500);
        }

        CloseMinigame();
    }

    public void CloseMinigame()
    {
        ClearAllMice();
        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    private void ClearAllMice()
    {
        foreach (var m in activeMice)
        {
            if (m != null) Destroy(m);
        }
        activeMice.Clear();
    }

    private Sprite GetMouseSprite(MouseType type)
    {
        switch (type)
        {
            case MouseType.Golden: return goldenMouseSprite;
            case MouseType.Silver: return silverMouseSprite;
            case MouseType.Black: return blackMouseSprite;
            default: return goldenMouseSprite;
        }
    }
}
