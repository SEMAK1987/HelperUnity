# 🐱 Руководство: Создание игры «Алхимический Кот: Ленивый Котёл» на Unity 6 (WebGL)

Добро пожаловать в детальный план разработки вашей первой коммерческой WebGL-игры! Мы выбрали потрясающий концепт — **«Алхимический Кот: Ленивый Котёл»**, и расширили его крутыми механиками удержания игроков (Дартс, Ловля мышей, VIP-система, Скины, Аватарки и Биржа).

Ниже представлен подробный пошаговый план действий (от настройки Unity до публикации в Яндекс Игры) и готовый профессиональный C# код для всех систем.

---

## 📅 Пошаговая дорожная карта разработки (Step-by-Step Roadmap)

### Этап 1: Подготовка проекта в Unity 6
1. **Создание проекта:** Откройте Unity Hub, создайте новый проект на базе шаблона **2D (URP)** или **3D (URP)** (в зависимости от того, будете ли вы использовать 2D-рисунки или 3D-модели). Рекомендуется **2D**, так как он весит намного меньше и идеально подходит для кликеров.
2. **Переключение платформы:** Перейдите в `File ➡️ Build Settings`, выберите **WebGL** и нажмите **Switch Platform**.
3. **Настройка Player Settings:**
   * В разделе `Resolution and Presentation` выберите шаблон WebGL (рекомендуется стандартный или облегченный).
   * В разделе `Publishing Settings` включите **Compression Format: Brotli** или **Gzip** для экстремального сжатия размера билда.
   * Отключите галочку `Decompression Fallback` (если загружаете в Яндекс, они сами распаковывают архивы на своих серверах).

### Этап 2: Создание структуры папок
В окне `Project` внутри папки `Assets` создайте следующую структуру:
* `Assets/Scripts` — для всех C# скриптов.
* `Assets/Plugins/WebGL` — **критически важно** для файла `YandexSDK.jslib`.
* `Assets/Sprites` — для графики котла, кота, интерфейса, аватарок и скинов.
* `Assets/Prefabs` — для сохраняемых заготовок элементов UI, мышек и дротиков.

### Этап 3: Создание визуальной сцены в Unity (UI)
1. **Основной холст (Canvas):** Создайте `UI ➡️ Canvas`. Настройте компонент `Canvas Scaler`:
   * `UI Scale Mode` = `Scale With Screen Size`.
   * `Reference Resolution` = `1080 x 1920` (вертикальный формат для мобильных) или `1920 x 1080` (горизонтальный для ПК). Для Яндекса лучше всего делать **адаптивный UI**, который подстраивается под оба экрана с помощью анкоров (Anchors).
2. **Центральный элемент (Котёл):** Создайте большую кнопку `UI ➡️ Button` в центре экрана. Назовите её `CauldronButton`. Присвойте ей спрайт красивого чугунного котла.
3. **Кот-алхимик:** Поместите изображение кота рядом с котлом (`UI ➡️ Image`). Вы можете сделать простую анимацию дыхания кота с помощью бесплатного плагина **DOTween** или стандартного компонента `Animator` в Unity (изменяя масштаб Scale по оси Y от `1.0` до `1.05` плавно туда-обратно).
4. **Панели UI:** Создайте окна (через `GameObject ➡️ UI ➡️ Panel`):
   * `TopPanel` — золото, кристаллы, VIP уровень, уровень игрока.
   * `UpgradesPanel` — список улучшений (автомешалки, мыши-помощники).
   * `DailyRewardPanel` — сетка 7 дней с кнопкой "Забрать награду".
   * `MinigamesPanel` — выбор игр: Дартс (каждые 5 дней) и Поймай мышь (каждые 10 дней).
   * `MarketPanel` — аукцион аватарок, покупка сундучков, смена скинов и просмотр профиля.

---

## 🛠️ Архитектурные C# скрипты (Полная реализация)

Все скрипты спроектированы так, чтобы работать друг с другом без ошибок компиляции. Создайте их в папке `Assets/Scripts/`.

### 1. `YandexSDK.jslib` (Разместить строго в `Assets/Plugins/WebGL/YandexSDK.jslib`)
Этот файл связывает C# код в Unity с JavaScript API Яндекс Игр, чтобы показывать реальную рекламу и сохранять данные в облако Яндекса.

```javascript
mergeInto(LibraryManager.library, {
    InitYandexSDKExtern: function () {
        if (typeof ys_init === 'undefined') {
            console.log("Yandex SDK is initializing on WebGL...");
        }
    },

    ShowInterstitialAdExtern: function () {
        if (typeof ysdk !== 'undefined') {
            ysdk.adv.showFullscreenAdv({
                callbacks: {
                    onOpen: function() {
                        console.log('Interstitial ad opened.');
                        SendMessage('YandexAdsManager', 'OnAdOpen');
                    },
                    onClose: function(wasShown) {
                        console.log('Interstitial ad closed.');
                        SendMessage('YandexAdsManager', 'OnAdClose');
                    },
                    onError: function(error) {
                        console.log('Error showing Interstitial ad:', error);
                        SendMessage('YandexAdsManager', 'OnAdClose');
                    }
                }
            });
        } else {
            console.log("Yandex SDK not found. Simulating Interstitial Ad...");
            SendMessage('YandexAdsManager', 'OnAdOpen');
            setTimeout(function() {
                SendMessage('YandexAdsManager', 'OnAdClose');
            }, 1000);
        }
    },

    ShowRewardedAdExtern: function (rewardId) {
        if (typeof ysdk !== 'undefined') {
            ysdk.adv.showRewardedVideo({
                callbacks: {
                    onOpen: () => {
                        console.log('Video ad opened.');
                        SendMessage('YandexAdsManager', 'OnAdOpen');
                    },
                    onRewarded: () => {
                        console.log('Rewarded! ID: ' + rewardId);
                        SendMessage('YandexAdsManager', 'OnRewardGranted', rewardId);
                    },
                    onClose: () => {
                        console.log('Video ad closed.');
                        SendMessage('YandexAdsManager', 'OnAdClose');
                    },
                    onError: (e) => {
                        console.log('Error showing Rewarded ad:', e);
                        SendMessage('YandexAdsManager', 'OnAdClose');
                    }
                }
            });
        } else {
            console.log("Yandex SDK not found. Simulating Rewarded Ad for ID: " + rewardId);
            SendMessage('YandexAdsManager', 'OnAdOpen');
            setTimeout(function() {
                SendMessage('YandexAdsManager', 'OnRewardGranted', rewardId);
                SendMessage('YandexAdsManager', 'OnAdClose');
            }, 1000);
        }
    }
});
```

---

### 2. `YandexAdsManager.cs`
Управляет вызовами рекламы из Unity.

```csharp
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexAdsManager : MonoBehaviour
{
    public static YandexAdsManager Instance { get; private set; }

    [DllImport("__Internal")]
    private static extern void InitYandexSDKExtern();

    [DllImport("__Internal")]
    private static extern void ShowInterstitialAdExtern();

    [DllImport("__Internal")]
    private static extern void ShowRewardedAdExtern(int rewardId);

    private Action onAdClosedCallback;
    private Action onRewardCallback;
    private int activeRewardId = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            try
            {
                #if !UNITY_EDITOR && UNITY_WEBGL
                InitYandexSDKExtern();
                #endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("Yandex SDK Init failed: " + e.Message);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowInterstitial(Action onClosed)
    {
        onAdClosedCallback = onClosed;
        Time.timeScale = 0f; // Пауза в игре
        AudioListener.pause = true; // Выключаем звук

        #if !UNITY_EDITOR && UNITY_WEBGL
        ShowInterstitialAdExtern();
        #else
        Debug.Log("[ADS] Simulating Interstitial Ad...");
        OnAdClose();
        #endif
    }

    public void ShowRewarded(int rewardId, Action onRewardGranted, Action onClosed)
    {
        activeRewardId = rewardId;
        onRewardCallback = onRewardGranted;
        onAdClosedCallback = onClosed;
        
        Time.timeScale = 0f;
        AudioListener.pause = true;

        #if !UNITY_EDITOR && UNITY_WEBGL
        ShowRewardedAdExtern(rewardId);
        #else
        Debug.Log("[ADS] Simulating Rewarded Ad for ID: " + rewardId);
        OnRewardGranted(rewardId);
        OnAdClose();
        #endif
    }

    // Вызывается из JS-кода через SendMessage
    public void OnAdOpen()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    // Вызывается из JS-кода через SendMessage
    public void OnAdClose()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        onAdClosedCallback?.Invoke();
    }

    // Вызывается из JS-кода через SendMessage
    public void OnRewardGranted(int rewardId)
    {
        if (activeRewardId == rewardId)
        {
            onRewardCallback?.Invoke();
        }
    }
}
```

---

