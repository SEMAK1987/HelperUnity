using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSfxBinder : MonoBehaviour
{
    [Header("Звуковые Клипы")]
    [Tooltip("Стандартный звук клика для обычных кнопок")]
    public AudioClip clickSound;  // Перетащите сюда звук клика (например, ui_click_modern.mp3)

    [Tooltip("Опциональный звук клика для кнопок возврата/назад/закрытия. Если не задан, используется стандартный.")]
    public AudioClip backClickSound; // Перетащите сюда звук назад (например, ui_back_click.mp3)

    public static UIButtonSfxBinder Instance { get; private set; }

    private float scanTimer = 0f;
    private const float SCAN_INTERVAL = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            // Чтобы предотвратить перенос всего Canvas, фонов или других UI-объектов из-за DontDestroyOnLoad(gameObject),
            // мы всегда инициализируем синглтон на чистом, отдельно созданном при старте GameObject.
            if (gameObject.name != "FATE_SFX_BINDER")
            {
                Debug.Log($"[FATE SFX BINDER] Инициализация синглтона на чистом объекте. Защищаем '{gameObject.name}' от переноса при переходе на сцену 1.");
                
                GameObject sfxObject = new GameObject("FATE_SFX_BINDER");
                UIButtonSfxBinder customBinder = sfxObject.AddComponent<UIButtonSfxBinder>();
                
                customBinder.clickSound = this.clickSound;
                customBinder.backClickSound = this.backClickSound;
                
                Instance = customBinder;
                DontDestroyOnLoad(sfxObject);
                
                // Уничтожаем только текущий компонент-заглушку, чтобы сцена и объекты (например, Canvas) остались нетронутыми
                Destroy(this);
                return;
            }

            Instance = this;
            Debug.Log("[FATE SFX BINDER] Глобальный синглтон UIButtonSfxBinder успешно запущен на выделенном объекте.");
        }
        else
        {
            if (Instance != this)
            {
                // Уничтожаем только дублирующий компонент в новых сценах, не ломая исходный UI
                Destroy(this);
            }
        }
    }

    void Start()
    {
        ScanAndBindAllButtons();
    }

    void Update()
    {
        // Каждые 0.5 секунд сканируем сцену на наличие новых или активированных кнопок
        scanTimer += Time.deltaTime;
        if (scanTimer >= SCAN_INTERVAL)
        {
            scanTimer = 0f;
            ScanAndBindAllButtons();
        }
    }

    private void ScanAndBindAllButtons()
    {
        // FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None) находит как активные, так и неактивные кнопки во всей активной сцене!
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;

            // Добавляем наш собственный триггер EventSystem, устойчивый к RemoveAllListeners()
            if (btn.gameObject.GetComponent<ButtonSfxTrigger>() == null)
            {
                btn.gameObject.AddComponent<ButtonSfxTrigger>();
            }
        }
    }
}

// Вспомогательный класс-компонент, перехватывающий клики от EventSystem (игнорирует RemoveAllListeners).
public class ButtonSfxTrigger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Button btn = GetComponent<Button>();
        // Воспроизводим звук только если кнопка кликабельна
        if (btn != null && btn.interactable)
        {
            if (UIButtonSfxBinder.Instance != null && SettingsManager.Instance != null)
            {
                bool isBack = CheckIsBackButton(gameObject);

                if (isBack && UIButtonSfxBinder.Instance.backClickSound != null)
                {
                    SettingsManager.Instance.PlaySoundEffect(UIButtonSfxBinder.Instance.backClickSound);
                    Debug.Log($"[FATE SFX BINDER] Клик по кнопке Назад ({gameObject.name}) -> Звук воспроизведен.");
                }
                else if (UIButtonSfxBinder.Instance.clickSound != null)
                {
                    SettingsManager.Instance.PlaySoundEffect(UIButtonSfxBinder.Instance.clickSound);
                    Debug.Log($"[FATE SFX BINDER] Клик по кнопке ({gameObject.name}) -> Звук воспроизведен.");
                }
            }
        }
    }

    private bool CheckIsBackButton(GameObject go)
    {
        string objName = go.name.ToLower();
        // Первичная проверка по имени объекта
        if (objName.Contains("back") || 
            objName.Contains("exit") || 
            objName.Contains("close") || 
            objName.Contains("return") || 
            objName.Contains("cancel") || 
            objName.Contains("назад") || 
            objName.Contains("выход") ||
            objName.Contains("arrow") ||
            objName.Contains("стрел"))
        {
            return true;
        }

        // Вторичная проверка текста внутри кнопки
        var texts = go.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var txt in texts)
        {
            string t = txt.text.ToLower();
            if (t.Contains("back") || 
                t.Contains("exit") || 
                t.Contains("close") || 
                t.Contains("назад") || 
                t.Contains("выход") || 
                t.Contains("отмена") || 
                t.Contains("return"))
            {
                return true;
            }
        }

        return false;
    }
}
