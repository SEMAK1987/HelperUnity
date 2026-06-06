using System.Collections.Generic;
using UnityEngine;

namespace FateContinent
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FateMapManager : MonoBehaviour
    {
        public static FateMapManager Instance { get; private set; }

        [System.Serializable]
        public class RingConfig
        {
            public string ringName = "Порт Аэлиссы";
            [TextArea(1, 3)]
            public string ringDescription = "Древние врата во владения эльфийского флота.";
            public Sprite ringSprite; // Спрайт кольца
            
            [Header("Пространственные настройки кольца")]
            public Vector2 localPosition = Vector2.zero; // Позиция на карте
            
            [Tooltip("Размер/Масштаб индивидуального кольца (X, Y, Z)")]
            public Vector3 localScale = Vector3.one;

            [Header("Диалоги и Аудио")]
            public int associatedDialogueIndex = 4;
            public string clickSfxName = "UI_Click_Metallic";
            public string hoverSfxName = "UI_Hover_Soft";
            
            [Header("Кастомизация Свечения")]
            [ColorUsage(true, true)]
            public Color normalGlowColor = new Color(0.1f, 0.4f, 1.0f, 1.0f);
            [ColorUsage(true, true)]
            public Color hoverGlowColor = new Color(0.3f, 0.7f, 1.0f, 2.0f);
        }

        [System.Serializable]
        public class MapConfig
        {
            public string mapName = "Новый Континент";
            public Sprite mapBackground; // Фоновая текстура карты
            public List<RingConfig> rings = new List<RingConfig>(); // Список интерактивных колец
        }

        [Header("Каталог Карт и Колец (Нажимайте [+] для расширения)")]
        [Tooltip("Список всех доступных карт. Добавляйте новые элементы и их кольца прямо здесь!")]
        public List<MapConfig> mapsList = new List<MapConfig>();

        [Header("⚙️ ИНСПЕКТОР КООРДИНАТ (TRANSFORM)")]
        [Tooltip("Смещение всей карты по X")]
        [Range(-1000f, 1000f)]
        public float mapOffsetX = 0f;

        [Tooltip("Смещение всей карты по Y")]
        [Range(-1000f, 1000f)]
        public float mapOffsetY = 0f;

        [Tooltip("Масштаб всей карты (Map Scale)")]
        [Range(0.1f, 3.0f)]
        public float mapScale = 1.0f;

        [Tooltip("Масштаб колец (Ring Scale)")]
        [Range(0.1f, 3.0f)]
        public float ringScale = 1.0f;

        [Header("СМЕЩЕНИЕ КОЛЕЦ ВЫСАДКИ (RING ANCHORS)")]
        [Header("1. Wastes Ring Offset")]
        [Range(-500f, 500f)] public float ring1OffsetX = 0f;
        [Range(-500f, 500f)] public float ring1OffsetY = 0f;

        [Header("2. Peak Ring Offset")]
        [Range(-500f, 500f)] public float ring2OffsetX = 0f;
        [Range(-500f, 500f)] public float ring2OffsetY = 0f;

        [Header("3. Ruins Ring Offset")]
        [Range(-500f, 500f)] public float ring3OffsetX = 0f;
        [Range(-500f, 500f)] public float ring3OffsetY = 0f;

        [Header("4. Sanctuary Ring Offset")]
        [Range(-500f, 500f)] public float ring4OffsetX = 0f;
        [Range(-500f, 500f)] public float ring4OffsetY = 0f;

        [Header("Общие настройки маркеров")]
        [Tooltip("Общий материал свечения с поддержкой Bloom (например, M_Neon_Glow)")]
        public Material defaultGlowMaterial;
        public float scaleSpeed = 8.0f;
        public float hoverPulseSpeed = 2.0f;

        [Header("Текущее Состояние")]
        [Tooltip("Индекс активной карты из списка (от 0 и выше)")]
        [SerializeField] private int activeMapIndex = 0;

        [Header("Отображение Карты")]
        [Tooltip("Показывать ли карту при старте игры (если false, она скрыта, пока не завершится диалог)")]
        public bool showMapOnStart = false;

        private bool isMapVisible = false;

        public bool IsMapVisible
        {
            get { return isMapVisible; }
        }

        private SpriteRenderer mapBgRenderer;
        private List<GameObject> activeRings = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mapBgRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            InitializeMap(activeMapIndex);

            if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
            {
                SetMapVisible(false);
            }
        }

        public void SetMapVisible(bool visible)
        {
            isMapVisible = visible;
            
            if (mapBgRenderer == null) mapBgRenderer = GetComponent<SpriteRenderer>();
            if (mapBgRenderer != null) mapBgRenderer.enabled = visible;

            foreach (GameObject ring in activeRings)
            {
                if (ring != null) ring.SetActive(visible);
            }

            Debug.Log($"[Fate Map] Видимость тактической карты изменена на: {visible}");
        }

        // КЛИЕНТСКИЙ СОВМЕСТИМЫЙ ИНТЕРФЕЙС SWITCH TO MAP
        public void SwitchToMap(int mapIndex)
        {
            if (mapIndex < 0 || mapIndex >= mapsList.Count)
            {
                Debug.LogWarning($"[Fate Map] Неверный индекс карты {mapIndex} для переключения.");
                return;
            }

            activeMapIndex = mapIndex;
            InitializeMap(activeMapIndex);
        }

        private void SetupFallbackMap()
        {
            mapsList = new List<MapConfig>();
            MapConfig fallbackMap = new MapConfig();
            fallbackMap.mapName = "Арделланд";
            
            RingConfig ring1 = new RingConfig();
            ring1.ringName = "Багровые Пустоши";
            ring1.ringDescription = "Выжженная мертвая пустыня, богатая кристаллами Зенита.";
            ring1.associatedDialogueIndex = 4;
            ring1.localPosition = new Vector2(-120f, 40f);
            ring1.localScale = Vector3.one;
            ring1.normalGlowColor = new Color(0.95f, 0.15f, 0.2f, 1.0f);
            ring1.hoverGlowColor = new Color(1.0f, 0.4f, 0.45f, 2.0f);

            RingConfig ring2 = new RingConfig();
            ring2.ringName = "Ледяной Пик";
            ring2.ringDescription = "Заснеженная горная гряда на севере. Здесь укрыты древние рудники.";
            ring2.associatedDialogueIndex = 5;
            ring2.localPosition = new Vector2(20f, 80f);
            ring2.localScale = Vector3.one;
            ring2.normalGlowColor = new Color(0.15f, 0.65f, 0.95f, 1.0f);
            ring2.hoverGlowColor = new Color(0.4f, 0.85f, 1.0f, 2.0f);

            RingConfig ring3 = new RingConfig();
            ring3.ringName = "Древние Руины";
            ring3.ringDescription = "Остатки разрушенной столицы первой династии.";
            ring3.associatedDialogueIndex = 6;
            ring3.localPosition = new Vector2(150f, -60f);
            ring3.localScale = Vector3.one;
            ring3.normalGlowColor = new Color(0.85f, 0.55f, 0.12f, 1.0f);
            ring3.hoverGlowColor = new Color(1.0f, 0.75f, 0.25f, 2.0f);

            RingConfig ring4 = new RingConfig();
            ring4.ringName = "Святилище Зенита";
            ring4.ringDescription = "Величественный лесной оазис, скрывающий истоки Кристалла сокрытого силой лесных духов.";
            ring4.associatedDialogueIndex = 7;
            ring4.localPosition = new Vector2(0f, -100f);
            ring4.localScale = Vector3.one;
            ring4.normalGlowColor = new Color(0.12f, 0.85f, 0.35f, 1.0f);
            ring4.hoverGlowColor = new Color(0.25f, 1.0f, 0.55f, 2.0f);

            fallbackMap.rings.Add(ring1);
            fallbackMap.rings.Add(ring2);
            fallbackMap.rings.Add(ring3);
            fallbackMap.rings.Add(ring4);

            mapsList.Add(fallbackMap);
        }

        private void PopulateDefaultRingsForMap(MapConfig map, int mapIndex)
        {
            if (map.rings == null) map.rings = new List<RingConfig>();
            map.rings.Clear();

            RingConfig ring1 = new RingConfig();
            ring1.ringName = "Кровавые Пустоши";
            ring1.ringDescription = "Выжженная мертвая пустыня, богатая кристаллами Зенита.";
            ring1.associatedDialogueIndex = 3;
            ring1.localPosition = new Vector2(-120f, 40f);
            ring1.localScale = Vector3.one;
            ring1.normalGlowColor = new Color(0.95f, 0.15f, 0.2f, 1.0f);
            ring1.hoverGlowColor = new Color(1.0f, 0.4f, 0.45f, 2.0f);

            RingConfig ring2 = new RingConfig();
            ring2.ringName = "Ледяной Пик";
            ring2.ringDescription = "Заснеженная горная гряда на севере. Здесь укрыты древние рудники.";
            ring2.associatedDialogueIndex = 3;
            ring2.localPosition = new Vector2(20f, 80f);
            ring2.localScale = Vector3.one;
            ring2.normalGlowColor = new Color(0.15f, 0.65f, 0.95f, 1.0f);
            ring2.hoverGlowColor = new Color(0.4f, 0.85f, 1.0f, 2.0f);

            RingConfig ring3 = new RingConfig();
            ring3.ringName = "Древние Руины";
            ring3.ringDescription = "Остатки разрушенной столицы первой династии.";
            ring3.associatedDialogueIndex = 3;
            ring3.localPosition = new Vector2(150f, -60f);
            ring3.localScale = Vector3.one;
            ring3.normalGlowColor = new Color(0.85f, 0.55f, 0.12f, 1.0f);
            ring3.hoverGlowColor = new Color(1.0f, 0.75f, 0.25f, 2.0f);

            RingConfig ring4 = new RingConfig();
            ring4.ringName = "Святилище Зенита";
            ring4.ringDescription = "Величественный лесной оазис, скрывающий истоки Кристалла сокрытого силой лесных духов.";
            ring4.associatedDialogueIndex = 3;
            ring4.localPosition = new Vector2(0f, -100f);
            ring4.localScale = Vector3.one;
            ring4.normalGlowColor = new Color(0.12f, 0.85f, 0.35f, 1.0f);
            ring4.hoverGlowColor = new Color(0.25f, 1.0f, 0.55f, 2.0f);

            map.rings.Add(ring1);
            map.rings.Add(ring2);
            map.rings.Add(ring3);
            map.rings.Add(ring4);

            Debug.Log($"[FateMapManager] Автоматически заполнили {map.rings.Count} колец высадки для карты '{map.mapName}' (Индекс: {mapIndex}) для бесшовной сюжетной завязки!");
        }

        public void InitializeMap(int mapIndex)
        {
            if (mapBgRenderer == null) mapBgRenderer = GetComponent<SpriteRenderer>();
            
            if (mapsList == null || mapsList.Count == 0)
            {
                SetupFallbackMap();
            }

            if (mapIndex < 0 || mapIndex >= mapsList.Count) mapIndex = 0;

            MapConfig activeMap = mapsList[mapIndex];

            ApplyTransformOffsets();

            if (activeMap.mapBackground != null)
            {
                mapBgRenderer.sprite = activeMap.mapBackground;
            }
            else
            {
                mapBgRenderer.sprite = CreateProceduralMapBackground();
            }

            List<FactionMapMarker> existingMarkers = new List<FactionMapMarker>(GetComponentsInChildren<FactionMapMarker>(true));
            foreach (var marker in existingMarkers)
            {
                if (marker != null) marker.gameObject.SetActive(false);
            }

            activeRings.Clear();

            for (int i = 0; i < activeMap.rings.Count; i++)
            {
                var ringConf = activeMap.rings[i];
                if (ringConf == null) continue;

                string expectedName = $"RingMarker_{ringConf.ringName}";
                FactionMapMarker marker = existingMarkers.Find(m => m != null && m.gameObject.name == expectedName);
                GameObject ringObj = null;

                if (marker != null)
                {
                    ringObj = marker.gameObject;
                    ringObj.SetActive(isMapVisible);

                    if (!Application.isPlaying)
                    {
                        #if UNITY_EDITOR
                        // Разрешаем обратную синхронизацию координат ТОЛЬКО если пользователь держит и двигает этот маркер в Hierarchy/Scene!
                        bool isDraggingInScene = UnityEditor.Selection.activeGameObject == ringObj;
                        if (isDraggingInScene)
                        {
                            Vector2 currentLocalPos = new Vector2(ringObj.transform.localPosition.x, ringObj.transform.localPosition.y);
                            Vector2 offset = Vector2.zero;
                            if (i == 0) offset = new Vector2(ring1OffsetX, ring1OffsetY);
                            else if (i == 1) offset = new Vector2(ring2OffsetX, ring2OffsetY);
                            else if (i == 2) offset = new Vector2(ring3OffsetX, ring3OffsetY);
                            else if (i == 3) offset = new Vector2(ring4OffsetX, ring4OffsetY);

                            Vector2 calculatedLocalPos = currentLocalPos - offset;

                            if (calculatedLocalPos != ringConf.localPosition && currentLocalPos != Vector2.zero)
                            {
                                ringConf.localPosition = calculatedLocalPos;
                                UnityEditor.EditorUtility.SetDirty(this);
                            }
                        }
                        #endif
                    }
                }
                else
                {
                    ringObj = new GameObject(expectedName);
                    ringObj.transform.SetParent(this.transform);
                    marker = ringObj.AddComponent<FactionMapMarker>();
                    
                    ringObj.transform.localPosition = new Vector3(ringConf.localPosition.x, ringConf.localPosition.y, -0.1f);
                    ringObj.transform.localScale = ringConf.localScale;
                }

                marker.localScaleOverride = ringConf.localScale;
                ringObj.transform.localPosition = new Vector3(ringConf.localPosition.x, ringConf.localPosition.y, -0.1f);
                ringObj.transform.localScale = ringConf.localScale;

                SpriteRenderer sr = ringObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = ringObj.AddComponent<SpriteRenderer>();
                
                if (ringConf.ringSprite != null)
                {
                    sr.sprite = ringConf.ringSprite;
                }
                else
                {
                    sr.sprite = CreateProceduralRingSprite();
                }

                CircleCollider2D col = ringObj.GetComponent<CircleCollider2D>();
                if (col == null) col = ringObj.AddComponent<CircleCollider2D>();
                
                if (sr.sprite != null)
                {
                    col.radius = Mathf.Max(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y);
                }
                else
                {
                    col.radius = 0.5f;
                }

                marker.factionName = ringConf.ringName;
                marker.factionDescription = ringConf.ringDescription;
                marker.associatedDialogueIndex = ringConf.associatedDialogueIndex;
                marker.clickSfxName = ringConf.clickSfxName;
                marker.hoverSfxName = ringConf.hoverSfxName;

                marker.glowMaterial = defaultGlowMaterial;
                marker.normalGlowColor = ringConf.normalGlowColor;
                marker.hoverGlowColor = ringConf.hoverGlowColor;
                marker.scaleSpeed = scaleSpeed;
                marker.pulseSpeed = hoverPulseSpeed;

                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    marker.ApplyGlowColorInEditor();
                }
                #endif

                activeRings.Add(ringObj);
            }

            foreach (var marker in existingMarkers)
            {
                if (marker != null && !activeRings.Contains(marker.gameObject))
                {
                    if (Application.isPlaying)
                    {
                        Destroy(marker.gameObject);
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        var go = marker.gameObject;
                        UnityEditor.EditorApplication.delayCall += () => {
                            if (go != null) DestroyImmediate(go);
                        };
                        #else
                        DestroyImmediate(marker.gameObject);
                        #endif
                    }
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            SetMapVisible(isMapVisible);
            ApplyTransformOffsets();
        }

        public void ApplyTransformOffsets()
        {
            transform.localPosition = new Vector3(mapOffsetX, mapOffsetY, 0f);
            transform.localScale = new Vector3(mapScale, mapScale, 1f);

            if (activeRings != null)
            {
                for (int i = 0; i < activeRings.Count; i++)
                {
                    if (activeRings[i] == null) continue;

                    Vector2 basePos = Vector2.zero;
                    if (mapsList != null && activeMapIndex < mapsList.Count && activeMapIndex >= 0)
                    {
                        var activeMap = mapsList[activeMapIndex];
                        if (i < activeMap.rings.Count)
                        {
                            basePos = activeMap.rings[i].localPosition;
                        }
                    }

                    float ringX = basePos.x;
                    float ringY = basePos.y;
                    if (i == 0) { ringX += ring1OffsetX; ringY += ring1OffsetY; }
                    else if (i == 1) { ringX += ring2OffsetX; ringY += ring2OffsetY; }
                    else if (i == 2) { ringX += ring3OffsetX; ringY += ring3OffsetY; }
                    else if (i == 3) { ringX += ring4OffsetX; ringY += ring4OffsetY; }

                    activeRings[i].transform.localPosition = new Vector3(ringX, ringY, -0.1f);
                    
                    // --- COMPENSATED INDEPENDENT SCALE ---
                    // Делим ringScale на mapScale, чтобы его размер на экране был абсолютно независим от масштаба карты!
                    float compensatedScale = ringScale / (mapScale > 0.001f ? mapScale : 1f);
                    activeRings[i].transform.localScale = new Vector3(compensatedScale, compensatedScale, 1f);

                    FactionMapMarker marker = activeRings[i].GetComponent<FactionMapMarker>();
                    if (marker != null)
                    {
                        marker.localScaleOverride = new Vector3(compensatedScale, compensatedScale, 1f);
                    }
                }
            }
        }

        public void HighlightRing(int ringIndex)
        {
            if (activeRings == null || activeRings.Count == 0) return;

            for (int i = 0; i < activeRings.Count; i++)
            {
                if (activeRings[i] == null) continue;
                FactionMapMarker marker = activeRings[i].GetComponent<FactionMapMarker>();
                if (marker == null) continue;

                if (i == ringIndex)
                {
                    marker.SetHighlightActive(true);
                }
                else
                {
                    marker.SetHighlightActive(false);
                }
            }
        }

        private Sprite CreateProceduralMapBackground()
        {
            Texture2D tex = new Texture2D(512, 512);
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    float xCoord = (float)x / 512 * 10f;
                    float yCoord = (float)y / 512 * 10f;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    Color col = Color.Lerp(new Color(0.04f, 0.05f, 0.12f), new Color(0.08f, 0.11f, 0.22f), sample);
                    if (x % 32 == 0 || y % 32 == 0) col += new Color(0.02f, 0.04f, 0.10f);
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
        }

        private Sprite CreateProceduralRingSprite()
        {
            Texture2D tex = new Texture2D(128, 128);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 64f;
                    float dy = y - 64f;
                    float dst = Mathf.Sqrt(dx * dx + dy * dy);
                    Color col = Color.clear;
                    if (dst > 48f && dst < 54f) col = new Color(1.0f, 1.0f, 1.0f, 0.95f);
                    else if (dst < 8f) col = new Color(1.0f, 1.0f, 1.0f, 0.70f);
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }

        private void OnValidate()
        {
            ApplyTransformOffsets();

            if (!Application.isPlaying)
            {
                InitializeMap(activeMapIndex);
            }
            else
            {
                if (mapBgRenderer != null && mapsList != null && mapsList.Count > 0)
                {
                    if (activeMapIndex >= 0 && activeMapIndex < mapsList.Count)
                    {
                        mapBgRenderer.sprite = mapsList[activeMapIndex].mapBackground;
                    }
                }
            }
        }

        #region CUSTOM COPY-PASTE FOR COORDINATES
        [ContextMenu("Copy/Position")]
        public void CopyPosition()
        {
            string json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{{\"type\":\"FateMapCoords\",\"mapOffsetX\":{0:F4},\"mapOffsetY\":{1:F4}}}",
                mapOffsetX, mapOffsetY);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("[FATE COORDINATES] Position (Map Offset) copied to clipboard!");
        }

        [ContextMenu("Copy/Rotation")]
        public void CopyRotation()
        {
            // Наша карта плоская, но добавим заглушку под стандартный Трансформ для полной копии функционала из скриншота
            string json = "{\"type\":\"FateMapCoords\",\"rotation\":0.0}";
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("[FATE COORDINATES] Default flat rotation (0.0) copied to clipboard!");
        }

        [ContextMenu("Copy/Scale")]
        public void CopyScale()
        {
            string json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{{\"type\":\"FateMapCoords\",\"mapScale\":{0:F4},\"ringScale\":{1:F4}}}",
                mapScale, ringScale);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("[FATE COORDINATES] Scales (Map & Rings) copied to clipboard!");
        }

        [ContextMenu("Copy/Offsets (Rings)")]
        public void CopyOffsets()
        {
            string json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{{\"type\":\"FateMapCoords\",\"ring1OffsetX\":{0:F4},\"ring1OffsetY\":{1:F4},\"ring2OffsetX\":{2:F4},\"ring2OffsetY\":{3:F4},\"ring3OffsetX\":{4:F4},\"ring3OffsetY\":{5:F4},\"ring4OffsetX\":{6:F4},\"ring4OffsetY\":{7:F4}}}",
                ring1OffsetX, ring1OffsetY, ring2OffsetX, ring2OffsetY, ring3OffsetX, ring3OffsetY, ring4OffsetX, ring4OffsetY);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("[FATE COORDINATES] All 4 Ring Anchors offsets copied to clipboard!");
        }

         [ContextMenu("Copy/Component Values")]
        public void CopyComponentValues()
        {
            string json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{{\"type\":\"FateMapCoords\",\"mapOffsetX\":{0:F4},\"mapOffsetY\":{1:F4},\"mapScale\":{2:F4},\"ringScale\":{3:F4},\"ring1OffsetX\":{4:F4},\"ring1OffsetY\":{5:F4},\"ring2OffsetX\":{6:F4},\"ring2OffsetY\":{7:F4},\"ring3OffsetX\":{8:F4},\"ring3OffsetY\":{9:F4},\"ring4OffsetX\":{10:F4},\"ring4OffsetY\":{11:F4}}}",
                mapOffsetX, mapOffsetY, mapScale, ringScale,
                ring1OffsetX, ring1OffsetY, ring2OffsetX, ring2OffsetY, ring3OffsetX, ring3OffsetY, ring4OffsetX, ring4OffsetY);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("[FATE COORDINATES] Entire Coordinators and Anchors Component data copied to clipboard!");
        }

        [ContextMenu("Paste/Position")]
        public void PastePosition()
        {
            string buffer = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer)) return;

            bool success = false;
            if (buffer.Contains("type") && buffer.Contains("FateMapCoords"))
            {
                success |= TryGetFloat(buffer, "mapOffsetX", ref mapOffsetX);
                success |= TryGetFloat(buffer, "mapOffsetY", ref mapOffsetY);
            }
            else
            {
                Vector3 vec;
                if (TryParseVectorAnywhere(buffer, out vec))
                {
                    mapOffsetX = vec.x;
                    mapOffsetY = vec.y;
                    success = true;
                }
            }

            if (success)
            {
                ApplyTransformOffsets();
                InitializeMap(activeMapIndex);
                Debug.Log($"[FATE COORDINATES] Position pasted successfully! Offset: ({mapOffsetX}, {mapOffsetY})");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning("[FATE COORDINATES] Paste Position failed. Clipboard contents invalid.");
            }
        }

        [ContextMenu("Paste/Rotation")]
        public void PasteRotation()
        {
            Debug.Log("[FATE COORDINATES] Rotation is locked on 2D flat Zenith Map.");
        }

        [ContextMenu("Paste/Scale")]
        public void PasteScale()
        {
            string buffer = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer)) return;

            bool success = false;
            if (buffer.Contains("type") && buffer.Contains("FateMapCoords"))
            {
                success |= TryGetFloat(buffer, "mapScale", ref mapScale);
                success |= TryGetFloat(buffer, "ringScale", ref ringScale);
            }
            else
            {
                Vector3 vec;
                if (TryParseVectorAnywhere(buffer, out vec))
                {
                    mapScale = vec.x;
                    if (vec.y > 0.01f) ringScale = vec.y;
                    success = true;
                }
            }

            if (success)
            {
                ApplyTransformOffsets();
                InitializeMap(activeMapIndex);
                Debug.Log($"[FATE COORDINATES] Scales pasted successfully! Map Scale: {mapScale}, Ring Scale: {ringScale}");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning("[FATE COORDINATES] Paste Scale failed. Clipboard contents invalid.");
            }
        }

        [ContextMenu("Paste/Offsets (Rings)")]
        public void PasteOffsets()
        {
            string buffer = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer)) return;

            bool success = false;
            if (buffer.Contains("type") && buffer.Contains("FateMapCoords"))
            {
                success |= TryGetFloat(buffer, "ring1OffsetX", ref ring1OffsetX);
                success |= TryGetFloat(buffer, "ring1OffsetY", ref ring1OffsetY);
                success |= TryGetFloat(buffer, "ring2OffsetX", ref ring2OffsetX);
                success |= TryGetFloat(buffer, "ring2OffsetY", ref ring2OffsetY);
                success |= TryGetFloat(buffer, "ring3OffsetX", ref ring3OffsetX);
                success |= TryGetFloat(buffer, "ring3OffsetY", ref ring3OffsetY);
                success |= TryGetFloat(buffer, "ring4OffsetX", ref ring4OffsetX);
                success |= TryGetFloat(buffer, "ring4OffsetY", ref ring4OffsetY);
            }

            if (success)
            {
                ApplyTransformOffsets();
                InitializeMap(activeMapIndex);
                Debug.Log("[FATE COORDINATES] All Ring Offsets pasted successfully from clipboard!");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning("[FATE COORDINATES] Paste Offsets failed. Make sure clipboard contains valid ring anchor offsets.");
            }
        }

        [ContextMenu("Paste/Component Values")]
        public void PasteComponentValues()
        {
            string buffer = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer)) return;

            bool success = false;
            if (buffer.Contains("type") && buffer.Contains("FateMapCoords"))
            {
                success |= TryGetFloat(buffer, "mapOffsetX", ref mapOffsetX);
                success |= TryGetFloat(buffer, "mapOffsetY", ref mapOffsetY);
                success |= TryGetFloat(buffer, "mapScale", ref mapScale);
                success |= TryGetFloat(buffer, "ringScale", ref ringScale);
                success |= TryGetFloat(buffer, "ring1OffsetX", ref ring1OffsetX);
                success |= TryGetFloat(buffer, "ring1OffsetY", ref ring1OffsetY);
                success |= TryGetFloat(buffer, "ring2OffsetX", ref ring2OffsetX);
                success |= TryGetFloat(buffer, "ring2OffsetY", ref ring2OffsetY);
                success |= TryGetFloat(buffer, "ring3OffsetX", ref ring3OffsetX);
                success |= TryGetFloat(buffer, "ring3OffsetY", ref ring3OffsetY);
                success |= TryGetFloat(buffer, "ring4OffsetX", ref ring4OffsetX);
                success |= TryGetFloat(buffer, "ring4OffsetY", ref ring4OffsetY);
            }

            if (success)
            {
                ApplyTransformOffsets();
                InitializeMap(activeMapIndex);
                Debug.Log("[FATE COORDINATES] All custom coordinator component values pasted successfully!");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                Debug.LogWarning("[FATE COORDINATES] Paste Component Values failed. Clipboard values are not custom FateMapCoords JSON format.");
            }
        }

        private bool TryGetFloat(string json, string key, ref float targetField)
        {
            // Пытаемся найти ключ "key": с кавычками в разных форматах экранирования
            string searchKey1 = "\"" + key + "\":";
            string searchKey2 = "\\\"" + key + "\\\":";
            
            int index = json.IndexOf(searchKey1);
            int keyLength = searchKey1.Length;
            
            if (index == -1)
            {
                index = json.IndexOf(searchKey2);
                keyLength = searchKey2.Length;
            }
            
            if (index == -1) return false;

            int valStart = index + keyLength;
            int commaIdx = json.IndexOf(",", valStart);
            int braceIdx = json.IndexOf("}", valStart);
            int endIdx = -1;
            if (commaIdx != -1 && braceIdx != -1) endIdx = Mathf.Min(commaIdx, braceIdx);
            else if (commaIdx != -1) endIdx = commaIdx;
            else if (braceIdx != -1) endIdx = braceIdx;

            if (endIdx == -1) return false;

            string rawVal = json.Substring(valStart, endIdx - valStart).Trim();
            rawVal = rawVal.Replace("\"", "").Replace("\\", "").Trim();
            float parsedVal;
            if (float.TryParse(rawVal, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsedVal))
            {
                targetField = parsedVal;
                return true;
            }
            return false;
        }

        private bool TryParseVectorAnywhere(string text, out Vector3 result)
        {
            result = Vector3.zero;
            string clean = text.Trim();
            
            if (clean.StartsWith("Vector3")) clean = clean.Substring(7).Trim();
            if (clean.StartsWith("Vector2")) clean = clean.Substring(7).Trim();
            if (clean.StartsWith("(") && clean.EndsWith(")")) clean = clean.Substring(1, clean.Length - 2).Trim();

            string[] parts = clean.Split(',');
            if (parts.Length >= 2)
            {
                float x = 0, y = 0, z = 0;
                bool successX = float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x);
                bool successY = float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y);
                if (parts.Length >= 3)
                {
                    float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z);
                }
                
                if (successX && successY)
                {
                    result = new Vector3(x, y, z);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

#if UNITY_EDITOR
namespace FateContinent
{
    using UnityEditor;

    [CustomEditor(typeof(FateMapManager))]
    public class FateMapManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FateMapManager manager = (FateMapManager)target;

            // Золотая панель быстрой вставки и копирования (Zenith Style)
            EditorGUILayout.Space(12);
            EditorGUILayout.BeginVertical("box");
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.normal.textColor = new Color(1.0f, 0.72f, 0.1f); // Насыщенный золотой цвет Zenith
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.fontSize = 11;
            
            EditorGUILayout.LabelField("⚡ БЫСТРОЕ КОПИРОВАНИЕ И ВСТАВКА (CLIPBOARD)", headerStyle);
            EditorGUILayout.Space(6);

            // Кнопки копирования в два удобных ряда
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Копировать Позицию", GUILayout.Height(28)))
            {
                manager.CopyPosition();
            }
            if (GUILayout.Button("📋 Копировать Масштабы", GUILayout.Height(28)))
            {
                manager.CopyScale();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Копировать Смещения Колец", GUILayout.Height(28)))
            {
                manager.CopyOffsets();
            }
            if (GUILayout.Button("📋 Копировать ВСЕ настройки", GUILayout.Height(28)))
            {
                manager.CopyComponentValues();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            Color originalBg = GUI.backgroundColor;
            
            // Кнопки вставки с интуитивной цветовой индикацией
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.35f, 0.95f, 0.45f); // Сочный зеленый для вставки
            if (GUILayout.Button("📥 Вставить Позицию", GUILayout.Height(28)))
            {
                manager.PastePosition();
            }
            if (GUILayout.Button("📥 Вставить Масштабы", GUILayout.Height(28)))
            {
                manager.PasteScale();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.4f, 0.85f, 1.0f); // Неоновый голубой для колец
            if (GUILayout.Button("📥 Вставить Смещения Колец", GUILayout.Height(28)))
            {
                manager.PasteOffsets();
            }
            GUI.backgroundColor = new Color(1.0f, 0.65f, 0.2f); // Насыщенный оранжевый для полной вставки
            if (GUILayout.Button("📥 Вставить ВСЕ параметры", GUILayout.Height(28)))
            {
                manager.PasteComponentValues();
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = originalBg;

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("💡 Совет: Вы можете скопировать обычный Transform (Position/Scale) любого объекта в Unity и нажать здесь 'Вставить Позицию' или 'Вставить Масштабы' — скрипт автоматически расшифрует координаты!", MessageType.Info);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(12);

            // Отрисовка стандартного инспектора после наших кастомных кнопок копирования-вставки
            DrawDefaultInspector();
        }
    }
}
#endif

