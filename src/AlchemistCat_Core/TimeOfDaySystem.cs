using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Система смены дня и ночи комнаты Кота-Алхимика.
/// Поддерживает работу по РЕАЛЬНОМУ местному времени игрока (системное время устройства)
/// и мгновенно/плавно переключает дневной и ночной фон.
/// </summary>
public class TimeOfDaySystem : MonoBehaviour
{
    public static TimeOfDaySystem Instance;

    [Header("Cozy Room Day/Night Blending")]
    [Tooltip("Дневной фон комнаты алхимика (яркий день)")]
    public Image dayRoomImage;
    [Tooltip("Ночной фон комнаты алхимика (уютный свет свечей и луны)")]
    public Image nightRoomImage;

    [Header("Настройки реального времени суток")]
    [Tooltip("Использовать реальное время устройства игрока")]
    public bool useRealTime = true;
    [Range(0, 23)]
    [Tooltip("Час начала дня (например, 6 утра)")]
    public int dayStartHour = 6;
    [Range(0, 23)]
    [Tooltip("Час начала ночи (например, 21 вечера)")]
    public int nightStartHour = 21;

    [Header("Плавность перехода")]
    [Tooltip("Длительность плавного фейда между днем и ночью в секундах")]
    public float transitionDuration = 2.5f;

    [Header("Текущее состояние (Debug)")]
    public bool isNight = false;

    [Header("Тестирование / Ручной режим")]
    public bool manualOverride = false;

    private Coroutine blendCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Первичная проверка при запуске
        CheckAndApplyTimeOfDay(instant: true);
        // Запуск периодической проверки раз в минуту
        StartCoroutine(PeriodicTimeCheck());
    }

    private void Update()
    {
        if (manualOverride)
        {
            ApplyBlendDirect(isNight ? 1f : 0f);
        }
    }

    /// <summary>
    /// Проверяет системное время игрока и плавно или мгновенно применяет фон.
    /// </summary>
    public void CheckAndApplyTimeOfDay(bool instant = false)
    {
        if (manualOverride) return;

        if (useRealTime)
        {
            // Получаем местный час из системы устройства игрока
            int currentHour = DateTime.Now.Hour;
            
            // Если час между днем и ночью -> День, иначе -> Ночь
            bool shouldBeNight = false;
            if (dayStartHour < nightStartHour)
            {
                shouldBeNight = (currentHour < dayStartHour || currentHour >= nightStartHour);
            }
            else
            {
                // Нестандартный диапазон через полночь
                shouldBeNight = (currentHour >= nightStartHour && currentHour < dayStartHour);
            }

            SetDayNightState(shouldBeNight, instant);
        }
    }

    private IEnumerator PeriodicTimeCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f); // Проверяем каждые 30 секунд
            if (!manualOverride && useRealTime)
            {
                CheckAndApplyTimeOfDay(instant: false);
            }
        }
    }

    public void SetDayNightState(bool night, bool instant)
    {
        isNight = night;

        if (dayRoomImage != null)
            dayRoomImage.gameObject.SetActive(true);
        if (nightRoomImage != null)
            nightRoomImage.gameObject.SetActive(true);

        float targetNightAlpha = night ? 1f : 0f;

        if (instant)
        {
            ApplyBlendDirect(targetNightAlpha);
        }
        else
        {
            if (blendCoroutine != null) StopCoroutine(blendCoroutine);
            blendCoroutine = StartCoroutine(BlendTransition(targetNightAlpha, transitionDuration));
        }
    }

    private void ApplyBlendDirect(float nightAlpha)
    {
        if (dayRoomImage != null)
        {
            Color dColor = dayRoomImage.color;
            dColor.a = 1f - nightAlpha;
            dayRoomImage.color = dColor;
        }

        if (nightRoomImage != null)
        {
            Color nColor = nightRoomImage.color;
            nColor.a = nightAlpha;
            nightRoomImage.color = nColor;
        }
    }

    private IEnumerator BlendTransition(float targetNightAlpha, float duration)
    {
        float startNightAlpha = nightRoomImage != null ? nightRoomImage.color.a : (isNight ? 0f : 1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentAlpha = Mathf.Lerp(startNightAlpha, targetNightAlpha, t);
            ApplyBlendDirect(currentAlpha);
            yield return null;
        }

        ApplyBlendDirect(targetNightAlpha);
    }
}
