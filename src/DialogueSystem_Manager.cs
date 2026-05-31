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
        public static DialogueSystem_Manager Instance { get; private set; }

        [Header("Действующие Персонажи")]
        public string companionNameRU = "Аэлисса";
        public string companionNameEN = "Aelyssa";
        
        [Header("Настройки Изображений Портретов (Спрайты)")]
        public Sprite companionPortrait;    // Назначается в инспекторе или генерируется программно
        public Sprite warriorPortrait;      // Портрет воина для правой стороны
        public Sprite archerPortrait;       // Портрет стрелка для правой стороны
        public Sprite magePortrait;         // Портрет мага для правой стороны

        [Header("Настройки Звуков Речи Помощника")]
        public AudioClip companionVoiceClip; // Короткие фоновые фразы помощника при репликах

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
        public Vector2 choiceContainerPosition = new Vector2(0f, 40f); // Приподнимайте по Y, чтобы не срезались кнопки!
        public Vector2 choiceContainerSizeDelta = new Vector2(-150f, 60f); // Ширина и высота панели кнопок

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
            public string textCH;         // Китайский перевод
            public string textKR;         // Корейский перевод
            public string[] choicesRU;    // Ответные реплики игрока (ветвление диалога)
            public string[] choicesEN;
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
        private bool isDialogueActive = false;

        public bool IsDialogueActive
        {
            get { return isDialogueActive; }
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
            else
            {
                Destroy(gameObject);
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
        }

        public void ApplyRectTransformTweaks()
        {
            if (!enforceCoordinates) return;

            if (leftSpeakerNameText != null)
            {
                RectTransform rt = leftSpeakerNameText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = companionNameAnchorMin;
                    rt.anchorMax = companionNameAnchorMax;
                    rt.anchoredPosition = companionNamePosition;
                    rt.sizeDelta = companionNameSizeDelta;
                }
                leftSpeakerNameText.alignment = companionNameAlignment;
            }

            if (rightSpeakerNameText != null)
            {
                RectTransform rt = rightSpeakerNameText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = heroNameAnchorMin;
                    rt.anchorMax = heroNameAnchorMax;
                    rt.anchoredPosition = heroNamePosition;
                    rt.sizeDelta = heroNameSizeDelta;
                }
                rightSpeakerNameText.alignment = heroNameAlignment;
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
                    rt.anchoredPosition = choiceContainerPosition;
                    rt.sizeDelta = choiceContainerSizeDelta;
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
            string[] currentChoices = null;
            switch (lang)
            {
                case 0: currentChoices = currentLine.choicesRU; break;
                case 8: currentChoices = currentLine.choicesCH; break;
                case 7: currentChoices = currentLine.choicesKR; break;
                default: currentChoices = currentLine.choicesEN; break;
            }

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
        /// Запустить диалог по индексу реплики
        /// </summary>
        public void StartDialogue(int startIndex = 0)
        {
            if (dialogueSteps.Count == 0)
            {
                SetupFallbackDialogues();
            }

            isDialogueActive = true;
            currentLineIndex = startIndex;

            if (dialogPanel == null)
            {
                InitializeDialogueUI();
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

            // Если есть варианты выбора, игрок должен кликнуть по кнопке выбора, обычный клик не продвигает диалог
            bool hasChoices = (Translator.LanguageID == 0 ? currentLine.choicesRU : currentLine.choicesEN) != null && 
                              (Translator.LanguageID == 0 ? currentLine.choicesRU.Length : currentLine.choicesEN.Length) > 0;

            if (hasChoices)
            {
                return; 
            }

            // Переход на следующую реплику
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
            if (!isDialogueActive) return;

            DialogLine currentLine = dialogueSteps[currentLineIndex];

            // Интерактивная синхронизация: если мы делаем выбор в шаге 3 (выбор места высадки на тактическую карту)
            if (currentLineIndex == 3)
            {
                if (FateMapManager.Instance != null)
                {
                    // Переключаем карту и делаем её видимой для игрока прямо сейчас!
                    FateMapManager.Instance.SwitchToMap(choiceIndex);
                    FateMapManager.Instance.SetMapVisible(true);
                    Debug.Log($"[DIALOGUE SYSTEM] Игрок выбрал зону {choiceIndex}. Динамически переключаем активную тактическую карту на индекс: {choiceIndex}");
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
            isDialogueActive = false;
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            // После окончания любого диалога автоматически открываем и показываем карту мира
            if (FateMapManager.Instance != null)
            {
                FateMapManager.Instance.SetMapVisible(true);
                Debug.Log("[DIALOGUE SYSTEM] Раскрываем интерактивную карту мира для выбора фракций.");
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
                return (lang == 0) ? "Снайпер" : "Archer";
            if (cls.Contains("mage") || cls.Contains("маг"))
                return (lang == 0) ? "Маг" : "Mage";
            return rawClass;
        }

        private TextMeshProUGUI CreatePortraitNameLabel(Image portraitImage, string defaultText, bool isLeft)
        {
            if (portraitImage == null) return null;

            // Ищем, не создавали ли мы уже этот текст ранее
            string goName = isLeft ? "Txt_CompanionName_Dynamic" : "Txt_PlayerName_Dynamic";
            Transform existingText = portraitImage.transform.Find(goName);
            if (existingText != null)
            {
                return existingText.GetComponent<TextMeshProUGUI>();
            }

            // Создаем игровой объект подписи под портретом
            GameObject txtGov = new GameObject(goName);
            txtGov.transform.SetParent(portraitImage.transform, false);

            RectTransform rect = txtGov.AddComponent<RectTransform>();
            // Позиционируем снизу под портретом (на 25 пикселей ниже нижней кромки)
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, -25f);
            rect.sizeDelta = new Vector2(160f, 30f);

            // Создаем красивую подложку для надписи имени
            GameObject badgeBg = new GameObject("BadgeBg");
            badgeBg.transform.SetParent(txtGov.transform, false);
            RectTransform bgRect = badgeBg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(-10f, 0f); // Слегка шире текста
            bgRect.offsetMax = new Vector2(10f, 0f);

            Image bgImg = badgeBg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.12f, 0.95f); // Космический глубокий индиго

            Outline outline = badgeBg.AddComponent<Outline>();
            outline.effectColor = isLeft ? new Color(0.12f, 0.64f, 0.94f, 0.6f) : new Color(0.95f, 0.61f, 0.07f, 0.6f);
            outline.effectDistance = new Vector2(1f, 1f);

            TextMeshProUGUI tmp = txtGov.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 13f;
            tmp.fontWeight = FontWeight.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            // Применяем шрифт
            if (Translator.Instance != null && Translator.Instance.defaultFont != null)
            {
                tmp.font = Translator.Instance.defaultFont;
            }

            // Перемещаем подложку на задний план, чтобы текст был поверх нее
            badgeBg.transform.SetAsFirstSibling();

            return tmp;
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

            // Самоисцеление: динамически создаем надписи имен под портретами в стиле Zenith Glassmorphism, если в канвасе они отсутствуют!
            if (leftSpeakerNameText == null && companionPortraitImage != null)
            {
                leftSpeakerNameText = CreatePortraitNameLabel(companionPortraitImage, companionName, true);
            }
            if (rightSpeakerNameText == null && playerPortraitImage != null)
            {
                rightSpeakerNameText = CreatePortraitNameLabel(playerPortraitImage, playerDisplayName, false);
            }

            // Текстовые подписи под левым и правым портеретами
            if (leftSpeakerNameText != null)
            {
                leftSpeakerNameText.text = companionName;
            }
            if (rightSpeakerNameText != null)
            {
                rightSpeakerNameText.text = playerDisplayName;
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

            if (speakerNameText != null)
            {
                speakerNameText.text = activeSpeakerName;
            }

            // Настройка текста реплики со специфичными переводами
            string mainText = "";
            switch (lang)
            {
                case 0: mainText = currentLine.textRU; break;
                case 8: mainText = !string.IsNullOrEmpty(currentLine.textCH) ? currentLine.textCH : currentLine.textEN; break;
                case 7: mainText = !string.IsNullOrEmpty(currentLine.textKR) ? currentLine.textKR : currentLine.textEN; break;
                default: mainText = currentLine.textEN; break;
            }
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

            // Применяем визуальный акцент (подсвечиваем говорящего и слегка затемняем слушателя)
            if (speaking)
            {
                // Говорит игрок
                if (playerPortraitImage != null) playerPortraitImage.color = Color.white;
                if (companionPortraitImage != null) companionPortraitImage.color = new Color(1f, 1f, 1f, 0.4f);

                if (rightSpeakerNameText != null) rightSpeakerNameText.color = new Color(0.95f, 0.61f, 0.07f, 1f); // Золотой
                if (leftSpeakerNameText != null) leftSpeakerNameText.color = new Color(0.7f, 0.8f, 0.9f, 0.4f); // Приглушенный
            }
            else
            {
                // Говорит компаньон (Аэлисса)
                if (companionPortraitImage != null) companionPortraitImage.color = Color.white;
                if (playerPortraitImage != null) playerPortraitImage.color = new Color(1f, 1f, 1f, 0.4f);

                if (leftSpeakerNameText != null) leftSpeakerNameText.color = new Color(0.95f, 0.61f, 0.07f, 1f); // Золотой
                if (rightSpeakerNameText != null) rightSpeakerNameText.color = new Color(0.7f, 0.8f, 0.9f, 0.4f); // Приглушенный
            }

            // ИЗЮМИНКА СЦЕНЫ: принудительно раскрываем интерактивную карту мира, когда диалог доходит до выбора места высадки (Index 3)!
            if (currentLineIndex == 3)
            {
                if (FateMapManager.Instance != null)
                {
                    FateMapManager.Instance.SetMapVisible(true);
                    Debug.Log("[DIALOGUE SYSTEM] Карта мира плавно открыта на фоне для выбора места высадки.");
                }
            }

            // Обработка вариантов ответов (кнопок выбора)
            string[] currentChoices = null;
            switch (lang)
            {
                case 0: currentChoices = currentLine.choicesRU; break;
                case 8: currentChoices = currentLine.choicesCH; break;
                case 7: currentChoices = currentLine.choicesKR; break;
                default: currentChoices = currentLine.choicesEN; break;
            }

            // Полностью очищаем все дочерние элементы контейнера, если это не сам DialoguePanel во избежание багов
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
                }
                if (nextDialogueButton != null) nextDialogueButton.gameObject.SetActive(false);

                for (int i = 0; i < currentChoices.Length; i++)
                {
                    int index = i;
                    Button btn = CreateChoiceButton(currentChoices[i], () => SelectChoice(index));
                    choiceButtons.Add(btn);
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
                    // Обновляем текст кнопки под язык локализации!
                    TextMeshProUGUI txtTmp = nextDialogueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (txtTmp != null)
                    {
                        txtTmp.text = (lang == 0) ? "Далее ▶" : "Next ▶";
                    }
                }
            }
        }

        private void UpdatePlayerPortrait()
        {
            if (playerPortraitImage == null) return;

            // Считываем текущий класс из SaveGameSystem в нижнем регистре для избежания проблем с регистром
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
                // Если сохранённый класс отсутствует или не совпадает, но в инспекторе/на сцене вручную задан спрайт (например, Снайпер/Strelok) —
                // сохраняем полную видимость (Alpha = 255), иначе делаем силуэтным/полупрозрачным
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
            // Очищаем старые ссылки, если сцена сменилась
            dialogCanvas = null;
            dialogPanel = null;
            speakerNameText = null;
            dialogueBodyText = null;
            choiceContainer = null;
            companionPortraitImage = null;
            playerPortraitImage = null;
            nextDialogueButton = null;

            // Умный поиск: ищем FATE_DIALOGUE_CANVAS в сцене, включая неактивные корневые объекты
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
                dialogCanvas = rootCanvas.GetComponent<Canvas>();

                // Хак самоисцеления: принудительно скрываем посторонние меню, чтобы избежать наложения в разных сценах
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
                    
                    // Поиск вспомогательных контейнеров и портретов рекурсивно и по ключевым словам!
                    Image[] allImages = panelT.GetComponentsInChildren<Image>(true);
                    foreach (var img in allImages)
                    {
                        string imgName = img.gameObject.name.ToLower();
                        if (imgName.Contains("companion") || imgName.Contains("aelis") || imgName.Contains("left") || imgName.Contains("helper"))
                        {
                            companionPortraitImage = img;
                        }
                        else if (imgName.Contains("player") || imgName.Contains("hero") || imgName.Contains("right") || imgName.Contains("class") || imgName.Contains("avatar"))
                        {
                            playerPortraitImage = img;
                        }
                    }

                    // Если не найдено по содержимому имени, делаем классический точечный поиск по иерархии
                    if (companionPortraitImage == null)
                    {
                        Transform compT = FindChildRecursive(panelT, "Img_CompanionPortrait");
                        if (compT != null) companionPortraitImage = compT.GetComponent<Image>();
                    }
                    if (playerPortraitImage == null)
                    {
                        Transform playerT = FindChildRecursive(panelT, "Img_PlayerPortrait");
                        if (playerT != null) playerPortraitImage = playerT.GetComponent<Image>();
                    }

                    // Кэшируем оригинальные спрайты из сцены для предотвращения затирания при перерисовке
                    if (companionPortraitImage != null && companionPortraitImage.sprite != null)
                    {
                        originalCompanionSprite = companionPortraitImage.sprite;
                    }
                    if (playerPortraitImage != null && playerPortraitImage.sprite != null)
                    {
                        originalPlayerSprite = playerPortraitImage.sprite;
                    }

                    // Ищем выборный контейнер по ключевым словам или иерархии
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
                    
                    // Умная классификация текстовых полей
                    leftSpeakerNameText = null;
                    rightSpeakerNameText = null;
                    dialogueBodyText = null;

                    TextMeshProUGUI[] tmps = panelT.GetComponentsInChildren<TextMeshProUGUI>(true);
                    List<TextMeshProUGUI> nameTexts = new List<TextMeshProUGUI>();

                    foreach (var tmp in tmps)
                    {
                        // Проверяем, не принадлежит ли текст кнопкам выбора или портрету
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

                        // Ищем тело диалога по имени или ключевым словам
                        if (goName == "txt_dialoguebody" || goName == "dialoguebody" || goName == "dialogue_body" || goName == "txt_dialogue" || goName == "dialoguebodytext" || goName == "text_dialoguebody" || goName.Contains("body") || goName.Contains("content"))
                        {
                            if (!goName.Contains("speaker") && !goName.Contains("name"))
                            {
                                dialogueBodyText = tmp;
                                continue;
                            }
                        }

                        // Собираем кандидаты для имен героев
                        if (goName.Contains("speaker") || goName.Contains("name") || goName.Contains("cap") || goName.Contains("header") || goName.Contains("title") || goName.Contains("left") || goName.Contains("right"))
                        {
                            nameTexts.Add(tmp);
                        }
                    }

                    // Если body text не сформирован, берём оставшийся текст, который не попал в nameTexts
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

                    // Сортируем nameTexts по экранным координатам по оси X для точного разделения Left vs Right
                    if (nameTexts.Count >= 2)
                    {
                        nameTexts.Sort((a, b) =>
                        {
                            float ax = 0f;
                            float bx = 0f;
                            RectTransform ar = a.GetComponent<RectTransform>();
                            RectTransform br = b.GetComponent<RectTransform>();
                            if (ar != null) ax = ar.anchoredPosition.x;
                            if (br != null) bx = br.anchoredPosition.x;
                            return ax.CompareTo(bx);
                        });
                        leftSpeakerNameText = nameTexts[0];
                        rightSpeakerNameText = nameTexts[nameTexts.Count - 1];
                    }
                    else if (nameTexts.Count == 1)
                    {
                        float ax = 0f;
                        RectTransform ar = nameTexts[0].GetComponent<RectTransform>();
                        if (ar != null) ax = ar.anchoredPosition.x;
                        if (ax < 0f) leftSpeakerNameText = nameTexts[0];
                        else rightSpeakerNameText = nameTexts[0];
                    }

                    // Обратная совместимость во всем коде
                    speakerNameText = leftSpeakerNameText != null ? leftSpeakerNameText : rightSpeakerNameText;

                    // Укрепляем разметку СЦЕНЫ
                    if (dialogueBodyText != null)
                    {
                        dialogueBodyText.textWrappingMode = TextWrappingModes.Normal;
                    }
                    if (leftSpeakerNameText != null)
                    {
                        leftSpeakerNameText.textWrappingMode = TextWrappingModes.NoWrap;
                    }
                    if (rightSpeakerNameText != null)
                    {
                        rightSpeakerNameText.textWrappingMode = TextWrappingModes.NoWrap;
                    }

                    // Всегда принудительно накладываем координаты и настройки из инспектора во время инициализации!
                    ApplyRectTransformTweaks();

                    // Восстанавливаем разметку контейнера выбора кнопок (мягкое позиционирование для предотвращения отрезания)
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
                        Debug.Log("[DIALOGUE SYSTEM] Инициализация: Обнаружены, верифицированы и привязаны существующие UI-компоненты диалога.");
                        SetupNextDialogueButton();
                        return;
                    }
                }
            }

            // Создаем Canvas для диалогов с нуля, если в сцене его нет
            GameObject canvasGov = new GameObject("FATE_DIALOGUE_CANVAS");
            dialogCanvas = canvasGov.AddComponent<Canvas>();
            dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogCanvas.sortingOrder = 999; // Поверх геймплея и под меню паузы
            
            canvasGov.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGov.AddComponent<GraphicRaycaster>();

            // Создаем основную панель диалога (Специализированная шапка-контейнер "Exclusive High density Polygon Shape")
            dialogPanel = new GameObject("DialoguePanel");
            dialogPanel.transform.SetParent(canvasGov.transform, false);
            RectTransform panelRect = dialogPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 120f);
            panelRect.sizeDelta = new Vector2(950f, 220f);

            // Фоновая подложка из матового стекла (Zenith Glassmorphism)
            Image bgImg = dialogPanel.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.09f, 0.85f); // Космический глубокий индиго
            
            Outline outline = dialogPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.6f); // Светящийся циановый ободок (как на фото)
            outline.effectDistance = new Vector2(2f, 2f);

            // Добавляем красивую декоративную шапку (Top Panel Label "Cap" с уникальными скосами)
            GameObject headerGov = new GameObject("DialogueCap_Header");
            headerGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform headerRect = headerGov.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, 10f);
            headerRect.sizeDelta = new Vector2(-40f, 35f);

            Image headerImg = headerGov.AddComponent<Image>();
            headerImg.color = new Color(0.95f, 0.61f, 0.07f, 0.75f); // Оранжево-золотой акцент (из скриншота)

            // Текст спикера
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

            // Текст самой речи
            GameObject textGov = new GameObject("Txt_DialogueBody");
            textGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform textRect = textGov.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(180f, 40f); // Оставляем место под левый портрет
            textRect.offsetMax = new Vector2(-180f, -65f); // Гарантированный зазор сверху (65 пикселей под шапку для предотвращения залезания)

            dialogueBodyText = textGov.AddComponent<TextMeshProUGUI>();
            dialogueBodyText.text = "Loading conversation...";
            dialogueBodyText.fontSize = 15f;
            dialogueBodyText.color = new Color(0.9f, 0.95f, 1f, 1f);
            dialogueBodyText.alignment = TextAlignmentOptions.TopLeft;
            dialogueBodyText.textWrappingMode = TextWrappingModes.Normal;

            // Левый Портрет: Помощница (Аэлисса) в гексагональном или круглом контейнере
            GameObject compPortraitGov = new GameObject("Img_CompanionPortrait");
            compPortraitGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform compPortraitRect = compPortraitGov.AddComponent<RectTransform>();
            compPortraitRect.anchorMin = new Vector2(0f, 0.5f);
            compPortraitRect.anchorMax = new Vector2(0f, 0.5f);
            compPortraitRect.anchoredPosition = new Vector2(-60f, 10f);
            compPortraitRect.sizeDelta = new Vector2(160f, 160f);

            companionPortraitImage = compPortraitGov.AddComponent<Image>();
            companionPortraitImage.color = new Color(1f, 1f, 1f, 0.9f);
            
            Outline compOutline = compPortraitGov.AddComponent<Outline>();
            compOutline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.9f); // Циан
            compOutline.effectDistance = new Vector2(3f, 3f);

            // Правый Портрет: Выбранный Герой
            GameObject playerPortraitGov = new GameObject("Img_PlayerPortrait");
            playerPortraitGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform playerPortraitRect = playerPortraitGov.AddComponent<RectTransform>();
            playerPortraitRect.anchorMin = new Vector2(1f, 0.5f);
            playerPortraitRect.anchorMax = new Vector2(1f, 0.5f);
            playerPortraitRect.anchoredPosition = new Vector2(60f, 10f);
            playerPortraitRect.sizeDelta = new Vector2(160f, 160f);

            playerPortraitImage = playerPortraitGov.AddComponent<Image>();
            playerPortraitImage.color = new Color(1f, 1f, 1f, 0.9f);

            Outline playerOutline = playerPortraitGov.AddComponent<Outline>();
            playerOutline.effectColor = new Color(0.95f, 0.61f, 0.07f, 0.9f); // Оранжевый
            playerOutline.effectDistance = new Vector2(3f, 3f);

            // Контейнер Кнопок Выбора
            choiceContainer = new GameObject("ChoiceContainer");
            choiceContainer.transform.SetParent(dialogPanel.transform, false);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0f, 0f);
            choiceRect.anchorMax = new Vector2(1f, 0f);
            choiceRect.anchoredPosition = new Vector2(0f, 30f); // Кнопки приподняты внутрь окна диалога во избежание обрезания на кромке экрана
            choiceRect.sizeDelta = new Vector2(-80f, 50f);

            HorizontalLayoutGroup hLayout = choiceContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandHeight = false;
            hLayout.childForceExpandWidth = true;

            SetupNextDialogueButton();
        }

        private void SetupNextDialogueButton()
        {
            if (dialogPanel == null) return;

            Transform nextT = FindChildRecursive(dialogPanel.transform, "Btn_NextDialogue");
            if (nextT != null)
            {
                nextDialogueButton = nextT.GetComponent<Button>();
                if (nextDialogueButton != null)
                {
                    nextDialogueButton.onClick.RemoveAllListeners();
                    nextDialogueButton.onClick.AddListener(() => AdvanceDialogue());
                }
            }
            else
            {
                // Создаем красивую интерактивную кнопку "Далее" в правом нижнем углу диалоговой панели
                GameObject nextBtnGov = new GameObject("Btn_NextDialogue");
                nextBtnGov.transform.SetParent(dialogPanel.transform, false);
                RectTransform nextBtnRect = nextBtnGov.AddComponent<RectTransform>();
                nextBtnRect.anchorMin = new Vector2(1f, 0f);
                nextBtnRect.anchorMax = new Vector2(1f, 0f);
                nextBtnRect.anchoredPosition = new Vector2(-60f, 25f);
                nextBtnRect.sizeDelta = new Vector2(140f, 38f);

                Image nextBtnImg = nextBtnGov.AddComponent<Image>();
                // Сапфирово-сине-голубой цвет кнопки
                nextBtnImg.color = new Color(0.04f, 0.42f, 0.70f, 0.95f); 
                
                Outline nextBtnOutline = nextBtnGov.AddComponent<Outline>();
                nextBtnOutline.effectColor = new Color(0f, 0.94f, 1f, 0.5f); // Циановое свечение

                nextDialogueButton = nextBtnGov.AddComponent<Button>();
                nextDialogueButton.onClick.AddListener(() => AdvanceDialogue());

                GameObject nextTextGov = new GameObject("Txt_NextDialogue");
                nextTextGov.transform.SetParent(nextBtnGov.transform, false);
                RectTransform nextTextRect = nextTextGov.AddComponent<RectTransform>();
                nextTextRect.anchorMin = Vector2.zero;
                nextTextRect.anchorMax = Vector2.one;
                nextTextRect.offsetMin = Vector2.zero;
                nextTextRect.offsetMax = Vector2.zero;

                TextMeshProUGUI nextTmp = nextTextGov.AddComponent<TextMeshProUGUI>();
                nextTmp.text = "Далее ▶";
                nextTmp.fontSize = 13f;
                nextTmp.fontWeight = FontWeight.Bold;
                nextTmp.color = Color.white;
                nextTmp.alignment = TextAlignmentOptions.Center;

                // Накатываем активный шрифт
                if (Translator.Instance != null && Translator.Instance.defaultFont != null)
                {
                    nextTmp.font = Translator.Instance.defaultFont;
                }

                nextBtnGov.AddComponent<UIButtonPauseHover>();
            }
        }

        private Button CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
        {
            GameObject btnGov = new GameObject("Btn_DialogueChoice");
            btnGov.transform.SetParent(choiceContainer.transform, false);
            
            // Настройка жестких макетов, чтобы текст вариантов никогда не сжимался по вертикали или буква к букве
            LayoutElement layout = btnGov.AddComponent<LayoutElement>();
            layout.minWidth = choiceButtonMinWidth;
            layout.preferredWidth = choiceButtonPreferredWidth;
            layout.minHeight = choiceButtonMinHeight;

            Image btnImg = btnGov.AddComponent<Image>();
            btnImg.color = new Color(0.08f, 0.09f, 0.15f, 0.95f);
            
            Outline btnOutline = btnGov.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.12f, 0.64f, 0.94f, 0.4f);

            Button button = btnGov.AddComponent<Button>();
            button.onClick.AddListener(onClickAction);

            // Текст внутри кнопки
            GameObject textGov = new GameObject("Txt_BtnChoice");
            textGov.transform.SetParent(btnGov.transform, false);
            RectTransform textRect = textGov.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGov.AddComponent<TextMeshProUGUI>();
            tmp.text = choiceText;
            tmp.fontSize = choiceButtonFontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            // Накатываем активный шрифт
            if (Translator.Instance != null)
            {
                tmp.font = Translator.Instance.defaultFont;
            }

            // Добавляем hover-компонент, чтобы проигрывать звуки при наведении
            btnGov.AddComponent<UIButtonPauseHover>();

            return button;
        }

        private void SetupFallbackDialogues()
        {
            dialogueSteps.Clear();

            // Создаем тестовую стартовую цепочку (Аэлисса помогает искателю приключений)
            DialogLine l1 = new DialogLine
            {
                textRU = "Здравствуй, путник! Наш Континент Судьбы погружается во тьму древнего безвременья. Я буду сопровождать тебя в этом опасном походе.",
                textEN = "Greetings, traveler! Our Fate Continent is sinking into the darkness of ancient timelessness. I will accompany you in this dangerous journey.",
                textCH = "你好，旅人！我们的命运大陆正在沉入远古无尽的黑暗之中。我将陪伴你度过这段危险的旅程。",
                textKR = "반갑다, 여행자여! 우리의 운명 대륙이 고대 무한의 어둠 속으로 잠기고 있다. 내가 이 위험한 여정에 동행하겠다.",
                choicesRU = new string[] { "Кто ты такая?", "Я готов к битве!" },
                choicesEN = new string[] { "Who are you?", "I am ready for battle!" },
                choicesCH = new string[] { "你是谁？", "我准备好战斗了！" },
                choicesKR = new string[] { "당신은 누구십니까?", "전투 준비 완료!" },
                nextLineIndexes = new int[] { 1, 2 }
            };

            DialogLine l2 = new DialogLine
            {
                textRU = "Меня зовут Аэлисса, хранительница священного Кристалла Зенита. Моя магия защитит тебя от коварства Кровавых Пустошей.",
                textEN = "My name is Aelyssa, keeper of the sacred Zenith Crystal. My magic will protect you from the treachery of the Crimson Wastes.",
                textCH = "我叫艾莉莎，神圣天顶水晶的守护者。我的魔法将保护你免受绯红荒野的背叛。",
                textKR = "내 이름은 앨리사, 신성한 제니스 크ريس탈의 수호자다. 나의 마법이 크림슨 황무지의 배신으로부터 당신을 지켜줄 것이다.",
                choicesRU = new string[] { "Продолжить поход" },
                choicesEN = new string[] { "Continue quest" },
                choicesCH = new string[] { "继续旅程" },
                choicesKR = new string[] { "여정 계속하기" },
                nextLineIndexes = new int[] { 3 }
            };

            DialogLine l3 = new DialogLine
            {
                textRU = "Отлично! Твое оружие заряжено энергией Зенита. Двинемся вперед через северные врата замка!",
                textEN = "Excellent! Your weapon is infused with Zenith energy. Let us move forward through the northern castle gates!",
                textCH = "太棒了！你的武被注入了天顶能量。让我们从北门穿过城堡前进吧！",
                textKR = "훌륭하다! 당신의 무기에 제니스 에нер지가 주입되었다. 북쪽 성문을 통해 전진하자!",
                choicesRU = new string[] { "Начать приключение" },
                choicesEN = new string[] { "Start adventure" },
                choicesCH = new string[] { "开始冒险" },
                choicesKR = new string[] { "모험 시작하기" },
                nextLineIndexes = new int[] { 3 }
            };

            // l4 (Index 3) - Предлагает выбрать локацию для зачистки
            DialogLine l4 = new DialogLine
            {
                textRU = "Помни: каждый выбор здесь имеет значение. Наш отряд готов к бою. Теперь выбери область на Континенте Судьбы для первой боевой зачистки:",
                textEN = "Remember: every choice here has consequences. Our squad is ready. Now select a territory on the Fate Continent for the initial tactical sweep:",
                textCH = "记住：这里的每一个选择都有其后果。我们的队伍已准备就绪。现在请选择命运大陆上的一个区域进行首次战术肃清：",
                textKR = "기억해라: 이곳에서의 모든 선택은 그 결과가 따른다. 우리 부대는 전투 준비가 끝났다. 이제 운명의 대륙에서 첫 전술적 소탕을 전개할 지역을 선택해라:",
                choicesRU = new string[] { "Кровавые Пустоши", "Ледяной Пик", "Древние Руины" },
                choicesEN = new string[] { "Crimson Wastes", "Ice-Bound Peak", "Ancient Ruins" },
                choicesCH = new string[] { "绯红荒野", "冰封之巅", "远古遗迹" },
                choicesKR = new string[] { "크림슨 황무지", "빙설의 봉우리", "고대 유적지" },
                nextLineIndexes = new int[] { 4, 5, 6 }
            };

            // l5 (Index 4) - Результат Кровавые Пустоши
            DialogLine l5 = new DialogLine
            {
                textRU = "Вы выбрали Кровавые Пустоши! Здесь сильны орды бандитов и адские ветры Зенита. Да пребудет с тобой благословение Кристалла! Мы отправляемся в бой.",
                textEN = "You have selected the Crimson Wastes! Bandit hordes and infernal Zenith winds plague this land. May the blessing of the Crystal guide us! Charging into battle.",
                textCH = "你选择了绯红荒野！这里充斥着强盗匪帮 and 狂暴的天顶狂风。愿水晶祝福我们！即刻出发，开辟战场。",
                textKR = "크림슨 황무지를 선택했다! 도적 떼와 거친 제니스 마력 폭풍이 몰아치는 대지다. 크리스탈의 축복이 당신을 인도하기를! 전장으로 진격한다.",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null // Конец диалога
            };

            // l6 (Index 5) - Результат Ледяной Пик
            DialogLine l6 = new DialogLine
            {
                textRU = "Вы выбрали Ледяной Пик! Вечная мерзлота проверяет волю на прочность, а Ледяные Големы стерегут древние сокровища. Да пребудет с тобой благословение Кристалла!",
                textEN = "You have selected the Ice-Bound Peak! The absolute permafrost tests our resolve, while giant Ice Golems stand guard over absolute wonders. May the Crystal protect us!",
                textCH = "你选择了冰封之巅！永恒的极寒将考验你的意志，而寒冰巨魔正守护着古老奇迹。愿水晶庇佑我们！",
                textKR = "빙설의 봉우리를 선택했다! 혹독한 영구 동토가 의지를 시험하며, 거대한 얼음 골렘들이 고대의 신비를 경비하고 있다. 크리스탈의 보살핌이 있기를!",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
                choicesCH = new string[] { "结束对话" },
                choicesKR = new string[] { "대화 종료" },
                nextLineIndexes = null
            };

            // l7 (Index 6) - Результат Древние Руины
            DialogLine l7 = new DialogLine
            {
                textRU = "Вы выбрали Древние Руины! Забытые катакомбы хранят остатки древних кристаллов Зенита, но берегись ловушек и древних теней. Да пребудет с тобой благословение Кристалла!",
                textEN = "You have selected the Ancient Ruins! Forgotten catacombs hold absolute remnants of ancient Zenith energy crystals, but beware deadly traps and immortal shadows. Crystal bless you!",
                textCH = "你选择了远古遗迹！被遗忘的墓穴藏有远古天顶能量水晶的余烬，但务必小心致命的陷阱与不死的幽影。愿水晶赐福于你！",
                textKR = "고대 유적지를 선택했다! 잊혀진 지하 묘지에 고대 제니스 마력 결정의 잔재가 숨겨져 있지만, 치명적인 함정과 불멸의 그림자를 경계해라. 크리스탈의 축복을!",
                choicesRU = new string[] { "Завершить диалог" },
                choicesEN = new string[] { "End dialogue" },
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
        }
    }
}
