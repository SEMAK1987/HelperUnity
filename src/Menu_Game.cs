using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu_Game : MonoBehaviour
{
    public static Menu_Game Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    [Header("Кнопки Главного Меню")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Панели Главного Меню")]
    public GameObject mainMenuPanel; 
    public GameObject settingsPanel; 
    public GameObject gameTitle; // ОБЪЕКТ С НАЗВАНИЕМ ИГРЫ
    public GameObject newGameOrLoadChoicePanel; 
    public GameObject loadGameSlotsPanel; 
    public GameObject startBackgroundPanel; 
    public GameObject newGameConfirmPanel; 

    [Header("Кнопки Выбора и Подтверждения")]
    public Button newGameButton;
    public Button loadGameChoiceButton;
    public Button backToMainMenuButton; 
    public Button confirmYesButton;
    public Button confirmNoButton;

    void Start()
    {
        // Привязка событий кнопкам
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnClickSettingsButton);
        if (exitButton != null) exitButton.onClick.AddListener(OnClickExitButton);
        
        // Кнопки в панели выбора
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClickNewGameButton);
        if (loadGameChoiceButton != null) loadGameChoiceButton.onClick.AddListener(OnClickLoadGameChoices);
        if (backToMainMenuButton != null) backToMainMenuButton.onClick.AddListener(ShowMainMenu);

        // Кнопки подтверждения
        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmNewGameYes);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnConfirmNewGameNo);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true); // Показываем логотип
        if (startBackgroundPanel != null) startBackgroundPanel.SetActive(false);
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameTitle != null) gameTitle.SetActive(false); // Скрываем логотип
        if (newGameOrLoadChoicePanel != null) newGameOrLoadChoicePanel.SetActive(false);
        if (loadGameSlotsPanel != null) loadGameSlotsPanel.SetActive(false);
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("[FATE CORE] Нажата кнопка СТАРТ. Показываем панель выбора.");
        HideAllPanels();
        if (newGameOrLoadChoicePanel != null) newGameOrLoadChoicePanel.SetActive(true);
        if (startBackgroundPanel != null) startBackgroundPanel.SetActive(true); 
    }

    public void OnClickSettingsButton()
    {
        Debug.Log("[FATE CORE] Нажата кнопка НАСТРОЙКИ.");
        HideAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnClickExitButton()
    {
        Debug.Log("[FATE CORE] Выход...");
        Application.Quit();
    }

    public void OnClickNewGameButton()
    {
        Debug.Log("[FATE CORE] Нажата Новая Игра. Показываем подтверждение.");
        if (newGameConfirmPanel != null)
        {
            HideAllPanels();
            newGameConfirmPanel.SetActive(true);
            if (startBackgroundPanel != null) startBackgroundPanel.SetActive(true);
        }
        else
        {
            // Если панель не назначена, запускаем сразу
            OnConfirmNewGameYes();
        }
    }

    public void OnConfirmNewGameYes()
    {
        Debug.Log("[FATE CORE] Новая игра ПОДТВЕРЖДЕНА. Загрузка сцены 1 (Выбор героя).");
        SceneManager.LoadScene(1);
    }

    public void OnConfirmNewGameNo()
    {
        Debug.Log("[FATE CORE] Отмена новой игры. Возврат в меню выбора.");
        OnStartButtonClicked(); // Это вернет нас к панели выбора
    }

    public void OnClickLoadGameChoices()
    {
        Debug.Log("[FATE CORE] Кнопка ЗАГРУЗИТЬ нажата. Переключаем на панель слотов.");
        HideAllPanels();
        
        if (loadGameSlotsPanel != null) 
        {
            loadGameSlotsPanel.SetActive(true);
            if (startBackgroundPanel != null) startBackgroundPanel.SetActive(true);
        }
        else 
        {
            Debug.LogError("[FATE CORE] ОШИБКА: loadGameSlotsPanel НЕ НАЗНАЧЕН в Menu_Game!");
        }
    }

    public void OnBackToMainMenu()
    {
        ShowMainMenu();
    }
}
