using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Менеджер интеграции Яндекс Игр (Yandex Games SDK) для показа рекламы и начисления бонусов.
/// </summary>
public class YandexAdsManager : MonoBehaviour
{
    public static YandexAdsManager Instance { get; private set; }

    [Header("Настройки")]
    public bool isTestMode = true;

    // Импортируем JavaScript функции для связи с Yandex SDK в WebGL
    [DllImport("__Internal")]
    private static extern void ShowYandexRewarded();

    [DllImport("__Internal")]
    private static extern void ShowYandexInterstitial();

    private Action rewardedSuccessCallback;
    private Action rewardedCloseCallback;
    private Action interstitialCloseCallback;

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

    /// <summary>
    /// Вызов показа вознаграждаемой рекламы (Rewarded Video).
    /// </summary>
    public void ShowRewarded(Action onSuccess, Action onClose = null)
    {
        rewardedSuccessCallback = onSuccess;
        rewardedCloseCallback = onClose;

        Debug.Log("[YANDEX ADS] Запрос на показ Rewarded видео.");

#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isTestMode)
        {
            try
            {
                ShowYandexRewarded();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[YANDEX ADS] Ошибка вызова JS SDK: {ex}");
                SimulateRewardedSuccess(); // Резервный запуск
            }
        }
        else
        {
            SimulateRewardedSuccess();
        }
#else
        SimulateRewardedSuccess();
#endif
    }

    /// <summary>
    /// Вызов показа межстраничной рекламы (Interstitial).
    /// </summary>
    public void ShowInterstitial(Action onClose = null)
    {
        interstitialCloseCallback = onClose;

        Debug.Log("[YANDEX ADS] Запрос на показ Interstitial рекламы.");

#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isTestMode)
        {
            try
            {
                ShowYandexInterstitial();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[YANDEX ADS] Ошибка вызова JS SDK: {ex}");
                SimulateInterstitialClose();
            }
        }
        else
        {
            SimulateInterstitialClose();
        }
#else
        SimulateInterstitialClose();
#endif
    }

    #region JS Обратные вызовы (Web -> Unity)
    // Эти методы вызываются из index.html / плагина Yandex SDK JS
    public void OnRewardedSuccess()
    {
        Debug.Log("[YANDEX ADS] Видео просмотрено! Начисляем награду.");
        rewardedSuccessCallback?.Invoke();
        rewardedSuccessCallback = null;
    }

    public void OnRewardedClosed()
    {
        Debug.Log("[YANDEX ADS] Реклама Rewarded закрыта.");
        rewardedCloseCallback?.Invoke();
        rewardedCloseCallback = null;
    }

    public void OnInterstitialClosed()
    {
        Debug.Log("[YANDEX ADS] Межстраничная реклама закрыта.");
        interstitialCloseCallback?.Invoke();
        interstitialCloseCallback = null;
    }
    #endregion

    #region Симулятор для Редактора / Тестов
    private void SimulateRewardedSuccess()
    {
        Debug.Log("[YANDEX ADS] Имитация успешного просмотра рекламы (Тестовый режим).");
        // Начисляем в симуляторе +500 золота
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(500);
        }
        OnRewardedSuccess();
        OnRewardedClosed();
    }

    private void SimulateInterstitialClose()
    {
        Debug.Log("[YANDEX ADS] Имитация закрытия межстраничной рекламы.");
        OnInterstitialClosed();
    }
    #endregion
}