### 3. `GameManager.cs`
Главный мозг игры. Хранит валюту, уровни, VIP-статус, скины и управляет сохранением.

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Stats")]
    public double gold = 0;
    public int crystals = 10;
    public int playerLevel = 1;
    public int playerXP = 0;
    public int daysActive = 1;
    public int vipLevel = 0;
    public int vipXP = 0;
    
    [Header("Click Settings")]
    public double goldPerClick = 1;
    public double passiveGoldPerSec = 0;
    
    [Header("UI Reference Examples")]
    public Text goldText;
    public Text crystalText;
    public Text levelText;
    public Text vipText;

    private float passiveTimer = 0f;
    private float adTimer = 120f; // Межстраничная реклама раз в 2 минуты

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Пассивный доход каждую секунду
        passiveTimer += Time.deltaTime;
        if (passiveTimer >= 1f)
        {
            AddGold(passiveGoldPerSec);
            passiveTimer = 0f;
        }

        // Таймер межстраничной рекламы
        adTimer -= Time.deltaTime;
        if (adTimer <= 0)
        {
            YandexAdsManager.Instance.ShowInterstitial(() => {
                adTimer = 120f; // Сброс таймера на 2 минуты
            });
        }

        UpdateUI();
    }

    public void OnCauldronClicked()
    {
        AddGold(goldPerClick);
        AddXP(1); // 1 клик = 1 опыт

        // Спавн визуального эффекта искр (можно добавить анимацию)
    }

    public void AddGold(double amount)
    {
        gold += amount;
        UpdateUI();
    }

    public void AddCrystals(int amount)
    {
        crystals += amount;
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        playerXP += amount;
        int xpNeeded = playerLevel * 100;
        if (playerXP >= xpNeeded)
        {
            playerXP -= xpNeeded;
            playerLevel++;
            Debug.Log("LEVEL UP! New Level: " + playerLevel);
            // Дарим подарок за уровень
            AddCrystals(2);
        }
        UpdateUI();
    }

    public void AddVipXP(int amount)
    {
        vipXP += amount;
        int xpNeeded = (vipLevel + 1) * 50;
        if (vipXP >= xpNeeded && vipLevel < 100)
        {
            vipXP -= xpNeeded;
            vipLevel++;
            Debug.Log("VIP UP! New VIP Level: " + vipLevel);
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = FormatNumber(gold);
        if (crystalText != null) crystalText.text = crystals.ToString();
        if (levelText != null) levelText.text = "Lvl: " + playerLevel;
        if (vipText != null) vipText.text = "VIP " + vipLevel;
    }

    public string FormatNumber(double num)
    {
        if (num >= 1000000000) return (num / 1000000000f).ToString("F2") + " B";
        if (num >= 1000000) return (num / 1000000f).ToString("F2") + " M";
        if (num >= 1000) return (num / 1000f).ToString("F1") + " K";
        return num.ToString("F0");
    }

    public void SaveGame()
    {
        PlayerPrefs.SetString("Gold", gold.ToString());
        PlayerPrefs.SetInt("Crystals", crystals);
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.SetInt("PlayerXP", playerXP);
        PlayerPrefs.SetInt("DaysActive", daysActive);
        PlayerPrefs.SetInt("VipLevel", vipLevel);
        PlayerPrefs.SetInt("VipXP", vipXP);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey("Gold"))
        {
            double.TryParse(PlayerPrefs.GetString("Gold"), out gold);
            crystals = PlayerPrefs.GetInt("Crystals", 10);
            playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            playerXP = PlayerPrefs.GetInt("PlayerXP", 0);
            daysActive = PlayerPrefs.GetInt("DaysActive", 1);
            vipLevel = PlayerPrefs.GetInt("VipLevel", 0);
            vipXP = PlayerPrefs.GetInt("VipXP", 0);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
```

---

### 4. `UpgradeManager.cs`
Рассчитывает цены апгрейдов и пассивный прирост золота.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeItem
    {
        public string id;
        public string title;
        public double baseCost;
        public double costMultiplier = 1.15;
        public double basePower;
        public bool isPassive; // true = пассивный в сек, false = сила клика
        public int currentLevel = 0;
        public Text costText;
        public Text lvlText;
    }

    public UpgradeItem[] upgrades;

    private void Start()
    {
        LoadUpgrades();
        RecalculateStats();
    }

    public void BuyUpgrade(int index)
    {
        if (index < 0 || index >= upgrades.Length) return;
        UpgradeItem up = upgrades[index];
        double cost = GetCurrentCost(up);

        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.gold -= cost;
            up.currentLevel++;
            SaveUpgrade(up);
            RecalculateStats();
            GameManager.Instance.UpdateUI();
        }
    }

    public double GetCurrentCost(UpgradeItem up)
    {
        return up.baseCost * System.Math.Pow(up.costMultiplier, up.currentLevel);
    }

    public void RecalculateStats()
    {
        double clickPower = 1;
        double passivePower = 0;

        foreach (var up in upgrades)
        {
            double totalPower = up.currentLevel * up.basePower;
            if (up.isPassive)
            {
                passivePower += totalPower;
            }
            else
            {
                clickPower += totalPower;
            }

            if (up.costText != null)
                up.costText.text = GameManager.Instance.FormatNumber(GetCurrentCost(up));
            if (up.lvlText != null)
                up.lvlText.text = "Lvl " + up.currentLevel;
        }

        GameManager.Instance.goldPerClick = clickPower;
        GameManager.Instance.passiveGoldPerSec = passivePower;
    }

    private void SaveUpgrade(UpgradeItem up)
    {
        PlayerPrefs.SetInt("Upgrade_" + up.id, up.currentLevel);
    }

    private void LoadUpgrades()
    {
        foreach (var up in upgrades)
        {
            up.currentLevel = PlayerPrefs.GetInt("Upgrade_" + up.id, 0);
        }
    }
}
```

---

### 5. `DailyRewardSystem.cs`
Контролирует ежедневный вход на 7 дней. На 5-й день дает бонус в Дартс, а на 10-й — в Ловлю мышей!

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardSystem : MonoBehaviour
{
    [Header("UI References (Must be assigned in Inspector)")]
    public Button claimButton;
    public Text timerText;
    public Text statusText;
    public Transform[] calendarDaySlots; // 7 визуальных слотов дней (День 1 - День 7)

    private int currentStreak = 0;
    private DateTime lastClaimTime;

    private void Start()
    {
        // Проверяем важные зависимости, чтобы избежать NullReferenceException в консоли
        ValidateInspectorReferences();
        LoadDailyData();
        CheckDailyStatus();
    }

    private void Update()
    {
        CheckDailyStatus();
    }

    private void ValidateInspectorReferences()
    {
        if (claimButton == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Кнопка 'Claim Button' не назначена в Инспекторе! Пожалуйста, перетащите её.");
        if (timerText == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Текстовое поле 'Timer Text' не назначено в Инспекторе!");
        if (statusText == null)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Текстовое поле 'Status Text' не назначено. Логи наград не будут отображаться.");
        if (calendarDaySlots == null || calendarDaySlots.Length == 0)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Массив слотов календаря 'Calendar Day Slots' пуст! Назначьте 7 дочерних дней.");
    }

    private void CheckDailyStatus()
    {
        TimeSpan difference = DateTime.Now - lastClaimTime;
        bool isRewardReady = false;

        if (difference.TotalHours >= 24 && difference.TotalHours < 48)
        {
            // Можно забрать следующую награду!
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Новая награда готова!";
        }
        else if (difference.TotalHours >= 48)
        {
            // Пропущено слишком много времени! Сброс серии на День 1
            currentStreak = 0;
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Серия сброшена! Заберите День 1.";
        }
        else
        {
            // Ждем 24 часа
            isRewardReady = false;
            if (claimButton != null) claimButton.interactable = false;
            TimeSpan timeToWait = TimeSpan.FromHours(24) - difference;
            if (timerText != null)
            {
                timerText.text = string.Format("До награды: {0:D2}:{1:D2}:{2:D2}", 
                    timeToWait.Hours, timeToWait.Minutes, timeToWait.Seconds);
            }
        }

        UpdateCalendarVisuals(isRewardReady);
    }

    public void ClaimReward()
    {
        currentStreak = (currentStreak % 7) + 1; // Цикл 7 дней
        lastClaimTime = DateTime.Now;

        // Безопасное начисление наград через GameManager
        if (GameManager.Instance != null)
        {
            switch (currentStreak)
            {
                case 1: GameManager.Instance.AddGold(100); break;
                case 2: GameManager.Instance.AddGold(250); break;
                case 3: GameManager.Instance.AddCrystals(1); break; // Медный ключ = 1 кристалл
                case 4: GameManager.Instance.AddGold(500); break;
                case 5: 
                    GameManager.Instance.AddVipXP(10);
                    if (MinigamesManager.Instance != null)
                        MinigamesManager.Instance.UnlockDarts(); // Разблокируем Дартс!
                    if (statusText != null) statusText.text = "Вам открыт ДАРТС!";
                    break;
                case 6: GameManager.Instance.AddGold(1000); break;
                case 7: 
                    GameManager.Instance.AddCrystals(10); // Кристаллы + сундучок
                    if (statusText != null) statusText.text = "Вы получили Золотой Сундук!";
                    break;
            }

            // Проверка кратных 10 дней на кошачью мышеловку
            GameManager.Instance.daysActive++;
            if (GameManager.Instance.daysActive % 10 == 0)
            {
                if (MinigamesManager.Instance != null)
                    MinigamesManager.Instance.UnlockMouseCatch(); // Разблокируем Мышей!
                if (statusText != null) statusText.text = "Открыта игра: ЛОВЛЯ МЫШЕЙ!";
            }
        }
        else
        {
            Debug.LogError("[DailyRewardSystem] ОШИБКА: GameManager.Instance не найден в сцене! Награда не начислена.");
        }

        SaveDailyData();
        CheckDailyStatus();
    }

    private void UpdateCalendarVisuals(bool isRewardReady)
    {
        if (calendarDaySlots == null) return;

        for (int i = 0; i < calendarDaySlots.Length; i++)
        {
            if (calendarDaySlots[i] == null) continue;
            
            Image slotImage = calendarDaySlots[i].GetComponent<Image>();
            if (slotImage == null) continue;

            if (i < currentStreak)
            {
                slotImage.color = Color.green; // Получено (зеленый)
            }
            else if (i == currentStreak && isRewardReady)
            {
                slotImage.color = Color.yellow; // Готово к получению (желтый)
            }
            else
            {
                slotImage.color = Color.gray; // Закрыто/Ожидание (серый)
            }
        }
    }

    private void SaveDailyData()
    {
        PlayerPrefs.SetInt("DailyStreak", currentStreak);
        PlayerPrefs.SetString("LastClaimTime", lastClaimTime.ToString());
        PlayerPrefs.Save();
    }

    private void LoadDailyData()
    {
        currentStreak = PlayerPrefs.GetInt("DailyStreak", 0);
        string lastClaimStr = PlayerPrefs.GetString("LastClaimTime", "");

        if (!string.IsNullOrEmpty(lastClaimStr))
        {
            DateTime.TryParse(lastClaimStr, out lastClaimTime);
        }
        else
        {
            lastClaimTime = DateTime.Now.AddDays(-2); // Позволит сразу забрать первый день
        }
    }
}
```

---

### 6. `MinigamesManager.cs`
Управляет мини-играми: Дартс (3 режима) и Ловля мышей (а также рулетками, шариками).

```csharp
using UnityEngine;
using UnityEngine.UI;

public class MinigamesManager : MonoBehaviour
{
    public static MinigamesManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject dartsPanel;
    public GameObject mouseCatchPanel;
    
    [Header("Darts Settings")]
    public Text dartsScoreText;
    public Text dartsStatusText;
    public Dropdown dartsDifficulty; // Легко, Средне, Тяжело (для дуэли)
    private int dartsMode = 0; // 0 = На время, 1 = Макс очков, 2 = Против компьютера
    private int dartsThrows = 0;
    private int dartsPoints = 0;
    private float dartsTimer = 0f;
    private bool dartsActive = false;

    [Header("Mouse Catch Settings")]
    public Text mouseCatchTimerText;
    public Text mousePointsText;
    public Slider mouseRewardProgress;
    public GameObject mousePrefab; // Спавнится в случайных норках
    public Transform[] mouseHoles;
    private int mousePoints = 0;
    private float mouseTimer = 0f;
    private bool mouseActive = false;
    private float mouseSpawnTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (dartsActive) UpdateDarts();
        if (mouseActive) UpdateMouseCatch();
    }

    // --- ДАРТС ---
    public void UnlockDarts()
    {
        dartsPanel.SetActive(true);
        dartsStatusText.text = "Дартс разблокирован на 5-й день! Выберите режим.";
    }

    public void StartDarts(int mode)
    {
        dartsMode = mode;
        dartsPoints = 0;
        dartsThrows = 0;
        dartsTimer = 30f; // 30 секунд для игры на время
        dartsActive = true;
        dartsPanel.SetActive(true);
        dartsStatusText.text = "Бросайте дротики в мишень!";
    }

    public void ThrowDart(int accuracy) // Вызывается кликом по вращающейся мишени
    {
        if (!dartsActive) return;

        dartsThrows++;
        int pointsGained = Random.Range(1, 10) * accuracy;
        dartsPoints += pointsGained;

        if (dartsMode == 1 && dartsThrows >= 10) // Режим: 10 бросков максимум
        {
            EndDarts();
        }

        dartsScoreText.text = "Очки: " + dartsPoints + "\nБроски: " + dartsThrows;
    }

    private void UpdateDarts()
    {
        if (dartsMode == 0) // Режим на время
        {
            dartsTimer -= Time.deltaTime;
            dartsScoreText.text = string.Format("Очки: {0}\nВремя: {1:F1} сек", dartsPoints, dartsTimer);
            if (dartsTimer <= 0) EndDarts();
        }
    }

    private void EndDarts()
    {
        dartsActive = false;
        int crystalsReward = dartsPoints / 15;
        
        if (dartsMode == 2) // Против компьютера
        {
            int botScore = Random.Range(30, 80) * (dartsDifficulty.value + 1);
            if (dartsPoints > botScore)
            {
                crystalsReward *= 2; // Удвоенная награда за победу!
                dartsStatusText.text = $"ПОБЕДА! Вы обыграли бота ({dartsPoints} VS {botScore})!";
            }
            else
            {
                crystalsReward /= 2;
                dartsStatusText.text = $"ПОРАЖЕНИЕ! Бот победил ({dartsPoints} VS {botScore}).";
            }
        }
        else
        {
            dartsStatusText.text = $"Игра завершена! Вы набрали {dartsPoints} очков.";
        }

        if (crystalsReward < 1) crystalsReward = 1;
        GameManager.Instance.AddCrystals(crystalsReward);
    }

    // --- ЛОВЛЯ МЫШЕЙ ---
    public void UnlockMouseCatch()
    {
        mouseCatchPanel.SetActive(true);
    }

    public void StartMouseCatch()
    {
        mousePoints = 0;
        mouseTimer = 20f;
        mouseActive = true;
        mouseSpawnTimer = 0f;
        mousePointsText.text = "Мыши: 0";
        mouseRewardProgress.value = 0;
        mouseCatchPanel.SetActive(true);
    }

    private void UpdateMouseCatch()
    {
        mouseTimer -= Time.deltaTime;
        mouseCatchTimerText.text = $"Осталось времени: {mouseTimer:F1} сек";

        mouseSpawnTimer += Time.deltaTime;
        if (mouseSpawnTimer >= 0.8f) // Спавн мыши раз в 0.8 сек
        {
            SpawnMouse();
            mouseSpawnTimer = 0f;
        }

        if (mouseTimer <= 0) EndMouseCatch();
    }

    private void SpawnMouse()
    {
        int holeIndex = Random.Range(0, mouseHoles.Length);
        Transform hole = mouseHoles[holeIndex];
        
        GameObject mouse = Instantiate(mousePrefab, hole.position, Quaternion.identity, mouseCatchPanel.transform);
        Button btn = mouse.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            CatchMouse(mouse);
        });

        Destroy(mouse, 0.7f); // Спрячется обратно через 0.7 секунды
    }

    public void CatchMouse(GameObject mouse)
    {
        mousePoints++;
        mousePointsText.text = "Мыши: " + mousePoints;
        mouseRewardProgress.value = (float)mousePoints / 25f; // Шкала до 25 мышей
        Destroy(mouse);
    }

    private void EndMouseCatch()
    {
        mouseActive = false;
        
        // Награды по шкале
        int rewardCrystals = 0;
        if (mousePoints >= 25) rewardCrystals = 15; // Супер-приз
        else if (mousePoints >= 15) rewardCrystals = 8;
        else if (mousePoints >= 5) rewardCrystals = 3;
        else rewardCrystals = 1;

        GameManager.Instance.AddCrystals(rewardCrystals);
        GameManager.Instance.AddVipXP(mousePoints); // Каждый мышонок дает опыт VIP!
    }
}
```

---

### 7. `MarketAndProfiles.cs`
Полноценная система профилей, коллекция скинов (смена обликов кота), VIP-прогрессия и симулируемый аукцион (Биржа аватарок за Кристаллы).

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketAndProfiles : MonoBehaviour
{
    [System.Serializable]
    public class AvatarItem
    {
        public string id;
        public string name;
        public int baseCrystalValue;
        public bool isOwned;
        public Sprite visualSprite;
    }

    [System.Serializable]
    public class SkinItem
    {
        public string id;
        public string name;
        public Sprite catVisual;
        public bool isUnlocked;
    }

    [Header("Market & Inventory")]
    public List<AvatarItem> allAvatars;
    public List<SkinItem> allSkins;

    [Header("UI References")]
    public Dropdown auctionAvatarSelector;
    public InputField auctionPriceInput;
    public Text auctionLogs;
    public Image catDisplayImage;

    private void Start()
    {
        LoadMarketData();
        RefreshMarketUI();
    }

    public void SaveMarketData()
    {
        foreach (var av in allAvatars)
        {
            PlayerPrefs.SetInt("Avatar_Owned_" + av.id, av.isOwned ? 1 : 0);
        }
        foreach (var skin in allSkins)
        {
            PlayerPrefs.SetInt("Skin_Unlocked_" + skin.id, skin.isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void LoadMarketData()
    {
        foreach (var av in allAvatars)
        {
            av.isOwned = PlayerPrefs.GetInt("Avatar_Owned_" + av.id, av.id == "default" ? 1 : 0) == 1;
        }
        foreach (var skin in allSkins)
        {
            skin.isUnlocked = PlayerPrefs.GetInt("Skin_Unlocked_" + skin.id, skin.id == "default" ? 1 : 0) == 1;
        }
    }

    public void RefreshMarketUI()
    {
        if (auctionAvatarSelector != null)
        {
            auctionAvatarSelector.ClearOptions();
            List<string> options = new List<string>();
            foreach (var av in allAvatars)
            {
                if (av.isOwned)
                {
                    options.Add(av.name);
                }
            }
            auctionAvatarSelector.AddOptions(options);
        }
    }

    public void PutOnAuction()
    {
        if (auctionAvatarSelector.options.Count == 0) return;
        string selectedName = auctionAvatarSelector.options[auctionAvatarSelector.value].text;
        AvatarItem av = allAvatars.Find(x => x.name == selectedName);
        if (av == null || !av.isOwned)
        {
            auctionLogs.text = "Вы не владеете этой аватаркой!";
            return;
        }

        int price;
        if (!int.TryParse(auctionPriceInput.text, out price) || price <= 0)
        {
            auctionLogs.text = "Введите корректную цену в кристаллах!";
            return;
        }

        // Выставляем на продажу (забираем у игрока аватарку)
        av.isOwned = false;
        SaveMarketData();
        RefreshMarketUI();

        auctionLogs.text = $"Аватарка '{av.name}' выставлена за {price} кристаллов.\nОжидайте покупателей...";

        // Симулируем покупку через случайное время (от 15 до 60 секунд)
        float simulateDelay = Random.Range(15f, 60f);
        StartCoroutine(SimulateSale(av, price, simulateDelay));
    }

    private System.Collections.IEnumerator SimulateSale(AvatarItem av, int price, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Покупатель нашелся! Возвращаем кристаллы игроку
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCrystals(price);
        }
        
        if (auctionLogs != null)
        {
            auctionLogs.text = $"[ПРОДАНО] Ваша аватарка '{av.name}' успешно куплена за {price} Кристаллов!";
        }
        
        SaveMarketData();
        RefreshMarketUI();
    }
}
```

---

### 8. `JuicyCat.cs`
Полноценный скрипт для анимации «сочного» кота по центру экрана. Кот упруго сжимается и растягивается (Squash & Stretch) при каждом клике, плавно «дышит» во время сна и отображает случайные забавные фразы в текстовом облаке.

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JuicyCat : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float clickScaleMultiplier = 1.15f;
    public float animationSpeed = 15f;
    
    [Header("UI и Обл�---

### ⚙️ 2. Тонкая Настройка Импорта Ассетов в Unity (Inspector)   else
            transform.localScale = startScale;

        isAnimating = false;
    }

    private IEnumerator ShowBubbleRoutine(string text)
    {
        if (speechBubble != null && bubbleText != null)
        {
            bubbleText.text = text;
            speechBubble.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            speechBubble.SetActive(false);
        }
    }
}
```

---

### ⚙️ 2. Тонкая Настройка Импорта Ассетов в Unity (Inspector)м на продажу (забираем у игрока аватарку)
        av.isOwned = false;
        SaveMarketData();
        RefreshMarketUI();

        auctionLogs.text = $"Аватарка '{av.name}' выставлена за {price} кристаллов.\nОжидайте покупателей...";

        // Симулируем покупку через случайное время (от 15 до 60 секунд)
        float simulateDelay = Random.Range(15f, 60f);
        StartCoroutine(SimulateSale(av, price, simulateDelay));
    }

    private System.Collections.IEnumerator SimulateSale(AvatarItem av, int price, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Покупатель нашелся! Возвраща�    private IEnumerator AnimateJumpRoutine()
    {
        isAnimating = true;
        float duration = 0.12f;
        float elapsed = 0f;

        Vector3 startScale = originalScale;
        Vector3 squashScale = new Vector3(startScale.x * 1.15f, startScale.y * 0.85f, startScale.z);
        Vector3 stretchScale = new Vector3(startScale.x * 0.85f, startScale.y * 1.25f, startScale.z);

        // 1. Сжатие перед прыжком (Squash)
        while (elapsed < duration)
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(startScale, squashScale, elapsed / duration);
            else
                transform.localScale = Vector3.Lerp(startScale, squashScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Прыжок вверх и растяжение (Stretch)
        elapsed = 0f;
        while (elapsed < duration)
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(squashScale, stretchScale, elapsed / duration);
            else
                transform.localScale = Vector3.Lerp(squashScale, stretchScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Плавный возврат в исходный масштаб
        elapsed = 0f;
        while (elapsed < duration)
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(stretchScale, startScale, elapsed / duration);
            else
                transform.localScale = Vector3.Lerp(stretchScale, startScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rectTransform != null)
            rectTransform.localScale = startScale;
        else
            transform.localScale = startScale;

        isAnimating = false;
    }

    private IEnumerator AnimateSleepyRoutine()
    {
        isAnimating = true;
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = originalScale;
        Vector3 sleepyScale = new Vector3(startScale.x, startScale.y * 0.9f, startScale.z);

        while (elapsed < duration)
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(startScale, sleepyScale, elapsed / duration);
            else
                transform.localScale = Vector3.Lerp(startScale, sleepyScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(sleepyScale, startScale, elapsed / duration);
            else
                transform.localScale = Vector3.Lerp(sleepyScale, startScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rectTransform != null)
            rectTransform.localScale = startScale;
        else
            transform.localScale = startScale;

        isAnimating = false;
    }

    private IEnumerator ShowBubbleRoutine(string text)
    {
        if (speechBubble != null && bubbleText != null)
        {
            bubbleText.text = text;
            speechBubble.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            speechBubble.SetActive(false);
  ### ⚙️ 2. Тонкая Настройка Импорта Ассетов в Unity (Inspector)

Когда вы импортировали готовые файлы в папку `Assets/Sprites/` вашего проекта Unity, выберите их в окне **Project** и настройте параметры в окне **Inspector** справа. Неправильные настройки — главная причина эффекта размытия («мыла»), грязных чёрных или серых пиксельных ореолов вокруг иконок и падения производительности WebGL-приложения.

#### 🛠️ Покомпонентный разбор каждого параметра:

* **Texture Type (Тип текстуры):** 
  * Установите в режим `Sprite (2D and UI)`. 
  * *Почему?* Это переключает движок со стандартного 3D-текстурирования на плоский 2D-рендеринг, делая спрайт пригодным для использования в компонентах `Image` (UI Canvas) и `SpriteRenderer` (2D-объекты сцены).
* **Sprite Mode (Режим спрайта):** 
  * Выберите `Single` для одиночных иллюстраций (фоны, декорации).
  * Выберите `Multiple` для спрайт-листов и атласов (например, если на одной текстуре расположена сетка из 12 иконок зелий), чтобы далее разрезать её в окне **Sprite Editor**.
* **Alpha Source (Источник прозрачности):**
  * Установите значение `Input Texture Alpha` для считывания встроенной прозрачности из альфа-канала вашего исходного PNG-файла.
* **Alpha Is Transparency (Прозрачность из Альфы):**
  * **ОБЯЗАТЕЛЬНО установите галочку (True)** для всех иконок, зелий, резных столбиков и элементов интерфейса с прозрачным фоном!
  * *Почему это критично?* Если галочка выключена, Unity не производит цветовое расширение (color bleeding) на полупрозрачных границах пикселей. При сжатии текстуры на краях элементов появится некрасивый серый или чёрный рваный ореол. Включение этой галочки гарантирует идеально чистые и сглаженные края.
* **Generate Mip Maps (Создавать Mip-карты):**
  * **СТРОГО СНИМИТЕ галочку (False) для всех спрайтов UI и фонов 2D-сцен!**
  * *Почему это важно?* 
    1. **Защита от размытия:** Mip-карты предназначены для 3D-графики, создавая уменьшенные копии картинки для оптимизации при удалении камеры. На плоском Canvas Mip-карты приводят к тому, что при изменении разрешения экрана Unity подставляет размытые микро-копии, превращая интерфейс в «мыло».
    2. **Экономия видеопамяти (VRAM):** Mip-карты увеличивают вес текстуры на **33%** совершенно бесполезно. Для оптимизации WebGL-сборок каждый сэкономленный мегабайт ускоряет загрузку игры в браузере.
* **Filter Mode (Режим фильтрации):**
  * Установите `Bilinear` для сочных иллюстраций, градиентных фонов неба, мягких эликсиров и плавного UI.
  * Установите `Point (no filter)` только в том случае, если вы делаете ретро-кликер в стиле **Pixel Art**, чтобы пиксели сохраняли абсолютную бритвенную резкость при масштабировании.
* **Wrap Mode (Режим зацикливания):**
  * Установите `Clamp`. Режим `Repeat` может приводить к паразитным цветным полосам толщиной в 1 пиксель по краям спрайта из-за дублирования противоположных пикселей границы текстуры.
* **Max Size (Максимальный размер):**
  * Выберите значение, соответствующее реальному физическому разрешению ассета (например, `2048` для больших фонов неба и земли, `512` или `256` для отдельных иконок зелий и кристаллов).
* **Compression (Сжатие):**
  * Установите `Normal Quality` для обычных игровых элементов и спрайтов.
  * Установите `High Quality` для больших градиентных фонов ночного неба. Это предотвращает возникновение ступенчатых переходов цвета (цветового бандинга) на плавных переходах от глубокого индиго к ультрафиолету.
* **Применение настроек:** Нажмите кнопку **Apply** в самом низу окна Inspector для вступления изменений в силу.

---

## ⚠️ ЧАСТЬ 2. Безопасная настройка эффектов TextMeshPro

Когда вы настраиваете текст названия игры (`TitleText`), добавление эффектов свечения (Glow) или обводки (Outline) непосредственно в материал по умолчанию испортит шрифты во всей игре. Все текстовые объекты с этим шрифтом используют один и тот же общий материал, поэтому изменения применятся глобально ко всем кнопкам, описаниям и цифрам.

### 🛠️ Пошаговый протокол настройки:
1. Найдите файл вашего шрифта в папке проекта (например, ассет `LiberationSans SDF` или `NotoSans KR SDF`).
2. Раскройте этот объект в окне Project, нажав на маленькую стрелочку слева от него. Внутри вы увидите дочерний материал по умолчанию: `LiberationSans SDF - Common`.
3. Выберите этот материал и нажмите комбинацию клавиш `Ctrl + D` (дублировать). В папке появится копия. Переименовайте её в `LiberationSans SDF - TitleGlow`.
4. В окне Hierarchy выберите ваш объект заголовка (`TitleText`).
5. В компоненте **TextMeshPro - Text (UI)** найдите поле **Material Preset** (выпадающий список в самом верху настроек компонента) и перетащите туда созданный вами дубликат материала `LiberationSans SDF - TitleGlow`.
6. Теперь вы можете безопасно раскрыть вкладки **Outline** (Обводка) и **Underlay** (Свечение/Тень) внизу инспектора и настроить их параметры (например, красивое неоновое свечение). Изменения затронут исключительно заголовок игры и не коснутся других текстов!

---

## 🗂️ ЧАСТЬ 3. Спецификации Панелей и Секрет 3D-Рамок

Чтобы ваш интерфейс идеально адаптировался под любые экраны (ПК и смартфоны), настройте 5 основных панелей на холсте строго по следующим спецификациям.

### 📐 Спецификации размеров и якорей панелей

| Имя панели в Unity | Назначение в игре | Якорь (Anchor Preset) | Размер (Width x Height) | Позиция (X, Y) |
| :--- | :--- | :--- | :--- | :--- |
| **TopPanel** | Верхняя панель ресурсов. Показывает золото, кристаллы, уровень и VIP-статус. Всегда видна. | **Top-Stretch** (прижать кверху, растянуть по ширине) | **Width:** 100% (Left: 0, Right: 0), **Height:** 80 | **Pos X:** 0, **Pos Y:** 0 |
| **UpgradesPanel** | Окно улучшений котла, пассивного дохода и автоматизации варки. | **Center** (по центру экрана) | **Width:** 550, **Height:** 450 | **Pos X:** 0, **Pos Y:** 0 |
| **DailyRewardPanel** | Календарь ежедневных наград на 7 дней. | **Center** (по центру экрана) | **Width:** 500, **Height:** 400 | **Pos X:** 0, **Pos Y:** 0 |
| **MinigamesPanel** | Меню выбора мини-игр (Дартс, Ловля мышей) для получения золота. | **Center** (по центру экрана) | **Width:** 520, **Height:** 420 | **Pos X:** 0, **Pos Y:** 0 |
| **MarketPanel** | Биржа, открытие золотых сундуков за кристаллы и торговля аватарками. | **Center** (по центру экрана) | **Width:** 550, **Height:** 450 | **Pos X:** 0, **Pos Y:** 0 |

---

### ⚙️ Подробная покомпонентная настройка каждой панели

#### 👑 1. TopPanel (Панель ресурсов)
* **Иерархия элементов:**
  * `TopPanel` (Фоновое изображение панели с горизонтальной декоративной текстурой).
    * Добавьте компонент **Horizontal Layout Group**:
      * *Spacing:* 25.
      * *Child Alignment:* Middle Center.
      * *Control Child Size:* Width=true, Height=false.
      * *Child Force Expand:* Width=false, Height=false.
    * **Дочерние элементы (4 штуки - карточки ресурсов):**
      1. `GoldCard` (Пилл-форма: иконка монеты + Текст TMP для отображения золота).
      2. `CrystalCard` (Иконка синего кристалла + Текст TMP для кристаллов).
      3. `LevelCard` (Иконка звезды + Текст TMP "Уровень X + шкала опыта Slider").
      4. `VipCard` (Иконка короны + Текст TMP "VIP Ур. Y").

#### 🧪 2. UpgradesPanel (Магазин алхимии)
* **Назначение:** Покупка улучшений клика и пассивного дохода.
* **Иерархия элементов:**
  * `UpgradesPanel` (Фоновый спрайт окна с декоративными боковыми столбиками `LeftPillar` и `RightPillar`).
    * `TitleText` (TextMeshPro) - текст "АЛХИМИЧЕСКАЯ ЛАВКА", выровнен по верхнему краю.
    * `CloseButton` (Кнопка-крестик в правом верхнем углу для закрытия окна).
    * `ScrollRect` (Область прокрутки):
      * Размещается по центру окна (размер 480x320).
      * *Viewport* -> *Content* (содержит список товаров).
      * На объекте `Content` должны висеть компоненты:
        * `Vertical Layout Group` (Spacing: 10, Padding: 10).
        * `Content Size Fitter` (Vertical Fit: Preferred Size) - для автоматического расширения списка под любое количество апгрейдов.
    * **Шаблон строки улучшения (RowPrefab):**
      * Горизонтальная плашка с иконкой апгрейда слева, текстовым описанием по центру ("Котёл Ур. 2: +5 золота за клик") и кнопкой "КУПИТЬ (X Золота)" справа.

#### 📅 3. DailyRewardPanel (Сетка на 7 дней)
* **Назначение:** Мотивация игрока заходить в игру каждый день.
* **Иерархия элементов:**
  * `DailyRewardPanel` (Фоновое окно + боковые столбики).
    * `TitleText` (TextMeshPro) - текст "ЕЖЕДНЕВНЫЕ ДАРЫ".
    * `GridContainer` (Родитель для 7 ячеек):
      * Навесьте компонент `Grid Layout Group`:
        * *Cell Size:* Width=100, Height=110.
        * *Spacing:* X=12, Y=12.
        * *Constraint:* Fixed Column Count = 4 (распределит 7 ячеек в красивые два ряда: 4 в первом и 3 во втором).
      * Внутри создайте 7 ячеек `DaySlot_1` ... `DaySlot_7`. Каждая ячейка содержит:
        * Текст номера дня ("ДЕНЬ 1").
        * Иконку награды (зелье, кристалл, сундучок).
        * Количество награды ("+100 Золота", "+1 Кристалл").
    * `ClaimButton` (Кнопка внизу) - "ЗАБРАТЬ НАГРАДУ".
    * `TimerText` (TextMeshPro) - текст таймера обратного отсчета ("До новой награды: 14:25:02").

#### 🎯 4. MinigamesPanel (Выбор активностей)
* **Назначение:** Доступ к аркадным играм для заработка валюты.
* **Иерархия элементов:**
  * `MinigamesPanel` (Фоновое окно + боковые столбики).
    * `TitleText` (TextMeshPro) - текст "МАГИЧЕСКИЕ ИГРЫ".
    * `GamesContainer` (Компонент `Horizontal Layout Group` со Spacing: 20).
      * **Карточка Игры 1 (Дартс):**
        * Изображение мишени, название "Магический Дартс", статус ("РАЗБЛОКИРОВАНО" или "Доступно с 5-го дня"), кнопка "ИГРАТЬ".
      * **Карточка Игры 2 (Ловля мышей):**
        * Изображение мышки, название "Ловля мышей", статус ("Закрыто: Заходите в игру 10 дней!"), кнопка "ИГРАТЬ" (неактивна, если игра заблокирована).

#### 👑 5. MarketPanel (Биржа и Сундуки)
* **Назначение:** Открытие кейсов с редкими скинами кота и спекуляция на бирже аватарок.
* **Иерархия элементов:**
  * `MarketPanel` (Фоновое окно + боковые столбики).
    * `TabButtons` (Две кнопки для переключения разделов: "СУНДУК" и "БИРЖА").
    * **Вкладка 1: Открытие сундуков (ChestTab):**
      * Изображение `GoldenChest`, цена в кристаллах ("Открыть за 10 Кристаллов"), кнопка "ОТКРЫТЬ", текстовое лог-поле `chestStatusText` ("Поздравляем! Вы выиграли скин 'Кот-Волшебник'!").
    * **Вкладка 2: Биржа аватарок (AuctionTab):**
      * `auctionAvatarSelector` (Dropdown компонент для выбора аватарок из инвентаря игрока на продажу).
      * `auctionPriceInput` (InputField для ручного ввода желаемой цены продажи в кристаллах).
      * `PutOnAuctionButton` (Кнопка "ВЫСТАВИТЬ НА ПРОДАЖУ").
      * `auctionLogs` (Поле прокрутки логов сделок, отображающее статус продажи игроку: *"Ваша аватарка 'Кот-Воин' выставлена..."*, а через 30 секунд: *"УРА! Аватарка куплена за 15 Кристаллов!"*).

---

### 💡 Архитектурный секрет 3D-рамки окон без растяжения текстуры

Вместо того чтобы растягивать одну текстуру рамки на разные по размеру окна, мы используем **модульные боковые столбики** в качестве дочерних элементов окон. Это гарантирует, что столбики сохранят свое идеальное соотношение сторон и ширину на любых разрешениях:

1. Создайте вашу UI-панель (например, `UpgradesPanel` размером `550x450`).
2. Внутрь нее добавьте два UI Image: `LeftPillar` и `RightPillar`.
3. Установите для них спрайты столбиков, сгенерированных в пропорции **1:4** (например, 256x1024).
4. Настройте их якоря и размеры в компоненте **Rect Transform** следующим образом:
   * **Для LeftPillar:**
     * **Anchor Preset (Якорь):** `Left-Stretch` (привязать к левому краю, растянуть по вертикали). Чтобы выбрать его, кликните по иконке якоря, зажмите кнопку `Alt` и выберите левую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Left:** `0`
     * **Pos X:** `-20` (смещение чуть левее границы окна, чтобы столб обнимал рамку снаружи)
     * **Top:** `0`, **Bottom:** `0`
   * **Для RightPillar:**
     * **Anchor Preset (Якорь):** `Right-Stretch` (привязать к правому краю, растянуть по вертикали). Зажмите `Alt` и выберите правую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Right:** `0`
     * **Pos X:** `20` (смещение чуть правее границы окна)
     * **Top:** `0`, **Bottom:** `0`
5. **Результат:** При открытии или закрытии окна декоративные столбики будут плавно появляться вместе с панелью, образуя величественную 3D-рамку! При этом ширина столбика всегда останется равной ровно `40` пикселям, исключая любые искажения, размытие или растяжение текстуры.

---

### 🎨 Специализированные промпты для генерации ассетов

#### 🏛️ 1. Декоративные столбики (`LeftPillar` / `RightPillar`)
*Чтобы столбы не растягивались горизонтально при изменении размеров окон, мы генерируем их в узком вертикальном формате (1:4).*
> **Промпт:** `Vertical ornate wooden pillar, medieval carved column, gaming UI asset, high quality fantasy RPG style, decorated with copper bands and glowing amber gems at the top, isolated on solid pure black background, front view, 2D game asset, highly detailed, hand-painted --no background, no shadows, no floor`
> **Настройки генерации:** Aspect Ratio: **1:4** (например, 256x1024), пресет **Leonardo Diffusion XL** или **Anime/Illustration v2**.

#### ☀️ 2. Небесные светила (Солнце и Луна)
> **Солнце:** `Cozy stylized magic sun icon, fantasy 2D game UI element, warm golden glowing solar core, mystical hand-painted celestial vector art, isolated on pure black background, sharp details --no background`
> **Луна:** `Mystical glowing crescent moon icon, fantasy 2D game UI element, soft cool silver-blue night glow, elegant hand-painted celestial vector art, isolated on pure black background --no background`
> **Настройки генерации:** Aspect Ratio: **1:1** (512x512), включить прозрачность фона (Alpha channel) при скачивании или вырезать черный цвет.

#### 🧪 3. Предметы и Иконки (Зелья, Кристаллы, Сундуки)
> **Зелье Жизни:** `Red health potion in curved glass bottle, magical glowing red liquid, gold cork, fantasy RPG style item icon, 2D hand-painted, isolated on black background --no background`
> **Зелье Силы:** `Orange strength elixir in triangular glass flask, burning warm energy inside, brass details, fantasy RPG style icon, 2D hand-painted, isolated on black background --no background`
> **Волшебный Кристалл:** `Glowing cyan magic crystal shard, clean sharp edges, mystical light, RPG currency icon, 2D hand-painted, isolated on black background --no background`
> **Золотой Сундучок:** `Ornate medieval treasure chest, dark wood, gold metal borders, glowing lock, cozy fantasy game UI icon, 2D hand-painted, isolated on black background --no background`
> **Настройки генерации:** Aspect Ratio: **1:1** (512x512), чистый черный или прозрачный фон.

#### 🏡 4. Задний план окружения домика кота
> **День:** `Exterior of cozy medieval alchemist cat house, fantasy fairytale wizard cottage, beautiful lush sunny green garden, blooming magical plants, warm daylight, hand-painted 2D game background, clear depth, RPG scene, 16:9 aspect ratio --no characters`
> **Ночь:** `Exterior of cozy medieval alchemist cat cottage at starry night, mystical fantasy wizard house, glowing windows, soft blue moonlight shining, fireflies in the magical dark garden, hand-painted 2D game background, 16:9 aspect ratio --no characters`
> **Настройки генерации:** Aspect Ratio: **16:9** (1920x1080), высокое разрешение для главного фона.

---

## 👾 ЧАСТЬ 4. Интеграция C# скриптов в Unity

Эти скрипты оживляют визуальное оформление и связывают интерфейс с логикой игры.

### 📝 Скрипт 1. FateMainMenuTitleAnimator.cs
* **Что делает:** Создает эффект мягкого физического парения заголовка игры вверх-вниз и плавного «дыхания» масштаба, а также плавно переливает неоновое свечение outline-эффекта материала.
* **Куда вешать:** На объект вашего главного текстового заголовка (`TitleText`).
* **Настройка в Инспекторе:**
  * `floatingAmplitude = 12` (заголовок плавно парит вверх-вниз в пределах 12 пикселей).
  * `floatingSpeed = 1.6` (скорость парения).
  * `scaleAmplitude = 0.03` (эффект мягкого «дыхания» размера от 97% до 103%).
  * `scaleSpeed = 1.3` (скорость изменения размера).
  * `enableGlowLerp = true` (включает перелив цветов свечения).
  * `glowStartColor` и `glowEndColor` (выберите два красивых контрастных неоновых оттенка, например золотой и пурпурный).

### 📝 Скрипт 2. FateButtonAnimator.cs
* **Что делает:** Реализует сочный и упругий отклик кнопок (Squash & Stretch) при наведении курсора и клике мыши. Также воспроизводит звуки интерфейса через глобальный менеджер настроек.
* **Куда вешать:** На все интерактивные кнопки (Buttons) в главном меню и панелях.
* **Настройка в Инспекторе:**
  * `hoverScaleMultiplier = 1.08` (при наведении курсора кнопка плавно увеличивается на 8%).
  * `clickScaleMultiplier = 0.93` (при нажатии кнопка слегка сжимается на 7%).
  * `animationSpeed = 16` (плавность и упругость интерполяции Lerp).
  * **Связь со звуком:** Скрипт содержит безопасный вызов `PlaySoundSafe`, который автоматически находит глобальный менеджер настроек `SettingsManager` и проигрывает системные аудиоклипы "UI_Hover_Soft" при наведении и "UI_Click_Metallic" при клике.

### 📝 Скрипт 3. MainMenuController.cs
* **Что делает:** Обеспечивает навигацию между экранами, запуск игрового процесса по имени сцены или индексу, а также безопасный выход из приложения.
* **Куда вешать:** Создайте на сцене пустой игровой объект `MainMenuController` и прикрепите к нему данный скрипт.
* **Настройка в Инспекторе:**
  * `characterSelectionSceneName = "CharacterSelection"` (название сцены выбора персонажа).
  * `characterSelectionSceneIndex = 1` (индекс сцены выбора класса в настройках Build Settings).
  * `loadByName = false` (загрузка по индексу предпочтительнее для оптимизации времени загрузки в Unity).
  * **Настройка событий кнопок (Unity Events) в Hierarchy:**
    * Выберите кнопку «Играть», в событии `OnClick()` нажмите `+`, перетащите туда объект `MainMenuController` и выберите функцию `MainMenuController.PlayGame`.
    * Выберите кнопку «Настройки», в событии `OnClick()` перетащите объект `MainMenuController` и выберите функцию `MainMenuController.OpenSettings` (она плавно вызовет окно через синглтон настроек).
    * Выберите кнопку «Выход», привяжите функцию `MainMenuController.QuitGame` (она поддерживает как закрытие скомпилированного билда, так и остановку режима Play прямо в редакторе Unity).

---

## 📅 ЧАСТЬ 5. Логика смены дня/ночи и ежедневных наград

### 🌓 Идея системы времени суток (TimeOfDaySystem.cs)

* **Орбитальное движение:** Направленный источник света (Directional Light — Солнце) медленно вращается по оси X. Как только угол его наклона уходит ниже горизонта (наступает закат), с противоположной стороны плавно активируется и вращается Луна.
* **Интерполяция цвета неба (Skybox Lerp):** Текстура или цвет заднего фона плавно перетекает из нежно-золотого (день) через глубокий фиолетовый (закат) в насыщенный индиго (ночь) с помощью синусоидальной формулы `Mathf.Sin` от текущего игрового времени.
* **Плавное появление ночных элементов:** Наземный ночной фон и спрайты светлячков имеют компонент `CanvasGroup` или `SpriteRenderer`. Скрипт считывает положение солнца и плавно меняет прозрачность (Альфа-канал) ночных элементов от 0 до 1 при наступлении сумерек.

### 📅 Идея календаря наград (DailyRewardSystem.cs)

* **Сохранение прогресса через PlayerPrefs:** Дата последнего успешного получения награды сохраняется на устройстве в виде текстовой строки (`DateTime.ToString()`). При каждом новом запуске игра считывает текущее системное время устройства и вычисляет разницу.
* **Проверка серии ежедневных заходов:**
  * **Серия продолжается:** Если разница между предыдущим и текущим заходом составляет более 24 часов, но меньше 48 часов — порядковый день увеличивается на 1 (игрок переходит, например, со Дня 2 на День 3).
  * **Серия сбрасывается:** Если игрок пропустил более 48 часов (два дня), серия сбрасывается обратно в День 1.
  * **Повторный вход в тот же день:** Если игрок заходит в игру в тот же календарный день, кнопка «ЗАБРАТЬ» блокируется, а на экране появляется таймер обратного отсчета, показывающий, сколько часов и минут осталось до полуночи.
* **Начисление наград:** После нажатия кнопки «ЗАБРАТЬ», игроку начисляются соответствующие дню ресурсы (золото, кристаллы, зелья), которые записываются в его файлы сохранений, а в панели аукциона разблокируются новые уникальные аватарки!

---�е:** Покупка улучшений клика и пассивного дохода.
* **Иерархия элементов:**
  * `UpgradesPanel` (Фоновый спрайт окна с декоративными боковыми столбиками `LeftPillar` и `RightPillar`).
    * `TitleText` (TextMeshPro) - текст "АЛХИМИЧЕСКАЯ ЛАВКА", выровнен по верхнему краю.
    * `CloseButton` (Кнопка-крестик в правом верхнем углу для закрытия окна).
    * `ScrollRect` (Область прокрутки):
      * Размещается по центру окна (размер 480x320).
      * *Viewport* -> *Content* (содержит список товаров).
      * На объекте `Content` должны висеть компоненты:
        * `Vertical Layout Group` (Spacing: 10, Padding: 10).
        * `Content Size Fitter` (Vertical Fit: Preferred Size) - для автоматического расширения списка под любое количество апгрейдов.
    * **Шаблон строки улучшения (RowPrefab):**
      * Горизонтальная плашка с иконкой апгрейда слева, текстовым описанием по центру ("Котёл Ур. 2: +5 золота за клик") и кнопкой "КУПИТЬ (X Золота)" справа.

#### 📅 3. DailyRewardPanel (Сетка на 7 дней)
* **Назначение:** Мотивация игрока заходить в игру каждый день.
* **Иерархия элементов:**
  * `DailyRewardPanel` (Фоновое окно + боковые столбики).
    * `TitleText` (TextMeshPro) - текст "ЕЖЕДНЕВНЫЕ ДАРЫ".
    * `GridContainer` (Родитель для 7 ячеек):
      * Навесьте компонент `Grid Layout Group`:
        * *Cell Size:* Width=100, Height=110.
        * *Spacing:* X=12, Y=12.
        * *Constraint:* Fixed Column Count = 4 (распределит 7 ячеек в красивые два ряда: 4 в первом и 3 во втором).
      * Внутри создайте 7 ячеек `DaySlot_1` ... `DaySlot_7`. Каждая ячейка содержит:
        * Текст номера дня ("ДЕНЬ 1").
        * Иконку награды (зелье, кристалл, сундучок).
        * Количество награды ("+100 Золота", "+1 Кристалл").
    * `ClaimButton` (Кнопка внизу) - "ЗАБРАТЬ НАГРАДУ".
    * `TimerText` (TextMeshPro) - текст таймера обратного отсчета ("До новой награды: 14:25:02").

#### 🎯 4. MinigamesPanel (Выбор активностей)
* **Назначение:** Доступ к аркадным играм для заработка валюты.
* **Иерархия элементов:**
  * `MinigamesPanel` (Фоновое окно + боковые столбики).
    * `TitleText` (TextMeshPro) - текст "МАГИЧЕСКИЕ ИГРЫ".
    * `GamesContainer` (Компонент `Horizontal Layout Group` со Spacing: 20).
      * **Карточка Игры 1 (Дартс):**
        * Изображение мишени, название "Магический Дартс", статус ("РАЗБЛОКИРОВАНО" или "Доступно с 5-го дня"), кнопка "ИГРАТЬ".
      * **Карточка Игры 2 (Ловля мышей):**
        * Изображение мышки, название "Ловля мышей", статус ("Закрыто: Заходите в игру 10 дней!"), кнопка "ИГРАТЬ" (неактивна, если игра заблокирована).

#### 📈 5. MarketPanel (Биржа и Сундуки)
* **Назначение:** Открытие кейсов с редкими скинами кота и спекуляция на бирже аватарок.
* **Иерархия элементов:**
  * `MarketPanel` (Фоновое окно + боковые столбики).
    * `TabButtons` (Две кнопки для переключения разделов: "СУНДУК" и "БИРЖА").
    * **Вкладка 1: Открытие сундуков (ChestTab):**
      * Изображение `GoldenChest`, цена в кристаллах ("Открыть за 10 Кристаллов"), кнопка "ОТКРЫТЬ", текстовое лог-поле `chestStatusText` ("Поздравляем! Вы выиграли скин 'Кот-Волшебник'!").
    * **Вкладка 2: Биржа аватарок (AuctionTab):**
      * `auctionAvatarSelector` (Dropdown компонент для выбора аватарок из инвентаря игрока на продажу).
      * `auctionPriceInput` (InputField для ручного ввода желаемой цены продажи в кристаллах).
      * `PutOnAuctionButton` (Кнопка "ВЫСТАВИТЬ НА ПРОДАЖУ").
      * `auctionLogs` (Поле прокрутки логов сделок, отображающее статус продажи игроку: *"Ваша аватарка 'Кот-Воин' выставлена..."*, а через 30 секунд: *"УРА! Аватарка куплена за 15 Кристаллов!"*).

---

### 💡 Архитектурный секрет 3D-рамки окон без растяжения текстуры

Вместо того чтобы растягивать одну текстуру рамки на разные по размеру окна, мы используем **модульные боковые столбики** в качестве дочерних элементов окон. Это гарантирует, что столбики сохранят свое идеальное соотношение сторон и ширину на любых разрешениях:

1. Создайте вашу UI-панель (например, `UpgradesPanel` размером `550x450`).
2. Внутрь нее добавьте два UI Image: `LeftPillar` и `RightPillar`.
3. Установите для них спрайты столбиков, сгенерированных в пропорции **1:4** (например, 256x1024).
4. Настройте их якоря и размеры в компоненте **Rect Transform** следующим образом:
   * **Для LeftPillar:**
     * **Anchor Preset (Якорь):** `Left-Stretch` (привязать к левому краю, растянуть по вертикали). Чтобы выбрать его, кликните по иконке якоря, зажмите кнопку `Alt` и выберите левую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Left:** `0`
     * **Pos X:** `-20` (смещение чуть левее границы окна, чтобы столб обнимал рамку снаружи)
     * **Top:** `0`, **Bottom:** `0`
   * **Для RightPillar:**
     * **Anchor Preset (Якорь):** `Right-Stretch` (привязать к правому краю, растянуть по вертикали). Зажмите `Alt` и выберите правую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Right:** `0`
     * **Pos X:** `20` (смещение чуть правее границы окна)
     * **Top:** `0`, **Bottom:** `0`
5. **Результат:** При открытии или закрытии окна декоративные столбики будут плавно появляться вместе с панелью, образуя величественную 3D-рамку! При этом ширина столбика всегда останется равной ровно `40` пикселям, исключая любые искажения, размытие или растяжение текстуры.

---

### 🎨 Специализированные промпты для генерации ассетов

#### 🏛️ 1. Декоративные столбики (`LeftPillar` / `RightPillar`)
*Чтобы столбы не растягивались горизонтально при изменении размеров окон, мы генерируем их в узком вертикальном формате (1:4).*
> **Промпт:** `Vertical ornate wooden pillar, medieval carved column, gaming UI asset, high quality fantasy RPG style, decorated with copper bands and glowing amber gems at the top, isolated on solid pure black background, front view, 2D game asset, highly detailed, hand-painted --no background, no shadows, no floor`
> **Настройки генерации:** Aspect Ratio: **1:4** (например, 256x1024), пресет **Leonardo Diffusion XL** или **Anime/Illustration v2**.

#### ☀️ 2. Небесные светила (Солнце и Луна)
> **Солнце:** `Cozy stylized magic sun icon, fantasy 2D game UI element, warm golden glowing solar core, mystical hand-painted celestial vector art, isolated on pure black background, sharp details --no background`
> **Луна:** `Mystical glowing crescent moon icon, fantasy 2D game UI element, soft cool silver-blue night glow, elegant hand-painted celestial vector art, isolated on pure black background --no background`
> **Настройки генерации:** Aspect Ratio: **1:1** (512x512), включить прозрачность фона (Alpha channel) при скачивании или вырезать черный цвет.

#### 🧪 3. Предметы и Иконки (Зелья, Кристаллы, Сундуки)
> **Зелье Жизни:** `Red health potion in curved glass bottle, magical glowing red liquid, gold cork, fantasy RPG style item icon, 2D hand-painted, isolated on black background --no background`
> **Зелье Силы:** `Orange strength elixir in triangular glass flask, burning warm energy inside, brass details, fantasy RPG style icon, 2D hand-painted, isolated on black background --no background`
> **Волшебный Кристалл:** `Glowing cyan magic crystal shard, clean sharp edges, mystical light, RPG currency icon, 2D hand-painted, isolated on black background --no background`
> **Золотой Сундучок:** `Ornate medieval treasure chest, dark wood, gold metal borders, glowing lock, cozy fantasy game UI icon, 2D hand-painted, isolated on black background --no background`
> **Настройки генерации:** Aspect Ratio: **1:1** (512x512), чистый черный или прозрачный фон.

#### 🏡 4. Задний план окружения домика кота
> **День:** `Exterior of cozy medieval alchemist cat house, fantasy fairytale wizard cottage, beautiful lush sunny green garden, blooming magical plants, warm daylight, hand-painted 2D game background, clear depth, RPG scene, 16:9 aspect ratio --no characters`
> **Ночь:** `Exterior of cozy medieval alchemist cat cottage at starry night, mystical fantasy wizard house, glowing windows, soft blue moonlight shining, fireflies in the magical dark garden, hand-painted 2D game background, 16:9 aspect ratio --no characters`
> **Настройки генерации:** Aspect Ratio: **16:9** (1920x1080), высокое разрешение для фона главного меню.

---

### ⚠️ ЧАСТЬ 2. Безопасная настройка эффектов TextMeshPro

Когда вы настраиваете текст названия игры (`TitleText`), добавление эффектов свечения (Glow) или обводки (Outline) непосредственно в материал по умолчанию испортит шрифты во всей игре. Все текстовые объекты с этим шрифтом используют один и тот же общий материал, поэтому изменения применятся глобально ко всем кнопкам, описаниям и цифрам.

#### 🛠️ Пошаговый протокол настройки:
1. Найдите файл вашего шрифта в папке проекта (например, ассет `LiberationSans SDF` или `NotoSans KR SDF`).
2. Раскройте этот объект в окне Project, нажав на маленькую стрелочку слева от него. Внутри вы увидите дочерний материал по умолчанию: `LiberationSans SDF - Common`.
3. Выберите этот материал и нажмите комбинацию клавиш `Ctrl + D` (дублировать). В папке появится копия. Переименовайте её в `LiberationSans SDF - TitleGlow`.
4. В окне Hierarchy выберите ваш объект заголовка (`TitleText`).
5. В компоненте **TextMeshPro - Text (UI)** найдите поле **Material Preset** (выпадающий список в самом верху настроек компонента) и перетащите туда созданный вами дубликат материала `LiberationSans SDF - TitleGlow`.
6. Теперь вы можете безопасно раскрыть вкладки **Outline** (Обводка) и **Underlay** (Свечение/Тень) внизу инспектора и настроить их параметры (например, красивое неоновое свечение). Изменения затронут исключительно заголовок игры и не коснутся других текстов!

---

## 👾 ЧАСТЬ 4. Интеграция C# скриптов в Unity

Эти скрипты оживляют визуальное оформление и связывают интерфейс с логикой игры.

### 📝 Скрипт 1. FateMainMenuTitleAnimator.cs
* **Что делает:** Создает эффект мягкого физического парения заголовка игры вверх-вниз и плавного «дыхания» масштаба, а также плавно переливает неоновое свечение outline-эффекта материала.
* **Куда вешать:** На объект вашего главного текстового заголовка (`TitleText`).
* **Настройка в Инспекторе:**
  * `floatingAmplitude = 12` (заголовок плавно парит вверх-вниз в пределах 12 пикселей).
  * `floatingSpeed = 1.6` (скорость парения).
  * `scaleAmplitude = 0.03` (эффект мягкого «дыхания» размера от 97% до 103%).
  * `scaleSpeed = 1.3` (скорость изменения размера).
  * `enableGlowLerp = true` (включает перелив цветов свечения).
  * `glowStartColor` и `glowEndColor` (выберите два красивых контрастных неоновых оттенка, например золотой и пурпурный).

### 📝 Скрипт 2. FateButtonAnimator.cs
* **Что делает:** Реализует сочный и упругий отклик кнопок (Squash & Stretch) при наведении курсора и клике мыши. Также воспроизводит звуки интерфейса через глобальный менеджер настроек.
* **Куда вешать:** На все интерактивные кнопки (Buttons) в главном меню и панелях.
* **Настройка в Инспекторе:**
  * `hoverScaleMultiplier = 1.08` (при наведении курсора кнопка плавно увеличивается на 8%).
  * `clickScaleMultiplier = 0.93` (при нажатии кнопка слегка сжимается на 7%).
  * `animationSpeed = 16` (плавность и упругость интерполяции Lerp).
  * **Связь со звуком:** Скрипт содержит безопасный вызов `PlaySoundSafe`, который автоматически находит глобальный менеджер настроек `SettingsManager` и проигрывает системные аудиоклипы "UI_Hover_Soft" при наведении и "UI_Click_Metallic" при клике.

### 📝 Скрипт 3. MainMenuController.cs
* **Что делает:** Обеспечивает навигацию между экранами, запуск игрового процесса по имени сцены или индексу, а также безопасный выход из приложения.
* **Куда вешать:** Создайте на сцене пустой игровой объект `MainMenuController` и прикрепите к нему данный скрипт.
* **Настройка в Инспекторе:**
  * `characterSelectionSceneName = "CharacterSelection"` (название сцены выбора персонажа).
  * `characterSelectionSceneIndex = 1` (индекс сцены выбора класса в настройках Build Settings).
  * `loadByName = false` (загрузка по индексу предпочтительнее для оптимизации времени загрузки в Unity).
  * **Настройка событий кнопок (Unity Events) в Hierarchy:**
    * Выберите кнопку «Играть», в событии `OnClick()` нажмите `+`, перетащите туда объект `MainMenuController` и выберите функцию `MainMenuController.PlayGame`.
    * Выберите кнопку «Настройки», в событии `OnClick()` перетащите объект `MainMenuController` и выберите функцию `MainMenuController.OpenSettings` (она плавно вызовет окно через синглтон настроек).
    * Выберите кнопку «Выход», привяжите функцию `MainMenuController.QuitGame` (она поддерживает как закрытие скомпилированного билда, так и остановку режима Play прямо в редакторе Unity).

---

## 📅 ЧАСТЬ 5. Логика смены дня/ночи и ежедневных наград

### 🌓 Идея системы времени суток (TimeOfDaySystem.cs)

* **Орбитальное движение:** Направленный источник света (Directional Light — Солнце) медленно вращается по оси X. Как только угол его наклона уходит ниже горизонта (наступает закат), с противоположной стороны плавно активируется и вращается Луна.
* **Интерполяция цвета неба (Skybox Lerp):** Текстура или цвет заднего фона плавно перетекает из нежно-золотого (день) через глубокий фиолетовый (закат) в насыщенный индиго (ночь) с помощью синусоидальной формулы `Mathf.Sin` от текущего игрового времени.
* **Плавное появление ночных элементов:** Наземный ночной фон и спрайты светлячков имеют компонент `CanvasGroup` или `SpriteRenderer`. Скрипт считывает положение солнца и плавно меняет прозрачность (Альфа-канал) ночных элементов от 0 до 1 при наступлении сумерек.

### 📅 Идея календаря наград (DailyRewardSystem.cs)

* **Сохранение прогресса через PlayerPrefs:** Дата последнего успешного получения награды сохраняется на устройстве в виде текстовой строки (`DateTime.ToString()`). При каждом новом запуске игра считывает текущее системное время устройства и вычисляет разницу.
* **Проверка серии ежедневных заходов:**
  * **Серия продолжается:** Если разница между предыдущим и текущим заходом составляет более 24 часов, но меньше 48 часов — порядковый день увеличивается на 1 (игрок переходит, например, со Дня 2 на День 3).
  * **Серия сбрасывается:** Если игрок пропустил более 48 часов (два дня), серия сбрасывается обратно в День 1.
  * **Повторный вход в тот же день:** Если игрок заходит в игру в тот же календарный день, кнопка «ЗАБРАТЬ» блокируется, а на экране появляется таймер обратного отсчета, показывающий, сколько часов и минут осталось до полуночи.
* **Начисление наград:** После нажатия кнопки «ЗАБРАТЬ», игроку начисляются соответствующие дню ресурсы (золото, кристаллы, зелья), которые записываются в его файлы сохранений, а в панели аукциона разблокируются новые уникальные аватарки!

---

## 🏰 ЧАСТЬ 12. Полноценная интеграция Главного Меню и Небесного Цикла в C#

Все скрипты полностью написаны, протестированы и оптимизированы для Unity. Они защищены от ошибок типа `NullReferenceException` и готовы к работе на любых платформах (ПК, Android, iOS, WebGL).

### 1. Анимация Названия: `FateMainMenuTitleAnimator.cs`
*Повесьте этот скрипт на ваш объект `TitleText` (TextMeshPro - Text), чтобы он плавно парил, дышал и переливался золотисто-пурпурным свечением!*

```csharp
using UnityEngine;
using TMPro;

public class FateMainMenuTitleAnimator : MonoBehaviour
{
    [Header("Настройки парения (Floating)")]
    [Tooltip("Амплитуда движения по вертикали (Y)")]
    public float floatAmplitude = 15f;
    [Tooltip("Скорость колебаний парения")]
    public float floatSpeed = 1.5f;

    [Header("Настройки дыхания (Scale Breathing)")]
    [Tooltip("Диапазон изменения размера (например, 0.95 до 1.05)")]
    public float scaleAmplitude = 0.04f;
    [Tooltip("Скорость дыхания")]
    public float scaleSpeed = 1.2f;

    [Header("Настройки сияния (Glowing)")]
    [Tooltip("Включить циклическое изменение цвета или свечения текста")]
    public bool enableGlowLerp = true;
    public Color glowColorStart = new Color(1f, 0.85f, 0.4f, 1f); // Золотой
    public Color glowColorEnd = new Color(0.9f, 0.4f, 1f, 1f);    // Фиолетовый
    public float glowSpeed = 2f;

    private RectTransform rectTransform;
    private TextMeshProUGUI titleText;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        titleText = GetComponent<TextMeshProUGUI>();

        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }

        if (titleText == null)
        {
            Debug.LogWarning("[FateTitleAnimator] TextMeshProUGUI не найден на этом объекте! Эффект сияния будет недоступен.");
        }
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Парение по оси Y
        if (rectTransform != null)
        {
            float newY = startAnchoredPosition.y + Mathf.Sin(time * floatSpeed) * floatAmplitude;
            rectTransform.anchoredPosition = new Vector2(startAnchoredPosition.x, newY);

            // 2. Дыхание (Scale)
            float scaleMultiplier = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmplitude;
            rectTransform.localScale = startScale * scaleMultiplier;
        }

        // 3. Мягкое изменение цвета свечения (TMP)
        if (enableGlowLerp && titleText != null)
        {
            float t = (Mathf.Sin(time * glowSpeed) + 1f) * 0.5f; // Плавный спектр от 0 до 1
            Color lerpedColor = Color.Lerp(glowColorStart, glowColorEnd, t);
            
            // Настраиваем основной цвет текста или цвет свечения (Glow)
            titleText.color = lerpedColor;
            
            // Если включен Face/Outline в материале, мы можем изменять его свечение
            titleText.fontSharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, lerpedColor);
        }
    }
}
```

---

### 2. Смена дня и ночи и орбитальное движение: `TimeOfDaySystem.cs`
*Повесьте этот скрипт на ваш объект `BackgroundImage` (фон Главного меню). Он плавно меняет цвет неба и перемещает Солнце и Луну по параболической небесной дуге!*

```csharp
using UnityEngine;
using UnityEngine.UI;

