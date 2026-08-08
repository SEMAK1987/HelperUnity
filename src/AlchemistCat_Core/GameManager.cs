using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Центральное ядро управления игрой "Алхимический Кот".
/// Хранит ресурсы, уровни, котел, и координирует все системы.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Экономика и Прогресс")]
    public int gold = 100;
    public int crystals = 0;
    public int vipXP = 0;
    public int daysActive = 1;
    public int catLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int cauldronLevel = 1;
    public int potionsBrewed = 0;

    [Header("Разблокированные Квесты / Игры")]
    public bool unlockedDarts = false;
    public bool unlockedMouseCatch = false;

    [Header("UI Ссылки на Ресурсы (Опционально)")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI cauldronText;
    public Slider xpSlider;

    private int activeSaveSlot = 0;

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

        // Загрузка активного слота
        activeSaveSlot = PlayerPrefs.GetInt("Alchemist_Active_Slot", 0);
        LoadGameOnStartup();
    }

    private void Start()
    {
        SyncUI();
    }

    private void LoadGameOnStartup()
    {
        if (PlayerPrefs.HasKey("Alchemist_Slot_Used_" + activeSaveSlot))
        {
            // Загружаем данные без смены сцены, так как мы уже в игре
            SaveGameSystem.Load(activeSaveSlot, false);
        }
        else
        {
            // Создаем новый файл
            SaveGameSystem.CurrentData = new SaveGameSystem.SaveData();
            SaveGameSystem.Save(activeSaveSlot);
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"[ALCHEMIST GAME] Начислено золота: {amount}. Всего: {gold}");
        SyncUI();
        AutoSave();
    }

    public void AddCrystals(int amount)
    {
        crystals += amount;
        Debug.Log($"[ALCHEMIST GAME] Начислены кристаллы: {amount}. Всего: {crystals}");
        SyncUI();
        AutoSave();
    }

    public void AddVipXP(int amount)
    {
        vipXP += amount;
        Debug.Log($"[ALCHEMIST GAME] Начислено VIP XP: {amount}. Всего: {vipXP}");
        AutoSave();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        xpToNextLevel = catLevel * 100;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            catLevel++;
            xpToNextLevel = catLevel * 100;
            OnCatLevelUp();
        }

        Debug.Log($"[ALCHEMIST GAME] Получен опыт: {amount}. Всего: {currentXP}/{xpToNextLevel}");
        SyncUI();
        AutoSave();
    }

    private void OnCatLevelUp()
    {
        Debug.Log($"[ALCHEMIST GAME] МЯУ! Ура! Наш Кот поднялся на уровень {catLevel}!");
        AddCrystals(2); // Подарок за уровень
        
        // Показ диалога или эффекта уровня
        if (DialogueSystem_Manager.Instance != null)
        {
            // Можем запустить кастомный диалог наставника
        }
    }

    public void UpgradeCauldron()
    {
        int cost = cauldronLevel * 150;
        if (gold >= cost)
        {
            gold -= cost;
            cauldronLevel++;
            Debug.Log($"[ALCHEMIST GAME] Котел улучшен до уровня {cauldronLevel}!");
            SyncUI();
            AutoSave();
        }
        else
        {
            Debug.LogWarning($"[ALCHEMIST GAME] Недостаточно золота для улучшения котла! Требуется: {cost}");
        }
    }

    public void SyncUI()
    {
        if (goldText != null) goldText.text = gold.ToString();
        if (crystalsText != null) crystalsText.text = crystals.ToString();
        
        string lvlPrefix = Translator.GetText(52); // "Уровень Кота: "
        if (levelText != null) levelText.text = $"{lvlPrefix}{catLevel}";
        
        xpToNextLevel = catLevel * 100;
        if (xpText != null) xpText.text = $"{currentXP} / {xpToNextLevel}";
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }

        if (cauldronText != null)
        {
            cauldronText.text = $"{Translator.GetText(30)}: Lvl {cauldronLevel}"; // Улучшить котел
        }
    }

    public void AutoSave()
    {
        SaveGameSystem.Save(activeSaveSlot);
    }

    private void OnApplicationQuit()
    {
        AutoSave();
    }
}
