using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.21)
/// Менеджер Аватарок, Рамок и Профиля Игрока с поддержкой Локализации (RU / EN / TR):
/// - 5 Рамок Профиля (1 стартовая, 3 магазинные с 5 ур., 1 донатная с 3 ур.)
/// - 26 Аватарок (до 100 уровня, 5 покупных за Золото, 5 премиум за Кристаллы)
/// - Автоматический перевод через Translator.GetText(ID)
/// - 4-цветный градиент полоски опыта (Белый -> Зеленый -> Оранжевый -> Красный)
/// </summary>
public class Avatar_Manager : MonoBehaviour
{
    public static Avatar_Manager Instance { get; private set; }

    [Header("UI Панель Аватарок и Рамок")]
    public GameObject avatarPanel;
    public Button closeButton;
    public Transform scrollContent;     // Content внутри ScrollRect
    public GameObject avatarItemPrefab;  // Префаб ячейки аватарки
    public GameObject categoryHeaderPrefab; // Префаб заголовка категории

    [Header("Настройки Сетки Гардероба (2-3 в ряд)")]
    public int columnsCount = 3;
    public Vector2 cellSize = new Vector2(130, 155);
    public Vector2 cellSpacing = new Vector2(12, 12);
    public Vector2 panelSize = new Vector2(580, 720);

    [Header("Иконка Профиля в верхнем левом углу")]
    public Button avatarIconButton;
    public Image currentAvatarDisplayImage;
    public Image currentFrameDisplayImage;
    public TextMeshProUGUI levelBadgeText;
    public Image expProgressBar;       // Полоска опыта (Image Type: Filled)
    public TextMeshProUGUI expProgressText; // Текст опыта "0/10 XP"

    [System.Serializable]
    public enum AvatarCategory
    {
        Free,      // Простые и уровневые до 100 ур.
        Shop,      // Покупные за золото (с 5 ур.)
        Premium    // Премиум за кристаллы (с 3 ур.)
    }

    [System.Serializable]
    public class AvatarData
    {
        public int id;
        public string avatarNameRU;
        public string avatarNameEN;
        public string avatarNameTR;
        public AvatarCategory category;
        public Sprite avatarSprite;
        public bool isUnlockedByDefault = false;
        public int unlockLevelRequired = 0;
        public int goldPrice = 0;
        public int crystalPrice = 0;

        public string GetLocalizedName()
        {
            int lang = PlayerPrefs.GetInt("SelectedLanguage", 0);
            if (lang == 1 && !string.IsNullOrEmpty(avatarNameEN)) return avatarNameEN;
            if (lang == 2 && !string.IsNullOrEmpty(avatarNameTR)) return avatarNameTR;
            return string.IsNullOrEmpty(avatarNameRU) ? $"Avatar #{id}" : avatarNameRU;
        }
    }

    [System.Serializable]
    public class FrameData
    {
        public int id;
        public string frameNameRU;
        public string frameNameEN;
        public string frameNameTR;
        public Sprite frameSprite;
        public AvatarCategory category;
        public bool isUnlockedByDefault = false;
        public int unlockLevelRequired = 0;
        public int goldPrice = 0;
        public int crystalPrice = 0;

        public string GetLocalizedName()
        {
            int lang = PlayerPrefs.GetInt("SelectedLanguage", 0);
            if (lang == 1 && !string.IsNullOrEmpty(frameNameEN)) return frameNameEN;
            if (lang == 2 && !string.IsNullOrEmpty(frameNameTR)) return frameNameTR;
            return string.IsNullOrEmpty(frameNameRU) ? $"Frame #{id}" : frameNameRU;
        }
    }

    [Header("Коллекция Аватарок (До 100 Уровня)")]
    public List<AvatarData> allAvatars = new List<AvatarData>();

    [Header("Коллекция 14 Рамок Профиля")]
    public List<FrameData> allFrames = new List<FrameData>();

    [Header("Звуки")]
    public AudioClip selectSound;
    public AudioClip levelUpSound;

