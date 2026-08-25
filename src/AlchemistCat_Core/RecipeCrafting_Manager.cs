using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.38)
/// Полный контроллер интерактивного крафта первого рецепта и перехода к Сундуку-Инвентарю:
/// 1. Панель Старого Свитка: отображение ресурсов (100 Золота + 5 Камней + 1 Свиток = Зелье), кнопка "Начать".
/// 2. Активация Котла на столе и сидящего Маленького Кота с диалоговой подсказкой.
/// 3. Появление плашки "Изготовить" над котлом при нажатии.
/// 4. 5-секундный процесс варки со шкалой прогресса и таймером обратного отсчета (5.0s -> 0.0s).
/// 5. Кнопка "Забрать" -> начисление +10 XP (10/10 XP -> Level Up до 2 Ур. 0/20 XP).
/// 6. Возврат к Основному Коту-Алхимику: поздравление, появление иконки Сундука слева от свитка.
/// 7. Рассказ про хранение предметов в сундуке (свитки, зелья, части пазлов/скинов с суммированием одинаковых в стаках).
/// 8. Кнопка "Открыть инвентарь" -> открытие окна Инвентаря с некликабельной (пока) кнопкой закрытия.
/// </summary>
public class RecipeCrafting_Manager : MonoBehaviour
{
    public static RecipeCrafting_Manager Instance { get; private set; }

    [Header("1. Большой Свиток Рецепта")]
    public GameObject recipeScrollPanel;
    public Button startCraftButton;
    public TextMeshProUGUI startCraftButtonText;

    [Header("2. Стол, Котел и Маленький Кот")]
    public GameObject tableCauldronObject;     // Объект/Кнопка котла на столе
    public Button cauldronClickButton;
    public GameObject tableMiniCatObject;       // Маленький кот на столе
    public GameObject miniCatBubblePanel;       // Рамка-реплика маленького кота
    public TextMeshProUGUI miniCatBubbleText;

    [Header("3. Кнопка 'Изготовить' над Котлом")]
    public GameObject makeBadgeButtonObject;    // Плашка с надписью 'Изготовить'
    public Button makeBadgeButton;

    [Header("4. Шкала Прогресса Варки Зелья")]
    public GameObject craftingProgressBarContainer;
    public Image craftingProgressFill;
    public TextMeshProUGUI craftingTimerText;
    public float craftDurationSeconds = 5.0f;

    [Header("5. Кнопка 'Забрать' и Всплывающий Опыт (Floating XP)")]
    public GameObject claimPotionButtonObject;
    public Button claimPotionButton;
    public int firstRecipeRewardXP = 10;
    public GameObject floatingXPPrefab;          // Префаб/Объект всплывающей плашки опыта
    public RectTransform floatingXPSpawnPoint;   // Точка над котлом, откуда взлетает опыт
    public CanvasGroup floatingXPCanvasGroup;    // Для плавного исчезновения/осветления вверх

    [Header("Спрайты/Значки опыта для разных рецептов (5..1000 XP)")]
    public Image floatingXPImage;                // Иконка спрайта опыта
    public Sprite xpBadge5;
    public Sprite xpBadge10;
    public Sprite xpBadge20;
    public Sprite xpBadge30;
    public Sprite xpBadge50;
    public Sprite xpBadge100;
    public Sprite xpBadge200;
    public Sprite xpBadge300;
    public Sprite xpBadge500;
    public Sprite xpBadge1000;

    [Header("6. Иконка Сундука в верхнем UI")]
    public GameObject chestIconButton;         // Иконка сундучка (слева от свитка)
    public Button chestButton;

    [Header("7. Окно Инвентаря")]
    public GameObject inventoryPanel;
    public Button inventoryCloseButton;        // Некликабельная кнопка закрытия

    [Header("Звуки")]
    public AudioClip craftStartSound;
    public AudioClip craftCompleteSound;
    public AudioClip chestOpenSound;

    private Coroutine craftCoroutine;

    private void Awake()
    {
        Instance = this;

        if (startCraftButton != null)
        {
            startCraftButton.onClick.RemoveAllListeners();
            startCraftButton.onClick.AddListener(OnStartCraftButtonClicked);
        }

        if (cauldronClickButton != null)
        {
            cauldronClickButton.onClick.RemoveAllListeners();
            cauldronClickButton.onClick.AddListener(OnCauldronClicked);
        }

        if (makeBadgeButton != null)
        {
            makeBadgeButton.onClick.RemoveAllListeners();
            makeBadgeButton.onClick.AddListener(OnMakeBadgeClicked);
        }

        if (claimPotionButton != null)
        {
            claimPotionButton.onClick.RemoveAllListeners();
            claimPotionButton.onClick.AddListener(OnClaimPotionClicked);
        }

        if (chestButton != null)
        {
            chestButton.onClick.RemoveAllListeners();
            chestButton.onClick.AddListener(OnChestButtonClicked);
        }

        // По умолчанию вспомогательные плашки скрыты
        if (makeBadgeButtonObject != null) makeBadgeButtonObject.SetActive(false);
        if (craftingProgressBarContainer != null) craftingProgressBarContainer.SetActive(false);
        if (claimPotionButtonObject != null) claimPotionButtonObject.SetActive(false);
        if (miniCatBubblePanel != null) miniCatBubblePanel.SetActive(false);
        if (chestIconButton != null) chestIconButton.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    /// <summary>
    /// Шаг 1: Игрок открыл Старый Свиток и нажал кнопку 'Начать' внизу свитка
    /// </summary>
    public void OnStartCraftButtonClicked()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(craftStartSound);

        if (recipeScrollPanel != null) recipeScrollPanel.SetActive(false);

        // Появляется котел и маленький кот на столе
        if (tableCauldronObject != null) tableCauldronObject.SetActive(true);
        if (tableMiniCatObject != null) tableMiniCatObject.SetActive(true);

        if (miniCatBubblePanel != null)
        {
            miniCatBubblePanel.SetActive(true);
            if (miniCatBubbleText != null)
            {
                miniCatBubbleText.text = "Мяу! Нажми на котёл, чтобы начать приготовление зелья!";
            }
        }
    }

