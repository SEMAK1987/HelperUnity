using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.40)
/// Полный сценарий игрового обучения и интерактивных переходов:
/// 1. Ввод имени и знакомство с ресурсами (Золото, Камни, Свитки, Кристаллы).
/// 2. Начисление стартового бонуса (5k Gold, 10 Stones, 3 Scrolls).
/// 3. Показ Календаря Наград (иконка заблокирована во время монолога Кота, разблокируется и открывается только по кнопке "Открыть календарь >>").
/// 4. Закрытие календаря -> Диалог про Опыт Алхимии, Уровень (0/10 XP) и 4-цветную полоску опыта.
/// 5. Рассказ про Аватарки (Простые, Покупные, Премиум) -> Кнопка "Показать аватарки >>" -> Окно аватарок со скроллингом.
/// 6. Закрытие аватарок -> Согласие на первый опыт и изготовление первого рецепта за 100 золота, 5 камней, 1 свиток.
/// 7. Кнопка "Да, я согласен" -> Списание ресурсов, выдача опыта (+10 XP, повышение уровня), появление иконки свитка и открытие Большого Свитка Рецепта!
/// 8. После варки зелья в котле -> Появление сундука и открытие инвентаря.
/// </summary>
public class DialogueSystem_Manager : MonoBehaviour
{
    public static DialogueSystem_Manager Instance { get; private set; }

    [Header("Режим Тестирования")]
    public bool testModeResetOnStart = true;

    [Header("UI Связи Диалога")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueBodyText;

    [Header("UI Ввода Имени")]
    public GameObject nameInputContainer;
    public TMP_InputField nameInputField;
    public Button confirmNameButton;

    [Header("UI Кнопки Продолжения")]
    public Button nextStepButton;
    public TextMeshProUGUI nextStepButtonText;

    [Header("Верхняя панель ресурсов (TopPanel)")]
    public GameObject topPanel;
    public GameObject slotGold;
    public GameObject slotStones;
    public GameObject slotScrolls;
    public GameObject slotCrystals;

    [Header("Тексты количества ресурсов")]
    public TextMeshProUGUI goldAmountText;
    public TextMeshProUGUI stonesAmountText;
    public TextMeshProUGUI scrollsAmountText;
    public TextMeshProUGUI crystalsAmountText;

    [Header("Аватарка и Профиль Игрока (Avatar & Level Bar)")]
    public GameObject playerAvatarContainer; // Контейнер аватарки в левом верхнем углу
    public Avatar_Manager avatarManager;     // Ссылка на Avatar_Manager

    [Header("Иконка Календаря в игре")]
    public GameObject calendarIconButton;   // Маленькая иконка календаря

    [Header("Иконка Маленького Свитка в игре")]
    public GameObject smallScrollIconButton; // Маленький значок свитка слева от календаря
    public Button smallScrollButton;
    public bool autoAlignSmallScrollToCalendar = true;
    public Vector2 smallScrollOffsetFromCalendar = new Vector2(-110f, 0f);

    [Header("Панель Календаря Наград (Calendar Panel)")]
    public GameObject calendarPanel;
    public Calendar_Manager calendarManager;

    [Header("Большой Свиток Рецепта (Recipe Scroll Panel)")]
    public GameObject recipeScrollPanel;
    public Button recipeScrollCloseButton;
    public AudioClip scrollOpenSound;

    [Header("Объекты игрового мира")]
    public GameObject cauldronButton;
    public GameObject roomCatObject;

    [Header("Музыка и Звуки")]
    public AudioClip backgroundMusic;
    public AudioClip textTypeSound;
    public AudioClip buttonClickSound;
    public AudioClip coinRewardSound;
    public AudioClip levelUpSound;

    [Header("Иконка Сундука в верхнем UI")]
    public GameObject chestIconButton;      // Иконка сундучка (слева от свитка)
    public Button chestButton;
    public bool autoAlignChestToScroll = true;
    public Vector2 chestOffsetFromScroll = new Vector2(-110f, 0f);

    [Header("Иконка Знаний в верхнем UI")]
    public GameObject knowledgeIconButton;  // Иконка книг знаний (слева от сундука)
    public Button knowledgeButton;
    public Knowledge_Manager knowledgeManager;
    public bool autoAlignKnowledgeToChest = true;
    public Vector2 knowledgeOffsetFromChest = new Vector2(-110f, 0f);

    [Header("Иконка Колеса Мини-Игр в верхнем UI")]
    public GameObject minigamesWheelIconButton; // Иконка Колеса Мини-Игр
    public Button minigamesButton;
    public GameObject minigamesPanel;           // Окно мини-игр
    public bool autoAlignMinigamesToKnowledge = true;
    public Vector2 minigamesOffsetFromKnowledge = new Vector2(-110f, 0f);

    [Header("Окно Инвентаря")]
    public GameObject inventoryPanel;
    public Button inventoryCloseButton;

    [Header("Настройки")]
    public float textSpeed = 0.025f;

    [System.Serializable]
    public class DialogStep
    {
        public string textRU;
        public string textEN;
        public string textTR;
        public bool isNameInputStep = false;
        public int revealResourceIndex = -1; // 0=Gold, 1=Stones, 2=Scrolls, 3=Crystals, 4=All
        public bool isClaimStarterRewardStep = false;
        public bool showCalendarIcon = false;
        public bool isCalendarOpenStep = false;

        // Новые фазы
        public bool revealAvatarUI = false;
        public bool isAvatarShowStep = false;
        public bool isConfirmHelpStep = false;
        public bool isConfirmRecipeStep = false;
        public bool showSmallScrollIcon = false;
        public bool isRecipeStep = false;
        public bool showChestIcon = false;
        public bool isInventoryOpenStep = false;
        public bool showKnowledgeIcon = false;
        public bool isKnowledgeOpenStep = false;
        public bool showMinigamesIcon = false;
        public bool isConfirmMinigamesStep = false;
        public bool isMinigamesWheelOpenStep = false;
    }

    private List<DialogStep> dialogueSteps = new List<DialogStep>();
    private int currentStepIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string activeFullText = "";
    private string playerName = "Путник";
    private AudioSource localMusicSource;
    private bool starterRewardClaimed = false;

    // Ресурсы игрока
    private int currentGold = 0;
    private int currentStones = 0;
    private int currentScrolls = 0;
    private int currentCrystals = 0;

    // Фазы сценария
    public enum DialoguePhase
    {
        IntroAndCalendar, // Фаза 1: Приветствие, ресурсы, календарь
        AvatarAndExp,     // Фаза 2: Опыт, уровень, аватарки
        RecipeCrafting    // Фаза 3: Первый опыт, списание ресурсов, котел и рецепт
    }

    private DialoguePhase currentPhase = DialoguePhase.IntroAndCalendar;
    private bool calendarOpenedOnce = false;
    private bool avatarPanelOpenedOnce = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        SilenceMenuMusicSources();

        if (testModeResetOnStart)
        {
            PlayerPrefs.DeleteKey("Player_Gold");
            PlayerPrefs.DeleteKey("Player_Stones");
            PlayerPrefs.DeleteKey("Player_Scrolls");
            PlayerPrefs.DeleteKey("Player_Crystals");
            PlayerPrefs.DeleteKey("Alchemist_Player_Name");
            PlayerPrefs.DeleteKey("Player_Level");
            PlayerPrefs.DeleteKey("Player_Exp");
            PlayerPrefs.DeleteKey("Player_MaxExp");
            PlayerPrefs.Save();
        }

        currentGold = PlayerPrefs.GetInt("Player_Gold", 0);
        currentStones = PlayerPrefs.GetInt("Player_Stones", 0);
        currentScrolls = PlayerPrefs.GetInt("Player_Scrolls", 0);
        currentCrystals = PlayerPrefs.GetInt("Player_Crystals", 0);
        playerName = PlayerPrefs.GetString("Alchemist_Player_Name", "Путник");
    }

