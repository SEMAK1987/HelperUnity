using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using TMPro;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы)
    /// Zenith 3D Character & Difficulty Selection Controller (v18.7.4)
    /// Скрипт управляет 3D сценой выбора героев (с подиумами, Spotlight-подсветкой, анимациями и выбором уровня сложности).
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        [Header("⚙️ Конфигурация баланса")]
        [SerializeField] private BalanceConfig balanceConfig;

        [Header("🏛️ 3D Подиумы & Герои")]
        [SerializeField] private PedestalData[] pedestals;
        [SerializeField] private float pedestalRotationSpeed = 30f;

        [Header("💡 Освещение (Spotlights)")]
        [SerializeField] private float idleSpotlightRange = 5f;
        [SerializeField] private float selectSpotlightRange = 10f;
        [SerializeField] private float spotlightIntensitySpeed = 8f;

        [Header("🖥️ UI Интерфейс (Отображение Героя)")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI heroTypeText;
        [SerializeField] private TextMeshProUGUI statsText; // HP, MP, ATK, DEF, SPD, LCK
        [SerializeField] private TextMeshProUGUI passivesText;
        [SerializeField] private TextMeshProUGUI activeSkillText;

        [Header("🎭 UI Интерфейс (Сложность игры)")]
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private TextMeshProUGUI difficultyDescriptionText;

        [Header("🚀 Навигация & Переходы")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private string mainGameplaySceneName = "GameplayScene";

        private int selectedHeroIndex = -1;
        private int selectedDifficultyIndex = 0;

        [System.Serializable]
        public class PedestalData
        {
            public string HeroID;                 // ID совпадает с ID в BalanceConfig (например: warrior_prem, archer_prem, mage_prem)
            public Transform PedestalTransform;   // Ссылка на 3D Cylinder (Подиум)
            public GameObject ModelGameObject;   // Ссылка на 3D модель персонажа
            public Animator ModelAnimator;        // Ссылка на компонент Animator на этой модели
            public Light SpotLight;               // Направленный Spotlight над подиумом
            public Color FactionColor = Color.cyan; // Индивидуальный атмосферный цвет
            public string CharacterWeaponPrompt;  // Описание оружия для ИИ (для справки разработчику)

            [HideInInspector] public float targetIntensity;
            [HideInInspector] public float targetRange;
        }

        private void Start()
        {
            // Очищаем текущие данные сохранения при заходе во вторую сцену (новая игра)
            SaveGameSystem.ResetData();

            InitializeDifficultyDropdown();
            InitializePedestals();
            SelectHero(0); // По умолчанию выбираем первого подиумного героя

            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelectionAndStart);
            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMainMenu);
        }

        private void Update()
        {
            HandlePedestalRotation();
            UpdateLightsIntensity();
            HandleMouseClickSelection();
        }

        private void InitializeDifficultyDropdown()
        {
            if (difficultyDropdown == null || balanceConfig == null || balanceConfig.Difficulties == null) return;

            difficultyDropdown.clearOptions();
            var options = balanceConfig.Difficulties.Select(d => new TMP_Dropdown.OptionData(d.LevelName)).ToList();
            difficultyDropdown.AddOptions(options);

            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            OnDifficultyChanged(0);
        }

        private void InitializePedestals()
        {
            if (pedestals == null) return;

            for (int i = 0; i < pedestals.Length; i++)
            {
                var p = pedestals[i];
                if (p.SpotLight != null)
                {
                    p.SpotLight.color = p.FactionColor;
                    p.targetIntensity = 2f;
                    p.targetRange = idleSpotlightRange;
                }
            }
        }

        private void HandlePedestalRotation()
        {
            // Плавное медленное вращение подиумов для живой атмосферы в 3D
            for (int i = 0; i < pedestals.Length; i++)
            {
                var p = pedestals[i];
                if (p.PedestalTransform != null)
                {
                    p.PedestalTransform.Rotate(Vector3.up, pedestalRotationSpeed * Time.deltaTime);
                }
            }
        }

        private void UpdateLightsIntensity()
        {
            // Плавная интерполяция дальности и силы света прожекторов при выборе
            for (int i = 0; i < pedestals.Length; i++)
            {
                var p = pedestals[i];
                if (p.SpotLight != null)
                {
                    p.SpotLight.range = Mathf.Lerp(p.SpotLight.range, p.targetRange, spotlightIntensitySpeed * Time.deltaTime);
                    p.SpotLight.intensity = Mathf.Lerp(p.SpotLight.intensity, p.targetIntensity, spotlightIntensitySpeed * Time.deltaTime);
                }
            }
        }

        private void HandleMouseClickSelection()
        {
            // Обрабатываем клики по 3D подиумам или персонажам во время фазы выбора
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    for (int i = 0; i < pedestals.Length; i++)
                    {
                        var p = pedestals[i];
                        // Проверяем, нажали ли мы на сам подиум или коллайдер модели
                        if (hit.transform == p.PedestalTransform || hit.transform.IsChildOf(p.PedestalTransform) || (p.ModelGameObject != null && hit.transform == p.ModelGameObject.transform))
                        {
                            SelectHero(i);
                            break;
                        }
                    }
                }
            }
        }

        public void SelectHero(int index)
        {
            if (pedestals == null || index < 0 || index >= pedestals.Length) return;

            selectedHeroIndex = index;
            var currentPedestal = pedestals[index];

            // Настраиваем свечение Spotlights (выбранный горит ярко, остальные затухают)
            for (int i = 0; i < pedestals.Length; i++)
            {
                var p = pedestals[i];
                if (p.SpotLight != null)
                {
                    if (i == index)
                    {
                        p.targetIntensity = 12f;
                        p.targetRange = selectSpotlightRange;
                    }
                    else
                    {
                        p.targetIntensity = 1.5f;
                        p.targetRange = idleSpotlightRange;
                    }
                }
            }

            // Запускаем триггер анимации реакции персонажа (пафосный жест)
            if (currentPedestal.ModelAnimator != null)
            {
                currentPedestal.ModelAnimator.SetTrigger("OnSelectTrigger");
            }

            // Звуковый щелчок через SettingsManager
            if (SettingsManager.Instance != null)
            {
                // Находим или воспроизводим аудио-клик в зависимости от структуры
                Debug.Log($"[FATE AUDIO] Воспроизводим звук выбора героя для: {currentPedestal.HeroID}");
            }

            // Обновляем панель информации по характеристикам из BalanceConfig
            UpdateHeroStatsUI(currentPedestal.HeroID);
        }

        private void UpdateHeroStatsUI(string heroID)
        {
            if (balanceConfig == null || balanceConfig.Heroes == null) return;

            // Находим данные о балансе героя в ScriptableObject
            var heroData = balanceConfig.Heroes.FirstOrDefault(h => h.ID == heroID);
            
            // Если не нашли конкретный ID, берем дефолтные параметры по структуре
            if (string.IsNullOrEmpty(heroData.ID))
            {
                heroNameText.text = "Неизвестный Герой";
                statsText.text = "N/A";
                return;
            }

            // Подгружаем красивый текст
            heroNameText.text = heroData.Name;
            heroTypeText.text = $"Класс: {(heroData.Type == "Premium" ? "<color=#FFD700>Премиум Герой</color>" : "Простой Герой")}";

            statsText.text = $"❤ Здоровье (HP): {heroData.HP}\n" +
                             $"✨ Мана (MP): {heroData.MP}\n" +
                             $"⚔ Атака (ATK): {heroData.ATK}\n" +
                             $"🛡 Защита (DEF): {heroData.DEF}\n" +
                             $"⚡ Скорость (SPD): {heroData.SPD}\n" +
                             $"🍀 Удача (LCK): {heroData.LCK}";

            // Чистим пассивки от технических индикаторов типа [0] (просто выводим имя навыка)
            if (heroData.Passives != null && heroData.Passives.Length > 0)
            {
                var cleanPassives = heroData.Passives.Select(p => {
                    // Убираем цифры [0], [1] если они записаны в конфиг напрямую
                    string clean = p;
                    if (p.StartsWith("["))
                    {
                        int closeBracket = p.IndexOf(']');
                        if (closeBracket != -1 && p.Length > closeBracket + 1)
                        {
                            clean = p.Substring(closeBracket + 1).Trim();
                        }
                    }
                    return $"• {clean}";
                });
                passivesText.text = "<b>Пассивные Умения:</b>\n" + string.Join("\n", cleanPassives);
            }
            else
            {
                passivesText.text = "<b>Пассивные Умения:</b>\n<i>Отсутствуют</i>";
            }

            // Навык Супера (Активный навык со временем перезарядки)
            if (!string.IsNullOrEmpty(heroData.SuperSkill))
            {
                activeSkillText.text = $"<b>Активный Суперудаp:</b> {heroData.SuperSkill}\n" +
                                      $"<i>Перезарядка:</i> {heroData.SuperCooldown} х. | <i>Мощь:</i> x{heroData.SuperPower}";
            }
            else
            {
                activeSkillText.text = "<b>Активный Суперудаp:</b>\n<i>Отсутствует</i>";
            }
        }

        private void OnDifficultyChanged(int index)
        {
            if (balanceConfig == null || balanceConfig.Difficulties == null || index < 0 || index >= balanceConfig.Difficulties.Length) return;

            selectedDifficultyIndex = index;
            var difficulty = balanceConfig.Difficulties[index];

            // Формируем красивую сводку сложности для игрока
            difficultyDescriptionText.text = $"Агрессия ИИ: <b>{difficulty.Aggression * 100}%</b>\n" +
                                             $"Защита целей: <b>x{difficulty.Defense}</b>\n" +
                                             $"Экономический множитель: <b>x{difficulty.EconMod}</b>\n" +
                                             $"Бонус к золоту ИИ: <b>+{difficulty.AIGoldBonus} золота</b>";
        }

        public void ConfirmSelectionAndStart()
        {
            if (selectedHeroIndex == -1 || pedestals == null || selectedHeroIndex >= pedestals.Length) return;

            var chosenPedestal = pedestals[selectedHeroIndex];
            var heroData = balanceConfig.Heroes.FirstOrDefault(h => h.ID == chosenPedestal.HeroID);

            if (string.IsNullOrEmpty(heroData.ID)) return;

            // Заполняем сохранение SaveGameSystem перед стартом игры!
            SaveGameSystem.CurrentData.saveName = heroData.Name;
            SaveGameSystem.CurrentData.characterClass = chosenPedestal.HeroID;
            SaveGameSystem.CurrentData.playerLevel = 1;
            SaveGameSystem.CurrentData.currentXP = 0;
            SaveGameSystem.CurrentData.gold = 150; // Стартовое золото игрока
            SaveGameSystem.CurrentData.currentHealth = heroData.HP;
            SaveGameSystem.CurrentData.maxHealth = heroData.HP;

            // Сохраняем в Слот 0 (первичный слот новой игры)
            SaveGameSystem.Save(0);

            Debug.Log($"[FATE START] Выбран герой {heroData.Name}. Уровень сложности: {balanceConfig.Difficulties[selectedDifficultyIndex].LevelName}. Готовим переход на игровую сцену.");

            // Плавный переход с растворением через LoadingScreenManager или прямой SceneManager
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(mainGameplaySceneName);
            }
            else
            {
                SceneManager.LoadScene(mainGameplaySceneName);
            }
        }

        public void BackToMainMenu()
        {
            // Возврат обратно в Главное Меню (Сцена 0)
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene("MenuScene");
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
