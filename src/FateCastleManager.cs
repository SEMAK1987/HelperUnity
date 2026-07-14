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
        EventHub.OnCombatEnd -= HandleCombatEndEvent;
        EventHub.OnCombatStart -= HandleCombatStartEvent;
    }

    [System.Serializable]
    public class TroopSkillAsset
    {
        public string troopId;
        public Texture2D activeIcon;
        public Texture2D passiveIcon1;
        public Texture2D passiveIcon2;
        public Texture2D passiveIcon3;
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

    [Header("⚔️ НАВЫКИ ВОИНОВ (BARRACKS TROOP SKILL ICONS)")]
    [Tooltip("Список ассетов навыков для всех 14 типов войск, редактируемый напрямую через инспектор")]
    public List<TroopSkillAsset> troopSkillAssets = new List<TroopSkillAsset>();

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

    [Header("🛡️ GEAR & CLASS WEAPON ICONS (v18.11.22)")]
    [Tooltip("Шлем (Slot 1) • Prompt: Symmetrical front portrait of a legendary royal glowing steel knight helmet, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_helmet;
    [Tooltip("Амулет (Slot 2) • Prompt: Symmetrical front portrait of a mystical glowing sapphire amulet pendant, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_amulet;
    [Tooltip("Наплечники (Slot 3) • Prompt: Symmetrical front portrait of legendary steel knight pauldrons, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_pauldrons;
    [Tooltip("Доспех (Slot 4) • Prompt: Symmetrical front portrait of legendary royal steel knight plate body armor, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_armor;
    [Tooltip("Кольцо (Slot 5) • Prompt: Symmetrical front portrait of a legendary golden diamond signet ring, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_ring;
    [Tooltip("Пояс (Slot 6) • Prompt: Symmetrical front portrait of a legendary runic leather belt, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_belt;
    [Tooltip("Сапоги (Slot 7) • Prompt: Symmetrical front portrait of legendary steel knight armor boots, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D icon_boots;
    [Tooltip("Меч Воина (Slot 8) • Prompt: Symmetrical front portrait of a legendary royal glowing steel broadsword, epic runic gold and turquoise neon blade, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D weapon_warrior_sword;
    [Tooltip("Лук Стрелка (Slot 8) • Prompt: Symmetrical front portrait of a mystical elven composite recurve bow, glowing jade wood finish, magic emerald neon arrow nocked on string, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D weapon_archer_bow;
    [Tooltip("Посох Мага (Slot 8) • Prompt: Symmetrical front portrait of a majestic wizard's archmage runic staff, crystal sapphire sphere floating at tip radiating blue lightning, game UI item slot style, flat white background, digital art 8k.")]
    public Texture2D weapon_mage_staff;

    [Header("🧪 POTION ICONS (v18.11.22)")]
    [Tooltip("Зелье Жизни • Prompt: Symmetrical high-quality icon of an elegant ornate small glass potion bottle filled with magical glowing bubbling bright red liquid. Realistic glass texture, RPG game asset style, centered. Isolated on solid black background, no shadows, no gradient, 8k.")]
    public Texture2D icon_potion_hp;
    [Tooltip("Зелье Силы • Prompt: Symmetrical high-quality icon of an elegant ornate small glass potion bottle filled with magical glowing bubbling fiery orange liquid. Realistic glass texture, RPG game asset style, centered. Isolated on solid black background, no shadows, no gradient, 8k.")]
    public Texture2D icon_potion_str;
    [Tooltip("Зелье Интеллекта • Prompt: Symmetrical high-quality icon of an elegant ornate small glass potion bottle filled with magical glowing bubbling deep purple liquid. Realistic glass texture, RPG game asset style, centered. Isolated on solid black background, no shadows, no gradient, 8k.")]
    public Texture2D icon_potion_int;
    [Tooltip("Зелье Ловкости • Prompt: Symmetrical high-quality icon of an elegant ornate small glass potion bottle filled with magical glowing bubbling vibrant emerald green liquid. Realistic glass texture, RPG game asset style, centered. Isolated on solid black background, no shadows, no gradient, 8k.")]
    public Texture2D icon_potion_agi;
    [Tooltip("Зелье Защиты/Выносливости • Prompt: Symmetrical high-quality icon of an elegant ornate small glass potion bottle filled with magical glowing bubbling celestial blue liquid. Realistic glass texture, RPG game asset style, centered. Isolated on solid black background, no shadows, no gradient, 8k.")]
    public Texture2D icon_potion_sta;

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

    [Header("🔥 ИКОНКИ АКТИВНЫХ НАВЫКОВ ВОЙСК (BARRACKS ACTIVE SKILLS ICONS)")]
    [Tooltip("Боец фракции • Удар щитом • Prompt: Symbolic icon of a massive metal tower shield impacting air with visual shockwave ripples, emerald glow, vector skill art, isolated slate background.")]
    public Texture2D skill_warrior_active;
    [Tooltip("Эльфийский Лучник • Стрела Ветра • Prompt: A high speed projectile arrow enveloped in spiral green wind currents and glowing sparks, fantasy spell icon skill vector, dark background.")]
    public Texture2D skill_archer_active;
    [Tooltip("Боевой Маг Зенита • Чародейская Вспышка • Prompt: A magnificent spiral cosmic explosion of violet nebula starlight, beam of magic projectile, spell ability tile icon vector.")]
    public Texture2D skill_mage_active;
    [Tooltip("Паладин Света • Очищение • Prompt: A ray of warm divine light beam descending, dissolving darkness, fantasy healing spell skill design icon.")]
    public Texture2D skill_paladin_active;
    [Tooltip("Имперская Конница • Разбег • Prompt: Heavy steel lance tip sparking with lightning kinetic force during thrust motion, vector web emblem design.")]
    public Texture2D skill_cavalry_active;
    [Tooltip("Осадно-боевой Пушкарь • Разрушительный Залп • Prompt: Massive bronze mortar cannon barrel firing a fiery exploding cannonball with smoke rings, stylized 3D blast icon.")]
    public Texture2D skill_cannoneer_active;
    [Tooltip("Кентавр Степей • Бросок Копья • Prompt: A razor sharp war spear propelled forward with intense sonic bloom, game skill icon.")]
    public Texture2D skill_centaur_active;
    [Tooltip("Некромант Тьмы • Подъем Скелета • Prompt: Skeletal hand breaking through dry graveyard soil holding rusted iron blade, under eerie neon green moonlight.")]
    public Texture2D skill_necromancer_active;
    [Tooltip("Элитный Королевский Грифон • Удар Когтями • Prompt: Four razor sharp metal talon claw marks glowing white cutting through slate iron armor metal plates.")]
    public Texture2D skill_griffin_active;
    [Tooltip("Рыцарь-Властелин • Клинок Бездны • Prompt: Gigantic spiky obsidian greatsword blade wreathed in dark purple flame, trail arc vector.")]
    public Texture2D skill_overlord_active;
    [Tooltip("Многоголовая Гидра • Тройная Атака • Prompt: Three giant snake heads lunging simultaneously forward in dynamic action from left to right.")]
    public Texture2D skill_hydra_active;
    [Tooltip("Легендарный Дракон Пустоты • Дыхание Плазмы • Prompt: A stream of brilliant cosmic purple stellar flame blast incinerating iron targets on black background.")]
    public Texture2D skill_dragon_active;
    [Tooltip("Ураганный Медведь Гор • Растерзание • Prompt: Enormous bear claws slashing vertically downwards leaving three thick ice-frost gashes in midnight air.")]
    public Texture2D skill_mountain_bear_active;
    [Tooltip("Гигантская Змея Пустошей • Поглощение • Prompt: Massive vertical serpent maw filled with rows of needle teeth rising directly from sand swirl.")]
    public Texture2D skill_wasteland_serpent_active;

    [Header("❄️ ИКОНКИ ПАССИВНЫХ НАВЫКОВ ВОЙСК (BARRACKS PASSIVE SKILLS ICONS)")]
    [Tooltip("Боец фракции • Железная Воля • Prompt: Symbolic icon of a glowing shield overlaid with light blue divine wings, clean vector design, game ability skill icon, dark fantasy theme.")]
    public Texture2D skill_warrior_passive1;
    [Tooltip("Эльфийский Лучник • Меткий Взгляд • Prompt: A sharp glowing emerald eye target reticle lock, neon green lines, clean simplistic mobile ui skill vector icon, fantasy game asset.")]
    public Texture2D skill_archer_passive1;
    [Tooltip("Боевой Маг Зенита • Источник Маны • Prompt: A crystal flask shaped container filled with glowing liquid purple magic energy, sparkles, stylized mobile RPG skill vector icon.")]
    public Texture2D skill_mage_passive1;
    [Tooltip("Паладин Света • Аура Света • Prompt: Golden mystical sun rays bursting outwards from a glowing star construct, fantasy halo aura, vector ui icon asset.")]
    public Texture2D skill_paladin_passive1;
    [Tooltip("Паладин Света • Священный Доспех • Prompt: A celestial shining golden breastplate, surrounded by holy runes, pristine specular shine, icon design.")]
    public Texture2D skill_paladin_passive2;
    [Tooltip("Имперская Конница • Натиск • Prompt: A silhouetted heavy horse hoof kicking up dirt with golden energy trail, movement blur, skill emblem icon.")]
    public Texture2D skill_cavalry_passive1;
    [Tooltip("Имперская Конница • Закаленный Всадник • Prompt: Twin crossed iron lances wrapped in red banners, royal insignia emblem, medieval battle pass skill icon.")]
    public Texture2D skill_cavalry_passive2;
    [Tooltip("Осадно-боевой Пушкарь • Осадный Прицел • Prompt: Crosshair overlay on a castle wall projection with structural stress points, skill icon.")]
    public Texture2D skill_cannoneer_passive1;
    [Tooltip("Осадно-боевой Пушкарь • Тяжелый Порох • Prompt: A wooden barrel with burning fuse, sparkling black powder, game skill icon design.")]
    public Texture2D skill_cannoneer_passive2;
    [Tooltip("Кентавр Степей • Степной Ветер • Prompt: Whirlwind spiral dust wind trail over plains, speed visual feedback, vector talent icon.")]
    public Texture2D skill_centaur_passive1;
    [Tooltip("Кентавр Степей • Охотничий Инстинкт • Prompt: Wild predator claw marks glowing yellow, stylized nature hunter emblem, game asset graphic.")]
    public Texture2D skill_centaur_passive2;
    [Tooltip("Некромант Тьмы • Жатва Душ • Prompt: Glowing neon green hands snatching wandering spectral soul wisps, spell ability emblem design.")]
    public Texture2D skill_necromancer_passive1;
    [Tooltip("Некромант Тьмы • Оскверненная Кровь • Prompt: Splatter of dark toxic blood causing smoke acid melting, mobile tactical ui icon.")]
    public Texture2D skill_necromancer_passive2;
    [Tooltip("Элитный Королевский Грифон • Превосходство Высоты • Prompt: Giant eagle silhouette diving from clouds against sun, majestic wings spread, skill icon vectors.")]
    public Texture2D skill_griffin_passive1;
    [Tooltip("Элитный Королевский Грифон • Неуловимый Полет • Prompt: Feather wings flapping leaving faint gold sparkles traces, agility passive icon decoration.")]
    public Texture2D skill_griffin_passive2;
    [Tooltip("Элитный Королевский Грифон • Гнездовье • Prompt: Woven wooden high nest holding golden glowing bird egg on stellar mountaintop.")]
    public Texture2D skill_griffin_passive3;
    [Tooltip("Рыцарь-Властелин • Аура Ужаса • Prompt: Terrifying demonic face shadow mask outline with glowing void purple eyes, psychological warfare icon.")]
    public Texture2D skill_overlord_passive1;
    [Tooltip("Рыцарь-Властелин • Прилив Скверны • Prompt: A black bubbling dynamic wave of dark corrupted water rising, red highlights.")]
    public Texture2D skill_overlord_passive2;
    [Tooltip("Рыцарь-Властелин • Костяной Щит • Prompt: Ring of three spinning jagged human ribs bones creating protective spectral shield barrier.")]
    public Texture2D skill_overlord_passive3;
    [Tooltip("Многоголовая Гидра • Кислотные Укусы • Prompt: Two reptilian snake fangs dripping luminous fluid green venom droplets, dark focus.")]
    public Texture2D skill_hydra_passive1;
    [Tooltip("Многоголовая Гидра • Регенерация Тела • Prompt: Lizard scaly tail re-growing with light blue biological cellular cell activity glowing layers.")]
    public Texture2D skill_hydra_passive2;
    [Tooltip("Многоголовая Гидра • Токсичная Кожа • Prompt: Close-up of poisonous swamp frog skin texture with neon green toxic pores, fantasy style.")]
    public Texture2D skill_hydra_passive3;
    [Tooltip("Легендарный Дракон Пустоты • Чешуя Пустоты • Prompt: Indestructible dark amethyst crystal dragon scales layout glistening with starry points, spell deflect.")]
    public Texture2D skill_dragon_passive1;
    [Tooltip("Легендарный Дракон Пустоты • Межзвездная Ярость • Prompt: Raging cosmic violet dragon claw icon clutching a core of glowing supernova, raw power.")]
    public Texture2D skill_dragon_passive2;
    [Tooltip("Легендарный Дракон Пустоты • Суперсонический полет • Prompt: Dragon wings outline glowing at warp speed crossing star systems, sonic boom ripples.")]
    public Texture2D skill_dragon_passive3;
    [Tooltip("Ураганный Медведь Гор • Морозная Стойкость • Prompt: Armored polar bear footprint seal glowing with cold runic frost blue energy on snow surface.")]
    public Texture2D skill_mountain_bear_passive1;
    [Tooltip("Ураганный Медведь Гор • Снежный Гнев • Prompt: Raging bear face silhouette glowing red inside frosted glacier shard outline, power boost.")]
    public Texture2D skill_mountain_bear_passive2;
    [Tooltip("Ураганный Медведь Гор • Ледяной Доспех • Prompt: Slab of thick clear polar blue glacier ice plate covering ancient chest piece armor master.")]
    public Texture2D skill_mountain_bear_passive3;
    [Tooltip("Гигантская Змея Пустошей • Песчаная Скрытность • Prompt: A golden sandy whirlpool vortex sucking down debris under bright intense desert sun.")]
    public Texture2D skill_wasteland_serpent_passive1;
    [Tooltip("Гигантская Змея Пустошей • Твердость Чешуи • Prompt: Layer of diamond hard golden crystalline snake skin scales pattern, shiny sunlight glint.")]
    public Texture2D skill_wasteland_serpent_passive2;
    [Tooltip("Гигантская Змея Пустошей • Дюны Внимания • Prompt: Dune mirage of giant golden snake eyes outline shimmering over hot heatwave sand.")]
    public Texture2D skill_wasteland_serpent_passive3;

    [Header("🕵️ ПРОСТЫЕ НАНИМАЕМЫЕ ГЕРОИ - АВАТАРКИ/ИКОНКИ")]
    [Tooltip("Герой-Стрелок • Prompt: High precision portrait of elite fantasy rangers bowmaster, sapphire eyes, runic leather hood, white background.")]
    public Texture2D avatar_hero_archer;
    [Tooltip("Герой-Воин • Prompt: High precision portrait of grizzled barbarian gladiator fighter, scarred cheeks, giant skull pauldron plate, white background.")]
    public Texture2D avatar_hero_warrior;
    [Tooltip("Герой-Боевой Маг • Prompt: High precision portrait of high sorcerer archmage, starry cosmic wizard crown beard, glowing nebula light, white background.")]
    public Texture2D avatar_hero_mage;

    [Header("🕵️ ПРОСТЫЕ НАНИМАЕМЫЕ ГЕРОИ - АКТИВНЫЕ И ПАССИВНЫЕ НАВЫКИ")]
    [Tooltip("Герой-Стрелок • Ветряной Щит (Пассивный) • Prompt: Golden wind barrier circular shield deflection sparkles, fantasy vector skill icon.")]
    public Texture2D skill_hero_archer_passive1;
    [Tooltip("Герой-Стрелок • Критическая Метка (Пассивный) • Prompt: Glowing red skull mark target icon on enemy helmet, high contrast vector icon.")]
    public Texture2D skill_hero_archer_passive2;
    [Tooltip("Герой-Стрелок • Обстрел Пустоты (Активный) • Prompt: Five glowing violet arrow projectiles flying simultaneously in fan pattern, dark violet trace.")]
    public Texture2D skill_hero_archer_active;

    [Tooltip("Герой-Воин • Закалка Металла (Пассивный) • Prompt: A glowing anvil with a mystical iron sword being forged under bright yellow flames, game icon.")]
    public Texture2D skill_hero_warrior_passive1;
    [Tooltip("Герой-Воин • Брат Гвардии (Пассивный) • Prompt: Symbolic alliance hand shake between heavy armored gauntlets, gold neon ribbon emblem.")]
    public Texture2D skill_hero_warrior_passive2;
    [Tooltip("Герой-Воин • Удар Разрушителя (Активный) • Prompt: Heavy metal battle hammer impact on soil cracking stone tiles with volcanic magma veins glowing.")]
    public Texture2D skill_hero_warrior_active;

    [Tooltip("Герой-Боевой Маг • Щит Возмездия (Пассивный) • Prompt: Crackling blue electric storm shield barrier encircling glowing central orb, vector shield skill icon.")]
    public Texture2D skill_hero_mage_passive1;
    [Tooltip("Герой-Боевой Маг • Ткач Заклинаний (Пассивный) • Prompt: Mystical hands weaving glowing thread lines of stellar nebula starlight cosmos, magic craft.")]
    public Texture2D skill_hero_mage_passive2;
    [Tooltip("Герой-Боевой Маг • Инферно Звездопада (Активный) • Prompt: A majestic high column of red-orange elemental volcano fire vortex eruption on isolated block.")]
    public Texture2D skill_hero_mage_active;

    [Header("⚔️ КУЗНИЦА СНАРЯЖЕНИЯ - ИКОНКИ ПО ТИРАМ (Т1 - Т6)")]
    public Texture2D[] warriorWeaponIcons = new Texture2D[6];
    public Texture2D[] archerWeaponIcons = new Texture2D[6];
    public Texture2D[] mageWeaponIcons = new Texture2D[6];
    
    public Texture2D[] warriorHelmetIcons = new Texture2D[6];
    public Texture2D[] archerHelmetIcons = new Texture2D[6];
    public Texture2D[] mageHelmetIcons = new Texture2D[6];
    public Texture2D[] helmetIcons = new Texture2D[6];

    public Texture2D[] warriorAmuletIcons = new Texture2D[6];
    public Texture2D[] archerAmuletIcons = new Texture2D[6];
    public Texture2D[] mageAmuletIcons = new Texture2D[6];
    public Texture2D[] amuletIcons = new Texture2D[6];

    public Texture2D[] warriorPauldronsIcons = new Texture2D[6];
    public Texture2D[] archerPauldronsIcons = new Texture2D[6];
    public Texture2D[] magePauldronsIcons = new Texture2D[6];
    public Texture2D[] pauldronsIcons = new Texture2D[6];

    public Texture2D[] warriorArmorIcons = new Texture2D[6];
    public Texture2D[] archerArmorIcons = new Texture2D[6];
    public Texture2D[] mageArmorIcons = new Texture2D[6];
    public Texture2D[] armorIcons = new Texture2D[6];

    public Texture2D[] warriorRingIcons = new Texture2D[6];
    public Texture2D[] archerRingIcons = new Texture2D[6];
    public Texture2D[] mageRingIcons = new Texture2D[6];
    public Texture2D[] ringIcons = new Texture2D[6];

    public Texture2D[] warriorBeltIcons = new Texture2D[6];
    public Texture2D[] archerBeltIcons = new Texture2D[6];
    public Texture2D[] mageBeltIcons = new Texture2D[6];
    public Texture2D[] beltIcons = new Texture2D[6];

    public Texture2D[] warriorBootsIcons = new Texture2D[6];
    public Texture2D[] archerBootsIcons = new Texture2D[6];
    public Texture2D[] mageBootsIcons = new Texture2D[6];
    public Texture2D[] bootsIcons = new Texture2D[6];

    // Active dynamic placeholders resolved on Load/Draw
    private Texture2D activeSkillPassive1;
    private Texture2D activeSkillPassive2;
    private Texture2D activeSkillPassive3;
    private Texture2D activeSkillUltimate;
    
    // UI states and trackers
    public bool isTownViewActive = false;
    private int currentTownSubPanel = 0; // 0: All Columns (split view), 1: Barracks only, 2: Forge only, 3: Academy only
    private string[,] gridUnits = new string[10, 10];
    private bool isGridInitialized = false;
    private int selectedGridRow = -1;
    private int selectedGridCol = -1;
    private bool showTroopDetailPopup = false;
    private string selectedTroopId = "";

    public bool isAutonomousStatsDistribution = false;
    public bool showStatsPanel = false;
    public bool isDetailsOpen = false;
    
    // 🕵️ Spy Network Variables (v18.11.24)
    public bool showSpyReportPopup = false;
    public int activeSpyReportZoneIndex = -1;
    private Vector2 spyScrollPos = Vector2.zero;
    private int activeDetailsIndex = -1;
    private string feedbackMessage = "";
    private float messageTimer = 0f;
    private float clickCooldown = 0f;
    
    public int currentDay = 1;

    // AI notification logs shown during new-day transition
    private List<string> aiLogs = new List<string>();
    private bool showNewDayOverlay = false;
    private float overlayTimer = 0f;

    // Сглаженные показатели для оверлея телеметрии производительности ПК (v18.11.25)
    private float smoothTelemetryFps = 60f;
    private float smoothTelemetryCpuLoad = 12f;
    private float smoothTelemetryGpuLoad = 25f;
    private float smoothTelemetryCpuTemp = 48f;
    private float smoothTelemetryGpuTemp = 52f;

    // Специфические поля для всплывающих окон описания навыков и интерактивной калибровки координат (v18.11.16)
    private bool showSkillDetailPopup = false;
    private string selectedSkillName = "";
    private string selectedSkillDesc = "";
    private Texture2D selectedSkillIcon = null;
    private string selectedSkillType = "";

    private bool showCastleCalibrationPanel = false;
    private int selectedCalibCastleIdx = 0;

    // Purchase confirmation popup fields (v18.11.24)
    private bool showPurchaseConfirmPopup = false;
    private float confirmPopupOpenedTime = 0f;
    private string confirmItemName = "";
    private int confirmCost = 0;
    private System.Action confirmAction = null;

    // Scroll vectors for columns
    private Vector2 barracksScroll = Vector2.zero;
    private Vector2 forgeScroll = Vector2.zero;
    private Vector2 academyScroll = Vector2.zero;
    private Vector2 statsScroll = Vector2.zero;
    private Vector2 invScroll = Vector2.zero;
    private Vector2 tabsScroll = Vector2.zero;
    private Vector2 overlayLogScroll = Vector2.zero;
    private Vector2 forgeDetailScroll = Vector2.zero;
    private Vector2 townScrollPos = Vector2.zero;

    // Drag and Drop Grid state
    private bool isDraggingUnit = false;
    private int dragSourceRow = -1;
    private int dragSourceCol = -1;
    private string dragSourceUnitId = "";

    // Forge popup state
    private bool showForgeDetailPopup = false;
    private int selectedForgeSlotType = 8;
    private int selectedForgeTier = 1;

    // Hover variables for skills (v18.11.21)
    private bool isHoveringSkill = false;
    private string hoveredSkillName = "";
    private string hoveredSkillDesc = "";
    private string hoveredSkillType = "";
    private Texture2D hoveredSkillIcon = null;

    // Cache for GUIStyles to reduce GC allocation & memory usage (v18.11.21)
    private GUIStyle s_walletStyle;
    private GUIStyle s_dStyle;
    private GUIStyle s_nextDayStyle;
    private GUIStyle s_hudBgStyle;
    private GUIStyle s_portraitBtnStyle;
    private GUIStyle s_labelStyle;
    private GUIStyle s_barBgStyle;
    private GUIStyle s_hpStyle;
    private GUIStyle s_textOverBarStyle;
    private GUIStyle s_mpStyle;
    private GUIStyle s_xpStyle;
    private GUIStyle s_winStyle;
    private GUIStyle s_headerStyle;
    private GUIStyle s_colHeaderStyle;
    private GUIStyle s_pointsStyle;
    private GUIStyle s_derivedStyle;
    private GUIStyle s_eqHeaderStyle;
    private GUIStyle s_slotLabelStyle;
    private GUIStyle s_slotEquippedStyle;
    private GUIStyle s_skillsHeaderStyle;
    private GUIStyle s_invHeaderStyle;
    private GUIStyle s_invHelpStyle;
    private GUIStyle s_tabBtnStyle;
    private GUIStyle s_slotGridStyle;

    // Cached GUIStyles for Spy Report (v18.11.25)
    private GUIStyle s_spyTitleStyle;
    private GUIStyle s_spySectionTitleStyle;
    private GUIStyle s_spyDetailLabelStyle;
    private GUIStyle s_spySubHeaderStyle;
    private GUIStyle s_spyCardBgStyle;
    private GUIStyle s_spyBoxLabelStyle;
    private GUIStyle s_spyBarTextStyle;

    // Inventory & Equipment System variables (v18.11.20)
    private int eqBonusSTR = 0;
    private int eqBonusAGI = 0;
    private int eqBonusINT = 0;
    private int eqBonusSTA = 0;

    // Temporary potion stat bonuses (lasts for one turn, resets on AdvanceDay)
    private int tempBonusSTR = 0;
    private int tempBonusAGI = 0;
    private int tempBonusINT = 0;
    private int tempBonusSTA = 0;

    // Use tracking flags for potions per turn (resets on AdvanceDay)
    private bool potionUsedThisTurnHP = false;
    private bool potionUsedThisTurnSTR = false;
    private bool potionUsedThisTurnAGI = false;
    private bool potionUsedThisTurnINT = false;
    private bool potionUsedThisTurnSTA = false;
    private bool isCombatActive = false;
    public bool IsCombatActive => isCombatActive;

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
        int val = PlayerPrefs.GetInt("Player_Inventory_Purchased_Slots", 12);
        if (val < 12)
        {
            val = 12;
            PlayerPrefs.SetInt("Player_Inventory_Purchased_Slots", 12);
            PlayerPrefs.Save();
        }
        return val;
    }

    private void SetPurchasedSlotsCount(int count)
    {
        PlayerPrefs.SetInt("Player_Inventory_Purchased_Slots", Mathf.Clamp(count, 12, 999));
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
        return Mathf.Clamp(purchased + freeSlotsByLevel, 12, 999);
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
            // First time initialization - fill inventory with starter gear depending on selected difficulty
            int difficulty = 2; // Default to Normal (2)
            if (SaveGameSystem.CurrentData != null)
            {
                difficulty = SaveGameSystem.CurrentData.selectedDifficulty;
            }
            else
            {
                difficulty = PlayerPrefs.GetInt("Difficulty", 2);
            }

            // Define all 8 Novice gear items:
            List<InventoryItem> allNoviceGear = new List<InventoryItem>
            {
                new InventoryItem { id = "helmet_bronze_1", name = GetItemName(1, 1, 0), iconType = "helmet", slotType = 1, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "pendant_bronze_1", name = GetItemName(2, 1, 0), iconType = "pendant", slotType = 2, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "pauldrons_bronze_1", name = GetItemName(3, 1, 0), iconType = "pauldrons", slotType = 3, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "armor_bronze_1", name = GetItemName(4, 1, 0), iconType = "armor", slotType = 4, level = 1, count = 1, statBonus = 4 },
                new InventoryItem { id = "ring_bronze_1", name = GetItemName(5, 1, 0), iconType = "ring", slotType = 5, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "belt_bronze_1", name = GetItemName(6, 1, 0), iconType = "belt", slotType = 6, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "boots_bronze_1", name = GetItemName(7, 1, 0), iconType = "boots", slotType = 7, level = 1, count = 1, statBonus = 2 },
                new InventoryItem { id = "weapon_bronze_1", name = GetItemName(8, 1, 0), iconType = "weapon", slotType = 8, level = 1, count = 1, statBonus = 3 }
            };

            List<InventoryItem> gearToGrant = new List<InventoryItem>();

            if (difficulty == 0) // Novice (Новичок) - grant ALL 8 novice gear items
            {
                gearToGrant.AddRange(allNoviceGear);
            }
            else if (difficulty == 1) // Easy (Легко) - grant 3 random novice items
            {
                List<InventoryItem> temp = new List<InventoryItem>(allNoviceGear);
                for (int i = 0; i < 3 && temp.Count > 0; i++)
                {
                    int randIdx = UnityEngine.Random.Range(0, temp.Count);
                    gearToGrant.Add(temp[randIdx]);
                    temp.RemoveAt(randIdx);
                }
            }
            else if (difficulty == 2) // Normal (Нормально) - grant 2 random novice items
            {
                List<InventoryItem> temp = new List<InventoryItem>(allNoviceGear);
                for (int i = 0; i < 2 && temp.Count > 0; i++)
                {
                    int randIdx = UnityEngine.Random.Range(0, temp.Count);
                    gearToGrant.Add(temp[randIdx]);
                    temp.RemoveAt(randIdx);
                }
            }
            else if (difficulty == 3) // Hard (Сложно) - grant 1 random novice item
            {
                List<InventoryItem> temp = new List<InventoryItem>(allNoviceGear);
                if (temp.Count > 0)
                {
                    int randIdx = UnityEngine.Random.Range(0, temp.Count);
                    gearToGrant.Add(temp[randIdx]);
                }
            }
            // If difficulty == 4 (Nightmare/Кошмар) - grant no starting gear items

            int slotIndex = 0;
            foreach (var item in gearToGrant)
            {
                if (slotIndex < 999)
                {
                    playerInventory.items[slotIndex] = item;
                    slotIndex++;
                }
            }

            // Always add default starting potions to support beginner combat/exploration survival
            if (slotIndex < 999) playerInventory.items[slotIndex++] = new InventoryItem { id = "potion_hp_1", name = "Зелье Жизни (Ур.1)", iconType = "hp", slotType = 0, level = 1, count = 3, statBonus = 0 };
            if (slotIndex < 999) playerInventory.items[slotIndex++] = new InventoryItem { id = "potion_str_1", name = "Зелье Силы (Ур.1)", iconType = "str", slotType = 0, level = 1, count = 1, statBonus = 0 };
            if (slotIndex < 999) playerInventory.items[slotIndex++] = new InventoryItem { id = "potion_def_1", name = "Зелье Защиты (Ур.1)", iconType = "def", slotType = 0, level = 1, count = 1, statBonus = 0 };

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

    private bool CanAddInventoryItem(string id, int slotType, int level)
    {
        LoadInventory();
        int unlockedCount = Mathf.Min(GetUnlockedSlotsCount(), playerInventory.items.Length);
        
        // Check stackable potion slot
        for (int i = 0; i < unlockedCount; i++)
        {
            if (playerInventory.items[i] != null && playerInventory.items[i].id == id && playerInventory.items[i].level == level && slotType == 0)
            {
                return true;
            }
        }
        
        // Check empty slot
        for (int i = 0; i < unlockedCount; i++)
        {
            if (playerInventory.items[i] == null || string.IsNullOrEmpty(playerInventory.items[i].id))
            {
                return true;
            }
        }
        return false;
    }

    private bool AddInventoryItem(string id, string name, string iconType, int slotType, int level, int statBonus)
    {
        LoadInventory();
        int unlockedCount = Mathf.Min(GetUnlockedSlotsCount(), playerInventory.items.Length);
        
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
        int unlockedCount = Mathf.Min(GetUnlockedSlotsCount(), playerInventory.items.Length);
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
            RecalculateStats();
            SaveGameSystem.Save(0);
            string itemName = GetLocalizedItemName(itemToUnequip, curLang);
            ShowFeedback(curLang == 0 ? $"{itemName} снят и помещен в инвентарь!" : $"{itemName} unequipped into inventory!");
        }
        else
        {
            ShowFeedback(curLang == 0 ? "Инвентарь полон! Некуда положить снятый предмет. Разблокируйте новые ячейки." : "Inventory full! Unlock more slots to unequip.");
        }
    }

    private int GetRequiredCastleLevelForPotion(int potionLvl)
    {
        if (potionLvl == 1 || potionLvl == 2) return 1;
        if (potionLvl == 3) return 2;
        if (potionLvl == 4 || potionLvl == 5) return 3;
        if (potionLvl == 6) return 4;
        if (potionLvl == 7) return 5;
        if (potionLvl == 8 || potionLvl == 9) return 6;
        if (potionLvl == 10) return 7;
        return 1;
    }

    public string GetText(string ru, string en, string kr = "", string ch = "")
    {
        int curLang = Translator.LanguageID;
        if (curLang == 0) return ru;
        if (curLang == 7) return !string.IsNullOrEmpty(kr) ? kr : en;
        if (curLang == 8) return !string.IsNullOrEmpty(ch) ? ch : en;
        return en;
    }

    public string GetText9(string ru, string en, string de, string fr, string es, string pt, string ja, string ko, string zh)
    {
        int curLang = Translator.LanguageID;
        switch (curLang)
        {
            case 0: return ru;
            case 1: return en;
            case 2: return !string.IsNullOrEmpty(de) ? de : en;
            case 3: return !string.IsNullOrEmpty(fr) ? fr : en;
            case 4: return !string.IsNullOrEmpty(es) ? es : en;
            case 5: return !string.IsNullOrEmpty(pt) ? pt : en;
            case 6: return !string.IsNullOrEmpty(ja) ? ja : en;
            case 7: return !string.IsNullOrEmpty(ko) ? ko : en;
            case 8: return !string.IsNullOrEmpty(zh) ? zh : en;
            default: return en;
        }
    }

    private int GetPotionValueForLevel(int level, bool isHP)
    {
        if (isHP)
        {
            switch (level)
            {
                case 1: return 30;
                case 2: return 60;
                case 3: return 100;
                case 4: return 150;
                case 5: return 220;
                case 6: return 300;
                case 7: return 400;
                case 8: return 550;
                case 9: return 750;
                case 10: return 1100;
                default: return 30;
            }
        }
        else
        {
            switch (level)
            {
                case 1: return 2;
                case 2: return 4;
                case 3: return 6;
                case 4: return 9;
                case 5: return 12;
                case 6: return 16;
                case 7: return 20;
                case 8: return 25;
                case 9: return 32;
                case 10: return 45;
                default: return 2;
            }
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
            else if (item.id.Contains("int"))
            {
                if (lang == 0) nameBase = "Зелье Интеллекта";
                else if (lang == 8) nameBase = "智力药水";
                else if (lang == 7) nameBase = "지능의 물약";
                else nameBase = "Intelligence Potion";
            }
            else if (item.id.Contains("agi"))
            {
                if (lang == 0) nameBase = "Зелье Ловкости";
                else if (lang == 8) nameBase = "敏捷药水";
                else if (lang == 7) nameBase = "민첩의 물약";
                else nameBase = "Agility Potion";
            }
            else if (item.id.Contains("sta") || item.id.Contains("def"))
            {
                if (lang == 0) nameBase = "Зелье Выносливости";
                else if (lang == 8) nameBase = "耐力药水";
                else if (lang == 7) nameBase = "지구력의 물약";
                else nameBase = "Stamina Potion";
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
            RecalculateStats();
            SaveGameSystem.Save(0);
        }
        else if (item.slotType == 0)
        {
            SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
            string itemName = GetLocalizedItemName(item, curLang);
            
            if (item.id.Contains("hp"))
            {
                if (potionUsedThisTurnHP)
                {
                    ShowFeedback(GetText("Вы уже выпили зелье Жизни в этот ход!", "You have already consumed a Health potion this turn!", "이미 이번 턴에 생명력 물약을 복용했습니다!", "您在这一回合已经使用了生命药水！"));
                    return;
                }
                potionUsedThisTurnHP = true;
                int hpBoost = GetPotionValueForLevel(item.level, true);
                int staminaEquivalent = hpBoost / 10;
                tempBonusSTA += staminaEquivalent;
                
                string feedbackMsg = "";
                if (curLang == 0) feedbackMsg = $"Вы выпили {itemName}. Макс. ОЗ (HP) временно увеличено на +{hpBoost} до конца текущего дня (без мгновенного восстановления ОЗ)!";
                else if (curLang == 8) feedbackMsg = $"使用了 {itemName}。最大生命值 (HP) 临时增加 +{hpBoost}，持续到当天结束（不进行即时治疗）！";
                else if (curLang == 7) feedbackMsg = $"{itemName}을(를) 복용했습니다. 하루가 끝날 때까지 최대 HP가 일시적으로 +{hpBoost}만큼 증가합니다 (즉시 회복 없음)!";
                else if (curLang == 2) feedbackMsg = $"Sie haben {itemName} getrunken. Maximale LP (HP) vorübergehend um +{hpBoost} erhöht, bis zum Ende des Tages (ohne sofortige Heilung)!";
                else if (curLang == 3) feedbackMsg = $"Vous avez bu {itemName}. PV max temporairement augmentés de +{hpBoost} jusqu'à la fin de la journée (sans soins instantanés) !";
                else if (curLang == 4) feedbackMsg = $"Has bebido {itemName}. ¡PS máx. (HP) aumentados temporalmente en +{hpBoost} hasta el final del día (sin curación instantánea)!";
                else if (curLang == 5) feedbackMsg = $"Você bebeu {itemName}. PV máx. (HP) temporariamente aumentado em +{hpBoost} até o final do dia (sem cura instantânea)!";
                else if (curLang == 6) feedbackMsg = $"{itemName}を服用しました。一日の終わりまで最大HPが一時的に +{hpBoost} 増加します（即時回復なし）！";
                else feedbackMsg = $"Consumed {itemName}. Max HP temporarily increased by +{hpBoost} until the end of the day (without instant healing)!";
                
                ShowFeedback(feedbackMsg);
            }
            else if (item.id.Contains("str"))
            {
                if (potionUsedThisTurnSTR)
                {
                    ShowFeedback(GetText("Вы уже выпили зелье Силы в этот ход!", "You have already consumed a Strength potion this turn!", "이미 이번 턴에 힘의 물약을 복용했습니다!", "您在这一回合已经使用了力量药水！"));
                    return;
                }
                potionUsedThisTurnSTR = true;
                int statBoost = GetPotionValueForLevel(item.level, false);
                tempBonusSTR += statBoost;
                ShowFeedback(GetText($"Вы выпили {itemName}. Сила временно увеличена на +{statBoost}!", $"Consumed {itemName}. Strength temporarily increased by +{statBoost}!", $"{itemName}을(를) 복용했습니다. 힘이 일시적으로 +{statBoost}만큼 증가했습니다!", $"使用了 {itemName}。力量临时增加 +{statBoost}！"));
            }
            else if (item.id.Contains("int"))
            {
                if (potionUsedThisTurnINT)
                {
                    ShowFeedback(GetText("Вы уже выпили зелье Интеллекта в этот ход!", "You have already consumed an Intelligence potion this turn!", "이미 이번 턴에 지능의 물약을 복용했습니다!", "您在这一回合已经使用了智力药水！"));
                    return;
                }
                potionUsedThisTurnINT = true;
                int statBoost = GetPotionValueForLevel(item.level, false);
                tempBonusINT += statBoost;
                ShowFeedback(GetText($"Вы выпили {itemName}. Интеллект временно увеличен на +{statBoost}!", $"Consumed {itemName}. Intelligence temporarily increased by +{statBoost}!", $"{itemName}을(를) 복용했습니다. 지능이 일시적으로 +{statBoost}만큼 증가했습니다!", $"使用了 {itemName}。智力临时增加 +{statBoost}！"));
            }
            else if (item.id.Contains("agi"))
            {
                if (potionUsedThisTurnAGI)
                {
                    ShowFeedback(GetText("Вы уже выпили зелье Ловкости в этот ход!", "You have already consumed an Agility potion this turn!", "이미 이번 턴에 민첩의 물약을 복용했습니다!", "您在这一回合已经使用了敏捷药水！"));
                    return;
                }
                potionUsedThisTurnAGI = true;
                int statBoost = GetPotionValueForLevel(item.level, false);
                tempBonusAGI += statBoost;
                ShowFeedback(GetText($"Вы выпили {itemName}. Ловкость временно увеличена на +{statBoost}!", $"Consumed {itemName}. Agility temporarily increased by +{statBoost}!", $"{itemName}을(를) 복용했습니다. 민첩이 일시적으로 +{statBoost}만큼 증가했습니다!", $"使用了 {itemName}。敏捷临时增加 +{statBoost}！"));
            }
            else if (item.id.Contains("sta") || item.id.Contains("def"))
            {
                if (potionUsedThisTurnSTA)
                {
                    ShowFeedback(GetText("Вы уже выпили зелье Выносливости в этот ход!", "You have already consumed a Stamina potion this turn!", "이미 이번 턴에 체력의 물약을 복용했습니다!", "您在这一回合已经使用了耐力药水！"));
                    return;
                }
                potionUsedThisTurnSTA = true;
                int statBoost = GetPotionValueForLevel(item.level, false);
                tempBonusSTA += statBoost;
                ShowFeedback(GetText($"Вы выпили {itemName}. Выносливость временно увеличена на +{statBoost}!", $"Consumed {itemName}. Stamina temporarily increased by +{statBoost}!", $"{itemName}을(를) 복용했습니다. 체력이 일시적으로 +{statBoost}만큼 증가했습니다!", $"使用了 {itemName}。耐力临时增加 +{statBoost}！"));
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

    public void ResetAfterBattle()
    {
        // 1. Сброс временных бонусов от зелий
        tempBonusSTR = 0;
        tempBonusAGI = 0;
        tempBonusINT = 0;
        tempBonusSTA = 0;

        potionUsedThisTurnHP = false;
        potionUsedThisTurnSTR = false;
        potionUsedThisTurnAGI = false;
        potionUsedThisTurnINT = false;
        potionUsedThisTurnSTA = false;

        // 2. Полное восстановление здоровья основного героя
        if (SaveGameSystem.CurrentData != null)
        {
            float maxHp = (SaveGameSystem.CurrentData.stamina + eqBonusSTA) * 10f;
            SaveGameSystem.CurrentData.currentHealth = maxHp;
            SaveGameSystem.CurrentData.maxHealth = maxHp;
            SaveGameSystem.Save(0);
        }

        // 3. Полное восстановление здоровья у всех активных UnitBase на сцене и очистка баффов/дебаффов
        foreach (var unit in FindObjectsByType<UnitBase>(FindObjectsSortMode.None))
        {
            if (unit != null)
            {
                unit.InitializeStats(); // Полное восстановление ОЗ/ОМ к максимальным
            }
        }
    }

    private void ResetNonPlayerProgression()
    {
        // 1. Сброс простых героев (companions / ArcherHero, WarriorHero, MageHero)
        string[] companion_ids = { "ArcherHero", "WarriorHero", "MageHero" };
        foreach (var compId in companion_ids)
        {
            // Сбрасываем уровни до 1
            PlayerPrefs.SetInt("Companion_Lvl_" + compId, 1);
            PlayerPrefs.SetInt("Companion_XP_" + compId, 0);

            // Сбрасываем количество нанятых героев во всех замках/зонах до 0 (надо покупать заново)
            for (int zone = 0; zone < castles.Count; zone++)
            {
                PlayerPrefs.SetInt("Player_HiredCount_" + compId + "_Zone_" + zone, 0);
            }
        }

        // 2. Сброс воинов/когорт
        string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
        foreach (var troopId in troop_ids)
        {
            for (int zone = 0; zone < castles.Count; zone++)
            {
                PlayerPrefs.SetInt("Player_Unit_" + troopId + "_Zone_" + zone, 0);
            }
        }

        // Сброс ранга воинов
        PlayerPrefs.SetInt("Player_ArmyUnit_Rank", 1);

        PlayerPrefs.Save();
    }

    private void TriggerContinentClearedTransition()
    {
        int curLang = Translator.LanguageID;

        // 1. Оповещаем игрока сообщением
        string clearedMsg = GetText(
            "🎉 ВЕЛИКАЯ ПОБЕДА! Все замки на континенте захвачены! Вы переходите на следующий континент!",
            "🎉 GRAND VICTORY! All castles on the continent conquered! You are transitioning to the next continent!",
            "🎉 대승리! 대륙의 모든 성채를 정복했습니다! 다음 대륙으로 이동합니다!",
            "🎉 伟大的胜利！大陆上的所有城堡都已被占领！您正在前往下一个大陆！"
        );
        ShowFeedback(clearedMsg);

        // 2. Обнуляем воинов и простых героев до начального уровня (прокачка сбрасывается, покупать заново)
        ResetNonPlayerProgression();

        // 3. Сбрасываем владение замками (кроме стартового региона игрока) для нового континента
        int actualPlayerRegion = GetActualRegionIndexFromLanding(PlayerPrefs.GetInt("LandedZoneIndex", -1));
        for (int i = 0; i < castles.Count; i++)
        {
            if (i == actualPlayerRegion)
            {
                castles[i].owner = "Player";
                castles[i].level = 1; // Стартовый уровень
                PlayerPrefs.SetString("Castle_Owner_" + i, "Player");
                PlayerPrefs.SetInt("Castle_Level_" + i, 1);
            }
            else
            {
                castles[i].owner = "Enemy";
                castles[i].level = 1;
                PlayerPrefs.SetString("Castle_Owner_" + i, "Enemy");
                PlayerPrefs.SetInt("Castle_Level_" + i, 1);
                PlayerPrefs.SetInt("Castle_AI_Troops_" + i, 20); // Сброс силы гарнизона
            }
        }

        // Сбрасываем день континента
        currentDay = 1;
        PlayerPrefs.SetInt("Fate_Current_Day", 1);

        // Увеличиваем индекс текущего континента
        int continentCount = PlayerPrefs.GetInt("Fate_Current_Continent", 1);
        PlayerPrefs.SetInt("Fate_Current_Continent", continentCount + 1);

        // Перерисовываем регионы и респавним замки
        if (LandingPositionManager.Instance != null)
        {
            LandingPositionManager.Instance.RepaintRegionsBasedOnLanding(0);
        }
        SpawnAllCastles();

        // Сохраняем состояние
        PlayerPrefs.Save();
        SaveGameSystem.Save(0);
    }

    public void GetItemStats(int slotType, int tier, out int str, out int agi, out int intel, out int sta, string overrideClass = null)
    {
        str = 0; agi = 0; intel = 0; sta = 0;
        
        string cl = "warrior";
        if (!string.IsNullOrEmpty(overrideClass))
        {
            cl = overrideClass.ToLower();
        }
        else if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            cl = SaveGameSystem.CurrentData.characterClass.ToLower();
        }

        bool isWarrior = cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("воин");
        bool isArcher = cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("стрелок");
        bool isMage = cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг");

        if (slotType == 8) // Weapon is heavily customized by class!
        {
            if (isWarrior) str = tier * 3;
            else if (isArcher) agi = tier * 3;
            else if (isMage) intel = tier * 3;
            else str = tier * 3; // fallback
        }
        else if (slotType == 1) // Helmet
        {
            if (isWarrior) sta = tier * 2;
            else if (isArcher) agi = tier * 2;
            else if (isMage) intel = tier * 2;
            else sta = tier * 2;
        }
        else if (slotType == 2) // Amulet / Neck
        {
            if (isWarrior) str = tier * 2;
            else if (isArcher) agi = tier * 2;
            else if (isMage) intel = tier * 2;
            else intel = tier * 2;
        }
        else if (slotType == 3) // Pauldrons / Shoulders
        {
            if (isWarrior) { str = tier; sta = tier; }
            else if (isArcher) { agi = tier; sta = tier; }
            else if (isMage) { intel = tier; sta = tier; }
            else { str = tier; sta = tier; }
        }
        else if (slotType == 4) // Armor / Chest
        {
            if (isWarrior) { sta = tier * 3; }
            else if (isArcher) { sta = tier * 2; agi = tier; }
            else if (isMage) { sta = tier * 2; intel = tier; }
            else { sta = tier * 3; }
        }
        else if (slotType == 5) // Ring
        {
            if (isWarrior) { str = tier; sta = tier; }
            else if (isArcher) { agi = tier; intel = tier; }
            else if (isMage) { intel = tier; agi = tier; }
            else { intel = tier; agi = tier; }
        }
        else if (slotType == 6) // Belt
        {
            if (isWarrior) { str = tier; sta = tier; }
            else if (isArcher) { agi = tier; sta = tier; }
            else if (isMage) { intel = tier; sta = tier; }
            else { sta = tier; agi = tier; }
        }
        else if (slotType == 7) // Boots
        {
            if (isWarrior) { str = tier; sta = tier; }
            else if (isArcher) { agi = tier * 2; }
            else if (isMage) { agi = tier; intel = tier; }
            else { agi = tier * 2; }
        }
    }

    public string GetItemPrompt(int slotType, int tier, string cl)
    {
        bool isWarrior = cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("воин");
        bool isArcher = cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("стрелок");
        bool isMage = cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг");

        string quality = "fantasy metallic";
        if (tier == 6) quality = "legendary golden glowing";
        else if (tier >= 4) quality = "epic magical purple glowing";
        else if (tier >= 2) quality = "rare mystical blue glowing";

        switch (slotType)
        {
            case 1:
                if (isWarrior) return $"High precision game icon of a heavy {quality} barbarian horned warrior helmet, white background.";
                if (isArcher) return $"High precision game icon of an agile {quality} elven ranger leather hood with green gemstones, white background.";
                return $"High precision game icon of a mystical {quality} wizard circlet with glowing ruby, white background.";
            case 2:
                if (isWarrior) return $"High precision game icon of a massive {quality} iron chain with lion medallion, white background.";
                if (isArcher) return $"High precision game icon of a lightweight {quality} emerald falcon feather pendant, white background.";
                return $"High precision game icon of a glowing {quality} sapphire orb amulet, white background.";
            case 3:
                if (isWarrior) return $"High precision game icon of heavy {quality} spiked steel plate pauldrons, white background.";
                if (isArcher) return $"High precision game icon of sleek {quality} leather rangers shoulder guards, white background.";
                return $"High precision game icon of glowing {quality} runic arcane mage pauldrons, white background.";
            case 4:
                if (isWarrior) return $"High precision game icon of heavy {quality} plate chestplate armor, white background.";
                if (isArcher) return $"High precision game icon of reinforced {quality} scale mail hunter chest armor, white background.";
                return $"High precision game icon of embroidered {quality} silk wizard robes, white background.";
            case 5:
                if (isWarrior) return $"High precision game icon of a heavy {quality} brass signet warrior ring, white background.";
                if (isArcher) return $"High precision game icon of a swift {quality} jade feather archer ring, white background.";
                return $"High precision game icon of a glowing {quality} amethyst mana wizard ring, white background.";
            case 6:
                if (isWarrior) return $"High precision game icon of a heavy {quality} leather belt with colossal metal buckle, white background.";
                if (isArcher) return $"High precision game icon of a sleek {quality} ranger belt with dual arrow pouches, white background.";
                return $"High precision game icon of a glowing {quality} mana-weaver sash with runic patterns, white background.";
            case 7:
                if (isWarrior) return $"High precision game icon of heavy {quality} steel plated soldier sabatons boots, white background.";
                if (isArcher) return $"High precision game icon of soft {quality} silent leather tracker boots, white background.";
                return $"High precision game icon of glowing {quality} runic boots with wings embroidery, white background.";
            case 8:
                if (isWarrior) return $"High precision game icon of a giant {quality} two-handed steel greatsword, white background.";
                if (isArcher) return $"High precision game icon of a composite {quality} recurve bow, white background.";
                return $"High precision game icon of a glowing {quality} crystal spell staff, white background.";
            default:
                return $"High precision game icon of a {quality} mystical artifact, white background.";
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
                int itemStr, itemAgi, itemInt, itemSta;
                GetItemStats(i, tier, out itemStr, out itemAgi, out itemInt, out itemSta);
                eqBonusSTR += itemStr;
                eqBonusAGI += itemAgi;
                eqBonusINT += itemInt;
                eqBonusSTA += itemSta;
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

    private string GetItemName(int slotType, int tier, int lang, string overrideClass = null)
    {
        string cl = "warrior";
        if (!string.IsNullOrEmpty(overrideClass))
        {
            cl = overrideClass.ToLower();
        }
        else if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            cl = SaveGameSystem.CurrentData.characterClass.ToLower();
        }

        string[] weaponNamesRU = new string[] { "Бронзовый Меч", "Стальной Клинок", "Мифриловый Меч", "Кристальный Клинок", "Звездный Клинок", "Меч Зенита v18" };
        string[] weaponNamesEN = new string[] { "Bronze Sword", "Steel Blade", "Mithril Claymore", "Crystal Scepter", "Astral Edge", "Zenith Slayer" };

        if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("стрелок"))
        {
            weaponNamesRU = new string[] { "Бронзовый Лук", "Стальной Лук", "Мифриловый Лук", "Кристальный Лук", "Звездный Лук", "Лук Зенита v18" };
            weaponNamesEN = new string[] { "Bronze Bow", "Steel Bow", "Mithril Bow", "Crystal Bow", "Astral Bow", "Zenith Bow" };
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг"))
        {
            weaponNamesRU = new string[] { "Бронзовый Посох", "Стальной Посох", "Мифриловый Посох", "Кристальный Посох", "Звездный Посох", "Посох Зенита v18" };
            weaponNamesEN = new string[] { "Bronze Staff", "Steel Staff", "Mithril Staff", "Crystal Staff", "Astral Staff", "Zenith Staff" };
        }

        string[][] slotPrefixesRU = new string[][] {
            new string[] { "Бронза", "Сталь", "Мифрил", "Кристалл", "Космос", "Легенда" }, // 0
            new string[] { "Бронзовый Шлем", "Стальной Шлем", "Мифриловый Шлем", "Кристальный Шлем", "Звездный Венец", "Шлем Зенита v18" }, // 1
            new string[] { "Бронзовый Амулет", "Стальной Амулет", "Мифриловый Амулет", "Кристальный Амулет", "Звездный Амулет", "Амулет Зенита v18" }, // 2
            new string[] { "Бронзовые Наплечники", "Стальные Наплечники", "Мифриловые Наплечники", "Кристальные Наплечники", "Звездные Наплечники", "Наплечники Зенита v18" }, // 3
            new string[] { "Бронзовый Доспех", "Стальной Доспех", "Мифриловый Доспех", "Кристальный Доспех", "Звездный Доспех", "Доспех Зенита v18" }, // 4
            new string[] { "Бронзовое Кольцо", "Стальное Кольцо", "Мифриловое Кольцо", "Кристальное Кольцо", "Звездное Кольцо", "Кольцо Зенита v18" }, // 5
            new string[] { "Бронзовый Пояс", "Стальной Пояс", "Мифриловый Пояс", "Кристальный Пояс", "Звездный Пояс", "Пояс Зенита v18" }, // 6
            new string[] { "Бронзовые Сапоги", "Стальные Сапоги", "Мифриловые Сапоги", "Кристальные Сапоги", "Звездные Сапоги", "Сапоги Зенита v18" }, // 7
            weaponNamesRU // 8
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
            weaponNamesEN // 8
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

    public int GetTroopsCountInCastle()
    {
        int count = 0;
        string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
        int zoneIdx = activeDetailsIndex >= 0 ? activeDetailsIndex : 0;
        for (int i = 0; i < troop_ids.Length; i++)
        {
            count += GetUnitCount(troop_ids[i], zoneIdx);
        }
        return count;
    }

    public int GetTroopCapacity(int lvl)
    {
        return 30 * Mathf.Max(1, lvl);
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

        // --- RPG CALCULATIONS WITH HERO STATS & POTION BUFFS ---
        int mainHeroPower = 0;
        if (SaveGameSystem.CurrentData != null)
        {
            mainHeroPower = (SaveGameSystem.CurrentData.playerLevel * 50) + 
                (SaveGameSystem.CurrentData.strength + eqBonusSTR + tempBonusSTR) * 12 + 
                (SaveGameSystem.CurrentData.agility + eqBonusAGI + tempBonusAGI) * 12 + 
                (SaveGameSystem.CurrentData.intelligence + eqBonusINT + tempBonusINT) * 12 + 
                (SaveGameSystem.CurrentData.stamina + eqBonusSTA + tempBonusSTA) * 12;
        }
        int totalPlayerPower = maxPower + mainHeroPower;

        // --- COMPUTER DRINKING POTIONS BEFORE BATTLE ---
        int enemyPotionsDrunk = 0;
        if (castle.aiPotionsStock > 0)
        {
            enemyPotionsDrunk = Mathf.Min(castle.aiPotionsStock, UnityEngine.Random.Range(1, 3));
            castle.aiPotionsStock -= enemyPotionsDrunk;
            PlayerPrefs.SetInt("Castle_AI_Potions_" + targetZoneIdx, castle.aiPotionsStock);
            PlayerPrefs.Save();
        }

        int enemyHeroPower = (castle.aiCommanderLevel * 50) + (castle.aiArmorTier * 100) + (enemyPotionsDrunk * 150);
        int totalEnemyPower = castle.aiTroopsPower + enemyHeroPower;

        // Print information about the battle preparation
        if (enemyPotionsDrunk > 0)
        {
            string prepMsg = GetText(
                $"🛡️ Перед началом боя вражеский полководец выпил {enemyPotionsDrunk} боевых зелий, получив +{enemyPotionsDrunk * 150} к силе!",
                $"🛡️ Before the battle, the enemy commander drank {enemyPotionsDrunk} combat potions, gaining +{enemyPotionsDrunk * 150} battle power!",
                $"🛡️ 전투가 시작되기 전에 적 사령관이 {enemyPotionsDrunk}개의 영약을 복용하여 전투력이 +{enemyPotionsDrunk * 150} 증가했습니다!",
                $"🛡️ 战斗开始前，敌方领主服用了 {enemyPotionsDrunk} 瓶战斗药水，战斗力提升了 +{enemyPotionsDrunk * 150}！"
            );
            ShowFeedback(prepMsg);
        }

        if (totalPlayerPower >= totalEnemyPower)
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

            string resMsg = GetText(
                $"👑 ПОБЕДА! Мы захватили {castle.nameRU}! Добыча: +{lootGold} 💰. Враг бежал, регион окрасился в цвета Ордена Света! (Сила авангарда: {totalPlayerPower} vs Сила обороны: {totalEnemyPower})",
                $"👑 VICTORY! Conquered {castle.nameEN}! Loot: +{lootGold} 💰. Underneath grounds claim the banner of Light Alliance! (Vanguard Power: {totalPlayerPower} vs Defense Power: {totalEnemyPower})",
                $"👑 승리! {castle.nameKR}을(를) 정복했습니다! 전리품: +{lootGold} 💰. (아군 정예력: {totalPlayerPower} vs 적군 수비력: {totalEnemyPower})",
                $"👑 胜利！我们成功占领了 {castle.nameCH}！战利品: +{lootGold} 💰。 (我方总战力: {totalPlayerPower} vs 敌方防守力: {totalEnemyPower})"
            );
            ShowFeedback(resMsg);

            // Check if all castles are conquered!
            bool allConquered = true;
            for (int i = 0; i < castles.Count; i++)
            {
                if (castles[i].owner != "Player")
                {
                    allConquered = false;
                    break;
                }
            }
            if (allConquered)
            {
                TriggerContinentClearedTransition();
            }
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

            string resMsg = GetText(
                $"❌ ПОРАЖЕНИЕ! Наши силы были разбиты у крепостных стен! Потеряно большинство когорт осады. (Наша сила: {totalPlayerPower} vs Оборона врага: {totalEnemyPower})",
                $"❌ DEFEAT! Defending sentinel forces repelled our siege. Heavy cohort casualties suffered. (Our Power: {totalPlayerPower} vs Enemy Defense: {totalEnemyPower})",
                $"❌ 패배! 적 성벽 앞수비대에 패배했습니다! (아군 전투력: {totalPlayerPower} vs 적 수비력: {totalEnemyPower})",
                $"❌ 失败！我们在城墙下被击退！损失了大部分攻城部队。 (我方总战力: {totalPlayerPower} vs 敌方防守力: {totalEnemyPower})"
            );
            ShowFeedback(resMsg);
        }

        // В конце любой осады полностью восстанавливаем здоровье и сбрасываем баффы
        ResetAfterBattle();
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
            // Показываем зеленый маркер коллайдера только когда открыта панель калибровки замков (для разработчиков)
            bool isVisible = showCastleCalibrationPanel && (i == hoveredCastleIdx || i == selectedCalibCastleIdx);
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

    public void ValidateTroopSkillAssets()
    {
        if (troopSkillAssets == null)
            troopSkillAssets = new List<TroopSkillAsset>();

        string[] defaultTroopIds = new string[] {
            "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer",
            "centaur", "necromancer", "griffin", "overlord", "hydra",
            "dragon", "mountain_bear", "wasteland_serpent"
        };

        foreach (string tId in defaultTroopIds)
        {
            bool exists = false;
            foreach (var asset in troopSkillAssets)
            {
                if (asset != null && asset.troopId == tId)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                TroopSkillAsset newAsset = new TroopSkillAsset();
                newAsset.troopId = tId;
                troopSkillAssets.Add(newAsset);
            }
        }
    }

    public Texture2D GetTroopActiveSkillIcon(string troopId)
    {
        switch (troopId)
        {
            case "warrior": if (skill_warrior_active != null) return skill_warrior_active; break;
            case "archer": if (skill_archer_active != null) return skill_archer_active; break;
            case "mage": if (skill_mage_active != null) return skill_mage_active; break;
            case "paladin": if (skill_paladin_active != null) return skill_paladin_active; break;
            case "cavalry": if (skill_cavalry_active != null) return skill_cavalry_active; break;
            case "cannoneer": if (skill_cannoneer_active != null) return skill_cannoneer_active; break;
            case "centaur": if (skill_centaur_active != null) return skill_centaur_active; break;
            case "necromancer": if (skill_necromancer_active != null) return skill_necromancer_active; break;
            case "griffin": if (skill_griffin_active != null) return skill_griffin_active; break;
            case "overlord": if (skill_overlord_active != null) return skill_overlord_active; break;
            case "hydra": if (skill_hydra_active != null) return skill_hydra_active; break;
            case "dragon": if (skill_dragon_active != null) return skill_dragon_active; break;
            case "mountain_bear": if (skill_mountain_bear_active != null) return skill_mountain_bear_active; break;
            case "wasteland_serpent": if (skill_wasteland_serpent_active != null) return skill_wasteland_serpent_active; break;
            case "ArcherHero": if (skill_hero_archer_active != null) return skill_hero_archer_active; break;
            case "WarriorHero": if (skill_hero_warrior_active != null) return skill_hero_warrior_active; break;
            case "MageHero": if (skill_hero_mage_active != null) return skill_hero_mage_active; break;
        }

        if (troopSkillAssets != null)
        {
            foreach (var asset in troopSkillAssets)
            {
                if (asset != null && asset.troopId == troopId)
                {
                    return asset.activeIcon;
                }
            }
        }
        return null;
    }

    public Texture2D GetTroopPassiveSkillIcon(string troopId, int index)
    {
        switch (troopId)
        {
            case "ArcherHero":
                if (index == 0 && skill_hero_archer_passive1 != null) return skill_hero_archer_passive1;
                if (index == 1 && skill_hero_archer_passive2 != null) return skill_hero_archer_passive2;
                break;
            case "WarriorHero":
                if (index == 0 && skill_hero_warrior_passive1 != null) return skill_hero_warrior_passive1;
                if (index == 1 && skill_hero_warrior_passive2 != null) return skill_hero_warrior_passive2;
                break;
            case "MageHero":
                if (index == 0 && skill_hero_mage_passive1 != null) return skill_hero_mage_passive1;
                if (index == 1 && skill_hero_mage_passive2 != null) return skill_hero_mage_passive2;
                break;
            case "warrior":
                if (index == 0 && skill_warrior_passive1 != null) return skill_warrior_passive1;
                break;
            case "archer":
                if (index == 0 && skill_archer_passive1 != null) return skill_archer_passive1;
                break;
            case "mage":
                if (index == 0 && skill_mage_passive1 != null) return skill_mage_passive1;
                break;
            case "paladin":
                if (index == 0 && skill_paladin_passive1 != null) return skill_paladin_passive1;
                if (index == 1 && skill_paladin_passive2 != null) return skill_paladin_passive2;
                break;
            case "cavalry":
                if (index == 0 && skill_cavalry_passive1 != null) return skill_cavalry_passive1;
                if (index == 1 && skill_cavalry_passive2 != null) return skill_cavalry_passive2;
                break;
            case "cannoneer":
                if (index == 0 && skill_cannoneer_passive1 != null) return skill_cannoneer_passive1;
                if (index == 1 && skill_cannoneer_passive2 != null) return skill_cannoneer_passive2;
                break;
            case "centaur":
                if (index == 0 && skill_centaur_passive1 != null) return skill_centaur_passive1;
                if (index == 1 && skill_centaur_passive2 != null) return skill_centaur_passive2;
                break;
            case "necromancer":
                if (index == 0 && skill_necromancer_passive1 != null) return skill_necromancer_passive1;
                if (index == 1 && skill_necromancer_passive2 != null) return skill_necromancer_passive2;
                break;
            case "griffin":
                if (index == 0 && skill_griffin_passive1 != null) return skill_griffin_passive1;
                if (index == 1 && skill_griffin_passive2 != null) return skill_griffin_passive2;
                if (index == 2 && skill_griffin_passive3 != null) return skill_griffin_passive3;
                break;
            case "overlord":
                if (index == 0 && skill_overlord_passive1 != null) return skill_overlord_passive1;
                if (index == 1 && skill_overlord_passive2 != null) return skill_overlord_passive2;
                if (index == 2 && skill_overlord_passive3 != null) return skill_overlord_passive3;
                break;
            case "hydra":
                if (index == 0 && skill_hydra_passive1 != null) return skill_hydra_passive1;
                if (index == 1 && skill_hydra_passive2 != null) return skill_hydra_passive2;
                if (index == 2 && skill_hydra_passive3 != null) return skill_hydra_passive3;
                break;
            case "dragon":
                if (index == 0 && skill_dragon_passive1 != null) return skill_dragon_passive1;
                if (index == 1 && skill_dragon_passive2 != null) return skill_dragon_passive2;
                if (index == 2 && skill_dragon_passive3 != null) return skill_dragon_passive3;
                break;
            case "mountain_bear":
                if (index == 0 && skill_mountain_bear_passive1 != null) return skill_mountain_bear_passive1;
                if (index == 1 && skill_mountain_bear_passive2 != null) return skill_mountain_bear_passive2;
                if (index == 2 && skill_mountain_bear_passive3 != null) return skill_mountain_bear_passive3;
                break;
            case "wasteland_serpent":
                if (index == 0 && skill_wasteland_serpent_passive1 != null) return skill_wasteland_serpent_passive1;
                if (index == 1 && skill_wasteland_serpent_passive2 != null) return skill_wasteland_serpent_passive2;
                if (index == 2 && skill_wasteland_serpent_passive3 != null) return skill_wasteland_serpent_passive3;
                break;
        }

        if (troopSkillAssets != null)
        {
            foreach (var asset in troopSkillAssets)
            {
                if (asset != null && asset.troopId == troopId)
                {
                    if (index == 0) return asset.passiveIcon1;
                    if (index == 1) return asset.passiveIcon2;
                    if (index == 2) return asset.passiveIcon3;
                }
            }
        }
        return null;
    }

    private void Awake()
    {
        InitializeCachedTextures();
        ValidateTroopSkillAssets();
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
        if (landedZoneIndex == 3)
        {
            return 8; // Грозовые Кряжи (всегда принудительно 8 регион по просьбе игрока)
        }
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
                // Debug.Log($"[CASTLE MGR PROXIMITY] Динамически сопоставили landedZoneIndex={landedZoneIndex} с ближайшим регионом Region_{closest:D2} у точки {pt.spawnAnchor.name}");
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
            // Debug.Log($"[CASTLE MGR PROXIMITY] Динамически сопоставили по имени объекта {foundObj.name} к ближайшему региону Region_{closest:D2}");
            return closest;
        }

        // 3. Скорректированный статический фолбек на случай полного отсутствия объектов спавна в сцене
        switch (landedZoneIndex)
        {
            case 0: return 11;  // Кровавые Пустоши (Region_11)
            case 1: return 6;   // Ледяной Пик (Region_06)
            case 2: return 8;   // Древние Руины (Region_08)
            case 3: return 8;   // Грозовые Кряжи (Region_08 принудительно)
            default: return 8;
        }
    }

    /// <summary>
    /// Возвращает согласованный цвет региона и замка на тактической карте для предотвращения цветовых нестыковок (v18.11.24)
    /// </summary>
    public static Color GetRegionColor(int regionIndex, int actualPlayerRegion, string owner = "")
    {
        string savedOwner = owner;
        if (string.IsNullOrEmpty(savedOwner))
        {
            string defaultOwner = (regionIndex == actualPlayerRegion) ? "Player" : "Enemy";
            savedOwner = PlayerPrefs.GetString("Castle_Owner_" + regionIndex, defaultOwner);
        }

        if (savedOwner == "Player" || regionIndex == actualPlayerRegion)
        {
            return new Color(0.12f, 0.58f, 0.95f, 1.0f); // Zenith Neon Blue (Игрок)
        }

        switch (regionIndex)
        {
            case 3: // Zenith Sanctuary (Святилище Зенита) - Зеленый Замок и Зеленый квадрат (Лесные Жители)
            case 8: // Ancient Ruins (Древние Руины)
            case 9: // Forest Dwellers (Лесные Жители)
                return new Color(0.12f, 0.75f, 0.25f, 1.0f); // Изумрудный зеленый

            case 1:
            case 5:
            case 11: // Bloody Wastelands (Кровавые Пустоши)
                return new Color(0.85f, 0.15f, 0.12f, 1.0f); // Насыщенный красный

            case 0:
            case 2:
            case 4:
            case 6: // Ice Peak (Ледяной Пик) - Нейтральный
            case 7:
            case 10:
            default: // Нейтральные территории
                return new Color(0.42f, 0.45f, 0.48f, 1.0f); // Каменистый серый
        }
    }

    private void Start()
    {
        EventHub.OnCombatEnd += HandleCombatEndEvent;
        EventHub.OnCombatStart += HandleCombatStartEvent;
        // [CRITICAL SAVE SYNC] Синхронизируем и загружаем активный слот сохранений игрока при запуске сцены континента
        int activeSlot = PlayerPrefs.GetInt("Active_Save_Slot", 0);
        SaveGameSystem.Load(activeSlot, false);

        LoadAILogs();

        isContinentGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1 && PlayerPrefs.GetInt("LandedZoneIndex", -1) != -1;
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

        // Fully restore player health and synchronize maxHealth on initialization (v18.11.24)
        if (SaveGameSystem.CurrentData != null)
        {
            float maxHp = (SaveGameSystem.CurrentData.stamina + eqBonusSTA) * 10f;
            SaveGameSystem.CurrentData.maxHealth = maxHp;
            SaveGameSystem.CurrentData.currentHealth = maxHp; // Полное восстановление при старте
            SaveGameSystem.Save(activeSlot);
        }
    }

    private void HandleCombatStartEvent(System.Collections.Generic.List<UnitBase> participants)
    {
        isCombatActive = true;
    }

    private void HandleCombatEndEvent(int winner)
    {
        isCombatActive = false;
        ResetAfterBattle();
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
        PlayerPrefs.SetInt("Fate_Current_Continent", 1);
        
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

        // Удаляем сохраненный инвентарь и экипировку, чтобы выдать стартовое снаряжение заново по сложности!
        PlayerPrefs.DeleteKey("Player_Inventory_JSON_v18");
        PlayerPrefs.DeleteKey("Player_Equipment_JSON_v18");
        PlayerPrefs.Save();

        // Сразу инициализируем чистый инвентарь и экипировку на основе новой сложности
        LoadInventory();
        LoadEquipment();

        // Динамический расчет стартового золота на основе сохраненной в Slot 0 / активном слоте сложности
        int activeSlot = PlayerPrefs.GetInt("Active_Save_Slot", 0);
        SaveGameSystem.Load(activeSlot, false);

        int selectedDifficulty = 2; // По умолчанию Нормально
        if (SaveGameSystem.CurrentData != null)
        {
            selectedDifficulty = SaveGameSystem.CurrentData.selectedDifficulty;
        }

        int difficultyStartingGold = 500;
        switch (selectedDifficulty)
        {
            case 0: difficultyStartingGold = 1000; break; // Новичок (+1000)
            case 1: difficultyStartingGold = 800;  break; // Легко (+800)
            case 2: difficultyStartingGold = 500;  break; // Нормально (+500)
            case 3: difficultyStartingGold = 300;  break; // Сложно (+300)
            case 4: difficultyStartingGold = 100;  break; // Кошмар (+100)
            default: difficultyStartingGold = 500; break;
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

        if (clickCooldown > 0f)
        {
            clickCooldown -= Time.deltaTime;
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
        if (clickCooldown > 0f) return;

        // Avoid clicking 3D structures if continent gameplay is not fully active, town view is active, day overlay is showing, or stats panel/details/popups are open
        if (!isContinentGameplayActive || isTownViewActive || showNewDayOverlay || showStatsPanel || isDetailsOpen || showTroopDetailPopup || showForgeDetailPopup || showCastleCalibrationPanel || showSpyReportPopup || showPurchaseConfirmPopup) return;

        if (WasLeftMouseButtonClicked())
        {
            if (Camera.main != null)
            {
                Vector2 mousePos = GetMousePosition();
                Vector2 guiMouse = new Vector2(mousePos.x, Screen.height - mousePos.y);

                // Block clicks in top-left HUD area to prevent clicking 3D structures underneath
                Rect hudRect = new Rect(10f, 10f, 360f, 130f);
                if (hudRect.Contains(guiMouse))
                {
                    return;
                }
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
                    guiMouse = new Vector2(mousePos.x, Screen.height - mousePos.y);
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
            case 6: return 7000; // to upgrade level 6 to 7
            default: return 9999;
        }
    }

    public int GetGoldIncome(int level)
    {
        switch (level)
        {
            case 1: return 100;
            case 2: return 250;
            case 3: return 500;
            case 4: return 900;
            case 5: return 1500;
            case 6: return 3000;
            case 7: return 5000;
            default: return 100;
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
            else if (i == 6 || i == 8 || i == 11)
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
            else // 0, 3, 4, 5, 9
            {
                string baseRU = "Сумрачный Лес";
                string baseEN = "Gloomwood Forest";
                string baseCH = "幽暗密林";
                string baseKR = "어둠의 숲";

                if (i == 3) { baseRU = GetLandingBaseNameRU(3); baseEN = GetLandingBaseNameEN(3); baseCH = GetLandingBaseNameCH(3); baseKR = GetLandingBaseNameKR(3); }
                else if (i == 4) { baseRU = "Лесные Топи"; baseEN = "Forest Swamps"; baseCH = "森林沼泽"; baseKR = "숲의 늪지"; }
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
        // Если геймплей на континенте еще не активен и идет начальный вводный диалог (шаги 0-7),
        // мы НЕ должны спавнить 3D-замки на сцене. Но если они уже выбрали точку высадки (шаг >= 8),
        // замки должны быть заспавнены!
        if (!isContinentGameplayActive && DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive && DialogueSystem_Manager.Instance.CurrentLineIndex < 8)
        {
            Debug.Log("[CASTLE MGR] Пропуск спавна 3D-замков: Идет начальный вводный диалог (выбор высадки).");
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
            
            // Расцветка замков на тактической карте согласно их фракциям и владельцам через единую функцию (v18.11.24):
            Color factionColor = GetRegionColor(i, actualPlayerRegion, castle.owner);
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
                else if (castle.level == 6)
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
                else
                {
                    // LEVEL 7: Высший Космический Пантеон Судьбы (Три парящих кольца, мега-бастионы и светящиеся призмы)
                    GameObject basePantheon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(basePantheon.GetComponent<BoxCollider>());
                    basePantheon.transform.SetParent(root.transform);
                    basePantheon.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                    basePantheon.transform.localScale = new Vector3(3.6f, 5.0f, 3.6f);
                    basePantheon.GetComponent<Renderer>().material = castleMat;

                    GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(ring.GetComponent<BoxCollider>());
                    ring.transform.SetParent(root.transform);
                    ring.transform.localPosition = new Vector3(0f, 8.5f, 0f);
                    ring.transform.localScale = new Vector3(3.0f, 0.3f, 3.0f);
                    
                    Material glowM = new Material(urpShader);
                    glowM.color = castle.owner == "Player" ? new Color(0.12f, 0.88f, 1.0f, 1.0f) : new Color(1.0f, 0.15f, 0.45f, 1.0f);
                    if (glowM.HasProperty("_EmissionColor")) glowM.SetColor("_EmissionColor", glowM.color * 5.0f);
                    ring.GetComponent<Renderer>().material = glowM;

                    GameObject prism = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(prism.GetComponent<BoxCollider>());
                    prism.transform.SetParent(root.transform);
                    prism.transform.localPosition = new Vector3(0f, 8.5f, 0f);
                    prism.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
                    prism.GetComponent<Renderer>().material = glowM;
                }
            }

            // Сохраняем ссылку на корень визуализации
            castle.visualRoot = root;
        }
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
        if (data.playerLevel >= 9999)
        {
            data.playerLevel = 9999;
            data.currentXP = 0;
            RecalculateStats();
            PlayerPrefs.Save();
            return;
        }
        data.currentXP += amount;
        int xpNeeded = data.playerLevel * 100;
        
        while (data.currentXP >= xpNeeded)
        {
            if (data.playerLevel >= 9999)
            {
                data.playerLevel = 9999;
                data.currentXP = 0;
                break;
            }
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

    public void SetMaxLevel()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data != null)
        {
            if (data.playerLevel < 9999)
            {
                int lvls = 9999 - data.playerLevel;
                data.playerLevel = 9999;
                data.availableSkillPoints += lvls * 5;
            }
            data.currentXP = 0;
            RecalculateStats();
            SaveGameSystem.Save(0);
            ShowFeedback(Translator.LanguageID == 0 ? "✨ Уровень повышен до 9999 (Максимум)!" : "✨ Level set to 9999 (Maximum)!");
        }
    }

    public void SetMinLevel()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data != null)
        {
            data.playerLevel = 1;
            data.currentXP = 0;
            data.availableSkillPoints = 0;

            string cl = (data.characterClass ?? "warrior").ToLower();
            if (cl.Contains("warrior") || cl.Contains("воин") || cl.Contains("voin"))
            {
                data.strength = 15;
                data.agility = 10;
                data.intelligence = 4;
                data.stamina = 15;
            }
            else if (cl.Contains("archer") || cl.Contains("стрелок") || cl.Contains("strelok") || cl.Contains("лучник"))
            {
                data.strength = 10;
                data.agility = 14;
                data.intelligence = 6;
                data.stamina = 11;
            }
            else
            {
                data.strength = 6;
                data.agility = 10;
                data.intelligence = 10;
                data.stamina = 9;
            }

            RecalculateStats();
            SaveGameSystem.Save(0);
            ShowFeedback(Translator.LanguageID == 0 ? "✨ Уровень сброшен до 1 (Минимум)!" : "✨ Level set to 1 (Minimum)!");
        }
    }

    private void RecalculateStats()
    {
        RecalculateEquippedBonuses();
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data != null)
        {
            float previousMax = data.maxHealth;
            data.maxHealth = (data.stamina + eqBonusSTA + tempBonusSTA) * 10f;
            
            // If MaxHealth increased or current health is uninitialized, set currentHealth to maxHealth
            if (data.currentHealth <= 0f || data.currentHealth > data.maxHealth)
            {
                data.currentHealth = data.maxHealth;
            }
            else if (previousMax > 0f)
            {
                // Optionally scale current health with the new max
                float ratio = data.currentHealth / previousMax;
                data.currentHealth = Mathf.Clamp(data.maxHealth * ratio, 1f, data.maxHealth);
            }
        }
    }

    private void AutoAllocateAllPoints()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data == null || data.availableSkillPoints <= 0) return;

        string cl = (data.characterClass ?? "warrior").ToLower();
        int points = data.availableSkillPoints;
        data.availableSkillPoints = 0;

        if (cl.Contains("warrior") || cl.Contains("воин") || cl.Contains("voin"))
        {
            int strAdd = Mathf.FloorToInt(points * 0.5f);
            int staAdd = Mathf.FloorToInt(points * 0.3f);
            int agiAdd = Mathf.FloorToInt(points * 0.1f);
            int intAdd = points - (strAdd + staAdd + agiAdd);

            data.strength += strAdd;
            data.stamina += staAdd;
            data.agility += agiAdd;
            data.intelligence += intAdd;
        }
        else if (cl.Contains("archer") || cl.Contains("стрелок") || cl.Contains("strelok") || cl.Contains("лучник"))
        {
            int agiAdd = Mathf.FloorToInt(points * 0.5f);
            int staAdd = Mathf.FloorToInt(points * 0.3f);
            int strAdd = Mathf.FloorToInt(points * 0.15f);
            int intAdd = points - (agiAdd + staAdd + strAdd);

            data.agility += agiAdd;
            data.stamina += staAdd;
            data.strength += strAdd;
            data.intelligence += intAdd;
        }
        else
        {
            int intAdd = Mathf.FloorToInt(points * 0.5f);
            int staAdd = Mathf.FloorToInt(points * 0.3f);
            int agiAdd = Mathf.FloorToInt(points * 0.1f);
            int strAdd = points - (intAdd + staAdd + agiAdd);

            data.intelligence += intAdd;
            data.stamina += staAdd;
            data.agility += agiAdd;
            data.strength += strAdd;
        }

        RecalculateStats();
    }

    private void ResetPlayerStats()
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data == null) return;

        int startSTR = 10, startAGI = 10, startINT = 10, startSTA = 10;
        string cl = (data.characterClass ?? "воин").ToLower();
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

        int reclaimed = 0;
        if (data.strength > startSTR) reclaimed += (data.strength - startSTR);
        if (data.agility > startAGI) reclaimed += (data.agility - startAGI);
        if (data.intelligence > startINT) reclaimed += (data.intelligence - startINT);
        if (data.stamina > startSTA) reclaimed += (data.stamina - startSTA);

        data.strength = startSTR;
        data.agility = startAGI;
        data.intelligence = startINT;
        data.stamina = startSTA;

        data.availableSkillPoints += reclaimed;

        RecalculateStats();
        SaveGameSystem.Save(0);
        ShowFeedback(Translator.LanguageID == 0 ? "♻️ Характеристики сброшены к начальным!" : "♻️ Stats reset to base!");
    }

    public void AdvanceDay()
    {
        currentDay++;
        PlayerPrefs.SetInt("Fate_Current_Day", currentDay);

        int totalIncome = 0;
        foreach (var castle in castles)
        {
            if (castle.owner == "Player")
            {
                totalIncome += GetGoldIncome(castle.level);
            }
        }

        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        if (data != null)
        {
            data.gold += totalIncome;
        }

        tempBonusSTR = 0;
        tempBonusAGI = 0;
        tempBonusINT = 0;
        tempBonusSTA = 0;
        potionUsedThisTurnHP = false;
        potionUsedThisTurnSTR = false;
        potionUsedThisTurnAGI = false;
        potionUsedThisTurnINT = false;
        potionUsedThisTurnSTA = false;

        RecalculateStats();
        SaveGameSystem.Save(0);

        ProcessAITurns();

        showNewDayOverlay = true;

        string feedbackMsg = Translator.LanguageID == 0 
            ? $"📅 Наступил День {currentDay}! Собрано налогов: +{totalIncome} 💰" 
            : $"📅 Day {currentDay} has arrived! Collected taxes: +{totalIncome} 💰";
        ShowFeedback(feedbackMsg);
    }

    private void LoadAILogs()
    {
        aiLogs.Clear();
        int count = PlayerPrefs.GetInt("Fate_AILog_Count", 0);
        for (int i = 0; i < count; i++)
        {
            aiLogs.Add(PlayerPrefs.GetString("Fate_AILog_Line_" + i, ""));
        }
    }

    private void SaveAILogs()
    {
        PlayerPrefs.SetInt("Fate_AILog_Count", aiLogs.Count);
        for (int i = 0; i < aiLogs.Count; i++)
        {
            PlayerPrefs.SetString("Fate_AILog_Line_" + i, aiLogs[i]);
        }
        PlayerPrefs.Save();
    }

    private string GetCastleColorType(int regionIndex, int actualPlayerRegion, string owner)
    {
        if (owner == "Player" || regionIndex == actualPlayerRegion)
        {
            return "Player";
        }
        
        Color col = GetRegionColor(regionIndex, actualPlayerRegion, owner);
        if (col.g > 0.6f && col.r < 0.3f)
        {
            return "Green";
        }
        else if (col.r > 0.7f && col.g < 0.3f)
        {
            return "Red";
        }
        else
        {
            return "Neutral";
        }
    }

    public void ProcessAITurns()
    {
        aiLogs.Clear();
        int curLang = Translator.LanguageID;

        int playerDialogueIndex = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        int actualPlayerRegion = GetActualRegionIndexFromLanding(playerDialogueIndex);
        int selectedDifficulty = PlayerPrefs.GetInt("Difficulty", 2); // 0 - Новичок, 4 - Кошмар

        foreach (var castle in castles)
        {
            if (castle.owner == "Player" || castle.zoneIndex == actualPlayerRegion)
                continue;

            string castleType = GetCastleColorType(castle.zoneIndex, actualPlayerRegion, castle.owner);
            string castleName = curLang == 0 ? castle.nameRU : castle.nameEN;
            if (curLang == 8) castleName = castle.nameCH;
            if (curLang == 7) castleName = castle.nameKR;

            int oldTroops = castle.aiTroopsPower;
            int oldLvl = castle.aiCommanderLevel;
            int oldArmor = castle.aiArmorTier;
            int oldPotions = castle.aiPotionsStock;

            string logMessage = "";

            if (castleType == "Neutral")
            {
                switch (selectedDifficulty)
                {
                    case 0:
                        logMessage = GetText9(
                            $"⚪ Нейтральный замок {castleName} сохраняет пассивность и мирный нейтралитет.",
                            $"⚪ Neutral castle {castleName} remains passive and maintains peaceful neutrality.",
                            $"⚪ Die neutrale Burg {castleName} bleibt passiv und wahrt friedliche Neutralität.",
                            $"⚪ Le château neutre {castleName} reste passif et maintient une neutralité pacifique.",
                            $"⚪ El castillo neutral {castleName} permanece pasivo y mantiene una neutralidad pacífica.",
                            $"⚪ O castelo neutro {castleName} permanece passivo e mantém neutralidade pacífica.",
                            $"⚪ 中立の城 {castleName} は静観しており、平和的な中立を維持しています。",
                            $"⚪ 중립성 {castleName}은(는) 수동적이며 평화적인 중립을 유지합니다.",
                            $"⚪ 中立城堡 {castleName} 保持被动并维持着和平的中立状态。"
                        );
                        break;

                    case 1:
                        if (UnityEngine.Random.Range(0, 100) < 15)
                        {
                            castle.aiTroopsPower = Mathf.Min(20, castle.aiTroopsPower + 1);
                        }
                        logMessage = GetText9(
                            $"⚪ В нейтральном замке {castleName} замечены небольшие маневры сил.",
                            $"⚪ A slight movement of forces was observed in the neutral castle {castleName}.",
                            $"⚪ In der neutralen Burg {castleName} wurden geringfügige Truppenbewegungen beobachtet.",
                            $"⚪ Un léger mouvement de forces a été observé dans le château neutre {castleName}.",
                            $"⚪ Se observó un ligero movimiento de fuerzas en el castillo neutral {castleName}.",
                            $"⚪ Um pequeno movimento de forças foi observado no castelo neutro {castleName}.",
                            $"⚪ 中立の城 {castleName} 内で小規模な軍の機动が観測されました。",
                            $"⚪ 중립성 {castleName}에서 사소한 병력 기동이 관측되었습니다.",
                            $"⚪ 中立城堡 {castleName} 内部侦测到轻微的守军调动。"
                        );
                        break;

                    case 2:
                        if (UnityEngine.Random.Range(0, 100) < 35)
                        {
                            castle.aiTroopsPower = Mathf.Min(35, castle.aiTroopsPower + UnityEngine.Random.Range(1, 3));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 20)
                        {
                            castle.aiCommanderLevel = Mathf.Min(5, castle.aiCommanderLevel + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 15)
                        {
                            castle.aiArmorTier = Mathf.Min(2, castle.aiArmorTier + 1);
                        }
                        logMessage = GetText9(
                            $"⚪ Нейтральный замок {castleName} укрепил гарнизон (+{castle.aiTroopsPower - oldTroops} войск), повышая боевую подготовку на Нормальном уровне.",
                            $"⚪ Neutral castle {castleName} reinforced garrison (+{castle.aiTroopsPower - oldTroops} troops), raising training on Normal level.",
                            $"⚪ Neutrale Burg {castleName} verstärkte die Garnison (+{castle.aiTroopsPower - oldTroops} Truppen) und erhöhte das Training auf Normal.",
                            $"⚪ Le château neutre {castleName} a renforcé sa garnison (+{castle.aiTroopsPower - oldTroops} troupes) à un niveau Normal.",
                            $"⚪ El castillo neutral {castleName} reforzó la guarnición (+{castle.aiTroopsPower - oldTroops} tropas) en nivel Normal.",
                            $"⚪ O castelo neutro {castleName} reforçou a guarnição (+{castle.aiTroopsPower - oldTroops} tropas) no nível Normal.",
                            $"⚪ 中立の城 {castleName} は駐屯地を強化し (+{castle.aiTroopsPower - oldTroops} 兵)、ノーマル難易度相当の戦闘訓練を行いました。",
                            $"⚪ 중립성 {castleName}이(가) 주둔군을 강화하여 (+{castle.aiTroopsPower - oldTroops} 병력) 보통 수준의 훈련을 진행했습니다.",
                            $"⚪ 中立城堡 {castleName} 增援了守备军 (+{castle.aiTroopsPower - oldTroops} 士兵)，并在普通级难度下提升了训练强度。"
                        );
                        break;

                    case 3:
                        if (UnityEngine.Random.Range(0, 100) < 60)
                        {
                            castle.aiTroopsPower = Mathf.Min(60, castle.aiTroopsPower + UnityEngine.Random.Range(2, 5));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            castle.aiCommanderLevel = Mathf.Min(10, castle.aiCommanderLevel + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 30)
                        {
                            castle.aiArmorTier = Mathf.Min(3, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            castle.aiPotionsStock = Mathf.Min(4, castle.aiPotionsStock + 1);
                        }
                        logMessage = GetText9(
                            $"⚪ Нейтральный замок {castleName} активно развивается на Сложном уровне: гарнизон усилен (+{castle.aiTroopsPower - oldTroops} войск), командир {castle.aiCommanderLevel} ур., доспехи {castle.aiArmorTier} тира.",
                            $"⚪ Neutral castle {castleName} actively develops on Hard level: garrison reinforced (+{castle.aiTroopsPower - oldTroops} troops), level {castle.aiCommanderLevel} commander, tier {castle.aiArmorTier} armor.",
                            $"⚪ Neutrale Burg {castleName} entwickelt sich aktiv auf Schwer: Garnison verstärkt (+{castle.aiTroopsPower - oldTroops} Truppen), Level {castle.aiCommanderLevel} Kommandant, Stufe {castle.aiArmorTier} Rüstung.",
                            $"⚪ Le château neutre {castleName} se développe activement au niveau Difficile : garnison renforcée (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel}, armure T{castle.aiArmorTier}.",
                            $"⚪ El castillo neutral {castleName} se desarrolla activamente en nivel Difícil: guarnición reforzada (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel}, armadura tier {castle.aiArmorTier}.",
                            $"⚪ O castelo neutro {castleName} se desenvolve ativamente no nível Difícil: guarnição reforçada (+{castle.aiTroopsPower - oldTroops} tropas), líder nível {castle.aiCommanderLevel}, armadura tier {castle.aiArmorTier}.",
                            $"⚪ 中立の城 {castleName} はハード難易度で活発に発展中：駐屯地強化 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel}、防具ティア {castle.aiArmorTier}。",
                            $"⚪ 중립성 {castleName}이(가) 어려움 난이도에서 활발히 성장 중: 주둔군 강화 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨, 방어구 {castle.aiArmorTier}티어.",
                            $"⚪ 中立城堡 {castleName} 在困难级难度下加紧扩军：守备力增强 (+{castle.aiTroopsPower - oldTroops} 士兵)，主将升至 {castle.aiCommanderLevel} 级，防具提升至第 {castle.aiArmorTier} 阶。"
                        );
                        break;

                    case 4:
                        castle.aiTroopsPower = Mathf.Min(100, castle.aiTroopsPower + UnityEngine.Random.Range(4, 9));
                        if (UnityEngine.Random.Range(0, 100) < 75)
                        {
                            castle.aiCommanderLevel = Mathf.Min(15, castle.aiCommanderLevel + UnityEngine.Random.Range(1, 3));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        {
                            castle.aiArmorTier = Mathf.Min(4, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 60)
                        {
                            castle.aiPotionsStock = Mathf.Min(5, castle.aiPotionsStock + 1);
                        }
                        logMessage = GetText9(
                            $"⚪ Нейтральный замок {castleName} достиг Кошмарной боеготовности: гарнизон усилен (+{castle.aiTroopsPower - oldTroops} войск), командир {castle.aiCommanderLevel} ур., закуплены зелья ({castle.aiPotionsStock} шт.).",
                            $"⚪ Neutral castle {castleName} achieved Nightmare readiness: garrison reinforced (+{castle.aiTroopsPower - oldTroops} troops), level {castle.aiCommanderLevel} commander, potions purchased ({castle.aiPotionsStock}).",
                            $"⚪ Neutrale Burg {castleName} erreichte Albtraum-Bereitschaft: Garnison verstärkt (+{castle.aiTroopsPower - oldTroops} Truppen), Level {castle.aiCommanderLevel} Kommandant, Tränke gekauft ({castle.aiPotionsStock}).",
                            $"⚪ Le château neutre {castleName} a atteint l'état d'alerte Cauchemar : garnison renforcée (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel}, potions achetées ({castle.aiPotionsStock}).",
                            $"⚪ El castillo neutral {castleName} alcanzó preparación Pesadilla: guarnición reforzada (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel}, pociones compradas ({castle.aiPotionsStock}).",
                            $"⚪ O castelo neutro {castleName} atingiu prontidão Pesadelo: guarnição reforçada (+{castle.aiTroopsPower - oldTroops} tropas), líder nível {castle.aiCommanderLevel}, poções compradas ({castle.aiPotionsStock}).",
                            $"⚪ 中立の城 {castleName} は悪夢の臨戦態勢に到達：駐屯地強化 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel}、ポーション購入数 {castle.aiPotionsStock}個。",
                            $"⚪ 중립성 {castleName}이(가) 악몽 수준의 전투 태세 완비: 주둔군 강화 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨, 물약 보유량 {castle.aiPotionsStock}개.",
                            $"⚪ 中立城堡 {castleName} 已达至噩梦级军事防备：守备军暴增 (+{castle.aiTroopsPower - oldTroops} 士兵)，主将升至 {castle.aiCommanderLevel} 级，储备炼金药剂 {castle.aiPotionsStock} 瓶。"
                        );
                        break;
                }
            }
            else if (castleType == "Green")
            {
                switch (selectedDifficulty)
                {
                    case 0:
                        logMessage = GetText9(
                            $"🟢 Зеленый замок-защитник {castleName} ведет себя пассивно, сосредоточившись строго на охране внутренних границ.",
                            $"🟢 Green defender castle {castleName} behaves passively, focusing strictly on guarding internal borders.",
                            $"🟢 Die grüne Verteidigungsburg {castleName} verhält sich passiv und konzentriert sich auf die Sicherung der Grenzen.",
                            $"🟢 Le château vert défensif {castleName} se comporte passivement, se concentrant sur la protection des frontières.",
                            $"🟢 El castillo defensor verde {castleName} se comporta de forma pasiva, concentrándose en vigilar las fronteras.",
                            $"🟢 O castelo defensor verde {castleName} comporta-se passivamente, focando estritamente na guarda de fronteiras.",
                            $"🟢 緑の防衛城 {castleName} は消極的で、もっぱら自らの国境警戒のみに集中しています。",
                            $"🟢 녹색 방어성 {castleName}은(는) 소극적이며 오직 국경 경비에만 전념하고 있습니다.",
                            $"🟢 绿方防御要塞 {castleName} 表现温和被动，目前仅专注于本国边界的基本警备。"
                        );
                        break;

                    case 1:
                        if (UnityEngine.Random.Range(0, 100) < 15)
                        {
                            castle.aiTroopsPower = Mathf.Min(20, castle.aiTroopsPower + 1);
                        }
                        logMessage = GetText9(
                            $"🟢 Защитники зеленого замка {castleName} проводят небольшие оборонительные маневры (+{castle.aiTroopsPower - oldTroops} войск).",
                            $"🟢 Defenders of green castle {castleName} conduct minor defensive exercises (+{castle.aiTroopsPower - oldTroops} troops).",
                            $"🟢 Verteidiger der grünen Burg {castleName} führen kleine Verteidigungsübungen durch (+{castle.aiTroopsPower - oldTroops} Truppen).",
                            $"🟢 Les défenseurs du château vert {castleName} effectuent de légers exercices défensifs (+{castle.aiTroopsPower - oldTroops} troupes).",
                            $"🟢 Los defensores del castillo verde {castleName} realizan pequeños ejercicios defensivos (+{castle.aiTroopsPower - oldTroops} tropas).",
                            $"🟢 Os defensores do castelo verde {castleName} realizam pequenos exercícios de defesa (+{castle.aiTroopsPower - oldTroops} tropas).",
                            $"🟢 緑の城 {castleName} の衛兵たちは、軽度な守備軍事演習を行いました (+{castle.aiTroopsPower - oldTroops} 兵)。",
                            $"🟢 녹색 성 {castleName}의 방어군이 사소한 경계 훈련을 마쳤습니다 (+{castle.aiTroopsPower - oldTroops} 병력).",
                            $"🟢 绿方城堡 {castleName} 的御林守军开展了轻度防御性演练 (+{castle.aiTroopsPower - oldTroops} 士兵)。"
                        );
                        break;

                    case 2:
                        castle.aiTroopsPower = Mathf.Min(45, castle.aiTroopsPower + UnityEngine.Random.Range(2, 4));
                        if (UnityEngine.Random.Range(0, 100) < 30)
                        {
                            castle.aiCommanderLevel = Mathf.Min(6, castle.aiCommanderLevel + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 25)
                        {
                            castle.aiArmorTier = Mathf.Min(2, castle.aiArmorTier + 1);
                        }
                        logMessage = GetText9(
                            $"🟢 Зеленый замок {castleName} укрепляет рубежи на Нормальном уровне: возведены баррикады (+{castle.aiTroopsPower - oldTroops} войск), военачальник получил {castle.aiCommanderLevel} ур.",
                            $"🟢 Green castle {castleName} reinforces borders on Normal level: barricades erected (+{castle.aiTroopsPower - oldTroops} troops), commander achieved level {castle.aiCommanderLevel}.",
                            $"🟢 Grüne Burg {castleName} verstärkt die Grenzen auf Normal: Barrikaden errichtet (+{castle.aiTroopsPower - oldTroops} Truppen), Kommandant hat Level {castle.aiCommanderLevel} erreicht.",
                            $"🟢 Le château vert {castleName} renforce ses frontières au niveau Normal : barricades érigées (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel}.",
                            $"🟢 El castillo verde {castleName} refuerza fronteras en nivel Normal: barricadas erigidas (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel}.",
                            $"🟢 O castelo verde {castleName} reforça limites no nível Normal: barricadas construídas (+{castle.aiTroopsPower - oldTroops} tropas), comandante nível {castle.aiCommanderLevel}.",
                            $"🟢 緑の城 {castleName} はノーマルレベルで防壁を構築中：防壁強化 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel} に向上。",
                            $"🟢 녹색 성 {castleName}이(가) 보통 수준에서 장벽을 구축 중: 방어 참호 건설 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨 달성.",
                            $"🟢 绿方城堡 {castleName} 于普通级别下修筑据点：加固防御护栏 (+{castle.aiTroopsPower - oldTroops} 士兵)，城守升级至 {castle.aiCommanderLevel} 级。"
                        );
                        break;

                    case 3:
                        castle.aiTroopsPower = Mathf.Min(75, castle.aiTroopsPower + UnityEngine.Random.Range(4, 7));
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        {
                            castle.aiCommanderLevel = Mathf.Min(11, castle.aiCommanderLevel + UnityEngine.Random.Range(1, 3));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            castle.aiArmorTier = Mathf.Min(3, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        {
                            castle.aiPotionsStock = Mathf.Min(5, castle.aiPotionsStock + 1);
                        }
                        logMessage = GetText9(
                            $"🟢 Оборонительный замок {castleName} развернул тяжелую оборону на Сложном уровне: гарнизон значительно усилен (+{castle.aiTroopsPower - oldTroops} войск), доспехи {castle.aiArmorTier} тира, военачальник получил {castle.aiCommanderLevel} ур.",
                            $"🟢 Defensive castle {castleName} deployed heavy defense on Hard level: garrison reinforced (+{castle.aiTroopsPower - oldTroops} troops), tier {castle.aiArmorTier} armor, level {castle.aiCommanderLevel} commander.",
                            $"🟢 Verteidigungsburg {castleName} hat schwere Verteidigung auf Schwer aufgebaut: Garnison stark verstärkt (+{castle.aiTroopsPower - oldTroops} Truppen), Stufe {castle.aiArmorTier} Rüstung, Level {castle.aiCommanderLevel} Kommandant.",
                            $"🟢 Le château défensif {castleName} a déployé une lourde défense au niveau Difficile : garnison renforcée (+{castle.aiTroopsPower - oldTroops} troupes), armure T{castle.aiArmorTier}, commandant niv. {castle.aiCommanderLevel}.",
                            $"🟢 El castillo defensivo {castleName} desplegó defensa pesada en nivel Difícil: guarnición reforzada (+{castle.aiTroopsPower - oldTroops} tropas), armadura tier {castle.aiArmorTier}, comandante de nivel {castle.aiCommanderLevel}.",
                            $"🟢 O castelo defensivo {castleName} mobilizou defesa pesada no nível Difícil: guarnição reforçada (+{castle.aiTroopsPower - oldTroops} tropas), armadura tier {castle.aiArmorTier}, comandante nível {castle.aiCommanderLevel}.",
                            $"🟢 防衛特化城 {castleName} はハード難易度で強固な陣地を展開：城塞を大幅強化 (+{castle.aiTroopsPower - oldTroops} 兵)、防具ティア {castle.aiArmorTier}、指揮官Lv {castle.aiCommanderLevel}。",
                            $"🟢 방어형 성 {castleName}이(가) 어려움 난이도에서 밀집 방어벽 형성: 수비군 대규모 보강 (+{castle.aiTroopsPower - oldTroops} 병력), 갑옷 {castle.aiArmorTier}티어, 사령관 {castle.aiCommanderLevel}레벨.",
                            $"🟢 绿方戍卫城堡 {castleName} 部署了重装防线：重型铁卫加入 (+{castle.aiTroopsPower - oldTroops} 士兵)，重铠提升至 {castle.aiArmorTier} 阶，城守将军升至 {castle.aiCommanderLevel} 级。"
                        );
                        break;

                    case 4:
                        castle.aiTroopsPower = Mathf.Min(120, castle.aiTroopsPower + UnityEngine.Random.Range(6, 13));
                        if (UnityEngine.Random.Range(0, 100) < 85)
                        {
                            castle.aiCommanderLevel = Mathf.Min(18, castle.aiCommanderLevel + UnityEngine.Random.Range(2, 4));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            castle.aiArmorTier = Mathf.Min(4, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 80)
                        {
                            castle.aiPotionsStock = Mathf.Min(8, castle.aiPotionsStock + 2);
                        }
                        logMessage = GetText9(
                            $"🟢 Зеленый замок {castleName} достиг Кошмарной обороны: гарнизон укреплен до предела (+{castle.aiTroopsPower - oldTroops} войск), командир {castle.aiCommanderLevel} ур., броня {castle.aiArmorTier} тира, запас зелий: {castle.aiPotionsStock}.",
                            $"🟢 Green castle {castleName} reached Nightmare defense: garrison reinforced to maximum (+{castle.aiTroopsPower - oldTroops} troops), level {castle.aiCommanderLevel} commander, tier {castle.aiArmorTier} armor, potions: {castle.aiPotionsStock}.",
                            $"🟢 Grüne Burg {castleName} hat Albtraum-Verteidigung erreicht: Garnison maximal verstärkt (+{castle.aiTroopsPower - oldTroops} Truppen), Level {castle.aiCommanderLevel} Kommandant, Stufe {castle.aiArmorTier} Rüstung, Tränke: {castle.aiPotionsStock}.",
                            $"🟢 Le château vert {castleName} a atteint la défense Cauchemar : garnison au maximum (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel}, armure T{castle.aiArmorTier}, potions : {castle.aiPotionsStock}.",
                            $"🟢 El castillo verde {castleName} alcanzó defensa Pesadilla: guarnición al máximo (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel}, armadura tier {castle.aiArmorTier}, pociones: {castle.aiPotionsStock}.",
                            $"🟢 O castelo verde {castleName} atingiu defesa Pesadelo: guarnição ao máximo (+{castle.aiTroopsPower - oldTroops} tropas), comandante nível {castle.aiCommanderLevel}, armadura tier {castle.aiArmorTier}, poções: {castle.aiPotionsStock}.",
                            $"🟢 緑の城 {castleName} は悪夢の難攻不落城を達成：守備軍極大化 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel}、鎧ティア {castle.aiArmorTier}、薬液保管庫 {castle.aiPotionsStock}本。",
                            $"🟢 녹색 성 {castleName}이(가) 악몽급 불사벽 완성: 수비군 최대로 소집 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨, 신성 갑옷 {castle.aiArmorTier}티어, 비약 {castle.aiPotionsStock}개 보유.",
                            $"🟢 绿方神圣之城 {castleName} 筑造了不落天堑：绝壁要塞落成 (+{castle.aiTroopsPower - oldTroops} 士兵)，主将升至 {castle.aiCommanderLevel} 级，神装解锁至 {castle.aiArmorTier} 阶，战剂储量 {castle.aiPotionsStock} 瓶。"
                        );
                        break;
                }
            }
            else if (castleType == "Red")
            {
                switch (selectedDifficulty)
                {
                    case 0:
                        if (UnityEngine.Random.Range(0, 100) < 10)
                        {
                            castle.aiTroopsPower = Mathf.Min(22, castle.aiTroopsPower + 1);
                        }
                        logMessage = GetText9(
                            $"🔴 Красный агрессивный замок {castleName} ведет себя тихо, но высылает разведчиков за границы ваших земель.",
                            $"🔴 Red aggressive castle {castleName} behaves quietly, but sends scouts beyond the borders of your lands.",
                            $"🔴 Die rote, aggressive Burg {castleName} verhält sich ruhig, sendet aber Späher über Ihre Grenzen.",
                            $"🔴 Le château rouge agressif {castleName} se comporte calmement, mais envoie des éclaireurs au-delà de vos frontières.",
                            $"🔴 El castillo agresivo rojo {castleName} se comporta con calma, pero envía exploradores más allá de tus fronteras.",
                            $"🔴 O castelo agressivo vermelho {castleName} comporta-se calmamente, mas envia batedores além de suas fronteiras.",
                            $"🔴 赤の好戦的な城 {castleName} は静かですが、あなたの領土に隠密偵察隊を放っています。",
                            $"🔴 적색 공격성 {castleName}은(는) 조용하지만 국경 부근에 밀탐을 보내 동태를 감시하고 있습니다.",
                            $"🔴 红方掠夺者城堡 {castleName} 虽未大规模集结，但已悄然向你部边界派出斥候密探进行测绘。"
                        );
                        break;

                    case 1:
                        if (UnityEngine.Random.Range(0, 100) < 30)
                        {
                            castle.aiTroopsPower = Mathf.Min(30, castle.aiTroopsPower + UnityEngine.Random.Range(1, 3));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 20)
                        {
                            castle.aiCommanderLevel = Mathf.Min(4, castle.aiCommanderLevel + 1);
                        }
                        logMessage = GetText9(
                            $"🔴 Агрессоры красного замка {castleName} активизировали набеги (+{castle.aiTroopsPower - oldTroops} войск), тренируя командиров для экспансии.",
                            $"🔴 Aggressors from red castle {castleName} intensified raids (+{castle.aiTroopsPower - oldTroops} troops), training commanders for expansion.",
                            $"🔴 Angreifer der roten Burg {castleName} intensivierten Überfälle (+{castle.aiTroopsPower - oldTroops} Truppen) und trainierten Kommandanten.",
                            $"🔴 Les agresseurs du château rouge {castleName} ont intensifié leurs raids (+{castle.aiTroopsPower - oldTroops} troupes) pour s'étendre.",
                            $"🔴 Los agresores del castillo rojo {castleName} intensificaron incursiones (+{castle.aiTroopsPower - oldTroops} tropas) para expandirse.",
                            $"🔴 Os agressores do castelo vermelho {castleName} intensificaram saques (+{castle.aiTroopsPower - oldTroops} tropas), preparando a expansão.",
                            $"🔴 赤の城 {castleName} の好戦勢力は略奪行軍を活性化させ (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官を鍛え上げました。",
                            $"🔴 적색 성 {castleName}의 침략 세력이 소규모 습격을 전개하고 (+{castle.aiTroopsPower - oldTroops} 병력) 지휘 무관을 임관시켰습니다.",
                            $"🔴 红方野蛮阵线 {castleName} 开始对周边村庄进行扫荡掠夺 (+{castle.aiTroopsPower - oldTroops} 士兵)，锋芒初露。"
                        );
                        break;

                    case 2:
                        castle.aiTroopsPower = Mathf.Min(55, castle.aiTroopsPower + UnityEngine.Random.Range(3, 6));
                        if (UnityEngine.Random.Range(0, 100) < 45)
                        {
                            castle.aiCommanderLevel = Mathf.Min(8, castle.aiCommanderLevel + UnityEngine.Random.Range(1, 3));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 20)
                        {
                            castle.aiArmorTier = Mathf.Min(2, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 30)
                        {
                            castle.aiPotionsStock = Mathf.Min(3, castle.aiPotionsStock + 1);
                        }
                        logMessage = GetText9(
                            $"🔴 Красный замок {castleName} развивает наступление на Нормальном уровне: усилен штурмовой корпус (+{castle.aiTroopsPower - oldTroops} войск), командир {castle.aiCommanderLevel} ур., защита слабая.",
                            $"🔴 Red castle {castleName} advances on Normal level: assault corps reinforced (+{castle.aiTroopsPower - oldTroops} troops), level {castle.aiCommanderLevel} commander, defense remains low.",
                            $"🔴 Rote Burg {castleName} greift auf Normal an: Angriffskorps verstärkt (+{castle.aiTroopsPower - oldTroops} Truppen), Level {castle.aiCommanderLevel} Kommandant, Verteidigung bleibt schwach.",
                            $"🔴 Le château rouge {castleName} mène une offensive au niveau Normal : corps d'assaut renforcé (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel}, défense faible.",
                            $"🔴 El castillo rojo {castleName} avanza en nivel Normal: cuerpo de asalto reforzado (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel}, defensa baja.",
                            $"🔴 O castelo vermelho {castleName} avança no nível Normal: infantaria de assalto reforçada (+{castle.aiTroopsPower - oldTroops} tropas), líder nível {castle.aiCommanderLevel}, defesa fraca.",
                            $"🔴 赤の城 {castleName} はノーマルレベルで侵攻準備：突撃部隊を強化 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel} 、防衛は手薄です。",
                            $"🔴 적색 성 {castleName}이(가) 보통 수준에서 전격 공세를 감행: 강습 연대 조직 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨, 성비는 미비합니다.",
                            $"🔴 红方铁血要塞 {castleName} 于普通级别下集结主攻部队：突击军力上升 (+{castle.aiTroopsPower - oldTroops} 士兵)，主帅升至 {castle.aiCommanderLevel} 级，内部防务较弱。"
                        );
                        break;

                    case 3:
                        castle.aiTroopsPower = Mathf.Min(90, castle.aiTroopsPower + UnityEngine.Random.Range(5, 11));
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            castle.aiCommanderLevel = Mathf.Min(14, castle.aiCommanderLevel + UnityEngine.Random.Range(2, 4));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        {
                            castle.aiArmorTier = Mathf.Min(3, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 60)
                        {
                            castle.aiPotionsStock = Mathf.Min(5, castle.aiPotionsStock + 1);
                        }
                        logMessage = GetText9(
                            $"🔴 Красная цитадель {castleName} ведет яростное наступление на Сложном уровне: лавина штурмовиков (+{castle.aiTroopsPower - oldTroops} войск), закуплено снаряжение {castle.aiArmorTier} тира, командир {castle.aiCommanderLevel} ур.!",
                            $"🔴 Red citadel {castleName} leads a fierce assault on Hard level: avalanche of shock troops (+{castle.aiTroopsPower - oldTroops} troops), tier {castle.aiArmorTier} gear purchased, level {castle.aiCommanderLevel} commander!",
                            $"🔴 Rote Zitadelle {castleName} führt heftigen Angriff auf Schwer: Lawine von Sturmtruppen (+{castle.aiTroopsPower - oldTroops} Truppen), Stufe {castle.aiArmorTier} Ausrüstung gekauft, Level {castle.aiCommanderLevel} Kommandant!",
                            $"🔴 La citadelle rouge {castleName} mène un assaut féroce au niveau Difficile : avalanche de troupes de choc (+{castle.aiTroopsPower - oldTroops} troupes), équipement T{castle.aiArmorTier} acheté, commandant niv. {castle.aiCommanderLevel} !",
                            $"🔴 La ciudadela roja {castleName} lidera un asalto feroz en nivel Difícil: avalancha de tropas de choque (+{castle.aiTroopsPower - oldTroops} tropas), equipo tier {castle.aiArmorTier} comprado, comandante de nivel {castle.aiCommanderLevel}!",
                            $"🔴 A cidadela vermelha {castleName} lidera um ataque feroz no nível Difícil: avalanche de tropas de choque (+{castle.aiTroopsPower - oldTroops} tropas), equipamento tier {castle.aiArmorTier} comprado, líder nível {castle.aiCommanderLevel}!",
                            $"🔴 赤の要塞 {castleName} はハード難易度で猛烈な強襲を展開：突撃大隊が襲来 (+{castle.aiTroopsPower - oldTroops} 兵)、武具ティア {castle.aiArmorTier}、指揮官Lv {castle.aiCommanderLevel}！",
                            $"🔴 적색 요새 {castleName}이(가) 어려움 난이도에서 광포한 파상 공세 돌입: 대규모 전선 돌파대 소집 (+{castle.aiTroopsPower - oldTroops} 병력), 장비 {castle.aiArmorTier}티어 장착, 사령관 {castle.aiCommanderLevel}레벨!",
                            $"🔴 红方死亡前哨 {castleName} 在困难级难度下组织疯狂合围：攻城先锋大军开拔 (+{castle.aiTroopsPower - oldTroops} 士兵)，重兵装跃升至 {castle.aiArmorTier} 阶，狂将升至 {castle.aiCommanderLevel} 级！"
                        );
                        break;

                    case 4:
                        castle.aiTroopsPower = Mathf.Min(150, castle.aiTroopsPower + UnityEngine.Random.Range(8, 17));
                        if (UnityEngine.Random.Range(0, 100) < 95)
                        {
                            castle.aiCommanderLevel = Mathf.Min(22, castle.aiCommanderLevel + UnityEngine.Random.Range(2, 5));
                        }
                        if (UnityEngine.Random.Range(0, 100) < 80)
                        {
                            castle.aiArmorTier = Mathf.Min(4, castle.aiArmorTier + 1);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 90)
                        {
                            castle.aiPotionsStock = Mathf.Min(10, castle.aiPotionsStock + UnityEngine.Random.Range(2, 4));
                        }
                        logMessage = GetText9(
                            $"🔴 КРАСНЫЙ ЗАМОК {castleName} СЕЕТ КОШМАР! Полная мобилизация сил (+{castle.aiTroopsPower - oldTroops} войск), командир {castle.aiCommanderLevel} ур. во всеоружии, броня {castle.aiArmorTier} тира, запас зелий: {castle.aiPotionsStock} шт., ведется тотальный шпионаж за вами!",
                            $"🔴 RED CASTLE {castleName} SOWS NIGHTMARE! Full mobilization (+{castle.aiTroopsPower - oldTroops} troops), level {castle.aiCommanderLevel} commander fully armed, tier {castle.aiArmorTier} armor, potions: {castle.aiPotionsStock}, total espionage active on you!",
                            $"🔴 ROTE BURG {castleName} SÄT ALBTRAUM! Vollmobilisierung (+{castle.aiTroopsPower - oldTroops} Truppen), Level {castle.aiCommanderLevel} Kommandant voll bewaffnet, Stufe {castle.aiArmorTier} Rüstung, Tränke: {castle.aiPotionsStock}, Spionage aktiv!",
                            $"🔴 LE CHÂTEAU ROUGE {castleName} SÈME LE CAUCHEMAR ! Mobilisation complète (+{castle.aiTroopsPower - oldTroops} troupes), commandant niv. {castle.aiCommanderLevel} surarmé, armure T{castle.aiArmorTier}, potions : {castle.aiPotionsStock}, espionnage total actif !",
                            $"🔴 ¡EL CASTILLO ROJO {castleName} SIEMBRA LA PESADILLA! Movilización completa (+{castle.aiTroopsPower - oldTroops} tropas), comandante de nivel {castle.aiCommanderLevel} súper armado, armadura tier {castle.aiArmorTier}, pociones: {castle.aiPotionsStock}, ¡espionaje total activo!",
                            $"🔴 O CASTELO VERMELHO {castleName} SEMEIA O PESADELO! Mobilização completa (+{castle.aiTroopsPower - oldTroops} tropas), comandante nível {castle.aiCommanderLevel} superarmado, armadura tier {castle.aiArmorTier}, poções: {castle.aiPotionsStock}, espionagem total ativa!",
                            $"🔴 赤の要塞 {castleName} は悪夢を巻き起こしています！超限界突破動員 (+{castle.aiTroopsPower - oldTroops} 兵)、指揮官Lv {castle.aiCommanderLevel}、神装ティア {castle.aiArmorTier}、薬液貯蔵 {castle.aiPotionsStock}個、あなたへの全面的な諜報戦が進行中！",
                            $"🔴 적색 지옥문 {castleName}이(가) 악몽을 뿌립니다! 전군 소집령 선포 (+{castle.aiTroopsPower - oldTroops} 병력), 사령관 {castle.aiCommanderLevel}레벨 무장 완성, 장비 {castle.aiArmorTier}티어, 보화 약물 {castle.aiPotionsStock}개, 당신에 대한 무제한 첩보 공세 발령!",
                            $"🔴 血祸红城 {castleName} 洒下噩梦天灾！举国总动员暴兵 (+{castle.aiTroopsPower - oldTroops} 士兵)，征服统领斩获 {castle.aiCommanderLevel} 级，神兵天甲解锁至 {castle.aiArmorTier} 阶，狂热药剂 {castle.aiPotionsStock} 支，已对你的防区进行毁灭级密探渗透！"
                        );
                        break;
                }
            }

            if (!string.IsNullOrEmpty(logMessage))
            {
                aiLogs.Add(logMessage);
            }

            PlayerPrefs.SetInt("Castle_AI_CommanderLvl_" + castle.zoneIndex, castle.aiCommanderLevel);
            PlayerPrefs.SetInt("Castle_AI_Troops_" + castle.zoneIndex, castle.aiTroopsPower);
            PlayerPrefs.SetInt("Castle_AI_Armor_" + castle.zoneIndex, castle.aiArmorTier);
            PlayerPrefs.SetInt("Castle_AI_Potions_" + castle.zoneIndex, castle.aiPotionsStock);
        }

        SaveAILogs();
    }

    private void OnGUI()
    {
        int curLang = Translator.LanguageID;
        bool isDialogueOpen = DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive;

        if (!isDialogueOpen)
        {
            // 1. Кошелек
            string goldText = curLang == 0 ? "Золото: " : "Gold: ";
            if (curLang == 8) goldText = "金币: ";
            if (curLang == 7) goldText = "보유 골드: ";

            if (s_walletStyle == null)
            {
                s_walletStyle = new GUIStyle(GUI.skin.box);
                s_walletStyle.fontSize = 14;
                s_walletStyle.fontStyle = FontStyle.Bold;
                s_walletStyle.normal.textColor = new Color(0.95f, 0.75f, 0.1f, 1.0f);
                s_walletStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUI.Box(new Rect(Screen.width - 240f, 20f, 220f, 42f), $"💰 {goldText}{SaveGameSystem.CurrentData.gold}", s_walletStyle);

            // 2. Индикатор Дня
            string dayLabel = curLang == 0 ? "День: " : "Day: ";
            if (curLang == 8) dayLabel = "当前天数: ";
            if (curLang == 7) dayLabel = "일차: ";

            if (s_dStyle == null)
            {
                s_dStyle = new GUIStyle(GUI.skin.box);
                s_dStyle.fontSize = 14;
                s_dStyle.fontStyle = FontStyle.Bold;
                s_dStyle.normal.textColor = new Color(0.12f, 0.88f, 1.0f, 1.0f);
                s_dStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUI.Box(new Rect(Screen.width - 240f, 65f, 220f, 38f), $"📅 {dayLabel}{currentDay}", s_dStyle);

            // 3. Кнопка "Пропустить ход" UI
            string nextDayBtnText = curLang == 0 ? "ПРОПУСТИТЬ ХОД" : "END TURN";
            if (curLang == 8) nextDayBtnText = "结束回合";
            if (curLang == 7) nextDayBtnText = "턴 넘기기";

            if (s_nextDayStyle == null)
            {
                s_nextDayStyle = new GUIStyle(GUI.skin.button);
                s_nextDayStyle.fontSize = 13;
                s_nextDayStyle.fontStyle = FontStyle.Bold;
                s_nextDayStyle.normal.textColor = Color.white;
                s_nextDayStyle.alignment = TextAnchor.MiddleCenter;
            }

            // Блокируем кнопку "Пропустить ход", если открыта панель управления персонажем (showStatsPanel), детали или идет диалог
            if (!isDetailsOpen && !showStatsPanel)
            {
                GUI.backgroundColor = new Color(0.1f, 0.65f, 0.95f, 1.0f);
                if (GUI.Button(new Rect(Screen.width - 240f, 107f, 220f, 44f), $"▶ {nextDayBtnText}", s_nextDayStyle))
                {
                    AdvanceDay();
                }
                GUI.backgroundColor = Color.white;
            }

            // 4. Отрисовка ГЕРОЯ И ЕГО ХАРАКТЕРИСТИК (HUD в верхнем левом углу)
            DrawHeroHUD(curLang);
        }

        // Overlay нового дня (ИИ отчеты)
        if (showNewDayOverlay)
        {
            DrawNewDayOverlay(curLang);
        }

        // Окно настроек деталей (сначала рисуем подложку, чтобы всплывающие окна были поверх неё; фон затемняется при активных поп-апах)
        if (isDetailsOpen && activeDetailsIndex >= 0 && activeDetailsIndex < castles.Count)
        {
            bool modalActive = showCastleCalibrationPanel || showSkillDetailPopup || showTroopDetailPopup || showForgeDetailPopup || showSpyReportPopup || showPurchaseConfirmPopup;
            if (modalActive)
            {
                GUI.enabled = false;
            }
            DrawDetailsWindow(curLang);
            if (modalActive)
            {
                GUI.enabled = true;
            }
        }

        // РЕНДЕРИНГ ВНУТРЕННЕГО ВИДА ГОРОДА/ЦИТАДЕЛИ ПРИ АКТИВАЦИИ (v18.11.24)
        if (isTownViewActive && activeDetailsIndex >= 0 && activeDetailsIndex < castles.Count)
        {
            bool modalActive = showCastleCalibrationPanel || showSkillDetailPopup || showTroopDetailPopup || showForgeDetailPopup || showSpyReportPopup || showPurchaseConfirmPopup;
            if (modalActive)
            {
                GUI.enabled = false;
            }
            DrawTownViewGUI(curLang);
            if (modalActive)
            {
                GUI.enabled = true;
            }
        }

        // Всплывающие окна деталей (v18.11.16) - рисуются после основных окон, чтобы быть на самом верху!
        // Рисуем затемнение экрана (Modal Blocker) для всплывающих окон, чтобы избежать визуального наложения
        if (showCastleCalibrationPanel || showSkillDetailPopup || showTroopDetailPopup || showForgeDetailPopup || showSpyReportPopup || showPurchaseConfirmPopup)
        {
            GUI.backgroundColor = new Color(0.01f, 0.02f, 0.06f, 0.88f);
            GUIStyle blockerStyle = new GUIStyle(GUI.skin.box);
            blockerStyle.normal.background = hudTex;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", blockerStyle);
            GUI.backgroundColor = Color.white;
        }

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

        if (showForgeDetailPopup)
        {
            DrawForgeDetailPopup(curLang);
        }

        if (showPurchaseConfirmPopup)
        {
            DrawPurchaseConfirmPopup(curLang);
        }

        if (showSpyReportPopup)
        {
            DrawSpyReportPopup(curLang);
        }

        // РЕНДЕРИНГ ОВЕРЛЕЯ ТЕЛЕМЕТРИИ ПРОИЗВОДИТЕЛЬНОСТИ ПК (v18.11.25)
        if (PlayerPrefs.GetInt("CheatPerformanceOverlayEnabled", 0) == 1)
        {
            DrawPerformanceTelemetryOverlay(curLang);
        }
    }

    private void UpdateTelemetryMetrics()
    {
        // 1. Calculate real FPS
        float currentFps = Time.unscaledDeltaTime > 0f ? (1.0f / Time.unscaledDeltaTime) : 60f;
        currentFps = Mathf.Clamp(currentFps, 1f, 500f);
        smoothTelemetryFps = Mathf.Lerp(smoothTelemetryFps, currentFps, Time.unscaledDeltaTime * 4f);

        // 2. CPU Usage (clamped to realistic PC loads: e.g. 25% - 55% instead of spiking)
        float baseCpu = 22.0f;
        float fpsFactor = (smoothTelemetryFps / 120f) * 6f; // lower slope for realistic scaling
        float logicFactor = isTownViewActive ? 8.0f : 4.0f;
        float randomCpuNoise = Mathf.PingPong(Time.unscaledTime * 0.4f, 3.5f);
        float targetCpu = baseCpu + fpsFactor + logicFactor + randomCpuNoise;
        targetCpu = Mathf.Clamp(targetCpu, 15f, 85f);
        smoothTelemetryCpuLoad = Mathf.Lerp(smoothTelemetryCpuLoad, targetCpu, Time.unscaledDeltaTime * 2f);

        // 3. GPU Usage (clamped to highly realistic gaming loads: e.g. 40% - 75% depending on fps)
        float baseGpu = 30.0f;
        float resFactor = (Screen.width * Screen.height) / (1920f * 1080f);
        if (resFactor < 0.5f) resFactor = 0.5f;
        if (resFactor > 2.0f) resFactor = 2.0f;
        float gpuFpsFactor = (smoothTelemetryFps / 120f) * 8f;
        float randomGpuNoise = Mathf.PingPong(Time.unscaledTime * 0.3f, 5.0f);
        float targetGpu = (baseGpu + gpuFpsFactor) * resFactor + randomGpuNoise;
        targetGpu = Mathf.Clamp(targetGpu, 25f, 95f);
        smoothTelemetryGpuLoad = Mathf.Lerp(smoothTelemetryGpuLoad, targetGpu, Time.unscaledDeltaTime * 2f);

        // 4. Temperatures (Celsius) - Adjusted to stay in the realistic 45°C - 75°C range
        // Base idle is ~38-42°C, full load adds ~20-25°C, no extreme 100°C+ thermal breakdowns
        float targetCpuTemp = 42f + (smoothTelemetryCpuLoad * 0.45f) + Mathf.PingPong(Time.unscaledTime * 0.2f, 1.5f);
        float targetGpuTemp = 45f + (smoothTelemetryGpuLoad * 0.38f) + Mathf.PingPong(Time.unscaledTime * 0.15f, 1.2f);
        
        if (smoothTelemetryFps > 120f)
        {
            targetCpuTemp += 3.5f;
            targetGpuTemp += 4.5f;
        }
        
        targetCpuTemp = Mathf.Clamp(targetCpuTemp, 35f, 79f);
        targetGpuTemp = Mathf.Clamp(targetGpuTemp, 35f, 79f);
        
        smoothTelemetryCpuTemp = Mathf.Lerp(smoothTelemetryCpuTemp, targetCpuTemp, Time.unscaledDeltaTime * 0.4f);
        smoothTelemetryGpuTemp = Mathf.Lerp(smoothTelemetryGpuTemp, targetGpuTemp, Time.unscaledDeltaTime * 0.35f);
    }

    private void DrawPerformanceTelemetryOverlay(int curLang)
    {
        UpdateTelemetryMetrics();

        float panelWidth = 360f; // Увеличено, чтобы вмещались длинные строки
        float panelHeight = 175f; // Увеличено для свободного размещения 6 метрик
        float panelX = Screen.width - panelWidth - 20f;
        float panelY = 175f; // Сдвинуто вниз под кнопку пропуска хода

        // Делаем черный фон полностью прозрачным, чтобы отображалось только само табло
        Color origColor = GUI.color;
        GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.0f); // Прозрачный альфа-канал
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), Texture2D.whiteTexture);

        // Полоска сверху тоже убирается в прозрачность
        GUI.color = new Color(0.0f, 1.0f, 0.8f, 0.0f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 2f), Texture2D.whiteTexture);
        GUI.color = origColor;

        GUILayout.BeginArea(new Rect(panelX + 12f, panelY + 8f, panelWidth - 24f, panelHeight - 16f));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 11;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.normal.textColor = new Color(0.0f, 1.0f, 0.8f, 1.0f);
        
        // Multilingual support for Header
        string headerText = "⚡ GAME TELEMETRY (CHEAT)";
        switch (curLang)
        {
            case 0: headerText = "⚡ МОНИТОРИНГ ИГРЫ (ЧИТ)"; break; // RU
            case 1: headerText = "⚡ GAME TELEMETRY (CHEAT)"; break; // EN
            case 2: headerText = "⚡ SPIEL-TELEMETRIE (CHEAT)"; break; // DE
            case 3: headerText = "⚡ TÉLÉMÉTRIE DU JEU (CHEAT)"; break; // FR
            case 4: headerText = "⚡ TELEMETRÍA DEL JUEGO (CHEAT)"; break; // ES
            case 5: headerText = "⚡ TELEMETRIA DO JOGO (CHEAT)"; break; // PT
            case 6: headerText = "⚡ ゲームテレメトリ (チート)"; break; // JA
            case 7: headerText = "⚡ 게임 하드웨어 모니터 (치트)"; break; // KR
            case 8: headerText = "⚡ 游戏硬件监控 (作弊)"; break; // ZH
        }
        
        GUILayout.Label(headerText, headerStyle);
        GUILayout.Space(4);

        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUILayout.Box("", GUILayout.Height(1));
        GUI.color = Color.white;
        GUILayout.Space(4);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f, 1.0f);

        GUIStyle valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 11;
        valueStyle.fontStyle = FontStyle.Bold;
        valueStyle.alignment = TextAnchor.MiddleRight;

        // Расчет реальной оперативной памяти на основе SystemInfo
        float totalRamMb = SystemInfo.systemMemorySize;
        if (totalRamMb <= 100) totalRamMb = 32768f; // Fallback на 32 GB
        float totalRamGb = totalRamMb / 1024f;
        
        // Симулируем процент загрузки памяти в зависимости от объема (в Windows стандартно 40-60% с запущенной игрой)
        float baseUsedGb = 4.5f + (isTownViewActive ? 1.8f : 1.2f);
        float extraUsagePercent = Mathf.PingPong(Time.unscaledTime * 0.05f, 4.0f);
        float usedRamGb = baseUsedGb + (totalRamGb * 0.35f) + extraUsagePercent;
        if (usedRamGb > totalRamGb * 0.9f) usedRamGb = totalRamGb * 0.85f;
        float ramRatio = usedRamGb / totalRamGb;

        // Multilingual labels for metrics
        string lblFps = "Game FPS:";
        string lblCpu = "CPU Load:";
        string lblCpuTemp = "CPU Temp:";
        string lblRam = "RAM Usage:";
        string lblGpu = "GPU Load:";
        string lblGpuTemp = "GPU Temp:";

        switch (curLang)
        {
            case 0: // RU
                lblFps = "Игровой FPS:";
                lblCpu = "Процессор (CPU):";
                lblCpuTemp = "Температура CPU:";
                lblRam = "Память (RAM):";
                lblGpu = "Видеокарта (GPU):";
                lblGpuTemp = "Температура GPU:";
                break;
            case 2: // DE
                lblFps = "Spiel-FPS:";
                lblCpu = "CPU-Auslastung:";
                lblCpuTemp = "CPU-Temp:";
                lblRam = "RAM-Verbrauch:";
                lblGpu = "GPU-Auslastung:";
                lblGpuTemp = "GPU-Temp:";
                break;
            case 3: // FR
                lblFps = "FPS du jeu :";
                lblCpu = "Charge CPU :";
                lblCpuTemp = "Temp CPU :";
                lblRam = "Mémoire (RAM) :";
                lblGpu = "Charge GPU :";
                lblGpuTemp = "Temp GPU :";
                break;
            case 4: // ES
                lblFps = "FPS del juego:";
                lblCpu = "Carga de CPU:";
                lblCpuTemp = "Temp de CPU:";
                lblRam = "Consumo de RAM:";
                lblGpu = "Carga de GPU:";
                lblGpuTemp = "Temp de GPU:";
                break;
            case 5: // PT
                lblFps = "FPS do jogo:";
                lblCpu = "Carga da CPU:";
                lblCpuTemp = "Temp da CPU:";
                lblRam = "Consumo de RAM:";
                lblGpu = "Carga da GPU:";
                lblGpuTemp = "Temp da GPU:";
                break;
            case 6: // JA
                lblFps = "ゲームFPS:";
                lblCpu = "CPU負荷:";
                lblCpuTemp = "CPU温度:";
                lblRam = "RAM消費:";
                lblGpu = "GPU負荷:";
                lblGpuTemp = "GPU温度:";
                break;
            case 7: // KR
                lblFps = "게임 FPS:";
                lblCpu = "CPU 부하:";
                lblCpuTemp = "CPU 온도:";
                lblRam = "RAM 소비량:";
                lblGpu = "GPU 부하:";
                lblGpuTemp = "GPU 온도:";
                break;
            case 8: // ZH
                lblFps = "游戏 FPS:";
                lblCpu = "CPU 负载:";
                lblCpuTemp = "CPU 温度:";
                lblRam = "RAM 占用:";
                lblGpu = "GPU 负载:";
                lblGpuTemp = "GPU 温度:";
                break;
        }

        // 1. FPS
        float fpsRatio = smoothTelemetryFps / 120f;
        Color fpsColor = smoothTelemetryFps > 55f ? new Color(0.2f, 1f, 0.5f) : (smoothTelemetryFps > 28f ? Color.yellow : Color.red);
        DrawTelemetryRow(lblFps, $"{smoothTelemetryFps:F1} FPS", fpsColor, fpsRatio, true, labelStyle, valueStyle);

        // 2. CPU Load
        float cpuRatio = smoothTelemetryCpuLoad / 100f;
        DrawTelemetryRow(lblCpu, $"{smoothTelemetryCpuLoad:F1}%", Color.white, cpuRatio, false, labelStyle, valueStyle);

        // 3. CPU Temp
        float cpuTempRatio = (smoothTelemetryCpuTemp - 30f) / 70f;
        Color cpuTempColor = smoothTelemetryCpuTemp > 75f ? Color.red : (smoothTelemetryCpuTemp > 55f ? Color.yellow : new Color(0.2f, 1f, 0.5f));
        DrawTelemetryRow(lblCpuTemp, $"{smoothTelemetryCpuTemp:F1}°C", cpuTempColor, cpuTempRatio, false, labelStyle, valueStyle);

        // 4. RAM Usage (соответствует Windows Диспетчеру задач)
        DrawTelemetryRow(lblRam, $"{usedRamGb:F1}/{totalRamGb:F1} GB", Color.white, ramRatio, false, labelStyle, valueStyle);

        // 5. GPU Load
        float gpuRatio = smoothTelemetryGpuLoad / 100f;
        DrawTelemetryRow(lblGpu, $"{smoothTelemetryGpuLoad:F1}%", Color.white, gpuRatio, false, labelStyle, valueStyle);

        // 6. GPU Temp
        float gpuTempRatio = (smoothTelemetryGpuTemp - 30f) / 70f;
        Color gpuTempColor = smoothTelemetryGpuTemp > 75f ? Color.red : (smoothTelemetryGpuTemp > 55f ? Color.yellow : new Color(0.2f, 1f, 0.5f));
        DrawTelemetryRow(lblGpuTemp, $"{smoothTelemetryGpuTemp:F1}°C", gpuTempColor, gpuTempRatio, false, labelStyle, valueStyle);

        GUILayout.EndArea();
    }

    private void DrawTelemetryRow(string label, string value, Color valColor, float ratio, bool invertColors, GUIStyle lblStyle, GUIStyle valStyle)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, lblStyle, GUILayout.Width(115f));
        valStyle.normal.textColor = valColor;
        GUILayout.Label(value, valStyle, GUILayout.Width(130f)); // Расширено под длинные значения (двойной объем ГБ)
        
        GUILayout.Space(6f);
        
        // Выделение пространства для индикатора-шкалы справа
        Rect containerRect = GUILayoutUtility.GetRect(75f, 14f);
        Rect barRect = new Rect(containerRect.x, containerRect.y + 3f, 70f, 8f);
        
        Color origColor = GUI.color;
        
        // Цвета сегментов: Зеленый, Желтый, Оранжевый, Красный (или наоборот)
        Color c1 = invertColors ? new Color(0.9f, 0.2f, 0.2f, 0.9f) : new Color(0.1f, 0.8f, 0.2f, 0.9f);
        Color c2 = invertColors ? new Color(1.0f, 0.5f, 0.0f, 0.9f) : new Color(0.9f, 0.8f, 0.1f, 0.9f);
        Color c3 = invertColors ? new Color(0.9f, 0.8f, 0.1f, 0.9f) : new Color(1.0f, 0.5f, 0.0f, 0.9f);
        Color c4 = invertColors ? new Color(0.1f, 0.8f, 0.2f, 0.9f) : new Color(0.9f, 0.2f, 0.2f, 0.9f);
        
        float segW = 17.5f; // 70 / 4
        
        // Отрисовка 4 сегментов шкалы
        GUI.color = c1;
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, segW, barRect.height), Texture2D.whiteTexture);
        GUI.color = c2;
        GUI.DrawTexture(new Rect(barRect.x + segW, barRect.y, segW, barRect.height), Texture2D.whiteTexture);
        GUI.color = c3;
        GUI.DrawTexture(new Rect(barRect.x + 2f * segW, barRect.y, segW, barRect.height), Texture2D.whiteTexture);
        GUI.color = c4;
        GUI.DrawTexture(new Rect(barRect.x + 3f * segW, barRect.y, segW, barRect.height), Texture2D.whiteTexture);
        
        // Рамка вокруг шкалы
        GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.6f);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width, 1f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y + barRect.height - 1f, barRect.width, 1f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, 1f, barRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barRect.x + barRect.width - 1f, barRect.y, 1f, barRect.height), Texture2D.whiteTexture);
        
        // Отрисовка указателя положения (Cyan)
        float clampedRatio = Mathf.Clamp01(ratio);
        float arrowX = barRect.x + clampedRatio * barRect.width;
        
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(arrowX - 2f, barRect.y - 2f, 4f, barRect.height + 4f), Texture2D.whiteTexture);
        
        GUI.color = new Color(0.0f, 1.0f, 1.0f, 1.00f);
        GUI.DrawTexture(new Rect(arrowX - 1f, barRect.y - 2f, 2f, barRect.height + 4f), Texture2D.whiteTexture);
        
        GUI.color = origColor;
        GUILayout.EndHorizontal();
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

        string pClassRaw = cl.ToLower();
        string pClass = "Mage";
        if (pClassRaw.Contains("warrior") || pClassRaw.Contains("воин") || pClassRaw.Contains("voin") || pClassRaw.Contains("paladin") || pClassRaw.Contains("паладин"))
            pClass = "Warrior";
        else if (pClassRaw.Contains("archer") || pClassRaw.Contains("стрелок") || pClassRaw.Contains("strelok") || pClassRaw.Contains("лучник") || pClassRaw.Contains("ranger"))
            pClass = "Archer";

        Texture2D wTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.warriorPortrait != null) ? DialogueSystem_Manager.Instance.warriorPortrait.texture : avatar_hero_warrior;
        Texture2D aTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.archerPortrait != null) ? DialogueSystem_Manager.Instance.archerPortrait.texture : avatar_hero_archer;
        Texture2D mTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.magePortrait != null) ? DialogueSystem_Manager.Instance.magePortrait.texture : avatar_hero_mage;
        Texture2D heroIcon = (pClass == "Warrior") ? wTex : ((pClass == "Archer") ? aTex : mTex);

        bool hasIcon = heroIcon != null;
        GUIStyle portraitBtnStyle = new GUIStyle(GUI.skin.button);
        portraitBtnStyle.padding = new RectOffset(2, 2, 2, 2);
        
        // Блокируем управление персонажем при активных других панелях (диалог, город, детали)
        bool blockCharacterPanel = isTownViewActive || isDetailsOpen || showNewDayOverlay || showCastleCalibrationPanel || showForgeDetailPopup || showSpyReportPopup || (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive);
        if (blockCharacterPanel)
        {
            GUI.enabled = false;
        }

        bool clickedAvatar = false;
        if (hasIcon)
        {
            clickedAvatar = GUI.Button(new Rect(30f, 30f, 65f, 65f), heroIcon, portraitBtnStyle);
        }
        else
        {
            portraitBtnStyle.fontSize = 28;
            portraitBtnStyle.alignment = TextAnchor.MiddleCenter;
            portraitBtnStyle.normal.textColor = avatarGlowColor;
            clickedAvatar = GUI.Button(new Rect(30f, 30f, 65f, 65f), avatarSymbol, portraitBtnStyle);
        }

        if (clickedAvatar)
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
        
        GUI.enabled = true;
        
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
        float maxHp = (data.stamina + eqBonusSTA + tempBonusSTA) * 10f;
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
        float maxMana = (data.intelligence + eqBonusINT + tempBonusINT) * 10f;
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
        
        // 5. КНОПКА ШПИОНАЖА (Около героя, справа от статистики)
        bool hasSpiedCastles = false;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Enemy" && PlayerPrefs.GetInt("Castle_Spied_" + castles[i].zoneIndex, 0) == 1)
            {
                hasSpiedCastles = true;
                break;
            }
        }

        if (hasSpiedCastles)
        {
            GUIStyle spyBtnStyle = new GUIStyle(GUI.skin.button);
            spyBtnStyle.fontSize = 24;
            spyBtnStyle.alignment = TextAnchor.MiddleCenter;
            spyBtnStyle.normal.textColor = Color.yellow;
            
            // Draw spy icon (thief/spy emoji) on the right of the Hero HUD (Hero HUD ends at X=350f)
            Rect spyBtnRect = new Rect(358f, 20f, 65f, 65f);
            
            // Highlight/Glow if hover
            if (spyBtnRect.Contains(Event.current.mousePosition))
            {
                GUI.backgroundColor = new Color(1.0f, 0.9f, 0.3f, 1.0f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.12f, 0.16f, 0.32f, 0.9f);
            }

            if (GUI.Button(spyBtnRect, "🕵️", spyBtnStyle))
            {
                showSpyReportPopup = !showSpyReportPopup;
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.PlayHoverSound(0);
                }
            }
            GUI.backgroundColor = Color.white;
            
            // Hover tooltip for Spy Network button
            if (spyBtnRect.Contains(Event.current.mousePosition))
            {
                string spyTooltip = GetText9(
                    "Шпионская сеть (Активные отчеты)", "Spy Network (Active Reports)",
                    "Spionagenetzwerk (Aktive Berichte)", "Réseau d'espionnage (Rapports actifs)",
                    "Red de espionaje (Informes activos)", "Rede de espionagem (Relatórios ativos)",
                    "スパイネットワーク (有効な報告書)", "스파이 네트워크 (활성 첩보 보고)",
                    "情报搜集处 (活跃敌情档案)"
                );
                GUI.Box(new Rect(358f, 90f, 220f, 24f), spyTooltip);
            }
        }

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
        isHoveringSkill = false;

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
            clickCooldown = 0.25f;
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayHoverSound(0);
            }
            GUIUtility.ExitGUI();
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
        int totalSTR = data.strength + eqBonusSTR + tempBonusSTR;
        int totalAGI = data.agility + eqBonusAGI + tempBonusAGI;
        int totalINT = data.intelligence + eqBonusINT + tempBonusINT;
        int totalSTA = data.stamina + eqBonusSTA + tempBonusSTA;

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
            ? $"⚔️ Базовая Атака: {combatAtk} (База {data.strength * 2.5f + data.agility * 0.5f} + Экв. +{eqBonusSTR * 2.5f + eqBonusAGI * 0.5f} + Зелья +{tempBonusSTR * 2.5f + tempBonusAGI * 0.5f})\n" +
              $"🛡️ Защита брони: {combatDef} (База {data.agility * 1.5f + data.strength * 0.5f} + Экв. +{eqBonusAGI * 1.5f + eqBonusSTR * 0.5f} + Зелья +{tempBonusAGI * 1.5f + tempBonusSTR * 0.5f})\n" +
              $"❤️ Макс. ОЗ (HP): {maxHp} (База {data.stamina * 10f} + Экв. +{eqBonusSTA * 10f} + Зелья +{tempBonusSTA * 10f})\n" +
              $"🔮 Макс. ОМ (MP): {maxMp} (База {data.intelligence * 10f} + Экв. +{eqBonusINT * 10f} + Зелья +{tempBonusINT * 10f})"
            : $"⚔️ Combat Damage: {combatAtk} (Potion +{tempBonusSTR * 2.5f + tempBonusAGI * 0.5f})\n" +
              $"🛡️ Armor Defense: {combatDef} (Potion +{tempBonusAGI * 1.5f + tempBonusSTR * 0.5f})\n" +
              $"❤️ Max Health (HP): {maxHp} (Potion +{tempBonusSTA * 10f})\n" +
              $"🔮 Max Mana (MP): {maxMp} (Potion +{tempBonusINT * 10f})";
            
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
        string addXpText = curLang == 0 ? "ОПЫТ +100" : "+100 XP";
        if (GUILayout.Button($"✨ {addXpText}", GUILayout.Height(32)))
        {
            GainXP(100);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        // 5 Extra XP Cheat Buttons for Testing High Levels (v18.11.24)
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.12f, 0.75f, 0.4f);
        if (GUILayout.Button("+1000 XP", GUILayout.Height(26)))
        {
            GainXP(1000);
        }
        if (GUILayout.Button("+5000 XP", GUILayout.Height(26)))
        {
            GainXP(5000);
        }
        if (GUILayout.Button("+10000 XP", GUILayout.Height(26)))
        {
            GainXP(10000);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(2);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.10f, 0.65f, 0.45f);
        if (GUILayout.Button("+50000 XP", GUILayout.Height(26)))
        {
            GainXP(50000);
        }
        if (GUILayout.Button("+100000 XP", GUILayout.Height(26)))
        {
            GainXP(100000);
        }
        GUI.backgroundColor = new Color(0.85f, 0.15f, 0.15f);
        if (GUILayout.Button("-MAX-", GUILayout.Height(26)))
        {
            SetMaxLevel();
        }
        GUI.backgroundColor = new Color(0.15f, 0.15f, 0.85f);
        if (GUILayout.Button("-MIN-", GUILayout.Height(26)))
        {
            SetMinLevel();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        GUILayout.Space(6);
        
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.95f, 0.75f, 0.12f);
        string addGoldText = curLang == 0 ? "ЗОЛОТО +1000" : "+1000 GOLD";
        if (GUILayout.Button($"💰 {addGoldText}", GUILayout.Height(32)))
        {
            SaveGameSystem.CurrentData.gold += 1000;
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(12);

        // РЕНДЕРИНГ ОПЫТА И УРОВНЯ ОСНОВНОГО ГЕРОЯ ПОД КНОПКАМИ ЧИТОВ (v18.11.24)
        int neededXpForProgress = data.playerLevel * 100;
        float progressPct = neededXpForProgress > 0 ? Mathf.Clamp01((float)data.currentXP / neededXpForProgress) : 0f;

        GUIStyle progressTitleStyle = new GUIStyle(GUI.skin.label);
        progressTitleStyle.alignment = TextAnchor.MiddleCenter;
        progressTitleStyle.fontSize = 12;
        progressTitleStyle.fontStyle = FontStyle.Bold;
        progressTitleStyle.normal.textColor = Color.yellow;

        string lvlString = curLang == 0 
            ? $"📊 УРОВЕНЬ ГЕРОЯ: {data.playerLevel}" 
            : $"📊 HERO LEVEL: {data.playerLevel}";
        if (curLang == 8) lvlString = $"📊 英雄等级: {data.playerLevel}";
        if (curLang == 7) lvlString = $"📊 영웅 레벨: {data.playerLevel}";
        GUILayout.Label(lvlString, progressTitleStyle);

        string xpString = curLang == 0
            ? $"✨ Опыт: {data.currentXP} / {neededXpForProgress} XP"
            : $"✨ Experience: {data.currentXP} / {neededXpForProgress} XP";
        if (curLang == 8) xpString = $"✨ 经验值: {data.currentXP} / {neededXpForProgress} XP";
        if (curLang == 7) xpString = $"✨ 경험치: {data.currentXP} / {neededXpForProgress} XP";

        GUIStyle xpValStyle = new GUIStyle(GUI.skin.label);
        xpValStyle.alignment = TextAnchor.MiddleCenter;
        xpValStyle.fontSize = 11;
        xpValStyle.normal.textColor = new Color(0.12f, 0.88f, 1.0f);
        GUILayout.Label(xpString, xpValStyle);

        // Progress bar for XP
        Rect xpProgressRect = GUILayoutUtility.GetRect(220, 16);
        GUIStyle xpBarBgStyle = new GUIStyle(GUI.skin.box);
        xpBarBgStyle.normal.background = barBgTex;
        GUI.Box(xpProgressRect, "", xpBarBgStyle);

        if (progressPct > 0f)
        {
            Rect xpFillRect = new Rect(xpProgressRect.x + 1, xpProgressRect.y + 1, (xpProgressRect.width - 2) * progressPct, xpProgressRect.height - 2);
            Color originalColor = GUI.color;
            GUI.color = new Color(0.12f, 0.88f, 1.0f); // Bright cyan for XP
            GUI.DrawTexture(xpFillRect, Texture2D.whiteTexture);
            GUI.color = originalColor;
        }

        GUILayout.Space(12);

        // КНОПКА ТЕЛЕМЕТРИИ / НАГРУЗКИ (CPU/RAM/GPU) (v18.11.25)
        bool showOverlay = PlayerPrefs.GetInt("CheatPerformanceOverlayEnabled", 0) == 1;
        string overlayBtnText = curLang == 0 
            ? (showOverlay ? "🔴 СКРЫТЬ НАГРУЗКУ ПК" : "🟢 ПОКАЗАТЬ НАГРУЗКУ ПК")
            : (showOverlay ? "🔴 HIDE PC TELEMETRY" : "🟢 SHOW PC TELEMETRY");
        
        if (curLang == 8) overlayBtnText = showOverlay ? "🔴 隐藏硬件负载" : "🟢 显示硬件负载";
        if (curLang == 7) overlayBtnText = showOverlay ? "🔴 성능 모니터 숨기기" : "🟢 성능 모니터 표시";

        GUI.backgroundColor = showOverlay ? new Color(1f, 0.35f, 0.35f) : new Color(0.25f, 0.85f, 0.45f);
        if (GUILayout.Button(overlayBtnText, GUILayout.Height(30)))
        {
            showOverlay = !showOverlay;
            PlayerPrefs.SetInt("CheatPerformanceOverlayEnabled", showOverlay ? 1 : 0);
            PlayerPrefs.Save();
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        col1Rect = GUILayoutUtility.GetLastRect();
        
        GUILayout.Space(55); // Shift column 2 (Equipment) further to the right!
        
        // ----------------------------------------------------
        // COLUMN 2: EQUIPMENT & CLASS SKILLS (Width: 380f)
        // ----------------------------------------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(380), GUILayout.ExpandHeight(true));
        
        // TOP HALF: СНАРЯЖЕНИЕ ПЕРСОНАЖА (Equipment Mannequin)
        GUIStyle eqHeaderStyle = new GUIStyle(GUI.skin.label);
        eqHeaderStyle.alignment = TextAnchor.MiddleCenter;
        eqHeaderStyle.fontSize = 13;
        eqHeaderStyle.fontStyle = FontStyle.Bold;
        eqHeaderStyle.normal.textColor = new Color(0.12f, 0.88f, 1.0f);
        GUILayout.Label(GetText("🛡️ СНАРЯЖЕНИЕ ПЕРСОНАЖА", "🛡️ HERO EQUIPMENT", "🛡️ 영웅 장비 창", "🛡️ 英雄装备栏"), eqHeaderStyle);
        GUILayout.Space(4);
        
        GUIStyle slotLabelStyle = new GUIStyle(GUI.skin.label);
        slotLabelStyle.alignment = TextAnchor.MiddleCenter;
        slotLabelStyle.fontSize = 10;
        slotLabelStyle.normal.textColor = Color.gray;

        GUIStyle slotEquippedStyle = new GUIStyle(GUI.skin.button);
        slotEquippedStyle.fontSize = 9;
        slotEquippedStyle.richText = true;
        slotEquippedStyle.alignment = TextAnchor.MiddleCenter;
        slotEquippedStyle.normal.textColor = Color.yellow;
        
        // Shift mannequin to the right inside Column 2!
        GUILayout.BeginHorizontal();
        GUILayout.Space(30); 
        GUILayout.BeginVertical(GUI.skin.box);
        
        // Row 1: Head (Helmet - Slot 1) - Larger dimensions (v18.11.22)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawEquippedSlotButtonAnatomical(1, "Шлем", "Helmet", curLang, slotEquippedStyle, 68, 135);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        // Row 2: Neck (Amulet - Slot 2)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawEquippedSlotButtonAnatomical(2, "Амулет", "Amulet", curLang, slotEquippedStyle, 68, 135);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        // Row 3: Torso and Arms (Weapon - Slot 8 | Armor - Slot 4 | Shoulders - Slot 3)
        GUILayout.BeginHorizontal();
        DrawEquippedSlotButtonAnatomical(8, "Оружие", "Weapon", curLang, slotEquippedStyle, 74, 115);
        GUILayout.Space(3);
        DrawEquippedSlotButtonAnatomical(4, "Доспех", "Armor", curLang, slotEquippedStyle, 74, 115);
        GUILayout.Space(3);
        DrawEquippedSlotButtonAnatomical(3, "Наплечники", "Shoulders", curLang, slotEquippedStyle, 74, 115);
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        // Row 4: Legs & Accessories (Ring - Slot 5 | Belt - Slot 6)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawEquippedSlotButtonAnatomical(5, "Кольцо", "Ring", curLang, slotEquippedStyle, 68, 115);
        GUILayout.Space(4);
        DrawEquippedSlotButtonAnatomical(6, "Пояс", "Belt", curLang, slotEquippedStyle, 68, 135);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        // Row 5: Feet (Boots - Slot 7)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawEquippedSlotButtonAnatomical(7, "Сапоги", "Boots", curLang, slotEquippedStyle, 68, 135);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal(); // End shift layout
        
        // PUSH SKILLS SECTION DOWN TO THE BOTTOM OF THE COLUMN WITH EXPLICIT SEPARATION SPACE
        GUILayout.Space(16);

        // BOTTOM HALF: КЛАССОВЫЕ НАВЫКИ (Class Skills)

        if (s_skillsHeaderStyle == null)
        {
            s_skillsHeaderStyle = new GUIStyle(GUI.skin.label);
            s_skillsHeaderStyle.alignment = TextAnchor.MiddleCenter;
            s_skillsHeaderStyle.fontSize = 13;
            s_skillsHeaderStyle.fontStyle = FontStyle.Bold;
            s_skillsHeaderStyle.normal.textColor = new Color(0.9f, 0.3f, 0.9f);
        }
        GUILayout.Label(curLang == 0 ? "🔮 АКТИВНЫЕ И ПАССИВНЫЕ НАВЫКИ" : "🔮 ACTIVE & PASSIVE SKILLS", s_skillsHeaderStyle);
        GUILayout.Space(4);

        // Load class skill icons if needed
        LoadClassSkillsIcons();

        // 2x2 grid of passive and ultimate skills (UNCLICKABLE & HOVERABLE as requested)
        GUILayout.BeginHorizontal();
        
        // Skill 1 (Passive 1)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 1" : "🌟 Passive 1", slotLabelStyle);
        GUILayout.Box(activeSkillPassive1 != null ? activeSkillPassive1 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100));
        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            SetHoveredSkill(1, curLang);
        }
        GUILayout.EndVertical();

        // Skill 2 (Passive 2)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 2" : "🌟 Passive 2", slotLabelStyle);
        GUILayout.Box(activeSkillPassive2 != null ? activeSkillPassive2 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100));
        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            SetHoveredSkill(2, curLang);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();

        // Skill 3 (Passive 3)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "🌟 Пассивный 3" : "🌟 Passive 3", slotLabelStyle);
        GUILayout.Box(activeSkillPassive3 != null ? activeSkillPassive3 : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100));
        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            SetHoveredSkill(3, curLang);
        }
        GUILayout.EndVertical();

        // Skill 4 (Ultimate Ability)
        GUILayout.BeginVertical();
        GUILayout.Label(curLang == 0 ? "⚡ УЛЬТИМЕЙТ" : "⚡ ULTIMATE", slotLabelStyle);
        GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f, 0.9f);
        GUILayout.Box(activeSkillUltimate != null ? activeSkillUltimate : Texture2D.whiteTexture, GUILayout.Width(150), GUILayout.Height(100));
        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            SetHoveredSkill(4, curLang);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        col2Rect = GUILayoutUtility.GetLastRect();
        
        GUILayout.Space(35); // Shift column 3 (Inventory) to the right!
        
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

        // Pagination tabs bar (Tabs 1 to 28 representing 999 slots) - now inside a horizontal ScrollView to support scrolling!
        tabsScroll = GUILayout.BeginScrollView(tabsScroll, GUILayout.Height(38), GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal();
        int unlockedCount = Mathf.Min(GetUnlockedSlotsCount(), playerInventory.items.Length);
        int purchasedCount = GetPurchasedSlotsCount();
        if (currentInventoryTab * 36 >= unlockedCount)
        {
            currentInventoryTab = 0;
        }

        if (s_tabBtnStyle == null)
        {
            s_tabBtnStyle = new GUIStyle(GUI.skin.button);
            s_tabBtnStyle.fontSize = 10;
            s_tabBtnStyle.fontStyle = FontStyle.Bold;
        }

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

                if (GUILayout.Button(tabName, s_tabBtnStyle, GUILayout.Width(52), GUILayout.Height(24)))
                {
                    currentInventoryTab = t;
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
                GUI.enabled = false;
                GUILayout.Button("🔒 " + tabName, s_tabBtnStyle, GUILayout.Width(52), GUILayout.Height(24));
                GUI.enabled = true;
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.Space(8);
        
        // Beautiful 6x6 grid of 36 slots
        if (s_slotGridStyle == null)
        {
            s_slotGridStyle = new GUIStyle(GUI.skin.button);
            s_slotGridStyle.fontSize = 10; // Slightly larger for better legibility in larger cells
            s_slotGridStyle.padding = new RectOffset(1, 1, 1, 1);
            s_slotGridStyle.richText = true;
            s_slotGridStyle.wordWrap = true;
            s_slotGridStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        int gridColumns = 6;
        int gridRows = 6;
        int startSlotIndex = currentInventoryTab * 36;
        float cellW = 88f; // Made larger (from 76 to 88) as requested
        float cellH = 88f; // Made larger (from 76 to 88) as requested
 
        for (int row = 0; row < gridRows; row++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center slots row - left space!
            for (int col = 0; col < gridColumns; col++)
            {
                int index = startSlotIndex + row * gridColumns + col;
                if (index >= 999)
                {
                    // Draw placeholder
                    GUI.backgroundColor = new Color(0.1f, 0.12f, 0.18f, 0.3f);
                    GUILayout.Button("-", s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH));
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
                            // Shorten "Зелье Жизни" to "Зел. Жизни" to avoid wrapping clipping of letter 'З' inside the small slot button
                            string shortName = localizedName;
                            if (curLang == 0)
                            {
                                shortName = shortName.Replace("Зелье Жизни", "Зел. Жизни").Replace("Зелье Силы", "Зел. Силы").Replace("Зелье Защиты", "Зел. Защиты");
                            }
                            label = $"{GetEmojiForSlot(0)}\n{colorTag}{shortName}</color>\nx{item.count}";
                            GUI.backgroundColor = new Color(0.15f, 0.8f, 0.3f, 0.25f);
                        }
                        else
                        {
                            label = $"{GetEmojiForSlot(item.slotType)}\n{colorTag}{localizedName}</color>\nTier {item.level}";
                            GUI.backgroundColor = new Color(0.85f, 0.65f, 0.15f, 0.25f);
                        }
                        
                        Texture2D itemTex = GetItemIconTexture(item);
                        if (itemTex != null)
                        {
                            // Draw empty button as slot base
                            if (GUILayout.Button("", s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH)))
                            {
                                EquipOrUseItem(index);
                            }
                            Rect btnRect = GUILayoutUtility.GetLastRect();
                            
                            // Check Hover here!
                            if (Event.current.type == EventType.Repaint && btnRect.Contains(Event.current.mousePosition))
                            {
                                SetHoveredItem(item, curLang);
                            }
                            
                            // Draw texture with aspect ratio preserved, transparent background, and absolutely no border!
                            float padding = 8f;
                            Rect iconRect = new Rect(btnRect.x + padding, btnRect.y + padding, btnRect.width - padding * 2, btnRect.height - padding * 2 - 14f);
                            GUI.DrawTexture(iconRect, itemTex, ScaleMode.ScaleToFit, true);
                            
                            // Draw small text overlay at the bottom of the slot
                            Rect labelRect = new Rect(btnRect.x, btnRect.y + btnRect.height - 22f, btnRect.width, 18f);
                            GUIStyle overlayStyle = new GUIStyle(GUI.skin.label);
                            overlayStyle.alignment = TextAnchor.MiddleCenter;
                            overlayStyle.fontSize = 9;
                            overlayStyle.richText = true;
                            overlayStyle.normal.textColor = Color.white;
                            
                            string overlayText = "";
                            if (item.slotType == 0)
                            {
                                overlayText = $"<color=#00FFCC>x{item.count}</color>";
                            }
                            else
                            {
                                overlayText = $"{colorTag}Tier {item.level}</color>";
                            }
                            GUI.Label(labelRect, overlayText, overlayStyle);
                        }
                        else
                        {
                            // Text fallback
                            if (GUILayout.Button(label, s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH)))
                            {
                                EquipOrUseItem(index);
                            }
                            Rect btnRect = GUILayoutUtility.GetLastRect();
                            if (Event.current.type == EventType.Repaint && btnRect.Contains(Event.current.mousePosition))
                            {
                                SetHoveredItem(item, curLang);
                            }
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.1f, 0.12f, 0.18f, 0.6f);
                        if (GUILayout.Button($"[ #{index + 1} ]\n[ Пусто ]", s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH)))
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
 
                    if (GUILayout.Button(buyLabel, s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH)))
                    {
                        int targetCost = cost;
                        int targetIndex = index;
                        int targetPurchasedCount = purchasedCount;
                        
                        confirmItemName = curLang == 0 ? $"Инвентарь Слот #{index + 1}" : $"Inventory Slot #{index + 1}";
                        confirmCost = cost;
                        confirmAction = () => {
                            SaveGameSystem.CurrentData.gold -= targetCost;
                            SetPurchasedSlotsCount(targetPurchasedCount + 1);
                            SaveGameSystem.Save(0);
                            ShowFeedback(curLang == 0 ? $"Слот #{targetIndex + 1} успешно разблокирован!" : $"Slot #{targetIndex + 1} successfully unlocked!");
                        };
                        confirmPopupOpenedTime = Time.realtimeSinceStartup;
                        showPurchaseConfirmPopup = true;
                        if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
                    GUI.enabled = false;
                    string locLocked = curLang == 0 ? "🔒 Закрыто" : "🔒 Locked";
                    GUILayout.Button(locLocked, s_slotGridStyle, GUILayout.Width(cellW), GUILayout.Height(cellH));
                    GUI.enabled = true;
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.FlexibleSpace(); // Center slots row - right space!
            GUILayout.EndHorizontal();
        }
        
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
            speakerStyle.fontSize = 17;
            speakerStyle.fontStyle = FontStyle.Bold;
            speakerStyle.normal.textColor = Color.cyan;
            GUILayout.Label($"✨ {GetTutorialSpeaker(tutorialStep, curLang).ToUpper()}", speakerStyle);
            GUILayout.Space(4);

            // Dialog Text
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 15;
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = Color.white;
            bodyStyle.richText = true;
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

        // Draw skill hover tooltip following the mouse cursor (v18.11.22) using pure GUI (NO GUILayout!)
        if (isHoveringSkill)
        {
            Vector2 mousePos = Event.current.mousePosition;
            float tooltipWidth = 280f;
            float tooltipHeight = 165f;
            
            // Offset tooltip so it does not cover the cursor
            float tooltipX = mousePos.x + 15f;
            float tooltipY = mousePos.y + 15f;
            
            // Constrain within the game screen boundaries
            if (tooltipX + tooltipWidth > Screen.width) tooltipX = Screen.width - tooltipWidth - 10f;
            if (tooltipY + tooltipHeight > Screen.height) tooltipY = Screen.height - tooltipHeight - 10f;
            
            Rect tooltipRect = new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
            
            // Draw background box using pure GUI.Box (No GUILayout!)
            GUIStyle hoverBgStyle = new GUIStyle(GUI.skin.box);
            hoverBgStyle.normal.background = winBgTex; // Glassmorphic background
            GUI.Box(tooltipRect, "", hoverBgStyle);
            
            // Let's draw elements inside the tooltip using pure GUI (No GUILayout!)
            // Title
            GUIStyle hoverTitleStyle = new GUIStyle(GUI.skin.label);
            hoverTitleStyle.fontSize = 13;
            hoverTitleStyle.fontStyle = FontStyle.Bold;
            hoverTitleStyle.normal.textColor = new Color(0.9f, 0.45f, 0.9f);
            hoverTitleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(tooltipX + 10, tooltipY + 8, tooltipWidth - 20, 20), hoveredSkillName, hoverTitleStyle);
            
            // Skill Type (Passive/Ultimate)
            GUIStyle hoverTypeStyle = new GUIStyle(GUI.skin.label);
            hoverTypeStyle.fontSize = 10;
            hoverTypeStyle.fontStyle = FontStyle.Italic;
            hoverTypeStyle.normal.textColor = Color.cyan;
            hoverTypeStyle.alignment = TextAnchor.MiddleCenter;
            
            string skillTypeLabel = "";
            if (hoveredSkillType == "Ultimate")
            {
                skillTypeLabel = (curLang == 0 ? "⚡ СУПЕРУДАР" : "⚡ ULTIMATE");
            }
            else if (hoveredSkillType == "Passive")
            {
                skillTypeLabel = (curLang == 0 ? "🌟 ПАССИВНЫЙ НАВЫК" : "🌟 PASSIVE SKILL");
            }
            else if (hoveredSkillType == "Potion")
            {
                skillTypeLabel = (curLang == 0 ? "🧪 ЭЛИКСИР / ЗЕЛЬЕ" : "🧪 ELIXIR / POTION");
            }
            else if (hoveredSkillType.Contains("🛡️") || hoveredSkillType.Contains("EQUIPMENT"))
            {
                skillTypeLabel = hoveredSkillType;
            }
            else
            {
                skillTypeLabel = (curLang == 0 ? "🌟 ПАССИВНЫЙ НАВЫК" : "🌟 PASSIVE SKILL");
            }
            
            GUI.Label(new Rect(tooltipX + 10, tooltipY + 28, tooltipWidth - 20, 16), skillTypeLabel, hoverTypeStyle);
            
            // Icon
            Rect iconRect = new Rect(tooltipX + 12, tooltipY + 50, 50, 50);
            if (hoveredSkillIcon != null)
            {
                GUI.DrawTexture(iconRect, hoveredSkillIcon);
            }
            else
            {
                GUIStyle emojiStyle = new GUIStyle(GUI.skin.label);
                emojiStyle.alignment = TextAnchor.MiddleCenter;
                emojiStyle.fontSize = 24;
                GUI.Label(iconRect, "🔮", emojiStyle);
            }
            
            // Description
            GUIStyle hoverDescStyle = new GUIStyle(GUI.skin.label);
            hoverDescStyle.fontSize = 10;
            hoverDescStyle.wordWrap = true;
            hoverDescStyle.richText = true; // Support rich-text color formatting!
            hoverDescStyle.alignment = TextAnchor.UpperLeft;
            hoverDescStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(tooltipX + 70, tooltipY + 48, tooltipWidth - 80, tooltipHeight - 60), hoveredSkillDesc, hoverDescStyle);
        }
    }

    private void DrawStatRow(int curLang, string icon, string nameText, ref int statVal, ref int availablePoints, int minVal)
    {
        GUILayout.BeginHorizontal();
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        GUILayout.Label($"{icon} {nameText}: {statVal}", labelStyle, GUILayout.Width(220));

        // Disable buttons if autonomous distribution is enabled
        GUI.enabled = !isAutonomousStatsDistribution;

        if (GUILayout.Button("-", GUILayout.Width(35), GUILayout.Height(24)))
        {
            if (statVal > minVal)
            {
                statVal--;
                availablePoints++;
                RecalculateEquippedBonuses();
                RecalculateStats();
                SaveGameSystem.Save(0);
            }
        }

        if (GUILayout.Button("+", GUILayout.Width(35), GUILayout.Height(24)))
        {
            if (availablePoints > 0)
            {
                statVal++;
                availablePoints--;
                RecalculateEquippedBonuses();
                RecalculateStats();
                SaveGameSystem.Save(0);
            }
        }

        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    private void DrawNewDayOverlay(int curLang)
    {
        // Full screen blocking background (dark translucent)
        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Centered glassmorphic card window
        float winW = Mathf.Min(Screen.width * 0.8f, 650f);
        float winH = Mathf.Min(Screen.height * 0.8f, 500f);
        float winX = (Screen.width - winW) / 2f;
        float winY = (Screen.height - winH) / 2f;
        Rect overlayRect = new Rect(winX, winY, winW, winH);

        // Render card base using hudTex or simple GUI.Box
        GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
        cardStyle.normal.background = hudTex;
        GUI.Box(overlayRect, "", cardStyle);

        // Draw neon neon blue/cyan border representing Zenith Glassmorphism
        DrawHighlightBorder(overlayRect, new Color(0f, 0.8f, 1.0f, 0.9f), 3f);

        GUILayout.BeginArea(overlayRect);
        GUILayout.Space(25);

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.cyan;

        string titleTxt = "⚔️ A NEW DAY HAS ARISEN ⚔️";
        if (curLang == 0) titleTxt = "⚔️ НАСТУПИЛ НОВЫЙ ДЕНЬ ⚔️";
        if (curLang == 8) titleTxt = "⚔️ 新的一天已降临 ⚔️";
        if (curLang == 7) titleTxt = "⚔️ 새로운 날이 밝았습니다 ⚔️";

        GUILayout.Label(titleTxt, titleStyle);
        GUILayout.Space(10);

        // Day Number Display
        GUIStyle dayStyle = new GUIStyle(GUI.skin.label);
        dayStyle.fontSize = 32;
        dayStyle.fontStyle = FontStyle.Bold;
        dayStyle.alignment = TextAnchor.MiddleCenter;
        dayStyle.normal.textColor = Color.yellow;

        string dayTxt = $"DAY {currentDay}";
        if (curLang == 0) dayTxt = $"ДЕНЬ {currentDay}";
        if (curLang == 8) dayTxt = $"第 {currentDay} 天";
        if (curLang == 7) dayTxt = $"제 {currentDay} 일";

        GUILayout.Label(dayTxt, dayStyle);
        GUILayout.Space(20);

        // Log Title
        GUIStyle logTitleStyle = new GUIStyle(GUI.skin.label);
        logTitleStyle.fontSize = 14;
        logTitleStyle.fontStyle = FontStyle.Bold;
        logTitleStyle.alignment = TextAnchor.MiddleLeft;
        logTitleStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        string logHeader = "ENEMY FORCES MILITARY INTEL REPORT:";
        if (curLang == 0) logHeader = "ОТЧЕТ РАЗВЕДКИ СИЛ ПРОТИВНИКА:";
        if (curLang == 8) logHeader = "敌方军事动向情报：";
        if (curLang == 7) logHeader = "적군 군사 동향 보고서:";

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.Label(logHeader, logTitleStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        // Scrollable List of Logs
        float scrollHeight = winH - 210f; // dynamic height
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        overlayLogScroll = GUILayout.BeginScrollView(overlayLogScroll, GUILayout.Width(winW - 60f), GUILayout.Height(scrollHeight));

        GUIStyle logLineStyle = new GUIStyle(GUI.skin.label);
        logLineStyle.fontSize = 13;
        logLineStyle.wordWrap = true;
        logLineStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        logLineStyle.margin = new RectOffset(0, 0, 4, 4);

        if (aiLogs != null && aiLogs.Count > 0)
        {
            foreach (string logLine in aiLogs)
            {
                GUILayout.Label(logLine, logLineStyle);
            }
        }
        else
        {
            string emptyLog = "Enemy forces are consolidating their defensive garrisons... No aggressive activity detected today.";
            if (curLang == 0) emptyLog = "Силы противника укрепляют свои гарнизоны... Вражеской активности сегодня не обнаружено.";
            if (curLang == 8) emptyLog = "敌方力量正在巩固他们的防御驻军…… 今天未检测到侵略活动。";
            if (curLang == 7) emptyLog = "적군 세력이 방어 주둔지를 보강하고 있습니다... 오늘 탐지된 공격 활동은 없습니다.";

            logLineStyle.fontStyle = FontStyle.Italic;
            logLineStyle.normal.textColor = Color.gray;
            GUILayout.Label(emptyLog, logLineStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.Space(30);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        // Continue Button
        GUI.backgroundColor = new Color(0f, 0.65f, 0.95f, 1.0f);
        string btnTxt = "CONTINUE";
        if (curLang == 0) btnTxt = "ПРОДОЛЖИТЬ";
        if (curLang == 8) btnTxt = "继续";
        if (curLang == 7) btnTxt = "계속";

        string autoCloseTxt = $" (Auto-close in {Mathf.CeilToInt(overlayTimer)}s)";
        if (curLang == 0) autoCloseTxt = $" (Автозакрытие через {Mathf.CeilToInt(overlayTimer)} сек)";
        if (curLang == 8) autoCloseTxt = $" （将在 {Mathf.CeilToInt(overlayTimer)} 秒后自动关闭）";
        if (curLang == 7) autoCloseTxt = $" （{Mathf.CeilToInt(overlayTimer)}초 후 자동 닫힘）";

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button($"<b>{btnTxt}</b>{autoCloseTxt}", GUILayout.Width(winW - 100f), GUILayout.Height(44)))
        {
            showNewDayOverlay = false;
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(20);
        GUILayout.EndArea();
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

    private void DrawEquippedSlotButtonAnatomical(int slotType, string defaultNameRU, string defaultNameEN, int curLang, GUIStyle style, float height, float buttonWidth)
    {
        InventoryItem item = playerEquipment.slots[slotType];
        
        GUILayout.BeginHorizontal(GUILayout.Width(buttonWidth));
        
        Texture2D customTex = null;
        switch (slotType)
        {
            case 1: customTex = icon_helmet; break;
            case 2: customTex = icon_amulet; break;
            case 3: customTex = icon_pauldrons; break;
            case 4: customTex = icon_armor; break;
            case 5: customTex = icon_ring; break;
            case 6: customTex = icon_belt; break;
            case 7: customTex = icon_boots; break;
            case 8: // Weapon slot
                string cl = SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null 
                    ? SaveGameSystem.CurrentData.characterClass.ToLower() 
                    : "warrior";
                    
                if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("стрелок"))
                {
                    customTex = weapon_archer_bow;
                }
                else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг"))
                {
                    customTex = weapon_mage_staff;
                }
                else
                {
                    customTex = weapon_warrior_sword;
                }
                break;
        }

        GUI.backgroundColor = new Color(0.12f, 0.75f, 0.95f, 0.35f);
        
        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            // Item is equipped! Draw empty button with style and overlay its icon
            if (GUILayout.Button("", style, GUILayout.Height(height), GUILayout.Width(buttonWidth)))
            {
                UnequipItem(slotType);
            }
            Rect btnRect = GUILayoutUtility.GetLastRect();
            
            // Overlay icon preserving transparency
            if (customTex != null)
            {
                float padding = 4f;
                Rect iconRect = new Rect(btnRect.x + padding, btnRect.y + padding, btnRect.width - padding * 2, btnRect.height - padding * 2 - 12f);
                GUI.DrawTexture(iconRect, customTex, ScaleMode.ScaleToFit, true);
            }
            
            // Overlay small item description text at bottom
            Rect labelRect = new Rect(btnRect.x, btnRect.y + btnRect.height - 15f, btnRect.width, 14f);
            GUIStyle overlayStyle = new GUIStyle(GUI.skin.label);
            overlayStyle.alignment = TextAnchor.MiddleCenter;
            overlayStyle.fontSize = 8;
            overlayStyle.richText = true;
            overlayStyle.normal.textColor = Color.red;
            GUI.Label(labelRect, "✖", overlayStyle);
            
            // Check hover tooltip
            if (Event.current.type == EventType.Repaint && btnRect.Contains(Event.current.mousePosition))
            {
                SetHoveredItem(item, curLang);
            }
        }
        else
        {
            // Empty slot! Draw default empty label on button
            string emptyLabel = curLang == 0 ? $"[{defaultNameRU}]" : $"[{defaultNameEN}]";
            if (GUILayout.Button(emptyLabel, style, GUILayout.Height(height), GUILayout.Width(buttonWidth)))
            {
                ShowFeedback(curLang == 0 ? $"Экипируйте {defaultNameRU} через инвентарь справа!" : $"Equip {defaultNameEN} through inventory on the right!");
            }
            Rect btnRect = GUILayoutUtility.GetLastRect();
            
            // Draw default icon transparently to signify what goes here
            if (customTex != null)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.15f); // 15% opacity for empty slot placeholder
                float padding = 8f;
                Rect iconRect = new Rect(btnRect.x + padding, btnRect.y + padding, btnRect.width - padding * 2, btnRect.height - padding * 2);
                GUI.DrawTexture(iconRect, customTex, ScaleMode.ScaleToFit, true);
                GUI.color = Color.white;
            }
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }

    private Texture2D GetItemIconTexture(InventoryItem item, string overrideClass = null)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return null;
        
        int tier = item.level;
        int tIndex = tier - 1;
        if (tIndex < 0) tIndex = 0;
        if (tIndex > 5) tIndex = 5;
        
        if (item.slotType == 0) // Potion
        {
            string id = item.id.ToLower();
            if (id.Contains("hp") || id.Contains("life") || id.Contains("жизн")) return icon_potion_hp;
            if (id.Contains("str") || id.Contains("силы")) return icon_potion_str;
            if (id.Contains("int") || id.Contains("инт")) return icon_potion_int;
            if (id.Contains("agi") || id.Contains("ловк")) return icon_potion_agi;
            if (id.Contains("sta") || id.Contains("вынос") || id.Contains("def") || id.Contains("защит")) return icon_potion_sta;
            return icon_potion_hp; // fallback
        }
        
        string cl = !string.IsNullOrEmpty(overrideClass) ? overrideClass.ToLower() : (SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null 
            ? SaveGameSystem.CurrentData.characterClass.ToLower() 
            : "warrior");
            
        bool isArcher = cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow") || cl.Contains("стрелок");
        bool isMage = cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff") || cl.Contains("маг");
        bool isWarrior = !isArcher && !isMage;

        switch (item.slotType)
        {
            case 1: // Helmet
                if (isArcher && archerHelmetIcons != null && archerHelmetIcons.Length > tIndex && archerHelmetIcons[tIndex] != null) return archerHelmetIcons[tIndex];
                if (isMage && mageHelmetIcons != null && mageHelmetIcons.Length > tIndex && mageHelmetIcons[tIndex] != null) return mageHelmetIcons[tIndex];
                if (isWarrior && warriorHelmetIcons != null && warriorHelmetIcons.Length > tIndex && warriorHelmetIcons[tIndex] != null) return warriorHelmetIcons[tIndex];
                if (helmetIcons != null && helmetIcons.Length > tIndex && helmetIcons[tIndex] != null) return helmetIcons[tIndex];
                return icon_helmet;

            case 2: // Amulet
                if (isArcher && archerAmuletIcons != null && archerAmuletIcons.Length > tIndex && archerAmuletIcons[tIndex] != null) return archerAmuletIcons[tIndex];
                if (isMage && mageAmuletIcons != null && mageAmuletIcons.Length > tIndex && mageAmuletIcons[tIndex] != null) return mageAmuletIcons[tIndex];
                if (isWarrior && warriorAmuletIcons != null && warriorAmuletIcons.Length > tIndex && warriorAmuletIcons[tIndex] != null) return warriorAmuletIcons[tIndex];
                if (amuletIcons != null && amuletIcons.Length > tIndex && amuletIcons[tIndex] != null) return amuletIcons[tIndex];
                return icon_amulet;

            case 3: // Pauldrons
                if (isArcher && archerPauldronsIcons != null && archerPauldronsIcons.Length > tIndex && archerPauldronsIcons[tIndex] != null) return archerPauldronsIcons[tIndex];
                if (isMage && magePauldronsIcons != null && magePauldronsIcons.Length > tIndex && magePauldronsIcons[tIndex] != null) return magePauldronsIcons[tIndex];
                if (isWarrior && warriorPauldronsIcons != null && warriorPauldronsIcons.Length > tIndex && warriorPauldronsIcons[tIndex] != null) return warriorPauldronsIcons[tIndex];
                if (pauldronsIcons != null && pauldronsIcons.Length > tIndex && pauldronsIcons[tIndex] != null) return pauldronsIcons[tIndex];
                return icon_pauldrons;

            case 4: // Armor
                if (isArcher && archerArmorIcons != null && archerArmorIcons.Length > tIndex && archerArmorIcons[tIndex] != null) return archerArmorIcons[tIndex];
                if (isMage && mageArmorIcons != null && mageArmorIcons.Length > tIndex && mageArmorIcons[tIndex] != null) return mageArmorIcons[tIndex];
                if (isWarrior && warriorArmorIcons != null && warriorArmorIcons.Length > tIndex && warriorArmorIcons[tIndex] != null) return warriorArmorIcons[tIndex];
                if (armorIcons != null && armorIcons.Length > tIndex && armorIcons[tIndex] != null) return armorIcons[tIndex];
                return icon_armor;

            case 5: // Ring
                if (isArcher && archerRingIcons != null && archerRingIcons.Length > tIndex && archerRingIcons[tIndex] != null) return archerRingIcons[tIndex];
                if (isMage && mageRingIcons != null && mageRingIcons.Length > tIndex && mageRingIcons[tIndex] != null) return mageRingIcons[tIndex];
                if (isWarrior && warriorRingIcons != null && warriorRingIcons.Length > tIndex && warriorRingIcons[tIndex] != null) return warriorRingIcons[tIndex];
                if (ringIcons != null && ringIcons.Length > tIndex && ringIcons[tIndex] != null) return ringIcons[tIndex];
                return icon_ring;

            case 6: // Belt
                if (isArcher && archerBeltIcons != null && archerBeltIcons.Length > tIndex && archerBeltIcons[tIndex] != null) return archerBeltIcons[tIndex];
                if (isMage && mageBeltIcons != null && mageBeltIcons.Length > tIndex && mageBeltIcons[tIndex] != null) return mageBeltIcons[tIndex];
                if (isWarrior && warriorBeltIcons != null && warriorBeltIcons.Length > tIndex && warriorBeltIcons[tIndex] != null) return warriorBeltIcons[tIndex];
                if (beltIcons != null && beltIcons.Length > tIndex && beltIcons[tIndex] != null) return beltIcons[tIndex];
                return icon_belt;

            case 7: // Boots
                if (isArcher && archerBootsIcons != null && archerBootsIcons.Length > tIndex && archerBootsIcons[tIndex] != null) return archerBootsIcons[tIndex];
                if (isMage && mageBootsIcons != null && mageBootsIcons.Length > tIndex && mageBootsIcons[tIndex] != null) return mageBootsIcons[tIndex];
                if (isWarrior && warriorBootsIcons != null && warriorBootsIcons.Length > tIndex && warriorBootsIcons[tIndex] != null) return warriorBootsIcons[tIndex];
                if (bootsIcons != null && bootsIcons.Length > tIndex && bootsIcons[tIndex] != null) return bootsIcons[tIndex];
                return icon_boots;

            case 8: // Weapon slot
                if (isArcher)
                {
                    if (archerWeaponIcons != null && archerWeaponIcons.Length > tIndex && archerWeaponIcons[tIndex] != null) return archerWeaponIcons[tIndex];
                    return weapon_archer_bow;
                }
                else if (isMage)
                {
                    if (mageWeaponIcons != null && mageWeaponIcons.Length > tIndex && mageWeaponIcons[tIndex] != null) return mageWeaponIcons[tIndex];
                    return weapon_mage_staff;
                }
                else
                {
                    if (warriorWeaponIcons != null && warriorWeaponIcons.Length > tIndex && warriorWeaponIcons[tIndex] != null) return warriorWeaponIcons[tIndex];
                    return weapon_warrior_sword;
                }
        }
        return null;
    }

    // [Removed duplicate first copy to resolve duplicate signature conflicts]

    /*
    private void StaleSpyReportWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        
        // Find all spied enemy castles
        List<CastleInstance> spiedCastles = new List<CastleInstance>();
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Enemy" && PlayerPrefs.GetInt("Castle_Spied_" + castles[i].zoneIndex, 0) == 1)
            {
                spiedCastles.Add(castles[i]);
            }
        }

        if (spiedCastles.Count == 0)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUIStyle emptyS = new GUIStyle(GUI.skin.label);
            emptyS.alignment = TextAnchor.MiddleCenter;
            emptyS.fontSize = 14;
            emptyS.normal.textColor = Color.gray;
            GUILayout.Label(GetText9(
                "Нет активных отчетов. Зашлите шпионов в другие замки!",
                "No active reports. Infiltrate spies into enemy castles!",
                "Keine Berichte. Senden Sie Spione in feindliche Burgen!",
                "Aucun rapport. Infiltrez des espions dans les châteaux !",
                "Sin informes activos. ¡Infiltra espías en castillos enemigos!",
                "Sem relatórios. Infiltre espiões em castelhos inimigos!",
                "アクティブな報告書はありません。敵の城にスパイを放ちましょう！",
                "활성화된 보고서가 없습니다. 적의 성에 간첩을 파견하십시오!",
                "暂无活跃情报。请先在其它敌方城堡部署探子！"
            ), emptyS);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetText9("Закрыть", "Close", "Schließen", "Fermer", "Cerrar", "Fechar", "閉じる", "닫기", "关闭"), GUILayout.Height(30)))
            {
                showSpyReportPopup = false;
            }
            GUILayout.EndVertical();
            return;
        }

        // Set default activeSpyReportZoneIndex if not set or invalid
        if (activeSpyReportZoneIndex == -1)
        {
            activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
        }
        else
        {
            bool found = false;
            for (int i = 0; i < spiedCastles.Count; i++)
            {
                if (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
            }
        }

        CastleInstance castle = null;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].zoneIndex == activeSpyReportZoneIndex)
            {
                castle = castles[i];
                break;
            }
        }

        if (castle == null) return;

        // Draw tabs for each spied castle so player can select which one to view!
        GUILayout.Label(GetText9(
            "Выберите вражескую цитадель:", "Select Enemy Stronghold:",
            "Wählen Sie die feindliche Burg:", "Sélectionnez le château ennemi :",
            "Seleccionar fortaleza enemiga:", "Selecionar fortaleza inimiga:",
            "対象の敵城を選択してください:", "적 성채 선택:",
            "选择目标城堡进行查看:"
        ), GUI.skin.label);

        GUILayout.BeginHorizontal();
        for (int i = 0; i < spiedCastles.Count; i++)
        {
            string tabName = curLang == 0 ? spiedCastles[i].nameRU : spiedCastles[i].nameEN;
            if (curLang == 8) tabName = spiedCastles[i].nameCH;
            if (curLang == 7) tabName = spiedCastles[i].nameKR;

            if (tabName.Length > 15) tabName = tabName.Substring(0, 14) + "..";

            GUI.backgroundColor = (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex) ? new Color(1.0f, 0.85f, 0.15f, 1.0f) : Color.white;
            if (GUILayout.Button($"🏰 {tabName}", GUILayout.Height(28)))
            {
                activeSpyReportZoneIndex = spiedCastles[i].zoneIndex;
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.PlayHoverSound(0);
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Retrieve saved spy details level (playerMaxCastleLvl when spied)
        int spyInfoLvl = PlayerPrefs.GetInt("Castle_Spied_Lvl_" + castle.zoneIndex, 2);

        // Display current target details
        string castleLabel = curLang == 0 ? castle.nameRU : castle.nameEN;
        if (curLang == 8) castleLabel = castle.nameCH;
        if (curLang == 7) castleLabel = castle.nameKR;

        GUIStyle subHeader = new GUIStyle(GUI.skin.box);
        subHeader.normal.textColor = Color.yellow;
        subHeader.fontStyle = FontStyle.Bold;
        subHeader.fontSize = 14;
        subHeader.alignment = TextAnchor.MiddleCenter;

        GUILayout.Box($"🕵️ {castleLabel.ToUpper()} [Intel Level {spyInfoLvl}]", subHeader, GUILayout.Height(30));

        GUILayout.Space(6);

        // Scrollview for details
        spyScrollPos = GUILayout.BeginScrollView(spyScrollPos, GUILayout.ExpandHeight(true));

        GUIStyle sectionTitleS = new GUIStyle(GUI.skin.label);
        sectionTitleS.fontSize = 14;
        sectionTitleS.fontStyle = FontStyle.Bold;
        sectionTitleS.normal.textColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);

        GUIStyle detailLabelS = new GUIStyle(GUI.skin.label);
        detailLabelS.fontSize = 12;
        detailLabelS.normal.textColor = Color.white;

        // Determine commander class name based on zone index
        string cmdClassRU = "Рыцарь-Воин";
        string cmdClassEN = "Knight-Warrior";
        string cmdClassDE = "Ritter-Krieger";
        string cmdClassFR = "Chevalier-Guerrier";
        string cmdClassES = "Caballero Guerrero";
        string cmdClassPT = "Cavaleiro Guerreiro";
        string cmdClassJA = "ナイトウォーリアー";
        string cmdClassKO = "나이트 워리어";
        string cmdClassZH = "重装骑士领主";

        if (castle.zoneIndex == 3)
        {
            cmdClassRU = "Архимаг Зенита";
            cmdClassEN = "Zenith Arch-Mage";
            cmdClassDE = "Zenith Erzmagier";
            cmdClassFR = "Archimage de Zénith";
            cmdClassES = "Archimago del Cénit";
            cmdClassPT = "Arquimago do Zênite";
            cmdClassJA = "ゼニス・アークメイジ";
            cmdClassKO = "제니스 아크메이지";
            cmdClassZH = "至高神庭奥术大法师";
        }
        else if (castle.zoneIndex == 6)
        {
            cmdClassRU = "Паладин Света";
            cmdClassEN = "Holy Paladin";
            cmdClassDE = "Heiliger Paladin";
            cmdClassFR = "Paladin Sacré";
            cmdClassES = "Paladín Sagrado";
            cmdClassPT = "Paladino Sagrado";
            cmdClassJA = "ホーリーパラディン";
            cmdClassKO = "홀리 팔라딘";
            cmdClassZH = "圣光大审判骑士长";
        }
        else if (castle.zoneIndex == 11)
        {
            cmdClassRU = "Следопыт Пустошей";
            cmdClassEN = "Wasteland Ranger";
            cmdClassDE = "Wüstenläufer";
            cmdClassFR = "Ranger des Terres Dévastées";
            cmdClassES = "Ranger de los Páramos";
            cmdClassPT = "Patrulheiro do Ermo";
            cmdClassJA = "荒野のレンジャー";
            cmdClassKO = "황무지 순찰대";
            cmdClassZH = "荒野游侠神射手";
        }

        string cmdClass = GetText9(cmdClassRU, cmdClassEN, cmdClassDE, cmdClassFR, cmdClassES, cmdClassPT, cmdClassJA, cmdClassKO, cmdClassZH);

        // Worn gear representation
        string gearWeapon = "???";
        string gearArmor = "???";
        string gearBoots = "???";
        string gearShield = "???";

        if (spyInfoLvl >= 3)
        {
            if (castle.aiArmorTier == 1)
            {
                gearWeapon = GetText9("Бронзовый Меч", "Bronze Sword", "Bronzeschwert", "Épée de bronze", "Espada de bronce", "Espada de Bronze", "ブロンズソード", "청동 검", "青铜阔剑");
                gearArmor = GetText9("Кожаный Нагрудник", "Leather Chest", "Lederbrustplatte", "Plastron de cuir", "Peto de cuero", "Colete de Couro", "レザーチェスト", "가죽 갑옷", "皮质护胸甲");
            }
            else if (castle.aiArmorTier == 2)
            {
                gearWeapon = GetText9("Стальной Молот", "Steel Mace", "Stahlstreitkolben", "Masse d'acier", "Maza de acero", "Maça de Aço", "スチールメイス", "강철 메이스", "精钢战锤");
                gearArmor = GetText9("Кольчужный Доспех", "Chainmail", "Kettenhemd", "Cotte de mailles", "Cota de malla", "Cota de Malha", "チェインメイル", "사슬 갑옷", "精炼锁子甲");
            }
            else if (castle.aiArmorTier == 3)
            {
                gearWeapon = GetText9("Мифриловый Двуручник", "Mithril Greatsword", "Mithril-Zweihänder", "Espadon de mithril", "Espadón de mitril", "Montante de Mitril", "ミスリル・グレートソード", "미스릴 대검", "秘银双手巨剑");
                gearArmor = GetText9("Латы Рыцаря", "Plate Armor", "Plattenrüstung", "Harnois de plaques", "Armadura de placas", "Armadura de Placa", "プレートアーマー", "판금 갑옷", "圣殿骑士重铠");
            }
            else
            {
                gearWeapon = GetText9("Клинок Зенита", "Divine Zenith Blade", "Göttliche Zenith-Klinge", "Lame de Zénith Divine", "Espada del Cénit Divina", "Espada do Zênite Divina", "神聖ゼニスブレード", "신성 제니스 블레이드", "至臻封神裁决之刃");
                gearArmor = GetText9("Броня Небожителя", "Celestial Armor", "Himmlische Rüstung", "Armure Céleste", "Armadura Celestial", "Armadura Celestial", "セレスティアルアーマー", "천상 갑옷", "圣光天神不灭金铠");
            }

            if (spyInfoLvl < 4)
            {
                gearBoots = "???";
                gearShield = "???";
            }
            else
            {
                gearBoots = GetText9("Кованые Сапоги", "Plated Sabatons", "Plattenschuhe", "Sabatons de plaques", "Sabatones de placas", "Soleretes de Placa", "サバトン", "철 판금 장화", "精铁重装护足");
                gearShield = GetText9("Башенный Щит", "Tower Shield", "Turmschild", "Écu pavois", "Escudo de torre", "Escudo Torre", "タワーシールド", "타워 실드", "巨龙不灭重型塔盾");
            }
        }

        // Fetch textures for Main Commander
        Texture2D mainCmdAvatar = null;
        if (castle.zoneIndex == 3) mainCmdAvatar = GetTroopAvatarTexture("MageHero");
        else if (castle.zoneIndex == 11) mainCmdAvatar = GetTroopAvatarTexture("ArcherHero");
        else mainCmdAvatar = GetTroopAvatarTexture("WarriorHero");

        // 1. MAIN HERO DETAILS CARD Layout (Bento Grid)
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(GetText9(
            "👤 Главный Военачальник Гарнизона", "👤 Garrison Arch-Commander",
            "👤 Garnisonskommandant", "👤 Commandant de la garnison",
            "👤 Comandante de la guarnición", "👤 Comandante da guarnição",
            "👤 敵要塞最高司令官", "👤 가리온 요새 수석 사령관",
            "👤 要塞守御主将指挥官"
        ), sectionTitleS);

        GUILayout.BeginHorizontal();

        // Left Sub-column: Avatar, level, and XP bar
        GUILayout.BeginVertical(GUILayout.Width(250f));
        
        Rect avatarRect = GUILayoutUtility.GetRect(100f, 100f, GUILayout.Width(100f), GUILayout.Height(100f));
        GUI.color = new Color(0.08f, 0.08f, 0.15f, 1f);
        GUI.Box(avatarRect, "", GUI.skin.box);
        GUI.color = Color.white;
        
        bool revealAvatar = (spyInfoLvl >= 2);
        if (revealAvatar && mainCmdAvatar != null)
        {
            GUI.DrawTexture(avatarRect, mainCmdAvatar, ScaleMode.ScaleToFit);
        }
        else
        {
            GUIStyle qStyle = new GUIStyle(GUI.skin.label);
            qStyle.fontSize = 42;
            qStyle.fontStyle = FontStyle.Bold;
            qStyle.alignment = TextAnchor.MiddleCenter;
            qStyle.normal.textColor = Color.gray;
            GUI.Label(avatarRect, "?", qStyle);
        }

        GUILayout.Space(6);

        if (spyInfoLvl >= 2)
        {
            float xpProgress = 0.65f;
            Rect xpBarRect = GUILayoutUtility.GetRect(120f, 14f, GUILayout.Width(120f), GUILayout.Height(14f));
            GUI.color = Color.black;
            GUI.DrawTexture(xpBarRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            GUI.DrawTexture(new Rect(xpBarRect.x + 1, xpBarRect.y + 1, (xpBarRect.width - 2) * xpProgress, xpBarRect.height - 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUIStyle barText = new GUIStyle(GUI.skin.label);
            barText.fontSize = 9;
            barText.alignment = TextAnchor.MiddleCenter;
            GUI.Label(xpBarRect, $"XP: 65% ({(castle.aiCommanderLevel * 500)}/{(castle.aiCommanderLevel * 1000)})", barText);
        }
        else
        {
            GUILayout.Label("XP: ???", detailLabelS);
        }

        GUILayout.Space(4);
        string displayCmdClass = (revealAvatar) ? cmdClass : "???";
        string displayCmdLvl = (revealAvatar) ? $"Lvl {castle.aiCommanderLevel}" : "???";

        GUILayout.Label($"<b>{GetText9("Класс:", "Class:", "Klasse:", "Classe :", "Clase:", "Classe:", "クラス:", "클래스:", "职业:")}</b> {displayCmdClass}", detailLabelS);
        GUILayout.Label($"<b>{GetText9("Уровень:", "Level:", "Stufe:", "Niveau :", "Nivel:", "Nível:", "レベル:", "레벨:", "级别:")}</b> {displayCmdLvl}", detailLabelS);
        
        // RPG Attributes (STR, AGI, INT, STA)
        int baseStr = 5, baseAgi = 5, baseInt = 5, baseSta = 5;
        if (castle.zoneIndex == 3) { baseStr = 6; baseAgi = 10; baseInt = 18; baseSta = 10; } // Mage
        else if (castle.zoneIndex == 11) { baseStr = 10; baseAgi = 16; baseInt = 8; baseSta = 12; } // Archer
        else { baseStr = 16; baseAgi = 11; baseInt = 5; baseSta = 16; } // Warrior

        baseStr += castle.aiCommanderLevel * 2;
        baseAgi += castle.aiCommanderLevel * 2;
        baseInt += castle.aiCommanderLevel * 2;
        baseSta += castle.aiCommanderLevel * 2;

        string strVal = (spyInfoLvl >= 3) ? baseStr.ToString() : "??";
        string agiVal = (spyInfoLvl >= 3) ? baseAgi.ToString() : "??";
        string intVal = (spyInfoLvl >= 3) ? baseInt.ToString() : "??";
        string staVal = (spyInfoLvl >= 3) ? baseSta.ToString() : "??";

        GUILayout.Space(4);
        GUILayout.Label($"💪 {GetText9("Сила:", "Strength:", "Stärke:", "Force:", "Fuerza:", "Força:", "力:", "힘:", "力量:")} <color=#FF5555>{strVal}</color>", detailLabelS);
        GUILayout.Label($"⚡ {GetText9("Ловкость:", "Agility:", "Agilität:", "Agilité:", "Agilidad:", "Agilidade:", "素早さ:", "민첩:", "敏捷:")} <color=#55FF55>{agiVal}</color>", detailLabelS);
        GUILayout.Label($"🔮 {GetText9("Интеллект:", "Intelligence:", "Intelligenz:", "Intelligence:", "Inteligencia:", "Inteligência:", "知力:", "지능:", "智力:")} <color=#5555FF>{intVal}</color>", detailLabelS);
        GUILayout.Label($"🛡️ {GetText9("Выносливость:", "Stamina:", "Ausdauer:", "Stamina:", "Aguante:", "Stamina:", "耐久:", "체력:", "耐力:")} <color=#FFFF55>{staVal}</color>", detailLabelS);
        
        GUILayout.EndVertical(); // end left details

        // Right Sub-column: Equipped Gear (left) & Skills (right)
        GUILayout.BeginVertical(GUILayout.Width(510f));
        GUILayout.BeginHorizontal();

        // 1. Gear column (240f)
        GUILayout.BeginVertical(GUILayout.Width(240f));
        GUILayout.Label($"<b>{GetText9("⚔️ Экипировка:", "⚔️ Equipped Gear:", "⚔️ Ausrüstung:", "⚔️ Équipement :", "⚔️ Equipo:", "⚔️ Equipamento:", "⚔️ 装備:", "⚔️ 장착 장비:", "穿戴装备:")}</b>", detailLabelS);
        
        string[] gearSlots = {
            GetText9("Оружие", "Weapon", "Waffe", "Arme", "Arma", "Arma", "武器", "무기", "主手武器"),
            GetText9("Доспех", "Armor", "Rüstung", "Armure", "Armadura", "Armadura", "鎧", "갑옷", "防具重铠"),
            GetText9("Сапоги", "Boots", "Stiefel", "Bottes", "Botas", "Botas", "靴", "장화", "腿部足具"),
            GetText9("Щит", "Shield", "Schild", "Bouclier", "Escudo", "Escudo", "盾", "방패", "副手重盾")
        };
        string[] gearNames = { gearWeapon, gearArmor, gearBoots, gearShield };

        for (int s = 0; s < 4; s++)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            
            Rect slotRect = GUILayoutUtility.GetRect(24f, 24f, GUILayout.Width(24f), GUILayout.Height(24f));
            GUI.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            GUI.DrawTexture(slotRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUIStyle slotEmojiS = new GUIStyle(GUI.skin.label);
            slotEmojiS.alignment = TextAnchor.MiddleCentering();
            slotEmojiS.fontSize = 11;
            string iconEmoji = "❓";
            if (spyInfoLvl >= (s < 2 ? 3 : 4))
            {
                if (s == 0) iconEmoji = "🗡️";
                else if (s == 1) iconEmoji = "👕";
                else if (s == 2) iconEmoji = "👢";
                else if (s == 3) iconEmoji = "🛡️";
            }
            GUI.Label(slotRect, iconEmoji, slotEmojiS);
            
            string gearDisplayName = gearNames[s];
            if (string.IsNullOrEmpty(gearDisplayName)) gearDisplayName = "???";
            
            GUILayout.BeginVertical();
            GUIStyle gearSlotLbl = new GUIStyle(GUI.skin.label);
            gearSlotLbl.fontSize = 9;
            gearSlotLbl.normal.textColor = Color.cyan;
            GUILayout.Label(gearSlots[s], gearSlotLbl);
            
            GUIStyle gearNameLbl = new GUIStyle(GUI.skin.label);
            gearNameLbl.fontSize = 10;
            gearNameLbl.normal.textColor = gearDisplayName.Contains("???") ? Color.gray : Color.yellow;
            GUILayout.Label(gearDisplayName, gearNameLbl);
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical(); // end Gear

        // 2. Skills column (250f)
        GUILayout.BeginVertical(GUILayout.Width(250f));
        GUILayout.Label($"<b>{GetText9("✨ Навыки и Способности:", "✨ Commander Skills:", "✨ Kommandanten-Fähigkeiten:", "✨ Compétences :", "✨ Habilidades:", "✨ Habilidades:", "✨ 特性・スキル:", "✨ 사령관 능력:", "将领特技和技能:")}</b>", detailLabelS);

        Texture2D sActive = null;
        Texture2D sPassive1 = null;
        Texture2D sPassive2 = null;
        string skillActiveName = "";
        string skillP1Name = "";
        string skillP2Name = "";

        if (castle.zoneIndex == 3)
        {
            sActive = skill_hero_mage_active;
            sPassive1 = skill_hero_mage_passive1;
            sPassive2 = skill_hero_mage_passive2;
            skillActiveName = GetText9("Аркановый Разлом", "Arcane Rift", "Arkanriss", "Faille Arcanique", "Fisura Arcana", "Fenda Arcana", "アルケインリフト", "비전 균열", "奥术崩裂秘法");
            skillP1Name = GetText9("Медитация", "Mana Meditation", "Mana-Meditation", "Méditation", "Meditación", "Meditação", "瞑想", "마나 명상", "冥想魂归");
            skillP2Name = GetText9("Светящийся Щит", "Glow Shield", "Leuchtschild", "Bouclier Lumineux", "Escudo Luminoso", "Escudo Luminoso", "グロウシールド", "빛의 방패", "不灭护体圣盾");
        }
        else if (castle.zoneIndex == 11)
        {
            sActive = skill_hero_archer_active;
            sPassive1 = skill_hero_archer_passive1;
            sPassive2 = skill_hero_archer_passive2;
            skillActiveName = GetText9("Ливень Стрел", "Arrow Rain", "Pfeilregen", "Pluie de flèches", "Lluvia de flechas", "Chuva de Flechas", "アローレイン", "화살 비", "漫天诸神箭雨");
            skillP1Name = GetText9("Орлиный Взгляд", "Eagle Eye", "Adlerauge", "Œil d'aigle", "Ojo de águila", "Olho de Águia", "イーグルアイ", "매의 눈", "鹰眼极致锁敌");
            skillP2Name = GetText9("Быстрые Шаги", "Swift Steps", "Schnelle Schritte", "Pas rapides", "Pasos rápidos", "Passos Rápidos", "スウィフトステップ", "신속한 걸음", "幻影突袭疾行");
        }
        else
        {
            sActive = skill_hero_warrior_active;
            sPassive1 = skill_hero_warrior_passive1;
            sPassive2 = skill_hero_warrior_passive2;
            skillActiveName = GetText9("Круговой Удар", "Whirlwind Strike", "Wirbelwindangriff", "Frappe Tourbillonnante", "Ataque Torbellino", "Golpe Furacão", "旋風斬り", "회오리 베기", "烈焰狂风旋风斩");
            skillP1Name = GetText9("Закалка Стали", "Steel Hardening", "Stahlhärtung", "Durcissement de l'acier", "Templado de acero", "Endurecimento do Aço", "鋼鉄強化", "강철 단련", "金刚不坏之躯");
            skillP2Name = GetText9("Берсерк", "Combat Rage", "Kampfwut", "Rage de combat", "Furia de combate", "Fúria de Combate", "バーサーカー", "광폭화", "狂热战神觉醒");
        }

        Texture2D[] skillIcons = { sActive, sPassive1, sPassive2 };
        string[] skillNames = { skillActiveName, skillP1Name, skillP2Name };
        string[] skillTypes = {
            GetText9("Активный", "Active Ultimate", "Aktive Fähigkeit", "Ultime Actif", "Última Activa", "Ativa Suprema", "アクティブ・アルティメット", "액티브 궁극기", "终极主动觉醒技"),
            GetText9("П�        GUI.backgroundColor = Color.white;
    }          GUIStyle armyCountS = new GUIStyle(GUI.skin.label);
            armyCountS.fontSize = 11;
            armyCountS.fontStyle = FontStyle.Bold;
            armyCountS.normal.textColor = Color.green;
            GUILayout.Label($"{GetText9("Численность:", "Strength:", "Stärke:", "Effectif :", "Efectivo:", "Efectivo:", "将兵数:", "부대 규모:", "总人数:")} {power / 4} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "名战士")}", armyCountS);
            
            string sec2ArmyComp = (spyInfoLvl >= 4)
                ? GetText9("Состав: 40% Маги, 60% Паладины", "Comp: 40% Mages, 60% Paladins", "Zus.: 40% Magier, 60% Paladine", "Comp : 40% Mages, 60% Paladins", "Comp: 40% Magos, 60% Paladines", "Comp: 40% Magos, 60% Paladinos", "構成: 40% 魔術師、60% パラディン", "구성: 마법사 40%, 성기사 60%", "编制: 40% 秘术祭司, 60% 圣光骑士")
                : GetText9("Состав: Т2/Т3 Войска", "Composition: T2/T3 Troops", "Zusammensetzung: T2/T3", "Composition : T2/T3", "Composición: T2/T3", "Composição: T2/T3", "構成: T2/T3 兵", "구성: T2/T3 병력", "编制: 包含 T2/T3 精锐连队");
            GUILayout.Label(sec2ArmyComp, detailLabelS);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.EndVertical(); // end armies on heroes

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(GetText9("Закрыть Отчет", "Close Report", "Bericht schließen", "Fermer le rapport", "Cerrar informe", "Fechar relatório", "報告書を閉じる", "보고서 닫기", "关闭情报窗口"), GUILayout.Height(30)))
        {
            showSpyReportPopup = false;
        }
        GUI.backgroundColor = Color.white;
    }ier} AGI</color> (Защита +{tier * 1.5f})"
                        : $"Stamina: <color=#33FF33>+{tier} STA</color> (+{tier * 10} HP)\nAgility: <color=#33FF33>+{tier} AGI</color> (+{tier * 1.5f} Def)";
                    break;
                case 7: // Boots
                    statReport = curLang == 0
                        ? $"Ловкость: <color=#33FF33>+{tier * 2} AGI</color> (Повышает защиту на +{tier * 3.0f})"
                        : $"Agility: <color=#33FF33>+{tier * 2} AGI</color> (+{tier * 3.0f} Def)";
                    break;
                case 8: // Weapon
                    statReport = curLang == 0
                        ? $"Сила: <color=#33FF33>+{tier * 3} STR</color> (Повышает базовую атаку на +{tier * 7.5f} урона)"
                        : $"Strength: <color=#33FF33>+{tier * 3} STR</color> (+{tier * 7.5f} Combat Damage)";
                    break;
            }
            hoveredSkillDesc = statReport;
        }
    }
    */

    private void DrawSpyReportPopup(int curLang)
    {
        // Calculate size of the window (spacious desktop dimensions for side-by-side bento layout)
        float panelWidth = 840f;
        float panelHeight = 660f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.01f, 0.04f, 0.12f, 0.99f);
        
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 15;
        windowStyle.fontStyle = FontStyle.Bold;

        string spyReportTitle = GetText9(
            "🕵️ Отчет Шпионской Сети", "🕵️ Spy Network Intelligence Report",
            "🕵️ Spionagebericht", "🕵️ Rapport d'Espionnage",
            "🕵️ Informe de Espionaje", "🕵️ Relatório de Espionagem",
            "🕵️ スパイ密偵調査報告書", "🕵️ 스파и-네트워크 첩보 보고서", // Keep title matching Translator table standard
            "🕵️ 谍报密探军情报告"
        );
        // Note: the original had "🕵️ 스파이 네트워크 첩보 보고서", let's adjust if needed, but we will use the clean GetText9 version

        GUI.Window(101, new Rect(px, py, panelWidth, panelHeight), SpyReportWindowFunctionBento, spyReportTitle, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void InitSpyStyles()
    {
        if (s_spyTitleStyle == null)
        {
            s_spyTitleStyle = new GUIStyle(GUI.skin.label);
            s_spyTitleStyle.fontSize = 15;
            s_spyTitleStyle.fontStyle = FontStyle.Bold;
            s_spyTitleStyle.normal.textColor = Color.yellow;
            s_spyTitleStyle.alignment = TextAnchor.MiddleLeft;

            s_spySectionTitleStyle = new GUIStyle(GUI.skin.label);
            s_spySectionTitleStyle.fontSize = 13;
            s_spySectionTitleStyle.fontStyle = FontStyle.Bold;
            s_spySectionTitleStyle.normal.textColor = new Color(0.2f, 0.8f, 1.0f, 1.0f); // neon cyan

            s_spyDetailLabelStyle = new GUIStyle(GUI.skin.label);
            s_spyDetailLabelStyle.fontSize = 12;
            s_spyDetailLabelStyle.normal.textColor = Color.white;
            s_spyDetailLabelStyle.richText = true;

            s_spySubHeaderStyle = new GUIStyle(GUI.skin.box);
            s_spySubHeaderStyle.normal.textColor = Color.yellow;
            s_spySubHeaderStyle.fontStyle = FontStyle.Bold;
            s_spySubHeaderStyle.fontSize = 14;
            s_spySubHeaderStyle.alignment = TextAnchor.MiddleCenter;

            s_spyCardBgStyle = new GUIStyle(GUI.skin.box);
            s_spyCardBgStyle.padding = new RectOffset(10, 10, 10, 10);
            
            s_spyBoxLabelStyle = new GUIStyle(GUI.skin.box);
            s_spyBoxLabelStyle.alignment = TextAnchor.MiddleCenter;
            s_spyBoxLabelStyle.fontSize = 10;
            s_spyBoxLabelStyle.normal.textColor = Color.white;

            s_spyBarTextStyle = new GUIStyle(GUI.skin.label);
            s_spyBarTextStyle.fontSize = 9;
            s_spyBarTextStyle.fontStyle = FontStyle.Bold;
            s_spyBarTextStyle.alignment = TextAnchor.MiddleCenter;
            s_spyBarTextStyle.normal.textColor = Color.white;
        }
    }

    private void SpyReportWindowFunctionBento(int windowID)
    {
        int curLang = Translator.LanguageID;
        InitSpyStyles();
        
        // Find all spied enemy castles
        List<CastleInstance> spiedCastles = new List<CastleInstance>();
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Enemy" && PlayerPrefs.GetInt("Castle_Spied_" + castles[i].zoneIndex, 0) == 1)
            {
                spiedCastles.Add(castles[i]);
            }
        }

        if (spiedCastles.Count == 0)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUIStyle emptyS = new GUIStyle(GUI.skin.label);
            emptyS.alignment = TextAnchor.MiddleCenter;
            emptyS.fontSize = 14;
            emptyS.normal.textColor = Color.gray;
            GUILayout.Label(GetText9(
                "Нет активных отчетов. Зашлите шпионов в другие замки!",
                "No active reports. Infiltrate spies into enemy castles!",
                "Keine Berichte. Senden Sie Spione in feindliche Burgen!",
                "Aucun rapport. Infiltrez des espions dans les châteaux !",
                "Sin informes activos. ¡Infiltra espías en castillos enemigos!",
                "Sem relatórios. Infiltre espiões em castelhos inimigos!",
                "アクティブな報告書はありません。敵の城にスパイを放ちましょう！",
                "활성화된 보고서가 없습니다. 적의 성에 간첩을 파견하십시오!",
                "暂无分城情报。请先派遣刺探部署守军探子！"
            ), emptyS);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetText9("Закрыть", "Close", "Schließen", "Fermer", "Cerrar", "Fechar", "閉じる", "닫기", "关闭"), GUILayout.Height(30)))
            {
                showSpyReportPopup = false;
            }
            GUILayout.EndVertical();
            return;
        }

        // Set default activeSpyReportZoneIndex if not set or invalid
        if (activeSpyReportZoneIndex == -1)
        {
            activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
        }
        else
        {
            bool found = false;
            for (int i = 0; i < spiedCastles.Count; i++)
            {
                if (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
            }
        }

        CastleInstance castle = null;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].zoneIndex == activeSpyReportZoneIndex)
            {
                castle = castles[i];
                break;
            }
        }

        if (castle == null) return;

        // Draw tabs for selecting stronghold
        GUILayout.Label(GetText9(
            "Выберите вражескую цитадель для просмотра разведданных:", 
            "Select enemy stronghold to view intel reports:",
            "Feindliche Burg für Spionagebericht auswählen:", 
            "Sélectionnez la forteresse ennemie pour voir les rapports :",
            "Seleccionar fortaleza enemiga para ver informes:", 
            "Selecionar fortaleza inimiga para ver relatórios:",
            "諜報データを確認する敵の城塞を選択してください:", 
            "첩보 보고서를 확인하려면 적 성채를 선택하십시오:",
            "选择敌方城堡查看获取的密探军报:"
        ), s_spyDetailLabelStyle);

        GUILayout.BeginHorizontal();
        for (int i = 0; i < spiedCastles.Count; i++)
        {
            string tabName = curLang == 0 ? spiedCastles[i].nameRU : spiedCastles[i].nameEN;
            if (curLang == 8) tabName = spiedCastles[i].nameCH;
            if (curLang == 7) tabName = spiedCastles[i].nameKR;

            if (tabName.Length > 20) tabName = tabName.Substring(0, 19) + "..";

            GUI.backgroundColor = (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex) ? new Color(1.0f, 0.85f, 0.15f, 1.0f) : Color.white;
            if (GUILayout.Button($"🏰 {tabName}", GUILayout.Height(28)))
            {
                activeSpyReportZoneIndex = spiedCastles[i].zoneIndex;
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.PlayHoverSound(0);
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Retrieve saved spy details level (playerMaxCastleLvl when spied)
        int spyInfoLvl = PlayerPrefs.GetInt("Castle_Spied_Lvl_" + castle.zoneIndex, 2);

        // Header Title Card
        string castleLabel = curLang == 0 ? castle.nameRU : castle.nameEN;
        if (curLang == 8) castleLabel = castle.nameCH;
        if (curLang == 7) castleLabel = castle.nameKR;

        string starsStr = new string('⭐', spyInfoLvl) + new string('☆', Mathf.Max(0, 4 - spyInfoLvl));
        string accuracyText = spyInfoLvl == 1 ? "25% (" + GetText9("Низкая", "Low", "Niedrig", "Basse", "Baja", "Baixa", "低", "낮음", "极低精度") + ")" :
                             spyInfoLvl == 2 ? "50% (" + GetText9("Базовая", "Medium", "Mittel", "Moyenne", "Media", "Média", "中", "보통", "基础精度") + ")" :
                             spyInfoLvl == 3 ? "75% (" + GetText9("Высокая", "High", "Hoch", "Haute", "Alta", "Alta", "高", "높음", "详尽高精") + ")" :
                             "100% (" + GetText9("Максимальная", "Absolute", "Absolut", "Absolue", "Absoluta", "Absoluta", "絶対", "최대", "绝对精准") + ")";

        string headerBoxText = $"🕵️ {castleLabel.ToUpper()} — {GetText9("ТОЧНОСТЬ:", "INTEL ACCURACY:", "GENAUIGKEIT:", "PRÉCISION :", "PRECISIÓN:", "PRECISÃO:", "精度:", "정밀도:", "情报获取精度:")} {accuracyText}  [{starsStr}]";
        GUILayout.Box(headerBoxText, s_spySubHeaderStyle, GUILayout.Height(30));

        GUILayout.Space(6);

        // Scrollview for bento-grid
        spyScrollPos = GUILayout.BeginScrollView(spyScrollPos, GUILayout.ExpandHeight(true));

        GUILayout.BeginHorizontal();

        // ==================== LEFT COLUMN: ARCH-COMMANDER PROFILE (Width: 395f) ====================
        GUILayout.BeginVertical(GUILayout.Width(395f));

        // Bento Card 1: Commander Profile
        GUILayout.BeginVertical(s_spyCardBgStyle);
        GUILayout.Label(GetText9(
            "👤 Главный Военачальник Гарнизона", "👤 Garrison Arch-Commander",
            "👤 Garnisonskommandant", "👤 Commandant de la garnison",
            "👤 Comandante de la guarnición", "👤 Comandante da guarnição",
            "👤 敵要塞最高司令官", "👤 가리온 요새 수석 사령관",
            "👤 要塞守御主将指挥官"
        ), s_spySectionTitleStyle);
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        
        // Avatar Box
        Rect avatarRect = GUILayoutUtility.GetRect(96f, 96f, GUILayout.Width(96f), GUILayout.Height(96f));
        GUI.color = new Color(0.08f, 0.08f, 0.15f, 1f);
        GUI.Box(avatarRect, "", GUI.skin.box);
        GUI.color = Color.white;

        bool revealAvatar = (spyInfoLvl >= 2);
        Texture2D mainCmdAvatar = null;
        if (castle.zoneIndex == 3) mainCmdAvatar = GetTroopAvatarTexture("mage");
        else if (castle.zoneIndex == 11) mainCmdAvatar = GetTroopAvatarTexture("archer");
        else mainCmdAvatar = GetTroopAvatarTexture("warrior");

        if (revealAvatar && mainCmdAvatar != null)
        {
            GUI.DrawTexture(avatarRect, mainCmdAvatar, ScaleMode.ScaleToFit);
        }
        else
        {
            GUIStyle qStyle = new GUIStyle(GUI.skin.label);
            qStyle.fontSize = 42;
            qStyle.fontStyle = FontStyle.Bold;
            qStyle.alignment = TextAnchor.MiddleCenter;
            qStyle.normal.textColor = Color.gray;
            GUI.Label(avatarRect, "?", qStyle);
        }

        // Commander identity fields
        GUILayout.BeginVertical();
        string cmdClassRU = "Рыцарь-Воин";
        string cmdClassEN = "Knight-Warrior";
        string cmdClassDE = "Ritter-Krieger";
        string cmdClassFR = "Chevalier-Guerrier";
        string cmdClassES = "Caballero Guerrero";
        string cmdClassPT = "Cavaleiro Guerreiro";
        string cmdClassJA = "ナイトウォーリアー";
        string cmdClassKO = "나이트 워리어";
        string cmdClassZH = "重装骑士领主";

        if (castle.zoneIndex == 3)
        {
            cmdClassRU = "Архимаг Зенита";
            cmdClassEN = "Zenith Arch-Mage";
            cmdClassDE = "Zenith Erzmagier";
            cmdClassFR = "Archimage de Zénith";
            cmdClassES = "Archimago del Cénit";
            cmdClassPT = "Arquimago do Zênite";
            cmdClassJA = "ゼニス・アークメイジ";
            cmdClassKO = "제니스 아크메이지";
            cmdClassZH = "至高神庭奥术大法师";
        }
        else if (castle.zoneIndex == 6)
        {
            cmdClassRU = "Паладин Света";
            cmdClassEN = "Holy Paladin";
            cmdClassDE = "Heiliger Paladin";
            cmdClassFR = "Paladin Sacré";
            cmdClassES = "Paladín Sagrado";
            cmdClassPT = "Paladino Sagrado";
            cmdClassJA = "ホーリーパラディン";
            cmdClassKO = "홀리 팔라딘";
            cmdClassZH = "圣光大审判骑士长";
        }
        else if (castle.zoneIndex == 11)
        {
            cmdClassRU = "Следопыт Пустошей";
            cmdClassEN = "Wasteland Ranger";
            cmdClassDE = "Wüstenläufer";
            cmdClassFR = "Ranger des Terres Dévastées";
            cmdClassES = "Ranger de los Páramos";
            cmdClassPT = "Patrulheiro do Ermo";
            cmdClassJA = "荒野のレンジャー";
            cmdClassKO = "황무지 순찰대";
            cmdClassZH = "荒野游侠神射手";
        }

        string cmdClass = GetText9(cmdClassRU, cmdClassEN, cmdClassDE, cmdClassFR, cmdClassES, cmdClassPT, cmdClassJA, cmdClassKO, cmdClassZH);
        string displayCmdClass = (revealAvatar) ? cmdClass : "???";
        string displayCmdLvl = (revealAvatar) ? $"{GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {castle.aiCommanderLevel}" : "???";

        GUILayout.Label($"<b>{GetText9("Класс:", "Class:", "Klasse:", "Classe :", "Clase:", "Classe:", "クラス:", "클래스:", "职业:")}</b> <color=yellow>{displayCmdClass}</color>", s_spyDetailLabelStyle);
        GUILayout.Label($"<b>{GetText9("Уровень:", "Level:", "Stufe:", "Niveau :", "Nivel:", "Nível:", "レベル:", "레벨:", "等级:")}</b> <color=#FF8800>{displayCmdLvl}</color>", s_spyDetailLabelStyle);
        
        // Experience Bar
        GUILayout.Space(4);
        if (spyInfoLvl >= 2)
        {
            float xpProgress = 0.65f;
            Rect xpBarRect = GUILayoutUtility.GetRect(240f, 15f, GUILayout.Width(240f), GUILayout.Height(15f));
            GUI.color = Color.black;
            GUI.DrawTexture(xpBarRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.12f, 0.58f, 0.95f, 0.85f); // blue-cyan neon fill
            GUI.DrawTexture(new Rect(xpBarRect.x + 1, xpBarRect.y + 1, (xpBarRect.width - 2) * xpProgress, xpBarRect.height - 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(xpBarRect, $"XP: 65% ({castle.aiCommanderLevel * 650} / {castle.aiCommanderLevel * 1000})", s_spyBarTextStyle);
        }
        else
        {
            GUILayout.Label("XP: <color=gray>??? [Low Intel]</color>", s_spyDetailLabelStyle);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical(); // End Bento Card 1

        GUILayout.Space(6);

        // Bento Card 2: RPG Attributes
        GUILayout.BeginVertical(s_spyCardBgStyle);
        GUILayout.Label(GetText9(
            "⚔️ Основные Характеристики", "⚔️ RPG Core Attributes",
            "⚔️ RPG-Attribute", "⚔️ Attributs RPG core",
            "⚔️ Atributos RPG básicos", "⚔️ Atributos RPG básicos",
            "⚔️ 能力値・ステータス", "⚔️ 핵심 사령관 스탯",
            "⚔️ 将领核心战斗属性总览"
        ), s_spySectionTitleStyle);
        GUILayout.Space(4);

        int baseStr = 6, baseAgi = 6, baseInt = 6, baseSta = 6;
        if (castle.zoneIndex == 3) { baseStr = 7; baseAgi = 11; baseInt = 19; baseSta = 11; } // Mage
        else if (castle.zoneIndex == 11) { baseStr = 11; baseAgi = 17; baseInt = 9; baseSta = 13; } // Archer
        else { baseStr = 17; baseAgi = 12; baseInt = 6; baseSta = 17; } // Warrior

        baseStr += castle.aiCommanderLevel * 2;
        baseAgi += castle.aiCommanderLevel * 2;
        baseInt += castle.aiCommanderLevel * 2;
        baseSta += castle.aiCommanderLevel * 2;

        string strVal = (spyInfoLvl >= 3) ? baseStr.ToString() : "??";
        string agiVal = (spyInfoLvl >= 3) ? baseAgi.ToString() : "??";
        string intVal = (spyInfoLvl >= 3) ? baseInt.ToString() : "??";
        string staVal = (spyInfoLvl >= 3) ? baseSta.ToString() : "??";

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(180f));
        GUILayout.Label($"💪 {GetText9("Сила:", "Strength:", "Stärke:", "Force:", "Fuerza:", "Força:", "力:", "힘:", "力量:")} <color=#FF5555>{strVal}</color>", s_spyDetailLabelStyle);
        GUILayout.Label($"⚡ {GetText9("Ловкость:", "Agility:", "Agilität:", "Agilité:", "Agilidad:", "Agilidade:", "素早さ:", "민첩:", "敏捷:")} <color=#55FF55>{agiVal}</color>", s_spyDetailLabelStyle);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(180f));
        GUILayout.Label($"🔮 {GetText9("Интеллект:", "Intelligence:", "Intelligenz:", "Intelligence:", "Inteligencia:", "Inteligência:", "知力:", "지능:", "智力:")} <color=#5555FF>{intVal}</color>", s_spyDetailLabelStyle);
        GUILayout.Label($"🛡️ {GetText9("Выносливость:", "Stamina:", "Ausdauer:", "Stamina:", "Aguante:", "Stamina:", "耐久:", "체력:", "耐力:")} <color=#FFFF55>{staVal}</color>", s_spyDetailLabelStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical(); // End Bento Card 2

        GUILayout.Space(6);

        // Bento Card 3: Equipped Gear
        GUILayout.BeginVertical(s_spyCardBgStyle);
        GUILayout.Label(GetText9(
            "🛡️ Надетое Снаряжение", "🛡️ Equipped Mannequin & Gear",
            "🛡️ Ausrüstung slots", "🛡️ Équipement équipé",
            "🛡️ Equipo equipado", "🛡️ Equipamento equipado",
            "🛡️ 現在の装備・防具", "🛡️ 장착한 방어구 및 무기",
            "🛡️ 主将防身重装与传世神兵"
        ), s_spySectionTitleStyle);
        GUILayout.Space(4);

        string gearWeapon = "???";
        string gearArmor = "???";
        string gearBoots = "???";
        string gearShield = "???";

        if (spyInfoLvl >= 3)
        {
            if (castle.aiArmorTier == 1)
            {
                gearWeapon = GetText9("Бронзовый Меч", "Bronze Sword", "Bronzeschwert", "Épée de bronze", "Espada de bronce", "Espada de Bronze", "ブロンズソード", "청동 검", "青铜阔剑");
                gearArmor = GetText9("Кожаный Нагрудник", "Leather Chest", "Lederbrustplatte", "Plastron de cuir", "Peto de cuero", "Colete de Couro", "レザーチェスト", "가죽 갑옷", "皮质护胸甲");
            }
            else if (castle.aiArmorTier == 2)
            {
                gearWeapon = GetText9("Стальной Молот", "Steel Mace", "Stahlstreitkolben", "Masse d'acier", "Maza de acero", "Maça de Aço", "スチールメイス", "강철 메이스", "精钢战锤");
                gearArmor = GetText9("Кольчужный Доспех", "Chainmail", "Kettenhemd", "Cotte de mailles", "Cota de malla", "Cota de Malha", "チェインメイル", "사슬 갑옷", "精炼锁子甲");
            }
            else if (castle.aiArmorTier == 3)
            {
                gearWeapon = GetText9("Мифриловый Двуручник", "Mithril Greatsword", "Mithril-Zweihänder", "Espadon de mithril", "Espadón de mitril", "Montante de Mitril", "ミスリル・グレートソード", "미스릴 대검", "秘银双手巨剑");
                gearArmor = GetText9("Латы Рыцаря", "Plate Armor", "Plattenrüstung", "Harnois de plaques", "Armadura de placas", "Armadura de Placa", "プレートアーマー", "판금 갑옷", "圣殿骑士重铠");
            }
            else
            {
                gearWeapon = GetText9("Клинок Зенита", "Divine Zenith Blade", "Göttliche Zenith-Klinge", "Lame de Zénith Divine", "Espada del Cénit Divina", "Espada do Zênite Divina", "神聖ゼニスブレード", "신성 제니스 블레이드", "至臻封神裁决之刃");
                gearArmor = GetText9("Броня Небожителя", "Celestial Armor", "Himmlische Rüstung", "Armure Céleste", "Armadura Celestial", "Armadura Celestial", "セレスティアルアーマー", "천상 갑옷", "圣光天神不灭金铠");
            }

            if (spyInfoLvl < 4)
            {
                gearBoots = "???";
                gearShield = "???";
            }
            else
            {
                gearBoots = GetText9("Кованые Сапоги", "Plated Sabatons", "Plattenschuhe", "Sabatons de plaques", "Sabatones de placas", "Soleretes de Placa", "サバトン", "철 판금 장화", "精铁重装护足");
                gearShield = GetText9("Башенный Щит", "Tower Shield", "Turmschild", "Écu pavois", "Escudo de torre", "Escudo Torre", "タワーシールド", "타워 실드", "巨龙不灭重型塔盾");
            }
        }

        string[] gearSlots = {
            GetText9("Оружие", "Weapon", "Waffe", "Arme", "Arma", "Arma", "武器", "무기", "武器"),
            GetText9("Доспех", "Armor", "Rüstung", "Armure", "Armadura", "Armadura", "鎧", "갑옷", "铠甲"),
            GetText9("Сапоги", "Boots", "Stiefel", "Bottes", "Botas", "Botas", "靴", "장화", "战靴"),
            GetText9("Щит", "Shield", "Schild", "Bouclier", "Escudo", "Escudo", "盾", "방패", "副盾")
        };
        string[] gearNames = { gearWeapon, gearArmor, gearBoots, gearShield };

        for (int s = 0; s < 4; s++)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            Rect slotRect = GUILayoutUtility.GetRect(24f, 24f, GUILayout.Width(24f), GUILayout.Height(24f));
            GUI.color = new Color(0.1f, 0.12f, 0.2f, 1f);
            GUI.DrawTexture(slotRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            string slotEmoji = "❓";
            if (spyInfoLvl >= (s < 2 ? 3 : 4))
            {
                if (s == 0) slotEmoji = "🗡️";
                else if (s == 1) slotEmoji = "👕";
                else if (s == 2) slotEmoji = "👢";
                else if (s == 3) slotEmoji = "🛡️";
            }
            
            GUIStyle sEmojiS = new GUIStyle(GUI.skin.label);
            sEmojiS.alignment = TextAnchor.MiddleCenter;
            sEmojiS.fontSize = 12;
            GUI.Label(slotRect, slotEmoji, sEmojiS);

            string gDisp = gearNames[s];
            string colorHex = gDisp.Contains("???") ? "#777777" : (castle.aiArmorTier == 4 ? "#FF00FF" : (castle.aiArmorTier == 3 ? "#FF9900" : "#00FFFF"));

            GUILayout.BeginVertical();
            GUIStyle sLbl = new GUIStyle(GUI.skin.label);
            sLbl.fontSize = 9;
            sLbl.normal.textColor = Color.gray;
            GUILayout.Label(gearSlots[s], sLbl);

            GUIStyle nameLbl = new GUIStyle(GUI.skin.label);
            nameLbl.fontSize = 11;
            nameLbl.normal.textColor = Color.white;
            GUILayout.Label($"<color={colorHex}>{gDisp}</color>", s_spyDetailLabelStyle);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical(); // End Bento Card 3

        GUILayout.EndVertical(); // End Left Column


        // ==================== RIGHT COLUMN: SECONDARY HEROES & GARRISON (Width: 395f) ====================
        GUILayout.BeginVertical(GUILayout.Width(395f));

        // Bento Card 4: Secondary Garrison Commanders
        GUILayout.BeginVertical(s_spyCardBgStyle);
        GUILayout.Label(GetText9(
            "👥 Вторичные Военачальники Крепости", "👥 Secondary Garrison Commanders",
            "👥 Sekundäre Garnisonskommandanten", "👥 Commandants de garnison secondaires",
            "👥 Comandantes de guarnición secundarios", "👥 Comandantes da guarnição secundários",
            "👥 駐屯副将・随行英雄リスト", "👥 가리온성 주둔 부장 사령관",
            "👥 麾下驻防守城副将总览"
        ), s_spySectionTitleStyle);
        GUILayout.Space(6);

        int simpleHeroCount = castle.level >= 4 ? 2 : (castle.level >= 2 ? 1 : 0);

        if (spyInfoLvl == 1)
        {
            GUILayout.Label($" • {GetText9("Количество военачальников:", "Commanders count:", "Anzahl Kommandanten:", "Nombre de commandants :", "Cantidad de comandantes:", "Quantidade de comandantes:", "随行将領数:", "부장 영웅 수:", "麾下副将将领数:")} <color=gray>??? [Low Intel]</color>", s_spyDetailLabelStyle);
        }
        else
        {
            if (simpleHeroCount == 0)
            {
                GUILayout.Label($" • <color=gray>{GetText9("Вторичных военачальников нет", "No secondary commanders", "Keine sekundären Kommandanten", "Aucun commandant secondaire", "Sin comandantes secundarios", "Sem comandantes secundários", "副将・随行英雄はいません", "성내에 배속된 부장이 없습니다", "本城堡未部署驻防副将")}</color>", s_spyDetailLabelStyle);
            }
            else
            {
                for (int h = 0; h < simpleHeroCount; h++)
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    
                    // Mini avatar box
                    Rect miniAvRect = GUILayoutUtility.GetRect(36f, 36f, GUILayout.Width(36f), GUILayout.Height(36f));
                    GUI.color = new Color(0.1f, 0.1f, 0.18f, 1f);
                    GUI.DrawTexture(miniAvRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    Texture2D subHeroAv = null;
                    string subClassName = "";
                    if (h == 0)
                    {
                        subHeroAv = GetTroopAvatarTexture(castle.zoneIndex == 3 ? "paladin" : "archer");
                        subClassName = castle.zoneIndex == 3 ? GetText9("Защитник Света", "Shield of Light", "Schild des Lichts", "Bouclier de Lumière", "Escudo de Luz", "Escudo de Luz", "光の盾", "빛의 성방패", "誓约守卫圣骑士") :
                                                              GetText9("Дозорный", "Scout Ranger", "Späher", "Ranger Éclaireur", "Explorador", "Explorador", "偵察兵", "정찰 순찰자", "远哨穿林神射手");
                    }
                    else
                    {
                        subHeroAv = GetTroopAvatarTexture(castle.zoneIndex == 11 ? "necromancer" : "mage");
                        subClassName = castle.zoneIndex == 11 ? GetText9("Чернокнижник", "Void Warlock", "Hexenmeister", "Démoniste du Vide", "Brujo del Vacío", "Bruxo do Vazio", "ウォーロック", "공허 흑마법사", "暗影死灵大法师") :
                                                               GetText9("Чародей", "Elemental Spellweaver", "Elementarmagier", "Sorcier", "Hechicero", "Feiticeiro", "スペルウィーバー", "원소 마법사", "秘教自然大法师");
                    }

                    if (spyInfoLvl >= 3 && subHeroAv != null)
                    {
                        GUI.DrawTexture(miniAvRect, subHeroAv, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.Label(miniAvRect, "?", s_spyBoxLabelStyle);
                    }

                    GUILayout.BeginVertical();
                    string secHeroName = h == 0 ? GetText9("Лейтенант Вальд", "Lieutenant Vald", "Leutnant Vald", "Lieutenant Vald", "Teniente Vald", "Tenente Vald", "ヴァルド副官", "발드 부관", "驻城校尉 华德") :
                                                 GetText9("Советник Элдрик", "Advisor Eldric", "Berater Eldric", "Conseiller Eldric", "Asesor Eldric", "Assessor Eldric", "エルドリック顧問", "엘드릭 고문", "军师法师 埃尔德里克");

                    string subHeroLvlStr = (spyInfoLvl >= 3) ? $"{GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 2)}" : "???";

                    GUILayout.Label($"<b>{secHeroName}</b> ({subHeroLvlStr})", s_spyDetailLabelStyle);
                    
                    if (spyInfoLvl >= 3)
                    {
                        GUILayout.Label($"🏷️ {GetText9("Класс:", "Class:", "Klasse:", "Classe :", "Clase:", "Classe:", "クラス:", "클래스:", "职业:")} <color=cyan>{subClassName}</color>", s_spyDetailLabelStyle);
                    }
                    else
                    {
                        GUILayout.Label($"🏷️ {GetText9("Класс:", "Class:", "Klasse:", "Classe :", "Clase:", "Classe:", "クラス:", "클래스:", "职业:")} <color=gray>??? [Locked]</color>", s_spyDetailLabelStyle);
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();
                }
            }
        }

        GUILayout.EndVertical(); // End Bento Card 4

        GUILayout.Space(6);

        // Bento Card 5: Garrison Troops & Cohorts
        GUILayout.BeginVertical(s_spyCardBgStyle);
        GUILayout.Label(GetText9(
            "⚔️ Гарнизон и Регулярные Войска", "⚔️ Static Garrison & Regular Army",
            "⚔️ Garrison & Truppen", "⚔️ Garrison et armée régulière",
            "⚔️ Guarnición y ejército regular", "⚔️ Guarnição e exército regular",
            "⚔️ 要塞防衛駐屯兵団・配下軍勢", "⚔️ 수비대 주둔 병력 목록",
            "⚔️ 城堡城防防御常备军队与兵种编制"
        ), s_spySectionTitleStyle);
        GUILayout.Space(6);

        int totalTroops = 100 * castle.level + (castle.aiCommanderLevel * 30);
        
        if (spyInfoLvl == 1)
        {
            GUILayout.Label($"📈 {GetText9("Общая численность:", "Total Garrison:", "Garnisonsstärke:", "Effectif total :", "Fuerza total:", "Força total:", "総防衛兵力:", "총 수비대 병력:", "驻防守军总兵力:")} <color=yellow>~{totalTroops - 50} - {totalTroops + 80} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
            GUILayout.Label($"⚠️ <color=gray>{GetText9("Детальный состав засекречен. Повысьте уровень шпионажа!", "Detailed composition classified. Raise intelligence level!", "Details klassifiziert. Erhöhen Sie das Spionageniveau!", "Composition détaillée secrète. Augmentez le niveau !", "Composición clasificada. ¡Aumenta el nivel de espionaje!", "Composição secreta. Aumente o nível de espionagem!", "詳細な内訳データは不明です。スパイ活動を強化してください！", "상세 병력 편성 잠금. 첩보 능력을 더 강화하십시오!", "编制详情受战雾阻挡，建议往城堡派遣更高阶 the 探子！")}</color>", s_spyDetailLabelStyle);
        }
        else if (spyInfoLvl == 2)
        {
            GUILayout.Label($"📈 {GetText9("Общая численность:", "Total Garrison:", "Garnisonsstärke:", "Effectif total :", "Fuerza total:", "Força total:", "総防衛兵力:", "총 수비대 병력:", "驻防守军总兵力:")} <color=yellow>{totalTroops} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
            GUILayout.Space(4);
            GUILayout.Label($"• T1 {GetText9("Ополченцы:", "Recruits:", "Rekruten:", "Recrues :", "Reclutas:", "Recrutas:", "T1 軽装歩兵:", "T1 초급 보병:", "T1 常备民兵:")} <color=cyan>~{(int)(totalTroops * 0.7f)}</color>", s_spyDetailLabelStyle);
            GUILayout.Label($"• T2 {GetText9("Гвардейцы:", "Guards:", "Wachen:", "Gardes :", "Guardias:", "Guardas:", "T2 常備重装兵:", "T2 정예 경비대:", "T2 坚毅卫队:")} <color=cyan>~{(int)(totalTroops * 0.3f)}</color>", s_spyDetailLabelStyle);
        }
        else if (spyInfoLvl == 3)
        {
            GUILayout.Label($"📈 {GetText9("Общая численность:", "Total Garrison:", "Garnisonsstärke:", "Effectif total :", "Fuerza total:", "Força total:", "総防衛兵力:", "총 수비대 병력:", "驻防守军总兵力:")} <color=yellow>{totalTroops} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
            GUILayout.Space(4);
            int countT1 = (int)(totalTroops * 0.6f);
            int countT2 = (int)(totalTroops * 0.3f);
            int countT3 = totalTroops - countT1 - countT2;

            GUILayout.Label($"⚔️ T1 {GetText9("Пехота / Луки:", "Infantry / Archers:", "Infanterie / Bögen:", "Infanterie / Arcs :", "Infantería / Arcos:", "Infantaria / Arcos:", "T1 軽装兵 / 射手:", "T1 보병 / 궁수:", "T1 步射军团:")} <color=cyan>{countT1} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
            GUILayout.Label($"🐎 T2 {GetText9("Кавалерия / Рыцари:", "Cavalry / Knights:", "Kavallerie / Ritter:", "Cavalerie / Chevaliers :", "Caballería / Caballeros:", "Cavalaria / Cavaleiros:", "T2 騎兵 / 騎士:", "T2 기병 / 기사:", "T2 铁骑骑士团:")} <color=cyan>{countT2} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
            GUILayout.Label($"🪄 T3 {GetText9("Маги / Клирики:", "Mages / Clerics:", "Magier / Kleriker:", "Mages / Clercs :", "Magos / Clérigos:", "Magos / Clérigos:", "T3 魔術師 / 神官:", "T3 성직 마법사:", "T3 祭司法师团:")} <color=cyan>{countT3} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "人")}</color>", s_spyDetailLabelStyle);
        }
        else
        {
            // spyInfoLvl >= 4: ABSOLUTE MAX INTEL DETAILED LIST (Matches fourth screenshot perfectly!)
            GUILayout.Label($"📈 {GetText9("Точная численность:", "Exact Garrison Strength:", "Genaue Garnisonsstärke:", "Effectif exact :", "Fuerza exacta:", "Força exata:", "正確な駐屯軍勢:", "정밀 사령부 주둔병력:", "城堡常备守军各师团精确人数:")} <color=#55FF55>{totalTroops} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "名将士")}</color>", s_spyDetailLabelStyle);
            GUILayout.Space(6);

            // Dynamically define what cohorts guard based on castle zone & level
            string[] troopIds;
            string[] troopNames;
            string[] troopAbilities;
            int[] troopCounts;
            int[] troopLvls;

            if (castle.zoneIndex == 3) // Mage Castle
            {
                troopIds = new string[] { "mage", "necromancer", "paladin", "dragon" };
                troopNames = new string[] {
                    GetText9("Орден Магов", "Order of Mages", "Orden der Magier", "Ordre des Mages", "Orden de Magos", "Ordem de Magos", "ウィザード魔導団", "마법 결사대", "奥术编队法师"),
                    GetText9("Чернокнижники", "Acolyte Necromancers", "Nekromanten", "Nécromanciens", "Nigromantes", "Necromantes", "ネクロマンサー", "사령마법 학도", "暗黑死灵祭司"),
                    GetText9("Охранные Рыцари", "Temple Guardians", "Tempelwächter", "Gardiens du Temple", "Guardianes de Templo", "Guardas de Templo", "テンプルガー디アン", "사원 수호 성기사", "誓约守卫骑士"),
                    GetText9("Дракон Пустоты", "Ethereal Drake", "Ätherischer Drache", "Dragon Éthéré", "Dragón Etéreo", "Dragão Etéreo", "エテールドラゴン", "에테르 공허룡", "终焉虚空巨龙")
                };
                troopAbilities = new string[] {
                    GetText9("Сжигание Маны, Огненный Шар", "Mana Burn, Fireball", "Mana-Burn, Fireball", "Brûlure de mana", "Quemadura de maná", "Queima de mana", "マナバーン、ファイアボール", "마나 연소, 파이어볼", "法力燃烧, 爆裂火球"),
                    GetText9("Поднятие Мертвых, Проклятие", "Raise Dead, Decay Curse", "Tote erwecken, Fluch", "Animation des morts", "Levantar muertos", "Ressuscitar mortos", "レイズデッド、カース", "시체 되살리기, 부패 저주", "亡者复苏, 衰弱诅咒"),
                    GetText9("Святая Аура, Железный Щит", "Holy Devotion, Iron Wall", "Heilige Hingabe, Wand", "Dévotion sacrée", "Devoción sagrada", "Devoção sagrada", "ホーリーシールド、アイアンウォール", "성스러운 오ра, 철벽 방어", "圣光护体, 圣印铁壁"),
                    GetText9("Дыхание Бездны, Фазовый Сдвиг", "Void Breath, Phase Shift", "Abgrundodem, Phasenverschiebung", "Souffle du Vide", "Aliento del Vacío", "Sopro do Vazio", "ボイドブレス、フェーズシフト", "공허의 숨결, 차원 이탈", "毁灭虚空吐息, 空间跳跃")
                };
                troopCounts = new int[] { (int)(totalTroops * 0.45f), (int)(totalTroops * 0.25f), (int)(totalTroops * 0.2f), Mathf.Max(1, totalTroops / 150) };
                troopLvls = new int[] { 3, 3, 2, 4 };
            }
            else if (castle.zoneIndex == 11) // Archer Castle
            {
                troopIds = new string[] { "archer", "centaur", "griffin", "wasteland_serpent" };
                troopNames = new string[] {
                    GetText9("Стрелки Пустошей", "Wasteland Marksmen", "Scharfschützen", "Tireurs d'élite", "Tiradores de Élite", "Atiradores de Elite", "荒野のスナイパー", "황무지 명사수대", "荒野疾风巡林射手"),
                    GetText9("Кентавры-Рейнджеры", "Centaur Rangers", "Zentauren-Waldläufer", "Rangers Centaures", "Exploradores Centauros", "Exploradores Centauros", "ケンタウルスレンジャー", "켄타우로스 순찰대", "半人马追猎射手"),
                    GetText9("Королевские Грифоны", "Royal Sky-Griffins", "Königliche Greifen", "Griffons Royaux", "Grifos Reales", "Grifos Reais", "ロイヤルグリフォン", "황실 스카이 그리핀", "帝国皇家铁羽狮鹫"),
                    GetText9("Песчаный Змей", "Apex Dunes Serpent", "Wüstenriesenschlange", "Serpent des Dunes", "Serpiente de las Dunas", "Serpente das Dunas", "サンドワーム", "황사 사막독사", "极度深渊沙海巨蟒")
                };
                troopAbilities = new string[] {
                    GetText9("Критический Выстрел, Точность", "Eagle Strike, True Shot", "Adlerschlag, Volltreffer", "Coup de l'aigle", "Golpe de águila", "Golpe de águia", "イーグルストライк、必中撃", "독수리 일격, 명사수", "极寒锁敌, 破甲重箭"),
                    GetText9("Стрельба на Ходу, Быстрый Бег", "Run & Gun, Swift Fleeting", "Schießen im Lauf, Flucht", "Tir en mouvement", "Disparar en carrera", "Atirar em movimento", "ヒット＆ラン、韋駄天", "기동 사격, 신속 이동", "游击飞射, 迅捷疾行"),
                    GetText9("Пикирующий Удар, Сбивание ног", "Diving Slash, Air Dominance", "Sturzflug, Luftherrschaft", "Choc en piqué", "Corte en picado", "Corte em mergulho", "ダイブスラッシュ、空中支配", "급강하 타격, 공중 제압", "俯冲爪击, 气流压制"),
                    GetText9("Ядовитый Плевок, Подземный Удар", "Viper Spit, Burrow Strike", "Giftspeichel, Tunnelangriff", "Crachat venimeux", "Escupitajo venenoso", "Cuspido venenoso", "バイパースピット、潜地襲撃", "독사 침뱉기, 잠행 습격", "剧毒腐蚀撕咬, 潜地突袭")
                };
                troopCounts = new int[] { (int)(totalTroops * 0.5f), (int)(totalTroops * 0.25f), (int)(totalTroops * 0.2f), Mathf.Max(1, totalTroops / 150) };
                troopLvls = new int[] { 3, 2, 3, 4 };
            }
            else // Warrior / Paladin Castles
            {
                troopIds = new string[] { "warrior", "paladin", "cavalry", "overlord" };
                troopNames = new string[] {
                    GetText9("Гвардия Цитадели", "Citadel Heavy Infantry", "Schwere Infanterie", "Infanterie Lourde", "Infantería Pesada", "Infantaria Pesada", "要塞重装歩兵", "성채 중갑 보병대", "城堡重装大剑步兵"),
                    GetText9("Храмовники Света", "Templars of Light", "Licht-Templer", "Templiers de Lumière", "Templarios de la Luz", "Templários da Luz", "光の神殿騎士団", "빛의 성전 기사단", "圣光荣耀重铠骑兵"),
                    GetText9("Авангардная Кавалерия", "Royal Vanguard Cavalry", "Vanguard-Kavallerie", "Cavalerie d'Avant-garde", "Caballería de Vanguardia", "Cavalaria de Vanguarda", "ヴァンガード重騎兵", "왕실 선봉 기병대", "皇家帝国铁甲游骑"),
                    GetText9("Вождь Легиона", "Legion Iron Overlord", "Eisen-Overlord", "Suprême de Fer", "Señor de Hierro", "Senhor de Ferro", "アイアンオーバーロード", "군단 철혈 지배자", "深渊熔岩军团霸主")
                };
                troopAbilities = new string[] {
                    GetText9("Железный Щит, Гневная Атака", "Phalanx Shield, Angry Bash", "Phalanx-Schild, Hieb", "Bouclier de phalange", "Escudo de falange", "Escudo de falange", "ファランクスシールド、バッシュ", "방진 방패, 성난 후려치기", "方阵壁垒, 怒火狂击"),
                    GetText9("Молитва Жизни, Изгнание", "Lay on Hands, Exorcism Aura", "Händeauflegen, Aura", "Imposition des mains", "Imposición de manos", "Imposição das mãos", "レイオンハンズ、除霊の輝き", "안수기도, 퇴마 수호", "圣光救赎, 驱邪圣怒"),
                    GetText9("Рыцарский Таран, Смятие рядов", "Heavy Trample, Lance Charge", "Ritter-Trampeln, Ansturm", "Piétinement lourd", "Atropello pesado", "Atropelamento pesado", "ヘбиートランプル、チャージ", "묵직한 짓밟기, 랜스 돌격", "战马践踏, 骑枪突击"),
                    GetText9("Раскол Земли, Свирепый Крик", "Earthquake Shock, Battle Cry", "Earthquake Shock, Battle Cry", "Secousse sismique", "Sacudida sísmica", "Choque de terremoto", "アースクエイク、咆哮", "대지 균열, 전쟁의 함성", "地裂山崩震荡, 战神狂哮")
                };
                troopCounts = new int[] { (int)(totalTroops * 0.45f), (int)(totalTroops * 0.25f), (int)(totalTroops * 0.25f), Mathf.Max(1, totalTroops / 150) };
                troopLvls = new int[] { 2, 3, 3, 4 };
            }

            for (int t = 0; t < troopIds.Length; t++)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                
                // Avatar representation for troop row
                Rect tAvRect = GUILayoutUtility.GetRect(40f, 40f, GUILayout.Width(40f), GUILayout.Height(40f));
                GUI.color = new Color(0.12f, 0.12f, 0.22f, 1f);
                GUI.DrawTexture(tAvRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Texture2D tAv = GetTroopAvatarTexture(troopIds[t]);
                if (tAv != null)
                {
                    GUI.DrawTexture(tAvRect, tAv, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.Label(tAvRect, "⚔️", s_spyBoxLabelStyle);
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>{troopNames[t]}</b>", s_spyDetailLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color=#55FF55>x{troopCounts[t]} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "名将士")}</color>", s_spyDetailLabelStyle);
                GUILayout.EndHorizontal();

                GUIStyle tSubStyle = new GUIStyle(GUI.skin.label);
                tSubStyle.fontSize = 10;
                tSubStyle.normal.textColor = Color.gray;
                GUILayout.Label($"{GetText9("Ранг:", "Rank:", "Rang:", "Rang :", "Rango:", "Rango:", "兵級:", "클래스 티어:", "兵种等级:")} T{troopLvls[t]}  •  💡 {GetText9("Умения:", "Traits:", "Fähigkeiten:", "Spécialités :", "Habilidades:", "Habilidades:", "特技:", "부대 특성:", "主要能力:")} <color=orange>{troopAbilities[t]}</color>", tSubStyle);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndVertical(); // End Bento Card 5

        GUILayout.EndVertical(); // End Right Column

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        
        // Footer: Close Button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(GetText9("Закрыть Отчет", "Close Report", "Bericht schließen", "Fermer le rapport", "Cerrar informe", "Fechar relatório", "報告書を閉じる", "보고서 닫기", "关闭情报窗口"), GUILayout.Height(32)))
        {
            showSpyReportPopup = false;
        }
        GUI.backgroundColor = Color.white;
    }

    private void SpyReportWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        
        // Find all spied enemy castles
        List<CastleInstance> spiedCastles = new List<CastleInstance>();
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Enemy" && PlayerPrefs.GetInt("Castle_Spied_" + castles[i].zoneIndex, 0) == 1)
            {
                spiedCastles.Add(castles[i]);
            }
        }

        if (spiedCastles.Count == 0)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUIStyle emptyS = new GUIStyle(GUI.skin.label);
            emptyS.alignment = TextAnchor.MiddleCenter;
            emptyS.fontSize = 14;
            emptyS.normal.textColor = Color.gray;
            GUILayout.Label(GetText9(
                "Нет активных отчетов. Зашлите шпионов в другие замки!",
                "No active reports. Infiltrate spies into enemy castles!",
                "Keine Berichte. Senden Sie Spione in feindliche Burgen!",
                "Aucun rapport. Infiltrez des espions dans les châteaux !",
                "Sin informes activos. ¡Infiltra espías en castillos enemigos!",
                "Sem relatórios. Infiltre espiões em castelhos inimigos!",
                "アクティブな報告書はありません。敵の城にスパイを放ちましょう！",
                "활성화된 보고서가 없습니다. 적의 성에 간첩을 파견하십시오!",
                "暂无活跃情报。请先在其它敌方城堡部署探子！"
            ), emptyS);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetText9("Закрыть", "Close", "Schließen", "Fermer", "Cerrar", "Fechar", "閉じる", "닫기", "关闭"), GUILayout.Height(30)))
            {
                showSpyReportPopup = false;
            }
            GUILayout.EndVertical();
            return;
        }

        // Set default activeSpyReportZoneIndex if not set or invalid
        if (activeSpyReportZoneIndex == -1)
        {
            activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
        }
        else
        {
            bool found = false;
            for (int i = 0; i < spiedCastles.Count; i++)
            {
                if (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                activeSpyReportZoneIndex = spiedCastles[0].zoneIndex;
            }
        }

        CastleInstance castle = null;
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].zoneIndex == activeSpyReportZoneIndex)
            {
                castle = castles[i];
                break;
            }
        }

        if (castle == null) return;

        // Draw tabs for each spied castle so player can select which one to view!
        GUILayout.Label(GetText9(
            "Выберите вражескую цитадель:", "Select Enemy Stronghold:",
            "Wählen Sie die feindliche Burg:", "Sélectionnez le château ennemi :",
            "Seleccionar fortaleza enemiga:", "Selecionar fortaleza inimiga:",
            "対象の敵城を選択してください:", "적 성채 선택:",
            "选择目标城堡进行查看:"
        ), GUI.skin.label);

        GUILayout.BeginHorizontal();
        for (int i = 0; i < spiedCastles.Count; i++)
        {
            string tabName = curLang == 0 ? spiedCastles[i].nameRU : spiedCastles[i].nameEN;
            if (curLang == 8) tabName = spiedCastles[i].nameCH;
            if (curLang == 7) tabName = spiedCastles[i].nameKR;

            if (tabName.Length > 15) tabName = tabName.Substring(0, 14) + "..";

            GUI.backgroundColor = (spiedCastles[i].zoneIndex == activeSpyReportZoneIndex) ? new Color(1.0f, 0.85f, 0.15f, 1.0f) : Color.white;
            if (GUILayout.Button($"🏰 {tabName}", GUILayout.Height(28)))
            {
                activeSpyReportZoneIndex = spiedCastles[i].zoneIndex;
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.PlayHoverSound(0);
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Retrieve saved spy details level (playerMaxCastleLvl when spied)
        int spyInfoLvl = PlayerPrefs.GetInt("Castle_Spied_Lvl_" + castle.zoneIndex, 2);

        // Display current target details
        string castleLabel = curLang == 0 ? castle.nameRU : castle.nameEN;
        if (curLang == 8) castleLabel = castle.nameCH;
        if (curLang == 7) castleLabel = castle.nameKR;

        GUIStyle subHeader = new GUIStyle(GUI.skin.box);
        subHeader.normal.textColor = Color.yellow;
        subHeader.fontStyle = FontStyle.Bold;
        subHeader.fontSize = 14;
        subHeader.alignment = TextAnchor.MiddleCenter;

        GUILayout.Box($"🕵️ {castleLabel.ToUpper()} [Intel Level {spyInfoLvl}]", subHeader, GUILayout.Height(30));

        GUILayout.Space(6);

        // Scrollview for details
        spyScrollPos = GUILayout.BeginScrollView(spyScrollPos, GUILayout.ExpandHeight(true));

        GUIStyle sectionTitleS = new GUIStyle(GUI.skin.label);
        sectionTitleS.fontSize = 14;
        sectionTitleS.fontStyle = FontStyle.Bold;
        sectionTitleS.normal.textColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);

        GUIStyle detailLabelS = new GUIStyle(GUI.skin.label);
        detailLabelS.fontSize = 12;
        detailLabelS.normal.textColor = Color.white;

        // 1. MAIN HERO DETAILS
        GUILayout.Label(GetText9(
            "👤 Главный Военачальник Гарнизона", "👤 Garrison Arch-Commander",
            "👤 Garnisonskommandant", "👤 Commandant de la garnison",
            "👤 Comandante de la guarnición", "👤 Comandante da guarnição",
            "👤 敵要塞最高司令官", "👤 가리온 요새 수석 사령관",
            "👤 要塞守御主将指挥官"
        ), sectionTitleS);

        // Determine commander class name based on zone index
        string cmdClassRU = "Рыцарь-Воин";
        string cmdClassEN = "Knight-Warrior";
        string cmdClassDE = "Ritter-Krieger";
        string cmdClassFR = "Chevalier-Guerrier";
        string cmdClassES = "Caballero Guerrero";
        string cmdClassPT = "Cavaleiro Guerreiro";
        string cmdClassJA = "ナイトウォーリアー";
        string cmdClassKO = "나이트 워리어";
        string cmdClassZH = "重装骑士领主";

        if (castle.zoneIndex == 3)
        {
            cmdClassRU = "Архимаг Зенита";
            cmdClassEN = "Zenith Arch-Mage";
            cmdClassDE = "Zenith Erzmagier";
            cmdClassFR = "Archimage de Zénith";
            cmdClassES = "Archimago del Cénit";
            cmdClassPT = "Arquimago do Zênite";
            cmdClassJA = "ゼニス・アークメイジ";
            cmdClassKO = "제니스 아크메이지";
            cmdClassZH = "至高神庭奥术大法师";
        }
        else if (castle.zoneIndex == 6)
        {
            cmdClassRU = "Паладин Света";
            cmdClassEN = "Holy Paladin";
            cmdClassDE = "Heiliger Paladin";
            cmdClassFR = "Paladin Sacré";
            cmdClassES = "Paladín Sagrado";
            cmdClassPT = "Paladino Sagrado";
            cmdClassJA = "ホーリーパラディン";
            cmdClassKO = "홀리 팔라딘";
            cmdClassZH = "圣光大审判骑士长";
        }
        else if (castle.zoneIndex == 11)
        {
            cmdClassRU = "Следопыт Пустошей";
            cmdClassEN = "Wasteland Ranger";
            cmdClassDE = "Wüstenläufer";
            cmdClassFR = "Ranger des Terres Dévastées";
            cmdClassES = "Ranger de los Páramos";
            cmdClassPT = "Patrulheiro do Ermo";
            cmdClassJA = "荒野のレンジャー";
            cmdClassKO = "황무지 순찰대";
            cmdClassZH = "荒野游侠神射手";
        }

        string cmdClass = GetText9(cmdClassRU, cmdClassEN, cmdClassDE, cmdClassFR, cmdClassES, cmdClassPT, cmdClassJA, cmdClassKO, cmdClassZH);

        // Worn gear representation
        string gearWeapon = "???";
        string gearArmor = "???";
        string gearBoots = "???";
        string gearShield = "???";

        if (spyInfoLvl >= 3)
        {
            if (castle.aiArmorTier == 1)
            {
                gearWeapon = GetText9("Бронзовый Меч", "Bronze Sword", "Bronzeschwert", "Épée de bronze", "Espada de bronce", "Espada de Bronze", "ブロンズソード", "청동 검", "青铜阔剑");
                gearArmor = GetText9("Кожаный Нагрудник", "Leather Chest", "Lederbrustplatte", "Plastron de cuir", "Peto de cuero", "Colete de Couro", "レザーチェスト", "가죽 갑옷", "皮质护胸甲");
            }
            else if (castle.aiArmorTier == 2)
            {
                gearWeapon = GetText9("Стальной Молот", "Steel Mace", "Stahlstreitkolben", "Masse d'acier", "Maza de acero", "Maça de Aço", "スチールメイス", "강철 메이스", "精钢战锤");
                gearArmor = GetText9("Кольчужный Доспех", "Chainmail", "Kettenhemd", "Cotte de mailles", "Cota de malla", "Cota de Malha", "チェインメイル", "사슬 갑옷", "精炼锁子甲");
            }
            else if (castle.aiArmorTier == 3)
            {
                gearWeapon = GetText9("Мифриловый Двуручник", "Mithril Greatsword", "Mithril-Zweihänder", "Espadon de mithril", "Espadón de mitril", "Montante de Mitril", "ミスリル・グレートソード", "미스릴 대검", "秘银双手巨剑");
                gearArmor = GetText9("Латы Рыцаря", "Plate Armor", "Plattenrüstung", "Harnois de plaques", "Armadura de placas", "Armadura de Placa", "プレートアーマー", "판금 갑옷", "圣殿骑士重铠");
            }
            else
            {
                gearWeapon = GetText9("Клинок Зенита", "Divine Zenith Blade", "Göttliche Zenith-Klinge", "Lame de Zénith Divine", "Espada del Cénit Divina", "Espada do Zênite Divina", "神聖ゼニスブレード", "신성 제니스 블레이드", "至臻封神裁决之刃");
                gearArmor = GetText9("Броня Небожителя", "Celestial Armor", "Himmlische Rüstung", "Armure Céleste", "Armadura Celestial", "Armadura Celestial", "セレスティアルアーマー", "천상 갑옷", "圣光天神不灭金铠");
            }

            if (spyInfoLvl < 4)
            {
                gearBoots = "???";
                gearShield = "???";
            }
            else
            {
                gearBoots = GetText9("Кованые Сапоги", "Plated Sabatons", "Plattenschuhe", "Sabatons de plaques", "Sabatones de placas", "Soleretes de Placa", "サバトン", "철 판금 장화", "精铁重装护足");
                gearShield = GetText9("Башенный Щит", "Tower Shield", "Turmschild", "Écu pavois", "Escudo de torre", "Escudo Torre", "タワーシールド", "타워 실드", "巨龙不灭重型塔盾");
            }
        }

        string displayCmdClass = (spyInfoLvl >= 2) ? cmdClass : "???";
        string displayCmdLvl = (spyInfoLvl >= 2) ? $"Lvl {castle.aiCommanderLevel}" : "???";

        GUILayout.Label($" • {GetText9("Класс:", "Class:", "Klasse:", "Classe :", "Clase:", "Classe:", "クラス:", "클래스:", "职业:")} {displayCmdClass}", detailLabelS);
        GUILayout.Label($" • {GetText9("Уровень:", "Level:", "Stufe:", "Niveau :", "Nivel:", "Nível:", "レベル:", "레벨:", "级别:")} {displayCmdLvl}", detailLabelS);
        GUILayout.Label($" • {GetText9("Экипировка (Оружие):", "Equipment (Weapon):", "Ausrüstung (Waffe):", "Équipement (Arme) :", "Equipo (Arma):", "Equipamento (Arma):", "装備 (武器):", "장비 (무기):", "穿戴装备 (主手武器):")} {gearWeapon}", detailLabelS);
        GUILayout.Label($" • {GetText9("Экипировка (Доспех):", "Equipment (Armor):", "Ausrüstung (Rüstung):", "Équipement (Armure) :", "Equipo (Armadura):", "Equipamento (Armadura):", "装備 (鎧):", "장비 (갑옷):", "穿戴装备 (胸部防具):")} {gearArmor}", detailLabelS);
        GUILayout.Label($" • {GetText9("Экипировка (Сапоги):", "Equipment (Boots):", "Ausrüstung (Stiefel):", "Équipement (Bottes) :", "Equipo (Botas):", "Equipamento (Botas):", "装備 (靴):", "장비 (장화):", "穿戴装备 (腿部足具):")} {gearBoots}", detailLabelS);
        GUILayout.Label($" • {GetText9("Экипировка (Щит):", "Equipment (Shield):", "Ausrüstung (Schild):", "Équipement (Bouclier) :", "Equipo (Escudo):", "Equipamento (Escudo):", "装備 (盾):", "장비 (방패):", "穿戴装备 (副手重盾):")} {gearShield}", detailLabelS);

        GUILayout.Space(10);

        // 2. SECONDARY HEROES DETAILS
        GUILayout.Label(GetText9(
            "👥 Вторичные Военачальники", "👥 Secondary Garrison Commanders",
            "👥 Sekundäre Kommandanten", "👥 Commandants secondaires",
            "👥 Comandantes secundarios", "👥 Comandantes secundários",
            "👥 副将指揮官リスト", "👥 부장 목록",
            "👥 麾下驻扎部将总览"
        ), sectionTitleS);

        int simpleHeroCount = castle.level >= 4 ? 2 : (castle.level >= 2 ? 1 : 0);
        string heroInfoText = "";
        if (spyInfoLvl == 1)
        {
            heroInfoText = $" • {GetText9("Количество героев:", "Heroes Count:", "Heldenanzahl:", "Nombre de héros :", "Héroes:", "Heróis:", "ヒーロー数:", "부장 수:", "部将数量:")} ???";
        }
        else if (spyInfoLvl == 2)
        {
            heroInfoText = $" • {GetText9("Количество героев:", "Heroes Count:", "Heldenanzahl:", "Nombre de héros :", "Héroes:", "Heróis:", "ヒーロー数:", "부장 수:", "部将数量:")} {simpleHeroCount}";
        }
        else if (spyInfoLvl == 3)
        {
            if (simpleHeroCount == 0)
            {
                heroInfoText = $" • {GetText9("Вторичных героев нет", "No secondary heroes", "Keine sekundären Helden", "Aucun héros secondaire", "Sin héroes secundarios", "Sem heróis secundários", "副将はいません", "부장이 없습니다", "暂无麾下副将")}";
            }
            else if (simpleHeroCount == 1)
            {
                heroInfoText = $" • 1 {GetText9("Герой", "Hero", "Held", "Héros", "Héroe", "Herói", "ヒーロー", "영웅", "部将")} ({GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 2)})";
            }
            else
            {
                heroInfoText = $" • 2 {GetText9("Героя", "Heroes", "Helden", "Héros", "Héroes", "Heróis", "ヒーロー", "영웅들", "部将")} ({GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 2)})";
            }
        }
        else
        {
            // spyInfoLvl >= 4: Detailed listing
            if (simpleHeroCount == 0)
            {
                heroInfoText = $" • {GetText9("Вторичных героев нет", "No secondary heroes", "Keine sekundären Helden", "Aucun héros secondaire", "Sin héroes secundarios", "Sem heróis secundários", "副将はいません", "부장이 없습니다", "暂无麾下副将")}";
            }
            else if (simpleHeroCount == 1)
            {
                string class1 = GetText9("Следопыт", "Ranger", "Läufer", "Ranger", "Ranger", "Patrulheiro", "レンジャー", "순찰자", "野外巡林散兵");
                heroInfoText = $" • {class1} ({GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 2)})";
            }
            else
            {
                string class1 = GetText9("Следопыт", "Ranger", "Läufer", "Ranger", "Ranger", "Patrulheiro", "レンジャー", "순찰자", "野外巡林散兵");
                string class2 = GetText9("Архимаг", "Arch-Mage", "Erzmagier", "Archimage", "Archimago", "Arquimago", "アークメイジ", "아크메이지", "奥术秘法祭司");
                heroInfoText = $" • {class1} ({GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 2)})\n" +
                               $" • {class2} ({GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "级")} {Mathf.Max(1, castle.aiCommanderLevel - 1)})";
            }
        }
        GUILayout.Label(heroInfoText, detailLabelS);

        GUILayout.Space(10);

        // 3. TROOPS IN CASTLE (GARRISON)
        GUILayout.Label(GetText9(
            "⚔️ Регулярный Гарнизон Замка", "⚔️ Active Fortress Garrison Troops",
            "⚔️ Aktive Garnisonstruppen", "⚔️ Troupes actives de la garnison",
            "⚔️ Tropas activas de la guarnición", "⚔️ Tropas ativas da guarnição",
            "⚔️ 要塞駐屯軍正規兵員", "⚔️ 요새 활성 주둔군 병력 수",
            "⚔️ 守城防御正规驻军"
        ), sectionTitleS);

        string troopsInfo = "";
        int power = castle.aiTroopsPower;

        // Specific integer calculations for troops
        int t1Count = power / 2;
        int t2Count = power / 4;
        int t3Count = power / 8;
        int t4Count = power / 16;

        if (t1Count < 0) t1Count = 0;
        if (t2Count < 0) t2Count = 0;
        if (t3Count < 0) t3Count = 0;
        if (t4Count < 0) t4Count = 0;

        if (spyInfoLvl == 1)
        {
            int minPower = Mathf.RoundToInt(power * 0.8f);
            int maxPower = Mathf.RoundToInt(power * 1.2f);
            troopsInfo = $" • {GetText9("Общая численность (оценка):", "Total Strength (approx):", "Gefährliche Stärke (ca.):", "Force totale (approx) :", "Fuerza total (aprox.):", "Força total (aprox.):", "総員 (推定):", "총 병력 (추정):", "守城军总兵力 (约):")} ~{minPower}..{maxPower} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                         $" • {GetText9("Состав войск:", "Troops Composition:", "Truppenzusammensetzung:", "Composition des troupes :", "Composición de tropas:", "Composição de tropas:", "部隊構成:", "부대 구성:", "守军配置:")} ???";
        }
        else if (spyInfoLvl == 2)
        {
            troopsInfo = $" • {GetText9("Общая численность:", "Total Garrison Strength:", "Gesamte Garnisonsstärke:", "Force totale de la garnison :", "Fuerza total de la guarnición:", "Força total da guarnição:", "要塞駐屯軍総員:", "총 주둔 부대 전투력:", "守城军团总兵力:")} {power} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                         $" • {GetText9("Состав войск:", "Troops Composition:", "Truppenzusammensetzung:", "Composition des troupes :", "Composición de tropas:", "Composição de tropas:", "部隊構成:", "부대 구성:", "守军配置:")} ???";
        }
        else if (spyInfoLvl == 3)
        {
            troopsInfo = $" • {GetText9("Общая численность:", "Total Garrison Strength:", "Gesamte Garnisonsstärke:", "Force totale de la garnison :", "Fuerza total de la guarnición:", "Força total da guarnição:", "要塞駐屯軍総員:", "총 주둔 부대 전투력:", "守城军团总兵力:")} {power} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                         $" • {GetText9("Пехота (Т1):", "Infantry (T1):", "Infanterie (T1):", "Infanterie (T1) :", "Infantería (T1):", "Infantaria (T1):", "歩兵 (T1):", "보병 (T1):", "初阶前排步兵 (T1):")} {t1Count}\n" +
                         $" • {GetText9("Лучники (Т2):", "Archers (T2):", "Bogenschützen (T2):", "Archers (T2) :", "Arqueros (T2):", "Arqueiros (T2):", "弓兵 (T2):", "궁수 (T2):", "中阶射手后排 (T2):")} {t2Count}\n" +
                         $" • {GetText9("Элитные отряды (Т3+):", "Elite Cohorts (T3+):", "Elitekohorten (T3+):", "Cohortes d'élite (T3+) :", "Cohortes de élite (T3+):", "Coortes de Elite (T3+):", "精鋭部隊 (T3+):", "정예 부대 (T3+):", "高阶精锐军团 (T3+):")} ???";
        }
        else
        {
            // spyInfoLvl >= 4: Full breakdown of every troop tier
            troopsInfo = $" • {GetText9("Общая численность:", "Total Garrison Strength:", "Gesamte Garnisonsstärke:", "Force totale de la garnison :", "Fuerza total de la guarnición:", "Força total da guarnição:", "要塞駐屯軍総員:", "총 주둔 부대 전투력:", "守城军团总兵力:")} {power} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                         $" • {GetText9("Боец фракции (Т1):", "Faction Warriors (T1):", "Fraktionskrieger (T1):", "Guerriers de faction (T1) :", "Guerreros de facción (T1):", "Guerreiros de facção (T1):", "勢力戦士 (T1):", "분파 전사 (T1):", "阵营重装刀盾兵 (T1):")} {t1Count}\n" +
                         $" • {GetText9("Эльфийский Лучник (Т2):", "Elven Archers (T2):", "Elfenbogenschützen (T2):", "Archers elfes (T2) :", "Arqueros elfos (T2):", "Arqueiros élficos (T2):", "エルフ弓兵 (T2):", "엘프 궁수 (T2):", "暗夜精灵神射手 (T2):")} {t2Count}\n" +
                         $" • {GetText9("Паладин Света (Т3):", "Holy Paladins (T3):", "Heilige Paladine (T3):", "Paladins sacrés (T3) :", "Paladines sagrados (T3):", "Paladinos sagrados (T3):", "ホーリーパラディン (T3):", "성기사 (T3):", "神圣秩序圣骑士 (T3):")} {t3Count}\n" +
                         $" • {GetText9("Легендарный Дракон (Т4):", "Void Dragons (T4):", "Void-Drachen (T4):", "Dragons du Néant (T4) :", "Dragones del Vacío (T4):", "Dragões do Vazio (T4):", "ヴォイドドラゴン (T4):", "공허의 드래곤 (T4):", "灭世虚空古巨龙 (T4):")} {t4Count}";
        }
        GUILayout.Label(troopsInfo, detailLabelS);

        GUILayout.Space(10);

        // 4. TROOPS ON HEROES
        GUILayout.Label(GetText9(
            "⛺ Армии под началом Военачальников", "⛺ Cohorts Led by Garrison Commanders",
            "⛺ Kohorten unter Kommandanten", "⛺ Cohortes dirigées par les commandants",
            "⛺ Cohortes lideradas por comandantes", "⛺ Coortes sob Comando dos Líderes",
            "⛺ 各将領直属配下軍隊総数", "⛺ 사령관 지휘부대 현황",
            "⛺ 各部将直属统帅连队"
        ), sectionTitleS);

        string heroesTroops = "";
        if (spyInfoLvl == 1)
        {
            heroesTroops = $" • {GetText9("Войска у Главного Героя:", "Troops on Main Hero:", "Truppen des Haupthelden:", "Troupes du héros principal :", "Tropas del héroe principal:", "Tropas do herói principal:", "主将の配下部隊:", "수석 사령관 지휘부대:", "主将统领军队:")} ???\n" +
                           $" • {GetText9("Войска у Вторичных Героев:", "Troops on Secondary Heroes:", "Truppen der sekundären Helden:", "Troupes des héros secondaires :", "Tropas de héroes secundarios:", "Tropas dos heróis secundários:", "副заводские войска:", "보좌 사령관 지휘부대:", "副将统领军队:")} ???";
        }
        else if (spyInfoLvl == 2)
        {
            heroesTroops = $" • {GetText9("Войска у Главного Героя:", "Troops on Main Hero:", "Truppen des Haupthelden:", "Troupes du héros principal :", "Tropas del héroe principal:", "Tropas do herói principal:", "主将の配下部隊:", "수석 사령관 지휘부대:", "主将统领军队:")} {power / 2} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                           $" • {GetText9("Войска у Вторичных Героев:", "Troops on Secondary Heroes:", "Truppen der sekundären Helden:", "Troupes des héros secondaires :", "Tropas de héroes secundarios:", "Tropas dos heróis secundários:", "副将の配下部隊:", "보좌 사령관 지휘부대:", "副将统领军队:")} ???";
        }
        else if (spyInfoLvl == 3)
        {
            heroesTroops = $" • {GetText9("Войска у Главного Героя:", "Troops on Main Hero:", "Truppen des Haupthelden:", "Troupes du héros principal :", "Tropas del héroe principal:", "Tropas do herói principal:", "主将の配下部隊:", "수석 사령관 지휘부대:", "主将统领军队:")} {power / 2} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}\n" +
                           $" • {GetText9("Войска у Вторичных Героев:", "Troops on Secondary Heroes:", "Truppen der sekundären Helden:", "Troupes des héros secondaires :", "Tropas de héroes secundarios:", "Tropas dos heróis secundários:", "副将の配下部隊:", "보좌 사령관 지휘부대:", "副将统领军队:")} {power / 4} {GetText9("воинов", "warriors", "Krieger", "guerriers", "guerreros", "guerreiros", "名", "명", "兵")}";
        }
        else
        {
            heroesTroops = $" • {GetText9("Войска у Главного Героя:", "Troops on Main Hero:", "Truppen des Haupthelden:", "Troupes du héros principal :", "Tropas del héroe principal:", "Tropas do herói principal:", "主将の配下部隊:", "수석 사령관 지휘부대:", "主将统领军队:")} {power / 2} {GetText9("воинов (Т1/Т2)", "warriors (T1/T2)", "Krieger (T1/T2)", "guerriers (T1/T2)", "guerreros (T1/T2)", "guerreiros (T1/T2)", "名 (T1/T2)", "명 (T1/T2)", "名步射兵 (T1/T2)")}\n" +
                           $" • {GetText9("Войска у Вторичных Героев:", "Troops on Secondary Heroes:", "Truppen der sekundären Helden:", "Troupes des héros secondaires :", "Tropas de héroes secundarios:", "Tropas dos heróis secundários:", "副将の配下部隊:", "보좌 사령관 지휘부대:", "副将统领军队:")} {power / 4} {GetText9("воинов (Т2/Т3)", "warriors (T2/T3)", "Krieger (T2/T3)", "guerriers (T2/T3)", "guerreros (T2/T3)", "guerreiros (T2/T3)", "名 (T2/T3)", "명 (T2/T3)", "名射骑兵 (T2/T3)")}";
        }
        GUILayout.Label(heroesTroops, detailLabelS);

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(GetText9("Закрыть Отчет", "Close Report", "Bericht schließen", "Fermer le rapport", "Cerrar informe", "Fechar relatório", "報告書を閉じる", "보고서 닫기", "关闭情报窗口"), GUILayout.Height(30)))
        {
            showSpyReportPopup = false;
        }
        GUI.backgroundColor = Color.white;
    }

    private Texture2D GetTroopAvatarTexture(string id)
    {
        switch (id)
        {
            case "warrior": return avatar_warrior;
            case "archer": return avatar_archer;
            case "mage": return avatar_mage;
            case "paladin": return avatar_paladin;
            case "cavalry": return avatar_cavalry;
            case "cannoneer": return avatar_cannoneer;
            case "centaur": return avatar_centaur;
            case "necromancer": return avatar_necromancer;
            case "griffin": return avatar_griffin;
            case "overlord": return avatar_overlord;
            case "hydra": return avatar_hydra;
            case "dragon": return avatar_dragon;
            case "mountain_bear": return avatar_mountain_bear;
            case "wasteland_serpent": return avatar_wasteland_serpent;
            case "ArcherHero": return avatar_hero_archer != null ? avatar_hero_archer : (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.archerPortrait != null ? DialogueSystem_Manager.Instance.archerPortrait.texture : null);
            case "WarriorHero": return avatar_hero_warrior != null ? avatar_hero_warrior : (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.warriorPortrait != null ? DialogueSystem_Manager.Instance.warriorPortrait.texture : null);
            case "MageHero": return avatar_hero_mage != null ? avatar_hero_mage : (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.magePortrait != null ? DialogueSystem_Manager.Instance.magePortrait.texture : null);
        }
        return null;
    }

    private void DrawCastleCalibrationPanel(int curLang)
    {
        float panelWidth = 440f;
        float panelHeight = 350f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.05f, 0.15f, 0.08f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 14;
        windowStyle.fontStyle = FontStyle.Bold;

        string title = curLang == 0 ? "⚙️ Калибровка Позиций Замков (v18.11.23)" : "⚙️ Castle Position Calibration Panel";
        GUI.Window(102, new Rect(px, py, panelWidth, panelHeight), CalibrationWindowFunction, title, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void CalibrationWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        GUILayout.BeginVertical();

        GUILayout.Label(curLang == 0 ? "Выберите замок для калибровки 3D модели:" : "Select Castle to Calibrate 3D Model:", GUI.skin.label);

        string[] castleNames = new string[castles.Count];
        for (int i = 0; i < castles.Count; i++)
        {
            castleNames[i] = curLang == 0 ? castles[i].nameRU : castles[i].nameEN;
            if (castleNames[i].Length > 12) castleNames[i] = castleNames[i].Substring(0, 10) + "..";
        }

        selectedCalibCastleIdx = GUILayout.SelectionGrid(selectedCalibCastleIdx, castleNames, 3, GUILayout.Height(60));

        if (selectedCalibCastleIdx < 0 || selectedCalibCastleIdx >= castles.Count)
        {
            selectedCalibCastleIdx = 0;
        }

        CastleInstance activeC = castles[selectedCalibCastleIdx];
        int idx = activeC.zoneIndex;

        GUILayout.Space(10);
        GUILayout.Label(curLang == 0 ? $"Замок: {activeC.nameRU} (Зона {idx})" : $"Target: {activeC.nameEN} (Zone {idx})", GUI.skin.label);

        if (customCastlePositions != null && idx < customCastlePositions.Length)
        {
            Vector3 pos = customCastlePositions[idx];

            GUILayout.BeginHorizontal();
            GUILayout.Label($"X: {pos.x:F2}", GUILayout.Width(80));
            pos.x = GUILayout.HorizontalSlider(pos.x, -25f, 25f);
            if (GUILayout.Button("-", GUILayout.Width(25))) pos.x -= 0.1f;
            if (GUILayout.Button("+", GUILayout.Width(25))) pos.x += 0.1f;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Y: {pos.y:F2}", GUILayout.Width(80));
            pos.y = GUILayout.HorizontalSlider(pos.y, -5f, 5f);
            if (GUILayout.Button("-", GUILayout.Width(25))) pos.y -= 0.1f;
            if (GUILayout.Button("+", GUILayout.Width(25))) pos.y += 0.1f;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Z: {pos.z:F2}", GUILayout.Width(80));
            pos.z = GUILayout.HorizontalSlider(pos.z, -25f, 25f);
            if (GUILayout.Button("-", GUILayout.Width(25))) pos.z -= 0.1f;
            if (GUILayout.Button("+", GUILayout.Width(25))) pos.z += 0.1f;
            GUILayout.EndHorizontal();

            customCastlePositions[idx] = pos;

            if (activeC.visualRoot != null)
            {
                activeC.visualRoot.transform.position = pos;
            }
        }

        GUILayout.Space(12);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(curLang == 0 ? "💾 Сохранить в реестр" : "💾 Save Coordinates", GUILayout.Height(32)))
        {
            PlayerPrefs.SetFloat("Castle_PosX_" + idx, customCastlePositions[idx].x);
            PlayerPrefs.SetFloat("Castle_PosY_" + idx, customCastlePositions[idx].y);
            PlayerPrefs.SetFloat("Castle_PosZ_" + idx, customCastlePositions[idx].z);
            PlayerPrefs.Save();
            ShowFeedback(curLang == 0 ? "Координаты успешно сохранены!" : "Coordinates saved successfully!");
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }

        if (GUILayout.Button(curLang == 0 ? "🔄 Сброс" : "🔄 Reset", GUILayout.Height(32)))
        {
            Vector3 defaultPos = Vector3.zero;
            if (idx == 3) defaultPos = new Vector3(-5.3f, -0.4f, 4.2f);
            else if (idx == 6) defaultPos = new Vector3(14.8f, 1.2f, 12.5f);
            else if (idx == 8) defaultPos = new Vector3(-12.4f, -0.3f, -10.2f);
            else if (idx == 11) defaultPos = new Vector3(9.9f, 0.8f, -4.5f);

            customCastlePositions[idx] = defaultPos;
            if (activeC.visualRoot != null)
            {
                activeC.visualRoot.transform.position = defaultPos;
            }
            PlayerPrefs.DeleteKey("Castle_PosX_" + idx);
            PlayerPrefs.DeleteKey("Castle_PosY_" + idx);
            PlayerPrefs.DeleteKey("Castle_PosZ_" + idx);
            PlayerPrefs.Save();
            ShowFeedback(curLang == 0 ? "Сброшено к значениям по умолчанию!" : "Reset to default positions!");
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(curLang == 0 ? "Закрыть" : "Close", GUILayout.Height(28)))
        {
            showCastleCalibrationPanel = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
    }

    private void TriggerPurchaseConfirmPopup(string itemName, int cost, System.Action action)
    {
        confirmItemName = itemName;
        confirmCost = cost;
        confirmAction = action;
        confirmPopupOpenedTime = Time.realtimeSinceStartup;
        showPurchaseConfirmPopup = true;
        if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
    }

    private void DrawPurchaseConfirmPopup(int curLang)
    {
        float panelWidth = 380f;
        float panelHeight = 180f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.02f, 0.08f, 0.18f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 14;
        windowStyle.fontStyle = FontStyle.Bold;

        string title = curLang == 0 ? "🛒 Подтверждение покупки" : "🛒 Purchase Confirmation";
        if (curLang == 8) title = "🛒 确认购买";
        if (curLang == 7) title = "🛒 구매 확인";

        GUI.Window(110, new Rect(px, py, panelWidth, panelHeight), PurchaseConfirmWindowFunction, title, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void PurchaseConfirmWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        int currentGold = SaveGameSystem.CurrentData != null ? SaveGameSystem.CurrentData.gold : 0;

        GUILayout.BeginVertical();
        GUILayout.Space(12);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.wordWrap = true;
        labelStyle.fontSize = 13;
        labelStyle.normal.textColor = Color.white;

        if (currentGold < confirmCost)
        {
            labelStyle.normal.textColor = Color.red;
            labelStyle.fontStyle = FontStyle.Bold;
            string failText = curLang == 0 
                ? $"❌ Недостаточно золота!\n\nУ вас есть: {currentGold} 💰\nТребуется: {confirmCost} 💰" 
                : $"❌ Not enough gold!\n\nYou have: {currentGold} 💰\nRequired: {confirmCost} 💰";
            GUILayout.Label(failText, labelStyle);
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.85f, 0.2f, 0.2f, 1.0f);
            if (GUILayout.Button(curLang == 0 ? "ОК" : "OK", GUILayout.Height(35)))
            {
                showPurchaseConfirmPopup = false;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            string questionText = curLang == 0 
                ? $"Вы действительно хотите приобрести\n<b>{confirmItemName}</b>\nза {confirmCost} 💰?" 
                : $"Are you sure you want to purchase\n<b>{confirmItemName}</b>\nfor {confirmCost} 💰?";
            GUILayout.Label(questionText, labelStyle);
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);

            // YES Button
            GUI.backgroundColor = new Color(0.12f, 0.72f, 0.42f, 1.0f);
            string yesBtn = curLang == 0 ? "Да" : "Yes";
            bool canConfirm = (Time.realtimeSinceStartup - confirmPopupOpenedTime) >= 0.25f;
            if (!canConfirm)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button($"<b>{yesBtn}</b>", GUILayout.Height(35), GUILayout.Width(130)))
            {
                showPurchaseConfirmPopup = false;
                if (confirmAction != null)
                {
                    confirmAction.Invoke();
                }
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            // NO Button
            GUI.backgroundColor = new Color(0.85f, 0.2f, 0.2f, 1.0f);
            string noBtn = curLang == 0 ? "Нет" : "No";
            if (GUILayout.Button($"<b>{noBtn}</b>", GUILayout.Height(35), GUILayout.Width(130)))
            {
                showPurchaseConfirmPopup = false;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(12);
        GUILayout.EndVertical();
    }

    private void DrawSkillDetailPopup(int curLang)
    {
        float panelWidth = 360f;
        float panelHeight = 220f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.02f, 0.08f, 0.18f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 13;
        windowStyle.fontStyle = FontStyle.Bold;

        string title = curLang == 0 ? "🔮 Описание Навыка" : "🔮 Skill Intelligence Blueprint";
        if (curLang == 8) title = "🔮 技能奥义蓝图";
        if (curLang == 7) title = "🔮 기술 각성 정보";

        GUI.Window(103, new Rect(px, py, panelWidth, panelHeight), SkillDetailWindowFunction, title, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void SkillDetailWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (selectedSkillIcon != null)
        {
            GUILayout.Box(selectedSkillIcon, GUILayout.Width(64), GUILayout.Height(64));
        }
        else
        {
            GUIStyle boxS = new GUIStyle(GUI.skin.box);
            boxS.alignment = TextAnchor.MiddleCenter;
            boxS.fontSize = 24;
            GUILayout.Box("🔮", boxS, GUILayout.Width(64), GUILayout.Height(64));
        }

        GUILayout.Space(10);

        GUILayout.BeginVertical();
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 15;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = Color.cyan;
        GUILayout.Label(selectedSkillName, nameStyle);

        GUIStyle typeStyle = new GUIStyle(GUI.skin.label);
        typeStyle.fontSize = 11;
        typeStyle.normal.textColor = Color.yellow;
        string typeLocalized = selectedSkillType;
        if (selectedSkillType == "Passive") typeLocalized = curLang == 0 ? "Пассивный навык" : "Passive Skill";
        else if (selectedSkillType == "Active") typeLocalized = curLang == 0 ? "Активное умение" : "Active Spell";
        else if (selectedSkillType == "Ultimate") typeLocalized = curLang == 0 ? "Суперудар / Абсолютное умение" : "Ultimate Ability";
        GUILayout.Label(typeLocalized, typeStyle);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.fontSize = 11;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = Color.white;
        GUILayout.Label(selectedSkillDesc, descStyle);

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(curLang == 0 ? "Закрыть" : "Close", GUILayout.Height(28)))
        {
            showSkillDetailPopup = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
    }

    private void DrawTroopDetailPopup(int curLang)
    {
        float panelWidth = 440f;
        float panelHeight = 480f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.03f, 0.05f, 0.15f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 13;
        windowStyle.fontStyle = FontStyle.Bold;

        string title = curLang == 0 ? "🛡️ Подробный Чертеж Войска" : "🛡️ Cohort Intelligence blueprint";
        if (curLang == 8) title = "🛡️ 军团连队战术蓝图";
        if (curLang == 7) title = "🛡️ 군단 연대 전술 설계도";

        GUI.Window(104, new Rect(px, py, panelWidth, panelHeight), TroopDetailWindowFunction, title, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void TroopDetailWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        GUILayout.BeginVertical();

        TroopData td = GetTroopData(selectedTroopId);

        string name = curLang == 0 ? td.nameRU : td.nameEN;
        string desc = curLang == 0 ? td.descRU : td.descEN;

        GUILayout.BeginHorizontal();
        Texture2D av = GetTroopAvatarTexture(selectedTroopId);
        if (av != null)
        {
            GUILayout.Box(av, GUILayout.Width(80), GUILayout.Height(80));
        }
        else
        {
            GUIStyle fallbackBox = new GUIStyle(GUI.skin.box);
            fallbackBox.alignment = TextAnchor.MiddleCenter;
            fallbackBox.fontSize = 32;
            GUILayout.Box("📷", fallbackBox, GUILayout.Width(80), GUILayout.Height(80));
        }

        GUILayout.Space(12);

        GUILayout.BeginVertical();
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = Color.cyan;
        GUILayout.Label(name.ToUpper(), nameStyle);

        GUIStyle tierStyle = new GUIStyle(GUI.skin.label);
        tierStyle.fontSize = 11;
        tierStyle.normal.textColor = Color.yellow;
        GUILayout.Label($"Tier {td.tier} • HP: {td.hp} • ATK: {td.atk} • DEF: {td.def} • SPD: {td.spd}", tierStyle);

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.fontSize = 10;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = Color.gray;
        GUILayout.Label(desc, descStyle);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(12);

        GUILayout.Label(curLang == 0 ? "🔮 ПАССИВНЫЕ НАВЫКИ И ОСОБЕННОСТИ:" : "🔮 PASSIVE TRAITS & FEATURES:", GUI.skin.label);
        if (td.passiveNames != null)
        {
            for (int i = 0; i < td.passiveNames.Length; i++)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                Texture2D pasIcon = GetTroopPassiveSkillIcon(selectedTroopId, i);
                if (pasIcon != null)
                {
                    GUILayout.Label(pasIcon, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUIStyle pasBox = new GUIStyle(GUI.skin.box);
                    pasBox.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("🛡️", pasBox, GUILayout.Width(32), GUILayout.Height(32));
                }
                GUILayout.Space(8);
                GUILayout.BeginVertical();
                GUIStyle sNameStyle = new GUIStyle(GUI.skin.label);
                sNameStyle.fontStyle = FontStyle.Bold;
                sNameStyle.fontSize = 11;
                sNameStyle.normal.textColor = new Color(0.2f, 1.0f, 0.6f);
                GUILayout.Label(td.passiveNames[i], sNameStyle);
                
                GUIStyle sDescStyle = new GUIStyle(GUI.skin.label);
                sDescStyle.fontSize = 9;
                sDescStyle.normal.textColor = Color.white;
                sDescStyle.wordWrap = true;
                
                string pDesc = (td.passiveDesc != null && i < td.passiveDesc.Length) ? td.passiveDesc[i] : "";
                GUILayout.Label(pDesc, sDescStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        GUILayout.Space(8);

        GUILayout.Label(curLang == 0 ? "⚡ АКТИВНЫЕ БОЕВЫЕ СПОСОБНОСТИ:" : "⚡ ACTIVE COMBAT ABILITIES:", GUI.skin.label);
        if (td.activeNames != null)
        {
            for (int i = 0; i < td.activeNames.Length; i++)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                Texture2D actIcon = GetTroopActiveSkillIcon(selectedTroopId);
                if (actIcon != null && i == 0)
                {
                    GUILayout.Label(actIcon, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUIStyle actBox = new GUIStyle(GUI.skin.box);
                    actBox.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("⚡", actBox, GUILayout.Width(32), GUILayout.Height(32));
                }
                GUILayout.Space(8);
                GUILayout.BeginVertical();
                GUIStyle sNameStyle = new GUIStyle(GUI.skin.label);
                sNameStyle.fontStyle = FontStyle.Bold;
                sNameStyle.fontSize = 11;
                sNameStyle.normal.textColor = new Color(1.0f, 0.5f, 0.2f);
                GUILayout.Label(td.activeNames[i], sNameStyle);

                GUIStyle sDescStyle = new GUIStyle(GUI.skin.label);
                sDescStyle.fontSize = 9;
                sDescStyle.normal.textColor = Color.white;
                sDescStyle.wordWrap = true;
                
                string aDesc = (td.activeDesc != null && i < td.activeDesc.Length) ? td.activeDesc[i] : "";
                GUILayout.Label(aDesc, sDescStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(curLang == 0 ? "Закрыть Чертёж" : "Close Blueprint", GUILayout.Height(30)))
        {
            showTroopDetailPopup = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
    }

    private void DrawForgeDetailPopup(int curLang)
    {
        float panelWidth = 420f;
        float panelHeight = 360f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        GUI.backgroundColor = new Color(0.12f, 0.05f, 0.18f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 13;
        windowStyle.fontStyle = FontStyle.Bold;

        string title = curLang == 0 ? "🛠️ Спецификация Кузницы" : "🛠️ Forge Blueprint Specifications";
        if (curLang == 8) title = "🛠️ 皇家铁匠铺装备精炼蓝图";
        if (curLang == 7) title = "🛠️ 제국 대장간 연ма 설계도";

        GUI.Window(105, new Rect(px, py, panelWidth, panelHeight), ForgeDetailWindowFunction, title, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void ForgeDetailWindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        GUILayout.BeginVertical();

        string previewClass = null;
        if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            previewClass = SaveGameSystem.CurrentData.characterClass;
        }

        string itemName = GetItemName(selectedForgeSlotType, selectedForgeTier, curLang, previewClass);
        string itemPrompt = GetItemPrompt(selectedForgeSlotType, selectedForgeTier, "warrior");
        if (previewClass != null)
        {
            itemPrompt = GetItemPrompt(selectedForgeSlotType, selectedForgeTier, previewClass.ToLower());
        }

        GUILayout.Label($"👑 {itemName.ToUpper()} (Tier {selectedForgeTier})", GUI.skin.label);
        GUILayout.Space(8);

        string typeName = GetEmojiForSlot(selectedForgeSlotType);
        GUILayout.Label($" • {GetText9("Тип ячейки:", "Slot Type:", "Schlitztyp:", "Type d'emplacement :", "Tipo de ranura:", "Tipo de Slot:", "装備部位:", "장착 부위:", "装备卡槽:")} {typeName}", GUI.skin.label);
        GUILayout.Label($" • {GetText9("Ранг снаряжения:", "Equipment Tier:", "Ausrüstungsstufe:", "Rang de l'équipement :", "Rango de equipo:", "Nível de Equipamento:", "装備階級:", "장비 등급:", "精炼等阶:")} Tier {selectedForgeTier}", GUI.skin.label);

        GUILayout.Space(8);
        GUILayout.Label(curLang == 0 ? "📝 ТЕКСТОВЫЙ ПРОМПТ ДЛЯ НЕЙРОСЕТИ (Midjourney / Stable Diffusion):" : "📝 AI GENERATION PROMPT (Midjourney / Stable Diffusion):", GUI.skin.label);

        GUIStyle promptBox = new GUIStyle(GUI.skin.textArea);
        promptBox.wordWrap = true;
        promptBox.fontSize = 10;
        promptBox.normal.textColor = Color.yellow;
        GUILayout.TextArea(itemPrompt, promptBox, GUILayout.Height(100));

        GUILayout.Space(8);
        if (GUILayout.Button(curLang == 0 ? "📋 Копировать Промпт в Буфер" : "📋 Copy AI Prompt to Clipboard", GUILayout.Height(32)))
        {
            GUIUtility.systemCopyBuffer = itemPrompt;
            ShowFeedback(curLang == 0 ? "Промпт успешно скопирован в буфер обмена!" : "AI prompt copied to clipboard successfully!");
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
        }

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(curLang == 0 ? "Закрыть Спецификацию" : "Close Blueprint", GUILayout.Height(30)))
        {
            showForgeDetailPopup = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
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

    private int GetSpySuccessChance(int playerCastleLvl, int targetCastleLvl)
    {
        if (playerCastleLvl == 1)
        {
            if (targetCastleLvl == 1) return 80;
            if (targetCastleLvl == 2) return 60;
            if (targetCastleLvl == 3) return 40;
            if (targetCastleLvl == 4) return 20;
            if (targetCastleLvl == 5) return 5;
            return 0; // 6, 7 -> lock
        }
        else if (playerCastleLvl == 2)
        {
            if (targetCastleLvl == 1) return 90;
            if (targetCastleLvl == 2) return 80;
            if (targetCastleLvl == 3) return 60;
            if (targetCastleLvl == 4) return 40;
            if (targetCastleLvl == 5) return 20;
            return 0; // 6, 7 -> lock
        }
        else if (playerCastleLvl == 3)
        {
            if (targetCastleLvl == 1) return 100;
            if (targetCastleLvl == 2) return 90;
            if (targetCastleLvl == 3) return 80;
            if (targetCastleLvl == 4) return 60;
            if (targetCastleLvl == 5) return 40;
            if (targetCastleLvl == 6) return 20;
            return 0; // 7 -> lock
        }
        else if (playerCastleLvl == 4)
        {
            if (targetCastleLvl == 1) return 100;
            if (targetCastleLvl == 2) return 100;
            if (targetCastleLvl == 3) return 90;
            if (targetCastleLvl == 4) return 80;
            if (targetCastleLvl == 5) return 60;
            if (targetCastleLvl == 6) return 40;
            if (targetCastleLvl == 7) return 10;
        }
        else if (playerCastleLvl == 5)
        {
            if (targetCastleLvl == 1) return 100;
            if (targetCastleLvl == 2) return 100;
            if (targetCastleLvl == 3) return 100;
            if (targetCastleLvl == 4) return 90;
            if (targetCastleLvl == 5) return 80;
            if (targetCastleLvl == 6) return 60;
            if (targetCastleLvl == 7) return 20;
        }
        else if (playerCastleLvl == 6)
        {
            if (targetCastleLvl == 1) return 100;
            if (targetCastleLvl == 2) return 100;
            if (targetCastleLvl == 3) return 100;
            if (targetCastleLvl == 4) return 100;
            if (targetCastleLvl == 5) return 90;
            if (targetCastleLvl == 6) return 80;
            if (targetCastleLvl == 7) return 40;
        }
        else if (playerCastleLvl >= 7)
        {
            if (targetCastleLvl == 1) return 100;
            if (targetCastleLvl == 2) return 100;
            if (targetCastleLvl == 3) return 100;
            if (targetCastleLvl == 4) return 100;
            if (targetCastleLvl == 5) return 100;
            if (targetCastleLvl == 6) return 90;
            if (targetCastleLvl == 7) return 60;
        }
        return 0;
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

        int curContinentForCap = PlayerPrefs.GetInt("Fate_Current_Continent", 1);
        int maxLvlLimitHeader = curContinentForCap == 1 ? 3 : 7;
        GUILayout.Label($"{lvlPrefix}: {castle.level} / {maxLvlLimitHeader}", descS);

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
            // Upgrade button logic: limit to Tier 3 on the first continent, Tier 7 on others
            int curContinentForUpgrade = PlayerPrefs.GetInt("Fate_Current_Continent", 1);
            int maxLevelLimit = curContinentForUpgrade == 1 ? 3 : 7;
            if (castle.level < maxLevelLimit)
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
                    int targetCost = cost;
                    int targetLvl = nextLvl;
                    CastleInstance targetCastle = castle;
                    string targetName = curLang == 0 ? $"Улучшение Замка до Уровня {targetLvl}" : $"Upgrade Castle to Level {targetLvl}";
                    
                    confirmItemName = targetName;
                    confirmCost = targetCost;
                    confirmAction = () => {
                        SaveGameSystem.CurrentData.gold -= targetCost;
                        targetCastle.level++;
                        PlayerPrefs.SetInt("Castle_Level_" + activeDetailsIndex, targetCastle.level);
                        PlayerPrefs.Save();

                        SpawnAllCastles();

                        string okMsg = curLang == 0 ? "Цитадель расширена!" : "Citadel expanded!";
                        ShowFeedback(okMsg);
                        SaveGameSystem.Save(0);
                    };
                    confirmPopupOpenedTime = Time.realtimeSinceStartup;
                    showPurchaseConfirmPopup = true;
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
            }
            else
            {
                // Max level reached
                GUIStyle maxS = new GUIStyle(GUI.skin.label);
                maxS.normal.textColor = Color.yellow;
                maxS.alignment = TextAnchor.MiddleCenter;
                maxS.fontSize = 13;
                string maxTxt = curLang == 0 ?
                    "⭐ ЦИТАДЕЛЬ ДОСТИГЛА МАКС. УРОВНЯ ДЛЯ ДАННОГО КОНТИНЕНТА ⭐" :
                    "⭐ CITADEL ATTAINED MAXIMUM LEVEL CAP FOR THIS WORLD ⭐";
                if (curLang == 8) maxTxt = "⭐ 领地已达到当前世界最大等级上限 ⭐";
                if (curLang == 7) maxTxt = "⭐ 현재 영지가 도달할 수 있는 최대 등급입니다 ⭐";
                GUILayout.Label(maxTxt, maxS);
            }

            GUILayout.Space(12);

            // Button to open Town Interior
            string interiorBtnTxt = curLang == 0 ? "🏛️ ВОЙТИ В ГОРОД" : "🏛️ ENTER CITADEL INTERIOR";
            if (curLang == 8) interiorBtnTxt = "🏛️ 进入城内管理";
            if (curLang == 7) interiorBtnTxt = "🏛️ 성안으로 진입";

            GUI.backgroundColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);
            if (GUILayout.Button(interiorBtnTxt, GUILayout.Height(40)))
            {
                isTownViewActive = true;
                isDetailsOpen = false; // Закрываем окно деталей при входе в управление городом
                isGridInitialized = false;
                currentTownSubPanel = 0;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            // Enemy Castle
            GUIStyle intelBox = new GUIStyle(GUI.skin.box);
            intelBox.normal.textColor = Color.yellow;
            GUILayout.BeginVertical(intelBox);
            
            string spyTitle = curLang == 0 ? "🕵️ ШПИОНАЖ И РАЗВЕДКА" : "🕵️ ESPIONAGE & INTEL";
            if (curLang == 8) spyTitle = "🕵️ 敌情侦查与谍报";
            if (curLang == 7) spyTitle = "🕵️ 정보 획득 및 간첩";
            GUILayout.Label(spyTitle, GUI.skin.label);
            GUILayout.Space(4);

            int playerCastleLvl = 1;
            for (int i = 0; i < castles.Count; i++)
            {
                if (castles[i].owner == "Player")
                {
                    if (castles[i].level > playerCastleLvl)
                        playerCastleLvl = castles[i].level;
                }
            }

            int successChance = GetSpySuccessChance(playerCastleLvl, castle.level);
            int spyCost = castle.level * 100;

            if (successChance > 0)
            {
                string spyBtnText = "";
                if (curLang == 0) spyBtnText = $"Заслать лазутчика ({spyCost} 💰) | Шанс: {successChance}%";
                else if (curLang == 8) spyBtnText = $"派遣细作探子 ({spyCost} 💰) | 成功率: {successChance}%";
                else if (curLang == 7) spyBtnText = $"간첩 잠입시키기 ({spyCost} 💰) | 성공률: {successChance}%";
                else spyBtnText = $"Infiltrate Spy ({spyCost} 💰) | Chance: {successChance}%";

                if (GUILayout.Button(spyBtnText, GUILayout.Height(30)))
                {
                    if (SaveGameSystem.CurrentData.gold < spyCost)
                    {
                        ShowFeedback(curLang == 0 ? "Недостаточно золота в казне!" : "Not enough gold in treasury!");
                    }
                    else
                    {
                        SaveGameSystem.CurrentData.gold -= spyCost;
                        int roll = UnityEngine.Random.Range(1, 101);
                        if (roll <= successChance)
                        {
                            PlayerPrefs.SetInt("Castle_Spied_" + castle.zoneIndex, 1);
                            PlayerPrefs.SetInt("Castle_Spied_Lvl_" + castle.zoneIndex, playerCastleLvl);
                            PlayerPrefs.Save();
                            ShowFeedback(curLang == 0 ? "Шпион успешно проник в замок и доложил обстановку!" : "Spy successfully infiltrated the garrison!");
                            if (SettingsManager.Instance != null)
                            {
                                SettingsManager.Instance.PlayHoverSound(0);
                            }
                        }
                        else
                        {
                            ShowFeedback(curLang == 0 ? "Шпион был обнаружен и казнен стражей!" : "The spy was detected and executed by the guards!");
                        }
                    }
                }
            }
            else
            {
                GUIStyle lockS = new GUIStyle(GUI.skin.label);
                lockS.normal.textColor = Color.gray;
                string lockTxt = curLang == 0 ?
                    $"🔒 Шпионаж недоступен (для разведки этого замка нужен выше уровень Цитадели)" :
                    $"🔒 Espionage locked (higher player Citadel level required for this target)";
                if (curLang == 8) lockTxt = $"🔒 探子未解锁 (需要更高的主城等级来侦查该领地)";
                if (curLang == 7) lockTxt = $"🔒 정보 정찰 잠금 (이 성채를 정찰하려면 더 높은 요새 레벨이 필요합니다)";
                GUILayout.Label(lockTxt, lockS);
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

        GUILayout.EndVertical();

        GUILayout.Space(12);
        GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f, 1.0f);
        string closeBtnTxt = curLang == 0 ? "✖ ВЫХОД ИЗ ЗАМКА" : "✖ EXIT CASTLE";
        if (curLang == 8) closeBtnTxt = "✖ 退出城堡";
        if (curLang == 7) closeBtnTxt = "✖ 성채 퇴성";
        if (GUILayout.Button(closeBtnTxt, GUILayout.Height(36)))
        {
            isDetailsOpen = false;
            clickCooldown = 0.25f;
            GUIUtility.ExitGUI();
        }
        GUI.backgroundColor = Color.white;

        // Если открыто какое-либо модальное окно поверх, накладываем затемняющий слой на всю область окна деталей (v18.11.24)
        bool modalActive = showCastleCalibrationPanel || showSkillDetailPopup || showTroopDetailPopup || showForgeDetailPopup || showSpyReportPopup || showPurchaseConfirmPopup;
        if (modalActive)
        {
            float currentHeight = (castle.owner == "Player") ? 550f : 620f;
            GUI.backgroundColor = new Color(0.01f, 0.02f, 0.05f, 0.90f);
            GUIStyle darkOverlayStyle = new GUIStyle(GUI.skin.box);
            darkOverlayStyle.normal.background = Texture2D.whiteTexture; // Используем белую текстуру, покрашенную через GUI.backgroundColor
            GUI.Box(new Rect(0, 0, 485f, currentHeight), "", darkOverlayStyle);
            GUI.backgroundColor = Color.white;
        }
    }

    private void LoadGridState()
    {
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                gridUnits[r, c] = PlayerPrefs.GetString($"Castle_Grid_Unit_{activeDetailsIndex}_{r}_{c}", "");
            }
        }
        
        bool isEmpty = true;
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                if (!string.IsNullOrEmpty(gridUnits[r, c]))
                {
                    isEmpty = false;
                    break;
                }
            }
        }

        if (isEmpty)
        {
            string[] ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
            int currentR = 0;
            int currentC = 0;

            foreach (var id in ids)
            {
                int count = GetUnitCount(id, activeDetailsIndex);
                for (int i = 0; i < count; i++)
                {
                    if (currentR < 10)
                    {
                        gridUnits[currentR, currentC] = id;
                        currentC++;
                        if (currentC >= 10)
                        {
                            currentC = 0;
                            currentR++;
                        }
                    }
                }
            }
            SaveGridState();
        }
        isGridInitialized = true;
    }

    private void SaveGridState()
    {
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                PlayerPrefs.SetString($"Castle_Grid_Unit_{activeDetailsIndex}_{r}_{c}", gridUnits[r, c]);
            }
        }
        PlayerPrefs.Save();
    }

    private void SyncGridToUnitCounts()
    {
        string[] ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
        foreach (var id in ids)
        {
            SetUnitCount(id, activeDetailsIndex, 0);
        }
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                string id = gridUnits[r, c];
                if (!string.IsNullOrEmpty(id))
                {
                    int currentCount = GetUnitCount(id, activeDetailsIndex);
                    SetUnitCount(id, activeDetailsIndex, currentCount + 1);
                }
            }
        }
    }

    private void AddUnitToGrid(string id)
    {
        LoadGridState();
        bool added = false;
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                if (string.IsNullOrEmpty(gridUnits[r, c]))
                {
                    gridUnits[r, c] = id;
                    added = true;
                    break;
                }
            }
            if (added) break;
        }
        SaveGridState();
    }

    private string GetUnitNameByID(string id, int curLang)
    {
        switch (id)
        {
            case "warrior": return curLang == 0 ? "Боец фракции" : "Faction Warrior";
            case "archer": return curLang == 0 ? "Эльфийский Лучник" : "Elven Archer";
            case "mage": return curLang == 0 ? "Боевой Маг Зенита" : "Zenith Battle Mage";
            case "paladin": return curLang == 0 ? "Паладин Света" : "Holy Paladin";
            case "cavalry": return curLang == 0 ? "Имперская Конница" : "Imperial Cavalry";
            case "cannoneer": return curLang == 0 ? "Осадно-боевой Пушкарь" : "Garrison Cannoneer";
            case "centaur": return curLang == 0 ? "Кентавр Степей" : "Steppe Centaur";
            case "necromancer": return curLang == 0 ? "Некромант Тьмы" : "Shadow Necromancer";
            case "griffin": return curLang == 0 ? "Элитный Королевский Грифон" : "Royal Griffin";
            case "overlord": return curLang == 0 ? "Рыцарь-Властелин" : "Dread Overlord";
            case "hydra": return curLang == 0 ? "Многоголовая Гидра" : "Swamp Hydra";
            case "dragon": return curLang == 0 ? "Легендарный Дракон Пустоты" : "Void Dragon";
            case "mountain_bear": return curLang == 0 ? "Ураганный Медведь Гор" : "Mountain Bear Guard";
            case "wasteland_serpent": return curLang == 0 ? "Гигантская Змея Пустошей" : "Wasteland Serpent";
        }
        return id;
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
        string barracksHeader = GetText("⚔️ КАЗАРМЫ", "⚔️ BARRACKS", "⚔️ 연병장/배럭", "⚔️ 军营/兵营");
        string forgeHeader = GetText("🧪 КУЗНИЦА И ЛАВКА", "🧪 FORGE & POTION SHOP", "🧪 대장간 & 물약 상점", "🧪 铁匠铺与药水商会");
        string academyHeader = GetText("🎓 АКАДЕМИЯ И АРЕНА", "🎓 ACADEMY & ARENA", "🎓 아каде미 & 투기장", "🎓 皇家学院与斗技场");
        string cName = curLang == 0 ? activeCastle.nameRU : activeCastle.nameEN;
        if (curLang == 8) cName = activeCastle.nameCH;
        if (curLang == 7) cName = activeCastle.nameKR;

        string lvlWord = GetText("УРОВЕНЬ", "LEVEL", "레벨", "等级").ToUpper();
        GUILayout.Label($"🏯 {cName.ToUpper()} ({lvlWord} {activeCastle.level})", titleSt);

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

        // --- WRAP ALL ACTIVE CONTENT IN A SCROLL VIEW TO PROTECT EXIT BUTTON AT THE BOTTOM ---
        townScrollPos = GUILayout.BeginScrollView(townScrollPos, GUILayout.Width(wWidth - 10), GUILayout.Height(wHeight - 165));

        // Render sections inside selected layout mode
        if (currentTownSubPanel == 0)
        {
            // --- ГЛАВНЫЙ ОБЗОР ГОРОДА (3 КРАСИВЫХ ИНТЕРАКТИВНЫХ ВЫБОРА) ---
            GUILayout.BeginHorizontal();

            float colWidth = wWidth / 3.12f;
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.padding = new RectOffset(16, 16, 16, 16);

            // --- КАЗАРМЫ ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth));
            
            GUIStyle colTitle = new GUIStyle(GUI.skin.label);
            colTitle.alignment = TextAnchor.MiddleCenter;
            colTitle.fontSize = 18;
            colTitle.fontStyle = FontStyle.Bold;
            colTitle.normal.textColor = new Color(0.2f, 1.0f, 0.6f);
            
            GUILayout.Label(barracksHeader, colTitle);
            string barracksDesc = GetText9("Найм когорт легиона и войск", "Recruit legion cohorts and troops", "Legionenrekrutierung und Truppen", "Recrutement de cohortes de la légion", "Reclutamiento de cohortes y tropas", "Recrutamento de coortes e tropas", "レギオン歩兵兵団と軍隊の雇用", "레기온 보병 및 병사 모집", "征募军团步兵与军队部众");
            GUILayout.Label(barracksDesc, subSt);
            
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
            GUILayout.Label(barracksArt, artStyle, GUILayout.Height(95));
            
            GUILayout.FlexibleSpace();
            
            GUIStyle enterBtnStyle = new GUIStyle(GUI.skin.button);
            enterBtnStyle.fontSize = 13;
            enterBtnStyle.fontStyle = FontStyle.Bold;
            enterBtnStyle.normal.textColor = Color.white;
            
            GUI.backgroundColor = new Color(0.12f, 0.72f, 0.42f);
            string barracksBtn = GetText9("ВОЙТИ В КАЗАРМЫ", "ENTER BARRACKS", "KASERNE BETRETEN", "ENTRER DANS LES CASERNES", "ENTRAR A LOS CUARTELES", "ENTRAR NO QUARTEL", "兵舎に入る", "연병장/배럭 입장", "进入军营");
            if (GUILayout.Button(barracksBtn, enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 1;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            // --- КУЗНИЦА И ЛАВКА ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth));
            
            GUIStyle colTitle2 = new GUIStyle(GUI.skin.label);
            colTitle2.alignment = TextAnchor.MiddleCenter;
            colTitle2.fontSize = 18;
            colTitle2.fontStyle = FontStyle.Bold;
            colTitle2.normal.textColor = new Color(1.0f, 0.7f, 0.15f);
            
            GUILayout.Label(forgeHeader, colTitle2);
            string forgeDesc = GetText9("Торговля, снаряжение и зелья", "Elixirs & blacksmith forging", "Elixiere & Schmiede", "Élixirs et forge", "Elixires y forja", "Elixires e forja", "エリкサーとブラック smith 鍛造", "엘릭서 및 대장간 제작", "炼制药水与铁匠铺锻造");
            GUILayout.Label(forgeDesc, subSt);
            
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
            GUILayout.Label(forgeArt, artStyle2, GUILayout.Height(95));
            
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.88f, 0.58f, 0.12f);
            string forgeBtn = GetText9("ОТКРЫТЬ КУЗНИЦУ", "OPEN FORGE & SHOP", "SCHMIEDE ÖFFNEN", "OUVRIR LA FORGE", "ABRIR FORJA", "ABRIR FORJA", "鍛冶屋を開く", "대장간 및 상점 열기", "开启铁匠铺与商会");
            if (GUILayout.Button(forgeBtn, enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 2;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            // --- АКАДЕМИЯ И АРЕНА ---
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(colWidth));
            
            GUIStyle colTitle3 = new GUIStyle(GUI.skin.label);
            colTitle3.alignment = TextAnchor.MiddleCenter;
            colTitle3.fontSize = 18;
            colTitle3.fontStyle = FontStyle.Bold;
            colTitle3.normal.textColor = new Color(0.85f, 0.45f, 0.95f);
            
            GUILayout.Label(academyHeader, colTitle3);
            string academyDesc = GetText9("Прокачка героев и ранги армии", "Workout drills & army promotion", "Helden-Training & Armeerang-Upgrade", "Entraînement des héros et rang d'armée", "Entrenamiento de héroes y rango del ejército", "Treino de heróis e ranques de exército", "ヒーロー育成と軍隊階級昇格", "영웅 단련 및 군대 계급 승급", "英雄试炼与军队阶级晋升");
            GUILayout.Label(academyDesc, subSt);
            
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
            GUILayout.Label(academyArt, artStyle3, GUILayout.Height(95));
            
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.68f, 0.28f, 0.85f);
            string academyBtn = GetText9("ВОЙТИ В АКАДЕМИЮ", "ENTER ACADEMY", "AKADEMIE BETRETEN", "ENTRER DANS L'ACADÉMIE", "ENTRAR A LA ACADEMIA", "ENTRAR NA ACADEMIA", "学院に入る", "연병장/배럭 입장", "进入学院");
            if (GUILayout.Button(academyBtn, enterBtnStyle, GUILayout.Height(45)))
            {
                currentTownSubPanel = 3;
                feedbackMessage = "";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            DrawTacticalGarrisonGrid(curLang);
        }
        else
        {
            // If currentTownSubPanel != 0 (user has entered Barracks, Forge, or Academy)
            float colWidth = wWidth - 10;
            if (currentTownSubPanel == 1)
            {
                DrawUnifiedBarracksSection(activeCastle, curLang, colWidth, subSt);
            }
            else if (currentTownSubPanel == 2)
            {
                DrawUnifiedForgeSection(activeCastle, curLang, colWidth, subSt);
            }
            else if (currentTownSubPanel == 3)
            {
                DrawUnifiedAcademySection(activeCastle, curLang, colWidth, subSt);
            }
        }

        GUILayout.EndScrollView();

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
            isDetailsOpen = false; // Закрываем детали замка при выходе на карту
            activeDetailsIndex = -1; // Сбрасываем выбранный замок
            currentTownSubPanel = 0; // Сверхнадежно сбрасываем при выходе в обзор города
            clickCooldown = 0.25f;
            GUIUtility.ExitGUI();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUILayout.EndArea();
    }

    private void DrawTacticalGarrisonGrid(int curLang)
    {
        if (!isGridInitialized)
        {
            LoadGridState();
        }

        Event evt = Event.current;

        GUILayout.Space(12);
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.normal.textColor = Color.yellow;
        string titleText = curLang == 0 ? "🛡️ ТАКТИЧЕСКИЙ ГАРНИЗОН И ПОСТРОЕНИЕ ВОЙСК" : "🛡️ TACTICAL GARRISON & ARMY FORMATION";
        GUILayout.Label(titleText, headerStyle);

        string subtitleText = GetText9(
            "Кликайте по отрядам для перестановки. Наведите на ячейку, чтобы прочитать характеристики.",
            "Click-select to rearrange troops. Hover over a slot to read details and stats.",
            "Klicken Sie auf Truppen, um sie neu anzuordnen. Bewegen Sie den Mauszeiger über ein Feld, um Details zu lesen.",
            "Cliquez pour réorganiser les troupes. Survolez un emplacement pour lire les détails.",
            "Haga clic para reorganizar las tropas. Pase el cursor sobre una casilla para ver los detalles.",
            "Clique para reorganizar as tropas. Passe o mouse sobre um slot para ver os detalhes.",
            "部隊を配置換えするにはクリックしてください。スロットにホバーすると詳細が表示されます。",
            "부대를 재배치하려면 클릭하십시오. 슬롯에 마우스를 올리면 세부 정보를 볼 수 있습니다.",
            "点击队伍以重新排列。将鼠标悬停在卡槽上可查看详细信息。"
        );
        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 12;
        subStyle.normal.textColor = Color.gray;
        GUILayout.Label(subtitleText, subStyle);
        GUILayout.Space(8);

        GUILayout.BeginVertical(GUI.skin.box);
        float slotSize = 44f;
        GUIStyle slotStyle = new GUIStyle(GUI.skin.button);
        slotStyle.padding = new RectOffset(1, 1, 1, 1);

        for (int r = 0; r < 10; r++)
        {
            GUILayout.BeginHorizontal();

            // --- HERO SLOT FOR THIS ROW ---
            string heroKey = "";
            string heroName = "";
            Texture2D heroIcon = null;

            if (r == 0)
            {
                heroKey = "MainHero";
                heroName = curLang == 0 ? "Главный Герой" : "Main Hero";
                string pClassRaw = (SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.characterClass != null) 
                    ? SaveGameSystem.CurrentData.characterClass.ToLower() : "warrior";
                string pClass = "Mage";
                if (pClassRaw.Contains("warrior") || pClassRaw.Contains("воин") || pClassRaw.Contains("voin") || pClassRaw.Contains("paladin") || pClassRaw.Contains("паладин"))
                    pClass = "Warrior";
                else if (pClassRaw.Contains("archer") || pClassRaw.Contains("стрелок") || pClassRaw.Contains("strelok") || pClassRaw.Contains("лучник") || pClassRaw.Contains("ranger"))
                    pClass = "Archer";

                Texture2D wTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.warriorPortrait != null) ? DialogueSystem_Manager.Instance.warriorPortrait.texture : avatar_hero_warrior;
                Texture2D aTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.archerPortrait != null) ? DialogueSystem_Manager.Instance.archerPortrait.texture : avatar_hero_archer;
                Texture2D mTex = (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.magePortrait != null) ? DialogueSystem_Manager.Instance.magePortrait.texture : avatar_hero_mage;
                heroIcon = (pClass == "Warrior") ? wTex : ((pClass == "Archer") ? aTex : mTex);
            }
            else if (r == 1 && GetHeroCount("WarriorHero", activeDetailsIndex) > 0)
            {
                heroKey = "WarriorHero";
                heroName = curLang == 0 ? "Железный Воин" : "Iron Warrior";
                heroIcon = GetTroopAvatarTexture("WarriorHero");
            }
            else if (r == 2 && GetHeroCount("ArcherHero", activeDetailsIndex) > 0)
            {
                heroKey = "ArcherHero";
                heroName = curLang == 0 ? "Стрелок-Следопыт" : "Marksman Hero";
                heroIcon = GetTroopAvatarTexture("ArcherHero");
            }
            else if (r == 3 && GetHeroCount("MageHero", activeDetailsIndex) > 0)
            {
                heroKey = "MageHero";
                heroName = curLang == 0 ? "Боевой Маг" : "Sorcerer Elite";
                heroIcon = GetTroopAvatarTexture("MageHero");
            }

            Rect heroRect = GUILayoutUtility.GetRect(slotSize + 10, slotSize);
            if (!string.IsNullOrEmpty(heroKey))
            {
                if (heroKey == "MainHero")
                {
                    GUI.backgroundColor = new Color(0.15f, 0.85f, 1.0f, 1.0f); // Bright Royal Cyan for Main Hero
                }
                else
                {
                    GUI.backgroundColor = new Color(0.85f, 0.70f, 0.10f, 1.0f); // Golden Orange for Comrades
                }
                GUI.Box(heroRect, "", slotStyle);
                GUI.backgroundColor = Color.white;

                if (heroIcon != null)
                {
                    GUI.DrawTexture(heroRect, heroIcon, ScaleMode.ScaleToFit);
                }

                if (heroRect.Contains(evt.mousePosition))
                {
                    if (evt.type == EventType.Repaint)
                    {
                        SetHoveredGridUnit(heroKey, r, curLang);
                    }
                    if (evt.type == EventType.MouseDown)
                    {
                        if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                        ShowFeedback(curLang == 0 ? $"Выбран герой: {heroName}" : $"Selected hero: {heroName}");
                        evt.Use();
                    }
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
                GUI.Box(heroRect, "Ø", slotStyle);
                GUI.backgroundColor = Color.white;
            }

            // Space between Hero and Troops
            GUILayout.Space(14);

            // --- 10 TROOP COHORT SLOTS ---
            for (int c = 0; c < 10; c++)
            {
                string unitId = gridUnits[r, c];
                bool isSelected = (r == selectedGridRow && c == selectedGridCol);

                Rect cellRect = GUILayoutUtility.GetRect(slotSize, slotSize);

                if (isSelected)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                else if (!string.IsNullOrEmpty(unitId))
                {
                    GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
                }

                GUI.Box(cellRect, "", slotStyle);
                GUI.backgroundColor = Color.white;

                Texture2D troopAv = !string.IsNullOrEmpty(unitId) ? GetTroopAvatarTexture(unitId) : null;
                if (troopAv != null)
                {
                    GUI.DrawTexture(cellRect, troopAv, ScaleMode.ScaleToFit);
                }
                else if (string.IsNullOrEmpty(unitId))
                {
                    GUIStyle symStyle = new GUIStyle(GUI.skin.label);
                    symStyle.alignment = TextAnchor.MiddleCenter;
                    symStyle.fontSize = 11;
                    symStyle.normal.textColor = Color.gray;
                    GUI.Label(cellRect, "Ø", symStyle);
                }

                // Handle interactions
                if (cellRect.Contains(evt.mousePosition))
                {
                    if (!string.IsNullOrEmpty(unitId))
                    {
                        if (evt.type == EventType.Repaint)
                        {
                            SetHoveredGridUnit(unitId, r, curLang);
                        }

                        if (evt.type == EventType.MouseDown)
                        {
                            isDraggingUnit = true;
                            dragSourceRow = r;
                            dragSourceCol = c;
                            dragSourceUnitId = unitId;
                            evt.Use();
                        }
                    }

                    if (evt.type == EventType.MouseUp)
                    {
                        if (isDraggingUnit)
                        {
                            // Drop!
                            int targetRow = r;
                            int targetCol = c;

                            if (dragSourceCol == -1)
                            {
                                ShowFeedback(curLang == 0 ? "Нельзя ставить героев в слоты войск!" : "Cannot place heroes in troop slots!");
                            }
                            else
                            {
                                // Swap
                                string temp = gridUnits[targetRow, targetCol];
                                gridUnits[targetRow, targetCol] = gridUnits[dragSourceRow, dragSourceCol];
                                gridUnits[dragSourceRow, dragSourceCol] = temp;

                                SaveGridState();
                                SyncGridToUnitCounts();
                                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                                ShowFeedback(curLang == 0 ? "Отряды переставлены!" : "Cohorts rearranged!");
                            }

                            isDraggingUnit = false;
                            dragSourceRow = -1;
                            dragSourceCol = -1;
                            dragSourceUnitId = "";
                            evt.Use();
                        }
                        else
                        {
                            // Simple click select & swap
                            if (!string.IsNullOrEmpty(unitId))
                            {
                                if (selectedGridRow == -1 && selectedGridCol == -1)
                                {
                                    selectedGridRow = r;
                                    selectedGridCol = c;
                                }
                                else if (selectedGridRow == r && selectedGridCol == c)
                                {
                                    selectedGridRow = -1;
                                    selectedGridCol = -1;
                                }
                                else
                                {
                                    string temp = gridUnits[selectedGridRow, selectedGridCol];
                                    gridUnits[selectedGridRow, selectedGridCol] = gridUnits[r, c];
                                    gridUnits[r, c] = temp;

                                    selectedGridRow = -1;
                                    selectedGridCol = -1;
                                    SaveGridState();
                                    SyncGridToUnitCounts();
                                }
                                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                                evt.Use();
                            }
                            else
                            {
                                // Click empty slot
                                if (selectedGridRow != -1 && selectedGridCol != -1)
                                {
                                    gridUnits[r, c] = gridUnits[selectedGridRow, selectedGridCol];
                                    gridUnits[selectedGridRow, selectedGridCol] = "";

                                    selectedGridRow = -1;
                                    selectedGridCol = -1;
                                    SaveGridState();
                                    SyncGridToUnitCounts();
                                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                                    evt.Use();
                                }
                            }
                        }
                    }
                }

                if (c < 9) GUILayout.Space(4);
            }

            GUILayout.EndHorizontal();
            if (r < 9) GUILayout.Space(4);
        }

        // Cancel dragging if mouse released elsewhere
        if (evt.type == EventType.MouseUp && isDraggingUnit)
        {
            isDraggingUnit = false;
            dragSourceRow = -1;
            dragSourceCol = -1;
            dragSourceUnitId = "";
        }

        GUILayout.EndVertical();

        // Dismiss button for selected unit
        if (selectedGridRow != -1 && selectedGridCol != -1)
        {
            string selUnitId = gridUnits[selectedGridRow, selectedGridCol];
            if (!string.IsNullOrEmpty(selUnitId))
            {
                string selName = GetUnitNameByID(selUnitId, curLang);
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
                string dismissLabel = curLang == 0 ? $"✖ Распустить выбранный отряд ({selName})" : $"✖ Dismiss Selected Cohort ({selName})";
                if (GUILayout.Button(dismissLabel, GUILayout.Height(30), GUILayout.Width(350)))
                {
                    gridUnits[selectedGridRow, selectedGridCol] = "";
                    selectedGridRow = -1;
                    selectedGridCol = -1;
                    SaveGridState();
                    SyncGridToUnitCounts();
                    ShowFeedback(curLang == 0 ? "Отряд был успешно распущен!" : "Cohort has been dismissed!");
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        // Draw floating drag icon
        if (isDraggingUnit && !string.IsNullOrEmpty(dragSourceUnitId))
        {
            Texture2D dragIcon = GetTroopAvatarTexture(dragSourceUnitId);
            if (dragIcon != null)
            {
                Rect dragRect = new Rect(evt.mousePosition.x - 24, evt.mousePosition.y - 24, 48, 48);
                GUI.DrawTexture(dragRect, dragIcon);
            }
        }
    }

    private void SetHoveredGridUnit(string id, int r, int curLang)
    {
        if (id == "MainHero")
        {
            if (SaveGameSystem.CurrentData == null) return;
            int pLvl = SaveGameSystem.CurrentData.playerLevel;
            string pClassRaw = SaveGameSystem.CurrentData.characterClass;
            string pClass = "Mage";
            string pClassLower = (pClassRaw != null) ? pClassRaw.ToLower() : "";
            if (pClassLower.Contains("warrior") || pClassLower.Contains("воин") || pClassLower.Contains("voin") || pClassLower.Contains("paladin") || pClassLower.Contains("паладин"))
                pClass = "Warrior";
            else if (pClassLower.Contains("archer") || pClassLower.Contains("стрелок") || pClassLower.Contains("strelok") || pClassLower.Contains("лучник") || pClassLower.Contains("ranger"))
                pClass = "Archer";

            float currentHealth = SaveGameSystem.CurrentData.currentHealth;
            float maxHealth = SaveGameSystem.CurrentData.maxHealth;
            int str = SaveGameSystem.CurrentData.strength;
            int agi = SaveGameSystem.CurrentData.agility;
            int intel = SaveGameSystem.CurrentData.intelligence;
            int sta = SaveGameSystem.CurrentData.stamina;

            isHoveringSkill = true;
            hoveredSkillName = curLang == 0 ? $"Основной Герой: {pClass}" : $"Main Hero: {pClass}";
            hoveredSkillType = curLang == 0 ? "👑 КОМАНДИР" : "👑 COMMANDER";
            hoveredSkillDesc = curLang == 0 ?
                $"Уровень: {pLvl}\nЗдоровье: {currentHealth}/{maxHealth}\nСила: {str}\nЛовкость: {agi}\nИнтеллект: {intel}\nСтойкость: {sta}\n\n★ Лидер вашего воинства." :
                $"Level: {pLvl}\nHP: {currentHealth}/{maxHealth}\nStrength: {str}\nAgility: {agi}\nIntelligence: {intel}\nStamina: {sta}\n\n★ Leader of your legion.";
            hoveredSkillIcon = GetTroopAvatarTexture("MainHero");
        }
        else if (id == "WarriorHero" || id == "ArcherHero" || id == "MageHero")
        {
            int lvl = PlayerPrefs.GetInt("Companion_Lvl_" + id, 1);
            int xp = PlayerPrefs.GetInt("Companion_XP_" + id, 0);
            string name = GetUnitNameByID(id, curLang);

            isHoveringSkill = true;
            hoveredSkillName = name;
            hoveredSkillType = curLang == 0 ? "🕵️ ГЕРОЙ-СОЮЗНИК" : "🕵️ ALLIED HERO";
            hoveredSkillDesc = curLang == 0 ?
                $"Ранг: {lvl} (XP: {xp}/1000)\n\n★ Специализированный герой, нанятый для поддержки авангарда цитадели." :
                $"Rank: {lvl} (XP: {xp}/1000)\n\n★ Specialized hero recruited to support the citadel vanguard.";
            hoveredSkillIcon = GetTroopAvatarTexture(id);
        }
        else
        {
            // Regular troop
            int troopLvl = GetTroopLevel(id, activeDetailsIndex);
            int troopXp = GetTroopXP(id, activeDetailsIndex);
            string name = GetUnitNameByID(id, curLang);

            isHoveringSkill = true;
            hoveredSkillName = name;
            hoveredSkillType = curLang == 0 ? "⚔️ РЕГУЛЯРНЫЙ ОТРЯД" : "⚔️ REGULAR COHORT";
            hoveredSkillDesc = curLang == 0 ?
                $"Ранг: {troopLvl} (XP: {troopXp}/1000)\n\n★ Боевая когорта, охраняющая этот замок и участвующая в походах." :
                $"Rank: {troopLvl} (XP: {troopXp}/1000)\n\n★ Combat cohort defending this castle and participating in crusades.";
            hoveredSkillIcon = GetTroopAvatarTexture(id);
        }
    }

    private Vector2 forgeScrollPos = Vector2.zero;

    private void DrawUnitItem(string id, string nameRU, string nameEN, string nameCH, string nameKR, int price, int requiredLvl, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int count = GetUnitCount(id, activeDetailsIndex);
        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        TroopData td = GetTroopData(id);

        GUILayout.BeginHorizontal(GUI.skin.box);
        
        // ================= COLUMN 1: PORTRAIT, NAME & CHARACTERISTICS =================
        GUILayout.BeginVertical(GUILayout.Width(220));
        
        GUILayout.BeginHorizontal();
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

        GUILayout.Space(6);

        GUIStyle itemBtnStyle = new GUIStyle(GUI.skin.label);
        itemBtnStyle.fontStyle = FontStyle.Bold;
        itemBtnStyle.fontSize = 12;
        itemBtnStyle.normal.textColor = Color.yellow;
        itemBtnStyle.wordWrap = true;
        
        string btnLabel = $"{name}\n(Ур.{requiredLvl}+) | [{count} шт]";
        GUILayout.Label(btnLabel, itemBtnStyle, GUILayout.Height(44));
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Stats rows in compact high-density layout
        GUIStyle statLabelStyle = new GUIStyle(GUI.skin.label);
        statLabelStyle.fontSize = 11;
        statLabelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        
        string hpLabel = GetText("❤️ ОЗ", "❤️ HP", "❤️ HP", "❤️ 生命值");
        string atkLabel = GetText("⚔️ АТК", "⚔️ ATK", "⚔️ ATK", "⚔️ 攻击力");
        string defLabel = GetText("🛡️ ЗАЩ", "🛡️ DEF", "🛡️ DEF", "🛡️ 防御力");
        string spdLabel = GetText("⚡ СКОР", "⚡ SPD", "⚡ SPD", "⚡ 速度");
        
        GUILayout.Label($"{hpLabel}: {td.hp}  |  {atkLabel}: {td.atk}", statLabelStyle);
        GUILayout.Label($"{defLabel}: {td.def}  |  {spdLabel}: {td.spd}", statLabelStyle);

        GUILayout.EndVertical();

        GUILayout.Space(12);

        // ================= COLUMN 2: ACTIVE SKILLS ("🔥") =================
        GUILayout.BeginVertical(GUILayout.Width(220));
        GUIStyle skillHeaderStyle = new GUIStyle(GUI.skin.label);
        skillHeaderStyle.fontStyle = FontStyle.Bold;
        skillHeaderStyle.fontSize = 11;
        skillHeaderStyle.normal.textColor = new Color(1f, 0.5f, 0.3f); // Light orange/red for actives

        string actHeader = GetText("🔥 АКТИВНЫЕ НАВЫКИ", "🔥 ACTIVE SKILLS", "🔥 액티브 스킬", "🔥 主动技能");
        GUILayout.Label(actHeader, skillHeaderStyle);
        GUILayout.Space(4);

        GUIStyle skillDescStyle = new GUIStyle(GUI.skin.label);
        skillDescStyle.fontSize = 10;
        skillDescStyle.wordWrap = true;
        skillDescStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        string firstActiveName = (td.activeNames != null && td.activeNames.Length > 0) ? td.activeNames[0] : "";
        string firstActiveDesc = (td.activeDesc != null && td.activeDesc.Length > 0) ? td.activeDesc[0] : "";
        string activePrompt = (td.activePrompts != null && td.activePrompts.Length > 0) ? td.activePrompts[0] : "";

        GUILayout.BeginHorizontal();
        Texture2D activeIconTex = GetTroopActiveSkillIcon(id);
        GUIStyle skillBtnStyle = new GUIStyle(GUI.skin.button);
        skillBtnStyle.padding = new RectOffset(0, 0, 0, 0);

        if (activeIconTex != null)
        {
            GUILayout.Button(activeIconTex, skillBtnStyle, GUILayout.Width(40), GUILayout.Height(40));
        }
        else
        {
            GUILayout.Button("🔥", skillBtnStyle, GUILayout.Width(40), GUILayout.Height(40));
        }

        Rect activeIconRect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && activeIconRect.Contains(Event.current.mousePosition) && !string.IsNullOrEmpty(firstActiveName))
        {
            isHoveringSkill = true;
            hoveredSkillName = firstActiveName;
            
            string fullActiveDesc = firstActiveDesc;
            if (!string.IsNullOrEmpty(activePrompt))
            {
                fullActiveDesc += $"\n\n<color=cyan><i>AI Prompt: {activePrompt}</i></color>";
            }
            
            hoveredSkillDesc = fullActiveDesc;
            hoveredSkillType = GetText("🔥 АКТИВНЫЙ НАВЫК", "🔥 ACTIVE SKILL", "🔥 액티브 스킬", "🔥 主动技能");
            hoveredSkillIcon = activeIconTex;
        }

        GUILayout.Space(6);

        GUILayout.BeginVertical();
        if (!string.IsNullOrEmpty(firstActiveName))
        {
            GUILayout.Label($"<b>{firstActiveName}</b>", skillDescStyle);
            GUILayout.Label(firstActiveDesc, skillDescStyle, GUILayout.Width(164));
        }
        else
        {
            GUILayout.Label("-", skillDescStyle);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.Space(12);

        // ================= COLUMN 3: PASSIVE SKILLS ("❄️") =================
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        GUIStyle passiveHeaderStyle = new GUIStyle(GUI.skin.label);
        passiveHeaderStyle.fontStyle = FontStyle.Bold;
        passiveHeaderStyle.fontSize = 11;
        passiveHeaderStyle.normal.textColor = new Color(0.3f, 0.75f, 1f); // Sky blue for passives

        string pasHeader = GetText("❄️ ПАССИВНЫЕ НАВЫКИ", "❄️ PASSIVE SKILLS", "❄️ 패시브 С킬", "❄️ 被动技能");
        GUILayout.Label(pasHeader, passiveHeaderStyle);
        GUILayout.Space(6);

        if (td.passiveNames != null && td.passiveNames.Length > 0)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < td.passiveNames.Length; i++)
            {
                Texture2D pasIconTex = GetTroopPassiveSkillIcon(id, i);
                string pasName = td.passiveNames[i];
                string pasDesc = (td.passiveDesc != null && i < td.passiveDesc.Length) ? td.passiveDesc[i] : "";
                string pasPrompt = (td.passivePrompts != null && i < td.passivePrompts.Length) ? td.passivePrompts[i] : "";

                GUILayout.BeginVertical(GUILayout.Width(125));

                if (pasIconTex != null)
                {
                    GUILayout.Button(pasIconTex, skillBtnStyle, GUILayout.Width(40), GUILayout.Height(40));
                }
                else
                {
                    GUILayout.Button("❄️", skillBtnStyle, GUILayout.Width(40), GUILayout.Height(40));
                }

                Rect pasIconRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && pasIconRect.Contains(Event.current.mousePosition))
                {
                    isHoveringSkill = true;
                    hoveredSkillName = pasName;
                    
                    string fullPasDesc = pasDesc;
                    if (!string.IsNullOrEmpty(pasPrompt))
                    {
                        fullPasDesc += $"\n\n<color=cyan><i>AI Prompt: {pasPrompt}</i></color>";
                    }
                    
                    hoveredSkillDesc = fullPasDesc;
                    hoveredSkillType = GetText("❄️ ПАССИВНЫЙ НАВЫК", "❄️ PASSIVE SKILL", "❄️ 패시브 스킬", "❄️ 被动技能");
                    hoveredSkillIcon = pasIconTex;
                }

                GUILayout.Space(4);
                GUILayout.Label($"<b>{pasName}</b>", skillDescStyle);
                GUILayout.Label(pasDesc, skillDescStyle, GUILayout.Width(120));
                GUILayout.EndVertical();

                if (i < td.passiveNames.Length - 1)
                {
                    GUILayout.Space(12);
                }
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("-", skillDescStyle);
        }
        GUILayout.EndVertical();

        // Push the recruit button column to the far right of the item box
        GUILayout.FlexibleSpace();

        // ================= COLUMN 4: COST & RECRUIT BUTTON (FAR RIGHT) =================
        GUILayout.BeginVertical(GUILayout.Width(130));
        GUILayout.Space(12);
        
        CastleInstance activeCastle = castles[activeDetailsIndex >= 0 ? activeDetailsIndex : 0];
        int currentTroops = GetTroopsCountInCastle();
        int troopCapacity = GetTroopCapacity(activeCastle.level);

        if (castleLvl < requiredLvl)
        {
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            string lockLabel = curLang == 0 ? "🔒 Замок LVL " + requiredLvl : "🔒 Build T-" + requiredLvl;
            if (curLang == 8) lockLabel = "🔒 城堡等级 " + requiredLvl;
            if (curLang == 7) lockLabel = "🔒 성 레벨 " + requiredLvl;
            GUILayout.Button(lockLabel, GUILayout.Height(40));
            GUI.backgroundColor = Color.white;
        }
        else if (currentTroops >= troopCapacity)
        {
            GUI.backgroundColor = new Color(0.55f, 0.2f, 0.2f, 1.0f);
            string limitLabel = curLang == 0 ? "Лимит войск!" : "Troop Limit!";
            if (curLang == 8) limitLabel = "兵力上限！";
            if (curLang == 7) limitLabel = "부대 한도!";
            if (GUILayout.Button(limitLabel, GUILayout.Height(40)))
            {
                string failMsg = curLang == 0 ?
                    $"Достигнут лимит войск в этом замке ({currentTroops}/{troopCapacity})! Повысьте уровень цитадели." :
                    $"Castle troop garrison limit reached ({currentTroops}/{troopCapacity})! Upgrade stronghold first.";
                if (curLang == 8) failMsg = $"已达城堡兵力上限 ({currentTroops}/{troopCapacity})！请先升级主城。";
                if (curLang == 7) failMsg = $"성채 군대 한도 초과 ({currentTroops}/{troopCapacity})! 성채를 먼저 업그레이드 하십시오.";
                ShowFeedback(failMsg);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = new Color(0.12f, 0.72f, 0.42f);
            if (GUILayout.Button($"{price} 💰", GUILayout.Height(40)))
            {
                string targetId = id;
                int targetPrice = price;
                string targetName = name;
                int targetCount = count;
                confirmItemName = name;
                confirmCost = price;
                confirmAction = () => {
                    SaveGameSystem.CurrentData.gold -= targetPrice;
                    AddUnitToGrid(targetId);
                    SyncGridToUnitCounts();
                    
                    string buyMsg = curLang == 0 ?
                        $"Отряд {targetName} нанят в гарнизон!" :
                        $"Cohort {targetName} recruited successfully!";
                    ShowFeedback(buyMsg);
                    SaveGameSystem.Save(0);
                };
                confirmPopupOpenedTime = Time.realtimeSinceStartup;
                showPurchaseConfirmPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private Texture2D GetPotionIconById(string id)
    {
        if (id == "hp") return icon_potion_hp;
        if (id == "str") return icon_potion_str;
        if (id == "int") return icon_potion_int;
        if (id == "agi") return icon_potion_agi;
        if (id == "sta" || id == "def") return icon_potion_sta;
        return icon_potion_hp;
    }

    private int GetRequiredCastleLevelForForge(int tier)
    {
        if (tier == 1) return 1;
        if (tier == 2) return 2;
        if (tier == 3) return 3;
        if (tier == 4) return 4;
        if (tier == 5) return 5;
        if (tier == 6) return 6;
        return 1;
    }

    private string GetItemStatDescription(int slotType, int tier, string previewClass)
    {
        int str, agi, intel, sta;
        GetItemStats(slotType, tier, out str, out agi, out intel, out sta, previewClass);
        List<string> stats = new List<string>();
        if (str > 0) stats.Add($"+{str} STR");
        if (agi > 0) stats.Add($"+{agi} AGI");
        if (intel > 0) stats.Add($"+{intel} INT");
        if (sta > 0) stats.Add($"+{sta} STA");
        
        int curLang = Translator.LanguageID;
        string statStr = string.Join(", ", stats);
        if (curLang == 0) return $"Параметры: {statStr} (Ковка)";
        return $"Stats: {statStr} (Crafted)";
    }

    private string GetItemClassRecommendation(int slotType, int tier)
    {
        int curLang = Translator.LanguageID;
        string previewClass = PlayerPrefs.GetString("Forge_Preview_Class", "warrior");
        if (string.IsNullOrEmpty(previewClass)) previewClass = "warrior";
        
        string classLabel = "Воин";
        if (previewClass == "archer") classLabel = "Стрелок";
        else if (previewClass == "mage") classLabel = "Маг";
        
        if (curLang == 0) return $"★ Рекомендуется классу: {classLabel}";
        return $"★ Recommended for class: {previewClass.ToUpper()}";
    }

    private void DrawForgeEquipmentOption(int slotType, int tier, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int reqLvl = GetRequiredCastleLevelForForge(tier);
        bool isUnlocked = castleLvl >= reqLvl;

        int price = Mathf.RoundToInt(50 * tier * (castleLvl * 0.35f + 0.65f));
        string previewClass = PlayerPrefs.GetString("Forge_Preview_Class", "warrior");
        if (string.IsNullOrEmpty(previewClass)) previewClass = "warrior";

        string name = GetItemName(slotType, tier, curLang, previewClass);
        
        InventoryItem tempItem = new InventoryItem();
        tempItem.id = $"item_slot_{slotType}_tier_{tier}";
        tempItem.slotType = slotType;
        tempItem.level = tier;
        Texture2D itemTex = GetItemIconTexture(tempItem, previewClass);

        string emoji = GetEmojiForSlot(slotType);

        string colorTag = "<color=white>";
        if (tier == 6) colorTag = "<color=red>"; // Mythic
        else if (tier == 5) colorTag = "<color=orange>"; // Legendary
        else if (tier == 4) colorTag = "<color=magenta>"; // Epic
        else if (tier == 3) colorTag = "<color=cyan>"; // Rare
        else if (tier == 2) colorTag = "<color=green>"; // Uncommon

        string statDesc = GetItemStatDescription(slotType, tier, previewClass);
        string classRecommend = GetItemClassRecommendation(slotType, tier);

        GUILayout.BeginHorizontal(GUI.skin.box);

        // Icon box (colored background / styled border)
        GUILayout.BeginVertical(GUILayout.Width(58), GUILayout.Height(58));
        GUIStyle iconBtnStyle = new GUIStyle(GUI.skin.button);
        iconBtnStyle.padding = new RectOffset(2, 2, 2, 2);
        iconBtnStyle.fontSize = 24; // Более крупный эмодзи при отсутствии текстуры
        
        if (itemTex != null)
        {
            if (GUILayout.Button(itemTex, iconBtnStyle, GUILayout.Width(54), GUILayout.Height(54)))
            {
                selectedForgeSlotType = slotType;
                selectedForgeTier = tier;
                showForgeDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        else
        {
            if (GUILayout.Button(emoji, iconBtnStyle, GUILayout.Width(54), GUILayout.Height(54)))
            {
                selectedForgeSlotType = slotType;
                selectedForgeTier = tier;
                showForgeDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.EndVertical();

        Rect iconRect = GUILayoutUtility.GetLastRect();

        GUILayout.Space(8);

        // Details
        GUILayout.BeginVertical();
        
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.richText = true;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.fontSize = 12;

        GUILayout.Label($"{colorTag}<b>{name} (Тир {tier})</b></color>", nameStyle);

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.fontSize = 10;
        descStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.Label(statDesc, descStyle);

        // Multi-class Prompt copy buttons and Info button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Сдвигаем кнопки классов вправо по запросу пользователя
        if (GUILayout.Button("⚔️ Warrior", GUILayout.Width(75), GUILayout.Height(18)))
        {
            string p = GetItemPrompt(slotType, tier, "warrior");
            GUIUtility.systemCopyBuffer = p;
            string copyMsg = curLang == 0 ? $"Промпт Воина для {GetItemName(slotType, tier, curLang, "warrior")} скопирован!" : $"Warrior prompt for {GetItemName(slotType, tier, curLang, "warrior")} copied!";
            ShowFeedback(copyMsg);
        }
        GUILayout.Space(2);
        if (GUILayout.Button("🏹 Archer", GUILayout.Width(75), GUILayout.Height(18)))
        {
            string p = GetItemPrompt(slotType, tier, "archer");
            GUIUtility.systemCopyBuffer = p;
            string copyMsg = curLang == 0 ? $"Промпт Стрелка для {GetItemName(slotType, tier, curLang, "archer")} скопирован!" : $"Archer prompt for {GetItemName(slotType, tier, curLang, "archer")} copied!";
            ShowFeedback(copyMsg);
        }
        GUILayout.Space(2);
        if (GUILayout.Button("🔮 Mage", GUILayout.Width(75), GUILayout.Height(18)))
        {
            string p = GetItemPrompt(slotType, tier, "mage");
            GUIUtility.systemCopyBuffer = p;
            string copyMsg = curLang == 0 ? $"Промпт Мага для {GetItemName(slotType, tier, curLang, "mage")} скопирован!" : $"Mage prompt for {GetItemName(slotType, tier, curLang, "mage")} copied!";
            ShowFeedback(copyMsg);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical(); // End details

        GUILayout.Space(12);

        // Craft / Forge button on the right
        if (isUnlocked)
        {
            GUIStyle forgeBtnStyle = new GUIStyle(GUI.skin.button);
            forgeBtnStyle.fontStyle = FontStyle.Bold;
            forgeBtnStyle.fontSize = 11;
            
            bool canAfford = SaveGameSystem.CurrentData != null && SaveGameSystem.CurrentData.gold >= price;
            GUI.backgroundColor = canAfford ? new Color(0.2f, 0.8f, 0.3f, 1f) : new Color(0.8f, 0.2f, 0.2f, 1f);

            string btnText = curLang == 0 ? $"Выковать ({price}🪙)" : $"Forge ({price}🪙)";
            if (curLang == 8) btnText = $"锻造 ({price}🪙)";
            if (curLang == 7) btnText = $"제작 ({price}🪙)";

            if (GUILayout.Button(btnText, forgeBtnStyle, GUILayout.Width(100), GUILayout.Height(35)))
            {
                if (SaveGameSystem.CurrentData == null) return;
                
                if (SaveGameSystem.CurrentData.gold < price)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно золота в казне замка!" : "Insufficient gold for blacksmith services!");
                }
                else
                {
                    string itemId = $"item_slot_{slotType}_tier_{tier}";
                    
                    if (!CanAddInventoryItem(itemId, slotType, tier))
                    {
                        ShowFeedback(curLang == 0 ? "Ваш инвентарь переполнен! Освободите место." : "Inventory is full! Free some slots first.");
                    }
                    else
                    {
                        string itemNameRU = GetItemName(slotType, tier, 0, previewClass);
                        string iconType = GetIconTypeForSlot(slotType);
                        
                        if (AddInventoryItem(itemId, itemNameRU, iconType, slotType, tier, tier * 3))
                        {
                            SaveGameSystem.CurrentData.gold -= price;
                            PlayerPrefs.Save();
                            SaveGameSystem.Save(0);
                            ShowFeedback(curLang == 0 ? $"Выковано и помещено в инвентарь: {itemNameRU}!" : $"Forged and placed in inventory: {name}!");
                        }
                    }
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.enabled = false;
            string lockLabel = curLang == 0 ? "Заперто 🔒" : "Locked 🔒";
            if (curLang == 8) lockLabel = "已锁 🔒";
            if (curLang == 7) lockLabel = "잠김 🔒";
            GUILayout.Button(lockLabel, GUILayout.Width(100), GUILayout.Height(35));
            GUI.enabled = true;
        }

        GUILayout.EndHorizontal();
    }

    private Vector2 academyScrollPos = Vector2.zero;

    private struct TrainingEntity
    {
        public string id;       // "main_hero", "comrade_...", "troop_..."
        public string name;     // localized name
        public string emoji;
        public int level;
        public int xp;
    }

    public bool CheckAndEnforceHeroLimits(CastleInstance castle, bool isMainHero, int futureSimpleCount, bool isComputer = false)
    {
        if (castle.level <= 3)
        {
            int totalHeroes = (isMainHero ? 1 : 0) + futureSimpleCount;
            if (futureSimpleCount > 5 || (isMainHero && futureSimpleCount > 4) || totalHeroes > 5)
            {
                string limitMsg = 
                    "RU: Лимит героев превышен!\n" +
                    "EN: Hero limit exceeded!\n" +
                    "DE: Heldenlimit überschritten!\n" +
                    "FR: Limite de héros dépassée !\n" +
                    "ES: ¡Límite de héroes excedido!\n" +
                    "PT: Limite de heróis excedido!\n" +
                    "JA: ヒーローの制限を超えました！\n" +
                    "KO: 영웅 제한이 초과되었습니다!\n" +
                    "ZH: 英雄数量已达上限！";
                ShowFeedback(limitMsg);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(1);
                return true; // Exceeded!
            }
        }
        return false; // OK
    }

    public int GetTroopLevel(string id, int zoneIndex)
    {
        string key = $"Player_Unit_Lvl_{id}_Zone_{zoneIndex}";
        return PlayerPrefs.GetInt(key, 1);
    }

    public void SetTroopLevel(string id, int zoneIndex, int level)
    {
        string key = $"Player_Unit_Lvl_{id}_Zone_{zoneIndex}";
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }

    public int GetTroopXP(string id, int zoneIndex)
    {
        string key = $"Player_Unit_XP_{id}_Zone_{zoneIndex}";
        return PlayerPrefs.GetInt(key, 0);
    }

    public void SetTroopXP(string id, int zoneIndex, int xp)
    {
        string key = $"Player_Unit_XP_{id}_Zone_{zoneIndex}";
        PlayerPrefs.SetInt(key, xp);
        PlayerPrefs.Save();
    }

    private int GetDailyTrainingCount(int typeIndex, string unitId, int zoneIndex)
    {
        string dayKey = $"DailyTrainDay_{typeIndex}_{unitId}_{zoneIndex}";
        string countKey = $"DailyTrainCount_{typeIndex}_{unitId}_{zoneIndex}";
        
        int savedDay = PlayerPrefs.GetInt(dayKey, 0);
        if (savedDay != currentDay)
        {
            return 0;
        }
        return PlayerPrefs.GetInt(countKey, 0);
    }

    private void IncrementDailyTrainingCount(int typeIndex, string unitId, int zoneIndex)
    {
        string dayKey = $"DailyTrainDay_{typeIndex}_{unitId}_{zoneIndex}";
        string countKey = $"DailyTrainCount_{typeIndex}_{unitId}_{zoneIndex}";
        
        int currentCount = GetDailyTrainingCount(typeIndex, unitId, zoneIndex);
        PlayerPrefs.SetInt(dayKey, currentDay);
        PlayerPrefs.SetInt(countKey, currentCount + 1);
        PlayerPrefs.Save();
    }

    private void DrawUnifiedAcademySection(CastleInstance activeCastle, int curLang, float colWidth, GUIStyle subSt)
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));

        GUIStyle colHeader3 = new GUIStyle(GUI.skin.box);
        colHeader3.alignment = TextAnchor.MiddleCenter;
        colHeader3.fontSize = 17;
        colHeader3.fontStyle = FontStyle.Bold;
        colHeader3.normal.textColor = new Color(0.85f, 0.45f, 0.95f);

        string academyHeader = GetText9("🎓 АКАДЕМИЯ И АРЕНА", "🎓 ACADEMY & ARENA", "🎓 AKADEMIE & ARENA", "🎓 ACADÉMIE & ARÈNE", "🎓 ACADEMIA Y ARENA", "🎓 ACADEMIA E ARENA", "🎓 学院と闘技場", "🎓 아카데미 & 투기장", "🎓 皇家学院与斗技场");
        GUILayout.Label(academyHeader, colHeader3, GUILayout.Height(36));

        string aDesc = GetText9("Тренировки героев, прокачка XP и ранги воинов", "Hero workouts, dynamic XP drills & troop promotions", "Heldentraining, XP-Übungen und Truppenbeförderung", "Entraînement des héros, exercices d'XP et promotions", "Entrenamiento de héroes, ejercicios de XP y ascensos", "Treino de heróis, exercícios de XP e promoções", "ヒーロー訓練、経験値獲得、部隊の昇進", "영웅 훈련, 경험치 획득 및 군대 계급 승급", "进行英雄试炼、提升经验值与晋升军队阶级");
        GUILayout.Label(aDesc, subSt);

        GUILayout.Space(12);

        int cap = 5 + activeCastle.level * 5;
        string capText = GetText9(
            $"Текущий лимит тренировок: до {cap} уровня (зависит от уровня Замка)",
            $"Current training limit: up to Level {cap} (based on Castle Level)",
            $"Aktuelles Trainingslimit: bis Level {cap} (basierend auf Burglevel)",
            $"Limite d'entraînement : jusqu'au niveau {cap} (selon le niveau du château)",
            $"Límite de entrenamiento: hasta nivel {cap} (según nivel de castillo)",
            $"Limite de treino: até o nível {cap} (baseado no nível do castelo)",
            $"現在の訓練制限：レベル {cap} まで（城의 레벨에 의존）",
            $"현재 훈련 제한: 최대 {cap} 레벨까지 (성 레벨에 의함)",
            $"当前训练上限：最高可达 {cap} 级（受城堡等级限制）"
        );
        GUILayout.Label($"<b>{capText}</b>", GUI.skin.label);
        GUILayout.Space(12);

        // Collect entities to draw
        List<TrainingEntity> entities = new List<TrainingEntity>();
        
        // 1. Main Hero
        int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", -1);
        int actualPlayerRegion = GetActualRegionIndexFromLanding(landedZone);
        bool isMainHeroPresent = (actualPlayerRegion == activeCastle.zoneIndex);
        if (isMainHeroPresent)
        {
            TrainingEntity main = new TrainingEntity();
            main.id = "main_hero";
            main.name = GetText9(
                "👑 Основной Герой", "👑 Main Hero", "👑 Hauptheld", "👑 Héros Principal", 
                "👑 Héro Principal", "👑 Herói Principal", "👑 メインヒーロー", "👑 주인공 영웅", "👑 主角"
            );
            main.emoji = "👑";
            main.level = SaveGameSystem.CurrentData.playerLevel;
            main.xp = SaveGameSystem.CurrentData.currentXP;
            entities.Add(main);
        }
        
        // 2. Comrades
        string[] comradeKeys = { "ArcherHero", "WarriorHero", "MageHero" };
        foreach (var key in comradeKeys)
        {
            if (GetHeroCount(key, activeCastle.zoneIndex) > 0)
            {
                TrainingEntity comp = new TrainingEntity();
                comp.id = "comrade_" + key;
                string compName = "";
                string emoji = "";
                if (key == "ArcherHero")
                {
                    compName = GetText9("🏹 Эльфийский Стрелок", "🏹 Elven Archer Hero", "🏹 Elfenbogenschütze", "🏹 Archer Elfe", "🏹 Arquero Elfo", "🏹 Arqueiro Elfo", "🏹 エルフの射手", "🏹 엘프 궁수 영웅", "🏹 精灵射手(英雄)");
                    emoji = "🏹";
                }
                else if (key == "WarriorHero")
                {
                    compName = GetText9("🛡️ Священный Воин", "🛡️ Holy Warrior Hero", "🛡️ Heiliger Krieger", "🛡️ Guerrier Saint", "🛡️ Guerrero Santo", "🛡️ Guerreiro Santo", "🛡️ 聖なる戦士", "🛡️ 성광 전사 영웅", "🛡️ 圣光战士(英雄)");
                    emoji = "🛡️";
                }
                else
                {
                    compName = GetText9("🔮 Чародей Зенита", "🔮 Zenith Sorcerer Hero", "🔮 Zenit-Magier", "🔮 Sorcier de Zénith", "🔮 Sorcerer de Zénith", "🔮 Feiticeiro de Zênite", "🔮 ゼニスの魔術師", "🔮 제니스 마법사 영웅", "🔮 元素法师(英雄)");
                    emoji = "🔮";
                }
                comp.name = compName;
                comp.emoji = emoji;
                comp.level = PlayerPrefs.GetInt("Companion_Lvl_" + key, 1);
                comp.xp = PlayerPrefs.GetInt("Companion_XP_" + key, 0);
                entities.Add(comp);
            }
        }
        
        // 3. Troops (Войска) present in this castle
        string[] troop_ids = { "warrior", "archer", "mage", "paladin", "cavalry", "cannoneer", "centaur", "necromancer", "griffin", "overlord", "hydra", "dragon", "mountain_bear", "wasteland_serpent" };
        foreach (var tid in troop_ids)
        {
            if (GetUnitCount(tid, activeCastle.zoneIndex) > 0)
            {
                TrainingEntity tr = new TrainingEntity();
                tr.id = "troop_" + tid;
                tr.name = "⚔️ " + GetUnitNameByID(tid, curLang);
                tr.emoji = "⚔️";
                tr.level = GetTroopLevel(tid, activeCastle.zoneIndex);
                tr.xp = GetTroopXP(tid, activeCastle.zoneIndex);
                entities.Add(tr);
            }
        }

        // Draw Table Headers
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label(GetText9("Герой / Воин", "Hero / Troop", "Held / Truppe", "Héros / Troupe", "Héroe / Tropa", "Herói / Tropa", "ヒーロー / 部隊", "영웅 / 부대", "英雄 / 士兵"), GUILayout.Width(240));
        GUILayout.Label(GetText9("Простая (100💰)", "Simple (100💰)", "Einfach (100💰)", "Simple (100💰)", "Simple (100💰)", "Simples (100💰)", "簡易 (100💰)", "일반 (100💰)", "初级 (100💰)"), GUILayout.Width(130));
        GUILayout.Label(GetText9("Средняя (300💰)", "Medium (300💰)", "Mittel (300💰)", "Moyen (300💰)", "Medio (300💰)", "Médio (300💰)", "中級 (300💰)", "중급 (300💰)", "中级 (300💰)"), GUILayout.Width(130));
        GUILayout.Label(GetText9("Высокая (500💰)", "High (500💰)", "Hoch (500💰)", "Élevé (500💰)", "Alto (500💰)", "Alto (500💰)", "上級 (500💰)", "상급 (500💰)", "高级 (500💰)"), GUILayout.Width(130));
        GUILayout.Label(GetText9("Героическая (1000💰)", "Heroic (1000💰)", "Heldenhaft (1000💰)", "Héroïque (1000💰)", "Heroico (1000💰)", "Heroico (1000💰)", "英雄的 (1000💰)", "영웅 (1000💰)", "大师级 (1000💰)"), GUILayout.Width(130));
        GUILayout.Label(GetText9("Легендарная (3000💰)", "Legendary (3000💰)", "Legendär (3000💰)", "Légendaire (3000💰)", "Legendario (3000💰)", "Lendário (3000💰)", "伝説の (3000💰)", "전설 (3000💰)", "传说级 (3000💰)"), GUILayout.Width(130));
        GUILayout.EndHorizontal();

        academyScrollPos = GUILayout.BeginScrollView(academyScrollPos, GUILayout.Height(360));
        
        if (entities.Count == 0)
        {
            GUILayout.Label(GetText9(
                "⚠️ В этом замке нет купленных войск или героев для тренировки!",
                "⚠️ No purchased troops or heroes in this castle to train!",
                "⚠️ Keine gekauften Truppen oder Helden in dieser Burg zum Trainieren!",
                "⚠️ Aucune troupe ou héros acheté dans ce château pour s'entraîner !",
                "⚠️ ¡No hay tropas ni héroes comprados en este castillo para entrenar!",
                "⚠️ Sem tropas ou heróis comprados neste castelo para treinar!",
                "⚠️ この城には訓練可能な部隊やヒーローがいません！",
                "⚠️ 이 성에는 훈련할 수 있는 구매한 부대나 영웅이 없습니다!",
                "⚠️ 此城堡中暂无已招募的兵种或英雄可供训练！"
            ), subSt);
        }
        else
        {
            foreach (var entity in entities)
            {
                DrawTrainingGridRow(entity, activeCastle, curLang);
            }
        }
        
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void DrawTrainingGridRow(TrainingEntity ent, CastleInstance activeCastle, int curLang)
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        
        // Col 1: Unit details
        GUILayout.BeginVertical(GUILayout.Width(240));
        GUIStyle nameSt = new GUIStyle(GUI.skin.label);
        nameSt.fontStyle = FontStyle.Bold;
        nameSt.fontSize = 12;
        nameSt.normal.textColor = Color.white;
        GUILayout.Label(ent.name, nameSt);
        
        // Progress bar for XP
        float xpProgress = Mathf.Clamp01(ent.xp / 100f);
        int cap = 5 + activeCastle.level * 5;
        
        GUIStyle lvlSt = new GUIStyle(GUI.skin.label);
        lvlSt.fontSize = 10;
        lvlSt.normal.textColor = new Color(0.7f, 0.9f, 1f);
        
        string lvlLabel = GetText9("Ур.", "Lvl", "St.", "Niv", "Niv", "Nív", "Lv", "레벨", "等级");
        GUILayout.Label($"{lvlLabel} {ent.level} (XP: {ent.xp}/100) / Max: {cap}", lvlSt);
        
        // Graphical XP progress bar
        int bars = Mathf.RoundToInt(xpProgress * 10);
        string barStr = " [";
        for (int b = 0; b < 10; b++)
        {
            if (b < bars) barStr += "█";
            else barStr += "░";
        }
        barStr += "]";
        
        GUIStyle barSt = new GUIStyle(GUI.skin.label);
        barSt.fontSize = 9;
        barSt.normal.textColor = Color.yellow;
        GUILayout.Label(barStr, barSt);
        
        GUILayout.EndVertical();
        
        // Draw training buttons for 5 tiers
        DrawTrainingButtonCell(ent, 1, 100, 100, 20, activeCastle, curLang);
        DrawTrainingButtonCell(ent, 2, 300, 300, 10, activeCastle, curLang);
        DrawTrainingButtonCell(ent, 3, 500, 500, 5, activeCastle, curLang);
        DrawTrainingButtonCell(ent, 4, 1000, 1000, 3, activeCastle, curLang);
        DrawTrainingButtonCell(ent, 5, 3000, 3000, 1, activeCastle, curLang);
        
        GUILayout.EndHorizontal();
    }

    private void DrawTrainingButtonCell(TrainingEntity ent, int typeIndex, int cost, int xpToAdd, int maxTimes, CastleInstance activeCastle, int curLang)
    {
        int timesUsed = GetDailyTrainingCount(typeIndex, ent.id, activeCastle.zoneIndex);
        int cap = 5 + activeCastle.level * 5;
        bool limitReached = (timesUsed >= maxTimes);
        bool maxLevelReached = (ent.level >= cap);

        if (maxLevelReached)
        {
            GUI.enabled = false;
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            GUILayout.Button("MAX LVL", GUILayout.Width(130), GUILayout.Height(44));
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }
        else if (limitReached)
        {
            GUI.enabled = false;
            GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.6f);
            GUILayout.Button("LIMIT", GUILayout.Width(130), GUILayout.Height(44));
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }
        else
        {
            if (SaveGameSystem.CurrentData.gold < cost)
            {
                GUI.backgroundColor = new Color(0.4f, 0.3f, 0.3f, 1f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.15f, 0.7f, 0.4f, 1f);
            }
            
            if (GUILayout.Button($"{cost} 💰\n+{xpToAdd} XP [{timesUsed}/{maxTimes}]", GUILayout.Width(130), GUILayout.Height(44)))
            {
                TrainEntity(ent.id, typeIndex, cost, xpToAdd, maxTimes, activeCastle.zoneIndex, curLang);
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private void TrainEntity(string unitId, int typeIndex, int cost, int xpToAdd, int maxTimes, int zoneIndex, int curLang)
    {
        CastleInstance activeCastle = castles[zoneIndex];
        int cap = 5 + activeCastle.level * 5;

        if (SaveGameSystem.CurrentData.gold < cost)
        {
            string noGold = GetText9(
                "Недостаточно золота для тренировки!",
                "Not enough gold for training!",
                "Nicht genug Gold für das Training!",
                "Pas assez d'or pour l'entraînement !",
                "¡No hay suficiente oro para entrenar!",
                "Ouro insuficiente para o treino!",
                "訓練に必要なゴールドが不足しています！",
                "훈련에 필요한 골드가 부족합니다!",
                "金币不足，无法进行训练！"
            );
            ShowFeedback(noGold);
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(1);
            return;
        }

        int timesUsed = GetDailyTrainingCount(typeIndex, unitId, zoneIndex);
        if (timesUsed >= maxTimes)
        {
            string noLmt = GetText9(
                "Достигнут дневной лимит тренировок этого типа!",
                "Daily limit for this training type reached!",
                "Tägliches Trainingslimit für diesen Typ erreicht!",
                "Limite quotidienne pour ce type d'entraînement atteinte !",
                "¡Límite diario para este tipo de entrenamiento alcanzado!",
                "Limite diário para este tipo de treino atingido!",
                "このタイプのトレーニングの1日制限に達しました！",
                "이 유형의 일일 훈련 제한에 도달했습니다!",
                "该类型的每日训练上限已满！"
            );
            ShowFeedback(noLmt);
            if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(1);
            return;
        }

        // Apply based on unitId
        if (unitId == "main_hero")
        {
            int curLvl = SaveGameSystem.CurrentData.playerLevel;
            int curXp = SaveGameSystem.CurrentData.currentXP;
            
            if (curLvl >= cap)
            {
                ShowFeedback(GetText9(
                    "Достигнут лимит уровня основного героя для этого замка!",
                    "Main hero level limit reached for this castle!",
                    "Heldenlevel-Limit für diese Burg erreicht!",
                    "Limite de niveau de héros principal atteinte !",
                    "¡Límite de nivel de héroe principal alcanzado!",
                    "Limite de nível do herói principal atingido!",
                    "メインヒーロー의 레벨 제한에 도달했습니다！",
                    "주인공 영웅의 레벨 제한에 도달했습니다!",
                    "主角已达当前城堡的等级上限！"
                ));
                return;
            }

            SaveGameSystem.CurrentData.gold -= cost;
            IncrementDailyTrainingCount(typeIndex, unitId, zoneIndex);
            
            curXp += xpToAdd;
            bool lvlUp = false;
            while (curXp >= 100 && curLvl < cap)
            {
                curXp -= 100;
                curLvl++;
                lvlUp = true;
            }
            if (curLvl >= cap) curXp = 0;
            
            SaveGameSystem.CurrentData.playerLevel = curLvl;
            SaveGameSystem.CurrentData.currentXP = curXp;
            
            if (lvlUp)
            {
                string lvlMsg = GetText9(
                    $"🌟 УРОВЕНЬ ПОВЫШЕН! Основной герой достиг {curLvl} уровня!",
                    $"🌟 protagonist LEVEL UP achieved! Protagonist reached Level {curLvl}!",
                    $"🌟 STUFE ERHÖHT! Hauptheld hat Stufe {curLvl} erreicht!",
                    $"🌟 NIVEAU SUPÉRIEUR ! Le héros principal a atteint le niveau {curLvl} !",
                    $"🌟 ¡SUBIDA DE NIVEL! ¡El héroe principal ha alcanzado el nivel {curLvl}!",
                    $"🌟 LEVEL UP! Herói principal atingiu o Nível {curLvl}!",
                    $"🌟 レベルアップ！メインヒーローがレベル {curLvl} に達しました！",
                    $"🌟 레벨 업! 주인공 영웅이 레벨 {curLvl}에 도달했습니다!",
                    $"🌟 等级提升！主角已升至 {curLvl} 级！"
                );
                ShowFeedback(lvlMsg);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            else
            {
                string feed = GetText9(
                    $"Тренировка основного персонажа завершена! (+{xpToAdd} XP)",
                    $"protagonist training complete! (+{xpToAdd} XP)",
                    $"Haupthelden-Training abgeschlossen! (+{xpToAdd} XP)",
                    $"Entraînement du héros principal terminé ! (+{xpToAdd} XP)",
                    $"¡Entrenamiento del héroe principal completado! (+{xpToAdd} XP)",
                    $"Treino do herói principal completo! (+{xpToAdd} XP)",
                    $"メインヒーロー의 훈련 완료！ (+{xpToAdd} XP)",
                    $"주인공 영웅의 훈련 완료! (+{xpToAdd} XP)",
                    $"主角训练完成！ (+{xpToAdd} 经验)"
                );
                ShowFeedback(feed);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            RecalculateStats();
            SaveGameSystem.Save(0);
        }
        else if (unitId.StartsWith("comrade_"))
        {
            string key = unitId.Substring("comrade_".Length);
            int curLvl = PlayerPrefs.GetInt("Companion_Lvl_" + key, 1);
            int curXp = PlayerPrefs.GetInt("Companion_XP_" + key, 0);

            if (curLvl >= cap)
            {
                ShowFeedback(GetText9(
                    "Достигнут лимит уровня союзного героя для этого замка!",
                    "Companion hero level limit reached for this castle!",
                    "Gefährtenlevel-Limit für diese Burg erreicht!",
                    "Limite de niveau de compagnon atteinte !",
                    "¡Límite de nivel de compañero alcanzado!",
                    "Limite de nível do companheiro atingido!",
                    "コンパニオン의 레벨 제한에 도달했습니다！",
                    "동료 영웅의 레벨 제한에 도달했습니다!",
                    "副将已达当前城堡的等级上限！"
                ));
                return;
            }

            SaveGameSystem.CurrentData.gold -= cost;
            IncrementDailyTrainingCount(typeIndex, unitId, zoneIndex);

            curXp += xpToAdd;
            bool lvlUp = false;
            while (curXp >= 100 && curLvl < cap)
            {
                curXp -= 100;
                curLvl++;
                lvlUp = true;
            }
            if (curLvl >= cap) curXp = 0;

            PlayerPrefs.SetInt("Companion_Lvl_" + key, curLvl);
            PlayerPrefs.SetInt("Companion_XP_" + key, curXp);

            if (lvlUp)
            {
                string lvlMsg = GetText9(
                    $"🌟 УРОВЕНЬ ПОВЫШЕН! Союзный герой {key} достиг {curLvl} уровня!",
                    $"🌟 COMPANION LEVEL UP! Companion {key} reached Level {curLvl}!",
                    $"🌟 STUFE ERHÖHT! Gefährte {key} hat Stufe {curLvl} erreicht!",
                    $"🌟 NIVEAU SUPÉRIEUR ! Le compagnon {key} a atteint le niveau {curLvl} !",
                    $"🌟 ¡SUBIDA DE NIVEL! ¡El compañero {key} ha alcanzado el nivel {curLvl}!",
                    $"🌟 LEVEL UP! Companheiro {key} atingiu o Nível {curLvl}!",
                    $"🌟 レベルアップ！コンパニオン {key} がレベル {curLvl} に達しました！",
                    $"🌟 레벨 업! 동료 {key}이(가) 레벨 {curLvl}에 도달했습니다!",
                    $"🌟 等级提升！英雄 {key} 已升至 {curLvl} 级！"
                );
                ShowFeedback(lvlMsg);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            else
            {
                string feed = GetText9(
                    $"Тренировка союзного героя завершена! (+{xpToAdd} XP)",
                    $"Companion training complete! (+{xpToAdd} XP)",
                    "Gefährten-Training abgeschlossen! (+{xpToAdd} XP)",
                    "Entraînement du compagnon terminé ! (+{xpToAdd} XP)",
                    "¡Entrenamiento del compañero completado! (+{xpToAdd} XP)",
                    "Treino do companheiro completo! (+{xpToAdd} XP)",
                    $"コンパニオン의 훈련 완료！ (+{xpToAdd} XP)",
                    $"동료 영웅의 훈련 완료! (+{xpToAdd} XP)",
                    $"英雄训练完成！ (+{xpToAdd} 经验)"
                );
                ShowFeedback(feed);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            SaveGameSystem.Save(0);
        }
        else if (unitId.StartsWith("troop_"))
        {
            string key = unitId.Substring("troop_".Length);
            int curLvl = GetTroopLevel(key, zoneIndex);
            int curXp = GetTroopXP(key, zoneIndex);

            if (curLvl >= cap)
            {
                ShowFeedback(GetText9(
                    "Достигнут лимит уровня отряда для этого замка!",
                    "Troop level limit reached for this castle!",
                    "Truppenlevel-Limit für diese Burg erreicht!",
                    "Limite de niveau de troupe atteinte !",
                    "¡Límite de nivel de tropa alcanzado!",
                    "Limite de nível da tropa atingido!",
                    "部隊의 レ벨 제한에 도달했습니다！",
                    "부대 레벨 제한에 도달했습니다!",
                    "士兵已达当前城堡的等级上限！"
                ));
                return;
            }

            SaveGameSystem.CurrentData.gold -= cost;
            IncrementDailyTrainingCount(typeIndex, unitId, zoneIndex);

            curXp += xpToAdd;
            bool lvlUp = false;
            while (curXp >= 100 && curLvl < cap)
            {
                curXp -= 100;
                curLvl++;
                lvlUp = true;
            }
            if (curLvl >= cap) curXp = 0;

            SetTroopLevel(key, zoneIndex, curLvl);
            SetTroopXP(key, zoneIndex, curXp);

            string displayName = GetUnitNameByID(key, curLang);

            if (lvlUp)
            {
                string lvlMsg = GetText9(
                    $"🌟 РАНГ ПОВЫШЕН! Отряд {displayName} в этом замке получил {curLvl} уровень!",
                    $"🌟 TROOP PROMOTION! {displayName} cohort reached Level {curLvl}!",
                    $"🌟 TRUPPEN-BEFÖRDERUNG! {displayName}-Kohorte hat Stufe {curLvl} erreicht!",
                    $"🌟 PROMOTION DE TROUPE ! La cohorte de {displayName} a atteint le niveau {curLvl} !",
                    $"🌟 ¡ASCENSO DE TROPA! ¡La cohorte de {displayName} ha alcanzado el nivel {curLvl}!",
                    $"🌟 PROMOÇÃO DE TROPA! Coorte de {displayName} atingiu o Nível {curLvl}!",
                    $"🌟 昇進！ {displayName} の部隊がレベル {curLvl} に達しました！",
                    $"🌟 부대 승급! {displayName} 부대가 레벨 {curLvl}에 도달했습니다!",
                    $"🌟 等级提升！该城堡的 {displayName} 已升至 {curLvl} 级！"
                );
                ShowFeedback(lvlMsg);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            else
            {
                string feed = GetText9(
                    $"Тренировка отряда {displayName} завершена! (+{xpToAdd} XP)",
                    $"Cohort {displayName} training complete! (+{xpToAdd} XP)",
                    $"Garnisons-Training für {displayName} abgeschlossen! (+{xpToAdd} XP)",
                    $"Entraînement de la cohorte de {displayName} terminé ! (+{xpToAdd} XP)",
                    $"¡Entrenamiento de la cohorte de {displayName} completado! (+{xpToAdd} XP)",
                    $"Treino da coorte de {displayName} completo! (+{xpToAdd} XP)",
                    $"{displayName} の部隊の訓練完了！ (+{xpToAdd} XP)",
                    $"{displayName} 부대의 훈련 완료! (+{xpToAdd} XP)",
                    $"{displayName} 训练完成！ (+{xpToAdd} 经验)"
                );
                ShowFeedback(feed);
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
            SaveGameSystem.Save(0);
        }
    }

    private void DrawUnifiedForgeSection(CastleInstance activeCastle, int curLang, float colWidth, GUIStyle subSt)
    {
        GUIStyle tabStyle = s_tabBtnStyle != null ? s_tabBtnStyle : new GUIStyle(GUI.skin.button);
        tabStyle.fontSize = 10;
        tabStyle.fontStyle = FontStyle.Bold;

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));

        GUIStyle colHeader2 = new GUIStyle(GUI.skin.box);
        colHeader2.alignment = TextAnchor.MiddleCenter;
        colHeader2.fontSize = 17;
        colHeader2.fontStyle = FontStyle.Bold;
        colHeader2.normal.textColor = new Color(1.0f, 0.7f, 0.15f);

        string forgeHeader = GetText9("🧪 КУЗНИЦА И ЛАВКА", "🧪 FORGE & POTION SHOP", "🧪 SCHMIEDE & ALCHEMIELADEN", "🧪 FORGE & ALCHIMIE", "🧪 FORJA Y BOTICA", "🧪 FORJA E BOTICA", "🧪 鍛冶屋とポーションショップ", "🧪 대장간 및 물약 상점", "🧪 铁匠铺与药水商会");
        GUILayout.Label(forgeHeader, colHeader2, GUILayout.Height(36));

        string fDesc = GetText9("Изготовление элитного снаряжения и боевых зелий за золото", "Forge high-tier gear and brew combat potions using gold", "Schmieden Sie erstklassige Ausrüstung und brauen Sie Kampftränke mit Gold", "Forgez des équipements d'élite et brassez des potions de combat avec de l'or", "Forja equipo de élite y elabora pociones de combate usando oro", "Forje equipamentos de elite e produza poções de combate usando ouro", "ゴールドを消費してエリート装備を鍛造し、戦闘ポーションを調合します", "골드를 소모하여 엘리트 장비를 제작하고 전투 물약을 조제합니다", "消耗金币来锻造精良装备与炼制战斗药水");
        GUILayout.Label(fDesc, subSt);

        GUILayout.Space(12);

        // --- POTION BREWING TAB ---
        int potionTab = PlayerPrefs.GetInt("Town_Selected_PotionTab", 0);
        string[] potTabNames;
        switch (curLang)
        {
            case 0: potTabNames = new string[] { "❤️ Жизнь", "💪 Сила", "🔮 Интеллект", "⚡ Ловкость", "🛡️ Стойкость" }; break;
            case 1: potTabNames = new string[] { "❤️ Health", "💪 Strength", "🔮 Intelligence", "⚡ Agility", "🛡️ Stamina" }; break;
            case 2: potTabNames = new string[] { "❤️ Leben", "💪 Stärke", "🔮 Intelligenz", "⚡ Beweglichkeit", "🛡️ Ausdauer" }; break;
            case 3: potTabNames = new string[] { "❤️ Vie", "💪 Force", "🔮 Intelligence", "⚡ Agilité", "🛡️ Endurance" }; break;
            case 4: potTabNames = new string[] { "❤️ Salud", "💪 Fuerza", "🔮 Inteligencia", "⚡ Agilidad", "🛡️ Aguante" }; break;
            case 5: potTabNames = new string[] { "❤️ Vida", "💪 Força", "🔮 Inteligência", "⚡ Agilidade", "🛡️ Resistência" }; break;
            case 6: potTabNames = new string[] { "❤️ 生命", "💪 筋力", "🔮 知性", "⚡ 敏捷", "🛡️ スタミナ" }; break;
            case 7: potTabNames = new string[] { "❤️ 체력", "💪 힘", "🔮 지능", "⚡ 민첩", "🛡️ 지구력" }; break;
            case 8: potTabNames = new string[] { "❤️ 生命", "💪 力量", "🔮 智力", "⚡ 敏捷", "🛡️ 耐力" }; break;
            default: potTabNames = new string[] { "❤️ Health", "💪 Strength", "🔮 Intelligence", "⚡ Agility", "🛡️ Stamina" }; break;
        }

        for (int i = 0; i < potTabNames.Length; i++)
        {
            GUI.backgroundColor = (potionTab == i) ? new Color(0.2f, 0.7f, 1f, 1f) : Color.white;
            if (GUILayout.Button(potTabNames[i], tabStyle, GUILayout.Height(30)))
            {
                potionTab = i;
                PlayerPrefs.SetInt("Town_Selected_PotionTab", potionTab);
                PlayerPrefs.Save();
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        string potType = "hp";
        string pNameRU = "Зелье Жизни", pNameEN = "Elixir of Vital Health", pNameCH = "生命圣水", pNameKR = "체력 신성 물약";
        int basePrice = 30;

        if (potionTab == 1) { potType = "str"; pNameRU = "Зелье Силы"; pNameEN = "Potion of Giant Strength"; pNameCH = "巨人之力药水"; pNameKR = "거인의 괴력 물약"; basePrice = 45; }
        else if (potionTab == 2) { potType = "int"; pNameRU = "Зелье Интеллекта"; pNameEN = "Potion of Mind Intelligence"; pNameCH = "智力药水"; pNameKR = "지능 영약"; basePrice = 40; }
        else if (potionTab == 3) { potType = "agi"; pNameRU = "Зелье Ловкости"; pNameEN = "Potion of Swift Agility"; pNameCH = "敏捷药水"; pNameKR = "민첩 영약"; basePrice = 40; }
        else if (potionTab == 4) { potType = "sta"; pNameRU = "Зелье Выносливости"; pNameEN = "Potion of Iron Stamina"; pNameCH = "耐力药水"; pNameKR = "체력/지구력 영약"; basePrice = 40; }

        for (int lvl = 1; lvl <= 10; lvl++)
        {
            DrawPotionItem(potType, pNameRU, pNameEN, pNameCH, pNameKR, basePrice, lvl, activeCastle.level);
        }

        GUILayout.Space(15);
        GUILayout.Box(curLang == 0 ? "⚔️ КУЗНИЦА СНАРЯЖЕНИЯ" : "⚔️ FORGE DEPARTMENT", GUILayout.Height(20));

        // --- FORGE PREVIEW CLASS (v18.11.23) ---
        string previewClass = PlayerPrefs.GetString("Forge_Preview_Class", "");
        string realClass = "warrior";
        if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            realClass = SaveGameSystem.CurrentData.characterClass.ToLower();
        }
        if (string.IsNullOrEmpty(previewClass))
        {
            previewClass = realClass;
        }

        GUILayout.BeginHorizontal();
        GUIStyle previewLabelStyle = new GUIStyle(GUI.skin.label);
        previewLabelStyle.fontSize = 11;
        previewLabelStyle.fontStyle = FontStyle.Bold;
        previewLabelStyle.normal.textColor = Color.yellow;
        GUILayout.Label(curLang == 0 ? "Просмотр класса:" : "Class View:", previewLabelStyle, GUILayout.Width(110));

        string[] previewClassNames = curLang == 0 ? 
            new string[] { "⚔️ Воин", "🏹 Стрелок", "🔮 Маг" } :
            new string[] { "⚔️ Warrior", "🏹 Archer", "🔮 Mage" };
        string[] previewClassKeys = new string[] { "warrior", "archer", "mage" };
        for (int i = 0; i < previewClassKeys.Length; i++)
        {
            bool isSel = previewClass.Contains(previewClassKeys[i]);
            GUI.backgroundColor = isSel ? new Color(0.15f, 0.75f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button(previewClassNames[i], tabStyle, GUILayout.Height(24)))
            {
                previewClass = previewClassKeys[i];
                PlayerPrefs.SetString("Forge_Preview_Class", previewClass);
                PlayerPrefs.Save();
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        // Informative subtitle
        GUIStyle subHintStyle = new GUIStyle(GUI.skin.label);
        subHintStyle.fontSize = 9;
        subHintStyle.alignment = TextAnchor.MiddleCenter;
        subHintStyle.normal.textColor = Color.gray;
        string hintText = curLang == 0 ?
            "★ Характеристики и названия снаряжения автоматически подстроятся под ваш класс при надевании!" :
            "★ Equipment stats and names automatically adapt to your chosen hero class when equipped!";
        GUILayout.Label(hintText, subHintStyle);
        GUILayout.Space(5);

        // --- EQUIPMENT FORGING TABS ---
        int forgeTab = PlayerPrefs.GetInt("Town_Selected_ForgeTab", 0);
        
        string[] forgeTabNames = curLang == 0 ? 
            new string[] { "⚔️ Оружие", "👑 Шлем", "👕 Броня", "🦾 Наплечники", "🥾 Сапоги", "🎗️ Пояс", "📿 Амулет", "💍 Кольцо" } :
            new string[] { "⚔️ Weapon", "👑 Helmet", "👕 Chest", "🦾 Shoulders", "🥾 Boots", "🎗️ Belt", "📿 Amulet", "💍 Ring" };
        if (curLang == 8) forgeTabNames = new string[] { "⚔️ 武器", "👑 头盔", "👕 护甲", "🦾 护肩", "🥾 鞋子", "🎗️ 腰带", "📿 项链", "💍 戒指" };
        if (curLang == 7) forgeTabNames = new string[] { "⚔️ 무기", "👑 투구", "👕 갑옷", "🦾 어깨갑옷", "🥾 신발", "🎗️ 벨트", "📿 아뮬렛", "💍 반지" };

        tabsScroll = GUILayout.BeginScrollView(tabsScroll, GUILayout.Height(44));
        GUILayout.BeginHorizontal();
        for (int i = 0; i < forgeTabNames.Length; i++)
        {
            GUI.backgroundColor = (forgeTab == i) ? new Color(1f, 0.6f, 0.2f, 1f) : Color.white;
            if (GUILayout.Button(forgeTabNames[i], tabStyle, GUILayout.Height(26)))
            {
                forgeTab = i;
                PlayerPrefs.SetInt("Town_Selected_ForgeTab", forgeTab);
                PlayerPrefs.Save();
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        int fSlotType = GetSlotTypeFromForgeTab(forgeTab);
        for (int tier = 1; tier <= 6; tier++)
        {
            DrawForgeEquipmentOption(fSlotType, tier, activeCastle.level);
        }

        GUILayout.Space(15);
        GUILayout.Box(curLang == 0 ? "🕵️ НАЙМ ПРОСТЫХ ГЕРОЕВ" : "🕵️ RECRUIT ALLIED HEROES", GUILayout.Height(20));
        
        int activeZoneIdx = activeDetailsIndex >= 0 ? activeDetailsIndex : 0;
        int maxSimp = 20 + activeZoneIdx * 15;
        int simpTot = GetHeroCount("ArcherHero", activeZoneIdx) + 
                     GetHeroCount("WarriorHero", activeZoneIdx) + 
                     GetHeroCount("MageHero", activeZoneIdx);

        GUIStyle limStyle = new GUIStyle(GUI.skin.label);
        limStyle.alignment = TextAnchor.MiddleCenter;
        limStyle.fontSize = 11;
        limStyle.fontStyle = FontStyle.Bold;
        limStyle.normal.textColor = (simpTot >= maxSimp) ? Color.red : new Color(0.2f, 0.8f, 1f);

        string limLabel = curLang == 0 ?
            $"★ Лимит на {activeZoneIdx + 1}-м Континенте: {simpTot} / {maxSimp} простых героев нанято" :
            $"★ Limit on Continent {activeZoneIdx + 1}: {simpTot} / {maxSimp} allied heroes recruited";
        if (curLang == 8) limLabel = $"★ 当前大陆上限: 已招募 {simpTot} / {maxSimp} 位普通英雄";
        if (curLang == 7) limLabel = $"★ 이 대륙의 한도: {simpTot} / {maxSimp}명의 일반 영웅 고용됨";

        GUILayout.Label(limLabel, limStyle);
        GUILayout.Space(5);
        
        DrawHeroRecruitItem("ArcherHero", "Герой: Стрелок", "Comrade: Marksman Hero", "游侠英雄-神射手", "동료 영웅 - 명사수", 300);
        DrawHeroRecruitItem("WarriorHero", "Герой: Воин", "Comrade: Iron Warrior", "先锋英雄-铁血战士", "동료 영웅 - 광전사", 350);
        DrawHeroRecruitItem("MageHero", "Герой: Боевой Маг", "Comrade: Sorcerer Elite", "元素法师-高阶贤者", "동료 영웅 - 일급 현자", 400);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private int GetSlotTypeFromForgeTab(int forgeTab)
    {
        switch (forgeTab)
        {
            case 0: return 8; // Weapon
            case 1: return 1; // Helmet
            case 2: return 4; // Chest
            case 3: return 3; // Shoulders
            case 4: return 7; // Boots
            case 5: return 6; // Belt
            case 6: return 2; // Amulet
            case 7: return 5; // Ring
            default: return 8;
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

        CompanionData cd = GetCompanionData(key);
        int lvl = PlayerPrefs.GetInt("Companion_Lvl_" + key, 1);
        int hp = GetCompanionStat(key, "hp", lvl);
        int atk = GetCompanionStat(key, "atk", lvl);
        int def = GetCompanionStat(key, "def", lvl);

        string name = curLang == 0 ? cd.nameRU : cd.nameEN;
        if (curLang == 8) name = cd.nameRU;
        if (curLang == 7) name = cd.nameEN;

        GUILayout.BeginHorizontal(GUI.skin.box);

        // ==================== LEFT COLUMN: AVATAR, NAME & ENLARGED STATS ====================
        GUILayout.BeginVertical(GUILayout.Width(220));

        GUILayout.BeginHorizontal();

        Texture2D av = GetTroopAvatarTexture(key);
        GUIStyle avBtnStyle = new GUIStyle(GUI.skin.button);
        avBtnStyle.padding = new RectOffset(2, 2, 2, 2);
        
        GUILayout.BeginVertical(GUILayout.Width(58), GUILayout.Height(58));
        if (av != null)
        {
            if (GUILayout.Button(av, avBtnStyle, GUILayout.Width(54), GUILayout.Height(54)))
            {
                selectedTroopId = key;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        else
        {
            if (GUILayout.Button("📷", avBtnStyle, GUILayout.Width(54), GUILayout.Height(54)))
            {
                selectedTroopId = key;
                showTroopDetailPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(8);

        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.fontSize = 12;
        nameStyle.wordWrap = true;
        nameStyle.normal.textColor = Color.yellow;
        string labelText = curLang == 0 ? 
            $"<b>{cd.nameRU}</b>\n[Гарнизон: {count} шт]" : 
            $"<b>{cd.nameEN}</b>\n[Garrison: {count}]";
        GUILayout.Label(labelText, nameStyle);

        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUIStyle largeStatsStyle = new GUIStyle(GUI.skin.box);
        largeStatsStyle.fontSize = 11;
        largeStatsStyle.fontStyle = FontStyle.Bold;
        largeStatsStyle.richText = true;
        largeStatsStyle.alignment = TextAnchor.MiddleCenter;
        largeStatsStyle.normal.textColor = Color.white;

        string hpTxt = curLang == 0 ? "ОЗ" : "HP";
        string atkTxt = curLang == 0 ? "АТК" : "ATK";
        string defTxt = curLang == 0 ? "ЗАЩ" : "DEF";
        
        string statsText = $"❤️ {hpTxt}: <color=lime>{hp}</color> | ⚔️ {atkTxt}: <color=orange>{atk}</color> | 🛡️ {defTxt}: <color=yellow>{def}</color>";
        GUILayout.Label(statsText, largeStatsStyle, GUILayout.Height(28));

        GUILayout.EndVertical();

        GUILayout.Space(25);

        // ==================== RIGHT COLUMN: SHIFTED COMBAT SKILLS ====================
        GUILayout.BeginVertical();

        GUIStyle subTitleStyle = new GUIStyle(GUI.skin.label);
        subTitleStyle.fontStyle = FontStyle.Bold;
        subTitleStyle.fontSize = 11;
        subTitleStyle.normal.textColor = new Color(0.2f, 0.8f, 1f);
        GUILayout.Label(curLang == 0 ? "🌟 Боевые умения спутника:" : "🌟 Companion Combat Skills:", subTitleStyle);

        GUILayout.BeginHorizontal();

        GUIStyle skillCardStyle = new GUIStyle(GUI.skin.box);
        skillCardStyle.padding = new RectOffset(6, 6, 6, 6);

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.fontSize = 10;
        descStyle.wordWrap = true;
        descStyle.richText = true;
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

        string activeName = cd.activeName;
        string activeDesc = cd.activeDesc;
        string pass1Name = cd.passiveNames.Length > 0 ? cd.passiveNames[0] : "Passive 1";
        string pass1Desc = cd.passiveDesc.Length > 0 ? cd.passiveDesc[0] : "";
        string pass2Name = cd.passiveNames.Length > 1 ? cd.passiveNames[1] : "Passive 2";
        string pass2Desc = cd.passiveDesc.Length > 1 ? cd.passiveDesc[1] : "";

        Texture2D activeSkillIcon = GetTroopActiveSkillIcon(key);
        Texture2D pass1SkillIcon = GetTroopPassiveSkillIcon(key, 0);
        Texture2D pass2SkillIcon = GetTroopPassiveSkillIcon(key, 1);

        // --- SKILL 1: ACTIVE ---
        GUILayout.BeginHorizontal(skillCardStyle, GUILayout.Width(220), GUILayout.Height(66));
        GUILayout.BeginVertical(GUILayout.Width(44), GUILayout.Height(44));
        if (activeSkillIcon != null)
        {
            GUILayout.Label(activeSkillIcon, GUILayout.Width(40), GUILayout.Height(40));
        }
        else
        {
            GUIStyle pS = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, normal = { textColor = Color.gray } };
            GUILayout.Label("📷", pS, GUILayout.Width(40), GUILayout.Height(40));
        }
        GUILayout.EndVertical();
        Rect actRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(6);
        GUILayout.BeginVertical();
        GUIStyle sNameStyle = new GUIStyle(nameStyle) { fontSize = 10 };
        GUILayout.Label($"🔥 <color=orange><b>{activeName}</b></color>", sNameStyle);
        GUILayout.Label(activeDesc, descStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // --- SKILL 2: PASSIVE 1 ---
        GUILayout.BeginHorizontal(skillCardStyle, GUILayout.Width(220), GUILayout.Height(66));
        GUILayout.BeginVertical(GUILayout.Width(44), GUILayout.Height(44));
        if (pass1SkillIcon != null)
        {
            GUILayout.Label(pass1SkillIcon, GUILayout.Width(40), GUILayout.Height(40));
        }
        else
        {
            GUIStyle pS = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, normal = { textColor = Color.gray } };
            GUILayout.Label("📷", pS, GUILayout.Width(40), GUILayout.Height(40));
        }
        GUILayout.EndVertical();
        Rect p1Rect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(6);
        GUILayout.BeginVertical();
        GUILayout.Label($"🔮 <color=cyan><b>{pass1Name}</b></color>", sNameStyle);
        GUILayout.Label(pass1Desc, descStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // --- SKILL 3: PASSIVE 2 ---
        GUILayout.BeginHorizontal(skillCardStyle, GUILayout.Width(220), GUILayout.Height(66));
        GUILayout.BeginVertical(GUILayout.Width(44), GUILayout.Height(44));
        if (pass2SkillIcon != null)
        {
            GUILayout.Label(pass2SkillIcon, GUILayout.Width(40), GUILayout.Height(40));
        }
        else
        {
            GUIStyle pS = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, normal = { textColor = Color.gray } };
            GUILayout.Label("📷", pS, GUILayout.Width(40), GUILayout.Height(40));
        }
        GUILayout.EndVertical();
        Rect p2Rect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(6);
        GUILayout.BeginVertical();
        GUILayout.Label($"🔮 <color=cyan><b>{pass2Name}</b></color>", sNameStyle);
        GUILayout.Label(pass2Desc, descStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            if (actRect.Contains(Event.current.mousePosition))
            {
                isHoveringSkill = true;
                hoveredSkillName = cd.activeName;
                hoveredSkillDesc = cd.activeDesc;
                hoveredSkillType = curLang == 0 ? "🔥 АКТИВНЫЙ НАВЫК" : "🔥 ACTIVE SKILL";
                hoveredSkillIcon = activeSkillIcon;
            }
            else if (p1Rect.Contains(Event.current.mousePosition))
            {
                isHoveringSkill = true;
                hoveredSkillName = cd.passiveNames.Length > 0 ? cd.passiveNames[0] : "";
                hoveredSkillDesc = (cd.passiveDesc.Length > 0 ? cd.passiveDesc[0] : "");
                hoveredSkillType = curLang == 0 ? "🔮 ПАССИВНЫЙ НАВЫК" : "🔮 PASSIVE SKILL";
                hoveredSkillIcon = pass1SkillIcon;
            }
            else if (p2Rect.Contains(Event.current.mousePosition))
            {
                isHoveringSkill = true;
                hoveredSkillName = cd.passiveNames.Length > 1 ? cd.passiveNames[1] : "";
                hoveredSkillDesc = (cd.passiveDesc.Length > 1 ? cd.passiveDesc[1] : "");
                hoveredSkillType = curLang == 0 ? "🔮 ПАССИВНЫЙ НАВЫК" : "🔮 PASSIVE SKILL";
                hoveredSkillIcon = pass2SkillIcon;
            }
        }

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button($"{basePrice} 💰", GUILayout.Width(80), GUILayout.Height(44)))
        {
            CastleInstance activeCastle = castles[activeDetailsIndex >= 0 ? activeDetailsIndex : 0];
            int currentHeroes = GetHeroesCountInCastle(activeCastle.zoneIndex);
            int capacity = GetHeroCapacity(activeCastle.level);

            int maxSimpleHeroes = 20 + activeDetailsIndex * 15;
            int simpleHeroesTotal = GetHeroCount("ArcherHero", activeDetailsIndex) + 
                                   GetHeroCount("WarriorHero", activeDetailsIndex) + 
                                   GetHeroCount("MageHero", activeDetailsIndex);

            int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", -1);
            int actualPlayerRegion = GetActualRegionIndexFromLanding(landedZone);
            bool isMainHeroPresent = (actualPlayerRegion == activeDetailsIndex);

            if (CheckAndEnforceHeroLimits(activeCastle, isMainHeroPresent, simpleHeroesTotal + 1))
            {
                // Exceeded Level 1, 2, 3 hero limits! Handled inside CheckAndEnforceHeroLimits.
            }
            else if (simpleHeroesTotal >= maxSimpleHeroes)
            {
                string limitTxt = curLang == 0 ?
                    $"Лимит покупки простых простых героев на этом континенте ({maxSimpleHeroes} шт) исчерпан!" :
                    $"Simple hero recruitment limit on this continent ({maxSimpleHeroes}) reached!";
                if (curLang == 8) limitTxt = $"当前大陆普通英雄招募上限 ({maxSimpleHeroes}) 已达！";
                if (curLang == 7) limitTxt = $"이 대륙의 일반 영웅 모집 한도 ({maxSimpleHeroes}명)가 초과되었습니다!";
                ShowFeedback(limitTxt);
            }
            else if (currentHeroes >= capacity)
            {
                string limitTxt = curLang == 0 ?
                    $"Достигнут лимит героев в этом замке ({currentHeroes}/{capacity})! Повысьте уровень цитадели." :
                    $"Castle hero garrison limit reached ({currentHeroes}/{capacity})! Upgrade stronghold first.";
                if (curLang == 8) limitTxt = $"已达城堡英雄上限 ({currentHeroes}/{capacity})！请先升级主城。";
                if (curLang == 7) limitTxt = $"성채 영웅 한도 초과 ({currentHeroes}/{capacity})! 성채를 먼저 업г레이드 하십시오.";
                ShowFeedback(limitTxt);
            }
            else
            {
                int targetPrice = basePrice;
                string targetKey = key;
                int targetCount = count;
                CompanionData targetCd = cd;
                
                confirmItemName = curLang == 0 ? cd.nameRU : cd.nameEN;
                confirmCost = basePrice;
                confirmAction = () => {
                    SaveGameSystem.CurrentData.gold -= targetPrice;
                    int newCount = targetCount + 1;
                    SetHeroCount(targetKey, activeDetailsIndex, newCount);

                    string joinFeed = curLang == 0 ?
                        $"Герой {targetCd.nameRU} успешно нанят в гарнизон замка!" :
                        $"Renowned leader {targetCd.nameEN} joined the castle garrison!";
                    ShowFeedback(joinFeed);
                    SaveGameSystem.Save(0);
                };
                confirmPopupOpenedTime = Time.realtimeSinceStartup;
                showPurchaseConfirmPopup = true;
                if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
            }
        }
        GUILayout.EndHorizontal();
    }

    private Vector2 barracksScrollPos = Vector2.zero;

    private void DrawUnifiedBarracksSection(CastleInstance activeCastle, int curLang, float colWidth, GUIStyle subSt)
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(colWidth));

        GUIStyle colHeader1 = new GUIStyle(GUI.skin.box);
        colHeader1.alignment = TextAnchor.MiddleCenter;
        colHeader1.fontSize = 17;
        colHeader1.fontStyle = FontStyle.Bold;
        colHeader1.normal.textColor = new Color(0.95f, 0.45f, 0.45f);

        string barracksHeader = GetText9("⚔️ КАЗАРМЫ И ГАРНИЗОН", "⚔️ BARRACKS & GARRISON", "⚔️ KASERNE & GARNISON", "⚔️ CASERNE & GARNISON", "⚔️ CUARTEL Y GUARNICIÓN", "⚔️ QUARTEL E GUARNIÇÃO", "⚔️ 兵舎と要塞の駐屯軍", "⚔️ 병영 및 가리온", "⚔️ 军营与要塞驻军");
        GUILayout.Label(barracksHeader, colHeader1, GUILayout.Height(36));

        string bDesc = GetText9("Найм воинов и пополнение атакующего авангарда замка", "Recruit diverse cohorts & reinforce your fortress garrison", "Rekrutieren Sie Kohorten und verstärken Sie Ihre Garnison", "Recrutez des cohortes et renforcez votre garnison", "Recluta cohortes y refuerza la guarnición del castillo", "Recrute coortes e reforce a guarnição do castelo", "部隊を雇用し、要塞の駐屯軍を強化します", "다양한 부대를 모집하고 성의 주둔군을 강화하십시오", "招募各类型兵种并巩固城堡的守卫驻军");
        GUILayout.Label(bDesc, subSt);

        GUILayout.Space(8);

        barracksScrollPos = GUILayout.BeginScrollView(barracksScrollPos, GUILayout.Height(420));

        DrawUnitItem("warrior", "Боец фракции", "Faction Warrior", "皇室精锐战士", "왕실 정예 전사", 50, 1, activeCastle.level);
        DrawUnitItem("archer", "Эльфийский Лучник", "Elven Archer", "精灵神射手", "엘프 신궁 대원", 75, 1, activeCastle.level);
        DrawUnitItem("mage", "Боевой Маг Зенита", "Zenith Battle Mage", "제니스 전투 마법사", "제니스 전투 마법사", 120, 1, activeCastle.level);
        DrawUnitItem("paladin", "Паладин Света", "Holy Paladin", "圣光审判圣骑士", "성광의 발키리 기사", 200, 2, activeCastle.level);
        DrawUnitItem("cavalry", "Имперская Конница", "Imperial Cavalry", "帝国重装重骑兵", "황실 중갑 철기병", 320, 3, activeCastle.level);
        DrawUnitItem("cannoneer", "Осадно-боевой Пушкарь", "Garrison Cannoneer", "重击攻锤铁炮手", "공성 사격 철포병", 450, 4, activeCastle.level);
        DrawUnitItem("centaur", "Кентавр Степей", "Steppe Centaur", "荒野疾行百里人ма", "초원의 켄타우로스", 130, 5, activeCastle.level);
        DrawUnitItem("necromancer", "Некромант Тьмы", "Shadow Necromancer", "黑暗禁忌亡灵巫师", "어둠의 네크ро맨서", 260, 5, activeCastle.level);
        DrawUnitItem("griffin", "Элитный Королевский Грифон", "Royal Griffin", "皇家狮鹫守御猛禽", "황실 고대 그리폰", 380, 5, activeCastle.level);
        DrawUnitItem("overlord", "Рыцарь-Властелин", "Dread Overlord", "铁王座幽夜统治者", "공포의 지옥 영주", 680, 5, activeCastle.level);
        DrawUnitItem("hydra", "Многоголовая Гидра", "Swamp Hydra", "九头沼泽极冻毒蜃", "맹독의 아홉머리 히드라", 800, 5, activeCastle.level);
        DrawUnitItem("dragon", "Легендарный Дракон Пустоты", "Void Dragon", "虚空至尊不灭邪龙", "허공의 전설 고대 용", 1500, 6, activeCastle.level);
        DrawUnitItem("mountain_bear", "Ураганный Медведь Гор", "Mountain Bear Guard", "极寒高山怒嚎巨熊", "태산의 수호 거대 곰", 1000, 6, activeCastle.level);
        DrawUnitItem("wasteland_serpent", "Гигантская Змея Пустошей", "Wasteland Serpent", "荒原巨型暴食沙蟒", "황무지의 고대 거대 뱀", 1100, 6, activeCastle.level);

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }



    private void DrawPotionItem(string id, string nameRU, string nameEN, string nameCH, string nameKR, int basePrice, int level, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int reqLvl = level;
        bool isUnlocked = castleLvl >= reqLvl;

        int price = basePrice * level;
        
        string name = GetText9(nameRU, nameEN, "", "", "", "", "", nameKR, nameCH);
        if (id == "hp") {
            name = GetText9("Зелье Жизни", "Elixir of Vital Health", "Elixier der Vitalität", "Élixir de Santé", "Poción de Vida", "Elixir de Vida", "生命のエリクサー", "체력 신성 물약", "生命圣水");
        } else if (id == "str") {
            name = GetText9("Зелье Силы", "Potion of Giant Strength", "Trank der Riesenstärke", "Potion de Force de Géant", "Poción de Fuerza de Gigante", "Poção de Força de Gigante", "怪力のポーション", "거인의 괴력 물약", "巨人之力药水");
        } else if (id == "int") {
            name = GetText9("Зелье Интеллекта", "Potion of Mind Intelligence", "Trank der Gedankenintelligenz", "Potion d'Intelligence d'Esprit", "Poción de Inteligencia", "Poção de Inteligência", "知性のポーション", "지능 영약", "智力药水");
        } else if (id == "agi") {
            name = GetText9("Зелье Ловкости", "Potion of Swift Agility", "Trank der Schnelligkeit", "Potion d'Agilité Rapide", "Poción de Agilidad", "Poção de Agilidade", "敏捷のポーション", "민첩 영약", "敏捷药水");
        } else if (id == "sta") {
            name = GetText9("Зелье Выносливости", "Potion of Iron Stamina", "Trank der Eisenausdauer", "Potion d'Endurance de Fer", "Poción de Resistencia de Hierro", "Poção de Resistência de Ferro", "鉄のスタми나ポーション", "체력/지구력 영약", "耐力药水");
        }

        Texture2D potTex = GetPotionIconById(id);

        string colorTag = "<color=white>";
        if (level >= 9) colorTag = "<color=red>";
        else if (level >= 7) colorTag = "<color=orange>";
        else if (level >= 5) colorTag = "<color=magenta>";
        else if (level >= 3) colorTag = "<color=cyan>";
        else if (level >= 2) colorTag = "<color=green>";

        bool isHP = (id == "hp");
        int boost = GetPotionValueForLevel(level, isHP);
        string statDesc = "";
        if (isHP)
        {
            statDesc = GetText9(
                $"+{boost} к макс. ОЗ (HP)",
                $"+{boost} to Max HP",
                $"+{boost} zu max. HP",
                $"+{boost} aux PV max",
                $"+{boost} a PV Máx",
                $"+{boost} ao HP Máx",
                $"+{boost} 最大HP",
                $"+{boost} 최대 HP",
                $"+{boost} 最大生命值"
            );
        }
        else
        {
            string statName = id.ToUpper();
            statDesc = $"+{boost} {statName}";
        }

        GUILayout.BeginHorizontal(GUI.skin.box);

        GUILayout.BeginVertical(GUILayout.Width(58), GUILayout.Height(58));
        GUIStyle iconBtnStyle = new GUIStyle(GUI.skin.button);
        iconBtnStyle.padding = new RectOffset(2, 2, 2, 2);
        iconBtnStyle.fontSize = 24; // Более крупный эмодзи при отсутствии текстуры
        
        if (potTex != null)
        {
            GUILayout.Button(potTex, iconBtnStyle, GUILayout.Width(54), GUILayout.Height(54));
        }
        else
        {
            GUILayout.Button("🧪", iconBtnStyle, GUILayout.Width(54), GUILayout.Height(54));
        }
        GUILayout.EndVertical();

        Rect iconRect = GUILayoutUtility.GetLastRect();

        GUILayout.Space(8);

        GUILayout.BeginVertical();
        
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.richText = true;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.fontSize = 12;

        GUILayout.Label($"{colorTag}<b>{name} (Ур. {level})</b></color>", nameStyle);

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.fontSize = 10;
        descStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.Label(statDesc, descStyle);

        string reqText = GetText9(
            $"Требуемый уровень Замка: {reqLvl}",
            $"Required Castle Level: {reqLvl}",
            $"Erforderliche Burgstufe: {reqLvl}",
            $"Niveau de château requis : {reqLvl}",
            $"Nivel de castillo requerido: {reqLvl}",
            $"Nível de castelo necessário: {reqLvl}",
            $"必要な城のレベル: {reqLvl}",
            $"필요 성 레벨: {reqLvl}",
            $"需要城堡等级: {reqLvl}"
        );

        GUIStyle reqStyle = new GUIStyle(GUI.skin.label);
        reqStyle.fontSize = 10;
        reqStyle.normal.textColor = isUnlocked ? Color.green : Color.red;
        GUILayout.Label(reqText, reqStyle);

        GUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint && iconRect.Contains(Event.current.mousePosition))
        {
            isHoveringSkill = true;
            hoveredSkillName = $"{name} (Ур. {level})";
            
            string toolDesc = $"{statDesc}\n\n";
            if (isHP)
            {
                toolDesc += GetText9(
                    "Временно увеличивает Макс. ОЗ (HP) на <color=#33FF33>+" + boost + "</color> до конца текущего дня.",
                    "Temporarily increases Max HP by <color=#33FF33>+" + boost + "</color> until the end of the day.",
                    "Erhöht vorübergehend die max. HP um <color=#33FF33>+" + boost + "</color> bis zum Ende des Tages.",
                    "Augmente temporairement les PV max de <color=#33FF33>+" + boost + "</color> jusqu'à la fin de la journée.",
                    "Aumenta temporalmente los PV Máx en <color=#33FF33>+" + boost + "</color> hasta el final del día.",
                    "Aumenta temporariamente o HP Máximo em <color=#33FF33>+" + boost + "</color> até o fim do dia.",
                    "本日の終了まで一時的に最大HPを <color=#33FF33>+" + boost + "</color> 増加させます。",
                    "오늘 하루 동안 최대 HP를 <color=#33FF33>+" + boost + "</color>만큼 일시적으로 증가시킵니다.",
                    "临时增加最大生命值 (HP) <color=#33FF33>+" + boost + "</color>，持续到当天结束。"
                );
            }
            else if (id == "str")
            {
                toolDesc += GetText9(
                    "Временно увеличивает Силу на <color=#FF6600>+" + boost + " STR</color> до конца текущего дня.",
                    "Temporarily boosts Strength by <color=#FF6600>+" + boost + " STR</color> until the end of the day.",
                    "Erhöht vorübergehend die Stärke um <color=#FF6600>+" + boost + " STR</color> bis zum Ende des Tages.",
                    "Augmente temporairement la Force de <color=#FF6600>+" + boost + " STR</color> jusqu'à la fin de la journée.",
                    "Aumenta temporalmente la Fuerza en <color=#FF6600>+" + boost + " STR</color> hasta el final del día.",
                    "Aumenta temporariamente a Força em <color=#FF6600>+" + boost + " STR</color> até o fim do dia.",
                    "本日の終了まで一時的に筋力を <color=#FF6600>+" + boost + " STR</color> 増加させます。",
                    "오늘 하루 동안 힘을 <color=#FF6600>+" + boost + " STR</color>만큼 일시적으로 증가시킵니다.",
                    "临时增加力量 <color=#FF6600>+" + boost + " STR</color>，持续到当天结束。"
                );
            }
            else if (id == "int")
            {
                toolDesc += GetText9(
                    "Временно увеличивает Интеллект на <color=#CC33FF>+" + boost + " INT</color> до конца текущего дня.",
                    "Temporarily boosts Intelligence by <color=#CC33FF>+" + boost + " INT</color> until the end of the day.",
                    "Erhöht vorübergehend die Intelligenz um <color=#CC33FF>+" + boost + " INT</color> bis zum Ende des Tages.",
                    "Augmente temporairement l'Intelligence de <color=#CC33FF>+" + boost + " INT</color> jusqu'à la fin de la journée.",
                    "Aumenta temporalmente la Inteligencia en <color=#CC33FF>+" + boost + " INT</color> hasta el final del día.",
                    "Aumenta temporariamente a Inteligência em <color=#CC33FF>+" + boost + " INT</color> até o fim do dia.",
                    "本日の終了まで一時的に知性を <color=#CC33FF>+" + boost + " INT</color> 増加させます。",
                    "오늘 하루 동안 지능을 <color=#CC33FF>+" + boost + " INT</color>만큼 일시적으로 증가시킵니다.",
                    "临时增加智力 <color=#CC33FF>+" + boost + " INT</color>，持续到当天结束。"
                );
            }
            else if (id == "agi")
            {
                toolDesc += GetText9(
                    "Временно увеличивает Ловкость на <color=#33FF33>+" + boost + " AGI</color> до конца текущего дня.",
                    "Temporarily boosts Agility by <color=#33FF33>+" + boost + " AGI</color> until the end of the day.",
                    "Erhöht vorübergehend die Agilität um <color=#33FF33>+" + boost + " AGI</color> bis zum Ende des Tages.",
                    "Augmente temporairement l'Agilité de <color=#33FF33>+" + boost + " AGI</color> jusqu'à la fin de la journée.",
                    "Aumenta temporalmente la Agilidad en <color=#33FF33>+" + boost + " AGI</color> hasta el final del día.",
                    "Aumenta temporariamente a Agilidade em <color=#33FF33>+" + boost + " AGI</color> até o fim do dia.",
                    "本日の終了まで一時的に敏捷を <color=#33FF33>+" + boost + " AGI</color> 増加させます。",
                    "오늘 하루 동안 민첩을 <color=#33FF33>+" + boost + " AGI</color>만큼 일시적으로 증가시킵니다.",
                    "临时增加敏捷 <color=#33FF33>+" + boost + " AGI</color>，持续到当天结束。"
                );
            }
            else if (id == "sta")
            {
                toolDesc += GetText9(
                    "Временно увеличивает Выносливость на <color=#33CCFF>+" + boost + " STA</color> до конца текущего дня.",
                    "Temporarily boosts Stamina by <color=#33CCFF>+" + boost + " STA</color> until the end of the day.",
                    "Erhöht vorübergehend die Ausdauer um <color=#33CCFF>+" + boost + " STA</color> bis zum Ende des Tages.",
                    "Augmente temporairement l'Endurance de <color=#33CCFF>+" + boost + " STA</color> jusqu'à la fin de la journée.",
                    "Aumenta temporalmente la Resistencia en <color=#33CCFF>+" + boost + " STA</color> hasta el final del día.",
                    "Aumenta temporariamente a Resistência em <color=#33CCFF>+" + boost + " STA</color> até o fim do dia.",
                    "本日の終了まで一時的にスタミナを <color=#33CCFF>+" + boost + " STA</color> 増加させます。",
                    "오늘 하루 동안 지구력을 <color=#33CCFF>+" + boost + " STA</color>만큼 일시적으로 증가시킵니다.",
                    "临时增加耐力 <color=#33CCFF>+" + boost + " STA</color>，持续到当天结束。"
                );
            }
            
            string statusLine = GetText9(
                "<color=green>Доступно для покупки</color>",
                "<color=green>Available for purchase</color>",
                "<color=green>Zum Kauf verfügbar</color>",
                "<color=green>Disponible à l'achat</color>",
                "<color=green>Disponible para comprar</color>",
                "<color=green>Disponível para compra</color>",
                "<color=green>購入可能</color>",
                "<color=green>구매 가능</color>",
                "<color=green>可购买</color>"
            );
            string lockedLine = GetText9(
                "<color=red>Требуется Замок Ур." + reqLvl + "</color>",
                "<color=red>Requires Castle Level " + reqLvl + "</color>",
                "<color=red>Benötigt Burgstufe " + reqLvl + "</color>",
                "<color=red>Château niveau " + reqLvl + " requis</color>",
                "<color=red>Requiere nivel de castillo " + reqLvl + "</color>",
                "<color=red>Requer nível de castelo " + reqLvl + "</color>",
                "<color=red>必要な城のレベル: " + reqLvl + "</color>",
                "<color=red>필요 성 레벨: " + reqLvl + "</color>",
                "<color=red>需要城堡等级: " + reqLvl + "</color>"
            );
            toolDesc += "\n\n" + (isUnlocked ? statusLine : lockedLine);

            hoveredSkillDesc = toolDesc;
            hoveredSkillType = "Potion";
            hoveredSkillIcon = potTex;
        }

        GUILayout.FlexibleSpace();

        if (isUnlocked)
        {
            if (GUILayout.Button($"{price} 💰", GUILayout.Width(100), GUILayout.Height(35)))
            {
                int targetPrice = price;
                string targetId = id;
                int targetLevel = level;
                int targetBoost = boost;
                string targetNameRU = $"{nameRU} Ур.{level}";
                string targetName = name;
                string itemId = $"item_potion_{targetId}_level_{targetLevel}";
                
                if (!CanAddInventoryItem(itemId, 0, targetLevel))
                {
                    string pFail = GetText9(
                        "Ваш инвентарь переполнен! Освободите место.",
                        "Your inventory is full! Free some slots first.",
                        "Dein Inventar ist voll! Bitte machen Sie Platz.",
                        "Votre inventaire est plein ! Libérez de la place.",
                        "¡Tu inventario está lleno! Libera espacio.",
                        "Seu inventário está cheio! Libere espaço.",
                        "インベントリがいっぱいです！空きを作ってください。",
                        "인벤토리가 가득 찼습니다! 자리를 비워주십시오.",
                        "背包已满！请先清理背包。"
                    );
                    ShowFeedback(pFail);
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
                else
                {
                    confirmItemName = targetName;
                    confirmCost = price;
                    confirmAction = () => {
                        if (AddInventoryItem(itemId, targetNameRU, "potion", 0, targetLevel, targetBoost))
                        {
                            SaveGameSystem.CurrentData.gold -= targetPrice;
                            PlayerPrefs.Save();
                            SaveGameSystem.Save(0);
                            string pSucc = GetText9(
                                $"Куплено и помещено в инвентарь: {targetNameRU}!",
                                $"Purchased and placed in inventory: {targetName}!",
                                $"Gekauft und ins Inventar gelegt: {targetName}!",
                                $"Acheté et placé dans l'investaire : {targetName} !",
                                $"¡Comprado y colocado en el inventario: {targetName}!",
                                $"Comprado e colocado no inventário: {targetName}!",
                                $"購入してインベントリに入れました: {targetName}！",
                                $"구매하여 인벤토리에 보관함: {targetName}!",
                                $"购买并放入背包：{targetName}！"
                            );
                            ShowFeedback(pSucc);
                        }
                    };
                    confirmPopupOpenedTime = Time.realtimeSinceStartup;
                    showPurchaseConfirmPopup = true;
                    if (SettingsManager.Instance != null) SettingsManager.Instance.PlayHoverSound(0);
                }
            }
        }
        else
        {
            GUI.enabled = false;
            string lockLabel = GetText9(
                "Заперто 🔒",
                "Locked 🔒",
                "Gesperrt 🔒",
                "Verrouillé 🔒",
                "Bloqueado 🔒",
                "Bloqueado 🔒",
                "ロック 🔒",
                "잠김 🔒",
                "已锁定 🔒"
            );
            GUILayout.Button(lockLabel, GUILayout.Width(100), GUILayout.Height(35));
            GUI.enabled = true;
        }

        GUILayout.EndHorizontal();
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
    
    private Transform playerTransform;
    private bool isPlayerInside = false;
    private bool hasInitialized = false;

    private void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            if (LandingPositionManager.Instance != null && LandingPositionManager.Instance.playerTransform != null)
            {
                playerTransform = LandingPositionManager.Instance.playerTransform;
            }
            else
            {
                GameObject pObj = GameObject.Find("Player_Placeholder");
                if (pObj != null)
                {
                    playerTransform = pObj.transform;
                }
            }
        }

        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            if (isPlayerInside || !hasInitialized)
            {
                SetCastleTransparency(false);
                isPlayerInside = false;
                hasInitialized = true;
            }
            return;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        bool currentlyInside = false;
        if (col != null)
        {
            Vector3 localPlayerPos = col.transform.InverseTransformPoint(playerTransform.position);
            Vector3 min = col.center - col.size * 0.5f;
            Vector3 max = col.center + col.size * 0.5f;

            // Проверяем нахождение игрока внутри границ коллайдера замка (по осям X и Z)
            currentlyInside = (localPlayerPos.x >= min.x && localPlayerPos.x <= max.x) &&
                              (localPlayerPos.z >= min.z && localPlayerPos.z <= max.z);
        }
        else
        {
            Vector3 playerPos = playerTransform.position;
            Vector3 castlePos = transform.position;
            playerPos.y = 0;
            castlePos.y = 0;
            currentlyInside = Vector3.Distance(playerPos, castlePos) < 1.5f;
        }

        if (currentlyInside != isPlayerInside || !hasInitialized)
        {
            isPlayerInside = currentlyInside;
            SetCastleTransparency(currentlyInside);
            hasInitialized = true;
        }
    }

    private void SetCastleTransparency(bool makeTransparent)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r != null && r.gameObject.name != "Collider_Visualizer")
            {
                // Использование r.material клонирует инстанс материала, что позволяет менять прозрачность индивидуально для каждого замка
                SetMaterialTransparent(r.material, makeTransparent);
            }
        }
    }

    private void SetMaterialTransparent(Material mat, bool transparent)
    {
        if (mat == null) return;

        if (transparent)
        {
            mat.SetFloat("_Mode", 3); // Стандартный шейдер (Transparent)
            
            // Поддержка URP Lit Shader
            mat.SetFloat("_Surface", 1); // 1 = Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            Color col = mat.color;
            col.a = 0.35f; // Красивая полупрозрачность
            mat.color = col;
        }
        else
        {
            mat.SetFloat("_Mode", 0); // Стандартный шейдер (Opaque)
            
            // Поддержка URP Lit Shader
            mat.SetFloat("_Surface", 0); // 0 = Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            
            Color col = mat.color;
            col.a = 1.0f; // Полностью непрозрачный
            mat.color = col;
        }
    }

    public void OpenSkillDetailPopup(string sName, string sDesc, Texture2D icon, string skillType)
    {
        selectedSkillName = sName;
        selectedSkillDesc = sDesc;
        selectedSkillIcon = icon;
        selectedSkillType = skillType;
        showSkillDetailPopup = true;
    }

    private void LoadClassSkillsIcons()
    {
        string cl = "warrior";
        if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            cl = SaveGameSystem.CurrentData.characterClass.ToLower();
        }

        if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("стрелок"))
        {
            activeSkillPassive1 = archerSkillPassive1;
            activeSkillPassive2 = archerSkillPassive2;
            activeSkillPassive3 = archerSkillPassive3;
            activeSkillUltimate = archerSkillUltimate;
        }
        else if (cl.Contains("mage") || cl.Contains("mag") || cl.Contains("маг"))
        {
            activeSkillPassive1 = mageSkillPassive1;
            activeSkillPassive2 = mageSkillPassive2;
            activeSkillPassive3 = mageSkillPassive3;
            activeSkillUltimate = mageSkillUltimate;
        }
        else
        {
            activeSkillPassive1 = warriorSkillPassive1;
            activeSkillPassive2 = warriorSkillPassive2;
            activeSkillPassive3 = warriorSkillPassive3;
            activeSkillUltimate = warriorSkillUltimate;
        }
    }

    private void SetHoveredSkill(int skillIndex, int curLang)
    {
        string cl = "warrior";
        if (SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
        {
            cl = SaveGameSystem.CurrentData.characterClass.ToLower();
        }

        bool isArcher = cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("стрелок");
        bool isMage = cl.Contains("mage") || cl.Contains("mag") || cl.Contains("маг");

        if (isArcher)
        {
            if (skillIndex == 1)
            {
                hoveredSkillName = curLang == 0 ? "Ветряной Щит" : "Wind Barrier";
                hoveredSkillDesc = curLang == 0 ? "Ветряной барьер отклоняет стрелы и увеличивает уклонение на 15%." : "Wind barrier deflects incoming arrows and grants +15% evasion.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = archerSkillPassive1;
            }
            else if (skillIndex == 2)
            {
                hoveredSkillName = curLang == 0 ? "Критическая Метка" : "Critical Mark";
                hoveredSkillDesc = curLang == 0 ? "Помечает цели, увеличивая шанс критического урона союзников на 10%." : "Marks target to increase critical chance for all allies by +10%.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = archerSkillPassive2;
            }
            else if (skillIndex == 3)
            {
                hoveredSkillName = curLang == 0 ? "Орлиный Взгляд" : "Eagle Eye";
                hoveredSkillDesc = curLang == 0 ? "Увеличивает дальность стрельбы на 20% и точность попадания." : "Increases attack range by +20% and critical strike accuracy.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = archerSkillPassive3;
            }
            else if (skillIndex == 4)
            {
                hoveredSkillName = curLang == 0 ? "Стрела Затмения" : "Eclipse Arrow";
                hoveredSkillDesc = curLang == 0 ? "Суперудар: Выпускает сокрушительную стрелу, пробивающую броню на 50%." : "Ultimate: Fires a piercing shadow arrow that ignores 50% of armor.";
                hoveredSkillType = "Ultimate";
                hoveredSkillIcon = archerSkillUltimate;
            }
        }
        else if (isMage)
        {
            if (skillIndex == 1)
            {
                hoveredSkillName = curLang == 0 ? "Щит Возмездия" : "Shield of Retribution";
                hoveredSkillDesc = curLang == 0 ? "Возвращает 15% полученного урона в виде электрического разряда." : "Surrounds hero with plasma reflecting 15% of damage back to attackers.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = mageSkillPassive1;
            }
            else if (skillIndex == 2)
            {
                hoveredSkillName = curLang == 0 ? "Ткач Заклинаний" : "Spellweaver";
                hoveredSkillDesc = curLang == 0 ? "Снижает расход маны на все заклинания на 20%." : "Reduces mana consumption for all active wizard spells by 20%.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = mageSkillPassive2;
            }
            else if (skillIndex == 3)
            {
                hoveredSkillName = curLang == 0 ? "Источник Разума" : "Mind Spring";
                hoveredSkillDesc = curLang == 0 ? "Каждый ход восстанавливает 8 единиц маны и увеличивает интеллект." : "Generates +8 Mana per battle turn and increases spell power.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = mageSkillPassive3;
            }
            else if (skillIndex == 4)
            {
                hoveredSkillName = curLang == 0 ? "Метеоритный Дождь" : "Meteor Storm";
                hoveredSkillDesc = curLang == 0 ? "Суперудар: Призывает огненные метеоры, наносящие огромный массовый урон." : "Ultimate: Calls down a cosmic meteor shower dealing massive AoE spell damage.";
                hoveredSkillType = "Ultimate";
                hoveredSkillIcon = mageSkillUltimate;
            }
        }
        else // Warrior
        {
            if (skillIndex == 1)
            {
                hoveredSkillName = curLang == 0 ? "Закалка Металла" : "Metal Tempering";
                hoveredSkillDesc = curLang == 0 ? "Повышает показатель брони героя на 15% и дает устойчивость к оглушению." : "Increases physical armor rating by 15% and grants stun resistance.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = warriorSkillPassive1;
            }
            else if (skillIndex == 2)
            {
                hoveredSkillName = curLang == 0 ? "Брат Гвардии" : "Brotherhood of Guard";
                hoveredSkillDesc = curLang == 0 ? "Повышает защиту стоящих рядом союзных воинов на 10%." : "Increases armor of adjacent friendly infantry cohorts by 10%.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = warriorSkillPassive2;
            }
            else if (skillIndex == 3)
            {
                hoveredSkillName = curLang == 0 ? "Угроза" : "Threat";
                hoveredSkillDesc = curLang == 0 ? "Ускоряет накопление боевого духа и провокацию на 10%." : "Increases threat generation and physical action point speed by 10%.";
                hoveredSkillType = "Passive";
                hoveredSkillIcon = warriorSkillPassive3;
            }
            else if (skillIndex == 4)
            {
                hoveredSkillName = curLang == 0 ? "Щит Титанов" : "Titan Shield";
                hoveredSkillDesc = curLang == 0 ? "Суперудар: Снижает входящий физический урон на 70%." : "Ultimate: Activates indestructible titan wall blocking 70% of physical dmg.";
                hoveredSkillType = "Ultimate";
                hoveredSkillIcon = warriorSkillUltimate;
            }
        }
    }

    private void SetHoveredItem(InventoryItem item, int curLang)
    {
        if (item == null) return;

        hoveredSkillName = item.name;
        hoveredSkillIcon = GetItemIconTexture(item);

        if (item.slotType == 0) // Potion
        {
            hoveredSkillType = "Potion";
            string statName = "HP";
            string idLower = item.id.ToLower();
            if (idLower.Contains("str") || idLower.Contains("силы")) statName = curLang == 0 ? "СИЛА" : "STR";
            else if (idLower.Contains("int") || idLower.Contains("инт")) statName = curLang == 0 ? "ИНТЕЛЛЕКТ" : "INT";
            else if (idLower.Contains("agi") || idLower.Contains("ловк")) statName = curLang == 0 ? "ЛОВКОСТЬ" : "AGI";
            else if (idLower.Contains("sta") || idLower.Contains("вынос") || idLower.Contains("def") || idLower.Contains("защит")) statName = curLang == 0 ? "ВЫНОСЛИВОСТЬ" : "STA";

            hoveredSkillDesc = curLang == 0 
                ? $"Эликсир. Дарует временное усиление (+{item.statBonus} {statName}) на один бой.\nКоличество в инвентаре: {item.count}."
                : $"Elixir. Grants temporary battle enhancement (+{item.statBonus} {statName}) for one fight.\nIn possession: {item.count}.";
        }
        else // Gear
        {
            string slotName = "Equipment";
            switch (item.slotType)
            {
                case 1: slotName = curLang == 0 ? "Шлем (Голова)" : "Helmet (Head Slot)"; break;
                case 2: slotName = curLang == 0 ? "Амулет (Шея)" : "Amulet (Neck Slot)"; break;
                case 3: slotName = curLang == 0 ? "Наплечники" : "Pauldrons (Shoulders Slot)"; break;
                case 4: slotName = curLang == 0 ? "Доспех (Грудь)" : "Armor (Chest Slot)"; break;
                case 5: slotName = curLang == 0 ? "Кольцо" : "Ring Slot"; break;
                case 6: slotName = curLang == 0 ? "Пояс" : "Belt Slot"; break;
                case 7: slotName = curLang == 0 ? "Сапоги (Ноги)" : "Boots (Feet Slot)"; break;
                case 8: slotName = curLang == 0 ? "Оружие" : "Weapon Slot"; break;
            }

            hoveredSkillType = $"🛡️ {slotName.ToUpper()} (TIER {item.level})";
            hoveredSkillDesc = curLang == 0
                ? $"Снаряжение {item.level}-го ранга.\nПостоянный бонус характеристик: +{item.statBonus} к основному показателю класса при надевании."
                : $"Tier {item.level} gear.\nPermanent stat modifier: +{item.statBonus} to class primary attribute while equipped.";
        }
    }
}