    private void Start()
    {
        StartBackgroundMusic();

        if (dialogueBodyText != null)
        {
            dialogueBodyText.enableAutoSizing = true;
            dialogueBodyText.fontSizeMin = 18;
            dialogueBodyText.fontSizeMax = 28;
            dialogueBodyText.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (topPanel != null) topPanel.SetActive(false);
        if (slotGold != null) slotGold.SetActive(false);
        if (slotStones != null) slotStones.SetActive(false);
        if (slotScrolls != null) slotScrolls.SetActive(false);
        if (slotCrystals != null) slotCrystals.SetActive(false);

        if (playerAvatarContainer != null) playerAvatarContainer.SetActive(false);
        if (calendarIconButton != null) calendarIconButton.SetActive(false);
        if (smallScrollIconButton != null) smallScrollIconButton.SetActive(false);
        if (chestIconButton != null) chestIconButton.SetActive(false);

        if (calendarPanel != null) calendarPanel.SetActive(false);
        if (recipeScrollPanel != null) recipeScrollPanel.SetActive(false);
        if (cauldronButton != null) cauldronButton.SetActive(false);
        if (roomCatObject != null) roomCatObject.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        UpdateResourceTextsInstant();

        if (confirmNameButton != null)
            confirmNameButton.onClick.AddListener(OnConfirmNameClicked);

        if (nextStepButton != null)
            nextStepButton.onClick.AddListener(OnNextStepClicked);

        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(delegate { OnConfirmNameClicked(); });
            nameInputField.characterLimit = 12;
            nameInputField.lineType = TMP_InputField.LineType.SingleLine;
        }

        if (recipeScrollCloseButton != null)
            recipeScrollCloseButton.onClick.AddListener(CloseRecipeScrollUI);

        if (smallScrollButton != null)
            smallScrollButton.onClick.AddListener(OnSmallScrollButtonClicked);

        if (inventoryCloseButton != null)
            inventoryCloseButton.onClick.AddListener(() => {
                if (inventoryPanel != null) inventoryPanel.SetActive(false);
            });

        if (chestButton != null)
            chestButton.onClick.AddListener(() => {
                if (RecipeCrafting_Manager.Instance != null)
                    RecipeCrafting_Manager.Instance.OpenInventory();
                else if (inventoryPanel != null)
                    inventoryPanel.SetActive(true);
            });

        // Старт Фазы 1
        currentPhase = DialoguePhase.IntroAndCalendar;
        BuildIntroScenario();
        DisplayStep(0);
    }

