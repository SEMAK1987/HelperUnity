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
        if (claimButton == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Кнопка 'Claim Button' не назначена в Инспекторе! Перетащите объект кнопки.");
        if (timerText == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Текстовое поле 'Timer Text' не назначено в Инспекторе!");
        if (statusText == null)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Текстовое поле 'Status Text' не назначено.");
        if (calendarDaySlots == null || calendarDaySlots.Length == 0)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Массив слотов 'Calendar Day Slots' пуст! Назначьте 7 дочерних дней.");
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
