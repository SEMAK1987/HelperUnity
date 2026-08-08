using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Интерактивный менеджер диалогов с Котом-Наставником.
/// Поддерживает посимвольный вывод текста, портреты, выборы и 9 языков локализации.
/// </summary>
public class DialogueSystem_Manager : MonoBehaviour
{
    public static DialogueSystem_Manager Instance { get; private set; }

    [Header("UI Связи")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueBodyText;
    public Image leftSpeakerPortrait;
    public Image rightSpeakerPortrait;
    public Transform choiceButtonsContainer;
    public GameObject choiceButtonPrefab;

    [Header("Портреты персонажей")]
    public Sprite mentorCatPortrait;
    public Sprite playerCatPortrait;

    [Header("Звуковые эффекты")]
    public AudioClip textTypeSound;
    public AudioClip choiceSelectedSound;

    [Header("Настройки")]
    public float textSpeed = 0.03f;
    public bool enforceCoordinates = true;

    [System.Serializable]
    public class DialogStep
    {
        public string speakerNameRU;
        public string speakerNameEN;
        public string textRU;
        public string textEN;
        public string textDE;
        public string textFR;
        public string textES;
        public string textPT;
        public string textJA;
        public string textKO;
        public string textZH;
        public bool isLeftSpeaker = true;
        public Sprite customPortrait;

        public List<DialogChoice> choices = new List<DialogChoice>();
    }

    [System.Serializable]
    public class DialogChoice
    {
        public string textRU;
        public string textEN;
        public string textDE;
        public string textFR;
        public string textES;
        public string textPT;
        public string textJA;
        public string textKO;
        public string textZH;
        public int targetStepIndex = -1;
    }

    private List<DialogStep> dialogueSteps = new List<DialogStep>();
    private int currentStepIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string activeFullText = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        BuildDefaultScenario();
    }

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        CalibrateLayout();
    }

    private void CalibrateLayout()
    {
        if (!enforceCoordinates || dialoguePanel == null) return;

        // Позиционирование контейнера выборов строго под панелью диалога
        if (choiceButtonsContainer != null)
        {
            RectTransform rect = choiceButtonsContainer.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0f, -60f); // Свешивается снизу
                rect.sizeDelta = new Vector2(-100f, 50f);
            }
        }
    }

    /// <summary>
    /// Запуск стартовой цепочки диалогов.
    /// </summary>
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
            // Автоматическое мгновенное завершение печати
            isTyping = false;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueBodyText.text = activeFullText;
            ShowChoices(dialogueSteps[currentStepIndex]);
            return;
        }

        DialogStep currentStep = dialogueSteps[currentStepIndex];
        if (currentStep.choices != null && currentStep.choices.Count > 0)
        {
            // Ждем выбора пользователя на кнопках
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

        ClearChoicesUI();

        DialogStep step = dialogueSteps[index];

        // Имя говорящего
        speakerNameText.text = Translator.GetText9(
            step.speakerNameRU, step.speakerNameEN, step.speakerNameEN, step.speakerNameEN,
            step.speakerNameEN, step.speakerNameEN, step.speakerNameEN, step.speakerNameEN, step.speakerNameEN
        );

        // Текст реплики
        activeFullText = Translator.GetText9(
            step.textRU, step.textEN, step.textDE, step.textFR, step.textES, step.textPT, step.textJA, step.textKO, step.textZH
        );

        // Активация портретов
        if (leftSpeakerPortrait != null)
        {
            leftSpeakerPortrait.gameObject.SetActive(step.isLeftSpeaker);
            if (step.isLeftSpeaker)
            {
                leftSpeakerPortrait.sprite = step.customPortrait != null ? step.customPortrait : mentorCatPortrait;
            }
        }

        if (rightSpeakerPortrait != null)
        {
            rightSpeakerPortrait.gameObject.SetActive(!step.isLeftSpeaker);
            if (!step.isLeftSpeaker)
            {
                rightSpeakerPortrait.sprite = step.customPortrait != null ? step.customPortrait : playerCatPortrait;
            }
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextCoroutine(activeFullText, step));
    }

    private IEnumerator TypeTextCoroutine(string text, DialogStep step)
    {
        isTyping = true;
        dialogueBodyText.text = "";

        foreach (char c in text)
        {
            dialogueBodyText.text += c;
            if (textTypeSound != null && SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlaySoundEffect(textTypeSound);
            }
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        ShowChoices(step);
    }

    private void ShowChoices(DialogStep step)
    {
        ClearChoicesUI();

        if (step.choices == null || step.choices.Count == 0 || choiceButtonsContainer == null || choiceButtonPrefab == null) return;

        foreach (var choice in step.choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonsContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = Translator.GetText9(
                    choice.textRU, choice.textEN, choice.textDE, choice.textFR, choice.textES, choice.textPT, choice.textJA, choice.textKO, choice.textZH
                );
            }

            if (btn != null)
            {
                btn.onClick.AddListener(() => OnChoiceSelected(choice));
            }
        }
    }

    private void OnChoiceSelected(DialogChoice choice)
    {
        if (choiceSelectedSound != null && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlaySoundEffect(choiceSelectedSound);
        }

        if (choice.targetStepIndex >= 0)
        {
            currentStepIndex = choice.targetStepIndex;
            DisplayStep(currentStepIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ClearChoicesUI()
    {
        if (choiceButtonsContainer == null) return;
        foreach (Transform child in choiceButtonsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearChoicesUI();
        Debug.Log("[ALCHEMIST DIALOGUE] Диалог успешно завершен.");
    }

    private void BuildDefaultScenario()
    {
        dialogueSteps.Clear();

        // Шаг 0: Наставник приветствует
        DialogStep step0 = new DialogStep
        {
            speakerNameRU = "Кот-Наставник",
            speakerNameEN = "Mentor Cat",
            textRU = "Приветствую тебя, юный Кот-Алхимик! Готов ли ты познать тайны варки великих зелий?",
            textEN = "Welcome, young Alchemist Cat! Are you ready to learn the secrets of brewing great potions?",
            textDE = "Willkommen, junge Alchemist Cat! Bist du bereit, die Geheimnisse des Brauens großer Tränke zu lernen?",
            textFR = "Bienvenue, jeune Chat Alchimiste! Es-tu prêt à apprendre les secrets du brassage de grandes potions?",
            textES = "¡Bienvenido, joven Gato Alquimista! ¿Estás listo para aprender los secretos de la elaboración de grandes pociones?",
            textPT = "Bem-vindo, jovem Gato Alquimista! Você está pronto para aprender os segredos de fazer grandes poções?",
            textJA = "ようこそ、若い錬金術師の猫！偉大なポーションを調合する秘密を学ぶ準備はできていますか？",
            textKO = "환영하네, 꼬마 연금술사 고양이! 위대한 포션 조합법의 비밀을 배울 준비가 되었나?",
            textZH = "欢迎，年轻的炼金猫！你准备好学习炼制伟大药水的秘密了吗？",
            isLeftSpeaker = true
        };

        // Шаг 1: Ответ ученика
        DialogStep step1 = new DialogStep
        {
            speakerNameRU = "Кот-Ученик",
            speakerNameEN = "Cat Apprentice",
            textRU = "Да, учитель! Я наточил когти и приготовил котел.",
            textEN = "Yes, master! I have sharpened my claws and prepared the cauldron.",
            textDE = "Ja, Meister! Ich habe meine Krallen geschärft und den Kessel vorbereitet.",
            textFR = "Oui, maître! J'ai aiguisé mes griffes et préparé le chaudron.",
            textES = "¡Sí, maestro! He afilado mis garras y preparado el caldero.",
            textPT = "Sim, mestre! Eu afiei minhas garras e preparei o caldeirão.",
            textJA = "はい、マスター！爪を研ぎ、大釜を準備しました。",
            textKO = "네, 스승님! 발톱을 갈고 가마솥을 준비했습니다.",
            textZH = "是的，师父！我已经磨好了爪子，准备好了炼金釜。",
            isLeftSpeaker = false
        };

        // Шаг 2: Наставник дает выбор
        DialogStep step2 = new DialogStep
        {
            speakerNameRU = "Кот-Наставник",
            speakerNameEN = "Mentor Cat",
            textRU = "Какое зелье мы попробуем сварить в первую очередь?",
            textEN = "Which potion shall we try to brew first?",
            textDE = "Welchen Trank wollen wir zuerst brauen?",
            textFR = "Quelle potion allons-nous essayer de brasser en premier?",
            textES = "¿Qué poción intentaremos preparar primero?",
            textPT = "Qual poção devemos tentar fazer primeiro?",
            textJA = "最初にどのポーションを調合してみますか？",
            textKO = "어떤 포션을 먼저 만들어 보겠는가?",
            textZH = "我们首先尝试炼制哪种药水？",
            isLeftSpeaker = true
        };

        // Шаг 3: Выбор Зелья Скорости
        DialogStep step3 = new DialogStep
        {
            speakerNameRU = "Кот-Наставник",
            speakerNameEN = "Mentor Cat",
            textRU = "Отлично! Зелье Скорости требует быстрых лапок. Подготовь 2 хвоста летучей мыши и 1 корень одуванчика.",
            textEN = "Excellent! The Speed Potion requires swift paws. Prepare 2 bat tails and 1 dandelion root.",
            textDE = "Hervorragend! Der Schnelligkeitstrank erfordert flinke Pfoten. Bereite 2 Fledermausschwänze und 1 Löwenzahnwurzel vor.",
            textFR = "Excellent! La Potion de Vitesse nécessite des pattes agiles. Prépare 2 queues de chauve-souris et 1 racine de pissenlit.",
            textES = "¡Excelente! La Poción de Velocidad requiere patas veloces. Prepara 2 colas de murciélago y 1 raíz de diente de león.",
            textPT = "Excelente! A Poção de Velocidade requer patas rápidas. Prepare 2 caudas de morcego e 1 raiz de dente-de-leão.",
            textJA = "素晴らしい！スピードポーションには素早い肉球が必要です。コウモリの尻尾2つとタンポポの根1つを用意してください。",
            textKO = "훌륭하군! 신속의 물약은 빠른 발놀림이 필요하지. 박쥐 꼬리 2개와 민들레 뿌리 1개를 준비하게.",
            textZH = "极好！速度药水需要敏捷的猫爪。准备2个蝙蝠尾巴和1个蒲公英根。",
            isLeftSpeaker = true
        };

        // Шаг 4: Выбор Зелья Силы
        DialogStep step4 = new DialogStep
        {
            speakerNameRU = "Кот-Наставник",
            speakerNameEN = "Mentor Cat",
            textRU = "Мощный выбор! Зелье Силы увеличивает мускулы. Принеси 1 коготь тигра и 3 светящихся гриба.",
            textEN = "A powerful choice! The Strength Potion boosts muscles. Bring 1 tiger claw and 3 glowing mushrooms.",
            textDE = "Eine kraftvolle Wahl! Der Stärketrank stärkt die Muskeln. Bring 1 Tigerkralle und 3 leuchtende Pilze.",
            textFR = "Un choix puissant! La Potion de Force renforce les muscles. Apporte 1 griffe de tigre et 3 champignons luminescents.",
            textES = "¡Una opción poderosa! La Poción de Fuerza aumenta los músculos. Trae 1 garra de tigre y 3 hongos brillantes.",
            textPT = "Uma escolha poderosa! A Poção de Força aumenta os músculos. Traga 1 garra de tigre e 3 cogumelos brilhantes.",
            textJA = "力強い選択です！ストレングスポーションは筋肉を強化します。虎の爪1つと光るキノコ3つを持ってきてください。",
            textKO = "강력한 선택이군! 힘의 물약은 근육을 강화시키지. 호랑이 발톱 1개와 빛나는 버섯 3개를 가져오게.",
            textZH = "充满力量的选择！力量药水可以增强肌肉。带来1个老虎爪子和3个发光蘑菇。",
            isLeftSpeaker = true
        };

        // Привязываем выборы к Шагу 2
        step2.choices.Add(new DialogChoice
        {
            textRU = "Зелье Скорости",
            textEN = "Speed Potion",
            textDE = "Schnelligkeitstrank",
            textFR = "Potion de Vitesse",
            textES = "Poción de Velocidad",
            textPT = "Poção de Velocidade",
            textJA = "スピードポーション",
            textKO = "신속의 물약",
            textZH = "速度药水",
            targetStepIndex = 3
        });

        step2.choices.Add(new DialogChoice
        {
            textRU = "Зелье Силы",
            textEN = "Strength Potion",
            textDE = "Stärketrank",
            textFR = "Potion de Force",
            textES = "Poción de Fuerza",
            textPT = "Poção de Força",
            textJA = "ストレングスポーション",
            textKO = "힘의 물약",
            textZH = "力量药水",
            targetStepIndex = 4
        });

        dialogueSteps.Add(step0);
        dialogueSteps.Add(step1);
        dialogueSteps.Add(step2);
        dialogueSteps.Add(step3);
        dialogueSteps.Add(step4);
    }
}
