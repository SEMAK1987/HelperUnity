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
    public GameObject newGameOrLoadChoicePanel; 
    public GameObject loadGameSlotsPanel; 
    public GameObject startBackgroundPanel; 

    [Header("UI элементы панели Выбора (Choice_Menu)")]
    public Button newGameButton;
    public Button loadGameChoiceButton;
    public Button backToMainMenuButton; 

    void Start()
    {
        // Привязка событий кнопкам
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnClickSettingsButton);
        if (exitButton != null) exitButton.onClick.AddListener(OnClickExitButton);
        
        // Кнопки в панели выбора
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameConfirmed);
        if (loadGameChoiceButton != null) loadGameChoiceButton.onClick.AddListener(OnClickLoadGameChoices);
        if (backToMainMenuButton != null) backToMainMenuButton.onClick.AddListener(ShowMainMenu);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (startBackgroundPanel != null) startBackgroundPanel.SetActive(false);
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (newGameOrLoadChoicePanel != null) newGameOrLoadChoicePanel.SetActive(false);
        if (loadGameSlotsPanel != null) loadGameSlotsPanel.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        HideAllPanels();
        if (newGameOrLoadChoicePanel != null) newGameOrLoadChoicePanel.SetActive(true);
        if (startBackgroundPanel != null) startBackgroundPanel.SetActive(true); 
    }

    public void OnClickSettingsButton()
    {
        HideAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }

    public void OnNewGameConfirmed()
    {
        Debug.Log("[FATE CORE] Запуск новой игры...");
        GameObject gm = GameObject.Find("_GameManager");
        if (gm != null) gm.SendMessage("StartNewGame", SendMessageOptions.DontRequireReceiver);
    }

    public void OnClickLoadGameChoices()
    {
        HideAllPanels();
        if (loadGameSlotsPanel != null) loadGameSlotsPanel.SetActive(true);
    }

    public void OnBackToMainMenu()
    {
        ShowMainMenu();
    }
}
