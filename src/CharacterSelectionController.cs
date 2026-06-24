using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.7.4
    /// Zenith 3D Character & Difficulty Selection Controller
    /// Скрипт управляет 3D-сценой выбора героев (с подиумами, Spotlight-подсветкой, анимациями и выбором уровня сложности).
    /// Полностью автоматизирована реактивная локализация на 9 языков с красивой перерисовкой.
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
        [Tooltip("Название сцены главного меню для возврата назад")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("Индекс сцены главного меню в Build Settings (по умолчанию 0)")]
        [SerializeField] private int mainMenuSceneIndex = 0;
        [Tooltip("Включите это свойство, чтобы загружать меню по Имени вместо Индекса")]
        [SerializeField] private bool loadMenuByName = true;

        private int selectedHeroIndex = -1;
        private int selectedDifficultyIndex = 0;
        private int lastLanguageID = -1;

        [System.Serializable]
        public class PedestalData
        {
            public string HeroID;                 // ID совпадает с ID в BalanceConfig (например: warrior_prem, archer_prem, mage_prem)
            public Transform PedestalTransform;   // Ссылка на 3D Cylinder (Подиум)
            public GameObject ModelGameObject;   // Ссылка на 3D модель персонажа
            public Light SpotLight;               // Направленный Spotlight над подиумом
            public Color FactionColor = Color.cyan; // Индивидуальный атмосферный цвет
            public string CharacterWeaponPrompt;  // Описание оружия для ИИ (для справки разработчику)

            [HideInInspector] public float targetIntensity;
            [HideInInspector] public float targetRange;
        }

        private void Awake()
        {
            // Очищаем любой предустановленный в редакторе текст (например, содержащий эмодзи короны '👑' в TMP_HeroType),
            // чтобы Unity не выводил предупреждения о нехватке символов в шрифте LiberationSans SDF до первой отрисовки.
            if (heroTypeText != null) heroTypeText.text = "";
            if (heroNameText != null) heroNameText.text = "";
            if (statsText != null) statsText.text = "";
            if (passivesText != null) passivesText.text = "";
            if (activeSkillText != null) activeSkillText.text = "";
        }

        private void Start()
        {
            // Очищаем текущие данные сохранения при заходе во вторую сцену (новая игра)
            SaveGameSystem.ResetData();

            InitializePedestals();
            SelectHero(0); // Выбираем первого персонажа

            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelectionAndStart);
            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMainMenu);
        }

        private void Update()
        {
            HandlePedestalRotation();
            UpdateLightsIntensity();
            HandleMouseClickSelection();

            // Проверяем реактивное изменение локализации через Настройки
            if (lastLanguageID != Translator.LanguageID)
            {
                lastLanguageID = Translator.LanguageID;
                RebuildUI();
            }
        }

        private void RebuildUI()
        {
            RebuildDifficultyDropdown();
            
            if (selectedHeroIndex >= 0 && pedestals != null && selectedHeroIndex < pedestals.Length)
            {
                UpdateHeroStatsUI(pedestals[selectedHeroIndex].HeroID);
            }
            
            UpdateDifficultyDescriptionText();
            UpdateButtonsLocalization();
        }

        private void RebuildDifficultyDropdown()
        {
            if (difficultyDropdown == null || balanceConfig == null || balanceConfig.Difficulties == null) return;

            difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
            difficultyDropdown.ClearOptions();

            string[] names = GetLocalizedDifficultyNames();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < Mathf.Min(names.Length, balanceConfig.Difficulties.Length); i++)
            {
                options.Add(new TMP_Dropdown.OptionData(names[i]));
            }
            difficultyDropdown.AddOptions(options);
            difficultyDropdown.value = selectedDifficultyIndex;
            difficultyDropdown.RefreshShownValue();
            
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

            ApplyDropdownStyling();
        }

        private void ApplyDropdownStyling()
        {
            int lang = Translator.LanguageID;
            TMP_FontAsset font = Translator.Instance != null ? Translator.Instance.defaultFont : null;
            float charSpacing = 0f;

            if (Translator.Instance != null)
            {
                if (lang == 7) font = Translator.Instance.koreanFont;
                else if (lang == 8 || lang == 6) font = Translator.Instance.chineseFont;
                else if (lang == 0) charSpacing = Translator.Instance.russianCharacterSpacing;
            }

            if (difficultyDropdown.captionText != null)
            {
                if (font != null) difficultyDropdown.captionText.font = font;
                difficultyDropdown.captionText.characterSpacing = charSpacing;
            }
            if (difficultyDropdown.itemText != null)
            {
                if (font != null) difficultyDropdown.itemText.font = font;
                difficultyDropdown.itemText.characterSpacing = charSpacing;
            }
        }

        private string[] GetLocalizedDifficultyNames()
        {
            switch (Translator.LanguageID)
            {
                case 0: // Russian
                    return new string[] { "Новичок", "Легко", "Нормально", "Сложно", "Кошмар" };
                case 2: // German
                    return new string[] { "Novize", "Leicht", "Normal", "Schwer", "Albtraum" };
                case 3: // French
                    return new string[] { "Novice", "Facile", "Normal", "Difficile", "Cauchemar" };
                case 4: // Spanish
                    return new string[] { "Novato", "Fácil", "Normal", "Difícil", "Pesadilla" };
                case 5: // Portuguese
                    return new string[] { "Novato", "Fácil", "Normal", "Difícil", "Pesadelo" };
                case 6: // Japanese
                    return new string[] { "ビギナー", "イージー", "ノーマル", "ハード", "ナイトメア" };
                case 7: // Korean
                    return new string[] { "초보자", "쉬움", "보통", "어려움", "악몽" };
                case 8: // Chinese
                    return new string[] { "新手", "简单", "普通", "困难", "噩梦" };
                case 1: // English
                default:
                    return new string[] { "Novice", "Easy", "Normal", "Hard", "Nightmare" };
            }
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

                // Гарантируем наличие физического коллайдера на подиуме, чтобы клик всегда проходил
                if (p.PedestalTransform != null)
                {
                    Collider col = p.PedestalTransform.GetComponent<Collider>();
                    if (col == null)
                    {
                        var boxCol = p.PedestalTransform.gameObject.AddComponent<BoxCollider>();
                        boxCol.size = new Vector3(2.5f, 1.2f, 2.5f);
                    }
                }

                // Гарантируем наличие физического коллайдера на самом персонаже
                if (p.ModelGameObject != null)
                {
                    Collider col = p.ModelGameObject.GetComponent<Collider>();
                    if (col == null)
                    {
                        var childCols = p.ModelGameObject.GetComponentsInChildren<Collider>();
                        if (childCols.Length == 0)
                        {
                            var cap = p.ModelGameObject.AddComponent<CapsuleCollider>();
                            cap.center = new Vector3(0, 1f, 0);
                            cap.radius = 0.6f;
                            cap.height = 2f;
                        }
                    }
                }
            }
        }

        private void HandlePedestalRotation()
        {
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
            bool clickDetected = false;
            Vector2 mousePos = Vector2.zero;

            // 1. Пытаемся считать данные через New Input System (если определен символ)
#if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
            try
            {
                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    clickDetected = true;
                    mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                }
            }
            catch (System.Exception) { }
