using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu_Game : MonoBehaviour
{
    public static Menu_Game Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Раскомментируйте строку ниже, если ваш Menu_Game должен выживать между сценами:
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log("[FATE CORE] Найден дубликат Menu_Game. Передаем ссылки на кнопки новому интерфейсу и уничтожаем дубликат.");
            
            // АВТОМАТИЧЕСКОЕ СПАСЕНИЕ ССЫЛОК:
            // Если выживший синглтон остался в сцене, а мы вернулись обратно — 
            // новый дубликат передаст свои свежие ссылки на кнопки из новой сцены старому инстансу.
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

    // Метод переноса свежих ссылок при перезапуске или возврате в сцену меню
    public void TransferNewReferences(Menu_Game newInstance)
    {
        try
        {
            Debug.Log("[FATE CORE] Перенос UI ссылок в выживший Синглтон...");
            
            // Копируем ссылки на панели
            this.mainMenuPanel = newInstance.mainMenuPanel;
            this.settingsPanel = newInstance.settingsPanel;
            this.gameTitle = newInstance.gameTitle;
            this.newGameOrLoadChoicePanel = newInstance.newGameOrLoadChoicePanel;
            this.loadGameSlotsPanel = newInstance.loadGameSlotsPanel;
            this.startBackgroundPanel = newInstance.startBackgroundPanel;
            this.newGameConfirmPanel = newInstance.newGameConfirmPanel;

            // Копируем ссылки на кнопки
            this.startButton = newInstance.startButton;
            this.settingsButton = newInstance.settingsButton;
            this.exitButton = newInstance.exitButton;
            
            this.newGameButton = newInstance.newGameButton;
            this.loadGameChoiceButton = newInstance.loadGameChoiceButton;
            this.backToMainMenuButton = newInstance.backToMainMenuButton;
            
            this.confirmYesButton = newInstance.confirmYesButton;
            this.confirmNoButton = newInstance.confirmNoButton;

            this.characterSelectionSceneName = newInstance.characterSelectionSceneName;
            this.characterSelectionSceneIndex = newInstance.characterSelectionSceneIndex;
            this.loadByName = newInstance.loadByName;

            // Инициализируем заново слушатели событий для свежих кнопок на сцене!
            this.SetupListeners();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE CORE ERROR] Критическая ошибка при автоматическом переносе ссылок: {ex}");
        }
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

    [Header("Настройки сцены")]
    [Tooltip("Название сцены выбора персонажа, если загружаем по имени")]
    public string characterSelectionSceneName = "CharacterSelection";
    [Tooltip("Индекс сцены выбора персонажа, если загружаем по индексу (по умолчанию 1)")]
    public int characterSelectionSceneIndex = 1;
    [Tooltip("Включите это свойство, чтобы загружать сцену по Имени вместо Индекса")]
    public bool loadByName = false;

    void Start()
    {
        SetupListeners();
    }

    // Слушатели событий теперь настраиваются безопасно из одного метода
    public void SetupListeners()
    {
        try
        {
            // Очищаем старые подписки (RemoveAllListeners) во избежание дублирования вызовов кликов
            if (startButton != null) { startButton.onClick.RemoveAllListeners(); startButton.onClick.AddListener(OnStartButtonClicked); }
            if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(OnClickSettingsButton); }
            if (exitButton != null) { exitButton.onClick.RemoveAllListeners(); exitButton.onClick.AddListener(OnClickExitButton); }
            
            // Кнопки во вспомогательной панели выбора
            if (newGameButton != null) { newGameButton.onClick.RemoveAllListeners(); newGameButton.onClick.AddListener(OnClickNewGameButton); }
            if (loadGameChoiceButton != null) { loadGameChoiceButton.onClick.RemoveAllListeners(); loadGameChoiceButton.onClick.AddListener(OnClickLoadGameChoices); }
            if (backToMainMenuButton != null) { backToMainMenuButton.onClick.RemoveAllListeners(); backToMainMenuButton.onClick.AddListener(ShowMainMenu); }

            // Кнопки подтверждения
            if (confirmYesButton != null) { confirmYesButton.onClick.RemoveAllListeners(); confirmYesButton.onClick.AddListener(OnConfirmNewGameYes); }
            if (confirmNoButton != null) { confirmNoButton.onClick.RemoveAllListeners(); confirmNoButton.onClick.AddListener(OnConfirmNewGameNo); }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE CORE ERROR] Ошибка при привязке кнопок в Menu_Game.SetupListeners: {ex}");
        }

        try
        {
            // Автонастройка обратной кнопки настроек и альтернативных кнопок выхода в панели настроек
            if (settingsPanel != null)
            {
                Button[] buttons = settingsPanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn != null)
                    {
                        string nameLower = btn.name.ToLower();
                        if (btn.name == "Btn_BackSettings" || 
                            nameLower.Contains("back") || 
                            nameLower.Contains("return") || 
                            nameLower.Contains("назад") || 
                            nameLower.Contains("выход") ||
                            nameLower.Contains("arrow") ||
                            nameLower.Contains("streл"))
                        {
                            btn.onClick.RemoveAllListeners();
                            btn.onClick.AddListener(ShowMainMenu);
                            Debug.Log($"[FATE CORE] Автоматически настроена кнопка настроек НАЗАД: {btn.name}!");
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE CORE ERROR] Ошибка при автоматическом сопоставлении кнопок возврата настроек: {ex}");
        }

        try
        {
            // Настройка кнопок слотов и их обратных слушателей
            UpdateSlotButtons();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE CORE ERROR] Ошибка при обновлении слотов сохранения: {ex}");
        }

        try
        {
            ShowMainMenu();
            Debug.Log("[FATE CORE] Состояние панелей сброшено. Активно Главное Меню.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FATE CORE ERROR] Ошибка при вызове ShowMainMenu: {ex}");
        }
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameTitle != null) gameTitle.SetActive(true); 
        if (startBackgroundPanel != null) startBackgroundPanel.SetActive(false);
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameTitle != null) gameTitle.SetActive(false); 
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
            OnConfirmNewGameYes();
        }
    }

    public void OnConfirmNewGameYes()
    {
        Debug.Log($"[FATE CORE] Новая игра ПОДТВЕРЖДЕНА. Сброс игровых данных и загрузка сцены выбора героя {(loadByName ? characterSelectionSceneName : characterSelectionSceneIndex.ToString())}.");
        SafeResetSaveData(); 
        
        if (loadByName)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneName);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneName);
            }
        }
        else
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(characterSelectionSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(characterSelectionSceneIndex);
            }
        }
    }

    public void OnConfirmNewGameNo()
    {
        Debug.Log("[FATE CORE] Отмена новой игры. Возврат в меню выбора.");
        OnStartButtonClicked(); 
    }

    public void OnClickLoadGameChoices()
    {
        Debug.Log("[FATE CORE] Кнопка ЗАГРУЗИТЬ нажата. Переключаем на панель слотов.");
        HideAllPanels();
        
        if (loadGameSlotsPanel != null) 
        {
            loadGameSlotsPanel.SetActive(true);
            if (startBackgroundPanel != null) startBackgroundPanel.SetActive(true);
            UpdateSlotButtons(); 
        }
        else 
        {
            Debug.LogError("[FATE CORE] ОШИБКА: loadGameSlotsPanel НЕ НАЗНАЧЕН в Menu_Game!");
        }
    }

    public void OnClickBackFromLoad()
    {
        Debug.Log("[FATE CORE] Назад из меню слотов. Показываем панель выбора.");
        OnStartButtonClicked(); 
    }

    public void UpdateSlotButtons()
    {
        if (loadGameSlotsPanel == null) return;

        Button[] buttons = loadGameSlotsPanel.GetComponentsInChildren<Button>(true);
        int slotIndex = 0;
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;

            if (btn.name == "Btn_BackLoad" || btn.name == "Btn_BackSlots" || btn.name.ToLower().Contains("back"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickBackFromLoad);
                Debug.Log("[FATE CORE] Кнопка КЛЮЧ_НАЗАД автоматически привязана: " + btn.name);
                continue;
            }

            if (slotIndex < 4)
            {
                int currentSlot = slotIndex; 

                TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    bool hasSave = PlayerPrefs.HasKey("Save_Slot_" + currentSlot);
                    if (currentSlot == 3) // Стыковка с автосохранением
                    {
                        if (hasSave)
                        {
                            string saveInfo = PlayerPrefs.GetString("Save_Slot_" + currentSlot + "_Info", "Autosave");
                            txt.text = GetAutosaveLabel() + " - " + saveInfo;
                        }
                        else
                        {
                            txt.text = GetAutosaveLabel() + " " + GetTranslation(27, "(Empty)");
                        }
                    }
                    else
                    {
                        if (hasSave)
                        {
                            string saveInfo = PlayerPrefs.GetString("Save_Slot_" + currentSlot + "_Info", "Saved Game");
                            txt.text = GetTranslation(24, "Slot ") + (currentSlot + 1) + " - " + saveInfo; 
                        }
                        else
                        {
                            txt.text = GetTranslation(24, "Slot ") + (currentSlot + 1) + " " + GetTranslation(27, "(Empty)"); 
                        }
                    }
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSlotClicked(currentSlot));

                slotIndex++;
            }
        }
    }

    private string GetAutosaveLabel()
    {
        switch (GetLanguageID())
        {
            case 0: return "Автосохранение";
            case 2: return "Auto-Speichern";
            case 3: return "Sauvegarde Auto";
            case 4: return "Guardado Automático";
            case 5: return "Salvamento Automático";
            case 6: return "オートセーブ";
            case 7: return "자동 저장";
            case 8: return "自动保存";
            case 1:
            default: return "Auto-Save";
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (PlayerPrefs.HasKey("Save_Slot_" + slotIndex))
        {
            Debug.Log("[FATE CORE] Загрузка игры из сохраненного слота " + slotIndex);
            
            // Фиксируем активный слот для нашей HUD системы
            PlayerPrefs.SetInt("Active_Save_Slot", slotIndex);
            PlayerPrefs.Save();
            
            bool loadSuccess = SafeLoadSaveData(slotIndex);
            if (!loadSuccess)
            {
                Debug.LogError("[FATE CORE] Ошибка при чтении или десериализации файла сохранения!");
            }
        }
        else
        {
            Debug.LogWarning("[FATE CORE] Попытка загрузки: Слот " + slotIndex + " пуст.");
        }
    }

    public void OnBackToMainMenu()
    {
        ShowMainMenu();
    }

    // --- Вспомогательные безопасные методы рефлексии (Zenith Decoupling Pattern) ---

    private string GetTranslation(int textKey, string fallback)
    {
        System.Type translatorType = System.Type.GetType("Translator");
        if (translatorType != null)
        {
            var method = translatorType.GetMethod("GetText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                return (string)method.Invoke(null, new object[] { textKey });
            }
        }
        return fallback;
    }

    private int GetLanguageID()
    {
        System.Type translatorType = System.Type.GetType("Translator");
        if (translatorType != null)
        {
            var prop = translatorType.GetProperty("LanguageID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop != null)
            {
                return (int)prop.GetValue(null);
            }
        }
        return 0; // Default Russian
    }

    private void SafeResetSaveData()
    {
        System.Type saveSystemType = System.Type.GetType("SaveGameSystem");
        if (saveSystemType != null)
        {
            var method = saveSystemType.GetMethod("ResetData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, null);
                return;
            }
        }
        Debug.LogWarning("[FATE CORE] SaveGameSystem.ResetData не найден в текущем контексте сборки.");
    }

    private bool SafeLoadSaveData(int slotIndex)
    {
        System.Type saveSystemType = System.Type.GetType("SaveGameSystem");
        if (saveSystemType != null)
        {
            // Пытаемся получить метод Load(int, bool)
            var method = saveSystemType.GetMethod("Load", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(int), typeof(bool) }, null);
            if (method != null)
            {
                return (bool)method.Invoke(null, new object[] { slotIndex, true });
            }
            
            // Если такой перегрузки нет, пробуем Load(int)
            method = saveSystemType.GetMethod("Load", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(int) }, null);
            if (method != null)
            {
                return (bool)method.Invoke(null, new object[] { slotIndex });
            }
        }
        Debug.LogWarning("[FATE CORE] SaveGameSystem.Load не найден в текущем контексте сборки.");
        return false;
    }
}
