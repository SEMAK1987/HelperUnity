using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Глобальный игровой менеджер (GameManager) — синглтон для сохранения, загрузки и учета ресурсов.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Экономика и Прогресс")]
    public int gold = 0;
    public int crystals = 10;
    public int stones = 10;
    public int scrolls = 3;
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
    public TextMeshProUGUI stonesText;
    public TextMeshProUGUI scrollsText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI cauldronText;
    public Slider xpSlider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadResourcesFromPlayerPrefs();
        UpdateUI();
    }

    public void LoadResourcesFromPlayerPrefs()
    {
        gold = PlayerPrefs.GetInt("Player_Gold", 5000);
        crystals = PlayerPrefs.GetInt("Player_Crystals", 0);
        stones = PlayerPrefs.GetInt("Player_Stones", 10);
        scrolls = PlayerPrefs.GetInt("Player_Scrolls", 3);
        currentXP = PlayerPrefs.GetInt("Player_XP", 0);
        catLevel = PlayerPrefs.GetInt("Player_Level", 1);
    }

    public void SaveResourcesToPlayerPrefs()
    {
        PlayerPrefs.SetInt("Player_Gold", gold);
        PlayerPrefs.SetInt("Player_Crystals", crystals);
        PlayerPrefs.SetInt("Player_Stones", stones);
        PlayerPrefs.SetInt("Player_Scrolls", scrolls);
        PlayerPrefs.SetInt("Player_XP", currentXP);
        PlayerPrefs.SetInt("Player_Level", catLevel);
        PlayerPrefs.Save();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        SaveResourcesToPlayerPrefs();
        UpdateUI();
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.SyncPlayerPrefsResources();
        }
    }

    public void AddCrystals(int amount)
    {
        crystals += amount;
        SaveResourcesToPlayerPrefs();
        UpdateUI();
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.SyncPlayerPrefsResources();
        }
    }

    public void AddResources(int addGold, int addStones, int addScrolls, int addCrystals)
    {
        gold += addGold;
        stones += addStones;
        scrolls += addScrolls;
        crystals += addCrystals;
        SaveResourcesToPlayerPrefs();
        UpdateUI();
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.SyncPlayerPrefsResources();
        }
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        if (Avatar_Manager.Instance != null)
        {
            Avatar_Manager.Instance.GainPlayerExperience(amount);
        }
        SaveResourcesToPlayerPrefs();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = gold.ToString();
        if (crystalsText != null) crystalsText.text = crystals.ToString();
        if (stonesText != null) stonesText.text = stones.ToString();
        if (scrollsText != null) scrollsText.text = scrolls.ToString();
        if (levelText != null) levelText.text = catLevel.ToString();
        if (xpText != null) xpText.text = $"{currentXP}/{xpToNextLevel}";
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }
        if (cauldronText != null) cauldronText.text = $"Ур. {cauldronLevel}";
    }

    public void SyncUI()
    {
        UpdateUI();
    }
}
