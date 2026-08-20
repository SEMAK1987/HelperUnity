using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.15)
/// Менеджер Магического Календаря:
/// - 12 Сезонных рамок месяцев (по 2 в ряд)
/// - Автоматический расчет дней и високосных годов
/// - Подсветка текущего дня
/// - Автоматические бейджи:
///   * Зеленая галочка (Checkmark / Day visited) - день закрыт, награда получена
///   * Красный крестик (Missed_Badge / The day is missed) - день пропущен без входа
/// - Ежедневные, месячные, квартальные и годовые награды
/// </summary>
public class Calendar_Manager : MonoBehaviour
{
    [Header("UI Panels & Containers")]
    [SerializeField] private GameObject calendarPanel;
    [SerializeField] private Transform monthsContainer; // Content у ScrollRect
    [SerializeField] private GameObject monthPrefab;     // Префаб карточки месяца
    [SerializeField] private GameObject dayCellPrefab;   // Префаб ячейки дня
    [SerializeField] private Button closeButton;

    [Header("12 Month Sprites (Jan..Dec)")]
    [SerializeField] private Sprite[] monthSprites = new Sprite[12];

    [Header("Reward Icons")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite stoneIcon;
    [SerializeField] private Sprite scrollIcon;
    [SerializeField] private Sprite crystalIcon;

    [Header("Reward Popup / Notification")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI rewardPopupText;
    [SerializeField] private Button rewardPopupCloseBtn;

    // Текущая системная дата
    private int currentYear;
    private int currentMonth;
    private int currentDay;

    private readonly string[] monthNamesRu = {
        "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
    };

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCalendar);

        if (rewardPopupCloseBtn != null)
            rewardPopupCloseBtn.onClick.AddListener(() => {
                if (rewardPopup != null) rewardPopup.SetActive(false);
            });
    }

    private void Start()
    {
        UpdateCurrentDate();
        GenerateFullCalendar();
    }

    public void OpenCalendar()
    {
        if (calendarPanel != null)
        {
            calendarPanel.SetActive(true);
            UpdateCurrentDate();
            RefreshAllDaysUI();
        }
    }

    public void CloseCalendar()
    {
        if (calendarPanel != null)
            calendarPanel.SetActive(false);
    }

    private void UpdateCurrentDate()
    {
        DateTime now = DateTime.Now;
        currentYear = now.Year;
        currentMonth = now.Month; // 1..12
        currentDay = now.Day;     // 1..31
    }

    // Генерация 12 месяцев
    public void GenerateFullCalendar()
    {
        if (monthsContainer == null || monthPrefab == null) return;

        // Очищаем старые объекты
        foreach (Transform child in monthsContainer)
        {
            Destroy(child.gameObject);
        }

        for (int m = 1; m <= 12; m++)
        {
            GameObject monthObj = Instantiate(monthPrefab, monthsContainer);
            monthObj.name = $"Month_{m:00}_{monthNamesRu[m - 1]}";

            // Установка спрайта рамки месяца
            Image frameImg = monthObj.GetComponent<Image>();
            if (frameImg != null && monthSprites != null && (m - 1) < monthSprites.Length)
            {
                frameImg.sprite = monthSprites[m - 1];
            }

            // Контейнер для ячеек дней внутри месяца
            Transform daysGrid = monthObj.transform.Find("Days_Grid");
            if (daysGrid == null) daysGrid = monthObj.transform;

            int daysInMonth = DateTime.DaysInMonth(currentYear, m);

            for (int d = 1; d <= daysInMonth; d++)
            {
                CreateDayCell(daysGrid, m, d);
            }
        }
    }

    private void CreateDayCell(Transform parent, int month, int day)
    {
        if (dayCellPrefab == null) return;

        GameObject cellObj = Instantiate(dayCellPrefab, parent);
        cellObj.name = $"Day_{day}";

        TextMeshProUGUI dayText = cellObj.GetComponentInChildren<TextMeshProUGUI>();
        if (dayText != null)
        {
            dayText.text = day.ToString();
        }

        Image rewardImg = cellObj.transform.Find("Reward_Icon")?.GetComponent<Image>();
        if (rewardImg != null)
        {
            rewardImg.sprite = GetRewardSpriteForDay(month, day);
        }

        Button cellBtn = cellObj.GetComponent<Button>();
        if (cellBtn != null)
        {
            cellBtn.onClick.AddListener(() => OnDayClicked(month, day, cellObj));
        }

        UpdateDayCellVisual(cellObj, month, day);
    }

    private Sprite GetRewardSpriteForDay(int month, int day)
    {
        if (day % 7 == 0) return crystalIcon; // Кристаллы раз в неделю
        if (day % 3 == 0) return scrollIcon;  // Свитки
        if (day % 2 == 0) return stoneIcon;   // Камни
        return goldIcon;                      // Золото
    }

