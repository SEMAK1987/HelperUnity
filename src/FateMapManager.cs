using UnityEngine;
using System.Collections.Generic;

/*
 * [FATE CONTINENT - ZENITH MAP MANAGER v18.9.0]
 * Центральный менеджер интерактивных карт и динамических декоративных колец (маркеров).
 * Позволяет настраивать список карт (континентов) прямо в Инспекторе через кнопку [+].
 * Для каждой карты можно задать свой фоновый спрайт и любое количество интерактивных колец-маркеров,
 * которые автоматически создаются, плавно светятся, издают звуки при наведении/клике и запускают диалоги.
 */

namespace FateContinent
{
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
            public Vector2 localPosition = Vector2.zero; // Позиция на карте
            
            [Header("Диалоги и Аудио")]
            public int associatedDialogueIndex = 3;
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

            // Если сейчас активен вводный диалог, скрываем карту до его завершения!
            if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive)
            {
                SetMapVisible(false);
            }
            else
            {
                SetMapVisible(showMapOnStart);
            }
        }

        public void SetMapVisible(bool visible)
        {
            isMapVisible = visible;
            
            // Скрываем/показываем фон карты
            if (mapBgRenderer != null)
            {
                mapBgRenderer.enabled = visible;
            }

            // Скрываем/показываем все дочерние кольца (Rings)
            foreach (GameObject ring in activeRings)
            {
                if (ring != null)
                {
                    ring.SetActive(visible);
                }
            }
            
            Debug.Log($"[Fate Map] Видимость карты установлена в {visible}");
        }

        // Совместимый псевдоним/метод для интеграции с другими скриптами
        public void ToggleWorldMap(bool visible)
        {
            SetMapVisible(visible);
        }

        public void ToggleWorldMap()
        {
            SetMapVisible(!isMapVisible);
        }

        // Переключение карты по индексу (например, из UI кнопок или скрипта)
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

        // Создает радиальное текстурное кольцо на лету, если спрайт не задан дизайнером в инспекторе!
        private Sprite CreateProceduralRingSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - size / 2f;
                    float dy = y - size / 2f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float rInner = size * 0.3f;
                    float rOuter = size * 0.45f;
                    
                    if (dist >= rInner && dist <= rOuter)
                    {
                        float alpha = 1f;
                        // Сглаживание краев кольца
                        if (dist - rInner < 3f) alpha = (dist - rInner) / 3f;
                        else if (rOuter - dist < 3f) alpha = (rOuter - dist) / 3f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // Создает красивый процедурный фон со звездами и кибер-сеткой для тактической карты,
        // если в инспекторе не настроена фоновая картинка!
        private Sprite CreateProceduralMapBackground()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dy = (float)y / height;
                    float dx = (float)x / width;
                    
                    float distCenter = Mathf.Sqrt((dx - 0.5f)*(dx - 0.5f) + (dy - 0.5f)*(dy - 0.5f));
                    Color baseCol = Color.Lerp(new Color(0.04f, 0.05f, 0.12f, 1f), new Color(0.01f, 0.02f, 0.05f, 1f), distCenter * 1.4f);
                    
                    // Добавляем красивую сеточку тактической карты
                    if (x % 32 == 0 || y % 32 == 0)
                    {
                        baseCol += new Color(0.12f, 0.45f, 0.85f, 0.07f);
                    }
                    
                    // Добавляем случайные мелкие звездочки на фон
                    float pseudoNoise = Mathf.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f;
                    pseudoNoise = pseudoNoise - Mathf.Floor(pseudoNoise);
                    if (pseudoNoise > 0.997f)
                    {
                        baseCol += new Color(1f, 1f, 1f, 0.6f);
                    }
                    
                    tex.SetPixel(x, y, baseCol);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        // Автозаполнение резервной карты при отсутствии данных в Инспекторе
        private void SetupFallbackMap()
        {
            mapsList = new List<MapConfig>();
            MapConfig fallbackMap = new MapConfig();
            fallbackMap.mapName = "Континент Судьбы (Zenith Master Map)";
            fallbackMap.mapBackground = null;

            // Кольцо 1: Кровавые Пустоши (Свечение красное)
            RingConfig ring1 = new RingConfig();
            ring1.ringName = "Кровавые Пустоши";
            ring1.ringDescription = "Опасный выжженный сектор Континента. Родина демонов-налетчиков и бушующих песчаных вихрей.";
            ring1.associatedDialogueIndex = 4;
            ring1.localPosition = new Vector2(-150f, -60f);
            ring1.normalGlowColor = new Color(0.9f, 0.15f, 0.15f, 1.0f);
            ring1.hoverGlowColor = new Color(1.0f, 0.35f, 0.35f, 2.0f);

            // Кольцо 2: Ледяной Пик (Свечение голубое/циановое)
            RingConfig ring2 = new RingConfig();
            ring2.ringName = "Ледяной Пик";
            ring2.ringDescription = "Царство вечной мерзлоты. По слухам, гигантские ледяные мимики охраняют заброшенные рудные копи.";
            ring2.associatedDialogueIndex = 5;
            ring2.localPosition = new Vector2(0f, 80f);
            ring2.normalGlowColor = new Color(0.15f, 0.65f, 0.95f, 1.0f);
            ring2.hoverGlowColor = new Color(0.4f, 0.85f, 1.0f, 2.0f);

            // Кольцо 3: Древние Руины (Свечение оранжево-золотое)
            RingConfig ring3 = new RingConfig();
            ring3.ringName = "Древние Руины";
            ring3.ringDescription = "Остатки разрушенной столицы первой династии. Таят кристаллы Зенита, но полны оживших каменных стражей.";
            ring3.associatedDialogueIndex = 6;
            ring3.localPosition = new Vector2(150f, -60f);
            ring3.normalGlowColor = new Color(0.85f, 0.55f, 0.12f, 1.0f);
            ring3.hoverGlowColor = new Color(1.0f, 0.75f, 0.25f, 2.0f);

            fallbackMap.rings.Add(ring1);
            fallbackMap.rings.Add(ring2);
            fallbackMap.rings.Add(ring3);

            mapsList.Add(fallbackMap);
        }

        // Инициализация выбранной карты и создание всех её колец-маркеров
        public void InitializeMap(int mapIndex)
        {
            if (mapBgRenderer == null) mapBgRenderer = GetComponent<SpriteRenderer>();
            
            // Если список карт в инспекторе пуст, автоматически генерируем великолепный сбалансированный пресет!
            if (mapsList == null || mapsList.Count == 0)
            {
                Debug.Log("[Fate Map] Обнаружен пустой MapsList. Авто-генерируем интерактивную карту Зенит v18.10.1...");
                SetupFallbackMap();
            }

            if (mapIndex < 0 || mapIndex >= mapsList.Count) mapIndex = 0;

            MapConfig activeMap = mapsList[mapIndex];
            Debug.Log($"[Fate Map] Загрузка карты '{activeMap.mapName}' (Индекс: {mapIndex})");

            // 1. Меняем фон карты
            if (activeMap.mapBackground != null)
            {
                mapBgRenderer.sprite = activeMap.mapBackground;
            }
            else
            {
                Debug.LogWarning($"[Fate Map] У карты '{activeMap.mapName}' отсутствует фоновый спрайт! Генерируем красивую процедурную кибер-карту Cosmos-Zenith...");
                mapBgRenderer.sprite = CreateProceduralMapBackground();
            }

            // 2. Очищаем старые созданные кольца-маркеры на сцене
            ClearActiveRings();

            // 3. Создаем новые кольца по заданным конфигам
            foreach (var ringConf in activeMap.rings)
            {
                if (ringConf == null) continue;

                // Создаем дочерний пустой игровой объект
                GameObject ringObj = new GameObject($"RingMarker_{ringConf.ringName}");
                ringObj.transform.SetParent(this.transform);
                ringObj.transform.localPosition = new Vector3(ringConf.localPosition.x, ringConf.localPosition.y, -0.1f); // Чуть ближе к камере

                // Добавляем SpriteRenderer для отображения кольца
                SpriteRenderer sr = ringObj.AddComponent<SpriteRenderer>();
                if (ringConf.ringSprite != null)
                {
                    sr.sprite = ringConf.ringSprite;
                }
                else
                {
                    sr.sprite = CreateProceduralRingSprite();
                }

                // Добавляем CircleCollider2D для перехвата мыши
                CircleCollider2D col = ringObj.AddComponent<CircleCollider2D>();
                // Вычисляем оптимальный радиус
                if (sr.sprite != null)
                {
                    col.radius = Mathf.Max(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y);
                }
                else
                {
                    col.radius = 0.5f;
                }

                // Добавляем наш интерактивный скрипт FactionMapMarker
                FactionMapMarker marker = ringObj.AddComponent<FactionMapMarker>();
                marker.factionName = ringConf.ringName;
                marker.factionDescription = ringConf.ringDescription;
                marker.associatedDialogueIndex = ringConf.associatedDialogueIndex;
                marker.clickSfxName = ringConf.clickSfxName;
                marker.hoverSfxName = ringConf.hoverSfxName;

                // Эффекты свечения
                marker.glowMaterial = defaultGlowMaterial;
                marker.normalGlowColor = ringConf.normalGlowColor;
                marker.hoverGlowColor = ringConf.hoverGlowColor;
                marker.scaleSpeed = scaleSpeed;
                marker.pulseSpeed = hoverPulseSpeed;

                activeRings.Add(ringObj);
            }

            // Накатываем текущее состояние видимости на все созданные кольца!
            SetMapVisible(isMapVisible);
        }

        private void ClearActiveRings()
        {
            foreach (GameObject ring in activeRings)
            {
                if (ring != null)
                {
                    // В режиме редактора (Editor) и во время игры безопасное уничтожение
                    if (Application.isPlaying)
                        Destroy(ring);
                    else
                        DestroyImmediate(ring);
                }
            }
            activeRings.Clear();
        }

        private void OnValidate()
        {
            // Позволяет мгновенно тестировать переключение карт в редакторе Unity при изменении индекса в Инспекторе
            if (mapBgRenderer != null && mapsList != null && mapsList.Count > 0)
            {
                if (activeMapIndex >= 0 && activeMapIndex < mapsList.Count)
                {
                    mapBgRenderer.sprite = mapsList[activeMapIndex].mapBackground;
                }
            }
        }
    }
}
