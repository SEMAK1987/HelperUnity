using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Автоматический биндер неоновых и алхимических звуков на все кнопки в сцене.
/// Сканирует активную иерархию и добавляет UIButtonHoverEffect.
/// </summary>
public class UIButtonSfxBinder : MonoBehaviour
{
    public static UIButtonSfxBinder Instance { get; private set; }

    [Header("Настройки Биндера")]
    public bool scanOnStart = true;
    public bool scanOnSceneLoaded = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (scanOnSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (scanOnStart)
        {
            ScanAndBindAllButtons();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanAndBindAllButtons();
    }

    /// <summary>
    /// Сканирует всю сцену и вешает компоненты озвучки на кнопки, у которых их еще нет.
    /// </summary>
    public void ScanAndBindAllButtons()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        int boundCount = 0;

        foreach (Button btn in buttons)
        {
            // Пропускаем префабы в ассетах
            if (btn.gameObject.scene.name == null) continue;

            // Проверяем наличие эффекта наведения
            UIButtonHoverEffect effect = btn.GetComponent<UIButtonHoverEffect>();
            if (effect == null)
            {
                // Добавляем скрипт hover эффекта
                effect = btn.gameObject.AddComponent<UIButtonHoverEffect>();
                effect.playSfxOnHover = true;
                effect.playSfxOnClick = true;
                boundCount++;
            }
        }

        Debug.Log($"[ALCHEMIST SFX BINDER] Сканирование завершено. Озвучено новых кнопок: {boundCount}");
    }
}
