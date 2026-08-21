using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.15)
/// Менеджер диалогов: глушение музыки меню, яркие цвета ресурсов,
/// непрерывный диалог (начисление стартового бонуса без закрытия рамки),
/// появление иконки календаря и полное обучение системе Ежедневного, 
/// Ежемесячного, Квартального и Годового Календаря наград!
/// </summary>
public class DialogueSystem_Manager : MonoBehaviour
{
    public static DialogueSystem_Manager Instance { get; private set; }

    [Header("Режим Тестирования")]
    [Tooltip("Если включено - при каждом запуске Play игра стартует с 0 ресурсов для удобного тестирования")]
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

    [Header("Иконка Календаря в игре")]
    public GameObject calendarIconButton; // Перетащите сюда Calendar_Button

    [Header("Панель Календаря Наград (Calendar Panel)")]
    public GameObject calendarPanel;         // Перетащите сюда Calendar_Panel
    public Calendar_Manager calendarManager; // Перетащите сюда Calendar_Panel (компонент Calendar_Manager)

    [Header("Большой Свиток Рецепта (Recipe Scroll Panel)")]
    public GameObject recipeScrollPanel;     // Перетащите сюда Recipe_Scroll_Panel
    public Button recipeScrollCloseButton;   // Кнопка закрытия свитка рецепта
    public AudioClip scrollOpenSound;        // Звук разворачивания свитка

    [Header("Объекты игрового мира (Активируются по ходу обучения)")]
    public GameObject cauldronButton;
    public GameObject roomCatObject;

    [Header("Музыка и Звуки")]
    public AudioClip backgroundMusic;
    public AudioClip textTypeSound;
    public AudioClip buttonClickSound;
    public AudioClip coinRewardSound;

    [Header("Настройки")]
    public float textSpeed = 0.025f;

