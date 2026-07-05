using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Разработчик: Fate Continent (Континент Судьбы)
/// Zenith Multi-Tool Synergy (v18.7.0)
/// Скрипт глобальной системы сохранения и загрузки прогресса.
/// Интегрируется со слотами сохранения панели SaveSlots_Panel в меню.
/// </summary>
public static class SaveGameSystem
{
    // Текущие данные игры в оперативной памяти. Они перезаписываются при загрузке.
    public static SaveData CurrentData = new SaveData();

    // Флаг того, что игра была начата через Главное Меню / Выбор Персонажа
    public static bool IsStartedFromMenu = false;

    [Serializable]
    public class SaveData
    {
        [Header("Основное")]
        public string saveName = "Игрок";
        public string characterClass = "Воин";
        public int currentSceneIndex = 1;
        public string saveDateTime = "";
        public int selectedDifficulty = 0; // Сохраненная сложность (0 - новичок, 4 - кошмар)

        [Header("Характеристики")]
        public int playerLevel = 1;
        public int currentXP = 0;
        public int gold = 100;
        public float currentHealth = 100f;
        public float maxHealth = 100f;
        public int strength = 10;
        public int agility = 10;
        public int intelligence = 10;
        public int stamina = 10;
        
        [Header("Weapon Levels & Skill Points (v18.11.15)")]
        public int swordLevel = 1;
        public int bowLevel = 1;
        public int staffLevel = 1;
        public int availableSkillPoints = 30;

        [Header("Координаты (Позиция игрока в мире)")]
        public float posX = 0f;
        public float posY = 0f;
        public float posZ = 0f;

        [Header("Прогресс и Квесты")]
        public int activeQuestID = 0;
        public int miceCollected = 0; // Пример квестового объекта из нашего контекста (мыши бегают!)
        public string completedQuestsJSON = ""; // Можно сохранить список ID решенных квестов в JSON-строке

        [Header("Кампания (Континент Судьбы)")]
        public bool isContinentGameplayActive = false;
        public int landedZoneIndex = -1;
        public int currentCampaignDay = 1;
    }

    /// <summary>
    /// Полное сохранение игры в определенный слот.
    /// Передавайте от 0 до 2 (3 слота).
    /// </summary>
    public static void Save(int slotIndex)
    {
        // 1. Собираем актуальные данные перед сохранением
        CurrentData.currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        CurrentData.saveDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        CurrentData.isContinentGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1;
        CurrentData.landedZoneIndex = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        CurrentData.currentCampaignDay = PlayerPrefs.GetInt("Fate_Current_Day", 1);

        // ПРИМЕЧАНИЕ для разработчика: здесь вы можете собрать данные из ваших игровых менеджеров:
        // Пример:
        // if (PlayerController.Instance != null) {
        //     CurrentData.currentHealth = PlayerController.Instance.Health;
        //     CurrentData.posX = PlayerController.Instance.transform.position.x;
        //     ...
        // }

        // 2. Сериализация в формат JSON
        string json = JsonUtility.ToJson(CurrentData, true);

        // 3. Формируем краткое описание для превью на кнопке слота (Слот X - [Информация])
        string languageSuffix = GetLanguageInfoSuffix();
        string infoText = $"{CurrentData.saveDateTime} | {languageSuffix} {CurrentData.playerLevel} | {SceneManager.GetActiveScene().name}";

        // 4. Запись в PlayerPrefs
        PlayerPrefs.SetInt("Save_Slot_" + slotIndex, 1); // Флаг наличия сохранения
        PlayerPrefs.SetInt("Save_Slot_" + slotIndex + "_Scene", CurrentData.currentSceneIndex);
        PlayerPrefs.SetString("Save_Slot_" + slotIndex + "_Info", infoText);
        PlayerPrefs.SetString("Save_Slot_" + slotIndex + "_Data", json);
        
        PlayerPrefs.Save(); // Запись на физический диск

        Debug.Log($"[FATE SAVE] Прогресс игры успешно СОХРАНЕН в Слот {slotIndex}. Данные: {infoText}");
    }

