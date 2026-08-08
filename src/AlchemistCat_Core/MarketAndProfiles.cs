using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Контроллер рынка ингредиентов и карточки профиля Кота.
/// </summary>
public class MarketAndProfiles : MonoBehaviour
{
    public static MarketAndProfiles Instance { get; private set; }

    [Header("UI Профиля")]
    public TextMeshProUGUI profNameText;
    public TextMeshProUGUI profLevelText;
    public TextMeshProUGUI profPotionsText;
    public TextMeshProUGUI profDaysText;

    [Header("UI Ссылки Рынка")]
    public Transform ingredientListContainer;
    public GameObject ingredientRowPrefab;

    [System.Serializable]
    public class Ingredient
    {
        public string nameRU;
        public string nameEN;
        public int buyCost;
        public Sprite icon;
    }

    [Header("База Ингредиентов")]
    public List<Ingredient> marketIngredients = new List<Ingredient>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        BuildDefaultIngredients();
        PopulateMarketUI();
        UpdateProfileCard();
    }

    private void BuildDefaultIngredients()
    {
        if (marketIngredients.Count > 0) return;

        marketIngredients.Add(new Ingredient { nameRU = "Хвост Летучей Мыши", nameEN = "Bat Tail", buyCost = 20 });
        marketIngredients.Add(new Ingredient { nameRU = "Светящийся Гриб", nameEN = "Glowing Mushroom", buyCost = 35 });
        marketIngredients.Add(new Ingredient { nameRU = "Корень Одуванчика", nameEN = "Dandelion Root", buyCost = 10 });
        marketIngredients.Add(new Ingredient { nameRU = "Глаз Тритона", nameEN = "Newt Eye", buyCost = 50 });
    }

    public void UpdateProfileCard()
    {
        if (GameManager.Instance == null) return;

        if (profNameText != null) profNameText.text = GameManager.Instance.catLevel >= 10 ? "Магистр Алхимии" : "Ученик Кота";
        if (profLevelText != null) profLevelText.text = $"Уровень: {GameManager.Instance.catLevel}";
        if (profPotionsText != null) profPotionsText.text = $"Сварено зелий: {GameManager.Instance.potionsBrewed}";
        if (profDaysText != null) profDaysText.text = $"Активных дней: {GameManager.Instance.daysActive}";
    }

    private void PopulateMarketUI()
    {
        if (ingredientListContainer == null || ingredientRowPrefab == null) return;

        // Очищаем старые строки
        foreach (Transform child in ingredientListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var ing in marketIngredients)
        {
            GameObject row = Instantiate(ingredientRowPrefab, ingredientListContainer);
            TextMeshProUGUI title = row.GetComponentInChildren<TextMeshProUGUI>();
            Button buyBtn = row.GetComponentInChildren<Button>();

            if (title != null)
            {
                string localizedName = Translator.GetText9(
                    ing.nameRU, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN
                );
                title.text = $"{localizedName} ({ing.buyCost} Золота)";
            }

            if (buyBtn != null)
            {
                buyBtn.onClick.AddListener(() => BuyIngredient(ing));
            }
        }
    }

    private void BuyIngredient(Ingredient ing)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.gold >= ing.buyCost)
        {
            GameManager.Instance.gold -= ing.buyCost;
            GameManager.Instance.AddXP(5);
            GameManager.Instance.SyncUI();
            UpdateProfileCard();
            
            if (CatController.Instance != null)
            {
                string localizedName = Translator.GetText9(
                    ing.nameRU, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN, ing.nameEN
                );
                CatController.Instance.ShowMeowBubble($"Мяу! Куплен ингредиент: {localizedName}!");
            }
            Debug.Log($"[ALCHEMIST MARKET] Куплен {ing.nameEN} за {ing.buyCost} золота.");
        }
        else
        {
            if (CatController.Instance != null)
            {
                CatController.Instance.ShowMeowBubble("Мяу... Не хватает золота!");
            }
            Debug.LogWarning("[ALCHEMIST MARKET] Недостаточно золота для покупки ингредиента.");
        }
    }
}
