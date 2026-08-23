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
[ExecuteAlways]
public class Calendar_Manager : MonoBehaviour
{
    public static Calendar_Manager Instance { get; private set; }

    [Header("UI Panels & Containers")]
    [SerializeField] private GameObject calendarPanel;
    [SerializeField] private Transform monthsContainer; // Content у ScrollRect
    [SerializeField] private GameObject monthPrefab;     // Префаб карточки месяца
    [SerializeField] private GameObject dayCellPrefab;   // Префаб ячейки дня
    [SerializeField] private Button closeButton;

    [Header("12 Month Sprites (Jan..Dec)")]
    [SerializeField] private Sprite[] monthSprites = new Sprite[12];

    [Header("Missed Day Icon (Broken Flask)")]
    [SerializeField] private Sprite missedFlaskSprite; // Спрайт разбитой колбы для пропущенных дней

    [Header("Reward Icons")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite stoneIcon;
    [SerializeField] private Sprite scrollIcon;
    [SerializeField] private Sprite crystalIcon;

    [System.Serializable]
    public class MonthLayoutConfig
    {
        public string monthName = "Month";
        public Vector2 cardSize = new Vector2(400f, 540f);
        public Vector2 cellSize = new Vector2(34f, 34f);
        public Vector2 spacing = new Vector2(5f, 5f);
        public int padLeft = 35;
        public int padRight = 35;
        public int padTop = 95;
        public int padBottom = 30;
    }

    [Header("Индивидуальная калибровка сеток для каждого месяца")]
    public MonthLayoutConfig[] customMonthLayouts = new MonthLayoutConfig[12];

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

    private void Reset()
    {
        ResetLayoutsToDefaults();
    }

    private void Awake()
    {
        Instance = this;

        // Если в инспекторе уже были заданы валидные калибровки пользователем, не перезаписываем их дефолтами
        bool hasValidInspectorLayouts = (customMonthLayouts != null && customMonthLayouts.Length == 12 && customMonthLayouts[0] != null && customMonthLayouts[0].cellSize.x > 0);

        if (!hasValidInspectorLayouts)
        {
            // Если в PlayerPrefs есть сохраненные настройки верстки, подгружаем их
            string savedJson = PlayerPrefs.GetString("FateContinent_Calendar_Layouts", "");
            if (!string.IsNullOrEmpty(savedJson))
            {
                try
                {
                    MonthLayoutDataWrapper wrapper = JsonUtility.FromJson<MonthLayoutDataWrapper>(savedJson);
                    if (wrapper != null && wrapper.layouts != null && wrapper.layouts.Length == 12)
                    {
                        customMonthLayouts = wrapper.layouts;
                        hasValidInspectorLayouts = true;
                    }
                }
                catch {}
            }
        }

        if (!hasValidInspectorLayouts)
        {
            ResetLayoutsToDefaults();
        }

        if (calendarPanel == null)
            calendarPanel = this.gameObject;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseCalendar);
        }