    /// <summary>
    /// Шаг 2: Нажатие на котел на столе -> появляется плашка 'Изготовить'
    /// </summary>
    public void OnCauldronClicked()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(craftStartSound);

        if (miniCatBubblePanel != null) miniCatBubblePanel.SetActive(false);
        if (makeBadgeButtonObject != null) makeBadgeButtonObject.SetActive(true);
    }

    /// <summary>
    /// Шаг 3: Нажатие на 'Изготовить' -> запуск шкалы варки на 5 секунд
    /// </summary>
    public void OnMakeBadgeClicked()
    {
        if (makeBadgeButtonObject != null) makeBadgeButtonObject.SetActive(false);

        if (craftCoroutine != null) StopCoroutine(craftCoroutine);
        craftCoroutine = StartCoroutine(CraftingProgressRoutine());
    }

    private IEnumerator CraftingProgressRoutine()
    {
        if (craftingProgressBarContainer != null) craftingProgressBarContainer.SetActive(true);
        if (craftingProgressFill != null) craftingProgressFill.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < craftDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / craftDurationSeconds);
            if (craftingProgressFill != null) craftingProgressFill.fillAmount = ratio;

            float remaining = Mathf.Max(0f, craftDurationSeconds - elapsed);
            if (craftingTimerText != null) craftingTimerText.text = $"{remaining:F1}s";

            yield return null;
        }

        if (craftingProgressFill != null) craftingProgressFill.fillAmount = 1f;
        if (craftingTimerText != null) craftingTimerText.text = "0.0s";

        yield return new WaitForSeconds(0.2f);

        if (craftingProgressBarContainer != null) craftingProgressBarContainer.SetActive(false);

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(craftCompleteSound);

        // Появляется кнопка 'Забрать'
        if (claimPotionButtonObject != null) claimPotionButtonObject.SetActive(true);
    }

    /// <summary>
    /// Шаг 4: Нажатие 'Забрать' -> скрытие котла и маленького кота, вылет "+10 XP" вверх со светлением, начисление XP
    /// </summary>
    public void OnClaimPotionClicked()
    {
        if (claimPotionButtonObject != null) claimPotionButtonObject.SetActive(false);
        if (tableCauldronObject != null) tableCauldronObject.SetActive(false);
        if (tableMiniCatObject != null) tableMiniCatObject.SetActive(false);
        if (miniCatBubblePanel != null) miniCatBubblePanel.SetActive(false);

        StartCoroutine(FlyFloatingXPAndProceed(firstRecipeRewardXP));
    }

    private IEnumerator FlyFloatingXPAndProceed(int xpAmount)
    {
        if (floatingXPPrefab != null)
        {
            floatingXPPrefab.SetActive(true);

            // Назначаем готовый спрайт опыта (5..1000 XP)
            if (floatingXPImage != null)
            {
                Sprite chosenSprite = GetSpriteForXp(xpAmount);
                if (chosenSprite != null) floatingXPImage.sprite = chosenSprite;
            }

            RectTransform rt = floatingXPPrefab.GetComponent<RectTransform>();
            Vector2 startPos = floatingXPSpawnPoint != null ? floatingXPSpawnPoint.anchoredPosition : (rt != null ? rt.anchoredPosition : Vector2.zero);
            if (rt != null) rt.anchoredPosition = startPos;

            float duration = 1.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Летит вверх на 140 пикселей
                if (rt != null)
                {
                    rt.anchoredPosition = startPos + new Vector2(0f, Mathf.Lerp(0f, 140f, t));
                }

                // Становится светлее и плавно растворяется
                if (floatingXPCanvasGroup != null)
                {
                    floatingXPCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t * t);
                }

                yield return null;
            }

            floatingXPPrefab.SetActive(false);
            if (floatingXPCanvasGroup != null) floatingXPCanvasGroup.alpha = 1f;
            if (rt != null) rt.anchoredPosition = startPos;
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        // Начисление опыта в профиль (10/10 XP -> повышает уровень до 2 Ур. 0/20 XP)
        if (Avatar_Manager.Instance != null)
        {
            Avatar_Manager.Instance.AddExperience(xpAmount);
        }

        // Запуск финальной фазы диалога с основным котом
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.StartPostCraftChestDialogue();
        }
    }

    private Sprite GetSpriteForXp(int xp)
    {
        switch (xp)
        {
            case 5: return xpBadge5;
            case 10: return xpBadge10;
            case 20: return xpBadge20;
            case 30: return xpBadge30;
            case 50: return xpBadge50;
            case 100: return xpBadge100;
            case 200: return xpBadge200;
            case 300: return xpBadge300;
            case 500: return xpBadge500;
            case 1000: return xpBadge1000;
            default: return xpBadge10;
        }
    }

    /// <summary>
    /// Шаг 5: Нажатие на сундучок в верхнем левом меню
    /// </summary>
    public void OnChestButtonClicked()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(chestOpenSound);

        OpenInventory();
    }

    public void OpenInventory()
    {
        if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.dialoguePanel != null)
        {
            DialogueSystem_Manager.Instance.dialoguePanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        // Кнопка закрытия пока некликабельна
        if (inventoryCloseButton != null)
        {
            inventoryCloseButton.interactable = false;
        }
    }
}
