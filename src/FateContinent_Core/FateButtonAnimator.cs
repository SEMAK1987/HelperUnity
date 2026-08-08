using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FateButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;

    [Header("Параметры масштабирования")]
    [Tooltip("Во сколько раз увеличивается кнопка при наведении мыши")]
    public float hoverScaleMultiplier = 1.08f;
    [Tooltip("Во сколько раз сжимается кнопка в момент клика")]
    public float clickScaleMultiplier = 0.93f;
    [Tooltip("Скорость плавного перехода (сглаживание Lerp)")]
    public float animationSpeed = 16f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
            targetScale = originalScale;
        }
    }

    private void Update()
    {
        if (rectTransform != null)
        {
            // Плавное интерполирование размеров к целевому результату
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }
    }

    // Событие: Курсор мыши зашел в область кнопки
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScaleMultiplier;
        PlaySoundSafe("UI_Hover_Soft");
    }

    // Событие: Курсор мыши покинул область видимости кнопки
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    // Событие: Игрок нажал на кнопку
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScaleMultiplier;
        PlaySoundSafe("UI_Click_Metallic");
    }

    // Событие: Игрок отпустил кнопку
    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScaleMultiplier;
    }

    /// <summary>
    /// Безопасное звуковое воздействие через SettingsManager без прямых жестких связей
    /// </summary>
    private void PlaySoundSafe(string sfxName)
    {
        System.Type settingsType = System.Type.GetType("SettingsManager");
        if (settingsType == null)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                settingsType = assembly.GetType("SettingsManager");
                if (settingsType != null) break;
            }
        }

        if (settingsType != null)
        {
            var instanceProperty = settingsType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    var playMethod = settingsType.GetMethod("PlaySFX", new System.Type[] { typeof(string) });
                    if (playMethod != null)
                    {
                        playMethod.Invoke(instance, new object[] { sfxName });
                        return;
                    }

                    var playSfxMethod = settingsType.GetMethod("PlaySfx", new System.Type[] { typeof(string) });
                    if (playSfxMethod != null)
                    {
                        playSfxMethod.Invoke(instance, new object[] { sfxName });
                        return;
                    }
                }
            }
        }
    }
}