    // Опыт и Уровень
    private int currentLevel = 1;
    private int currentExp = 0;
    private int maxExp = 10;
    private int selectedAvatarId = 0;
    private int selectedFrameId = 0;

    private void Awake()
    {
        Instance = this;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAvatarPanel);
        }

        if (avatarIconButton != null)
        {
            avatarIconButton.onClick.RemoveAllListeners();
            avatarIconButton.onClick.AddListener(OnAvatarIconClicked);
        }

        LoadPlayerProfile();
        InitDefaultData();
    }

    private void Start()
    {
        UpdateProfileUI();
    }

    private void LoadPlayerProfile()
    {
        currentLevel = PlayerPrefs.GetInt("Player_Level", 1);
        currentExp = PlayerPrefs.GetInt("Player_Exp", 0);
        maxExp = PlayerPrefs.GetInt("Player_MaxExp", 10);
        selectedAvatarId = PlayerPrefs.GetInt("Selected_Avatar_Id", 0);
        selectedFrameId = PlayerPrefs.GetInt("Selected_Frame_Id", 0);
    }

    public void AddExperience(int amount)
    {
        currentExp += amount;
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            currentLevel++;
            maxExp = Mathf.RoundToInt(maxExp * 1.5f);

            if (levelUpSound != null && SettingsManager.Instance != null)
                SettingsManager.Instance.PlaySoundEffect(levelUpSound);
        }

        PlayerPrefs.SetInt("Player_Level", currentLevel);
        PlayerPrefs.SetInt("Player_Exp", currentExp);
        PlayerPrefs.SetInt("Player_MaxExp", maxExp);
        PlayerPrefs.Save();

        UpdateProfileUI();
    }

    public void UpdateProfileUI()
    {
        string lvlPrefix = Translator.GetText(54); // "Ур. " / "Lvl. " / "Seviye "
        if (levelBadgeText != null)
        {
            levelBadgeText.text = $"{lvlPrefix}{currentLevel}";
        }

        if (expProgressText != null)
        {
            expProgressText.text = $"{currentExp}/{maxExp} XP";
        }

        if (expProgressBar != null)
        {
            float fillRatio = maxExp > 0 ? Mathf.Clamp01((float)currentExp / maxExp) : 0f;
            expProgressBar.fillAmount = fillRatio;

            // 4-цветный градиент: Белый -> Зеленый -> Оранжевый -> Красный
            if (fillRatio <= 0.01f)
                expProgressBar.color = new Color(0.95f, 0.95f, 0.95f, 1f); // Белый
            else if (fillRatio < 0.45f)
                expProgressBar.color = new Color(0.2f, 0.85f, 0.35f, 1f); // Зеленый
            else if (fillRatio < 0.85f)
                expProgressBar.color = new Color(1f, 0.65f, 0.1f, 1f);   // Оранжевый
            else
                expProgressBar.color = new Color(0.95f, 0.2f, 0.2f, 1f);  // Красный
        }

        if (currentAvatarDisplayImage != null && allAvatars.Count > 0)
        {
            AvatarData cur = allAvatars.Find(a => a.id == selectedAvatarId);
            if (cur != null && cur.avatarSprite != null)
            {
                currentAvatarDisplayImage.sprite = cur.avatarSprite;
            }
        }

        if (currentFrameDisplayImage != null && allFrames.Count > 0)
        {
            FrameData curF = allFrames.Find(f => f.id == selectedFrameId);
            if (curF != null && curF.frameSprite != null)
            {
                currentFrameDisplayImage.sprite = curF.frameSprite;
            }
        }
    }

    public void SetAvatarButtonInteractable(bool interactable)
    {
        if (avatarIconButton != null)
        {
            avatarIconButton.interactable = interactable;
        }
    }

    public void OnAvatarIconClicked()
    {
        if (DialogueSystem_Manager.Instance != null && !DialogueSystem_Manager.Instance.CanInteractWithAvatarIcon())
        {
            return;
        }
        OpenAvatarPanel();
    }

    public void OpenAvatarPanel()
    {
        if (avatarPanel != null)
        {
            avatarPanel.SetActive(true);

            // Настройка размеров окна гардероба (ширина и высота для удобного отображения по 3 в ряд)
            RectTransform panelRect = avatarPanel.GetComponent<RectTransform>();
            if (panelRect != null && panelSize.x > 0 && panelSize.y > 0)
            {
                panelRect.sizeDelta = panelSize;
            }

            BuildAvatarGrid();
        }
    }

    public void CloseAvatarPanel()
    {
        if (avatarPanel != null)
        {
            avatarPanel.SetActive(false);
        }

        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.OnAvatarPanelClosed();
        }
    }

    private void InitDefaultData()
    {
        // 14 Рамок профиля (1 Бесплатная, 11 за Золото в магазине, 2 Премиум за Кристаллы)
        if (allFrames.Count == 0)
        {
            // Базовые 7 рамок
            allFrames.Add(new FrameData { id = 0, frameNameRU = "Стартовая Рамка Ученика", frameNameEN = "Starter Apprentice Frame", frameNameTR = "Başlangıç Çırak Çerçevesi", category = AvatarCategory.Free, isUnlockedByDefault = true });
            allFrames.Add(new FrameData { id = 1, frameNameRU = "Медная Рамка Лавки", frameNameEN = "Copper Shop Frame", frameNameTR = "Bakır Dükkan Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 5, goldPrice = 3000 });
            allFrames.Add(new FrameData { id = 2, frameNameRU = "Серебряная Рамка Мастера", frameNameEN = "Silver Master Frame", frameNameTR = "Gümüş Usta Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 5, goldPrice = 6000 });
            allFrames.Add(new FrameData { id = 3, frameNameRU = "Золотая Рамка Алхимика", frameNameEN = "Golden Alchemist Frame", frameNameTR = "Altın Simyacı Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 5, goldPrice = 10000 });
            allFrames.Add(new FrameData { id = 4, frameNameRU = "Королевская Изумрудная Рамка", frameNameEN = "Royal Emerald Frame", frameNameTR = "Kraliyet Zümrüt Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 10, goldPrice = 25000 });
            allFrames.Add(new FrameData { id = 5, frameNameRU = "Астральная Донатная Рамка", frameNameEN = "Astral Premium Frame", frameNameTR = "Astral Özel Çerçeve", category = AvatarCategory.Premium, unlockLevelRequired = 3, crystalPrice = 50 });
            allFrames.Add(new FrameData { id = 6, frameNameRU = "Божественная Солнечная Рамка", frameNameEN = "Divine Solar Frame", frameNameTR = "İlahi Güneş Çerçevesi", category = AvatarCategory.Premium, unlockLevelRequired = 5, crystalPrice = 100 });

            // 7 Дополнительных покупных рамок в Магазине (Shop)
            allFrames.Add(new FrameData { id = 7, frameNameRU = "Аметистовая Рамка Травника", frameNameEN = "Herbalist Amethyst Frame", frameNameTR = "Bitkici Ametist Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 6, goldPrice = 12000 });
            allFrames.Add(new FrameData { id = 8, frameNameRU = "Рубиновая Рамка Пламени", frameNameEN = "Flame Ruby Frame", frameNameTR = "Alev Yakut Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 7, goldPrice = 15000 });
            allFrames.Add(new FrameData { id = 9, frameNameRU = "Сапфировая Рамка Мороза", frameNameEN = "Frost Sapphire Frame", frameNameTR = "Buz Safir Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 8, goldPrice = 18000 });
            allFrames.Add(new FrameData { id = 10, frameNameRU = "Нефритовая Рамка Друида", frameNameEN = "Druid Jade Frame", frameNameTR = "Druid Yeşim Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 9, goldPrice = 22000 });
            allFrames.Add(new FrameData { id = 11, frameNameRU = "Обсидиановая Рамка Теней", frameNameEN = "Shadow Obsidian Frame", frameNameTR = "Gölge Obsidyen Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 11, goldPrice = 30000 });
            allFrames.Add(new FrameData { id = 12, frameNameRU = "Небесная Лазурная Рамка", frameNameEN = "Celestial Azure Frame", frameNameTR = "Göksel Azur Çerçevesi", category = AvatarCategory.Shop, unlockLevelRequired = 13, goldPrice = 35000 });
            allFrames.Add(new FrameData { id = 13, frameNameRU = "Древняя Руническая Рамка", frameNameEN = "Ancient Runic Frame", frameNameTR = "Kadim Rünik Çerçeve", category = AvatarCategory.Shop, unlockLevelRequired = 15, goldPrice = 40000 });
        }

        // Коллекция Аватарок
        if (allAvatars.Count == 0)
        {
            // Стартовые и уровни 1..20 (16 штук)
            int[] earlyLevels = new int[] { 0, 0, 0, 2, 4, 6, 8, 10, 12, 14, 15, 16, 17, 18, 19, 20 };
            for (int i = 0; i < earlyLevels.Length; i++)
            {
                allAvatars.Add(new AvatarData
                {
                    id = i,
                    avatarNameRU = (i < 3) ? $"Стартовый Ученик #{i + 1}" : $"Мастер {earlyLevels[i]} Уровня",
                    category = AvatarCategory.Free,
                    isUnlockedByDefault = (i < 3),
                    unlockLevelRequired = earlyLevels[i]
                });
            }

            // Гранд-Мастера: 30, 40, 50, 60, 70, 80, 90, 100 уровни (8 штук)
            int[] grandLevels = new int[] { 30, 40, 50, 60, 70, 80, 90, 100 };
            for (int i = 0; i < grandLevels.Length; i++)
            {
                allAvatars.Add(new AvatarData
                {
                    id = 16 + i,
                    avatarNameRU = $"Гранд-Алхимик {grandLevels[i]} Уровня",
                    category = AvatarCategory.Free,
                    isUnlockedByDefault = false,
                    unlockLevelRequired = grandLevels[i]
                });
            }

            // 5 Покупных аватарок (Обычный магазин с 5 уровня)
            for (int i = 0; i < 5; i++)
            {
                allAvatars.Add(new AvatarData
                {
                    id = 24 + i,
                    avatarNameRU = $"Мастер Лавки #{i + 1}",
                    category = AvatarCategory.Shop,
                    unlockLevelRequired = 5,
                    goldPrice = (i + 1) * 5000
                });
            }

            // 5 Премиум аватарок (Премиум магазин с 3 уровня)
            for (int i = 0; i < 5; i++)
            {
                allAvatars.Add(new AvatarData
                {
                    id = 29 + i,
                    avatarNameRU = $"Астральный Архимаг #{i + 1}",
                    category = AvatarCategory.Premium,
                    unlockLevelRequired = 3,
                    crystalPrice = (i + 1) * 20
                });
            }
        }
    }

    private void BuildAvatarGrid()
    {
        if (scrollContent == null) return;

        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // Переводимые заголовки через Translator (ID 58, 59, 60)
        CreateCategorySection(Translator.GetText(58), AvatarCategory.Free);
        CreateCategorySection(Translator.GetText(59), AvatarCategory.Shop);
        CreateCategorySection(Translator.GetText(60), AvatarCategory.Premium);
    }

    private void CreateCategorySection(string headerTitle, AvatarCategory cat)
    {
        if (categoryHeaderPrefab != null)
        {
            GameObject headerObj = Instantiate(categoryHeaderPrefab, scrollContent);
            TextMeshProUGUI txt = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = headerTitle;
        }

        List<AvatarData> catList = allAvatars.FindAll(a => a.category == cat);
        if (catList.Count == 0) return;

        // Создаем контейнер-сетку с GridLayoutGroup для размещения по 2-3 аватарки по горизонтали
        GameObject gridContainer = new GameObject($"GridSection_{cat}", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        gridContainer.transform.SetParent(scrollContent, false);

        GridLayoutGroup grid = gridContainer.GetComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = cellSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columnsCount);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.padding = new RectOffset(8, 8, 8, 16);

        ContentSizeFitter fitter = gridContainer.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (AvatarData data in catList)
        {
            CreateAvatarCell(data, gridContainer.transform);
        }
    }

    private void CreateAvatarCell(AvatarData data, Transform parentContainer)
    {
        if (avatarItemPrefab == null) return;

        Transform targetParent = parentContainer != null ? parentContainer : scrollContent;
        GameObject cell = Instantiate(avatarItemPrefab, targetParent);
        cell.name = $"Avatar_{data.id}";

        Image iconImg = cell.transform.Find("Avatar_Icon")?.GetComponent<Image>();
        Image frameImg = cell.transform.Find("Avatar_Frame")?.GetComponent<Image>();
        GameObject lockObj = cell.transform.Find("Lock_Overlay")?.gameObject;
        TextMeshProUGUI statusText = cell.transform.Find("Status_Text")?.GetComponent<TextMeshProUGUI>();
        Button cellBtn = cell.GetComponent<Button>();

        bool isUnlocked = IsAvatarUnlocked(data);
        bool isSelected = (selectedAvatarId == data.id);

        if (iconImg != null)
        {
            if (data.avatarSprite != null)
            {
                iconImg.sprite = data.avatarSprite;
                iconImg.color = Color.white;
                iconImg.enabled = true;
            }
            else
            {
                // Защита от белого квадрата: если спрайт еще не прикреплен, делаем темный полупрозрачный фон
                iconImg.sprite = null;
                iconImg.color = new Color(0.15f, 0.15f, 0.22f, 0.4f);
            }
        }

        if (lockObj != null)
        {
            lockObj.SetActive(!isUnlocked);
            Image lockImg = lockObj.GetComponent<Image>();
            if (lockImg != null)
            {
                lockImg.color = Color.white;
            }
        }

        if (statusText != null)
        {
            if (isSelected)
            {
                statusText.text = $"<color=#80FFDB><b>{Translator.GetText(55)}</b></color>"; // Выбрано
            }
            else if (isUnlocked)
            {
                statusText.text = $"<color=#FFE57F>{Translator.GetText(56)}</color>"; // Надеть
            }
            else
            {
                if (data.category == AvatarCategory.Free)
                {
                    statusText.text = $"<color=#FF758F>{Translator.GetText(54)}{data.unlockLevelRequired}</color>"; // Ур. X
                }
                else if (data.category == AvatarCategory.Shop)
                {
                    statusText.text = currentLevel < 5 
                        ? $"<color=#FF758F>{Translator.GetText(62)}</color>" // С 5 Ур.
                        : $"<color=#FFE57F>{data.goldPrice} G</color>";
                }
                else
                {
                    statusText.text = currentLevel < 3 
                        ? $"<color=#F384FF>{Translator.GetText(63)}</color>" // С 3 Ур.
                        : $"<color=#F384FF>{data.crystalPrice} C</color>";
                }
            }
        }

        if (cellBtn != null)
        {
            cellBtn.onClick.AddListener(() => OnSelectAvatar(data));
        }
    }

    public bool IsAvatarUnlocked(AvatarData data)
    {
        if (data.isUnlockedByDefault) return true;
        if (PlayerPrefs.GetInt($"Avatar_Unlocked_{data.id}", 0) == 1) return true;

        if (data.category == AvatarCategory.Free && currentLevel >= data.unlockLevelRequired && data.unlockLevelRequired > 0)
        {
            return true;
        }

        return false;
    }

    private void OnSelectAvatar(AvatarData data)
    {
        if (!IsAvatarUnlocked(data))
        {
            Debug.Log($"[AVATAR] {data.avatarNameRU} is locked!");
            return;
        }

        selectedAvatarId = data.id;
        PlayerPrefs.SetInt("Selected_Avatar_Id", selectedAvatarId);
        PlayerPrefs.Save();

        if (selectSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(selectSound);

        UpdateProfileUI();
        BuildAvatarGrid();
    }
}
