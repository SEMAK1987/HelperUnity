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
            // По умолчанию включаем главное меню Hub и выключаем другие вкладки под Unity 6
            ShowHub();
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
