using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.7.9
    /// Zenith Self-Healing UI & Ultimate Pause Manager
    /// Управляет паузой (ESC) и системой сохранения во время геймплея.
    /// Автоматически устраняет неверные биндинги текста и кнопок в инспекторе.
    /// Изолирует элементы паузы во время отображения окна подтверждения выхода, исключая наложение интерфейсов.
    /// </summary>
    public class GamePause_Manager : MonoBehaviour
    {
        public static GamePause_Manager Instance { get; private set; }

        [Header("🖥️ Панели Интерфейса")]
        [Tooltip("Основная панель паузы. Если пуста, будет найдена на сцене или сгенерирована.")]
        public GameObject pauseMenuPanel;
        [Tooltip("Панель подтверждения выхода. Если пуста, будет найдена на сцене или сгенерирована.")]
        public GameObject confirmExitPanel;

        [Header("🎛️ Кнопки Меню Паузы")]
        public Button saveSlot1Button;
        public Button saveSlot2Button;
        public Button saveSlot3Button;
        public Button autosaveButton;
        public Button exitToMenuButton;
        public Button resumeGameButton;

        [Header("⚠️ Кнопки выбора подтверждения (Изолированные)")]
        public Button confirmYesButton;
        public Button confirmNoButton;

        [Header("📝 Текстовые компоненты")]
        public TextMeshProUGUI pauseTitleText;
        public TextMeshProUGUI confirmPromptText;
        public TextMeshProUGUI toastNotificationText;

        [Header("⚙️ Настройки Навигации")]
        [Tooltip("Имя сцены главного меню для выхода")]
        public string mainMenuSceneName = "MainMenu";
        [Tooltip("Индекс сцены главного меню (по умолчанию 0)")]
        public int mainMenuSceneIndex = 0;
        [Tooltip("Включите, чтобы осуществлять переходы по имени, а не индексу")]
        public bool exitByName = true;

        private bool isPaused = false;
        private bool wasDialogueActiveBeforePause = false;
        private int lastLanguageID = -1;
        private Canvas createdCanvas; // Временный Canvas для рантайм-меню

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
        }

        private void Start()
        {
            // СИСТЕМА ДИНАМИЧЕСКОГО САМОЛЕЧЕНИЯ И АВТОПОИСКА (Zenith Auto-Discovery System)
            if (pauseMenuPanel == null)
            {
                var foundPause = GameObject.Find("PausePanel");
                if (foundPause == null) foundPause = GameObject.Find("Pause_Panel");
                if (foundPause != null)
                {
                    pauseMenuPanel = foundPause;
                    Debug.Log($"[FATE SELF-HEAL] Автоматически обнаружена существующая панель паузы в сцене: '{foundPause.name}'");
                }
            }

            if (confirmExitPanel == null)
            {
                var foundConfirm = GameObject.Find("ConfirmPanel");
                if (foundConfirm == null) foundConfirm = GameObject.Find("Confirm_Exit_Panel");
                if (foundConfirm == null) foundConfirm = GameObject.Find("ConfirmExitPanel");
                if (foundConfirm != null)
                {
                    confirmExitPanel = foundConfirm;
                    Debug.Log($"[FATE SELF-HEAL] Автоматически обнаружена панель подтверждения выхода: '{foundConfirm.name}'");
                }
            }

            // Если панелей все еще нет в сцене и инспекторе, генерируем красивую Zenith Glassmorphism панель в рантайме
            if (pauseMenuPanel == null)
            {
                CreateRuntimePauseUI();
            }

            SetupButtonListeners();
            UpdateLocalization();

            // Гарантируем, что игра в начале сцены не стоит на паузе
            ResumeGame();
        }

        private enum InputSystemType { Unchecked, OldInput, NewInput }
        private InputSystemType activeInputType = InputSystemType.Unchecked;

        private bool IsEscapePressed()
        {
            if (activeInputType == InputSystemType.NewInput)
            {
                return CheckNewInputSystemEsc();
            }
            else if (activeInputType == InputSystemType.OldInput)
            {
                try
                {
                    return Input.GetKeyDown(KeyCode.Escape);
                }
                catch (System.InvalidOperationException)
                {
                    activeInputType = InputSystemType.NewInput;
                    return CheckNewInputSystemEsc();
                }
            }

            // First time check: determine active system safely
            try
            {
                bool pressed = Input.GetKeyDown(KeyCode.Escape);
                activeInputType = InputSystemType.OldInput;
                return pressed;
            }
            catch (System.InvalidOperationException)
            {
                activeInputType = InputSystemType.NewInput;
                return CheckNewInputSystemEsc();
            }
        }

        private bool CheckNewInputSystemEsc()
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
                            var escapeKeyProperty = keyboardInstance.GetType().GetProperty("escapeKey");
                            var escapeKeyInstance = escapeKeyProperty?.GetValue(keyboardInstance);
                            if (escapeKeyInstance != null)
                            {
                                var wasPressedProperty = escapeKeyInstance.GetType().GetProperty("wasPressedThisFrame");
                                if (wasPressedProperty != null)
                                {
                                    return (bool)wasPressedProperty.GetValue(escapeKeyInstance);
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

        public bool isPauseBlockedManually = false;

        private void Update()
        {
            // Обработка кнопки ESC с использованием безопасного метода IsEscapePressed
            if (IsEscapePressed())
            {
                if (isPauseBlockedManually)
                {
                    Debug.Log("[FATE PAUSE] ESC заблокирован вручную (блокировка во время важных сцен)!");
                    return;
                }

                // Запрещаем ESC во время активного диалога
                if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
                {
                    Debug.Log("[FATE PAUSE] ESC заблокирован: идет диалог!");
                    return;
                }

                // Проверяем ограничение сцен: "в других сценах в начальной и в сцене битвы что бы она была не активна только в этой сцене"
                string activeScene = SceneManager.GetActiveScene().name.ToLower();
                if (activeScene.Contains("menu") || 
                    activeScene.Contains("selection") || 
                    activeScene.Contains("battle") || 
                    activeScene.Contains("combat") || 
                    activeScene.Contains("fight") || 
                    activeScene.Contains("wastes") || 
                    activeScene.Contains("peak") || 
                    activeScene.Contains("ruins") || 
                    activeScene == "loading")
                {
                    Debug.Log($"[FATE PAUSE] Сцена '{SceneManager.GetActiveScene().name}' запрещена для паузы (битва или меню).");
                    return;
                }

                TogglePause();
            }

            // Динамическая локализация в рантайме при смене языка
            if (lastLanguageID != Translator.LanguageID)
            {
                UpdateLocalization();
            }
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Полная заморозка времени физики и апдейтов

            // Проверяем, активен ли диалог Помощника, и временно гасим его панель
            if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
            {
                wasDialogueActiveBeforePause = true;
                DialogueSystem_Manager.Instance.SetDialoguePanelActive(false);
                Debug.Log("[FATE PAUSE] Обнаружен активный диалог. Панель временно скрыта на время паузы.");
            }
            else
            {
                wasDialogueActiveBeforePause = false;
            }

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            
            // Включаем нормальное меню паузы (скрываем подтверждение выхода и показываем кнопки сохранений)
            SetPauseMenuState(false);

            Debug.Log("[FATE PAUSE] Игра поставлена на ПАУЗУ.");
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Возобновление нормального времени

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
            if (confirmPromptText != null) confirmPromptText.gameObject.SetActive(false);

            // Восстанавливаем диалоговую панель, если она была скрыта перед паузой
            if (wasDialogueActiveBeforePause && DialogueSystem_Manager.Instance != null)
            {
                DialogueSystem_Manager.Instance.SetDialoguePanelActive(true);
                Debug.Log("[FATE PAUSE] Восстановлено отображение диалога с Аэлиссой.");
            }
            wasDialogueActiveBeforePause = false;

            Debug.Log("[FATE PAUSE] Возобновление нормального хода времени.");
        }

        /// <summary>
        /// Zenith State Manager: Контролирует фазы отображения паузы
        /// Если isConfirmMode = true: скрываются кнопки сохранений и возврата в игру, 
        /// отображается только изолированный диалог выхода без наложения графических плашек.
        /// </summary>
        private void SetPauseMenuState(bool isConfirmMode)
        {
            bool showNormalPauseElements = !isConfirmMode;

            // 1. Управляем видимостью кнопок сохранений и возврата
            if (saveSlot1Button != null) saveSlot1Button.gameObject.SetActive(showNormalPauseElements);
            if (saveSlot2Button != null) saveSlot2Button.gameObject.SetActive(showNormalPauseElements);
            if (saveSlot3Button != null) saveSlot3Button.gameObject.SetActive(showNormalPauseElements);
            if (autosaveButton != null) autosaveButton.gameObject.SetActive(showNormalPauseElements);
            if (resumeGameButton != null) resumeGameButton.gameObject.SetActive(showNormalPauseElements);
            if (exitToMenuButton != null) exitToMenuButton.gameObject.SetActive(showNormalPauseElements);

            // Скрываем заголовок паузы во время диалога выхода во избежание визуального наслоения
            if (pauseTitleText != null) pauseTitleText.gameObject.SetActive(showNormalPauseElements);

            // 2. Управляем видимостью предупреждения выхода
            if (confirmPromptText != null)
            {
                confirmPromptText.gameObject.SetActive(isConfirmMode);
            }

            // 3. Управляем видимостью панели подтверждения (ДА/НЕТ кнопки)
            if (confirmExitPanel != null)
            {
                confirmExitPanel.SetActive(isConfirmMode);
            }

            // На всякий случай дублируем скрытие кнопок подтверждения под их непосредственную активность, если они лежат вне панели
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(isConfirmMode);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(isConfirmMode);

            // Скрываем плашку уведомления о сохранении во время выхода
            if (isConfirmMode && toastNotificationText != null)
            {
                toastNotificationText.gameObject.SetActive(false);
            }
        }

        private void SetupButtonListeners()
        {
            // Очищаем старые подписки во избежание дублирования вызовов
            if (saveSlot1Button != null) { saveSlot1Button.onClick.RemoveAllListeners(); saveSlot1Button.onClick.AddListener(() => SaveGame(0)); }
            if (saveSlot2Button != null) { saveSlot2Button.onClick.RemoveAllListeners(); saveSlot2Button.onClick.AddListener(() => SaveGame(1)); }
            if (saveSlot3Button != null) { saveSlot3Button.onClick.RemoveAllListeners(); saveSlot3Button.onClick.AddListener(() => SaveGame(2)); }
            if (autosaveButton != null)  { autosaveButton.onClick.RemoveAllListeners(); autosaveButton.onClick.AddListener(TriggerAutosave); }
            if (resumeGameButton != null) { resumeGameButton.onClick.RemoveAllListeners(); resumeGameButton.onClick.AddListener(ResumeGame); }
            
            if (exitToMenuButton != null)
            {
                exitToMenuButton.onClick.RemoveAllListeners();
                exitToMenuButton.onClick.AddListener(ShowExitConfirmation);
            }

            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.RemoveAllListeners();
                confirmYesButton.onClick.AddListener(OnConfirmExitYes);
            }

            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.RemoveAllListeners();
                confirmNoButton.onClick.AddListener(OnConfirmExitNo);
            }
        }

        public void SaveGame(int slotIndex)
        {
            try
            {
                // Вызываем сохранение через глобальный SaveGameSystem
                SaveGameSystem.Save(slotIndex);

                // Воспроизводим аудио-клик в SettingsManager
                if (SettingsManager.Instance != null)
                {
                    Debug.Log($"[FATE AUDIO] Воспроизводим звук успешного сохранения для слота {slotIndex}");
                }

                ShowToastMessage(string.Format(GetLocalizedToastText(), slotIndex + 1));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FATE PAUSE ERROR] Не удалось сохранить игру в слот {slotIndex}: {ex}");
            }
        }

        public void TriggerAutosave()
        {
            // Автосохранение записывается в 4-й слот (индекс 3)
            try
            {
                SaveGameSystem.Save(3);

                if (SettingsManager.Instance != null)
                {
                    Debug.Log("[FATE AUDIO] Воспроизводим звук автосохранения");
                }

                ShowToastMessage(GetLocalizedAutosaveSuccessText());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FATE PAUSE ERROR] Не удалось выполнить автосохранение: {ex}");
            }
        }

        public void ShowExitConfirmation()
        {
            // Включаем режим подтверждения выхода
            SetPauseMenuState(true);
        }

        public void OnConfirmExitYes()
        {
            Debug.Log("[FATE PAUSE] Выход подтвержден. Возвращаемся в главное меню.");
            Time.timeScale = 1f; // Полностью размораживаем время ПЕРЕД сменой сцены!

            if (exitByName)
            {
                if (LoadingScreenManager.Instance != null)
                {
                    LoadingScreenManager.Instance.LoadScene(mainMenuSceneName);
                }
                else
                {
                    SceneManager.LoadScene(mainMenuSceneName);
                }
            }
            else
            {
                if (LoadingScreenManager.Instance != null)
                {
                    LoadingScreenManager.Instance.LoadScene(mainMenuSceneIndex);
                }
                else
                {
                    SceneManager.LoadScene(mainMenuSceneIndex);
                }
            }
        }

        public void OnConfirmExitNo()
        {
            Debug.Log("[FATE PAUSE] Выход отменен. Возврат на паузу.");
            // Выключаем режим подтверждения выхода и возвращаем элементы меню паузы back
            SetPauseMenuState(false);
        }

        private void ShowToastMessage(string message)
        {
            if (toastNotificationText != null)
            {
                toastNotificationText.text = message;
                StopAllCoroutines();
                StartCoroutine(FadeToastRoutine());
            }
            else
            {
                Debug.Log($"[FATE TOAST] {message}");
            }
        }

        private IEnumerator FadeToastRoutine()
        {
            if (toastNotificationText != null)
            {
                toastNotificationText.gameObject.SetActive(true);
                toastNotificationText.color = new Color(toastNotificationText.color.r, toastNotificationText.color.g, toastNotificationText.color.b, 1f);
                yield return new WaitForSecondsRealtime(2.5f);

                float elapsed = 0f;
                float duration = 0.5f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float alpha = Mathf.Lerp(1, 0, elapsed / duration);
                    toastNotificationText.color = new Color(toastNotificationText.color.r, toastNotificationText.color.g, toastNotificationText.color.b, alpha);
                    yield return null;
                }
                toastNotificationText.gameObject.SetActive(false);
            }
        }

        private void UpdateLocalization()
        {
            lastLanguageID = Translator.LanguageID;

            ApplyFontToText(pauseTitleText, true);
            ApplyFontToText(confirmPromptText, false);
            ApplyFontToText(toastNotificationText, false);

            if (pauseTitleText != null) pauseTitleText.text = GetLocalizedTitleText();
            if (confirmPromptText != null) confirmPromptText.text = GetLocalizedConfirmText();

            // Локализуем кнопки слотов
            TranslateButton(saveSlot1Button, GetLocalizedSaveButtonPrefix() + " 1");
            TranslateButton(saveSlot2Button, GetLocalizedSaveButtonPrefix() + " 2");
            TranslateButton(saveSlot3Button, GetLocalizedSaveButtonPrefix() + " 3");
            TranslateButton(autosaveButton, GetLocalizedAutosaveButtonText());
            TranslateButton(resumeGameButton, GetLocalizedResumeButtonText());
            TranslateButton(exitToMenuButton, GetLocalizedExitButtonText());
            
            TranslateButton(confirmYesButton, GetLocalizedYesText());
            TranslateButton(confirmNoButton, GetLocalizedNoText());
        }

        private void TranslateButton(Button btn, string text)
        {
            if (btn == null) return;
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                ApplyFontToText(txt, true);
                txt.text = text;
            }
        }

        private void ApplyFontToText(TextMeshProUGUI tmpText, bool isHeadline = false)
        {
            if (tmpText == null) return;
            int lang = Translator.LanguageID;
            TMP_FontAsset font = Translator.Instance != null ? Translator.Instance.defaultFont : null;
            float charSpacing = 0f;

            if (Translator.Instance != null)
            {
                if (lang == 7) font = Translator.Instance.koreanFont;
                else if (lang == 8 || lang == 6) font = Translator.Instance.chineseFont;
                else if (lang == 0 && isHeadline) charSpacing = Translator.Instance.russianCharacterSpacing;
            }

            if (font != null) tmpText.font = font;
            tmpText.characterSpacing = charSpacing;
        }

        // ==========================================
        // 📚 9-Языковой Локализационный Массив
        // ==========================================

        private string GetLocalizedTitleText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "ПАУЗА ИГРЫ";
                case 2: return "SPIELPAUSE";
                case 3: return "JEU PAUSÉ";
                case 4: return "JUEGO EN PAUSA";
                case 5: return "JOGO PAUSADO";
                case 6: return "ゲーム一時停止";
                case 7: return "게임 일시정지";
                case 8: return "游戏暂停";
                case 1:
                default: return "GAME PAUSED";
            }
        }

        private string GetLocalizedSaveButtonPrefix()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Сохранить: Слот";
                case 2: return "Speichern: Slot";
                case 3: return "Sauvegarder: Slot";
                case 4: return "Guardar: Ranura";
                case 5: return "Salvar: Slot";
                case 6: return "セーブ: スロット";
                case 7: return "저장: 슬롯";
                case 8: return "保存: 栏位";
                case 1:
                default: return "Save: Slot";
            }
        }

        private string GetLocalizedAutosaveButtonText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Создать автосохранение";
                case 2: return "Auto-Speichern auslösen";
                case 3: return "Lancer Sauvegarde Auto";
                case 4: return "Crear Guardado Automático";
                case 5: return "Criar Salvamento Automático";
                case 6: return "オートセーブを実行";
                case 7: return "자동 저장 실행";
                case 8: return "执行自动保存";
                case 1:
                default: return "Trigger Auto-Save";
            }
        }

        private string GetLocalizedResumeButtonText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "ВЕРНУТЬСЯ В ИГРУ";
                case 2: return "WEITER INTERAKTIV";
                case 3: return "REPRENDRE LE JEU";
                case 4: return "CONTINUAR JUEGO";
                case 5: return "VOLTAR AO JOGO";
                case 6: return "ゲームに戻る";
                case 7: return "게임으로 돌아가기";
                case 8: return "继续游戏";
                case 1:
                default: return "RESUME GAME";
            }
        }

        private string GetLocalizedExitButtonText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "ВЫХОД В ГЛАВНОЕ МЕНЮ";
                case 2: return "ZUM HAUPTMENÜ BEENDEN";
                case 3: return "RETOUR AU MENU PRINCIPAL";
                case 4: return "SALIR AL MENÚ PRINCIPAL";
                case 5: return "SAIR PARA O MENU PRINCIPAL";
                case 6: return "メインメニューに戻る";
                case 7: return "메인 메뉴로 나가기";
                case 8: return "退出至主菜单";
                case 1:
                default: return "EXIT TO MAIN MENU";
            }
        }

        private string GetLocalizedConfirmText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Вы уверены, что хотите выйти? Весь несохраненный прогресс будет утерян.";
                case 2: return "Bist du sicher, dass du beenden willst? Ungespeicherter Fortschritt geht verloren.";
                case 3: return "Êtes-vous sûr de vouloir quitter ? Progression non sauvegarde de l'échec.";
                case 4: return "¿Estás seguro de HTML? El progreso no guardado se perderá.";
                case 5: return "Tem certeza de que deseja sair? O progresso não salvo será perdido.";
                case 6: return "本当に終了しますか？保存されていない進行状況は失われます。";
                case 7: return "정말 종료하시겠습니까? 저장되지 않은 진행 상황은 사라집니다.";
                case 8: return "您确定要退出吗？未保存的进度将会丢失。";
                case 1:
                default: return "Are you sure you want to exit? All unsaved progress will be lost.";
            }
        }

        private string GetLocalizedYesText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "ДА";
                case 2: return "JA";
                case 3: return "OUI";
                case 4: return "SÍ";
                case 5: return "SIM";
                case 6: return "はい";
                case 7: return "예";
                case 8: return "是";
                case 1:
                default: return "YES";
            }
        }

        private string GetLocalizedNoText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "НЕТ";
                case 2: return "NEIN";
                case 3: return "NON";
                case 4: return "NO";
                case 5: return "NÃO";
                case 6: return "いいえ";
                case 7: return "아니오";
                case 8: return "否";
                case 1:
                default: return "NO";
            }
        }

        private string GetLocalizedToastText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Игра успешно сохранена в Слот {0}!";
                case 2: return "Spiel erfolgreich in Slot {0} gespeichert!";
                case 3: return "Jeu sauvegardé avec succès dans l'emplacement {0}!";
                case 4: return "¡Juego guardado con éxito en la ranura {0}!";
                case 5: return "Jogo salvo com sucesso no slot {0}!";
                case 6: return "スロット {0} にセーブしました！";
                case 7: return "{0}번 슬롯에 성공적으로 저장되었습니다!";
                case 8: return "游戏成功保存至栏位 {0}！";
                case 1:
                default: return "Game successfully saved to Slot {0}!";
            }
        }

        private string GetLocalizedAutosaveSuccessText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Автосохранение успешно создано!";
                case 2: return "Auto-Speicherung успешно создана!";
                case 3: return "Sauvegarde automatique créée avec succès!";
                case 4: return "¡Guardado automático creado con éxito!";
                case 5: return "Salvamento automático criado com sucesso!";
                case 6: return "オートセーブを正常に作成しました！";
                case 7: return "자동 저장을 완료했습니다!";
                case 8: return "自动保存已成功创建！";
                case 1:
                default: return "Autosave successfully created!";
            }
        }

        // ==========================================
        // 🔮 Автоматическая генерация UI при запуске
        // ==========================================
        private void CreateRuntimePauseUI()
        {
            Debug.Log("[FATE PAUSE] Нет назначенных панелей в Inspector. Автоматически создаем Zenith Canvas UI...");

            var canvasGov = new GameObject("Fate_Pause_Canvas");
            DontDestroyOnLoad(canvasGov); // Выживает в буфере, пока активна сцена

            var canvas = canvasGov.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1500; // Поверх всего (выше диалогов, которые на 999)

            canvasGov.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGov.AddComponent<GraphicRaycaster>();
            
            createdCanvas = canvas;

            // 1. Фоновый Размытый Оверлей (Zenith Glassmorphic Blackout)
            var blackoutGov = new GameObject("Blackout");
            blackoutGov.transform.SetParent(canvasGov.transform, false);
            var blackoutImg = blackoutGov.AddComponent<Image>();
            blackoutImg.color = new Color(0.02f, 0.03f, 0.08f, 0.85f);
            
            var blackoutRect = blackoutGov.GetComponent<RectTransform>();
            blackoutRect.anchorMin = Vector2.zero;
            blackoutRect.anchorMax = Vector2.one;
            blackoutRect.offsetMin = Vector2.zero;
            blackoutRect.offsetMax = Vector2.zero;

            pauseMenuPanel = blackoutGov;

            // 2. Тело Меню (Окна)
            var menuPanelGov = new GameObject("Pause_Window");
            menuPanelGov.transform.SetParent(blackoutGov.transform, false);
            var menuImg = menuPanelGov.AddComponent<Image>();
            menuImg.color = new Color(0.08f, 0.11f, 0.22f, 0.95f); // Космически-синее стекло
            
            var menuRect = menuPanelGov.GetComponent<RectTransform>();
            menuRect.sizeDelta = new Vector4(450, 520);
            menuRect.anchoredPosition = Vector2.zero;

            // Outline закругленный
            var outline = menuPanelGov.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.5f, 1f, 0.25f);
            outline.effectDistance = new Vector2(2, 2);

            // 3. Заголовок "ПАУЗА ИГРЫ"
            var titleGov = new GameObject("Title_Text");
            titleGov.transform.SetParent(menuPanelGov.transform, false);
            pauseTitleText = titleGov.AddComponent<TextMeshProUGUI>();
            pauseTitleText.text = "ПАУЗА ИГРЫ";
            pauseTitleText.alignment = TextAlignmentOptions.Center;
            pauseTitleText.fontSize = 28f;
            pauseTitleText.fontStyle = FontStyles.Bold | FontStyles.Italic;
            pauseTitleText.color = Color.white;

            var titleRect = titleGov.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 210);
            titleRect.sizeDelta = new Vector2(400, 50);

            // Навешиваем вертикальную разметку кнопкам
            float startY = 120;
            float spacingY = 55;

            // Вспомогательный метод спавна кнопки
            saveSlot1Button = CreateMenuButton(menuPanelGov.transform, "Btn_SaveSlot1", "Сохранить: Слот 1", new Vector2(0, startY));
            saveSlot2Button = CreateMenuButton(menuPanelGov.transform, "Btn_SaveSlot2", "Сохранить: Слот 2", new Vector2(0, startY - spacingY));
            saveSlot3Button = CreateMenuButton(menuPanelGov.transform, "Btn_SaveSlot3", "Сохранить: Слот 3", new Vector2(0, startY - spacingY * 2));
            autosaveButton  = CreateMenuButton(menuPanelGov.transform, "Btn_Autosave",  "Создать автосохранение", new Vector2(0, startY - spacingY * 3), new Color(0.1f, 0.6f, 0.3f, 0.8f));
            resumeGameButton = CreateMenuButton(menuPanelGov.transform, "Btn_Resume",    "ВЕРНУТЬСЯ В ИГРУ", new Vector2(0, startY - spacingY * 4.2f), new Color(0.2f, 0.4f, 1f, 0.9f));
            exitToMenuButton = CreateMenuButton(menuPanelGov.transform, "Btn_ExitToMenu", "ВЫХОД В ГЛАВНОЕ МЕНЮ", new Vector2(0, startY - spacingY * 5.2f), new Color(0.8f, 0.15f, 0.15f, 0.8f));

            // 4. Текст Оповещений Тлеющий (Toast)
            var toastGov = new GameObject("Toast_Text");
            toastGov.transform.SetParent(menuPanelGov.transform, false);
            toastNotificationText = toastGov.AddComponent<TextMeshProUGUI>();
            toastNotificationText.text = "";
            toastNotificationText.alignment = TextAlignmentOptions.Center;
            toastNotificationText.fontSize = 13f;
            toastNotificationText.color = new Color(1f, 0.85f, 0f, 1f); // Золотой
            
            var toastRect = toastGov.GetComponent<RectTransform>();
            toastRect.anchoredPosition = new Vector2(0, -230);
            toastRect.sizeDelta = new Vector2(400, 30);
            toastGov.SetActive(false);

            // 5. ИЗОЛИРОВАННАЯ Панель Выхода
            confirmExitPanel = new GameObject("Confirm_Exit_Panel");
            confirmExitPanel.transform.SetParent(blackoutGov.transform, false);
            var confirmImg = confirmExitPanel.AddComponent<Image>();
            confirmImg.color = new Color(0.04f, 0.05f, 0.1f, 0.98f);
            
            var confirmRect = confirmExitPanel.GetComponent<RectTransform>();
            confirmRect.anchorMin = Vector2.zero;
            confirmRect.anchorMax = Vector2.one;
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;

            var warningWindowGov = new GameObject("Warning_Window");
            warningWindowGov.transform.SetParent(confirmExitPanel.transform, false);
            var warningImg = warningWindowGov.AddComponent<Image>();
            warningImg.color = new Color(0.15f, 0.05f, 0.05f, 0.95f); // Тревожно-красный оттенок
            
            var warningRect = warningWindowGov.GetComponent<RectTransform>();
            warningRect.sizeDelta = new Vector3(480, 240);
            warningRect.anchoredPosition = Vector2.zero;
            
            var warnOutline = warningWindowGov.AddComponent<Outline>();
            warnOutline.effectColor = new Color(1f, 0.2f, 0.2f, 0.35f);
            warnOutline.effectDistance = new Vector2(2, 2);

            // Текст Предупреждения
            var warnTextGov = new GameObject("Warning_Text");
            warnTextGov.transform.SetParent(warningWindowGov.transform, false);
            confirmPromptText = warnTextGov.AddComponent<TextMeshProUGUI>();
            confirmPromptText.text = "Вы уверены, что хотите выйти?";
            confirmPromptText.alignment = TextAlignmentOptions.Center;
            confirmPromptText.fontSize = 18f;
            confirmPromptText.fontStyle = FontStyles.Bold;
            confirmPromptText.color = Color.white;
            
            var warnTextRect = warnTextGov.GetComponent<RectTransform>();
            warnTextRect.anchoredPosition = new Vector2(0, 45);
            warnTextRect.sizeDelta = new Vector2(440, 100);

            // Кнопки Да и Нет (Уникальные коллбэки)
            confirmYesButton = CreateMenuButton(warningWindowGov.transform, "Btn_ConfirmExitYes", "ДА", new Vector2(-110, -50), new Color(0.7f, 0.1f, 0.1f, 0.9f), 180);
            confirmNoButton  = CreateMenuButton(warningWindowGov.transform, "Btn_ConfirmExitNo",  "НЕТ", new Vector2(110, -50), new Color(0.25f, 0.25f, 0.25f, 0.9f), 180);

            confirmExitPanel.SetActive(false); // Выключено по умолчанию
        }

        private Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Color? colColor = null, float width = 360f)
        {
            var btnGov = new GameObject(name);
            btnGov.transform.SetParent(parent, false);
            
            var img = btnGov.AddComponent<Image>();
            img.color = colColor ?? new Color(0.15f, 0.18f, 0.3f, 0.85f);

            var shadow = btnGov.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(1, -1);

            var button = btnGov.AddComponent<Button>();

            var btnRect = btnGov.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(width, 42f);
            btnRect.anchoredPosition = pos;

            // Навешиваем текст на кнопку
            var textGov = new GameObject("Label");
            textGov.transform.SetParent(btnGov.transform, false);
            var textComp = textGov.AddComponent<TextMeshProUGUI>();
            textComp.text = label;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.fontSize = 14f;
            textComp.fontStyle = FontStyles.Bold;
            textComp.color = Color.white;

            var txtRect = textGov.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            // Добавляем красивый ховер эффект
            btnGov.AddComponent<UIButtonPauseHover>();

            return button;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Очищаем созданный в рантайме Canvas
            if (createdCanvas != null)
            {
                Destroy(createdCanvas.gameObject);
            }
        }
    }
}