    /// <summary>
    /// Полная загрузка игры из слота.
    /// Возвращает true, если слот успешно загружен.
    /// </summary>
    public static bool Load(int slotIndex, bool loadScene = true)
    {
        if (!PlayerPrefs.HasKey("Save_Slot_" + slotIndex))
        {
            Debug.LogWarning($"[FATE SAVE] Попытка загрузки пустого слота: {slotIndex}");
            return false;
        }

        // 1. Извлечение JSON строки и десериализация обратно в структуру класса
        string json = PlayerPrefs.GetString("Save_Slot_" + slotIndex + "_Data");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"[FATE SAVE] Ошибка: тело сохранения в слоте {slotIndex} пусто!");
            return false;
        }

        CurrentData = JsonUtility.FromJson<SaveData>(json);

        // Синхронизируем уровень сложности в PlayerPrefs, чтобы геймплей его подхватил
        PlayerPrefs.SetInt("Difficulty", CurrentData.selectedDifficulty);
        PlayerPrefs.SetInt("ContinentGameplayActive", CurrentData.isContinentGameplayActive ? 1 : 0);
        PlayerPrefs.SetInt("LandedZoneIndex", CurrentData.landedZoneIndex);
        PlayerPrefs.SetInt("Fate_Current_Day", CurrentData.currentCampaignDay);
        PlayerPrefs.Save();

        if (!loadScene)
        {
            Debug.Log($"[FATE SAVE] Данные прогресса успешно загружены в память из Слота {slotIndex} (без загрузки сцены).");
            return true;
        }

        // 2. Загружаем сцену, на которой остановился игрок
        int targetScene = PlayerPrefs.GetInt("Save_Slot_" + slotIndex + "_Scene", 1);
        
        Debug.Log($"[FATE SAVE] Прогресс успешно ЗАГРУЖЕН из Слота {slotIndex}. Запуск сцены {targetScene}.");

        // 3. Начинаем загрузку сцены через менеджер загрузок с анимацией (или напрямую)
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }

        // ПРИМЕЧАНИЕ для разработчика: после загрузки сцены вам нужно применить координаты!
        // В методе Start или Awake вашего игрового скрипта напишите:
        // if (PlayerController.Instance != null) {
        //     PlayerController.Instance.transform.position = new Vector3(SaveGameSystem.CurrentData.posX, SaveGameSystem.CurrentData.posY, SaveGameSystem.CurrentData.posZ);
        //     PlayerController.Instance.Health = SaveGameSystem.CurrentData.currentHealth;
        // }

        return true;
    }

    /// <summary>
    /// Очистка конкретного слота
    /// </summary>
    public static void DeleteSave(int slotIndex)
    {
        PlayerPrefs.DeleteKey("Save_Slot_" + slotIndex);
        PlayerPrefs.DeleteKey("Save_Slot_" + slotIndex + "_Scene");
        PlayerPrefs.DeleteKey("Save_Slot_" + slotIndex + "_Info");
        PlayerPrefs.DeleteKey("Save_Slot_" + slotIndex + "_Data");
        PlayerPrefs.Save();
        Debug.Log($"[FATE SAVE] Слот сохранения {slotIndex} успешно ОЧИЩЕН.");
    }

    /// <summary>
    /// Сброс текущей игровой сессии для "Новой Игры"
    /// </summary>
    public static void ResetData()
    {
        CurrentData = new SaveData();
        Debug.Log("[FATE SAVE] Текущая сессия сброшена (Готовность к Новой Игре).");
    }

    /// <summary>
    /// Полный сброс всех PlayerPrefs-ключей, связанных с кампанией, инвентарем, замками и прогрессом игрока.
    /// Вызывается при запуске Новой Игры, чтобы предотвратить перенос старых открытых слотов, экипировки и уровней замков.
    /// </summary>
    public static void ClearCampaignAndPlayerProgression()
    {
        // 1. Очистка инвентаря и экипировки
        PlayerPrefs.DeleteKey("Player_Inventory_JSON_v18");
        PlayerPrefs.DeleteKey("Player_Equipment_JSON_v18");
        PlayerPrefs.DeleteKey("Player_Inventory_Purchased_Slots");

        // 2. Сброс уровней замков, владельцев и разведданных для всех зон (до 16 зон)
        for (int i = 0; i < 16; i++)
        {
            PlayerPrefs.DeleteKey("Castle_Level_" + i);
            PlayerPrefs.DeleteKey("Castle_Owner_" + i);
            PlayerPrefs.DeleteKey("Castle_Spied_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Troops_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Armor_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Potions_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_CommanderLvl_" + i);
            
            // Очищаем войска в зонах
            string[] unitTypes = { "swordsman", "archer", "mage", "knight", "healer", "scout", "beast" };
            foreach (var ut in unitTypes)
            {
                PlayerPrefs.DeleteKey("Player_HiredCount_" + ut + "_Zone_" + i);
                PlayerPrefs.DeleteKey("Player_Unit_" + ut + "_Zone_" + i);
            }
        }

        // 3. Сброс дополнительных ключей прокачки и туториалов
        PlayerPrefs.DeleteKey("Aelyssa_Character_Tutorial_Done2");
        PlayerPrefs.DeleteKey("Player_Stats_Autonomous");
        PlayerPrefs.DeleteKey("Town_Selected_PotionLvl");
        PlayerPrefs.DeleteKey("Player_ArmyUnit_Rank");

        // Сброс зелий
        string[] potionIds = { "hp", "str", "def", "xp" };
        for (int lvl = 1; lvl <= 5; lvl++)
        {
            foreach (var pid in potionIds)
            {
                PlayerPrefs.DeleteKey($"Player_Potion_{pid}_Lvl_{lvl}");
            }
        }

        // Сброс тиров экипировки
        for (int slot = 1; slot <= 8; slot++)
        {
            PlayerPrefs.DeleteKey($"Player_Equipped_Tier_Slot_{slot}");
        }

        // Сброс опыта компаньонов
        string[] classes = { "warrior", "archer", "mage", "voin", "strelok", "mag" };
        foreach (var cl in classes)
        {
            PlayerPrefs.DeleteKey("Companion_Lvl_" + cl);
            PlayerPrefs.DeleteKey("Companion_XP_" + cl);
        }

        PlayerPrefs.Save();
        Debug.Log("[FATE SAVE] Все ключи инвентаря, экипировки, войск и замков предыдущих кампаний полностью СБРОШЕНЫ для Новой Игры!");
    }

    /// <summary>
    /// Вспомогательный метод для выбора слова "Уровень" на соответствующем языке к моменту сохранения
    /// </summary>
    private static string GetLanguageInfoSuffix()
    {
        int lang = PlayerPrefs.GetInt("Language", 0);
        switch (lang)
        {
            case 0: return "Ур.";       // Русский
            case 1: return "Lvl";       // Английский
            case 2: return "St.";       // Немецкий (Stufe)
            case 3: return "Niv.";      // Французский (Niveau)
            case 4: return "Nivel";     // Испанский
            case 5: return "Nív.";      // Португальский
            case 6: return "Lv";        // Японский
            case 7: return "레벨";       // Корейский
            case 8: return "等级";       // Китайский
            default: return "Lvl";
        }
    }
}