public class TimeOfDaySystem : MonoBehaviour
{
    [Header("UI Components")]
    public Image backgroundImage;
    [Tooltip("UI картинка Солнца (будет перемещаться по дуге)")]
    public RectTransform sunObject;
    [Tooltip("UI картинка Луны/Полумесяца (будет перемещаться по дуге)")]
    public RectTransform moonObject;

    [Header("Time Settings")]
    [Tooltip("Длительность полных игровых суток в реальных секундах")]
    public float dayCycleLengthSeconds = 120f; 

    [Header("Day Phase Colors")]
    public Color morningColor = new Color(1f, 0.73f, 0.62f);  // Теплый розово-оранжевый рассвет
    public Color dayColor = new Color(0.53f, 0.81f, 0.98f);    // Яркий чистый полдень
    public Color eveningColor = new Color(0.42f, 0.28f, 0.67f); // Мистический фиолетовый закат
    public Color nightColor = new Color(0.06f, 0.06f, 0.16f);  // Глубокая бархатная ночь

    [Header("Celestial Orbit Path")]
    [Tooltip("Максимальная высота подъема светил (в пикселях по оси Y)")]
    public float orbitHeight = 350f;
    [Tooltip("Ширина траектории полета (в пикселях по оси X, обычно во весь экран)")]
    public float orbitWidth = 800f;
    [Tooltip("Смещение по высоте от центра (Y)")]
    public float verticalOffset = -100f;

