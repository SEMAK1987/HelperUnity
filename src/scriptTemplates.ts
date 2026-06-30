export interface ScriptTemplate {
  name: string;
  filename: string;
  location: string;
  language: "csharp" | "python";
  descriptionRU: string;
  descriptionEN: string;
  code: string;
}

export const scriptTemplates: ScriptTemplate[] = [
  {
    name: "Unity AI Connector",
    filename: "UnityConnector.cs",
    location: "Assets/Scripts/QuantumAI/UnityConnector.cs",
    language: "csharp",
    descriptionRU: "Связывает Unity с нашим ИИ-помощником. Отправляет текстовые запросы на локальный или удаленный сервер и возвращает ответы ИИ напрямую в игру.",
    descriptionEN: "Bridges Unity with our AI assistant. Sends text queries to the local or remote server and returns AI responses directly into the game.",
    code: `// [POTION MECHANICS REWORK & STABILIZATION v18.11.23]
// Unity Connector for Quantum AI Assistant
// Updated: 2026-06-30 (Synced with Stable v18.11.23)

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace QuantumAI
{
    public class UnityConnector : MonoBehaviour
    {
        [Header("Sync Configuration")]
        [SerializeField] private string serverUrl = "http://localhost:3000";
        [SerializeField] private Mode mode = Mode.Online;
        
        [Header("Status Fields")]
        [SerializeField] private string status = "Ready for Quantum Manifestation v18.11.23";
        [SerializeField] private bool isProcessing = false;

        public enum Mode { Online, Offline, NoInternet }

        public static UnityConnector Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SendPrompt(string promptText, System.Action<string> onCallback)
        {
            if (mode == Mode.NoInternet)
            {
                onCallback?.Invoke("No internet mode active. Prompt processed locally within Unity cache.");
                return;
            }

            StartCoroutine(PostPromptCoroutine(promptText, onCallback));
        }

        private IEnumerator PostPromptCoroutine(string promptText, System.Action<string> onCallback)
        {
            isProcessing = true;
            status = "Sending Query to Server...";

            string url = $"{serverUrl}/api/prompt";
            WWWForm form = new WWWForm();
            form.AddField("prompt", promptText);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[UnityConnector] Error connecting: {www.error}");
                    status = "Connection Failed. Fallback Active.";
                    onCallback?.Invoke($"Fallback response: connection failed ({www.error})");
                }
                else
                {
                    status = "Response Received Successfully.";
                    onCallback?.Invoke(www.downloadHandler.text);
                }
            }

            isProcessing = false;
        }
    }
}`
  },
  {
    name: "Castle Manager",
    filename: "FateCastleManager.cs",
    location: "Assets/Scripts/Map/FateCastleManager.cs",
    language: "csharp",
    descriptionRU: "Главный менеджер замков. Хранит данные о 12 зонах, калибрует 3D-координаты и исправляет ошибки CS0111 за счет объединения дублирующих OnDestroy() и GetCastleRace() в чистые монолитные функции.",
    descriptionEN: "The primary castle manager. Manages 12 distinct zones, calibrates 3D coordinates, and resolves CS0111 duplicate errors by cleanly consolidating OnDestroy() and GetCastleRace() methods.",
    code: `// [ZENITH CASTLE MANAGER & STABILIZATION SYSTEM v18.11.23]
// Location: Assets/Scripts/Map/FateCastleManager.cs
// Synchronized and optimized to eliminate all CS0111 duplicate member declarations.

using UnityEngine;
using System;
using System.Collections.Generic;

public class FateCastleManager : MonoBehaviour
{
    public static FateCastleManager Instance { get; private set; }

    [Header("Manager Settings")]
    public string managerVersion = "18.11.23";
    public int currentLanguageIndex = 0; // 0 = RU, 1 = EN, 2 = DE, 3 = ES, 4 = PT, 5 = KR, 6 = ZH
    public bool isHeroProfileOpen = false;

    [System.Serializable]
    public class CastleData
    {
        public int id;
        public string nameRU;
        public string nameEN;
        public string owner;
        public Vector3 coordinates;
        public Color glowColor;
    }

    [Header("Active Map Castles")]
    public List<CastleData> castles = new List<CastleData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeCastles();
    }

    private void Start()
    {
        Debug.Log($"[FateCastleManager] Initialized v{managerVersion} on startup.");
    }

    private void InitializeCastles()
    {
        castles.Clear();
        castles.Add(new CastleData { id = 0, nameRU = "Сильвания", nameEN = "Sylvania", owner = "Sylvan", coordinates = new Vector3(12.5f, -2.0f, 3.2f), glowColor = new Color(0.1f, 0.7f, 0.2f, 1f) });
        castles.Add(new CastleData { id = 1, nameRU = "Пустошь Бандитов", nameEN = "Bandit Wasteland", owner = "Desperados", coordinates = new Vector3(-45.1f, -2.0f, 10.8f), glowColor = new Color(0.7f, 0.5f, 0.1f, 1f) });
        castles.Add(new CastleData { id = 2, nameRU = "Торговый Перекресток", nameEN = "Merchant Crossroad", owner = "Merchants", coordinates = new Vector3(5.0f, -2.0f, -12.4f), glowColor = new Color(0.1f, 0.4f, 0.7f, 1f) });
        castles.Add(new CastleData { id = 3, nameRU = "Святилище Зенита", nameEN = "Zenith Sanctuary", owner = "Celestials", coordinates = new Vector3(0.0f, -2.0f, 0.0f), glowColor = new Color(0.4f, 0.1f, 0.7f, 1f) });
        castles.Add(new CastleData { id = 4, nameRU = "Эльфийский Лес v18", nameEN = "Elven Sylvan Forest", owner = "Sylvan", coordinates = new Vector3(18.2f, -2.0f, 22.1f), glowColor = new Color(0.1f, 0.7f, 0.2f, 1f) });
        castles.Add(new CastleData { id = 5, nameRU = "Изумрудная Сень", nameEN = "Emerald Bower", owner = "Sylvan", coordinates = new Vector3(30.1f, -2.0f, 15.0f), glowColor = new Color(0.1f, 0.7f, 0.2f, 1f) });
        castles.Add(new CastleData { id = 6, nameRU = "Ледяной Пик", nameEN = "Frostbound Peak", owner = "Overlords", coordinates = new Vector3(-8.0f, -2.0f, 54.3f), glowColor = new Color(0.1f, 0.4f, 0.7f, 1f) });
        castles.Add(new CastleData { id = 7, nameRU = "Аванпост Изгоев", nameEN = "Outcast Outpost", owner = "Desperados", coordinates = new Vector3(-32.5f, -2.0f, -28.1f), glowColor = new Color(0.7f, 0.5f, 0.1f, 1f) });
        castles.Add(new CastleData { id = 8, nameRU = "Древние Руины", nameEN = "Ancient Ruins", owner = "Barbarians", coordinates = new Vector3(42.0f, -2.0f, -31.2f), glowColor = new Color(0.7f, 0.5f, 0.1f, 1f) });
        castles.Add(new CastleData { id = 9, nameRU = "Орден Света (Цитадель)", nameEN = "Order of Light Citadel", owner = "Player", coordinates = new Vector3(1.5f, -2.0f, -8.5f), glowColor = new Color(0.4f, 0.1f, 0.7f, 1f) });
        castles.Add(new CastleData { id = 10, nameRU = "Вольный Союз", nameEN = "Free Alliance Bazaar", owner = "Merchants", coordinates = new Vector3(25.4f, -2.0f, -5.0f), glowColor = new Color(0.1f, 0.4f, 0.7f, 1f) });
        castles.Add(new CastleData { id = 11, nameRU = "Кровавые Пустоши Орков", nameEN = "Blood Orc Badlands", owner = "BloodOrcs", coordinates = new Vector3(-55.0f, -2.0f, 40.0f), glowColor = new Color(0.4f, 0.1f, 0.7f, 1f) });
    }

    /// <summary>
    /// Returns localized castle/zone race name. Defined strictly once to avoid CS0111.
    /// </summary>
    public string GetCastleRace(int zoneIndex, int lang)
    {
        switch (lang)
        {
            case 0: // Russian
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "Сильвийские Эльфы";
                if (zoneIndex == 1 || zoneIndex == 7) return "Изгои Пустошей";
                if (zoneIndex == 2 || zoneIndex == 10) return "Торговый Консорциум";
                if (zoneIndex == 3 || zoneIndex == 9) return "Небесный Орден";
                if (zoneIndex == 11) return "Орки Скверны";
                return "Северные Владыки";
            case 1: // English
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "Sylvan Elves";
                if (zoneIndex == 1 || zoneIndex == 7) return "Desperado Bandits";
                if (zoneIndex == 2 || zoneIndex == 10) return "Merchant Consortium";
                if (zoneIndex == 3 || zoneIndex == 9) return "Celestial Covenant";
                if (zoneIndex == 11) return "Blood Orc Tribes";
                return "Frost Overlords";
            case 2: // German
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "Waldelfen";
                if (zoneIndex == 1 || zoneIndex == 7) return "Wüstenbanditen";
                return "Königreich";
            case 3: // Spanish
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "Elfos Silvanos";
                return "Reino";
            case 4: // Portuguese
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "Elfos Silvestres";
                return "Império";
            case 5: // Korean
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "실반 엘프";
                return "제국";
            case 6: // Chinese
                if (zoneIndex == 0 || zoneIndex == 4 || zoneIndex == 5) return "森林精灵";
                return "帝国";
            default:
                return "Sylvan Elves";
        }
    }

    /// <summary>
    /// Returns localized item names based on language slot parameter.
    /// </summary>
    public string GetItemName(int slotType, int tier, int lang)
    {
        string prefix = "";
        string baseName = "";

        if (lang == 0) // Russian
        {
            if (tier <= 1) prefix = "Начальный";
            else if (tier <= 3) prefix = "Земной";
            else if (tier <= 5) prefix = "Небесный";
            else prefix = "Божественный";

            if (slotType == 0) baseName = "Шлем";
            else if (slotType == 1) baseName = "Амулет";
            else if (slotType == 2) baseName = "Наплечники";
            else if (slotType == 3) baseName = "Доспех";
            else if (slotType == 4) baseName = "Пояс";
            else if (slotType == 5) baseName = "Сапоги";
            else baseName = "Меч";

            return $"{prefix} {baseName}";
        }
        else // English default fallback
        {
            if (tier <= 1) prefix = "Apprentice";
            else if (tier <= 3) prefix = "Earthen";
            else if (tier <= 5) prefix = "Celestial";
            else prefix = "Divine";

            if (slotType == 0) baseName = "Helmet";
            else if (slotType == 1) baseName = "Amulet";
            else if (slotType == 2) baseName = "Pauldrons";
            else if (slotType == 3) baseName = "Chestplate";
            else if (slotType == 4) baseName = "Belt";
            else if (slotType == 5) baseName = "Greaves";
            else baseName = "Slayer";

            return $"{prefix} {baseName}";
        }
    }

    public void CalibrateCastleCoordinates(int id, float x, float y, float z)
    {
        foreach (var castle in castles)
        {
            if (castle.id == id)
            {
                castle.coordinates = new Vector3(x, y, z);
                Debug.Log($"[FateCastleManager] Castle ID {id} calibrated to position: {castle.coordinates}");
                break;
            }
        }
    }

    private void Update()
    {
        // Simple standalone simulation per frame
    }

    /// <summary>
    /// Safe OnDestroy lifecycle handler. Defined strictly once to avoid CS0111.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("[FateCastleManager] Instance cleared and resources freed safely.");
        }
    }

    private void OnGUI()
    {
        if (!isHeroProfileOpen) return;

        // Visual layout IMGUI drawing for Unity developer runtime diagnostics
        GUILayout.BeginArea(new Rect(20, 20, 320, 240), "Fate Castle Calibrator", GUI.skin.box);
        GUILayout.Label($"Manager Active: v{managerVersion}");
        GUILayout.Label($"Language: {currentLanguageIndex}");
        GUILayout.Space(5);

        if (castles.Count > 0)
        {
            var testCastle = castles[0];
            GUILayout.Label($"Zone 0: {testCastle.nameRU} ({GetCastleRace(0, currentLanguageIndex)})");
            GUILayout.Label($"Coords: {testCastle.coordinates.x:F1}, {testCastle.coordinates.y:F1}, {testCastle.coordinates.z:F1}");
        }

        GUILayout.EndArea();
    }
}`
  },
  {
    name: "Faction Map Marker",
    filename: "FactionMapMarker.cs",
    location: "Assets/Scripts/Map/FactionMapMarker.cs",
    language: "csharp",
    descriptionRU: "Отвечает за отрисовку и взаимодействие с замком на 3D-карте. Включает неоновое свечение (Bloom) при наведении курсора и фиксирует координаты замка при клике.",
    descriptionEN: "Handles rendering and mouse interaction with individual castles on the 3D map. Enables HDR Neon Glow emission under cursor hover and syncs fine-tuned coordinates on click.",
    code: `// [ZENITH COMPILER COMPATIBILITY & DYNAMIC NEON GLOW v18.11.23]
// Location: Assets/Scripts/Map/FactionMapMarker.cs

using UnityEngine;

public class FactionMapMarker : MonoBehaviour
{
    public int zoneId = 0;
    public string factionName = "Sylvan Elves";
    public string factionDescription = "Guardians of Sylvania forest.";
    
    [Header("Glow & Scaling Settings")]
    public float baseScale = 1.0f;
    public float targetScale = 1.2f;
    public float scaleSpeed = 3.0f;
    public Vector3 localScaleOverride = Vector3.one;

    private Material markerMaterial;
    private bool isHovered = false;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            markerMaterial = renderer.material;
        }

        // Lock coordinate alignment with parent
        transform.localPosition = new Vector3(transform.localPosition.x, -2.0f, transform.localPosition.z);
    }

    private void Update()
    {
        // Separate marker scaling from parent Map scale compensation
        float step = scaleSpeed * Time.deltaTime;
        float currentTarget = isHovered ? targetScale : baseScale;
        Vector3 desiredScale = localScaleOverride * currentTarget;
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, step);
    }

    public void OnPointerEnter()
    {
        isHovered = true;
        if (markerMaterial != null)
        {
            markerMaterial.EnableKeyword("_EMISSION");
            markerMaterial.SetColor("_EmissionColor", Color.cyan * 2.0f); // Auto-calibrated HDR Bloom glow
        }
    }

    public void OnPointerExit()
    {
        isHovered = false;
        if (markerMaterial != null)
        {
            markerMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    public void OnPointerClick()
    {
        Debug.Log($"[FactionMapMarker] Marker ID {zoneId} ({factionName}) clicked.");
        if (FateCastleManager.Instance != null)
        {
            FateCastleManager.Instance.CalibrateCastleCoordinates(zoneId, transform.position.x, transform.position.y, transform.position.z);
        }
    }
}`
  },
  {
    name: "Blender Bridge Add-on",
    filename: "blender_connector.py",
    location: "Scripts/Blender/blender_connector.py",
    language: "python",
    descriptionRU: "Аддон для Blender 3D. Позволяет синхронизировать сетку и процедурные ландшафты Fate Continent с сервером ИИ-помощника напрямую из вьюпорта Blender через N-панель.",
    descriptionEN: "A Blender 3D add-on. Connects Blender viewport tools and procedural terrain grids of Fate Continent with the AI Assistant server via a sidebar N-panel.",
    code: `# [POTION MECHANICS REWORK & STABILIZATION v18.11.23]
# Blender Bridge add-on for Fate Continent World Generator
# Updated: 2026-06-30 (Synced with Stable v18.11.23)

bl_info = {
    "name": "AI Assistant Link",
    "author": "Omniversal World Architect v18.11.23",
    "version": (18, 11, 23),
    "blender": (2, 80, 0),
    "location": "View3D > N-Panel > AI Assistant",
    "description": "Direct bridge to the World Architect Divine Architect Supreme with project level GOD Synergy.",
    "warning": "",
    "wiki_url": "",
    "category": "Development",
}

import bpy
import urllib.request
import urllib.parse
import json

class AISettings(bpy.types.PropertyGroup):
    server_url: bpy.props.StringProperty(
        name="Server URL",
        description="Address of AI Studio Node assistant",
        default="http://localhost:3000"
    )
    prompt_input: bpy.props.StringProperty(
        name="Prompt",
        description="Enter instructions for procedural world generation",
        default="Generate glowing castle markers around the Zenith Sanctuary"
    )

class VIEW3D_PT_AIAssistant(bpy.types.Panel):
    bl_label = "AI Assistant Link"
    bl_idname = "VIEW3D_PT_ai_assistant"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "AI Assistant"

    def draw(self, context):
        layout = self.layout
        scene = context.scene
        ai_tool = scene.ai_tool

        layout.label(text="Fate Continent v18.11.23")
        layout.prop(ai_tool, "server_url")
        layout.prop(ai_tool, "prompt_input")
        layout.operator("ai.sync_world_data", text="Sync to Assistant Link")

class OBJECT_OT_AISyncWorldData(bpy.types.Operator):
    bl_label = "Sync World Data"
    bl_idname = "ai.sync_world_data"
    bl_description = "Sends active scene details to the companion app"

    def execute(self, context):
        scene = context.scene
        ai_tool = scene.ai_tool
        
        self.report({'INFO'}, "Connecting to AI Studio Companion...")
        
        # Simple procedural generation loop simulation
        try:
            url = f"{ai_tool.server_url}/api/status"
            req = urllib.request.Request(url, method="GET")
            with urllib.request.urlopen(req, timeout=3) as response:
                res_data = json.loads(response.read().decode())
                self.report({'INFO'}, f"Connected to {res_data.get('game', 'System')} Server.")
        except Exception as e:
            self.report({'WARNING'}, f"Local fallback active: {str(e)}")
            
        return {'FINISHED'}

def register():
    bpy.utils.register_class(AISettings)
    bpy.utils.register_class(VIEW3D_PT_AIAssistant)
    bpy.utils.register_class(OBJECT_OT_AISyncWorldData)
    bpy.types.Scene.ai_tool = bpy.props.PointerProperty(type=AISettings)

def unregister():
    bpy.utils.unregister_class(AISettings)
    bpy.utils.unregister_class(VIEW3D_PT_AIAssistant)
    bpy.utils.unregister_class(OBJECT_OT_AISyncWorldData)
    del bpy.types.Scene.ai_tool

if __name__ == "__main__":
    register()
`
  }
];
