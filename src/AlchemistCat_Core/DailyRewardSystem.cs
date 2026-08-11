using UnityEngine;
using UnityEngine.UI;
using System;

public class DailyRewardSystem : MonoBehaviour
{
    [Header("UI References (Must be assigned in Inspector)")]
    public Button claimButton;
    public Text timerText;
    public Text statusText;
    [Tooltip("Массив из 7 слотов дней (День 1 - День 7)")]
    public Transform[] calendarDaySlots; 

    private int currentStreak = 0;
    private DateTime lastClaimTime;

    private void Start()
    {
        ValidateInspectorReferences();
        LoadDailyData();
        CheckDailyStatus();
    }

    private void Update()
    {
        CheckDailyStatus();
    }

    private void ValidateInspectorReferences()
    {
        // Попытка авто-поиска кнопок и текстов в дочерних объектах, если они не заданы в Инспекторе
        if (claimButton == null)
        {
            claimButton = GetComponentInChildren<Button>(true);
            if (claimButton == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b.name.ToLower().Contains("claim") || b.name.ToLower().Contains("reward") || b.name.ToLower().Contains("button"))
                    {
                        claimButton = b;
                        break;
                    }
                }
            }
        }

        if (timerText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name.ToLower().Contains("timer") || t.name.ToLower().Contains("time"))
                {
                    timerText = t;
                    break;
                }
            }
        }

        if (statusText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name.ToLower().Contains("status") || t.name.ToLower().Contains("info") || t.name.ToLower().Contains("log"))
                {
                    statusText = t;
                    break;
                }
            }
        }

        if (calendarDaySlots == null || calendarDaySlots.Length == 0)
        {
            // Пытаемся найти дочерние объекты, представляющие собой дни календаря
            System.Collections.Generic.List<Transform> foundSlots = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("day") || child.name.ToLower().Contains("slot") || child.name.ToLower().Contains("calendar"))
                {
                    foundSlots.Add(child);
                }
            }
            if (foundSlots.Count > 0)
            {
                calendarDaySlots = foundSlots.ToArray();
            }
        }

        // Выводим только мягкие информативные предупреждения, чтобы не засорять консоль красными ошибками
        if (claimButton == null)
            Debug.Log("[DailyRewardSystem] Мягкое уведомление: Кнопка 'Claim Button' не назначена. Система наград будет работать в фоновом режиме.");
        if (timerText == null)
            Debug.Log("[DailyRewardSystem] Мягкое уведомление: Текстовое поле 'Timer Text' отсутствует. Отсчет времени будет скрыт.");
        if (statusText == null)
            Debug.Log("[DailyRewardSystem] Мягкое уведомление: Текстовое поле 'Status Text' не назначено.");
        if (calendarDaySlots == null || calendarDaySlots.Length == 0)
            Debug.Log("[DailyRewardSystem] Мягкое уведомление: Массив слотов дней 'Calendar Day Slots' пуст.");
    }

    private void CheckDailyStatus()
    {
        TimeSpan difference = DateTime.Now - lastClaimTime;
        bool isRewardReady = false;

        if (difference.TotalHours >= 24 && difference.TotalHours < 48)
        {
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Новая награда готова!";
        }
        else if (difference.TotalHours >= 48)
        {
            // Сброс серии за пропуск дня
            currentStreak = 0;
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Серия сброшена! Заберите День 1.";
        }
        else
        {
            isRewardReady = false;
            if (claimButton != null) claimButton.interactable = false;
            TimeSpan timeToWait = TimeSpan.FromHours(24) - difference;
            if (timerText != null)
            {
                timerText.text = string.Format("До награды: {0:D2}:{1:D2}:{2:D2}", 
                    timeToWait.Hours, timeToWait.Minutes, timeToWait.Seconds);
            }
        }

        UpdateCalendarVisuals(isRewardReady);
    }

    public void ClaimReward()
    {
        currentStreak = (currentStreak % 7) + 1; // Цикл 7 дней
        lastClaimTime = DateTime.Now;

        // Начисление наград
        if (GameManager.Instance != null)
        {
            // Начисление золота и кристаллов
            switch (currentStreak)
            {
                case 1: GameManager.Instance.AddGold(100); break;
                case 2: GameManager.Instance.AddGold(250); break;
                case 3: GameManager.Instance.AddCrystals(1); break;
                case 4: GameManager.Instance.AddGold(500); break;
                case 5: 
                    GameManager.Instance.AddVipXP(10);
                    if (MinigamesManager.Instance != null)
                        MinigamesManager.Instance.UnlockDarts();
                    if (statusText != null) statusText.text = "Вам открыт ДАРТС!";
                    break;
                case 6: GameManager.Instance.AddGold(1000); break;
                case 7: 
                    GameManager.Instance.AddCrystals(10);
                    if (statusText != null) statusText.text = "Вы получили Золотой Сундук!";
                    break;
            }

            // Дополнительная проверка на активность дней
            GameManager.Instance.daysActive++;
            if (GameManager.Instance.daysActive % 10 == 0)
            {
                if (MinigamesManager.Instance != null)
                    MinigamesManager.Instance.UnlockMouseCatch();
                if (statusText != null) statusText.text = "Открыта игра: ЛОВЛЯ МЫШЕЙ!";
            }
        }
        else
        {
            // Запасная заглушка, если GameManager отсутствует (для тестов вне основной сцены)
            Debug.LogWarning($"[DailyRewardSystem] GameManager.Instance не найден. Имитация начисления за день {currentStreak}.");
            if (statusText != null) statusText.text = $"Забрана награда дня {currentStreak} (Тестовый режим)";
        }

        SaveDailyData();
    }

    private void UpdateCalendarVisuals(bool isRewardReady)
    {
        if (calendarDaySlots == null) return;

        for (int i = 0; i < calendarDaySlots.Length; i++)
        {
            if (calendarDaySlots[i] == null) continue;
            
            Image slotImage = calendarDaySlots[i].GetComponent<Image>();
            if (slotImage == null) continue;

            if (i < currentStreak)
            {
                slotImage.color = Color.green; // Зеленый - получено
            }
            else if (i == currentStreak && isRewardReady)
            {
                slotImage.color = Color.yellow; // Желтый - готово к получению
            }
            else
            {
                slotImage.color = Color.gray; // Серый - закрыто
            }
        }
    }

    private void LoadDailyData()
    {
        currentStreak = PlayerPrefs.GetInt("DailyStreak", 0);
        string lastClaimStr = PlayerPrefs.GetString("LastDailyClaim", "");
        if (!string.IsNullOrEmpty(lastClaimStr))
        {
            lastClaimTime = DateTime.Parse(lastClaimStr);
        }
        else
        {
            // По умолчанию даем забрать сразу
            lastClaimTime = DateTime.Now.AddDays(-2);
        }
    }

    private void SaveDailyData()
    {
        PlayerPrefs.SetInt("DailyStreak", currentStreak);
        PlayerPrefs.SetString("LastDailyClaim", lastClaimTime.ToString());
        PlayerPrefs.Save();
    }
}