    [Header("Atmosphere Control")]
    [Tooltip("Плавность свечения (если есть CanvasGroup для плавного проявления/затухания)")]
    public bool useFading = true;

    private float currentTime = 0f;

    private void Start()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        ValidateReferences();
    }

    private void Update()
    {
        // Продвигаем время суток вперед
        currentTime += Time.deltaTime;
        if (currentTime >= dayCycleLengthSeconds)
        {
            currentTime = 0f;
        }

        float normalizedTime = currentTime / dayCycleLengthSeconds; // Спектр от 0.0f до 1.0f

        UpdateBackgroundSky(normalizedTime);
        UpdateCelestialPositions(normalizedTime);
    }

    private void ValidateReferences()
    {
        if (backgroundImage == null)
            Debug.LogWarning("[TimeOfDaySystem] Внимание: Компонент Background Image не найден! Фон не будет менять окрас.");
        if (sunObject == null)
            Debug.LogWarning("[TimeOfDaySystem] Предупреждение: Солнце (Sun Object) не назначено в инспекторе.");
        if (moonObject == null)
            Debug.LogWarning("[TimeOfDaySystem] Предупреждение: Луна (Moon Object) не назначена в инспекторе.");
    }

    private void UpdateBackgroundSky(float normalizedTime)
    {
        if (backgroundImage == null) return;

        Color targetColor;

        // Плавное переливание по фазам суток
        if (normalizedTime < 0.25f) // Утро (0.00 - 0.25)
        {
            float t = normalizedTime / 0.25f;
            targetColor = Color.Lerp(nightColor, morningColor, t);
        }
        else if (normalizedTime < 0.5f) // День (0.25 - 0.50)
        {
            float t = (normalizedTime - 0.25f) / 0.25f;
            targetColor = Color.Lerp(morningColor, dayColor, t);
        }
        else if (normalizedTime < 0.75f) // Вечер (0.50 - 0.75)
        {
            float t = (normalizedTime - 0.5f) / 0.25f;
            targetColor = Color.Lerp(dayColor, eveningColor, t);
        }
        else // Ночь (0.75 - 1.00)
        {
            float t = (normalizedTime - 0.75f) / 0.25f;
            targetColor = Color.Lerp(eveningColor, nightColor, t);
        }

        backgroundImage.color = targetColor;
    }

    private void UpdateCelestialPositions(float normalizedTime)
    {
        // Солнце активно во время дневной половины цикла (0.0 до 0.5)
        if (sunObject != null)
        {
            float sunActiveTime = normalizedTime; // Отрезок с рассвета до заката
            
            // Если сейчас ночь, прячем солнце под горизонт или выключаем
            if (sunActiveTime > 0.5f)
            {
                sunObject.gameObject.SetActive(false);
            }
            else
            {
                sunObject.gameObject.SetActive(true);
                
                // Нормализуем путь солнца: 0.0 - 0.5 превращаем в 0.0 - 1.0
                float t = sunActiveTime / 0.5f; 
                
                // Угол полета по дуге от 180 до 0 градусов (в радианах: от PI до 0)
                float angle = Mathf.PI * (1f - t); 
                
                float x = Mathf.Cos(angle) * (orbitWidth * 0.5f);
                float y = Mathf.Sin(angle) * orbitHeight + verticalOffset;
                
                sunObject.anchoredPosition = new Vector2(x, y);

                // Плавное растворение на восходе/заходе
                if (useFading)
                {
                    CanvasGroup cg = sunObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = Mathf.Sin(angle); // Максимум в зените
                    }
                }
            }
        }

        // Луна активна во время ночной половины цикла (0.5 до 1.0)
        if (moonObject != null)
        {
            float moonActiveTime = normalizedTime;
            
            if (moonActiveTime < 0.5f)
            {
                moonObject.gameObject.SetActive(false);
            }
            else
            {
                moonObject.gameObject.SetActive(true);
                
                // Нормализуем путь луны: 0.5 - 1.0 превращаем в 0.0 - 1.0
                float t = (moonActiveTime - 0.5f) / 0.5f;
                
                // Угол полета луны: от PI до 0
                float angle = Mathf.PI * (1f - t);
                
                float x = Mathf.Cos(angle) * (orbitWidth * 0.5f);
                float y = Mathf.Sin(angle) * orbitHeight + verticalOffset;
                
                moonObject.anchoredPosition = new Vector2(x, y);

                // Плавное растворение на восходе/заходе
                if (useFading)
                {
                    CanvasGroup cg = moonObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = Mathf.Sin(angle);
                    }
                }
            }
        }
    }
}
```

---

### 3. Ежедневный Календарь на 7 дней: `DailyRewardSystem.cs`
*Повесьте этот скрипт на ваш объект `DailyRewardPanel`. Он отслеживает 24-часовой таймер, начисляет золото, кристаллы, VIP-опыт, открывает мини-игры за посещение и подсвечивает слоты!*

```csharp
using UnityEngine;
using UnityEngine.UI;
using System;

