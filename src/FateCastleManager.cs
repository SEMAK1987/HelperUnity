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
    
    [Tooltip("Manual 3D positions for the 4 castles (0: Wastes, 1: Peak, 2: Ruins, 3: Zenith). Default is matched to continent aesthetics")]
    public Vector3[] customCastlePositions = new Vector3[4]
    {
        new Vector3(-5.3f, -0.4f, 4.2f),   // Кровавые Пустоши
        new Vector3(14.8f, 1.2f, 12.5f),   // Ледяной Пик
        new Vector3(-12.4f, -0.3f, -10.2f), // Древние Руины
        new Vector3(6.5f, 0.8f, -4.5f)     // Святилище Зенита
    };

    [Tooltip("Manual offset added to the spawn anchor of each landing point if not using customCastlePositions")]
    public Vector3[] castleManualOffsets = new Vector3[4]
    {
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f),
        new Vector3(3.2f, 0f, 3.2f)
    };

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
    
    // UI states and trackers
    public bool isTownViewActive = false;
    public bool isAutonomousStatsDistribution = false;
    private bool showStatsPanel = false;
    private bool isDetailsOpen = false;
    private int activeDetailsIndex = -1;
    private string feedbackMessage = "";
    private float messageTimer = 0f;
    
    public int currentDay = 1;

    // AI notification logs shown during new-day transition
    private List<string> aiLogs = new List<string>();
    private bool showNewDayOverlay = false;
    private float overlayTimer = 0f;

    // Scroll vectors for columns
    private Vector2 barracksScroll = Vector2.zero;
    private Vector2 forgeScroll = Vector2.zero;
    private Vector2 academyScroll = Vector2.zero;

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
        int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
        if (landedZone == zoneIndex)
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
        switch (zoneIndex)
        {
            case 0:
                return lang == 0 ? "Орки Кровавых Пустошей" : "Wasteland Orcs";
            case 1:
                return lang == 0 ? "Владыки Ледяного Пика" : "Frost Peak Overlords";
            case 2:
                return lang == 0 ? "Дикари Древних Руин" : "Ancient Ruins Savages";
            case 3:
                return lang == 0 ? "Небожители Сакрального Зенита" : "Sacred Zenith Celestials";
            default:
                return lang == 0 ? "Имперский Альянс" : "Imperial Alliance";
        }
    }

    // ZONE SPECIFIC HERO & UNIT PERSISTENCE (MIGRATION INCLUDED)
    public int GetHeroCount(string key, int zoneIndex)
    {
        string zoneKey = "Player_HiredCount_" + key + "_Zone_" + zoneIndex;
        if (!PlayerPrefs.HasKey(zoneKey))
        {
            int oldVal = PlayerPrefs.GetInt("Player_HiredCount_" + key, 0);
            int mainZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
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
            int mainZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
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

    private void Awake()
    {
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

    private void Start()
    {
        isContinentGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1;
        currentDay = PlayerPrefs.GetInt("Fate_Current_Day", initialDaySetting);
        if (isContinentGameplayActive)
        {
            SpawnAllCastles();
        }
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
        
        PlayerPrefs.SetInt("Fate_Current_Day", initialDaySetting);
        currentDay = initialDaySetting;
        
        SaveGameSystem.CurrentData.gold = initialGoldSetting;

        // Remove spawned castles if any
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].visualRoot != null)
            {
                Destroy(castles[i].visualRoot);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log($"[CASTLE MGR] Сброс параметров кампании: День={initialDaySetting}, Золото={initialGoldSetting}");
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

        // High gloss rotations on active landmarks
        RotateCastleGems();
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
        // Avoid clicking 3D structures if continent gameplay is not fully active, town view is active, or day overlay is showing
        if (!isContinentGameplayActive || isTownViewActive || showNewDayOverlay) return;

        if (WasLeftMouseButtonClicked())
        {
            if (Camera.main != null)
            {
                Vector2 mousePos = GetMousePosition();
                // Ensure we don't click on GUI Windows
                if (isDetailsOpen)
                {
                    // GUI coordinates are Y-down, but screen coordinates are Y-up.
                    // Keep clicking robust: if click is in the lower/upper right pane or middle details rect, ignore 3D raycast.
                    float panelWidth = 360f;
                    float panelHeight = 520f;
                    float px = Screen.width - panelWidth - 30f;
                    float py = 30f; // from top
                    Rect guiRect = new Rect(px, py, panelWidth, panelHeight);
                    // Match screen pos (Y up) to GUI pos (Y down)
                    Vector2 guiMouse = new Vector2(mousePos.x, Screen.height - mousePos.y);
                    if (guiRect.Contains(guiMouse))
                    {
                        return; // Clicked inside Details panel, ignore raycast
                    }
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

    private void InitializeCastleStates()
    {
        castles.Clear();
        currentDay = PlayerPrefs.GetInt("Fate_Current_Day", 1);

        string[] zonesRU = { "Кровавые Пустоши", "Ледяной Пик", "Древние Руины", "Святилище Зенита" };
        string[] zonesEN = { "Crimson Wastes", "Ice-Bound Peak", "Ancient Ruins", "Zenith Sanctuary" };
        string[] zonesCH = { "血红荒原", "冰封之巅", "古代废墟", "巅峰避难所" };
        string[] zonesKR = { "붉은 황무지", "얼음 봉우리", "고대 유적지", "제니스 성소" };

        for (int i = 0; i < 4; i++)
        {
            CastleInstance castle = new CastleInstance
            {
                zoneIndex = i,
                nameRU = zonesRU[i],
                nameEN = zonesEN[i],
                nameCH = zonesCH[i],
                nameKR = zonesKR[i],
                level = PlayerPrefs.GetInt("Castle_Level_" + i, 1),
                owner = PlayerPrefs.GetString("Castle_Owner_" + i, i == 0 ? "Player" : "Enemy") // Auto start Player in first zone
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
        int playerZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
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

        for (int i = 0; i < castles.Count; i++)
        {
            CastleInstance castle = castles[i];
            
            // Синхронизация роли
            if (i == playerZone)
            {
                castle.owner = "Player";
            }
            else
            {
                castle.owner = "Enemy";
            }
            PlayerPrefs.SetString("Castle_Owner_" + i, castle.owner);
            PlayerPrefs.Save();

            if (castle.visualRoot != null)
            {
                Destroy(castle.visualRoot);
            }

            Vector3 spawnPos;
            if (useManualCastlePositions)
            {
                if (customCastlePositions != null && i < customCastlePositions.Length)
                {
                    spawnPos = customCastlePositions[i];
                }
                else
                {
                    spawnPos = new Vector3((i - 1.5f) * 15f, 0f, 0f);
                }
            }
            else
            {
                if (i < lpm.landingPoints.Length && lpm.landingPoints[i].spawnAnchor != null)
                {
                    Transform anchor = lpm.landingPoints[i].spawnAnchor;
                    Vector3 offset = (castleManualOffsets != null && i < castleManualOffsets.Length) ? castleManualOffsets[i] : new Vector3(3.2f, 0f, 3.2f);
                    spawnPos = anchor.position + offset;
                }
                else
                {
                    // НАДЁЖНЫЙ РЕЗЕРВНЫЙ ФОЛЛБЕК: Если физический анкер стёрт или отсутствует в иерархии Unity,
                    // мы выставляем безопасные 3D-координаты. Замки НИКОГДА больше не пропадают после пропуска хода (AdvanceDay)!
                    spawnPos = new Vector3((i - 1.5f) * 18f, 1.2f, (i % 2 == 0 ? 8f : -8f));
                    Debug.LogWarning($"[CASTLE MGR] Точка привязки для замка '{castle.nameRU}' не задана. Использована резервная 3D координата: {spawnPos}");
                }
            }

            // Проецирование на террейн заземленно
            if (snapCastlesToTerrain)
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
            col.center = new Vector3(0f, 2.5f, 0f);
            col.size = new Vector3(4.5f, 6.0f, 4.5f);

            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
            Material castleMat = new Material(urpShader);
            castleMat.color = castle.owner == "Player" ? new Color(0.12f, 0.88f, 0.45f, 1.0f) : new Color(0.92f, 0.12f, 0.28f, 1.0f);

            if (castleMat.HasProperty("_Glossiness")) castleMat.SetFloat("_Glossiness", 0.7f);
            if (castleMat.HasProperty("_Smoothness")) castleMat.SetFloat("_Smoothness", 0.7f);
            if (castleMat.HasProperty("_Metallic")) castleMat.SetFloat("_Metallic", 0.45f);

            // МОРФИНГ ФОРМ только для активного замка Игрока!
            if (castle.owner == "Player")
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
        if (cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("paladin"))
        {
            startSTR = 15;
            startAGI = 10;
            startINT = 4;
            startSTA = 15;
        }
        else if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow"))
        {
            startSTR = 10;
            startAGI = 14;
            startINT = 6;
            startSTA = 11;
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff"))
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

        GUI.backgroundColor = new Color(0.1f, 0.65f, 0.95f, 1.0f);
        if (GUI.Button(new Rect(Screen.width - 240f, 107f, 220f, 44f), $"▶ {nextDayBtnText}", nextDayStyle))
        {
            AdvanceDay();
        }
        GUI.backgroundColor = Color.white;

        // 4. Отрисовка ГЕРОЯ И ЕГО ХАРАКТЕРИСТИК (HUD в верхнем левом углу)
        DrawHeroHUD(curLang);

        // Overlay нового дня (ИИ отчеты)
        if (showNewDayOverlay)
        {
            DrawNewDayOverlay(curLang);
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
        hudBgStyle.normal.background = null; // убираем стандартную заливку для кастомного цвета
        
        // Фон плашки: темно-синий глянцевый полупрозрачный
        Texture2D hudTex = new Texture2D(1, 1);
        hudTex.SetPixel(0, 0, new Color(0.04f, 0.08f, 0.22f, 0.90f));
        hudTex.Apply();
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
        Texture2D barBgTex = new Texture2D(1, 1);
        barBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.6f));
        barBgTex.Apply();
        barBgStyle.normal.background = barBgTex;
        
        GUIStyle hpStyle = new GUIStyle(GUI.skin.box);
        Texture2D hpTex = new Texture2D(1, 1);
        hpTex.SetPixel(0, 0, new Color(0.85f, 0.15f, 0.2f, 1.0f));
        hpTex.Apply();
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
        Texture2D mpTex = new Texture2D(1, 1);
        mpTex.SetPixel(0, 0, new Color(0.12f, 0.5f, 0.95f, 1.0f));
        mpTex.Apply();
        mpStyle.normal.background = mpTex;
        
        GUI.Box(new Rect(110f, 65f, 230f, 13f), "", barBgStyle);
        GUI.Box(new Rect(110f, 65f, 230f * manaPct, 13f), "", mpStyle);
        GUI.Label(new Rect(110f, 64f, 230f, 13f), $"MP: {maxMana} / {maxMana}", textOverBarStyle);
        
        // 3. Опыт (Яркий неоново-бирюзовый цвет)
        int xpNeeded = data.playerLevel * 100;
        float xpPct = xpNeeded > 0 ? Mathf.Clamp01((float)data.currentXP / xpNeeded) : 0f;
        
        GUIStyle xpStyle = new GUIStyle(GUI.skin.box);
        Texture2D xpTex = new Texture2D(1, 1);
        xpTex.SetPixel(0, 0, new Color(0.05f, 0.85f, 0.65f, 1.0f));
        xpTex.Apply();
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

    private void DrawStatsAllocationPanel(int curLang, Texture2D winBgTex, GUIStyle barBgStyle)
    {
        SaveGameSystem.SaveData data = SaveGameSystem.CurrentData;
        
        // Определяем базовые стартовые характеристики в зависимости от веток
        int startSTR = 10, startAGI = 10, startINT = 10, startSTA = 10;
        string cl = (data != null && !string.IsNullOrEmpty(data.characterClass)) ? data.characterClass.ToLower() : "воин";
        if (cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("paladin"))
        {
            startSTR = 15; startAGI = 10; startINT = 4; startSTA = 15;
        }
        else if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow"))
        {
            startSTR = 10; startAGI = 14; startINT = 6; startSTA = 11;
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff"))
        {
            startSTR = 6; startAGI = 10; startINT = 10; startSTA = 9;
        }

        GUIStyle winStyle = new GUIStyle(GUI.skin.box);
        winStyle.normal.background = winBgTex;
        
        // Увеличили высоту панели, чтобы добавить раздел скиллов и пассивок класса
        Rect winRect = new Rect(20f, 130f, 330f, 575f);
        GUI.Box(winRect, "", winStyle);
        
        GUILayout.BeginArea(winRect);
        GUILayout.Space(12);
        
        // Заголовок панели
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.cyan;
        
        string headText = curLang == 0 ? "⚡ ХАРАКТЕРИСТИКИ ГЕРОЯ" : "⚡ HERO CHARACTERISTICS";
        if (curLang == 8) headText = "⚡ 英雄属性星盘配点";
        if (curLang == 7) headText = "⚡ 영웅 능력치 통계 제어";
        GUILayout.Label(headText, headerStyle);
        GUILayout.Space(8);
        
        // Переключатель автономного авто-распределения
        bool oldAuto = isAutonomousStatsDistribution;
        string autoLabel = curLang == 0 ? "🤖 Авто-распределение очков" : "🤖 Autonomous Allocation";
        if (curLang == 8) autoLabel = "🤖 智能AI自动加配属性点";
        if (curLang == 7) autoLabel = "🤖 인공지능 능력치 자동 배포";
        
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
        
        GUILayout.Space(12);
        
        // Линии атрибутов с защитой от спуска характеристик ниже стартовых значений класса
        DrawStatRow(curLang, "🔥", curLang == 0 ? "Сила (STR)" : "Strength (STR)", ref data.strength, ref data.availableSkillPoints, startSTR);
        DrawStatRow(curLang, "⚡", curLang == 0 ? "Ловкость (AGI)" : "Agility (AGI)", ref data.agility, ref data.availableSkillPoints, startAGI);
        DrawStatRow(curLang, "🔮", curLang == 0 ? "Интеллект (INT)" : "Intelligence (INT)", ref data.intelligence, ref data.availableSkillPoints, startINT);
        DrawStatRow(curLang, "💚", curLang == 0 ? "Выносливость (STA)" : "Stamina (STA)", ref data.stamina, ref data.availableSkillPoints, startSTA);
        
        GUILayout.Space(8);
        
        // Свободные очки
        GUIStyle pointsStyle = new GUIStyle(GUI.skin.label);
        pointsStyle.alignment = TextAnchor.MiddleCenter;
        pointsStyle.fontSize = 13;
        pointsStyle.fontStyle = FontStyle.Bold;
        pointsStyle.normal.textColor = data.availableSkillPoints > 0 ? new Color(1.0f, 0.64f, 0.0f) : Color.gray;
        
        string pointsLabel = curLang == 0 ? "Свободные очки: " : "Unassigned Points: ";
        if (curLang == 8) pointsLabel = "未分配属性星能点: ";
        if (curLang == 7) pointsLabel = "남은 속성 수치 점수: ";
        GUILayout.Label($"{pointsLabel}{data.availableSkillPoints}", pointsStyle);
        GUILayout.Space(6);
        
        // Вычисляемые боевые параметры
        float combatAtk = data.strength * 2.5f + data.agility * 0.5f;
        float combatDef = data.agility * 1.5f + data.strength * 0.5f;
        float maxHp = data.stamina * 10f;
        float maxMp = data.intelligence * 10f;
        
        GUIStyle derivedStyle = new GUIStyle(GUI.skin.box);
        derivedStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);
        derivedStyle.fontSize = 11;
        derivedStyle.alignment = TextAnchor.MiddleLeft;
        derivedStyle.padding = new RectOffset(12, 12, 6, 6);
        
        string statsReport = curLang == 0 
            ? $"⚔️ Базовая Атака: {combatAtk}\n🛡️ Защита брони: {combatDef}\n❤️ Макс. ОЗ (HP): {maxHp}\n🔮 Макс. ОМ (MP): {maxMp}"
            : $"⚔️ Combat Damage: {combatAtk}\n🛡️ Armor Defense: {combatDef}\n❤️ Max Health (HP): {maxHp}\n🔮 Max Mana (MP): {maxMp}";
            
        GUILayout.Label(statsReport, derivedStyle);
        GUILayout.Space(10);
        
        // 🧬 ДОПОЛНИТЕЛЬНЫЙ КЛАССОВЫЙ БЛОК НАВЫКОВ
        GUIStyle skillsHeaderStyle = new GUIStyle(GUI.skin.label);
        skillsHeaderStyle.alignment = TextAnchor.MiddleCenter;
        skillsHeaderStyle.fontSize = 12;
        skillsHeaderStyle.fontStyle = FontStyle.Bold;
        skillsHeaderStyle.normal.textColor = Color.yellow;
        
        string skillsTitle = curLang == 0 ? "🧬 КЛАССОВЫЕ НАВЫКИ ГЕРОЯ" : "🧬 HERO CLASS SKILLS";
        GUILayout.Label(skillsTitle, skillsHeaderStyle);
        GUILayout.Space(4);
        
        string skillsBody = "";
        if (cl.Contains("warrior") || cl.Contains("voin") || cl.Contains("paladin"))
        {
            skillsBody = curLang == 0
                ? "<b>Пассивные</b>:\n • IronSkin (Прочная кожа: +15% Защиты)\n • Regen (Восстановление: +5 HP за ход)\n • Threat (Угроза: +10% аггро)\n<b>Суперудар</b>: TitanShield (Перезарядка: 4х, Сила: x0.3, снижение урона на 70%)"
                : "<b>Passives</b>:\n • IronSkin (+15% Defense bonus)\n • Regen (+5 HP per turn gain)\n • Threat (+10% threat level)\n<b>Ultimate</b>: TitanShield (CD: 4t, Power: x0.3, blocks 70% of incoming damage)";
        }
        else if (cl.Contains("archer") || cl.Contains("strelok") || cl.Contains("ranger") || cl.Contains("bow"))
        {
            skillsBody = curLang == 0
                ? "<b>Пассивные</b>:\n • Крит-Мастер (+15% шанс крита)\n • LongShot (Дальний выстрел: +10% урона)\n • Evasion (Уклонение: +10% уворота)\n<b>Суперудар</b>: Ливень Смерти (Перезарядка: 3х, Сила: x1.8 по площади)"
                : "<b>Passives</b>:\n • Crit Master (+15% Critical Chance)\n • LongShot (+10% damage over range)\n • Evasion (+10% dodge rate)\n<b>Ultimate</b>: Death Rain (CD: 3t, Power: x1.8 AoE damage)";
        }
        else if (cl.Contains("mage") || cl.Contains("wizard") || cl.Contains("mag") || cl.Contains("staff"))
        {
            skillsBody = curLang == 0
                ? "<b>Пассивные</b>:\n • ManaFlow (Поток маны: +5 MP за ход)\n • Elemental (Стихии: +15% маг. урона)\n • Resist (Сопротивление: +15% маг. деф)\n<b>Суперудар</b>: TimeRift (Перезарядка: 4х, Сила: x0, замедляет врагов на 2 хода)"
                : "<b>Passives</b>:\n • ManaFlow (+5 MP regain per turn)\n • Elemental (+15% elemental spell power)\n • Resist (+15% magic resist)\n<b>Ultimate</b>: Time Rift (CD: 4t, Power: x0, slows down enemies for 2 turns)";
        }
        else
        {
            skillsBody = curLang == 0 ? "Фирменные навыки вашего класса будут доступны на тактической арене." : "Signature skills for your character class are active on the tactical map.";
        }
        
        GUIStyle skillsBodyStyle = new GUIStyle(GUI.skin.box);
        skillsBodyStyle.normal.textColor = new Color(0.9f, 0.95f, 1.0f);
        skillsBodyStyle.fontSize = 10;
        skillsBodyStyle.alignment = TextAnchor.MiddleLeft;
        skillsBodyStyle.padding = new RectOffset(10, 10, 6, 6);
        
        GUILayout.Label(skillsBody, skillsBodyStyle);
        GUILayout.Space(8);
        
        // Панель управления (Сброс и Тестовые читы)
        GUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(1.0f, 0.22f, 0.22f);
        string resetBtnLabel = curLang == 0 ? "СБРОС" : "RESET";
        if (GUILayout.Button($"♻️ {resetBtnLabel}", GUILayout.Height(30)))
        {
            ResetPlayerStats();
        }
        
        GUI.backgroundColor = new Color(0.15f, 0.8f, 0.35f);
        string addXpText = curLang == 0 ? "ОПЫТ +50" : "+50 XP";
        if (GUILayout.Button($"✨ {addXpText}", GUILayout.Height(30)))
        {
            GainXP(50);
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        GUILayout.EndArea();
    }xt = "⚡ 英雄属性星盘配点";
        if (curLang == 7) headText = "⚡ 영웅 능력치 통계 제어";
        GUILayout.Label(headText, headerStyle);
        GUILayout.Space(8);
        
        // Переключатель автономного авто-распределения
        bool oldAuto = isAutonomousStatsDistribution;
        string autoLabel = curLang == 0 ? "🤖 Авто-распределение очков" : "🤖 Autonomous Allocation";
        if (curLang == 8) autoLabel = "🤖 智能AI自动加配属性点";
        if (curLang == 7) autoLabel = "🤖 인공지능 능력치 자동 배포";
        
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
        
        GUILayout.Space(12);
        
        // Линии атрибутов
        DrawStatRow(curLang, "🔥", curLang == 0 ? "Сила (STR)" : "Strength (STR)", ref data.strength, ref data.availableSkillPoints);
        DrawStatRow(curLang, "⚡", curLang == 0 ? "Ловкость (AGI)" : "Agility (AGI)", ref data.agility, ref data.availableSkillPoints);
        DrawStatRow(curLang, "🔮", curLang == 0 ? "Интеллект (INT)" : "Intelligence (INT)", ref data.intelligence, ref data.availableSkillPoints);
        DrawStatRow(curLang, "💚", curLang == 0 ? "Выносливость (STA)" : "Stamina (STA)", ref data.stamina, ref data.availableSkillPoints);
        
        GUILayout.Space(8);
        
        // Свободные очки
        GUIStyle pointsStyle = new GUIStyle(GUI.skin.label);
        pointsStyle.alignment = TextAnchor.MiddleCenter;
        pointsStyle.fontSize = 13;
        pointsStyle.fontStyle = FontStyle.Bold;
        pointsStyle.normal.textColor = data.availableSkillPoints > 0 ? new Color(1.0f, 0.64f, 0.0f) : Color.gray;
        
        string pointsLabel = curLang == 0 ? "Свободные очки: " : "Unassigned Points: ";
        if (curLang == 8) pointsLabel = "未分配属性星能点: ";
        if (curLang == 7) pointsLabel = "남은 속성 수치 점수: ";
        GUILayout.Label($"{pointsLabel}{data.availableSkillPoints}", pointsStyle);
        GUILayout.Space(6);
        
        // Вычисляемые боевые параметры
        float combatAtk = data.strength * 2.5f + data.agility * 0.5f;
        float combatDef = data.agility * 1.5f + data.strength * 0.5f;
        float maxHp = data.stamina * 10f;
        float maxMp = data.intelligence * 10f;
        
        GUIStyle derivedStyle = new GUIStyle(GUI.skin.box);
        derivedStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);
        derivedStyle.fontSize = 11;
        derivedStyle.alignment = TextAnchor.MiddleLeft;
        derivedStyle.padding = new RectOffset(12, 12, 6, 6);
        
        string statsReport = curLang == 0 
            ? $"⚔️ Базовая Атака: {combatAtk}\n🛡️ Защита брони: {combatDef}\n❤️ Макс. ОЗ (HP): {maxHp}\n🔮 Макс. ОМ (MP): {maxMp}"
            : $"⚔️ Combat Damage: {combatAtk}\n🛡️ Armor Defense: {combatDef}\n❤️ Max Health (HP): {maxHp}\n🔮 Max Mana (MP): {maxMp}";
            
        GUILayout.Label(statsReport, derivedStyle);
        GUILayout.Space(10);
        
        // Панель управления (Сброс и Тестовые читы)
        GUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(1.0f, 0.22f, 0.22f);
        string resetBtnLabel = curLang == 0 ? "СБРОС" : "RESET";
        if (GUILayout.Button($"♻️ {resetBtnLabel}", GUILayout.Height(30)))
        {
            ResetPlayerStats();
        }
        
        GUI.backgroundColor = new Color(0.15f, 0.8f, 0.35f);
        string addXpText = curLang == 0 ? "ОПЫТ +50" : "+50 XP";
        if (GUILayout.Button($"✨ {addXpText}", GUILayout.Height(30)))
        {
            GainXP(50);
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
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
        float panelHeight = 540f;
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
            // Upgrade button logic
            if (castle.level < 6)
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
                GUILayout.Label(curLang == 0 ? "👑 ДОСТИГНУТ ЛЕГЕНДАРНЫЙ УРОВЕНЬ ЦИТАДЕЛИ" : "👑 LEGENDARY ZENITH OUTPOST FULLY EXPANDED", maxS);
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
                okS.fontStyle = FontStyle.Bold;
                GUILayout.Label(curLang == 0 ? "✓ [РАЗВЕДДАННЫЕ ПОЛУЧЕНЫ]" : "✓ [INTEL ACQUIRED]", okS);
            }
            else
            {
                string hiddenText = curLang == 0 ?
                    "• Общая мощь гарнизона: ??? (Скрыто)\n" +
                    "• Уровень Полководца: ??? (Скрыто)\n" +
                    "• Класс ковки защиты: ??? (Скрыто)\n" +
                    "• Запас боевых зелий: ??? (Скрыто)" :
                    "• Total Defense Power: ??? (Hidden)\n" +
                    "• Faction Commander: ??? (Hidden)\n" +
                    "• Guard Armor Quality: ??? (Hidden)\n" +
                    "• Supply Potions count: ??? (Hidden)";
                
                if (curLang == 8) hiddenText = "• 戍军总战斗力: ??? (未知)\n• 守城将领等级: ??? (未知)\n• 防具锻造等级: ??? (未知)\n• 备用药水数量: ??? (未知)";
                if (curLang == 7) hiddenText = "• 총 가д 수비력: ??? (백지)\n• 영주 사령관 훈련: ??? (백지)\n• 장갑 무구 구조: ??? (백지)\n• 회복 약물 비축량: ??? (백지)";

                GUILayout.Label(hiddenText, GUI.skin.label);
                
                GUILayout.Space(5);

                int cost = GetSpyCost(castle.level);

                if (!hasMinLvlUnlocked)
                {
                    GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                    GUILayout.Label(curLang == 0 ? 
                        $"🔒 Шпионаж заблокирован!\nТребуется уровень Вашего замка: {reqMinLevel}+. У вас: {pMaxLvl}." :
                        $"🔒 Espionage limits reached!\nRequires your castle tier: {reqMinLevel}+. You have: {pMaxLvl}.", GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                }
                else if (!lvlMatch)
                {
                    GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                    GUILayout.Label(curLang == 0 ? 
                        $"🔒 Уровень Вашего замка низок!\nТребуется ранг Вашего замка: {castle.level}+. У вас: {pMaxLvl}." :
                        $"🔒 Target protection too secure!\nRequires your castle tier: {castle.level}+ representing equal standard. You have: {pMaxLvl}.", GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    string btnSpyText = curLang == 0 ?
                        $"🕵️ КУПИТЬ ШПИОНАЖ ({cost} 💰)" :
                        $"🕵️ ACTIVATE ESPIONAGE ({cost} 💰)";
                    if (curLang == 8) btnSpyText = $"🕵️ 派遣密探渗透 ({cost} 💰)";
                    if (curLang == 7) btnSpyText = $"🕵️ 첩자 파견 ({cost} 💰)";

                    GUI.backgroundColor = new Color(0.12f, 0.82f, 0.98f, 1.0f);
                    if (GUILayout.Button(btnSpyText, GUILayout.Height(36)))
                    {
                        if (SaveGameSystem.CurrentData.gold < cost)
                        {
                            ShowFeedback(curLang == 0 ? "Недостаточно королевского золота!" : "Insufficient gold supplies!");
                        }
                        else
                        {
                            SaveGameSystem.CurrentData.gold -= cost;
                            PlayerPrefs.SetInt("Castle_Spied_" + castle.zoneIndex, 1);
                            PlayerPrefs.Save();
                            
                            string spyDone = curLang == 0 ?
                                "Шпион проник в лагерь врага! Данные составлены." :
                                "Espionage infiltration successful! Report compiled.";
                            ShowFeedback(spyDone);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = new Color(0.9f, 0.25f, 0.3f, 1.0f);
        string closeText = curLang == 0 ? "ЗАКРЫТЬ ОКНО" : "CLOSE WINDOW";
        if (GUILayout.Button(closeText, GUILayout.Height(36)))
        {
            isDetailsOpen = false;
        }
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// ТРЕХКОЛОНОЧНЫЙ КРАСИВЫЙ 2D ИНТЕРФЕЙС ЗАМКОВОГО ГОРОДА
    /// </summary>
    private void DrawTownViewGUI(int curLang)
    {
        float wWidth = Screen.width * 0.94f;
        float wHeight = Screen.height * 0.88f;
        float wx = (Screen.width - wWidth) / 2f;
        float wy = (Screen.height - wHeight) / 2f;

        GUI.backgroundColor = new Color(0.01f, 0.04f, 0.15f, 0.99f);
        GUILayout.BeginArea(new Rect(wx, wy, wWidth, wHeight), GUI.skin.box);

        GUILayout.Space(10);

        // Header
        GUIStyle tStyle = new GUIStyle(GUI.skin.label);
        tStyle.alignment = TextAnchor.MiddleCenter;
        tStyle.fontSize = 24;
        tStyle.fontStyle = FontStyle.Bold;
        tStyle.normal.textColor = Color.cyan;

        string header = curLang == 0 ? "🏟️ 2D ЗАМКОВЫЙ ГОРОД • ПАНЕЛЬ УПРАВЛЕНИЯ 🏟️" : "🏟️ 2D CASTLE TOWN • OUTPOST REGULATION 🏟️";
        if (curLang == 8) header = "🏟️ 2D 城堡内政管制大厅 🏟️";
        if (curLang == 7) header = "🏟️ 2D 영토 성채 제어반 🏟️";
        GUILayout.Label(header, tStyle);

        // TOWN SELECT QUICK SWITCH TABS (v18.11.15)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        for (int i = 0; i < castles.Count; i++)
        {
            if (castles[i].owner == "Player")
            {
                string tabName = curLang == 0 ? castles[i].nameRU : castles[i].nameEN;
                if (curLang == 8) tabName = castles[i].nameCH;
                if (curLang == 7) tabName = castles[i].nameKR;

                GUI.backgroundColor = (i == activeDetailsIndex) ? new Color(0.12f, 0.82f, 0.98f, 1.0f) : Color.white;
                string btnLabel = curLang == 0 ? $"🏰 {tabName.ToUpper()} (Ур.{castles[i].level})" : $"🏰 {tabName.ToUpper()} (Tier {castles[i].level})";
                if (GUILayout.Button(btnLabel, GUILayout.Height(32), GUILayout.Width(wWidth / 4.4f)))
                {
                    activeDetailsIndex = i;
                    feedbackMessage = "";
                }
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Subhead
        GUIStyle subSt = new GUIStyle(GUI.skin.label);
        subSt.alignment = TextAnchor.MiddleCenter;
        subSt.fontSize = 13;
        subSt.normal.textColor = Color.gray;

        CastleInstance activeCastle = castles[activeDetailsIndex >= 0 ? activeDetailsIndex : 0];
        string cLabel = curLang == 0 ? activeCastle.nameRU : activeCastle.nameEN;
        if (curLang == 8) cLabel = activeCastle.nameCH;
        if (curLang == 7) cLabel = activeCastle.nameKR;

        int activeIncome = GetGoldIncome(activeCastle.level);
        int activeHeroes = GetHeroesCountInCastle(activeCastle.zoneIndex);
        int activeCap = GetHeroCapacity(activeCastle.level);
        
        string subLabel = curLang == 0 ?
            $"{cLabel.ToUpper()} (Ур.{activeCastle.level}) | Доход: +{activeIncome} 💰/ход | Вместимость: {activeHeroes}/{activeCap} Героев 👥\nКазна фракции: {SaveGameSystem.CurrentData.gold} 💰 | Ранг игрока: {SaveGameSystem.CurrentData.playerLevel} (XP: {SaveGameSystem.CurrentData.currentXP}/100)" :
            $"{cLabel.ToUpper()} (Tier {activeCastle.level}) | Income: +{activeIncome} 💰/turn | Population: {activeHeroes}/{activeCap} Heroes 👥\nKingdom Gold: {SaveGameSystem.CurrentData.gold} 💰 | Player Level: {SaveGameSystem.CurrentData.playerLevel} (XP: {SaveGameSystem.CurrentData.currentXP}/100)";
        
        if (curLang == 8) subLabel = $"{cLabel.ToUpper()} (等级 {activeCastle.level}) | 收益: +{activeIncome} 💰/回合 | 英雄容量: {activeHeroes}/{activeCap} 👥\n帝国资金: {SaveGameSystem.CurrentData.gold} 💰 | 角色级别: {SaveGameSystem.CurrentData.playerLevel}";
        if (curLang == 7) subLabel = $"{cLabel.ToUpper()} (레벨 {activeCastle.level}) | 영지 소득: +{activeIncome} 💰/턴 | 인구: {activeHeroes}/{activeCap} 영웅 👥\n종족 금고: {SaveGameSystem.CurrentData.gold} 💰 | 플레이어 등급: {SaveGameSystem.CurrentData.playerLevel}";

        GUILayout.Label(subLabel, subSt);

        GUILayout.Space(12);

        // 3-Column horizontal division space
        GUILayout.BeginHorizontal();

        // ------------------ COLUMN 1: BARRACKS ------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(wWidth / 3.12f));
        GUIStyle colHeader1 = new GUIStyle(GUI.skin.label);
        colHeader1.alignment = TextAnchor.MiddleCenter;
        colHeader1.fontSize = 17;
        colHeader1.fontStyle = FontStyle.Bold;
        colHeader1.normal.textColor = new Color(0.2f, 1.0f, 0.6f);
        GUILayout.Label("⚔️ КАЗАРМЫ [Казармы]", colHeader1);
        
        string bDesc = curLang == 0 ? "Найм войск в армию согласно уровню замка" : "Troop recruitment matching castle tier";
        GUILayout.Label(bDesc, subSt);

        GUILayout.Space(10);
        barracksScroll = GUILayout.BeginScrollView(barracksScroll);

        // Unit definitions (cost, level requirement, display names, PlayerPrefs key suffix)
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

        // ------------------ COLUMN 2: FORGE & HEALTH SHOP ------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(wWidth / 3.12f));
        GUIStyle colHeader2 = new GUIStyle(GUI.skin.label);
        colHeader2.alignment = TextAnchor.MiddleCenter;
        colHeader2.fontSize = 17;
        colHeader2.fontStyle = FontStyle.Bold;
        colHeader2.normal.textColor = new Color(1.0f, 0.7f, 0.15f);
        GUILayout.Label("🧪 КУЗНИЦА И ЛАВКА [Магазин]", colHeader2);
        
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
        GUILayout.Box(curLang == 0 ? "⚔️ КУЗНИЦА ДОСПЕХОВ" : "⚔️ FORGE DEPARTMENT", GUILayout.Height(20));

        // Progressive 6 levels of equipment with gold values proportional to level
        int armorLv = PlayerPrefs.GetInt("Player_HeroArmor_Tier", 1);
        string curArmorName = GetEquipmentTierName(armorLv, curLang);
        GUILayout.Label($"{(curLang == 0 ? "Текущие Латы" : "Current Gear")}: {curArmorName} (Tier {armorLv})");

        if (armorLv < 6)
        {
            int nextLv = armorLv + 1;
            int armorPrice = 90 * nextLv * activeCastle.level;
            string eqBtnName = curLang == 0 ? 
                $"Выковать {GetEquipmentTierName(nextLv, 0)} ({armorPrice} 💰)" : 
                $"Forge {GetEquipmentTierName(nextLv, 1)} ({armorPrice} 💰)";
            if (curLang == 8) eqBtnName = $"锻造 {GetEquipmentTierName(nextLv, 8)} ({armorPrice} 💰)";
            if (curLang == 7) eqBtnName = $"제작 {GetEquipmentTierName(nextLv, 7)} ({armorPrice} 💰)";

            if (GUILayout.Button(eqBtnName, GUILayout.Height(40)))
            {
                if (SaveGameSystem.CurrentData.gold < armorPrice)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно королевского угля и золота!" : "Insufficient gold for smithy services!");
                }
                else
                {
                    SaveGameSystem.CurrentData.gold -= armorPrice;
                    PlayerPrefs.SetInt("Player_HeroArmor_Tier", nextLv);
                    PlayerPrefs.Save();
                    ShowFeedback(curLang == 0 ? "Кузнец успешно перековал ваши латы!" : "Plate armor upgraded permanently!");
                }
            }
        }
        else
        {
            GUILayout.Label(curLang == 0 ? "✓ Достигнуто легендарное качество ковки!" : "✓ Ultimate god-roll forging reached!", GUI.skin.box);
        }

        GUILayout.Space(15);
        GUILayout.Box(curLang == 0 ? "🕵️ НАЙМ ПРОСТЫХ ГЕРОЕВ" : "🕵️ RECRUIT ALLIED HEROES", GUILayout.Height(20));
        
        // Simple Heroes recruitment
        DrawHeroRecruitItem("ArcherHero", "Герой: Стрелок", "Comrade: Marksman Hero", "游侠英雄-神射手", "동료 영웅 - 명사수", 300);
        DrawHeroRecruitItem("WarriorHero", "Герой: Воин", "Comrade: Iron Warrior", "先锋英雄-铁血战士", "동료 영웅 - 광전사", 350);
        DrawHeroRecruitItem("MageHero", "Герой: Боевой Маг", "Comrade: Sorcerer Elite", "元素英雄-奥术法皇", "동료 영웅 - 원소 법사", 450);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // ------------------ COLUMN 3: TRAINING ACADEMY ------------------
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(wWidth / 3.12f));
        GUIStyle colHeader3 = new GUIStyle(GUI.skin.label);
        colHeader3.alignment = TextAnchor.MiddleCenter;
        colHeader3.fontSize = 17;
        colHeader3.fontStyle = FontStyle.Bold;
        colHeader3.normal.textColor = new Color(0.8f, 0.35f, 1.0f);
        GUILayout.Label("🔮 АКАДЕМИЯ И АРЕНА [Арена]", colHeader3);
        
        string aDesc = curLang == 0 ? "Тренировки, прокачка XP и ранги воинов" : "Gladiator maneuvers and combat upgrades";
        GUILayout.Label(aDesc, subSt);

        GUILayout.Space(10);
        academyScroll = GUILayout.BeginScrollView(academyScroll);

        // MAX LEVEL CAP formula based on Castle level on 1st island
        int maxLvlLimit = 10 + activeCastle.level * 5; // Level 1: Max 15, Level 6: Max 40
        GUILayout.Box($"{(curLang == 0 ? "Предел Уровня на 1-ой карте: " : "Map Lvl limit: ")} {maxLvlLimit}", GUI.skin.box);

        GUILayout.Space(8);

        // TARGET SELECTION for training (Main Hero vs Hired basic heroes)
        int selectedTrainingTarget = PlayerPrefs.GetInt("Town_Training_Target", 0); // 0: Main hero, 1: Archer comrades, 2: Warrior comrades, 3: Mage comrades
        GUILayout.Label(curLang == 0 ? "Кого тренируем на плацу:" : "Choose tactical trainee:");
        
        string[] targNames = curLang == 0 ? 
            new string[] { "Основной Герой", "Нанятые Стрелки", "Нанятые Воины", "Нанятые Маги" } :
            new string[] { "Main protagonist", "Recruited Archers", "Recruited Warriors", "Recruited Mages" };
        
        for (int t = 0; t < 4; t++)
        {
            if (GUILayout.Toggle(selectedTrainingTarget == t, targNames[t], GUI.skin.button))
            {
                selectedTrainingTarget = t;
                PlayerPrefs.SetInt("Town_Training_Target", t);
            }
        }

        GUILayout.Space(12);

        // 1. FREE TRAINING (Maneuvers)
        string freeTrainingLabel = curLang == 0 ? 
            "Арена: Маневры (XP +15/+30) | БЕСПЛАТНО" : 
            "Arena Tactics (XP +15/+30) | FREE";
        if (GUILayout.Button(freeTrainingLabel, GUILayout.Height(40)))
        {
            TriggerTraining(selectedTrainingTarget, 15, 30, maxLvlLimit, 0, curLang);
        }

        GUILayout.Space(8);

        // 2. COMMAND COURSE (PAID ELITE COURSE)
        int paidCourseCo = 150 * activeCastle.level;
        string paidLabel = curLang == 0 ?
            $"Курс Командоров (+60 XP) | {paidCourseCo} 💰" :
            $"Elite Captain drills (+60 XP) | {paidCourseCo} 💰";
        if (GUILayout.Button(paidLabel, GUILayout.Height(40)))
        {
            if (SaveGameSystem.CurrentData.gold < paidCourseCo)
            {
                ShowFeedback(curLang == 0 ? "Недостаточно золота в бюджете фракции!" : "Insufficient gold to purchase tactics textbook!");
            }
            else
            {
                TriggerTraining(selectedTrainingTarget, 60, 100, maxLvlLimit, paidCourseCo, curLang);
            }
        }

        GUILayout.Space(15);
        GUILayout.Box(curLang == 0 ? "🏆 ПРОКАЧКА РАНГОВ ВОЙНОВ" : "🏆 TROOP RANK ASCENSION", GUILayout.Height(20));

        // Troop Ranks upgrade mechanics
        int unitRank = PlayerPrefs.GetInt("Player_ArmyUnit_Rank", 1);
        string[] rankNamesRU = { "Новобранцы", "Регулярная армия", "Ветераны", "Гвардия завета", "Элита Зенита" };
        string[] rankNamesEN = { "Conscripts", "Regular Infantry", "Honored Veterans", "Covenant Guard", "Zenith Champions Elite" };

        string currentRankName = curLang == 0 ? rankNamesRU[Mathf.Clamp(unitRank - 1, 0, 4)] : rankNamesEN[Mathf.Clamp(unitRank - 1, 0, 4)];
        GUILayout.Label($"{(curLang == 0 ? "Ранг воинов: " : "Troop Rank: ")} {currentRankName}");

        if (unitRank < 5)
        {
            int rankPrice = 250 * unitRank * activeCastle.level;
            string rankUpLabel = curLang == 0 ?
                $"Повысить ранг до {unitRank + 1} ({rankPrice} 💰)" :
                $"Promote rank to {unitRank + 1} ({rankPrice} 💰)";
            if (curLang == 8) rankUpLabel = $"提升士兵级别至 {unitRank + 1} ({rankPrice} 💰)";
            if (curLang == 7) rankUpLabel = $"아군 병사 등급 향상 {unitRank + 1}단계 ({rankPrice} 💰)";

            if (GUILayout.Button(rankUpLabel, GUILayout.Height(40)))
            {
                if (SaveGameSystem.CurrentData.gold < rankPrice)
                {
                    ShowFeedback(curLang == 0 ? "Недостаточно золога на экипировку гвардии!" : "Not enough gold to replace weapons!");
                }
                else
                {
                    SaveGameSystem.CurrentData.gold -= rankPrice;
                    unitRank++;
                    PlayerPrefs.SetInt("Player_ArmyUnit_Rank", unitRank);
                    PlayerPrefs.Save();
                    
                    string rankMsg = curLang == 0 ?
                        "Ранг всей вашей армии повышен! Характеристики возросли." :
                        "Your global cohort promoted successfully to absolute veteran height!";
                    ShowFeedback(rankMsg);
                }
            }
        }
        else
        {
            GUILayout.Label(curLang == 0 ? "✓ Ваша армия достигла максимальной боевой славы!" : "✓ Army reached supreme zenith classification!", GUI.skin.box);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

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
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUILayout.EndArea();
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

    private void DrawUnitItem(string id, string nameRU, string nameEN, string nameCH, string nameKR, int price, int requiredLvl, int castleLvl)
    {
        int curLang = Translator.LanguageID;
        int count = GetUnitCount(id, activeDetailsIndex);
        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label($"{name}\n(Ур.{requiredLvl} +) | [В наличии: {count}]", GUILayout.Width(180));

        if (castleLvl < requiredLvl)
        {
            GUI.backgroundColor = Color.grey;
            GUILayout.Button(curLang == 0 ? "⚡ Замок LVL " + requiredLvl : "⚡ Build T-" + requiredLvl, GUILayout.Height(35));
            GUI.backgroundColor = Color.white;
        }
        else
        {
            if (GUILayout.Button($"{price} 💰", GUILayout.Height(35)))
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
                SaveGameSystem.CurrentData.gold -= price;
                count++;
                PlayerPrefs.SetInt($"Player_Potion_{id}_Lvl_{potionLvl}", count);
                PlayerPrefs.Save();

                string okFeed = curLang == 0 ?
                    $"Куплено: {name} (Ур.{potionLvl})!" :
                    $"Acquired: {name} (Level {potionLvl})!";
                ShowFeedback(okFeed);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawHeroRecruitItem(string key, string nameRU, string nameEN, string nameCH, string nameKR, int basePrice)
    {
        int curLang = Translator.LanguageID;
        int count = GetHeroCount(key, activeDetailsIndex);

        string name = curLang == 0 ? nameRU : nameEN;
        if (curLang == 8) name = nameCH;
        if (curLang == 7) name = nameKR;

        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label($"{name}\n[В замке: {count}]", GUILayout.Width(180));

        if (GUILayout.Button($"{basePrice} 💰", GUILayout.Height(35)))
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
                if (curLang == 7) limitTxt = $"성채 영웅 한도 초과 ({currentHeroes}/{capacity})! 성채를 먼저 업그레이드 하십시오.";
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
}

/// <summary>
/// Скрипт-маркер, вещаемый на 3D замки для улавливания кликов
/// </summary>
public class InteractiveCastle : MonoBehaviour
{
    public int zoneIndex;
}