    [System.Serializable]
    public class DialogStep
    {
        public string textRU;
        public string textEN;
        public string textTR;
        public bool isNameInputStep = false;
        public int revealResourceIndex = -1; // 0=Gold, 1=Stones, 2=Scrolls, 3=Crystals, 4=StarterReward
        public bool isClaimStarterRewardStep = false;
        public bool showCalendarIcon = false; // Показать иконку календарика на этом шаге
        public bool revealCauldron = false;   // Показать котел на этом шаге
        public bool isRecipeStep = false;     // Шаг перехода к свитку рецепта
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
            PlayerPrefs.Save();
            currentGold = 0;
            currentStones = 0;
            currentScrolls = 0;
            currentCrystals = 0;
        }
        else
        {
            if (PlayerPrefs.HasKey("Alchemist_Player_Name"))
            {
                playerName = PlayerPrefs.GetString("Alchemist_Player_Name");
            }
            currentGold = PlayerPrefs.GetInt("Player_Gold", 0);
            currentStones = PlayerPrefs.GetInt("Player_Stones", 0);
            currentScrolls = PlayerPrefs.GetInt("Player_Scrolls", 0);
            currentCrystals = PlayerPrefs.GetInt("Player_Crystals", 0);
        }
    }

    private void Start()
    {
        InitBackgroundMusic();

        if (topPanel != null) topPanel.SetActive(false);
        if (slotGold != null) slotGold.SetActive(false);
        if (slotStones != null) slotStones.SetActive(false);
        if (slotScrolls != null) slotScrolls.SetActive(false);
        if (slotCrystals != null) slotCrystals.SetActive(false);

        if (cauldronButton != null) cauldronButton.SetActive(false);
        if (roomCatObject != null) roomCatObject.SetActive(false);
        if (calendarIconButton != null) calendarIconButton.SetActive(false);

        UpdateResourceTextsInstant();

        if (nameInputField != null)
        {
            nameInputField.characterLimit = 12;
            nameInputField.contentType = TMP_InputField.ContentType.Standard;
            nameInputField.lineType = TMP_InputField.LineType.SingleLine;

            if (nameInputField.placeholder is TextMeshProUGUI placeholderText)
            {
                string curLang = PlayerPrefs.GetString("Selected_Language", "RU");
                if (curLang == "EN") placeholderText.text = "Name (2-12 letters)";
                else if (curLang == "TR") placeholderText.text = "İsim (2-12 harf)";
                else placeholderText.text = "Имя (от 2 до 12 букв)";
            }

            nameInputField.onSubmit.RemoveAllListeners();
            nameInputField.onSubmit.AddListener((val) => OnConfirmNameClicked());
        }

        if (confirmNameButton != null)
        {
            confirmNameButton.onClick.RemoveAllListeners();
            confirmNameButton.onClick.AddListener(OnConfirmNameClicked);
        }

        if (nextStepButton != null)
        {
            nextStepButton.onClick.RemoveAllListeners();
            nextStepButton.onClick.AddListener(NextStep);
        }

        // Авто-привязка клика по маленькой иконке календаря
        if (calendarIconButton != null)
        {
            Button calBtn = calendarIconButton.GetComponent<Button>();
            if (calBtn != null)
            {
                calBtn.onClick.RemoveAllListeners();
                calBtn.onClick.AddListener(OnCalendarIconButtonClicked);
            }
        }

        // Авто-привязка клика по котлу
        if (cauldronButton != null)
        {
            Button cBtn = cauldronButton.GetComponent<Button>();
            if (cBtn != null)
            {
                cBtn.onClick.RemoveAllListeners();
                cBtn.onClick.AddListener(OnCauldronButtonClicked);
            }
        }

        // Авто-привязка кнопки закрытия свитка рецепта
        if (recipeScrollCloseButton != null)
        {
            recipeScrollCloseButton.onClick.RemoveAllListeners();
            recipeScrollCloseButton.onClick.AddListener(CloseRecipeScrollUI);
        }

        if (recipeScrollPanel != null)
        {
            recipeScrollPanel.SetActive(false);
        }

        BuildDefaultScenario();
        StartDialogue();
    }

    private void SilenceMenuMusicSources()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource src in allAudioSources)
        {
            if (src != null && src.gameObject != this.gameObject)
            {
                if (src.isPlaying && src.loop)
                {
                    src.Stop();
                }
            }
        }
    }

    private void InitBackgroundMusic()
    {
        if (backgroundMusic == null) return;

        localMusicSource = GetComponent<AudioSource>();
        if (localMusicSource == null)
        {
            localMusicSource = gameObject.AddComponent<AudioSource>();
        }

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (savedVolume <= 0.05f) savedVolume = 0.75f;

        localMusicSource.clip = backgroundMusic;
        localMusicSource.loop = true;
        localMusicSource.volume = savedVolume;
        localMusicSource.playOnAwake = false;

        if (!localMusicSource.isPlaying)
        {
            localMusicSource.Play();
        }
    }

    public void StartDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        currentStepIndex = 0;
        DisplayStep(currentStepIndex);
    }

    public void NextStep()
    {
        if (isTyping)
        {
            isTyping = false;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (dialogueBodyText != null) dialogueBodyText.text = FormatPlayerName(activeFullText);
            OnTypingFinished();
            return;
        }

        if (currentStepIndex < 0 || currentStepIndex >= dialogueSteps.Count)
        {
            EndDialogue();
            return;
        }

        DialogStep currentStep = dialogueSteps[currentStepIndex];
        if (currentStep.isNameInputStep) return;

        // Если это шаг нажатия "Забрать бонус!"
        if (currentStep.isClaimStarterRewardStep && !starterRewardClaimed)
        {
            StartCoroutine(AnimateStarterRewardAndContinue());
            return;
        }

        currentStepIndex++;
        if (currentStepIndex < dialogueSteps.Count)
        {
            DisplayStep(currentStepIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    private void DisplayStep(int index)
    {
        if (index < 0 || index >= dialogueSteps.Count)
        {
            EndDialogue();
            return;
        }

        if (nameInputContainer != null) nameInputContainer.SetActive(false);
        if (nextStepButton != null) nextStepButton.gameObject.SetActive(false);

        DialogStep step = dialogueSteps[index];

        HandleResourceReveal(step.revealResourceIndex);

        if (step.showCalendarIcon && calendarIconButton != null)
        {
            calendarIconButton.SetActive(true);
            // Если Кот еще объясняет правила (шаги 7..10) — иконка видна, но некликабельна, чтобы не перекрывать диалог
            bool isFinalCalendarStep = (index == dialogueSteps.Count - 1 && !isCauldronPhase);
            SetCalendarButtonInteractable(isFinalCalendarStep);
        }

        if (step.revealCauldron && cauldronButton != null)
        {
            cauldronButton.SetActive(true);
        }

        string rawText = GetLocalizedText(step.textRU, step.textEN, step.textTR);
        activeFullText = FormatPlayerName(rawText);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextCoroutine(activeFullText, step));
    }

    private void HandleResourceReveal(int resourceIndex)
    {
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
                    if (step.isRecipeStep)
                        nextStepButtonText.text = "Открыть рецепт >>";
                    else if (step.isClaimStarterRewardStep)
                        nextStepButtonText.text = "Забрать бонус!";
                    else if (currentStepIndex == dialogueSteps.Count - 1)
                        nextStepButtonText.text = isCauldronPhase ? "Открыть рецепт >>" : "Открыть календарь";
                    else
                        nextStepButtonText.text = "Далее >>";
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
            ShowNameValidationError("Имя должно быть от 2 до 12 букв (только буквы и цифры, без знаков)!");
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

        float duration = 1.3f;
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

        yield return new WaitForSeconds(0.4f);

        // Продолжаем разговор и показываем иконку календарика
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

    private void SetCalendarButtonInteractable(bool interactable)
    {
        if (calendarIconButton != null)
        {
            Button btn = calendarIconButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }

    private bool isCauldronPhase = false;
    private bool cauldronDialogueCompleted = false;

    public void OnCalendarIconButtonClicked()
    {
        // Если окно диалога еще активно и Кот объясняет что-то, кроме финального шага — блокируем клик
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            bool isFinalCalendarStep = (currentStepIndex == dialogueSteps.Count - 1 && !isCauldronPhase);
            if (!isFinalCalendarStep)
            {
                Debug.Log("[ALCHEMIST DIALOGUE] Кот ещё говорит! Иконка календаря временно заблокирована.");
                return;
            }

            // Если это финальный шаг — закрываем диалог перед открытием календаря, чтобы не накладывались друг на друга!
            dialoguePanel.SetActive(false);
        }

        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        OpenCalendarUI();
    }

    public void OnCalendarClosed()
    {
        // Когда игрок закрывает календарь крестиком — стартуем диалог про котел и первое зелье!
        if (!cauldronDialogueCompleted)
        {
            StartCauldronDialoguePhase();
        }
    }

    public void StartCauldronDialoguePhase()
    {
        isCauldronPhase = true;
        currentStepIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (calendarIconButton != null) calendarIconButton.SetActive(true);

        BuildCauldronScenario();
        DisplayStep(0);
    }

    private void BuildCauldronScenario()
    {
        dialogueSteps.Clear();

        // 1. Появление котла и рассказ об изготовлении зелий
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Отлично! С календарем наград мы разобрались. Теперь пора заняться настоящей алхимией!\n\nВзгляни: в комнате появился наш <b><color=#FFE57F>Магический Котёл</color></b> — в нём мы сможем варить могущественные зелья и эликсиры из собранных ресурсов!",
            textEN = "Excellent! Now that we understand the calendar, it's time for true alchemy!\n\nLook: our <b><color=#FFE57F>Magic Cauldron</color></b> has appeared — here we can brew powerful potions and elixirs from gathered resources!",
            textTR = "Harika! Odul takvimini ogrendik, simdi gercek simya vakti!\n\nBak: odada <b><color=#FFE57F>Buyulu Kazanimiz</color></b> belirdi — burada topladigimiz kaynaklardan guclu iksirler uretebiliriz!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            revealCauldron = true,
            isRecipeStep = false
        });

        // 2. Первое зелье и рецепт (100 золота, 1 камень, 1 свиток)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Давай я помогу тебе сварить твоё самое первое зелье!\n\nДля его приготовления нам потребуется:\n• <b><color=#FFE57F>100 Золота</color></b>\n• <b><color=#80FFDB>1 Магический Камень</color></b>\n• <b><color=#FFD166>1 Древний Свиток</color></b>\n\nНажми кнопку ниже, чтобы открыть свиток рецепта!",
            textEN = "Let me assist you in brewing your very first potion!\n\nTo brew it, we will need:\n• <b><color=#FFE57F>100 Gold</color></b>\n• <b><color=#80FFDB>1 Magic Stone</color></b>\n• <b><color=#FFD166>1 Ancient Scroll</color></b>\n\nTap below to open the recipe scroll!",
            textTR = "Ilk iksirini hazirlamana yardim edeyim!\n\nGerekli malzemeler:\n• <b><color=#FFE57F>100 Altin</color></b>\n• <b><color=#80FFDB>1 Buyulu Tas</color></b>\n• <b><color=#FFD166>1 Kadim Parsomen</color></b>\n\nTarif parsomenini acmak icin asagidaki butona bas!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            revealCauldron = true,
            isRecipeStep = true
        });
    }

    public void OnCauldronButtonClicked()
    {
        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (!cauldronDialogueCompleted)
        {
            Debug.Log("[ALCHEMIST CAULDRON] Котел ожидает открытия свитка рецепта!");
            return;
        }

        // После завершения обучения клик по котлу открывает свиток рецептов
        OpenRecipeScrollUI();
    }

    public void OpenRecipeScrollUI()
    {
        if (scrollOpenSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(scrollOpenSound);
        else if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (recipeScrollPanel != null)
        {
            recipeScrollPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[ALCHEMIST DIALOGUE] Recipe_Scroll_Panel не назначена в инспекторе!");
        }

        cauldronDialogueCompleted = true;
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
    }

    public void EndDialogue()
    {
        if (isCauldronPhase)
        {
            OpenRecipeScrollUI();
            return;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 🛑 ВАЖНО: Кот в комнате и Котёл НЕ появляются во время интро! Они появляются только после закрытия календаря!
        if (cauldronButton != null) cauldronButton.SetActive(false);
        if (roomCatObject != null) roomCatObject.SetActive(false);

        // 🌟 Иконка календаря на экране всегда остается активной!
        if (calendarIconButton != null) calendarIconButton.SetActive(true);

        // 🌟 МГНОВЕННО ОТКРЫВАЕМ САМ КАЛЕНДАРЬ НАГРАД!
        OpenCalendarUI();

        Debug.Log("[ALCHEMIST DIALOGUE] Интро завершено. Окно диалога закрыто, открыт Магический Календарь!");
    }

    public void OpenCalendarUI()
    {
        // 1. Прямая ссылка на скрипт из инспектора
        if (calendarManager != null)
        {
            calendarManager.OpenCalendar();
            return;
        }

        // 2. Прямая ссылка на панель календаря из инспектора
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

        // 3. Статический Singleton (если объект уже активен)
        if (Calendar_Manager.Instance != null)
        {
            Calendar_Manager.Instance.OpenCalendar();
            return;
        }

        // 4. Поиск компонента на сцене, включая неактивные (Inactive) объекты
        Calendar_Manager cal = FindAnyObjectByType<Calendar_Manager>(FindObjectsInactive.Include);
        if (cal != null)
        {
            cal.OpenCalendar();
            return;
        }

        // 5. Поиск объекта по имени "Calendar_Panel"
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

        Debug.LogWarning("[ALCHEMIST DIALOGUE] Calendar_Manager не найден на сцене! Перетащите Calendar_Panel в поле Calendar Panel в инспекторе DialogueSystem_Manager.");
    }

    public void SyncPlayerPrefsResources()
    {
        currentGold = PlayerPrefs.GetInt("Player_Gold", currentGold);
        currentStones = PlayerPrefs.GetInt("Player_Stones", currentStones);
        currentScrolls = PlayerPrefs.GetInt("Player_Scrolls", currentScrolls);
        currentCrystals = PlayerPrefs.GetInt("Player_Crystals", currentCrystals);
        UpdateResourceTextsInstant();
    }

    private void BuildDefaultScenario()
    {
        dialogueSteps.Clear();

        // 0. Запрос имени
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Здравствуй, путник! Я Кот-Алхимик. Я буду помогать тебе по всей игре во всём!\n\nКак к тебе обращаться? (введи от 2 до 12 букв, без знаков)",
            textEN = "Greetings, traveler! I am the Alchemist Cat. I will assist you throughout your journey!\n\nHow may I call you? (enter 2-12 letters, no symbols)",
            textTR = "Selam gezgin! Ben Simyaci Kedi. Yolculugun boyunca sana yardim edecegim!\n\nSana nasil hitap edebilirim? (2-12 harf girin, sembolsuz)",
            isNameInputStep = true,
            revealResourceIndex = -1
        });

        // 1. Приветствие
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Приятно познакомиться, {PLAYER_NAME}!\nДобро пожаловать в нашу алхимическую лабораторию. Позволь мне познакомить тебя с главными ресурсами нашего мастерства!",
            textEN = "Pleasure to meet you, {PLAYER_NAME}!\nWelcome to our alchemy sanctuary. Let me introduce you to the core resources of our craft!",
            textTR = "Tanistigimiza memnun oldum, {PLAYER_NAME}!\nSimya mabedimize hos geldin. Sana zanaatimizin temel kaynaklarini tanitmama izin ver!",
            isNameInputStep = false,
            revealResourceIndex = -1
        });

        // 2. Золото
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<b><color=#FFE57F>Золотые Монеты</color></b> — наша основная валюта! За них мы улучшаем котёл, открываем новые колбы и покупаем базовые ингредиенты.",
            textEN = "<b><color=#FFE57F>Gold Coins</color></b> are our main currency! We use them to upgrade the cauldron, unlock new flasks, and buy basic ingredients.",
            textTR = "<b><color=#FFE57F>Altin Paralar</color></b> temel para birimimizdir! Kazani gelistirmek, yeni siseler acmak ve temel malzemeler almak icin kullanilir.",
            isNameInputStep = false,
            revealResourceIndex = 0
        });

        // 3. Камни
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<b><color=#80FFDB>Магические Камни</color></b> — редкий минерал стихий! Они нужны для усиления магических зелий и постоянных улучшений лаборатории.",
            textEN = "<b><color=#80FFDB>Magic Rune Stones</color></b> are rare elemental minerals! Required for boosting magical potions and permanent lab upgrades.",
            textTR = "<b><color=#80FFDB>Buyulu Run Taslari</color></b> nadir element mineralleridir! Buyulu iksirleri guclendirmek ve kalici gelistirmeler icin gereklidir.",
            isNameInputStep = false,
            revealResourceIndex = 1
        });

        // 4. Свитки
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<b><color=#FFD166>Древние Свитки</color></b> — тайные знания предков! С их помощью мы изучаем рецепты легендарных эликсиров и открываем мистические формулы.",
            textEN = "<b><color=#FFD166>Ancient Scrolls</color></b> hold ancestral wisdom! They allow us to research legendary elixir recipes and decipher mystic formulas.",
            textTR = "<b><color=#FFD166>Kadim Parsomenler</color></b> atalarin gizemli bilgileridir! Efsanevi iksir tariflerini ogrenmek icin kullanilir.",
            isNameInputStep = false,
            revealResourceIndex = 2
        });

        // 5. Астральные Кристаллы
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<b><color=#F384FF>Астральные Кристаллы</color></b> — драгоценная энергия небес! Это самый ценный премиум-ресурс, позволяющий мгновенно творить чудеса.",
            textEN = "<b><color=#F384FF>Astral Crystals</color></b> contain celestial energy! The most valuable premium resource for instant magical miracles.",
            textTR = "<b><color=#F384FF>Astral Kristaller</color></b> goklerin enerjisidir! Aninda harikalar yaratmak icin en degerli premium kaynaktir.",
            isNameInputStep = false,
            revealResourceIndex = 3
        });

        // 6. Стартовый бонус
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Каждому новому мастеру полагается стартовый набор алхимика! Прими в подарок: <b><color=#FFE57F>5 000 Монет</color></b>, <b><color=#80FFDB>10 Камней</color></b> и <b><color=#FFD166>3 Свитка</color></b>. Нажми кнопку, чтобы забрать!",
            textEN = "Every apprentice deserves a starter kit! Accept this gift: <b><color=#FFE57F>5,000 Coins</color></b>, <b><color=#80FFDB>10 Stones</color></b>, and <b><color=#FFD166>3 Scrolls</color></b>. Click below to claim!",
            textTR = "Her yeni ustaya bir baslangic kiti verilir! Hediyeni kabul et: <b><color=#FFE57F>5.000 Altin</color></b>, <b><color=#80FFDB>10 Tas</color></b> ve <b><color=#FFD166>3 Parsomen</color></b>. Almak icin tikla!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            isClaimStarterRewardStep = true
        });

        // 7. Появление иконки календаря и объяснение
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Отлично! Ресурсы у тебя. Взгляни на экран: там появился наш <b><color=#FFE57F>Магический Календарь Алхимика</color></b>!",
            textEN = "Great! You have the resources. Look at the screen: our <b><color=#FFE57F>Alchemist Magic Calendar</color></b> has appeared!",
            textTR = "Harika! Kaynaklar sende. Ekrana bak: <b><color=#FFE57F>Simyaci Buyulu Takvimimiz</color></b> belirdi!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 8. Ежедневные награды и полный месяц (Компактный размер <size=85%>)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>Каждый день при входе в игру день будет отмечаться печатью, а ты получишь порцию золота, камней, свитков и кристаллов!\n\nА за <b>полный закрытый месяц</b> без пропусков: <b><color=#FFE57F>30 000 Монет</color></b>, <b><color=#80FFDB>10 Камней</color></b>, <b><color=#FFD166>5 Свитков</color></b> и <b><color=#F384FF>3 Кристалла</color></b>!</size>",
            textEN = "<size=85%>Each daily login marks the day and grants gold, stones, scrolls, and crystals!\n\nCompleting a <b>full month</b> without skipping grants: <b><color=#FFE57F>30,000 Coins</color></b>, <b><color=#80FFDB>10 Stones</color></b>, <b><color=#FFD166>5 Scrolls</color></b>, and <b><color=#F384FF>3 Crystals</color></b>!</size>",
            textTR = "<size=85%>Her gun giris yaptiginda gun damgalanir ve altin, tas, parsomen ve kristal kazanirsin!\n\n<b>Tam bir ayi</b> tamamlamak: <b><color=#FFE57F>30.000 Altin</color></b>, <b><color=#80FFDB>10 Tas</color></b>, <b><color=#FFD166>5 Parsomen</color></b> ve <b><color=#F384FF>3 Kristal</color></b> kazandirir!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 9. Квартальные супер-бонусы (Компактный размер <size=85%>)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>За каждый <b>3-й, 6-й, 9-й и 12-й месяц</b> тебя ждут <b>Квартальные Супер-Бонусы</b>: от <b><color=#FFE57F>35 000 до 90 000 Монет</color></b>, до <b><color=#80FFDB>20 Камней</color></b>, <b><color=#FFD166>15 Свитков</color></b> и до <b><color=#F384FF>20 Кристаллов</color></b>!</size>",
            textEN = "<size=85%>Every <b>3rd, 6th, 9th, and 12th month</b> unlocks <b>Quarterly Super Bonuses</b>: up to <b><color=#FFE57F>90k Coins</color></b>, <b><color=#80FFDB>20 Stones</color></b>, <b><color=#FFD166>15 Scrolls</color></b>, and <b><color=#F384FF>20 Crystals</color></b>!</size>",
            textTR = "<size=85%>Her <b>3., 6., 9. ve 12. ayda</b> <b>Super Bonuslar</b> seni bekliyor: <b><color=#FFE57F>90k Altin</color></b>, <b><color=#80FFDB>20 Tas</color></b>, <b><color=#FFD166>15 Parsomen</color></b> ve <b><color=#F384FF>20 Kristal</color></b>!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 10. Годовой Джекпот (Компактный размер <size=85%>)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>А если ты зайдёшь <b>целый год (365 дней) без единого пропуска</b> — получишь грандиозный <b>Годовой Джекпот</b>: <b><color=#FFE57F>500 000 Монет</color></b>, <b><color=#80FFDB>200 Камней</color></b>, <b><color=#FFD166>100 Свитков</color></b> и <b><color=#F384FF>200 Кристаллов</color></b>!</size>",
            textEN = "<size=85%>And if you log in for a <b>full year (365 days) without missing a single day</b> — you receive the mythical <b>Annual Jackpot</b>: <b><color=#FFE57F>500k Coins</color></b>, <b><color=#80FFDB>200 Stones</color></b>, <b><color=#FFD166>100 Scrolls</color></b>, and <b><color=#F384FF>200 Crystals</color></b>!</size>",
            textTR = "<size=85%>Ve <b>tam bir yil (365 gun) hic gun kacirmadan</b> giris yaparsan: <b><color=#FFE57F>500k Altin</color></b>, <b><color=#80FFDB>200 Tas</color></b>, <b><color=#FFD166>100 Parsomen</color></b> ve <b><color=#F384FF>200 Kristal</color></b> seni bekliyor!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 11. Призыв нажать на календарик
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Нажми на значок маленького календарика на экране, чтобы открыть его и забрать свою первую награду!",
            textEN = "Tap the small calendar icon on the screen to open it and claim your first reward!",
            textTR = "Ilk odulunu almak icin ekrandaki kucuk takvim simgesine dokun!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });
    }
}
