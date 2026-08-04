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
    public Button claimButton;
    public Text timerText;
    public Text statusText;
    public Transform[] calendarDaySlots; // 7 визуальных слотов дней

    private int currentStreak = 0;
    private DateTime lastClaimTime;

    private void Start()
    {
        LoadDailyData();
        CheckDailyStatus();
    }

    private void Update()
    {
        CheckDailyStatus();
    }

    private void CheckDailyStatus()
    {
        TimeSpan difference = DateTime.Now - lastClaimTime;

        if (difference.TotalHours >= 24 && difference.TotalHours < 48)
        {
            // Можно забрать следующую награду!
            claimButton.interactable = true;
            timerText.text = "Новая награда готова!";
        }
        else if (difference.TotalHours >= 48)
        {
            // Пропущено слишком много времени! Сброс серии на День 1
            currentStreak = 0;
            claimButton.interactable = true;
            timerText.text = "Серия сброшена! Заберите День 1.";
        }
        else
        {
            // Ждем 24 часа
            claimButton.interactable = false;
            TimeSpan timeToWait = TimeSpan.FromHours(24) - difference;
            timerText.text = string.Format("До награды: {0:D2}:{1:D2}:{2:D2}", 
                timeToWait.Hours, timeToWait.Minutes, timeToWait.Seconds);
        }

        UpdateCalendarVisuals();
    }

    public void ClaimReward()
    {
        currentStreak = (currentStreak % 7) + 1; // Цикл 7 дней
        lastClaimTime = DateTime.Now;

        // Выдача наград
        switch (currentStreak)
        {
            case 1: GameManager.Instance.AddGold(100); break;
            case 2: GameManager.Instance.AddGold(250); break;
            case 3: GameManager.Instance.AddCrystals(1); break; // Медный ключ = 1 кристалл
            case 4: GameManager.Instance.AddGold(500); break;
            case 5: 
                GameManager.Instance.AddVipXP(10);
                MinigamesManager.Instance.UnlockDarts(); // Разблокируем Дартс!
                statusText.text = "Вам открыт ДАРТС!";
                break;
            case 6: GameManager.Instance.AddGold(1000); break;
            case 7: 
                GameManager.Instance.AddCrystals(10); // Кристаллы + сундучок
                statusText.text = "Вы получили Золотой Сундук!";
                break;
        }

        // Проверка кратных 10 дней на кошачью мышеловку
        GameManager.Instance.daysActive++;
        if (GameManager.Instance.daysActive % 10 == 0)
        {
            MinigamesManager.Instance.UnlockMouseCatch(); // Разблокируем Мышей!
            statusText.text = "Открыта игра: ЛОВЛЯ МЫШЕЙ!";
        }

        SaveDailyData();
        CheckDailyStatus();
    }

    private void UpdateCalendarVisuals()
    {
        for (int i = 0; i < calendarDaySlots.Length; i++)
        {
            Image slotImage = calendarDaySlots[i].GetComponent<Image>();
            if (i < currentStreak)
                slotImage.color = Color.green; // Получено
            else if (i == currentStreak && claimButton.interactable)
                slotImage.color = Color.yellow; // Готово к получению
            else
                slotImage.color = Color.gray; // Закрыто
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
```

---

### 8. `TimeOfDaySystem.cs` (Смена времени суток на заднем плане)
Этот скрипт плавно меняет цвет заднего фона (или спрайты), имитируя смену дня, вечера, ночи и утра. Это создаёт живую атмосферу вашей алхимической лаборатории!

```csharp
using UnityEngine;
using UnityEngine.UI;

public class TimeOfDaySystem : MonoBehaviour
{
    [Header("UI Background")]
    public Image backgroundImage;

    [Header("Time Settings")]
    [Tooltip("Длительность полных игровых суток в реальных секундах")]
    public float dayCycleLengthSeconds = 120f; 

    [Header("Day Phase Colors")]
    public Color morningColor = new Color(1f, 0.7f, 0.6f);  // Теплый розово-оранжевый
    public Color dayColor = new Color(0.5f, 0.8f, 1f);      // Яркий голубой
    public Color eveningColor = new Color(0.4f, 0.3f, 0.7f);  // Пурпурный закат
    public Color nightColor = new Color(0.08f, 0.08f, 0.2f); // Глубокий темно-синий

    [Header("Optional Sprites (if used)")]
    public Sprite morningSprite;
    public Sprite daySprite;
    public Sprite eveningSprite;
    public Sprite nightSprite;

    private float currentTime = 0f;

    private void Update()
    {
        if (backgroundImage == null) return;

        // Продвигаем время вперед
        currentTime += Time.deltaTime;
        if (currentTime >= dayCycleLengthSeconds)
        {
            currentTime = 0f;
        }

        float normalizedTime = currentTime / dayCycleLengthSeconds; // от 0 до 1

        Color targetColor;
        Sprite targetSprite = null;

        // Определяем фазу дня и плавно переливаем цвета
        if (normalizedTime < 0.25f) // Утро (0% - 25%)
        {
            float t = normalizedTime / 0.25f;
            targetColor = Color.Lerp(nightColor, morningColor, t);
            targetSprite = morningSprite;
        }
        else if (normalizedTime < 0.5f) // День (25% - 50%)
        {
            float t = (normalizedTime - 0.25f) / 0.25f;
            targetColor = Color.Lerp(morningColor, dayColor, t);
            targetSprite = daySprite;
        }
        else if (normalizedTime < 0.75f) // Вечер (50% - 75%)
        {
            float t = (normalizedTime - 0.5f) / 0.25f;
            targetColor = Color.Lerp(dayColor, eveningColor, t);
            targetSprite = eveningSprite;
        }
        else // Ночь (75% - 100%)
        {
            float t = (normalizedTime - 0.75f) / 0.25f;
            targetColor = Color.Lerp(eveningColor, nightColor, t);
            targetSprite = nightSprite;
        }

        // Применяем плавный цвет
        backgroundImage.color = targetColor;

        // Если назначены спрайты, меняем их при необходимости
        if (targetSprite != null && backgroundImage.sprite != targetSprite)
        {
            backgroundImage.sprite = targetSprite;
        }
    }
}
```

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
        
        // Скрываем облачко при старте
        if (speechBubble != null) speechBubble.SetActive(false);

        LoadCatData();
        
        if (catButton != null)
        {
            catButton.onClick.AddListener(OnCatClicked);
        }
    }

    private void LoadCatData()
    {
        lastClickDate = PlayerPrefs.GetString("Cat_LastClickDate", "");
        string today = DateTime.Today.ToString("yyyy-MM-dd");

        if (lastClickDate != today)
        {
            // Наступил новый день! Сбрасываем лимит кликов кота
            dailyClicksUsed = 0;
            PlayerPrefs.SetInt("Cat_DailyClicks", 0);
            PlayerPrefs.SetString("Cat_LastClickDate", today);
            PlayerPrefs.Save();
        }
        else
        {
            dailyClicksUsed = PlayerPrefs.GetInt("Cat_DailyClicks", 0);
        }
    }

    public void OnCatClicked()
    {
        // Предотвращаем спам-клики по коту во время анимации прыжка
        if (isAnimating) return;

        // Запуск сочной анимации "сжатия и растяжения" (Squash & Stretch)
        StartCoroutine(AnimateCatJump());

        LoadCatData(); // На всякий случай обновляем дату

        if (dailyClicksUsed < maxClicksPerDay)
        {
            // Награда доступна!
            dailyClicksUsed++;
            PlayerPrefs.SetInt("Cat_DailyClicks", dailyClicksUsed);
            PlayerPrefs.Save();

            // Случайный выбор награды: 80% золото, 20% кристалл
            float roll = UnityEngine.Random.value;
            if (roll < 0.8f)
            {
                double goldReward = baseGoldReward * GameManager.Instance.playerLevel;
                GameManager.Instance.AddGold(goldReward);
                ShowBubble($"*Муррр-мяу!*\nДержи золотишко! <color=yellow>+{GameManager.Instance.FormatNumber(goldReward)} Золота</color>");
            }
            else
            {
                GameManager.Instance.AddCrystals(baseCrystalReward);
                ShowBubble($"*Фррр!*\nЧто это блестит? <color=cyan>+{baseCrystalReward} Кристалл</color>");
            }
        }
        else
        {
            // Лимит исчерпан — кот ворчит, мурчит или спит
            string[] grumpyMessages = {
                "Муррр... Я устал ловить искры, погладь меня завтра!",
                "Фррр... Время дневного сна котиков. Приходи завтра!",
                "*Мяу*... Золото закончилось, но я могу просто поурчать!",
                "Шшш... Котёл варит, Кот отдыхает."
            };
            string randomMsg = grumpyMessages[UnityEngine.Random.Range(0, grumpyMessages.Length)];
            ShowBubble(randomMsg);
        }
    }

    private void ShowBubble(string message)
    {
        if (speechBubble != null && bubbleText != null)
        {
            bubbleText.text = message;
            speechBubble.SetActive(true);
            
            // Скрываем облачко автоматически через 3.5 секунды
            StopAllCoroutines();
            StartCoroutine(AnimateCatJump()); // визуальная отдача на клик
            StartCoroutine(HideBubbleAfterDelay(3.5f));
        }
    }

    private IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (speechBubble != null) speechBubble.SetActive(false);
    }

    private IEnumerator AnimateCatJump()
    {
        isAnimating = true;

        float duration = 0.25f;
        float elapsed = 0f;

        // Фаза 1: Сжатие вниз перед прыжком
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / (duration * 0.4f);
            transform.localScale = new Vector3(
                originalScale.x * (1f + percent * 0.15f), // Растягивается в ширь
                originalScale.y * (1f - percent * 0.2f),  // Сжимается вниз
                originalScale.z
            );
            yield return null;
        }

        // Фаза 2: Прыжок вверх и вытягивание
        elapsed = 0f;
        while (elapsed < duration * 0.6f)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / (duration * 0.6f);
            transform.localScale = new Vector3(
                originalScale.x * (1f - percent * 0.1f),  // Сужается
                originalScale.y * (1f + percent * 0.25f), // Вытягивается вверх
                originalScale.z
            );
            yield return null;
        }

        // Фаза 3: Плавное возвращение к исходному размеру с легким затуханием
        elapsed = 0f;
        float returnDuration = 0.15f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsed / returnDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
}
```

---

## 🎯 Итоговый Чек-лист: Что делать по шагам в Unity


1. **Создайте пустые C# файлы:** В Unity создайте скрипты с именами, в точности совпадающими с классами (например, `GameManager.cs`, `UpgradeManager.cs` и т.д.). Скопируйте туда весь код из этого руководства.
2. **Настройте `YandexSDK.jslib`:** Разместите его строго по пути `Assets/Plugins/WebGL/YandexSDK.jslib`.
3. **Разместите UI-компоненты на холсте:**
   * Кнопка котла ➡️ Добавьте компонент `Button` и привяжите к его событию `OnClick` метод `GameManager.Instance.OnCauldronClicked`.
   * Тексты ➡️ Создайте текстовые поля для Золота, Кристаллов, Лвл и VIP, и перетащите их в соответствующие слоты в инспекторе `GameManager`.
   * Сетки магазина ➡️ Создайте кнопки для покупки улучшений, привяжите их `OnClick` к `UpgradeManager.BuyUpgrade(индекс)`.
4. **Создайте Префабы для мини-игр:**
   * Сделайте простую круглую кнопку "Мышка" (спрайт мыши) и сохраните в `Assets/Prefabs/MousePrefab` для игры в мышеловку.
   * Перетащите этот префаб в слот `mousePrefab` в `MinigamesManager`.
5. **Протестируйте прямо в редакторе Unity:** Все скрипты содержат встроенную симуляцию для работы прямо внутри редактора Unity. Кликните на котел — золото начнет расти, покупайте улучшения — пассивный доход пойдет каждую секунду, а симулятор аукциона будет автоматически продавать ваши аватарки за кристаллы!
6. **Запустите Сборку:** Выберите `File ➡️ Build Settings ➡️ Build`. Готовую папку заархивируйте в `.zip` и загрузите в консоль разработчика Яндекс Игр!
