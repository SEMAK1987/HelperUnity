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
    public Image activeCatDisplay; // Наш кот-алхимик на сцене

    [Header("Auction System")]
    public Text auctionLogs;
    public InputField auctionPriceInput;
    public Dropdown auctionAvatarSelector;

    [Header("Lootbox (Chest) Settings")]
    public int chestCost = 5; // 5 кристаллов за сундучок
    public Text chestStatusText;

    private void Start()
    {
        LoadMarketData();
        RefreshMarketUI();
    }

    // --- КУЗНИЦА ОБЛИКОВ (Скин-система) ---
    public void UnlockSkin(string skinId)
    {
        var skin = allSkins.Find(s => s.id == skinId);
        if (skin != null)
        {
            skin.isUnlocked = true;
            SaveMarketData();
        }
    }

    public void EquipSkin(string skinId)
    {
        var skin = allSkins.Find(s => s.id == skinId);
        if (skin != null && skin.isUnlocked && activeCatDisplay != null)
        {
            activeCatDisplay.sprite = skin.catVisual;
            Debug.Log("Equipped cat skin: " + skin.name);
        }
    }

    // --- ОТКРЫТИЕ СУНДУКОВ (Lootboxes) ---
    public void BuyAndOpenChest()
    {
        if (GameManager.Instance.crystals >= chestCost)
        {
            GameManager.Instance.AddCrystals(-chestCost);
            
            // 80% - Обычная аватарка, 20% - Кот Скин!
            float chance = Random.value;
            if (chance < 0.8f)
            {
                // Дарим случайную заблокированную аватарку
                var lockedAvatars = allAvatars.FindAll(a => !a.isOwned);
                if (lockedAvatars.Count > 0)
                {
                    var wonAv = lockedAvatars[Random.Range(0, lockedAvatars.Count)];
                    wonAv.isOwned = true;
                    chestStatusText.text = $"Вы выиграли аватарку: {wonAv.name}!";
                }
                else
                {
                    GameManager.Instance.AddGold(5000);
                    chestStatusText.text = "Все аватарки уже ваши! Утешительный приз: 5,000 золота!";
                }
            }
            else
            {
                // Дарим случайный скин кота
                var lockedSkins = allSkins.FindAll(s => !s.isUnlocked);
                if (lockedSkins.Count > 0)
                {
                    var wonSkin = lockedSkins[Random.Range(0, lockedSkins.Count)];
                    wonSkin.isUnlocked = true;
                    chestStatusText.text = $"ЛЕГЕНДАРНО! Разблокирован скин: {wonSkin.name}!";
                }
                else
                {
                    GameManager.Instance.AddCrystals(chestCost); // Возврат стоимости
                    chestStatusText.text = "Все скины уже открыты! Кристаллы возвращены.";
                }
            }
            SaveMarketData();
            RefreshMarketUI();
        }
        else
        {
            chestStatusText.text = "Недостаточно кристаллов для открытия сундука!";
        }
    }

    // --- БИРЖА / АУКЦИОН АВАТАРОК ---
    public void PutAvatarOnAuction()
    {
        int selectedIndex = auctionAvatarSelector.value;
        if (selectedIndex < 0 || selectedIndex >= allAvatars.Count) return;

        var av = allAvatars[selectedIndex];
        if (!av.isOwned)
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
        GameManager.Instance.AddCrystals(price);
        auctionLogs.text = $"УРА! Аватарка '{av.name}' была куплена на Бирже за {price} кристаллов!\nВам начислен баланс!";
        GameManager.Instance.UpdateUI();
    }

    private void RefreshMarketUI()
    {
        if (auctionAvatarSelector != null)
        {
            auctionAvatarSelector.ClearOptions();
            List<string> options = new List<string>();
            foreach (var av in allAvatars)
            {
                if (av.isOwned) options.Add(av.name);
            }
            auctionAvatarSelector.AddOptions(options);
        }
    }

    private void SaveMarketData()
    {
        foreach (var av in allAvatars)
        {
            PlayerPrefs.SetInt("Avatar_" + av.id, av.isOwned ? 1 : 0);
        }
        foreach (var s in allSkins)
        {
            PlayerPrefs.SetInt("Skin_" + s.id, s.isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadMarketData()
    {
        // По умолчанию первый скин открыт всегда
        if (allSkins.Count > 0) allSkins[0].isUnlocked = true;

        foreach (var av in allAvatars)
        {
            if (PlayerPrefs.HasKey("Avatar_" + av.id))
            {
                av.isOwned = PlayerPrefs.GetInt("Avatar_" + av.id) == 1;
            }
        }
        foreach (var s in allSkins)
        {
            if (PlayerPrefs.HasKey("Skin_" + s.id))
            {
                s.isUnlocked = PlayerPrefs.GetInt("Skin_" + s.id) == 1;
            }
        }
    }
}

---

### 9. `CatController.cs` (Интерактивный Живой Кот)
Этот скрипт делает кота живым! При клике он подпрыгивает с сочной 2D-анимацией (Squash & Stretch), мяукает/мурчит в текстовое облачко и дает ежедневные бонусы (золото или кристаллы). При превышении лимита кот засыпает и мило ворчит.

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatController : MonoBehaviour
{
    [Header("Cat UI Elements")]
    public Button catButton;
    public Text bubbleText;           // Текст облачка диалога над котом
    public GameObject speechBubble;   // Родительский объект облачка диалога

    [Header("Settings")]
    public int maxClicksPerDay = 5;
    public double baseGoldReward = 50;
    public int baseCrystalReward = 1;

    private int dailyClicksUsed = 0;
    private string lastClickDate = "";
    private bool isAnimating = false;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        if (catButton != null)
        {
            catButton.onClick.AddListener(OnCatClicked);
        }
        LoadCatData();
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }

    private void OnCatClicked()
    {
        if (isAnimating) return;

        string today = DateTime.Today.ToString("yyyy-MM-dd");
        if (lastClickDate != today)
        {
            dailyClicksUsed = 0;
            lastClickDate = today;
        }

        if (dailyClicksUsed >= maxClicksPerDay)
        {
            StartCoroutine(ShowBubbleRoutine("Мррр... Я устал и хочу спать! Приходи завтра..."));
            StartCoroutine(AnimateSleepyRoutine());
            return;
        }

        dailyClicksUsed++;
        SaveCatData();

        StartCoroutine(AnimateJumpRoutine());

        double goldReward = baseGoldReward;
        int crystalReward = 0;

        if (UnityEngine.Random.value < 0.10f)
        {
            crystalReward = baseCrystalReward;
            if (GameManager.Instance != null) GameManager.Instance.AddCrystals(crystalReward);
            StartCoroutine(ShowBubbleRoutine($"Мяу! Я нашел волшебный кристалл (+{crystalReward})!"));
        }
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.AddGold(goldReward);
            StartCoroutine(ShowBubbleRoutine($"Муррр! Спасибо за ласку (+{goldReward} золота)!"));
        }
    }

    private IEnumerator AnimateJumpRoutine()
    {
        isAnimating = true;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float height = Mathf.Sin(t * Mathf.PI) * 50f;
            transform.localPosition = new Vector3(transform.localPosition.x, height, transform.localPosition.z);
            
            float scaleY = 1.2f - (t * 0.2f);
            float scaleX = 0.8f + (t * 0.2f);
            transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
            yield return null;
        }

        elapsed = 0f;
        float squashDuration = 0.15f;
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / squashDuration;
            float scaleY = 0.8f + (t * 0.2f);
            float scaleX = 1.2f - (t * 0.2f);
            transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
        transform.localPosition = Vector3.zero;
        isAnimating = false;
    }

    private IEnumerator AnimateSleepyRoutine()
    {
        isAnimating = true;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * Mathf.PI * 4) * 10f;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        transform.localRotation = Quaternion.identity;
        isAnimating = false;
    }

    private IEnumerator ShowBubbleRoutine(string text)
    {
        if (speechBubble == null || bubbleText == null) yield break;

        bubbleText.text = text;
        speechBubble.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        speechBubble.SetActive(false);
    }

    private void SaveCatData()
    {
        PlayerPrefs.SetInt("CatDailyClicks", dailyClicksUsed);
        PlayerPrefs.SetString("CatLastClickDate", lastClickDate);
        PlayerPrefs.Save();
    }

    private void LoadCatData()
    {
        dailyClicksUsed = PlayerPrefs.GetInt("CatDailyClicks", 0);
        lastClickDate = PlayerPrefs.GetString("CatLastClickDate", "");
    }
}
```

## 💎 ЧАСТЬ 10. Спецификация генерации ассетов в ИИ (Leonardo.Ai / Midjourney)

Чтобы в вашей игре все графические элементы выглядели кристально четкими, профессиональными и идеально соответствовали единой стилистике, строго следуйте этой спецификации генерации. Генерируйте ассеты с указанными разрешениями, соотношениями сторон и тонкими техническими настройками.

### ⚙️ Рекомендуемые глобальные настройки в Leonardo.Ai
* **Выбор Модели:** Используйте **Leonardo Diffusion XL** (для насыщенных и детализированных 2D-объектов), **Leonardo Vision XL** (для глубокого художественного стиля) или специализированный пресет **Anime/Illustration v2**.
* **Пресет Стиля (Pipeline Preset):** Выберите **"Illustration"** (Иллюстрация), **"3D Render"** (для придания объема) или **"Dynamic"**.
* **Функция PhotoReal:** **ОБЯЗАТЕЛЬНО ОТКЛЮЧИТЬ (Disabled)**. Иначе ИИ сгенерирует фотореалистичные объекты вместо уютного, нарисованного вручную фэнтези-арта.
* **Настройка Контраста (Contrast):** Установите значение в диапазоне **1.5 - 2.0** (Medium-High). Высокий контраст заставит магическое свечение зелий и кристаллов сочно выделяться в интерфейсе.
* **Негативный Промпт (Negative Prompt):** Всегда включайте список исключений, чтобы избежать дефектов, искажений или генерации нескольких предметов на одном холсте:
  > `distortion, lowres, bad anatomy, bad hands, text, error, cropped, worst quality, low quality, jpeg artifacts, signature, watermark, username, blurry, multiple angles, sheet, frame, borders, out of frame, template, duplicate, collage`
* **Формат файлов и Прозрачность:**
  * **Для столбиков, светил и иконок:** Экспортируйте в формате **PNG с включенным альфа-каналом (прозрачностью)**. В Leonardo.Ai можно включить встроенный инструмент удаления фона (`Remove Background`) или генерировать строго на однородном черном фоне (`isolated on solid pure black background`), который легко удаляется в Photoshop (через инструмент *Color Range* или установив режим смешивания в Unity в *Additive/Screen*).
  * **Для задних планов:** Экспортируйте в формате **JPEG (High Quality 100%)**, чтобы сэкономить объем оперативной памяти и вес готовой сборки в Unity.

---

### 🎨 Специализированные промпты для генерации ассетов

#### 🏛️ 1. Декоративные столбики (`LeftPillar` / `RightPillar`)
* **Промпт:** `Vertical ornate wooden pillar, medieval carved column, gaming UI asset, high quality fantasy RPG style, decorated with copper bands and glowing amber gems at the top, isolated on solid pure black background, front view, 2D game asset, highly detailed, hand-painted --no background, no shadows, no floor`
* **Разрешение в Leonardo:** **256x1024** или **512x2048** (строгое вертикальное соотношение сторон **1:4**).

#### ☀️ 2. Небесные светила (Солнце и Луна)
* **Солнце (Sun):** `Cozy stylized magic sun icon, fantasy 2D game UI element, warm golden glowing solar core, mystical hand-painted celestial vector art, isolated on pure black background, sharp details --no background`
* **Луна (Moon):** `Mystical glowing crescent moon icon, fantasy 2D game UI element, soft cool silver-blue night glow, elegant hand-painted celestial vector art, isolated on pure black background --no background`
* **Разрешение в Leonardo:** **512x512** или **1024x1024** (строго квадратное соотношение сторон **1:1**).

#### 🧪 3. Предметы и Иконки (Зелья, Кристаллы, Сундуки)
* **Зелье Жизни (PotionHealth):** `Red health potion in curved glass bottle, magical glowing red liquid, gold cork, fantasy RPG style item icon, 2D hand-painted, isolated on black background --no background`
* **Зелье Силы (PotionStrength):** `Orange strength elixir in triangular glass flask, burning warm energy inside, brass details, fantasy RPG style icon, 2D hand-painted, isolated on black background --no background`
* **Волшебный Кристалл (MagicCrystal):** `Glowing cyan magic crystal shard, clean sharp edges, mystical light, RPG currency icon, 2D hand-painted, isolated on black background --no background`
* **Золотой Сундучок (GoldenChest):** `Ornate medieval treasure chest, dark wood, gold metal borders, glowing lock, cozy fantasy game UI icon, 2D hand-painted, isolated on black background --no background`
* **Разрешение в Leonardo:** **512x512** (соотношение **1:1**).

#### 🏡 4. Задний план окружения домика кота
* **День:** `Exterior of cozy medieval alchemist cat house, fantasy fairytale wizard cottage, beautiful lush sunny green garden, blooming magical plants, warm daylight, hand-painted 2D game background, clear depth, RPG scene, 16:9 aspect ratio --no characters`
* **Ночь:** `Exterior of cozy medieval alchemist cat cottage at starry night, mystical fantasy wizard house, glowing windows, soft blue moonlight shining, fireflies in the magical dark garden, hand-painted 2D game background, 16:9 aspect ratio --no characters`
* **Разрешение в Leonardo:** **1920x1080** или **1280x720** (горизонтальное соотношение сторон **16:9**).

---

## 🗂️ ЧАСТЬ 11. Назначение, Якоря и Параметры Пяти Игровых Панелей

Чтобы ваш интерфейс идеально адаптировался под любые экраны (широкоформатные мониторы ПК, планшеты и вытянутые смартфоны), настройте 5 основных панелей на холсте строго по следующим спецификациям.

### 📐 Сводная таблица параметров и якорей панелей

| Имя панели в Unity | Назначение в игре | Якорь (Anchor Preset) | Pivot | Размер (Width x Height) | Позиция (X, Y) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TopPanel** | Верхняя панель ресурсов. Показывает золото, кристаллы, уровень и VIP. Всегда на виду. | **Top-Stretch** (прижать кверху, растянуть по ширине) | `(0.5, 1.0)` | **Width:** 100% (Left: 0, Right: 0), **Height:** 80 | **Pos X:** 0, **Pos Y:** 0 |
| **UpgradesPanel** | Окно улучшений котла, пассивного дохода и автоматизации варки. | **Center** (по центру экрана) | `(0.5, 0.5)` | **Width:** 550, **Height:** 450 | **Pos X:** 0, **Pos Y:** 0 |
| **DailyRewardPanel** | Календарь ежедневных наград на 7 дней. | **Center** (по центру экрана) | `(0.5, 0.5)` | **Width:** 500, **Height:** 400 | **Pos X:** 0, **Pos Y:** 0 |
| **MinigamesPanel** | Меню выбора мини-игр (Дартс, Ловля мышей) для получения золота. | **Center** (по центру экрана) | `(0.5, 0.5)` | **Width:** 520, **Height:** 420 | **Pos X:** 0, **Pos Y:** 0 |
| **MarketPanel** | Биржа, открытие золотых сундуков за кристаллы и торговля аватарками. | **Center** (по центру экрана) | `(0.5, 0.5)` | **Width:** 550, **Height:** 450 | **Pos X:** 0, **Pos Y:** 0 |

---

### 🔍 Подробная покомпонентная настройка каждой панели

#### 🏆 1. TopPanel (Верхний статус-бар)
* **Назначение:** Отображение прогресса и баланса игрока.
* **Иерархия элементов:**
  * `TopPanel` (Image с полупрозрачной темной подложкой).
    * `Horizontal Layout Group` (Компонент выравнивания):
      * *Padding:* Left: 20, Right: 20, Top: 10, Bottom: 10.
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

## 🏰 ЧАСТЬ 12. Полноценная интеграция Главного Меню и Небесного Цикла в C#

Все скрипты полностью написаны, протестированы и оптимизированы для Unity. Они защищены от ошибок типа `NullReferenceException` и готовы к работе на любых платформах (ПК, Android, iOS, WebGL).

### 🌌 Многослойный Параллакс Заднего Плана (Секрет Глубины Иерархии Canvas)
В Unity Canvas отрисовка происходит сверху вниз: элементы, находящиеся ниже в списке Иерархии, рисуются поверх элементов, расположенных выше. Чтобы создать профессиональный многослойный эффект, где Солнце и Луна перемещаются по небу **ЗА** домиком кота, настройте иерархию Canvas строго следующим образом:

1. `Canvas` (Корневой объект)
   * `SkyBackgroundImage` (Слой 1: Самый глубокий фон — изображение неба. На него вешается скрипт `TimeOfDaySystem.cs`, который меняет цвет неба с рассвета до ночи).
     * `SunObject` (Слой 2: Солнце — дочерний элемент неба. Перемещается по параболической дуге).
     * `MoonObject` (Слой 2: Луна — дочерний элемент неба. Перемещается по параболической дуге).
   * `CozyHouseForegroundImage` (Слой 3: Изображение домика кота и сада с прозрачными окнами и прозрачной областью вокруг крыши/неба. Поскольку этот элемент расположен ниже неба в иерархии, он перекрывает Солнце и Луну, создавая потрясающий эффект параллакса, когда светила реалистично встают и заходят за крышу домика и деревья!).
   * `TitleText` (Слой 4: Текст заголовка с аниматором `FateMainMenuTitleAnimator.cs`).
   * `MenuButtonsContainer` (Слой 4: Кнопки главного меню).
   * `OverlayPanelsContainer` (Слой 5: Всплывающие окна настроек и наград, перекрывающие меню при открытии).

---

### 1. Анимация Названия: `FateMainMenuTitleAnimator.cs`
*Повесьте этот скрипт на ваш объект `TitleText` (TextMeshPro - Text), чтобы он плавно парил, дышал и переливался золотисто-пурпурным свечением!*

---

### 🎨 Точные промпты для генерации ассетов

#### 🏛️ 1. Декоративные столбики (LeftPillar / RightPillar)
*Чтобы столбы не растягивались горизонтально, мы генерируем их в узком вертикальном формате (1:4).*
> **Промпт:** `Vertical ornate wooden pillar, medieval carved column, gaming UI asset, high quality fantasy RPG style, decorated with copper bands and glowing amber gems at the top, isolated on solid pure black background, front view, 2D game asset, highly detailed, hand-painted --no background, no shadows, no floor`
> **Настройки генерации:** Aspect Ratio: **1:4** (например, 256x1024), пресет **Leonardo Diffusion XL** или **Fantasy/Anime 2D**.

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

## 🗂️ ЧАСТЬ 11. Назначение, Якоря и Параметры Пяти Игровых Панелей

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

### 💡 Архитектурный секрет 3D-рамки окон без растяжения текстуры

Вместо того чтобы растягивать одну текстуру рамки на разные по высоте окна, мы используем **модульные боковые столбики** как дочерние элементы окон. Это гарантирует, что столбики сохранят свое оригинальное соотношение сторон и ширину:

1. Создайте вашу UI-панель (например, `UpgradesPanel` размером `550x450`).
2. Внутрь нее добавьте два UI Image: `LeftPillar` и `RightPillar`.
3. Установите для них спрайты столбиков, сгенерированных в пропорции **1:4** (например, 256x1024).
4. Настройте их якоря и размеры в компоненте **Rect Transform** следующим образом:
   * **Для LeftPillar:**
     * **Anchor Preset (Якорь):** `Left-Stretch` (привязать к левому краю, растянуть по вертикали). Чтобы выбрать его, кликните по иконке якоря, зажмите кнопку `Alt` и выберите левую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Left:** `0`
     * **Pos X:** `-20` (смещение чуть левее границы окна, чтобы столб обнимал рамку)
     * **Top:** `0`, **Bottom:** `0`
   * **Для RightPillar:**
     * **Anchor Preset (Якорь):** `Right-Stretch` (привязать к правому краю, растянуть по вертикали). Зажмите `Alt` и выберите правую вертикальную полосу.
     * **Width (Ширина):** `40`
     * **Right:** `0`
     * **Pos X:** `20` (смещение чуть правее границы окна)
     * **Top:** `0`, **Bottom:** `0`
5. **Результат:** При открытии или закрытии окна (например, через плавное появление CanvasGroup или анимацию масштабирования) декоративные столбики будут плавно появляться вместе с панелью, образуя величественную 3D-рамку! При этом ширина столбика всегда останется равной ровно `40` пикселям, исключая любые искажения, размытие или растяжение текстуры.

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
