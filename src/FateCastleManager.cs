using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateContinent;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Разработчик: Fate Continent (Континент Судьбы)
/// Zenith Glassmorphism Design System (8K Ultra-High Density) • v18.11.15
/// Скрипт пошаговой экономики, процедурного 3D-морфинга замков, 2D-города и ИИ-оппонентов.
/// </summary>
public class FateCastleManager : MonoBehaviour
{
    public static FateCastleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FateCastleManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FateCastleManager_Runtime");
                    instance = go.AddComponent<FateCastleManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("<color=cyan>[FATE CASTLE] Dynamic runtime self-instantiation triggered.</color>");
                }
            }
            return instance;
        }
    }
    private static FateCastleManager instance;

    // Drifting fields for autonomous castle movement
    private float driftTimer = 0f;
    private bool makeCastlesDriftAutonomously = false;

    // Cached Texture2D objects to optimize RAM and completely stop memory leaks in OnGUI
    private Texture2D hudTex;
    private Texture2D barBgTex;
    private Texture2D hpTex;
    private Texture2D mpTex;
    private Texture2D xpTex;

    private void InitializeCachedTextures()
    {
        if (hudTex == null)
        {
            hudTex = new Texture2D(1, 1);
            hudTex.SetPixel(0, 0, new Color(0.04f, 0.08f, 0.22f, 0.90f));
            hudTex.Apply();
        }
        if (barBgTex == null)
        {
            barBgTex = new Texture2D(1, 1);
            barBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.6f));
            barBgTex.Apply();
        }
        if (hpTex == null)
        {
            hpTex = new Texture2D(1, 1);
            hpTex.SetPixel(0, 0, new Color(0.85f, 0.15f, 0.2f, 1.0f));
            hpTex.Apply();
        }
        if (mpTex == null)
        {
            mpTex = new Texture2D(1, 1);
            mpTex.SetPixel(0, 0, new Color(0.12f, 0.5f, 0.95f, 1.0f));
            mpTex.Apply();
        }
        if (xpTex == null)
        {
            xpTex = new Texture2D(1, 1);
            xpTex.SetPixel(0, 0, new Color(0.05f, 0.85f, 0.65f, 1.0f));
            xpTex.Apply();
        }
    }

    private void OnDestroy()
    {
        if (hudTex != null) { Destroy(hudTex); hudTex = null; }
        if (barBgTex != null) { Destroy(barBgTex); barBgTex = null; }
        if (hpTex != null) { Destroy(hpTex); hpTex = null; }
        if (mpTex != null) { Destroy(mpTex); mpTex = null; }
        if (xpTex != null) { Destroy(xpTex); xpTex = null; }
    }

    [System.Serializable]
    public class CastleInstance
    {
        public int zoneIndex;
        public string nameRU;
        public string nameEN;
        public string nameCH;
        public string nameKR;
        public string owner; // "Player" or "Enemy"
        public int level = 1; // 1 to 6
        public float goldAccumulated;
        [System.NonSerialized] public GameObject visualRoot;
        
        // AI commander stats
        public int aiCommanderLevel = 1;
        public int aiTroopsPower = 10;
        public int aiArmorTier = 1;
        public int aiPotionsStock = 1;
    }

    public List<CastleInstance> castles = new List<CastleInstance>();

    [Header("🏰 MANUAL CASTLE PLACEMENT & OVERRIDES")]
    [Tooltip("If checked, the script will use the custom manual positions specified below instead of landing point anchors")]
    public bool useManualCastlePositions = true;
    
    [Tooltip("If checked, the script will strictly load customCastlePositions from the C# code, ignoring PlayerPrefs local registry configurations.")]
    public bool preferScriptCoordinates = false;
    
    [Tooltip("Manual 3D positions for the 12 castles (matched to the 12 region cells of New_Kontinent)")]
    public Vector3[] customCastlePositions = new Vector3[12]
    {
        new Vector3(-15f, 0f, 10f),    // Region_00
        new Vector3(-5f, 0f, 10f),     // Region_01
        new Vector3(5f, 0f, 10f),      // Region_02
        new Vector3(-5.3f, -0.4f, 4.2f), // Region_03 (Святилище Зенита)
        new Vector3(-15f, 0f, 0f),     // Region_04
        new Vector3(-5f, 0f, 0f),      // Region_05
        new Vector3(14.8f, 1.2f, 12.5f), // Region_06 (Ледяной Пик)
        new Vector3(15f, 0f, 0f),      // Region_07
        new Vector3(-12.4f, -0.3f, -10.2f), // Region_08 (Древние Руины)
        new Vector3(-5f, 0f, -10f),    // Region_09
        new Vector3(5f, 0f, -10f),     // Region_10
        new Vector3(9.9f, 0.8f, -4.5f) // Region_11 (Кровавые Пустоши)
    };

    [Tooltip("Manual offset added to the spawn anchor of each landing point if not using customCastlePositions")]
    public Vector3[] castleManualOffsets = new Vector3[4]
    {
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f)
    };

    [Tooltip("Manual override of BoxCollider size for each castle. Edit here directly in inspector, or use calibrator!")]
    public Vector3[] castleColliderSizes = new Vector3[12];

    [Tooltip("Manual override of BoxCollider center for each castle. Edit here directly in inspector, or use calibrator!")]
    public Vector3[] castleColliderCenters = new Vector3[12];

    [Tooltip("Snaps the final height coordinate of any spawned 3D castle onto the terrain below it")]
    public bool snapCastlesToTerrain = true;

    [Header("💵 MANUAL CHRONO & TREASURY CONFIG")]
    [Tooltip("Initial day of the campaign (Default to 1)")]
    public int initialDaySetting = 1;
    
    [Tooltip("Initial gold/treasury of the campaign (Default to 100)")]
    public int initialGoldSetting = 100;

    [HideInInspector]
    public bool isContinentGameplayActive = false;

    [Header("MANUAL / AUTONOMOUS ESPIONAGE CONFIG (v18.11.15)")]
    public bool useAutonomousEspionageSettings = true;
    public int manualMinLevelForEspionage = 3;
    public int manualBaseSpyCost = 150;
    
    [Header("MANUAL / AUTONOMOUS GARRISON CAPACITY (v18.11.15)")]
    public bool useAutonomousGarrisonSettings = true;
    public int manualLevel1_2_Cap = 4;
    public int manualLevel3_4_Cap = 5;
    public int manualLevel5_Cap = 6;
    public int manualLevel6_Cap = 7;

    [Header("🤖 MANUAL AI OPPONENT SIMULATION CONFIG (v18.11.15 EXPOSED)")]
    [Tooltip("If checked, use the manual opponent variables declared below instead of the auto formulas based on selected difficulty settings")]
    public bool useManualAiSimulationSettings = false;
    
    [Range(0f, 1f)]
    [Tooltip("Manual base probability of opponent castle automatic upgrade per turn")]
    public float manualAiUpgradeProbability = 0.30f;

    [Range(0f, 1f)]
    [Tooltip("Manual base probability of opponent garrison troop recruitment per turn")]
    public float manualAiRecruitProbability = 0.40f;

    [Range(0f, 1f)]
    [Tooltip("Manual base probability of opponent equipment tier upgrade per turn")]
    public float manualAiEquipmentProbability = 0.35f;

    [Tooltip("Manual opponent starting base gold income per level multiplier")]
    public float manualAiIncomeMultiplier = 1.35f;
    
    [Tooltip("Manual starting troop power boost for all opponent castles on campaign start")]
    public int manualAiStartingTroopPower = 15;

    [Header("⚔️ WARRIOR GLASSMORPHIC SKILLS ICONS")]
    [Tooltip("Passive 1: IronSkin Icon")]
    public Texture2D warriorSkillPassive1;
    [Tooltip("Passive 2: Regen Icon")]
    public Texture2D warriorSkillPassive2;
    [Tooltip("Passive 3: Threat Icon")]
    public Texture2D warriorSkillPassive3;
    [Tooltip("Ultimate: TitanShield Icon")]
    public Texture2D warriorSkillUltimate;

    [Header("🎯 ARCHER GLASSMORPHIC SKILLS ICONS")]
    [Tooltip("Passive 1: Crit-Master Icon")]
    public Texture2D archerSkillPassive1;
    [Tooltip("Passive 2: LongShot Icon")]
    public Texture2D archerSkillPassive2;
    [Tooltip("Passive 3: Evasion Icon")]
    public Texture2D archerSkillPassive3;
    [Tooltip("Ultimate: Death Rain Icon")]
    public Texture2D archerSkillUltimate;

    [Header("🔮 MAGE GLASSMORPHIC SKILLS ICONS")]
    [Tooltip("Passive 1: ManaFlow Icon")]
    public Texture2D mageSkillPassive1;
    [Tooltip("Passive 2: Elemental Icon")]
    public Texture2D mageSkillPassive2;
    [Tooltip("Passive 3: Resist Icon")]
    public Texture2D mageSkillPassive3;
    [Tooltip("Ultimate: Time Rift Icon")]
    public Texture2D mageSkillUltimate;

    [Header("👥 ВОИНЫ КАЗАРМЫ - АВАТАРКИ/ИКОНКИ (BARRACKS TROOP AVATARS)")]
    [Tooltip("Боец фракции • Prompt: Symmetrical front portrait of medieval royal infantry fighter, chromium helmet, turquoise neon accents, flat white background.")]
    public Texture2D avatar_warrior;
    [Tooltip("Эльфийский Лучник • Prompt: Symmetrical front portrait of elegant elven forest scout archer, emerald leather hood cowl, glowing green eyes, flat white background.")]
    public Texture2D avatar_archer;
    [Tooltip("Боевой Маг Зенита • Prompt: Symmetrical front portrait of majestic battle mage, cosmic violet hood, glowing arcane face runes, flat white background.")]
    public Texture2D avatar_mage;
    [Tooltip("Паладин Света • Prompt: Symmetrical front portrait of legendary holy paladin templar knight, golden runic plate armor, bright halo, flat white background.")]
    public Texture2D avatar_paladin;
    [Tooltip("Имперская Конница • Prompt: Symmetrical front portrait of heavy royal cavalry crusader knight on armored destrier horse, obsidian lance, flat white background.")]
    public Texture2D avatar_cavalry;
    [Tooltip("Осадно-боевой Пушкарь • Prompt: Symmetrical front portrait of seasoned dwarf fortress cannon engineer with brass machinery goggles, coal smoke, flat white background.")]
    public Texture2D avatar_cannoneer;
    [Tooltip("Кентавр Степей • Prompt: Symmetrical front portrait of wild plain centaur hunter master, braided rustic hair, holding ashwood spear, flat white background.")]
    public Texture2D avatar_centaur;
    [Tooltip("Некромант Тьмы • Prompt: Symmetrical front portrait of dark occult necromancer mage, skull mask cowl hood, green eerie bone spell particles, flat white background.")]
    public Texture2D avatar_necromancer;
    [Tooltip("Элитный Королевский Грифон • Prompt: Symmetrical close-up front-facing hawk portrait of ancient royal phoenix griffin beast with golden feather crest, flat white background.")]
    public Texture2D avatar_griffin;
    [Tooltip("Рыцарь-Властелин • Prompt: Symmetrical front portrait of dread skeleton doom warlord in spiky dark void iron crown plates, purple glow, flat white background.")]
    public Texture2D avatar_overlord;
    [Tooltip("Многоголовая Гидра • Prompt: Symmetrical front portrait of terrifying multi-headed swamp hydra dragon serpent reptilian heads, glowing green venom spit, flat white background.")]
    public Texture2D avatar_hydra;
    [Tooltip("Легендарный Дракон Пустоты • Prompt: Symmetrical front portrait of giant celestial void leviathan dragon beast, body of glowing purple nebula gas, flat white background.")]
    public Texture2D avatar_dragon;
    [Tooltip("Ураганный Медведь Гор • Prompt: Symmetrical front portrait of colossus runic polar bear guardian, chestplates carved of mountain range blue runic ice, flat white background.")]
    public Texture2D avatar_mountain_bear;
    [Tooltip("Гигантская Змея Пустошей • Prompt: Symmetrical front portrait of massive desert dunes sands serpent, golden crystalline scales, open jaws of crystalline sand-fire, flat white background.")]
    public Texture2D avatar_wasteland_serpent;

    [Header("🕵️ ПРОСТЫЕ НАНИМАЕМЫЕ ГЕРОИ - АВАТАРКИ/ИКОНКИ")]
    [Tooltip("Герой-Стрелок • Prompt: High precision portrait of elite fantasy rangers bowmaster, sapphire eyes, runic leather hood, white background.")]
    public Texture2D avatar_hero_archer;
    [Tooltip("Герой-Воин • Prompt: High precision portrait of grizzled barbarian gladiator fighter, scarred cheeks, giant skull pauldron plate, white background.")]
    public Texture2D avatar_hero_warrior;
    [Tooltip("Герой-Боевой Маг • Prompt: High precision portrait of high sorcerer archmage, starry cosmic wizard crown beard, glowing nebula light, white background.")]
    public Texture2D avatar_hero_mage;

    // Active dynamic placeholders resolved on Load/Draw
    private Texture2D activeSkillPassive1;
    private Texture2D activeSkillPassive2;
    private Texture2D activeSkillPassive3;
    private Texture2D activeSkillUltimate;
    
    // UI states and trackers
    public bool isTownViewActive = false;
    private int currentTownSubPanel = 0; // 0: All Columns (split view), 1: Barracks only, 2: Forge only, 3: Academy only
    private bool showTroopDetailPopup = false;
    private string selectedTroopId = "";

    public bool isAutonomousStatsDistribution = false;
    public bool showStatsPanel = false;
    public bool isDetailsOpen = false;
    private int activeDetailsIndex = -1;
    private string feedbackMessage = "";
    private float messageTimer = 0f;
    
    public int currentDay = 1;

    // AI notification logs shown during new-day transition
    private List<string> aiLogs = new List<string>();
    private bool showNewDayOverlay = false;
    private float overlayTimer = 0f;

    // Специфические поля для всплывающих окон описания навыков и интерактивной калибровки координат (v18.11.16)
    private bool showSkillDetailPopup = false;
    private string selectedSkillName = "";
    private string selectedSkillDesc = "";
    private Texture2D selectedSkillIcon = null;
    private string selectedSkillType = "";

    private bool showCastleCalibrationPanel = false;
    private int selectedCalibCastleIdx = 0;

    // Scroll vectors for columns
    private Vector2 barracksScroll = Vector2.zero;
    private Vector2 forgeScroll = Vector2.zero;
    private Vector2 academyScroll = Vector2.zero;
    private Vector2 statsScroll = Vector2.zero;
    private Vector2 invScroll = Vector2.zero;

    // Inventory & Equipment System variables (v18.11.20)
    private int eqBonusSTR = 0;
    private int eqBonusAGI = 0;
    private int eqBonusINT = 0;
    private int eqBonusSTA = 0;

    [Serializable]
    public class InventoryItem
    {
        public string id = "";
        public string name = "";
        public string iconType = ""; // "hp", "str", "def", "armor", "weapon", "ring", "shoulders", "helmet", "boots", "belt", "necklace"
        public int slotType = 0; // 1: head, 2: neck, 3: shoulders, 4: chest, 5: ring, 6: belt, 7: boots, 8: weapon, 0: consumable potion
        public int level = 1;
        public int count = 0;
        public int statBonus = 0;
    }

    [Serializable]
    public class InventoryData
    {
        public InventoryItem[] items = new InventoryItem[999];
    }

    [Serializable]
    public class EquipmentData
    {
        public InventoryItem[] slots = new InventoryItem[9]; // index 1 to 8 matches Slot 1 to Slot 8
    }

    private InventoryData playerInventory = new InventoryData();
    private EquipmentData playerEquipment = new EquipmentData();

    // Aelyssa character panel tutorial state (v18.11.20)
    public bool isAelyssaTutorialActive = false;
    private int tutorialStep = 0;

    private string[][] aelyssaTutorialDialogsRU = new string[][] {
        new string[] { "Аэлисса", "Приветствую тебя, герой Континента Судьбы! Я вижу, ты открыл Панель Управления Героем. Давай я расскажу тебе, как устроен этот интерфейс, чтобы ты мог эффективно использовать все его возможности!" },
        new string[] { "Аэлисса", "Слева находится вкладка ХАРАКТЕРИСТИК. Здесь ты можешь распределять очки, полученные за новые уровни. Сила (STR) повышает урон, Ловкость (AGI) — защиту, Интеллект (INT) — ману, а Выносливость (STA) — твое максимальное здоровье!" },
        new string[] { "Аэлисса", "По центру сверху ты видишь МАНЕКЕН СНАРЯЖЕНИЯ. Всего у тебя есть 8 слотов для экипировки (Оружие, Шлем, Доспех, Наплечники, Сапоги, Пояс, Амулет и Кольцо). Нажатие на экипированный предмет снимет его!" },
        new string[] { "Аэлисса", "Снизу по центру расположены твои КЛАССОВЫЕ НАВЫКИ — пассивные и мощные ультимативные способности, которые зависят от твоего выбранного класса (Воин, Охотник или Маг). Они дают тебе огромные преимущества в бою!" },
        new string[] { "Аэлисса", "Справа находится твой ИНВЕНТАРЬ. Он огромный — расширяется до 999 ячеек! Изначально открыто только 10 слотов. Ты можешь покупать новые ячейки за золото (чем больше клеток, тем они дороже), либо получать БЕСПЛАТНО по 1 клетке за каждые 10 уровней героя!" },
        new string[] { "Аэлисса", "Чтобы надеть оружие или броню, либо выпить купленное зелье жизни, силы или защиты — просто кликни по предмету в инвентаре! Твои характеристики пересчитаются мгновенно, давая тебе силу!" },
        new string[] { "Аэлисса", "Помни, что инвентарь, зелья и индивидуальное снаряжение доступны ТОЛЬКО для тебя — главного героя! Обычные наемные отряды и солдаты сохраняют свою базовую простую структуру." },
        new string[] { "Аэлисса", "Также твой инвентарь напрямую связан с Кузницей и Магазином снадобий в замке! Выкованное оружие и купленные зелья теперь аккуратно складываются прямо сюда. Ну что, теперь ты полностью готов к битвам!" }
    };

    private string[][] aelyssaTutorialDialogsEN = new string[][] {
        new string[] { "Aelyssa", "Greetings, hero of the Fate Continent! I see you've opened the Hero Control Panel. Let me show you how this interface works so you can utilize all its power!" },
        new string[] { "Aelyssa", "On the left is the ATTRIBUTES tab. Here you allocate stat points gained from leveling up. Strength (STR) boosts damage, Agility (AGI) increases defense, Intelligence (INT) raises mana, and Stamina (STA) expands max health!" },
        new string[] { "Aelyssa", "In the upper center, you see the EQUIPMENT MANNEQUIN. You have 8 gear slots (Weapon, Helmet, Plate, Pauldrons, Boots, Belt, Pendant, and Ring). Clicking an equipped item unequips it back to your bag!" },
        new string[] { "Aelyssa", "In the lower center are your CLASS SKILLS — passive boosts and ultimate active powers matching your chosen class (Warrior, Archer, or Mage) to dominate the battlefield!" },
        new string[] { "Aelyssa", "On the right is your INVENTORY. It is massive, expanding up to 999 slots! You start with 10 slots. You can buy more with gold (prices rise dynamically), or get 1 free slot every 10 hero levels!" },
        new string[] { "Aelyssa", "To equip gear or drink health/might/ward elixirs, simply click the item in your inventory! Your hero statistics will instantly update with all equipment bonuses." },
        new string[] { "Aelyssa", "Remember, the inventory, potions, and equipment are strictly reserved for you — the main hero! Regular hired units and mercenary soldiers retain their basic simple structures." },
        new string[] { "Aelyssa", "Your inventory is also linked directly to the Forge and Potion Shop in your castle! Forged armor and bought elixirs are placed here. Now, you are fully prepared for the journey!" }
    };

    private string[][] aelyssaTutorialDialogsCH = new string[][] {
        new string[] { "阿艾莉萨", "你好，命运大陆的英雄！我看到你打开了英雄控制面板。让我为你介绍这个界面的构造，以便你完美发挥其全部力量！" },
        new string[] { "阿艾莉萨", "左侧是属性面板。你可以分配升级获得的点数。力量（STR）增加伤害，敏捷（AGI）提升防御，智力（INT）增加法力值，耐力（STA）提高生命值上限！" },
        new string[] { "阿艾莉萨", "中上方是装备模特。你拥有8个装备槽（武器、头盔、胸甲、护肩、鞋子、腰带、项链和戒指）。点击已装备的物品可以将其卸下并放回背包！" },
        new string[] { "阿艾莉萨", "中下方是你的职业技能——根据你选择的职业（战士、弓箭手或法师）提供的被动和强大的终极主动技能，助你掌控战场！" },
        new string[] { "阿艾莉萨", "右侧是你的背包，最大可扩展至999格！初始解锁10格。你可以用金币购买格位（越往后越贵），或者每升10级免费领取1格！" },
        new string[] { "阿艾莉萨", "要穿戴装备或饮用购买的生命、力量、防御药水，只需点击背包中的对应物品！你的英雄属性将会瞬间更新，立竿见影！" },
        new string[] { "阿艾莉萨", "请记住，背包、药水和个人装备仅供你——主角使用！普通的雇用士兵和佣兵队伍仍保持其原有的简单属性结构。" },
        new string[] { "阿艾莉萨", "你的背包也已与城堡的铁匠铺和药水商店直接关联！铁匠铺锻造的装备和购买的药水都会自动放入背包中。现在，你已完全准备好迎接战斗了！" }
    };

    private string[][] aelyssaTutorialDialogsKR = new string[][] {
        new string[] { "엘리사", "환영합니다, 운명의 대륙의 영웅이여! 영웅 제어 패널을 열어보셨군요. 모든 기능을 마스터할 수 있도록 이 인터페이스를 설명해 드릴게요!" },
        new string[] { "엘리사", "왼쪽은 능력치 탭입니다. 레벨업 시 획득한 포인트를 분배할 수 있죠. 힘(STR)은 공격력, 민첩(AGI)은 방어력, 지능(INT)은 마나, 체력(STA)은 최대 생명력을 증가시킵니다!" },
        new string[] { "엘리사", "중앙 상단에는 장비 마네킹이 있습니다. 무기, 투구, 갑옷, 어깨갑옷, 신발, 벨트, 아뮬렛, 반지 등 총 8개의 슬롯이 있죠. 장착 중인 아이템을 클릭하면 인벤토리로 장착 해제됩니다!" },
        new string[] { "엘리사", "중앙 하단은 선택한 클래스(전사, 궁수, 마법사)에 따른 패시브 및 궁극기 클래스 스킬입니다. 전투에서 엄청난 이점을 가져다 줄 거예요!" },
        new string[] { "엘리사", "오른쪽은 인벤토리입니다. 최대 999칸까지 확장 가능한 엄청난 공간이죠! 처음에는 10칸만 열려 있으며, 골드로 추가 슬롯을 구매하거나 10레벨마다 1칸씩 무료로 열 수 있습니다!" },
        new string[] { "엘리사", "무기나 방어구를 장착하거나, 구매한 물약을 사용하려면 인벤토리의 아이템을 클릭하세요! 모든 스탯이 실시간으로 계산되어 강해집니다." },
        new string[] { "엘리사", "참고로 인벤토리, 물약 및 개인 장비는 오직 주인공인 당신만 사용할 수 있습니다! 일반 용병들과 유닛들은 기본의 단순한 상태를 유지합니다." },
        new string[] { "엘리사", "또한 당신의 인벤토리는 성의 대장간과 성수 상점과 직접 연동됩니다! 제작된 무기나 구매한 물약은 바로 여기로 들어오죠. 자, 이제 전장으로 나갈 준비가 끝났습니다!" }
    };

    private int GetPurchasedSlotsCount()
    {
        return PlayerPrefs.GetInt("Player_Inventory_Purchased_Slots", 10);
    }

    private void SetPurchasedSlotsCount(int count)
    {
        PlayerPrefs.SetInt("Player_Inventory_Purchased_Slots", Mathf.Clamp(count, 10, 999));
        PlayerPrefs.Save();
    }

    private int GetUnlockedSlotsCount()
    {
        int purchased = GetPurchasedSlotsCount();
        int freeSlotsByLevel = 0;
        if (SaveGameSystem.CurrentData != null)
        {
            freeSlotsByLevel = SaveGameSystem.CurrentData.playerLevel / 10;
        }
        return Mathf.Clamp(purchased + freeSlotsByLevel, 10, 999);
    }

    private string GetTutorialSpeaker(int step, int lang)
    {
        string[][] arr = aelyssaTutorialDialogsEN;
        if (lang == 0) arr = aelyssaTutorialDialogsRU;
        else if (lang == 8) arr = aelyssaTutorialDialogsCH;
        else if (lang == 7) arr = aelyssaTutorialDialogsKR;

        if (step >= 0 && step < arr.Length) return arr[step][0];
        return "Aelyssa";
    }

    private string GetTutorialText(int step, int lang)
    {
        string[][] arr = aelyssaTutorialDialogsEN;
        if (lang == 0) arr = aelyssaTutorialDialogsRU;
        else if (lang == 8) arr = aelyssaTutorialDialogsCH;
        else if (lang == 7) arr = aelyssaTutorialDialogsKR;

        if (step >= 0 && step < arr.Length) return arr[step][1];
        return "";
    }

    private void SaveInventory()
    {
        string json = JsonUtility.ToJson(playerInventory);
        PlayerPrefs.SetString("Player_Inventory_JSON_v18", json);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        if (playerInventory == null) playerInventory = new InventoryData();
        if (playerInventory.items == null || playerInventory.items.Length < 999)
        {
            InventoryItem[] newItems = new InventoryItem[999];
            if (playerInventory.items != null)
            {
                for (int i = 0; i < Mathf.Min(playerInventory.items.Length, 999); i++)
                {
                    newItems[i] = playerInventory.items[i];
                }
            }
            playerInventory.items = newItems;
        }

        for (int i = 0; i < 999; i++)
        {
            if (playerInventory.items[i] == null) playerInventory.items[i] = new InventoryItem();
        }

        string json = PlayerPrefs.GetString("Player_Inventory_JSON_v18", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                InventoryData loaded = JsonUtility.FromJson<InventoryData>(json);
                if (loaded != null && loaded.items != null)
                {
                    for (int i = 0; i < Mathf.Min(999, loaded.items.Length); i++)
                    {
                        if (loaded.items[i] != null)
                        {
                            playerInventory.items[i] = loaded.items[i];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading inventory: " + ex.Message);
            }
        }
        else
        {
            // First time initialization - fill inventory with starter items
            playerInventory.items[0] = new InventoryItem { id = "weapon_bronze_1", name = GetItemName(8, 1, 0), iconType = "weapon", slotType = 8, level = 1, count = 1, statBonus = 3 };
            playerInventory.items[1] = new InventoryItem { id = "helmet_bronze_1", name = GetItemName(1, 1, 0), iconType = "helmet", slotType = 1, level = 1, count = 1, statBonus = 2 };
            playerInventory.items[2] = new InventoryItem { id = "potion_hp_1", name = "Зелье Жизни (Ур.1)", iconType = "hp", slotType = 0, level = 1, count = 3, statBonus = 0 };
            playerInventory.items[3] = new InventoryItem { id = "potion_str_1", name = "Зелье Силы (Ур.1)", iconType = "str", slotType = 0, level = 1, count = 1, statBonus = 0 };
            playerInventory.items[4] = new InventoryItem { id = "potion_def_1", name = "Зелье Защиты (Ур.1)", iconType = "def", slotType = 0, level = 1, count = 1, statBonus = 0 };
            SaveInventory();
        }
    }

    private void SaveEquipment()
    {
        string json = JsonUtility.ToJson(playerEquipment);
        PlayerPrefs.SetString("Player_Equipment_JSON_v18", json);
        PlayerPrefs.Save();
    }

    private void LoadEquipment()
    {
        if (playerEquipment == null) playerEquipment = new EquipmentData();
        for (int i = 0; i < 9; i++)
        {
            if (playerEquipment.slots[i] == null) playerEquipment.slots[i] = new InventoryItem();
        }

        string json = PlayerPrefs.GetString("Player_Equipment_JSON_v18", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                EquipmentData loaded = JsonUtility.FromJson<EquipmentData>(json);
                if (loaded != null && loaded.slots != null)
                {
                    for (int i = 0; i < Mathf.Min(9, loaded.slots.Length); i++)
                    {
                        if (loaded.slots[i] != null)
                        {
                            playerEquipment.slots[i] = loaded.slots[i];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading equipment: " + ex.Message);
            }
        }
    }

    private bool AddInventoryItem(string id, string name, string iconType, int slotType, int level, int statBonus)
    {
        LoadInventory();
        int unlockedCount = GetUnlockedSlotsCount();
        
        // Find stackable potion slot within unlocked slots
        for (int i = 0; i < unlockedCount; i++)
        {
            if (playerInventory.items[i] != null && playerInventory.items[i].id == id && playerInventory.items[i].level == level && slotType == 0)
            {
                playerInventory.items[i].count++;
                SaveInventory();
                return true;
            }
        }
        
        // Find empty slot within unlocked slots
        for (int i = 0; i < unlockedCount; i++)
        {
            if (playerInventory.items[i] == null || string.IsNullOrEmpty(playerInventory.items[i].id))
            {
                if (playerInventory.items[i] == null) playerInventory.items[i] = new InventoryItem();
                playerInventory.items[i].id = id;
                playerInventory.items[i].name = name;
                playerInventory.items[i].iconType = iconType;
                playerInventory.items[i].slotType = slotType;
                playerInventory.items[i].level = level;
                playerInventory.items[i].count = 1;
                playerInventory.items[i].statBonus = statBonus;
                SaveInventory();
                return true;
            }
        }
        return false;
    }

    private void UnequipItem(int slotIndex)
    {
        LoadEquipment();
        LoadInventory();
        InventoryItem itemToUnequip = playerEquipment.slots[slotIndex];
        if (itemToUnequip == null || string.IsNullOrEmpty(itemToUnequip.id)) return;

        int curLang = Translator.LanguageID;
        int unlockedCount = GetUnlockedSlotsCount();
        bool added = false;
        
        for (int i = 0; i < unlockedCount; i++)
        {
            if (playerInventory.items[i] == null || string.IsNullOrEmpty(playerInventory.items[i].id))
            {
                playerInventory.items[i] = itemToUnequip;
                playerEquipment.slots[slotIndex] = new InventoryItem();
                added = true;
                break;
            }
        }

        if (added)
        {
            SaveInventory();
            SaveEquipment();
            RecalculateEquippedBonuses();
            string itemName = GetLocalizedItemName(itemToUnequip, curLang);
            ShowFeedback(curLang == 0 ? $"{itemName} снят и помещен в инвентарь!" : $"{itemName} unequipped into inventory!");
        }
        else
        {
            ShowFeedback(curLang == 0 ? "Инвентарь полон! Некуда положить снятый предмет. Разблокируйте новые ячейки." : "Inventory full! Unlock more slots to unequip.");
        }
    }

    private string GetLocalizedItemName(InventoryItem item, int lang)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return "";
        if (item.slotType == 0) // Potion
        {
            string nameBase = "Зелье";
            if (item.id.Contains("hp"))
            {
                if (lang == 0) nameBase = "Зелье Жизни";
                else if (lang == 8) nameBase = "生命药水";
                else if (lang == 7) nameBase = "체력 물약";
                else nameBase = "Health Elixir";
            }
            else if (item.id.Contains("str"))
            {
                if (lang == 0) nameBase = "Зелье Силы";
                else if (lang == 8) nameBase = "力量药水";
                else if (lang == 7) nameBase = "힘의 물약";
                else nameBase = "Strength Potion";
            }
            else if (item.id.Contains("def"))
            {
                if (lang == 0) nameBase = "Зелье Защиты";
                else if (lang == 8) nameBase = "防御药水";
                else if (lang == 7) nameBase = "방어의 물약";
                else nameBase = "Defense Potion";
            }
            
            string lvlSuffix = lang == 0 ? $" (Ур.{item.level})" : $" (Lvl {item.level})";
            if (lang == 8) lvlSuffix = $" ({item.level} 级)";
            if (lang == 7) lvlSuffix = $" ({item.level} 레벨)";
            return nameBase + lvlSuffix;
        }
        else
        {
            return GetItemName(item.slotType, item.level, lang);
        }
    }

    private void EquipOrUseItem(int inventoryIndex)
    {
        LoadInventory();
        LoadEquipment();
        InventoryItem item = playerInventory.items[inventoryIndex];
        if (item == null || string.IsNullOrEmpty(item.id)) return;

        int curLang = Translator.LanguageID;

        if (item.slotType >= 1 && item.slotType <= 8)
        {
            int slot = item.slotType;
            InventoryItem currentlyEquipped = playerEquipment.slots[slot];
            string itemName = GetLocalizedItemName(item, curLang);

            if (currentlyEquipped != null && !string.IsNullOrEmpty(currentlyEquipped.id))
            {
                string oldName = GetLocalizedItemName(currentlyEquipped, curLang);
                playerInventory.items[inventoryIndex] = currentlyEquipped;
                playerEquipment.slots[slot] = item;
                ShowFeedback(curLang == 0 ? $"Вы надели {itemName}, а {oldName} вернулся в инвентарь." : $"Equipped {itemName}, returned {oldName} to inventory.");
            }
            else
            {
                playerEquipment.slots[slot] = item;
                playerInventory.items[inventoryIndex] = new InventoryItem();
                ShowFeedback(curLang == 0 ? $"Вы успешно экипировали {itemName}!" : $"Successfully equipped {itemName}!");
            }

            SaveInventory();
            SaveEquipment();
            RecalculateEquippedBonuses();
        }
        else if (item.slotType == 0)
        {
            SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
            string itemName = GetLocalizedItemName(item, curLang);
            if (item.id.Contains("hp"))
            {
                float maxHp = (data.stamina + eqBonusSTA) * 10f;
                data.currentHealth = Mathf.Min(maxHp, data.currentHealth + (item.level * 30f));
                ShowFeedback(curLang == 0 ? $"Вы выпили {itemName}. ОЗ восстановлено!" : $"Consumed {itemName}. Health restored!");
            }
            else if (item.id.Contains("str"))
            {
                data.strength += item.level;
                ShowFeedback(curLang == 0 ? $"Вы выпили {itemName}. Сила увеличена на +{item.level}!" : $"Consumed {itemName}. Strength increased by +{item.level}!");
            }
            else if (item.id.Contains("def"))
            {
                data.stamina += item.level;
                ShowFeedback(curLang == 0 ? $"Вы выпили {itemName}. Выносливость увеличена на +{item.level}!" : $"Consumed {itemName}. Stamina increased by +{item.level}!");
            }

            item.count--;
            if (item.count <= 0)
            {
                playerInventory.items[inventoryIndex] = new InventoryItem();
            }

            SaveInventory();
            SaveGameSystem.Save(0);
        }
    }

    private void RecalculateEquippedBonuses()
    {
        LoadEquipment();
        eqBonusSTR = 0;
        eqBonusAGI = 0;
        eqBonusINT = 0;
        eqBonusSTA = 0;

        for (int i = 1; i <= 8; i++)
        {
            InventoryItem item = playerEquipment.slots[i];
            if (item != null && !string.IsNullOrEmpty(item.id))
            {
                int tier = item.level;
                switch (i)
                {
                    case 1: eqBonusSTA += tier * 2; break; // Head
                    case 2: eqBonusINT += tier * 2; break; // Neck
                    case 3: eqBonusSTR += tier; eqBonusSTA += tier; break; // Shoulders
                    case 4: eqBonusSTA += tier * 3; break; // Chest
                    case 5: eqBonusINT += tier; eqBonusAGI += tier; break; // Ring
                    case 6: eqBonusSTA += tier; eqBonusAGI += tier; break; // Belt
                    case 7: eqBonusAGI += tier * 2; break; // Boots
                    case 8: eqBonusSTR += tier * 3; break; // Weapon
                }
            }
        }
    }

    private string GetEmojiForSlot(int slot)
    {
        switch (slot)
        {
            case 1: return "👑"; // Head
            case 2: return "📿"; // Neck
            case 3: return "🦾"; // Shoulders
            case 4: return "👕"; // Chest
            case 5: return "💍"; // Ring
            case 6: return "🎗️"; // Belt
            case 7: return "🥾"; // Boots
            case 8: return "⚔️"; // Weapon
            default: return "🧪"; // Potion / Consumable
        }
    }

    private string GetItemName(int slotType, int tier, int lang)
    {
        string[][] slotPrefixesRU = new string[][] {
            new string[] { "Бронза", "Сталь", "Мифрил", "Кристалл", "Космос", "Легенда" }, // 0
            new string[] { "Бронзовый Шлем", "Стальной Шлем", "Мифриловый Шлем", "Кристальный Шлем", "Звездный Венец", "Шлем Зенита v18" }, // 1
            new string[] { "Бронзовый Амулет", "Стальной Амулет", "Мифриловый Амулет", "Кристальный Амулет", "Звездный Амулет", "Амулет Зенита v18" }, // 2
            new string[] { "Бронзовые Наплечники", "Стальные Наплечники", "Мифриловые Наплечники", "Кристальные Наплечники", "Звездные Наплечники", "Наплечники Зенита v18" }, // 3
            new string[] { "Бронзовый Доспех", "Стальной Доспех", "Мифриловый Доспех", "Кристальный Доспех", "Звездный Доспех", "Доспех Зенита v18" }, // 4
            new string[] { "Бронзовое Кольцо", "Стальное Кольцо", "Мифриловое Кольцо", "Кристальное Кольцо", "Звездное Кольцо", "Кольцо Зенита v18" }, // 5
            new string[] { "Бронзовый Пояс", "Стальной Пояс", "Мифриловый Пояс", "Кристальный Пояс", "Звездный Пояс", "Пояс Зенита v18" }, // 6
            new string[] { "Бронзовые Сапоги", "Стальные Сапоги", "Мифриловые Сапоги", "Кристальные Сапоги", "Звездные Сапоги", "Сапоги Зенита v18" }, // 7
            new string[] { "Бронзовый Меч", "Стальной Клинок", "Мифриловый Меч", "Кристальный Клинок", "Звездный Клинок", "Меч Зенита v18" } // 8
        };

        string[][] slotPrefixesEN = new string[][] {
            new string[] { "Bronze", "Steel", "Mithril", "Crystalline", "Star-Forged", "Legendary Zenith" },
            new string[] { "Bronze Helmet", "Steel Helm", "Mithril Visor", "Crystal Crown", "Astral Crest", "Zenith Crown" },
            new string[] { "Bronze Pendant", "Steel Choker", "Mithril Talisman", "Crystal Eye", "Stellar Pendant", "Zenith Relic" },
            new string[] { "Bronze Spaulders", "Steel Guards", "Mithril Pauldrons", "Crystal Pauldrons", "Star Shoulders", "Zenith Epaulets" },
            new string[] { "Bronze Plate", "Steel Chestplate", "Mithril Greatplate", "Crystal Platemail", "Star Sentinel Armor", "Zenith Plate" },
            new string[] { "Bronze Ring", "Steel Band", "Mithril Signet", "Crystal Ring", "Cosmic Loop", "Zenith Ring" },
            new string[] { "Bronze Belt", "Steel Buckle", "Mithril Belt", "Crystal Girdle", "Nova Sash", "Zenith Girdle" },
            new string[] { "Bronze Sabatons", "Steel Boots", "Mithril Greaves", "Crystal Treads", "Star Sabatons", "Zenith Greaves" },
            new string[] { "Bronze Sword", "Steel Blade", "Mithril Claymore", "Crystal Scepter", "Astral Edge", "Zenith Slayer" }
        };

        int tIdx = Mathf.Clamp(tier - 1, 0, 5);
        int sIdx = Mathf.Clamp(slotType, 0, 8);

        if (lang == 0) return slotPrefixesRU[sIdx][tIdx];
        return slotPrefixesEN[sIdx][tIdx];
    }

    public bool IsHeroProfileOpen
    {
        get { return showStatsPanel; }
    }

    // ==========================================
    // HELPER METHODS FOR GARRISON & ESPIONAGE (v18.11.15)
    // ==========================================
    public int GetHeroCapacity(int lvl)
    {
        if (!useAutonomousGarrisonSettings)
        {
            if (lvl <= 2) return manualLevel1_2_Cap;
            if (lvl <= 4) return manualLevel3_4_Cap;
            if (lvl == 5) return manualLevel5_Cap;
            return manualLevel6_Cap;
        }
        
        // Autonomous logic requested by the user:
        if (lvl <= 2) return 4;
        if (lvl <= 4) return 5;
        if (lvl == 5) return 6;
        return 7;
    }

    public int GetSpyCost(int enemyLvl)
    {
        int baseCost = useAutonomousEspionageSettings ? 150 : manualBaseSpyCost;
        return enemyLvl * baseCost;
    }

    public int GetMinSpyRequiredLevel()
    {
        return useAutonomousEspionageSettings ? 3 : manualMinLevelForEspionage;
    }

    public int GetHeroesCountInCastle(int zoneIndex)
    {
        int count = 0;
        // Main hero is present in player's active landed zone (or if activeDetailsIndex matches)
        int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        int actualPlayerRegion = GetActualRegionIndexFromLanding(landedZone);
        if (actualPlayerRegion == zoneIndex)
        {
            count += 1; // Protagonist
        }
        count += GetHeroCount("ArcherHero", zoneIndex);
        count += GetHeroCount("WarriorHero", zoneIndex);
        count += GetHeroCount("MageHero", zoneIndex);
        return count;
    }

    public int GetPlayerMaxCastleLevel()
    {
        int maxLvl = 1;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Player" && castles[i].level > maxLvl)
            {
                maxLvl = castles[i].level;
            }
        }
        return maxLvl;
    }

    public string GetCastleRace(int zoneIndex, int lang)
    {
        if (zoneIndex >= 0 && zoneIndex < castles.Count)
        {
            if (castles[zoneIndex].owner == "Player")
            {
                return lang == 0 ? "Орден Света (Альянс Игрока)" : "Zenith Citadel (Player Alliance)";
            }
        }

        switch (zoneIndex)
        {
            case 0:
            case 4:
            case 5:
            case 9:
                return lang == 0 ? "Лесные Эльфы Сильвании" : "Sylvan Wood Elves";
            case 1:
            case 7:
                return lang == 0 ? "Дикие Бандиты и Авантюристы" : "Wasteland Desperados";
            case 2:
            case 10:
                return lang == 0 ? "Свободные Нейтральные Торговцы" : "Free Neutral Merchants";
            case 3:
                return lang == 0 ? "Небожители Сакрального Зенита" : "Sacred Zenith Celestials";
            case 6:
                return lang == 0 ? "Владыки Ледяного Пика" : "Frostbound High Overlords";
            case 8:
                return lang == 0 ? "Дикари Древнейших Руин" : "Ancient Ruins Barbarians";
            case 11:
                return lang == 0 ? "Племена Орков Кровавых Пустошей" : "Wasteland Blood Orcs";
            default:
                return lang == 0 ? "Теневой Синдикат Пустоты" : "Void Shadow Syndicate";
        }
    }

    // ZONE SPECIFIC HERO & UNIT PERSISTENCE (MIGRATION INCLUDED)
    public int GetHeroCount(string key, int zoneIndex)
    {
        string zoneKey = "Player_HiredCount_" + key + "_Zone_" + zoneIndex;
        if (!PlayerPrefs.HasKey(zoneKey))
        {
            int oldVal = PlayerPrefs.GetInt("Player_HiredCount_" + key, 0);
            int mainZone = GetActualRegionIndexFromLanding(PlayerPrefs.GetInt("LandedZoneIndex", -1));
            if (zoneIndex == mainZone)
            {
                PlayerPrefs.SetInt(zoneKey, oldVal);
            }
            else
            {
                PlayerPrefs.SetInt(zoneKey, 0);
            }
            // Clear old global to complete migration safely
            PlayerPrefs.DeleteKey("Player_HiredCount_" + key);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetInt(zoneKey, 0);
    }

    public void SetHeroCount(string key, int zoneIndex, int val)
    {
        PlayerPrefs.SetInt("Player_HiredCount_" + key + "_Zone_" + zoneIndex, val);
        PlayerPrefs.Save();
    }

    public int GetUnitCount(string id, int zoneIndex)
    {
        string zoneKey = "Player_Unit_" + id + "_Zone_" + zoneIndex;
        if (!PlayerPrefs.HasKey(zoneKey))
        {
            int oldVal = PlayerPrefs.GetInt("Player_Unit_" + id, 0);
            int mainZone = GetActualRegionIndexFromLanding(PlayerPrefs.GetInt("LandedZoneIndex", -1));
            if (zoneIndex == mainZone)
            {
                PlayerPrefs.SetInt(zoneKey, oldVal);
            }
            else
            {
                PlayerPrefs.SetInt(zoneKey, 0);
            }
            // Clear old global to complete migration safely
            PlayerPrefs.DeleteKey("Player_Unit_" + id);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetInt(zoneKey, 0);
    }

    public void SetUnitCount(string id, int zoneIndex, int val)
    {
        PlayerPrefs.SetInt("Player_Unit_" + id + "_Zone_" + zoneIndex, val);
        PlayerPrefs.Save();
    }

    private int hoveredCastleIdx = -1;
    private int prevSelectedCalibCastleIdx = -1;
    private float calibColorR = 1.0f;
    private float calibColorG = 1.0f;
    private float calibColorB = 1.0f;

    public int GetPlayerArmyPower(int fromZoneIndex)
    {
        int power = 0;
        string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
        int[] troop_powers = { 10, 15, 25, 40, 60, 80, 30, 50, 75, 120, 150, 300, 200, 220 };
        
        for (int i = 0; i < troop_ids.Length; i++)
        {
            int count = GetUnitCount(troop_ids[i], fromZoneIndex);
            power += count * troop_powers[i];
        }

        string[] companion_ids = { "ArcherHero", "WarriorHero", "MageHero" };
        foreach (var compId in companion_ids)
        {
            int countHero = GetHeroCount(compId, fromZoneIndex);
            if (countHero > 0)
            {
                int lvl = PlayerPrefs.GetInt("Companion_Lvl_" + compId, 1);
                power += countHero * lvl * 50;
            }
        }
        
        return power + 5;
    }

    public void PerformBattleShieldSiege(int targetZoneIdx)
    {
        int curLang = Translator.LanguageID;
        CastleInstance castle = castles[targetZoneIdx];
        
        int launchZone = -1;
        int maxPower = 0;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Player")
            {
                int pwr = GetPlayerArmyPower(i);
                if (pwr > maxPower)
                {
                    maxPower = pwr;
                    launchZone = i;
                }
            }
        }

        if (launchZone == -1 || maxPower <= 5)
        {
            string fb = curLang == 0 ? 
                "У вас нет готовой армии! Наберите когорты воинов в Казарме вашего замка!" : 
                "No standing army found! Recruit legion cohorts in your castle barracks first!";
            ShowFeedback(fb);
            return;
        }

        int bossPower = castle.aiTroopsPower;
        
        if (maxPower >= bossPower)
        {
            castle.owner = "Player";
            PlayerPrefs.SetString("Castle_Owner_" + targetZoneIdx, "Player");
            
            string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
            float casualtyRate = UnityEngine.Random.Range(0.15f, 0.35f);
            for (int i = 0; i < troop_ids.Length; i++)
            {
                int currentCount = GetUnitCount(troop_ids[i], launchZone);
                if (currentCount > 0)
                {
                    int loss = Mathf.Max(1, Mathf.RoundToInt(currentCount * casualtyRate));
                    SetUnitCount(troop_ids[i], launchZone, Mathf.Max(0, currentCount - loss));
                }
            }

            int lootGold = castle.level * 300 + UnityEngine.Random.Range(100, 400);
            SaveGameSystem.CurrentData.gold += lootGold;
            
            PlayerPrefs.Save();
            
            if (LandingPositionManager.Instance != null)
            {
                LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
            }
            
            SpawnAllCastles();

            string resMsg = curLang == 0 ?
                $"👑 ПОБЕДА! Мы захватили {castle.nameRU}! Добыча: +{lootGold} 💰. Враг бежал, регион окрасился в цвета Ордена Света!" :
                $"👑 VICTORY! Conquered {castle.nameEN}! Loot: +{lootGold} 💰. Underneath grounds claim the banner of Light Alliance!";
            ShowFeedback(resMsg);
        }
        else
        {
            string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
            float casualtyRate = UnityEngine.Random.Range(0.40f, 0.70f);
            for (int i = 0; i < troop_ids.Length; i++)
            {
                int currentCount = GetUnitCount(troop_ids[i], launchZone);
                if (currentCount > 0)
                {
                    int loss = Mathf.Max(1, Mathf.RoundToInt(currentCount * casualtyRate));
                    SetUnitCount(troop_ids[i], launchZone, Mathf.Max(0, currentCount - loss));
                }
            }
            
            PlayerPrefs.Save();

            string resMsg = curLang == 0 ?
                $"❌ ПОРАЖЕНИЕ! Наши силы были разбиты у крепостных стен! Потеряно большинство когорт осады." :
                $"❌ DEFEAT! Defending sentinel forces repelled our siege. Heavy cohort casualties suffered.";
            ShowFeedback(resMsg);
        }
    }

    private void UpdateHoveredCastle()
    {
        hoveredCastleIdx = -1;
        if (!isContinentGameplayActive || isTownViewActive || showNewDayOverlay || isDetailsOpen) return;

        if (Camera.main != null)
        {
            Vector2 mousePos = GetMousePosition();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 150f))
            {
                InteractiveCastle ic = hit.collider.GetComponentInParent<InteractiveCastle>() 
                                     ?? hit.collider.GetComponent<InteractiveCastle>();
                if (ic != null)
                {
                    hoveredCastleIdx = ic.zoneIndex;
                }
            }
        }
    }

    private void UpdateColliderVisualizer(int i, bool visible)
    {
        CastleInstance castle = (i >= 0 && i < castles.Count) ? castles[i] : null;
        if (castle == null || castle.visualRoot == null) return;

        Transform helperTrans = castle.visualRoot.transform.Find("Collider_Visualizer");
        if (visible)
        {
            BoxCollider col = castle.visualRoot.GetComponent<BoxCollider>();
            if (col == null) return;

            GameObject helper;
            if (helperTrans == null)
            {
                helper = GameObject.CreatePrimitive(PrimitiveType.Cube);
                helper.name = "Collider_Visualizer";
                helper.transform.SetParent(castle.visualRoot.transform, false);
                Destroy(helper.GetComponent<BoxCollider>());
                
                Renderer r = helper.GetComponent<Renderer>();
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(urpShader);
                
                mat.color = new Color(0.1f, 1.0f, 0.4f, 0.35f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                
                r.material = mat;
            }
            else
            {
                helper = helperTrans.gameObject;
            }

            helper.SetActive(true);
            helper.transform.localPosition = col.center;
            helper.transform.localScale = col.size;
        }
        else
        {
            if (helperTrans != null)
            {
                helperTrans.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateCollisionHelpers()
    {
        for (int i = 0; i < castles.Count; i++)
        {
            bool isVisible = (i == hoveredCastleIdx) || (showCastleCalibrationPanel && i == selectedCalibCastleIdx);
            UpdateColliderVisualizer(i, isVisible);
        }
    }

    private void OnValidate()
    {
        if (customCastlePositions == null || customCastlePositions.Length != 12)
        {
            System.Array.Resize(ref customCastlePositions, 12);
        }
        if (customCastlePositions[0] == Vector3.zero) customCastlePositions[0] = new Vector3(-15f, 0f, 10f);
        if (customCastlePositions[1] == Vector3.zero) customCastlePositions[1] = new Vector3(-5f, 0f, 10f);
        if (customCastlePositions[2] == Vector3.zero) customCastlePositions[2] = new Vector3(5f, 0f, 10f);
        if (customCastlePositions[3] == Vector3.zero) customCastlePositions[3] = new Vector3(-5.3f, -0.4f, 4.2f);
        if (customCastlePositions[4] == Vector3.zero) customCastlePositions[4] = new Vector3(-15f, 0f, 0f);
        if (customCastlePositions[5] == Vector3.zero) customCastlePositions[5] = new Vector3(-5f, 0f, 0f);
        if (customCastlePositions[6] == Vector3.zero) customCastlePositions[6] = new Vector3(14.8f, 1.2f, 12.5f);
        if (customCastlePositions[7] == Vector3.zero) customCastlePositions[7] = new Vector3(15f, 0f, 0f);
        if (customCastlePositions[8] == Vector3.zero) customCastlePositions[8] = new Vector3(-12.4f, -0.3f, -10.2f);
        if (customCastlePositions[9] == Vector3.zero) customCastlePositions[9] = new Vector3(-5f, 0f, -10f);
        if (customCastlePositions[10] == Vector3.zero) customCastlePositions[10] = new Vector3(5f, 0f, -10f);
        if (customCastlePositions[11] == Vector3.zero) customCastlePositions[11] = new Vector3(9.9f, 0.8f, -4.5f);
        
        if (castleManualOffsets == null || castleManualOffsets.Length != 12)
        {
            System.Array.Resize(ref castleManualOffsets, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleManualOffsets[i] == Vector3.zero)
            {
                castleManualOffsets[i] = new Vector3(3.2f, 0f, 3.2f);
            }
        }

        if (castleColliderSizes == null || castleColliderSizes.Length != 12)
        {
            System.Array.Resize(ref castleColliderSizes, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleColliderSizes[i] == Vector3.zero)
            {
                castleColliderSizes[i] = new Vector3(2.5f, 3.5f, 2.5f);
            }
        }

        if (castleColliderCenters == null || castleColliderCenters.Length != 12)
        {
            System.Array.Resize(ref castleColliderCenters, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleColliderCenters[i] == Vector3.zero)
            {
                castleColliderCenters[i] = new Vector3(0f, 1.5f, 0f);
            }
        }
    }

    private void Awake()
    {
        if (customCastlePositions == null || customCastlePositions.Length != 12)
        {
            System.Array.Resize(ref customCastlePositions, 12);
        }
        if (customCastlePositions[0] == Vector3.zero) customCastlePositions[0] = new Vector3(-15f, 0f, 10f);
        if (customCastlePositions[1] == Vector3.zero) customCastlePositions[1] = new Vector3(-5f, 0f, 10f);
        if (customCastlePositions[2] == Vector3.zero) customCastlePositions[2] = new Vector3(5f, 0f, 10f);
        if (customCastlePositions[3] == Vector3.zero) customCastlePositions[3] = new Vector3(-5.3f, -0.4f, 4.2f);
        if (customCastlePositions[4] == Vector3.zero) customCastlePositions[4] = new Vector3(-15f, 0f, 0f);
        if (customCastlePositions[5] == Vector3.zero) customCastlePositions[5] = new Vector3(-5f, 0f, 0f);
        if (customCastlePositions[6] == Vector3.zero) customCastlePositions[6] = new Vector3(14.8f, 1.2f, 12.5f);
        if (customCastlePositions[7] == Vector3.zero) customCastlePositions[7] = new Vector3(15f, 0f, 0f);
        if (customCastlePositions[8] == Vector3.zero) customCastlePositions[8] = new Vector3(-12.4f, -0.3f, -10.2f);
        if (customCastlePositions[9] == Vector3.zero) customCastlePositions[9] = new Vector3(-5f, 0f, -10f);
        if (customCastlePositions[10] == Vector3.zero) customCastlePositions[10] = new Vector3(5f, 0f, -10f);
        if (customCastlePositions[11] == Vector3.zero) customCastlePositions[11] = new Vector3(9.9f, 0.8f, -4.5f);
        
        if (castleManualOffsets == null || castleManualOffsets.Length != 12)
        {
            System.Array.Resize(ref castleManualOffsets, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleManualOffsets[i] == Vector3.zero)
            {
                castleManualOffsets[i] = new Vector3(3.2f, 0f, 3.2f);
            }
        }

        if (castleColliderSizes == null || castleColliderSizes.Length != 12)
        {
            System.Array.Resize(ref castleColliderSizes, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleColliderSizes[i] == Vector3.zero)
            {
                castleColliderSizes[i] = new Vector3(2.5f, 3.5f, 2.5f);
            }
        }

        if (castleColliderCenters == null || castleColliderCenters.Length != 12)
        {
            System.Array.Resize(ref castleColliderCenters, 12);
        }
        for (int i = 0; i < 12; i++)
        {
            if (castleColliderCenters[i] == Vector3.zero)
            {
                castleColliderCenters[i] = new Vector3(0f, 1.5f, 0f);
            }
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        InitializeCastleStates();
    }

    /// <summary>
    /// Находит индекс региона (0-11) на тактической 3D-карте, который физически ближе всего к переданной позиции
    /// </summary>
    public static int FindClosestRegionToPosition(Vector3 position)
    {
        GameObject newContinent = GameObject.Find("New_Kontinent");
        if (newContinent == null)
        {
            newContinent = GameObject.Find("Континент");
        }
        if (newContinent == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if ((go.name.Contains("New_Kontinent") || go.name == "Континент") && go.scene.isLoaded)
                {
                    newContinent = go;
                    break;
                }
            }
        }

        if (newContinent == null)
        {
            return 3; // Возврат по умолчанию
        }

        int closestIndex = 3;
        float minDistance = float.MaxValue;

        for (int i = 0; i < 12; i++)
        {
            string regionName = "Region_" + i.ToString("D2");
            Transform regTrans = newContinent.transform.Find(regionName);
            if (regTrans == null)
            {
                foreach (Transform child in newContinent.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == regionName)
                    {
                        regTrans = child;
                        break;
                    }
                }
            }

            if (regTrans != null)
            {
                Vector3 regionPos = regTrans.position;
                Renderer r = regTrans.GetComponent<Renderer>();
                if (r != null)
                {
                    regionPos = r.bounds.center;
                }

                // Измеряем 2D расстояние (без учета высоты Y) для максимальной точности на тактическом поле
                Vector2 pos2D = new Vector2(position.x, position.z);
                Vector2 reg2D = new Vector2(regionPos.x, regionPos.z);
                float dist = Vector2.Distance(pos2D, reg2D);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Преобразует индекс зоны высадки из диалогов (0-3) в реальный индекс региона на континенте (0-11)
    /// </summary>
    public static int GetActualRegionIndexFromLanding(int landedZoneIndex)
    {
        if (landedZoneIndex < 0)
        {
            return -1;
        }

        // 1. Попытка динамически связать через Proximity (близость) к спавн-анкорам из LandingPositionManager
        if (LandingPositionManager.Instance != null && LandingPositionManager.Instance.landingPoints != null)
        {
            int idx = Mathf.Clamp(landedZoneIndex, 0, LandingPositionManager.Instance.landingPoints.Length - 1);
            var pt = LandingPositionManager.Instance.landingPoints[idx];
            if (pt != null && pt.spawnAnchor != null)
            {
                int closest = FindClosestRegionToPosition(pt.spawnAnchor.position);
                Debug.Log($"[CASTLE MGR PROXIMITY] Динамически сопоставили landedZoneIndex={landedZoneIndex} с ближайшим регионом Region_{closest:D2} у точки {pt.spawnAnchor.name}");
                return closest;
            }
        }

        // 2. Если LandingPositionManager еще не готов, пробуем найти пустышки-анкоры в сцене по именам напрямую
        string[] defaultAnchorNames = new string[] { 
            "Oasis_SpawnPoint", 
            "Outpost_SpawnPoint", 
            "Shore_SpawnPoint", 
            "Citadel_SpawnPoint" 
        };
        int safeIdx = Mathf.Clamp(landedZoneIndex, 0, defaultAnchorNames.Length - 1);
        string targetName = defaultAnchorNames[safeIdx];
        GameObject foundObj = GameObject.Find(targetName);
        if (foundObj == null)
        {
            // Также проверим старые или альтернативные имена для совместимости
            string altName = landedZoneIndex == 0 ? "Wastes_SpawnPoint" :
                             landedZoneIndex == 1 ? "Peak_SpawnPoint" :
                             landedZoneIndex == 2 ? "Ruins_SpawnPoint" : "Crags_SpawnPoint";
            foundObj = GameObject.Find(altName);
        }

        if (foundObj != null)
        {
            int closest = FindClosestRegionToPosition(foundObj.transform.position);
            Debug.Log($"[CASTLE MGR PROXIMITY] Динамически сопоставили по имени объекта {foundObj.name} к ближайшему региону Region_{closest:D2}");
            return closest;
        }

        // 3. Скорректированный статический фолбек на случай полного отсутствия объектов спавна в сцене
        switch (landedZoneIndex)
        {
            case 0: return 11;  // Кровавые Пустоши (Region_11)
            case 1: return 6;   // Ледяной Пик (Region_06)
            case 2: return 8;   // Древние Руины (Region_08)
            case 3: return 3;   // Святилище Зенита / Грозовые Кряжи (Region_03)
            default: return 3;
        }
    }

    private void Start()
    {
        // [CRITICAL SAVE SYNC] Синхронизируем и загружаем активный слот сохранений игрока при запуске сцены континента
        int activeSlot = PlayerPrefs.GetInt("Active_Save_Slot", 0);
        SaveGameSystem.Load(activeSlot, false);

        isContinentGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1;
        currentDay = PlayerPrefs.GetInt("Fate_Current_Day", initialDaySetting);
        if (isContinentGameplayActive)
        {
            SpawnAllCastles();
        }
        LoadClassSkillsIcons();

        // Load Inventory and Equipment
        LoadInventory();
        LoadEquipment();
        RecalculateEquippedBonuses();
    }

    public void EnableContinentGameplay()
    {
        isContinentGameplayActive = true;
        PlayerPrefs.SetInt("ContinentGameplayActive", 1);
        PlayerPrefs.Save();
        
        // Spawn the 3D castles visual structures now!
        SpawnAllCastles();
    }

    public void ResetToInitialSettings()
    {
        PlayerPrefs.SetInt("ContinentGameplayActive", 0);
        isContinentGameplayActive = false;
        PlayerPrefs.SetInt("LandedZoneIndex", -1);
        
        PlayerPrefs.SetInt("Fate_Current_Day", initialDaySetting);
        currentDay = initialDaySetting;
        
        // Сбрасываем все PlayerPrefs для всех 12 замков, чтобы гарантировать чистоту новой кампании
        for (int i = 0; i < 12; i++)
        {
            PlayerPrefs.DeleteKey("Castle_Owner_" + i);
            PlayerPrefs.DeleteKey("Castle_Level_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_CommanderLvl_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Troops_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Armor_" + i);
            PlayerPrefs.DeleteKey("Castle_AI_Potions_" + i);
        }

        // Динамический расчет стартового золота на основе сохраненной в Slot 0 / активном слоте сложности
        int activeSlot = PlayerPrefs.GetInt("Active_Save_Slot", 0);
        SaveGameSystem.Load(activeSlot, false);

        int selectedDifficulty = 2; // По умолчанию Нормально
        if (SaveGameSystem.CurrentData != null)
        {
            selectedDifficulty = SaveGameSystem.CurrentData.selectedDifficulty;
        }

        int difficultyStartingGold = 300;
        switch (selectedDifficulty)
        {
            case 0: difficultyStartingGold = 1000; break; // Новичок (+1000)
            case 1: difficultyStartingGold = 500;  break; // Легко (+500)
            case 2: difficultyStartingGold = 300;  break; // Нормально (+300)
            case 3: difficultyStartingGold = 200;  break; // Сложно (+200)
            case 4: difficultyStartingGold = 100;  break; // Кошмар (+100)
            default: difficultyStartingGold = 300; break;
        }

        if (SaveGameSystem.CurrentData != null)
        {
            SaveGameSystem.CurrentData.gold = difficultyStartingGold;
        }
        PlayerPrefs.SetInt("Player_Current_Gold", difficultyStartingGold);
        PlayerPrefs.SetInt("Player_Gold_Reserve", difficultyStartingGold); // Синхронизируем также резерв для HUD
        PlayerPrefs.Save();

        // Remove spawned castles if any
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].visualRoot != null)
            {
                Destroy(castles[i].visualRoot);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log($"[CASTLE MGR] Сброс параметров кампании: День={initialDaySetting}, Золото={difficultyStartingGold}, Сложность={selectedDifficulty}");
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                feedbackMessage = "";
            }
        }

        if (showNewDayOverlay)
        {
            overlayTimer -= Time.deltaTime;
            if (overlayTimer <= 0f)
            {
                showNewDayOverlay = false;
            }
        }

        // Raycast click tracking on 3D castle structures
        HandleCastleClicks();

        UpdateHoveredCastle();
        UpdateCollisionHelpers();

        // High gloss rotations on active landmarks
        RotateCastleGems();

        // Apply autonomous patrol movement to spawned castles in Unity!
        if (makeCastlesDriftAutonomously && useManualCastlePositions && (castles != null) && (customCastlePositions != null))
        {
            driftTimer += Time.deltaTime * 0.4f;
            for (int i = 0; i < castles.Count; i++)
            {
                if (castles[i].visualRoot != null && i < customCastlePositions.Length)
                {
                    Vector3 basePos = customCastlePositions[i];
                    float offsetX = Mathf.Sin(driftTimer + i * 1.8f) * 1.5f;
                    float offsetZ = Mathf.Cos(driftTimer * 0.7f + i * 1.2f) * 1.5f;
                    castles[i].visualRoot.transform.position = basePos + new Vector3(offsetX, 0f, offsetZ);
                }
            }
        }
    }

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null) return mouse.position.ReadValue();
        return Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private bool WasLeftMouseButtonClicked()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null) return mouse.leftButton.wasPressedThisFrame;
        return false;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private void HandleCastleClicks()
    {
        // Avoid clicking 3D structures if continent gameplay is not fully active, town view is active, day overlay is showing, or stats panel/details/popups are open
        if (!isContinentGameplayActive || isTownViewActive || showNewDayOverlay || showStatsPanel || isDetailsOpen || showTroopDetailPopup || showCastleCalibrationPanel) return;

        if (WasLeftMouseButtonClicked())
        {
            if (Camera.main != null)
            {
                Vector2 mousePos = GetMousePosition();
                // Ensure we don't click on GUI Windows
                if (isDetailsOpen)
                {
                    // GUI coordinates are Y-down, but screen coordinates are Y-up.
                    CastleInstance castle = castles[activeDetailsIndex];
                    float panelWidth = 485f;
                    float panelHeight = (castle.owner == "Player") ? 550f : 620f;
                    float px = (Screen.width - panelWidth) / 2f;
                    float py = (Screen.height - panelHeight) / 2f;
                    Rect guiRect = new Rect(px, py, panelWidth, panelHeight);
                    // Match screen pos (Y up) to GUI pos (Y down)
                    Vector2 guiMouse = new Vector2(mousePos.x, Screen.height - mousePos.y);
                    if (guiRect.Contains(guiMouse))
                    {
                        return; // Clicked inside Details panel, ignore raycast
                    }

                    // Clicked outside Details panel: Do NOT close, but consume/absorb the click event completely so we don't raycast or click on anything else!
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 150f))
                {
                    InteractiveCastle ic = hit.collider.GetComponentInParent<InteractiveCastle>() 
                                         ?? hit.collider.GetComponent<InteractiveCastle>();
                    if (ic != null)
                    {
                        activeDetailsIndex = ic.zoneIndex;
                        isDetailsOpen = true;
                        isTownViewActive = false;
                        feedbackMessage = "";
                        Debug.Log($"[CASTLE MGR] Clicked Castle at zone {ic.zoneIndex}");
                    }
                }
            }
        }
    }

    private void RotateCastleGems()
    {
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].visualRoot != null)
            {
                Transform spire = castles[i].visualRoot.transform.Find("SpireDiamond");
                if (spire != null)
                {
                    spire.Rotate(Vector3.up, 42f * Time.deltaTime, Space.Self);
                }

                Transform ring = castles[i].visualRoot.transform.Find("ZenithRing");
                if (ring != null)
                {
                    ring.Rotate(Vector3.up, -28f * Time.deltaTime, Space.Self);
                    ring.Rotate(Vector3.right, 14f * Time.deltaTime, Space.Self);
                }
            }
        }
    }

    public int GetUpgradeCost(int currentLevel)
    {
        switch (currentLevel)
        {
            case 1: return 300;
            case 2: return 500;
            case 3: return 1200;
            case 4: return 2300;
            case 5: return 4000;
            default: return 7000;
        }
    }

    public int GetGoldIncome(int level)
    {
        switch (level)
        {
            case 1: return 5;
            case 2: return 15;
            case 3: return 35;
            case 4: return 75;
            case 5: return 150;
            case 6: return 280;
            default: return 5;
        }
    }

    private static string GetLandingBaseNameRU(int i)
    {
        switch (i)
        {
            case 3: return "Святилище Зенита";
            case 6: return "Ледяной Пик";
            case 8: return "Древние Руины";
            case 11: return "Кровавые Пустоши";
            default: return "Военный Форпост";
        }
    }

    private static string GetLandingBaseNameEN(int i)
    {
        switch (i)
        {
            case 3: return "Zenith Sanctuary";
            case 6: return "Ice-Bound Peak";
            case 8: return "Ancient Ruins";
            case 11: return "Crimson Wastes";
            default: return "Military Outpost";
        }
    }

    private static string GetLandingBaseNameCH(int i)
    {
        switch (i)
        {
            case 3: return "极星圣所";
            case 6: return "冰封山顶";
            case 8: return "古代遗迹";
            case 11: return "深红荒野";
            default: return "军事前哨";
        }
    }

    private static string GetLandingBaseNameKR(int i)
    {
        switch (i)
        {
            case 3: return "제니스 성소";
            case 6: return "얼음 봉우리";
            case 8: return "고대 유적지";
            case 11: return "붉은 황무지";
            default: return "군사 전초기지";
        }
    }

    private void InitializeCastleStates()
    {
        castles.Clear();
        currentDay = PlayerPrefs.GetInt("Fate_Current_Day", 1);

        int playerDialogueIndex = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        int actualPlayerRegion = GetActualRegionIndexFromLanding(playerDialogueIndex);

        // Динамическое распределение имен регионов согласно бизнес-логике:
        // 2, 10 - Нейтралы
        // 1, 7 - Бандиты (Агрессивные)
        // 0, 4, 5, 9 - Лесные жители
        // 3, 6, 8, 11 - Потенциальная высадка игрока
        string[] zonesRU = new string[12];
        string[] zonesEN = new string[12];
        string[] zonesCH = new string[12];
        string[] zonesKR = new string[12];

        for (int i = 0; i < 12; i++)
        {
            if (i == actualPlayerRegion)
            {
                zonesRU[i] = GetLandingBaseNameRU(i) + " (Альянс Игрока)";
                zonesEN[i] = GetLandingBaseNameEN(i) + " (Player Alliance)";
                zonesCH[i] = GetLandingBaseNameCH(i) + " (玩家同盟)";
                zonesKR[i] = GetLandingBaseNameKR(i) + " (플레이어 동맹)";
            }
            else if (i == 3 || i == 6 || i == 8 || i == 11)
            {
                zonesRU[i] = GetLandingBaseNameRU(i) + " (Нейтралы)";
                zonesEN[i] = GetLandingBaseNameEN(i) + " (Neutrals)";
                zonesCH[i] = GetLandingBaseNameCH(i) + " (中立方)";
                zonesKR[i] = GetLandingBaseNameKR(i) + " (중립국)";
            }
            else if (i == 1 || i == 7)
            {
                zonesRU[i] = i == 1 ? "Убежище Разбойников (Бандиты)" : "Притон Грабителей (Бандиты)";
                zonesEN[i] = i == 1 ? "Rogue Cave (Bandits)" : "Robber Hideout (Bandits)";
                zonesCH[i] = i == 1 ? "强盗洞穴 (强盗)" : "劫匪藏身处 (强盗)";
                zonesKR[i] = i == 1 ? "도적의 동굴 (도적단)" : "강도 은신처 (도적단)";
            }
            else if (i == 2 || i == 10)
            {
                zonesRU[i] = i == 2 ? "Нейтральное Межгорье (Нейтралы)" : "Тихое Перепутье (Нейтралы)";
                zonesEN[i] = i == 2 ? "Neutral Gateway (Neutrals)" : "Quiet Crossroads (Neutrals)";
                zonesCH[i] = i == 2 ? "中立关卡 (中立方)" : "寂静十字路口 (中立方)";
                zonesKR[i] = i == 2 ? "중립의 통로 (중립국)" : "조용한 교차로 (중립국)";
            }
            else // 0, 4, 5, 9
            {
                string baseRU = "Сумрачный Лес";
                string baseEN = "Gloomwood Forest";
                string baseCH = "幽暗密林";
                string baseKR = "어둠의 숲";

                if (i == 4) { baseRU = "Лесные Топи"; baseEN = "Forest Swamps"; baseCH = "森林沼泽"; baseKR = "숲의 늪지"; }
                else if (i == 5) { baseRU = "Изумрудный Сад"; baseEN = "Emerald Garden"; baseCH = "翡翠花园"; baseKR = "에메랄드 정원"; }
                else if (i == 9) { baseRU = "Магическая Роща"; baseEN = "Spiritual Grove"; baseCH = "灵能树林"; baseKR = "영적인 숲"; }

                zonesRU[i] = baseRU + " (Лесные Жители)";
                zonesEN[i] = baseEN + " (Forest Dwellers)";
                zonesCH[i] = baseCH + " (森林居民)";
                zonesKR[i] = baseKR + " (숲의 주민)";
            }
        }

        for (int i = 0; i < 12; i++)
        {
            string ownerStyle = (i == actualPlayerRegion) ? "Player" : "Enemy";

            // СВЕРХВАЖНО: Гарантируем, что замок игрока ТОЛЬКО один и находится строго на его точке высадки!
            string savedOwner = PlayerPrefs.GetString("Castle_Owner_" + i, ownerStyle);
            if (i == actualPlayerRegion)
            {
                savedOwner = "Player";
            }
            else if (savedOwner == "Player")
            {
                savedOwner = "Enemy";
            }
            PlayerPrefs.SetString("Castle_Owner_" + i, savedOwner);

            CastleInstance castle = new CastleInstance
            {
                zoneIndex = i,
                nameRU = zonesRU[i],
                nameEN = zonesEN[i],
                nameCH = zonesCH[i],
                nameKR = zonesKR[i],
                level = PlayerPrefs.GetInt("Castle_Level_" + i, 1),
                owner = savedOwner
            };
            
            // Re-load opponent simulated progression
            castle.aiCommanderLevel = PlayerPrefs.GetInt("Castle_AI_CommanderLvl_" + i, UnityEngine.Random.Range(1, 3));
            
            int defaultPower = useManualAiSimulationSettings ? manualAiStartingTroopPower : UnityEngine.Random.Range(8, 20);
            castle.aiTroopsPower = PlayerPrefs.GetInt("Castle_AI_Troops_" + i, defaultPower);

            castle.aiArmorTier = PlayerPrefs.GetInt("Castle_AI_Armor_" + i, 1);
            castle.aiPotionsStock = PlayerPrefs.GetInt("Castle_AI_Potions_" + i, UnityEngine.Random.Range(1, 4));

            castles.Add(castle);
        }
    }

    /// <summary>
    /// Автоматический процедурный спавнер 3D замков на тактической карте (Морфинг уровней 1-6)
    /// </summary>
    public void SpawnAllCastles()
    {
        // Если геймплей на континенте еще не активен и идет стартовый диалог,
        // мы НЕ должны спавнить 3D-замки на сцене!
        if (!isContinentGameplayActive && DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
        {
            Debug.Log("[CASTLE MGR] Пропуск спавна 3D-замков: Геймплей на континенте еще не активен (идет диалог).");
            return;
        }

        // ПОЛНАЯ ПЕРЕИНИЦИАЛИЗАЦИЯ ИМЕН И СОСТОЯНИЙ ЗАМКОВ СОГЛАСНО ВЫБРАННОЙ ТОЧКЕ ВЫСАДКИ
        InitializeCastleStates();

        // Считываем настройки ручного/автоматического скоординированного размещения из PlayerPrefs
        if (PlayerPrefs.HasKey("Castle_Placement_Manual"))
        {
            useManualCastlePositions = PlayerPrefs.GetInt("Castle_Placement_Manual") == 1;
        }
        if (PlayerPrefs.HasKey("Castle_Drift_Autonomous"))
        {
            makeCastlesDriftAutonomously = PlayerPrefs.GetInt("Castle_Drift_Autonomous") == 1;
        }

        if (!preferScriptCoordinates)
        {
            for (int i = 0; i < 12; i++)
            {
                if (PlayerPrefs.HasKey("Castle_PosX_" + i))
                {
                    if (customCastlePositions != null && i < customCastlePositions.Length)
                    {
                        customCastlePositions[i].x = PlayerPrefs.GetFloat("Castle_PosX_" + i);
                        customCastlePositions[i].y = PlayerPrefs.GetFloat("Castle_PosY_" + i);
                        customCastlePositions[i].z = PlayerPrefs.GetFloat("Castle_PosZ_" + i);
                    }
                }
                if (PlayerPrefs.HasKey("Castle_ManualOffset_PosX_" + i))
                {
                    if (castleManualOffsets != null && i < castleManualOffsets.Length)
                    {
                        castleManualOffsets[i].x = PlayerPrefs.GetFloat("Castle_ManualOffset_PosX_" + i);
                        castleManualOffsets[i].y = PlayerPrefs.GetFloat("Castle_ManualOffset_PosY_" + i);
                        castleManualOffsets[i].z = PlayerPrefs.GetFloat("Castle_ManualOffset_PosZ_" + i);
                    }
                }
                if (PlayerPrefs.HasKey("Castle_ColSizeX_" + i))
                {
                    if (castleColliderSizes != null && i < castleColliderSizes.Length)
                    {
                        castleColliderSizes[i].x = PlayerPrefs.GetFloat("Castle_ColSizeX_" + i);
                        castleColliderSizes[i].y = PlayerPrefs.GetFloat("Castle_ColSizeY_" + i);
                        castleColliderSizes[i].z = PlayerPrefs.GetFloat("Castle_ColSizeZ_" + i);
                    }
                }
                if (PlayerPrefs.HasKey("Castle_ColCentX_" + i))
                {
                    if (castleColliderCenters != null && i < castleColliderCenters.Length)
                    {
                        castleColliderCenters[i].x = PlayerPrefs.GetFloat("Castle_ColCentX_" + i);
                        castleColliderCenters[i].y = PlayerPrefs.GetFloat("Castle_ColCentY_" + i);
                        castleColliderCenters[i].z = PlayerPrefs.GetFloat("Castle_ColCentZ_" + i);
                    }
                }
            }
        }

        int playerZone = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        if (DialogueSystem_Manager.Instance != null)
        {
            int selected = DialogueSystem_Manager.Instance.SelectedZoneIndex;
            if (selected > 0)
            {
                playerZone = selected;
            }
        }

        LandingPositionManager lpm = LandingPositionManager.Instance;
        if (lpm == null)
        {
            Debug.LogWarning("[CASTLE MGR] LandingPositionManager не найден. Пропуск 3D спавна.");
            return;
        }

        // Автоматически находим New_Kontinent и вешаем MeshCollider на все дочерние зоны, если их еще нет,
        // чтобы блокировать прохождение кликов (блокируем клик "в землю")!
        GameObject newContinent = GameObject.Find("New_Kontinent") ?? GameObject.Find("/New_Kontinent");
        if (newContinent != null)
        {
            foreach (Transform rTrans in newContinent.GetComponentsInChildren<Transform>(true))
            {
                if (rTrans.name.StartsWith("Region_"))
                {
                    MeshFilter mf = rTrans.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        MeshCollider mc = rTrans.GetComponent<MeshCollider>();
                        if (mc == null)
                        {
                            mc = rTrans.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < castles.Count; i++)
        {
            CastleInstance castle = castles[i];
            
            // Синхронизация роли: сопоставляем диалоговый индекс высадки с реальным индексом региона континента
            int actualPlayerRegion = GetActualRegionIndexFromLanding(playerZone);
            string initialOwner = (i == actualPlayerRegion) ? "Player" : "Enemy";
            castle.owner = PlayerPrefs.GetString("Castle_Owner_" + i, initialOwner);
            PlayerPrefs.SetString("Castle_Owner_" + i, castle.owner);
            PlayerPrefs.Save();

            if (castle.visualRoot != null)
            {
                Destroy(castle.visualRoot);
            }

            Vector3 spawnPos = Vector3.zero;
            bool foundRegionPos = false;
            if (newContinent != null)
            {
                string regionName = "Region_" + i.ToString("D2"); // Region_00, Region_01, etc.
                Transform regTrans = newContinent.transform.Find(regionName);
                if (regTrans != null)
                {
                    Renderer regRend = regTrans.GetComponent<Renderer>();
                    if (regRend != null)
                    {
                        spawnPos = regRend.bounds.center;
                        foundRegionPos = true;
                    }
                    else
                    {
                        spawnPos = regTrans.position;
                        foundRegionPos = true;
                    }
                    // Приподнимаем на 0.2f для красоты размещения
                    spawnPos.y += 0.2f;
                }
            }

            if (!foundRegionPos)
            {
                if (useManualCastlePositions || preferScriptCoordinates)
                {
                    if (customCastlePositions != null && i < customCastlePositions.Length)
                    {
                        spawnPos = customCastlePositions[i];
                    }
                    else
                    {
                        spawnPos = new Vector3((i - 5.5f) * 12f, 0.5f, (i % 2 == 0 ? 5f : -5f));
                    }
                }
                else
                {
                    if (i < 4 && i < lpm.landingPoints.Length && lpm.landingPoints[i].spawnAnchor != null)
                    {
                        Transform anchor = lpm.landingPoints[i].spawnAnchor;
                        Vector3 offset = (castleManualOffsets != null && i < castleManualOffsets.Length) ? castleManualOffsets[i] : new Vector3(3.2f, 0f, 3.2f);
                        spawnPos = anchor.position + offset;
                    }
                    else
                    {
                        spawnPos = new Vector3((i - 5.5f) * 12f, 1.2f, (i % 2 == 0 ? 5f : -5f));
                    }
                }
            }

            // Проецирование на террейн заземленно (если не ручные координаты)
            if (snapCastlesToTerrain && !useManualCastlePositions && !foundRegionPos)
            {
                RaycastHit hit;
                if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
                {
                    spawnPos.y = hit.point.y;
                }
            }

            GameObject root = new GameObject("3D_Castle_" + i);
            root.transform.position = spawnPos;
            root.transform.rotation = Quaternion.identity;

            InteractiveCastle ic = root.AddComponent<InteractiveCastle>();
            ic.zoneIndex = i;

            BoxCollider col = root.AddComponent<BoxCollider>();
            
            // Загружаем настройки родительского триггера (BoxCollider) на основе инспектора или PlayerPrefs
            Vector3 cct = (castleColliderCenters != null && i < castleColliderCenters.Length && castleColliderCenters[i] != Vector3.zero) ? castleColliderCenters[i] : new Vector3(0f, 1.5f, 0f);
            float colCentX = cct.x;
            float colCentY = cct.y;
            float colCentZ = cct.z;
            
            if (!preferScriptCoordinates && PlayerPrefs.HasKey("Castle_ColCentX_" + i))
            {
                colCentX = PlayerPrefs.GetFloat("Castle_ColCentX_" + i, colCentX);
                colCentY = PlayerPrefs.GetFloat("Castle_ColCentY_" + i, colCentY);
                colCentZ = PlayerPrefs.GetFloat("Castle_ColCentZ_" + i, colCentZ);
            }
            col.center = new Vector3(colCentX, colCentY, colCentZ);

            Vector3 csz = (castleColliderSizes != null && i < castleColliderSizes.Length && castleColliderSizes[i] != Vector3.zero) ? castleColliderSizes[i] : new Vector3(2.5f, 3.5f, 2.5f);
            float colSizeX = csz.x;
            float colSizeY = csz.y;
            float colSizeZ = csz.z;
            
            if (!preferScriptCoordinates && PlayerPrefs.HasKey("Castle_ColSizeX_" + i))
            {
                colSizeX = PlayerPrefs.GetFloat("Castle_ColSizeX_" + i, colSizeX);
                colSizeY = PlayerPrefs.GetFloat("Castle_ColSizeY_" + i, colSizeY);
                colSizeZ = PlayerPrefs.GetFloat("Castle_ColSizeZ_" + i, colSizeZ);
            }
            col.size = new Vector3(colSizeX, colSizeY, colSizeZ);

            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
            Material castleMat = new Material(urpShader);
            
            // Расцветка замков на тактической карте согласно их фракциям:
            // owner == "Player" -> Яркий синий неон игрока
            // i == 2 || i == 10 -> Серый (Нейтралы)
            // i == 1 || i == 7 -> Красный (Бандиты)
            // i == 0 || i == 4 || i == 5 || i == 9 -> Зеленый (Лесные жители)
            // Остальные -> Сине-серый темный (Орден Зенита)
            Color factionColor;
            if (castle.owner == "Player")
            {
                factionColor = new Color(0.12f, 0.58f, 0.95f, 1.0f); // Zenith Neon Blue (Игрок)
            }
            else
            {
                if (i == 2 || i == 10)
                {
                    factionColor = new Color(0.6f, 0.62f, 0.65f, 1.0f); // Neutral Slate Grey (Нейтралы)
                }
                else if (i == 1 || i == 7)
                {
                    factionColor = new Color(0.92f, 0.12f, 0.28f, 1.0f); // Aggressive Bandit Crimson (Бандиты)
                }
                else if (i == 0 || i == 4 || i == 5 || i == 9)
                {
                    factionColor = new Color(0.12f, 0.75f, 0.25f, 1.0f); // Forest Nature Green (Лесные жители)
                }
                else
                {
                    factionColor = new Color(0.35f, 0.4f, 0.5f, 1.0f); // Unaligned Zenith Outposts (Заставы Зенита)
                }
            }
            castleMat.color = factionColor;

            if (castleMat.HasProperty("_Glossiness")) castleMat.SetFloat("_Glossiness", 0.7f);
            if (castleMat.HasProperty("_Smoothness")) castleMat.SetFloat("_Smoothness", 0.7f);
            if (castleMat.HasProperty("_Metallic")) castleMat.SetFloat("_Metallic", 0.45f);

            // МОРФИНГ ФОРМ для активных замков Игрока и Компьютера противника!
            if (castle.owner == "Player" || castle.owner == "Enemy")
            {
                if (castle.level == 1)
                {
                    // LEVEL 1: Одиночный форпост (Один квадратный блок с малой башней)
                    GameObject fort = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(fort.GetComponent<BoxCollider>());
                    fort.transform.SetParent(root.transform);
                    fort.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                    fort.transform.localScale = new Vector3(1.1f, 1.6f, 1.1f);
                    fort.GetComponent<Renderer>().material = castleMat;

                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(crown.GetComponent<BoxCollider>());
                    crown.transform.SetParent(root.transform);
                    crown.transform.localPosition = new Vector3(0f, 1.75f, 0f);
                    crown.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    crown.GetComponent<Renderer>().material = castleMat;
                }
                else if (castle.level == 2)
                {
                    // LEVEL 2: Укреплённый донжон (2 боковые стены и центральный шпиль)
                    GameObject keep = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(keep.GetComponent<BoxCollider>());
                    keep.transform.SetParent(root.transform);
                    keep.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                    keep.transform.localScale = new Vector3(1.4f, 2.8f, 1.4f);
                    keep.GetComponent<Renderer>().material = castleMat;

                    float offset = 0.75f;
                    for (float z = -offset; z <= offset; z += offset * 2)
                    {
                        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(w.GetComponent<BoxCollider>());
                        w.transform.SetParent(root.transform);
                        w.transform.localPosition = new Vector3(0f, 0.7f, z);
                        w.transform.localScale = new Vector3(0.5f, 1.4f, 0.5f);
                        w.GetComponent<Renderer>().material = castleMat;
                    }
                }
                else if (castle.level == 3)
                {
                    // LEVEL 3: Мифриловая крепость (Квадратный мощный бастион с угловыми башнями)
                    GameObject baseBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(baseBlock.GetComponent<BoxCollider>());
                    baseBlock.transform.SetParent(root.transform);
                    baseBlock.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                    baseBlock.transform.localScale = new Vector3(2.1f, 2.0f, 2.1f);
                    baseBlock.GetComponent<Renderer>().material = castleMat;

                    float off = 0.95f;
                    for (float x = -off; x <= off; x += off * 2)
                    {
                        for (float z = -off; z <= off; z += off * 2)
                        {
                            GameObject colTower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(colTower.GetComponent<BoxCollider>());
                            colTower.transform.SetParent(root.transform);
                            colTower.transform.localPosition = new Vector3(x, 1.5f, z);
                            colTower.transform.localScale = new Vector3(0.5f, 3.0f, 0.5f);
                            colTower.GetComponent<Renderer>().material = castleMat;
                        }
                    }
                }
                else if (castle.level == 4)
                {
                    // LEVEL 4: Облачный бастион (Двухъярусная цитадель, боковые пристройки и эммисионный энергетический кристалл)
                    GameObject bodyLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(bodyLower.GetComponent<BoxCollider>());
                    bodyLower.transform.SetParent(root.transform);
                    bodyLower.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                    bodyLower.transform.localScale = new Vector3(2.5f, 2.4f, 1.8f);
                    bodyLower.GetComponent<Renderer>().material = castleMat;

                    GameObject coreTower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(coreTower.GetComponent<BoxCollider>());
                    coreTower.transform.SetParent(root.transform);
                    coreTower.transform.localPosition = new Vector3(0f, 3.2f, 0f);
                    coreTower.transform.localScale = new Vector3(1.1f, 2.2f, 1.1f);
                    coreTower.GetComponent<Renderer>().material = castleMat;

                    // Glowing core spire
                    Material glowingMat = new Material(urpShader);
                    glowingMat.color = castle.owner == "Player" ? new Color(0.1f, 0.9f, 1.0f, 1.0f) : new Color(1.0f, 0.2f, 0.1f, 1.0f);
                    if (glowingMat.HasProperty("_EmissionColor")) glowingMat.SetColor("_EmissionColor", glowingMat.color * 3.0f);

                    GameObject spireDiamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spireDiamond.name = "SpireDiamond";
                    Destroy(spireDiamond.GetComponent<BoxCollider>());
                    spireDiamond.transform.SetParent(root.transform);
                    spireDiamond.transform.localPosition = new Vector3(0f, 4.5f, 0f);
                    spireDiamond.transform.localScale = new Vector3(0.4f, 0.7f, 0.4f);
                    spireDiamond.transform.localRotation = Quaternion.Euler(45f, 45f, 45f);
                    spireDiamond.GetComponent<Renderer>().material = glowingMat;
                }
                else if (castle.level == 5)
                {
                    // LEVEL 5: Имперская твердыня (Парящие платформы, огромные донжоны, монументальные парапеты)
                    GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(structure.GetComponent<BoxCollider>());
                    structure.transform.SetParent(root.transform);
                    structure.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                    structure.transform.localScale = new Vector3(2.8f, 3.0f, 2.8f);
                    structure.GetComponent<Renderer>().material = castleMat;

                    GameObject highKeep = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(highKeep.GetComponent<BoxCollider>());
                    highKeep.transform.SetParent(root.transform);
                    highKeep.transform.localPosition = new Vector3(0f, 4.1f, 0f);
                    highKeep.transform.localScale = new Vector3(1.3f, 2.5f, 1.3f);
                    highKeep.GetComponent<Renderer>().material = castleMat;

                    // Flanking barriers
                    float offset = 1.7f;
                    GameObject lWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(lWall.GetComponent<BoxCollider>());
                    lWall.transform.SetParent(root.transform);
                    lWall.transform.localPosition = new Vector3(-offset, 1.0f, 0f);
                    lWall.transform.localScale = new Vector3(0.8f, 2.0f, 0.8f);
                    lWall.GetComponent<Renderer>().material = castleMat;

                    GameObject rWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(rWall.GetComponent<BoxCollider>());
                    rWall.transform.SetParent(root.transform);
                    rWall.transform.localPosition = new Vector3(offset, 1.0f, 0f);
                    rWall.transform.localScale = new Vector3(0.8f, 2.0f, 0.8f);
                    rWall.GetComponent<Renderer>().material = castleMat;

                    Material coreMat = new Material(urpShader);
                    coreMat.color = castle.owner == "Player" ? new Color(1.0f, 0.8f, 0.1f, 1.0f) : new Color(0.9f, 0.1f, 0.5f, 1.0f);
                    if (coreMat.HasProperty("_EmissionColor")) coreMat.SetColor("_EmissionColor", coreMat.color * 4.0f);

                    GameObject spireDiamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spireDiamond.name = "SpireDiamond";
                    Destroy(spireDiamond.GetComponent<BoxCollider>());
                    spireDiamond.transform.SetParent(root.transform);
                    spireDiamond.transform.localPosition = new Vector3(0f, 5.6f, 0f);
                    spireDiamond.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
                    spireDiamond.transform.localRotation = Quaternion.Euler(30f, 45f, 30f);
                    spireDiamond.GetComponent<Renderer>().material = coreMat;
                }
                else
                {
                    // LEVEL 6: Легендарная Цитадель Зенита (Парящие защитные кольца, многоуровневая структура, супер-излучение)
                    GameObject comp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(comp.GetComponent<BoxCollider>());
                    comp.transform.SetParent(root.transform);
                    comp.transform.localPosition = new Vector3(0f, 2.0f, 0f);
                    comp.transform.localScale = new Vector3(3.2f, 4.0f, 3.2f);
                    comp.GetComponent<Renderer>().material = castleMat;

                    // Twin massive peak wings
                    GameObject pk1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(pk1.GetComponent<BoxCollider>());
                    pk1.transform.SetParent(root.transform);
                    pk1.transform.localPosition = new Vector3(-0.9f, 5.2f, 0f);
                    pk1.transform.localScale = new Vector3(0.7f, 3.2f, 0.7f);
                    pk1.GetComponent<Renderer>().material = castleMat;

                    GameObject pk2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(pk2.GetComponent<BoxCollider>());
                    pk2.transform.SetParent(root.transform);
                    pk2.transform.localPosition = new Vector3(0.9f, 5.2f, 0f);
                    pk2.transform.localScale = new Vector3(0.7f, 3.2f, 0.7f);
                    pk2.GetComponent<Renderer>().material = castleMat;

                    // Zenith Protection Aegis Sphere
                    GameObject zenithRing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    zenithRing.name = "ZenithRing";
                    Destroy(zenithRing.GetComponent<BoxCollider>());
                    zenithRing.transform.SetParent(root.transform);
                    zenithRing.transform.localPosition = new Vector3(0f, 7.2f, 0f);
                    zenithRing.transform.localScale = new Vector3(2.4f, 0.2f, 2.4f);

                    Material ringMat = new Material(urpShader);
                    ringMat.color = castle.owner == "Player" ? new Color(0.1f, 1.0f, 1.0f, 1.0f) : new Color(1.0f, 0.1f, 0.4f, 1.0f);
                    if (ringMat.HasProperty("_EmissionColor")) ringMat.SetColor("_EmissionColor", ringMat.color * 4.5f);
                    zenithRing.GetComponent<Renderer>().material = ringMat;

                    // Central high diamond
                    GameObject spireDiamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spireDiamond.name = "SpireDiamond";
                    Destroy(spireDiamond.GetComponent<BoxCollider>());
                    spireDiamond.transform.SetParent(root.transform);
                    spireDiamond.transform.localPosition = new Vector3(0f, 7.2f, 0f);
                    spireDiamond.transform.localScale = new Vector3(0.6f, 1.2f, 0.6f);
                    spireDiamond.GetComponent<Renderer>().material = ringMat;

                    // Flanking side-annex blocks for grandeur size
                    GameObject leftA = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(leftA.GetComponent<BoxCollider>());
                    leftA.transform.SetParent(root.transform);
                    leftA.transform.localPosition = new Vector3(-2.5f, 1.2f, 0f);
                    leftA.transform.localScale = new Vector3(1.5f, 2.4f, 1.5f);
                    leftA.GetComponent<Renderer>().material = castleMat;

                    GameObject rightA = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(rightA.GetComponent<BoxCollider>());
                    rightA.transform.SetParent(root.transform);
                    rightA.transform.localPosition = new Vector3(2.5f, 1.2f, 0f);
                    rightA.transform.localScale = new Vector3(1.5f, 2.4f, 1.5f);
                    rightA.GetComponent<Renderer>().material = castleMat;
                }
            }
            else
            {
                // Для НЕ-игровых нейтральных замков выстраивается упрощённый аскетичный форт матового серого цвета
                Material neutralMat = new Material(urpShader);
                neutralMat.color = new Color(0.44f, 0.46f, 0.50f, 1.0f); // Красивый нейтральный матовый стальной цвет
                if (neutralMat.HasProperty("_Glossiness")) neutralMat.SetFloat("_Glossiness", 0.45f);
                if (neutralMat.HasProperty("_Smoothness")) neutralMat.SetFloat("_Smoothness", 0.45f);
                if (neutralMat.HasProperty("_Metallic")) neutralMat.SetFloat("_Metallic", 0.3f);

                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(tower.GetComponent<CapsuleCollider>());
                tower.transform.SetParent(root.transform);
                tower.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                tower.transform.localScale = new Vector3(1.1f, 2.4f, 1.1f);
                tower.GetComponent<Renderer>().material = neutralMat;

                GameObject topRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(topRim.GetComponent<BoxCollider>());
                topRim.transform.SetParent(root.transform);
                topRim.transform.localPosition = new Vector3(0f, 2.3f, 0f);
                topRim.transform.localScale = new Vector3(1.3f, 0.3f, 1.3f);
                topRim.GetComponent<Renderer>().material = neutralMat;
            }

            castle.visualRoot = root;
        }
    }

    /// <summary>
    /// ПОШАГОВОЕ ЗАВЕРШЕНИЕ ДНЯ (End Turn / Пропустить ход)
    /// </summary>
    public void AdvanceDay()
    {
        currentDay++;
        PlayerPrefs.SetInt("Fate_Current_Day", currentDay);
        PlayerPrefs.Save();

        aiLogs.Clear();

        // 1. Пополнение кошелька за счёт принадлежащих игроку территорий
        int totalIncome = 0;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Player")
            {
                totalIncome += GetGoldIncome(castles[i].level);
            }
        }
        SaveGameSystem.CurrentData.gold += totalIncome;

        // 2. АВТОМАТИЧЕСКАЯ ИУ-СИМУЛЯЦИЯ (Действия Лордов компьютера в зависимости от сложности)
        int diff = SaveGameSystem.CurrentData.selectedDifficulty; // 0: Easy, 1: Med, 2: Hard, 3: Nightmare, 4: Hell
        
        for (int i = 0; i < castles.Count; i++)
        {
            CastleInstance c = castles[i];
            if (c.owner == "Enemy")
            {
                // Начисление золота ИИ
                int aiIncome = GetGoldIncome(c.level);
                float diffMultiplier = useManualAiSimulationSettings ? manualAiIncomeMultiplier : (1.0f + (diff * 0.4f)); // Чем выше сложность, тем больше золота у ИИ
                c.goldAccumulated += aiIncome * diffMultiplier;

                // Логические вероятности прокачек компьютера на ход
                float upgradeChance = useManualAiSimulationSettings ? manualAiUpgradeProbability : (0.15f + (diff * 0.20f)); 
                float recruitChance = useManualAiSimulationSettings ? manualAiRecruitProbability : (0.25f + (diff * 0.15f));
                float equipmentChance = useManualAiSimulationSettings ? manualAiEquipmentProbability : (0.20f + (diff * 0.12f));

                // А. Попытка улучшения замка компьютером
                if (c.level < 6 && c.goldAccumulated >= GetUpgradeCost(c.level) && UnityEngine.Random.value < upgradeChance)
                {
                    c.goldAccumulated -= GetUpgradeCost(c.level);
                    c.level++;
                    PlayerPrefs.SetInt("Castle_Level_" + i, c.level);
                    
                    string upLog = Translator.LanguageID == 0 ? 
                        $"🛡️ [{c.nameRU}] Вражеский Лорд улучшил цитадель до {c.level} уровня!" :
                        $"🛡️ [{c.nameEN}] Enemy Lord upgraded fortress to Level {c.level}!";
                    if (Translator.LanguageID == 8) upLog = $"🛡️ [{c.nameCH}] 敌方领主将主城升级至第 {c.level} 级！";
                    if (Translator.LanguageID == 7) upLog = $"🛡️ [{c.nameKR}] 적 군주가 요새를 {c.level}단계로 강화했습니다!";
                    
                    aiLogs.Add(upLog);
                }

                // Б. Вербовка войск компьютером в гарнизон
                if (UnityEngine.Random.value < recruitChance)
                {
                    int troopGain = UnityEngine.Random.Range(5, 12) + (diff * 4);
                    c.aiTroopsPower += troopGain;
                    PlayerPrefs.SetInt("Castle_AI_Troops_" + i, c.aiTroopsPower);

                    string trLog = Translator.LanguageID == 0 ?
                        $"⚔️ [{c.nameRU}] Силы гарнизона усилены ветеранами (+{troopGain} боевая мощь)." :
                        $"⚔️ [{c.nameEN}] Garrison defenses reinforced (+{troopGain} combat power).";
                    if (Translator.LanguageID == 8) trLog = $"⚔️ [{c.nameCH}] 守军获得精锐部队增援（+{troopGain} 战斗力）。";
                    if (Translator.LanguageID == 7) trLog = $"⚔️ [{c.nameKR}] 수비 대열에 숙련병 훈련 완료 소집 (+{troopGain} 전투 성능).";

                    aiLogs.Add(trLog);
                }

                // В. Закупка снаряжения и раздача ИИ героям (Формула зависимости)
                if (c.aiArmorTier < 6 && UnityEngine.Random.value < equipmentChance)
                {
                    c.aiArmorTier++;
                    PlayerPrefs.SetInt("Castle_AI_Armor_" + i, c.aiArmorTier);

                    string armorNameRU = GetEquipmentTierName(c.aiArmorTier, 0);
                    string armorNameEN = GetEquipmentTierName(c.aiArmorTier, 1);

                    string eqLog = Translator.LanguageID == 0 ?
                        $"🛡️ Вражеский полководец в [{c.nameRU}] экипирован: {armorNameRU}!" :
                        $"🛡️ Enemy commander in [{c.nameEN}] equipped: {armorNameEN}!";
                    if (Translator.LanguageID == 8) eqLog = $"🛡️ 在 [{c.nameCH}] 的敌方军官装备了新的防具：{GetEquipmentTierName(c.aiArmorTier, 8)}！";
                    if (Translator.LanguageID == 7) eqLog = $"🛡️ [{c.nameKR}] 의 적 지휘관이 중갑 보급품 {GetEquipmentTierName(c.aiArmorTier, 7)}을(를) 수령 및 무장했습니다!";

                    aiLogs.Add(eqLog);
                }

                // Г. Покупка зелий и их применение
                if (UnityEngine.Random.value < 0.4f)
                {
                    c.aiPotionsStock += UnityEngine.Random.Range(1, 3);
                    PlayerPrefs.SetInt("Castle_AI_Potions_" + i, c.aiPotionsStock);
                }

                // Д. Прогресс прокачки уровней воинов ИИ
                c.aiCommanderLevel += UnityEngine.Random.value < (0.3f + diff * 0.1f) ? 1 : 0;
                PlayerPrefs.SetInt("Castle_AI_CommanderLvl_" + i, c.aiCommanderLevel);
            }
        }

        PlayerPrefs.Save();

        // Пересоздаем визуал, чтобы отразить процедурный морфинг для крепостей ИИ
        SpawnAllCastles();

        // 3. Вызов наглядного системного отчета
        showNewDayOverlay = true;
        overlayTimer = 5.5f;

        string finishMsg = Translator.LanguageID == 0 ? 
            $"Пассивный доход зачислен! Доход: +{totalIncome} 💰. Начался День {currentDay}." : 
            $"Passive taxes received! Gold flow: +{totalIncome} 💰. Day {currentDay} has arisen!";
        if (Translator.LanguageID == 8) finishMsg = $"已发放财富岁入！税税金所得：+{totalIncome} 💰。第 {currentDay} 天开始。";
        if (Translator.LanguageID == 7) finishMsg = $"자원 획득 완료! 이자 배당금: +{totalIncome} 💰. 제 {currentDay} 일이 되었습니다.";

        ShowFeedback(finishMsg);
    }

    private string GetEquipmentTierName(int tier, int lang)
    {
        string[][] names = new string[][] {
            new string[] { "Бронзовая броня", "Стальной комплект", "Мифриловое вооружение", "Кристальные пластины", "Звездный доспех эгиды", "Легендарный Сет Зенита" },
            new string[] { "Bronze Aegis", "Iron Garrison Gear", "Mithril Greatplates", "Crystalline Platemail", "Star-Forged Sentinel", "Legendary Zenith Crest" },
            new string[] { "青铜卫士半身护铠", "强化精制钢重型甲", "秘银高密晶刃防具", "水晶雕琢流光束装", "铸星不灭光环御盾", "巅峰至尊神格圣甲" },
            new string[] { "청동 에이전트 아머", "강철 가리슨 장비", "미스릴 중장 대갑옷", "크리스탈 유광 플레이트", "정련된 별의 구도자", "전설의 제니스 신성 세트" }
        };

        int idx = Mathf.Clamp(tier - 1, 0, 5);
        int langIdx = 1; // Default EN
        if (lang == 0) langIdx = 0;
        if (lang == 8) langIdx = 2;
        if (lang == 7) langIdx = 3;

        return names[langIdx][idx];
    }

    private void ShowFeedback(string msg)
    {
        feedbackMessage = msg;
        messageTimer = 4.0f;
    }

    // ==========================================
    // HERO LEVELING & STATS SYSTEM (v18.11.15)
    // ==========================================
    public void GainXP(int amount)
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        data.currentXP += amount;
        int xpNeeded = data.playerLevel * 100;
        
        while (data.currentXP >= xpNeeded)
        {
            data.currentXP -= xpNeeded;
            data.playerLevel++;
            data.availableSkillPoints += 5; // Даем 5 очков характеристик за уровень!
            
            string levelMsg = Translator.LanguageID == 0 
                ? $"✨ НОВЫЙ УРОВЕНЬ! Вы достигли Уровня {data.playerLevel}! (+5 очков характеристик)" 
                : $"✨ LEVEL UP! You reached Level {data.playerLevel}! (+5 Stat Points)";
            ShowFeedback(levelMsg);
            
            if (isAutonomousStatsDistribution)
            {
                AutoAllocateAllPoints();
            }
            xpNeeded = data.playerLevel * 100;
        }
        RecalculateStats();
        PlayerPrefs.Save();
    }

    public void RecalculateStats()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        data.maxHealth = data.stamina * 10f;
        if (data.currentHealth > data.maxHealth) data.currentHealth = data.maxHealth;
        if (data.currentHealth <= 0f) data.currentHealth = data.maxHealth; // Воскрешение
    }

    public void AutoAllocateAllPoints()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        string cl = data.characterClass;
        if (string.IsNullOrEmpty(cl)) cl = "Воин";

        while (data.availableSkillPoints > 0)
        {
            if (cl.Contains("Воин") || cl.Contains("Paladin") || cl.Contains("Warrior") || cl.Contains("Паладин"))
            {
                // Воин: +3 Сила, +2 Выносливость, +1 Ловкость
                if (data.availableSkillPoints >= 6)
                {
                    data.strength += 3;
                    data.stamina += 2;
                    data.agility += 1;
                    data.availableSkillPoints -= 6;
                }
                else
                {
                    data.strength += data.availableSkillPoints;
                    data.availableSkillPoints = 0;
                }
            }
            else if (cl.Contains("Лук") || cl.Contains("Archer") || cl.Contains("Стрелок") || cl.Contains("Ranger") || cl.Contains("Охотник"))
            {
                // Лучник: +3 Ловкость, +2 Сила, +1 Выносливость
                if (data.availableSkillPoints >= 6)
                {
                    data.agility += 3;
                    data.strength += 2;
                    data.stamina += 1;
                    data.availableSkillPoints -= 6;
                }
                else
                {
                    data.agility += data.availableSkillPoints;
                    data.availableSkillPoints = 0;
                }
            }
            else
            {
                // Маг: +3 Интеллект, +2 Ловкость, +1 Выносливость
                if (data.availableSkillPoints >= 6)
                {
                    data.intelligence += 3;
                    data.agility += 2;
                    data.stamina += 1;
                    data.availableSkillPoints -= 6;
                }
                else
                {
                    data.agility += data.availableSkillPoints;
                    data.availableSkillPoints = 0;
                }
            }
        }
        RecalculateStats();
        PlayerPrefs.Save();
    }

    public void ResetPlayerStats()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        
        // Определяем базовые исходные атрибуты в зависимости от класса героя
        int startSTR = 10;
        int startAGI = 10;
        int startINT = 10;
        int startSTA = 10;
        
        string cl = (data != null && !string.IsNullOrEmpty(data.characterClass)) ? data.characterClass.ToLower() : "воин";
        if (cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("paladin") || cl.Contains("воин") || cl.Contains("паладин") || cl.Contains("рыцар"))
        {
            startSTR = 15;
            startAGI = 10;
            startINT = 4;
            startSTA = 15;
        }
        else if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("лучник") || cl.Contains("стрел") || cl.Contains("охотн"))
        {
            startSTR = 10;
            startAGI = 14;
            startINT = 6;
            startSTA = 11;
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг") || cl.Contains("колдун") || cl.Contains("волшеб"))
        {
            startSTR = 6;
            startAGI = 10;
            startINT = 10;
            startSTA = 9;
        }

        int spent = (data.strength - startSTR) + (data.agility - startAGI) + (data.intelligence - startINT) + (data.stamina - startSTA);
        if (spent > 0)
        {
            data.availableSkillPoints += spent;
        }
        
        data.strength = startSTR;
        data.agility = startAGI;
        data.intelligence = startINT;
        data.stamina = startSTA;
        
        RecalculateStats();
        PlayerPrefs.Save();
        
        string resetMsg = Translator.LanguageID == 0 
            ? "♻️ Атрибуты сброшены к базовым значениям вашего класса!" 
            : "♻️ Reverted stats back to your class baseline attributes!";
        ShowFeedback(resetMsg);
    }

    private void OnGUI()
    {
        // Не рисуем игровой HUD (кошелек, день, пропустить ход, новое наложение дня и информацию о замке), 
        // пока игрок полностью не завершил 2-й диалог-инструктаж с Аэлиссой!
        if (!isContinentGameplayActive) return;

        InitializeCachedTextures();

        int curLang = Translator.LanguageID;

        // Если активен 2D вид города, рисуем его во весь экран
        if (isTownViewActive)
        {
            DrawTownViewGUI(curLang);
            return;
        }

        // РИСОВАНИЕ НА ТАКТИЧЕСКОЙ КАРТЕ
        // 1. Кошелек золота в верхнем правом углу
        string goldText = curLang == 0 ? "Казна: " : "Treasury: ";
        if (curLang == 8) goldText = "国库金币: ";
        if (curLang == 7) goldText = "소지금: ";

        GUIStyle walletStyle = new GUIStyle(GUI.skin.box);
        walletStyle.fontSize = 16;
        walletStyle.fontStyle = FontStyle.Bold;
        walletStyle.normal.textColor = new Color(1.0f, 0.84f, 0.0f, 1.0f);
        walletStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Box(new Rect(Screen.width - 240f, 20f, 220f, 42f), $"💰 {goldText}{SaveGameSystem.CurrentData.gold}", walletStyle);

        // 2. Индикатор Дня
        string dayLabel = curLang == 0 ? "День: " : "Day: ";
        if (curLang == 8) dayLabel = "当前天数: ";
        if (curLang == 7) dayLabel = "일차: ";

        GUIStyle dStyle = new GUIStyle(GUI.skin.box);
        dStyle.fontSize = 14;
        dStyle.fontStyle = FontStyle.Bold;
        dStyle.normal.textColor = new Color(0.12f, 0.88f, 1.0f, 1.0f);
        dStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Box(new Rect(Screen.width - 240f, 65f, 220f, 38f), $"📅 {dayLabel}{currentDay}", dStyle);

        // 3. Кнопка "Пропустить ход" UI
        string nextDayBtnText = curLang == 0 ? "ПРОПУСТИТЬ ХОД" : "END TURN";
        if (curLang == 8) nextDayBtnText = "结束回合";
        if (curLang == 7) nextDayBtnText = "턴 넘기기";

        GUIStyle nextDayStyle = new GUIStyle(GUI.skin.button);
        nextDayStyle.fontSize = 13;
        nextDayStyle.fontStyle = FontStyle.Bold;
        nextDayStyle.normal.textColor = Color.white;
        nextDayStyle.alignment = TextAnchor.MiddleCenter;

        if (!isDetailsOpen)
        {
            GUI.backgroundColor = new Color(0.1f, 0.65f, 0.95f, 1.0f);
            if (GUI.Button(new Rect(Screen.width - 240f, 107f, 220f, 44f), $"▶ {nextDayBtnText}", nextDayStyle))
            {
                AdvanceDay();
            }
            GUI.backgroundColor = Color.white;
        }

        // 4. Отрисовка ГЕРОЯ И ЕГО ХАРАКТЕРИСТИК (HUD в верхнем левом углу)
        DrawHeroHUD(curLang);

        // Overlay нового дня (ИИ отчеты)
        if (showNewDayOverlay)
        {
            DrawNewDayOverlay(curLang);
        }

        // Всплывающие окна деталей (v18.11.16)
        if (showCastleCalibrationPanel)
        {
            DrawCastleCalibrationPanel(curLang);
        }

        if (showSkillDetailPopup)
        {
            DrawSkillDetailPopup(curLang);
        }

        if (showTroopDetailPopup)
        {
            DrawTroopDetailPopup(curLang);
        }

        // Окно настроек деталей
        if (!isDetailsOpen || activeDetailsIndex < 0 || activeDetailsIndex >= castles.Count) return;

        DrawDetailsWindow(curLang);
    }

    private void DrawHeroHUD(int curLang)
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        
        // 1. СТИЛИ ГЕЙМПЛЕЯ HUD (Зенит Глассморфизм)
        GUIStyle hudBgStyle = new GUIStyle(GUI.skin.box);
        hudBgStyle.normal.background = hudTex;
        
        // Рисуем базовую панель HUD в левом верхнем углу
        Rect hudRect = new Rect(20f, 20f, 330f, 105f);
        GUI.Box(hudRect, "", hudBgStyle);
        
        // Портрет-Кнопка героя на основе класса
        string cl = data.characterClass;
        if (string.IsNullOrEmpty(cl)) cl = "Воин";
        
        string avatarSymbol = "⚔️";
        Color avatarGlowColor = Color.cyan;
        if (cl.Contains("Лук") || cl.Contains("Archer") || cl.Contains("Стрелок"))
        {
            avatarSymbol = "🏹";
            avatarGlowColor = new Color(0.2f, 0.9f, 0.3f);
        }
        else if (cl.Contains("Маг") || cl.Contains("Mage") || cl.Contains("Wizard") || cl.Contains("Sorcerer"))
        {
            avatarSymbol = "🔮";
            avatarGlowColor = new Color(0.7f, 0.3f, 1.0f);
        }

        GUIStyle portraitBtnStyle = new GUIStyle(GUI.skin.button);
        portraitBtnStyle.fontSize = 28;
        portraitBtnStyle.alignment = TextAnchor.MiddleCenter;
        portraitBtnStyle.normal.textColor = avatarGlowColor;
        
        // Кнопка аватара (при нажатии раскрывает меню распределения характеристик)
        if (GUI.Button(new Rect(30f, 30f, 65f, 65f), avatarSymbol, portraitBtnStyle))
        {
            showStatsPanel = !showStatsPanel;
            // Управляем шкалой времени Unity (замораживаем игру при открытии профиля)
            Time.timeScale = showStatsPanel ? 0f : 1f;

            // Воспроизводим звук ховера/клика из диспетчера
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        
        // Имя и класс героя
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 13;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
        
        string nameLabel = Translator.LanguageID == 0 ? "Герой: " : "Hero: ";
        if (Translator.LanguageID == 8) nameLabel = "角色名称: ";
        if (Translator.LanguageID == 7) nameLabel = "영웅: ";
        
        string classTranslated = cl;
        if (Translator.LanguageID == 0)
        {
            if (cl.Contains("Warrior") || cl.Contains("Воин")) classTranslated = "Паладин";
            else if (cl.Contains("Archer") || cl.Contains("Лучник") || cl.Contains("Стрелок")) classTranslated = "Следопыт";
            else if (cl.Contains("Mage") || cl.Contains("Маг")) classTranslated = "Архимаг";
        }
        
        string levelText = Translator.LanguageID == 0 ? "Ур." : "Lvl";
        if (Translator.LanguageID == 8) levelText = "等级";
        if (Translator.LanguageID == 7) levelText = "레벨";

        GUI.Label(new Rect(110f, 26f, 230f, 20f), $"{nameLabel}{data.saveName} ({classTranslated})", labelStyle);
        
        // РИСОВАНИЕ БАРОВ ХАРАКТЕРИСТИК (ЗДОРОВЬЕ / МАНА / ОПЫТ)
        // 1. Здоровье (Красный)
        float maxHp = data.stamina * 10f;
        if (data.currentHealth > maxHp) data.currentHealth = maxHp;
        float hpPct = maxHp > 0f ? (data.currentHealth / maxHp) : 1f;
        
        GUIStyle barBgStyle = new GUIStyle(GUI.skin.box);
        barBgStyle.normal.background = barBgTex;
        
        GUIStyle hpStyle = new GUIStyle(GUI.skin.box);
        hpStyle.normal.background = hpTex;
        
        GUI.Box(new Rect(110f, 48f, 230f, 13f), "", barBgStyle);
        GUI.Box(new Rect(110f, 48f, 230f * hpPct, 13f), "", hpStyle);
        
        GUIStyle textOverBarStyle = new GUIStyle(GUI.skin.label);
        textOverBarStyle.alignment = TextAnchor.MiddleCenter;
        textOverBarStyle.fontSize = 9;
        textOverBarStyle.fontStyle = FontStyle.Bold;
        textOverBarStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(110f, 47f, 230f, 13f), $"HP: {Mathf.CeilToInt(data.currentHealth)} / {maxHp}", textOverBarStyle);
        
        // 2. Мана (Красивый сине-фиолетовый энергетический бар)
        float maxMana = data.intelligence * 10f;
        float manaPct = 1.0f; // Всегда полная мана для заклинаний
        
        GUIStyle mpStyle = new GUIStyle(GUI.skin.box);
        mpStyle.normal.background = mpTex;
        
        GUI.Box(new Rect(110f, 65f, 230f, 13f), "", barBgStyle);
        GUI.Box(new Rect(110f, 65f, 230f * manaPct, 13f), "", mpStyle);
        GUI.Label(new Rect(110f, 64f, 230f, 13f), $"MP: {maxMana} / {maxMana}", textOverBarStyle);
        
        // 3. Опыт (Яркий неоново-бирюзовый цвет)
        int xpNeeded = data.playerLevel * 100;
        float xpPct = xpNeeded > 0 ? Mathf.Clamp01((float)data.currentXP / xpNeeded) : 0f;
        
        GUIStyle xpStyle = new GUIStyle(GUI.skin.box);
        xpStyle.normal.background = xpTex;
        
        GUI.Box(new Rect(110f, 82f, 230f, 13f), "", barBgStyle);
        GUI.Box(new Rect(110f, 82f, 230f * xpPct, 13f), "", xpStyle);
        GUI.Label(new Rect(110f, 81f, 230f, 13f), $"{levelText}: {data.playerLevel} ({data.currentXP} / {xpNeeded} XP)", textOverBarStyle);
        
        // 4. ПАНЕЛЬ ХАРАКТЕРИСТИК (Если showStatsPanel = true)
        if (showStatsPanel)
        {
            DrawStatsAllocationPanel(curLang, hudTex, barBgStyle);
        }
    }

    private int currentInventoryTab = 0;

    private void DrawHighlightBorder(Rect rect, Color color, float thickness = 3f)
    {
        GUI.color = color;
        // Top
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        // Bottom
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        // Left
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        // Right
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawStatsAllocationPanel(int curLang, Texture2D winBgTex, GUIStyle barBgStyle)
    {
        Rect col1Rect = Rect.zero;
        Rect col2Rect = Rect.zero;
        Rect col3Rect = Rect.zero;

        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        
        // Ensure inventory and equipment are loaded and synchronized
        LoadInventory();
        LoadEquipment();
        RecalculateEquippedBonuses();

        // Trigger tutorial if not completed yet
        bool isTutorialCompleted = PlayerPrefs.GetInt("Aelyssa_Character_Tutorial_Done2", 0) == 1;
        if (!isTutorialCompleted && !isAelyssaTutorialActive && tutorialStep == 0)
        {
            isAelyssaTutorialActive = true;
            tutorialStep = 0;
        }

        if (isAelyssaTutorialActive && GamePause_Manager.Instance != null)
        {
            GamePause_Manager.Instance.isPauseBlockedManually = true;
        }

        // Определяем базовые стартовые характеристики в зависимости от веток
        int startSTR = 10, startAGI = 10, startINT = 10, startSTA = 10;
        string cl = (data != null && !string.IsNullOrEmpty(data.characterClass)) ? data.characterClass.ToLower() : "воин";
        if (cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("paladin") || cl.Contains("воин") || cl.Contains("паладин") || cl.Contains("рыцар"))
        {
            startSTR = 15; startAGI = 10; startINT = 4; startSTA = 15;
        }
        else if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("лучник") || cl.Contains("стрел") || cl.Contains("охотн"))
        {
            startSTR = 10; startAGI = 14; startINT = 6; startSTA = 11;
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг") || cl.Contains("колдун") || cl.Contains("волшеб"))
        {
            startSTR = 6; startAGI = 10; startINT = 10; startSTA = 9;
        }

        GUIStyle winStyle = new GUIStyle(GUI.skin.box);
        winStyle.normal.background = winBgTex;
        
        // Fullscreen layout
        float actualWidth = Screen.width - 40f;
        float actualHeight = Screen.height - 40f;
        Rect winRect = new Rect(20f, 20f, actualWidth, actualHeight);
        GUI.Box(winRect, "", winStyle);
        
        GUILayout.BeginArea(winRect);
        GUILayout.Space(12);
        
        // Container header and close "X" button
        GUILayout.BeginHorizontal();
        GUILayout.Space(40); // align title
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.cyan;
        
        string headText = curLang == 0 ? "⚡ ПАНЕЛЬ УПРАВЛЕНИЯ ПЕРСОНАЖЕМ" : "⚡ HERO CONTROL PANEL";
        if (curLang == 8) headText = "⚡ 英雄控制面板与背包";
        if (curLang == 7) headText = "⚡ 영웅 능력치 및 장비 가방";
        GUILayout.Label(headText, headerStyle);
        
        if (isAelyssaTutorialActive) GUI.enabled = false;
        GUI.backgroundColor = new Color(1.0f, 0.22f, 0.22f, 0.95f);
        if (GUILayout.Button("<b>X</b>", GUILayout.Width(30), GUILayout.Height(28)))
        {
            showStatsPanel = false;
            Time.timeScale = 1f; // Resume gameplay
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Three-column fullscreen GUI structure
        GUILayout.BeginHorizontal();
        
        // ----------------------------------------------------
        // COLUMN 1: CHARACTERISTICS (Width: 320f)
        // ----------------------------------------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(320), GUILayout.ExpandHeight(true));
        
        GUIStyle colHeaderStyle = new GUIStyle(GUI.skin.label);
        colHeaderStyle.fontSize = 13;
        colHeaderStyle.fontStyle = FontStyle.Bold;
        colHeaderStyle.normal.textColor = Color.yellow;
        colHeaderStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(curLang == 0 ? "⚡ ХАРАКТЕРИСТИКИ" : "⚡ ATTRIBUTES", colHeaderStyle);
        GUILayout.Space(6);
        
        statsScroll = GUILayout.BeginScrollView(statsScroll, GUILayout.Width(310), GUILayout.Height(actualHeight - 120f));

        // Auto allocation toggle
        bool oldAuto = isAutonomousStatsDistribution;
        string autoLabel = curLang == 0 ? "🤖 Авто-распределение очков" : "🤖 Autonomous Allocation";
        isAutonomousStatsDistribution = GUILayout.Toggle(isAutonomousStatsDistribution, "  " + autoLabel, GUILayout.Height(24));
        if (isAutonomousStatsDistribution != oldAuto)
        {
            PlayerPrefs.SetInt("Player_Stats_Autonomous", isAutonomousStatsDistribution ? 1 : 0);
            PlayerPrefs.Save();
            if (isAutonomousStatsDistribution)
            {
                AutoAllocateAllPoints();
            }
        }
        GUILayout.Space(10);
        
        DrawStatRow(curLang, "🔥", curLang == 0 ? "Сила (STR)" : "Strength (STR)", ref data.strength, ref data.availableSkillPoints, startSTR);
        DrawStatRow(curLang, "⚡", curLang == 0 ? "Ловкость (AGI)" : "Agility (AGI)", ref data.agility, ref data.availableSkillPoints, startAGI);
        DrawStatRow(curLang, "🔮", curLang == 0 ? "Интеллект (INT)" : "Intelligence (INT)", ref data.intelligence, ref data.availableSkillPoints, startINT);
        DrawStatRow(curLang, "💚", curLang == 0 ? "Выносливость (STA)" : "Stamina (STA)", ref data.stamina, ref data.availableSkillPoints, startSTA);
        GUILayout.Space(8);
        
        // Unassigned Points
        GUIStyle pointsStyle = new GUIStyle(GUI.skin.label);
        pointsStyle.alignment = TextAnchor.MiddleCenter;
        pointsStyle.fontSize = 13;
        pointsStyle.fontStyle = FontStyle.Bold;
        pointsStyle.normal.textColor = data.availableSkillPoints > 0 ? new Color(1.0f, 0.64f, 0.0f) : Color.gray;
        
        string pointsLabel = curLang == 0 ? "Свободные очки: " : "Unassigned Points: ";
        GUILayout.Label($"{pointsLabel}{data.availableSkillPoints}", pointsStyle);
        GUILayout.Space(10);
        
        // Derived Combat Parameters
        int totalSTR = data.strength + eqBonusSTR;
        int totalAGI = data.agility + eqBonusAGI;
        int totalINT = data.intelligence + eqBonusINT;
        int totalSTA = data.stamina + eqBonusSTA;

        float combatAtk = totalSTR * 2.5f + totalAGI * 0.5f;
        float combatDef = totalAGI * 1.5f + totalSTR * 0.5f;
        float maxHp = totalSTA * 10f;
        float maxMp = totalINT * 10f;
        
        GUIStyle derivedStyle = new GUIStyle(GUI.skin.box);
        derivedStyle.normal.textColor = new Color(0.85f, 0.9f, 0.98f);
        derivedStyle.fontSize = 11;
        derivedStyle.alignment = TextAnchor.MiddleLeft;
        derivedStyle.padding = new RectOffset(12, 12, 8, 8);
        
        string statsReport = curLang == 0 
            ? $"⚔️ Базовая Атака: {combatAtk} (База {data.strength * 2.5f + data.agility * 0.5f} + Экв. +{eqBonusSTR * 2.5f + eqBonusAGI * 0.5f})\n🛡️ Защита брони: {combatDef} (База {data.agility * 1.5f + data.strength * 0.5f} + Экв. +{eqBonusAGI * 1.5f + eqBonusSTR * 0.5f})\n❤️ Макс. ОЗ (HP): {maxHp} (База {data.stamina * 10f} + Экв. +{eqBonusSTA * 10f})\n🔮 Макс. ОМ (MP): {maxMp} (База {data.intelligence * 10f} + Экв. +{eqBonusINT * 10f})"
            : $"⚔️ Combat Damage: {combatAtk}\n🛡️ Armor Defense: {combatDef}\n❤️ Max Health (HP): {maxHp}\n🔮 Max Mana (MP): {maxMp}";
            
        GUILayout.Label(statsReport, derivedStyle);
        GUILayout.Space(12);
        
        // Control buttons (Reset & Add XP)
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(1.0f, 0.22f, 0.22f);
        string resetBtnLabel = curLang == 0 ? "СБРОС" : "RESET";
        if (GUILayout.Button($"♻️ {resetBtnLabel}", GUILayout.Height(32)))
        {
            ResetPlayerStats();
        }
        
        GUI.backgroundColor = new Color(0.15f, 0.8f, 0.35f);
        string addXpText = curLang == 0 ? "ОПЫТ +50" : "+50 XP";
        if (GUILayout.Button($"✨ {addXpText}", GUILayout.Height(32)))
        {
            GainXP(50);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        col1Rect = GUILayoutUtility.GetLastRect();
        
        GUILayout.Space(15);
        
        // ----------------------------------------------------
        // COLUMN 2: EQUIPMENT & CLASS SKILLS (Width: 340f)
        // ----------------------------------------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(340), GUILayout.ExpandHeight(true));
        
        // TOP HALF: СНАРЯЖЕНИЕ ПЕРСОНАЖА (Equipment Mannequin)
        GUIStyle eqHeaderStyle = new GUIStyle(GUI.skin.label);
        eqHeaderStyle.alignment = TextAnchor.MiddleCenter;
        eqHeaderStyle.fontSize = 13;
        eqHeaderStyle.fontStyle = FontStyle.Bold;
        eqHeaderStyle.normal.textColor = new Color(0.12f, 0.88f, 1.0f);
        GUILayout.Label(curLang == 0 ? "🛡️ СНАРЯЖЕНИЕ ПЕРСОНАЖА" : "🛡️ HERO EQUIPMENT", eqHeaderStyle);
        GUILayout.Space(4);
        
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(180));
        
        GUIStyle slotLabelStyle = new GUIStyle(GUI.skin.label);
        slotLabelStyle.alignment = TextAnchor.MiddleCenter;
        slotLabelStyle.fontSize = 10;
        slotLabelStyle.normal.textColor = Color.gray;

        GUIStyle slotEquippedStyle = new GUIStyle(GUI.skin.button);
        slotEquippedStyle.fontSize = 9;
        slotEquippedStyle.richText = true;
        slotEquippedStyle.alignment = TextAnchor.MiddleCenter;
        slotEquippedStyle.normal.textColor = Color.yellow;

        // LEFT COLUMN: Slot 8 (Weapon)
        GUILayout.BeginVertical(GUILayout.Width(80));
        GUILayout.Label(curLang == 0 ? "⚔️ Слот 8" : "⚔️ Slot 8", slotLabelStyle);
        GUI.backgroundColor = new Color(0.12f, 0.75f, 0.95f, 0.35f);
        
        InventoryItem wpnItem = playerEquipment.slots[8];
        if (wpnItem != null && !string.IsNullOrEmpty(wpnItem.id))
        {
            if (GUILayout.Button($"<b>[ 8 ] ⚔️</b>\n{wpnItem.name}\n<color=cyan>Tier {wpnItem.level}</color>\n<size=8>Нажмите,\nчтобы снять</size>", slotEquippedStyle, GUILayout.Height(140), GUILayout.Width(76)))
            {
                UnequipItem(8);
            }
        }
        else
        {
            string emptyWpn = curLang == 0 ? "[ Оружие ]" : "[ Weapon ]";
            if (GUILayout.Button(emptyWpn, GUILayout.Height(140), GUILayout.Width(76)))
            {
                ShowFeedback(curLang == 0 ? "Экипируйте Оружие через инвентарь справа!" : "Equip Weapon through inventory on the right!");
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();

        // MIDDLE 2 COLUMN grid for standard slots
        GUILayout.BeginVertical();
        
        // First Row: Slots 1, 2
        GUILayout.BeginHorizontal();
        DrawEquippedSlotButton(1, "Шлем", "Helmet", curLang, slotEquippedStyle, 44);
        DrawEquippedSlotButton(2, "Доспех", "Armor", curLang, slotEquippedStyle, 44);
        GUILayout.EndHorizontal();

        // Second Row: Slots 3, 4
        GUILayout.BeginHorizontal();
        DrawEquippedSlotButton(3, "Поножи", "Greaves", curLang, slotEquippedStyle, 44);
        DrawEquippedSlotButton(4, "Перчатки", "Gloves", curLang, slotEquippedStyle, 44);
        GUILayout.EndHorizontal();

        // Third Row: Slots 5, 6
        GUILayout.BeginHorizontal();
        DrawEquippedSlotButton(5, "Сапоги", "Boots", curLang, slotEquippedStyle, 44);
        DrawEquippedSlotButton(6, "Кольцо", "Ring", curLang, slotEquippedStyle, 44);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        // RIGHT COLUMN: Slot 7 (Amulet)
        GUILayout.BeginVertical(GUILayout.Width(80));
        GUILayout.Label(curLang == 0 ? "🔮 Слот 7" : "🔮 Slot 7", slotLabelStyle);
        GUI.backgroundColor = new Color(0.12f, 0.75f, 0.95f, 0.35f);
        
        InventoryItem amuItem = playerEquipment.slots[7];
        if (amuItem != null && !string.IsNullOrEmpty(amuItem.id))
        {
            if (GUILayout.Button($"<b>[ 7 ] 🔮</b>\n{amuItem.name}\n<color=cyan>Tier {amuItem.level}</color>\n<size=8>Нажмите,\nчтобы снять</size>", slotEquippedStyle, GUILayout.Height(140), GUILayout.Width(76)))
            {
                UnequipItem(7);
            }
        }
        else
        {
            string emptyAmu = curLang == 0 ? "[ Амулет ]" : "[ Amulet ]";
            if (GUILayout.Button(emptyAmu, GUILayout.Height(140), GUILayout.Width(76)))
            {
                ShowFeedback(curLang == 0 ? "Экипируйте Амулет через инвентарь справа!" : "Equip Amulet through inventory on the right!");
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        // BOTTOM HALF: КЛАССОВЫЕ НАВЫКИ (Class Skills)
        GUIStyle skillsHeaderStyle = new GUIStyle(GUI.skin.label);
        skillsHeaderStyle.alignment = TextAnchor.MiddleCenter;
        skillsHeaderStyle.fontSize = 13;
        skillsHeaderStyle.fontStyle = FontStyle.Bold;
        skillsHeaderStyle.normal.textColor = new Color(0.9f, 0.3f, 0.9f);
        GUILayout.Label(curLang == 0 ? "🔮 АКТИВНЫЕ И ПАССИВНЫЕ НАВЫКИ" : "🔮 ACTIVE & PASSIVE SKILLS", skillsHeaderStyle);
        GUILayout.Space(4);

        // Load class skill icons if needed
        LoadClassSkillsIcons();

        // 2x2 grid of passive and ultimate skills (LARGER sizes as requested)
        GUILayout.BeginHorizontal();
        
        // Skill 1 (Passive 1)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 1" : "🌟 Passive 1", slotLabelStyle);
        if (GUILayout.Button(activeSkillPassive1 != null ? activeSkillPassive1 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100)))
        {
            ShowSkillDetailPopup(1, curLang);
        }
        GUILayout.EndVertical();

        // Skill 2 (Passive 2)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 2" : "🌟 Passive 2", slotLabelStyle);
        if (GUILayout.Button(activeSkillPassive2 != null ? activeSkillPassive2 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100)))
        {
            ShowSkillDetailPopup(2, curLang);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();

        // Skill 3 (Passive 3)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 3" : "🌟 Passive 3", slotLabelStyle);
        if (GUILayout.Button(activeSkillPassive3 != null ? activeSkillPassive3 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100)))
        {
            ShowSkillDetailPopup(3, curLang);
        }
        GUILayout.EndVertical();

        // Skill 4 (Ultimate Ability)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "⚡ УЛЬТИМЕЙТ" : "⚡ ULTIMATE", slotLabelStyle);
        GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f, 0.9f);
        if (GUILayout.Button(activeSkillUltimate != null ? activeSkillUltimate : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100)))
        {
            ShowSkillDetailPopup(4, curLang);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        col2Rect = GUILayoutUtility.GetLastRect();
        
        GUILayout.Space(15);
        
        // ----------------------------------------------------
        // COLUMN 3: INVENTORY GRID (Width: Remaining space)
        // ----------------------------------------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        GUIStyle invHeaderStyle = new GUIStyle(GUI.skin.label);
        invHeaderStyle.alignment = TextAnchor.MiddleCenter;
        invHeaderStyle.fontSize = 14;
        invHeaderStyle.fontStyle = FontStyle.Bold;
        invHeaderStyle.normal.textColor = new Color(0.9f, 0.45f, 0.1f);
        GUILayout.Label(curLang == 0 ? "🎒 ИНВЕНТАРЬ ПЕРСОНАЖА" : "🎒 CHARACTER INVENTORY", invHeaderStyle);
        
        string invHelpText = curLang == 0 ? 
            "Все купленные зелья и снаряжение попадают сюда. Нажмите на предмет, чтобы надеть или использовать его!" :
            "All purchased elixirs and gear are placed here. Click an item to equip or consume it!";
        GUIStyle invHelpStyle = new GUIStyle(GUI.skin.label);
        invHelpStyle.fontSize = 10;
        invHelpStyle.alignment = TextAnchor.MiddleCenter;
        invHelpStyle.normal.textColor = Color.gray;
        GUILayout.Label(invHelpText, invHelpStyle);
        GUILayout.Space(6);

        // Pagination tabs bar (Tabs 1 to 28 representing 999 slots)
        GUILayout.BeginHorizontal();
        int unlockedCount = GetUnlockedSlotsCount();
        int purchasedCount = GetPurchasedSlotsCount();

        GUIStyle tabBtnStyle = new GUIStyle(GUI.skin.button);
        tabBtnStyle.fontSize = 10;
        tabBtnStyle.fontStyle = FontStyle.Bold;

        // Draw tab buttons
        for (int t = 0; t < 28; t++)
        {
            bool isTabUnlocked = (t * 36) < unlockedCount;
            string tabName = curLang == 0 ? $"Вкл.{t + 1}" : $"Tab {t + 1}";
            if (curLang == 8) tabName = $"分页 {t + 1}";
            if (curLang == 7) tabName = $"탭 {t + 1}";

            if (isTabUnlocked)
            {
                if (t == currentInventoryTab)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                else
                {
                    GUI.backgroundColor = new Color(0.18f, 0.45f, 0.7f, 0.85f);
                }

                if (GUILayout.Button(tabName, tabBtnStyle, GUILayout.Width(52), GUILayout.Height(24)))
                {
                    currentInventoryTab = t;
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
                GUI.enabled = false;
                GUILayout.Button("🔒 " + tabName, tabBtnStyle, GUILayout.Width(52), GUILayout.Height(24));
                GUI.enabled = true;
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        
        // Beautiful 6x6 grid of 36 slots
        GUIStyle slotGridStyle = new GUIStyle(GUI.skin.button);
        slotGridStyle.fontSize = 9;
        slotGridStyle.padding = new RectOffset(2, 2, 2, 2);
        slotGridStyle.richText = true;
        
        invScroll = GUILayout.BeginScrollView(invScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        int gridColumns = 6;
        int gridRows = 6;
        int startSlotIndex = currentInventoryTab * 36;

        for (int row = 0; row < gridRows; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < gridColumns; col++)
            {
                int index = startSlotIndex + row * gridColumns + col;
                if (index >= 999)
                {
                    // Draw placeholder
                    GUI.backgroundColor = new Color(0.1f, 0.12f, 0.18f, 0.3f);
                    GUILayout.Button("-", slotGridStyle, GUILayout.Width(76), GUILayout.Height(76));
                    GUI.backgroundColor = Color.white;
                    continue;
                }

                if (index < unlockedCount)
                {
                    InventoryItem item = playerInventory.items[index];
                    if (item != null && !string.IsNullOrEmpty(item.id))
                    {
                        string label = "";
                        string colorTag = "<color=white>";
                        if (item.level >= 5) colorTag = "<color=orange>"; // Legendary
                        else if (item.level >= 3) colorTag = "<color=magenta>"; // Epic
                        else if (item.level >= 2) colorTag = "<color=cyan>"; // Rare
                        
                        string localizedName = GetLocalizedItemName(item, curLang);
                        if (item.slotType == 0)
                        {
                            label = $"{GetEmojiForSlot(0)}\n{colorTag}{localizedName}</color>\nx{item.count}";
                            GUI.backgroundColor = new Color(0.15f, 0.8f, 0.3f, 0.25f);
                        }
                        else
                        {
                            label = $"{GetEmojiForSlot(item.slotType)}\n{colorTag}{localizedName}</color>\nTier {item.level}";
                            GUI.backgroundColor = new Color(0.85f, 0.65f, 0.15f, 0.25f);
                        }
                        
                        if (GUILayout.Button(label, slotGridStyle, GUILayout.Width(76), GUILayout.Height(76)))
                        {
                            EquipOrUseItem(index);
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.1f, 0.12f, 0.18f, 0.6f);
                        if (GUILayout.Button($"[ #{index + 1} ]\n[ Пусто ]", slotGridStyle, GUILayout.Width(76), GUILayout.Height(76)))
                        {
                            // Click empty
                        }
                    }
                }
                else if (index == unlockedCount)
                {
                    // The first locked slot
                    GUI.backgroundColor = new Color(0.85f, 0.2f, 0.2f, 0.85f);
                    int cost = 30 + (purchasedCount - 10) * 12 + (int)(Mathf.Pow(purchasedCount - 10, 1.8f) * 1.5f);
                    
                    string buyLabel = curLang == 0 
                        ? $"🔒 Слот {index + 1}\nКупить\nЗа {cost} 💰" 
                        : $"🔒 Slot {index + 1}\nUnlock\nFor {cost} 💰";
                    if (curLang == 8) buyLabel = $"🔒 槽位 {index + 1}\n解锁\n需要 {cost} 💰";
                    if (curLang == 7) buyLabel = $"🔒 슬롯 {index + 1}\n잠금 해제\n{cost} 💰";

                    if (GUILayout.Button(buyLabel, slotGridStyle, GUILayout.Width(76), GUILayout.Height(76)))
                    {
                        if (SaveGameSystem.CurrentData.gold < cost)
                        {
                            ShowFeedback(curLang == 0 ? "Недостаточно золота для покупки новой ячейки!" : "Not enough gold to unlock a slot!");
                        }
                        else
                        {
                            SaveGameSystem.CurrentData.gold -= cost;
                            SetPurchasedSlotsCount(purchasedCount + 1);
                            SaveGameSystem.Save(0);
                            ShowFeedback(curLang == 0 ? $"Слот #{index + 1} успешно разблокирован!" : $"Slot #{index + 1} successfully unlocked!");
                        }
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
                    GUI.enabled = false;
                    string locLocked = curLang == 0 ? "🔒 Закрыто" : "🔒 Locked";
                    GUILayout.Button(locLocked, slotGridStyle, GUILayout.Width(76), GUILayout.Height(76));
                    GUI.enabled = true;
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        col3Rect = GUILayoutUtility.GetLastRect();

        GUILayout.EndHorizontal();
        GUILayout.Space(12);
        GUILayout.EndArea();

        // ----------------------------------------------------
        // AELYSSA TUTORIAL OVERLAY & DIALOGUE BOX (v18.11.20)
        // ----------------------------------------------------
        if (isAelyssaTutorialActive)
        {
            // Translucent dark overlay to focus dialog area
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Highlight the corresponding column
            Rect highlightRect = Rect.zero;
            if (tutorialStep == 1) highlightRect = col1Rect;
            else if (tutorialStep == 2 || tutorialStep == 3) highlightRect = col2Rect;
            else if (tutorialStep == 4 || tutorialStep == 5) highlightRect = col3Rect;

            if (highlightRect != Rect.zero)
            {
                Rect absHighlight = new Rect(winRect.x + highlightRect.x, winRect.y + highlightRect.y, highlightRect.width, highlightRect.height);
                float pulse = 0.5f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4.5f)) * 0.5f;
                Color glowColor = new Color(0.15f, 0.85f, 1.0f, pulse);
                DrawHighlightBorder(absHighlight, glowColor, 4f);
            }

            // Draw Aelyssa Dialogue Box
            float boxWidth = actualWidth - 40f;
            float boxHeight = 150f;
            float boxX = 20f;
            float boxY = actualHeight - boxHeight - 10f;
            Rect dialogBoxRect = new Rect(winRect.x + boxX, winRect.y + boxY, boxWidth, boxHeight);

            GUIStyle dialogBoxStyle = new GUIStyle(GUI.skin.box);
            dialogBoxStyle.normal.background = winBgTex; // Glassmorphic texture
            GUI.Box(dialogBoxRect, "", dialogBoxStyle);

            GUILayout.BeginArea(dialogBoxRect);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            
            GUILayout.BeginVertical();
            GUILayout.Space(12);

            // Speaker Name
            GUIStyle speakerStyle = new GUIStyle(GUI.skin.label);
            speakerStyle.fontSize = 15;
            speakerStyle.fontStyle = FontStyle.Bold;
            speakerStyle.normal.textColor = Color.cyan;
            GUILayout.Label($"✨ {GetTutorialSpeaker(tutorialStep, curLang).ToUpper()}", speakerStyle);
            GUILayout.Space(4);

            // Dialog Text
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 12;
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = Color.white;
            GUILayout.Label(GetTutorialText(tutorialStep, curLang), bodyStyle);
            
            GUILayout.FlexibleSpace();

            // Row of buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string nextText = "Next ▶";
            string skipText = "Skip ⏭";
            string finishText = "Finish ✓";
            if (curLang == 0) { nextText = "Далее ▶"; skipText = "Пропустить ⏭"; finishText = "Завершить ✓"; }
            if (curLang == 8) { nextText = "下一步 ▶"; skipText = "跳过 ⏭"; finishText = "完成 ✓"; }
            if (curLang == 7) { nextText = "다음 ▶"; skipText = "건너뛰기 ⏭"; finishText = "완료 ✓"; }

            GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            if (GUILayout.Button($"<b>{skipText}</b>", GUILayout.Width(130), GUILayout.Height(32)))
            {
                isAelyssaTutorialActive = false;
                PlayerPrefs.SetInt("Aelyssa_Character_Tutorial_Done2", 1);
                PlayerPrefs.Save();
                if (GamePause_Manager.Instance != null) GamePause_Manager.Instance.isPauseBlockedManually = false;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }

            GUILayout.Space(12);

            GUI.backgroundColor = new Color(0.12f, 0.72f, 0.42f, 1.0f);
            bool isLastStep = (tutorialStep == 7);
            string mainBtnText = isLastStep ? finishText : nextText;
            if (GUILayout.Button($"<b>{mainBtnText}</b>", GUILayout.Width(130), GUILayout.Height(32)))
            {
                if (isLastStep)
                {
                    isAelyssaTutorialActive = false;
                    PlayerPrefs.SetInt("Aelyssa_Character_Tutorial_Done2", 1);
                    PlayerPrefs.Save();
                    if (GamePause_Manager.Instance != null) GamePause_Manager.Instance.isPauseBlockedManually = false;
                }
                else
                {
                    tutorialStep++;
                }
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    private void DrawEquippedSlotButton(int slotType, string defaultNameRU, string defaultNameEN, int curLang, GUIStyle style, float height = 28)
    {
        InventoryItem item = playerEquipment.slots[slotType];
        
        GUILayout.BeginHorizontal();
        GUILayout.Label($"<b>{GetEmojiForSlot(slotType)} [{slotType}]</b>", GUILayout.Width(40));
        
        GUI.backgroundColor = new Color(0.12f, 0.75f, 0.95f, 0.35f);
        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            string btnText = $"<b>{item.name}</b> (Tier {item.level}) - <color=red>[Снять]</color>";
            if (GUILayout.Button(btnText, style, GUILayout.Height(height), GUILayout.Width(220)))
            {
                UnequipItem(slotType);
            }
        }
        else
        {
            string emptyLabel = curLang == 0 ? $"[ Нет {defaultNameRU} ]" : $"[ Empty {defaultNameEN} ]";
            if (GUILayout.Button(emptyLabel, GUILayout.Height(height), GUILayout.Width(220)))
            {
                ShowFeedback(curLang == 0 ? $"Экипируйте {defaultNameRU} через инвентарь справа!" : $"Equip {defaultNameEN} through inventory on the right!");
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }

    private void LoadClassSkillsIcons()
    {
        string cl = SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null 
            ? SaveGameSystem.CurrentData.characterClass.ToLower() 
            : "warrior";
            
        if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow"))
        {
            activeSkillPassive1 = archerSkillPassive1 != null ? archerSkillPassive1 : Resources.Load<Texture2D>("Skills/Archer/passive1");
            activeSkillPassive2 = archerSkillPassive2 != null ? archerSkillPassive2 : Resources.Load<Texture2D>("Skills/Archer/passive2");
            activeSkillPassive3 = archerSkillPassive3 != null ? archerSkillPassive3 : Resources.Load<Texture2D>("Skills/Archer/passive3");
            activeSkillUltimate = archerSkillUltimate != null ? archerSkillUltimate : Resources.Load<Texture2D>("Skills/Archer/ultimate");
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff"))
        {
            activeSkillPassive1 = mageSkillPassive1 != null ? mageSkillPassive1 : Resources.Load<Texture2D>("Skills/Mage/passive1");
            activeSkillPassive2 = mageSkillPassive2 != null ? mageSkillPassive2 : Resources.Load<Texture2D>("Skills/Mage/passive2");
            activeSkillPassive3 = mageSkillPassive3 != null ? mageSkillPassive3 : Resources.Load<Texture2D>("Skills/Mage/passive3");
            activeSkillUltimate = mageSkillUltimate != null ? mageSkillUltimate : Resources.Load<Texture2D>("Skills/Mage/ultimate");
        }
        else
        {
            activeSkillPassive1 = warriorSkillPassive1 != null ? warriorSkillPassive1 : Resources.Load<Texture2D>("Skills/Warrior/passive1");
            activeSkillPassive2 = warriorSkillPassive2 != null ? warriorSkillPassive2 : Resources.Load<Texture2D>("Skills/Warrior/passive2");
            activeSkillPassive3 = warriorSkillPassive3 != null ? warriorSkillPassive3 : Resources.Load<Texture2D>("Skills/Warrior/passive3");
            activeSkillUltimate = warriorSkillUltimate != null ? warriorSkillUltimate : Resources.Load<Texture2D>("Skills/Warrior/ultimate");
        }
    }

    private void DrawSkillRow(int curLang, Texture2D icon, string emoji, string header, string desc, string skillType)
    {
        GUILayout.BeginHorizontal();
        if (icon != null)
        {
            GUILayout.Label(icon, GUILayout.Width(24), GUILayout.Height(24));
        }
        else
        {
            GUIStyle fallbackEmojiStyle = new GUIStyle(GUI.skin.label);
            fallbackEmojiStyle.alignment = TextAnchor.MiddleCenter;
            fallbackEmojiStyle.fontSize = 14;
            GUILayout.Label(emoji, fallbackEmojiStyle, GUILayout.Width(24), GUILayout.Height(24));
        }
        GUILayout.Space(6);
        string text = $"<b>{header}</b>: {desc}";
        
        GUIStyle skStyle = new GUIStyle(GUI.skin.button);
        skStyle.fontSize = 11;
        skStyle.richText = true;
        skStyle.wordWrap = true;
        skStyle.alignment = TextAnchor.MiddleLeft;
        skStyle.normal.textColor = new Color(0.9f, 0.95f, 1.0f);
        skStyle.hover.textColor = Color.yellow;
        skStyle.padding = new RectOffset(4, 4, 2, 2);

        if (GUILayout.Button(text, skStyle, GUILayout.MinHeight(26)))
        {
            selectedSkillName = header;
            selectedSkillDesc = desc;
            selectedSkillIcon = icon;
            selectedSkillType = skillType;
            showSkillDetailPopup = true;
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    public void OpenSkillDetailPopup(string skillName, string skillDesc, Texture2D skillIcon, string skillType)
    {
        selectedSkillName = skillName;
        selectedSkillDesc = skillDesc;
        selectedSkillIcon = skillIcon;
        selectedSkillType = skillType;
        showSkillDetailPopup = true;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayHoverSound(0);
        }
    }

    public void ShowSkillDetailPopup(int oneBasedIndex, int curLang)
    {
        int index = oneBasedIndex - 1;
        string sName = "";
        string sDesc = "";
        Texture2D icon = null;
        string skillType = "Passive";

        string cl = SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null 
            ? SaveGameSystem.CurrentData.characterClass.ToLower() 
            : "warrior";

        if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("лучник") || cl.Contains("стрел") || cl.Contains("охотн"))
        {
            if (index == 0) { sName = curLang == 0 ? "Крит-Мастер" : "Crit-Master"; sDesc = curLang == 0 ? "Повышает вероятность нанесения критического урона на 15%" : "+15% critical hit probability"; icon = activeSkillPassive1; }
            else if (index == 1) { sName = "LongShot"; sDesc = curLang == 0 ? "Дальний выстрел: Усиливает урон на расстоянии на 10%" : "+10% damage over wide distance range"; icon = activeSkillPassive2; }
            else if (index == 2) { sName = "Evasion"; sDesc = curLang == 0 ? "Поворотливость: Дарует 10% шанс полного уклонения от вражеских атак" : "+10% complete dodge probability"; icon = activeSkillPassive3; }
            else if (index == 3) { sName = curLang == 0 ? "Ливень Смерти" : "Death Rain"; sDesc = curLang == 0 ? "Суперудар (CD 3х): Смертоносный град стрел наносит масштабный урон 1.8х по всем врагам в зоне" : "Ultimate (CD 3t): AoE volley dealing massive x1.8 damage to enemies"; icon = activeSkillUltimate; skillType = "Ultimate"; }
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг") || cl.Contains("колдун") || cl.Contains("волшеб"))
        {
            if (index == 0) { sName = "ManaFlow"; sDesc = curLang == 0 ? "Поток маны: Позволяет восстанавливать 5 очков маны за каждый совершённый ход" : "+5 mana points gain per turn"; icon = activeSkillPassive1; }
            else if (index == 1) { sName = "Elemental"; sDesc = curLang == 0 ? "Сила стихий: Усиливает разрушительный потенциал ваших заклинаний на 15%" : "+15% magic spell power booster"; icon = activeSkillPassive2; }
            else if (index == 2) { sName = "Resist"; sDesc = curLang == 0 ? "Сопротивление: Наделяет мистическим барьером, поглощающим 15% магического урона" : "+15% spell resistance shield"; icon = activeSkillPassive3; }
            else if (index == 3) { sName = "Time Rift"; sDesc = curLang == 0 ? "Суперудар (CD 4х): Изменяет пространственно-временной континуум, полностью замедляя противников на 2 хода" : "Ultimate (CD 4t): Slows down all active enemy actions for 2 turns"; icon = activeSkillUltimate; skillType = "Ultimate"; }
        }
        else // Warrior
        {
            if (index == 0) { sName = "IronSkin"; sDesc = curLang == 0 ? "Прочная кожа: Успешно увеличивает показатель защиты и стойкости на 15%" : "+15% Armor/Defense bonus"; icon = activeSkillPassive1; }
            else if (index == 1) { sName = "Regen"; sDesc = curLang == 0 ? "Регенерация: Обеспечивает исцеление вашего героя на 5 ОЗ каждый игровой ход" : "+5 HP recovery per turn"; icon = activeSkillPassive2; }
            else if (index == 2) { sName = "Threat"; sDesc = curLang == 0 ? "Угроза: Ускоряет накопление боевого духа и провокацию на 10%" : "+10% aggro multiplier bonus"; icon = activeSkillPassive3; }
            else if (index == 3) { sName = "TitanShield"; sDesc = curLang == 0 ? "Суперудар (CD 4х): Активирует нерушимый щит Титанов, снижая входящий физический урон на 70%" : "Ultimate (CD 4t): Blocks 70% of incoming physical damage"; icon = activeSkillUltimate; skillType = "Ultimate"; }
        }

        OpenSkillDetailPopup(sName, sDesc, icon, skillType);
    }

    private void DrawSkillDetailPopup(int curLang)
    {
        GUIStyle winStyle = new GUIStyle(GUI.skin.box);
        winStyle.normal.background = hudTex;

        float winWidth = 340f;
        float winHeight = 280f;
        Rect winRect = new Rect((Screen.width - winWidth) / 2f, (Screen.height - winHeight) / 2f, winWidth, winHeight);
        GUI.Box(winRect, "", winStyle);

        GUILayout.BeginArea(winRect);
        GUILayout.Space(12);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 14;
        titleStyle.normal.textColor = Color.cyan;
        GUILayout.Label($"🧬 {selectedSkillName.ToUpper()}", titleStyle);
        GUILayout.Space(6);

        GUIStyle typeStyle = new GUIStyle(GUI.skin.label);
        typeStyle.alignment = TextAnchor.MiddleCenter;
        typeStyle.fontStyle = FontStyle.Italic;
        typeStyle.fontSize = 11;
        typeStyle.normal.textColor = Color.yellow;
        string typeLabel = selectedSkillType == "Ultimate" ? (curLang == 0 ? "Суперудар (Активный)" : "Ultimate Skill (Active)") : (curLang == 0 ? "Пассивный Навык" : "Passive Skill");
        GUILayout.Label(typeLabel, typeStyle);
        GUILayout.Space(12);

        // Слот под фото (окошко под навык!) - "окошки под них и я туда от них кину фото"
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (selectedSkillIcon != null)
        {
            GUILayout.Label(selectedSkillIcon, GUILayout.Width(64), GUILayout.Height(64));
        }
        else
        {
            GUIStyle fallbackEmojiStyle = new GUIStyle(GUI.skin.label);
            fallbackEmojiStyle.alignment = TextAnchor.MiddleCenter;
            fallbackEmojiStyle.fontSize = 28;
            string emoji = selectedSkillType == "Ultimate" ? "🔱" : "🛡️";
            GUILayout.Label(emoji, fallbackEmojiStyle, GUILayout.Width(64), GUILayout.Height(64));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(12);

        // Описание навыка
        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.alignment = TextAnchor.MiddleCenter;
        descStyle.fontSize = 12;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.9f, 0.95f, 1.0f);
        GUILayout.Label(selectedSkillDesc, descStyle);
        
        GUILayout.Space(16);

        GUI.backgroundColor = new Color(0.1f, 0.8f, 0.4f);
        string closeBtn = curLang == 0 ? "ОТЛИЧНО" : "CLOSE";
        if (GUILayout.Button(closeBtn, GUILayout.Height(36)))
        {
            showSkillDetailPopup = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndArea();
    }

    private Vector2 troopScrollPos = Vector2.zero;

    private Texture2D GetTroopAvatarTexture(string id)
    {
        if (id == "warrior") return avatar_warrior;
        if (id == "archer") return avatar_archer;
        if (id == "mage") return avatar_mage;
        if (id == "paladin") return avatar_paladin;
        if (id == "cavalry") return avatar_cavalry;
        if (id == "cannoneer") return avatar_cannoneer;
        if (id == "centaur") return avatar_centaur;
        if (id == "necromancer") return avatar_necromancer;
        if (id == "griffin") return avatar_griffin;
        if (id == "overlord") return avatar_overlord;
        if (id == "hydra") return avatar_hydra;
        if (id == "dragon") return avatar_dragon;
        if (id == "mountain_bear") return avatar_mountain_bear;
        if (id == "wasteland_serpent") return avatar_wasteland_serpent;
        
        if (id == "ArcherHero") return avatar_hero_archer;
        if (id == "WarriorHero") return avatar_hero_warrior;
        if (id == "MageHero") return avatar_hero_mage;
        
        return null;
    }

    private void DrawTroopDetailPopup(int curLang)
    {
        GUIStyle winStyle = new GUIStyle(GUI.skin.box);
        winStyle.normal.background = hudTex;

        float winWidth = 580f;
        float winHeight = 500f;
        Rect winRect = new Rect((Screen.width - winWidth) / 2f, (Screen.height - winHeight) / 2f, winWidth, winHeight);
        GUI.Box(winRect, "", winStyle);

        GUILayout.BeginArea(winRect);
        GUILayout.Space(12);

        bool isComrade = selectedTroopId.EndsWith("Hero");
        string titleText = "";
        string descText = "";
        int hp = 100, atk = 10, def = 10, spd = 10, tier = 1;
        string avatarPrompt = "";
        
        string[] pNames = new string[0];
        string[] pDescs = new string[0];
        string[] pPrompts = new string[0];
        
        string[] aNames = new string[0];
        string[] aDescs = new string[0];
        string[] aPrompts = new string[0];

        if (isComrade)
        {
            CompanionData cd = GetCompanionData(selectedTroopId);
            titleText = curLang == 0 ? cd.nameRU : cd.nameEN;
            descText = curLang == 0 ? cd.descRU : cd.descEN;
            avatarPrompt = cd.avatarPrompt;
            
            int currentLevel = PlayerPrefs.GetInt("Companion_Lvl_" + selectedTroopId, 1);
            hp = GetCompanionStat(selectedTroopId, "hp", currentLevel);
            atk = GetCompanionStat(selectedTroopId, "atk", currentLevel);
            def = GetCompanionStat(selectedTroopId, "def", currentLevel);
            spd = 12; // Static base speed
            tier = currentLevel; // Represent level
            
            pNames = cd.passiveNames;
            pDescs = cd.passiveDesc;
            pPrompts = cd.passivePrompts;
            
            aNames = new string[] { cd.activeName };
            aDescs = new string[] { cd.activeDesc };
            aPrompts = new string[] { cd.activePrompt };
        }
        else
        {
            TroopData td = GetTroopData(selectedTroopId);
            titleText = curLang == 0 ? td.nameRU : td.nameEN;
            descText = curLang == 0 ? td.descRU : td.descEN;
            avatarPrompt = td.avatarPrompt;
            hp = td.hp;
            atk = td.atk;
            def = td.def;
            spd = td.spd;
            tier = td.tier;
            
            pNames = td.passiveNames;
            pDescs = td.passiveDesc;
            pPrompts = td.passivePrompts;
            
            aNames = td.activeNames;
            aDescs = td.activeDesc;
            aPrompts = td.activePrompts;
        }

        GUIStyle mainTitle = new GUIStyle(GUI.skin.label);
        mainTitle.alignment = TextAnchor.MiddleCenter;
        mainTitle.fontStyle = FontStyle.Bold;
        mainTitle.fontSize = 18;
        mainTitle.normal.textColor = Color.yellow;
        GUILayout.Label($"🛡️ {titleText.ToUpper()} 🛡️", mainTitle);

        GUILayout.Space(8);
        troopScrollPos = GUILayout.BeginScrollView(troopScrollPos, GUILayout.Width(winWidth - 20), GUILayout.Height(winHeight - 90));

        // Profile row (avatar + stats)
        GUILayout.BeginHorizontal();
        
        // Avatar draw
        Texture2D avTex = GetTroopAvatarTexture(selectedTroopId);
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(110), GUILayout.Height(110));
        if (avTex != null)
        {
            GUILayout.Label(avTex, GUILayout.Width(100), GUILayout.Height(100));
        }
        else
        {
            GUIStyle placeholderS = new GUIStyle(GUI.skin.label);
            placeholderS.alignment = TextAnchor.MiddleCenter;
            placeholderS.normal.textColor = Color.gray;
            GUILayout.Label("📷\nNO AVATAR", placeholderS, GUILayout.Width(100), GUILayout.Height(100));
        }
        GUILayout.EndVertical();

        GUILayout.Space(12);

        // Stats section
        GUILayout.BeginVertical();
        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 13;
        statStyle.normal.textColor = Color.white;
        
        string tLabel = isComrade ? (curLang == 0 ? "Уровень:" : "Level:") : (curLang == 0 ? "Ранг:" : "Tier:");
        GUILayout.Label($"★ {tLabel} {tier}", statStyle);
        GUILayout.Label($"❤️ Здоровье (HP): {hp}", statStyle);
        GUILayout.Label($"⚔️ Атака (ATK): {atk}", statStyle);
        GUILayout.Label($"🛡️ Защита (DEF): {def}", statStyle);
        GUILayout.Label($"⚡ Скорость (SPD): {spd}", statStyle);

        if (isComrade)
        {
            int currentLevel = PlayerPrefs.GetInt("Companion_Lvl_" + selectedTroopId, 1);
            int compSTR = GetCompanionStat(selectedTroopId, "strength", currentLevel);
            int compAGI = GetCompanionStat(selectedTroopId, "agility", currentLevel);
            int compINT = GetCompanionStat(selectedTroopId, "intelligence", currentLevel);
            int compSTA = GetCompanionStat(selectedTroopId, "stamina", currentLevel);

            GUILayout.Space(4);
            GUIStyle attrHeaderStyle = new GUIStyle(GUI.skin.label);
            attrHeaderStyle.fontStyle = FontStyle.Bold;
            attrHeaderStyle.fontSize = 11;
            attrHeaderStyle.normal.textColor = Color.yellow;
            GUILayout.Label(curLang == 0 ? "🔥 Базовые параметры:" : "🔥 Base Stats:", attrHeaderStyle);
            GUILayout.Label($"🔥 Сила (STR): {compSTR}", statStyle);
            GUILayout.Label($"⚡ Ловкость (AGI): {compAGI}", statStyle);
            GUILayout.Label($"🔮 Интеллект (INT): {compINT}", statStyle);
            GUILayout.Label($"💚 Выносливость (STA): {compSTA}", statStyle);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        
        // Text/Bio description
        GUIStyle bioStyle = new GUIStyle(GUI.skin.box);
        bioStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
        bioStyle.fontSize = 12;
        GUILayout.Label(descText, bioStyle);

        GUILayout.Space(12);

        // AVATAR prompt section
        GUIStyle promptHeader = new GUIStyle(GUI.skin.label);
        promptHeader.fontStyle = FontStyle.Bold;
        promptHeader.normal.textColor = Color.cyan;
        GUILayout.Label(curLang == 0 ? "🎨 ПРОМПТ ДЛЯ ГЕНЕРАЦИИ АВАТАРКИ:" : "🎨 AVATAR GENERATION AI PROMPT:", promptHeader);
        
        GUILayout.BeginHorizontal();
        avatarPrompt = GUILayout.TextField(avatarPrompt, GUILayout.Height(40));
        if (GUILayout.Button(curLang == 0 ? "📋 Копировать" : "📋 Copy", GUILayout.Width(100), GUILayout.Height(40)))
        {
            GUIUtility.systemCopyBuffer = avatarPrompt;
            ShowFeedback(curLang == 0 ? "Промпт скопирован в буфер обмена!" : "Prompt copied to Clipboard!");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        // PASSIVE SKILLS list
        GUILayout.Label(curLang == 0 ? "🔮 ПАССИВНЫЕ НАВЫКИ И ПРОМПТЫ:" : "🔮 PASSIVE ABILITIES & PROMPTS:", promptHeader);
        for (int i = 0; i < pNames.Length; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"★ {pNames[i]}", GUI.skin.label);
            GUILayout.Label(pDescs[i], GUI.skin.label);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(curLang == 0 ? "Иконка:" : "Icon:", GUILayout.Width(60));
            string skillPrompt = pPrompts[i];
            skillPrompt = GUILayout.TextField(skillPrompt, GUILayout.Height(28));
            if (GUILayout.Button(curLang == 0 ? "Копировать" : "Copy", GUILayout.Width(80), GUILayout.Height(28)))
            {
                GUIUtility.systemCopyBuffer = skillPrompt;
                ShowFeedback(curLang == 0 ? $"Промпт {pNames[i]} скопирован!" : $"Prompt {pNames[i]} copied!");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        GUILayout.Space(10);

        // ACTIVE SKILLS list
        GUILayout.Label(curLang == 0 ? "💥 АКТИВНЫЕ НАВЫКИ И ПРОМПТЫ:" : "💥 ACTIVE ABILITIES & PROMPTS:", promptHeader);
        for (int i = 0; i < aNames.Length; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"⚡ {aNames[i]}", GUI.skin.label);
            GUILayout.Label(aDescs[i], GUI.skin.label);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(curLang == 0 ? "Иконка:" : "Icon:", GUILayout.Width(60));
            string skillPrompt = aPrompts[i];
            skillPrompt = GUILayout.TextField(skillPrompt, GUILayout.Height(28));
            if (GUILayout.Button(curLang == 0 ? "Копировать" : "Copy", GUILayout.Width(80), GUILayout.Height(28)))
            {
                GUIUtility.systemCopyBuffer = skillPrompt;
                ShowFeedback(curLang == 0 ? $"Промпт {aNames[i]} скопирован!" : $"Prompt {aNames[i]} copied!");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        GUILayout.EndScrollView();

        // Footer CLOSE button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(curLang == 0 ? "ЗАКРЫТЬ ДЕТАЛИ" : "CLOSE DETAILS", GUILayout.Height(36)))
        {
            showTroopDetailPopup = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndArea();
    }

    private void DrawCastleCalibrationPanel(int curLang)
    {
        GUIStyle winStyle = new GUIStyle(GUI.skin.box);
        winStyle.normal.background = hudTex;

        float winWidth = 330f;
        float winHeight = 560f;
        Rect winRect = new Rect(Screen.width - 350f, 150f, winWidth, winHeight);
        GUI.Box(winRect, "", winStyle);

        GUILayout.BeginArea(winRect);
        GUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 13;
        titleStyle.normal.textColor = Color.yellow;
        GUILayout.Label("🏰 КАЛИБРОВКА КООРДИНАТ & ТРИГГЕРОВ", titleStyle);
        GUILayout.Space(8);

        // Переключатель на 12 замков в виде сетки 2x6
        int columns = 6;
        for (int r = 0; r < 2; r++)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < columns; c++)
            {
                int castleIdx = r * columns + c;
                if (castleIdx < castles.Count)
                {
                    GUI.backgroundColor = (selectedCalibCastleIdx == castleIdx) ? Color.cyan : Color.white;
                    string label = castleIdx.ToString("D2");
                    if (GUILayout.Button(label, GUILayout.Width(42), GUILayout.Height(24)))
                    {
                        selectedCalibCastleIdx = castleIdx;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        if (selectedCalibCastleIdx >= 0 && selectedCalibCastleIdx < castles.Count && selectedCalibCastleIdx < customCastlePositions.Length)
        {
            if (selectedCalibCastleIdx != prevSelectedCalibCastleIdx)
            {
                calibColorR = PlayerPrefs.GetFloat("Region_ColorR_" + selectedCalibCastleIdx, 1.0f);
                calibColorG = PlayerPrefs.GetFloat("Region_ColorG_" + selectedCalibCastleIdx, 1.0f);
                calibColorB = PlayerPrefs.GetFloat("Region_ColorB_" + selectedCalibCastleIdx, 1.0f);
                prevSelectedCalibCastleIdx = selectedCalibCastleIdx;
            }

            CastleInstance castle = castles[selectedCalibCastleIdx];
            Vector3 pos = customCastlePositions[selectedCalibCastleIdx];
            
            GUILayout.Label($"<b>Замок {selectedCalibCastleIdx:D2}</b>: {castle.nameRU}", GUI.skin.label);
            GUILayout.Space(4);

            // Слайдеры для 3D координат
            GUILayout.BeginHorizontal();
            GUILayout.Label($"X: {pos.x:F1}", GUILayout.Width(70));
            pos.x = GUILayout.HorizontalSlider(pos.x, -50f, 50f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Y: {pos.y:F1}", GUILayout.Width(70));
            pos.y = GUILayout.HorizontalSlider(pos.y, -15f, 15f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Z: {pos.z:F1}", GUILayout.Width(70));
            pos.z = GUILayout.HorizontalSlider(pos.z, -50f, 50f);
            GUILayout.EndHorizontal();

            // Применяем позицию замка в реальном времени
            customCastlePositions[selectedCalibCastleIdx] = pos;
            if (castle != null && castle.visualRoot != null)
            {
                castle.visualRoot.transform.position = pos;
            }

            GUILayout.Space(10);

            // Настройка BoxCollider (сенсор наведения мыши)
            BoxCollider col = null;
            if (castle != null && castle.visualRoot != null)
            {
                col = castle.visualRoot.GetComponent<BoxCollider>();
            }

            Vector3 cSize = col != null ? col.size : new Vector3(2.5f, 3.5f, 2.5f);
            Vector3 cCent = col != null ? col.center : new Vector3(0f, 1.5f, 0f);

            GUILayout.Label("📐 ТРИГГЕР НАВЕДЕНИЯ (КОЛЛАЙДЕР)", titleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ширина: {cSize.x:F1}", GUILayout.Width(80));
            cSize.x = GUILayout.HorizontalSlider(cSize.x, 0.5f, 10f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Высота: {cSize.y:F1}", GUILayout.Width(80));
            cSize.y = GUILayout.HorizontalSlider(cSize.y, 0.5f, 15f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Глубина: {cSize.z:F1}", GUILayout.Width(80));
            cSize.z = GUILayout.HorizontalSlider(cSize.z, 0.5f, 10f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Центр Y: {cCent.y:F1}", GUILayout.Width(80));
            cCent.y = GUILayout.HorizontalSlider(cCent.y, -5f, 5f);
            GUILayout.EndHorizontal();

            if (col != null)
            {
                col.size = cSize;
                col.center = cCent;
            }

            GUILayout.Space(10);

            // Настройка уникального цвета региона земли под замком
            GUILayout.Label("🎨 ЦВЕТ РЕГИОНА ПОД ЗАМКОМ", titleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"R: {calibColorR:F2}", GUILayout.Width(50));
            float newR = GUILayout.HorizontalSlider(calibColorR, 0.0f, 1.0f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"G: {calibColorG:F2}", GUILayout.Width(50));
            float newG = GUILayout.HorizontalSlider(calibColorG, 0.0f, 1.0f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"B: {calibColorB:F2}", GUILayout.Width(50));
            float newB = GUILayout.HorizontalSlider(calibColorB, 0.0f, 1.0f);
            GUILayout.EndHorizontal();

            if (newR != calibColorR || newG != calibColorG || newB != calibColorB)
            {
                calibColorR = newR;
                calibColorG = newG;
                calibColorB = newB;
                
                // Временно сохраняем в PlayerPrefs для живой отрисовки!
                PlayerPrefs.SetFloat("Region_ColorR_" + selectedCalibCastleIdx, calibColorR);
                PlayerPrefs.SetFloat("Region_ColorG_" + selectedCalibCastleIdx, calibColorG);
                PlayerPrefs.SetFloat("Region_ColorB_" + selectedCalibCastleIdx, calibColorB);
                
                if (LandingPositionManager.Instance != null)
                {
                    LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
                }
            }

            // Кнопки Сохранить / Сбросить
            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.2f, 0.82f, 0.2f);
            if (GUILayout.Button("ЗАПОМНИТЬ", GUILayout.Height(30)))
            {
                PlayerPrefs.SetFloat("Castle_PosX_" + selectedCalibCastleIdx, pos.x);
                PlayerPrefs.SetFloat("Castle_PosY_" + selectedCalibCastleIdx, pos.y);
                PlayerPrefs.SetFloat("Castle_PosZ_" + selectedCalibCastleIdx, pos.z);
                
                PlayerPrefs.SetFloat("Castle_ColCentX_" + selectedCalibCastleIdx, cCent.x);
                PlayerPrefs.SetFloat("Castle_ColCentY_" + selectedCalibCastleIdx, cCent.y);
                PlayerPrefs.SetFloat("Castle_ColCentZ_" + selectedCalibCastleIdx, cCent.z);
                
                PlayerPrefs.SetFloat("Castle_ColSizeX_" + selectedCalibCastleIdx, cSize.x);
                PlayerPrefs.SetFloat("Castle_ColSizeY_" + selectedCalibCastleIdx, cSize.y);
                PlayerPrefs.SetFloat("Castle_ColSizeZ_" + selectedCalibCastleIdx, cSize.z);

                PlayerPrefs.SetFloat("Region_ColorR_" + selectedCalibCastleIdx, calibColorR);
                PlayerPrefs.SetFloat("Region_ColorG_" + selectedCalibCastleIdx, calibColorG);
                PlayerPrefs.SetFloat("Region_ColorB_" + selectedCalibCastleIdx, calibColorB);

                if (customCastlePositions != null && selectedCalibCastleIdx < customCastlePositions.Length)
                {
                    customCastlePositions[selectedCalibCastleIdx] = pos;
                }
                if (castleColliderCenters != null && selectedCalibCastleIdx < castleColliderCenters.Length)
                {
                    castleColliderCenters[selectedCalibCastleIdx] = cCent;
                }
                if (castleColliderSizes != null && selectedCalibCastleIdx < castleColliderSizes.Length)
                {
                    castleColliderSizes[selectedCalibCastleIdx] = cSize;
                }

                PlayerPrefs.SetInt("Castle_Placement_Manual", 1);
                PlayerPrefs.Save();
                
                useManualCastlePositions = true;
                preferScriptCoordinates = false;

                // Перекрашиваем карту в реальном времени!
                if (LandingPositionManager.Instance != null)
                {
                    LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
                }

                Debug.Log($"[CASTLE MGR] Все параметры замка {selectedCalibCastleIdx} и региона сохранены!");
            }

            GUI.backgroundColor = new Color(0.85f, 0.22f, 0.22f);
            if (GUILayout.Button("ОЧИСТИТЬ", GUILayout.Height(30)))
            {
                PlayerPrefs.DeleteKey("Castle_PosX_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_PosY_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_PosZ_" + selectedCalibCastleIdx);
                
                PlayerPrefs.DeleteKey("Castle_ColCentX_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_ColCentY_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_ColCentZ_" + selectedCalibCastleIdx);
                
                PlayerPrefs.DeleteKey("Castle_ColSizeX_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_ColSizeY_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Castle_ColSizeZ_" + selectedCalibCastleIdx);

                PlayerPrefs.DeleteKey("Region_ColorR_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Region_ColorG_" + selectedCalibCastleIdx);
                PlayerPrefs.DeleteKey("Region_ColorB_" + selectedCalibCastleIdx);
                PlayerPrefs.Save();

                // Сброс координат до начальных С# дефолтов
                Vector3[] defaults = new Vector3[12]
                {
                    new Vector3(-15f, 0f, 10f),    // Region_00
                    new Vector3(-5f, 0f, 10f),     // Region_01
                    new Vector3(5f, 0f, 10f),      // Region_02
                    new Vector3(-5.3f, -0.4f, 4.2f), // Region_03 (Святилище Зенита)
                    new Vector3(-15f, 0f, 0f),     // Region_04
                    new Vector3(-5f, 0f, 0f),      // Region_05
                    new Vector3(14.8f, 1.2f, 12.5f), // Region_06 (Ледяной Пик)
                    new Vector3(15f, 0f, 0f),      // Region_07
                    new Vector3(-12.4f, -0.3f, -10.2f), // Region_08 (Древние Руины)
                    new Vector3(-5f, 0f, -10f),    // Region_09
                    new Vector3(5f, 0f, -10f),     // Region_10
                    new Vector3(9.9f, 0.8f, -4.5f) // Region_11 (Кровавые Пустоши)
                };
                customCastlePositions[selectedCalibCastleIdx] = defaults[selectedCalibCastleIdx];
                if (castle != null && castle.visualRoot != null)
                {
                    castle.visualRoot.transform.position = defaults[selectedCalibCastleIdx];
                }

                if (col != null)
                {
                    col.size = new Vector3(2.5f, 3.5f, 2.5f);
                    col.center = new Vector3(0f, 1.5f, 0f);
                }

                if (castleColliderSizes != null && selectedCalibCastleIdx < castleColliderSizes.Length)
                {
                    castleColliderSizes[selectedCalibCastleIdx] = new Vector3(2.5f, 3.5f, 2.5f);
                }
                if (castleColliderCenters != null && selectedCalibCastleIdx < castleColliderCenters.Length)
                {
                    castleColliderCenters[selectedCalibCastleIdx] = new Vector3(0f, 1.5f, 0f);
                }

                // Перекрашиваем карту обратно
                if (LandingPositionManager.Instance != null)
                {
                    LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
                }

                Debug.Log($"[CASTLE MGR] Все параметры замка {selectedCalibCastleIdx} сброшены на дефолты!");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.15f, 0.55f, 0.95f); // Beautiful Zenith Blue
            if (GUILayout.Button("📋 КОПИРОВАТЬ КООРДИНАТЫ", GUILayout.Height(28)))
            {
                CopyAllCoordinatesToClipboard();
            }
            GUI.backgroundColor = new Color(0.95f, 0.65f, 0.1f); // Luxurious gold
            if (GUILayout.Button("📥 ВСТАВИТЬ КООРДИНАТЫ", GUILayout.Height(28)))
            {
                PasteAllCoordinatesFromClipboard();
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(10);
        GUI.backgroundColor = new Color(0.85f, 0.15f, 0.15f);
        if (GUILayout.Button("ЗАКРЫТЬ", GUILayout.Height(28)))
        {
            showCastleCalibrationPanel = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndArea();
    }

    private void DrawStatRow(int curLang, string emoji, string statName, ref int statValue, ref int availablePoints, int minValue)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($" {emoji} {statName}:", GUILayout.Width(150), GUILayout.Height(22));
        GUILayout.Label($"{statValue}", GUILayout.Width(40), GUILayout.Height(22));
        
        // Ограничиваем не-автономным ручным распределением
        if (!isAutonomousStatsDistribution)
        {
            // Кнопка Уменьшить (Если прокачали выше минимального базового значения для класса)
            if (statValue > minValue)
            {
                if (GUILayout.Button("-", GUILayout.Width(32), GUILayout.Height(20)))
                {
                    statValue--;
                    availablePoints++;
                    RecalculateStats();
                    PlayerPrefs.Save();
                }
            }
            else
            {
                GUILayout.Space(36); // Пустышка для удержания сетки
            }
            
            // Кнопка Увеличить (Если есть свободные очки)
            if (availablePoints > 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(32), GUILayout.Height(20)))
                {
                    statValue++;
                    availablePoints--;
                    RecalculateStats();
                    PlayerPrefs.Save();
                }
            }
            else
            {
                GUILayout.Space(36);
            }
        }
        else
        {
            GUILayout.Label("🤖", GUILayout.Width(64), GUILayout.Height(22));
        }
        
        GUILayout.EndHorizontal();
    }

    private void DrawNewDayOverlay(int curLang)
    {
        float wWidth = Screen.width * 0.55f;
        float wHeight = Screen.height * 0.60f;
        float wx = (Screen.width - wWidth) / 2f;
        float wy = (Screen.height - wHeight) / 2f;

        GUI.backgroundColor = new Color(0.02f, 0.05f, 0.12f, 0.99f);
        GUILayout.BeginArea(new Rect(wx, wy, wWidth, wHeight), GUI.skin.box);
        
        GUILayout.Space(12);
        GUIStyle tStyle = new GUIStyle(GUI.skin.label);
        tStyle.alignment = TextAnchor.MiddleCenter;
        tStyle.fontSize = 24;
        tStyle.fontStyle = FontStyle.Bold;
        tStyle.normal.textColor = Color.cyan;

        string repHeader = curLang == 0 ? "ОТЧЕТ КОНТИНЕНТА СУДЬБЫ" : "REPORT OF THE FATE CONTINENT";
        if (curLang == 8) repHeader = "命运大陆军事汇报总览";
        if (curLang == 7) repHeader = "운명의 대륙 군사 정찰 보고서";
        GUILayout.Label($"📅 {repHeader} (День {currentDay})", tStyle);

        GUILayout.Space(15);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
        itemStyle.fontSize = 14;
        itemStyle.alignment = TextAnchor.MiddleLeft;
        itemStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);

        GUILayout.Label(curLang == 0 ? "События на континенте во время смены хода:" : "Actions taken by rival factions during overnight transition:", itemStyle);
        GUILayout.Space(8);

        if (aiLogs.Count == 0)
        {
            GUILayout.Label(curLang == 0 ? "• Спокойный ход времени. Активных конфликтов не обнаружено." : "• Tranquil hours. No unexpected border development or assaults reported.", itemStyle);
        }
        else
        {
            for (int i = 0; i < aiLogs.Count; i++)
            {
                GUILayout.Label($"• {aiLogs[i]}", itemStyle);
            }
        }

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = new Color(0.12f, 0.88f, 0.45f, 1.0f);
        string contBtnLabel = curLang == 0 ? "ПРИНЯТЬ ОТЧЕТ" : "ACKNOWLEDGE REPORT";
        if (curLang == 8) contBtnLabel = "阅览并关闭汇报";
        if (curLang == 7) contBtnLabel = "정찰 보고서 확인";

        if (GUILayout.Button(contBtnLabel, GUILayout.Height(40)))
        {
            showNewDayOverlay = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(12);
        GUILayout.EndArea();
    }

    private void DrawDetailsWindow(int curLang)
    {
        CastleInstance castle = castles[activeDetailsIndex];

        float panelWidth = 485f;
        float panelHeight = (castle.owner == "Player") ? 550f : 620f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.04f, 0.08f, 0.22f, 0.98f);
        
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 15;
        windowStyle.fontStyle = FontStyle.Bold;

        string detailsTitle = curLang == 0 ? "Панель Информации" : "Citadel Interface";
        if (curLang == 8) detailsTitle = "内政控制枢纽";
        if (curLang == 7) detailsTitle = "성채 제어 장치";

        GUI.Window(100, new Rect(px, py, panelWidth, panelHeight), DetailsWindowFunction, detailsTitle, windowStyle);
    }

    private void DetailsWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        CastleInstance castle = castles[activeDetailsIndex];

        GUIStyle titleS = new GUIStyle(GUI.skin.label);
        titleS.alignment = TextAnchor.MiddleCenter;
        titleS.fontSize = 20;
        titleS.fontStyle = FontStyle.Bold;
        titleS.normal.textColor = castle.owner == "Player" ? new Color(0.2f, 1.0f, 0.6f) : new Color(1.0f, 0.3f, 0.4f);

        string labelName = curLang == 0 ? castle.nameRU : castle.nameEN;
        if (curLang == 8) labelName = castle.nameCH;
        if (curLang == 7) labelName = castle.nameKR;

        GUILayout.Label($"🏰 {labelName.ToUpper()}", titleS);

        // QUICK PLAYER CASTLE SWITCHER (v18.11.15)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Player")
            {
                string tabName = curLang == 0 ? castles[i].nameRU : castles[i].nameEN;
                if (curLang == 8) tabName = castles[i].nameCH;
                if (curLang == 7) tabName = castles[i].nameKR;

                // Simple truncation for small buttons
                if (tabName.Length > 10) tabName = tabName.Substring(0, 9) + "..";

                GUI.backgroundColor = (i == activeDetailsIndex) ? new Color(0.12f, 0.82f, 0.98f, 1.0f) : Color.white;
                if (GUILayout.Button($"🏰 {tabName} (L-{castles[i].level})", GUILayout.Height(26)))
                {
                    activeDetailsIndex = i;
                    feedbackMessage = "";
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        GUIStyle subSt = new GUIStyle(GUI.skin.label);
        subSt.alignment = TextAnchor.MiddleCenter;
        subSt.fontSize = 13;
        subSt.normal.textColor = Color.gray;

        string ownerTxt = castle.owner == "Player" ? 
            (curLang == 0 ? "ФРАКЦИОННЫЙ ЦЕНТР ВЫСАДКИ" : "CONTROLLED INTEGRATED outpost") : 
            (curLang == 0 ? "ВРАЖЕСКАЯ ЦИТАДЕЛЬ КЛАНА" : "GARRISON FORTS OF THE ENEMY CLAN");
        GUILayout.Label(ownerTxt, subSt);

        GUILayout.Space(8);

        GUIStyle descS = new GUIStyle(GUI.skin.label);
        descS.fontSize = 15;
        descS.alignment = TextAnchor.MiddleCenter;
        descS.normal.textColor = Color.white;

        string lvlPrefix = curLang == 0 ? "Уровень Цитадели" : "Stronghold Tier";
        if (curLang == 8) lvlPrefix = "领地级别";
        if (curLang == 7) lvlPrefix = "성채 레벨";

        GUILayout.Label($"{lvlPrefix}: {castle.level} / 6", descS);

        int inc = GetGoldIncome(castle.level);
        string flowTxt = curLang == 0 ?
            $"Ежедневный сбор налога: +{inc} 💰 за ход" :
            $"Daily base gold tax: +{inc} 💰 per turn";
        if (curLang == 8) flowTxt = $"每日领地税金: +{inc} 💰 每回合";
        if (curLang == 7) flowTxt = $"일일 연구비 영수: +{inc} 💰 턴당";
        GUILayout.Label(flowTxt, subSt);

        if (castle.owner == "Player")
        {
            int hCount = GetHeroesCountInCastle(castle.zoneIndex);
            int hCap = GetHeroCapacity(castle.level);
            
            string capLabel = curLang == 0 ?
                $"Размещено Героев: {hCount} / {hCap} 👥" :
                $"Garrisoned Heroes: {hCount} / {hCap} 👥";
            if (curLang == 8) capLabel = $"驻军部将: {hCount} / {hCap} 👥";
            if (curLang == 7) capLabel = $"수령 완료 영웅: {hCount} / {hCap} 👥";
            
            GUILayout.Label(capLabel, descS);

            string rNamePlayer = GetCastleRace(castle.zoneIndex, curLang);
            string rLabelPlayer = curLang == 0 ?
                $"Раса цитадели: {rNamePlayer} 🛡️" :
                $"Citadel Race: {rNamePlayer} 🛡️";
            GUILayout.Label(rLabelPlayer, descS);
        }
        else
        {
            // Enemy Castle
            string rName = GetCastleRace(castle.zoneIndex, curLang);
            string rLabel = curLang == 0 ?
                $"Раса гарнизона: {rName} 🛡️" :
                $"Garrison Clan: {rName} 🛡️";
            if (curLang == 8) rLabel = $"防守部族: {rName} 🛡️";
            if (curLang == 7) rLabel = $"수비 진영: {rName} 🛡️";
            
            GUILayout.Label(rLabel, descS);
        }

        GUILayout.Space(12);

        // Feedback
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            GUIStyle feedS = new GUIStyle(GUI.skin.box);
            feedS.normal.textColor = Color.cyan;
            feedS.alignment = TextAnchor.MiddleCenter;
            feedS.fontSize = 13;
            GUILayout.Box(feedbackMessage, feedS, GUILayout.Height(30));
        }
        else
        {
            GUILayout.Space(34);
        }

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUI.skin.box);

        if (castle.owner == "Player")
        {
            // Upgrade button logic: limit to Tier 3 on the first continent
            if (castle.level < 3)
            {
                int nextLvl = castle.level + 1;
                int cost = GetUpgradeCost(castle.level);
                int nextInc = GetGoldIncome(nextLvl);

                string upLabel = curLang == 0 ?
                    $"ПОВЫСИТЬ ДО УРОВНЯ {nextLvl} ({cost} 💰) | Доход: +{nextInc} 💰" :
                    $"UPGRADE TO TIER {nextLvl} ({cost} 💰) | Income: +{nextInc} 💰";
                if (curLang == 8) upLabel = $"升级领地级别 {nextLvl} ({cost} 💰) | 收益: +{nextInc}";
                if (curLang == 7) upLabel = $"영지 무구 개량 {nextLvl}단 ({cost} 💰) | 배당금: +{nextInc}";

                GUI.backgroundColor = new Color(1.0f, 0.85f, 0.15f, 1.0f);
                if (GUILayout.Button(upLabel, GUILayout.Height(42)))
                {
                    if (SaveGameSystem.CurrentData.gold < cost)
                    {
                        string nog = curLang == 0 ? "Недостаточно королевского золота!" : "Insufficient gold supplies!";
                        ShowFeedback(nog);
                    }
                    else
                    {
                        SaveGameSystem.CurrentData.gold -= cost;
                        castle.level++;
                        PlayerPrefs.SetInt("Castle_Level_" + activeDetailsIndex, castle.level);
                        PlayerPrefs.Save();

                        SpawnAllCastles();

                        string okMsg = curLang == 0 ?
                            $"Цитадель расширена до Уровня {castle.level}! Новые чертежи разблокированы." :
                            $"Fortress expanded to Tier {castle.level}! New strategic blueprints unlocked.";
                        ShowFeedback(okMsg);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUIStyle maxS = new GUIStyle(GUI.skin.label);
                maxS.alignment = TextAnchor.MiddleCenter;
                maxS.fontStyle = FontStyle.Bold;
                maxS.normal.textColor = new Color(0.2f, 1.0f, 0.95f, 1.0f);

                string maxLabel = curLang == 0 ? "👑 ДОСТИГНУТ ЛИМИТ 3 УРОВНЯ НА 1-м КОНТИНЕНТЕ!" : "👑 TIER 3 LIMIT REACHED ON THE 1st CONTINENT!";
                if (curLang == 8) maxLabel = "👑 已达到第一大陆的3级上限！";
                if (curLang == 7) maxLabel = "👑 제1대륙 3단계 한계 도달!";

                string subLabel = curLang == 0 ? "Дальнейшее развитие возможно на других континентах." : "Further upgrades are available on other continents.";
                if (curLang == 8) subLabel = "后续升级可在其他大陆进行。";
                if (curLang == 7) subLabel = "추가 확장은 다른 대륙에서 가능합니다.";

                GUILayout.Label(maxLabel, maxS);

                GUIStyle subS = new GUIStyle(GUI.skin.label);
                subS.alignment = TextAnchor.MiddleCenter;
                subS.fontSize = 11;
                subS.normal.textColor = Color.gray;
                GUILayout.Label(subLabel, subS);
            }

            GUILayout.Space(10);

            // КНОПКА ВХОДА В 2D ГОРОД
            string townBtnLabel = curLang == 0 ? "🏟️ ВОЙТИ В 2D ЗАМКОВЫЙ ГОРОД" : "🏟️ ENTER 2D CASTLE TOWN";
            if (curLang == 8) townBtnLabel = "🏟️ 进入2D城堡内城";
            if (curLang == 7) townBtnLabel = "🏟️ 2D 영지 마을 입장";

            GUI.backgroundColor = new Color(0.0f, 0.82f, 0.98f, 1.0f);
            if (GUILayout.Button(townBtnLabel, GUILayout.Height(46)))
            {
                isTownViewActive = true;
                currentTownSubPanel = 0; // Начинаем всегда с основных Обзор города
                isDetailsOpen = false;
                ShowFeedback("");
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            // Вражеский замок - Espionage & Info Panel
            GUIStyle warnS = new GUIStyle(GUI.skin.label);
            warnS.alignment = TextAnchor.MiddleCenter;
            warnS.fontStyle = FontStyle.Bold;
            warnS.fontSize = 12;
            warnS.normal.textColor = new Color(1.0f, 0.35f, 0.35f, 1.0f);

            string warnDesc = curLang == 0 ?
                "ЭТИ ЗЕМЛИ НАХОДЯТСЯ ПОД КОНТРОЛЕМ ВРАЖЕСКИХ ЛОРДОВ.\nДля захвата зачистите Пустоши и поднимите армию!" :
                "THIS PROVINCE LIES DEEP WITHIN ENEMY BORDERS.\nClear tasks and raise an imperial army to assault!";
            GUILayout.Label(warnDesc, warnS);

            GUILayout.Space(8);

            // Espionage logic (v18.11.15)
            int reqMinLevel = GetMinSpyRequiredLevel();
            int pMaxLvl = GetPlayerMaxCastleLevel();
            bool hasMinLvlUnlocked = pMaxLvl >= reqMinLevel;
            bool lvlMatch = pMaxLvl >= castle.level;
            bool isSpied = PlayerPrefs.GetInt("Castle_Spied_" + castle.zoneIndex, 0) == 1;

            GUIStyle intelBox = new GUIStyle(GUI.skin.box);
            intelBox.normal.textColor = Color.yellow;
            intelBox.alignment = TextAnchor.MiddleLeft;
            intelBox.fontSize = 13;

            GUILayout.BeginVertical(intelBox);
            
            string reportHeader = curLang == 0 ? "📋 ДАННЫЕ ВОЕННОЙ РАЗВЕДКИ:" : "📋 MILITARY INTELLIGENCE REPORT:";
            if (curLang == 8) reportHeader = "📋 军事情报搜集总览:";
            if (curLang == 7) reportHeader = "📋 군사 정찰 보고 데이터:";
            GUILayout.Label(reportHeader, GUI.skin.label);
            
            GUILayout.Space(4);

            if (isSpied)
            {
                string guardPowerText = curLang == 0 ?
                    $"• Общая мощь гарнизона: {castle.aiTroopsPower} ед. мощи\n" +
                    $"• Уровень Полководца: {castle.aiCommanderLevel} ур.\n" +
                    $"• Класс ковки защиты: Tier {castle.aiArmorTier}\n" +
                    $"• Запас боевых зелий: {castle.aiPotionsStock} шт." :
                    $"• Total Defense Power: {castle.aiTroopsPower} Combat rating\n" +
                    $"• Faction Commander: Level {castle.aiCommanderLevel}\n" +
                    $"• Guard Armor Quality: Tier {castle.aiArmorTier}\n" +
                    $"• Supply Potions count: {castle.aiPotionsStock} bottles";
                
                if (curLang == 8) guardPowerText = $"• 戍军总战斗力: {castle.aiTroopsPower} 点\n• 守城将领等级: {castle.aiCommanderLevel} 级\n• 防具锻造等级: Tier {castle.aiArmorTier}\n• 备用药水数量: {castle.aiPotionsStock} 瓶";
                if (curLang == 7) guardPowerText = $"• 총 가드 수비력: {castle.aiTroopsPower}\n• 영주 사령관 훈련: {castle.aiCommanderLevel} 렙\n• 장갑 무구 구조: {castle.aiArmorTier} 단계\n• 회복 약물 비축량: {castle.aiPotionsStock} 개";

                GUILayout.Label(guardPowerText, GUI.skin.label);
                
                GUILayout.Space(5);
                GUIStyle okS = new GUIStyle(GUI.skin.label);
                okS.normal.textColor = Color.green;
                okS.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(curLang == 0 ? "✓ Данные разведки получены!" : "✓ Espionage data acquired!", okS);
            }
            else
            {
                string unknownTxt = curLang == 0 ?
                    "⚠️ Гарнизон скрыт туманом войны! Вы не видите численность войск.\n" +
                    $"Требуется уровень вашей цитадели: {reqMinLevel}+" :
                    "⚠️ Garrison obscured by fog of war! Defending army details unknown.\n" +
                    $"Your citadel level required: {reqMinLevel}+";
                if (curLang == 8) unknownTxt = "⚠️ 北境大本营戍军信息处于迷雾中！\n需要我方主城等级达到: " + reqMinLevel + "+";
                if (curLang == 7) unknownTxt = "⚠️ 수비대 정보가 안개로 가려져 있습니다!\n필요 영지 요새 레벨: " + reqMinLevel + "+";
                
                GUILayout.Label(unknownTxt, GUI.skin.label);

                GUILayout.Space(8);

                int spyCost = GetSpyCost(castle.level);

                if (hasMinLvlUnlocked)
                {
                    string spyBtnText = curLang == 0 ?
                        $"Заслать Шпиона ({spyCost} 💰)" :
                        $"Infiltrate Spy ({spyCost} 💰)";
                    if (curLang == 8) spyBtnText = $"派遣细作探子 ({spyCost} 💰)";
                    if (curLang == 7) spyBtnText = $"간첩 잠입시키기 ({spyCost} 💰)";

                    if (GUILayout.Button(spyBtnText, GUILayout.Height(30)))
                    {
                        if (SaveGameSystem.CurrentData.gold < spyCost)
                        {
                            ShowFeedback(curLang == 0 ? "Недостаточно золота в казне!" : "Not enough gold in treasury!");
                        }
                        else
                        {
                            SaveGameSystem.CurrentData.gold -= spyCost;
                            PlayerPrefs.SetInt("Castle_Spied_" + castle.zoneIndex, 1);
                            PlayerPrefs.Save();
                            ShowFeedback(curLang == 0 ? "Шпион успешно проник в замок и доложил обстановку!" : "Spy successfully infiltrated the garrison!");
                        }
                    }
                }
                else
                {
                    GUIStyle lockS = new GUIStyle(GUI.skin.label);
                    lockS.normal.textColor = Color.gray;
                    string lockTxt = curLang == 0 ?
                        $"🔒 Шпионаж недоступен (нужен уровень Замка {reqMinLevel})" :
                        $"🔒 Espionage locked (Citadel Level {reqMinLevel} required)";
                    if (curLang == 8) lockTxt = $"🔒 探子未解锁 (需要主城等级 {reqMinLevel})";
                    if (curLang == 7) lockTxt = $"🔒 정보 정찰 잠금 (요새 레벨 {reqMinLevel} 필요)";
                    GUILayout.Label(lockTxt, lockS);
                }
            }

            GUILayout.EndVertical(); // ends intelBox

            GUILayout.Space(8);
            
            int launchZoneIdx = -1;
            int playerPower = 0;
            for (int i = 0; i < castles.Count; i++)
            {
                if (castles[i].owner == "Player")
                {
                    int pwr = GetPlayerArmyPower(i);
                    if (pwr > playerPower)
                    {
                        playerPower = pwr;
                        launchZoneIdx = i;
                    }
                }
            }

            string armyHeader = curLang == 0 ? "⚔️ ИМПЕРСКИЙ ШТУРМОВОЙ АВАНГАРД:" : "⚔️ IMPERIAL SIEGE VANGUARD:";
            if (curLang == 8) armyHeader = "⚔️ 帝国攻城预备梯队:";
            if (curLang == 7) armyHeader = "⚔️ 제국 공성 준비 전위대:";
            
            GUIStyle armyBox = new GUIStyle(GUI.skin.box);
            armyBox.normal.textColor = Color.cyan;
            armyBox.alignment = TextAnchor.MiddleLeft;
            armyBox.fontSize = 12;
            
            GUILayout.BeginVertical(armyBox);
            GUILayout.Label(armyHeader, GUI.skin.label);
            GUILayout.Space(4);
            
            if (launchZoneIdx != -1)
            {
                string launchCastleName = curLang == 0 ? castles[launchZoneIdx].nameRU : castles[launchZoneIdx].nameEN;
                string armyPowerTxt = curLang == 0 ?
                    $"• Сила атакующей армии: {playerPower} ед. силы\n" +
                    $"• Точка сбора: {launchCastleName} (Замок {launchZoneIdx:D2})" :
                    $"• Available Siege Power: {playerPower} combat score\n" +
                    $"• Gathering point: {launchCastleName} (Castle {launchZoneIdx:D2})";
                GUILayout.Label(armyPowerTxt, GUI.skin.label);
            }
            else
            {
                GUILayout.Label(curLang == 0 ? "❌ У вас нет замков для сбора армии!" : "❌ No player castles found to dispatch armies from!", GUI.skin.label);
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(1.0f, 0.2f, 0.3f, 1.0f);
            string assaultBtnText = curLang == 0 ? "⚔️ НАЧАТЬ ШТУРМ ЗАМКА" : "⚔️ LAUNCH FORTRESS SIEGE";
            if (curLang == 8) assaultBtnText = "⚔️ 发起攻城总攻";
            if (curLang == 7) assaultBtnText = "⚔️ 요새 공성 습격 시작";
            
            if (GUILayout.Button(assaultBtnText, GUILayout.Height(36)))
            {
                PerformBattleShieldSiege(castle.zoneIndex);
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndVertical(); // ends outer box from line 2523

        GUILayout.Space(12);
        GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f, 1.0f);
        string closeBtnTxt = curLang == 0 ? "✖ ВЫХОД ИЗ ЗАМКА" : "✖ EXIT CASTLE";
        if (curLang == 8) closeBtnTxt = "✖ 退出城堡";
        if (curLang == 7) closeBtnTxt = "✖ 성채 퇴성";
        if (GUILayout.Button(closeBtnTxt, GUILayout.Height(36)))
        {
            isDetailsOpen = false;
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawTownViewGUI(int curLang)
    {
        float wWidth = Screen.width * 0.95f;
        float wHeight = Screen.height * 0.82f;
        float wx = (Screen.width - wWidth) / 2f;
        float wy = (Screen.height - wHeight) / 2f;

        GUILayout.BeginArea(new Rect(wx, wy, wWidth, wHeight), GUI.skin.box);

        GUIStyle titleSt = new GUIStyle(GUI.skin.label);
        titleSt.fontSize = 22;
        titleSt.fontStyle = FontStyle.Bold;
        titleSt.alignment = TextAnchor.MiddleCenter;
        titleSt.normal.textColor = Color.cyan;

        CastleInstance activeCastle = castles[activeDetailsIndex];
        string cName = curLang == 0 ? activeCastle.nameRU : activeCastle.nameEN;
        if (curLang == 8) cName = activeCastle.nameCH;
        if (curLang == 7) cName = activeCastle.nameKR;

        GUILayout.Label($"🏯 {cName.ToUpper()} (УРОВЕНЬ {activeCastle.level})", titleSt);

        GUIStyle subSt = new GUIStyle(GUI.skin.label);
        subSt.fontSize = 13;
        subSt.alignment = TextAnchor.MiddleCenter;
        subSt.normal.textColor = Color.gray;

        string subLabel = curLang == 0 ?
            "УПРАВЛЕНИЕ АКТИВНОЙ ФРАКЦИОННОЙ ЦИТАДЕЛЬЮ И НАЙМ ВОИНСТВА" :
            "ACTIVE CASTLE MANAGEMENT, RECRUITMENT & HEROIC PARADE DRILL";
        
        GUILayout.Label(subLabel, subSt);

        GUILayout.Space(6);

        // КНОПКА ВОЗВРАТА В ОБЗОР ГОРОДА (ПОКАЗЫВАЕТСЯ ТОЛЬКО ВНУТРИ ВДРУГ ВЫБРАННЫХ ВЛАДОК)
        if (currentTownSubPanel != 0)
        {
            GUI.backgroundColor = new Color(0.15f, 0.65f, 0.95f, 1.0f);
            if (GUILayout.Button(curLang == 0 ? "◀ ВЕРНУТЬСЯ В ОБЗОР ГОРОДА" : "◀ RETURN TO TOWN OVERVIEW", GUILayout.Height(36)))
            {
                currentTownSubPanel = 0;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(8);
        }

        // Render sections inside selected layout mode
        if (currentTownSubPanel == 0)
        {
            // --- ГЛАВНЫЙ ОБЗОР ГОРОДА (3 КРАСИВЫХ ИНТЕРАКТИВНЫХ ВЫБОРА) ---
            GUILayout.BeginHorizontal();

            float colWidth = wWidth / 3.12f;
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.padding = new RectOffset(16, 16, 16, 16);

            // --- КАЗАРМЫ ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth), GUILayout.Height(wHeight * 0.68f));
            
            GUIStyle colTitle = new GUIStyle(GUI.skin.label);
            colTitle.alignment = TextAnchor.MiddleCenter;
            colTitle.fontSize = 18;
            colTitle.fontStyle = FontStyle.Bold;
            colTitle.normal.textColor = new Color(0.2f, 1.0f, 0.6f);
            
            GUILayout.Label("⚔️ КАЗАРМЫ", colTitle);
            GUILayout.Label(curLang == 0 ? "Найм когерт легиона и войск" : "Legion cohort recruitment", subSt);
            
            GUILayout.FlexibleSpace();
            
            // Mini Castle / Barracks ASCII art
            GUIStyle artStyle = new GUIStyle(GUI.skin.box);
            artStyle.alignment = TextAnchor.MiddleCenter;
            artStyle.fontStyle = FontStyle.Bold;
            artStyle.fontSize = 11;
            artStyle.normal.textColor = new Color(0.4f, 0.95f, 0.55f);
            
            string barracksArt = 
                "       [⚔️]\n" +
                "     ===|===\n" +
                "    [| o o |]\n" +
                "   /_|__-__|_\\\n" +
                "  [___________]\n" +
                "   ||  [ ]  ||\n" +
                "==============#";
            GUILayout.Label(barracksArt, artStyle, GUILayout.Height(125));
            
            GUILayout.FlexibleSpace();
            
            GUIStyle enterBtnStyle = new GUIStyle(GUI.skin.button);
            enterBtnStyle.fontSize = 13;
            enterBtnStyle.fontStyle = FontStyle.Bold;
            enterBtnStyle.normal.textColor = Color.white;
            
            GUI.backgroundColor = new Color(0.12f, 0.72f, 0.42f);
            if (GUILayout.Button(curLang == 0 ? "ВОЙТИ В КАЗАРМЫ" : "ENTER BARRACKS", enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 1;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            // --- КУЗНИЦА И ЛАВКА ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth), GUILayout.Height(wHeight * 0.68f));
            
            GUIStyle colTitle2 = new GUIStyle(GUI.skin.label);
            colTitle2.alignment = TextAnchor.MiddleCenter;
            colTitle2.fontSize = 18;
            colTitle2.fontStyle = FontStyle.Bold;
            colTitle2.normal.textColor = new Color(1.0f, 0.7f, 0.15f);
            
            GUILayout.Label("🧪 КУЗНИЦА И ЛАВКА", colTitle2);
            GUILayout.Label(curLang == 0 ? "Торговля, снаряжение и зелья" : "Elixirs & blacksmith forging", subSt);
            
            GUILayout.FlexibleSpace();
            
            // Mini Forge with Lock ASCII art
            GUIStyle artStyle2 = new GUIStyle(GUI.skin.box);
            artStyle2.alignment = TextAnchor.MiddleCenter;
            artStyle2.fontStyle = FontStyle.Bold;
            artStyle2.fontSize = 11;
            artStyle2.normal.textColor = new Color(1.0f, 0.76f, 0.35f);
            
            string forgeArt = 
                "       [🧪]\n" +
                "     ===|===\n" +
                "    [| 🔒  |]\n" +
                "   /_|_ANVIL_|_\\\n" +
                "  [___________]\n" +
                "   ||       ||\n" +
                "==============#";
            GUILayout.Label(forgeArt, artStyle2, GUILayout.Height(125));
            
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.88f, 0.58f, 0.12f);
            if (GUILayout.Button(curLang == 0 ? "ОТКРЫТЬ КУЗНИЦУ" : "OPEN FORGE & SHOP", enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 2;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            // --- АКАДЕМИЯ И АРЕНА ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth), GUILayout.Height(wHeight * 0.68f));
            
            GUIStyle colTitle3 = new GUIStyle(GUI.skin.label);
            colTitle3.alignment = TextAnchor.MiddleCenter;
            colTitle3.fontSize = 18;
            colTitle3.fontStyle = FontStyle.Bold;
            colTitle3.normal.textColor = new Color(0.85f, 0.45f, 0.95f);
            
            GUILayout.Label("🎓 АКАДЕМИЯ И АРЕНА", colTitle3);
            GUILayout.Label(curLang == 0 ? "Прокачка героев и ранги армии" : "Workout drills & army promotion", subSt);
            
            GUILayout.FlexibleSpace();
            
            // Mini Academic Castle ASCII art
            GUIStyle artStyle3 = new GUIStyle(GUI.skin.box);
            artStyle3.alignment = TextAnchor.MiddleCenter;
            artStyle3.fontStyle = FontStyle.Bold;
            artStyle3.fontSize = 11;
            artStyle3.normal.textColor = new Color(0.82f, 0.58f, 0.95f);
            
            string academyArt = 
                "       [🎓]\n" +
                "     ===|===\n" +
                "    [| 🏛️  |]\n" +
                "   /_|_ARENA_|_\\\n" +
                "  [___________]\n" +
                "   ||  | |  ||\n" +
                "==============#";
            GUILayout.Label(academyArt, artStyle3, GUILayout.Height(125));
            
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.68f, 0.28f, 0.85f);
            if (GUILayout.Button(curLang == 0 ? "ВОЙТИ В АКАДЕМИЮ" : "ENTER ACADEMY", enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 3;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        else
        {
            // Render active section
            GUILayout.BeginHorizontal();

            // --- Column 1: BARRACKS ---
            if (currentTownSubPanel == 1)
            {
                float colWidth = wWidth - 24;
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));
                
                GUIStyle colHeader1 = new GUIStyle(GUI.skin.box);
                colHeader1.alignment = TextAnchor.MiddleCenter;
                colHeader1.fontSize = 17;
                colHeader1.fontStyle = FontStyle.Bold;
                colHeader1.normal.textColor = new Color(0.2f, 1.0f, 0.6f);
                GUILayout.Label("⚔️ КАЗАРМЫ", colHeader1, GUILayout.Height(36));
            
            string bDesc = curLang == 0 ? "Найм войск в армию согласно уровню замка" : "Troop recruitment matching castle tier";
            GUILayout.Label(bDesc, subSt);

            GUILayout.Space(10);
            barracksScroll = GUILayout.BeginScrollView(barracksScroll);

            DrawUnitItem("warrior", "Боец фракции", "Faction Warrior", "皇室精锐战士", "왕실 정예 전사", 50, 1, activeCastle.level);
            DrawUnitItem("archer", "Эльфийский Лучник", "Elven Archer", "精灵神射手", "엘프 신궁 대원", 75, 1, activeCastle.level);
            DrawUnitItem("mage", "Боевой Маг Зенита", "Zenith Battle Mage", "제니스 전투 마법사", "제니스 전투 마법사", 120, 1, activeCastle.level);
            DrawUnitItem("paladin", "Паладин Света", "Holy Paladin", "圣光审判圣骑士", "성광의 발키리 기사", 200, 2, activeCastle.level);
            DrawUnitItem("cavalry", "Имперская Конница", "Imperial Cavalry", "帝国重装重骑兵", "황실 중갑 철기병", 320, 3, activeCastle.level);
            DrawUnitItem("cannoneer", "Осадно-боевой Пушкарь", "Garrison Cannoneer", "重击攻锤铁炮手", "공성 사격 철포병", 450, 4, activeCastle.level);
            DrawUnitItem("centaur", "Кентавр Степей", "Steppe Centaur", "荒野疾行百里人马", "초원의 켄타우로스", 130, 5, activeCastle.level);
            DrawUnitItem("necromancer", "Некромант Тьмы", "Shadow Necromancer", "黑暗禁忌亡灵巫师", "어둠의 네크로맨서", 260, 5, activeCastle.level);
            DrawUnitItem("griffin", "Элитный Королевский Грифон", "Royal Griffin", "皇家狮鹫守御猛禽", "황실 고대 그리폰", 380, 5, activeCastle.level);
            DrawUnitItem("overlord", "Рыцарь-Властелин", "Dread Overlord", "铁王座幽夜统治者", "공포의 지옥 영주", 680, 5, activeCastle.level);
            DrawUnitItem("hydra", "Многоголовая Гидра", "Swamp Hydra", "九头沼泽极冻毒蜃", "맹독의 아홉머리 히드라", 800, 5, activeCastle.level);
            DrawUnitItem("dragon", "Легендарный Дракон Пустоты", "Void Dragon", "虚空至尊不灭邪龙", "허공의 전설 고대 용", 1500, 6, activeCastle.level);
            DrawUnitItem("mountain_bear", "Ураганный Медведь Гор", "Mountain Bear Guard", "极寒高山怒嚎巨熊", "태산의 수호 거대 곰", 1000, 6, activeCastle.level);
            DrawUnitItem("wasteland_serpent", "Гигантская Змея Пустошей", "Wasteland Serpent", "荒原巨型暴食沙蟒", "황무지의 고대 거대 뱀", 1100, 6, activeCastle.level);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // --- Column 2: FORGE & HEALTH SHOP ---
        if (currentTownSubPanel == 0 || currentTownSubPanel == 2)
        {
            float colWidth = (currentTownSubPanel == 2) ? (wWidth - 24) : (wWidth / 3.12f);
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));
            
            GUIStyle colHeader2 = new GUIStyle(GUI.skin.box);
            colHeader2.alignment = TextAnchor.MiddleCenter;
            colHeader2.fontSize = 17;
            colHeader2.fontStyle = FontStyle.Bold;
            colHeader2.normal.textColor = new Color(1.0f, 0.7f, 0.15f);
            GUILayout.Label("🧪 КУЗНИЦА И ЛАВКА", colHeader2, GUILayout.Height(36));
            
            string fDesc = curLang == 0 ? "Покупка зелий разного уровня и 6 тиров доспехов" : "Purchase elixirs & progressive 6 tiers armor gear";
            GUILayout.Label(fDesc, subSt);

            GUILayout.Space(10);
            forgeScroll = GUILayout.BeginScrollView(forgeScroll);

            // POTIONS sections (dynamic scaling with Castle Level and potion levels (1 to 3))
            int potionIndex = PlayerPrefs.GetInt("Town_Selected_PotionLvl", 1);
            GUILayout.BeginHorizontal();
            GUILayout.Label((curLang == 0 ? "Выбор ур-ня зелья: " : "Select Potion level: ") + potionIndex, GUILayout.Width(170));
            if (GUILayout.Button("-", GUILayout.Width(35))) { if (potionIndex > 1) potionIndex--; PlayerPrefs.SetInt("Town_Selected_PotionLvl", potionIndex); }
            if (GUILayout.Button("+", GUILayout.Width(35))) { if (potionIndex < 3) potionIndex++; PlayerPrefs.SetInt("Town_Selected_PotionLvl", potionIndex); }
            GUILayout.EndHorizontal();

            DrawPotionItem("hp", "Зелье Жизни", "Elixir of Vital Health", "生命圣水", "체력 신성 물약", 30, potionIndex, activeCastle.level);
            DrawPotionItem("str", "Зелье Силы", "Potion of Giant Strength", "巨人之力药水", "거인의 괴력 물약", 45, potionIndex, activeCastle.level);
            DrawPotionItem("def", "Зелье Защиты", "Tome of Bastion Defense", "石像鬼坚韧合剂", "철갑 안개의 물약", 40, potionIndex, activeCastle.level);

            GUILayout.Space(15);
            GUILayout.Box(curLang == 0 ? "⚔️ КУЗНИЦА СНАРЯЖЕНИЯ" : "⚔️ FORGE DEPARTMENT", GUILayout.Height(20));

            // Beautiful 8-slot forging options
            DrawForgeEquipmentOption(8, "Оружие", "Weapon", activeCastle.level);
            DrawForgeEquipmentOption(1, "Шлем", "Helmet", activeCastle.level);
            DrawForgeEquipmentOption(4, "Броня", "Chestplate", activeCastle.level);
            DrawForgeEquipmentOption(3, "Наплечники", "Shoulders", activeCastle.level);
            DrawForgeEquipmentOption(7, "Сапоги", "Boots", activeCastle.level);
            DrawForgeEquipmentOption(6, "Пояс", "Belt", activeCastle.level);
            DrawForgeEquipmentOption(2, "Амулет", "Amulet", activeCastle.level);
            DrawForgeEquipmentOption(5, "Кольцо", "Ring", activeCastle.level);

            GUILayout.Space(15);
            GUILayout.Box(curLang == 0 ? "🕵️ НАЙМ ПРОСТЫХ ГЕРОЕВ" : "🕵️ RECRUIT ALLIED HEROES", GUILayout.Height(20));
            
            // Simple Heroes recruitment
            DrawHeroRecruitItem("ArcherHero", "Герой: Стрелок", "Comrade: Marksman Hero", "游侠英雄-神射手", "동료 영웅 - 명사수", 300);
            DrawHeroRecruitItem("WarriorHero", "Герой: Воин", "Comrade: Iron Warrior", "先锋英雄-铁血战士", "동료 영웅 - 광전사", 350);
            DrawHeroRecruitItem("MageHero", "Герой: Боевой Маг", "Comrade: Sorcerer Elite", "元素法师-高阶贤者", "동료 영웅 - 일급 현자", 400);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // --- Column 3: ACADEMY & ARENA ---
        if (currentTownSubPanel == 0 || currentTownSubPanel == 3)
        {
            float colWidth = (currentTownSubPanel == 3) ? (wWidth - 24) : (wWidth / 3.12f);
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));
            
            GUIStyle colHeader3 = new GUIStyle(GUI.skin.box);
            colHeader3.alignment = TextAnchor.MiddleCenter;
            colHeader3.fontSize = 17;
            colHeader3.fontStyle = FontStyle.Bold;
            colHeader3.normal.textColor = new Color(0.85f, 0.45f, 0.95f);
            GUILayout.Label("🎓 АКАДЕМИЯ И АРЕНА", colHeader3, GUILayout.Height(36));
            
            string aDesc = curLang == 0 ? "Тренировки героев, прокачка XP и ранги воинов" : "Hero workouts, dynamic XP drills & troop promotions";
            GUILayout.Label(aDesc, subSt);

            GUILayout.Space(10);
            academyScroll = GUILayout.BeginScrollView(academyScroll);
                        // Display maps level limits
            GUIStyle infoBoxStyle = new GUIStyle(GUI.skin.box);
            infoBoxStyle.normal.textColor = Color.yellow;
            infoBoxStyle.fontSize = 12;
            GUILayout.Label(curLang == 0 ? "📌 Предел Уровня на 1-ой карте: 15" : "📌 Level Ceiling on Map 1: 15", infoBoxStyle);

            GUILayout.Space(10);
            GUILayout.Box(curLang == 0 ? "🎯 ТРЕНИРОВОЧНЫЙ ПЛАЦ" : "🎯 PARADE INSTRUCTION GROUND", GUILayout.Height(20));

            int actLvl = activeCastle.level;
            GUIStyle barGS = new GUIStyle(GUI.skin.label);
            barGS.normal.textColor = Color.green;

            // 1. Main Hero Training
            GUILayout.BeginVertical(GUI.skin.box);
            int pLvl = SaveGameSystem.CurrentData.playerLevel;
            int pXp = SaveGameSystem.CurrentData.currentXP;
            string mainHeroLabel = curLang == 0 ? 
                $"Основной Герой (Ур.{pLvl})\n[Опыт: {pXp}/100]" : 
                $"Main Protagonist (Lvl {pLvl})\n[XP: {pXp}/100]";
            GUILayout.Label(mainHeroLabel, GUI.skin.label);
            
            // Draw visual progress bar for Protagonist
            string pBar = "[";
            int pBlocks = pXp / 10;
            for (int b = 0; b < 10; b++) pBar += (b < pBlocks) ? "■" : "░";
            pBar += "]";
            GUILayout.Label(pBar, barGS);

            int mainTrainCost = pLvl * 15 * actLvl;
            string mainTrainBtn = curLang == 0 ? $"Тренировать ({mainTrainCost} 💰)" : $"Train Protagonist ({mainTrainCost} 💰)";
            if (GUILayout.Button(mainTrainBtn, GUILayout.Height(30)))
            {
                if (SaveGameSystem.CurrentData.gold < mainTrainCost)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно королевского угля и золота в казне!" : "Not enough kingdom gold!");
                }
                else
                {
                    TriggerTraining(0, 0, 35, 15, mainTrainCost, curLang);
                }
            }
            GUILayout.EndVertical();

            // 2. Companion Ranged Training
            GUILayout.BeginVertical(GUI.skin.box);
            int countArcher = GetHeroCount("ArcherHero", activeDetailsIndex);
            int archerLvl = PlayerPrefs.GetInt("Companion_Lvl_ArcherHero", 1);
            int archerXp = PlayerPrefs.GetInt("Companion_XP_ArcherHero", 0);
            GUILayout.Label(curLang == 0 ? 
                $"Нанятые Стрелки (Ур.{archerLvl}) [Нанято: {countArcher}]\n[Опыт: {archerXp}/100]" : 
                $"Hired Marksmen (Lvl {archerLvl}) [Hired: {countArcher}]\n[XP: {archerXp}/100]");
            
            string archBar = "[";
            int archBlocks = archerXp / 10;
            for (int b = 0; b < 10; b++) archBar += (b < archBlocks) ? "■" : "░";
            archBar += "]";
            GUILayout.Label(archBar, barGS);

            int archerTrainCost = archerLvl * 12 * actLvl;
            if (GUILayout.Button(curLang == 0 ? $"Тренировать ({archerTrainCost} 💰)" : $"Train Archers ({archerTrainCost} 💰)", GUILayout.Height(30)))
            {
                if (countArcher <= 0)
                {
                    ShowFeedback(curLang == 0 ? "Необходимо нанять Стрелков в лавке!" : "Hire Marksman First!");
                }
                else if (SaveGameSystem.CurrentData.gold < archerTrainCost)
                {
                    ShowFeedback(curLang == 0 ? "Казна пуста!" : "Insufficient gold!");
                }
                else
                {
                    TriggerTraining(1, 40, 0, 15, archerTrainCost, curLang);
                }
            }
            GUILayout.EndVertical();

            // 3. Companion Warrior Training
            GUILayout.BeginVertical(GUI.skin.box);
            int countWarrior = GetHeroCount("WarriorHero", activeDetailsIndex);
            int warriorLvl = PlayerPrefs.GetInt("Companion_Lvl_WarriorHero", 1);
            int warriorXp = PlayerPrefs.GetInt("Companion_XP_WarriorHero", 0);
            GUILayout.Label(curLang == 0 ? 
                $"Нанятые Воины (Ур.{warriorLvl}) [Нанято: {countWarrior}]\n[Опыт: {warriorXp}/100]" : 
                $"Hired Gladiators (Lvl {warriorLvl}) [Hired: {countWarrior}]\n[XP: {warriorXp}/100]");
            
            string warBar = "[";
            int warBlocks = warriorXp / 10;
            for (int b = 0; b < 10; b++) warBar += (b < warBlocks) ? "■" : "░";
            warBar += "]";
            GUILayout.Label(warBar, barGS);

            int warriorTrainCost = warriorLvl * 12 * actLvl;
            if (GUILayout.Button(curLang == 0 ? $"Тренировать ({warriorTrainCost} 💰)" : $"Train Gladiators ({warriorTrainCost} 💰)", GUILayout.Height(30)))
            {
                if (countWarrior <= 0)
                {
                    ShowFeedback(curLang == 0 ? "Необходимо нанять Воинов в лавке!" : "Hire Gladiator First!");
                }
                else if (SaveGameSystem.CurrentData.gold < warriorTrainCost)
                {
                    ShowFeedback(curLang == 0 ? "Казна пуста!" : "Insufficient gold!");
                }
                else
                {
                    TriggerTraining(2, 40, 0, 15, warriorTrainCost, curLang);
                }
            }
            GUILayout.EndVertical();

            // 4. Companion Mage Training
            GUILayout.BeginVertical(GUI.skin.box);
            int countMage = GetHeroCount("MageHero", activeDetailsIndex);
            int mageLvl = PlayerPrefs.GetInt("Companion_Lvl_MageHero", 1);
            int mageXp = PlayerPrefs.GetInt("Companion_XP_MageHero", 0);
            GUILayout.Label(curLang == 0 ? 
                $"Нанятые Маги (Ур.{mageLvl}) [Нанято: {countMage}]\n[Опыт: {mageXp}/100]" : 
                $"Hired Magicians (Lvl {mageLvl}) [Hired: {countMage}]\n[XP: {mageXp}/100]");
            
            string mageBar = "[";
            int mageBlocks = mageXp / 10;
            for (int b = 0; b < 10; b++) mageBar += (b < mageBlocks) ? "■" : "░";
            mageBar += "]";
            GUILayout.Label(mageBar, barGS);

            int mageTrainCost = mageLvl * 12 * actLvl;
            if (GUILayout.Button(curLang == 0 ? $"Тренировать ({mageTrainCost} 💰)" : $"Train Magicians ({mageTrainCost} 💰)", GUILayout.Height(30)))
            {
                if (countMage <= 0)
                {
                    ShowFeedback(curLang == 0 ? "Необходимо нанять Магов в лавке!" : "Hire Magician First!");
                }
                else if (SaveGameSystem.CurrentData.gold < mageTrainCost)
                {
                    ShowFeedback(curLang == 0 ? "Казна пуста!" : "Insufficient gold!");
                }
                else
                {
                    TriggerTraining(3, 40, 0, 15, mageTrainCost, curLang);
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(15);
            GUILayout.Box(curLang == 0 ? "🛡️ ПОВЫШЕНИЕ РАНГА ВСЕЙ АРМИИ" : "🛡️ ARMY COHORT GLOBAL PROMOTION", GUILayout.Height(20));

            int unitRank = PlayerPrefs.GetInt("Player_ArmyUnit_Rank", 1);
            GUILayout.Label(curLang == 0 ? $"Текущий боевой ранг войск: Кавалеры Tier-{unitRank}" : $"Current Cohort Battle Badge: Tier-{unitRank}");

            if (unitRank < 5)
            {
                int rankPrice = 250 * unitRank * actLvl;
                string promoBtn = curLang == 0 ? $"Повысить Ранг Воинства ({rankPrice} 💰)" : $"Promote Army Rank ({rankPrice} 💰)";
                if (GUILayout.Button(promoBtn, GUILayout.Height(40)))
                {
                    if (SaveGameSystem.CurrentData.gold < rankPrice)
                    {
                        ShowFeedback(curLang == 0 ? "Недостаточно золота!" : "Not enough gold!");
                    }
                    else
                    {
                        SaveGameSystem.CurrentData.gold -= rankPrice;
                        unitRank++;
                        PlayerPrefs.SetInt("Player_ArmyUnit_Rank", unitRank);
                        PlayerPrefs.Save();
                        ShowFeedback(curLang == 0 ? "Ранг воинов успешно повышен! Все войска получили прирост к характеристикам." : "Army rank increased! All units got a stats boost.");
                    }
                }
            }
            else
            {
                GUILayout.Label(curLang == 0 ? "✓ Достигнута максимальная слава воинства!" : "✓ Ultimate military vanguard status reached!", GUI.skin.box);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
    }

        GUILayout.FlexibleSpace();

        // Feedback in Town block
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            GUIStyle feedbackS = new GUIStyle(GUI.skin.box);
            feedbackS.normal.textColor = Color.cyan;
            feedbackS.alignment = TextAnchor.MiddleCenter;
            feedbackS.fontSize = 14;
            GUILayout.Box(feedbackMessage, feedbackS, GUILayout.Height(36));
        }

        GUILayout.Space(8);

        // Return button
        GUI.backgroundColor = new Color(0.9f, 0.2f, 0.25f, 1.0f);
        string leaveLabel = curLang == 0 ? "◀ ВЕРНУТЬСЯ НА ТАКТИЧЕСКУЮ КАРТУ" : "◀ LEAVE AND RETURN TO STRATEGIC WORLD";
        if (curLang == 8) leaveLabel = "◀ 返回战略沙盘图";
        if (curLang == 7) leaveLabel = "◀ 전략 지도 뷰어 복귀";

        if (GUILayout.Button(leaveLabel, GUILayout.Height(45)))
        {
            isTownViewActive = false;
            currentTownSubPanel = 0; // Сверхнадежно сбрасываем при выходе в обзор города
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUILayout.EndArea();
    }

    private void DrawUnitItem(string id, string nameRU, string nameEN, string nameCH, string nameKR, int price, int requiredLvl, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int count = GetUnitCount(id, activeDetailsIndex);
        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        GUILayout.BeginHorizontal(GUI.skin.box);
        
        // Render assignable Avatar box
        Texture2D av = GetTroopAvatarTexture(id);
        GUIStyle avBtnStyle = new GUIStyle(GUI.skin.button);
        avBtnStyle.padding = new RectOffset(0, 0, 0, 0);
        if (av != null)
        {
            if (GUILayout.Button(av, avBtnStyle, GUILayout.Width(44), GUILayout.Height(44)))
            {
                selectedTroopId = id;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        else
        {
            if (GUILayout.Button("📷", GUILayout.Width(44), GUILayout.Height(44)))
            {
                selectedTroopId = id;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }

        GUILayout.Space(5);

        GUIStyle itemBtnStyle = new GUIStyle(GUI.skin.button);
        itemBtnStyle.alignment = TextAnchor.MiddleLeft;
        itemBtnStyle.fontStyle = FontStyle.Bold;
        itemBtnStyle.fontSize = 12;
        itemBtnStyle.normal.textColor = Color.white;
        
        string btnLabel = $"{name}\n(Ур.{requiredLvl} +) | [{count} шт]";
        if (GUILayout.Button(btnLabel, itemBtnStyle, GUILayout.Width(130), GUILayout.Height(44)))
        {
            selectedTroopId = id;
            showTroopDetailPopup = true;
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }

        if (castleLvl < requiredLvl)
        {
            GUI.backgroundColor = Color.grey;
            GUILayout.Button(curLang == 0 ? "⚡ Замок LVL " + requiredLvl : "⚡ Build T-" + requiredLvl, GUILayout.Height(44));
            GUI.backgroundColor = Color.white;
        }
        else
        {
            if (GUILayout.Button($"{price} 💰", GUILayout.Height(44)))
            {
                if (SaveGameSystem.CurrentData.gold < price)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно золота в казне!" : "Not enough gold!");
                }
                else
                {
                    SaveGameSystem.CurrentData.gold -= price;
                    count++;
                    SetUnitCount(id, activeDetailsIndex, count);
                    
                    string buyMsg = curLang == 0 ?
                        $"Отряд {name} нанят в гарнизон!" :
                        $"Cohort {name} recruited successfully!";
                    ShowFeedback(buyMsg);
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawPotionItem(string id, string nameRU, string nameEN, string nameCH, string nameKR, int basePrice, int potionLvl, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int price = Mathf.RoundToInt(basePrice * potionLvl * (castleLvl * 0.4f + 0.6f));
        int count = PlayerPrefs.GetInt($"Player_Potion_{id}_Lvl_{potionLvl}", 0);

        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        GUILayout.BeginHorizontal(GUI.skin.box);
        string itemTitle = $"{name} (Ур.{potionLvl})\n[Запас: {count}]";
        GUILayout.Label(itemTitle, GUILayout.Width(180));

        if (GUILayout.Button($"{price} 💰", GUILayout.Height(35)))
        {
            if (SaveGameSystem.CurrentData.gold < price)
            {
                ShowFeedback(curLang == 0 ? "Жители города отказываются продавать снадобье в долг!" : "Potion merchants deny debt!");
            }
            else
            {
                string potionId = $"potion_{id}_{potionLvl}";
                string itemNameRU = $"{nameRU} (Ур.{potionLvl})";
                if (AddInventoryItem(potionId, itemNameRU, id, 0, potionLvl, 0))
                {
                    SaveGameSystem.CurrentData.gold -= price;
                    count++;
                    PlayerPrefs.SetInt($"Player_Potion_{id}_Lvl_{potionLvl}", count);
                    PlayerPrefs.Save();
                    SaveGameSystem.Save(0);

                    string okFeed = curLang == 0 ?
                        $"Куплено и добавлено в инвентарь: {name} (Ур.{potionLvl})!" :
                        $"Acquired and placed in inventory: {name} (Level {potionLvl})!";
                    ShowFeedback(okFeed);
                }
                else
                {
                    ShowFeedback(curLang == 0 ? "Ваш инвентарь переполнен!" : "Inventory is full!");
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawForgeEquipmentOption(int slotType, string slotNameRU, string slotNameEN, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int currentTier = PlayerPrefs.GetInt($"Player_Equipped_Tier_Slot_{slotType}", 0);
        
        if (currentTier < 6)
        {
            int nextTier = currentTier + 1;
            int price = 80 * nextTier * (int)(castleLvl * 0.4f + 0.6f);
            string name = GetItemName(slotType, nextTier, curLang);
            
            GUILayout.BeginHorizontal(GUI.skin.box);
            string desc = curLang == 0 ? 
                $"Выковать {name}\n[Слот: {slotNameRU}, Tier {nextTier}]" :
                $"Forge {name}\n[Slot: {slotNameEN}, Tier {nextTier}]";
            GUILayout.Label(desc, GUILayout.Width(220));
            
            if (GUILayout.Button($"{price} 💰", GUILayout.Height(35)))
            {
                if (SaveGameSystem.CurrentData.gold < price)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно золота в казне замка!" : "Insufficient gold for blacksmith services!");
                }
                else
                {
                    string itemId = $"item_slot_{slotType}_tier_{nextTier}";
                    string itemNameRU = GetItemName(slotType, nextTier, 0);
                    string iconType = GetIconTypeForSlot(slotType);
                    
                    if (AddInventoryItem(itemId, itemNameRU, iconType, slotType, nextTier, nextTier * 3))
                    {
                        SaveGameSystem.CurrentData.gold -= price;
                        PlayerPrefs.SetInt($"Player_Equipped_Tier_Slot_{slotType}", nextTier);
                        PlayerPrefs.Save();
                        SaveGameSystem.Save(0);
                        ShowFeedback(curLang == 0 ? $"Выковано и помещено в инвентарь: {itemNameRU}!" : $"Forged and placed in inventory: {name}!");
                    }
                    else
                    {
                        ShowFeedback(curLang == 0 ? "Ваш инвентарь переполнен! Освободите место." : "Inventory is full! Free some slots first.");
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label(curLang == 0 ? $"✓ {slotNameRU}: Легендарное качество!" : $"✓ {slotNameEN}: Legendary Quality!", GUI.skin.box);
        }
    }

    private string GetIconTypeForSlot(int slotType)
    {
        switch (slotType)
        {
            case 1: return "helmet";
            case 2: return "necklace";
            case 3: return "shoulders";
            case 4: return "armor";
            case 5: return "ring";
            case 6: return "belt";
            case 7: return "boots";
            case 8: return "weapon";
            default: return "armor";
        }
    }

    private void DrawHeroRecruitItem(string key, string nameRU, string nameEN, string nameCH, string nameKR, int basePrice)
    {
        int curLang = Translator.LanguageID;
        int count = GetHeroCount(key, activeDetailsIndex);

        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        GUILayout.BeginHorizontal(GUI.skin.box);

        // Render assignable Companion Avatar box
        Texture2D av = GetTroopAvatarTexture(key);
        GUIStyle avBtnStyle = new GUIStyle(GUI.skin.button);
        avBtnStyle.padding = new RectOffset(0, 0, 0, 0);
        if (av != null)
        {
            if (GUILayout.Button(av, avBtnStyle, GUILayout.Width(44), GUILayout.Height(44)))
            {
                selectedTroopId = key;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        else
        {
            if (GUILayout.Button("📷", GUILayout.Width(44), GUILayout.Height(44)))
            {
                selectedTroopId = key;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }

        GUILayout.Space(5);

        GUIStyle itemBtnStyle = new GUIStyle(GUI.skin.button);
        itemBtnStyle.alignment = TextAnchor.MiddleLeft;
        itemBtnStyle.fontStyle = FontStyle.Bold;
        itemBtnStyle.fontSize = 12;
        itemBtnStyle.normal.textColor = Color.yellow;

        string btnLabel = $"{name}\n[В замке: {count}]";
        if (GUILayout.Button(btnLabel, itemBtnStyle, GUILayout.Width(130), GUILayout.Height(44)))
        {
            selectedTroopId = key;
            showTroopDetailPopup = true;
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }

        if (GUILayout.Button($"{basePrice} 💰", GUILayout.Height(44)))
        {
            CastleInstance activeCastle = castles[activeDetailsIndex >= 0 ? activeDetailsIndex : 0];
            int currentHeroes = GetHeroesCountInCastle(activeCastle.zoneIndex);
            int capacity = GetHeroCapacity(activeCastle.level);

            if (currentHeroes >= capacity)
            {
                string limitTxt = curLang == 0 ?
                    $"Достигнут лимит героев в этом замке ({currentHeroes}/{capacity})! Повысьте уровень цитадели." :
                    $"Castle hero garrison limit reached ({currentHeroes}/{capacity})! Upgrade stronghold first.";
                if (curLang == 8) limitTxt = $"已达城堡英雄上限 ({currentHeroes}/{capacity})！请先升级主城。";
                if (curLang == 7) limitTxt = $"성채 영웅 한도 초과 ({currentHeroes}/{capacity})! 성채를 먼저 업г레이드 하십시오.";
                ShowFeedback(limitTxt);
            }
            else if (SaveGameSystem.CurrentData.gold < basePrice)
            {
                ShowFeedback(curLang == 0 ? "Легендарные рекруты стоят дорого!" : "Noble adventurers deny cheap calls!");
            }
            else
            {
                SaveGameSystem.CurrentData.gold -= basePrice;
                count++;
                SetHeroCount(key, activeDetailsIndex, count);

                string joinFeed = curLang == 0 ?
                    $"Герой успешно размещен в гарнизоне замка!" :
                    $"Renowned combat leader joined the castle garrison!";
                ShowFeedback(joinFeed);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void TriggerTraining(int target, int baseXP, int mainHeroXP, int cap, int goldCost, int lang)
    {
        if (goldCost > 0)
        {
            SaveGameSystem.CurrentData.gold -= goldCost;
        }

        if (target == 0)
        {
            // Main Hero training
            if (SaveGameSystem.CurrentData.playerLevel >= cap)
            {
                string maxMsg = lang == 0 ? 
                    "Основной герой достиг лимита для этой карты! Улучшите замок." : 
                    "Main protagonist reached maximum level limit of this area! Expand castle.";
                ShowFeedback(maxMsg);
                return;
            }

            SaveGameSystem.CurrentData.currentXP += mainHeroXP;
            if (SaveGameSystem.CurrentData.currentXP >= 100)
            {
                SaveGameSystem.CurrentData.currentXP -= 100;
                SaveGameSystem.CurrentData.playerLevel++;
                
                string sfxLvl = lang == 0 ?
                    $"🌟 УРОВЕНЬ ПОВЫШЕН! Основной герой достиг {SaveGameSystem.CurrentData.playerLevel} уровня!" :
                    $"🌟 protagonist LEVEL UP achieved! Protagonist reached Level {SaveGameSystem.CurrentData.playerLevel}!";
                ShowFeedback(sfxLvl);
            }
            else
            {
                string feed = lang == 0 ?
                    $"Тренировка основного персонажа завершена! (+{mainHeroXP} XP)" :
                    $"protagonist training complete! (+{mainHeroXP} XP)";
                ShowFeedback(feed);
            }
        }
        else
        {
            // Companion training
            string classKey = target == 1 ? "ArcherHero" : (target == 2 ? "WarriorHero" : "MageHero");
            int comradeCount = PlayerPrefs.GetInt("Player_HiredCount_" + classKey, 0);

            if (comradeCount == 0)
            {
                string noH = lang == 0 ?
                    "Вы ещё не наняли союзных героев этого класса в лавке!" :
                    "You have not hired any companion heroes of this class yet!";
                ShowFeedback(noH);
                return;
            }

            int currentCompLvl = PlayerPrefs.GetInt("Companion_Lvl_" + classKey, 1);
            int currentCompXP = PlayerPrefs.GetInt("Companion_XP_" + classKey, 0);

            if (currentCompLvl >= cap - 2) // Companion cap slightly lower for progression balance
            {
                string maxComp = lang == 0 ?
                    "Покупные герои достигли лимита опыта для этой области!" :
                    "Comrades reached local combat ceiling!";
                ShowFeedback(maxComp);
                return;
            }

            currentCompXP += baseXP;
            if (currentCompXP >= 100)
            {
                currentCompXP -= 100;
                currentCompLvl++;
                PlayerPrefs.SetInt("Companion_Lvl_" + classKey, currentCompLvl);

                string lvlMsg = lang == 0 ?
                    $"🌟 ПОДДЕРЖКА ПОВЫШЕНА! Союзные {classKey} подняты до {currentCompLvl} раунда!" :
                    $"🌟 COM COMPANION UPGRADE COMPLETE! Supporting comrades scaled to {currentCompLvl} round!";
                ShowFeedback(lvlMsg);
            }
            else
            {
                string xpMsg = lang == 0 ?
                    $"Отряд союзников потренировался. (+{baseXP} XP)" :
                    $"Recruited cohort trained beautifully. (+{baseXP} XP)";
                ShowFeedback(xpMsg);
            }

            PlayerPrefs.SetInt("Companion_XP_" + classKey, currentCompXP);
            PlayerPrefs.Save();
        }
    }



    public TroopData GetTroopData(string id)
    {
        TroopData td = new TroopData();
        td.id = id;
        
        switch (id)
        {
            case "warrior":
                td.nameRU = "Боец фракции";
                td.nameEN = "Faction Warrior";
                td.descRU = "Базовый пехотинец королевской гвардии. Наносит стабильный физический урон.";
                td.descEN = "Standard frontline foot soldier of the royal guards. Delivers reliable physical impacts.";
                td.tier = 1; td.hp = 120; td.atk = 18; td.def = 10; td.spd = 8;
                td.avatarPrompt = "Symmetrical front portrait of medieval royal infantry fighter, chromium helmet, turquoise neon accents, flat white background.";
                td.passiveNames = new string[] { "Железная Воля" };
                td.passiveDesc = new string[] { "Усиливает защиту на 10% в оборонительной стойке." };
                td.passivePrompts = new string[] { "Symbolic icon of a glowing shield overlaid with light blue divine wings, clean vector design, game ability skill icon, dark fantasy theme." };
                td.activeNames = new string[] { "Удар щитом" };
                td.activeDesc = new string[] { "Наносит врагу 1.2х урона и оглушает на 1 ход." };
                td.activePrompts = new string[] { "Symbolic icon of a massive metal tower shield impacting air with visual shockwave ripples, emerald glow, vector skill art, isolated slate background." };
                break;
                
            case "archer":
                td.nameRU = "Эльфийский Лучник";
                td.nameEN = "Elven Archer";
                td.descRU = "Меткий лесной разведчик. Атакует цели на дальней дистанции со смертельной скоростью.";
                td.descEN = "Sharpshooting woodland ranger. Attacks distant targets with deadly speed and dexterity.";
                td.tier = 1; td.hp = 90; td.atk = 22; td.def = 5; td.spd = 12;
                td.avatarPrompt = "Symmetrical front portrait of elegant elven forest scout archer, emerald leather hood cowl, glowing green eyes, flat white background.";
                td.passiveNames = new string[] { "Меткий Взгляд" };
                td.passiveDesc = new string[] { "Игнорирует 15% показателя уклонения вражеской цели." };
                td.passivePrompts = new string[] { "A sharp glowing emerald eye target reticle lock, neon green lines, clean simplistic mobile ui skill vector icon, fantasy game asset." };
                td.activeNames = new string[] { "Стрела Ветра" };
                td.activeDesc = new string[] { "Стремительный выстрел, наносящий 1.5х урона с вероятностью наложения кровотечения." };
                td.activePrompts = new string[] { "A high speed projectile arrow enveloped in spiral green wind currents and glowing sparks, fantasy spell icon skill vector, dark background." };
                break;

            case "mage":
                td.nameRU = "Боевой Маг Зенита";
                td.nameEN = "Zenith Battle Mage";
                td.descRU = "Призывает чистую энергию Тайной магии для пробития плотных вражеских шеренг.";
                td.descEN = "Channels raw arcane elements to obliterate packed enemy battle ranks from behind.";
                td.tier = 1; td.hp = 80; td.atk = 28; td.def = 4; td.spd = 10;
                td.avatarPrompt = "Symmetrical front portrait of majestic battle mage, cosmic violet hood, glowing arcane face runes, flat white background.";
                td.passiveNames = new string[] { "Источник Маны" };
                td.passiveDesc = new string[] { "Восстанавливает 10 единиц маны каждый ход." };
                td.passivePrompts = new string[] { "A crystal flask shaped container filled with glowing liquid purple magic energy, sparkles, stylized mobile RPG skill vector icon." };
                td.activeNames = new string[] { "Чародейская Вспышка" };
                td.activeDesc = new string[] { "Мощный взрыв стихий во вражеской зоне с игнорированием 50% брони противника." };
                td.activePrompts = new string[] { "A magnificent spiral cosmic explosion of violet nebula starlight, beam of magic projectile, spell ability tile icon vector." };
                break;

            case "paladin":
                td.nameRU = "Паладин Света";
                td.nameEN = "Holy Paladin";
                td.descRU = "Святой рыцарь-храмовник, защищающий союзников щитом праведной ненависти к скверне.";
                td.descEN = "Holy templar knight defending companions with a shield of righteous indignation against decay.";
                td.tier = 2; td.hp = 240; td.atk = 25; td.def = 22; td.spd = 6;
                td.avatarPrompt = "Symmetrical front portrait of legendary holy paladin templar knight, golden runic plate armor, bright halo, flat white background.";
                td.passiveNames = new string[] { "Аура Света", "Священный Доспех" };
                td.passiveDesc = new string[] { "Повышает ОЗ всех союзных воинов на 15%.", "Урон по паладину снижен на 15% на линии фронта." };
                td.passivePrompts = new string[] { "Golden mystical sun rays bursting outwards from a glowing star construct, fantasy halo aura, vector ui icon asset.", "A celestial shining golden breastplate, surrounded by holy runes, pristine specular shine, icon design." };
                td.activeNames = new string[] { "Очищение" };
                td.activeDesc = new string[] { "Исцеляет выбранную союзную цель на 40 ОЗ и снимает негативные эффекты." };
                td.activePrompts = new string[] { "A ray of warm divine light beam descending, dissolving darkness, fantasy healing spell skill design icon." };
                break;

            case "cavalry":
                td.nameRU = "Имперская Конница";
                td.nameEN = "Imperial Cavalry";
                td.descRU = "Элитное ударное подразделение империи. Наводит ужас быстрыми прорывами с флангов.";
                td.descEN = "Elite devastating cavalry corps of the empire. Spreads terror with swift flanking maneuvers.";
                td.tier = 3; td.hp = 350; td.atk = 34; td.def = 25; td.spd = 14;
                td.avatarPrompt = "Symmetrical front portrait of heavy royal cavalry crusader knight on armored destrier horse, obsidian lance, flat white background.";
                td.passiveNames = new string[] { "Натиск", "Закаленный Всадник" };
                td.passiveDesc = new string[] { "Урон увеличивается на 2% за каждую клетку, пройденную перед ударом.", "Иммунитет к эффектам замедления хода." };
                td.passivePrompts = new string[] { "A silhouetted heavy horse hoof kicking up dirt with golden energy trail, movement blur, skill emblem icon.", "Twin crossed iron lances wrapped in red banners, royal insignia emblem, medieval battle pass skill icon." };
                td.activeNames = new string[] { "Разбег" };
                td.activeDesc = new string[] { "Мощный копейный выпад, сметающий первую линию обороны и оглушающий цель." };
                td.activePrompts = new string[] { "Heavy steel lance tip sparking with lightning kinetic force during thrust motion, vector web emblem design." };
                break;

            case "cannoneer":
                td.nameRU = "Осадно-боевой Пушкарь";
                td.nameEN = "Garrison Cannoneer";
                td.descRU = "Заведует тяжелыми осадными мортирами. Стиль ведения огня абсолютно разрушителен.";
                td.descEN = "Controls devastating siege mortar batteries. Fire pattern is completely annihilating.";
                td.tier = 4; td.hp = 400; td.atk = 55; td.def = 15; td.spd = 5;
                td.avatarPrompt = "Symmetrical front portrait of seasoned dwarf fortress cannon engineer with brass machinery goggles, coal smoke, flat white background.";
                td.passiveNames = new string[] { "Осадный Прицел", "Тяжелый Порох" };
                td.passiveDesc = new string[] { "Корректирует разброс: +20% к урону по укреплениям и замкам.", "Увеличивает радиус поражения зоны поражения активного навыка." };
                td.passivePrompts = new string[] { "Crosshair overlay on a castle wall projection with structural stress points, skill icon.", "A wooden barrel with burning fuse, sparkling black powder, game skill icon design." };
                td.activeNames = new string[] { "Разрушительный Залп" };
                td.activeDesc = new string[] { "Артиллерийский выстрел по площади, наносящий 1.7х огненного урона по всем врагам в зоне." };
                td.activePrompts = new string[] { "Massive bronze mortar cannon barrel firing a fiery exploding cannonball with smoke rings, stylized 3D blast icon." };
                break;

            case "centaur":
                td.nameRU = "Кентавр Степей";
                td.nameEN = "Steppe Centaur";
                td.descRU = "Быстрый получеловек-полулошадь. Осыпает врага градом дротиков на высокой скорости.";
                td.descEN = "Fast half-human half-horse hybrid scouts. Showers targets with spears at lightning swift rates.";
                td.tier = 5; td.hp = 500; td.atk = 40; td.def = 18; td.spd = 16;
                td.avatarPrompt = "Symmetrical front portrait of wild plain centaur hunter master, braided rustic hair, holding ashwood spear, flat white background.";
                td.passiveNames = new string[] { "Степной Ветер", "Охотничий Инстинкт" };
                td.passiveDesc = new string[] { "Опережает инициативу противника: +20% шанс нанести удар первым.", "Повышает урон по бестиям и драконам на 25%." };
                td.passivePrompts = new string[] { "Whirlwind spiral dust wind trail over plains, speed visual feedback, vector talent icon.", "Wild predator claw marks glowing yellow, stylized nature hunter emblem, game asset graphic." };
                td.activeNames = new string[] { "Бросок Копья" };
                td.activeDesc = new string[] { "Бросает копье сквозь строй врага, нанося линейный сквозной урон." };
                td.activePrompts = new string[] { "A razor sharp war spear propelled forward with intense sonic bloom, game skill icon." };
                break;

            case "necromancer":
                td.nameRU = "Некромант Тьмы";
                td.nameEN = "Shadow Necromancer";
                td.descRU = "Манипулирует темной магией смерти, поднимая павших бойцов из могилы.";
                td.descEN = "Manipulates shadow sorcery of decay, raising fallen infantrymen from the damp graves.";
                td.tier = 5; td.hp = 450; td.atk = 48; td.def = 12; td.spd = 11;
                td.avatarPrompt = "Symmetrical front portrait of dark occult necromancer mage, skull mask cowl hood, green eerie bone spell particles, flat white background.";
                td.passiveNames = new string[] { "Жатва Душ", "Оскверненная Кровь" };
                td.passiveDesc = new string[] { "Каждая смерть воина на поле боя лечит некроманта на 15% ОЗ.", "Враги, атакующие некроманта, получают отравление кислотой." };
                td.passivePrompts = new string[] { "Glowing neon green hands snatching wandering spectral soul wisps, spell ability emblem design.", "Splatter of dark toxic blood causing smoke acid melting, mobile tactical ui icon." };
                td.activeNames = new string[] { "Подъем Скелета" };
                td.activeDesc = new string[] { "Призывает скелета-воина на случайную пустую клетку поля." };
                td.activePrompts = new string[] { "Skeletal hand breaking through dry graveyard soil holding rusted iron blade, under eerie neon green moonlight." };
                break;

            case "griffin":
                td.nameRU = "Элитный Королевский Грифон";
                td.nameEN = "Royal Griffin";
                td.descRU = "Летающий хищник. Пикирует на врагов с небес, игнорируя препятствия и ландшафт.";
                td.descEN = "Legendary winged predator. Dives down at enemy forces bypassing defensive terrain blocks.";
                td.tier = 5; td.hp = 650; td.atk = 52; td.def = 20; td.spd = 18;
                td.avatarPrompt = "Symmetrical close-up front-facing hawk portrait of ancient royal phoenix griffin beast with golden feather crest, flat white background.";
                td.passiveNames = new string[] { "Превосходство Высоты", "Неуловимый Полет", "Гнездовье" };
                td.passiveDesc = new string[] { "+25% урона по целям на равнинной местности.", "Игнорирует наземные ловушки и препятствия.", "Ускоряет регенерацию здоровья на 10% вне активного боя." };
                td.passivePrompts = new string[] { "Giant eagle silhouette diving from clouds against sun, majestic wings spread, skill icon vectors.", "Feather wings flapping leaving faint gold sparkles traces, agility passive icon decoration.", "Woven wooden high nest holding golden glowing bird egg on stellar mountaintop." };
                td.activeNames = new string[] { "Удар Когтями" };
                td.activeDesc = new string[] { "Смертоносный порез, вызывающий длительное кровотечение у цели." };
                td.activePrompts = new string[] { "Four razor sharp metal talon claw marks glowing white cutting through slate iron armor metal plates." };
                break;

            case "overlord":
                td.nameRU = "Рыцарь-Властелин";
                td.nameEN = "Dread Overlord";
                td.descRU = "Тяжелобронированный владыка тьмы, вселяющий парализующий ужас во вражеские сердца.";
                td.descEN = "Heavily plated dark lord spreading paralyzing horror and compliance directly among foes.";
                td.tier = 5; td.hp = 850; td.atk = 68; td.def = 35; td.spd = 9;
                td.avatarPrompt = "Symmetrical front portrait of dread skeleton doom warlord in spiky dark void iron crown plates, purple glow, flat white background.";
                td.passiveNames = new string[] { "Аура Ужаса", "Прилив Скверны", "Костяной Щит" };
                td.passiveDesc = new string[] { "Снижает боевой дух и атаку всех окружающих врагов на 15%.", "+20% к урону, когда показатель ОЗ падает ниже половины.", "Поглощает первые 100 единиц любого физического урона." };
                td.passivePrompts = new string[] { "Terrifying demonic face shadow mask outline with glowing void purple eyes, psychological warfare icon.", "A black bubbling dynamic wave of dark corrupted water rising, red highlights.", "Ring of three spinning jagged human ribs bones creating protective spectral shield barrier." };
                td.activeNames = new string[] { "Клинок Бездны" };
                td.activeDesc = new string[] { "Фронтальный взмах мечом, крадущий жизнь у пораженных противников (30% вампиризм)." };
                td.activePrompts = new string[] { "Gigantic spiky obsidian greatsword blade wreathed in dark purple flame, trail arc vector." };
                break;

            case "hydra":
                td.nameRU = "Многоголовая Гидра";
                td.nameEN = "Swamp Hydra";
                td.descRU = "Титаническое чудовище болот. Атакует несколько противников одновременно круговым укусом.";
                td.descEN = "Titan boss of the murky swamps. Sweeps multiple nearby enemies simultaneously with jaws.";
                td.tier = 5; td.hp = 1000; td.atk = 58; td.def = 30; td.spd = 8;
                td.avatarPrompt = "Symmetrical front portrait of terrifying multi-headed swamp hydra dragon serpent reptilian heads, glowing green venom spit, flat white background.";
                td.passiveNames = new string[] { "Кислотные Укусы", "Регенерация Тела", "Токсичная Кожа" };
                td.passiveDesc = new string[] { "Снижает броню целей на 5 единиц при каждом укусе.", "Восстанавливает 10% максимального запаса ОЗ каждый раунд.", "Нападающие на гидру отряды ближнего боя отравляются ядом." };
                td.passivePrompts = new string[] { "Two reptilian snake fangs dripping luminous fluid green venom droplets, dark focus.", "Lizard scaly tail re-growing with light blue biological cellular cell activity glowing layers.", "Close-up of poisonous swamp frog skin texture with neon green toxic pores, fantasy style." };
                td.activeNames = new string[] { "Тройная Атака" };
                td.activeDesc = new string[] { "Три головы наносят удар по трем различным соседствующим целям в секторе." };
                td.activePrompts = new string[] { "Three giant snake heads lunging simultaneously forward in dynamic action from left to right." };
                break;

            case "dragon":
                td.nameRU = "Легендарный Дракон Пустоты";
                td.nameEN = "Void Dragon";
                td.descRU = "Высшее существо межзвездной бездны. Испепеляет целые отряды дыханием чистой плазмы.";
                td.descEN = "Supreme sovereign of interstellar void. Devastates cohorts with concentrated plasma breath.";
                td.tier = 6; td.hp = 2000; td.atk = 120; td.def = 50; td.spd = 20;
                td.avatarPrompt = "Symmetrical front portrait of giant celestial void leviathan dragon beast, body of glowing purple nebula gas, flat white background.";
                td.passiveNames = new string[] { "Чешуя Пустоты", "Межзвездная Ярость", "Суперсонический полет" };
                td.passiveDesc = new string[] { "Полный иммунитет к заклинаниям контроля разума и оглушениям.", "Урон повышается на 10% за каждого павшего союзника.", "Увеличивает скорость перемещения по тактическому полю на 50%." };
                td.passivePrompts = new string[] { "Indestructible dark amethyst crystal dragon scales layout glistening with starry points, spell deflect.", "Raging cosmic violet dragon claw icon clutching a core of glowing supernova, raw power.", "Dragon wings outline glowing at warp speed crossing star systems, sonic boom ripples." };
                td.activeNames = new string[] { "Дыхание Плазмы" };
                td.activeDesc = new string[] { "Извергает поток космического огня на всю вражескую колонну, сжигая цели." };
                td.activePrompts = new string[] { "A stream of brilliant cosmic purple stellar flame blast incinerating iron targets on black background." };
                break;

            case "mountain_bear":
                td.nameRU = "Ураганный Медведь Гор";
                td.nameEN = "Mountain Bear Guard";
                td.descRU = "Величественный северный гигант. Крушит тяжелые латы врагов сокрушительными лапами.";
                td.descEN = "Colossal ancient mountain giant. Crushes enemy armor with heavy frosted iron-hard paws.";
                td.tier = 6; td.hp = 1800; td.atk = 95; td.def = 60; td.spd = 10;
                td.avatarPrompt = "Symmetrical front portrait of colossus runic polar bear guardian, chestplates carved of mountain range blue runic ice, flat white background.";
                td.passiveNames = new string[] { "Морозная Стойкость", "Снежный Гнев", "Ледяной Доспех" };
                td.passiveDesc = new string[] { "Снижает входящий урон на 20%, пока активен эффект щитов.", "С каждым полученным ударом сила атаки медведя вырастает на 5%.", "+30% сопротивления магии огня." };
                td.passivePrompts = new string[] { "Armored polar bear footprint seal glowing with cold runic frost blue energy on snow surface.", "Raging bear face silhouette glowing red inside frosted glacier shard outline, power boost.", "Slab of thick clear polar blue glacier ice plate covering ancient chest piece armor master." };
                td.activeNames = new string[] { "Растерзание" };
                td.activeDesc = new string[] { "Удар лапой, наносящий 2.0х урона и снижающий инициативу цели на 50%." };
                td.activePrompts = new string[] { "Enormous bear claws slashing vertically downwards leaving three thick ice-frost gashes in midnight air." };
                break;

            case "wasteland_serpent":
                td.nameRU = "Гигантская Змея Пустошей";
                td.nameEN = "Wasteland Serpent";
                td.descRU = "Colossus-червь песчаных дюн. Реализует внезапные броски из-под земли, поглощая пехоту.";
                td.descEN = "Colossal dunes crawler. Executes surprise breaches from underground, swallowing infantry whole.";
                td.tier = 6; td.hp = 1500; td.atk = 110; td.def = 40; td.spd = 15;
                td.avatarPrompt = "Symmetrical front portrait of massive desert dunes sands serpent, golden crystalline scales, open jaws of crystalline sand-fire, flat white background.";
                td.passiveNames = new string[] { "Песчаная Скрытность", "Твердость Чешуи", "Дюны Внимания" };
                td.passiveDesc = new string[] { "Имеет 30% шанс полностью уворачиваться от стрелковых атак.", "Уменьшает пробитие брони от вражеских копейщиков на 40%.", "Ослепляет атакующих противников облаком поднятой песчаной пыли." };
                td.passivePrompts = new string[] { "A golden sandy whirlpool vortex sucking down debris under bright intense desert sun.", "Layer of diamond hard golden crystalline snake skin scales pattern, shiny sunlight glint.", "Dune mirage of giant golden snake eyes outline shimmering over hot heatwave sand." };
                td.activeNames = new string[] { "Поглощение" };
                td.activeDesc = new string[] { "Заглатывает одиночного слабого вражеского пехотинца, мгновенно убивая его." };
                td.activePrompts = new string[] { "Massive vertical serpent maw filled with rows of needle teeth rising directly from sand swirl." };
                break;
                
            default:
                td.nameRU = "Секретный Наемник";
                td.nameEN = "Secret Mercenary";
                td.descRU = "Неизвестный странник континента.";
                td.descEN = "Rogue traveler from remote borders.";
                td.tier = 1; td.hp = 100; td.atk = 15; td.def = 8; td.spd = 10;
                td.avatarPrompt = "Medieval masked assassin silhouette.";
                td.passiveNames = new string[] { "Скрытность" };
                td.passiveDesc = new string[] { "Защищает от первой атаки врага." };
                td.passivePrompts = new string[] { "Smoke bomb explosion." };
                td.activeNames = new string[] { "Удар в спину" };
                td.activeDesc = new string[] { "Критический скрытый узел." };
                td.activePrompts = new string[] { "Crossed daggers." };
                break;
        }
        
        return td;
    }

    public CompanionData GetCompanionData(string key)
    {
        CompanionData cd = new CompanionData();
        cd.id = key;
        
        if (key == "ArcherHero")
        {
            cd.nameRU = "Герой: Стрелок-Следопыт";
            cd.nameEN = "Comrade: Marksman Scout";
            cd.descRU = "Элитный наемный герой дальнего боя. Растет в уровне, повышая урон и точность.";
            cd.descEN = "Elite contract ranged shooter. Scales stats automatically upon training ground drills.";
            cd.avatarPrompt = "High precision portrait of elite fantasy rangers bowmaster, sapphire eyes, runic leather hood, white background.";
            cd.passiveNames = new string[] { "Ветряной Щит", "Критическая Метка" };
            cd.passiveDesc = new string[] { "Дарует +15% шанс полностью увернуться от стрелковых атак врага.", "Каждый 3-й выстрел гарантированно наносит критический урон 2.0х." };
            cd.passivePrompts = new string[] { "Golden wind barrier circular shield deflection sparkles, fantasy vector skill icon.", "Glowing red skull mark target icon on enemy helmet, high contrast vector icon." };
            cd.activeName = "Обстрел Пустоты";
            cd.activeDesc = "Выпускает веер из пяти светящихся стрел наносящий масштабный урон по площади.";
            cd.activePrompt = "Five glowing violet arrow projectiles flying simultaneously in fan pattern, dark violet trace.";
        }
        else if (key == "WarriorHero")
        {
            cd.nameRU = "Герой: Железный Воин";
            cd.nameEN = "Comrade: Iron Gladiator";
            cd.descRU = "Могучий боец авангарда. Превосходно сдерживает фланги и защищает пушкарей.";
            cd.descEN = "Mighty vanguard swordsman. Excels at crowd control and shielding ranged artillery lines.";
            cd.avatarPrompt = "High precision portrait of grizzled barbarian gladiator fighter, scarred cheeks, giant skull pauldron plate, white background.";
            cd.passiveNames = new string[] { "Закалка Металла", "Брат Гвардии" };
            cd.passiveDesc = new string[] { "Увеличивает защиту на 4 единицы за каждых 20% потерянного здоровья.", "Близстоящие союзные воины получают на 10% меньше физического урона." };
            cd.passivePrompts = new string[] { "A glowing anvil with a mystical iron sword being forged under bright yellow flames, game icon.", "Symbolic alliance hand shake between heavy armored gauntlets, gold neon ribbon emblem." };
            cd.activeName = "Удар Разрушителя";
            cd.activeDesc = "Оглушает цель на 2 хода и пробивает 35% показателя брони тяжелого пехотинца.";
            cd.activePrompt = "Heavy metal battle hammer impact on soil cracking stone tiles with volcanic magma veins glowing.";
        }
        else // MageHero
        {
            cd.nameRU = "Герой: Боевой Маг Элит";
            cd.nameEN = "Comrade: Sorcerer Elite";
            cd.descRU = "Стихийный заклинатель. Поддерживает соратников щитами и поражает врагов звездами.";
            cd.descEN = "Elemental summoner. Protects military lines with fire shield barriers and frozen starlights.";
            cd.avatarPrompt = "High precision portrait of high sorcerer archmage, starry cosmic wizard crown beard, glowing nebula light, white background.";
            cd.passiveNames = new string[] { "Щит Возмездия", "Тайный Ткач" };
            cd.passiveDesc = new string[] { "Атакующие мага в ближнем бою противники получают 15 ед. ответного урона молнией.", "+15% к магическому урону за каждые 200 накопленных в казне золотых монет фракции." };
            cd.passivePrompts = new string[] { "Crackling blue electric storm shield barrier encircling glowing central orb, vector shield skill icon.", "Mystical hands weaving glowing thread lines of stellar nebula starlight cosmos, magic craft." };
            cd.activeName = "Инферно Звездопада";
            cd.activeDesc = "Призывает столб пламени, сжигающий линию врагов с наложением горения на 3 хода.";
            cd.activePrompt = "A majestic high column of red-orange elemental volcano fire vortex eruption on isolated block.";
        }
        
        return cd;
    }

    public int GetCompanionStat(string classKey, string statName, int level)
    {
        int str = 10, agi = 10, intel = 10, sta = 10;
        if (classKey == "WarriorHero")
        {
            str = 15 + (level - 1) * 3;
            agi = 10 + (level - 1) * 1;
            intel = 4 + (level - 1) * 1;
            sta = 15 + (level - 1) * 3;
        }
        else if (classKey == "ArcherHero")
        {
            str = 10 + (level - 1) * 1;
            agi = 14 + (level - 1) * 3;
            intel = 6 + (level - 1) * 1;
            sta = 11 + (level - 1) * 2;
        }
        else // MageHero
        {
            str = 6 + (level - 1) * 1;
            agi = 10 + (level - 1) * 1;
            intel = 10 + (level - 1) * 4;
            sta = 9 + (level - 1) * 1;
        }

        if (statName == "strength") return str;
        if (statName == "agility") return agi;
        if (statName == "intelligence") return intel;
        if (statName == "stamina") return sta;

        if (statName == "hp") return sta * 10;
        if (statName == "atk") return Mathf.RoundToInt(str * 2.5f + agi * 0.5f);
        if (statName == "def") return Mathf.RoundToInt(agi * 1.5f + str * 0.5f);

        return 10;
    }

    #region CLIPBOARD COPY/PASTE MULTI-COORDINATE ENGINE (Zenith Calibration Sync)
    [ContextMenu("Copy All Castle Coordinates")]
    public void CopyAllCoordinatesToClipboard()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("FATE_CASTLE_CONFIG_START");
        for (int i = 0; i < 12; i++)
        {
            Vector3 pos = (customCastlePositions != null && i < customCastlePositions.Length) ? customCastlePositions[i] : Vector3.zero;
            Vector3 off = (castleManualOffsets != null && i < castleManualOffsets.Length) ? castleManualOffsets[i] : new Vector3(3.2f, 0f, 3.2f);
            Vector3 csz = (castleColliderSizes != null && i < castleColliderSizes.Length) ? castleColliderSizes[i] : new Vector3(2.5f, 3.5f, 2.5f);
            Vector3 cct = (castleColliderCenters != null && i < castleColliderCenters.Length) ? castleColliderCenters[i] : new Vector3(0f, 1.5f, 0f);
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CASTLE_{0:D2}_POS:{1:F3},{2:F3},{3:F3}", i, pos.x, pos.y, pos.z));
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CASTLE_{0:D2}_OFF:{1:F3},{2:F3},{3:F3}", i, off.x, off.y, off.z));
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CASTLE_{0:D2}_CSZ:{1:F3},{2:F3},{3:F3}", i, csz.x, csz.y, csz.z));
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CASTLE_{0:D2}_CCT:{1:F3},{2:F3},{3:F3}", i, cct.x, cct.y, cct.z));
        }
        sb.AppendLine("FATE_CASTLE_CONFIG_END");
        
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("<color=gold>[CASTLE MGR] Все 12 позиций, смещений и триггеров успешно скопированы в буфер!</color>");
    }

    [ContextMenu("Paste All Castle Coordinates")]
    public bool PasteAllCoordinatesFromClipboard()
    {
        string text = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(text) || !text.Contains("FATE_CASTLE_CONFIG_START"))
        {
            Debug.LogError("[CASTLE MGR] Буфер обмена пуст или не содержит корректную сигнатуру FATE_CASTLE_CONFIG_START!");
            return false;
        }

        try
        {
            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int parsedPositionsCount = 0;
            int parsedOffsetsCount = 0;
            int parsedCollidersCount = 0;

            if (customCastlePositions == null || customCastlePositions.Length != 12)
            {
                System.Array.Resize(ref customCastlePositions, 12);
            }
            if (castleManualOffsets == null || castleManualOffsets.Length != 12)
            {
                System.Array.Resize(ref castleManualOffsets, 12);
            }
            if (castleColliderSizes == null || castleColliderSizes.Length != 12)
            {
                System.Array.Resize(ref castleColliderSizes, 12);
            }
            if (castleColliderCenters == null || castleColliderCenters.Length != 12)
            {
                System.Array.Resize(ref castleColliderCenters, 12);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("CASTLE_") && line.Contains("_POS:"))
                {
                    int posIdx = line.IndexOf("_POS:");
                    string indexStr = line.Substring(7, 2);
                    int idx = int.Parse(indexStr);
                    string coordsStr = line.Substring(posIdx + 5);
                    string[] coords = coordsStr.Split(',');
                    if (coords.Length == 3 && idx >= 0 && idx < 12)
                    {
                        float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);
                        customCastlePositions[idx] = new Vector3(x, y, z);
                        
                        // Сохранение в PlayerPrefs
                        PlayerPrefs.SetFloat("Castle_PosX_" + idx, x);
                        PlayerPrefs.SetFloat("Castle_PosY_" + idx, y);
                        PlayerPrefs.SetFloat("Castle_PosZ_" + idx, z);
                        parsedPositionsCount++;
                    }
                }
                else if (line.StartsWith("CASTLE_") && line.Contains("_OFF:"))
                {
                    int offIdx = line.IndexOf("_OFF:");
                    string indexStr = line.Substring(7, 2);
                    int idx = int.Parse(indexStr);
                    string coordsStr = line.Substring(offIdx + 5);
                    string[] coords = coordsStr.Split(',');
                    if (coords.Length == 3 && idx >= 0 && idx < 12)
                    {
                        float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);
                        castleManualOffsets[idx] = new Vector3(x, y, z);
                        
                        // Сохранение смещений в PlayerPrefs
                        PlayerPrefs.SetFloat("Castle_ManualOffset_PosX_" + idx, x);
                        PlayerPrefs.SetFloat("Castle_ManualOffset_PosY_" + idx, y);
                        PlayerPrefs.SetFloat("Castle_ManualOffset_PosZ_" + idx, z);
                        parsedOffsetsCount++;
                    }
                }
                else if (line.StartsWith("CASTLE_") && line.Contains("_CSZ:"))
                {
                    int cszIdx = line.IndexOf("_CSZ:");
                    string indexStr = line.Substring(7, 2);
                    int idx = int.Parse(indexStr);
                    string coordsStr = line.Substring(cszIdx + 5);
                    string[] coords = coordsStr.Split(',');
                    if (coords.Length == 3 && idx >= 0 && idx < 12)
                    {
                        float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);
                        castleColliderSizes[idx] = new Vector3(x, y, z);
                        
                        // Сохранение в PlayerPrefs
                        PlayerPrefs.SetFloat("Castle_ColSizeX_" + idx, x);
                        PlayerPrefs.SetFloat("Castle_ColSizeY_" + idx, y);
                        PlayerPrefs.SetFloat("Castle_ColSizeZ_" + idx, z);
                        parsedCollidersCount++;
                    }
                }
                else if (line.StartsWith("CASTLE_") && line.Contains("_CCT:"))
                {
                    int cctIdx = line.IndexOf("_CCT:");
                    string indexStr = line.Substring(7, 2);
                    int idx = int.Parse(indexStr);
                    string coordsStr = line.Substring(cctIdx + 5);
                    string[] coords = coordsStr.Split(',');
                    if (coords.Length == 3 && idx >= 0 && idx < 12)
                    {
                        float x = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture);
                        castleColliderCenters[idx] = new Vector3(x, y, z);
                        
                        // Сохранение в PlayerPrefs
                        PlayerPrefs.SetFloat("Castle_ColCentX_" + idx, x);
                        PlayerPrefs.SetFloat("Castle_ColCentY_" + idx, y);
                        PlayerPrefs.SetFloat("Castle_ColCentZ_" + idx, z);
                        parsedCollidersCount++;
                    }
                }
            }

            if (parsedPositionsCount > 0 || parsedOffsetsCount > 0 || parsedCollidersCount > 0)
            {
                PlayerPrefs.SetInt("Castle_Placement_Manual", 1);
                PlayerPrefs.Save();
                
                useManualCastlePositions = true;
                preferScriptCoordinates = false;

                // Перестроение замков, если в игровом контексте
                SpawnAllCastles();
                
                // Перекраска регионов, если LandingPositionManager активен
                if (LandingPositionManager.Instance != null)
                {
                    LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
                }
                
                Debug.Log(string.Format("<color=green>[CASTLE MGR] Успешно импортировано {0} позиций, {1} смещений и {2} триггеров из буфера!</color>", parsedPositionsCount, parsedOffsetsCount, parsedCollidersCount));
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[CASTLE MGR] Ошибка критического парсинга координат: " + e.Message);
        }
        return false;
    }
    #endregion
}

#if UNITY_EDITOR
namespace FateContinent
{
    using UnityEditor;

    [CustomEditor(typeof(FateCastleManager))]
    public class FateCastleManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FateCastleManager manager = (FateCastleManager)target;

            // Золотая панель Zenith Glassmorphism для быстрого копирования всего замка в инспекторе
            EditorGUILayout.Space(12);
            EditorGUILayout.BeginVertical("box");
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.normal.textColor = new Color(1.0f, 0.72f, 0.1f); // Насыщенный золотой цвет Zenith
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.fontSize = 11;
            
            EditorGUILayout.LabelField("⚡ FATE CASTLE CALIBRATION (CLIPBOARD SYNC)", headerStyle);
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Копировать ВСЕ Позиции & Смещения", GUILayout.Height(30)))
            {
                manager.CopyAllCoordinatesToClipboard();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.35f, 0.95f, 0.45f); // Сочный зеленый для вставки
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📥 Вставить ВСЕ Позиции & Смещения", GUILayout.Height(30)))
            {
                manager.PasteAllCoordinatesFromClipboard();
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = originalBg;

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("💡 Совет: Скопируйте всю конфигурацию замков одной кнопкой для переноса между сценами или сохранения настроек позиционирования в текстовый файл!", MessageType.Info);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(12);

            // Отрисовка стандартного инспектора после кастомного хедера
            DrawDefaultInspector();
        }
    }
}
#endif

public class TroopData
{
    public string id;
    public string nameRU;
    public string nameEN;
    public string descRU;
    public string descEN;
    public int tier;
    public int hp;
    public int atk;
    public int def;
    public int spd;
    public string avatarPrompt;
    
    public string[] passiveNames;
    public string[] passiveDesc;
    public string[] passivePrompts;
    
    public string[] activeNames;
    public string[] activeDesc;
    public string[] activePrompts;
}

public class CompanionData
{
    public string id;
    public string nameRU;
    public string nameEN;
    public string descRU;
    public string descEN;
    public string avatarPrompt;
    
    public string[] passiveNames;
    public string[] passiveDesc;
    public string[] passivePrompts;
    
    public string activeName;
    public string activeDesc;
    public string activePrompt;
}

/// <summary>
/// Скрипт-маркер, вещаемый на 3D замки для улавливания кликов
/// </summary>
public class InteractiveCastle : MonoBehaviour
{
    public int zoneIndex;
}
