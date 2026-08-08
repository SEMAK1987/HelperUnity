using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Управляет процессом улучшения оборудования лаборатории,
/// покупкой новых рецептов зелий и ростом алхимической мощи Кота.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("UI Тексты Стоимости")]
    public TextMeshProUGUI cauldronCostText;
    public TextMeshProUGUI kitchenCostText;
    public TextMeshProUGUI recipeCostText;

    [Header("UI Описания")]
    public TextMeshProUGUI cauldronStatsText;
    public TextMeshProUGUI kitchenStatsText;
    public TextMeshProUGUI recipeStatsText;

    [Header("Кнопки Улучшений")]
    public Button cauldronUpgradeButton;
    public Button kitchenUpgradeButton;
    public Button recipeUnlockButton;

    private int kitchenLevel = 1;
    private int recipeLevel = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadUpgradeData();
        RefreshUI();
    }

    private void LoadUpgradeData()
    {
        kitchenLevel = PlayerPrefs.GetInt("Alchemist_Kitchen_Lvl", 1);
        recipeLevel = PlayerPrefs.GetInt("Alchemist_Recipe_Lvl", 1);
    }

    private void SaveUpgradeData()
    {
        PlayerPrefs.SetInt("Alchemist_Kitchen_Lvl", kitchenLevel);
        PlayerPrefs.SetInt("Alchemist_Recipe_Lvl", recipeLevel);
        PlayerPrefs.Save();
    }

    public void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        // 1. Котел (Cauldron)
        int cauldronUpgradeCost = GameManager.Instance.cauldronLevel * 150;
        if (cauldronCostText != null) cauldronCostText.text = $"{cauldronUpgradeCost} Золота";
        if (cauldronStatsText != null) cauldronStatsText.text = $"Уровень Котла: {GameManager.Instance.cauldronLevel}\nСкорость варки: +{GameManager.Instance.cauldronLevel * 10}%";
        if (cauldronUpgradeButton != null) cauldronUpgradeButton.interactable = (GameManager.Instance.gold >= cauldronUpgradeCost);

        // 2. Кухня / Лаборатория (Kitchen)
        int kitchenUpgradeCost = kitchenLevel * 250;
        if (kitchenCostText != null) kitchenCostText.text = $"{kitchenUpgradeCost} Золота";
        if (kitchenStatsText != null) kitchenStatsText.text = $"Чистота Лаборатории: {kitchenLevel}\nПрирост опыта Кота: +{kitchenLevel * 15}%";
        if (kitchenUpgradeButton != null) kitchenUpgradeButton.interactable = (GameManager.Instance.gold >= kitchenUpgradeCost);

        // 3. Рецепты Зелий (Recipes)
        int recipeUpgradeCost = recipeLevel * 400;
        if (recipeCostText != null) recipeCostText.text = $"{recipeUpgradeCost} Золота";
        if (recipeStatsText != null) recipeStatsText.text = $"Сила рецептов: {recipeLevel}\nЦенность зелий: +{recipeLevel * 20}%";
        if (recipeUnlockButton != null) recipeUnlockButton.interactable = (GameManager.Instance.gold >= recipeUpgradeCost);
    }

    /// <summary>
    /// Метод улучшения котла через GameManager.
    /// </summary>
    public void BuyCauldronUpgrade()
    {
        if (GameManager.Instance == null) return;

        int cost = GameManager.Instance.cauldronLevel * 150;
        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.UpgradeCauldron();
            RefreshUI();
            PlayUpgradeSfx();
        }
    }

    /// <summary>
    /// Метод улучшения кухни/лаборатории.
    /// </summary>
    public void BuyKitchenUpgrade()
    {
        if (GameManager.Instance == null) return;

        int cost = kitchenLevel * 250;
        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.gold -= cost;
            kitchenLevel++;
            SaveUpgradeData();
            GameManager.Instance.SyncUI();
            RefreshUI();
            PlayUpgradeSfx();
            Debug.Log($"[ALCHEMIST UPGRADES] Кухня улучшена до уровня {kitchenLevel}!");
        }
    }

    /// <summary>
    /// Покупка / Усиление рецептов зелий.
    /// </summary>
    public void BuyRecipeUpgrade()
    {
        if (GameManager.Instance == null) return;

        int cost = recipeLevel * 400;
        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.gold -= cost;
            recipeLevel++;
            SaveUpgradeData();
            GameManager.Instance.SyncUI();
            RefreshUI();
            PlayUpgradeSfx();
            Debug.Log($"[ALCHEMIST UPGRADES] Сила рецептов выросла до уровня {recipeLevel}!");
        }
    }

    private void PlayUpgradeSfx()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayClickSound();
        }
    }
}
