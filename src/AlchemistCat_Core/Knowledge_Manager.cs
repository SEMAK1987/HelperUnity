using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core v18.12.43)
/// Менеджер Окна 'Знания' (Древо Рангов и Прокачки Алхимического Мастерства):
/// - 4 Этапа и 21 Ранг мастерства
/// - Иконка сложенных книг (Knowledge_Icon_Button) слева от Сундука
/// - Полноценный ScrollView с отображением текущего прогресса, выполненных и закрытых рангов
/// </summary>
public class Knowledge_Manager : MonoBehaviour
{
    public static Knowledge_Manager Instance { get; private set; }

    [Header("UI Панель Знаний и Рангов")]
    public GameObject knowledgePanel;
    public Button knowledgeCloseButton;
    public ScrollRect knowledgeScrollView;
    public Transform knowledgeContent;
    public bool isKnowledgeCompleted = false;

    [Header("Иконка Книг Знаний в верхнем UI")]
    public GameObject knowledgeIconButton; // Иконка сложенных книг слева от сундука
    public Button knowledgeButton;
    public bool autoAlignKnowledgeToChest = true;
    public Vector2 knowledgeOffsetFromChest = new Vector2(-110f, 0f);

    [Header("Звуки")]
    public AudioClip openKnowledgeSound;
    public AudioClip closeSound;
    public AudioClip unlockSound;

    [System.Serializable]
    public class AlchemyRankInfo
    {
        public int rankIndex;
        public string stageNameRU;
        public string rankNameRU;
        public string rankNameEN;
        public string rankNameTR;
        public int requiredMasteryExp;
        public Color rankTextColor = Color.white;
    }

    [Header("Список 21 Рангов Мастерства (4 Этапа)")]
    public List<AlchemyRankInfo> allRanks = new List<AlchemyRankInfo>();

    private void Awake()
    {
        Instance = this;

        if (knowledgeCloseButton != null)
        {
            knowledgeCloseButton.onClick.RemoveAllListeners();
            knowledgeCloseButton.onClick.AddListener(CloseKnowledgeUI);
        }

        if (knowledgeButton != null)
        {
            knowledgeButton.onClick.RemoveAllListeners();
            knowledgeButton.onClick.AddListener(OnKnowledgeButtonClicked);
        }

        if (knowledgeScrollView != null)
        {
            knowledgeScrollView.onValueChanged.RemoveAllListeners();
            knowledgeScrollView.onValueChanged.AddListener(OnScrollValueChanged);
        }

        InitDefaultRanks();
    }

    private void Start()
    {
        if (knowledgePanel != null) knowledgePanel.SetActive(false);
        if (knowledgeIconButton != null) knowledgeIconButton.SetActive(false);
    }