#endif

            // 2. Если клик не перехвачен, пробуем через классический Input
            if (!clickDetected)
            {
                try
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        clickDetected = true;
                        mousePos = Input.mousePosition;
                    }
                }
                catch (System.Exception)
                {
                    // Вызывается, если в Player Settings переключили на New Input System, но макрос #if не сработал.
                    // Решаем проблему безупречно через Reflection в рантайме!
                    clickDetected = TryReadNewInputSystemReflection(out mousePos);
                }
            }

            // 3. Если клик зафиксирован - испускаем физический луч
            if (clickDetected)
            {
                PerformPhysicsSelectionRaycast(mousePos);
            }
        }

        /// <summary>
        /// Безопасный опрос New Input System в обход препроцессора через Reflection.
        /// Гарантирует работоспособность новой системы ввода даже если в Unity не настроен символ компиляции.
        /// </summary>
        private bool TryReadNewInputSystemReflection(out Vector2 mousePosition)
        {
            mousePosition = Vector2.zero;
            try
            {
                var mouseType = System.Type.GetType("UnityEngine.InputSystem.Mouse, Unity.InputSystem");
                if (mouseType != null)
                {
                    var currentProp = mouseType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var mouseInstance = currentProp?.GetValue(null);
                    if (mouseInstance != null)
                    {
                        var leftButtonProp = mouseInstance.GetType().GetProperty("leftButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        var leftButtonInstance = leftButtonProp?.GetValue(mouseInstance);
                        if (leftButtonInstance != null)
                        {
                            var wasPressedProp = leftButtonInstance.GetType().GetProperty("wasPressedThisFrame", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            bool wasPressed = (bool)(wasPressedProp?.GetValue(leftButtonInstance) ?? false);
                            if (wasPressed)
                            {
                                var positionProp = mouseInstance.GetType().GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                var positionInstance = positionProp?.GetValue(mouseInstance);
                                if (positionInstance != null)
                                {
                                    var readValueMethod = positionInstance.GetType().GetMethod("ReadValue");
                                    if (readValueMethod != null)
                                    {
                                        mousePosition = (Vector2)readValueMethod.Invoke(positionInstance, null);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception) { }
            return false;
        }

        private void PerformPhysicsSelectionRaycast(Vector2 screenPos)
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < pedestals.Length; i++)
                {
                    var p = pedestals[i];
                    
                    // Попадание по подиуму или его детям
                    bool hitPedestal = p.PedestalTransform != null && (hit.transform == p.PedestalTransform || hit.transform.IsChildOf(p.PedestalTransform));
                    
                    // Попадание по 3D модели или её под-мешам (доспехи, оружие, кости)
                    bool hitModel = p.ModelGameObject != null && (hit.transform == p.ModelGameObject.transform || hit.transform.IsChildOf(p.ModelGameObject.transform));

                    if (hitPedestal || hitModel)
                    {
                        SelectHero(i);
                        break;
                    }
                }
            }
        }

        public void SelectHero(int index)
        {
            if (pedestals == null || index < 0 || index >= pedestals.Length) return;

            selectedHeroIndex = index;
            var currentPedestal = pedestals[index];

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

            // Проигрываем клик выбора через SettingsManager
            if (SettingsManager.Instance != null)
            {
                Debug.Log($"[FATE AUDIO] Воспроизводим звук выбора героя для: {currentPedestal.HeroID}");
            }

            UpdateHeroStatsUI(currentPedestal.HeroID);
        }

        private void ApplyFontToText(TextMeshProUGUI tmpText, bool isHeadline = false)
        {
            if (tmpText == null) return;
            int lang = Translator.LanguageID;
            TMP_FontAsset font = Translator.Instance != null ? Translator.Instance.defaultFont : null;
            float charSpacing = 0f;

            if (Translator.Instance != null)
            {
                if (lang == 7) font = Translator.Instance.koreanFont;
                else if (lang == 8 || lang == 6) font = Translator.Instance.chineseFont;
                else if (lang == 0 && isHeadline) charSpacing = Translator.Instance.russianCharacterSpacing;
            }

            if (font != null) tmpText.font = font;
            tmpText.characterSpacing = charSpacing;
        }

        private void UpdateHeroStatsUI(string heroID)
        {
            if (balanceConfig == null || balanceConfig.Heroes == null) return;

            var heroData = balanceConfig.Heroes.FirstOrDefault(h => h.ID == heroID);

            ApplyFontToText(heroNameText, true);
            ApplyFontToText(heroTypeText, true);
            ApplyFontToText(statsText, true);
            ApplyFontToText(passivesText, false);
            ApplyFontToText(activeSkillText, false);

            if (heroNameText != null)
            {
                if (string.IsNullOrEmpty(heroData.ID))
                    heroNameText.text = GetLocalizedUnknownHero();
                else
                    heroNameText.text = GetLocalizedHeroName(heroData.ID, heroData.Name);
            }

            if (heroTypeText != null)
            {
                if (string.IsNullOrEmpty(heroData.ID))
                    heroTypeText.text = "";
                else
                    heroTypeText.text = GetLocalizedHeroType(heroData.Type);
            }

            if (statsText != null)
            {
                if (string.IsNullOrEmpty(heroData.ID))
                    statsText.text = "N/A";
                else
                    statsText.text = GetLocalizedStatsBlock(heroData);
            }

            if (passivesText != null)
            {
                if (string.IsNullOrEmpty(heroData.ID))
                    passivesText.text = "";
                else
                    passivesText.text = GetLocalizedPassivesBlock(heroData);
            }

            if (activeSkillText != null)
            {
                if (string.IsNullOrEmpty(heroData.ID))
                    activeSkillText.text = "";
                else
                    activeSkillText.text = GetLocalizedSuperSkillBlock(heroData);
            }
        }

        private string GetLocalizedUnknownHero()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "Неизвестный Герой";
                case 2: return "Unbekannter Held";
                case 3: return "Héros inconnu";
                case 4: return "Héroe desconocido";
                case 5: return "Herói desconhecido";
                case 6: return "未知のヒーロー";
                case 7: return "알 수 없는 영웅";
                case 8: return "未知英雄";
                default: return "Unknown Hero";
            }
        }

        private string GetLocalizedHeroName(string id, string defaultName)
        {
            int lang = Translator.LanguageID;
            if (id == "warrior_prem")
            {
                switch (lang)
                {
                    case 0: return "Воин";
                    case 2: return "Krieger (Premium)";
                    case 3: return "Guerrier (Premium)";
                    case 4: return "Guerrero (Premium)";
                    case 5: return "Guerreiro (Premium)";
                    case 6: return "戦士 (プレミアム)";
                    case 7: return "전사 (프리미엄)";
                    case 8: return "战士 (豪华)";
                    default: return "Warrior";
                }
            }
            if (id == "archer_prem")
            {
                switch (lang)
                {
                    case 0: return "Стрелок";
                    case 2: return "Bogenschütze (Premium)";
                    case 3: return "Archer (Premium)";
                    case 4: return "Arquero (Premium)";
                    case 5: return "Arqueiro (Premium)";
                    case 6: return "射手 (プレミアム)";
                    case 7: return "궁수 (프리미엄)";
                    case 8: return "游侠 (豪华)";
                    default: return "Archer";
                }
            }
            if (id == "mage_prem")
            {
                switch (lang)
                {
                    case 0: return "Маг";
                    case 2: return "Magier (Premium)";
                    case 3: return "Mage (Premium)";
                    case 4: return "Mago (Premium)";
                    case 5: return "Mago (Premium)";
                    case 6: return "魔術師 (プレミアム)";
                    case 7: return "마법사 (프리미엄)";
                    case 8: return "法师 (豪华)";
                    default: return "Mage";
                }
            }
            return defaultName;
        }

        private string GetLocalizedHeroType(string type)
        {
            int lang = Translator.LanguageID;
            bool isPrem = type == "Premium";

            if (isPrem)
            {
                switch (lang)
                {
                    case 0: return "Класс: <color=#FFD700>Премиум Герой</color>";
                    case 2: return "Klasse: <color=#FFD700>Premium-Held</color>";
                    case 3: return "Classe: <color=#FFD700>Héros Premium</color>";
                    case 4: return "Clase: <color=#FFD700>Héroe Premium</color>";
                    case 5: return "Classe: <color=#FFD700>Herói Premium</color>";
                    case 6: return "クラス: <color=#FFD700>プレミアムヒーロー</color>";
                    case 7: return "클래스: <color=#FFD700>프리미엄 영웅</color>";
                    case 8: return "职业: <color=#FFD700>豪华级英雄</color>";
                    default: return "Class: <color=#FFD700>Premium Hero</color>";
                }
            }
            else
            {
                switch (lang)
                {
                    case 0: return "Класс: Простой Герой";
                    case 2: return "Klasse: Basisheld";
                    case 3: return "Classe: Héros de base";
                    case 4: return "Clase: Héroe básico";
                    case 5: return "Classe: Herói básico";
                    case 6: return "クラス: 一般ヒーロー";
                    case 7: return "클래스: 기본 영웅";
                    case 8: return "职业: 基础英雄";
                    default: return "Class: Basic Hero";
                }
            }
        }

        private string GetLocalizedStatsBlock(BalanceConfig.HeroBalance data)
        {
            int lang = Translator.LanguageID;
            string keyHP = "HP", keyMP = "MP", keyATK = "ATK", keyDEF = "DEF", keySPD = "SPD", keyLCK = "LCK";

            switch (lang)
            {
                case 0:
                    keyHP = "Здоровье (HP)"; keyMP = "Мана (MP)"; keyATK = "Атака (ATK)"; keyDEF = "Защита (DEF)"; keySPD = "Скорость (SPD)"; keyLCK = "Удача (LCK)";
                    break;
                case 2:
                    keyHP = "Gesundheit (HP)"; keyMP = "Mana (MP)"; keyATK = "Angriff (ATK)"; keyDEF = "Verteidigung (DEF)"; keySPD = "Geschwindigkeit (SPD)"; keyLCK = "Glück (LCK)";
                    break;
                case 3:
                    keyHP = "Santé (HP)"; keyMP = "Mana (MP)"; keyATK = "Attaque (ATK)"; keyDEF = "Défense (DEF)"; keySPD = "Vitesse (SPD)"; keyLCK = "Chance (LCK)";
                    break;
                case 4:
                    keyHP = "Salud (HP)"; keyMP = "Maná (MP)"; keyATK = "Ataque (ATK)"; keyDEF = "Defensa (DEF)"; keySPD = "Velocidad (SPD)"; keyLCK = "Suerte (LCK)";
                    break;
                case 5:
                    keyHP = "Saúde (HP)"; keyMP = "Mana (MP)"; keyATK = "Ataque (ATK)"; keyDEF = "Defesa (DEF)"; keySPD = "Velocidade (SPD)"; keyLCK = "Sorte (LCK)";
                    break;
                case 6:
                    keyHP = "体力 (HP)"; keyMP = "魔力 (MP)"; keyATK = "攻撃 (ATK)"; keyDEF = "防御 (DEF)"; keySPD = "速度 (SPD)"; keyLCK = "幸運 (LCK)";
                    break;
                case 7:
                    keyHP = "체력 (HP)"; keyMP = "마나 (MP)"; keyATK = "공격 (ATK)"; keyDEF = "방어 (DEF)"; keySPD = "속도 (SPD)"; keyLCK = "행운 (LCK)";
                    break;
                case 8:
                    keyHP = "生命值 (HP)"; keyMP = "魔法值 (MP)"; keyATK = "攻击 (ATK)"; keyDEF = "防御 (DEF)"; keySPD = "速度 (SPD)"; keyLCK = "运气 (LCK)";
                    break;
                default:
                    keyHP = "Health (HP)"; keyMP = "Mana (MP)"; keyATK = "Attack (ATK)"; keyDEF = "Defense (DEF)"; keySPD = "Speed (SPD)"; keyLCK = "Luck (LCK)";
                    break;
            }

            return $"<line-height=145%><color=#FF5555><b>{keyHP}:</b></color> <b>{data.HP}</b>\n" +
                   $"<color=#3399FF><b>{keyMP}:</b></color> <b>{data.MP}</b>\n" +
                   $"<color=#FF9933><b>{keyATK}:</b></color> <b>{data.ATK}</b>\n" +
                   $"<color=#A0A0A0><b>{keyDEF}:</b></color> <b>{data.DEF}</b>\n" +
                   $"<color=#FFFF55><b>{keySPD}:</b></color> <b>{data.SPD}</b>\n" +
                   $"<color=#33FF99><b>{keyLCK}:</b></color> <b>{data.LCK}</b></line-height>";
        }

        private string GetLocalizedPassivesBlock(BalanceConfig.HeroBalance data)
        {
            int lang = Translator.LanguageID;
            string title = "Passive Skills:";
            string none = "None";

            switch (lang)
            {
                case 0: title = "Пассивные Умения:"; none = "Отсутствуют"; break;
                case 2: title = "Passive Fähigkeiten:"; none = "Keine"; break;
                case 3: title = "Compétences passives:"; none = "Aucun"; break;
                case 4: title = "Habilidades pasivas:"; none = "Ninguno"; break;
                case 5: title = "Habilidades pasivas:"; none = "Nenhum"; break;
                case 6: title = "パッシブスキル:"; none = "なし"; break;
                case 7: title = "패시브 스킬:"; none = "없음"; break;
                case 8: title = "被动技能:"; none = "无"; break;
            }

            if (data.Passives == null || data.Passives.Length == 0)
            {
                return $"<b>{title}</b>\n<i>{none}</i>";
            }

            var cleanPassives = data.Passives.Select(p => 
            {
                string clean = p;
                if (p.StartsWith("["))
                {
                    int closeBracket = p.IndexOf(']');
                    if (closeBracket != -1 && p.Length > closeBracket + 1)
                    {
                        clean = p.Substring(closeBracket + 1).Trim();
                    }
                }
                
                if (clean == "CritMaster")
                {
                    switch (lang)
                    {
                        case 0: return "Крит-Мастер (+15% шанс крита)";
                        case 2: return "Krit-Meister (+15% Krit-Chance)";
                        case 3: return "Maître du critique (+15% chance de critique)";
                        case 4: return "Maestro crítico (+15% probabilidad de crítico)";
                        case 5: return "Mestre do crítico (+15% chance de crítico)";
                        case 6: return "クリティカルマスター (会心率+15%)";
                        case 7: return "크리티컬 마스터 (치명타 확률 +15%)";
                        case 8: return "暴击大师 (暴击率+15%)";
                        default: return "Crit Master (+15% Crit Chance)";
                    }
                }
                if (clean == "DoubleShot")
                {
                    switch (lang)
                    {
                        case 0: return "Двойной Выстрел (Доп. стрела 30% урона)";
                        case 2: return "Doppelschuss (Zusätzlicher Pfeil verursacht 30% Schaden)";
                        case 3: return "Double Tir (Tir supplémentaire infligeant 30% dégâts)";
                        case 4: return "Tiro Doble (Disparo adicional inflige 30% de daño)";
                        case 5: return "Tiro Duplo (Disparo adicional que causa 30% de dano)";
                        case 6: return "ダブルショット (追撃が30%の威力)";
                        case 7: return "더블 샷 (단 한 대 추가 공격으로 30%의 데미지)";
                        case 8: return "连环双射 (追加30%伤害的额外射击)";
                        default: return "Double Shot (Extra shot with 30% damage)";
                    }
                }
                if (clean == "ManaRegen")
                {
                    switch (lang)
                    {
                        case 0: return "Регенерация Маны (+5 MP за ход)";
                        case 2: return "Manaregeneration (+5 MP pro Runde)";
                        case 3: return "Régén. Mana (+5 MP par tour)";
                        case 4: return "Regen. de Maná (+5 MP por turno)";
                        case 5: return "Regen. de Mana (+5 MP por turno)";
                        case 6: return "マナ持続回復 (毎ターン+5 MP)";
                        case 7: return "마나 재생 (매 턴 마나 +5 재생)";
                        case 8: return "被动：魔力涌动 (+5 MP/每回合)";
                        default: return "Mana Regen (+5 MP regain per turn)";
                    }
                }
                return clean;
            });

            return $"<line-height=140%><b>{title}</b>\n\n" + string.Join("\n", cleanPassives.Select(s => $"• {s}")) + "</line-height>";
        }

        private string GetLocalizedSuperSkillBlock(BalanceConfig.HeroBalance data)
        {
            if (string.IsNullOrEmpty(data.SuperSkill))
            {
                switch (Translator.LanguageID)
                {
                    case 0: return "<line-height=140%><b>Активный Суперудар:</b>\n\n<i>Отсутствует</i></line-height>";
                    case 2: return "<line-height=140%><b>Spezialfähigkeit (Super):</b>\n\n<i>Nichts</i></line-height>";
                    case 3: return "<line-height=140%><b>Super-compétence active :</b>\n\n<i>Aucune</i></line-height>";
                    case 4: return "<line-height=140%><b>Superactiva:</b>\n\n<i>Ninguna</i></line-height>";
                    case 5: return "<line-height=140%><b>Superativa:</b>\n\n<i>Nenhuma</i></line-height>";
                    case 6: return "<line-height=140%><b>アクティブ必殺技:</b>\n\n<i>なし</i></line-height>";
                    case 7: return "<line-height=140%><b>액티브 슈퍼 궁극기:</b>\n\n<i>없음</i></line-height>";
                    case 8: return "<line-height=140%><b>终极主动技能：</b>\n\n<i>无</i></line-height>";
                    default: return "<line-height=140%><b>Active Ultimate Skill:</b>\n\n<i>None</i></line-height>";
                }
            }

            int lang = Translator.LanguageID;
            string title = "Active Ultimate:";
            string cd = "Cooldown:";
            string pwr = "Power:";
            string turnSuffix = " t.";
            string superName = data.SuperSkill;

            if (superName == "HolyShield")
            {
                switch (lang)
                {
                    case 0: superName = "Святой Щит"; break;
                    case 2: superName = "Heiliger Schild"; break;
                    case 3: superName = "Bouclier sacré"; break;
                    case 4: superName = "Escudo sagrado"; break;
                    case 5: superName = "Escudo sagrado"; break;
                    case 6: superName = "ホーリーシールド"; break;
                    case 7: superName = "신성한 방패"; break;
                    case 8: superName = "圣光护盾"; break;
                    default: superName = "Holy Shield"; break;
                }
            }
            else if (superName == "Multishot" || superName == "ArrowRain" || superName == "DeathRain")
            {
                switch (lang)
                {
                    case 0: superName = (data.SuperSkill == "DeathRain") ? "Ливень Смерти" : "Залп Стрел"; break;
                    case 2: superName = (data.SuperSkill == "DeathRain") ? "Todesregen" : "Pfeilregen"; break;
                    case 3: superName = (data.SuperSkill == "DeathRain") ? "Pluie de la mort" : "Pluie de flèches"; break;
                    case 4: superName = (data.SuperSkill == "DeathRain") ? "Lluvia de muerte" : "Lluvia de flechas"; break;
                    case 5: superName = (data.SuperSkill == "DeathRain") ? "Chuva da morte" : "Chuva de flechas"; break;
                    case 6: superName = (data.SuperSkill == "DeathRain") ? "デスレイン" : "アローレイン"; break;
                    case 7: superName = (data.SuperSkill == "DeathRain") ? "죽음의 비" : "화살 소나기"; break;
                    case 8: superName = (data.SuperSkill == "DeathRain") ? "死亡之雨" : "箭雨风暴"; break;
                    default: superName = (data.SuperSkill == "DeathRain") ? "Death Rain" : "Arrow Rain"; break;
                }
            }
            else if (superName == "Fireball")
            {
                switch (lang)
                {
                    case 0: superName = "Огненный Шар"; break;
                    case 2: superName = "Feuerball"; break;
                    case 3: superName = "Boule de feu"; break;
                    case 4: superName = "Bola de fuego"; break;
                    case 5: superName = "Bola de fogo"; break;
                    case 6: superName = "ファイアボール"; break;
                    case 7: superName = "화염구"; break;
                    case 8: superName = "烈焰火球"; break;
                    default: superName = "Fireball"; break;
                }
            }

            switch (lang)
            {
                case 0: title = "Активный Суперудар:"; cd = "Перезарядка:"; pwr = "Мощь:"; turnSuffix = " х."; break;
                case 2: title = "Spezialfähigkeit:"; cd = "Abklingzeit:"; pwr = "Macht:"; turnSuffix = " R."; break;
                case 3: title = "Super-compétence :"; cd = "Recharge :"; pwr = "Puissance :"; turnSuffix = " tr."; break;
                case 4: title = "Superactiva:"; cd = "Enfriamiento:"; pwr = "Poder:"; turnSuffix = " t."; break;
                case 5: title = "Superativa:"; cd = "Recarga:"; pwr = "Poder:"; turnSuffix = " t."; break;
                case 6: title = "アクティブ必殺技:"; cd = "クールダウン:"; pwr = "威力:"; turnSuffix = " ターン"; break;
                case 7: title = "액티브 궁극기:"; cd = "재사용 대기시간:"; pwr = "위력:"; turnSuffix = " 턴"; break;
                case 8: title = "终极主动技能："; cd = "冷却时间："; pwr = "技能威力："; turnSuffix = " 回合"; break;
            }

            return $"<line-height=140%><b>{title}</b>\n" +
                   $"<color=#FFD700><b>{superName}</b></color>\n\n" +
                   $"• {cd} <b>{data.SuperCooldown}{turnSuffix}</b>\n" +
                   $"• {pwr} <b>x{data.SuperPower}</b></line-height>";
        }

        private void OnDifficultyChanged(int index)
        {
            if (balanceConfig == null || balanceConfig.Difficulties == null || index < 0 || index >= balanceConfig.Difficulties.Length) return;

            selectedDifficultyIndex = index;
            PlayerPrefs.SetInt("Difficulty", selectedDifficultyIndex); // Фиксируем выбор уровня сложности для геймплея
            UpdateDifficultyDescriptionText();
        }

        private void UpdateDifficultyDescriptionText()
        {
            if (difficultyDescriptionText == null || balanceConfig == null || balanceConfig.Difficulties == null || selectedDifficultyIndex >= balanceConfig.Difficulties.Length) return;

            var d = balanceConfig.Difficulties[selectedDifficultyIndex];
            int lang = Translator.LanguageID;

            ApplyFontToText(difficultyDescriptionText, false);

            string phraseAggro = "AI Aggro: ";
            string phraseDef = "AI Defense: ";
            string phraseEcon = "Player Gold: ";
            string phraseGoldBonus = "AI Gold: ";
            string phraseTurns = "AI Predict: ";
            string phraseGoldSuffix = "g";

            switch (lang)
            {
                case 0: // Russian
                    phraseAggro = "Агрессия ИИ: ";
                    phraseDef = "Защита ИИ: ";
                    phraseEcon = "Золото игрока: ";
                    phraseGoldBonus = "Бонус золота ИИ: ";
                    phraseTurns = "Прогноз ходов: ";
                    phraseGoldSuffix = " зол.";
                    break;
                case 2: // German
                    phraseAggro = "KI-Aggro: ";
                    phraseDef = "KI-Verteidigung: ";
                    phraseEcon = "Spieler-Gold: ";
                    phraseGoldBonus = "KI-Startgold: ";
                    phraseTurns = "KI-Prognose: ";
                    phraseGoldSuffix = " G";
                    break;
                case 3: // French
                    phraseAggro = "Aggro IA: ";
                    phraseDef = "Défense IA: ";
                    phraseEcon = "Or joueur: ";
                    phraseGoldBonus = "Or bonus IA: ";
                    phraseTurns = "Prév. tours: ";
                    phraseGoldSuffix = " or";
                    break;
                case 4: // Spanish
                    phraseAggro = "Agresión IA: ";
                    phraseDef = "Defensa IA: ";
                    phraseEcon = "Oro jugador: ";
                    phraseGoldBonus = "Oro extra IA: ";
                    phraseTurns = "Pred. turnos: ";
                    phraseGoldSuffix = " oro";
                    break;
                case 5: // Portuguese
                    phraseAggro = "Agressão IA: ";
                    phraseDef = "Defesa IA: ";
                    phraseEcon = "Ouro jogador: ";
                    phraseGoldBonus = "Ouro extra IA: ";
                    phraseTurns = "Pred. turnos: ";
                    phraseGoldSuffix = " ouro";
                    break;
                case 6: // Japanese
                    phraseAggro = "AI 攻撃性: ";
                    phraseDef = "AI 防御力: ";
                    phraseEcon = "ゴールド倍率: ";
                    phraseGoldBonus = "AI ボーナス: ";
                    phraseTurns = "AI 予測: ";
                    phraseGoldSuffix = " G";
                    break;
                case 7: // Korean
                    phraseAggro = "AI 공격성: ";
                    phraseDef = "AI 방어력: ";
                    phraseEcon = "골드 배율: ";
                    phraseGoldBonus = "AI 보너스: ";
                    phraseTurns = "AI 예측 턴: ";
                    phraseGoldSuffix = " 골드";
                    break;
                case 8: // Chinese
                    phraseAggro = "AI 攻击倾向: ";
                    phraseDef = "AI 防御倍率: ";
                    phraseEcon = "玩家黄金乘数: ";
                    phraseGoldBonus = "AI 初始黄金: ";
                    phraseTurns = "预测回合数: ";
                    phraseGoldSuffix = " 黄金";
                    break;
            }

            difficultyDescriptionText.text = $"<line-height=150%>" +
                                             $"{phraseAggro}<b><color=#FF5555>{d.Aggression * 100}%</color></b>\n" +
                                             $"{phraseDef}<b><color=#55AAFF>x{d.Defense}</color></b>\n" +
                                             $"{phraseEcon}<b><color=#FFFF55>x{d.EconMod}</color></b>\n" +
                                             $"{phraseGoldBonus}<b><color=#FFCC00>+{d.AIGoldBonus}{phraseGoldSuffix}</color></b>\n" +
                                             $"{phraseTurns}<b><color=#55FF55>{d.ForecastTurns}</color></b>" +
                                             $"</line-height>";
        }

        private void UpdateButtonsLocalization()
        {
            if (confirmButton != null)
            {
                var txt = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    ApplyFontToText(txt, true);
                    txt.text = GetLocalizedConfirmText();
                }
            }
            if (backToMenuButton != null)
            {
                var txt = backToMenuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    ApplyFontToText(txt, true);
                    txt.text = GetLocalizedBackText();
                }
            }
        }

        private string GetLocalizedConfirmText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "НАЧАТЬ ПУТЕШЕСТВИЕ";
                case 2: return "REISE STARTEN";
                case 3: return "COMMENCER LE VOYAGE";
                case 4: return "COMENZAR VIAJE";
                case 5: return "INICIAR JORNADA";
                case 6: return "旅を始める";
                case 7: return "여정 시작";
                case 8: return "开始征途";
                default: return "START JOURNEY";
            }
        }

        private string GetLocalizedBackText()
        {
            switch (Translator.LanguageID)
            {
                case 0: return "ВЕРНУТЬСЯ В МЕНЮ";
                case 2: return "ZUM MENÜ ZURÜCK";
                case 3: return "RETOUR AU MENU";
                case 4: return "VOLVER AL MENÚ";
                case 5: return "VOLTAR AO MENU";
                case 6: return "MENUに戻る";
                case 7: return "메뉴로 돌아가기";
                case 8: return "返回主菜单";
                default: return "RETURN TO MENU";
            }
        }

        public void ConfirmSelectionAndStart()
        {
            if (selectedHeroIndex == -1 || pedestals == null || selectedHeroIndex >= pedestals.Length) return;

            var chosenPedestal = pedestals[selectedHeroIndex];
            var heroData = balanceConfig.Heroes.FirstOrDefault(h => h.ID == chosenPedestal.HeroID);

            if (string.IsNullOrEmpty(heroData.ID)) return;

            // Заполняем сохранение SaveGameSystem перед стартом игры
            SaveGameSystem.CurrentData.saveName = GetLocalizedHeroName(chosenPedestal.HeroID, heroData.Name);
            SaveGameSystem.CurrentData.characterClass = chosenPedestal.HeroID;
            SaveGameSystem.CurrentData.playerLevel = 1;
            SaveGameSystem.CurrentData.currentXP = 0;
            int difficultyStartingGold = 150;
            switch (selectedDifficultyIndex)
            {
                case 0: difficultyStartingGold = 1000; break; // Новичок
                case 1: difficultyStartingGold = 500;  break; // Легко
                case 2: difficultyStartingGold = 300;  break; // Нормально
                case 3: difficultyStartingGold = 200;  break; // Сложно
                case 4: difficultyStartingGold = 100;  break; // Кошмар (Самый сложный)
            }
            SaveGameSystem.CurrentData.gold = difficultyStartingGold;
            SaveGameSystem.CurrentData.currentHealth = heroData.HP;
            SaveGameSystem.CurrentData.maxHealth = heroData.HP;
            SaveGameSystem.CurrentData.selectedDifficulty = selectedDifficultyIndex; // Записываем сложность в сохранение!

            // Присваиваем дефолтные атрибуты характеристик в зависимости от класса
            int startingSTR = 10;
            int startingAGI = 10;
            int startingINT = 10;
            int startingSTA = 10;

            string hid = chosenPedestal.HeroID.ToLower();
            if (hid.Contains("warrior") || hid.Contains("voin") || hid.Contains("paladin"))
            {
                startingSTR = 15;
                startingAGI = 10;
                startingINT = 4;
                startingSTA = 15;
            }
            else if (hid.Contains("archer") || hid.Contains("strelok") || hid.Contains("ranger") || hid.Contains("bow"))
            {
                startingSTR = 10;
                startingAGI = 14;
                startingINT = 6;
                startingSTA = 11;
            }
            else if (hid.Contains("mage") || hid.Contains("wizard") || hid.Contains("mag") || hid.Contains("staff"))
            {
                startingSTR = 6;
                startingAGI = 10;
                startingINT = 10;
                startingSTA = 9;
            }

            SaveGameSystem.CurrentData.strength = startingSTR;
            SaveGameSystem.CurrentData.agility = startingAGI;
            SaveGameSystem.CurrentData.intelligence = startingINT;
            SaveGameSystem.CurrentData.stamina = startingSTA;

            // Расчет стартового пула свободных очков (difficulty bonus) в зависимости от уровня сложности:
            // Новичок: +30, Легко: +20, Нормально: +10, Сложно: +5, Кошмар: +0
            int difficultyBonusPoints = 10;
            switch (selectedDifficultyIndex)
            {
                case 0: difficultyBonusPoints = 30; break; // Новичок (+30)
                case 1: difficultyBonusPoints = 20; break; // Легко (+20)
                case 2: difficultyBonusPoints = 10; break; // Нормально (+10)
                case 3: difficultyBonusPoints = 5;  break; // Сложно (+5)
                case 4: difficultyBonusPoints = 0;  break; // Кошмар (+0)
            }
            SaveGameSystem.CurrentData.availableSkillPoints = difficultyBonusPoints;

            // Сохраняем в Слот 0 (первичный слот новой игры) и сбрасываем состояние тактической кампании в PlayerPrefs
            PlayerPrefs.SetInt("Active_Save_Slot", 0);
            PlayerPrefs.SetInt("ContinentGameplayActive", 0);
            PlayerPrefs.SetInt("LandedZoneIndex", -1);
            PlayerPrefs.SetInt("Fate_Current_Day", 1);
            PlayerPrefs.SetInt("Player_Current_Gold", difficultyStartingGold);
            PlayerPrefs.SetInt("Player_Gold_Reserve", difficultyStartingGold);
            PlayerPrefs.Save();
            SaveGameSystem.Save(0);

            Debug.Log($"[FATE START] Выбран герой {heroData.Name}. Уровень сложности: {balanceConfig.Difficulties[selectedDifficultyIndex].LevelName}. Готовим переход на игровую сцену.");

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
            Debug.Log($"[FATE START] Возврат в Главное Меню. loadMenuByName={loadMenuByName}, Name='{mainMenuSceneName}', Index={mainMenuSceneIndex}");
            if (loadMenuByName)
            {
                if (LoadingScreenManager.Instance != null)
                {
                    LoadingScreenManager.Instance.LoadScene(mainMenuSceneName);
                }
                else
                {
                    SceneManager.LoadScene(mainMenuSceneName);
                }
            }
            else
            {
                if (LoadingScreenManager.Instance != null)
                {
                    LoadingScreenManager.Instance.LoadScene(mainMenuSceneIndex);
                }
                else
                {
                    SceneManager.LoadScene(mainMenuSceneIndex);
                }
            }
        }
    }
}