    private void SilenceMenuMusicSources()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var s in sources)
        {
            if (s != null && s.clip != null)
            {
                string clipName = s.clip.name.ToLower();
                if (clipName.Contains("main") || clipName.Contains("menu") || clipName.Contains("intro"))
                {
                    s.Stop();
                    s.volume = 0f;
                }
            }
        }
    }

    private void StartBackgroundMusic()
    {
        if (backgroundMusic == null) return;
        if (localMusicSource == null) localMusicSource = gameObject.AddComponent<AudioSource>();

        localMusicSource.clip = backgroundMusic;
        localMusicSource.loop = true;
        localMusicSource.playOnAwake = false;
        localMusicSource.volume = 0.35f;
        localMusicSource.Play();
    }

    public void OnNextStepClicked()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping = false;
            if (dialogueBodyText != null) dialogueBodyText.text = activeFullText;
            OnTypingFinished();
            return;
        }

        if (currentStepIndex < 0 || currentStepIndex >= dialogueSteps.Count) return;
        DialogStep currentStep = dialogueSteps[currentStepIndex];

        // Если это шаг сбора стартовой награды
        if (currentStep.isClaimStarterRewardStep && !starterRewardClaimed)
        {
            StartCoroutine(AnimateStarterRewardAndContinue());
            return;
        }

        // Если это шаг открытия календаря
        if (currentStep.isCalendarOpenStep)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            SetCalendarButtonInteractable(true);
            OpenCalendarUI();
            return;
        }

        // Если это шаг открытия окна аватарок
        if (currentStep.isAvatarShowStep)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (avatarManager != null)
            {
                avatarManager.SetAvatarButtonInteractable(true);
                avatarManager.OpenAvatarPanel();
            }
            return;
        }

        // Если это шаг согласия на изготовление первого рецепта (100 золота, 5 камней, 1 свиток)
        if (currentStep.isConfirmRecipeStep)
        {
            StartCoroutine(ProcessFirstRecipeCraftAndContinue());
            return;
        }

        // Если это шаг открытия Большого Свитка Рецепта
        if (currentStep.isRecipeStep)
        {
            OpenRecipeScrollUI();
            return;
        }

        // Если это шаг открытия Инвентаря
        if (currentStep.isInventoryOpenStep)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (RecipeCrafting_Manager.Instance != null)
            {
                RecipeCrafting_Manager.Instance.OpenInventory();
            }
            else if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                if (inventoryCloseButton != null) inventoryCloseButton.interactable = false;
            }
            return;
        }

        // Если это шаг открытия Окна Знаний
        if (currentStep.isKnowledgeOpenStep)
        {
            OpenKnowledgeUI();
            return;
        }

        // Если это шаг открытия Колеса Мини-Игр
        if (currentStep.isMinigamesWheelOpenStep)
        {
            OpenMinigamesWheelUI();
            return;
        }

        currentStepIndex++;
        if (currentStepIndex < dialogueSteps.Count)
        {
            DisplayStep(currentStepIndex);
        }
        else
        {
            if (currentPhase == DialoguePhase.IntroAndCalendar)
            {
                SetCalendarButtonInteractable(true);
                OpenCalendarUI();
            }
        }
    }

    private void DisplayStep(int index)
    {
        if (index < 0 || index >= dialogueSteps.Count) return;

        if (nameInputContainer != null) nameInputContainer.SetActive(false);
        if (nextStepButton != null) nextStepButton.gameObject.SetActive(false);

        DialogStep step = dialogueSteps[index];

        HandleResourceReveal(step.revealResourceIndex);

        if (step.showCalendarIcon && calendarIconButton != null)
        {
            calendarIconButton.SetActive(true);
            SetCalendarButtonInteractable(false);
        }

        if (step.revealAvatarUI && playerAvatarContainer != null)
        {
            playerAvatarContainer.SetActive(true);
            if (avatarManager != null)
            {
                avatarManager.UpdateProfileUI();
                avatarManager.SetAvatarButtonInteractable(false);
            }
        }

        if (step.showSmallScrollIcon && smallScrollIconButton != null)
        {
            AlignSmallScrollButtonToCalendar();
            smallScrollIconButton.SetActive(true);
            SetSmallScrollInteractable(false);
        }

        if (step.showChestIcon && chestIconButton != null)
        {
            AlignChestButtonToSmallScroll();
            chestIconButton.SetActive(true);
            SetChestButtonInteractable(false);
        }

        if (step.showKnowledgeIcon && knowledgeIconButton != null)
        {
            AlignKnowledgeButtonToChest();
            knowledgeIconButton.SetActive(true);
            SetKnowledgeButtonInteractable(false);
        }

        if (step.showMinigamesIcon && minigamesWheelIconButton != null)
        {
            AlignMinigamesButtonToKnowledge();
            minigamesWheelIconButton.SetActive(true);
            SetMinigamesButtonInteractable(false);
        }

        string rawText = GetLocalizedText(step.textRU, step.textEN, step.textTR);
        activeFullText = FormatPlayerName(rawText);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextCoroutine(activeFullText, step));
    }

    private void HandleResourceReveal(int resourceIndex)
    {
        if (resourceIndex == -1) return;
        if (topPanel != null) topPanel.SetActive(true);

        if (resourceIndex == 0)
        {
            if (slotGold != null) slotGold.SetActive(true);
        }
        else if (resourceIndex == 1)
        {
            if (slotGold != null) slotGold.SetActive(true);
            if (slotStones != null) slotStones.SetActive(true);
        }
        else if (resourceIndex == 2)
        {
            if (slotGold != null) slotGold.SetActive(true);
            if (slotStones != null) slotStones.SetActive(true);
            if (slotScrolls != null) slotScrolls.SetActive(true);
        }
        else if (resourceIndex >= 3)
        {
            if (slotGold != null) slotGold.SetActive(true);
            if (slotStones != null) slotStones.SetActive(true);
            if (slotScrolls != null) slotScrolls.SetActive(true);
            if (slotCrystals != null) slotCrystals.SetActive(true);
        }
    }

    private IEnumerator TypeTextCoroutine(string text, DialogStep step)
    {
        isTyping = true;
        if (dialogueBodyText != null) dialogueBodyText.text = "";

        int length = text.Length;
        int i = 0;

        while (i < length)
        {
            if (text[i] == '<')
            {
                int closeIndex = text.IndexOf('>', i);
                if (closeIndex != -1)
                {
                    i = closeIndex + 1;
                    if (dialogueBodyText != null) dialogueBodyText.text = text.Substring(0, i);
                    continue;
                }
            }

            i++;
            if (dialogueBodyText != null) dialogueBodyText.text = text.Substring(0, i);

            if (textTypeSound != null && SettingsManager.Instance != null && i - 1 < length && text[i - 1] != ' ')
            {
                SettingsManager.Instance.PlaySoundEffect(textTypeSound);
            }
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        OnTypingFinished();
    }

    private void OnTypingFinished()
    {
        if (currentStepIndex < 0 || currentStepIndex >= dialogueSteps.Count) return;
        DialogStep step = dialogueSteps[currentStepIndex];

        if (step.isNameInputStep)
        {
            if (nameInputContainer != null)
            {
                nameInputContainer.SetActive(true);
                if (nameInputField != null)
                {
                    nameInputField.Select();
                    nameInputField.ActivateInputField();
                }
            }
        }
        else
        {
            if (nextStepButton != null)
            {
                nextStepButton.gameObject.SetActive(true);
                nextStepButton.interactable = true;
                if (nextStepButtonText != null)
                {
                    nextStepButtonText.enableAutoSizing = true;
                    nextStepButtonText.fontSizeMin = 14;
                    nextStepButtonText.fontSizeMax = 22;
                    nextStepButtonText.textWrappingMode = TextWrappingModes.NoWrap;
                    nextStepButtonText.overflowMode = TextOverflowModes.Ellipsis;

                    if (step.isClaimStarterRewardStep)
                        nextStepButtonText.text = "Забрать бонус!";
                    else if (step.isCalendarOpenStep)
                        nextStepButtonText.text = "Открыть календарь";
                    else if (step.isAvatarShowStep)
                        nextStepButtonText.text = "Показать аватарки";
                    else if (step.isConfirmHelpStep)
                        nextStepButtonText.text = "Согласен";
                    else if (step.isConfirmRecipeStep)
                        nextStepButtonText.text = "Да, я согласен!";
                    else if (step.isRecipeStep)
                        nextStepButtonText.text = "Посмотреть рецепт";
                    else if (step.isInventoryOpenStep)
                        nextStepButtonText.text = "Открыть инвентарь";
                    else if (step.isKnowledgeOpenStep)
                        nextStepButtonText.text = "Изучить знания";
                    else if (step.isConfirmMinigamesStep)
                        nextStepButtonText.text = "Я согласен!";
                    else if (step.isMinigamesWheelOpenStep)
                        nextStepButtonText.text = "Открыть Колесо Игр";
                    else
                        nextStepButtonText.text = "Далее";
                }
            }
        }
    }

    public void OnConfirmNameClicked()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        string rawEntered = "";
        if (nameInputField != null) rawEntered = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(rawEntered))
        {
            ShowNameValidationError("Мяу! Пожалуйста, введи своё имя в поле ниже.");
            return;
        }

        string cleanedName = Regex.Replace(rawEntered, @"[^a-zA-Zа-яА-ЯёЁ0-9çÇğĞıİöÖşŞüÜ\-]", "").Trim();

        if (cleanedName.Length < 2 || cleanedName.Length > 12)
        {
            ShowNameValidationError("Имя должно быть от 2 до 12 букв (без спецсимволов)!");
            return;
        }

        playerName = cleanedName;
        PlayerPrefs.SetString("Alchemist_Player_Name", playerName);
        PlayerPrefs.Save();

        if (nameInputContainer != null) 
            nameInputContainer.SetActive(false);

        currentStepIndex++;
        DisplayStep(currentStepIndex);
    }

    private void ShowNameValidationError(string message)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        if (dialogueBodyText != null) dialogueBodyText.text = $"<color=#FF758F>{message}</color>";

        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private IEnumerator AnimateStarterRewardAndContinue()
    {
        if (nextStepButton != null) nextStepButton.interactable = false;

        if (coinRewardSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(coinRewardSound);

        starterRewardClaimed = true;
        int targetGold = 5000;
        int targetStones = 10;
        int targetScrolls = 3;
        int targetCrystals = 0;

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            int animGold = (int)Mathf.Lerp(0, targetGold, t);
            int animStones = (int)Mathf.Lerp(0, targetStones, t);
            int animScrolls = (int)Mathf.Lerp(0, targetScrolls, t);

            if (goldAmountText != null) goldAmountText.text = FormatNumber(animGold);
            if (stonesAmountText != null) stonesAmountText.text = animStones.ToString();
            if (scrollsAmountText != null) scrollsAmountText.text = animScrolls.ToString();
            if (crystalsAmountText != null) crystalsAmountText.text = "0";

            yield return null;
        }

        currentGold = targetGold;
        currentStones = targetStones;
        currentScrolls = targetScrolls;
        currentCrystals = targetCrystals;

        PlayerPrefs.SetInt("Player_Gold", currentGold);
        PlayerPrefs.SetInt("Player_Stones", currentStones);
        PlayerPrefs.SetInt("Player_Scrolls", currentScrolls);
        PlayerPrefs.SetInt("Player_Crystals", currentCrystals);
        PlayerPrefs.Save();

        UpdateResourceTextsInstant();

        yield return new WaitForSeconds(0.3f);

        currentStepIndex++;
        DisplayStep(currentStepIndex);
    }

    private IEnumerator ProcessFirstRecipeCraftAndContinue()
    {
        if (nextStepButton != null) nextStepButton.interactable = false;

        // Списание ресурсов: 100 золота, 5 камней, 1 свиток
        currentGold = Mathf.Max(0, currentGold - 100);
        currentStones = Mathf.Max(0, currentStones - 5);
        currentScrolls = Mathf.Max(0, currentScrolls - 1);

        PlayerPrefs.SetInt("Player_Gold", currentGold);
        PlayerPrefs.SetInt("Player_Stones", currentStones);
        PlayerPrefs.SetInt("Player_Scrolls", currentScrolls);
        PlayerPrefs.Save();

        UpdateResourceTextsInstant();

        if (coinRewardSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(coinRewardSound);

        yield return new WaitForSeconds(0.3f);

        if (smallScrollIconButton != null)
        {
            smallScrollIconButton.SetActive(true);
            SetSmallScrollInteractable(true);
        }

        currentStepIndex++;
        DisplayStep(currentStepIndex);
    }

    private void UpdateResourceTextsInstant()
    {
        if (goldAmountText != null) goldAmountText.text = FormatNumber(currentGold);
        if (stonesAmountText != null) stonesAmountText.text = currentStones.ToString();
        if (scrollsAmountText != null) scrollsAmountText.text = currentScrolls.ToString();
        if (crystalsAmountText != null) crystalsAmountText.text = currentCrystals.ToString();
    }

    private string FormatNumber(int num)
    {
        if (num >= 1000000) return (num / 1000000f).ToString("0.#") + "M";
        if (num >= 10000) return (num / 1000f).ToString("0.#") + "K";
        return num.ToString("N0");
    }

    private string GetLocalizedText(string ru, string en, string tr)
    {
        string currentLang = PlayerPrefs.GetString("Selected_Language", "RU");
        if (currentLang == "EN") return en;
        if (currentLang == "TR") return !string.IsNullOrEmpty(tr) ? tr : en;
        return ru;
    }

    private string FormatPlayerName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return raw.Replace("{PLAYER_NAME}", $"<b><color=#FFE57F>{playerName}</color></b>");
    }

    [Header("Состояние Режима Варки Зелья")]
    public bool isCraftingInProgress = false;

    public bool CanInteractWithAvatarIcon()
    {
        if (isCraftingInProgress) return false;
        return (dialoguePanel == null || !dialoguePanel.activeSelf);
    }

    public void SetCalendarButtonInteractable(bool interactable)
    {
        if (calendarIconButton != null)
        {
            Button btn = calendarIconButton.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    public void SetSmallScrollInteractable(bool interactable)
    {
        if (smallScrollButton != null)
        {
            smallScrollButton.interactable = interactable;
        }
        else if (smallScrollIconButton != null)
        {
            Button btn = smallScrollIconButton.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    public void AlignSmallScrollButtonToCalendar()
    {
        if (!autoAlignSmallScrollToCalendar) return;
        if (smallScrollIconButton != null && calendarIconButton != null)
        {
            RectTransform calRect = calendarIconButton.GetComponent<RectTransform>();
            RectTransform scrollRect = smallScrollIconButton.GetComponent<RectTransform>();
            if (calRect != null && scrollRect != null)
            {
                if (smallScrollIconButton.transform.parent != calendarIconButton.transform.parent)
                {
                    smallScrollIconButton.transform.SetParent(calendarIconButton.transform.parent, false);
                }
                scrollRect.anchorMin = calRect.anchorMin;
                scrollRect.anchorMax = calRect.anchorMax;
                scrollRect.pivot = calRect.pivot;
                scrollRect.anchoredPosition = calRect.anchoredPosition + smallScrollOffsetFromCalendar;
                scrollRect.sizeDelta = calRect.sizeDelta;
            }

            // Очищаем стандартный текст кнопки, если он присутствует
            TextMeshProUGUI tmp = smallScrollIconButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "";
            UnityEngine.UI.Text unityTxt = smallScrollIconButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (unityTxt != null) unityTxt.text = "";
        }
    }

    public void SetChestButtonInteractable(bool interactable)
    {
        if (chestButton != null)
        {
            chestButton.interactable = interactable;
        }
        else if (chestIconButton != null)
        {
            Button btn = chestIconButton.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    public void AlignChestButtonToSmallScroll()
    {
        if (!autoAlignChestToScroll) return;
        if (chestIconButton != null)
        {
            // Базовый референс для родителя и размера
            GameObject refObj = smallScrollIconButton != null ? smallScrollIconButton : calendarIconButton;
            if (refObj != null)
            {
                RectTransform refRect = refObj.GetComponent<RectTransform>();
                RectTransform chestRect = chestIconButton.GetComponent<RectTransform>();
                if (refRect != null && chestRect != null)
                {
                    // Если сундук случайно был вложен в TopPanel, переносим его к свитковым иконкам
                    if (chestIconButton.transform.parent != refObj.transform.parent)
                    {
                        chestIconButton.transform.SetParent(refObj.transform.parent, false);
                    }

                    chestRect.anchorMin = refRect.anchorMin;
                    chestRect.anchorMax = refRect.anchorMax;
                    chestRect.pivot = refRect.pivot;

                    // Размер сундука точно такой же, как у свитка и календаря
                    chestRect.sizeDelta = refRect.sizeDelta;

                    if (smallScrollIconButton != null)
                    {
                        RectTransform scrollRect = smallScrollIconButton.GetComponent<RectTransform>();
                        chestRect.anchoredPosition = scrollRect.anchoredPosition + chestOffsetFromScroll;
                    }
                    else if (calendarIconButton != null)
                    {
                        RectTransform calRect = calendarIconButton.GetComponent<RectTransform>();
                        chestRect.anchoredPosition = calRect.anchoredPosition + (smallScrollOffsetFromCalendar * 2f);
                    }
                }
            }

            // Очищаем стандартный текст "Button" на сундучке
            TextMeshProUGUI[] tmps = chestIconButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps) t.text = "";
            UnityEngine.UI.Text[] texts = chestIconButton.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in texts) t.text = "";
        }
    }

    public void SetKnowledgeButtonInteractable(bool interactable)
    {
        if (knowledgeButton != null)
        {
            knowledgeButton.interactable = interactable;
        }
        else if (knowledgeIconButton != null)
        {
            Button btn = knowledgeIconButton.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    public void AlignKnowledgeButtonToChest()
    {
        if (!autoAlignKnowledgeToChest) return;
        if (knowledgeIconButton != null)
        {
            GameObject refObj = chestIconButton != null ? chestIconButton : (smallScrollIconButton != null ? smallScrollIconButton : calendarIconButton);
            if (refObj != null)
            {
                RectTransform refRect = refObj.GetComponent<RectTransform>();
                RectTransform knowRect = knowledgeIconButton.GetComponent<RectTransform>();
                if (refRect != null && knowRect != null)
                {
                    if (knowledgeIconButton.transform.parent != refObj.transform.parent)
                    {
                        knowledgeIconButton.transform.SetParent(refObj.transform.parent, false);
                    }

                    knowRect.anchorMin = refRect.anchorMin;
                    knowRect.anchorMax = refRect.anchorMax;
                    knowRect.pivot = refRect.pivot;
                    knowRect.sizeDelta = refRect.sizeDelta;

                    if (chestIconButton != null)
                    {
                        RectTransform chestRect = chestIconButton.GetComponent<RectTransform>();
                        knowRect.anchoredPosition = chestRect.anchoredPosition + knowledgeOffsetFromChest;
                    }
                    else
                    {
                        knowRect.anchoredPosition = refRect.anchoredPosition + knowledgeOffsetFromChest;
                    }
                }
            }

            // Очищаем стандартный текст "Button"
            TextMeshProUGUI[] tmps = knowledgeIconButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps) t.text = "";
            UnityEngine.UI.Text[] texts = knowledgeIconButton.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in texts) t.text = "";
        }
    }

    public void OnKnowledgeButtonClicked()
    {
        if (isCraftingInProgress || (dialoguePanel != null && dialoguePanel.activeSelf))
        {
            Debug.Log("[ALCHEMIST] Раздел 'Знания' заблокирован во время варки зелья или речи Кота.");
            return;
        }

        OpenKnowledgeUI();
    }

    public void OpenKnowledgeUI()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (Knowledge_Manager.Instance != null)
        {
            Knowledge_Manager.Instance.OpenKnowledgeUI();
        }
        else if (knowledgeManager != null)
        {
            knowledgeManager.OpenKnowledgeUI();
        }
    }

    public void SetMinigamesButtonInteractable(bool interactable)
    {
        if (minigamesButton != null)
        {
            minigamesButton.interactable = interactable;
        }
        else if (minigamesWheelIconButton != null)
        {
            Button btn = minigamesWheelIconButton.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    public void AlignMinigamesButtonToKnowledge()
    {
        if (!autoAlignMinigamesToKnowledge) return;
        if (minigamesWheelIconButton != null)
        {
            GameObject refObj = knowledgeIconButton != null ? knowledgeIconButton : (chestIconButton != null ? chestIconButton : smallScrollIconButton);
            if (refObj != null)
            {
                RectTransform refRect = refObj.GetComponent<RectTransform>();
                RectTransform wheelRect = minigamesWheelIconButton.GetComponent<RectTransform>();
                if (refRect != null && wheelRect != null)
                {
                    if (minigamesWheelIconButton.transform.parent != refObj.transform.parent)
                    {
                        minigamesWheelIconButton.transform.SetParent(refObj.transform.parent, false);
                    }

                    wheelRect.anchorMin = refRect.anchorMin;
                    wheelRect.anchorMax = refRect.anchorMax;
                    wheelRect.pivot = refRect.pivot;
                    wheelRect.sizeDelta = refRect.sizeDelta;

                    if (knowledgeIconButton != null)
                    {
                        RectTransform knowRect = knowledgeIconButton.GetComponent<RectTransform>();
                        wheelRect.anchoredPosition = knowRect.anchoredPosition + minigamesOffsetFromKnowledge;
                    }
                    else
                    {
                        wheelRect.anchoredPosition = refRect.anchoredPosition + minigamesOffsetFromKnowledge;
                    }
                }
            }

            // Очищаем стандартный текст "Button"
            TextMeshProUGUI[] tmps = minigamesWheelIconButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps) t.text = "";
            UnityEngine.UI.Text[] texts = minigamesWheelIconButton.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in texts) t.text = "";
        }
    }

    public void OnMinigamesButtonClicked()
    {
        if (isCraftingInProgress || (dialoguePanel != null && dialoguePanel.activeSelf))
        {
            Debug.Log("[ALCHEMIST] Колесо Мини-Игр заблокировано во время речи Кота.");
            return;
        }

        OpenMinigamesWheelUI();
    }

    public void OpenMinigamesWheelUI()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (minigamesPanel != null)
        {
            minigamesPanel.SetActive(true);
        }
        else
        {
            GameObject foundPanel = GameObject.Find("MinigamesPanel");
            if (foundPanel != null) foundPanel.SetActive(true);
        }
    }

    public void OnCalendarIconButtonClicked()
    {
        if (isCraftingInProgress || (dialoguePanel != null && dialoguePanel.activeSelf))
        {
            Debug.Log("[ALCHEMIST] Календарь заблокирован во время варки зелья или речи Кота.");
            return;
        }

        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        OpenCalendarUI();
    }

    public void OnSmallScrollButtonClicked()
    {
        if (isCraftingInProgress || (dialoguePanel != null && dialoguePanel.activeSelf))
        {
            Debug.Log("[ALCHEMIST] Свиток заблокирован во время варки зелья или речи Кота.");
            return;
        }

        OpenRecipeScrollUI();
    }

    public void OnCauldronButtonClicked()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf) return;
        OpenRecipeScrollUI();
    }

    public void OnCalendarClosed()
    {
        SetCalendarButtonInteractable(false);

        if (!calendarOpenedOnce)
        {
            calendarOpenedOnce = true;
            StartAvatarAndExpDialoguePhase();
        }
    }

    public void OnAvatarPanelClosed()
    {
        if (avatarManager != null)
        {
            avatarManager.SetAvatarButtonInteractable(false);
        }

        if (!avatarPanelOpenedOnce)
        {
            avatarPanelOpenedOnce = true;
            StartRecipeDialoguePhase();
        }
    }

    // -------------------------------------------------------------
    // ФАЗА 2: ОПЫТ, УРОВЕНЬ И АВАТАРКИ
    // -------------------------------------------------------------
    public void StartAvatarAndExpDialoguePhase()
    {
        currentPhase = DialoguePhase.AvatarAndExp;
        currentStepIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (playerAvatarContainer != null) playerAvatarContainer.SetActive(true);

        BuildAvatarScenario();
        DisplayStep(0);
    }

    private void BuildAvatarScenario()
    {
        dialogueSteps.Clear();

        // 1. Появление аватара и шкалы опыта (0/10 XP)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=84%>Замечательно! Посещаемость отмечена. Теперь взгляни в левый верхний угол:\n\nТам появилась твоя <b><color=#FFE57F>Аватарка и Полоска Опыта (0/10 XP)</color></b>. За изготовление любых зелий и эликсиров ты будешь накапливать опыт!</size>",
            textEN = "<size=84%>Splendid! Attendance marked. Now look at the top-left corner:\n\nThere is your <b><color=#FFE57F>Avatar & EXP Bar (0/10 XP)</color></b>. Brewing any potion grants you valuable alchemy experience!</size>",
            textTR = "<size=84%>Harika! Katilim damgalandi. Simdi sol ust koseye bak:\n\nOrada <b><color=#FFE57F>Avatarin ve Deneyim Cubugun (0/10 XP)</color></b> belirdi. Iksir urettikce tecrube kazanacaksin!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true
        });

        // 2. Цвета полоски опыта (Скриншот 5)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=104%>Полоска опыта меняет цвет: сначала она <b>белая</b>, затем при заполнении станет <b>зеленой</b>, ближе к уровню — <b>оранжевой</b>, а перед самым повышением — <b>красной</b>!</size>",
            textEN = "<size=104%>The EXP bar dynamically changes color: <b>White</b> at start, <b>Green</b> midway, <b>Orange</b> near the top, and <b>Red</b> right before Level Up!</size>",
            textTR = "<size=104%>Deneyim cubugu renk degistirir: basta <b>Beyaz</b>, doldukca <b>Yesil</b>, seviyeye yaklasinca <b>Turuncu</b> ve seviye atlamadan once <b>Kirmizi</b> olur!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true
        });

        // 3. Аватарки: полная коллекция из 34 аватарок и 14 рамок
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=82%>Также у нас богатая коллекция: <b>34 уникальные Аватарки</b> (3 открыты со старта, 21 за уровни до 100 Ур., 5 в лавке за золото и 5 за кристаллы) и <b>14 волшебных Рамок</b>!\n\nНажми кнопку ниже, я открою гардероб, чтобы ты мог выбрать себе облик!</size>",
            textEN = "<size=82%>We also have a rich wardrobe: <b>34 unique Avatars</b> (3 starter, 21 level-up up to Lv.100, 5 in shop, 5 premium) and <b>14 magical Frames</b>!\n\nTap below to explore and pick your avatar!</size>",
            textTR = "<size=82%>Ayrica zengin bir koleksiyonumuz var: <b>34 ozel Avatar</b> (3 baslangic, 21 seviye odulu 100'e kadar, 5 dukkan, 5 kristal) ve <b>14 buyulu Cerceve</b>!\n\nKiyafet secimi icin asagidaki butona bas!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            isAvatarShowStep = true
        });
    }

    // -------------------------------------------------------------
    // ФАЗА 3: ПЕРВЫЙ ОПЫТ, СПИСАНИЕ РЕСУРСОВ И СВИТОК РЕЦЕПТА
    // -------------------------------------------------------------
    public void StartRecipeDialoguePhase()
    {
        currentPhase = DialoguePhase.RecipeCrafting;
        currentStepIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        BuildRecipeScenario();
        DisplayStep(0);
    }

    private void BuildRecipeScenario()
    {
        dialogueSteps.Clear();

        // 1. Предложение помощи с первым опытом
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=86%>Отличный выбор облика! Теперь я помогу тебе получить твой <b>первый опыт алхимии</b> и повысить уровень до 2-го!\n\nНажми «Согласен», чтобы приступить к изучению первого рецепта!</size>",
            textEN = "<size=86%>Great avatar choice! Now I will assist you in gaining your <b>first alchemy experience</b> and leveling up to Level 2!\n\nClick «Agree» to begin learning the first recipe!</size>",
            textTR = "<size=86%>Harika secim! Simdi <b>ilk simya tecrubeni</b> kazanmana ve Seviye 2'ye yukselmene yardim edecegim!\n\nIlk tarifi ogrenmek icin «Kabul» butonuna bas!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            isConfirmHelpStep = true
        });

        // 2. Согласие на списание 100 золота, 5 камней, 1 свитка
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=84%>Для изучения и создания первого зелья нам потребуется:\n• <b><color=#FFE57F>100 Золота</color></b>\n• <b><color=#80FFDB>5 Магических Камней</color></b>\n• <b><color=#FFD166>1 Древний Свиток</color></b>\n\nТы согласен отдать эти ресурсы на изготовление первого рецепта?</size>",
            textEN = "<size=84%>To research and brew the first potion, we need:\n• <b><color=#FFE57F>100 Gold</color></b>\n• <b><color=#80FFDB>5 Magic Stones</color></b>\n• <b><color=#FFD166>1 Ancient Scroll</color></b>\n\nDo you agree to grant these resources for the first craft?</size>",
            textTR = "<size=84%>Ilk iksiri hazirlamak icin gerekenler:\n• <b><color=#FFE57F>100 Altin</color></b>\n• <b><color=#80FFDB>5 Buyulu Tas</color></b>\n• <b><color=#FFD166>1 Kadim Parsomen</color></b>\n\nBu kaynaklari vermeyi kabul ediyor musun?</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            isConfirmRecipeStep = true
        });

        // 3. Появление свитка слева от календаря и открытие (Скриншот 7)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=102%>Отлично! Слева от календаря появился наш <b><color=#FFD166>Свиток Рецептов</color></b>!\n\nНажми кнопку ниже, чтобы развернуть его и посмотреть формулу приготовления первого зелья!</size>",
            textEN = "<size=102%>Look! Next to the calendar, our <b><color=#FFD166>Recipe Scroll</color></b> has appeared!\n\nTap below to open it and inspect the first potion formula!</size>",
            textTR = "<size=102%>Takvimin solunda <b><color=#FFD166>Tarif Parsomenimiz</color></b> belirdi!\n\nIlk iksir formulu icin asagidaki butona bas!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            isRecipeStep = true
        });
    }

    // -------------------------------------------------------------
    // ФАЗА 4: ПОСЛЕ ВАРКИ ЗЕЛЬЯ — СУНДУК, ИНВЕНТАРЬ И КОЛБА ОПЫТА
    // -------------------------------------------------------------
    public void StartPostCraftChestDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        dialogueSteps.Clear();
        currentStepIndex = 0;

        // 1. Поздравление с уровнем и появление сундука
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Поздравляю! Ты получил <b><color=#80FFDB>+10 XP</color></b> и поднял <b><color=#FFE57F>2-й Уровень</color></b>!\n\nСлева от свитка рецептов появился твой <b><color=#FFD166>Сундук Алхимика</color></b>!</size>",
            textEN = "<size=108%>Congratulations! You gained <b><color=#80FFDB>+10 XP</color></b> and reached <b><color=#FFE57F>Level 2</color></b>!\n\nNext to the scroll, your <b><color=#FFD166>Alchemist Chest</color></b> appeared!</size>",
            textTR = "<size=108%>Tebrikler! <b><color=#80FFDB>+10 XP</color></b> kazandin ve <b><color=#FFE57F>Seviye 2</color></b> oldun!\n\nParsomenin solunda <b><color=#FFD166>Sandigin</color></b> belirdi!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true
        });

        // 2. Рассказ про Опыт Мастерства и колбу в сундуке
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=100%>Помимо обычного уровня, алхимик развивает свой <b><color=#52B788>Опыт Мастерства</color></b>!\n\nЯ положил в твой сундук редкую <b><color=#80FFDB>Колбу Опыта Мастерства</color></b> — она поможет мгновенно повысить твой алхимический ранг!</size>",
            textEN = "<size=100%>Besides normal levels, you develop <b><color=#52B788>Mastery Experience</color></b>!\n\nI placed a rare <b><color=#80FFDB>Mastery Flask</color></b> in your chest to elevate your rank instantly!</size>",
            textTR = "<size=100%>Normal seviyenin yani sira <b><color=#52B788>Ustalik Deneyimi</color></b> gelistirirsin!\n\nSandigina rutbeni yukseltecek ozel bir <b><color=#80FFDB>Ustalik Sisesi</color></b> koydum!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true
        });

        // 3. Предложение открыть инвентарь и выпить колбу
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=105%>Давай откроем твой инвентарь!\n\nНажми на <b>Колбу Опыта</b> в первом слоте, чтобы выпить её и получить <b><color=#80FFDB>+100 Опыта Мастерства</color></b>!</size>",
            textEN = "<size=105%>Let us open your inventory!\n\nTap the <b>Mastery Flask</b> in the first slot to drink it and receive <b><color=#80FFDB>+100 Mastery XP</color></b>!</size>",
            textTR = "<size=105%>Hadi envanterini acalim!\n\nIlk yuvadaki <b>Ustalik Sisesine</b> basarak <b><color=#80FFDB>+100 Ustalik XP</color></b> kazan!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            isInventoryOpenStep = true
        });

        DisplayStep(0);
    }

    // -------------------------------------------------------------
    // ФАЗА 5: ПОСЛЕ ВЫПИТОЙ КОЛБЫ — РАНГ НОВИЧОК-ТРАВНИК И РАЗДЕЛ ЗНАНИЯ
    // -------------------------------------------------------------
    public void StartPostMasteryKnowledgeDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        dialogueSteps.Clear();
        currentStepIndex = 0;

        // 1. Поздравление со званием Новичок-травник
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Поздравляю! Ты выпил колбу, получил <b><color=#80FFDB>+100 Опыта Мастерства</color></b> и стал <b><color=#52B788>Новичком-травником</color></b>!\n\nТвоя шкала мастерства в профиле заполнилась и открыла путь к великим таинствам!</size>",
            textEN = "<size=108%>Congratulations! You drank the flask, gained <b><color=#80FFDB>+100 Mastery XP</color></b>, and became an <b><color=#52B788>Herbalist Novice</color></b>!\n\nYour mastery bar reached the next milestone!</size>",
            textTR = "<size=108%>Tebrikler! Siseyi ictin, <b><color=#80FFDB>+100 Ustalik XP</color></b> kazandin ve <b><color=#52B788>Bitkici Acemi</color></b> oldun!\n\nUstalik cubugun yeni bir asama acti!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            showKnowledgeIcon = true
        });

        // 2. Рассказ про раздел Знания (Древо Рангов)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=100%>Слева от сундука открылся раздел <b><color=#FFD166>Знания</color></b> (древние фолианты)!\n\nВ нем хранятся все <b>4 Этапа</b> и <b>21 Алхимический Ранг</b> — от Новичка до Создателя Философского камня.</size>",
            textEN = "<size=100%>To the left of your chest, the <b><color=#FFD166>Knowledge</color></b> section has unlocked!\n\nIt displays all <b>4 Stages</b> and <b>21 Alchemy Ranks</b> — from Novice to Philosopher's Stone Creator.</size>",
            textTR = "<size=100%>Sandigin solunda <b><color=#FFD166>Bilgi</color></b> bolumu acildi!\n\nBurada Acemiden Felsefe Tasi Yaraticisina kadar tum <b>4 Asama</b> ve <b>21 Rutbe</b> bulunur.</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            showKnowledgeIcon = true
        });

        // 3. Предложение изучить древо знаний
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=105%>Давай изучим древо мастерства!\n\nНажми кнопку ниже, чтобы открыть окно <b>Знаний</b> и посмотреть свои награды и следующие ранги!</size>",
            textEN = "<size=105%>Let us explore the mastery tree!\n\nTap below to open the <b>Knowledge</b> window and inspect your upcoming ranks and rewards!</size>",
            textTR = "<size=105%>Hadi ustalik agacini kesfedelim!\n\n<b>Bilgi</b> penceresini acip sonraki rutbeleri gormek icin butona bas!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            showKnowledgeIcon = true,
            isKnowledgeOpenStep = true
        });

        DisplayStep(0);
    }

    // -------------------------------------------------------------
    // ФАЗА 6: ПОСЛЕ ЗАКРЫТИЯ ЗНАНИЙ — МИНИ-ИГРЫ И КОЛЕСО ИГР («ПОЙМАЙ МЫШКУ»)
    // -------------------------------------------------------------
    public void StartMinigamesIntroductionDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // Прячем столик, маленького кота и котел во время диалога
        if (cauldronButton != null) cauldronButton.SetActive(false);
        if (roomCatObject != null) roomCatObject.SetActive(false);

        dialogueSteps.Clear();
        currentStepIndex = 0;

        // 1. Кот рассказывает, что в башне есть мини-игры, но забыл как играть, спрашивает согласен ли игрок помочь
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Поздравляю! Ты изучил великие таинства алхимии и ранги мастеров!\n\nЗнаешь, в нашей башне есть ещё <b><color=#FFE57F>волшебные мини-игры</color></b>, но я совсем позабыл, как в них играть... Поможешь мне вспомнить их правила?</size>",
            textEN = "<size=108%>Congratulations! You have studied the great alchemy mysteries!\n\nOur tower also holds <b><color=#FFE57F>magical mini-games</color></b>, but I forgot how to play them... Will you help me recall the rules?</size>",
            textTR = "<size=108%>Tebrikler! Simyanin buyuk gizemlerini ogrendin!\n\nKulemizde <b><color=#FFE57F>mini oyunlar</color></b> var ama nasil oynandigini unuttum... Bana kurallari hatirlatmamda yardim eder misin?</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            showKnowledgeIcon = true,
            showMinigamesIcon = true,
            isConfirmMinigamesStep = true
        });

        // 2. Игрок соглашается, кот представляет первую игру «Поймай мышку» и открывает колесо
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=105%>Мурр! Замечательно! Вверху открылось наше <b><color=#FFE57F>Колесо Мини-Игр</color></b>!\n\nПервая игра, которую мы вспомним — <b><color=#80FFDB>«Поймай мышку»</color></b>!\n\nНажми кнопку ниже, чтобы взглянуть на Колесо Игр!</size>",
            textEN = "<size=105%>Purr! Wonderful! Our <b><color=#FFE57F>Mini-Games Wheel</color></b> has unlocked above!\n\nThe very first game to recall is <b><color=#80FFDB>«Catch the Mouse»</color></b>!\n\nTap below to open the Games Wheel!</size>",
            textTR = "<size=105%>Miyav! Harika! Yukarida <b><color=#FFE57F>Mini Oyun Carki</color></b> acildi!\n\nHatirlayacagimiz ilk oyun <b><color=#80FFDB>«Fareyi Yakala»</color></b>!\n\nCarki gormek icin asagidaki butona bas!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            showChestIcon = true,
            showKnowledgeIcon = true,
            showMinigamesIcon = true,
            isMinigamesWheelOpenStep = true
        });

        DisplayStep(0);
    }

    public void OpenRecipeScrollUI()
    {
        if (scrollOpenSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(scrollOpenSound);
        else if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // При открытом свитке отключаем интерактивность и скрываем фон ресурсов/календаря/аватарки
        if (topPanel != null) topPanel.SetActive(false);
        if (calendarIconButton != null) calendarIconButton.SetActive(false);
        if (playerAvatarContainer != null) playerAvatarContainer.SetActive(false);
        if (smallScrollIconButton != null) smallScrollIconButton.SetActive(false);

        if (recipeScrollPanel != null)
        {
            recipeScrollPanel.SetActive(true);
        }

        if (cauldronButton != null) cauldronButton.SetActive(true);
        if (roomCatObject != null) roomCatObject.SetActive(true);
    }

    public void CloseRecipeScrollUI()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (recipeScrollPanel != null)
        {
            recipeScrollPanel.SetActive(false);
        }

        // При выходе из свитка рецептов восстанавливаем видимость ресурсов, аватарки, календаря и свитка
        if (topPanel != null) topPanel.SetActive(true);
        if (calendarIconButton != null) calendarIconButton.SetActive(true);
        if (playerAvatarContainer != null) playerAvatarContainer.SetActive(true);
        if (smallScrollIconButton != null) smallScrollIconButton.SetActive(true);

        if (!isCraftingInProgress)
        {
            SetSmallScrollInteractable(true);
            SetCalendarButtonInteractable(true);
            if (avatarManager != null) avatarManager.SetAvatarButtonInteractable(true);
        }
        else
        {
            SetSmallScrollInteractable(false);
            SetCalendarButtonInteractable(false);
            if (avatarManager != null) avatarManager.SetAvatarButtonInteractable(false);
        }
    }

    public void OpenCalendarUI()
    {
        if (calendarManager != null)
        {
            calendarManager.OpenCalendar();
            return;
        }

        if (calendarPanel != null)
        {
            calendarPanel.SetActive(true);
            Calendar_Manager cm = calendarPanel.GetComponent<Calendar_Manager>();
            if (cm != null)
            {
                cm.OpenCalendar();
                return;
            }
        }

        if (Calendar_Manager.Instance != null)
        {
            Calendar_Manager.Instance.OpenCalendar();
            return;
        }

        Calendar_Manager cal = FindAnyObjectByType<Calendar_Manager>(FindObjectsInactive.Include);
        if (cal != null)
        {
            cal.OpenCalendar();
            return;
        }

        GameObject foundPanel = GameObject.Find("Calendar_Panel");
        if (foundPanel != null)
        {
            foundPanel.SetActive(true);
            Calendar_Manager foundCm = foundPanel.GetComponent<Calendar_Manager>();
            if (foundCm != null)
            {
                foundCm.OpenCalendar();
                return;
            }
        }
    }

    public void SyncPlayerPrefsResources()
    {
        currentGold = PlayerPrefs.GetInt("Player_Gold", currentGold);
        currentStones = PlayerPrefs.GetInt("Player_Stones", currentStones);
        currentScrolls = PlayerPrefs.GetInt("Player_Scrolls", currentScrolls);
        currentCrystals = PlayerPrefs.GetInt("Player_Crystals", currentCrystals);
        UpdateResourceTextsInstant();
    }

    public void RefreshResourceDisplay()
    {
        SyncPlayerPrefsResources();
    }

    // -------------------------------------------------------------
    // ФАЗА 1: СТАРТОВЫЙ ДИАЛОГ И КАЛЕНДАРЬ
    // -------------------------------------------------------------
    private void BuildIntroScenario()
    {
        dialogueSteps.Clear();

        // 0. Имя (Скриншот 1)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Здравствуй, путник! Я Кот-Алхимик. Я буду помогать тебе по всей игре во всём!\n\nКак к тебе обращаться? (введи от 2 до 12 букв, без знаков)</size>",
            textEN = "<size=108%>Greetings, traveler! I am the Alchemist Cat. I will assist you throughout your journey!\n\nHow may I call you? (enter 2-12 letters, no symbols)</size>",
            textTR = "<size=108%>Selam gezgin! Ben Simyaci Kedi. Yolculugun boyunca sana yardim edecegim!\n\nSana nasil hitap edebilirim? (2-12 harf girin, sembolsuz)</size>",
            isNameInputStep = true,
            revealResourceIndex = -1
        });

        // 1. Приветствие (Скриншот 2)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Приятно познакомиться, {PLAYER_NAME}!\nДобро пожаловать в нашу алхимическую лабораторию. Позволь мне познакомить тебя с главными ресурсами нашего мастерства!</size>",
            textEN = "<size=108%>Pleasure to meet you, {PLAYER_NAME}!\nWelcome to our alchemy sanctuary. Let me introduce you to the core resources of our craft!</size>",
            textTR = "<size=108%>Tanistigimiza memnun oldum, {PLAYER_NAME}!\nSimya mabedimize hos geldin. Sana zanaatimizin temel kaynaklarini tanitmama izin ver!",
            isNameInputStep = false,
            revealResourceIndex = -1
        });

        // 2. Золото (Скриншот 3)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=112%><b><color=#FFE57F>Золотые Монеты</color></b> — наша основная валюта! За них мы улучшаем котёл, открываем новые колбы и покупаем базовые ингредиенты.</size>",
            textEN = "<size=112%><b><color=#FFE57F>Gold Coins</color></b> are our main currency! We use them to upgrade the cauldron, unlock new flasks, and buy basic ingredients.</size>",
            textTR = "<size=112%><b><color=#FFE57F>Altin Paralar</color></b> temel para birimimizdir! Kazani gelistirmek, yeni siseler acmak ve temel malzemeler almak icin kullanilir.</size>",
            isNameInputStep = false,
            revealResourceIndex = 0
        });

        // 3. Камни (Скриншот 4)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=112%><b><color=#80FFDB>Магические Камни</color></b> — редкий минерал стихий! Они нужны для усиления магических зелий и постоянных улучшений лаборатории.</size>",
            textEN = "<size=112%><b><color=#80FFDB>Magic Rune Stones</color></b> are rare elemental minerals! Required for boosting magical potions and permanent lab upgrades.</size>",
            textTR = "<size=112%><b><color=#80FFDB>Buyulu Run Taslari</color></b> nadir element mineralleridir! Buyulu iksirleri guclendirmek ve kalici gelistirmeler icin gereklidir.</size>",
            isNameInputStep = false,
            revealResourceIndex = 1
        });

        // 4. Свитки (Скриншот 5)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=112%><b><color=#FFD166>Древние Свитки</color></b> — тайные знания предков! С их помощью мы изучаем рецепты легендарных эликсиров и открываем мистические формулы.</size>",
            textEN = "<size=112%><b><color=#FFD166>Ancient Scrolls</color></b> hold ancestral wisdom! They allow us to research legendary elixir recipes and decipher mystic formulas.</size>",
            textTR = "<size=112%><b><color=#FFD166>Kadim Parsomenler</color></b> atalarin gizemli bilgileridir! Efsanevi iksir tariflerini ogrenmek icin kullanilir.</size>",
            isNameInputStep = false,
            revealResourceIndex = 2
        });

        // 5. Кристаллы (Скриншот 6)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=112%><b><color=#F384FF>Астральные Кристаллы</color></b> — драгоценная энергия небес! Это самый ценный премиум-ресурс, позволяющий мгновенно творить чудеса.</size>",
            textEN = "<size=112%><b><color=#F384FF>Astral Crystals</color></b> contain celestial energy! The most valuable premium resource for instant magical miracles.</size>",
            textTR = "<size=112%><b><color=#F384FF>Astral Kristaller</color></b> goklerin enerjisidir! Aninda harikalar yaratmak icin en degerli premium kaynaktir.</size>",
            isNameInputStep = false,
            revealResourceIndex = 3
        });

        // 6. Стартовый бонус (Скриншот 7)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=108%>Каждому новому мастеру полагается стартовый набор алхимика! Прими в подарок: <b><color=#FFE57F>5 000 Монет</color></b>, <b><color=#80FFDB>10 Камней</color></b> и <b><color=#FFD166>3 Свитка</color></b>. Нажми кнопку, чтобы забрать!</size>",
            textEN = "<size=108%>Every apprentice deserves a starter kit! Accept this gift: <b><color=#FFE57F>5,000 Coins</color></b>, <b><color=#80FFDB>10 Stones</color></b>, and <b><color=#FFD166>3 Scrolls</color></b>. Click below to claim!</size>",
            textTR = "<size=108%>Her yeni ustaya bir baslangic kiti verilir! Hediyeni kabul et: <b><color=#FFE57F>5.000 Altin</color></b>, <b><color=#80FFDB>10 Tas</color></b> ve <b><color=#FFD166>3 Parsomen</color></b>. Almak icin tikla!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            isClaimStarterRewardStep = true
        });

        // 7. Показ календаря (Скриншот 8)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=112%>Отлично! Ресурсы у тебя. Взгляни: на экране появился наш <b><color=#FFE57F>Магический Календарь Алхимика</color></b>!</size>",
            textEN = "<size=112%>Great! You have the resources. Look: our <b><color=#FFE57F>Alchemist Magic Calendar</color></b> has appeared!</size>",
            textTR = "<size=112%>Harika! Kaynaklar sende. Ekrana bak: <b><color=#FFE57F>Simyaci Buyulu Takvimimiz</color></b> belirdi!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 8. Ежемесячные награды (Скриншот 1)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=93%>Каждый день ты будешь ставить печать на числе в календаре и получать порцию наград!\n\nА за <b>полный закрытый месяц</b> без пропусков: <b><color=#FFE57F>30 000 Монет</color></b>, <b><color=#80FFDB>10 Камней</color></b>, <b><color=#FFD166>5 Свитков</color></b> и <b><color=#F384FF>3 Кристалла</color></b>!</size>",
            textEN = "<size=93%>Each day you stamp your date in the calendar and get rewards!\n\nFull month complete: <b><color=#FFE57F>30,000 Coins</color></b>, <b><color=#80FFDB>10 Stones</color></b>, <b><color=#FFD166>5 Scrolls</color></b>, and <b><color=#F384FF>3 Crystals</color></b>!</size>",
            textTR = "<size=93%>Her gun takvime damga vuracaksin!\n\nTam ay bonusu: <b><color=#FFE57F>30.000 Altin</color></b>, <b><color=#80FFDB>10 Tas</color></b>, <b><color=#FFD166>5 Parsomen</color></b> ve <b><color=#F384FF>3 Kristal</color></b>!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 9. Квартальные супер-бонусы (Скриншот 2)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=98%>За каждый <b>3-й, 6-й, 9-й и 12-й месяц</b> тебя ждут <b>Квартальные Супер-Бонусы</b>: от <b><color=#FFE57F>35 000 до 90 000 Монет</color></b>, до <b><color=#80FFDB>20 Камней</color></b>, <b><color=#FFD166>15 Свитков</color></b> и до <b><color=#F384FF>20 Кристаллов</color></b>!</size>",
            textEN = "<size=98%>Every <b>3rd, 6th, 9th, and 12th month</b> unlocks <b>Quarterly Super Bonuses</b> up to <b><color=#FFE57F>90k Coins</color></b> and <b><color=#F384FF>20 Crystals</color></b>!</size>",
            textTR = "<size=98%>Her <b>3., 6., 9. ve 12. ayda</b> <b>Super Bonuslar</b> seni bekliyor: <b><color=#FFE57F>90k Altin</color></b> ve <b><color=#F384FF>20 Kristal</color></b>!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 10. Годовой джекпот (Скриншот 3)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=102%>А за <b>целый год (365 дней) без пропусков</b> — мифический <b>Годовой Джекпот</b>: <b><color=#FFE57F>500 000 Монет</color></b>, <b><color=#80FFDB>200 Камней</color></b>, <b><color=#FFD166>100 Свитков</color></b> и <b><color=#F384FF>200 Кристаллов</color></b>!</size>",
            textEN = "<size=102%>And for a <b>full year (365 days)</b> — the <b>Annual Jackpot</b>: <b><color=#FFE57F>500k Coins</color></b>, <b><color=#80FFDB>200 Stones</color></b>, <b><color=#FFD166>100 Scrolls</color></b>, and <b><color=#F384FF>200 Crystals</color></b>!</size>",
            textTR = "<size=102%>Ve <b>tam bir yil (365 gun)</b> boyunca: <b><color=#FFE57F>500k Altin</color></b>, <b><color=#80FFDB>200 Tas</color></b>, <b><color=#FFD166>100 Parsomen</color></b> ve <b><color=#F384FF>200 Kristal</color></b>!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 11. Переход в календарь (Скриншот 4)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=98%>Сейчас я покажу тебе календарь. Поставь отметку на сегодняшнем числе — с этого дня начнется твой отсчет посещаемости!\n\nНажми кнопку ниже, чтобы открыть календарь!</size>",
            textEN = "<size=98%>Now I will show you the calendar. Stamp today's date — your attendance streak begins today!\n\nClick the button below to open the calendar!</size>",
            textTR = "<size=98%>Simdi takvimi gosterecegim. Bugunku tarihi damgala — giris takibin baslasin!\n\nTakvimi acmak icin asagidaki butona bas!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true,
            isCalendarOpenStep = true
        });
    }
}