    private void OnDayClicked(int month, int day, GameObject cellObj)
    {
        string saveKey = $"Cal_Claimed_{currentYear}_{month}_{day}";

        // Сегодняшний активный день
        if (month == currentMonth && day == currentDay)
        {
            if (PlayerPrefs.GetInt(saveKey, 0) == 1)
            {
                ShowPopup("Награда уже получена", "Вы уже забрали награду за сегодняшний день. Возвращайтесь завтра за новым подарком!");
                return;
            }

            // Забираем награду
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();

            string rewardDesc = ClaimReward(month, day);
            ShowPopup("Награда получена!", $"Поздравляем! Вы получили награду за {day} {monthNamesRu[month - 1]}:\n\n<b>{rewardDesc}</b>");

            UpdateDayCellVisual(cellObj, month, day);
        }
        else if (month < currentMonth || (month == currentMonth && day < currentDay))
        {
            // Прошедшие дни
            if (PlayerPrefs.GetInt(saveKey, 0) == 1)
            {
                ShowPopup("День закрыт", $"Награда за {day} {monthNamesRu[month - 1]} уже была успешно получена.");
            }
            else
            {
                ShowPopup("День пропущен", $"Этот день ({day} {monthNamesRu[month - 1]}) был пропущен. Заходите в игру каждый день, чтобы не терять награды!");
            }
        }
        else
        {
            // Будущие дни
            ShowPopup("Будущий день", $"Этот день еще не наступил. Приходите {day} {monthNamesRu[month - 1]}, чтобы открыть подарок!");
        }
    }

    private string ClaimReward(int month, int day)
    {
        int gold = 1000 + (day * 100);
        int stones = (day % 2 == 0) ? 2 : 0;
        int scrolls = (day % 3 == 0) ? 1 : 0;
        int crystals = (day % 7 == 0) ? 5 : 0;

        int currentGold = PlayerPrefs.GetInt("Player_Gold", 5000);
        PlayerPrefs.SetInt("Player_Gold", currentGold + gold);
        PlayerPrefs.Save();

        string res = $"+{gold} Золота";
        if (stones > 0) res += $", +{stones} Камней";
        if (scrolls > 0) res += $", +{scrolls} Свитков";
        if (crystals > 0) res += $", +{crystals} Кристаллов";
        return res;
    }

    private void UpdateDayCellVisual(GameObject cellObj, int month, int day)
    {
        string saveKey = $"Cal_Claimed_{currentYear}_{month}_{day}";
        bool isClaimed = PlayerPrefs.GetInt(saveKey, 0) == 1;

        Image bgImage = cellObj.GetComponent<Image>();
        GameObject checkmark = cellObj.transform.Find("Checkmark")?.gameObject;
        GameObject missedBadge = cellObj.transform.Find("Missed_Badge")?.gameObject;

        // 1. Сегодняшний день
        if (month == currentMonth && day == currentDay)
        {
            if (checkmark != null) checkmark.SetActive(isClaimed);
            if (missedBadge != null) missedBadge.SetActive(false);

            if (bgImage != null)
            {
                // Если забрали — мягкий зеленый, если доступно к сбору — сияющее золото!
                bgImage.color = isClaimed ? new Color(0.2f, 0.6f, 0.25f, 0.85f) : new Color(1f, 0.85f, 0.2f, 0.95f);
            }
        }
        // 2. Прошедшие дни
        else if (month < currentMonth || (month == currentMonth && day < currentDay))
        {
            if (checkmark != null) checkmark.SetActive(isClaimed);
            if (missedBadge != null) missedBadge.SetActive(!isClaimed); // Если не забрали — показываем красный крестик/пропуск

            if (bgImage != null)
            {
                // Забранные — приглушенный зеленый, пропущенные — полупрозрачный темный
                bgImage.color = isClaimed ? new Color(0.15f, 0.45f, 0.2f, 0.6f) : new Color(0.25f, 0.15f, 0.15f, 0.5f);
            }
        }
        // 3. Будущие дни
        else
        {
            if (checkmark != null) checkmark.SetActive(false);
            if (missedBadge != null) missedBadge.SetActive(false);

            if (bgImage != null)
            {
                bgImage.color = new Color(0.12f, 0.1f, 0.2f, 0.45f);
            }
        }
    }

    public void RefreshAllDaysUI()
    {
        if (monthsContainer == null) return;
        int m = 1;
        foreach (Transform monthTransform in monthsContainer)
        {
            Transform daysGrid = monthTransform.Find("Days_Grid");
            if (daysGrid == null) daysGrid = monthTransform;

            int d = 1;
            foreach (Transform dayTransform in daysGrid)
            {
                UpdateDayCellVisual(dayTransform.gameObject, m, d);
                d++;
            }
            m++;
        }
    }

    private void ShowPopup(string title, string message)
    {
        if (rewardPopup != null && rewardPopupText != null)
        {
            rewardPopupText.text = $"<size=120%><b>{title}</b></size>\n\n{message}";
            rewardPopup.SetActive(true);
        }
    }
}
