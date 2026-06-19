using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.11.16
    /// Интерактивный HUD Менеджер помещений замка (Казармы, Кузница, Академия).
    /// Полный С# аналог CastleFacilities.tsx для Unity 6 с поддержкой сохранений и неоновых переключений!
    /// </summary>
    public class CastleFacilitiesHUD : MonoBehaviour
    {
        public static CastleFacilitiesHUD Instance { get; private set; }

        [Header("🏰 Ссылка на родительский объект HUD")]
        [Tooltip("Родительский контейнер CastleFacilities_HUD")]
        public GameObject castleFacilitiesRoot;

        [Header("🎛️ Панели помещений замка (UI Panels)")]
        [Tooltip("Главное неоновое кольцо выбора помещений")]
        public GameObject panelHubSelection;
        
        [Tooltip("Окно казарм с промптами и списком воинов")]
        public GameObject panelBarracks;
        
        [Tooltip("Кузница с экипировкой и улучшением брони")]
        public GameObject panelForge;
        
        [Tooltip("Академия с ареной тренировки и прокачки героя")]
        public GameObject panelAcademy;

        [Header("⚔️ Иконки и профили навыков (Barracks Visuals)")]
        public Image iconSkillPassive1;
        public Image iconSkillPassive2;
        public Image iconSkillPassive3;
        public Image iconSkillUltimate;

        [Header("🔊 Звуковые клики (UI Audio sfx)")]
        public AudioClip clickSfx;
        public AudioClip hoverSfx;
        public AudioClip purchaseSuccessSfx;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // По умолчанию сбрасываем состояние панелей на главное меню Hub
            ShowHub();

            // Автоматически программируем события на кнопках при старте, чтобы избавить вас от ручной рутины в Инспекторе!
            AutoBindAllButtons();
        }

        private void Update()
        {
            FateCastleManager cm = FateCastleManager.Instance;
            if (cm != null)
            {
                // Наша панель должна быть выключена всю игру до входа в замок (isTownViewActive == true)
                bool shouldBeActive = cm.isTownViewActive;
                
                // Сверхважно: если открыт диалог, она тоже должна быть строго выключена!
                if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
                {
                    shouldBeActive = false;
                }

                GameObject root = castleFacilitiesRoot != null ? castleFacilitiesRoot : gameObject;
                if (root.activeSelf != shouldBeActive)
                {
                    root.SetActive(shouldBeActive);
                    Debug.Log($"[CASTLE HUD] Синхронизировали активность панели: {shouldBeActive}");
                    if (shouldBeActive)
                    {
                        ShowHub(); // Возвращаем на хаб при входе
                    }
                }
            }
            else
            {
                // Если менеджера замка нет на сцене, скрываем панель для безопасности
                GameObject root = castleFacilitiesRoot != null ? castleFacilitiesRoot : gameObject;
                if (root.activeSelf)
                {
                    root.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Сканирует иерархию и автоматически вешает onClick-события на все кнопки, чтобы избавить вас от ручной рутины в Инспекторе!
        /// </summary>
        public void AutoBindAllButtons()
        {
            Debug.Log("[CASTLE HUD] Начинаем авто-привязку UI-кнопок...");

            GameObject root = castleFacilitiesRoot != null ? castleFacilitiesRoot : gameObject;

            // Ищем кнопки в главном хабе Panel_Hub_Selection
            if (panelHubSelection != null)
            {
                Button[] buttons = panelHubSelection.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    string nameLower = btn.name.ToLower();
                    btn.onClick.RemoveAllListeners();

                    if (nameLower.Contains("barrack") || nameLower.Contains("казарм") || nameLower.Contains("войск") || nameLower.Contains("troop"))
                    {
                        btn.onClick.AddListener(ShowBarracks);
                        Debug.Log($"[CASTLE HUD] Успешно связана кнопка Казарм: {btn.name}");
                    }
                    else if (nameLower.Contains("forge") || nameLower.Contains("кузниц") || nameLower.Contains("экипиров") || nameLower.Contains("armor"))
                    {
                        btn.onClick.AddListener(ShowForge);
                        Debug.Log($"[CASTLE HUD] Успешно связана кнопка Кузницы: {btn.name}");
                    }
                    else if (nameLower.Contains("academy") || nameLower.Contains("академ") || nameLower.Contains("арен") || nameLower.Contains("hero"))
                    {
                        btn.onClick.AddListener(ShowAcademy);
                        Debug.Log($"[CASTLE HUD] Успешно связана кнопка Академии: {btn.name}");
                    }
                }
            }

            // Ищем кнопки "Назад" во всех внутренних окнах
            GameObject[] subPanels = new GameObject[] { panelBarracks, panelForge, panelAcademy };
            foreach (GameObject panel in subPanels)
            {
                if (panel == null) continue;
                Button[] buttons = panel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    string nameLower = btn.name.ToLower();

                    // Если имя кнопки содержит "назад", "back" или "hub" - вешаем возврат
                    if (nameLower.Contains("back") || nameLower.Contains("назад") || nameLower.Contains("hub") || nameLower.Contains("close") || nameLower.Contains("закрыт"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(ShowHub);
                        Debug.Log($"[CASTLE HUD] Авто-связали кнопку НАЗАД на панели {panel.name}: {btn.name}");
                    }
                }
            }

            // Автоматическая привязка функционала (Наем, Улучшение, Тренировка) по именам и тексту:
            // 1. Barracks - Наем воинов
            if (panelBarracks != null)
            {
                Button[] buttons = panelBarracks.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    string nameLower = btn.name.ToLower();
                    if (nameLower.Contains("buy") || nameLower.Contains("hire") || nameLower.Contains("нанять") || nameLower.Contains("производ") || nameLower.Contains("btn_") || nameLower.Contains("button"))
                    {
                        // Пропускаем кнопку "Назад", которая уже привязана выше
                        if (nameLower.Contains("back") || nameLower.Contains("назад") || nameLower.Contains("hub")) continue;

                        btn.onClick.RemoveAllListeners();
                        
                        string troopId = "Paladin";
                        int cost = 50;

                        // Пытаемся автоматически распознать тип воина по имени кнопки или тексту
                        Text textComponent = btn.GetComponentInChildren<Text>();
                        string btnText = textComponent != null ? textComponent.text.ToLower() : "";

                        if (nameLower.Contains("archer") || nameLower.Contains("лук") || nameLower.Contains("стрел") || btnText.Contains("лук") || btnText.Contains("archer") || btnText.Contains("стрел"))
                        {
                            troopId = "Archer";
                            cost = 40;
                        }
                        else if (nameLower.Contains("mage") || nameLower.Contains("wizard") || nameLower.Contains("маг") || btnText.Contains("маг") || btnText.Contains("mage") || btnText.Contains("wizard"))
                        {
                            troopId = "Mage";
                            cost = 60;
                        }
                        else if (nameLower.Contains("warrior") || nameLower.Contains("paladin") || nameLower.Contains("воин") || btnText.Contains("воин") || btnText.Contains("рыцар") || btnText.Contains("paladin"))
                        {
                            troopId = "Warrior";
                            cost = 50;
                        }

                        string finalTroop = troopId;
                        int finalCost = cost;
                        btn.onClick.AddListener(() => BuyTroop(finalTroop, finalCost));
                        Debug.Log($"[CASTLE HUD] Авто-связали кнопку воина: {btn.name} -> BuyTroop(\"{finalTroop}\", {finalCost})");
                    }
                }
            }

            // 2. Forge - Улучшение брони
            if (panelForge != null)
            {
                Button[] buttons = panelForge.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    string nameLower = btn.name.ToLower();
                    if (nameLower.Contains("upgrade") || nameLower.Contains("улучшить") || nameLower.Contains("armor") || nameLower.Contains("экипиров") || nameLower.Contains("button_"))
                    {
                        if (nameLower.Contains("back") || nameLower.Contains("назад") || nameLower.Contains("hub")) continue;

                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => UpgradeArmor(40));
                        Debug.Log($"[CASTLE HUD] Связали кнопку Улучшения Брони: {btn.name} -> UpgradeArmor(40)");
                    }
                }
            }

            // 3. Academy - Тренировка героя
            if (panelAcademy != null)
            {
                Button[] buttons = panelAcademy.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    string nameLower = btn.name.ToLower();
                    if (nameLower.Contains("train") || nameLower.Contains("трениров") || nameLower.Contains("arena") || nameLower.Contains("прокач") || nameLower.Contains("xp") || nameLower.Contains("button_"))
                    {
                        if (nameLower.Contains("back") || nameLower.Contains("назад") || nameLower.Contains("hub")) continue;

                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => TrainHero(60, 50));
                        Debug.Log($"[CASTLE HUD] Связали кнопку Тренировки Героя: {btn.name} -> TrainHero(60, 50)");
                    }
                }
            }

            // Авто-привязываем клики по иконкам пассивных навыков и суперудара
            BindSkillToButton(iconSkillPassive1, 0);
            BindSkillToButton(iconSkillPassive2, 1);
            BindSkillToButton(iconSkillPassive3, 2);
            BindSkillToButton(iconSkillUltimate, 3);
        }

        private void BindSkillToButton(Image img, int index)
        {
            if (img == null) return;
            Button btn = img.GetComponent<Button>();
            if (btn == null)
            {
                btn = img.gameObject.AddComponent<Button>();
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSkillClicked(index));
            Debug.Log($"[CASTLE HUD] Авто-привязали клик по иконке навыка {index} на объекте {img.name}");
        }

        public void OnSkillClicked(int index)
        {
            FateCastleManager cm = FateCastleManager.Instance;
            if (cm == null) return;

            int language = PlayerPrefs.GetInt("SelectedLanguage", 0);
            int heroClass = PlayerPrefs.GetInt("SelectedClassIndex", 0);

            string sName = "";
            string sDesc = "";
            Texture2D icon = null;
            string skillType = "Passive";

            if (heroClass == 1) // Archer
            {
                if (index == 0) { sName = language == 0 ? "Крит-Мастер" : "Crit-Master"; sDesc = language == 0 ? "Повышает вероятность нанесения критического урона на 15%" : "+15% critical hit probability"; icon = cm.archerSkillPassive1; }
                else if (index == 1) { sName = "LongShot"; sDesc = language == 0 ? "Дальний выстрел: Усиливает урон на расстоянии на 10%" : "+10% damage over wide distance range"; icon = cm.archerSkillPassive2; }
                else if (index == 2) { sName = "Evasion"; sDesc = language == 0 ? "Поворотливость: Дарует 10% шанс полного уклонения от вражеских атак" : "+10% complete dodge probability"; icon = cm.archerSkillPassive3; }
                else if (index == 3) { sName = language == 0 ? "Ливень Смерти" : "Death Rain"; sDesc = language == 0 ? "Суперудар (CD 3х): Смертоносный град стрел наносит масштабный урон 1.8х по всем врагам в зоне" : "Ultimate (CD 3t): AoE volley dealing massive x1.8 damage to enemies"; icon = cm.archerSkillUltimate; skillType = "Ultimate"; }
            }
            else if (heroClass == 2) // Mage
            {
                if (index == 0) { sName = "ManaFlow"; sDesc = language == 0 ? "Поток маны: Позволяет восстанавливать 5 очков маны за каждый совершённый ход" : "+5 mana points gain per turn"; icon = cm.mageSkillPassive1; }
                else if (index == 1) { sName = "Elemental"; sDesc = language == 0 ? "Сила стихий: Усиливает разрушительный потенциал ваших заклинаний на 15%" : "+15% magic spell power booster"; icon = cm.mageSkillPassive2; }
                else if (index == 2) { sName = "Resist"; sDesc = language == 0 ? "Сопротивление: Наделяет мистическим барьером, поглощающим 15% магического урона" : "+15% spell resistance shield"; icon = cm.mageSkillPassive3; }
                else if (index == 3) { sName = "Time Rift"; sDesc = language == 0 ? "Суперудар (CD 4х): Изменяет пространственно-временной континуум, полностью замедляя противников на 2 хода" : "Ultimate (CD 4t): Slows down all active enemy actions for 2 turns"; icon = cm.mageSkillUltimate; skillType = "Ultimate"; }
            }
            else // Warrior
            {
                if (index == 0) { sName = "IronSkin"; sDesc = language == 0 ? "Прочная кожа: Успешно увеличивает показатель защиты и стойкости на 15%" : "+15% Armor/Defense bonus"; icon = cm.warriorSkillPassive1; }
                else if (index == 1) { sName = "Regen"; sDesc = language == 0 ? "Регенерация: Обеспечивает исцеление вашего героя на 5 ОЗ каждый игровой ход" : "+5 HP recovery per turn"; icon = cm.warriorSkillPassive2; }
                else if (index == 2) { sName = "Threat"; sDesc = language == 0 ? "Угроза: Ускоряет накопление боевого духа и провокацию на 10%" : "+10% aggro multiplier bonus"; icon = cm.warriorSkillPassive3; }
                else if (index == 3) { sName = "TitanShield"; sDesc = language == 0 ? "Суперудар (CD 4х): Активирует нерушимый щит Титанов, снижая входящий физический урон на 70%" : "Ultimate (CD 4t): Blocks 70% of incoming physical damage"; icon = cm.warriorSkillUltimate; skillType = "Ultimate"; }
            }

            cm.OpenSkillDetailPopup(sName, sDesc, icon, skillType);
        }

        /// <summary>
        /// Универсальный переключатель панелей
        /// </summary>
        public void SwitchPanel(GameObject targetPanel)
        {
            PlayClickSound();

            if (panelHubSelection != null) panelHubSelection.SetActive(false);
            if (panelBarracks != null) panelBarracks.SetActive(false);
            if (panelForge != null) panelForge.SetActive(false);
            if (panelAcademy != null) panelAcademy.SetActive(false);

            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
                Debug.Log($"[CASTLE HUD] Успешно переключились на панель: {targetPanel.name}");
            }
        }

        // --- Удобные хендлеры для инспектора и OnClick() кнопок ---

        public void ShowHub() => SwitchPanel(panelHubSelection);
        public void ShowBarracks()
        {
            SwitchPanel(panelBarracks);
            UpdateBarracksSkillsUI();
        }
        public void ShowForge() => SwitchPanel(panelForge);
        public void ShowAcademy() => SwitchPanel(panelAcademy);

        /// <summary>
        /// Шаг 4: Динамически перегружает иконки умений в UI Image на HUD-панели кабин за секунду!
        /// </summary>
        public void UpdateBarracksSkillsUI()
        {
            FateCastleManager cm = FateCastleManager.Instance;
            if (cm == null) return;

            // Вычисляем текущий класс нашего героя: Воин/Лучник/Маг
            // 0 - Воин, 1 - Лучник, 2 - Маг
            int heroClass = PlayerPrefs.GetInt("SelectedClassIndex", 0);

            Texture2D p1 = null;
            Texture2D p2 = null;
            Texture2D p3 = null;
            Texture2D ult = null;

            if (heroClass == 1) // Archer
            {
                p1 = cm.archerSkillPassive1;
                p2 = cm.archerSkillPassive2;
                p3 = cm.archerSkillPassive3;
                ult = cm.archerSkillUltimate;
            }
            else if (heroClass == 2) // Mage
            {
                p1 = cm.mageSkillPassive1;
                p2 = cm.mageSkillPassive2;
                p3 = cm.mageSkillPassive3;
                ult = cm.mageSkillUltimate;
            }
            else // Warrior
            {
                p1 = cm.warriorSkillPassive1;
                p2 = cm.warriorSkillPassive2;
                p3 = cm.warriorSkillPassive3;
                ult = cm.warriorSkillUltimate;
            }

            // Накатываем текстуры в Image элементы холста Canvas
            ApplyTextureToImage(iconSkillPassive1, p1);
            ApplyTextureToImage(iconSkillPassive2, p2);
            ApplyTextureToImage(iconSkillPassive3, p3);
            ApplyTextureToImage(iconSkillUltimate, ult);
        }

        private void ApplyTextureToImage(Image img, Texture2D tex)
        {
            if (img == null) return;
            if (tex != null)
            {
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                img.gameObject.SetActive(true);
            }
            else
            {
                img.gameObject.SetActive(false); // Прячем слот, если текстура пустая
            }
        }

        /// <summary>
        /// Метод покупки воинов (кнопка Нанять / Hire)
        /// </summary>
        public void BuyTroop(string troopId, int cost)
        {
            int currentGold = SaveGameSystem.CurrentData != null ? SaveGameSystem.CurrentData.gold : PlayerPrefs.GetInt("Player_Gold_Reserve", 100);

            if (currentGold >= cost)
            {
                // Списываем золото
                if (SaveGameSystem.CurrentData != null)
                {
                    SaveGameSystem.CurrentData.gold -= cost;
                }
                else
                {
                    currentGold -= cost;
                    PlayerPrefs.SetInt("Player_Gold_Reserve", currentGold);
                }

                // Добавляем воина в армию (сохраняем уровень нанятого воина)
                int currentUnitsCount = PlayerPrefs.GetInt("Castle_Troop_Count_" + troopId, 0);
                PlayerPrefs.SetInt("Castle_Troop_Count_" + troopId, currentUnitsCount + 1);

                PlaySuccessSound();
                
                // 💾 Шаг 5: Моментальная авто-фиксация прогресса в системе RPG сохранений!
                SaveProgress();

                Debug.Log($"[CASTLE HUD] Наняли воина {troopId}. Списано {cost} золота. Текущая армия: {currentUnitsCount + 1}");
            }
            else
            {
                Debug.LogWarning("[CASTLE HUD] Недостаточно золота для найма дивизии!");
            }
        }

        /// <summary>
        /// Метод апгрейда экипировки / брони в Кузнице
        /// </summary>
        public void UpgradeArmor(int cost)
        {
            int currentGold = SaveGameSystem.CurrentData != null ? SaveGameSystem.CurrentData.gold : PlayerPrefs.GetInt("Player_Gold_Reserve", 100);

            if (currentGold >= cost)
            {
                if (SaveGameSystem.CurrentData != null)
                {
                    SaveGameSystem.CurrentData.gold -= cost;
                }
                else
                {
                    currentGold -= cost;
                    PlayerPrefs.SetInt("Player_Gold_Reserve", currentGold);
                }

                // Повышаем уровень прочности брони
                int armorLvl = PlayerPrefs.GetInt("Castle_Forge_Armor_Level", 1);
                PlayerPrefs.SetInt("Castle_Forge_Armor_Level", armorLvl + 1);

                PlaySuccessSound();

                // 💾 Шаг 5: Запись изменений
                SaveProgress();

                Debug.Log($"[CASTLE HUD] Броня улучшена до уровня {armorLvl + 1}. Потрачено {cost} золотых.");
            }
            else
            {
                Debug.LogWarning("[CASTLE HUD] Недостаточно средств для улучшения брони в Кузнице!");
            }
        }

        /// <summary>
        /// Тренировка Героя на Арене Академии
        /// </summary>
        public void TrainHero(int cost, int xpGained)
        {
            int currentGold = SaveGameSystem.CurrentData != null ? SaveGameSystem.CurrentData.gold : PlayerPrefs.GetInt("Player_Gold_Reserve", 100);

            if (currentGold >= cost)
            {
                if (SaveGameSystem.CurrentData != null)
                {
                    SaveGameSystem.CurrentData.gold -= cost;
                }
                else
                {
                    currentGold -= cost;
                    PlayerPrefs.SetInt("Player_Gold_Reserve", currentGold);
                }

                // Прибавляем опыт герою
                int currentHeroXP = PlayerPrefs.GetInt("Hero_XP_Stat", 0);
                int currentHeroLvl = PlayerPrefs.GetInt("Hero_Level_Stat", 1);

                currentHeroXP += xpGained;
                int xpNeeded = currentHeroLvl * 100;

                if (currentHeroXP >= xpNeeded)
                {
                    currentHeroXP -= xpNeeded;
                    currentHeroLvl++;
                    PlayerPrefs.SetInt("Hero_Level_Stat", currentHeroLvl);
                    Debug.Log($"[CASTLE HUD] Уровень Героя повышен! Новый уровень: {currentHeroLvl}");
                }

                PlayerPrefs.SetInt("Hero_XP_Stat", currentHeroXP);
                PlayerPrefs.Save();

                PlaySuccessSound();

                // 💾 Шаг 5: Запись изменений
                SaveProgress();

                Debug.Log($"[CASTLE HUD] Герой потренировался! Получено {xpGained} XP. Потрачено {cost} золотых.");
            }
            else
            {
                Debug.LogWarning("[CASTLE HUD] Не хватает золота для тренировок на Арене!");
            }
        }

        private void SaveProgress()
        {
            // Получаем активный слот из PlayerPrefs, по умолчанию 0
            int activeSlot = PlayerPrefs.GetInt("Active_Save_Slot", 0);
            
            // Синхронизируем золото в PlayerPrefs перед непосредственной сериализацией
            if (SaveGameSystem.CurrentData != null)
            {
                PlayerPrefs.SetInt("Player_Gold_Reserve", SaveGameSystem.CurrentData.gold);
            }
            
            // Вызываем глобальную систему сохранений Fate Continent напрямую
            try
            {
                SaveGameSystem.Save(activeSlot);
                Debug.Log($"[CASTLE HUD] Сработало авто-сохранение SaveGameSystem.Save({activeSlot})!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CASTLE HUD] Не удалось автоматически сохранить игру: {ex}");
                PlayerPrefs.Save();
            }
        }

        private void PlayClickSound()
        {
            if (SettingsManager.Instance != null && clickSfx != null)
            {
                SettingsManager.Instance.PlaySoundEffect(clickSfx);
            }
        }

        private void PlaySuccessSound()
        {
            if (SettingsManager.Instance != null && purchaseSuccessSfx != null)
            {
                SettingsManager.Instance.PlaySoundEffect(purchaseSuccessSfx);
            }
        }
    }
}