    public void InitDefaultRanks()
    {
        if (allRanks.Count > 0) return;

        // Этап I: Ученик (Ранги 1–6)
        AddRank(1, "Этап I: Ученик — Основы и базовые экстракты", "Новичок", "Novice", "Acemi", 100, Color.white);
        AddRank(2, "Этап I: Ученик — Основы и базовые экстракты", "Новичок-травник", "Herbalist Novice", "Bitkici Acemi", 300, new Color(0.32f, 0.75f, 0.50f, 1f));
        AddRank(3, "Этап I: Ученик — Основы и базовые экстракты", "Подмастерье угля", "Coal Apprentice", "Komur Ciragi", 500, new Color(0.40f, 0.80f, 0.60f, 1f));
        AddRank(4, "Этап I: Ученик — Основы и базовые экстракты", "Экстрактор", "Extractor", "Ekstraktor", 1000, new Color(0.45f, 0.85f, 0.70f, 1f));
        AddRank(5, "Этап I: Ученик — Основы и базовые экстракты", "Знаток пропорций", "Proportion Master", "Oran Ustasi", 1500, new Color(0.50f, 0.90f, 0.80f, 1f));
        AddRank(6, "Этап I: Ученик — Основы и базовые экстракты", "Сертифицированный ученик", "Certified Apprentice", "Sertifikali Cirak", 3000, new Color(0.55f, 0.95f, 0.90f, 1f));

        // Этап II: Адепт (Ранги 7–11)
        AddRank(7, "Этап II: Адепт — Нестабильные субстанции и катализаторы", "Практик масел", "Oil Practitioner", "Yag Uygulayicisi", 5000, new Color(0.30f, 0.70f, 1f, 1f));
        AddRank(8, "Этап II: Адепт — Нестабильные субстанции и катализаторы", "Дистиллятор", "Distiller", "Damitici", 7000, new Color(0.35f, 0.75f, 1f, 1f));
        AddRank(9, "Этап II: Адепт — Нестабильные субстанции и катализаторы", "Мастер ферментации", "Fermentation Master", "Mayalama Ustasi", 10000, new Color(0.40f, 0.80f, 1f, 1f));
        AddRank(10, "Этап II: Адепт — Нестабильные субстанции и катализаторы", "Каталитический химик", "Catalytic Chemist", "Katalitik Kimyager", 15000, new Color(0.50f, 0.85f, 1f, 1f));
        AddRank(11, "Этап II: Адепт — Нестабильные субстанции и катализаторы", "Старший фармацевт", "Senior Pharmacist", "Kidemli Eczaci", 20000, new Color(0.60f, 0.90f, 1f, 1f));

        // Этап III: Магистр (Ранги 12–16)
        AddRank(12, "Этап III: Магистр — Эфир, пустота и кристаллы", "Эфирный экспериментатор", "Aether Experimenter", "Eter Deneycisi", 25000, new Color(0.80f, 0.50f, 1f, 1f));
        AddRank(13, "Этап III: Магистр — Эфир, пустота и кристаллы", "Кристаллограф", "Crystallographer", "Kristalograft", 30000, new Color(0.85f, 0.55f, 1f, 1f));
        AddRank(14, "Этап III: Магистр — Эфир, пустота и кристаллы", "Мастер трансмутации", "Transmutation Master", "Donusum Ustasi", 37000, new Color(0.90f, 0.60f, 1f, 1f));
        AddRank(15, "Этап III: Магистр — Эфир, пустота и кристаллы", "Вивисектор сущностей", "Essence Vivisector", "Oz Kasifi", 45000, new Color(0.95f, 0.65f, 1f, 1f));
        AddRank(16, "Этап III: Магистр — Эфир, пустота и кристаллы", "Архимагистр рецептуры", "Archmagister of Formulas", "Formul Basbuyucusu", 60000, new Color(1f, 0.70f, 0.95f, 1f));

        // Этап IV: Великий Алхимик (Ранги 17–21)
        AddRank(17, "Этап IV: Великий Алхимик — Легенды и Первозданная магия", "Повелитель температур", "Lord of Temperatures", "Sicaklik Efendisi", 70000, new Color(1f, 0.80f, 0.30f, 1f));
        AddRank(18, "Этап IV: Великий Алхимик — Легенды и Первозданная магия", "Ткач реальности", "Reality Weaver", "Gerceklik Dokuyucusu", 85000, new Color(1f, 0.85f, 0.35f, 1f));
        AddRank(19, "Этап IV: Великий Алхимик — Легенды и Первозданная магия", "Конструктор душ", "Soul Constructor", "Ruh Yapicisi", 120000, new Color(1f, 0.90f, 0.40f, 1f));
        AddRank(20, "Этап IV: Великий Алхимик — Легенды и Первозданная магия", "Хранитель Первоматерии", "Keeper of Prima Materia", "Ilk Maddenin Bekcisi", 200000, new Color(1f, 0.95f, 0.50f, 1f));
        AddRank(21, "Этап IV: Великий Алхимик — Легенды и Первозданная магия", "Создатель Философского камня", "Creator of Philosopher's Stone", "Felsefe Tasi Yaraticisi", 500000, new Color(1f, 0.98f, 0.60f, 1f));
    }

