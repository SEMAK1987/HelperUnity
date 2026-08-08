using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Главный контроллер меню игры "Алхимический Кот".
/// </summary>
public class Menu_Game : MonoBehaviour
{
    public static Menu_Game Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Instance.TransferNewReferences(this);
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void TransferNewReferences(Menu_Game newInstance)
    {
        try
        {
            this.mainMenuPanel = newInstance.mainMenuPanel;
            this.settingsPanel = newInstance.settingsPanel;
            this.choicePanel = newInstance.choicePanel;
            this.slotsPanel = newInstance.slotsPanel;
            this.confirmPanel = newInstance.confirmPanel;

            this.startButton = newInstance.startButton;
            this.settingsButton = newInstance.settingsButton;
            this.exitButton = newInstance.exitButton;
            this.newGameButton = newInstance.newGameButton;
            this.loadGameButton = newInstance.loadGameButton;
            this.backToMainButton = newInstance.backToMainButton;
            this.confirmYesButton = newInstance.confirmYesButton;
            this.confirmNoButton = newInstance.confirmNoButton;

            this.slotTexts = newInstance.slotTexts;
            this.slotButtons = newInstance.slotButtons;

            SetupListeners();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ALCHEMIST MENU ERROR] Ошибка при автоматическом переносе ссылок: {ex}");
        }
    }

    [Header("Панели Меню")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject choicePanel; // Панель выбора: Новая Игра / Загрузить
    public GameObject slotsPanel;  // Панель со слотами сохранений
    public GameObject confirmPanel; // Подтверждение перезаписи

    [Header("Кнопки Меню")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Кнопки Действий")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button backToMainButton;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Элементы Слотов Сохранения")]
    public TextMeshProUGUI[] slotTexts = new TextMeshProUGUI[3];
    public Button[] slotButtons = new Button[3];

    private int selectedSlot = 0;

    private void Start()
    {
        SetupListeners();
        ShowPanel(mainMenuPanel);
    }

    private void SetupListeners()
    {
        // Сброс старых слушателей для безопасности
        if (startButton != null) { startButton.onClick.RemoveAllListeners(); startButton.onClick.AddListener(OnStartPressed); }
        if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(OnSettingsPressed); }
        if (exitButton != null) { exitButton.onClick.RemoveAllListeners(); exitButton.onClick.AddListener(OnExitPressed); }

        if (newGameButton != null) { newGameButton.onClick.RemoveAllListeners(); newGameButton.onClick.AddListener(OnNewGamePressed); }
        if (loadGameButton != null) { loadGameButton.onClick.RemoveAllListeners(); loadGameButton.onClick.AddListener(OnLoadGamePressed); }
        if (backToMainButton != null) { backToMainButton.onClick.RemoveAllListeners(); backToMainButton.onClick.AddListener(OnBackToMainPressed); }

        if (confirmYesButton != null) { confirmYesButton.onClick.RemoveAllListeners(); confirmYesButton.onClick.AddListener(OnConfirmYesPressed); }
        if (confirmNoButton != null) { confirmNoButton.onClick.RemoveAllListeners(); confirmNoButton.onClick.AddListener(OnConfirmNoPressed); }

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                int index = i;
                slotButtons[index].onClick.RemoveAllListeners();
                slotButtons[index].onClick.AddListener(() => OnSlotClicked(index));
            }
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(panel == mainMenuPanel);
        if (settingsPanel != null) settingsPanel.SetActive(panel == settingsPanel);
        if (choicePanel != null) choicePanel.SetActive(panel == choicePanel);
        if (slotsPanel != null) slotsPanel.SetActive(panel == slotsPanel);
        if (confirmPanel != null) confirmPanel.SetActive(panel == confirmPanel);
    }

    private void OnStartPressed()
    {
        ShowPanel(choicePanel);
    }

    private void OnSettingsPressed()
    {
        ShowPanel(settingsPanel);
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.BindUIElements();
        }
    }

    private void OnExitPressed()
    {
        Debug.Log("[ALCHEMIST MENU] Выход из игры...");
        Application.Quit();
    }

    private void OnNewGamePressed()
    {
        // Открываем слоты для выбора места сохранения новой игры
        UpdateSlotsUI(true);
        ShowPanel(slotsPanel);
    }

    private void OnLoadGamePressed()
    {
        // Открываем слоты для выбора сохранения для загрузки
        UpdateSlotsUI(false);
        ShowPanel(slotsPanel);
    }

    private void OnBackToMainPressed()
    {
        ShowPanel(mainMenuPanel);
    }

    private void UpdateSlotsUI(bool isNewGameMode)
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] == null) continue;

            if (PlayerPrefs.HasKey("Alchemist_Slot_Used_" + i))
            {
                string info = PlayerPrefs.GetString("Alchemist_Slot_Info_" + i);
                slotTexts[i].text = info;
                if (slotButtons[i] != null) slotButtons[i].interactable = true;
            }
            else
            {
                slotTexts[i].text = Translator.GetText9(
                    "(Пусто)", "(Empty)", "(Leer)", "(Vide)", "(Vacío)", "(Vazio)", "(空き)", "(비어있음)", "(空)"
                );
                // В режиме загрузки пустые слоты кликать нельзя
                if (slotButtons[i] != null) slotButtons[i].interactable = isNewGameMode;
            }
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        selectedSlot = slotIndex;

        if (choicePanel.activeSelf && PlayerPrefs.HasKey("Alchemist_Slot_Used_" + slotIndex))
        {
            // Если выбран слот новой игры, но он занят, запрашиваем подтверждение перезаписи
            ShowPanel(confirmPanel);
        }
        else
        {
            ExecuteSlotAction();
        }
    }

    private void OnConfirmYesPressed()
    {
        ExecuteSlotAction();
    }

    private void OnConfirmNoPressed()
    {
        ShowPanel(slotsPanel);
    }

    private void ExecuteSlotAction()
    {
        if (PlayerPrefs.HasKey("Alchemist_Slot_Used_" + selectedSlot) && !confirmPanel.activeSelf)
        {
            // Режим загрузки существующего сейва
            SaveGameSystem.Load(selectedSlot);
        }
        else
        {
            // Режим новой игры: очищаем и создаем новые данные
            SaveGameSystem.DeleteSave(selectedSlot);
            SaveGameSystem.CurrentData = new SaveGameSystem.SaveData();
            SaveGameSystem.CurrentData.saveName = Translator.GetText9(
                "Кот-Алхимик", "Alchemist Cat", "Alchemist Cat", "Chat Alchimiste", "Gato Alquimista", "Gato Alquimista", "錬金術師の猫", "연금술사 고양이", "炼金猫"
            );
            SaveGameSystem.Save(selectedSlot);
            
            // Запуск сцены лаборатории (Индекс 1)
            SceneManager.LoadScene(1);
        }
    }
}
