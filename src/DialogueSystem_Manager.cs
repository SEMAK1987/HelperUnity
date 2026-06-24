// [ZENITH AUDIO AUTONOMY & STANDALONE ROUTING v18.9.0]
// Dynamic Exclusive Dialogue System for Fate Continent
// Handles custom styled polygon dialogue canvases, companion on LHS, player avatar on RHS, and translation updates automatically.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // ОБЯЗАТЕЛЬНО ДОБАВИТЬ ДЛЯ ПЕРЕХОДА МЕЖДУ СЦЕНАМИ И КАРТАМИ ЗАЧИСТКИ

namespace FateContinent
{
    public class DialogueSystem_Manager : MonoBehaviour
    {
        public static DialogueSystem_Manager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DialogueSystem_Manager>();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        private static DialogueSystem_Manager instance;

        [Header("Действующие Персонажи")]
        public string companionNameRU = "Аэлисса";
        public string companionNameEN = "Aelyssa";
        
        [Header("Настройки Изображений Портретов (Спрайты)")]
        public Sprite companionPortrait;    // Назначается в инспекторе или генерируется программно
        public Sprite warriorPortrait;      // Портрет воина для правой стороны
        public Sprite archerPortrait;       // Портрет стрелка для правой стороны
        public Sprite magePortrait;         // Портрет мага для правой стороны

        [Header("Настройки Звуков Речи Помощника")]
        public AudioClip companionVoiceClip; // Короткие фракции помощника при репликах

        [Header("📐 Ручная Настройка Координат Текста (Inspector Tweaks)")]
        [Tooltip("Включить принудительное применение этих координат в игре. Позволяет двигать текст и кнопки прямо из инспектора в режиме игры!")]
        public bool enforceCoordinates = true;

        [Header("Левый текст имени (Companion Left Name)")]
        public Vector2 companionNameAnchorMin = new Vector2(0f, 0f);
        public Vector2 companionNameAnchorMax = new Vector2(0f, 0f);
        public Vector2 companionNamePosition = new Vector2(173f, 40f);
        public Vector2 companionNameSizeDelta = new Vector2(250f, 40f);
        public TextAlignmentOptions companionNameAlignment = TextAlignmentOptions.Left;

        [Header("Правый текст имени (Hero/Player Right Name)")]
        public Vector2 heroNameAnchorMin = new Vector2(1f, 0f);
        public Vector2 heroNameAnchorMax = new Vector2(1f, 0f);
        public Vector2 heroNamePosition = new Vector2(-173f, 40f);
        public Vector2 heroNameSizeDelta = new Vector2(250f, 40f);
        public TextAlignmentOptions heroNameAlignment = TextAlignmentOptions.Right;

        [Header("Текст реплики диалога (Dialogue Body Text)")]
        public Vector2 bodyTextAnchorMin = new Vector2(0f, 0f);
        public Vector2 bodyTextAnchorMax = new Vector2(1f, 1f);
        [Tooltip("Отступ снизу и отступ слева в формате (Left, Bottom)")]
        public Vector2 bodyTextOffsetMin = new Vector2(250f, 65f); // 250px слева, чтобы не накладываться на левого героя, 65px снизу
        [Tooltip("Отступ сверху и отступ справа в формате (-Right, -Top)")]
        public Vector2 bodyTextOffsetMax = new Vector2(-250f, -65f); // 250px справа, чтобы не накладываться на правого героя, 65px сверху под шапку!
        public TextAlignmentOptions bodyTextAlignment = TextAlignmentOptions.TopLeft;

        [Header("Панель кнопок выбора (Choice Container Panel)")]
        public Vector2 choiceContainerPosition = new Vector2(0f, -15f); // Сдвинуто вниз, чтобы кнопки не накладывались на текст и лица!
        public Vector2 choiceContainerSizeDelta = new Vector2(-120f, 44f); // Оптимальная ширина и высота панели кнопок

        [Header("Кнопки выбора (Choice Buttons Style)")]
        public float choiceButtonFontSize = 13f;
        public float choiceButtonMinWidth = 180f;
        public float choiceButtonPreferredWidth = 260f;
        public float choiceButtonMinHeight = 42f;

        [Header("🔄 Переход на сцены уровней")]
        [Tooltip("Загружать ли сцены уровней при выборе во вводном диалоге? Если выключено, игрок просто останется на интерактивной карте мира.")]
        public bool loadScenesOnEnd = false;
        
        [Tooltip("Имя сцены для Кровавых Пустошей")]
        public string sceneCrimsonWastes = "Scene_CrimsonWastes";
        [Tooltip("Имя сцены для Ледяного Пика")]
        public string sceneIceBoundPeak = "Scene_IceBoundPeak";
        [Tooltip("Имя сцены для Древних Руин")]
        public string sceneAncientRuins = "Scene_AncientRuins";

        [System.Serializable]
        public class DialogLine
        {
            public string characterName;  // Если пусто, используется имя помощника
            public string textRU;
            public string textEN;
            public string textDE;         // Немецкий перевод
            public string textFR;         // Французский перевод
            public string textES;         // Испанский перевод
            public string textPT;         // Португальский перевод
            public string textJA;         // Японский перевод
            public string textCH;         // Китайский перевод
            public string textKR;         // Корейский перевод
            public string[] choicesRU;    // Ответные реплики игрока (ветвление диалога)
            public string[] choicesEN;
            public string[] choicesDE;
            public string[] choicesFR;
            public string[] choicesES;
            public string[] choicesPT;
            public string[] choicesJA;
            public string[] choicesCH;
            public string[] choicesKR;
            public int[] nextLineIndexes; // Индексы следующих реплик при выборе ответов
        }

        [Header("Диалоговый пул")]
        public List<DialogLine> dialogueSteps = new List<DialogLine>();

        // Элементы UI (строятся динамически, чтобы избежать пропажи ссылок при смене сцен)
        private Canvas dialogCanvas;
        private GameObject dialogPanel;
        private Image companionPortraitImage;
        private Image playerPortraitImage;
        private TextMeshProUGUI speakerNameText;
        private TextMeshProUGUI leftSpeakerNameText;
        private TextMeshProUGUI rightSpeakerNameText;
        private TextMeshProUGUI dialogueBodyText;
        private List<Button> choiceButtons = new List<Button>();
        private GameObject choiceContainer;
        private Button nextDialogueButton;
        private Sprite originalCompanionSprite;
        private Sprite originalPlayerSprite;

        private int currentLineIndex = 0;
        private int selectedZoneIndex = 0; // Текущая выбранная интерактивная область высадки
        private bool isDialogueActive = false;

        public bool IsDialogueActive
        {
            get { return isDialogueActive; }
        }

        public int SelectedZoneIndex
        {
            get { return selectedZoneIndex; }
        }

        private enum InputSystemType { Unchecked, OldInput, NewInput }
        private InputSystemType activeInputType = InputSystemType.Unchecked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDialogueUI();
                SetupFallbackDialogues();
            }
            else if (Instance != this)
            {
                // Если мы дубликат, не уничтожаем весь GameObject, если на нем висит Canvas или важная структура диалогов!
                // Иначе при переходе между сценами удалится вся канва диалогов!
                if (GetComponent<Canvas>() != null || name.Contains("CANVAS") || transform.Find("DialoguePanel") != null)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Автоматическое переподключение и избавление от лишних окон во всех сценах геймплея
            InitializeDialogueUI();
            
            // Скрываемый на старте диалог во избежание визуальных багов
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // При старте скрываем диалоговое окно
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }

            // Загружаем сохраненный индекс высадки игрока, чтобы избежать сброса в 0 при обновлении или переходе
            selectedZoneIndex = PlayerPrefs.GetInt("LandedZoneIndex", 0);
            if (selectedZoneIndex < 0) selectedZoneIndex = 0;
            Debug.Log($"[DIALOGUE SYSTEM] Восстановлен LandedZoneIndex из PlayerPrefs: {selectedZoneIndex}");
        }

        public void ApplyRectTransformTweaks()
        {
            if (!enforceCoordinates) return;

            if (leftSpeakerNameText != null)
            {
                RectTransform rt = leftSpeakerNameText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    if (leftSpeakerNameText.gameObject.name != "Txt_CompanionName_Dynamic")
                    {
                        rt.anchorMin = companionNameAnchorMin;
                        rt.anchorMax = companionNameAnchorMax;
                        rt.anchoredPosition = companionNamePosition;
                        rt.sizeDelta = companionNameSizeDelta;
                        leftSpeakerNameText.alignment = companionNameAlignment;
                    }
                    else
                    {
                        // Для динамической плашки позиция строго внизу по центру портрета
                        rt.anchorMin = new Vector2(0.5f, 0f);
                        rt.anchorMax = new Vector2(0.5f, 0f);
                        rt.pivot = new Vector2(0.5f, 1f);
                        rt.anchoredPosition = new Vector2(0f, -10f);
                        rt.sizeDelta = new Vector2(250f, 40f);
                        leftSpeakerNameText.alignment = TextAlignmentOptions.Center;
                    }
                }
            }

            if (rightSpeakerNameText != null)
            {
                RectTransform rt = rightSpeakerNameText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    if (rightSpeakerNameText.gameObject.name != "Txt_PlayerName_Dynamic")
                    {
                        rt.anchorMin = heroNameAnchorMin;
                        rt.anchorMax = heroNameAnchorMax;
                        rt.anchoredPosition = heroNamePosition;
                        rt.sizeDelta = heroNameSizeDelta;
                        rightSpeakerNameText.alignment = heroNameAlignment;
                    }
                    else
                    {
                        // Для динамической плашки героя позиция строго внизу по центру портрета
                        rt.anchorMin = new Vector2(0.5f, 0f);
                        rt.anchorMax = new Vector2(0.5f, 0f);
                        rt.pivot = new Vector2(0.5f, 1f);
                        rt.anchoredPosition = new Vector2(0f, -10f);
                        rt.sizeDelta = new Vector2(250f, 40f);
                        rightSpeakerNameText.alignment = TextAlignmentOptions.Center;
                    }
                }
            }

            if (dialogueBodyText != null)
            {
                RectTransform rt = dialogueBodyText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = bodyTextAnchorMin;
                    rt.anchorMax = bodyTextAnchorMax;
                    rt.offsetMin = bodyTextOffsetMin;
                    rt.offsetMax = bodyTextOffsetMax;
                }
                dialogueBodyText.alignment = bodyTextAlignment;
            }

            if (choiceContainer != null)
            {
                RectTransform rt = choiceContainer.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    
                    if (currentLineIndex == 3)
                    {
                        rt.anchoredPosition = new Vector2(0f, -20f);
                        rt.sizeDelta = new Vector2(-120f, 44f);
                    }
                    else
                    {
                        rt.anchoredPosition = choiceContainerPosition;
                        rt.sizeDelta = choiceContainerSizeDelta;
                    }
                }
            }

            if (nextDialogueButton != null && enforceCoordinates)
            {
                RectTransform rt = nextDialogueButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.anchoredPosition = new Vector2(-40f, 25f); // Идеальный отступ снизу и справа
                    rt.sizeDelta = new Vector2(150f, 42f);
                }

                Image bg = nextDialogueButton.GetComponent<Image>();
                if (bg == null) bg = nextDialogueButton.gameObject.AddComponent<Image>();
                if (bg != null)
                {
                    if (bg.color.a < 0.1f || bg.color == Color.white)
                    {
                        bg.color = new Color(0.08f, 0.12f, 0.22f, 0.85f);
                    }
                }

                Outline outline = nextDialogueButton.GetComponent<Outline>();
                if (outline == null) outline = nextDialogueButton.gameObject.AddComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.75f); // Голубой Zenith неон
                    outline.effectDistance = new Vector2(1.5f, 1.5f);
                    outline.enabled = true;
                }
            }
        }

        private void Update()
        {
            if (!isDialogueActive) return;

            // Применяем настройки ручного позиционирования каждый кадр в Update для мгновенного тюнинга в Редакторе Unity!
            ApplyRectTransformTweaks();

            // Если есть активные варианты выбора, клавиатура / случайные клики не продвигают диалог автоматически
            DialogLine currentLine = dialogueSteps[currentLineIndex];
            int lang = Translator.LanguageID;
            string[] currentChoices = GetDialogueChoices(currentLine, lang);

            bool hasChoices = currentChoices != null && currentChoices.Length > 0;

            if (!hasChoices)
            {
                // Позволяет пропускать диалог кликом мыши или кнопками пробел/ввод!
                if (IsMouseButtonDownZero() || IsSpaceOrReturnPressed())
                {
                    AdvanceDialogue();
                }
            }
            else
            {
                // Позволяет делать выбор цифрами 1, 2, 3 на клавиатуре для невероятного удобства!
                if (IsAlphaPressed(1))
                {
                    if (currentChoices.Length >= 1) SelectChoice(0);
                }
                else if (IsAlphaPressed(2))
                {
                    if (currentChoices.Length >= 2) SelectChoice(1);
                }
                else if (IsAlphaPressed(3))
                {
                    if (currentChoices.Length >= 3) SelectChoice(2);
                }
            }
        }

        private bool IsSpaceOrReturnPressed()
        {
            if (activeInputType == InputSystemType.NewInput)
            {
                return CheckNewInputSpaceOrReturn();
            }
            else if (activeInputType == InputSystemType.OldInput)
            {
                try
                {
                    return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
                }
                catch (System.InvalidOperationException)
                {
                    activeInputType = InputSystemType.NewInput;
                    return CheckNewInputSpaceOrReturn();
                }
            }

            try
            {
                bool pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
                activeInputType = InputSystemType.OldInput;
                return pressed;
            }
            catch (System.InvalidOperationException)
            {
                activeInputType = InputSystemType.NewInput;
                return CheckNewInputSpaceOrReturn();
            }
        }

        private bool IsMouseButtonDownZero()
        {
            if (activeInputType == InputSystemType.NewInput)
            {
                return CheckNewInputMousePressed();
            }
            else if (activeInputType == InputSystemType.OldInput)
            {
                try
                {
                    return Input.GetMouseButtonDown(0);
                }
                catch (System.InvalidOperationException)
                {
                    activeInputType = InputSystemType.NewInput;
                    return CheckNewInputMousePressed();
                }
            }

            try
            {
                bool pressed = Input.GetMouseButtonDown(0);
                activeInputType = InputSystemType.OldInput;
                return pressed;
            }
            catch (System.InvalidOperationException)
            {
                activeInputType = InputSystemType.NewInput;
                return CheckNewInputMousePressed();
            }
        }

        private bool IsAlphaPressed(int alphaNum)
        {
            if (activeInputType == InputSystemType.NewInput)
            {
                return CheckNewInputAlpha(alphaNum);
            }
            else if (activeInputType == InputSystemType.OldInput)
            {
                try
                {
                    if (alphaNum == 1) return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
                    if (alphaNum == 2) return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
                    if (alphaNum == 3) return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
                    return false;
                }
                catch (System.InvalidOperationException)
                {
                    activeInputType = InputSystemType.NewInput;
                    return CheckNewInputAlpha(alphaNum);
                }
            }

            try
            {
                bool pressed = false;
                if (alphaNum == 1) pressed = Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
                if (alphaNum == 2) pressed = Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
                if (alphaNum == 3) pressed = Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
                activeInputType = InputSystemType.OldInput;
                return pressed;
            }
            catch (System.InvalidOperationException)
            {
                activeInputType = InputSystemType.NewInput;
                return CheckNewInputAlpha(alphaNum);
            }
        }

        private bool CheckNewInputSpaceOrReturn()
        {
            try
            {
                var inputSystemAssembly = System.Reflection.Assembly.Load("Unity.InputSystem");
                if (inputSystemAssembly != null)
                {
                    var keyboardType = inputSystemAssembly.GetType("UnityEngine.InputSystem.Keyboard");
                    if (keyboardType != null)
                    {
                        var currentProperty = keyboardType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var keyboardInstance = currentProperty?.GetValue(null);
                        if (keyboardInstance != null)
                        {
                            var spaceKeyProperty = keyboardInstance.GetType().GetProperty("spaceKey");
                            var spaceKeyInstance = spaceKeyProperty?.GetValue(keyboardInstance);
                            if (spaceKeyInstance != null)
                            {
                                var wasPressedProperty = spaceKeyInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(spaceKeyInstance))
                                {
                                    return true;
                                }
                            }

                            var enterKeyProperty = keyboardInstance.GetType().GetProperty("enterKey");
                            var enterKeyInstance = enterKeyProperty?.GetValue(keyboardInstance);
                            if (enterKeyInstance != null)
                            {
                                var wasPressedProperty = enterKeyInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(enterKeyInstance))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки рефлексии
            }
            return false;
        }

        private bool CheckNewInputMousePressed()
        {
            try
            {
                var inputSystemAssembly = System.Reflection.Assembly.Load("Unity.InputSystem");
                if (inputSystemAssembly != null)
                {
                    var mouseType = inputSystemAssembly.GetType("UnityEngine.InputSystem.Mouse");
                    if (mouseType != null)
                    {
                        var currentProperty = mouseType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var mouseInstance = currentProperty?.GetValue(null);
                        if (mouseInstance != null)
                        {
                            var leftButtonProperty = mouseInstance.GetType().GetProperty("leftButton");
                            var leftButtonInstance = leftButtonProperty?.GetValue(mouseInstance);
                            if (leftButtonInstance != null)
                            {
                                var wasPressedProperty = leftButtonInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(leftButtonInstance))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    var pointerType = inputSystemAssembly.GetType("UnityEngine.InputSystem.Pointer");
                    if (pointerType != null)
                    {
                        var currentProperty = pointerType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var pointerInstance = currentProperty?.GetValue(null);
                        if (pointerInstance != null)
                        {
                            var pressProperty = pointerInstance.GetType().GetProperty("press");
                            var pressInstance = pressProperty?.GetValue(pointerInstance);
                            if (pressInstance != null)
                            {
                                var wasPressedProperty = pressInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(pressInstance))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки рефлексии
            }
            return false;
        }

        private bool CheckNewInputAlpha(int number)
        {
            try
            {
                var inputSystemAssembly = System.Reflection.Assembly.Load("Unity.InputSystem");
                if (inputSystemAssembly != null)
                {
                    var keyboardType = inputSystemAssembly.GetType("UnityEngine.InputSystem.Keyboard");
                    if (keyboardType != null)
                    {
                        var currentProperty = keyboardType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var keyboardInstance = currentProperty?.GetValue(null);
                        if (keyboardInstance != null)
                        {
                            string keyName = "digit" + number + "Key";
                            var keyProperty = keyboardInstance.GetType().GetProperty(keyName);
                            var keyInstance = keyProperty?.GetValue(keyboardInstance);
                            if (keyInstance != null)
                            {
                                var wasPressedProperty = keyInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(keyInstance))
                                {
                                    return true;
                                }
                            }

                            string numpadName = "numpad" + number + "Key";
                            var numpadProperty = keyboardInstance.GetType().GetProperty(numpadName);
                            var numpadInstance = numpadProperty?.GetValue(keyboardInstance);
                            if (numpadInstance != null)
                            {
                                var wasPressedProperty = numpadInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null && (bool)wasPressedProperty.GetValue(numpadInstance))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки рефлексии
            }
            return false;
        }

        /// <summary>
        /// Событие вызова при клике на маркер карты. v18.11.2 (3-argument overload)
        /// </summary>
        public void OnMapMarkerClicked(int associatedDialogueIndex, string factionName, string factionDescription)
        {
            OnMapMarkerClicked((object)associatedDialogueIndex, (object)factionName, (object)factionDescription);
        }

        /// <summary>
        /// Общий динамический оверлод для поддержки различных типов. v18.11.2 (3-argument overload)
        /// </summary>
        public void OnMapMarkerClicked(object associatedDialogueIndex, object factionName, object factionDescription)
        {
            // Пытаемся безопасно распарсить dialogueIndex
            int dialogueIndex = 3; // fallback default
            if (associatedDialogueIndex != null)
            {
                if (associatedDialogueIndex is int)
                    dialogueIndex = (int)associatedDialogueIndex;
                else if (associatedDialogueIndex is float)
                    dialogueIndex = Mathf.RoundToInt((float)associatedDialogueIndex);
                else if (associatedDialogueIndex is double)
                    dialogueIndex = Mathf.RoundToInt((float)(double)associatedDialogueIndex);
                else
                    int.TryParse(associatedDialogueIndex.ToString(), out dialogueIndex);
            }

            string fName = factionName != null ? factionName.ToString() : "";
            string fDesc = factionDescription != null ? factionDescription.ToString() : "";

            Debug.Log($"[DialogueSystem_Manager] Разбор клика по маркеру: Index={dialogueIndex}, Name={fName}, Desc={fDesc}");

            // Открываем диалоговое окно на нужном шаге
            StartDialogue(dialogueIndex);

            // Если шаг равен 3 (выбор локации), автоматически устанавливаем selectedZoneIndex!
            if (dialogueIndex == 3)
            {
                // Поиск подходящей зоны по названию фракции/маркера
                string fLower = fName.ToLower();
                int targetZone = 0; // По умолчанию Кровавые Пустоши

                if (fLower.Contains("пик") || fLower.Contains("ice") || fLower.Contains("ледян"))
                {
                    targetZone = 1;
                }
                else if (fLower.Contains("руин") || fLower.Contains("ancient") || fLower.Contains("развалин") || fLower.Contains("окраин"))
                {
                    targetZone = 2;
                }
                else if (fLower.Contains("святилищ") || fLower.Contains("zenith") || fLower.Contains("грозов") || fLower.Contains("storm"))
                {
                    targetZone = 3;
                }

                selectedZoneIndex = targetZone;

                // Обновляем визуальные состояния и карту
                if (FateMapManager.Instance != null)
                {
                    FateMapManager.Instance.HighlightRing(targetZone);
                }

                UpdateChoiceSelectionVisuals(targetZone);
                UpdateInteractiveZoneText(targetZone);
            }
        }

        /// <summary>
        /// Запустить диалог по индексу реплики
        /// </summary>
        public void StartDialogue(int startIndex = 0)
        {
            if (dialogueSteps.Count < 13 || startIndex == 0)
            {
                SetupFallbackDialogues();
            }

            if (startIndex == 0)
            {
                PlayerPrefs.SetInt("ContinentGameplayActive", 0);
                PlayerPrefs.Save();
                if (FateCastleManager.Instance != null)
                {
                    FateCastleManager.Instance.ResetToInitialSettings();
                }
                
                // [BACKGROUND SYNC] Показываем мистический фон Zenith при входе в стартовый диалог выбора
                if (FateDialogueBackgroundController.Instance != null)
                {
                    FateDialogueBackgroundController.Instance.ShowBackground(1.0f);
                }
            }

            isDialogueActive = true;
            currentLineIndex = startIndex;

            if (dialogPanel == null)
            {
                InitializeDialogueUI();
            }
            else
            {
                // Гарантируем, что корневой холст активен
                if (dialogCanvas != null)
                {
                    dialogCanvas.gameObject.SetActive(true);
                }
                else
                {
                    GameObject rootCanvas = GameObject.Find("FATE_DIALOGUE_CANVAS");
                    if (rootCanvas != null) rootCanvas.SetActive(true);
                }
            }

            dialogPanel.SetActive(true);
            UpdateDialogueView();

            // Блокируем движение игрока или воспроизводим звук появления диалога
            if (SettingsManager.Instance != null && companionVoiceClip != null)
            {
                SettingsManager.Instance.PlaySoundEffect(companionVoiceClip);
            }
            
            Debug.Log($"[DIALOGUE SYSTEM] Диалог запущен с реплики {startIndex}. Помощник на левой стороне, Герой - на правой.");
        }

        /// <summary>
        /// Показать следующую реплику
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!isDialogueActive) return;

            DialogLine currentLine = dialogueSteps[currentLineIndex];

            // Если мы находимся на шаге 3 (выбор места высадки) и нажимаем "Далее/Высадка" (кнопка подтверждения)
            if (currentLineIndex == 3)
            {
                // Подтверждаем текущую выбранную зону высадки принудительно!
                SelectChoice(selectedZoneIndex, true);
                return;
            }

            // Если есть варианты выбора, игрок должен кликнуть по кнопке выбора, обычный клик не продвигает диалог
            string[] currentChoices = GetDialogueChoices(currentLine, Translator.LanguageID);
            bool hasChoices = currentChoices != null && currentChoices.Length > 0;

            if (hasChoices)
            {
                return; 
            }

            // Переход на следующую реплику
            if (currentLineIndex == 11)
            {
                if (FateCastleManager.Instance != null)
                {
                    FateCastleManager.Instance.SpawnAllCastles();
                }
            }

            if (currentLine.nextLineIndexes != null && currentLine.nextLineIndexes.Length > 0)
            {
                currentLineIndex = currentLine.nextLineIndexes[0];
                UpdateDialogueView();
            }
            else
            {
                // Реплики закончились — закрываем диалог
                EndDialogue();
            }
        }

        private bool IsSceneInBuildSettings(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public void SelectChoice(int choiceIndex)
        {
            SelectChoice(choiceIndex, false);
        }

        public void SelectChoice(int choiceIndex, bool forceConfirm)
        {
            if (!isDialogueActive) return;

            DialogLine currentLine = dialogueSteps[currentLineIndex];

            // Интерактивная синхронизация: если мы делаем выбор в шаге 3 (выбор места высадки на тактическую карту)
            if (currentLineIndex == 3)
            {
                if (!forceConfirm)
                {
                    // Игрок переключил выбор зоны кликом по кнопке, подсвечиваем ее на карте но не переходим на следующий слайд!
                    selectedZoneIndex = choiceIndex;
                    
                    if (FateMapManager.Instance != null)
                    {
                        FateMapManager.Instance.SetMapVisible(true);
                        FateMapManager.Instance.HighlightRing(choiceIndex);
                        Debug.Log($"[DIALOGUE SYSTEM] Игрок сфокусировался на зоне {choiceIndex}. Подсвечиваем маркер на карте.");
                    }

                    // Обновляем визуальные состояния кнопок выбора и текст описания зоны
                    UpdateChoiceSelectionVisuals(choiceIndex);
                    UpdateInteractiveZoneText(choiceIndex);

                    // Воспроизводим звук ховера/переключения меню
                    if (SettingsManager.Instance != null && UIButtonSfxBinder.Instance != null && UIButtonSfxBinder.Instance.clickSound != null)
                    {
                        SettingsManager.Instance.PlaySoundEffect(UIButtonSfxBinder.Instance.clickSound);
                    }
                    return;
                }
                
                // Если мы кликнули повторно по уже выбранному региону или подтверждаем через "Высадка ➔"
                if (FateMapManager.Instance != null)
                {
                    // Гарантируем сброс в обычное состояние перед закрытием
                    FateMapManager.Instance.SwitchToMap(0);
                    FateMapManager.Instance.SetMapVisible(true);
                }

                // СОХРАНЯЕМ ВЫБРАННЫЙ ТАКТИЧЕСКИЙ ДЕСАНТ!
                string factionName = "Багровые Пустоши";
                if (selectedZoneIndex == 1) factionName = "Ледяной Пик";
                else if (selectedZoneIndex == 2) factionName = "Древние Руины";
                else if (selectedZoneIndex == 3) factionName = "Святилище Зенита";

                PlayerPrefs.SetInt("LandedZoneIndex", selectedZoneIndex);
                PlayerPrefs.SetString("LandedZoneName", factionName);
                PlayerPrefs.Save();
                Debug.Log($"[DIALOGUE SYSTEM] Сохранено в PlayerPrefs: LandedZoneIndex={selectedZoneIndex}, LandedZoneName={factionName}");
                
                if (FateCastleManager.Instance != null)
                {
                    FateCastleManager.Instance.SpawnAllCastles();
                }
            }

            if (currentLine.nextLineIndexes != null && choiceIndex < currentLine.nextLineIndexes.Length)
            {
                // Проигрываем приятный звук клика по диалоговому варианту
                if (SettingsManager.Instance != null && UIButtonSfxBinder.Instance != null && UIButtonSfxBinder.Instance.clickSound != null)
                {
                    SettingsManager.Instance.PlaySoundEffect(UIButtonSfxBinder.Instance.clickSound);
                }

                currentLineIndex = currentLine.nextLineIndexes[choiceIndex];
                UpdateDialogueView();
            }
            else
            {
                EndDialogue();
            }
        }

        // Обновляет визуальное выделение интерактивных кнопок выбора (делает выбранную золотой/неоновой, а остальные приглушенными)
        private void UpdateChoiceSelectionVisuals(int selectedIndex)
        {
            if (choiceButtons == null || choiceButtons.Count == 0) return;

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                Button btn = choiceButtons[i];
                if (btn == null) continue;

                // Завершающая кнопка подтверждения (последний элемент на шаге 3) имеет свой собственный статический стиль изумрудного Zenith Glass
                if (currentLineIndex == 3 && i == choiceButtons.Count - 1) continue;

                Image btnImg = btn.GetComponent<Image>();
                Outline outline = btn.GetComponent<Outline>();
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i == selectedIndex)
                {
                    // Подсвечиваем выбранную кнопку: тепло-золотой фон
                    if (btnImg != null) btnImg.color = new Color(0.95f, 0.61f, 0.07f, 0.95f);
                    if (outline != null)
                    {
                        outline.effectColor = new Color(1f, 0.85f, 0.1f, 0.95f); // Золотой неон
                        outline.effectDistance = new Vector2(3f, 3f);
                    }
                    if (tmp != null)
                    {
                        tmp.fontWeight = FontWeight.Black;
                        tmp.color = Color.white;
                    }
                }
                else
                {
                    // Приглушенные невыбранные варианты
                    if (btnImg != null) btnImg.color = new Color(0.08f, 0.09f, 0.15f, 0.95f);
                    if (outline != null)
                    {
                        outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.45f); // Обычный циановый неон
                        outline.effectDistance = new Vector2(1f, 1f);
                    }
                    if (tmp != null)
                    {
                        tmp.fontWeight = FontWeight.Regular;
                        tmp.color = new Color(0.7f, 0.8f, 0.9f, 0.85f);
                    }
                }
            }
        }

        // Динамическое обновление текста диалога с подробным тактическим описанием миров
        private void UpdateInteractiveZoneText(int zoneIndex)
        {
            int lang = Translator.LanguageID;
            string desc = "";
            if (lang == 0) // RU
            {
                if (zoneIndex == 0) desc = "<b>[Кровавые Пустоши]:</b> Выжженные песчаные дюны и бури под вечным багровым небом. Регенерация снижена. <i>(Нажмите кнопку завершения для подтверждения)</i>";
                else if (zoneIndex == 1) desc = "<b>[Ледяной Пик]:</b> Царство вечной мерзлоты. Обитают ледяные гиганты. Скорость отряда снижена на льду. <i>(Нажмите кнопку завершения для подтверждения)</i>";
                else if (zoneIndex == 2) desc = "<b>[Древние Руины]:</b> Величественные руины династии. Энергетические осколки и каменные стражи. <i>(Нажмите кнопку завершения для подтверждения)</i>";
                else desc = "<b>[Грозовые Кряжи]:</b> Облачный архипелаг, парящий над бездной. Здесь бушуют постоянные молнии, а воздух раздирают стихийные бури. <i>(Нажмите кнопку завершения для подтверждения)</i>";
            }
            else if (lang == 8) // CH
            {
                if (zoneIndex == 0) desc = "<b>[绯红荒野]:</b> 烈日下的沙丘与强盗。战士生命回复减慢。<i>(请点击确认按钮以完成选择)</i>";
                else if (zoneIndex == 1) desc = "<b>[冰封之巅]:</b> 极寒冻土与巨人，生存难度极高。移动速度降低。<i>(请点击确认按钮以完成选择)</i>";
                else if (zoneIndex == 2) desc = "<b>[远古遗迹]:</b> 遗留的能量水晶，充斥着机关巨像。<i>(请点击确认按钮以完成选择)</i>";
                else desc = "<b>[雷暴山脊]:</b> 悬浮在深渊之上的云中群岛。雷电肆虐，元素风暴撕裂空气。<i>(请点击确认按钮以完成选择)</i>";
            }
            else if (lang == 7) // KR
            {
                if (zoneIndex == 0) desc = "<b>[크림슨 황무지]:</b> 뜨거운 모래 사막과 마력 폭풍. 체력 재생 감소. <i>(확인 버튼을 누르면 선택이 완료됩니다)</i>";
                else if (zoneIndex == 1) desc = "<b>[빙설의 봉우리]:</b> 영구 동토와 혹한. 부대 이동 속도 감소. <i>(확인 버튼을 누르면 선택이 완료됩니다)</i>";
                else if (zoneIndex == 2) desc = "<b>[고대 유적지]:</b> 고대 문명의 파편 and 힘의 결정. 경비 석상이 작동 중. <i>(확인 버튼을 누르면 선택이 완료됩니다)</i>";
                else desc = "<b>[폭풍 산맥]:</b> 심연 위에 떠 있는 구름 군도. 끊임없는 번개와 원소의 폭풍. <i>(확인 버튼을 누르면 선택이 완료됩니다)</i>";
            }
            else // EN / fallback
            {
                if (zoneIndex == 0) desc = "<b>[Crimson Wastes]:</b> Scorched ruins and firestorms. Reduced health regeneration. <i>(Press the confirmation button to proceed)</i>";
                else if (zoneIndex == 1) desc = "<b>[Ice-Bound Peak]:</b> Ultimate permafrost and frost behemoths. Reduced move speed on ice. <i>(Press the confirmation button to proceed)</i>";
                else if (zoneIndex == 2) desc = "<b>[Ancient Ruins]:</b> Ancient ruins with pure energy crystals but guarded by stone traps. <i>(Press the confirmation button to proceed)</i>";
                else desc = "<b>[Storm Ridges]:</b> Cloud archipelago floating over the abyss. Constant lightning storms and elemental tempests. <i>(Press the confirmation button to proceed)</i>";
            }

            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = desc;
            }
        }

        public void SetDialoguePanelActive(bool active)
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(active);
            }
        }

        private void TryLoadScene(string sceneName)
        {
            if (!loadScenesOnEnd || string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"[GATEWAY] Загрузка сцен отключена или имя пустое ({sceneName}). Остаемся на интерактивной карте континентов.");
                return;
            }

            // Интеллектуальное самоисцеление от пустого/зависшего экрана: если сцена отсутствует в Build Settings,
            // мы не пытаемся загрузить её, предотвращая вечную пустышку и позволяя играть на интерактивной карте!
            if (!IsSceneInBuildSettings(sceneName))
            {
                Debug.LogWarning($"[GATEWAY] Сцена '{sceneName}' отсутствует в Build Settings. Переход отменен, чтобы избежать пустого экрана. Игрок остается в 'GameScene' на тактической карте.");
                return;
            }

            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GATEWAY] Ошибка загрузки сцены '{sceneName}'. Убедитесь, что вы создали данную сцену и добавили её в Build Settings: {ex.Message}");
            }
        }

        public void EndDialogue()
        {
            if (currentLineIndex == 12)
            {
                isDialogueActive = false;
                if (dialogPanel != null)
                {
                    dialogPanel.SetActive(false);
                }
                
                // [BACKGROUND SYNC] Скрываем фон при окончании пост-десантирования
                if (FateDialogueBackgroundController.Instance != null)
                {
                    FateDialogueBackgroundController.Instance.HideBackground(1.2f);
                }
                
                // Активируем полнофункциональный HUD и 3D спавн замков кампании Континента
                if (FateCastleManager.Instance != null)
                {
                    FateCastleManager.Instance.EnableContinentGameplay();
                }
                
                if (FateMapManager.Instance != null)
                {
                    FateMapManager.Instance.SetMapVisible(false);
                }
                if (StrategicCameraController.Instance != null)
                {
                    StrategicCameraController.Instance.isControlEnabled = true;
                    // Сфокусируем камеру на выбранном замке
                    int playerZone = selectedZoneIndex;
                    if (LandingPositionManager.Instance != null && playerZone < LandingPositionManager.Instance.landingPoints.Length)
                    {
                        var anchor = LandingPositionManager.Instance.landingPoints[playerZone].spawnAnchor;
                        if (anchor != null)
                        {
                            StrategicCameraController.Instance.FocusOnPoint(anchor.position, LandingPositionManager.Instance.cameraOffset);
                        }
                    }
                }
                if (GamePause_Manager.Instance != null)
                {
                    GamePause_Manager.Instance.isPauseBlockedManually = false;
                }
                Debug.Log("[DIALOGUE SYSTEM] Пост-высадочный инструктаж о замках окончен. Все системы разблокированы для свободной игры!");
                return;
            }

            isDialogueActive = false;
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            // [BACKGROUND SYNC] Увядаем и отключаем фон Zenith перед десантированием на 3D карту континента
            if (FateDialogueBackgroundController.Instance != null)
            {
                FateDialogueBackgroundController.Instance.HideBackground(1.5f);
            }
            
            // После завершения диалога скрываем интерактивную тактическую карту и маркеры, чтобы они не засоряли экран!
            if (FateMapManager.Instance != null)
            {
                FateMapManager.Instance.SetMapVisible(false);
                Debug.Log("[DIALOGUE SYSTEM] Сворачиваем интерактивную тактическую карту континентов.");
            }

            // Вызываем ручное 3D позиционирование на физической карте с умным самовосстановлением (включая неактивные объекты)
            LandingPositionManager lpm = LandingPositionManager.Instance;
            if (lpm == null)
            {
                var allLpms = Resources.FindObjectsOfTypeAll<LandingPositionManager>();
                if (allLpms != null && allLpms.Length > 0)
                {
                    foreach (var l in allLpms)
                    {
                        if (l != null && l.gameObject.scene.isLoaded)
                        {
                            lpm = l;
                            lpm.gameObject.SetActive(true);
                            Debug.Log("<color=#00FFCC>[DIALOGUE SYSTEM]</color> Обнаружили и успешно активировали неактивный LandingPositionManager в иерархии сцены!");
                            break;
                        }
                    }
                }
            }

            if (lpm != null)
            {
                lpm.DispatchLanding(selectedZoneIndex);
            }
            else
            {
                Debug.LogWarning("[DIALOGUE SYSTEM] LandingPositionManager не найден ни в активной, ни в неактивной иерархии. Пропускаем физическое 3D десантирование.");
            }

            // Проверяем, на каком шаге мы закончили диалог (это определяет выбранную локацию!)
            if (currentLineIndex == 4)
            {
                Debug.Log("[GATEWAY] Попытка загрузки уровня: Кровавые Пустоши!");
                TryLoadScene(sceneCrimsonWastes);
            }
            else if (currentLineIndex == 5)
            {
                Debug.Log("[GATEWAY] Попытка загрузки уровня: Ледяной Пик!");
                TryLoadScene(sceneIceBoundPeak);
            }
            else if (currentLineIndex == 6)
            {
                Debug.Log("[GATEWAY] Попытка загрузки уровня: Древние Руины!");
                TryLoadScene(sceneAncientRuins);
            }
            
            Debug.Log("[DIALOGUE SYSTEM] Диалог завершен.");
        }

        private string GetLocalizedClassName(string rawClass, int lang)
        {
            if (string.IsNullOrEmpty(rawClass)) return (lang == 0) ? "Герой" : "Hero";
            string cls = rawClass.ToLower();
            if (cls.Contains("warrior") || cls.Contains("воин"))
                return (lang == 0) ? "Воин" : "Warrior";
            if (cls.Contains("archer") || cls.Contains("лучник") || cls.Contains("стрелок") || cls.Contains("strelok") || cls.Contains("снайпер"))
                return (lang == 0) ? "Лучник" : "Archer";
            if (cls.Contains("mage") || cls.Contains("маг"))
                return (lang == 0) ? "Маг" : "Mage";
            return rawClass;
        }

        private void ClearUnwantedBackgrounds(GameObject targetObj)
        {
            if (targetObj == null) return;

            // 1. Убираем компонент Image непосредственно с самого объекта (если вдруг висит)
            Image img = targetObj.GetComponent<Image>();
            if (img != null)
            {
                DestroyImmediate(img);
            }

            // 2. Убираем компонент Outline
            Outline outline = targetObj.GetComponent<Outline>();
            if (outline != null)
            {
                DestroyImmediate(outline);
            }

            // 3. Ищем и уничтожаем дочерние объекты-подложки (BadgeBg, BG, Background и т.д.)
            for (int i = targetObj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = targetObj.transform.GetChild(i);
                if (child != null)
                {
                    string childName = child.name.ToLower();
                    if (childName.Contains("bg") || childName.Contains("badge") || childName.Contains("back") || childName.Contains("image") || childName.Contains("panel") || childName.Contains("banner"))
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }

        private TextMeshProUGUI CreatePortraitNameLabel(Image portraitImage, string defaultText, bool isLeft)
        {
            if (portraitImage == null) return null;

            // Ищем, не создавали ли мы уже этот текст ранее
            string goName = isLeft ? "Txt_CompanionName_Dynamic" : "Txt_PlayerName_Dynamic";
            Transform existingText = portraitImage.transform.Find(goName);
            
            GameObject txtGov;
            RectTransform rect;
            TextMeshProUGUI tmp;

            if (existingText != null)
            {
                txtGov = existingText.gameObject;
                rect = txtGov.GetComponent<RectTransform>();
                tmp = txtGov.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // Создаем игровой объект подписи под портретом с нуля
                txtGov = new GameObject(goName);
                txtGov.transform.SetParent(portraitImage.transform, false);

                rect = txtGov.AddComponent<RectTransform>();
                tmp = txtGov.AddComponent<TextMeshProUGUI>();
            }

            // Очищаем любые мешающие плашки и прямоугольники
            ClearUnwantedBackgrounds(txtGov);

            // --- БЛОК АВТОКОРРЕКЦИИ И САМОИСЦЕЛЕНИЯ КОРДИНАТ И СВОЙСТВ (ЭФФЕКТ ZENITH) ---
            // Сбрасываем локальный масштаб в 1, чтобы избежать сжатия текста
            txtGov.transform.localScale = Vector3.one;

            // Позиционируем плашку имени внизу под портретом по центру (Pivot у верхней кромки: 0.5, 1)
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -10f); // Ровно на 10 пикселей ниже нижней границы портрета
            rect.sizeDelta = new Vector2(250f, 40f);

            // Настройки текста в стиле Аэлиссы (сочный крупный шрифт, золотой/оранжевый цвет)
            if (tmp != null)
            {
                tmp.text = defaultText;

                // Чтобы правая надпись героя выглядела так же, как у Аэлиссы
                if (!isLeft && leftSpeakerNameText != null)
                {
                    tmp.font = leftSpeakerNameText.font;
                    float targetSize = leftSpeakerNameText.fontSize;
                    if (targetSize < 18f) targetSize = 18f; // ...
                    tmp.fontSize = targetSize;
                    tmp.fontWeight = FontWeight.Bold; // Очень жирный
                    tmp.fontStyle = leftSpeakerNameText.fontStyle;
                    tmp.characterSpacing = leftSpeakerNameText.characterSpacing; // Синхронизируем широкий интервал букв
                    tmp.lineSpacing = leftSpeakerNameText.lineSpacing;
                    tmp.wordSpacing = leftSpeakerNameText.wordSpacing;
                    tmp.color = leftSpeakerNameText.color;
                }
                else
                {
                    tmp.fontSize = 18f; // Наш сочный крупный шрифт 18px!
                    tmp.fontWeight = FontWeight.Bold; // Сверхжирный
                    tmp.color = new Color(0.95f, 0.61f, 0.07f, 1f); // Насыщенный золотисто-оранжевый в стиле Аэлиссы

                    if (Translator.Instance != null && Translator.Instance.defaultFont != null)
                    {
                        tmp.font = Translator.Instance.defaultFont;
                    }
                }

                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
            }

            return tmp;
        }

        public string GetDialogueText(DialogLine line, int lang)
        {
            if (line == null) return "";
            switch (lang)
            {
                case 0: return !string.IsNullOrEmpty(line.textRU) ? line.textRU : line.textEN;
                case 1: return !string.IsNullOrEmpty(line.textEN) ? line.textEN : line.textRU;
                case 2: return !string.IsNullOrEmpty(line.textDE) ? line.textDE : line.textEN;
                case 3: return !string.IsNullOrEmpty(line.textFR) ? line.textFR : line.textEN;
                case 4: return !string.IsNullOrEmpty(line.textES) ? line.textES : line.textEN;
                case 5: return !string.IsNullOrEmpty(line.textPT) ? line.textPT : line.textEN;
                case 6: return !string.IsNullOrEmpty(line.textJA) ? line.textJA : line.textEN;
                case 7: return !string.IsNullOrEmpty(line.textKR) ? line.textKR : line.textEN;
                case 8: return !string.IsNullOrEmpty(line.textCH) ? line.textCH : line.textEN;
                default: return !string.IsNullOrEmpty(line.textEN) ? line.textEN : line.textRU;
            }
        }

        public string[] GetDialogueChoices(DialogLine line, int lang)
        {
            if (line == null) return new string[0];
            string[] selected = null;
            switch (lang)
            {
                case 0: selected = line.choicesRU; break;
                case 1: selected = line.choicesEN; break;
                case 2: selected = line.choicesDE; break;
                case 3: selected = line.choicesFR; break;
                case 4: selected = line.choicesES; break;
                case 5: selected = line.choicesPT; break;
                case 6: selected = line.choicesJA; break;
                case 7: selected = line.choicesKR; break;
                case 8: selected = line.choicesCH; break;
            }
            if (selected != null && selected.Length > 0) return selected;
            // Fallbacks:
            if (line.choicesEN != null && line.choicesEN.Length > 0) return line.choicesEN;
            if (line.choicesRU != null && line.choicesRU.Length > 0) return line.choicesRU;
            return new string[0];
        }

        private void UpdateDialogueView()
        {
            if (currentLineIndex < 0 || currentLineIndex >= dialogueSteps.Count)
            {
                EndDialogue();
                return;
            }

            DialogLine currentLine = dialogueSteps[currentLineIndex];

            // Определяем язык перевода
            int lang = Translator.LanguageID; // 0=RU, 1=EN, 7=KR, 8=CH (или по умолчанию EN)

            // Конфигурируем базовые имена
            string companionName = (lang == 0) ? companionNameRU : companionNameEN;
            string rawClass = (SaveGameSystem.CurrentData != null) ? SaveGameSystem.CurrentData.characterClass : "";
            string localizedClass = GetLocalizedClassName(rawClass, lang);

            // Имя выбранного героя приоритетно берем из SaveGameSystem.saveName,
            // так как там лежит уже полная красивая локализованная строка вида "Воин (Премиум)" или "Стрелок (Премиум)"!
            string playerDisplayName = (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.saveName) && SaveGameSystem.CurrentData.saveName != "Игрок")
                ? SaveGameSystem.CurrentData.saveName
                : localizedClass;

            // Очищаем имя от суффикса премиум-класса для лаконичности и делаем имена чистыми (Воин, Лучник, Маг)
            if (!string.IsNullOrEmpty(playerDisplayName))
            {
                playerDisplayName = playerDisplayName
                    .Replace(" (Премиум)", "")
                    .Replace("(Премиум)", "")
                    .Replace(" (Premium)", "")
                    .Replace("(Premium)", "")
                    .Replace(" (프리미엄)", "")
                    .Replace("(프리미엄)", "")
                    .Replace(" (豪华)", "")
                    .Replace("(豪华)", "")
                    .Replace(" (プレミアム)", "")
                    .Replace("(プレミアム)", "")
                    .Replace("Снайпер", "Лучник")
                    .Replace("Стрелок", "Лучник")
                    .Trim();
            }

            // [ZENITH PROTECTIVE CHECK] Enforce companion and player portraits are distinct and separated
            if (companionPortraitImage != null && companionPortraitImage == playerPortraitImage)
            {
                if (dialogPanel != null)
                {
                    Image[] allImgs = dialogPanel.GetComponentsInChildren<Image>(true);
                    Image leftImg = null;
                    Image rightImg = null;
                    float minX = 9999f;
                    float maxX = -9999f;
                    foreach (var img in allImgs)
                    {
                        RectTransform rt = img.GetComponent<RectTransform>();
                        if (rt != null && rt.rect.width > 20f && rt.rect.height > 20f)
                        {
                            float x = rt.anchoredPosition.x;
                            if (x < minX) { minX = x; leftImg = img; }
                            if (x > maxX) { maxX = x; rightImg = img; }
                        }
                    }
                    if (leftImg != null) companionPortraitImage = leftImg;
                    if (rightImg != null && rightImg != leftImg) playerPortraitImage = rightImg;
                }
            }

            // Навеки зачищаем некорректно созданные динамические имена по взаимному пересечению под портретами
            if (companionPortraitImage != null)
            {
                Transform wrongPlayerName = companionPortraitImage.transform.Find("Txt_PlayerName_Dynamic");
                if (wrongPlayerName != null) DestroyImmediate(wrongPlayerName.gameObject);
            }
            if (playerPortraitImage != null)
            {
                Transform wrongCompName = playerPortraitImage.transform.Find("Txt_CompanionName_Dynamic");
                if (wrongCompName != null) DestroyImmediate(wrongCompName.gameObject);
            }

            // Наличие и чистота надписей имен в стиле Zenith
            if (leftSpeakerNameText != null && leftSpeakerNameText.gameObject.name != "Txt_CompanionName_Dynamic")
            {
                // Нативный текстовой элемент уже есть в префабе! Нам НЕ нужна динамическая копия.
                // Навсегда удаляем Txt_CompanionName_Dynamic из иерархии под портретом Аэлиссы, чтобы убрать наложения текстов.
                if (companionPortraitImage != null)
                {
                    Transform t = companionPortraitImage.transform.Find("Txt_CompanionName_Dynamic");
                    if (t != null) DestroyImmediate(t.gameObject);
                }
            }
            else
            {
                // Если нативной подписи нет, используем автоматическую динамическую подпись под портретом
                if (companionPortraitImage != null)
                {
                    leftSpeakerNameText = CreatePortraitNameLabel(companionPortraitImage, companionName, true);
                }
            }

            if (rightSpeakerNameText != null && rightSpeakerNameText.gameObject.name != "Txt_PlayerName_Dynamic")
            {
                // Нативный текстовой элемент уже есть в префабе! Нам НЕ нужна динамическая копия.
                // Навсегда удаляем Txt_PlayerName_Dynamic из иерархии под портретом героя, чтобы убрать лишний текст.
                if (playerPortraitImage != null)
                {
                    Transform t = playerPortraitImage.transform.Find("Txt_PlayerName_Dynamic");
                    if (t != null) DestroyImmediate(t.gameObject);
                }
            }
            else
            {
                // Если нативной подписи нет, автоматически создаем крупный жирный золотой текст под портретом выбранного героя
                if (playerPortraitImage != null)
                {
                    rightSpeakerNameText = CreatePortraitNameLabel(playerPortraitImage, playerDisplayName, false);
                }
            }

            // Текстовые подписи под левым и правым портеретами
            if (leftSpeakerNameText != null)
            {
                ClearUnwantedBackgrounds(leftSpeakerNameText.gameObject);
                leftSpeakerNameText.text = companionName;
            }
            if (rightSpeakerNameText != null)
            {
                ClearUnwantedBackgrounds(rightSpeakerNameText.gameObject);
                rightSpeakerNameText.text = playerDisplayName;
            }

            // [ZENITH SAFETY OVERWRITE] Навеки убираем наложения на имена левого компаньона и правого героя!
            if (dialogPanel != null)
            {
                TextMeshProUGUI[] panelTmps = dialogPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var pTmp in panelTmps)
                {
                    if (pTmp == null) continue;
                    
                    // Блокируем ЛЮБЫЕ наложения внутри портрета игрока
                    if (playerPortraitImage != null && pTmp.transform.IsChildOf(playerPortraitImage.transform))
                    {
                        if (pTmp != rightSpeakerNameText)
                        {
                            pTmp.gameObject.SetActive(false);
                            continue;
                        }
                    }

                    // Блокируем ЛЮБЫЕ наложения внутри портрета компаньона
                    if (companionPortraitImage != null && pTmp.transform.IsChildOf(companionPortraitImage.transform))
                    {
                        if (pTmp != leftSpeakerNameText)
                        {
                            pTmp.gameObject.SetActive(false);
                            continue;
                        }
                    }

                    RectTransform rt = pTmp.GetComponent<RectTransform>();
                    if (rt != null && pTmp != dialogueBodyText && pTmp != speakerNameText)
                    {
                        // Если этот компонент находится на правой стороне по мировым или локальным смещениям,
                        // и это не наше официальное rightSpeakerNameText, деактивируем его!
                        float localX = rt.anchoredPosition.x;
                        if (localX > 100f || pTmp.gameObject.name.Contains("Player") || pTmp.gameObject.name.Contains("Voin") || pTmp.gameObject.name.Contains("Warrior"))
                        {
                            if (pTmp != rightSpeakerNameText)
                            {
                                pTmp.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            // Вычисляем, кто говорит на текущем шаге
            string rawSpeaker = currentLine.characterName;
            bool speaking = false;

            if (!string.IsNullOrEmpty(rawSpeaker))
            {
                string rawLower = rawSpeaker.ToLower();
                if (rawLower.Contains("игрок") || rawLower.Contains("player") || rawLower.Contains("воин") || rawLower.Contains("warrior") || rawLower.Contains("лучник") || rawLower.Contains("archer") || rawLower.Contains("маг") || rawLower.Contains("mage") || rawLower.Contains("стрелок") || rawLower.Contains("снайпер") || rawLower.Contains("hero"))
                {
                    speaking = true;
                }
            }

            // Настройка главного текста имени активного говорящего
            string activeSpeakerName = companionName;
            if (speaking)
            {
                activeSpeakerName = playerDisplayName;
            }
            else if (!string.IsNullOrEmpty(rawSpeaker))
            {
                string rawSpeakerLower = rawSpeaker.ToLower();
                if (rawSpeakerLower.Contains("aelis") || rawSpeakerLower.Contains("аэлис"))
                {
                    activeSpeakerName = companionName;
                }
                else
                {
                    activeSpeakerName = rawSpeaker;
                }
            }

            if (speakerNameText != null && speakerNameText != leftSpeakerNameText && speakerNameText != rightSpeakerNameText)
            {
                speakerNameText.text = activeSpeakerName;
            }

            // Настройка текста реплики со специфичными переводами
            string mainText = GetDialogueText(currentLine, lang);
            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = mainText;
            }

            // Накатываем шрифты в зависимости от языка
            ApplyDialogueFonts();

            // Динамический выбор портрета игрока на правой стороне
            UpdatePlayerPortrait();

            // Привязка и обновление портрета компаньона (Аэлиссы) на левой стороне с кэш-защитой
            if (companionPortraitImage != null)
            {
                if (companionPortrait != null)
                {
                    companionPortraitImage.sprite = companionPortrait;
                }
                else if (originalCompanionSprite != null)
                {
                    companionPortraitImage.sprite = originalCompanionSprite;
                }
            }

            // Применяем визуальный акцент
            if (speaking)
            {
                if (playerPortraitImage != null) playerPortraitImage.color = Color.white;
                if (companionPortraitImage != null) companionPortraitImage.color = new Color(1f, 1f, 1f, 0.85f);

                if (rightSpeakerNameText != null) rightSpeakerNameText.color = new Color(0.95f, 0.61f, 0.07f, 1f);
                if (leftSpeakerNameText != null) leftSpeakerNameText.color = new Color(0.7f, 0.8f, 0.9f, 0.6f);
            }
            else
            {
                if (companionPortraitImage != null) companionPortraitImage.color = Color.white;
                if (playerPortraitImage != null) playerPortraitImage.color = new Color(1f, 1f, 1f, 0.85f);

                if (leftSpeakerNameText != null) leftSpeakerNameText.color = new Color(0.95f, 0.61f, 0.07f, 1f);
                if (rightSpeakerNameText != null) rightSpeakerNameText.color = new Color(0.7f, 0.8f, 0.9f, 0.6f);
            }

            if (currentLineIndex == 3)
            {
                if (FateMapManager.Instance != null)
                {
                    FateMapManager.Instance.SetMapVisible(true);
                    FateMapManager.Instance.HighlightRing(selectedZoneIndex);
                }
            }
            else
            {
                if (FateMapManager.Instance != null)
                {
                    FateMapManager.Instance.SetMapVisible(false);
                }
            }

            // Обработка вариантов ответов (кнопок выбора)
            string[] currentChoices = GetDialogueChoices(currentLine, lang);

            if (choiceContainer != null && choiceContainer != dialogPanel)
            {
                List<Transform> children = new List<Transform>();
                foreach (Transform child in choiceContainer.transform)
                {
                    children.Add(child);
                }
                foreach (Transform child in children)
                {
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }

            if (dialogPanel != null)
            {
                Transform existingConfirm = FindChildRecursive(dialogPanel.transform, "Btn_ConfirmDialogueChoice");
                if (existingConfirm != null)
                {
                    if (Application.isPlaying) Destroy(existingConfirm.gameObject);
                    else DestroyImmediate(existingConfirm.gameObject);
                }
            }

            choiceButtons.Clear();

            if (nextDialogueButton == null)
            {
                SetupNextDialogueButton();
            }

            if (currentChoices != null && currentChoices.Length > 0)
            {
                if (choiceContainer != null)
                {
                    choiceContainer.SetActive(true);
                    HorizontalLayoutGroup hLayout = choiceContainer.GetComponent<HorizontalLayoutGroup>();
                    if (hLayout != null)
                    {
                        hLayout.spacing = (currentLineIndex == 3) ? 10f : 20f;
                    }
                }
                
                if (nextDialogueButton != null)
                {
                    nextDialogueButton.gameObject.SetActive(false);
                }

                if (currentLineIndex == 3)
                {
                    for (int i = 0; i < currentChoices.Length; i++)
                    {
                        int index = i;
                        Button btn = CreateChoiceButton(currentChoices[i], () => SelectChoice(index));
                        choiceButtons.Add(btn);
                    }

                    string confirmText = (lang == 0) ? "Завершить диалог" : (lang == 8) ? "结束对话" : (lang == 7) ? "대화 종료" : "End Dialogue";
                    Button confirmBtn = CreateConfirmButton(confirmText, () => SelectChoice(selectedZoneIndex, true));
                    choiceButtons.Add(confirmBtn);
                }
                else
                {
                    for (int i = 0; i < currentChoices.Length; i++)
                    {
                        int index = i;
                        Button btn = CreateChoiceButton(currentChoices[i], () => SelectChoice(index));
                        choiceButtons.Add(btn);
                    }
                }

                if (currentLineIndex == 3)
                {
                    UpdateChoiceSelectionVisuals(selectedZoneIndex);
                    UpdateInteractiveZoneText(selectedZoneIndex);
                }
            }
            else
            {
                if (choiceContainer != null)
                {
                    choiceContainer.SetActive(false);
                }
                if (nextDialogueButton != null)
                {
                    nextDialogueButton.gameObject.SetActive(true);
                    TextMeshProUGUI txtTmp = nextDialogueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (txtTmp != null)
                    {
                        txtTmp.text = (lang == 0) ? "Далее" : "Next";
                    }
                }
            }
        }

        private void SetupNextDialogueButton()
        {
            if (dialogPanel == null) return;

            // Try to find an existing button named "Btn_NextDialogue" or "NextButton" in panel
            Transform btnTrans = FindChildRecursive(dialogPanel.transform, "Btn_NextDialogue");
            if (btnTrans != null)
            {
                nextDialogueButton = btnTrans.GetComponent<Button>();
            }

            if (nextDialogueButton == null)
            {
                // Create a next button dynamically
                GameObject btnGO = new GameObject("Btn_NextDialogue");
                btnGO.transform.SetParent(dialogPanel.transform, false);

                RectTransform rect = btnGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-120f, 30f);
                rect.sizeDelta = new Vector2(140f, 40f);

                Image bg = btnGO.AddComponent<Image>();
                bg.color = new Color(0.12f, 0.16f, 0.28f, 0.85f);

                Outline outline = btnGO.AddComponent<Outline>();
                outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.6f);
                outline.effectDistance = new Vector2(1.5f, 1.5f);

                nextDialogueButton = btnGO.AddComponent<Button>();

                GameObject txtGO = new GameObject("Txt_Next");
                txtGO.transform.SetParent(btnGO.transform, false);
                RectTransform txtRect = txtGO.AddComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = Vector2.zero;
                txtRect.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "Next";
                tmp.fontSize = 14f;
                tmp.fontWeight = FontWeight.Bold;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.font = dialogueBodyText != null ? dialogueBodyText.font : null;
            }

            if (nextDialogueButton != null)
            {
                nextDialogueButton.onClick.RemoveAllListeners();
                nextDialogueButton.onClick.AddListener(AdvanceDialogue);
                
                // Add click SFX binding
                if (SettingsManager.Instance != null)
                {
                    nextDialogueButton.onClick.AddListener(() => {
                        SettingsManager.Instance.PlaySFX("UI_Click_Metallic");
                    });
                }
            }
        }

        private Button CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGO = new GameObject("Btn_DialogueChoice");
            if (choiceContainer != null)
            {
                btnGO.transform.SetParent(choiceContainer.transform, false);
            }
            
            // Add UI components
            RectTransform rect = btnGO.AddComponent<RectTransform>();
            Image bg = btnGO.AddComponent<Image>();
            
            // Изумрудная адаптивность: Пытаемся скопировать красивый спрайт-рамку из префаба!
            Sprite templateSprite = null;
            Material templateMat = null;
            Image.Type templateType = Image.Type.Simple;
            if (nextDialogueButton != null)
            {
                Image nextImg = nextDialogueButton.GetComponent<Image>();
                if (nextImg != null)
                {
                    templateSprite = nextImg.sprite;
                    templateMat = nextImg.material;
                    templateType = nextImg.type;
                }
            }

            if (templateSprite != null)
            {
                bg.sprite = templateSprite;
                bg.type = templateType;
                if (templateMat != null) bg.material = templateMat;
                bg.color = new Color(0.08f, 0.12f, 0.22f, 0.95f); // Сохраняем Zenith-синий полупрозрачный оттенок с рамкой
            }
            else
            {
                bg.color = new Color(0.08f, 0.12f, 0.22f, 0.9f);
            }
            
            Outline outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.7f); // Свечение
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Буферный леяут для гибкости HorizonLayoutGroup
            LayoutElement layout = btnGO.AddComponent<LayoutElement>();
            layout.minWidth = choiceButtonMinWidth;
            layout.preferredWidth = choiceButtonPreferredWidth;
            layout.minHeight = choiceButtonMinHeight;

            Button btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(action);

            // Add text child
            GameObject txtGO = new GameObject("Txt_Choice");
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRect = txtGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(15f, 6f);
            txtRect.offsetMax = new Vector2(-15f, -6f);

            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = choiceText;
            tmp.fontSize = choiceButtonFontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = dialogueBodyText != null ? dialogueBodyText.font : null;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 9f;
            tmp.fontSizeMax = choiceButtonFontSize;

            // Bind UI SFX using SettingsManager if exists
            if (SettingsManager.Instance != null && btn != null)
            {
                btn.onClick.AddListener(() => {
                    SettingsManager.Instance.PlaySFX("UI_Click_Metallic");
                });
            }

            return btn;
        }

        private Button CreateConfirmButton(string choiceText, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGO = new GameObject("Btn_ConfirmDialogueChoice");
            if (choiceContainer != null)
            {
                btnGO.transform.SetParent(choiceContainer.transform, false);
            }
            
            // Add UI components
            RectTransform rect = btnGO.AddComponent<RectTransform>();
            Image bg = btnGO.AddComponent<Image>();
            
            Sprite templateSprite = null;
            Material templateMat = null;
            Image.Type templateType = Image.Type.Simple;
            if (nextDialogueButton != null)
            {
                Image nextImg = nextDialogueButton.GetComponent<Image>();
                if (nextImg != null)
                {
                    templateSprite = nextImg.sprite;
                    templateMat = nextImg.material;
                    templateType = nextImg.type;
                }
            }

            if (templateSprite != null)
            {
                bg.sprite = templateSprite;
                bg.type = templateType;
                if (templateMat != null) bg.material = templateMat;
                bg.color = new Color(0.12f, 0.35f, 0.18f, 0.95f); // Изумрудный оттенок на основе спрайта кнопки
            }
            else
            {
                bg.color = new Color(0.1f, 0.25f, 0.15f, 0.9f);
            }
            
            Outline outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.9f, 0.35f, 0.85f);
            outline.effectDistance = new Vector2(2f, 2f);

            LayoutElement layout = btnGO.AddComponent<LayoutElement>();
            layout.minWidth = choiceButtonMinWidth + 20f; // Подтверждение делим чуть крупнее
            layout.preferredWidth = choiceButtonPreferredWidth + 30f;
            layout.minHeight = choiceButtonMinHeight;

            Button btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(action);

            // Add text child
            GameObject txtGO = new GameObject("Txt_ChoiceConfirm");
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRect = txtGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(15f, 6f);
            txtRect.offsetMax = new Vector2(-15f, -6f);

            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = choiceText;
            tmp.fontSize = choiceButtonFontSize;
            tmp.fontWeight = FontWeight.Bold;
            tmp.color = new Color(0.85f, 1f, 0.9f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = dialogueBodyText != null ? dialogueBodyText.font : null;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 9f;
            tmp.fontSizeMax = choiceButtonFontSize;

            if (SettingsManager.Instance != null)
            {
                btn.onClick.AddListener(() => {
                    SettingsManager.Instance.PlaySFX("UI_Click_Metallic");
                });
            }

            return btn;
        }

        private void UpdatePlayerPortrait()
        {
            if (playerPortraitImage == null) return;

            string savedClass = (SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null) 
                ? SaveGameSystem.CurrentData.characterClass.ToLower() 
                : "";
            
            Sprite chosenSprite = null;
            if (savedClass.Contains("warrior") || savedClass.Contains("воин") || savedClass.Contains("voin"))
            {
                chosenSprite = warriorPortrait;
            }
            else if (savedClass.Contains("archer") || savedClass.Contains("лучник") || savedClass.Contains("стрелок") || savedClass.Contains("strelok") || savedClass.Contains("снайпер"))
            {
                chosenSprite = archerPortrait;
            }
            else if (savedClass.Contains("mage") || savedClass.Contains("маг") || savedClass.Contains("mag"))
            {
                chosenSprite = magePortrait;
            }

            if (chosenSprite != null)
            {
                playerPortraitImage.sprite = chosenSprite;
                playerPortraitImage.color = Color.white;
            }
            else
            {
                if (playerPortraitImage.sprite != null)
                {
                    playerPortraitImage.color = Color.white;
                }
                else
                {
                    playerPortraitImage.color = new Color(1f, 1f, 1f, 0.25f);
                }
            }
        }

        private void ApplyDialogueFonts()
        {
            if (Translator.Instance == null) return;

            TMP_FontAsset activeFont = Translator.Instance.defaultFont;
            int lang = Translator.LanguageID;

            if (lang == 8 && Translator.Instance.chineseFont != null) activeFont = Translator.Instance.chineseFont;
            else if (lang == 7 && Translator.Instance.koreanFont != null) activeFont = Translator.Instance.koreanFont;

            if (leftSpeakerNameText != null) leftSpeakerNameText.font = activeFont;
            if (rightSpeakerNameText != null) rightSpeakerNameText.font = activeFont;
            if (speakerNameText != null) speakerNameText.font = activeFont;
            if (dialogueBodyText != null) dialogueBodyText.font = activeFont;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return parent;
            }
            foreach (Transform child in parent)
            {
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void InitializeDialogueUI()
        {
            dialogCanvas = null;
            dialogPanel = null;
            speakerNameText = null;
            dialogueBodyText = null;
            choiceContainer = null;
            companionPortraitImage = null;
            playerPortraitImage = null;
            nextDialogueButton = null;

            GameObject rootCanvas = GameObject.Find("FATE_DIALOGUE_CANVAS");
            if (rootCanvas == null)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    GameObject[] rootGOs = activeScene.GetRootGameObjects();
                    foreach (var go in rootGOs)
                    {
                        if (go.name == "FATE_DIALOGUE_CANVAS" || go.name.ToUpper().Contains("DIALOGUE") || go.GetComponent<Canvas>() != null)
                        {
                            if (FindChildRecursive(go.transform, "DialoguePanel") != null || go.name == "FATE_DIALOGUE_CANVAS")
                            {
                                rootCanvas = go;
                                break;
                            }
                        }
                    }
                }
            }

            if (rootCanvas == null)
            {
                Canvas[] existingCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var c in existingCanvases)
                {
                    if (c.name.ToUpper().Contains("DIALOGUE") || c.name.ToUpper().Contains("CANVAS"))
                    {
                        if (FindChildRecursive(c.transform, "DialoguePanel") != null)
                        {
                            rootCanvas = c.gameObject;
                            break;
                        }
                    }
                }
            }

            if (rootCanvas != null)
            {
                rootCanvas.SetActive(true); // Гарантируем, что холст активен и не отключен в инспекторе!
                dialogCanvas = rootCanvas.GetComponent<Canvas>();

                Transform pauseP = FindChildRecursive(rootCanvas.transform, "PausePanel");
                if (pauseP != null) pauseP.gameObject.SetActive(false);

                Transform confirmP = FindChildRecursive(rootCanvas.transform, "ConfirmPanel");
                if (confirmP != null) confirmP.gameObject.SetActive(false);

                Transform confirmP2 = FindChildRecursive(rootCanvas.transform, "ConfirmPanel (1)");
                if (confirmP2 != null) confirmP2.gameObject.SetActive(false);

                Transform panelT = FindChildRecursive(rootCanvas.transform, "DialoguePanel");
                if (panelT != null)
                {
                    dialogPanel = panelT.gameObject;
                    
                    Image[] allImages = panelT.GetComponentsInChildren<Image>(true);
                    foreach (var img in allImages)
                    {
                        string imgName = img.gameObject.name.ToLower();
                        if (imgName.Contains("companion") || imgName.Contains("aelis") || imgName.Contains("left") || imgName.Contains("helper") || imgName.Contains("pomoshnica") || imgName.Contains("помощница"))
                        {
                            companionPortraitImage = img;
                        }
                        else if (imgName.Contains("player") || imgName.Contains("hero") || imgName.Contains("right") || imgName.Contains("class") || imgName.Contains("avatar") || imgName.Contains("voin") || imgName.Contains("воин") || imgName.Contains("strelok") || imgName.Contains("стрелок") || imgName.Contains("mag") || imgName.Contains("маг") || imgName.Contains("archer") || imgName.Contains("лучник") || imgName.Contains("снайпер"))
                        {
                            playerPortraitImage = img;
                        }
                    }

                    if (companionPortraitImage == null)
                    {
                        Transform compT = FindChildRecursive(panelT, "Img_CompanionPortrait");
                        if (compT != null) companionPortraitImage = compT.GetComponent<Image>();
                    }
                    if (companionPortraitImage == null)
                    {
                        Transform compT = FindChildRecursive(panelT, "Pomoshnica");
                        if (compT != null) companionPortraitImage = compT.GetComponent<Image>();
                    }
                    if (companionPortraitImage == null)
                    {
                        // Позиционный фолбэк для левого портрета
                        foreach (var img in allImages)
                        {
                            RectTransform rt = img.GetComponent<RectTransform>();
                            if (rt != null && rt.anchoredPosition.x < -100f)
                            {
                                companionPortraitImage = img;
                                break;
                            }
                        }
                    }

                    if (playerPortraitImage == null)
                    {
                        Transform playerT = FindChildRecursive(panelT, "Img_PlayerPortrait");
                        if (playerT != null) playerPortraitImage = playerT.GetComponent<Image>();
                    }
                    if (playerPortraitImage == null)
                    {
                        string[] classNames = { "Voin", "Strelok", "Mag", "Воин", "Стрелок", "Маг" };
                        foreach (var cName in classNames)
                        {
                            Transform t = FindChildRecursive(panelT, cName);
                            if (t != null)
                            {
                                playerPortraitImage = t.GetComponent<Image>();
                                break;
                            }
                        }
                    }
                    if (playerPortraitImage == null)
                    {
                        // Позиционный фолбэк для правого портрета
                        foreach (var img in allImages)
                        {
                            RectTransform rt = img.GetComponent<RectTransform>();
                            if (rt != null && rt.anchoredPosition.x > 100f)
                            {
                                playerPortraitImage = img;
                                break;
                            }
                        }
                    }

                    if (companionPortraitImage != null && companionPortraitImage.sprite != null)
                    {
                        originalCompanionSprite = companionPortraitImage.sprite;
                    }
                    if (playerPortraitImage != null && playerPortraitImage.sprite != null)
                    {
                        originalPlayerSprite = playerPortraitImage.sprite;
                    }

                    Transform containerT = null;
                    foreach (Transform child in panelT)
                    {
                        string childName = child.name.ToLower();
                        if (childName.Contains("choice") || childName.Contains("button") || childName.Contains("container"))
                        {
                            containerT = child;
                            break;
                        }
                    }
                    if (containerT == null)
                    {
                        containerT = FindChildRecursive(panelT, "ChoiceContainer");
                        if (containerT == null) containerT = FindChildRecursive(panelT, "Choice_Container");
                        if (containerT == null) containerT = FindChildRecursive(panelT, "Choices");
                    }
                    if (containerT != null) choiceContainer = containerT.gameObject;
                    
                    leftSpeakerNameText = null;
                    rightSpeakerNameText = null;
                    dialogueBodyText = null;

                    TextMeshProUGUI[] tmps = panelT.GetComponentsInChildren<TextMeshProUGUI>(true);
                    List<TextMeshProUGUI> nameTexts = new List<TextMeshProUGUI>();

                    foreach (var tmp in tmps)
                    {
                        Transform parentIter = tmp.transform;
                        bool isExcluded = false;
                        while (parentIter != null && parentIter != panelT)
                        {
                            string pName = parentIter.name.ToLower();
                            if (pName.Contains("choice") || pName.Contains("btn") || pName.Contains("button"))
                            {
                                isExcluded = true;
                                break;
                            }
                            parentIter = parentIter.parent;
                        }
                        if (isExcluded) continue;

                        string goName = tmp.gameObject.name.ToLower();

                        if (goName == "txt_dialoguebody" || goName == "dialoguebody" || goName == "dialogue_body" || goName == "txt_dialogue" || goName == "dialoguebodytext" || goName == "text_dialoguebody" || goName.Contains("body") || goName.Contains("content"))
                        {
                            if (!goName.Contains("speaker") && !goName.Contains("name"))
                            {
                                dialogueBodyText = tmp;
                                continue;
                            }
                        }

                        if (goName.Contains("speaker") || goName.Contains("name") || goName.Contains("cap") || goName.Contains("header") || goName.Contains("title") || goName.Contains("left") || goName.Contains("right"))
                        {
                            nameTexts.Add(tmp);
                        }
                    }

                    if (dialogueBodyText == null)
                    {
                        foreach (var tmp in tmps)
                        {
                            Transform parentIter = tmp.transform;
                            bool isExcluded = false;
                            while (parentIter != null && parentIter != panelT)
                            {
                                if (parentIter.name.ToLower().Contains("choice") || parentIter.name.ToLower().Contains("btn"))
                                {
                                    isExcluded = true;
                                    break;
                                }
                                parentIter = parentIter.parent;
                            }
                            if (isExcluded) continue;

                            if (!nameTexts.Contains(tmp))
                            {
                                dialogueBodyText = tmp;
                                break;
                            }
                        }
                    }

                    // Изумрудный поиск спикеров: Сначала вычленяем центральный Txt_SpeakerName
                    speakerNameText = null;
                    foreach (var tmp in tmps)
                    {
                        string nameL = tmp.gameObject.name.ToLower();
                        if (nameL.Contains("speaker") || nameL.Contains("active") || nameL.Contains("dialoguecap") || nameL == "txt_speakername")
                        {
                            // Это центральный заголовок, он не должен быть привязан к портретам
                            if ((companionPortraitImage == null || !tmp.transform.IsChildOf(companionPortraitImage.transform)) &&
                                (playerPortraitImage == null || !tmp.transform.IsChildOf(playerPortraitImage.transform)))
                            {
                                speakerNameText = tmp;
                                break;
                            }
                        }
                    }

                    // Если в префабе вообще не нашли центральный заголовок по имени, берем любой верхний текст по центру
                    if (speakerNameText == null)
                    {
                        foreach (var tmp in tmps)
                        {
                            RectTransform rt = tmp.GetComponent<RectTransform>();
                            if (rt != null && Mathf.Abs(rt.anchoredPosition.x) < 80f && rt.anchoredPosition.y > 60f)
                            {
                                speakerNameText = tmp;
                                break;
                            }
                        }
                    }

                    // Теперь находим или готовим левую/правую плашки строго под портретами
                    leftSpeakerNameText = null;
                    if (companionPortraitImage != null)
                    {
                        foreach (var tmp in tmps)
                        {
                            if (tmp.transform.IsChildOf(companionPortraitImage.transform) || tmp.gameObject.name == "Txt_CompanionName_Dynamic")
                            {
                                leftSpeakerNameText = tmp;
                                break;
                            }
                        }
                    }

                    rightSpeakerNameText = null;
                    if (playerPortraitImage != null)
                    {
                        foreach (var tmp in tmps)
                        {
                            if (tmp != speakerNameText && (tmp.transform.IsChildOf(playerPortraitImage.transform) || tmp.gameObject.name == "Txt_PlayerName_Dynamic"))
                            {
                                rightSpeakerNameText = tmp;
                                break;
                            }
                        }
                    }

                    // Если центральный заголовок всё ещё пуст, используем левый заголовок как резерв для смены реплик
                    if (speakerNameText == null)
                    {
                        speakerNameText = leftSpeakerNameText != null ? leftSpeakerNameText : rightSpeakerNameText;
                    }

                    if (dialogueBodyText != null)
                    {
                        dialogueBodyText.textWrappingMode = TextWrappingModes.Normal;
                        dialogueBodyText.enableAutoSizing = true;
                        dialogueBodyText.fontSizeMin = 11f;
                        dialogueBodyText.fontSizeMax = 16f;
                    }
                    if (leftSpeakerNameText != null)
                    {
                        leftSpeakerNameText.textWrappingMode = TextWrappingModes.NoWrap;
                    }
                    if (rightSpeakerNameText != null)
                    {
                        rightSpeakerNameText.textWrappingMode = TextWrappingModes.NoWrap;
                    }

                    ApplyRectTransformTweaks();

                    if (choiceContainer != null)
                    {
                        HorizontalLayoutGroup existingHLayout = choiceContainer.GetComponent<HorizontalLayoutGroup>();
                        if (existingHLayout == null)
                        {
                            existingHLayout = choiceContainer.AddComponent<HorizontalLayoutGroup>();
                        }
                        existingHLayout.spacing = 20f;
                        existingHLayout.childAlignment = TextAnchor.MiddleCenter;
                        existingHLayout.childControlHeight = true;
                        existingHLayout.childControlWidth = true;
                        existingHLayout.childForceExpandHeight = false;
                        existingHLayout.childForceExpandWidth = true;
                    }

                    if (speakerNameText != null && dialogueBodyText != null)
                    {
                        SetupNextDialogueButton();
                        return;
                    }
                }
            }

            GameObject canvasGov = new GameObject("FATE_DIALOGUE_CANVAS");
            dialogCanvas = canvasGov.AddComponent<Canvas>();
            dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogCanvas.sortingOrder = 999;
            
            canvasGov.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGov.AddComponent<GraphicRaycaster>();

            dialogPanel = new GameObject("DialoguePanel");
            dialogPanel.transform.SetParent(canvasGov.transform, false);
            RectTransform panelRect = dialogPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 120f);
            panelRect.sizeDelta = new Vector2(950f, 220f);

            Image bgImg = dialogPanel.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.09f, 0.85f);
            
            Outline outline = dialogPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.6f);
            outline.effectDistance = new Vector2(2f, 2f);

            GameObject headerGov = new GameObject("DialogueCap_Header");
            headerGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform headerRect = headerGov.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, 10f);
            headerRect.sizeDelta = new Vector2(-40f, 35f);

            Image headerImg = headerGov.AddComponent<Image>();
            headerImg.color = new Color(0.95f, 0.61f, 0.07f, 0.75f);

            GameObject speakerGov = new GameObject("Txt_SpeakerName");
            speakerGov.transform.SetParent(headerGov.transform, false);
            RectTransform speakerRect = speakerGov.AddComponent<RectTransform>();
            speakerRect.anchorMin = Vector2.zero;
            speakerRect.anchorMax = Vector2.one;
            speakerRect.offsetMin = new Vector2(15f, 0f);
            speakerRect.offsetMax = Vector2.zero;

            speakerNameText = speakerGov.AddComponent<TextMeshProUGUI>();
            speakerNameText.text = "Aelyssa";
            speakerNameText.fontSize = 18f;
            speakerNameText.fontWeight = FontWeight.Black;
            speakerNameText.color = Color.white;
            speakerNameText.alignment = TextAlignmentOptions.Left;
            speakerNameText.textWrappingMode = TextWrappingModes.NoWrap;

            GameObject textGov = new GameObject("Txt_DialogueBody");
            textGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform textRect = textGov.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(180f, 40f);
            textRect.offsetMax = new Vector2(-180f, -65f);

            dialogueBodyText = textGov.AddComponent<TextMeshProUGUI>();
            dialogueBodyText.text = "Loading conversation...";
            dialogueBodyText.fontSize = 15f;
            dialogueBodyText.color = new Color(0.9f, 0.95f, 1f, 1f);
            dialogueBodyText.alignment = TextAlignmentOptions.TopLeft;
            dialogueBodyText.textWrappingMode = TextWrappingModes.Normal;

            GameObject compPortraitGov = new GameObject("Img_CompanionPortrait");
            compPortraitGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform compPortraitRect = compPortraitGov.AddComponent<RectTransform>();
            compPortraitRect.anchorMin = new Vector2(0f, 0.5f);
            compPortraitRect.anchorMax = new Vector2(0f, 0.5f);
            compPortraitRect.anchoredPosition = new Vector3(-60f, 10f);
            compPortraitRect.sizeDelta = new Vector2(160f, 160f);

            companionPortraitImage = compPortraitGov.AddComponent<Image>();
            companionPortraitImage.color = new Color(1f, 1f, 1f, 0.9f);
            
            Outline compOutline = compPortraitGov.AddComponent<Outline>();
            compOutline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.9f);
            compOutline.effectDistance = new Vector2(3f, 3f);

            GameObject playerPortraitGov = new GameObject("Img_PlayerPortrait");
            playerPortraitGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform playerPortraitRect = playerPortraitGov.AddComponent<RectTransform>();
            playerPortraitRect.anchorMin = new Vector2(1f, 0.5f);
            playerPortraitRect.anchorMax = new Vector2(1f, 0.5f);
            playerPortraitRect.anchoredPosition = new Vector3(60f, 10f);
            playerPortraitRect.sizeDelta = new Vector2(160f, 160f);

            playerPortraitImage = playerPortraitGov.AddComponent<Image>();
            playerPortraitImage.color = new Color(1f, 1f, 1f, 0.9f);

            Outline playerOutline = playerPortraitGov.AddComponent<Outline>();
            playerOutline.effectColor = new Color(0.95f, 0.61f, 0.07f, 0.9f);
            playerOutline.effectDistance = new Vector2(3f, 3f);

            choiceContainer = new GameObject("ChoiceContainer");
            choiceContainer.transform.SetParent(dialogPanel.transform, false);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0f, 0f);
            choiceRect.anchorMax = new Vector2(1f, 0f);
            choiceRect.anchoredPosition = new Vector2(0f, 30f);
            choiceRect.sizeDelta = new Vector2(-40f, 100f);

            HorizontalLayoutGroup choiceLayout = choiceContainer.AddComponent<HorizontalLayoutGroup>();
            choiceLayout.spacing = 15f;
            choiceLayout.childAlignment = TextAnchor.MiddleCenter;
            choiceLayout.childControlHeight = true;
            choiceLayout.childControlWidth = true;
            choiceLayout.childForceExpandHeight = false;
            choiceLayout.childForceExpandWidth = true;

            SetupNextDialogueButton();
            ApplyDialogueFonts();
            ApplyRectTransformTweaks();
        }

        private void SetupFallbackDialogues()
        {
            dialogueSteps.Clear();

            DialogLine l1 = new DialogLine
            {
                textRU = "Здравствуй, путник! Наш Континент Судьбы погружается во тьму древнего безвременья. Я буду сопровождать тебя в этом опасном походе.",
                textEN = "Greetings, traveler! Our Fate Continent is sinking into the darkness of ancient timelessness. I will accompany you in this dangerous journey.",
                textDE = "Seid gegrüßt, Reisender! Unser Schicksalskontinent versinkt in der Dunkelheit der alten Zeitlosigkeit. Ich werde euch auf dieser gefährlichen Reise begleiten.",
                textFR = "Salutations, voyageur ! Notre Continent du Destin sombre dans les ténèbres d'une intemporalité ancienne. Je vous accompagnerai dans ce voyage dangereux.",
                textES = "¡Saludos, viajero! Nuestro Continente del Destino se está hundiendo en la oscuridad de una atemporalidad antigua. Te acompañaré en este peligroso viaje.",
                textPT = "Saudações, viajante! Nosso Continente do Destino está afundando na escuridão de uma atemporalidade antiga. Eu irei acompanhá-lo nesta jornada perigosa.",
                textJA = "旅人よ、挨拶を！私たちの運命の大陸は、古代の永遠の闇へと沈みつつあります。この危険な旅路、私が同行しましょう。",
                textCH = "你好，旅人！我们的命运大陆正在沉入远古无尽的黑暗之中。我将陪伴你度过这段危险的旅程。",
                textKR = "반갑다, 여행자여! 우리의 운명 대륙이 고대 무한의 어둠 속으로 잠기고 있다. 내가 이 위험한 여정에 동행하겠다.",
                choicesRU = new string[] { "Кто ты такая?", "Я готов к битве!" },
                choicesEN = new string[] { "Who are you?", "I am ready for battle!" },
                choicesDE = new string[] { "Wer bist du?", "Ich bin bereit für den Kampf!" },
                choicesFR = new string[] { "Qui es-tu ?", "Je suis prêt pour le combat !" },
                choicesES = new string[] { "¿Quién eres?", "¡Estoy listo para la batalla!" },
                choicesPT = new string[] { "Quem é você?", "Estou pronto para a batalha!" },
                choicesJA = new string[] { "お前は誰だ？", "戦いの準備はできている！" },
                choicesCH = new string[] { "你是谁？", "我准备好战斗了！" },
                choicesKR = new string[] { "당신은 누구십니까?", "전투 준비 완료!" },
                nextLineIndexes = new int[] { 1, 2 }
            };

            DialogLine l2 = new DialogLine
            {
                textRU = "Меня зовут Аэлисса, хранительница священного Кристалла Зенита. Моя магия защитит тебя от коварства Кровавых Пустошей.",
                textEN = "My name is Aelyssa, keeper of the sacred Zenith Crystal. My magic will protect you from the treachery of the Crimson Wastes.",
                textDE = "Mein Name ist Aelyssa, Hüterin des heiligen Zenit-Kristalls. Meine Magie wird euch vor dem Verrat der Blutigen Ödlande schützen.",
                textFR = "Je m'appelle Aelyssa, gardienne du cristal sacré du Zénith. Ma magie vous protégera de la trahison des Landes Sanglantes.",
                textES = "Mi nombre es Aelyssa, guardiana del sagrado Cristal Cenit. Mi magia te protegerá de la traición de los Páramos Sangrientos.",
                textPT = "Meu nome é Aelyssa, guardiã do sagrado Cristal Zenith. Minha magia irá protegê-lo da traição das Terras Desoladas Carmesins.",
                textJA = "私はアエリッサ、聖なるゼнискристаллаの守護者です。私の魔法が、血の荒野の邪悪からあなたを守るでしょう。",
                textCH = "我叫艾莉莎，神圣天顶水晶的守护者。我的魔法将保护你免受绯红荒野的背叛。",
                textKR = "내 이름은 앨리사, 신성한 제니스 크리스탈의 수호자다. 나의 마법이 크림슨 황무지의 배신으로부터 당신을 지켜줄 것이다.",
                choicesRU = new string[] { "Продолжить поход" },
                choicesEN = new string[] { "Continue quest" },
                choicesDE = new string[] { "Quest fortsetzen" },
                choicesFR = new string[] { "Continuer la quête" },
                choicesES = new string[] { "Continuar misión" },
                choicesPT = new string[] { "Continuar jornada" },
                choicesJA = new string[] { "冒険を続ける" },
                choicesCH = new string[] { "继续旅程" },
                choicesKR = new string[] { "여정 계속하기" },
                nextLineIndexes = new int[] { 3 }
            };

            DialogLine l3 = new DialogLine
            {
                textRU = "Отлично! Твое оружие заряжено энергией Зенита. Двинемся вперед через северные врата замка!",
                textEN = "Excellent! Your weapon is infused with Zenith energy. Let us move forward through the northern castle gates!",
                textDE = "Hervorragend! Ihre Waffe ist mit Zenit-Energie erfüllt. Lasst uns durch die nördlichen Burgtore vorrücken!",
                textFR = "Excellent ! Votre arme est imprégnée de l'énergie du Zénith. Avançons par les portes nord du château !",
                textES = "¡Excelente! Tu arma está infundida con energía Cenit. ¡Avancemos por las puertas del norte del castillo!",
                textPT = "Excelente! Sua arma está infundida com energia Zenith. Vamos avançar pelos portões norte do castelo!",
                textJA = "素晴らしい！あなたの武器にはゼニスのマ力が注入されました。城の北門を通り、前進しましょう！",
                textCH = "太棒了！你的武器被注入了天顶能量。让我们从北门穿过城堡前进吧！",
                textKR = "훌륭하다! 당신의 무기에 제니스 에нер지가 주입되었다. 북쪽 성문을 통해 전진하자!",
                choicesRU = new string[] { "Начать приключение" },
                choicesEN = new string[] { "Start adventure" },
                choicesDE = new string[] { "Abenteuer starten" },
                choicesFR = new string[] { "Commencer l'aventure" },
                choicesES = new string[] { "Iniciar aventura" },
                choicesPT = new string[] { "Iniciar aventura" },
                choicesJA = new string[] { "冒険を始める" },
                choicesCH = new string[] { "开始冒险" },
                choicesKR = new string[] { "모험 시작하기" },
                nextLineIndexes = new int[] { 3 }
            };

            DialogLine l4 = new DialogLine
            {
                textRU = "Помни: каждый выбор здесь имеет значение. Наш отряд готов к бою. Теперь выбери область на Континенте Судьбы для первой боевой зачистки:",
                textEN = "Remember: every choice here has consequences. Our squad is ready. Now select a territory on the Fate Continent for the initial tactical sweep:",
                textDE = "Denkt daran: Jede Entscheidung hier hat Konsequenzen. Unsere Truppe ist bereit. Wählt nun ein Gebiet auf dem Schicksalskontinent für die erste taktische Säuberung aus:",
                textFR = "Rappelez-vous : chaque choix ici a des conséquences. Notre escouade est prête. Sélectionnez maintenant un territoire sur le Continent du Destin pour le nettoyage tactique initial :",
                textES = "Recuerda: cada elección aquí tiene consecuencias. Nuestro escuadro está listo. Ahora selecciona un territorio en el Continente del Destino para el barrido táctico inicial:",
                textPT = "Lembre-se: cada escolha aqui tem consequências. Nosso esquadrão está pronto. Agora selecione um território no Continente do Destino para a varredura tática inicial:",
                textJA = "忘れないでください、ここでの選択にはすべて結果が伴います。私たちの部隊は準備万端です。さあ、最初の戦術的掃討のために、運命の大陸から地域を選択してください：",
                textCH = "记住：这里的每一个选择都有其后果。我们的队伍已准备就绪。现在请选择命运大陆上的一个区域进行首次战术肃清：",
                textKR = "기억해라: 이곳에서의 모든 선택은 그 결과가 따른다. 우리 부대는 전투 준비가 끝났다. 이제 운명의 대륙에서 첫 전술적 소탕을 전개할 지역을 선택해라:",
                choicesRU = new string[] { "Кровавые Пустоши", "Ледяной Пик", "Древние Руины", "Грозовые Кряжи" },
                choicesEN = new string[] { "Crimson Wastes", "Ice-Bound Peak", "Ancient Ruins", "Storm Ridges" },
                choicesDE = new string[] { "Blutige Ödlande", "Eisgipfel", "Alte Ruinen", "Sturmkämme" },
                choicesFR = new string[] { "Landes Sanglantes", "Pic de Glace", "Ruines Anciennes", "Crêtes de Tempête" },
                choicesES = new string[] { "Páramos Sangrientos", "Pico Helado", "Ruinas Antiguas", "Crestas de Tormenta" },
                choicesPT = new string[] { "Terras Desoladas Carmesins", "Pico Congelado", "Ruínas Antigas", "Cumes da Tempestade" },
                choicesJA = new string[] { "血の荒野", "氷結の峰", "古代の遺跡", "嵐の尾根" },
                choicesCH = new string[] { "绯红荒野", "冰封之巅", "远古遗迹", "雷暴山脊" },
                choicesKR = new string[] { "크림슨 황무지", "빙설의 봉우리", "고대 유적지", "폭풍 산맥" },
                nextLineIndexes = new int[] { 4, 5, 6, 7 }
            };

            DialogLine l5 = new DialogLine
            {
                textRU = "Вы выбрали Кровавые Пустоши! Здесь сильны орды бандитов и адские ветры Зенита. Да пребудет с тобой благословение Кристалла! Мы отправляемся в бой.",
                textEN = "You have selected the Crimson Wastes! Bandit hordes and infernal Zenith winds plague this land. May the blessing of the Crystal guide us! Charging into battle.",
                textDE = "Ihr habt die Blutigen Ödlande gewählt! Banditenhorden und höllische Zenit-Winde plagen dieses Land. Möge der Segen des Kristalls uns leiten! Wir ziehen in den Kampf.",
                textFR = "Vous avez choisi les Landes Sanglantes ! Des hordes de bandits et des vents infernaux du Zénith accablent cette terre. Que la bénédiction du Cristal nous guide ! En route vers la bataille.",
                textES = "¡Has seleccionado los Páramos Sangrientos! Las hordas de bandidos y los vientos infernales de Cenit asolan esta tierra. ¡Que la bendición del Cristal nos guíe! Entrando en batalla.",
                textPT = "Você selecionou as Terras Desoladas Carmesins! Hordas de bandidos e ventos infernais de Zenith assolam esta terra. Que a bênção do Cristal nos guie! Entrando em batalha.",
                textJA = "血の荒野を選択しましたね！山賊の群れと荒れ狂うゼニスの魔力嵐が大地を襲っています。クリスタルの祝福があなたを導いてくれますように！戦場へ進撃します。",
                textCH = "你选择了绯红荒野！这里充斥着强盗匪帮和狂暴的天顶狂风。愿水晶祝福我们！即刻出发，开辟战场。",
                textKR = "크림슨 황무지를 선택했다! 도적 떼와 거친 제니스 마력 폭풍이 몰아치는 대지다. 크리스탈의 축복이 당신을 인도하기를! 전장으로 진격한다.",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesDE = new string[] { "Dialog beenden" },
                choicesFR = new string[] { "Terminer le dialogue" },
                choicesES = new string[] { "Terminar diálogo" },
                choicesPT = new string[] { "Encerrar diálogo" },
                choicesJA = new string[] { "対話を終了する" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null
            };

            DialogLine l6 = new DialogLine
            {
                textRU = "Вы выбрали Ледяной Пик! Вечная мерзлота проверяет волю на прочность, а Ледяные Големы стерегут древние сокровища. Да пребудет с тобой благословение Кристалла!",
                textEN = "You have selected the Ice-Bound Peak! The absolute permafrost tests our resolve, while giant Ice Golems stand guard over absolute wonders. May the Crystal protect us!",
                textDE = "Ihr habt den Eisgipfel gewählt! Der ewige Permafrost stellt unsere Entschlossenheit auf die Probe, während riesige Eisgolems uralte Schätze bewachen. Möge der Kristall uns schützen!",
                textFR = "Vous avez choisi le Pic de Glace ! Le pergélisol éternel met notre volonté à l'épreuve, tandis que d'immenses Golems de Glace gardent des merveilles anciennes. Que le Cristal nous protège !",
                textES = "¡Has seleccionado el Pico Helado! El permafrost perpetuo pone a prueba nuestra voluntad, mientras gigantescos Golems de Hielo custodian los antiguos tesoros. ¡Que el Cristal nos proteja!",
                textPT = "Você selecionou o Pico Congelado! O permafrost eterno testa nossa determinação, enquanto gigantescos Golems de Gelo protegem os tesouros antigos. Que o Cristal nos proteja!",
                textJA = "氷結の峰を選択しましたね！過酷な永久凍土が意志を試しており、巨大な氷のゴーレムたちが古代の神秘を守護しています。クリスタルの加護がありますように！",
                textCH = "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！",
                textKR = "빙설의 봉우리를 선택했다! 혹독한 영구 동토가 의지를 시험하며, 거대한 얼음 골렘들이 고대의 신비를 경비하고 있다. 크리스탈의 보살핌이 있기를!",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesDE = new string[] { "Dialog beenden" },
                choicesFR = new string[] { "Terminer le dialogue" },
                choicesES = new string[] { "Terminar diálogo" },
                choicesPT = new string[] { "Encerrar diálogo" },
                choicesJA = new string[] { "対話を終了する" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null
            };

            DialogLine l7 = new DialogLine
            {
                textRU = "Вы выбрали Древние Руины! Забытые катакомбы хранят остатки древних кристаллов Зенита, но берегись ловушек и древних теней. Да пребудет с тобой благословение Кристалла!",
                textEN = "You have selected the Ancient Ruins! Forgotten catacombs hold absolute remnants of ancient Zenith energy crystals, but beware deadly traps and immortal shadows. Crystal bless you!",
                textDE = "Ihr habt die Alten Ruinen gewählt! Vergessene Katakomben bergen Reste uralter Zenit-Kristalle, aber hütet euch vor tödlichen Fallen und unsterblichen Schatten. Möge der Kristall euch segnen!",
                textFR = "Vous avez choisi les Ruines Anciennes ! Des catacombes oubliées recèlent des vestiges d'anciens cristaux d'énergie du Zénith, mais gare aux pièges mortels et aux ombres anciennes. Que le Cristal vous bénisse !",
                textES = "¡Has seleccionado las Ruinas Antiguas! Catacumbas olvidadas albergan restos de los antiguos cristales de energía Cenit, pero ten cuidado con las trampas mortales y las sombras antiguas. ¡El Cristal te bendiga!",
                textPT = "Você selecionou as Ruínas Antigas! Catacumbas esquecidas guardam vestígios dos antigos cristais de energia Zenith, mas cuidado com armadilhas mortais e sombras imortais. Que o Cristal o abençoe!",
                textJA = "古代の遺跡を選択しましたね！忘れられた地下墓地には古代のゼニスマ力結晶の残骸が隠されていますが、致命的な罠と不滅の影を警戒してください。クリスタルの祝福を！",
                textCH = "你选择了远古遗迹！被遗忘的墓穴藏有远古天顶能量水晶的余烬，但务必小心致命的陷阱与不死的幽影。愿水晶赐福于你！",
                textKR = "고대 유적지를 선택했다! 잊혀진 지하 묘지에 고대 제니스 마력 결정의 잔재가 숨겨져 있지만, 치명적인 함정과 불멸의 그림자를 경계해라. 크리스탈의 축복을!",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesDE = new string[] { "Dialog beenden" },
                choicesFR = new string[] { "Terminer le dialogue" },
                choicesES = new string[] { "Terminar diálogo" },
                choicesPT = new string[] { "Encerrar diálogo" },
                choicesJA = new string[] { "対話を終了する" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null
            };

            DialogLine l8 = new DialogLine
            {
                textRU = "Вы выбрали Грозовые Кряжи! Облачный архипелаг, парящий над бездной. Здесь бушуют постоянные молнии, а воздух раздирают стихийные бури. Да пребудет с нами Кристалл!",
                textEN = "You have selected the Storm Ridges! A cloud archipelago floating over the abyss. Constant lightning storms rage here, and elemental tempests tear the air. May the Crystal protect us!",
                textDE = "Ihr habt die Sturmkämme gewählt! Ein Wolkenarchipel, der über dem Abgrund schwebt. Ständige Blitze wüten hier, und elementare Stürme zereißen die Luft. Möge der Kristall uns schützen!",
                textFR = "Vous avez choisi les Crêtes de Tempête ! Un archipel de nuages flottant au-dessus de l'abîme. Des tempêtes d'éclairs continuelles y font rage et des tempêtes élémentaires déchirent l'air. Que le Cristal nous protège !",
                textES = "¡Has seleccionado las Crestas de Tormenta! Un archipiélago de nubes flotando sobre el abismo. Constantes tormentas de rayos rugen aquí y tormentas elementales desgarran el aire. ¡Que el Cristal nos proteja!",
                textPT = "Você selecionou os Cumes da Tempestade! Um arquipélago de nuvens flutuando sobre o abismo. Tempestades de raios constantes rugem aqui e tempestades elementais rasgam o ar. Que o Cristal nos proteja!",
                textJA = "嵐の尾根を選択しましたね！深淵の上に浮かぶ雲の群島です。ここには絶え間ない雷雨が吹き荒れ、元素の嵐が空気を引き裂いています。クリスタルの加護がありますように！",
                textCH = "你选择了雷暴山脊！悬浮在深渊之上的云中群岛。这里肆虐着连绵不断的雷暴，元素风暴撕裂着空气。愿水晶庇护我们！",
                textKR = "폭풍 산맥을 선택했다! 심연 위에 떠 있는 구름 군도입니다. 이곳에는 끊임없는 번개 폭풍이 치고 원소의 폭풍이 공기를 찢고 있습니다. 크리스탈의 보살핌이 있기를!",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesDE = new string[] { "Dialog beenden" },
                choicesFR = new string[] { "Terminer le dialogue" },
                choicesES = new string[] { "Terminar diálogo" },
                choicesPT = new string[] { "Encerrar diálogo" },
                choicesJA = new string[] { "対話を終了する" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null
            };

            dialogueSteps.Add(l1);
            dialogueSteps.Add(l2);
            dialogueSteps.Add(l3);
            dialogueSteps.Add(l4);
            dialogueSteps.Add(l5);
            dialogueSteps.Add(l6);
            dialogueSteps.Add(l7);
            dialogueSteps.Add(l8);

            DialogLine l9 = new DialogLine
            {
                textRU = "Мы успешно приземлились на выбранную точку! Это место станет нашей базой. Мы начинаем наш боевой поход со своей точки высадки и будем захватывать территории на Континенте Судьбы!",
                textEN = "We have successfully landed at our chosen point! This place will become our base. We begin our campaign from our landing site and will capture territories on the Fate Continent!",
                textDE = "Wir sind erfolgreich am gewählten Punkt gelandet! Dieser Ort wird unsere Basis. Wir beginnen unseren Feldzug von unserem Landeplatz aus und werden Gebiete auf dem Schicksalskontinent erobern!",
                textFR = "Nous avons atterri avec succès au point choisi ! Cet endroit deviendra notre base. Nous commençons notre campagne depuis notre site d'atterrissage et capturerons des territoires sur le Continent du Destin !",
                textES = "¡Hemos aterrizado con éxito en el punto elegido! Este lugar se convertirá en nuestra base. ¡Comenzamos nuestra campaña desde nuestro lugar de aterrizaje y capturaremos territorios en el Continente del Destino!",
                textPT = "Pousamos com sucesso no ponto escolhido! Este lugar se tornará nossa base. Iniciamos nossa campanha a partir do nosso local de pouso e capturaremos territórios no Continente do Destino!",
                textJA = "選択した場所に無事着陸しました！ここが私たちの基礎基地になります。着陸地点から遠征を開始し、運命の大陸全ての領土を征服していきます！",
                textCH = "我们成功降落在了选择的地点！这个地方将成为我们的基地。我们从登陆地点开始征程，将占领命运大陆上的领土！",
                textKR = "우리는 선택한 장소에 성공적으로 착륙했다! 이곳은 우리의 기지가 될 것이다. 우리는 착륙 지점에서 캠페인을 시작하여 운명 대륙의 영토를 모두 정복할 것이다!",
                choicesRU = new string[] { "Продолжить" },
                choicesEN = new string[] { "Continue" },
                choicesDE = new string[] { "Weiter" },
                choicesFR = new string[] { "Continuer" },
                choicesES = new string[] { "Continuar" },
                choicesPT = new string[] { "Continuar" },
                choicesJA = new string[] { "続ける" },
                choicesCH = new string[] { "继续" },
                choicesKR = new string[] { "계속" },
                nextLineIndexes = new int[] { 9 }
            };

            DialogLine l10 = new DialogLine
            {
                textRU = "Помни, здесь повсюду вражеские кланы, которых нам предстоит победить для полного освобождения нашего великого континента.",
                textEN = "Remember, enemy clans are everywhere, whom we must defeat for the complete liberation of our great continent.",
                textDE = "Denkt daran, dass überall feindliche Clans sind, die wir besiegen müssen, um unseren großen Kontinent vollständig zu befreien.",
                textFR = "N'oubliez pas que les clans ennemis sont partout. Nous devons les vaincre pour la libération totale de notre grand continent.",
                textES = "Recuerda, los clanes enemigos están en todas partes y debemos derrotarlos para la liberación total de nuestro gran continente.",
                textPT = "Lembre-se, clãs inimigos estão em toda parte, os quais devemos derrotar para a libertação completa de nosso grande continente.",
                textJA = "大界の完全な解放のために、戦って撃退しなければならない敵対する狂暴な部族たちが至る所に溢れていることを忘れないでください。",
                textCH = "记住，敌人氏族无处不在，为了彻底解放我们伟大的大陆，我们必须击败他们。",
                textKR = "기억해라, 대륙의 완전한 해방을 위해 반드시 격퇴해야 할 적 부족들이 도처에 널려 있다.",
                choicesRU = new string[] { "Продолжить" },
                choicesEN = new string[] { "Continue" },
                choicesDE = new string[] { "Weiter" },
                choicesFR = new string[] { "Continuer" },
                choicesES = new string[] { "Continuar" },
                choicesPT = new string[] { "Continuar" },
                choicesJA = new string[] { "続ける" },
                choicesCH = new string[] { "继续" },
                choicesKR = new string[] { "계속" },
                nextLineIndexes = new int[] { 10 }
            };

            DialogLine l11 = new DialogLine
            {
                textRU = "Со временем враги будут становиться всё сильнее. Чтобы выстоять и сокрушить их, нам необходимо усердно развивать наши опорные замки.",
                textEN = "Over time, enemies will become stronger. To withstand and crush them, we need to diligently develop our stronghold castles.",
                textDE = "Mit der Zeit werden die Feinde stärker. Um ihnen standzuhalten und sie zu vernichten, müssen wir unsere Festungsburgen fleißig ausbauen.",
                textFR = "Avec le temps, les ennemis deviendront plus forts. Pour leur résister et les écraser, nous devons développer avec diligence nos châteaux forts.",
                textES = "Con el tiempo, los enemigos se volverán más fuertes. Para resistir y aplastarlos, debemos desarrollar diligentemente nuestros castillos fortaleza.",
                textPT = "Com o tempo, os inimigos se tornarão mais fortes. Para resistir e esmagá-los, precisamos desenvolver diligentemente nossos castelos fortaleza.",
                textJA = "時間の経過とともに、敵はますます強くなってきます。これらに耐え、粉砕するためには、拠点の城を強固に発展させる必要があります。",
                textCH = "随着时间的推移，敌人会变得越来越强大。为了抵御并粉碎他们，我们需要努力发展我们的据点城堡。",
                textKR = "시간이 지날수록 귀중한 적들은 점점 강해질 것이다. 그들을 버텨내고 분쇄하려면 우리 지점의 거점 성들을 견고히 발전시켜야 한다.",
                choicesRU = new string[] { "Продолжить" },
                choicesEN = new string[] { "Continue" },
                choicesDE = new string[] { "Weiter" },
                choicesFR = new string[] { "Continuer" },
                choicesES = new string[] { "Continuar" },
                choicesPT = new string[] { "Continuar" },
                choicesJA = new string[] { "続ける" },
                choicesCH = new string[] { "继续" },
                choicesKR = new string[] { "계속" },
                nextLineIndexes = new int[] { 11 }
            };

            DialogLine l12 = new DialogLine
            {
                textRU = "Каждый замок имеет пятиуровневую систему улучшения. В зависимости от его уровня мы получим большие привилегии: пассивную добычу золота, покупку воинов, приобретение лечебных зелий и снаряжения, а также место для тренировки воинов и нашего героя.",
                textEN = "Each castle operates on a 5-level upgrade system. Depending on its level, we gain significant privileges: passive gold income, buying troops, purchasing health potions and equipment, and a training ground for soldiers and our hero.",
                textDE = "Jede Burg verfügt über ein 5-stufiges Upgrade-System. Je nach Stufe erhalten wir erhebliche Vorteile: passiven Goldertrag, Truppenrekrutierung, Kauf von Tränken und Ausrüstung und einen Übungsplatz.",
                textFR = "Chaque château fonctionne sur un système d'amélioration à 5 niveaux. Selon son niveau, nous obtenons des privilèges importants : revenu d'or passif, recrutement de troupes, achat de potions et d'équipement, et un terrain d'entraînement.",
                textES = "Cada castillo opera con un sistema de mejoras de 5 niveles. Según su nivel, obtenemos privilegios significativos: ingresos pasivos de oro, reclutamiento de tropas, compra de pociones y equipo, y un campo de entrenamiento.",
                textPT = "Cada castelo opera em um sistema de atualização de 5 níveis. Dependendo do seu nível, ganhamos privilégios significativos: renda passiva de ouro, recrutamento de tropas, compra de poções e equipamentos, e um campo de treinamento.",
                textJA = "個々の城は5段階のアップグレードシステムを採用しています。城のレベルに応じて、ゴールドの自動獲得、部隊の募集、回復薬や装備の購入、軍隊や英雄を訓練する演習場など、甚大な特恵を得られます。",
                textCH = "每个城堡都采用 5 级升级系统。根据其级别，我们将获得显着的特权：被动金币收益、招募士兵、在店面里购买恢复药水和装备，以及供士兵和我们英雄训练的场所。",
                textKR = "각 성은 5단계 업그레이드 시스템으로 운영된다. 성의 레벨에 따라 골드 자동 획득, 군사 모집, 치료용 물약 및 풍부한 장비 구매, 그리고 군사들과 영웅을 훈련시킬 연병장 등 거대한 혜택을 제공받게 된다.",
                choicesRU = new string[] { "Показать наши замки!" },
                choicesEN = new string[] { "Show our Castles!" },
                choicesDE = new string[] { "Zeige unsere Burgen!" },
                choicesFR = new string[] { "Montrer nos châteaux !" },
                choicesES = new string[] { "¡Muestra nuestros castillos!" },
                choicesPT = new string[] { "Mostrar nossos castelhos!" },
                choicesJA = new string[] { "私たちの城を見せて！" },
                choicesCH = new string[] { "展示我们的城堡！" },
                choicesKR = new string[] { "우리의 성 보여주기!" },
                nextLineIndexes = new int[] { 12 }
            };

            DialogLine l13 = new DialogLine
            {
                textRU = "Взгляни на карту! Вот наш главный замок и замки твоих противников. Прокачивай и улучшай свои владения! На первом континенте максимальный уровень замков равен 2. Повышай уровень замка, развивай свои войска, и победа будет за нами!",
                textEN = "Look at the map! Here is our primary castle and the castles of your opponents. Upgrade and improve your domains! On the first continent, the maximum castle level is Level 2. Raise your castle level, grow your armies, and victory shall be ours!",
                textDE = "Schaut auf die Karte! Hier ist unsere Hauptburg und die Burgen eurer Gegner. Rüstet eure Gebiete auf! Auf dem ersten Kontinent beträgt die maximale Burgstufe 2. Erhöht die Stufe, vergrößert eure Armeen, und der Sieg wird unser sein!",
                textFR = "Regardez la carte ! Voici notre château principal et les châteaux de vos adversaires. Améliorez vos domaines ! Sur le premier continent, le niveau maximum du château est le niveau 2. Augmentez-le, formez vos armées, et la victoire sera nôtre !",
                textES = "¡Mira el mapa! Aquí está nuestro castillo principal y los castillos de tus oponentes. ¡Mejora tus dominios! En el primer continente, el nivel máximo del castillo es el Nivel 2. ¡Sube el nivel del castillo, entrena a tus ejércitos y la victoria será nuestra!",
                textPT = "Olhe para o mapa! Aqui está o nosso castelo principal e os castelos de seus oponentes. Atualize seus domínios! No primeiro continente, o nível máximo do castelo é o Nível 2. Suba o nível do castelo, treine seus exércitos e a vitória será nossa!",
                textJA = "地図をご覧ください！ここが私たちの主城であり、周囲が相手の要塞です。領地をアップグレードして改善しましょう！最初の領土での最大レベルは2です。城砦を育成して部隊を強化すれば、勝利は私たちのものです！",
                textCH = "看地图！这是我们的主城和对手的城堡。升级并改善你的领地！在第一大陆，城堡的最高等级为二级。提高城堡等级，训练你的军队，胜利一定属于我们！",
                textKR = "지도를 보아라! 이곳이 우리의 주 성이며 주변이 상대 적들의 요새다. 영토의 지배권을 높이고 업г레이드해라! 첫 번째 대륙에서 성의 최대 레벨은 2단계까지다. 성채를 육성하고 부대를 강화한다면 승리는 우리의 것이다!",
                choicesRU = new string[] { "Начать завоевание" },
                choicesEN = new string[] { "Start conquest" },
                choicesDE = new string[] { "Feldzug starten" },
                choicesFR = new string[] { "Commencer la conquête" },
                choicesES = new string[] { "Iniciar conquista" },
                choicesPT = new string[] { "Começar conquista" },
                choicesJA = new string[] { "征服を開始する" },
                choicesCH = new string[] { "开始征服" },
                choicesKR = new string[] { "정복 시작하기" },
                nextLineIndexes = null // Конец диалога
            };

            dialogueSteps.Add(l9);
            dialogueSteps.Add(l10);
            dialogueSteps.Add(l11);
            dialogueSteps.Add(l12);
            dialogueSteps.Add(l13);
        }
    }
}