    private void AddRank(int idx, string stage, string ru, string en, string tr, int exp, Color col)
    {
        allRanks.Add(new AlchemyRankInfo
        {
            rankIndex = idx,
            stageNameRU = stage,
            rankNameRU = ru,
            rankNameEN = en,
            rankNameTR = tr,
            requiredMasteryExp = exp,
            rankTextColor = col
        });
    }

    public void AlignKnowledgeButtonToChest()
    {
        if (!autoAlignKnowledgeToChest) return;
        if (knowledgeIconButton != null)
        {
            // Базовый референс - сундучок, свиток или календарь
            GameObject refObj = null;
            if (DialogueSystem_Manager.Instance != null)
            {
                if (DialogueSystem_Manager.Instance.chestIconButton != null) refObj = DialogueSystem_Manager.Instance.chestIconButton;
                else if (DialogueSystem_Manager.Instance.smallScrollIconButton != null) refObj = DialogueSystem_Manager.Instance.smallScrollIconButton;
                else if (DialogueSystem_Manager.Instance.calendarIconButton != null) refObj = DialogueSystem_Manager.Instance.calendarIconButton;
            }

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
                    knowRect.anchoredPosition = refRect.anchoredPosition + knowledgeOffsetFromChest;
                }
            }

            // Очищаем стандартный текст "Button"
            TextMeshProUGUI[] tmps = knowledgeIconButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps) t.text = "";
            UnityEngine.UI.Text[] texts = knowledgeIconButton.GetComponentsInChildren<UnityEngine.UI.Text>(true);
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

    public void OnKnowledgeButtonClicked()
    {
        if (DialogueSystem_Manager.Instance != null && 
            (DialogueSystem_Manager.Instance.isCraftingInProgress || 
            (DialogueSystem_Manager.Instance.dialoguePanel != null && DialogueSystem_Manager.Instance.dialoguePanel.activeSelf)))
        {
            Debug.Log("[KNOWLEDGE] Знания заблокированы во время диалога Кота.");
            return;
        }

        OpenKnowledgeUI();
    }

    public void OpenKnowledgeUI()
    {
        if (openKnowledgeSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(openKnowledgeSound);

        if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.dialoguePanel != null)
        {
            DialogueSystem_Manager.Instance.dialoguePanel.SetActive(false);
        }

        if (knowledgePanel != null)
        {
            knowledgePanel.SetActive(true);
        }

        // При открытии ставим скролл в самый верх
        if (knowledgeScrollView != null)
        {
            knowledgeScrollView.verticalNormalizedPosition = 1f;
        }

        // Если книги еще не были прочитаны до конца - блокируем крестик закрытия
        if (knowledgeCloseButton != null)
        {
            knowledgeCloseButton.interactable = isKnowledgeCompleted;
        }
    }

    /// <summary>
    /// Проверка прокрутки: когда игрок долистал до самого низа (pos.y <= 0.05f), разблокируем крестик
    /// </summary>
    private void OnScrollValueChanged(Vector2 pos)
    {
        if (isKnowledgeCompleted) return;

        // В Unity ScrollRect verticalNormalizedPosition равен 1 вверху и 0 внизу
        if (pos.y <= 0.08f)
        {
            isKnowledgeCompleted = true;
            if (knowledgeCloseButton != null)
            {
                knowledgeCloseButton.interactable = true;
            }

            if (unlockSound != null && SettingsManager.Instance != null)
                SettingsManager.Instance.PlaySoundEffect(unlockSound);
        }
    }

    public void CloseKnowledgeUI()
    {
        if (closeSound != null && SettingsManager.Instance != null)
            SettingsManager.Instance.PlaySoundEffect(closeSound);

        if (knowledgePanel != null)
        {
            knowledgePanel.SetActive(false);
        }

        // Иконка книг знаний остается заблокированной во время разговора
        SetKnowledgeButtonInteractable(false);

        // Переходим к фазе диалога о Мини-играх (Колесо игр)
        if (DialogueSystem_Manager.Instance != null)
        {
            DialogueSystem_Manager.Instance.StartMinigamesIntroductionDialogue();
        }
    }
}