        if (rewardPopupCloseBtn != null)
        {
            rewardPopupCloseBtn.onClick.RemoveAllListeners();
            rewardPopupCloseBtn.onClick.AddListener(() => {
                if (rewardPopup != null) rewardPopup.SetActive(false);
            });
        }
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
        }
        else
        {
            gameObject.SetActive(true);
        }

        UpdateCurrentDate();

        // Всегда синхронизируем и применяем свежие координаты ячеек при открытии
        if (monthsContainer != null && monthsContainer.childCount == 0)
        {
            GenerateFullCalendar();
        }
        else
        {
            ApplyAllLayoutsInRealtime();
            RefreshAllDaysUI();
        }
    }

    private Coroutine popupAutoHideCoroutine;

    public void CloseCalendar()
    {
        UpdateCurrentDate();
        string todayKey = $"Cal_Claimed_{currentYear}_{currentMonth}_{currentDay}";
        bool isTodayClaimed = PlayerPrefs.GetInt(todayKey, 0) == 1;
        bool isTutorialDone = PlayerPrefs.GetInt("Tutorial_Calendar_Claim_Done", 0) == 1;

        // Если это первый обязательный визит с Котом и день еще не отмечен — блокируем выход и показываем подсказку
        if (!isTodayClaimed && !isTutorialDone)
        {
            string curLang = PlayerPrefs.GetString("Selected_Language", "RU");
            string title = curLang == "EN" ? "Attendance Required" : (curLang == "TR" ? "Giriş Damgası Gerekli" : "Отметьте день");
            string msg = curLang == "EN" ? "Meow! Please stamp today's date in the calendar first to claim your reward!" :
                         (curLang == "TR" ? "Miyav! Ödülünüzü almak için lütfen önce takvimde bugünkü tarihi işaretleyin!" :
                         "Мяу! Сначала отметьте сегодняшний день в календаре (нажав на сияющее число), чтобы получить награду!");

            ShowPopup(title, msg, 2.5f);
            return;
        }

        // Если день отмечен или туториал уже пройден — закрываем календарь
        PlayerPrefs.SetInt("Tutorial_Calendar_Claim_Done", 1);
        PlayerPrefs.Save();

        if (calendarPanel != null)
            calendarPanel.SetActive(false);
        else
            gameObject.SetActive(false);

        // Уведомляем диалоговую систему о закрытии календаря (старт фазы Опыта, Уровня и Аватарок)
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.OnCalendarClosed();
        }
    }

    private void UpdateCurrentDate()
    {
        DateTime now = DateTime.Now;
        currentYear = now.Year;
        currentMonth = now.Month; // 1..12
        currentDay = now.Day;     // 1..31
    }

    [Header("Принудительно использовать калибровочные настройки из кода")]
    [Tooltip("Если выключено (false), календарь использует ваши настройки из инспектора в реальном времени!")]
    public bool useCodeDefaultPaddings = false;

    // Таблица параметров калибровки для 12 сезонных рамок:
    // [padLeft, padRight, padTop, padBottom, cardWidth, cardHeight, cellWidth, cellHeight, spacingX, spacingY]
    private static readonly int[,] DefaultPaddings = new int[,]
    {
        { 110, 110, 44, 75, 600, 680, 27, 44, 4, 13 },  // 1. Январь (Цифры подняты выше: padTop 44 - не касаются нижней планки рамки 29..31)
        { 40, 40, 44, 30, 455, 540, 27, 44, 4, 15 },   // 2. Февраль (Цифры на высоте Января: padTop 44, рамка 455x540)
        { 75, 75, 38, 70, 525, 675, 27, 44, 4, 15 },   // 3. Март (Цифры чуть подняты вверх: padTop 38 - не касаются низа рамки 29..31)
        { 38, 38, 35, 28, 460, 540, 27, 44, 4, 15 },   // 4. Апрель (Эталон)
        { 60, 60, 35, 35, 460, 540, 27, 44, 4, 15 },   // 5. Май (Эталон)
        { 40, 40, 35, 30, 460, 540, 27, 44, 4, 15 },   // 6. Июнь (Эталон)
        { 40, 40, 35, 30, 460, 540, 27, 44, 4, 15 },   // 7. Июль (Эталон: padTop 35)
        { 85, 85, 35, 75, 575, 685, 27, 44, 4, 15 },   // 8. Август (Цифры подняты на точный уровень Июля: padTop 35, рамка не тронута)
        { 40, 40, 30, 30, 460, 540, 27, 44, 4, 15 },   // 9. Сентябрь (Эталон: padTop 30)
        { 90, 90, 30, 75, 585, 685, 27, 44, 4, 15 },   // 10. Октябрь (Цифры подняты до уровня Сентября: padTop 30, рамка не тронута)
        { 60, 60, 35, 35, 460, 540, 27, 44, 4, 15 },   // 11. Ноябрь (Эталон)
        { 60, 60, 35, 35, 460, 540, 27, 44, 4, 15 }    // 12. Декабрь (Высота Ноября: padTop 35, ячейки 27x44)
    };

    [Header("Кнопки управления калибровкой (Включайте галочку для действия в Инспекторе)")]
    [Tooltip("Поставьте галочку, чтобы скопировать все координаты месяцев в буфер обмена")]
    public bool btn_CopyAllLayouts = false;
    [Tooltip("Поставьте галочку, чтобы вставить координаты месяцев из буфера обмена")]
    public bool btn_PasteAllLayouts = false;
    [Tooltip("Поставьте галочку, чтобы сохранить текущие координаты в PlayerPrefs на устройстве")]
    public bool btn_SaveLayoutsToPrefs = false;
    [Tooltip("Поставьте галочку, чтобы загрузить сохраненные координаты из PlayerPrefs")]
    public bool btn_LoadLayoutsFromPrefs = false;
    [Tooltip("Поставьте галочку, чтобы сбросить все месяцы на стандартные значения")]
    public bool btn_ResetToDefaults = false;

    [ContextMenu("Сбросить калибровку на значения по умолчанию")]
    public void ResetLayoutsToDefaults()
    {
        customMonthLayouts = new MonthLayoutConfig[12];
        for (int i = 0; i < 12; i++)
        {
            customMonthLayouts[i] = new MonthLayoutConfig
            {
                monthName = (monthNamesRu != null && i < monthNamesRu.Length) ? monthNamesRu[i] : $"Месяц {i + 1}",
                padLeft = DefaultPaddings[i, 0],
                padRight = DefaultPaddings[i, 1],
                padTop = DefaultPaddings[i, 2],
                padBottom = DefaultPaddings[i, 3],
                cardSize = new Vector2(DefaultPaddings[i, 4], DefaultPaddings[i, 5]),
                cellSize = new Vector2(DefaultPaddings[i, 6], DefaultPaddings[i, 7]),
                spacing = new Vector2(DefaultPaddings[i, 8], DefaultPaddings[i, 9])
            };
        }
        ApplyAllLayoutsInRealtime();
    }

    [ContextMenu("Скопировать все координаты месяцев (В Буфер JSON)")]
    public void CopyAllLayoutsToClipboard()
    {
        if (customMonthLayouts == null || customMonthLayouts.Length == 0)
        {
            ResetLayoutsToDefaults();
        }

        MonthLayoutDataWrapper wrapper = new MonthLayoutDataWrapper { layouts = customMonthLayouts };
        string json = JsonUtility.ToJson(wrapper, true);
        GUIUtility.systemCopyBuffer = json;
        Debug.Log("<color=#80FFDB><b>[CALENDAR] Все координаты 12 месяцев скопированы в буфер обмена!</b></color>");
    }

    [ContextMenu("Вставить координаты месяцев (Из Буфера JSON)")]
    public void PasteAllLayoutsFromClipboard()
    {
        string json = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[CALENDAR] Буфер обмена пуст!");
            return;
        }

        try
        {
            MonthLayoutDataWrapper wrapper = JsonUtility.FromJson<MonthLayoutDataWrapper>(json);
            if (wrapper != null && wrapper.layouts != null && wrapper.layouts.Length == 12)
            {
                customMonthLayouts = wrapper.layouts;
                ApplyAllLayoutsInRealtime();
                Debug.Log("<color=#FFE57F><b>[CALENDAR] Координаты месяцев успешно вставлены и применены!</b></color>");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CALENDAR] Ошибка разбора JSON из буфера: {ex.Message}");
        }
    }

    [ContextMenu("Сохранить координаты в PlayerPrefs")]
    public void SaveLayoutsToPrefs()
    {
        if (customMonthLayouts == null || customMonthLayouts.Length == 0) return;
        MonthLayoutDataWrapper wrapper = new MonthLayoutDataWrapper { layouts = customMonthLayouts };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("FateContinent_Calendar_Layouts", json);
        PlayerPrefs.Save();
        Debug.Log("<color=#80FFDB><b>[CALENDAR] Координаты успешно сохранены в постоянную память игры!</b></color>");
    }

    [ContextMenu("Загрузить координаты из PlayerPrefs")]
    public void LoadLayoutsFromPrefs()
    {
        string json = PlayerPrefs.GetString("FateContinent_Calendar_Layouts", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[CALENDAR] Нет сохраненных координат в PlayerPrefs.");
            return;
        }

        try
        {
            MonthLayoutDataWrapper wrapper = JsonUtility.FromJson<MonthLayoutDataWrapper>(json);
            if (wrapper != null && wrapper.layouts != null && wrapper.layouts.Length == 12)
            {
                customMonthLayouts = wrapper.layouts;
                ApplyAllLayoutsInRealtime();
                Debug.Log("<color=#FFE57F><b>[CALENDAR] Координаты успешно загружены из PlayerPrefs и применены!</b></color>");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CALENDAR] Ошибка загрузки координат: {ex.Message}");
        }
    }

    [System.Serializable]
    private class MonthLayoutDataWrapper
    {
        public MonthLayoutConfig[] layouts;
    }

    private bool _needsLayoutUpdate = false;

    private void OnValidate()
    {
        // Кнопки-триггеры в Инспекторе
        if (btn_CopyAllLayouts)
        {
            btn_CopyAllLayouts = false;
            CopyAllLayoutsToClipboard();
        }
        if (btn_PasteAllLayouts)
        {
            btn_PasteAllLayouts = false;
            PasteAllLayoutsFromClipboard();
        }
        if (btn_SaveLayoutsToPrefs)
        {
            btn_SaveLayoutsToPrefs = false;
            SaveLayoutsToPrefs();
        }
        if (btn_LoadLayoutsFromPrefs)
        {
            btn_LoadLayoutsFromPrefs = false;
            LoadLayoutsFromPrefs();
        }
        if (btn_ResetToDefaults)
        {
            btn_ResetToDefaults = false;
            ResetLayoutsToDefaults();
        }

        // Помечаем флаг для безопасного обновления верстки в кадре LateUpdate без исключений SendMessage!
        _needsLayoutUpdate = true;
    }

    private void Update()
    {
        // Если в Play Mode вы меняете координаты в Инспекторе — они мгновенно применяются к сетке!
        if (Application.isPlaying && calendarPanel != null && calendarPanel.activeInHierarchy)
        {
            ApplyAllLayoutsInRealtime();
        }
    }

    private void LateUpdate()
    {
        if (_needsLayoutUpdate)
        {
            _needsLayoutUpdate = false;
            ApplyAllLayoutsInRealtime();
        }
    }

    /// <summary>
    /// Применяет измененные размеры карточек, отступы и сетки ячеек ко всем 12 месяцам в реальном времени
    /// </summary>
    public void ApplyAllLayoutsInRealtime()
    {
        if (monthsContainer == null || monthsContainer.childCount == 0) return;

        // 1. Проверяем все дочерние объекты в контейнере
        int childCount = monthsContainer.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform monthTransform = monthsContainer.GetChild(i);
            if (monthTransform == null) continue;

            // Определяем порядковый номер месяца (1..12) из имени объекта или по индексу
            int monthNum = i + 1;
            string objName = monthTransform.name;
            if (objName.StartsWith("Month_") && objName.Length >= 8)
            {
                int parsed;
                if (int.TryParse(objName.Substring(6, 2), out parsed))
                {
                    monthNum = parsed;
                }
            }

            MonthLayoutConfig cfg = GetLayoutConfigForMonth(monthNum);
            if (cfg == null) continue;

            // Настройка размера карточки месяца
            RectTransform cardRect = monthTransform.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.sizeDelta = cfg.cardSize;
            }

            // Находим сетку чисел внутри месяца
            Transform daysGrid = monthTransform.Find("Days_Grid");
            if (daysGrid == null) daysGrid = monthTransform;

            GridLayoutGroup gridGroup = daysGrid.GetComponent<GridLayoutGroup>();
            if (gridGroup != null)
            {
                gridGroup.cellSize = cfg.cellSize;
                gridGroup.spacing = cfg.spacing;
                gridGroup.padding = new RectOffset(cfg.padLeft, cfg.padRight, cfg.padTop, cfg.padBottom);
                gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridGroup.constraintCount = 7;
                gridGroup.childAlignment = TextAnchor.UpperCenter;

                // Немедленно обновляем внутреннюю сетку чисел
                RectTransform gridRect = daysGrid.GetComponent<RectTransform>();
                if (gridRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
                }
            }

            if (cardRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(cardRect);
            }
        }

        // Обновляем родительский контейнер Scroll Content
        RectTransform containerRect = monthsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
    }

    public MonthLayoutConfig GetLayoutConfigForMonth(int monthIndex1Based)
    {
        int idx = Mathf.Clamp(monthIndex1Based - 1, 0, 11);
        if (!useCodeDefaultPaddings && customMonthLayouts != null && idx < customMonthLayouts.Length && customMonthLayouts[idx] != null && customMonthLayouts[idx].cellSize.x > 0)
        {
            return customMonthLayouts[idx];
        }

        MonthLayoutConfig cfg = new MonthLayoutConfig
        {
            monthName = monthNamesRu[idx],
            padLeft = DefaultPaddings[idx, 0],
            padRight = DefaultPaddings[idx, 1],
            padTop = DefaultPaddings[idx, 2],
            padBottom = DefaultPaddings[idx, 3],
            cardSize = new Vector2(DefaultPaddings[idx, 4], DefaultPaddings[idx, 5]),
            cellSize = new Vector2(DefaultPaddings[idx, 6], DefaultPaddings[idx, 7]),
            spacing = new Vector2(DefaultPaddings[idx, 8], DefaultPaddings[idx, 9])
        };

        return cfg;
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

            MonthLayoutConfig cfg = GetLayoutConfigForMonth(m);

            // Настройка размера карточки месяца
            RectTransform cardRect = monthObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.sizeDelta = cfg.cardSize;
            }

            // Установка спрайта рамки месяца
            Image frameImg = monthObj.GetComponent<Image>();
            if (frameImg != null && monthSprites != null && (m - 1) < monthSprites.Length)
            {
                frameImg.sprite = monthSprites[m - 1];
            }

            // Контейнер для ячеек дней внутри месяца
            Transform daysGrid = monthObj.transform.Find("Days_Grid");
            if (daysGrid == null) daysGrid = monthObj.transform;

            // Настройка сетки GridLayoutGroup под пропорции конкретной рамки
            GridLayoutGroup gridGroup = daysGrid.GetComponent<GridLayoutGroup>();
            if (gridGroup != null)
            {
                gridGroup.cellSize = cfg.cellSize;
                gridGroup.spacing = cfg.spacing;
                gridGroup.padding = new RectOffset(cfg.padLeft, cfg.padRight, cfg.padTop, cfg.padBottom);
                gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridGroup.constraintCount = 7;
                gridGroup.childAlignment = TextAnchor.UpperCenter;
            }

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
        // Гармоничное чередование наград на каждый день (без повторения только камней):
        // 7, 14, 21, 28-й день (каждое воскресенье) и конец месяца -> Кристаллы
        if (day % 7 == 0 || day == DateTime.DaysInMonth(currentYear, month)) return crystalIcon != null ? crystalIcon : goldIcon;
        // 5, 10, 15, 20, 25-й день -> Древние Свитки
        if (day % 5 == 0) return scrollIcon != null ? scrollIcon : goldIcon;
        // 3, 6, 9, 12, 18, 24-й день -> Камни Улучшения
        if (day % 3 == 0) return stoneIcon != null ? stoneIcon : goldIcon;
        // Остальные дни (1, 2, 4, 8, 11, 13, 16, 17, 19, 22, 23, 26, 27, 29) -> Золото
        return goldIcon;
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
        int gold = 1000 + (day * 80);
        int stones = 0;
        int scrolls = 0;
        int crystals = 0;

        if (day % 7 == 0 || day == DateTime.DaysInMonth(currentYear, month))
        {
            crystals = 5 + (month >= 6 ? 3 : 0);
        }
        else if (day % 5 == 0)
        {
            scrolls = 2;
        }
        else if (day % 3 == 0)
        {
            stones = 3;
        }

        int currentGold = PlayerPrefs.GetInt("Player_Gold", 5000);
        int currentStones = PlayerPrefs.GetInt("Player_Stones", 10);
        int currentScrolls = PlayerPrefs.GetInt("Player_Scrolls", 3);
        int currentCrystals = PlayerPrefs.GetInt("Player_Crystals", 0);

        PlayerPrefs.SetInt("Player_Gold", currentGold + gold);
        PlayerPrefs.SetInt("Player_Stones", currentStones + stones);
        PlayerPrefs.SetInt("Player_Scrolls", currentScrolls + scrolls);
        PlayerPrefs.SetInt("Player_Crystals", currentCrystals + crystals);
        PlayerPrefs.Save();

        // Мгновенная синхронизация цифр в верхней панели (TopPanel)
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.SyncPlayerPrefsResources();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddResources(gold, stones, scrolls, crystals);
        }

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
            if (missedBadge != null)
            {
                missedBadge.SetActive(!isClaimed); // Если не забрали — показываем разбитую колбу
                if (missedFlaskSprite != null && !isClaimed)
                {
                    Image missedImg = missedBadge.GetComponent<Image>();
                    if (missedImg != null) missedImg.sprite = missedFlaskSprite;
                }
            }

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

    private void ShowPopup(string title, string message, float autoHideSeconds = 0f)
    {
        if (rewardPopup != null && rewardPopupText != null)
        {
            rewardPopupText.text = $"<size=120%><b>{title}</b></size>\n\n{message}";
            rewardPopup.SetActive(true);

            if (popupAutoHideCoroutine != null)
            {
                StopCoroutine(popupAutoHideCoroutine);
                popupAutoHideCoroutine = null;
            }

            if (autoHideSeconds > 0f)
            {
                popupAutoHideCoroutine = StartCoroutine(AutoHidePopupRoutine(autoHideSeconds));
            }
        }
    }

    private System.Collections.IEnumerator AutoHidePopupRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardPopup != null)
        {
            rewardPopup.SetActive(false);
        }
        popupAutoHideCoroutine = null;
    }
}
