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
        private TextMeshProUGUI dialogueBodyText;
        private List<Button> choiceButtons = new List<Button>();
        private GameObject choiceContainer;

        private int currentLineIndex = 0;
        private bool isDialogueActive = false;

        public bool IsDialogueActive
        {
            get { return isDialogueActive; }
        }

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

        private void Start()
        {
            // При старте скрываем диалоговое окно
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
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

        public void SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive) return;

            DialogLine currentLine = dialogueSteps[currentLineIndex];
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

        public void EndDialogue()
        {
            isDialogueActive = false;
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            // Проверяем, на каком шаге мы закончили диалог (это определяет выбранную локацию!)
            if (currentLineIndex == 4)
            {
                Debug.Log("[GATEWAY] Загрузка уровня: Кровавые Пустоши!");
                SceneManager.LoadScene("Scene_CrimsonWastes"); // Имя сцены вашей локации
            }
            else if (currentLineIndex == 5)
            {
                Debug.Log("[GATEWAY] Загрузка уровня: Ледяной Пик!");
                SceneManager.LoadScene("Scene_IceBoundPeak"); // Имя сцены вашей локации
            }
            else if (currentLineIndex == 6)
            {
                Debug.Log("[GATEWAY] Загрузка уровня: Древние Руины!");
                SceneManager.LoadScene("Scene_AncientRuins"); // Имя сцены вашей локации
            }
            
            Debug.Log("[DIALOGUE SYSTEM] Диалог завершен.");
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
            
            // Настройка текста имени имя
            string speakerName = currentLine.characterName;
            if (string.IsNullOrEmpty(speakerName))
            {
                speakerName = (lang == 0) ? companionNameRU : companionNameEN;
            }
            speakerNameText.text = speakerName;

            // Настройка текста реплики со специфичными переводами
            string mainText = "";
            switch (lang)
            {
                case 0: mainText = currentLine.textRU; break;
                case 8: mainText = !string.IsNullOrEmpty(currentLine.textCH) ? currentLine.textCH : currentLine.textEN; break;
                case 7: mainText = !string.IsNullOrEmpty(currentLine.textKR) ? currentLine.textKR : currentLine.textEN; break;
                default: mainText = currentLine.textEN; break;
            }
            dialogueBodyText.text = mainText;

            // Накатываем шрифты в зависимости от языка
            ApplyDialogueFonts();

            // Динамический выбор портрета игрока на правой стороне
            UpdatePlayerPortrait();

            // Обработка вариантов ответов (кнопок выбора)
            string[] currentChoices = null;
            switch (lang)
            {
                case 0: currentChoices = currentLine.choicesRU; break;
                case 8: currentChoices = currentLine.choicesCH; break;
                case 7: currentChoices = currentLine.choicesKR; break;
                default: currentChoices = currentLine.choicesEN; break;
            }

            // Очищаем предыдущие кнопки
            foreach (var btn in choiceButtons)
            {
                Destroy(btn.gameObject);
            }
            choiceButtons.Clear();

            if (currentChoices != null && currentChoices.Length > 0)
            {
                choiceContainer.SetActive(true);
                for (int i = 0; i < currentChoices.Length; i++)
                {
                    int index = i;
                    Button btn = CreateChoiceButton(currentChoices[i], () => SelectChoice(index));
                    choiceButtons.Add(btn);
                }
            }
            else
            {
                choiceContainer.SetActive(false);
                // Создаем невидимую кнопку на весь корпус панели для возможности клика "Далее" мышкой
                Button nextBtn = CreateChoiceButton((lang == 0) ? "Далее..." : "Continue...", () => AdvanceDialogue());
                choiceButtons.Add(nextBtn);
            }
        }

        private void UpdatePlayerPortrait()
        {
            if (playerPortraitImage == null) return;

            // Считываем текущий класс из SaveGameSystem
            string savedClass = SaveGameSystem.CurrentData != null ? SaveGameSystem.CurrentData.characterClass : "";
            
            Sprite chosenSprite = null;
            if (savedClass.Contains("warrior"))
            {
                chosenSprite = warriorPortrait;
            }
            else if (savedClass.Contains("archer"))
            {
                chosenSprite = archerPortrait;
            }
            else if (savedClass.Contains("mage"))
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
                // Если портрет отсутствует, делаем его силуэтным/затемненным полупрозрачным
                playerPortraitImage.color = new Color(1f, 1f, 1f, 0.25f);
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

        private void InitializeDialogueUI()
        {
            // Создаем Canvas для диалогов
            GameObject canvasGov = new GameObject("FATE_DIALOGUE_CANVAS");
            dialogCanvas = canvasGov.AddComponent<Canvas>();
            dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogCanvas.sortingOrder = 999; // Поверх геймплея и под меню паузы
            
            canvasGov.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGov.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGov);

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
            speakerRect.fillShareWithParent(); // Занимает всю шапку
            speakerRect.offsetMin = new Vector2(15f, 0f);

            speakerNameText = speakerGov.AddComponent<TextMeshProUGUI>();
            speakerNameText.text = "Aelyssa";
            speakerNameText.fontSize = 18f;
            speakerNameText.fontWeight = FontWeight.Black;
            speakerNameText.color = Color.white;
            speakerNameText.alignment = TextAlignmentOptions.Left;

            // Текст самой речи
            GameObject textGov = new GameObject("Txt_DialogueBody");
            textGov.transform.SetParent(dialogPanel.transform, false);
            RectTransform textRect = textGov.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(180f, 40f); // Оставляем место под левый портрет
            textRect.offsetMax = new Vector2(-180f, -40f); // Оставляем место под правый портрет

            dialogueBodyText = textGov.AddComponent<TextMeshProUGUI>();
            dialogueBodyText.text = "Loading conversation...";
            dialogueBodyText.fontSize = 15f;
            dialogueBodyText.color = new Color(0.9f, 0.95f, 1f, 1f);
            dialogueBodyText.alignment = TextAlignmentOptions.TopLeft;

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
            choiceRect.anchoredPosition = new Vector2(0f, -60f); // Кнопки сдвинуты чуть ниже тела диалога
            choiceRect.sizeDelta = new Vector2(-80f, 50f);

            HorizontalLayoutGroup hLayout = choiceContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20f;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
        }

        private Button CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
        {
            GameObject btnGov = new GameObject("Btn_DialogueChoice");
            btnGov.transform.SetParent(choiceContainer.transform, false);
            
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
            textRect.fillShareWithParent();

            TextMeshProUGUI tmp = textGov.AddComponent<TextMeshProUGUI>();
            tmp.text = choiceText;
            tmp.fontSize = 13f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

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
                choicesRU = new string[] { "🩸 Кровавые Пустоши", "❄️ Ледяной Пик", "🏛️ Древние Руины" },
                choicesEN = new string[] { "🩸 Crimson Wastes", "❄️ Ice-Bound Peak", "🏛️ Ancient Ruins" },
                choicesCH = new string[] { "🩸 绯红荒野", "❄️ 冰封之巅", "🏛️ 远古遗迹" },
                choicesKR = new string[] { "🩸 크림슨 황무지", "❄️ 빙설의 봉우리", "🏛️ 고대 유적지" },
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

    public static class DialogueRectExtensions
    {
        public static void fillShareWithParent(this RectTransform t)
        {
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
        }
    }
}
