using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

            this.startButton = newInstance.startButton;
            this.settingsButton = newInstance.settingsButton;
            this.exitButton = newInstance.exitButton;
            this.settingsBackButton = newInstance.settingsBackButton;

            this.startButtonPadding = newInstance.startButtonPadding;
            this.settingsButtonPadding = newInstance.settingsButtonPadding;
            this.exitButtonPadding = newInstance.exitButtonPadding;
            this.backButtonPadding = newInstance.backButtonPadding;

            this.dayBackgroundImage = newInstance.dayBackgroundImage;
            this.nightBackgroundImage = newInstance.nightBackgroundImage;
            this.autoCycleBackgrounds = newInstance.autoCycleBackgrounds;
            this.dayNightCycleSpeed = newInstance.dayNightCycleSpeed;
            this.dayNightBlendFactor = newInstance.dayNightBlendFactor;
            this.cycleType = newInstance.cycleType;

            this.gameTitleText = newInstance.gameTitleText;
            this.backgroundLayer = newInstance.backgroundLayer;
            this.parallaxStrength = newInstance.parallaxStrength;

            SetupListeners();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ALCHEMIST MENU ERROR] Ошибка при автоматическом переносе ссылок: {ex}");
        }
    }

    public enum DayNightCycleType
    {
        AutomaticPingPong, // Бесконечный цикл туда-обратно (как сейчас)
        RealTimeClock,     // Привязка к часам реального компьютера (00:00 - пик ночи 1.0, 12:00 - пик дня 0.0)
        Manual             // Полностью ручное управление (например, через ползунок или внешние скрипты)
    }

    [Header("Панели Меню")]
    [Tooltip("Основная панель главного меню")]
    public GameObject mainMenuPanel;
    [Tooltip("Панель настроек")]
    public GameObject settingsPanel;

    [Header("Кнопки Меню")]
    [Tooltip("Кнопка запуска/продолжения игры (Играть)")]
    public Button startButton;
    [Tooltip("Кнопка открытия настроек")]
    public Button settingsButton;
    [Tooltip("Кнопка выхода")]
    public Button exitButton;
    [Tooltip("Кнопка возврата из настроек в главное меню (Назад)")]
    public Button settingsBackButton;

    [Header("Ограничение Клик-Зоны (Raycast Padding)")]
    [Tooltip("Отступы внутрь для клик-зоны Кнопки ИГРАТЬ (X=Слева, Y=Снизу, Z=Справа, W=Сверху)")]
    public Vector4 startButtonPadding = new Vector4(0f, 0f, 0f, 0f);
    [Tooltip("Отступы внутрь для клик-зоны Кнопки НАСТРОЙКИ (X=Слева, Y=Снизу, Z=Справа, W=Сверху)")]
    public Vector4 settingsButtonPadding = new Vector4(0f, 0f, 0f, 0f);
    [Tooltip("Отступы внутрь для клик-зоны Кнопки ВЫХОД (X=Слева, Y=Снизу, Z=Справа, W=Сверху)")]
    public Vector4 exitButtonPadding = new Vector4(0f, 0f, 0f, 0f);
    [Tooltip("Отступы внутрь для клик-зоны Кнопки НАЗАД (X=Слева, Y=Снизу, Z=Справа, W=Сверху)")]
    public Vector4 backButtonPadding = new Vector4(0f, 0f, 0f, 0f);

    [Header("Настройки Дня и Ночи (Day/Night Blending)")]
    [Tooltip("Режим смены дня и ночи:\n- AutomaticPingPong: Плавное качание туда-обратно\n- RealTimeClock: Привязка к реальному времени компьютера\n- Manual: Смена происходит вручную (из инспектора или внешних скриптов)")]
    public DayNightCycleType cycleType = DayNightCycleType.AutomaticPingPong;
    [Tooltip("Картинка Дневного Фона (Day Background Image)")]
    public Image dayBackgroundImage;
    [Tooltip("Картинка Ночного Фона (Night Background Image)")]
    public Image nightBackgroundImage;
    [Tooltip("Включить автоматическую плавную смену суток в меню (Устаревшее, используйте cycleType)")]
    public bool autoCycleBackgrounds = true;
    [Tooltip("Скорость перехода (чем выше, тем быстрее меняются день и ночь)")]
    public float dayNightCycleSpeed = 0.15f;
    [Tooltip("Ручное смешивание (0 - чистый день, 1 - чистая ночь)")]
    [Range(0f, 1f)]
    public float dayNightBlendFactor = 0f;

    [Header("Элементы Анимации и Параллакса")]
    [Tooltip("Объект названия игры (для эффекта парения)")]
    public RectTransform gameTitleText;
    [Tooltip("Слой заднего фона для параллакса")]
    public RectTransform backgroundLayer;
    [Tooltip("Сила параллакса")]
    public float parallaxStrength = 20f;
    [Tooltip("Скорость плавного парения заголовка")]
    public float titleAnimSpeed = 3f;

    private Vector2 bgStartPos;
    private float titleTimer = 0f;
    private float titleStartY = 0f;
    private bool cycleDirectionUp = true;

    private void Start()
    {
        // Поддержка совместимости со старыми сценами
        if (!autoCycleBackgrounds && cycleType == DayNightCycleType.AutomaticPingPong)
        {
            cycleType = DayNightCycleType.Manual;
        }

        if (backgroundLayer != null)
        {
            bgStartPos = backgroundLayer.anchoredPosition;
        }

        if (gameTitleText != null)
        {
            titleStartY = gameTitleText.anchoredPosition.y;
        }

        UpdateBackgroundBlending();
        SetupListeners();
        ShowPanel(mainMenuPanel);

        // Автоматически запускаем музыку меню через SettingsManager
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayThemeForActiveScene();
        }
    }

    private void Update()
    {
        // 1. Анимация парения заголовка (Легкое дыхание)
        if (gameTitleText != null)
        {
            titleTimer += Time.deltaTime * titleAnimSpeed;
            float offset = Mathf.Sin(titleTimer) * 12f;
            gameTitleText.anchoredPosition = new Vector2(gameTitleText.anchoredPosition.x, titleStartY + offset);
        }

        // 2. Интерактивный Параллакс фона за счет наклона мыши
        if (backgroundLayer != null)
        {
            Vector2 mousePos = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
            }
            else
            {
                mousePos = Input.mousePosition;
            }
#else
            mousePos = Input.mousePosition;
#endif

            float normX = (mousePos.x / Screen.width) - 0.5f;
            float normY = (mousePos.y / Screen.height) - 0.5f;

            Vector2 targetPos = bgStartPos + new Vector2(normX * parallaxStrength, normY * parallaxStrength);
            backgroundLayer.anchoredPosition = Vector2.Lerp(backgroundLayer.anchoredPosition, targetPos, Time.deltaTime * 5f);
        }

        // 3. Плавный цикл смены дня и ночи в зависимости от выбранного режима
        if (cycleType == DayNightCycleType.AutomaticPingPong)
        {
            if (cycleDirectionUp)
            {
                dayNightBlendFactor += Time.deltaTime * dayNightCycleSpeed;
                if (dayNightBlendFactor >= 1f)
                {
                    dayNightBlendFactor = 1f;
                    cycleDirectionUp = false;
                }
            }
            else
            {
                dayNightBlendFactor -= Time.deltaTime * dayNightCycleSpeed;
                if (dayNightBlendFactor <= 0f)
                {
                    dayNightBlendFactor = 0f;
                    cycleDirectionUp = true;
                }
            }
        }
        else if (cycleType == DayNightCycleType.RealTimeClock)
        {
            // Получаем часы и минуты реального компьютера
            System.DateTime now = System.DateTime.Now;
            float hour = (float)now.Hour + (float)now.Minute / 60f; // Отрезок от 0 до 24
            
            // Формула плавной гармонической волны:
            // В 12:00 -> cos(PI) = -1.0 -> dayNightBlendFactor = 0.0 (Чистый день)
            // В 00:00 -> cos(0)  = 1.0  -> dayNightBlendFactor = 1.0 (Чистая ночь)
            // В 06:00 -> cos(PI/2) = 0.0 -> dayNightBlendFactor = 0.5 (Рассвет / Сумерки)
            // В 18:00 -> cos(3PI/2)= 0.0 -> dayNightBlendFactor = 0.5 (Закат / Полумрак)
            float angle = (hour / 24f) * 2f * Mathf.PI;
            dayNightBlendFactor = (Mathf.Cos(angle) + 1f) / 2f;
        }
        // Если выбран режим DayNightCycleType.Manual, мы ничего не делаем автоматически.
        // Значение dayNightBlendFactor полностью контролируется вручную в инспекторе или из других скриптов.

        UpdateBackgroundBlending();
    }

    /// <summary>
    /// Обновляет прозрачность дневного и ночного слоев на основе dayNightBlendFactor (0 = чистый день, 1 = чистая ночь)
    /// </summary>
    public void UpdateBackgroundBlending()
    {
        if (dayBackgroundImage != null)
        {
            Color c = dayBackgroundImage.color;
            // Дневной фон плавно затухает от 1 до 0
            c.a = 1f - dayNightBlendFactor;
            dayBackgroundImage.color = c;
        }

        if (nightBackgroundImage != null)
        {
            Color c = nightBackgroundImage.color;
            // Ночной фон плавно проявляется от 0 до 1
            c.a = dayNightBlendFactor;
            nightBackgroundImage.color = c;
        }
    }

    private void SetupListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartPressed);
            SetAlphaHitThreshold(startButton, 0.5f);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsPressed);
            SetAlphaHitThreshold(settingsButton, 0.5f);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitPressed);
            SetAlphaHitThreshold(exitButton, 0.5f);
        }

        // Автоматический поиск кнопки Назад в панели настроек, если она не задана вручную
        if (settingsBackButton == null && settingsPanel != null)
        {
            Button[] buttons = settingsPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string nameLower = b.name.ToLower();
                if (nameLower.Contains("back") || nameLower.Contains("назад") || nameLower.Contains("close") || nameLower.Contains("return"))
                {
                    settingsBackButton = b;
                    break;
                }
            }
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.RemoveAllListeners();
            settingsBackButton.onClick.AddListener(OnBackPressed);
            SetAlphaHitThreshold(settingsBackButton, 0.5f);
        }

        // Применяем отступы кликабельной зоны (Raycast Padding)
        ApplyRaycastPadding(startButton, startButtonPadding);
        ApplyRaycastPadding(settingsButton, settingsButtonPadding);
        ApplyRaycastPadding(exitButton, exitButtonPadding);
        ApplyRaycastPadding(settingsBackButton, backButtonPadding);
    }

    private void ApplyRaycastPadding(Button button, Vector4 padding)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            try
            {
                img.raycastPadding = padding;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ALCHEMIST MENU] Не удалось установить raycastPadding для {button.name}: {ex.Message}");
            }
        }
    }

    private void SetAlphaHitThreshold(Button button, float threshold)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null && img.sprite != null)
        {
            // Пропускаем стандартные спрайты Unity, чтобы избежать ошибки в консоли
            string spriteName = img.sprite.name;
            if (spriteName == "UISprite" || spriteName == "Background" || spriteName == "Knob" || spriteName == "Checkmark" || spriteName == "InputPen")
            {
                return;
            }

            try
            {
                img.alphaHitTestMinimumThreshold = threshold;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ALCHEMIST MENU] Не удалось установить alphaHitTestMinimumThreshold для {button.name}. " +
                                 $"Убедитесь, что в настройках импорта текстуры '{img.sprite.texture.name}' включена галочка 'Read/Write' в Unity Inspector! Ошибка: {ex.Message}");
            }
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(panel == mainMenuPanel);
        if (settingsPanel != null) settingsPanel.SetActive(panel == settingsPanel);
    }

    public void OnStartPressed()
    {
        // Автоматическая загрузка или старт новой игры в единственный слот 0
        if (PlayerPrefs.HasKey("Alchemist_Slot_Used_0"))
        {
            SaveGameSystem.Load(0);
        }
        else
        {
            SaveGameSystem.DeleteSave(0);
            SaveGameSystem.CurrentData = new SaveGameSystem.SaveData();
            SaveGameSystem.CurrentData.saveName = Translator.GetText9(
                "Кот-Алхимик", "Alchemist Cat", "Alchemist Cat", "Chat Alchimiste", "Gato Alquimista", "Gato Alquimista", "錬金術師の猫", "연금술사 고양이", "炼金猫"
            );
            SaveGameSystem.Save(0);
        }
        // Запуск сцены лаборатории (Индекс 1)
        SceneManager.LoadScene(1);
    }

    public void OnSettingsPressed()
    {
        ShowPanel(settingsPanel);
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.BindUIElements();
        }
    }

    public void OnBackPressed()
    {
        ShowPanel(mainMenuPanel);
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayClickSound();
        }
    }

    public void OnExitPressed()
    {
        Debug.Log("[ALCHEMIST MENU] Выход из игры...");
        Application.Quit();
    }
}