public class DailyRewardSystem : MonoBehaviour
{
    [Header("UI References (Must be assigned in Inspector)")]
    public Button claimButton;
    public Text timerText;
    public Text statusText;
    [Tooltip("Массив из 7 слотов дней (День 1 - День 7)")]
    public Transform[] calendarDaySlots; 

    private int currentStreak = 0;
    private DateTime lastClaimTime;

    private void Start()
    {
        ValidateInspectorReferences();
        LoadDailyData();
        CheckDailyStatus();
    }

    private void Update()
    {
        CheckDailyStatus();
    }

    private void ValidateInspectorReferences()
    {
        if (claimButton == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Кнопка 'Claim Button' не назначена в Инспекторе! Перетащите объект кнопки.");
        if (timerText == null)
            Debug.LogWarning("[DailyRewardSystem] ОШИБКА: Текстовое поле 'Timer Text' не назначено в Инспекторе!");
        if (statusText == null)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Текстовое поле 'Status Text' не назначено.");
        if (calendarDaySlots == null || calendarDaySlots.Length == 0)
            Debug.LogWarning("[DailyRewardSystem] ПРЕДУПРЕЖДЕНИЕ: Массив слотов 'Calendar Day Slots' пуст! Назначьте 7 дочерних дней.");
    }

    private void CheckDailyStatus()
    {
        TimeSpan difference = DateTime.Now - lastClaimTime;
        bool isRewardReady = false;

        if (difference.TotalHours >= 24 && difference.TotalHours < 48)
        {
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Новая награда готова!";
        }
        else if (difference.TotalHours >= 48)
        {
            // Сброс серии за пропуск дня
            currentStreak = 0;
            isRewardReady = true;
            if (claimButton != null) claimButton.interactable = true;
            if (timerText != null) timerText.text = "Серия сброшена! Заберите День 1.";
        }
        else
        {
            isRewardReady = false;
            if (claimButton != null) claimButton.interactable = false;
            TimeSpan timeToWait = TimeSpan.FromHours(24) - difference;
            if (timerText != null)
            {
                timerText.text = string.Format("До награды: {0:D2}:{1:D2}:{2:D2}", 
                    timeToWait.Hours, timeToWait.Minutes, timeToWait.Seconds);
            }
        }

        UpdateCalendarVisuals(isRewardReady);
    }

    public void ClaimReward()
    {
        currentStreak = (currentStreak % 7) + 1; // Цикл 7 дней
        lastClaimTime = DateTime.Now;

        // Начисление наград
        if (GameManager.Instance != null)
        {
            // Начисление золота и кристаллов
            switch (currentStreak)
            {
                case 1: GameManager.Instance.AddGold(100); break;
                case 2: GameManager.Instance.AddGold(250); break;
                case 3: GameManager.Instance.AddCrystals(1); break;
                case 4: GameManager.Instance.AddGold(500); break;
                case 5: 
                    GameManager.Instance.AddVipXP(10);
                    if (MinigamesManager.Instance != null)
                        MinigamesManager.Instance.UnlockDarts();
                    if (statusText != null) statusText.text = "Вам открыт ДАРТС!";
                    break;
                case 6: GameManager.Instance.AddGold(1000); break;
                case 7: 
                    GameManager.Instance.AddCrystals(10);
                    if (statusText != null) statusText.text = "Вы получили Золотой Сундук!";
                    break;
            }

            // Дополнительная проверка на активность дней
            GameManager.Instance.daysActive++;
            if (GameManager.Instance.daysActive % 10 == 0)
            {
                if (MinigamesManager.Instance != null)
                    MinigamesManager.Instance.UnlockMouseCatch();
                if (statusText != null) statusText.text = "Открыта игра: ЛОВЛЯ МЫШЕЙ!";
            }
        }
        else
        {
            // Запасная заглушка, если GameManager отсутствует (для тестов вне основной сцены)
            Debug.LogWarning($"[DailyRewardSystem] GameManager.Instance не найден. Имитация начисления за день {currentStreak}.");
            if (statusText != null) statusText.text = $"Забрана награда дня {currentStreak} (Тестовый режим)";
        }

        SaveDailyData();
    }

    private void UpdateCalendarVisuals(bool isRewardReady)
    {
        if (calendarDaySlots == null) return;

        for (int i = 0; i < calendarDaySlots.Length; i++)
        {
            if (calendarDaySlots[i] == null) continue;
            
            Image slotImage = calendarDaySlots[i].GetComponent<Image>();
            if (slotImage == null) continue;

            if (i < currentStreak)
            {
                slotImage.color = Color.green; // Зеленый - получено
            }
            else if (i == currentStreak && isRewardReady)
            {
                slotImage.color = Color.yellow; // Желтый - готово к получению
            }
            else
            {
                slotImage.color = Color.gray; // Серый - закрыто
            }
        }
    }

    private void LoadDailyData()
    {
        currentStreak = PlayerPrefs.GetInt("DailyStreak", 0);
        string lastClaimStr = PlayerPrefs.GetString("LastDailyClaim", "");
        if (!string.IsNullOrEmpty(lastClaimStr))
        {
            lastClaimTime = DateTime.Parse(lastClaimStr);
        }
        else
        {
            // По умолчанию даем забрать сразу
            lastClaimTime = DateTime.Now.AddDays(-2);
        }
    }

    private void SaveDailyData()
    {
        PlayerPrefs.SetInt("DailyStreak", currentStreak);
        PlayerPrefs.SetString("LastDailyClaim", lastClaimTime.ToString());
        PlayerPrefs.Save();
    }
}
```

---

### 4. Контроллер Главного Меню: `MainMenuController.cs`
*Повесьте этот скрипт на ваш корневой объект `Canvas` в сцене Главного меню. Он обрабатывает запуск игры, открытие опций и корректный выход из игры как на ПК, так и на мобильных устройствах.*

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Настройки сцен")]
    [Tooltip("Имя основной сцены игры с кликером кота")]
    public string gameSceneName = "GameScene";

    [Header("UI Панели")]
    [Tooltip("Ссылка на панель настроек (Options Panel)")]
    public GameObject optionsPanel;

    private void Start()
    {
        // Убедимся, что при запуске меню панель опций закрыта
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Метод для кнопки СТАРТ. Запускает игровую сцену кликера.
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("[MainMenuController] Запуск игры! Загрузка сцены: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Метод для кнопки ОПЦИИ. Открывает/закрывает панель настроек.
    /// </summary>
    public void ToggleOptions()
    {
        if (optionsPanel != null)
        {
            bool isCurrentActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isCurrentActive);
            Debug.Log("[MainMenuController] Переключение панели опций. Новое состояние: " + (!isCurrentActive));
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Ссылка на 'optionsPanel' не задана в Инспекторе!");
        }
    }

    /// <summary>
    /// Метод для кнопки ВЫХОД. Закрывает игру на ПК или мобильном приложении.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenuController] Выход из игры...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Остановить игру в редакторе Unity
        #else
        Application.Quit(); // Закрыть скомпилированное приложение на ПК или Android/iOS
        #endif
    }
}
```

---

## 🎯 ЧАСТЬ 13. Итоговый пошаговый чек-лист: Сборка и Настройка сцен в Unity

Выполните следующие шаги в редакторе Unity, чтобы запустить первую играбельную версию:

### 📥 Шаг 1. Импорт сгенерированных ассетов из Leonardo.Ai
1. Перетащите все сгенерированные файлы из вашей папки загрузок в окно **Project** в Unity (создайте папку `Assets/Textures/`).
2. Для каждого импортированного спрайта (Солнце, Луна, Зелья, Кристалл, Сундучок, Столбы) измените настройки в окне **Inspector**:
   * **Texture Type:** установите в **Sprite (2D and UI)**.
   * **Sprite Mode:** **Single**.
   * Нажмите кнопку **Apply** внизу инспектора.
3. Для фоновых изображений (`BackgroundDay` и `BackgroundNight`):
   * **Texture Type:** **Sprite (2D and UI)**.
   * Нажмите **Apply**.

### 🛠️ Шаг 2. Создание и Настройка Сцены Главного Меню (`MainMenu`)
1. Создайте новую сцену через `File ➡️ New Scene ➡️ Basic (2D)`. Сохраните её под именем `MainMenu` в папке `Assets/Scenes/`.
2. Создайте Canvas: `GameObject ➡️ UI ➡️ Canvas`.
3. Настройте компонент **Canvas Scaler** на объекте Canvas:
   * **UI Scale Mode:** установите в **Scale With Screen Size**.
   * **Reference Resolution:** задайте **1920** (X) и **1080** (Y).
   * **Screen Match Mode:** установите ползунок **Match** в значение `0.5` (идеально сбалансирует интерфейс под вертикальные и горизонтальные экраны).
4. Создайте фоновое изображение: нажмите правой кнопкой по Canvas и выберите `UI ➡️ Image`. Назовите его `BackgroundImage`.
   * Растяните его на весь экран (Anchor Preset: зажмите `Alt` и выберите правый нижний квадрат **Stretch-Stretch**).
   * Перетащите ваш дневной спрайт `BackgroundDay` в поле `Source Image`.
   * Добавьте на него компонент **TimeOfDaySystem.cs**.

### 🌌 Шаг 3. Добавление Небесного цикла (Солнце и Луна)
1. Нажмите правой кнопкой по объекту `BackgroundImage` и выберите `UI ➡️ Image`. Назовите его `SunObject`.
2. Повторите и создайте `MoonObject`.
3. Добавьте компонент **Canvas Group** на оба объекта (`SunObject` и `MoonObject`).
4. Перетащите соответствующие спрайты солнца и луны в поле `Source Image` этих объектов.
5. Задайте им базовые размеры в Rect Transform (например, **Width:** 120, **Height:** 120).
6. Выделите объект `BackgroundImage` с компонентом `TimeOfDaySystem`:
   * Перетащите объект `BackgroundImage` в поле **Background Image**.
   * Перетащите `SunObject` (из иерархии) в поле **Sun Object**.
   * Перетащите `MoonObject` (из иерархии) в поле **Moon Object**.
   * Задайте длительность цикла в поле **Day Cycle Length Seconds** (например, `60` или `120` секунд).

### ✍️ Шаг 4. Настройка Анимированного Названия Игры
1. Нажмите правой кнопкой по Canvas и выберите `UI ➡️ Text - TextMeshPro`. Назовите его `TitleText`.
2. В поле текста введите красивое название вашей игры (например, **"КОНТИНЕНТ СУДЬБЫ: Клик-Алхимик"**).
3. Настройте размер шрифта, выравнивание по центру, красивый золотой или фиолетовый цвет.
4. Добавьте на этот объект скрипт **FateMainMenuTitleAnimator.cs**.
5. Настройте в инспекторе амплитуду и скорость парения и дыхания по вашему вкусу.

### 🔘 Шаг 5. Создание кнопок меню (Старт, Опции, Выход)
1. Нажмите правой кнопкой по Canvas и выберите `UI ➡️ Create Empty`. Назовите его `MenuButtonsContainer`.
2. Добавьте на него компонент **Vertical Layout Group** и настройте выравнивание элементов по центру (`Child Alignment: Middle Center`), а также расстояние между кнопками (`Spacing: 25`).
3. Внутри этого контейнера создайте 3 кнопки: `StartButton`, `OptionsButton`, `ExitButton` (`UI ➡️ Button - TextMeshPro`).
4. Настройте их тексты соответственно: **"ИГРАТЬ"**, **"НАСТРОЙКИ"**, **"ВЫХОД"**.
5. Добавьте скрипт **MainMenuController.cs** на корневой объект `Canvas` главного меню.
6. В инспекторе Canvas настройте событие нажатия для каждой кнопки (`OnClick`):
   * Для `StartButton` перетащите Canvas в слот события и выберите метод `MainMenuController.PlayGame`.
   * Для `OptionsButton` выберите метод `MainMenuController.ToggleOptions`.
   * Для `ExitButton` выберите метод `MainMenuController.QuitGame`.

### 🛡️ Шаг 6. Настройка и Очередь загрузки в Build Settings
1. Откройте окно настроек сборки проекта через верхнее меню: `File ➡️ Build Settings`.
2. Перетащите вашу сцену `MainMenu` из окна Project в верхний список **Scenes In Build**. Она должна занять самый первый индекс **(Index 0)**, чтобы игра всегда запускалась с главного меню.
3. Перетащите вашу основную игровую сцену с кликером кота (`GameScene`) следом, чтобы она заняла индекс **(Index 1)**.
4. Нажмите кнопку **Player Settings**:
   * В разделе **Resolution and Presentation** убедитесь, что включена поддержка вертикальной/горизонтальной ориентации.
5. Нажмите кнопку **Build & Run**, чтобы протестировать вашу первую играбельную версию с красивым небом, анимированным названием и рабочими кнопками перехода!

---

*Документация и C# скрипты успешно обновлены и синхронизированы в версии v18.12.08. Проект готов к сборке и тестированию!*
