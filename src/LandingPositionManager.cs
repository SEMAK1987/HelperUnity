using UnityEngine;
using System.Collections;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.11.7
    /// Менеджер позиционирования высадки (Landing Position Manager).
    /// Считывает выбор точки высадки из DialogueSystem_Manager или сохранений SaveGameSystem,
    /// находит вручную заданные 3D координаты на вашей высокополигональной карте
    /// и плавно перемещает туда камеру или десантирует фигурку выбранного героя.
    /// </summary>
    public class LandingPositionManager : MonoBehaviour
    {
        public static LandingPositionManager Instance { get; private set; }

        [System.Serializable]
        public class LandingPoint
        {
            [Tooltip("Техническое имя зоны (для удобства)")]
            public string zoneID = "Wastes";
            
            [Tooltip("Красивое отображаемое имя зоны")]
            public string zoneName = "Кровавые Пустоши";
            
            [Tooltip("Точка в 3D мире (сюда перенесется игрок/камера)")]
            public Transform spawnAnchor;
            
            [Tooltip("Аффилированные 3D объекты этой локации, которые активируются при высадке")]
            public GameObject[] localObjectsToActivate;
        }

        [Header("📍 Настройка Физических Точек Высадки в 3D Мире")]
        [Tooltip("Список ручных точек высадки на 3D карте Континента")]
        public LandingPoint[] landingPoints = new LandingPoint[4];

        [Header("🎥 Ссылки на объекты сцены")]
        [Tooltip("Объект игрока, который будет десантирован")]
        public Transform playerTransform;

        [Tooltip("3D Модель Континента (будет автоматически скрыта во время диалога и включена при десантировании)")]
        public GameObject continentObject;

        [Tooltip("Главная камера для плавной фокусировки")]
        public Transform mainCameraTransform;

        [Header("✨ Настройки плавного перемещения камеры")]
        public bool smoothCameraMove = true;
        public float cameraMoveSpeed = 3.0f;
        public Vector3 cameraOffset = new Vector3(0, 2.5f, -2.0f); // Ракурс сверху на точку

        private void OnValidate()
        {
            InitializeDefaultPoints();
        }

        private void InitializeDefaultPoints()
        {
            // Автоматическое расширение и инициализация массива до 4 элементов (под 4 кнопки диалога)
            if (landingPoints == null || landingPoints.Length != 4)
            {
                landingPoints = new LandingPoint[4];
            }

            for (int i = 0; i < 4; i++)
            {
                if (landingPoints[i] == null)
                {
                    landingPoints[i] = new LandingPoint();
                }
            }

            // Настройка имен по умолчанию
            landingPoints[0].zoneID = "Wastes";
            landingPoints[0].zoneName = "Кровавые Пустоши";

            landingPoints[1].zoneID = "Peak";
            landingPoints[1].zoneName = "Ледяной Пик";

            landingPoints[2].zoneID = "Ruins";
            landingPoints[2].zoneName = "Древние Руины";

            landingPoints[3].zoneID = "Sanctuary";
            landingPoints[3].zoneName = "Святилище Зенита";

            // Автоматический поиск пустышек, чтобы ничего не сбрасывалось и не требовалось перетаскивать вручную
            AutoFindSpawnAnchors();
        }

        private void AutoFindSpawnAnchors()
        {
            if (landingPoints == null) return;

            string[] defaultAnchorNames = new string[] { 
                "Oasis_SpawnPoint", 
                "Outpost_SpawnPoint", 
                "Shore_SpawnPoint", 
                "Citadel_SpawnPoint" 
            };

            for (int i = 0; i < landingPoints.Length; i++)
            {
                if (landingPoints[i] == null) continue;

                if (landingPoints[i].spawnAnchor == null)
                {
                    string targetName = defaultAnchorNames[i];
                    GameObject foundObj = GameObject.Find(targetName);
                    
                    // Если не нашли напрямую, ищем по всей иерархии (включая неактивные или вложенные объекты)
                    if (foundObj == null)
                    {
                        var transforms = Resources.FindObjectsOfTypeAll<Transform>();
                        foreach (var t in transforms)
                        {
                            if (t.name == targetName && t.gameObject.scene.isLoaded)
                            {
                                foundObj = t.gameObject;
                                break;
                            }
                        }
                    }

                    if (foundObj != null)
                    {
                        landingPoints[i].spawnAnchor = foundObj.transform;
                        Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Успешно авто-связали точку: <b>{landingPoints[i].zoneName}</b> с объектом <b>{targetName}</b> в сцене!");
                    }
                }
            }
        }

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

            // Гарантируем корректное заполнение при старте
            InitializeDefaultPoints();

            // Создаем красивую плоскость океана под континентом для фонового ландшафта, если она отсутствует
            SetupOceanPlane();

            // Автоматически понижаем ракурс камеры, если значение имеет устаревший или завышенный дефолт в инспекторе (например, y > 5.0f)
            if (cameraOffset.y > 5.0f)
            {
                cameraOffset = new Vector3(0, 2.5f, -2.0f);
                Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Скорректировали слишком высокое смещение камеры ({cameraOffset.y}f) до идеального ракурса 2.5f для детального масштаба континента.");
            }

            // Автоматическое нахождение 3D-карты в Hierarchy на старте
            if (continentObject == null)
            {
                continentObject = GameObject.Find("Континент");
                if (continentObject == null)
                {
                    // Ищем по части имени
                    foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if (go.name.Contains("Континент") && go.scene.isLoaded)
                        {
                            continentObject = go;
                            break;
                        }
                    }
                }

                if (continentObject != null)
                {
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел 3D-модель Континента.");
                }
            }

            // Автоматическое нахождение синей сферы игрока в Hierarchy на старте
            if (playerTransform == null)
            {
                GameObject pObj = GameObject.Find("Player_Placeholder");
                if (pObj == null)
                {
                    foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if (go.name.Contains("Player_Placeholder") && go.scene.isLoaded)
                        {
                            pObj = go;
                            break;
                        }
                    }
                }

                if (pObj != null)
                {
                    playerTransform = pObj.transform;
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел Player_Placeholder.");
                }
            }

            // Кэшируем оригинальные красивые материалы регионов в самом начале, пока сцена полностью активна
            CacheOriginalMaterials();
        }

        private void Start()
        {
            // СВЕРХВАЖНО: Кэшируем оригинальные красивые материалы регионов перед их деактивацией или перекраской!
            CacheOriginalMaterials();

            // СВЕРХВАЖНО: Изначально полностью выключаем 3D-карту, океан и героя, чтобы они не лезли на задний план вступительных диалогов Аэлиссы!
            if (continentObject != null)
            {
                continentObject.SetActive(false);
                Debug.Log("[LANDING SYS] Изначально деактивировали 3D Континент, чтобы очистить задний план диалога.");
            }

            if (playerTransform != null)
            {
                playerTransform.gameObject.SetActive(false);
                Debug.Log("[LANDING SYS] Изначально деактивировали Player_Placeholder.");
            }

            // Изначально скрываем океан, чтобы не мешал во время разговора
            GameObject oceanObj = GameObject.Find("Fate_Ocean_Plane");
            if (oceanObj != null)
            {
                oceanObj.SetActive(false);
                Debug.Log("[LANDING SYS] Изначально деактивировали океан Fate_Ocean_Plane.");
            }
        }

        /// <summary>
        /// Вызывается автоматически, когда игрок выбирает точку высадки и завершает диалог!
        /// </summary>
        /// <param name="zoneIndex">Индекс выбранной зоны (0: Кровавые Пустоши, 1: Ледяной Пик, 2: Древние Руины, 3: Грозовые Кряжи)</param>
        public void DispatchLanding(int zoneIndex)
        {
            // Скрываем полноэкранный фоновый канвас высадки Аэлиссы, чтобы он не перекрывал карты в момент высадки
            if (FateDialogueBackgroundController.Instance != null)
            {
                FateDialogueBackgroundController.Instance.HideBackground();
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Досрочно скрываем полноэкранный диалоговый фон перед полетом камеры!");
            }

            // Включаем 3D-карту и героя обратно при начале перемещения камеры
            if (continentObject != null)
            {
                continentObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем 3D-модель Континента!");
            }

            // Перекрашиваем регионы высадки: выбранный оставляем цветным, невыбранные делаем серыми
            RepaintRegionsBasedOnLanding(zoneIndex);

            if (playerTransform != null)
            {
                playerTransform.gameObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем Player_Placeholder!");
            }

            // Активируем океан обратно при старте полета
            GameObject oceanObj = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "Fate_Ocean_Plane" && go.scene.isLoaded)
                {
                    oceanObj = go;
                    break;
                }
            }
            if (oceanObj != null)
            {
                oceanObj.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем океан Fate_Ocean_Plane!");
            }

            if (landingPoints == null || landingPoints.Length == 0)
            {
                Debug.LogWarning("[LANDING SYS] Точки высадки не настроены в LandingPositionManager!");
                return;
            }

            // Безопасное ограничение индекса в пределах массива
            int targetIndex = Mathf.Clamp(zoneIndex, 0, landingPoints.Length - 1);
            LandingPoint point = landingPoints[targetIndex];

            Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Выполняем десантирование на точку: <b>{point.zoneName}</b> (Индекс: {zoneIndex})");

            // 1. Активируем локальные объекты этой зоны
            if (point.localObjectsToActivate != null)
            {
                foreach (var obj in point.localObjectsToActivate)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            // 2. Телепортируем игрока на физические координаты 3D Континента
            if (point.spawnAnchor != null)
            {
                if (playerTransform != null)
                {
                    playerTransform.position = point.spawnAnchor.position;
                    playerTransform.rotation = point.spawnAnchor.rotation;
                    Debug.Log($"[LANDING SYS] Игрок успешно перенесен в 3D координаты: {point.spawnAnchor.position}");
                }

                // Блокируем свободное перемещение камеры на время полета
                if (StrategicCameraController.Instance != null)
                {
                    StrategicCameraController.Instance.isControlEnabled = false;
                }

                // 3. Перемещаем камеру
                if (mainCameraTransform != null)
                {
                    Vector3 targetCameraPos = point.spawnAnchor.position + cameraOffset;
                    if (smoothCameraMove)
                    {
                        StartCoroutine(SmoothMoveCameraCoroutine(targetCameraPos, point.spawnAnchor.position, targetIndex));
                    }
                    else
                    {
                        mainCameraTransform.position = targetCameraPos;
                        mainCameraTransform.LookAt(point.spawnAnchor.position);
                        CompleteLandingAndStartDialogue(point.spawnAnchor.position);
                    }
                }
            }
            else
            {
                Debug.LogError($"[LANDING SYS] Критическая ошибка: Spawn Anchor для точки '{point.zoneName}' равен Null! Укажите пустышку Transform в инспекторе.");
            }
        }

        private void CompleteLandingAndStartDialogue(Vector3 lookAtPoint)
        {
            if (StrategicCameraController.Instance != null)
            {
                StrategicCameraController.Instance.FocusOnPoint(lookAtPoint, cameraOffset);
                StrategicCameraController.Instance.isControlEnabled = false;
            }
            if (GamePause_Manager.Instance != null)
            {
                GamePause_Manager.Instance.isPauseBlockedManually = true;
            }
            if (FateMapManager.Instance != null)
            {
                FateMapManager.Instance.SetMapVisible(false);
            }

            if (DialogueSystem_Manager.Instance != null)
            {
                DialogueSystem_Manager.Instance.StartDialogue(8);
            }
            Debug.Log("[LANDING SYS] Десантирование завершено. Пауза заблокирована. Запускаем Инструктаж о замках (шаг 8).");
        }

        private void EnableStrategicCamera(Vector3 anchorPosition)
        {
            if (StrategicCameraController.Instance != null)
            {
                StrategicCameraController.Instance.FocusOnPoint(anchorPosition, cameraOffset);
                StrategicCameraController.Instance.isControlEnabled = true;
                Debug.Log("[LANDING SYS] Свободный режим стратегической камеры успешно активирован!");
            }
        }

        private IEnumerator SmoothMoveCameraCoroutine(Vector3 targetPosition, Vector3 lookAtPoint, int zoneIndex)
        {
            float elapsed = 0f;
            Vector3 startPos = mainCameraTransform.position;
            Quaternion startRot = mainCameraTransform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(lookAtPoint - targetPosition);

            while (elapsed < 2.0f) // Длительность плавного полета 2 секунды
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 2.0f;
                
                // Плавное ускорение и замедление (Smooth Step)
                t = t * t * (3f - 2f * t);

                mainCameraTransform.position = Vector3.Lerp(startPos, targetPosition, t);
                mainCameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            mainCameraTransform.position = targetPosition;
            mainCameraTransform.rotation = targetRot;
            Debug.Log("[LANDING SYS] Камера успешно наведена на новую зону высадки.");

            // Активируем свободное перемещение камеры игроком после приземления
            CompleteLandingAndStartDialogue(lookAtPoint);
        }

        /// <summary>
        /// Создает красивую плоскость океана под континентом для фонового ландшафта, если она отсутствует.
        /// </summary>
        public void SetupOceanPlane()
        {
            GameObject oceanObj = GameObject.Find("Fate_Ocean_Plane");
            if (oceanObj == null)
            {
                oceanObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                oceanObj.name = "Fate_Ocean_Plane";
                
                // Располагаем под континентом (чуть ниже Y = -0.5f, чтобы не было Z-файта с ландшафтом)
                oceanObj.transform.position = new Vector3(0f, -0.6f, 0f);
                
                // Делаем плоскость достаточно огромной для стратегического обзора
                oceanObj.transform.localScale = new Vector3(80f, 1f, 80f); 
            }

            // Гарантируем правильную настройку материала, текстур и тайлинга при каждом запуске!
            MeshRenderer mr = oceanObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material oceanMat = null;
                
                // Сначала проверяем, назначен ли уже материал M_Ocean_Background непосредственно на плоскости
                if (mr.sharedMaterial != null && mr.sharedMaterial.name.Contains("M_Ocean_Background"))
                {
                    oceanMat = mr.sharedMaterial;
                }
                
                // Попытка найти уже настроенный в ассетах материал M_Ocean_Background, чтобы избежать сброса текстур
                if (oceanMat == null)
                {
                    Material[] availableMats = Resources.FindObjectsOfTypeAll<Material>();
                    foreach (var m in availableMats)
                    {
                        if (m != null && m.name == "M_Ocean_Background")
                        {
                            oceanMat = m;
                            break;
                        }
                    }
                }

                // Если материала нет, создаем его динамически с поддержкой Universal Render Pipeline
                if (oceanMat == null)
                {
                    Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpShader == null) urpShader = Shader.Find("URP/Lit");
                    if (urpShader == null) urpShader = Shader.Find("Standard");
                    
                    oceanMat = new Material(urpShader);
                    oceanMat.name = "M_Ocean_Background";
                    
                    // Цветовой оттенок темного космического океана
                    oceanMat.color = new Color(0.05f, 0.18f, 0.38f, 1.0f);
                    
                    // Шероховатость и отражения
                    if (oceanMat.HasProperty("_Glossiness")) oceanMat.SetFloat("_Glossiness", 0.75f);
                    if (oceanMat.HasProperty("_Smoothness")) oceanMat.SetFloat("_Smoothness", 0.75f);
                    if (oceanMat.HasProperty("_Metallic")) oceanMat.SetFloat("_Metallic", 0.4f);
                }

                // Восстанавливаем текстуру, только если она действительно пуста
                if (oceanMat.mainTexture == null)
                {
                    Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                    foreach (var tex in allTextures)
                    {
                        if (tex != null && (tex.name.ToLower().Contains("water") || tex.name.ToLower().Contains("ocean") || tex.name.ToLower().Contains("sea")))
                        {
                            // Избегаем случайного назначения текстур нормалей (bump maps, normal maps)
                            if (tex.name.ToLower().Contains("normal") || tex.name.ToLower().Contains("bump"))
                                continue;

                            oceanMat.mainTexture = tex;
                            break;
                        }
                    }
                }

                // Принудительно задаем 40x40 тайлинг, чтобы 8K текстура воды не растягивалась мылом
                if (oceanMat.HasProperty("_BaseMap"))
                {
                    oceanMat.SetTextureScale("_BaseMap", new Vector2(40f, 40f));
                }
                else if (oceanMat.HasProperty("_MainTex"))
                {
                    oceanMat.SetTextureScale("_MainTex", new Vector2(40f, 40f));
                }

                mr.material = oceanMat;
                
                // Отключаем тени, чтобы плоскость выглядела чисто
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;

                Debug.Log("<color=#00FFCC>[OCEAN GENERATOR]</color> Успешно проверили и настроили материал Fate_Ocean_Plane (Y = -0.6f, масштаб 80x80).");
            }
        }

        private System.Collections.Generic.Dictionary<string, Material> originalRegionMaterials = new System.Collections.Generic.Dictionary<string, Material>();

        /// <summary>
        /// Кэширует изначальные оригинальные материалы регионов, чтобы сохранить их текстуры и шейдеры безупречно
        /// </summary>
        public void CacheOriginalMaterials()
        {
            // СВЕРХВАЖНО: предотвращаем перезапись красивых оригинальных материалов!
            if (originalRegionMaterials.Count > 0)
            {
                return;
            }

            GameObject continent = GameObject.Find("New_Kontinent") ?? GameObject.Find("/New_Kontinent");
            if (continent == null && continentObject != null)
            {
                continent = continentObject;
            }

            if (continent != null)
            {
                string[] regionNames = new string[] { "Region_03", "Region_06", "Region_08", "Region_11" };
                foreach (string name in regionNames)
                {
                    Transform regionTrans = continent.transform.Find(name);
                    if (regionTrans == null)
                    {
                        foreach (Transform child in continent.GetComponentsInChildren<Transform>(true))
                        {
                            if (child.name == name)
                            {
                                regionTrans = child;
                                break;
                            }
                        }
                    }

                    if (regionTrans != null)
                    {
                        MeshRenderer mr = regionTrans.GetComponent<MeshRenderer>();
                        if (mr != null && mr.sharedMaterial != null)
                        {
                            // Сохраняем ссылку на оригинальный материал
                            originalRegionMaterials[name] = mr.sharedMaterial;
                            Debug.Log($"[LANDING SYS] Успешно закеширован оригинальный материал для {name}: {mr.sharedMaterial.name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Восстанавливает оригинальный красивый материал под игроком, а остальные делает нейтрально серыми,
        /// с поддержкой ручной настройки цвета каждого из 12 регионов при калибровке!
        /// </summary>
        public void RepaintRegionsBasedOnLanding(int activeZoneIndex)
        {
            string[] regionNames = new string[12];
            for (int r = 0; r < 12; r++)
            {
                regionNames[r] = "Region_" + r.ToString("D2");
            }

            GameObject continent = GameObject.Find("New_Kontinent") ?? GameObject.Find("/New_Kontinent");
            if (continent == null && continentObject != null)
            {
                continent = continentObject;
            }

            if (continent != null)
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");

                for (int i = 0; i < 12; i++)
                {
                    string name = regionNames[i];
                    Transform regionTrans = continent.transform.Find(name);
                    if (regionTrans == null)
                    {
                        foreach (Transform child in continent.GetComponentsInChildren<Transform>(true))
                        {
                            if (child.name == name)
                            {
                                regionTrans = child;
                                break;
                            }
                        }
                    }

                    if (regionTrans != null)
                    {
                        MeshRenderer mr = regionTrans.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            // Определяем владельца замка 'i'
                            string owner = "Enemy";
                            if (FateCastleManager.Instance != null && i < FateCastleManager.Instance.castles.Count)
                            {
                                owner = FateCastleManager.Instance.castles[i].owner;
                            }
                            else
                            {
                                int tempLanded = PlayerPrefs.GetInt("LandedZoneIndex", 0);
                                int actReg = FateCastleManager.GetActualRegionIndexFromLanding(tempLanded);
                                owner = (i == actReg) ? "Player" : "Enemy";
                            }

                            // Определяем целевой цвет региона
                            Color targetColor;
                            if (owner == "Player")
                            {
                                // Регион игрока: Красивый неоновый сине-голубой Zenith Neon Blue!
                                targetColor = new Color(0.12f, 0.58f, 0.95f, 1.0f);
                            }
                            else
                            {
                                // Регион врагов или нейтралов:
                                // Проверяем, есть ли сохраненный пользователем вручную цвет для этого региона (Level Editor)
                                if (PlayerPrefs.HasKey("Region_ColorR_" + i))
                                {
                                    float rVal = PlayerPrefs.GetFloat("Region_ColorR_" + i);
                                    float gVal = PlayerPrefs.GetFloat("Region_ColorG_" + i);
                                    float bVal = PlayerPrefs.GetFloat("Region_ColorB_" + i);
                                    targetColor = new Color(rVal, gVal, bVal, 1.0f);
                                }
                                else
                                {
                                    // Цветовая палитра фракций по умолчанию для закрытых туманов войны
                                    if (i == 2 || i == 10) targetColor = new Color(0.48f, 0.52f, 0.55f, 1.0f); // Slate Neutrals
                                    else if (i == 1 || i == 7) targetColor = new Color(0.9f, 0.2f, 0.3f, 1.0f); // Bandit Crimson
                                    else if (i == 0 || i == 4 || i == 5 || i == 9) targetColor = new Color(0.15f, 0.72f, 0.28f, 1.0f); // Elven Green
                                    else targetColor = new Color(0.45f, 0.35f, 0.65f, 1.0f); // Dark Violet Void
                                }
                            }

                            // Если это активный регион под ГЕРОЕМ, подсвечиваем его дополнительной яркостью!
                            int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", activeZoneIndex);
                            int actualPlayerRegion = FateCastleManager.GetActualRegionIndexFromLanding(landedZone);
                            if (i == actualPlayerRegion)
                            {
                                // Смешиваем на 35% с ярким цветом морской волны для подсветки активного региона под героем!
                                targetColor = Color.Lerp(targetColor, Color.cyan, 0.35f);
                            }

                            // Создаем оригинальный материал
                            Material dynamicMat = new Material(urpShader);
                            dynamicMat.color = targetColor;
                            
                            // Сохраняем исходную текстуру, если она была на оригинальном материале
                            if (originalRegionMaterials.ContainsKey(name) && originalRegionMaterials[name] != null)
                            {
                                dynamicMat.mainTexture = originalRegionMaterials[name].mainTexture;
                            }

                            // Настройка шейдера и отражений
                            if (dynamicMat.HasProperty("_Glossiness")) dynamicMat.SetFloat("_Glossiness", (i == actualPlayerRegion) ? 0.75f : 0.4f);
                            if (dynamicMat.HasProperty("_Smoothness")) dynamicMat.SetFloat("_Smoothness", (i == actualPlayerRegion) ? 0.75f : 0.4f);
                            if (dynamicMat.HasProperty("_Metallic")) dynamicMat.SetFloat("_Metallic", (i == actualPlayerRegion) ? 0.25f : 0.12f);

                            mr.material = dynamicMat;
                        }
                    }
                }
            }
        }
    }
}
