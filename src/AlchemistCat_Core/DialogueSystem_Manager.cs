using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.18)
/// Полный сценарий игрового обучения и интерактивных переходов:
/// 1. Ввод имени и знакомство с ресурсами (Золото, Камни, Свитки, Кристаллы).
/// 2. Начисление стартового бонуса (5k Gold, 10 Stones, 3 Scrolls).
/// 3. Показ Календаря Наград (иконка заблокирована во время монолога Кота, разблокируется и открывается только по кнопке "Открыть календарь >>").
/// 4. Закрытие календаря -> Диалог про Опыт Алхимии, Уровень (0/10 XP) и 4-цветную полоску опыта.
/// 5. Рассказ про Аватарки (Простые, Покупные, Премиум) -> Кнопка "Показать аватарки >>" -> Окно аватарок со скроллингом.
/// 6. Закрытие аватарок -> Согласие на первый опыт и изготовление первого рецепта за 100 золота, 5 камней, 1 свиток.
/// 7. Кнопка "Да, я согласен" -> Списание ресурсов, выдача опыта (+10 XP, повышение уровня), появление иконки свитка и открытие Большого Свитка Рецепта!
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
            PlayerPrefs.DeleteKey("Tutorial_Calendar_Claim_Done");
            PlayerPrefs.DeleteKey($"Cal_Claimed_{System.DateTime.Now.Year}_{System.DateTime.Now.Month}_{System.DateTime.Now.Day}");
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

        // 1. Скрываем все лишние UI до их фазы
        if (topPanel != null) topPanel.SetActive(false);
        if (slotGold != null) slotGold.SetActive(false);
        if (slotStones != null) slotStones.SetActive(false);
        if (slotScrolls != null) slotScrolls.SetActive(false);
        if (slotCrystals != null) slotCrystals.SetActive(false);

        if (playerAvatarContainer != null) playerAvatarContainer.SetActive(false);
        if (cauldronButton != null) cauldronButton.SetActive(false);
        if (roomCatObject != null) roomCatObject.SetActive(false);
        if (calendarIconButton != null) calendarIconButton.SetActive(false);
        if (smallScrollIconButton != null) smallScrollIconButton.SetActive(false);

        UpdateResourceTextsInstant();

        // 2. Настройка поля ввода имени
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

        // 3. Авто-привязка иконки календаря
        if (calendarIconButton != null)
        {
            Button calBtn = calendarIconButton.GetComponent<Button>();
            if (calBtn != null)
            {
                calBtn.onClick.RemoveAllListeners();
                calBtn.onClick.AddListener(OnCalendarIconButtonClicked);
            }
        }

        // 4. Авто-привязка иконки маленького свитка
        if (smallScrollButton != null)
        {
            smallScrollButton.onClick.RemoveAllListeners();
            smallScrollButton.onClick.AddListener(OnSmallScrollButtonClicked);
        }

        // 5. Авто-привязка котла
        if (cauldronButton != null)
        {
            Button cBtn = cauldronButton.GetComponent<Button>();
            if (cBtn != null)
            {
                cBtn.onClick.RemoveAllListeners();
                cBtn.onClick.AddListener(OnCauldronButtonClicked);
            }
        }

        // 6. Закрытие свитка рецепта
        if (recipeScrollCloseButton != null)
        {
            recipeScrollCloseButton.onClick.RemoveAllListeners();
            recipeScrollCloseButton.onClick.AddListener(CloseRecipeScrollUI);
        }

        if (recipeScrollPanel != null)
        {
            recipeScrollPanel.SetActive(false);
        }

        // Старт Фазы 1
        currentPhase = DialoguePhase.IntroAndCalendar;
        BuildIntroScenario();
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
            return;
        }

        DialogStep currentStep = dialogueSteps[currentStepIndex];
        if (currentStep.isNameInputStep) return;

        // Если это шаг "Забрать бонус!"
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

        currentStepIndex++;
        if (currentStepIndex < dialogueSteps.Count)
        {
            DisplayStep(currentStepIndex);
        }
        else
        {
            if (currentPhase == DialoguePhase.IntroAndCalendar)
            {
                // Финал 1 фазы
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
            // Иконка заблокирована во время разговора, клик только через кнопку
            SetCalendarButtonInteractable(false);
        }

        if (step.revealAvatarUI && playerAvatarContainer != null)
        {
            playerAvatarContainer.SetActive(true);
            if (avatarManager != null)
            {
                avatarManager.UpdateProfileUI();
                avatarManager.SetAvatarButtonInteractable(false); // Заблокирована во время речи
            }
        }

        if (step.showSmallScrollIcon && smallScrollIconButton != null)
        {
            smallScrollIconButton.SetActive(true);
            SetSmallScrollInteractable(false); // Заблокирован во время объяснения
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
                    if (step.isClaimStarterRewardStep)
                        nextStepButtonText.text = "Забрать бонус!";
                    else if (step.isCalendarOpenStep)
                        nextStepButtonText.text = "Открыть календарь >>";
                    else if (step.isAvatarShowStep)
                        nextStepButtonText.text = "Показать аватарки >>";
                    else if (step.isConfirmHelpStep)
                        nextStepButtonText.text = "Согласен >>";
                    else if (step.isConfirmRecipeStep)
                        nextStepButtonText.text = "Да, я согласен!";
                    else if (step.isRecipeStep)
                        nextStepButtonText.text = "Открыть рецепт >>";
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

        // Начисление опыта (+10 XP) и апгрейд уровня
        if (avatarManager != null)
        {
            avatarManager.AddExperience(10);
        }

        if (coinRewardSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(coinRewardSound);

        yield return new WaitForSeconds(0.4f);

        // Показываем котел и свиток
        if (cauldronButton != null) cauldronButton.SetActive(true);
        if (smallScrollIconButton != null)
        {
            smallScrollIconButton.SetActive(true);
            SetSmallScrollInteractable(true); // Разблокируем свиток!
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

    public bool CanInteractWithAvatarIcon()
    {
        // Разрешено кликать только если диалог закрыт или мы вне обучения
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
    }

    public void OnCalendarIconButtonClicked()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            Debug.Log("[ALCHEMIST] Кот ещё говорит! Используйте кнопку диалога.");
            return;
        }

        if (buttonClickSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(buttonClickSound);

        OpenCalendarUI();
    }

    public void OnSmallScrollButtonClicked()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            Debug.Log("[ALCHEMIST] Свиток заблокирован до окончания речи Кота.");
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
        // 🌟 Игрок закрыл календарь -> иконка календаря блокируется, стартует Фаза 2 (Опыт, Уровень, Аватарки)
        SetCalendarButtonInteractable(false);

        if (!calendarOpenedOnce)
        {
            calendarOpenedOnce = true;
            StartAvatarAndExpDialoguePhase();
        }
    }

    public void OnAvatarPanelClosed()
    {
        // 🌟 Игрок выбрал/закрыл аватарку -> иконка аватарок блокируется, стартует Фаза 3 (Первый опыт и рецепт)
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
            textRU = "Замечательно! Посещаемость отмечена. Теперь взгляни в левый верхний угол:\n\nТам появилась твоя <b><color=#FFE57F>Аватарка и Полоска Опыта (0/10 XP)</color></b>. За изготовление любых зелий и эликсиров ты будешь накапливать опыт!",
            textEN = "Splendid! Attendance marked. Now look at the top-left corner:\n\nThere is your <b><color=#FFE57F>Avatar & EXP Bar (0/10 XP)</color></b>. Brewing any potion grants you valuable alchemy experience!",
            textTR = "Harika! Katilim damgalandi. Simdi sol ust koseye bak:\n\nOrada <b><color=#FFE57F>Avatarin ve Deneyim Cubugun (0/10 XP)</color></b> belirdi. Iksir urettikce tecrube kazanacaksin!",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true
        });

        // 2. Цвета полоски опыта
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>Полоска опыта меняет цвет: сначала она <b>белая</b>, затем при заполнении станет <b>зеленой</b>, ближе к уровню — <b>оранжевой</b>, а перед самым повышением — <b>красной</b>!</size>",
            textEN = "<size=85%>The EXP bar dynamically changes color: <b>White</b> at start, <b>Green</b> midway, <b>Orange</b> near the top, and <b>Red</b> right before Level Up!</size>",
            textTR = "<size=85%>Deneyim cubugu renk degistirir: basta <b>Beyaz</b>, doldukca <b>Yesil</b>, seviyeye yaklasinca <b>Turuncu</b> ve seviye atlamadan once <b>Kirmizi</b> olur!</size>",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true
        });

        // 3. Аватарки: простые, магазинные и премиум
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>Также у нас есть коллекция аватарок: <b>10 простых</b> (3 открыты сразу, 7 откроются за уровни), <b>5 покупных</b> за золото в лавке и <b>5 донатных</b> за кристаллы!\n\nНажми кнопку ниже, я открою гардероб, чтобы ты мог выбрать себе облик!</size>",
            textEN = "<size=85%>We also have an avatar wardrobe: <b>10 free</b> (3 open, 7 via level), <b>5 shop</b> (Gold), and <b>5 premium</b> (Crystals)!\n\nTap below to explore and pick your avatar!</size>",
            textTR = "<size=85%>Ayrica avatar gardirobumuz var: <b>10 ucretsiz</b>, <b>5 magaza</b> ve <b>5 premium</b>!\n\nKiyafet secimi icin asagidaki butona bas!</size>",
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
            textRU = "Отличный выбор облика! Теперь я помогу тебе получить твой <b>первый опыт алхимии</b> и повысить уровень до 2-го!\n\nНажми «Согласен», чтобы приступить к изучению первого рецепта!",
            textEN = "Great avatar choice! Now I will assist you in gaining your <b>first alchemy experience</b> and leveling up to Level 2!\n\nClick «Agree» to begin learning the first recipe!",
            textTR = "Harika secim! Simdi <b>ilk simya tecrubeni</b> kazanmana ve Seviye 2'ye yukselmene yardim edecegim!\n\nIlk tarifi ogrenmek icin «Kabul» butonuna bas!",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            isConfirmHelpStep = true
        });

        // 2. Согласие на списание 100 золота, 5 камней, 1 свитка
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Для изучения и создания первого зелья нам потребуется:\n• <b><color=#FFE57F>100 Золота</color></b>\n• <b><color=#80FFDB>5 Магических Камней</color></b>\n• <b><color=#FFD166>1 Древний Свиток</color></b>\n\nТы согласен отдать эти ресурсы на изготовление первого рецепта?",
            textEN = "To research and brew the first potion, we need:\n• <b><color=#FFE57F>100 Gold</color></b>\n• <b><color=#80FFDB>5 Magic Stones</color></b>\n• <b><color=#FFD166>1 Ancient Scroll</color></b>\n\nDo you agree to grant these resources for the first craft?",
            textTR = "Ilk iksiri hazirlamak icin gerekenler:\n• <b><color=#FFE57F>100 Altin</color></b>\n• <b><color=#80FFDB>5 Buyulu Tas</color></b>\n• <b><color=#FFD166>1 Kadim Parsomen</color></b>\n\nBu kaynaklari vermeyi kabul ediyor musun?",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            isConfirmRecipeStep = true
        });

        // 3. Появление свитка слева от календаря и открытие
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Ура! Ты получил <b><color=#80FFDB>+10 XP</color></b> и поднял свой уровень! Слева от календарика появился наш <b><color=#FFD166>Свиток Рецептов</color></b>!\n\nНажми кнопку ниже, чтобы развернуть его и посмотреть формулу!",
            textEN = "Hooray! You gained <b><color=#80FFDB>+10 XP</color></b> and leveled up! Our <b><color=#FFD166>Recipe Scroll</color></b> appeared next to the calendar!\n\nTap below to open and inspect the formula!",
            textTR = "Tebrikler! <b><color=#80FFDB>+10 XP</color></b> kazandin ve seviye atladin! Takvimin solunda <b><color=#FFD166>Tarif Parsomenimiz</color></b> belirdi!\n\nFormulu gormek icin asagidaki butona bas!",
            revealResourceIndex = 4,
            showCalendarIcon = true,
            revealAvatarUI = true,
            showSmallScrollIcon = true,
            isRecipeStep = true
        });
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

        if (cauldronButton != null) cauldronButton.SetActive(true);
        if (roomCatObject != null) roomCatObject.SetActive(true);
        if (smallScrollIconButton != null) smallScrollIconButton.SetActive(true);
        SetSmallScrollInteractable(true);
        SetCalendarButtonInteractable(true);
        if (avatarManager != null) avatarManager.SetAvatarButtonInteractable(true);
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

    // -------------------------------------------------------------
    // ФАЗА 1: СТАРТОВЫЙ ДИАЛОГ И КАЛЕНДАРЬ
    // -------------------------------------------------------------
    private void BuildIntroScenario()
    {
        dialogueSteps.Clear();

        // 0. Имя
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

        // 5. Кристаллы
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

        // 7. Показ календаря
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Отлично! Ресурсы у тебя. Взгляни: на экране появился наш <b><color=#FFE57F>Магический Календарь Алхимика</color></b>!",
            textEN = "Great! You have the resources. Look: our <b><color=#FFE57F>Alchemist Magic Calendar</color></b> has appeared!",
            textTR = "Harika! Kaynaklar sende. Ekrana bak: <b><color=#FFE57F>Simyaci Buyulu Takvimimiz</color></b> belirdi!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 8. Ежемесячные награды
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>Каждый день ты будешь ставить печать на числе в календаре и получать порцию наград!\n\nА за <b>полный закрытый месяц</b> без пропусков: <b><color=#FFE57F>30 000 Монет</color></b>, <b><color=#80FFDB>10 Камней</color></b>, <b><color=#FFD166>5 Свитков</color></b> и <b><color=#F384FF>3 Кристалла</color></b>!</size>",
            textEN = "<size=85%>Each day you stamp your date in the calendar and get rewards!\n\nFull month complete: <b><color=#FFE57F>30,000 Coins</color></b>, <b><color=#80FFDB>10 Stones</color></b>, <b><color=#FFD166>5 Scrolls</color></b>, and <b><color=#F384FF>3 Crystals</color></b>!</size>",
            textTR = "<size=85%>Her gun takvime damga vuracaksin!\n\nTam ay bonusu: <b><color=#FFE57F>30.000 Altin</color></b>, <b><color=#80FFDB>10 Tas</color></b>, <b><color=#FFD166>5 Parsomen</color></b> ve <b><color=#F384FF>3 Kristal</color></b>!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 9. Квартальные супер-бонусы
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>За каждый <b>3-й, 6-й, 9-й и 12-й месяц</b> тебя ждут <b>Квартальные Супер-Бонусы</b>: от <b><color=#FFE57F>35 000 до 90 000 Монет</color></b>, до <b><color=#80FFDB>20 Камней</color></b>, <b><color=#FFD166>15 Свитков</color></b> и до <b><color=#F384FF>20 Кристаллов</color></b>!</size>",
            textEN = "<size=85%>Every <b>3rd, 6th, 9th, and 12th month</b> unlocks <b>Quarterly Super Bonuses</b> up to <b><color=#FFE57F>90k Coins</color></b> and <b><color=#F384FF>20 Crystals</color></b>!</size>",
            textTR = "<size=85%>Her <b>3., 6., 9. ve 12. ayda</b> <b>Super Bonuslar</b> seni bekliyor: <b><color=#FFE57F>90k Altin</color></b> ve <b><color=#F384FF>20 Kristal</color></b>!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 10. Годовой джекпот
        dialogueSteps.Add(new DialogStep
        {
            textRU = "<size=85%>А за <b>целый год (365 дней) без пропусков</b> — мифический <b>Годовой Джекпот</b>: <b><color=#FFE57F>500 000 Монет</color></b>, <b><color=#80FFDB>200 Камней</color></b>, <b><color=#FFD166>100 Свитков</color></b> и <b><color=#F384FF>200 Кристаллов</color></b>!</size>",
            textEN = "<size=85%>And for a <b>full year (365 days)</b> — the <b>Annual Jackpot</b>: <b><color=#FFE57F>500k Coins</color></b>, <b><color=#80FFDB>200 Stones</color></b>, <b><color=#FFD166>100 Scrolls</color></b>, and <b><color=#F384FF>200 Crystals</color></b>!</size>",
            textTR = "<size=85%>Ve <b>tam bir yil (365 gun)</b> boyunca: <b><color=#FFE57F>500k Altin</color></b>, <b><color=#80FFDB>200 Tas</color></b>, <b><color=#FFD166>100 Parsomen</color></b> ve <b><color=#F384FF>200 Kristal</color></b>!</size>",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true
        });

        // 11. Переход в календарь (нажатие кнопки внизу)
        dialogueSteps.Add(new DialogStep
        {
            textRU = "Сейчас я покажу тебе календарь. Поставь отметку на сегодняшнем числе — с этого дня начнется твой отсчет посещаемости!\n\nНажми кнопку ниже, чтобы открыть календарь!",
            textEN = "Now I will show you the calendar. Stamp today's date — your attendance streak begins today!\n\nClick the button below to open the calendar!",
            textTR = "Simdi takvimi gosterecegim. Bugunku tarihi damgala — giris takibin baslasin!\n\nTakvimi acmak icin asagidaki butona bas!",
            isNameInputStep = false,
            revealResourceIndex = 4,
            showCalendarIcon = true,
            isCalendarOpenStep = true
        });
    }
}
