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

        // Инициализация выбранной карты и создание всех её колец-маркеров
        public void InitializeMap(int mapIndex)
        {
            if (mapBgRenderer == null) mapBgRenderer = GetComponent<SpriteRenderer>();
            if (mapsList == null || mapsList.Count == 0)
            {
                Debug.LogWarning("[Fate Map] Список карт пуст! Добавьте элементы карт в Инспекторе.");
                return;
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
                Debug.LogWarning($"[Fate Map] У карты '{activeMap.mapName}' отсутствует фоновый спрайт!");
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
                sr.sprite = ringConf.ringSprite;

                // Добавляем CircleCollider2D для перехвата мыши
                CircleCollider2D col = ringObj.AddComponent<CircleCollider2D>();
                // Вычисляем оптимальный радиус
                if (ringConf.ringSprite != null)
                {
                    col.radius = Mathf.Max(ringConf.ringSprite.bounds.extents.x, ringConf.ringSprite.bounds.extents.y);
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
